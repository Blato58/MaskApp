# Product And Migration Progress

Use this file as the source of truth for product and migration status. Update it
in the same change that moves a slice, adapter, or validation gate forward.

Product goal: MaskApp should become a wearable face controller. The user should
be able to open the app, pick a mood, reaction, caption, image, beat mode, or
prepared pack, preview it, and make the mask become an expressive face within
seconds.

Migration remains the implementation path, not the product goal. A slice should
not be considered complete just because code was moved; it should also move the
app toward the product vision in `docs/product-vision.md`.

Status key:

- `[x]` done and validated
- `[~]` started, partially implemented, or compile-only validated
- `[ ]` not started
- `[!]` blocked or needs a product/platform decision

Capability confidence and physical validation status are defined in
`docs/product-vision.md` and `docs/modernization-slice-template.md`.

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
- [x] Add wearable face controller product vision, capability-confidence model, RAVE MVP definition, and overclaim guardrails.
- [x] Add stock Shining Mask protocol reference from community reverse-engineering evidence for BLE, Text, Image, Rhythm, DIY, and RAVE FAST work.
- [x] Add real-mask validation checklist for iPhone festival readiness testing.
- [x] Refresh the MAUI shell with a dark neon visual system, mode tab icons, mask branding, and compact operation-first layouts across Control, React, Text, RAVE, Built-ins, and Connect.
- [x] Harden the live festival UX around five visible tabs: Control, React, RAVE, Faces, and Connect. Text Composer stays reachable from Control, React, and RAVE instead of occupying a root tab that can push Faces under More.

## Product Milestones

| Milestone | Status | Capability confidence | Physical validation status | Next step |
| --- | --- | --- | --- | --- |
| Text validation/fix | [~] | Implemented | Needs real-mask test | Protocol constants, text `MODE`, ACK parsing, upload-characteristic routing, and quick-caption defaults are corrected for real-mask testing; validate Flash/Blink mode, speed 100, ACK-required, and write-only upload on physical iOS first, then Android. |
| Control Room | [~] | Implemented | Needs real-mask test | Validate Control Room connect recovery, blackout, brightness, random reaction, and recent/favorite reaction sends on a real mask. |
| Reaction Deck MVP | [~] | Implemented | Needs real-mask test | Validate one-tap short captions, BLACKOUT, Random Reaction, and write-only fallback on a real mask. |
| RAVE MVP entry point | [~] | Implemented | Needs real-mask test | Validate manual RAVE buttons, command fallbacks, brightness cap, BLACKOUT, Festival Lock, and reconnect affordance on physical iOS; follow RAVE FAST guidance in `docs/stock-mask-protocol.md`. |
| Built-in Faces Deck + Archive | [~] | Implemented | Needs real-mask test | Faces tab now defaults to a fast Favorite Faces deck for favorite/working metadata records, keeps Built-in Scanner and Archive/Edit flows, autosaves one-tap Favorite/Works/Bad/Weird status changes, surfaces command-only faces in React/RAVE, and keeps compact live statuses; validate useful IDs and persistence on a real mask/iPhone. |
| Preset Library and Mask Packs | [~] | Implemented | Docs-only | Built-in archive JSON persistence and MaskPack manifest models/docs now exist; broader preset library, package import/export, preview, upload, and playback remain future work. |
| Image Studio and DIY slots | [ ] | Protocol-documented | Needs real-mask test | Implement from `docs/stock-mask-protocol.md`; validate image upload, `CHEC`, `DELE`, and `PLAY` before relying on slot sequencing. |
| Rhythm and RAVE Labs | [ ] | Experimental | Needs real-mask test | Test visualizer protocol from `docs/stock-mask-protocol.md`, audio behavior, Drop Detector, Voice Mouth, Bass Face, GIF-ish playback, and real-time effects before product claims. |
| AI Composer | [ ] | Vision | Docs-only | Future MaskPack generation/import guidance is documented, but no AI/API code is implemented; add offline templates first when this slice starts. |
| Device reliability | [~] | Implemented | Needs real-mask test | Add reconnect, known device memory, command queue visibility, diagnostics, and device checklist. |
| Apple Watch Quick Deck + Mode Switcher | [ ] | Vision | Docs-only | Backlog only; phone-side quick-action and mode IDs now exist, but no watchOS code is implemented. |

## Feature Slices

| Slice | Status | Current scope | UI/UX or functionality goal | Next step |
| --- | --- | --- | --- | --- |
| Connect | [~] | MAUI page, view model, contracts, iOS CoreBluetooth adapter, Android Bluetooth LE adapter, unit tests, Control Room recovery entry point, iOS/Android compile validation. | Make discovery and connection state visible, predictable, and recoverable from Control Room and RAVE instead of hiding behavior in a utility tab. | Validate scanning and connection on real iOS and Android devices. |
| Mask control MVP | [~] | Power-as-dim/restore, brightness, image preset, animation preset, encrypted command builders, simulator transport for unsupported targets, diagnostics UI, exact command characteristic routing, iOS write characteristic discovery/debug logging, and Android BLE command writes. | Let the user interact with the mask from Control Room/RAVE while keeping command readiness, active transport, and failures visible. | Validate encrypted writes on physical iOS and Android devices and confirm preset IDs against real hardware. |
| Control Room | [~] | Home page/view model now shows control/text transport status, recovery hints, brightness, BLACKOUT, restore, random reaction, favorite reactions, recent reactions, Text Composer entry, and persisted quick-caption settings through shared quick-action intents. | Make the first screen useful for live mask operation instead of showing a migration roadmap. | Validate open-connect, blackout, brightness cap, Flash/Blink quick captions, and short reactions on a real mask. |
| Reaction Deck | [~] | React page/view model groups short offline reactions by catalog category, pins BLACKOUT plus Random Reaction, links to Text Composer, and can show favorite archived built-ins as command-only one-tap faces when saved records exist. | Give the wearer a one-tap deck for social/meme/welfare reactions and reusable stock faces. | Validate short caption sends, favorite built-in sends, and write-only/ACK status on a real mask. |
| RAVE MVP | [~] | RAVE page/view model provides large manual DnB/festival buttons, command-only built-in fallbacks, favorite archived built-ins under secondary controls, BLACKOUT, brightness cap, Festival Lock, sticky Text/Connect/BLACKOUT actions, concise live statuses, and shared intent dispatch. | Make the mask useful in a loud/dark festival setting without internet, AI, microphone, or automation. | Validate physical text timing, command fallbacks, favorite built-ins, blackout, brightness cap, and reconnect in iOS real-mask testing. |
| Built-in Faces Deck + Archive | [~] | Faces page/view model sends static image and animation IDs through command transport, shows a fast Favorite Faces deck for favorite/working records, keeps Built-in Scanner and Archive/Edit detail fields, autosaves one-tap Favorite/Works/Bad/Weird controls, persists metadata in local JSON, surfaces command-only faces in React/RAVE, keeps BLACKOUT visible, and uses compact live statuses. | Let the wearer scroll useful stock-firmware visuals and tap once to send while still recording scan notes after physical testing. | Test `IMAG`/`ANIM` IDs on physical iPhone/mask, mark useful records with one tap, reopen the app, confirm deck persistence, and confirm React/RAVE face actions. |
| Neon shell UI refresh | [x] | Shared dark theme resources, SVG mask/tab assets, action/state color helpers, React filter state, Text 64-character guard, and redesigned Control, React, Text, RAVE, Built-ins, and Connect pages. | Make the implemented features feel like a fast wearable face controller instead of a light migration workbench. | Validate refreshed UI on a physical iPhone and Android device after BLE/text hardware testing is available. |
| Settings | [ ] | Java source mapped only. | Rebuild settings as clear, grouped controls with explicit saved state and platform-appropriate permission paths. | Define settings model and iOS-first page behavior. |
| Text | [~] | Deterministic ASCII glyph rasterizer, text upload payload/framing, encrypted `DATS`/`DATCP`/`MODE`/speed commands, documented text modes, broader ACK parsing, simulator transport, MAUI composer, LED preview, diagnostics, transport state events, ACK capability reporting, ACK-required upload mode, write-only compatibility upload mode, tests, iOS upload transport, Android BLE upload transport, and compile validation. | Improve text creation/editing flow with immediate preview feedback, visible transport readiness, explicit ACK/write-only mode, send progress, and actionable diagnostics before claiming real hardware parity. | Validate ACK-required and write-only text upload on physical iOS first, then Android; expand glyph parity beyond the MVP set. |
| Image | [ ] | Java source mapped only. | Make image import/crop/preview understandable and enforce mask limits before sending data. | Map image storage, picker, crop, and LED transform behavior. |
| Rhythm | [ ] | Java source mapped only. | Provide clear audio/rhythm state, permission handling, and stop/recovery behavior. | Decide audio capture/visualizer approach for iOS. |
| Microphone | [ ] | Java source mapped only. | Separate microphone capture state from rhythm playback and make permission failures actionable. | Define iOS microphone permission and capture slice. |

## Platform Adapters

| Adapter | iOS | Android | Notes |
| --- | --- | --- | --- |
| BLE scan/connect | [~] | [~] | iOS CoreBluetooth adapter compiles; Android Bluetooth LE adapter scans by advertisement data, requests runtime permissions, and connects through GATT. Physical-device validation is still open. |
| BLE mask commands | [~] | [~] | iOS and Android discover service `0000fff0-0000-1000-8000-00805f9b34fb`; encrypted commands use `d44bc439-abfd-45a2-b575-925416129600`; ACK notifications prefer `d44bc439-abfd-45a2-b575-925416129601`; text/image frames prefer `d44bc439-abfd-45a2-b575-92541612960a` with command-characteristic compatibility fallback; audio visualization UUID `d44bc439-abfd-45a2-b575-92541612960b` is documented only. Physical validation remains open. |
| Permissions | [~] | [~] | iOS Info.plist and Android manifest include current permission baseline. Runtime request UX still belongs to slices. |
| Storage | [~] | [~] | Built-in archive metadata uses simple versioned JSON in app data. GreenDAO replacement for broader migrated data is not selected yet. |
| Camera/media picker | [ ] | [ ] | iOS usage strings added; implementation not started. |
| Audio capture | [ ] | [ ] | iOS microphone usage string added; implementation not started. |

## Core Migration

- [x] BLE advertisement product matcher ported from `android/base/app/BleConfig.java`.
- [x] Connect slice state and BLE abstraction contracts added.
- [~] Port `Agreement` command builders with tests before UI wiring. MVP coverage includes AES-128 ECB `LIGHT`, `IMAG`, and `ANIM` commands from Java plus external reverse-engineered protocol evidence; built-in fallback quick actions now dispatch `IMAG`/`ANIM` through command transport.
- [x] Add built-in archive metadata models, fast favorite/working Faces deck behavior, and MaskPack manifest models/parser for future app-owned custom content. Built-in archive records are metadata only and do not extract stock mask frames; Custom Static Faces, Custom Animations, MaskPacks, and Mood/Expression categories remain future/Labs until upload/playback validation.
- [x] Port text upload frame splitting and acknowledgement parsing with tests.
- [ ] Port stock-protocol `MODE`, `M`, `FC`, and `BC` text controls with focused tests before exposing advanced text styling.
- [ ] Port DIY/image frame splitting and acknowledgement parsing from `docs/stock-mask-protocol.md` with tests.
- [ ] Port audio visualizer packet builder from `docs/stock-mask-protocol.md` behind Rhythm/RAVE Labs with tests.
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
- [x] RAVE FAST MVP validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`, Roslyn diagnostics, `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`, and `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`.
- [x] Real-mask RAVE validation slice compile/test validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` (76 tests), Roslyn diagnostics (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios` (0 warnings, 0 errors), and `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android` (0 warnings, 0 errors).
- [x] Built-in archive and MaskPack format slice validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` (86 tests), Roslyn diagnostics (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android` (0 warnings, 0 errors), and `git diff --check`.
- [x] Neon shell UI refresh validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` (92 tests), Roslyn diagnostics (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android` (0 warnings, 0 errors), and `git diff --check`.
- [x] Built-in Faces deck UX validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` (101 tests), Roslyn test impact, `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android` (0 warnings, 0 errors), and `git diff --check`.
- [x] Festival UX hardening validated with Roslyn diagnostics (0 warnings, 0 errors), `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` (103 tests), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android` (0 warnings, 0 errors), and `git diff --check`.
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
- [!] Move Experimental features to product capability only after physical validation, especially Drop Detector, Voice Mouth, Bass Face, GIF-ish playback, fast DIY sequencing, and real-time effects.
- [x] Choose a stable app-layer quick-action and mode intent ID model before any future Apple Watch companion work.
