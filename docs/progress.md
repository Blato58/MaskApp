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
- [x] Add optional ntfy webhook notification for release-backed iOS update publishes.
- [x] Add modernization execution plan, readiness checklist, slice template, and per-slice record folder.
- [x] Add wearable face controller product vision, capability-confidence model, RAVE MVP definition, and overclaim guardrails.
- [x] Add stock Shining Mask protocol reference from community reverse-engineering evidence for BLE, Text, Image, Rhythm, DIY, and RAVE FAST work.
- [x] Refresh the MAUI shell with a dark neon visual system, mode tab icons, mask branding, and compact operation-first layouts across Control, React, Text, RAVE, Built-ins, and Connect.
- [x] Replace the live-use shell with three visible tabs: Library, Pages, and Device. React, RAVE, and Faces are no longer root tabs; their sendable text, quick-action, and built-in face/animation content is represented through Library and Pages. Text Composer and the built-in scanner/archive stay reachable as manage/add routes.
- [x] Adapt the attached iPhone UI concept into the MAUI shell: Library Browse/Arrange, Library filter/group/add/manage sheets, Pages Use/Manage, page add/edit/add-items/delete-confirmation surfaces, and a Device dashboard with real BLE state plus brightness presets.
- [x] Polish festival/live-use UI around lock-free RAVE controls, foreground-only auto-connect, known-mask memory, and global quick-caption text color. Festival Lock is removed from normal UI; BLACKOUT stays always visible.

## Product Milestones

| Milestone | Status | Capability confidence | Physical validation status | Next step |
| --- | --- | --- | --- | --- |
| Text validation/fix | [~] | Implemented | Needs real-mask test | Physical feedback corrected the issue from cold-start behavior to every-send static pre-roll: quick-caption text became visible before Blink `MODE 2` was applied. Quick captions now default to Low-static Flash: centered 44-column Blink at speed 50, no per-send `MODE 1` reset, pre-upload `SPEED 50`/`MODE 2`, and immediate post-`DATCP` `MODE 2`. Stable Flash remains available as the black-`BC` reset fallback; Fast Flash remains unstable. Validate repeated React/RAVE sends on physical iOS first, then Android. |
| Control Room | [~] | Implemented | Needs real-mask test | Control is now a focused connection and brightness tab only. Validate scan, connect, disconnect, foreground auto-connect, remembered-mask recovery, brightness, BLACKOUT, and restore on a real mask. |
| Reaction Deck MVP | [~] | Implemented | Needs real-mask test | Validate one-tap short captions, BLACKOUT, Random Reaction, and write-only fallback on a real mask. |
| RAVE MVP entry point | [~] | Implemented | Needs real-mask test | Validate manual RAVE buttons, command fallbacks, favorite faces, brightness cap, BLACKOUT, lock-free live controls, and reconnect/auto-connect affordance on physical iOS; follow RAVE FAST guidance in `docs/stock-mask-protocol.md`. |
| Built-in Faces Deck + Archive | [~] | Implemented | Needs real-mask test | Built-in face and animation records now surface through Gallery and Pages, while the scanner/archive remains a hidden manage/add route. Validate useful IDs, send status, and persistence on a real mask/iPhone. |
| Preset Library and Mask Packs | [~] | Implemented | Needs real-mask test | Built-in archive JSON persistence, MaskPack manifest models/docs, and local text preset JSON persistence now exist. Text presets include Czech Basic, Czech Meme, Czech Political/Satire, Czech RAVE, custom presets, favorites, direct Composer open/send, duplicate/edit/delete, per-caption style, bold, manual 3-line centered layout, and React/RAVE/Control visibility. Package import/export, image presets, rhythm presets, and broader MaskPack playback remain future work. |
| Image Studio and DIY slots | [ ] | Protocol-documented | Needs real-mask test | Implement from `docs/stock-mask-protocol.md`; validate image upload, `CHEC`, `DELE`, and `PLAY` before relying on slot sequencing. |
| Rhythm and RAVE Labs | [ ] | Experimental | Needs real-mask test | Test visualizer protocol from `docs/stock-mask-protocol.md`, audio behavior, Drop Detector, Voice Mouth, Bass Face, GIF-ish playback, and real-time effects before product claims. |
| AI Composer | [ ] | Vision | Docs-only | Future MaskPack generation/import guidance is documented, but no AI/API code is implemented; add offline templates first when this slice starts. |
| Device reliability | [~] | Implemented | Needs real-mask test | Known-mask memory and foreground auto-connect settings now exist for app-open reconnect. Validate real iPhone/mask discovery, remembered-device ID matching, name fallback behavior, disable, and forget-mask flow. |
| Apple Watch Quick Deck + Mode Switcher | [ ] | Vision | Docs-only | Backlog only; phone-side quick-action and mode IDs now exist, but no watchOS code is implemented. |

## Feature Slices

| Slice | Status | Current scope | UI/UX or functionality goal | Next step |
| --- | --- | --- | --- | --- |
| Library | [~] | Shared gallery projection for text presets, quick actions, built-in static faces, built-in animations, future/Labs placeholders, JSON-backed manual ordering, search with explicit keyboard dismissal, favorites filter, full-row grouping selector, Browse/Arrange modes, full-screen add route, text-preset edit routing, flat grouped list rendering, multi-select edit actions, text-only bulk delete, send/manage actions, and two-column MAUI cards. | Put every reusable sendable item in one searchable, groupable, reorderable place instead of splitting it across React, RAVE, and Faces tabs. | Validate Library rendering, keyboard behavior, scrolling smoothness, add/edit routes, and sends on physical iPhone/mask. |
| Pages | [~] | Concept-style shortcut page card with Use/Manage segmented mode, manage action row, page dots, four-column shortcut grid, add tile, delete badges, reorder controls, page color, multiple pages, dedicated per-page add screen, choose existing Gallery item, custom shortcut label/icon/color, curated Mask/Lucide/Material/Phosphor icon catalog, remove without deleting source, page editor/delete-confirmation surfaces, and shared send routing. | Let the wearer prepare live pages of small icon/text shortcuts for fast use while wearing the mask. | Validate page ergonomics, add-screen navigation, icon preview clarity, and send behavior on a physical iPhone/mask. |
| Connect | [~] | MAUI page, view model, contracts, iOS CoreBluetooth adapter, Android Bluetooth LE adapter, unit tests, Control Room recovery entry point, iOS/Android compile validation. | Make discovery and connection state visible, predictable, and recoverable from Control Room and RAVE instead of hiding behavior in a utility tab. | Validate scanning and connection on real iOS and Android devices. |
| Mask control MVP | [~] | Power-as-dim/restore, brightness, image preset, animation preset, encrypted command builders, simulator transport for unsupported targets, diagnostics UI, exact command characteristic routing, iOS write characteristic discovery/debug logging, and Android BLE command writes. | Let the user interact with the mask from Control Room/RAVE while keeping command readiness, active transport, and failures visible. | Validate encrypted writes on physical iOS and Android devices and confirm preset IDs against real hardware. |
| Control Room | [~] | Control tab now shows BLE scan/stop/connect/disconnect, foreground-only auto-connect, known-mask memory, brightness slider/apply, BLACKOUT, restore, transport readiness, and nearby-mask selection. Reaction strips, quick-caption settings, effect presets, and built-in scanner/archive controls moved out of Control. | Keep recovery and brightness controls predictable while content sending moves to Gallery and Pages. | Validate open-connect, remembered-mask auto-connect, blackout, and brightness on a real mask. |
| Reaction Deck | [~] | React is no longer a root tab. Quick actions and local text presets now appear in Gallery and can be added to Pages. Existing React view-model code remains as reusable implementation evidence until fully retired. | Give the wearer one searchable/reorderable reaction library instead of a separate reaction tab. | Validate short preset sends, mask-safe transliteration, and write-only/ACK status from Gallery/Pages on a real mask. |
| RAVE MVP | [~] | RAVE is no longer a root tab. RAVE text presets, hardcoded RAVE quick actions, and future/Labs placeholders now surface through Gallery and Pages. Festival Lock remains removed from normal UI. | Let RAVE content be organized into prepared pages instead of a separate mode tab. | Validate physical text timing, Czech RAVE preset sends, command fallbacks, blackout, brightness, and reconnect/auto-connect from Gallery/Pages/Control. |
| Built-in Faces Deck + Archive | [~] | Built-in scanner/archive remains available through Gallery manage/add. Favorite/working/tested built-in static image and animation records project into Gallery and can be placed on Pages. | Let the wearer send useful stock-firmware visuals from the same gallery/page system as captions and quick actions. | Test `IMAG`/`ANIM` IDs on physical iPhone/mask, mark useful records, reopen the app, confirm Gallery/Page persistence and sends. |
| Neon shell UI refresh | [x] | Shared dark theme resources, SVG mask/tab assets, action/state color helpers, React filter state, Text 64-character guard, and redesigned Control, React, Text, RAVE, Built-ins, and Connect pages. | Make the implemented features feel like a fast wearable face controller instead of a light migration workbench. | Validate refreshed UI on a physical iPhone and Android device after BLE/text hardware testing is available. |
| Settings | [ ] | Java source mapped only. | Rebuild settings as clear, grouped controls with explicit saved state and platform-appropriate permission paths. | Define settings model and iOS-first page behavior. |
| Text | [~] | Deterministic ASCII glyph rasterizer, text upload payload/framing, encrypted `DATS`/`DATCP`/`MODE`/speed commands, documented text modes, broader ACK parsing, simulator transport, MAUI composer, debounced/list-replaced LED preview, truncated diagnostics, transport state events, ACK capability reporting, ACK-required upload mode, write-only compatibility upload mode, `TextSendProfile`/`TextSendPlan` package factory, Low-static/Stable/Reliable quick-caption profiles with Fast Flash marked unstable, explicit Composer Scroll/Centered/3-line centered profiles, per-preset foreground color/layout/mode/speed/send profile/bold style, Czech transliteration with original/mask-safe UI, local JSON text preset persistence, Text Composer preset list/open/direct send/save/edit/duplicate/delete, optional fail-soft black `BC` background reset, serialized upload transactions, quick-caption pre-upload `SPEED`/`MODE` arming, immediate post-`DATCP` `MODE` before `SPEED`, stable speed/mode repeat fallback, quick-caption 44-column centered/fitted/two-line payloads, black/off quick-caption blank columns, tests, iOS upload transport, Android BLE upload transport, and compile validation. | Improve text creation/editing flow with smooth preview feedback, reusable presets, visible transport readiness, explicit ACK/write-only mode, deterministic quick-caption send profiles, send progress, no crash on text tap, no typing freeze, reduced per-send static pre-roll, per-caption style, direct unsaved draft sends, manual 3-line centered captions, bold styling, and actionable diagnostics before claiming real hardware parity. | Revalidate the Low-static Flash default, per-preset colors, bold payloads, manual 3-line centered captions, and Czech mask-safe captions on physical iOS: repeated centered 44-column Blink at speed 50 should begin blinking immediately or noticeably faster than the old ~300 ms static pre-roll, and preset colors should display from React/RAVE/Text Composer. Compare Stable Flash fallback and keep Fast Flash failed/unstable until a later pacing change proves it no longer left-aligns or stays solid. |
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
- [~] Port stock-protocol `MODE`, `SPEED`, and black `BC` reset for text controls. Colored `BC` background styling was tried on a real mask and should stay disabled; `M`/`FC` and broader styling remain unexposed.
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
- [x] P0 text crash/freeze and quick-caption layout hotfix validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` (116 tests), Roslyn diagnostics (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android` (0 warnings, 0 errors), and `git diff --check`.
- [x] Physical-feedback follow-up for quick-caption sends and fast typing: quick actions now use paced Fast write-only sends, and Text Creator preview refresh is debounced so typing does not synchronously rebuild the LED grid; validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` (116 tests), Roslyn diagnostics (0 warnings, 0 errors), iOS build, and Android build.
- [x] Physical text-display regression follow-up after `bb11987`: zero-delay quick-caption writes were too aggressive for the real mask, so Fast write-only now uses 20 ms inter-frame pacing while staying faster than 40 ms compatibility mode.
- [x] Quick-caption artifact fix: `VIBE CHECK` now renders as two centered rows and quick-caption blank columns upload as black/off color bytes to avoid right-side rectangle artifacts; core tests cover the package layout and color payload.
- [x] Quick-caption repeated-send stabilization: text uploads now pass through a shared serialized transport, request a pre-upload `MODE 1` reset, and keep the Fast write-only 20 ms inter-frame delay; core tests cover serialization and quick-action option shape.
- [x] Deterministic text-send profile slice validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` (133 tests), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android` (0 warnings, 0 errors), and `git diff --check`. Stable Flash is the default quick-caption profile until physical testing proves Fast Flash reliable.
- [x] Physical text profile feedback: core physical text checks otherwise passed, but Fast Flash still produced left-aligned and solid text, text backgrounds looked bad, and the best observed text path was Text Creator Centered 44-column + Blink at speed 50. Defaults now follow that profile and send an explicit fail-soft black `BC` reset to clear stale mask background state.
- [x] Low-static quick-caption sequencing validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` (137 tests), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android` (0 warnings, 0 errors), and `git diff --check`. Low-static Flash is now the default quick-caption profile; it skips the per-send display reset/black `BC` delay, pre-arms `SPEED 50` and `MODE 2`, and sends post-upload `MODE 2` immediately before speed.
- [x] Festival/live polish with lock-free RAVE, known-mask memory, foreground-only auto-connect, global quick-caption text color, and Text Composer global color defaults validated with core tests, iOS build, Android build, and `git diff --check`. Physical validation remains required for auto-connect and color appearance on the real mask.
- [x] Text presets and Czech starter packs validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore` (174 tests), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore` (0 warnings, 0 errors), and `git diff --check`.
- [x] Text Composer preset-list, manual 3-line centered, and bold style slice validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore` (182 tests), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore` (0 warnings, 0 errors), and `git diff --check`.
- [x] Gallery/Pages/Control redesign validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore` (191 tests), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore` (0 warnings, 0 errors), and `git diff --check`. Physical validation remains required for rendered phone ergonomics, BLE scan/connect, auto-connect, brightness, and real mask sends.
- [x] Library/Pages/Device concept adaptation validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore` (196 tests), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore` (0 warnings, 0 errors), and `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore` (0 warnings, 0 errors). Physical iPhone/mask validation remains required for rendered ergonomics, BLE scan/connect, remembered-mask recovery, brightness, and real sends.
- [x] Library/Pages concept repair validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore` (199 tests), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore` (0 warnings, 0 errors), and `git diff --check`. Physical iPhone/mask and rendered simulator/device validation remain required for Library keyboard dismissal, scrolling ergonomics, Pages layout ergonomics, BLE scan/connect, brightness, and real sends.
- [x] Pages add-item screen and icon catalog validated with `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore` (202 tests), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore` (0 warnings, 0 errors), `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore` (0 warnings, 0 errors), and `git diff --check`. Physical iPhone/mask and rendered simulator/device validation remain required for add-screen navigation, icon preview clarity, shortcut tile ergonomics, BLE scan/connect, brightness, and real sends.
- [x] Android BLE adapter compile validation for scan/connect, command writes, and text upload transport wiring.
- [~] iOS CI distribution scripts validated locally; signed IPA build awaits GitHub Actions secrets and macOS runner execution. `Publish signed IPA` now uses the pre-restored `ios-arm64` runtime with `--no-restore`, without globally overriding project-reference target frameworks; live timing improvement requires a GitHub Actions run.
- [x] iOS update notification helper syntax and docs validated locally; live ntfy delivery awaits a GitHub Actions run with `NTFY_UPDATE_TOPIC_URL`.
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
