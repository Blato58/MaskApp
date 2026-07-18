# Stock Shining Mask Protocol Reference

Last updated: 2026-07-18

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
- Static analysis of the two bundled TR1906 OTA images, recorded in
  [TR1906 firmware and BLE protocol analysis](tr1906-firmware-analysis.md).

The tables below are summarized and normalized for this repository. Do not copy
external protocol notes verbatim into code comments or user-facing copy.

## Confidence Labels

| Label | Meaning |
| --- | --- |
| Protocol-documented | Present in reverse-engineered evidence, but not necessarily implemented in MaskApp. |
| Java evidence | Also supported by the local Android source snapshot. |
| Firmware-static | Present in the command or GATT path of both analyzed TR1906 OTA images, but not physically verified. |
| Implemented | Implemented in this repo and covered by compile/unit validation where practical. |
| Needs real-mask test | Not yet physically verified on the user's mask. |
| Experimental | Plausible from protocol evidence, but timing, visual result, or reliability is unknown. |

## BLE Topology

MaskApp currently targets this stock mask service:

| Role | UUID | Properties | Notes |
| --- | --- | --- | --- |
| Service | `0000fff0-0000-1000-8000-00805f9b34fb` | Service | Present in the Android client and both analyzed TR1906 firmware builds. Verify during real-mask discovery. |
| Command | `d44bc439-abfd-45a2-b575-925416129600` | Write / Write Without Response | Encrypted 16-byte commands: brightness, built-ins, text controls, and upload start/finish. DIY management commands belong to a broader app/device profile. |
| Notification | `d44bc439-abfd-45a2-b575-925416129601` | Notify | Encrypted 16-byte ACK and response messages in the analyzed firmware. |
| Image/Text upload | `d44bc439-abfd-45a2-b575-92541612960a` | Write / Write Without Response | Raw 20/100-byte frames in the Java-derived, physically working MaskApp profile. The bundled TR1906 firmware instead AES-decrypts one 16-byte block here; see the profile mismatch below. |
| Audio visualization | `d44bc439-abfd-45a2-b575-92541612960b` | Write / Write Without Response | Encrypted 16-byte live visualizer packets, confirmed by firmware-static analysis. |

Do not assume every clone exposes every characteristic identically. Text and
image upload paths should report which characteristics were found and whether
ACK notifications are available. In particular, do not combine the raw upload
framing used by MaskApp with the encrypted upload framing found in the bundled
TR1906 images.

## AES/Encryption

Commands sent to the command characteristic are AES-128-ECB encrypted with this
fixed key:

```text
32672f7974ad43451d9c6c894a0e8764
```

MaskApp implements this in `MaskProtocolCrypto`. The TR1906 images contain AES
encrypt/decrypt routines and a RAM key schedule, but load their key from a flash
address outside the OTA image. The fixed key above is therefore app evidence,
not a value independently recovered from those two OTA files.

## Encrypted Command Block Format

Plaintext command blocks are exactly 16 bytes:

| Offset | Size | Meaning |
| --- | --- | --- |
| `0` | 1 byte | Length of command name plus argument bytes, not including the length byte. |
| `1..n` | 1-5 bytes | ASCII command name. |
| after command | variable | Command arguments. |
| remaining bytes | variable | Zero padding to 16 bytes unless a command source proves otherwise. |

The resulting 16-byte block is encrypted once and written as a single command
payload. In the Java-derived MaskApp upload profile, image/text chunks written
to the upload characteristic are not encrypted. In the analyzed TR1906
firmware profile, command, upload, and visualizer values are each passed through
one AES block decrypt before dispatch. Audio visualizer packets are 16-byte
encrypted packets in both sources.

## Bundled TR1906 Firmware Profile

The two decrypted OTA assets implement the same relocated command parser. It
recognizes `DATS`, `DATCP`, `SPEED`, `SMVEW`, `SOUT`, `LIGHT`, `LOOP`, `ANIM`,
`CLRL`, `IMAG`, and `MODE`. It does not route `M`, `FC`, `BC`, `CHEC`, `DELE`,
`PLAY`, `TIME`, `FACE`, `MANY`, or `MANCP` from this GATT command path.

Its upload characteristic accepts independently AES-wrapped 16-byte blocks
with a length byte and streamed data, without the frame index used by the
Android app's raw 20/100-byte upload path. It emits encrypted `DATSOK`,
`DATCPOK`, and `ERROR00`, but no per-frame `REOK` from that handler.

This is a separate protocol profile, not a correction to the physically used
MaskApp DIY upload flow. The images also contain the identity string
`GLASSES-`. See the [full firmware analysis](tr1906-firmware-analysis.md) for
decryption hashes, function addresses, exact command mappings, upload types,
and visualizer nibble packing.

## Utility/Control Commands

| Command | Arguments | Expected behavior | Confidence |
| --- | --- | --- | --- |
| `LIGHT` | 1 byte brightness | Sets LED brightness. The analyzed firmware quantizes `0..100` into five internal levels. Prefer capping normal UI brightness at or below `100` until physical flicker behavior is verified. | Implemented, firmware-static, needs real-mask test |
| `IMAG` | 1 byte built-in image id | Displays a stock static image. The Android app's UI catalog lists 70 decimal IDs, `0..69`; MaskApp ships the corresponding original-app UI previews. Older `0x69` notes were approximate protocol evidence, not a complete Android gallery count. | Implemented, firmware-static, needs real-mask test |
| `ANIM` | 1 byte built-in animation id | Plays a stock animation. The Android app's UI catalog lists 45 decimal command IDs: `0`, `1`, `2`, `3`, and `5..45`. ID `4` is present in generated resource IDs but `AnimFragment` skips it before sending. MaskApp previews the referenced frames at the original 100 ms cadence. | Implemented, firmware-static, needs real-mask test |
| `CHEC` | none | Requests the number of DIY images stored on the mask; response is sent on the notification characteristic. Available evidence exposes a count, not a complete slot inventory. | Protocol-documented |
| `DELE` | 1 byte count, then up to 10 DIY ids | Deletes uploaded DIY image slots. Response behavior needs physical confirmation. | Protocol-documented |
| `PLAY` | 1 byte count, then up to 10 DIY ids | Plays uploaded DIY image slots in order. Pages uses one slot for prepared faces/text. While MaskApp is active, app-built animations continuously repeat individual one-slot commands at their catalog-defined cadence. When the app receives its stop/lock lifecycle event, playback pauses its phone timer and makes a best-effort single-write multi-slot `PLAY` handoff so the mask can continue at its fixed, slower firmware cadence. Returning sends an immediate one-slot reclaim before configured app timing resumes. The Holy Priest set currently uses 150-240 ms per frame; the black/white flash uses 150 ms. | Implemented; lock/background handoff and foreground reclaim need follow-up real-mask tests |

The original Android DIY flow caps storage at 20 numbered slots (`1..20`). Pages
exposes preparation and refresh as explicit actions rather than implying that
app-local state is a verified device inventory. Switching masks, clearing DIY
storage, or editing content can require a refresh. MaskApp keeps a durable
per-slot content fingerprint for its own DIY writes, so an in-app overwrite or
failed refresh invalidates older Pages shortcuts even if the source face is
later renamed, moved to another preferred slot, or deleted from the Library.
The current app-built animation catalog reserves slots `15..20` for a shared
six-frame Holy Priest bank. Six animations reuse those prepared cross,
inversion, antihero, bass, sonar, and off-balance frames at independently
configured 150-240 ms cadences. A built-in Holy Priest Page exposes the full
face and animation collection and is available in Stage whenever Stage is using
Pages rather than an active setlist. Stage animation tiles are tap-to-start so
the app-wide session can continue after leaving Stage and can attempt the
mask-owned lifecycle handoff when the phone locks;
explicit Stop, Blackout, or another visual replaces it.
Automatic Pages slot allocation skips those numbers;
Face Studio's automatic custom-face allocation skips them too. An explicit
user-selected face slot can still overwrite them, which invalidates the affected
animation until it is prepared again.

## Text Commands

These commands are separate from the bitmap upload procedure.

| Command | Arguments | Expected behavior | Confidence |
| --- | --- | --- | --- |
| `MODE` | 1 byte mode | Text display mode. Community mapping: `1` off, `2` blink, `3` scroll right-to-left, `4` scroll left-to-right; `0` and values above `4` are not useful. The analyzed TR1906 profile handles only `1..3`, with optional alternate-mode flags. | Implemented, firmware-static, needs real-mask test |
| `SPEED` | 1 byte speed | Sets the timing level used by supported firmware display modes. The analyzed TR1906 profile quantizes `0..100` into ten internal levels, but that profile does not route `PLAY`, and real-mask testing confirmed that `SPEED` does not alter DIY-slot playback cadence. Holy Priest therefore does not send this command. | Implemented, firmware-static; not effective for DIY playback on the tested mask |
| `M` | 1 byte enabled flag, 1 byte mode | Enables special text/background effects, including random dots, fades, and stock backgrounds. Not handled by the analyzed TR1906 command path. | Protocol-documented |
| `FC` | 1 byte enabled flag, 3 bytes RGB | Sets foreground text color. Not handled by the analyzed TR1906 command path. | Protocol-documented |
| `BC` | 1 byte enabled flag, 3 bytes RGB | Sets background color; black can clear background image effects. Not handled by the analyzed TR1906 command path. | Protocol-documented |

Implementation note: use the stock `MODE` command name for future text-mode work
unless a real-mask test proves the local Java-derived behavior needs a
compatibility branch.

## Text Upload Procedure

This section describes the Java-derived MaskApp profile, not the alternate
encrypted-block upload handler in the bundled TR1906 images. Text upload uses
the same upload characteristic as image data. Treat it as a bitmap upload with
optional per-column or per-stripe color data.

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

This section describes the Java-derived MaskApp profile, not the alternate
encrypted-block upload handler in the bundled TR1906 images. Static DIY face
upload is implemented in MaskApp. A full 46x58 calibration face
was displayed on the user's physical mask on 2026-07-17, confirming canvas
orientation and static visual output. App-built custom animations reuse this
same static-frame upload path across several numbered DIY slots. While the app
is active, playback sends each requested slot as an individual one-slot `PLAY`
command at the animation's configured interval. Navigation between app pages no
longer stops that app-wide session. When the MAUI window stops for lock or
background, the engine pauses its timer and attempts one multi-slot `PLAY`
command; after that write the mask owns the loop at its fixed firmware cadence.
Returning to the app sends one single-slot command to reclaim the current app
frame before resuming the configured timing. iOS declares `bluetooth-central`
and requests a short, finite background execution window to finish this handoff;
it does not keep the subsecond phone timer running indefinitely. `SPEED` does not
affect DIY playback on the tested mask. The lifecycle handoff, sustained cadence,
battery impact, persistence, overwrite behavior, and ACK behavior still need
physical confirmation.

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

The built-in `Mask Calibration · Color Anchors` face exercises every logical
pixel and adds orientation rails, registered color anchors, and an eye-region
ruler. Its exact logical coordinates and the physically registered visibility
map live in [Mask display calibration](mask-display-calibration.md). Use that
map for eye placement and usable bounds in new face artwork.

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
   App-built animations use the same rule per frame: each slot is uploaded only
   when its stored content fingerprint is missing or changed. Playback keeps a
   list of slot steps and sends every entry as an individual one-slot `PLAY`
   command at the animation's app-timed cadence. Repeated slot ids create the
   pulse or color cycle without storing duplicate frames, and the app repeats
   the list continuously. Before suspension, continuous playback is handed to
    the mask in one protocol-limited sequence of at most ten steps. Longer custom
    sequences use their first ten steps for this background fallback. Finite
   animations pause instead of being converted into an unintended endless loop.
   Another send, explicit Stop/Blackout, disconnect, or command failure ends the
   app-owned session; reconnect still does not replay content automatically.
   The visible Refresh action deliberately re-uploads every animation frame so
   it can recover after switching masks, clearing mask storage, or an out-of-band
   slot overwrite that the app cannot detect.
9. Use standalone `DELE` only after delete response behavior is understood; avoid making
   destructive UI easy to trigger.

Static custom-image visual output is physically confirmed for this mask. The
default firmware-timed multi-slot cadence was physically observed to be too
slow, and `SPEED` did not change it. A 75 ms app-timed sequence initially worked
for a short test but later failed to produce a reliable black/white flash. The
Holy Priest catalog therefore uses explicit per-animation delays, starting at
150 ms for black/white and ranging up to 240 ms for slower motifs. Sustained
looping, the slower firmware-owned background cadence, lock-screen handoff,
battery impact, GIF-ish playback, persistence, overwrite behavior, and ACK
behavior remain unproven product capability. Force-stopping the app or losing
the BLE link before the handoff write completes cannot be guaranteed by either
mobile OS.

## Audio Visualization Protocol

The audio visualization characteristic accepts encrypted 16-byte packets. It is
intended for live visualizer/rhythm behavior rather than ordinary command or
upload traffic.

Firmware-static analysis shows a length-prefixed payload whose first byte after
the length selects one of four packing modes. The handler expands packed
4-bit levels `0..9` into a 24-element render buffer; values above `9` are
clamped to zero. Modes `0`/`1` consume 12 packed bytes, mode `2` consumes six,
and mode `3` consumes four. Palette, orientation, and visible behavior still
need a real-mask test; see the detailed firmware analysis before implementing
this alternate live-view path.

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
| `PLAYOK` | DIY playback accepted | Parsed by the face upload protocol. Prepared-slot and app-animation replay currently confirms that the `PLAY` write was sent, not that `PLAYOK` arrived, so the UI asks for visual confirmation on the mask. |
| `DELEOK` | Delete accepted | Protocol-documented; avoid destructive UI until physically verified. |
| `CHEC` | DIY slot/check response | Response payload format needs local physical validation. |
| `ERROR` | Command/upload failed | Parsed as `Error` for text upload. |

The two analyzed TR1906 builds always encrypt a 16-byte notification block and
only generate `DATSOK`, `DATCPOK`, and `ERROR00` in this parser path. Other
firmware/clone profiles may return the broader response set as encrypted,
plaintext, length-prefixed, or minimally padded values. Current parsing accepts
encrypted 16-byte blocks and plaintext fallback.

## Feature Capability Table

| Feature | Protocol basis | MaskApp status | Physical validation |
| --- | --- | --- | --- |
| BLE scan/connect | Java advertisement evidence plus BLE topology | Implemented | Needs real-mask test |
| Brightness / BLACKOUT | `LIGHT` | Implemented | Needs real-mask test |
| Built-in static image | `IMAG` | Implemented command builder and scanner/fallback UI | Needs real-mask test |
| Built-in animation | `ANIM` | Implemented command builder and scanner/fallback UI | Needs real-mask test |
| Text upload | `DATS`, upload frames, `DATCP`, `MODE`, `SPEED` | Implemented MVP | Needs real-mask test |
| Text colors/effects | `M`, `FC`, `BC` | Protocol-documented | Needs real-mask test |
| DIY/custom image upload | `DATS`/payload/`DATCP`, `CHEC`, `PLAY`, `DELE` | Implemented Face Studio upload plus Library/Pages prepare-once and PLAY-only replay | Static 46x58 orientation and visual output confirmed; slot lifecycle and ACK behavior need real-mask tests |
| App-built custom animation | Static DIY frames, continuously repeated one-slot `PLAY` commands, and a multi-slot background handoff | Implemented Experimental catalog with per-frame fingerprints, configurable per-animation foreground timing, app-wide playback ownership, and a best-effort mask-owned lock/background fallback; Holy Priest currently uses 150-240 ms | Firmware sequence is slower and `SPEED` is ineffective; foreground cadence, background handoff, battery impact, persistence, and visuals need real-mask tests |
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
6. Prepared Pages/Stage shortcuts use one DIY slot `PLAY`; app-built animations
   use a continuously repeated per-animation-timed series of one-slot `PLAY`
   commands because `SPEED` does not affect DIY playback on the tested mask.
   On lock/background, pause that phone-timed loop and make one best-effort
   multi-slot handoff to the mask during the lifecycle transition. Keep explicit Stop/Blackout and
   disconnect cancellation, keep refresh visible, and do not treat app-local
   preparation metadata as a device inventory until slot IDs, persistence,
   foreground timing, and background handoff are physically verified.
7. Keep audio visualizer, Drop Detector, Voice Mouth, Bass Face, GIF-ish
   playback, and real-time effects in Labs until deterministic real-mask tests
   pass.
