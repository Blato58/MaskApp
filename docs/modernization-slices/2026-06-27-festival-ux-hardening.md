# Festival UX Hardening

## Slice

- Name: Festival UX Hardening
- Date: 2026-06-27
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions, RAVE / DnB Festival Mode, Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Make the live-use app shape faster before custom animation/import work. The
wearer should not need a hidden More tab, a slow scrolling caption default, or
an editor-first Faces flow during a festival.

## Target user moment

Open the app, choose Control, React, RAVE, Faces, or Connect directly, then send
a short caption or favorite stock face with one tap.

## Observer-facing value

The mask changes quickly enough that people nearby can understand the reaction,
caption, blackout, or built-in face while the moment is still happening.

## Final-goal contribution

This moves MaskApp toward the wearable face controller vision by prioritizing
fast manual decks, command-only fallbacks, and concise send feedback over
configuration-heavy utility screens.

## Capability claims

- Text quick captions use implemented text upload plus stock `MODE` and `SPEED`
  commands.
- Built-in Faces use implemented command-only `IMAG`/`ANIM` metadata records.
- Background presets are persisted as a user setting only. `BC`/`FC` styling
  remains Needs real-mask test and is not required for quick-caption sending.
- Firmware/custom firmware, image upload, DIY `PLAY`, custom animation playback,
  Drop Detector, Voice Mouth, real-time video, and AI generation stay out of
  scope.

## User-visible improvement

- Root tabs are now Control, React, RAVE, Faces, and Connect.
- Text Composer remains available from Control, React, and RAVE.
- Quick captions default to Flash / Blink, speed 100, and Fast write-only.
- Control exposes a lightweight quick-caption settings panel with reset.
- React and RAVE keep Favorite Faces command-only actions available when archive
  records exist.
- Faces favorite/working cards remain one-tap send with secondary edit.
- Primary live statuses are short: Ready, Connect to send, Sent, confirm on
  mask, Failed, and Needs real-mask test.

## Current evidence

- Repo files: `src/MaskApp.App/AppShell.xaml.cs`,
  `src/MaskApp.App/Features/Home/HomePage.xaml`,
  `src/MaskApp.App/Features/React/ReactPage.xaml`,
  `src/MaskApp.App/Features/Rave/RavePage.xaml`,
  `src/MaskApp.App/Features/BuiltIns/BuiltInsPage.xaml`,
  `src/MaskApp.Core/Features/QuickActions`,
  `src/MaskApp.Core/Features/Home/HomeViewModel.cs`,
  `src/MaskApp.Core/Features/React/ReactViewModel.cs`,
  `src/MaskApp.Core/Features/Rave/RaveViewModel.cs`,
  `src/MaskApp.Core/Features/BuiltIns/BuiltInsViewModel.cs`.
- Java evidence: not changed in this slice; stock command behavior comes from
  the protocol reference.
- Existing tests: core quick-action, Control, React, RAVE, and Faces view model
  tests.
- Existing validation gaps: physical iPhone/mask behavior remains untested.

## Scope

In scope:

- Navigation tab cleanup.
- Quick-caption defaults and lightweight persisted settings.
- One-tap favorite face affordances in React/RAVE/Faces.
- Concise live-use status copy.
- Docs and validation checklist updates.

Out of scope:

- Image upload, DIY playback, custom animations, firmware/custom firmware, Drop
  Detector, Voice Mouth, real-time video, and AI generation.

## Files and flows

- Core: quick-caption settings, dispatcher defaults, live status copy.
- App UI: Shell tabs, Control settings panel, React/RAVE Text Composer entry,
  RAVE sticky recovery controls.
- Platform adapters: no direct BLE adapter changes.
- Docs: progress, real-mask validation, and this slice record.

## Test plan

- Unit tests: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
- Build validation:
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`
  and
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
- Browser/simulator/device validation: not applicable in this MAUI CLI slice.
- Skipped validation and reason: physical iPhone/mask validation requires the
  real mask.

## Deferred validation

- Confirm on physical iPhone/mask that Flash / Blink mode avoids the slow
  right-to-left scroll for LOL and DROP.
- Confirm saved favorite/working built-ins send from Faces, React, and RAVE.
- Confirm background presets either work through future `BC`/`FC` support or
  remain harmless when unsupported.

## Overclaim check

- No Vision or Experimental item is presented as proven mask capability.
- Firmware/custom firmware is excluded.
- Real-mask gaps remain named in `docs/progress.md`.
- Apple Watch remains backlog only.

## Measured outcome

- Changes made: five-tab live navigation, routed Text Composer, quick-caption
  settings/defaults, favorite face one-tap affordances, shorter statuses, and
  docs/checklist updates.
- Commands run:
  Roslyn diagnostics summary,
  Roslyn test impact,
  `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`,
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`,
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`,
  and `git diff --check`.
- Result: Roslyn diagnostics found 0 warnings/errors; core tests passed 103/103;
  iOS and Android builds passed with 0 warnings/errors; `git diff --check`
  passed.
- Remaining risk: physical mask behavior for text mode, speed, built-in IDs, and
  background style settings is still unverified.

## Next slice candidate

- Physical iPhone/mask validation for quick captions, built-in IDs, and Favorite
  Faces persistence.
