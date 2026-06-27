# Migration Progress

Use this file as the source of truth for migration status. Update it in the same change that moves a slice, adapter, or validation gate forward.

Migration goal: improve UI/UX and functionality while preserving required behavior from the old Android app. A slice should not be considered complete just because code was moved; it should also make the user flow clearer, more reliable, easier to validate, or better aligned with iOS/MAUI platform conventions.

Status key:

- `[x]` done and validated
- `[~]` started, partially implemented, or compile-only validated
- `[ ]` not started
- `[!]` blocked or needs a product/platform decision

## Foundation

- [x] Set MAUI app identity to `app.turquoise6409.green2444`, display version `1.2.6`, build `126`; keep the original Android app id only in source-map evidence.
- [x] Keep Java snapshot under `android/` as read-only migration evidence.
- [x] Create MAUI app project at `src/MaskApp.App`.
- [x] Target iOS first with Android as a secondary target.
- [x] Keep platform-neutral code in `src/MaskApp.Core`.
- [x] Use vertical slice folders for migrated app features.
- [x] Add a modern workbench home screen and tab navigation that makes available and locked migration slices visible.
- [x] Install and document `maui-mobile` workload.
- [x] Add GitHub Actions macOS IPA distribution workflow, signing-secret setup docs, and Feather/AltStore-style Pages output generation.
- [x] Add modernization execution plan, readiness checklist, slice template, and per-slice record folder.

## Feature Slices

| Slice | Status | Current scope | UI/UX or functionality goal | Next step |
| --- | --- | --- | --- | --- |
| Connect | [~] | MAUI page, view model, contracts, iOS CoreBluetooth adapter, Android Bluetooth LE adapter, unit tests, workbench navigation, iOS/Android compile validation. | Make discovery and connection state visible, predictable, and recoverable inside a modern operational workbench instead of hiding behavior inside a large activity. | Validate scanning and connection on real iOS and Android devices. |
| Mask control MVP | [~] | Power-as-dim/restore, brightness, image preset, animation preset, encrypted command builders, simulator transport for unsupported targets, diagnostics UI, iOS write characteristic discovery/debug logging, and Android BLE command writes. | Let the user interact with the mask from the first screen while keeping command readiness, command payloads, active transport, and failures visible. | Validate encrypted writes on physical iOS and Android devices and confirm preset IDs against real hardware. |
| Settings | [ ] | Java source mapped only. | Rebuild settings as clear, grouped controls with explicit saved state and platform-appropriate permission paths. | Define settings model and iOS-first page behavior. |
| Text | [~] | Deterministic ASCII glyph rasterizer, text upload payload/framing, encrypted `DATS`/`DATCP`/mode/speed commands, ACK parsing, simulator transport, MAUI composer, LED preview, diagnostics, transport state events, ACK capability reporting, ACK-required upload mode, write-only compatibility upload mode, tests, iOS upload transport, Android BLE upload transport, and compile validation. | Improve text creation/editing flow with immediate preview feedback, visible transport readiness, explicit ACK/write-only mode, send progress, and actionable diagnostics before claiming real hardware parity. | Validate ACK-required and write-only text upload on physical iOS first, then Android; expand glyph parity beyond the MVP set. |
| Image | [ ] | Java source mapped only. | Make image import/crop/preview understandable and enforce mask limits before sending data. | Map image storage, picker, crop, and LED transform behavior. |
| Rhythm | [ ] | Java source mapped only. | Provide clear audio/rhythm state, permission handling, and stop/recovery behavior. | Decide audio capture/visualizer approach for iOS. |
| Microphone | [ ] | Java source mapped only. | Separate microphone capture state from rhythm playback and make permission failures actionable. | Define iOS microphone permission and capture slice. |

## Platform Adapters

| Adapter | iOS | Android | Notes |
| --- | --- | --- | --- |
| BLE scan/connect | [~] | [~] | iOS CoreBluetooth adapter compiles; Android Bluetooth LE adapter scans by advertisement data, requests runtime permissions, and connects through GATT. Physical-device validation is still open. |
| BLE mask commands | [~] | [~] | iOS discovers service `0000fff0-0000-1000-8000-00805f9b34fb`, writes encrypted commands to characteristic `d44bc439-abfd-45a2-b575-925416129600`, reports text upload readiness separately from control readiness, supports ACK-required text upload when notify/indicate characteristics are found, supports write-only compatibility upload when ACK notifications are missing, and logs discovery/write details in debug builds; Android uses the same service/characteristic and mirrored text upload readiness/options. |
| Permissions | [~] | [~] | iOS Info.plist and Android manifest include current permission baseline. Runtime request UX still belongs to slices. |
| Storage | [ ] | [ ] | GreenDAO replacement not selected yet. |
| Camera/media picker | [ ] | [ ] | iOS usage strings added; implementation not started. |
| Audio capture | [ ] | [ ] | iOS microphone usage string added; implementation not started. |

## Core Migration

- [x] BLE advertisement product matcher ported from `android/base/app/BleConfig.java`.
- [x] Connect slice state and BLE abstraction contracts added.
- [~] Port `Agreement` command builders with tests before UI wiring. MVP coverage includes AES-128 ECB `LIGHT`, `IMAG`, and `ANIM` commands from Java plus external reverse-engineered protocol evidence.
- [x] Port text upload frame splitting and acknowledgement parsing with tests.
- [ ] Port DIY/image frame splitting and acknowledgement parsing with tests.
- [ ] Map GreenDAO entities and choose persistence replacement.

## Validation

- [x] `dotnet restore MaskApp.slnx`
- [x] `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
- [x] `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore`
- [x] `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore`
- [x] Roslyn diagnostics: no warnings or errors.
- [x] Diagnostics/logging slice validated with core tests plus iOS and Android target builds.
- [x] Text MVP validated with core tests plus iOS and Android target builds.
- [x] Text repair slice core tests cover transport readiness events, ACK-required upload options, write-only compatibility upload options, unavailable transport, empty text, and diagnostics.
- [x] Text repair slice validated with `dotnet test`, `dotnet build -f net10.0-ios`, and `dotnet build -f net10.0-android`.
- [x] Android BLE adapter compile validation for scan/connect, command writes, and text upload transport wiring.
- [~] iOS CI distribution scripts validated locally; signed IPA build awaits GitHub Actions secrets and macOS runner execution.
- [ ] iOS simulator launch smoke test.
- [ ] iOS physical-device BLE scan/connect validation.
- [ ] iOS physical-device encrypted command write validation.
- [ ] iOS physical-device text upload ACK/write validation.
- [ ] Android physical-device BLE scan/connect validation.
- [ ] Android physical-device encrypted command write validation.
- [ ] Android physical-device text upload ACK/write validation.
- [ ] Android emulator UI smoke test.

## Open Decisions

- [x] Align MAUI app id with the current CI provisioning profile: `app.turquoise6409.green2444`.
- [!] Decide whether to keep the third-party provisioned app id long term or later move to a Blato58-owned Apple Developer profile.
- [!] Confirm minimum supported iOS version.
- [!] Confirm minimum supported Android API level beyond the current compile baseline.
- [!] Choose persistence strategy for migrated GreenDAO data.
