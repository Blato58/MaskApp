# Stock Shining Mask Protocol Reference

Last updated: 2026-06-30

## Purpose

This is a repo-local implementation reference for the stock Shining Mask BLE
protocol used by MaskApp. It summarizes community reverse-engineering evidence
for app implementation work; it is not manufacturer documentation.

Use this document before changing protocol, BLE, Text, Image, Rhythm, DIY-slot,
or RAVE code. Keep product claims aligned with the confidence labels below until
a real mask test has been run and the result is captured in the task summary or
a durable protocol note.

## Source Attribution

Primary source:

- BrickCraftDream,
  [Shining-Mask-stuff `ble-protocol.md`](https://github.com/BrickCraftDream/Shining-Mask-stuff/blob/main/ble-protocol.md)

Secondary/context sources:

- adrihd,
  [ShiningAppMask-ReverseEngineering](https://github.com/adrihd/ShiningAppMask-ReverseEngineering)
- Reddit r/ReverseEngineering thread,
  [Help me figure out how to reverse engineer the Shining Mask app](https://www.reddit.com/r/ReverseEngineering/comments/lr9xxr/help_me_figure_out_how_to_reverse_engineer_the/)
- Local Java snapshot under `android/`, especially `android/base/app`.

The tables below are summarized and normalized for this repository. Do not copy
external protocol notes verbatim into code comments or user-facing copy.

## Confidence Labels

| Label | Meaning |
| --- | --- |
| Protocol-documented | Present in reverse-engineered evidence, but not necessarily implemented in MaskApp. |
| Java evidence | Also supported by the local Android source snapshot. |
| Implemented | Implemented in this repo and covered by compile/unit validation where practical. |
| Needs real-mask test | Not yet physically verified on the user's mask. |
| Experimental | Plausible from protocol evidence, but timing, visual result, or reliability is unknown. |

## BLE Topology

MaskApp currently targets this stock mask service:

| Role | UUID | Properties | Notes |
| --- | --- | --- | --- |
| Service | `0000fff0-0000-1000-8000-00805f9b34fb` | Service | Verify during real-mask discovery. |
| Command | `d44bc439-abfd-45a2-b575-925416129600` | Write | Encrypted 16-byte commands: brightness, built-ins, text controls, upload start/finish, DIY management. |
| Notification | `d44bc439-abfd-45a2-b575-925416129601` | Notify | ACK and response messages for uploads and utility commands. |
| Image/Text upload | `d44bc439-abfd-45a2-b575-92541612960a` | Write | Unencrypted upload chunks for custom bitmap/text payloads. |
| Audio visualization | `d44bc439-abfd-45a2-b575-92541612960b` | Write | Encrypted 16-byte live visualizer packets. |

Do not assume every clone exposes every characteristic identically. Text and
image upload paths should report which characteristics were found and whether
ACK notifications are available.

## AES/Encryption

Commands sent to the command characteristic are AES-128-ECB encrypted with this
fixed key:

```text
32672f7974ad43451d9c6c894a0e8764
```

MaskApp implements this in `MaskProtocolCrypto`.

## Encrypted Command Block Format

Plaintext command blocks are exactly 16 bytes:

| Offset | Size | Meaning |
| --- | --- | --- |
| `0` | 1 byte | Length of command name plus argument bytes, not including the length byte. |
| `1..n` | 1-5 bytes | ASCII command name. |
| after command | variable | Command arguments. |
| remaining bytes | variable | Zero padding to 16 bytes unless a command source proves otherwise. |

The resulting 16-byte block is encrypted once and written as a single command
payload. Image/text data chunks written to the upload characteristic are not
encrypted. Audio visualizer packets are 16-byte encrypted packets.

## Utility/Control Commands

| Command | Arguments | Expected behavior | Confidence |
| --- | --- | --- | --- |
| `LIGHT` | 1 byte brightness | Sets LED brightness. Prefer capping normal UI brightness at or below `100` until physical flicker behavior is verified. | Implemented, needs real-mask test |
| `IMAG` | 1 byte built-in image id | Displays a stock static image. The Android app's UI catalog lists 70 decimal IDs, `0..69`; MaskApp ships the corresponding original-app UI previews. Older `0x69` notes were approximate protocol evidence, not a complete Android gallery count. | Implemented, needs real-mask test |
| `ANIM` | 1 byte built-in animation id | Plays a stock animation. The Android app's UI catalog lists 45 decimal command IDs: `0`, `1`, `2`, `3`, and `5..45`. ID `4` is present in generated resource IDs but `AnimFragment` skips it before sending. MaskApp previews the referenced frames at the original 100 ms cadence. | Implemented, needs real-mask test |
| `CHEC` | none | Requests the number of DIY images stored on the mask; response is sent on the notification characteristic. Available evidence exposes a count, not a complete slot inventory. | Protocol-documented |
| `DELE` | 1 byte count, then up to 10 DIY ids | Deletes uploaded DIY image slots. Response behavior needs physical confirmation. | Protocol-documented |
| `PLAY` | 1 byte count, then up to 10 DIY ids | Plays uploaded DIY image slots in order. Pages uses the single-slot form for prepared shortcuts; timing and persistence still need physical confirmation. | Implemented, needs real-mask test |

The original Android DIY flow caps storage at 20 numbered slots (`1..20`). Pages
exposes preparation and refresh as explicit actions rather than implying that
app-local state is a verified device inventory. Switching masks, clearing DIY
storage, or editing content can require a refresh. MaskApp keeps a durable
per-slot content fingerprint for its own DIY writes, so an in-app overwrite or
failed refresh invalidates older Pages shortcuts even if the source face is
later renamed, moved to another preferred slot, or deleted from the Library.

## Text Commands

These commands are separate from the bitmap upload procedure.

| Command | Arguments | Expected behavior | Confidence |
| --- | --- | --- | --- |
| `MODE` | 1 byte mode | Text display mode. Community mapping: `1` off, `2` blink, `3` scroll right-to-left, `4` scroll left-to-right; `0` and values above `4` are not useful. | Implemented, needs real-mask test |
| `SPEED` | 1 byte speed | Sets the speed used by text display modes. | Implemented, needs real-mask test |
| `M` | 1 byte enabled flag, 1 byte mode | Enables special text/background effects, including random dots, fades, and stock backgrounds. | Protocol-documented |
| `FC` | 1 byte enabled flag, 3 bytes RGB | Sets foreground text color. | Protocol-documented |
| `BC` | 1 byte enabled flag, 3 bytes RGB | Sets background color; black can clear background image effects. | Protocol-documented |

Implementation note: use the stock `MODE` command name for future text-mode work
unless a real-mask test proves the local Java-derived behavior needs a
compatibility branch.

## Text Upload Procedure

Text upload uses the same upload characteristic as image data. Treat it as a
bitmap upload with optional per-column or per-stripe color data.

1. Render text as a 16-pixel-high bitmap. Keep widths conservative; long
   bitmaps can make some masks unresponsive.
2. Build the upload payload from LED column data plus RGB color data.
3. Send encrypted `DATS` on the command characteristic. MaskApp currently uses
   payload length and text-data length as two big-endian 16-bit values.
4. Wait for `DATOK` or `DATSOK` when ACK notifications are available.
5. Split payload into upload frames. Current MaskApp defaults to 18-byte frame
   payloads with a frame header containing length and frame index.
6. Write each frame to the upload characteristic when available, otherwise use
   command-characteristic compatibility.
7. Wait for `REOK` or `REOKOK` after each frame when ACK notifications are
   available.
8. Send encrypted `DATCP` to complete the upload.
9. Wait for `DATCPOK` when ACK notifications are available.
10. Send `MODE` and `SPEED` commands to start the display behavior.

MaskApp supports ACK-required mode and write-only compatibility mode. Physical
testing must record whether the user's mask exposes notifications and whether
write-only mode reliably displays short text.

Native text upload has no slot id in `DATS`, no slot/timestamp in `DATCP`, and
no later text-specific `PLAY` command. Pages can instead render a short preset
as a static 46x58 DIY image and prepare that bitmap in a numbered DIY slot. This
is deliberately labeled as a static fast slot: it preserves the text pixels and
foreground color, but not scrolling, blink timing, native text modes, or text
effects. Text Composer and ordinary Library sends continue to use native text
upload when those behaviors are needed.

## Image Upload Procedure

Static DIY face upload is implemented in MaskApp but still needs corrected
real-mask visual confirmation. Custom animation and broader image sequencing
remain protocol-documented, not proven product behavior.

Static DIY face upload has Java evidence from `UCropActivity` and
`BitmapUtils.getBitmapData`: the original Android crop path uses a 46x58 crop
target, then `CropImage.imageData` stores 2668 RGB triplets ordered
column-first (`x`, then `y`). Static image-mode uploads should not include a
packed LED-byte prefix. Java also sets `CropImage.timeInt` to the current Unix
timestamp before upload and sends those four big-endian bytes in `DATCP`; real
uploads should not use a zero timestamp. The original crop flow assigns the
first unused `CropImage.imageIndex` rather than overwriting an existing slot, so
MaskApp clears the selected slot with `DELE` before upload until overwrite
behavior is physically proven.

Face Studio stores and edits artwork directly on that native 46x58 portrait
canvas. Stores written by the earlier 36x12 editor are migrated with uniform
nearest-neighbor scaling and centered vertically so existing drawings retain
their proportions instead of being stretched to fill the portrait canvas.
Seeded faces use layered full-canvas artwork with silhouettes, shading, facial
details, and props rather than enlarged low-resolution glyphs.

The image upload frame shape is different from MaskApp's conservative text
upload default: static DIY image data is split into 98 image bytes per packet.
Each written packet is 100 bytes total: 1 length byte, 1 packet counter byte,
up to 98 RGB image bytes, and zero padding for the final short packet. The
length byte includes the packet counter byte, so a full packet starts with
`0x63` and the final 66-byte data packet for an 8004-byte 46x58 image starts
with `0x43`. Android requests MTU 103 before service discovery so these packets
can be written as single BLE values.

Expected procedure:

1. Transform source artwork into the mask's LED bitmap shape and color limits.
2. Best-effort delete the target DIY slot with encrypted `DELE` before upload.
3. Start the upload with an encrypted command on the command characteristic.
   Face Studio uses the Java-evidenced 8004-byte `DATS` length, DIY slot id,
   and image toggle: two bytes for image size, two bytes for image index, and
   `0x01` for static image upload. Slot overwrite behavior still needs physical
   validation on the user's mask.
4. Write unencrypted 100-byte payload packets to the image/text upload
   characteristic. Each full packet carries 98 RGB image bytes.
5. Complete the upload with encrypted `DATCP` carrying the image timestamp.
6. Track `DELEOK`, `DATOK`/`DATSOK`, `REOK`/`REOKOK`, and `DATCPOK` when notifications are
   available.
7. Use `CHEC` to inspect DIY slot count where supported.
8. Use `PLAY` to display uploaded DIY slots. Pages implements this as an
   optimistic fast path after a successful app-side preparation; the user can
   explicitly refresh a slot because `CHEC` does not prove which content is in
   each slot or which physical mask was prepared.
9. Use standalone `DELE` only after delete response behavior is understood; avoid making
   destructive UI easy to trigger.

Until physical tests pass, custom image upload visual output, DIY sequencing,
GIF-ish playback, and fast slot playback remain unproven product capability.

## Audio Visualization Protocol

The audio visualization characteristic accepts encrypted 16-byte packets. It is
intended for live visualizer/rhythm behavior rather than ordinary command or
upload traffic.

MaskApp status:

- The UUID is documented in `MaskBleProtocol`.
- No microphone-driven visualizer product flow is implemented yet.
- Rhythm, Voice Mouth, Bass Face, Drop Detector, and real-time effects must stay
  Labs/Experimental until deterministic packets are implemented and tested on a
  real mask.

Future implementation should start with deterministic test packets and an
explicit stop/recovery path before adding microphone input.

## Responses/ACKs

| Response | Meaning in MaskApp | Notes |
| --- | --- | --- |
| `DATOK` | Upload start accepted | Parsed as `StartAccepted`. |
| `DATSOK` | Upload start accepted | Alternate start ACK seen in protocol evidence. |
| `REOK` | Upload frame accepted | Parsed as `FrameAccepted`. |
| `REOKOK` | Upload frame accepted | Alternate frame ACK seen in protocol evidence. |
| `DATCPOK` | Upload complete | Parsed as `Complete`. |
| `PLAYOK` | DIY playback accepted | Parsed by the face protocol; Pages sends single-slot `PLAY` and asks for visual confirmation on the mask. |
| `DELEOK` | Delete accepted | Protocol-documented; avoid destructive UI until physically verified. |
| `CHEC` | DIY slot/check response | Response payload format needs local physical validation. |
| `ERROR` | Command/upload failed | Parsed as `Error` for text upload. |

ACK values may arrive encrypted, plaintext, length-prefixed, or minimally padded
depending on firmware/clone behavior. Current parsing accepts encrypted 16-byte
blocks and plaintext fallback.

## Feature Capability Table

| Feature | Protocol basis | MaskApp status | Physical validation |
| --- | --- | --- | --- |
| BLE scan/connect | Java advertisement evidence plus BLE topology | Implemented | Needs real-mask test |
| Brightness / BLACKOUT | `LIGHT` | Implemented | Needs real-mask test |
| Built-in static image | `IMAG` | Implemented command builder and scanner/fallback UI | Needs real-mask test |
| Built-in animation | `ANIM` | Implemented command builder and scanner/fallback UI | Needs real-mask test |
| Text upload | `DATS`, upload frames, `DATCP`, `MODE`, `SPEED` | Implemented MVP | Needs real-mask test |
| Text colors/effects | `M`, `FC`, `BC` | Protocol-documented | Needs real-mask test |
| DIY/custom image upload | `DATS`/payload/`DATCP`, `CHEC`, `PLAY`, `DELE` | Implemented Face Studio upload plus Pages prepare/refresh/fast-play flow | Needs real-mask test |
| Static text fast slot | Text rasterized into a 46x58 DIY image, then `PLAY` | Implemented in Pages; native text modes intentionally not preserved | Needs real-mask test |
| Audio visualizer | audio characteristic encrypted packets | Documented only | Needs real-mask test |
| RAVE command fallbacks | `LIGHT`, `IMAG`, `ANIM` | Implemented as test/fallback controls | Needs real-mask test |
| Drop Detector / Voice Mouth / Bass Face | App-layer composition over visualizer/text/DIY | Labs/Experimental | Needs real-mask test |

## RAVE FAST Implementation Guidance

RAVE FAST should be reliable in a dark, crowded setting before it is clever.

Use this order:

1. Keep `BLACKOUT` always visible and send `LIGHT 1`.
2. Prefer instant encrypted commands: `LIGHT`, `IMAG`, and `ANIM`.
3. Use short text only after ACK-required and write-only modes are physically
   tested on iPhone.
4. Keep long uploads out of the event-time path.
5. Treat built-in IDs as test/fallback content until useful IDs are written down
   from real-mask scanning.
6. Prepared Pages shortcuts may use DIY slot `PLAY` for the fast path, but keep
   refresh visible and do not treat app-local preparation metadata as a device
   inventory until slot IDs, persistence, and playback timing are physically
   verified.
7. Keep audio visualizer, Drop Detector, Voice Mouth, Bass Face, GIF-ish
   playback, and real-time effects in Labs until deterministic real-mask tests
   pass.
