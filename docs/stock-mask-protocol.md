# Stock Shining Mask Protocol Reference

Last updated: 2026-06-27

This is a repo-local implementation reference for the stock Shining Mask BLE
protocol used by MaskApp. It summarizes community reverse-engineering evidence
for app implementation work; it is not manufacturer documentation.

Primary source:

- BrickCraftDream,
  [Shining-Mask-stuff `ble-protocol.md`](https://github.com/BrickCraftDream/Shining-Mask-stuff/blob/main/ble-protocol.md)

Secondary/context sources:

- adrihd,
  [ShiningAppMask-ReverseEngineering](https://github.com/adrihd/ShiningAppMask-ReverseEngineering)
- Reddit r/ReverseEngineering thread,
  [Help me figure out how to reverse engineer the Shining Mask app](https://www.reddit.com/r/ReverseEngineering/comments/lr9xxr/help_me_figure_out_how_to_reverse_engineer_the/)

Use this document before changing protocol, BLE, Text, Image, Rhythm, or RAVE
code. Keep product claims aligned with the confidence table below until a real
mask test is recorded in `docs/progress.md`.

## BLE Topology

MaskApp currently targets this stock mask service:

| Role | UUID | Properties | Notes |
| --- | --- | --- | --- |
| Service | `0000fff0-0000-1000-8000-00805f9b34fb` | Service | Present in current MaskApp code; verify during real-mask discovery. |
| Command | `d44bc439-abfd-45a2-b575-925416129600` | Write | Encrypted 16-byte commands: brightness, built-ins, text controls, upload start/finish, DIY management. |
| Notification | `d44bc439-abfd-45a2-b575-925416129601` | Notify | ACK and response messages for uploads and utility commands. |
| Image/Text upload | `d44bc439-abfd-45a2-b575-92541612960a` | Write | Unencrypted data chunks for custom image and text bitmap payloads. |
| Audio visualization | `d44bc439-abfd-45a2-b575-92541612960b` | Write | Encrypted 16-byte live visualizer packets. |

Do not assume every clone exposes every characteristic identically. Text/Image
upload should support a diagnostic path that reports which characteristics were
found and whether ACK notifications are available.

## Encrypted Command Format

Commands sent to the command characteristic are AES-128-ECB encrypted with this
fixed key:

```text
32672f7974ad43451d9c6c894a0e8764
```

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

## Utility Commands

| Command | Arguments | Expected behavior | Confidence |
| --- | --- | --- | --- |
| `LIGHT` | 1 byte brightness | Sets LED brightness. Prefer capping normal UI brightness at or below `100` until physical flicker behavior is verified. | Implemented, needs real-mask test |
| `IMAG` | 1 byte built-in image id | Displays a stock static image. Source notes useful IDs up to about `0x69`; higher values may show out-of-bounds stock data. | Implemented, needs real-mask test |
| `ANIM` | 1 byte built-in animation id | Plays a stock animation. Source notes useful IDs up to about `0x45`; higher values may show undefined pixels. | Implemented, needs real-mask test |
| `CHEC` | none | Requests the number of DIY images stored on the mask; response is sent on the notification characteristic. | Protocol-documented |
| `DELE` | 1 byte count, then up to 10 DIY ids | Deletes uploaded DIY image slots. Source response behavior is not fully decoded. | Protocol-documented |
| `PLAY` | 1 byte count, then up to 10 DIY ids | Plays uploaded DIY image slots in order. | Protocol-documented |

For MaskApp UX, keep built-in gallery scanning and DIY management behind
diagnostics or clearly labeled flows until the ID ranges and response behavior
are physically validated.

## Text Commands

These commands are separate from the bitmap upload procedure.

| Command | Arguments | Expected behavior | Confidence |
| --- | --- | --- | --- |
| `MODE` | 1 byte mode | Text display mode. Community mapping: `1` off, `2` blink, `3` scroll right-to-left, `4` scroll left-to-right; `0` and values above `4` are not useful. | Protocol-documented |
| `SPEED` | 1 byte speed | Sets the speed used by text display modes. | Implemented, needs real-mask test |
| `M` | 1 byte enabled flag, 1 byte mode | Enables special text/background effects, including random dots, fades, and stock backgrounds. | Protocol-documented |
| `FC` | 1 byte enabled flag, 3 bytes RGB | Sets foreground text color. | Protocol-documented |
| `BC` | 1 byte enabled flag, 3 bytes RGB | Sets background color; black can clear background image effects. | Protocol-documented |

Implementation note: use the stock `MODE` command name for future text-mode
work unless a real-mask test proves the local Java-derived behavior needs a
compatibility branch.

## Text Upload Procedure

Text upload uses the same upload characteristic as image data. Treat it as a
bitmap upload with optional per-column or per-stripe color data.

1. Render the text as a 16-pixel-high bitmap. Keep widths conservative; the
   primary source warns that very long bitmaps can make the mask unresponsive.