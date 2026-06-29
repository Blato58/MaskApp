# UI Auto-Connect And Global Text Settings

## Slice

- Name: UI auto-connect and global text settings
- Date: 2026-06-30
- Status: validated
- Owner: Codex
- Product pillar: Reliability As A Feature; RAVE / DnB Festival Mode; Instant Reactions
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Polish the app for festival/live use now that the core text, RAVE, and Faces
behavior has working validation feedback. The slice removes the Festival Lock
gate, makes live controls easier to hit, remembers the last mask, and lets quick
captions share a persistent foreground color.

## Target user moment

The wearer opens the app at an event, sees connection and auto-connect state,
uses BLACKOUT immediately, sends big RAVE/React actions, and gets consistent
quick-caption text color without digging into diagnostics.

## Observer-facing value

Observers see captions and favorite faces in the selected foreground color,
while the wearer can recover quickly with BLACKOUT or reconnect if the mask
drops.

## Capability claims

- Known-mask memory and foreground auto-connect are app-layer BLE scan/connect
  behavior only.
- Auto-connect is foreground/app-open only; no iOS background BLE mode is added.
- Text foreground color uses existing text payload color data. Colored
  backgrounds remain disabled and black-reset only where existing text profiles
  already use it.
- Drop Detector, Voice Mouth, Bass Face automation, custom animation playback,
  AI, firmware, and Watch work remain out of scope.

## User-visible improvement

- RAVE no longer hides useful controls behind Festival Lock.
- Control and Connect show auto-connect status, remembered mask, toggles, and
  forget/connect-now actions.
- Quick captions from Control, React, and RAVE use one global foreground color.
- Text Composer defaults to the global foreground color but keeps per-send manual
  color choices.

## Current evidence

- Repo files: `RaveViewModel`, `ConnectViewModel`, `HomeViewModel`,
  `QuickActionDispatcher`, `TextUploadViewModel`, MAUI pages, JSON stores.
- Java evidence: `DataManager` had text color state and reconnect preference,
  but the new model is app-owned rather than a one-to-one Java port.
- Existing tests: Core quick-action, connect, RAVE, React, Home, and Text tests.
- Existing validation gaps: physical iPhone/mask auto-connect, foreground color,
  and repeated RAVE/React sends still need real-mask recording.

## Scope

In scope:

- Lock-free RAVE UI, foreground-only auto-connect settings, known-mask JSON
  memory, global quick-caption color, Text Composer defaulting, tests, and docs.

Out of scope:

- Background BLE, protocol rewrite, colored backgrounds, Image Studio, custom
  animation playback, audio visualizer, Drop Detector, Voice Mouth, MaskPack
  import/export, AI, firmware, and Watch work.

## Files and flows

- Core: Connect auto-connect model/coordinator, quick-caption foreground
  settings, RAVE/Home/Connect/Text view-model wiring.
- App UI: Control, Connect, RAVE, React, and Text pages.
- Platform adapters: no low-level BLE adapter changes.
- Docs: progress tracker, real-mask checklist, slice record.

## Test plan

- Unit tests:
  - auto-connect defaults, disabled/no-target behavior, manual remember, ID
    match, guarded name fallback, multiple-name safety, forget mask.
  - quick-caption foreground normalization and dispatcher payload color.
  - Text Composer global default and manual override.
  - lock-free RAVE controls and Connect remembering.
- Build validation:
  - `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
  - `git diff --check`
- Browser/simulator/device validation: not run; MAUI mobile physical flow needs
  iPhone/mask.

## Deferred validation

- Run the updated real-mask checklist on iPhone with the physical mask.
- Confirm auto-connect finds the remembered mask after app reopen.
- Confirm Cyan/Pink/global foreground colors show from React, RAVE, and Text
  Composer.
- Confirm BLACKOUT still works from RAVE sticky footer and body controls.

## Overclaim check

- No Vision or Experimental feature is presented as proven.
- Firmware/custom firmware changes are excluded.
- Physical validation gaps are recorded in `docs/progress.md`.
- Apple Watch remains backlog only.

## Measured outcome

- Changes made: lock-free RAVE UI, known-mask auto-connect, global foreground
  quick-caption color, Text Composer defaulting, updated Control/Connect/React
  UI, tests, and docs.
- Commands run:
  - `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
  - `git diff --check`
- Result: Core tests passed with 152 tests; iOS and Android builds passed with
  0 warnings and 0 errors; `git diff --check` passed.
- Remaining risk: physical mask behavior still needs validation.

## Next slice candidate

- Physical iPhone/mask validation for foreground colors, auto-connect, repeated
  Low-static Flash sends, favorite faces, BLACKOUT, and manual scan recovery.
