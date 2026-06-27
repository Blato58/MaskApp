# Stock Shining Mask Protocol Reference

Last updated: 2026-06-27

## Purpose

This is a repo-local implementation reference for the stock Shining Mask BLE
protocol used by MaskApp. It summarizes community reverse-engineering evidence
for app implementation work; it is not manufacturer documentation.

Use this document before changing protocol, BLE, Text, Image, Rhythm, DIY-slot,
or RAVE code. Keep product claims aligned with the confidence labels below until
a real mask test is recorded in `docs/progress.md`.

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
| `IMAG` | 1 byte built-in image id | Displays a stock static image. Evidence suggests useful IDs up to about `0x69`; higher values may show undefined stock data. | Implemented, needs real-mask test |
| `ANIM` | 1 byte built-in animation id | Plays a stock animation. Evidence suggests useful IDs up to about `0x45`; higher values may show undefined pixels. | Implemented, needs real-mask test |
| `CHEC` | none | Requests the number of DIY images stored on the mask; response is sent on the notification characteristic. | Protocol-documented |
| `DELE` | 1 byte count, then up to 10 DIY ids | Deletes uploaded DIY image slots. Response behavior needs physical confirmation. | Protocol-documented |
| `PLAY` | 1 byte count, then up to 10 DIY ids | Plays uploaded DIY image slots in order. Timing and repeat behavior need physical confirmation. | Protocol-documented |

For MaskApp UX, keep built-in gallery scanning and DIY management behind
diagnostics or clearly labeled flows until the ID ranges and response behavior
are physically validated.

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

## Image Upload Procedure

Custom image upload is protocol-documented but not yet implemented as a product
feature in MaskApp.

Expected procedure:

1. Transform source artwork into the mask's LED bitmap shape and color limits.
2. Start the upload with an encrypted command on the command characteristic.
   Reverse-engineered evidence points to the same `DATS`/`DATCP` family used by
   text/image payload transfers, but exact image metadata must be confirmed
   before user-facing upload is exposed.
3. Write unencrypted payload chunks to the image/text upload characteristic.
4. Track `DATOK`/`DATSOK`, `REOK`/`REOKOK`, and `DATCPOK` when notifications are
   available.
5. Use `CHEC` to inspect DIY slot count where supported.
6. Use `PLAY` to display uploaded DIY slots only after slot IDs and timing are
   physically verified.
7. Use `DELE` only after delete response behavior is understood; avoid making
   destructive UI easy to trigger.

Until this is implemented and tested, custom image upload, DIY sequencing,
GIF-ish playback, and fast slot playback remain Protocol-documented or
Experimental, not proven product capability.

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
| `PLAYOK` | DIY playback accepted | Protocol-documented; not implemented in product UI. |
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
| DIY/custom image upload | `DATS`/payload/`DATCP`, `CHEC`, `PLAY`, `DELE` | Not implemented as product flow | Needs real-mask test |
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
6. Do not rely on DIY slot `PLAY` until upload, slot IDs, and playback timing are
   physically verified.
7. Keep audio visualizer, Drop Detector, Voice Mouth, Bass Face, GIF-ish
   playback, and real-time effects in Labs until deterministic real-mask tests
   pass.

## Real-Mask Validation Checklist

Run this on iPhone with the physical mask:

1. Scan finds mask.
2. Connect succeeds.
3. Disconnection/reconnect path is understandable.
4. BLACKOUT sends `LIGHT 1`.
5. Brightness cap sends expected brightness.
6. `IMAG 1` changes the mask.
7. `ANIM 1` changes the mask.
8. Text short caption `LOL` sends in ACK mode if notifications work.
9. Text short caption sends in write-only mode if ACK is missing.
10. React page sends `NOPE`, `LOL`, and `SUS`.
11. RAVE page sends `DROP`, `WHEEL UP`, and `HYDRATE`.
12. RAVE command fallbacks work.
13. Festival Lock keeps BLACKOUT available.
14. Reconnect does not break RAVE UI.
15. Write down which built-in image/animation IDs are good.
