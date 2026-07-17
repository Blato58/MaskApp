# TR1906 Firmware and BLE Protocol Analysis

Last updated: 2026-07-17

## Scope

This document records static analysis of these two encrypted OTA assets:

| Firmware | Size | Encrypted SHA-1 |
| --- | ---: | --- |
| `TR1906R04-10_OTA.bin` | 66,100 bytes | `36a3b4a1144ada273e03e08c91d6cb1b7fdb9f35` |
| `TR1906R04-1-10_OTA.bin` | 65,840 bytes | `f0f38c1faacf3fc2730b0809d381aecdd56566e2` |

Both files are present in the decompiled Android application's assets. The
analysis describes the protocol compiled into these exact binaries. It does not
prove that every Shining Mask clone, or the user's physical mask, runs this
firmware profile.

The images identify themselves as `TR1906R04-10` and `TR1906R04-01-10`. They
also contain the device-name string `GLASSES-`. Their BLE protocol is narrower
than the DIY-slot protocol used elsewhere in the Android application, so those
profiles must not be treated as interchangeable.

## Reproducible Decryption

Use the repo script; it needs only Python's standard library:

```powershell
python build/scripts/decrypt-tr1906-firmware.py `
  path\to\TR1906R04-10_OTA.bin `
  path\to\TR1906R04-1-10_OTA.bin `
  --output-dir artifacts\firmware-analysis\decrypted
```

It writes a decrypted OTA container and a header-free Cortex-M image for each
input. `artifacts/` is intentionally ignored by Git.

### Effective XOR operation

The published key is correct after swapping each adjacent byte. Equivalently,
the posted hex is a sequence of displayed 16-bit words whose bytes must be read
in the opposite order for the byte-wise XOR stream.

For published key bytes `P` and effective key bytes `K`:

```text
K[i] = P[i XOR 1]
```

The resulting 128-byte stream is:

```text
76279963bb13ccb1dd89e6586ec4f32c376279961bb13ccb8dd89e65c6ec4f32
63762799b1bb13cc58dd89e62c6ec4f396376279cb1bb13c658dd89e32c6ec4f
99637627ccb1bb13e658dd89f32c6ec4799637623ccb1bb19e658dd84f32c6ec
2799637613ccb1bb89e658ddc4f32c6e62799637b13ccb1bd89e658dec4f32c6
```

Its SHA-256 is
`e13144a3d189ccc11b99d331270e0338b982b476d42e763ad08a03bc0b784c46`.

The first 16 bytes of each OTA file are plaintext. The exact transform is:

```text
output[i] = input[i]                         when i < 0x10
output[i] = input[i] XOR K[i modulo 128]    when i >= 0x10
```

The key phase is based on the absolute file offset; it does not restart at
offset `0x10`.

The supplied `.tar.xz` archive's `.out` files match this result exactly from
offset `0x10` to EOF. Their first 16 bytes were also XORed, however, which
corrupts the plaintext OTA header. The repo script preserves that header.

### Container and load layout

| File offset | Size | Meaning |
| --- | ---: | --- |
| `0x00` | 16 | Plaintext OTA header. Its first little-endian word equals file size minus 16. The remaining fields are not yet named. |
| `0x10` | 8 | Decrypted inner image header. Its field meanings are not yet confirmed. |
| `0x18` | remainder | Raw Cortex-M application image, loaded at address `0x00010000`. |

The first two image words are `0x20003910` and `0x00016a01`. The first is a
plausible initial stack pointer and the second is an in-range odd Thumb address,
but the second word must be described as an **entry candidate**, not a confirmed
Cortex-M reset vector. At `0x00016a00`, one build contains a ten-byte function
tail and the other build's decompiler enters a larger function with
already-live registers. Forcing `Reset_Handler` at that address created a
misleading symbol.

Bytes at image address `0x00010008` look like a startup stub: they load
`0x20003910` into `sp`, call an initializer-record walker at `0x000108e4`, then
transfer through a target stored at `0x00010014`. That final target points to
bytes that decompile as data in both builds. The exact boot contract is
therefore unresolved. What is established is that the Cortex-M image begins at
decrypted file offset `0x18`, not at the `0x400` offset suggested by the earlier
binwalk heuristic.

| Firmware | Correct decrypted container SHA-256 | Raw image SHA-256 |
| --- | --- | --- |
| `TR1906R04-10_OTA.bin` | `70da370a61f74a8d9aef20370f75b83eabe2e390e40150bba1ec25e161f2f556` | `083ae84b313e641037a7ec42d003b6b0965b4dbe4b04231b087a12cdb7be537f` |
| `TR1906R04-1-10_OTA.bin` | `503557e2ed6aba141b5e90834590bd2e40811a80ab8b5285f2acba3d4334a177` | `f53a8cb6619cb605244355932d49dc3b032c8679bf7f5b5a299d24814b22e1de` |

## Static Analysis Setup

The raw images were loaded in Ghidra as `ARM:LE:32:Cortex` binaries at base
address `0x00010000`, with Thumb mode enabled. The two header words are defined
as data. `0x00016a00` is retained as `ImageHeaderEntryCandidate`, with an
explicit unresolved comment, rather than being forced to `Reset_Handler`. Both
builds contain the same BLE parser, cryptography, persistence, and display
handlers after relocation.

Useful function addresses are:

| Function | `R04-10` | `R04-01-10` |
| --- | ---: | ---: |
| BLE service initialization | `0x00011954` | `0x0001185c` |
| Command parser | `0x00011a5c` | `0x00011964` |
| Upload write handler | `0x00011de0` | `0x00011ce8` |
| Visualizer write handler | `0x00011eac` | `0x00011db4` |
| AES encrypt block | `0x00016284` | `0x00016180` |
| AES decrypt block | `0x00018abc` | `0x000189b8` |
| AES-128 key expansion | `0x00018df4` | `0x00018cf0` |
| GATT write dispatcher | `0x000199b8` | `0x000198b4` |
| Encrypted notification builder | `0x0001ac54` | `0x0001ab50` |
| Persist type-1 upload to flash | `0x0001b0c4` | `0x0001afc0` |
| Visualizer frame decoder | `0x0001b2fc` | `0x0001b1f8` |
| Display-mode selector | `0x0001b5c8` | `0x0001b4c4` |
| LED-frame serializer/sender | `0x0001b9c0` | `0x0001b8bc` |

### Reproducible Ghidra import and annotation

The tracked scripts under `build/scripts/ghidra/` replace the earlier
machine-local-only annotations:

- `SetupTr1906Firmware.py` configures Thumb mode, replaces the raw loader's
  initial disassembly with two header dwords, and records the unresolved
  startup/entry evidence.
- `ApplyTr1906Annotations.py` applies the address map for both revisions,
  including evidence-graded function comments and selected parameter/local
  names. It identifies the revision from the exact image range and embedded
  version string and rejects unsupported images rather than guessing from the
  project filename.
- `ExportTr1906Firmware.py` decompiles every discovered function and exports
  function inventory, call graph, variables, instructions, raw strings, and
  unresolved-function inventory.

With Ghidra installed at `C:\Program Files\Ghidra`, a fresh raw-image import is:

```powershell
$ghidra = 'C:\Program Files\Ghidra\support\analyzeHeadless.bat'
$projectDir = 'artifacts\firmware-analysis\ghidra-project'
New-Item -ItemType Directory -Force $projectDir | Out-Null

& $ghidra $projectDir TR1906Firmware `
  -import 'artifacts\firmware-analysis\decrypted\TR1906R04-10_OTA.bin.image.bin' `
  -processor 'ARM:LE:32:Cortex' `
  -loader 'BinaryLoader' `
  -loader-baseAddr '0x00010000' `
  -scriptPath 'build\scripts\ghidra' `
  -preScript 'SetupTr1906Firmware.py' `
  -postScript 'ApplyTr1906Annotations.py'
```

Repeat the import with the `TR1906R04-1-10` image. To refresh annotations in
an existing project without rerunning analysis:

```powershell
& $ghidra $projectDir TR1906Firmware `
  -process 'TR1906R04-10_OTA.bin.image.bin' `
  -noanalysis `
  -scriptPath 'build\scripts\ghidra' `
  -postScript 'ApplyTr1906Annotations.py'
```

Generate a complete review snapshot under ignored `artifacts/` with:

```powershell
& $ghidra $projectDir TR1906Firmware `
  -process 'TR1906R04-10_OTA.bin.image.bin' `
  -readOnly `
  -noanalysis `
  -scriptPath 'build\scripts\ghidra' `
  -postScript 'ExportTr1906Firmware.py' `
  'artifacts\firmware-analysis\annotated-export\TR1906R04-10'
```

The scripts are idempotent: rerunning the annotation pass preserves the same
names and comments instead of accumulating generated aliases.

### What "whole firmware" means for these files

The exporter covers every function Ghidra discovers in each supplied OTA
image, not every byte that exists on the physical device. Final export coverage
is:

| Firmware | Discovered | Decompiled | Meaningfully named | Documented | Deliberately generic |
| --- | ---: | ---: | ---: | ---: | ---: |
| `R04-10` | 230 | 230 | 228 | 230 | 2 |
| `R04-01-10` | 228 | 228 | 225 | 228 | 3 |

The portable map covers compiler arithmetic and formatting helpers, controller
register and timing operations, message queues, the record/attribute database,
link-profile lifecycle, service-handle lookup, AES, the BLE application
protocol, flash persistence, display modes, and all 19 built-in-animation
initializer/tick pairs. Names and comments are evidence graded as `CONFIRMED`,
`INFERRED`, `LOW-CONFIDENCE`, or `UNRESOLVED`. A clean `R04-10` import applies
228 function names/comments and 215 selected parameter/local names in one pass;
an immediate second pass makes zero changes.

Only these Ghidra-discovered entries intentionally retain `FUN_*` names:

| Firmware | Address | Reason |
| --- | ---: | --- |
| `R04-10` | `0x00016730` | The eight-byte body has no stable cross-build boundary and runs into later code. |
| `R04-10` | `0x0001f86c` | The indirect entry target decodes as repetitive invalid instructions/data. |
| `R04-01-10` | `0x0001662c` | Its boundary is incompatible with the paired build and does not expose a stable routine. |
| `R04-01-10` | `0x00016700` | Ghidra finds a four-byte body containing bad instruction data. |
| `R04-01-10` | `0x0001f768` | The indirect entry target decodes as repetitive invalid instructions/data. |

Each unresolved entry still has an explicit Ghidra comment explaining why a
semantic name would be misleading.

The OTA images also contain absolute references beyond their loaded ranges:

- AES key material is read from `0x00022b94` / `0x00022a90`.
- Visualizer palettes are read from `0x00022da8` and `0x00022dd0` in `R04-10`.
- Built-in-animation frame tables span referenced addresses from approximately
  `0x00022f06` through `0x00025cde` in `R04-10`.
- Uploaded content is persisted in flash sectors `0x0003c000` through
  `0x0003c800`.

Those regions, the bootloader, and any complete hardware vector table are not
present in either OTA asset. Their contents cannot be decompiled from these
files. Recovering them requires a full physical flash dump or another firmware
package that actually includes those address ranges.

## BLE GATT Topology

The firmware GATT database confirms service `FFF0` and these 128-bit
characteristics:

| Relative attribute index | UUID | Properties | Firmware use |
| ---: | --- | --- | --- |
| `2` | `d44bc439-abfd-45a2-b575-925416129600` | Write and Write Without Response (`0x0c`) | Encrypted command blocks |
| `5` | `d44bc439-abfd-45a2-b575-92541612960a` | Write and Write Without Response (`0x0c`) | Encrypted streamed upload blocks |
| `8` | `d44bc439-abfd-45a2-b575-92541612960b` | Write and Write Without Response (`0x0c`) | Encrypted live visualizer blocks |
| `11` | `d44bc439-abfd-45a2-b575-925416129601` | Notify (`0x10`) | Encrypted 16-byte responses |
| `12` | standard UUID `2902` | Read/write descriptor | Client Characteristic Configuration Descriptor; writes are ignored by the application dispatcher |

The numbers above are service-relative indexes used by the firmware. A BLE
stack may expose different absolute ATT handles if the service is rebased.

## BLE Block Encryption and Framing

The write dispatcher sends the value received at attribute indexes `2`, `5`,
and `8` through the same 16-byte AES inverse-cipher function before parsing it.
The response path encrypts one 16-byte block and notifies attribute index `11`.
There is no IV, chaining state, or authentication, so this is raw AES-128 block
operation, equivalent to one-block AES-128-ECB packets.

The AES key schedule is stored in RAM at `0x20002f90`. Its initializer reads the
key from `0x00022b94` in `R04-10` and `0x00022a90` in `R04-01-10`; both addresses
are outside their OTA images. The OTA binaries therefore prove the AES
algorithm and packet boundary, but cannot by themselves prove the key bytes.
The Android client and MaskApp use this application-protocol key:

```text
32672f7974ad43451d9c6c894a0e8764
```

The common decrypted block shape is:

| Plaintext offset | Meaning |
| ---: | --- |
| `0` | Payload length in bytes |
| `1..length` | Command or data payload |
| remaining bytes through `15` | Ignored padding; clients use either zeros or random bytes |

The parser has a generic `length <= 20` guard, but the AES routine produces one
16-byte block. A directly compatible sender must therefore keep the useful
payload at 15 bytes or fewer.

## Commands Implemented by These Builds

The command characteristic receives a decrypted block whose payload starts at
plaintext offset `1`. These are the only command names reached by this GATT
dispatcher in both analyzed builds:

| Command | Plaintext arguments | Firmware behavior |
| --- | --- | --- |
| `DATS` | byte `type`, then big-endian 16-bit data length | Resets upload state and replies `DATSOK`. Types `1` and `2` have handlers described below. |
| `DATCP` | none | Checks received count against the expected count. Replies `DATCPOK` on success or `ERROR00` on mismatch. |
| `SPEED` | one byte, nominally `0..100` | Quantizes the value into ten internal timing levels. |
| `SMVEW` | one state byte | Selects or resets live-view state and its 24-element render buffer. |
| `SOUT` | none | Switches to internal display mode `1`, effectively leaving the live view. |
| `LIGHT` | one byte, nominally `0..100` | Quantizes brightness into five internal levels. |
| `LOOP` | none | Switches to internal display mode `0x18`. |
| `ANIM` | one animation id byte | Switches to internal display mode `id + 5`. |
| `CLRL` | none | Clears the `0x60`-byte live-view buffer and submits the cleared buffer. |
| `IMAG` | one image id byte | Stores the built-in id and switches to internal display mode `0x19`. |
| `MODE` | mode byte and optional nonzero flag | Maps modes `1..3` to the normal or alternate internal modes shown below. |

### SPEED mapping

| Input | Internal level |
| ---: | ---: |
| `0..10` | `13` |
| `11..20` | `12` |
| `21..30` | `11` |
| `31..40` | `10` |
| `41..50` | `9` |
| `51..60` | `8` |
| `61..70` | `7` |
| `71..80` | `6` |
| `81..90` | `5` |
| `91..255` | `4` |

### LIGHT mapping

| Input | Internal level |
| ---: | ---: |
| `0..20` | `1` |
| `21..40` | `2` |
| `41..60` | `3` |
| `61..80` | `4` |
| `81..255` | `5` |

### MODE mapping

| Mode argument | Optional flag absent or zero | Optional flag nonzero |
| ---: | ---: | ---: |
| `1` | `1` | `0x1f` |
| `2` | `2` | `0x1d` |
| `3` | `3` | `0x1e` |

Other mode values are ignored. The internal numbers identify firmware render
states; their user-visible effect still needs physical confirmation.

### Commands not handled by this firmware profile

`M`, `FC`, `BC`, `CHEC`, `DELE`, `PLAY`, `TIME`, `FACE`, `MANY`, and `MANCP`
are used or documented by other Shining Mask application profiles, but are not
handled by this service's command parser in either analyzed OTA build.

## Upload Protocol on Characteristic `960A`

The exact start block for these binaries is:

```text
offset  0      1..4       5       6       7       8..15
       +------+----------+-------+-------+-------+--------+
       |  7   |  "DATS"  | type  | lenHi | lenLo | padding|
       +------+----------+-------+-------+-------+--------+
```

Each subsequent upload value is one independently AES-encrypted block:

```text
offset  0       1..length          remaining bytes
       +-------+------------------+----------------+
       | count | streamed data    | ignored padding|
       +-------+------------------+----------------+
```

There is no packet-index field in this firmware handler. In particular, the
first byte after `count` is data, not the frame counter used by the Android
application's separate raw 20/100-byte upload profile.

The type-specific behavior is:

- Type `1` starts its received counter at `48`, sets the expected counter to
  `48 + len`, and alternates incoming bytes between two storage regions. The
  shared offset advances by two after every pair. Storage wraps after `0x600`
  bytes.
- Type `2` interprets the stream as RGB triples, stores each triple as a
  24-bit `0xRRGGBB` value, and compares a received-pixel count against
  `len / 3`. Its active buffer wraps after `0x180` pixels.
- Other type values pass the start parser's non-RGB length calculation but have
  no data-copy branch and therefore cannot complete normally.

`DATCP` has no parsed arguments in these builds. The handler emits only start
and completion responses; no `REOK` per-block response is present in this
firmware path.

### Type-1 flash persistence

After a successful `DATCP` for type `1`, `PersistUploadToFlash` performs this
sequence:

1. Erase the sectors beginning at `0x0003c000`, `0x0003c200`,
   `0x0003c400`, `0x0003c600`, and `0x0003c800`.
2. Write the complete `0x600`-byte upload buffer at `0x0003c000`.
3. Write an eight-byte metadata record at `0x0003c800`.

The known metadata fields are:

| Metadata offset | Size | Meaning |
| ---: | ---: | --- |
| `0` | 4 | Magic `0x5a5a5a6a` |
| `4` | 2 | Received-count value passed by the completion path |
| `6` | 1 | Upload type (`1` on this path) |
| `7` | 1 | Not yet established |

These flash addresses lie outside the supplied OTA image. Static analysis
establishes the erase/write calls and record shape, but not the preexisting
contents, bootloader ownership, or wear/failure behavior of the physical flash.

## Live Visualizer Protocol on Characteristic `960B`

`SMVEW` controls the live-view state stored at RAM address `0x2000309c`.
Visualizer values are AES-encrypted 16-byte blocks using the common length
prefix. The main data branch treats plaintext byte `1` as a packing mode and
expands 4-bit values into a 24-element render buffer. Nibbles above `9` are
clamped to zero.

| Packing mode | Source consumed after mode byte | Expansion to 24 values |
| ---: | ---: | --- |
| `0` | 12 bytes | Low nibble, then high nibble; palette/lookup A |
| `1` | 12 bytes | Low nibble, then high nibble; palette/lookup B |
| `2` | 6 bytes | Each low and high nibble is duplicated |
| `3` | 4 bytes | Each low/high pair expands to two values plus a zero spacer |

Modes `0` and `1` therefore normally use a length byte of `13`; mode `2` uses
`7`. The dispatcher enters this render branch only when length is greater than
`5`, so a mode-`3` packet must declare at least `6` even though the expansion
routine consumes four packed bytes. This path has no explicit ACK.

The visible meaning of the two palettes and `SMVEW` state values requires a
real-device test. Static analysis confirms the framing and expansion, not the
orientation or resulting colors on the LEDs.

## Encrypted Notifications

The notification builder writes a length-prefixed response into a 16-byte
buffer, AES-encrypts it, and sends exactly 16 bytes on characteristic `9601`.

| Response | Trigger in these builds |
| --- | --- |
| `DATSOK` | `DATS` accepted. The firmware passes length `7`, so the six ASCII bytes are followed by a NUL inside the declared payload. |
| `DATCPOK` | `DATCP` received count matches expected count. |
| `ERROR00` | `DATCP` count mismatch. |

No `DATOK`, `REOK`, `REOKOK`, `PLAYOK`, `DELEOK`, `FACEOK`, `TIMEOK`, or
`TIMEERR` response is generated by this parser path.

## Important Android-App Compatibility Mismatch

The decompiled Android application and these OTA binaries agree on the service,
UUIDs, 16-byte command encryption, and encrypted notification format. They do
not agree on the upload protocol:

| Area | These OTA binaries | Decompiled Android DIY/text path |
| --- | --- | --- |
| `DATS` arguments | `type` + one 16-bit byte length | Text sends total length + text length; DIY sends image length + slot metadata |
| Upload encryption | AES-decrypts one 16-byte block on `960A` | Calls `writeCharacteristicBy2` with raw 20- or 100-byte values; the base `onEncrypt` hook is identity |
| Upload payload | Length + data, no frame index | Length + frame index + 18 or 98 data bytes |
| Per-frame ACK | None in this handler | Client waits for `REOK`/`REOKOK` when available |
| DIY slot commands | Not handled | Uses `CHEC`, `DELE`, `PLAY`, `TIME`, and related responses |

This is not a minor padding difference: the two upload shapes cannot be sent to
the same parser unchanged. The likely explanation is that the OTA assets target
a narrower or older `GLASSES-` device profile while the application also
supports a different Shining Mask firmware family. Shipping an OTA asset in the
APK does not prove that a connected mask is currently running it.

Do not change MaskApp's physically working raw 100-byte DIY upload path solely
from this firmware analysis. Before implementing this alternate profile, capture
the connected device's advertised name, discovered UUIDs, firmware/version
identity, write lengths, and encrypted notifications on a real device.

## Confidence and Remaining Work

High-confidence static findings:

- OTA XOR transform, header boundary, and Cortex-M load mapping.
- GATT UUIDs, characteristic properties, and relative attribute indexes.
- One-block AES processing on all three inbound characteristics and responses.
- AES-128 key expansion plus the forward/inverse round helpers used by those
  block operations.
- The exact command names and branch mappings listed above.
- Upload type `1`/`2` data handling and visualizer nibble expansion.
- Type-1 flash erase/write layout and its metadata record.
- The 24-pixel LED packet serializer, RGB565 expansion, additive checksum, and
  distinction between normal and live-frame submission.
- Control-flow roles for all 19 built-in-animation initializer/tick pairs,
  although their frame tables are absent.
- Equivalent BLE behavior in both firmware revisions after relocation.

Not yet established:

- Meanings of the remaining OTA and image-header fields, including the exact
  startup contract of the `0x00016a01` entry candidate.
- The AES key bytes stored outside each OTA image, independent of app evidence.
- Contents of the external palette, built-in-animation, bootloader, and flash
  regions referenced by the OTA code.
- Which physical mask/glasses hardware revisions run these binaries.
- LED orientation, palette colors, timing, and recovery behavior for this
  alternate profile.
- Any protocol implemented by bootloader or flash regions absent from the OTA
  images.
