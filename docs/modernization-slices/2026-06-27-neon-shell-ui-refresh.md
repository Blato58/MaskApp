# Neon Shell UI Refresh

## Slice

- Name: Neon shell UI refresh
- Date: 2026-06-27
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions, Creative Composition, RAVE / DnB Festival Mode, Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Make the existing implemented flows feel like a wearable face controller rather
than a light migration workbench.

## Target user moment

The wearer opens the app, immediately sees mask readiness, picks Control,
React, Text, RAVE, Built-ins, or Connect, and can trigger the important action
without hunting through utility screens.

## Observer-facing value

The refreshed deck emphasizes quick captions, BLACKOUT recovery, RAVE buttons,
and reusable mask looks so people looking at the mask can understand the
reaction or performance moment quickly.

## Final-goal contribution

This slice moves the MAUI app toward the product vision by making implemented
mask capabilities visually organized around live use: status, reaction,
caption, scanner, and festival modes.

## Capability claims

This slice does not add new hardware capability. It re-presents implemented
Control, React, Text, RAVE, Built-ins, and Connect behaviors. BLE command,
text upload, built-in ID, and ACK/write-only behavior still require the
existing real-mask validation path.

## User-visible improvement

The app now has a dark neon visual system, mask branding, mode tab icons,
compact action cards, React category filters, a larger Text composer with a
64-character count, festival-focused RAVE controls, clearer Connect status, and
a scanner-style Built-ins workflow.

## Current evidence

- Repo files: `src/MaskApp.App`, `src/MaskApp.Core/Features`, `tests/MaskApp.Core.Tests`
- Java evidence: Not needed for this visual shell slice.
- Existing tests: Core tests cover quick-action, React, Text, RAVE, Built-ins, and Connect view-model behavior.
- Existing validation gaps: Physical iPhone/mask and Android hardware validation remain open.

## Scope

In scope:

- Shared MAUI resources and SVG assets.
- Visual helper converters for action and transport state styling.
- UI-only presentation contracts for stable action IDs, React filtering, and Text length/count.
- XAML redesign for Control, React, Text, RAVE, Built-ins, and Connect.

Out of scope:

- BLE protocol changes.
- Text upload packet changes.
- Built-in archive schema changes.
- New packages, firmware work, AI, microphone, or DIY-slot behavior.

## Files and flows

- Core: Home card ID, React filter state, Text max length/count.
- App UI: Shared resources, Shell icons, six refreshed feature pages.
- Platform adapters: No changes.
- Docs: `docs/progress.md` and this slice record.

## Test plan

- Unit tests: Core xUnit tests for Home card IDs, React filters, and Text max length/count.
- Build validation: iOS and Android MAUI target builds.
- Browser/simulator/device validation: Not applicable for this MAUI shell slice in the local environment.
- Skipped validation and reason: Physical mask/device checks require hardware.

## Deferred validation

- Launch on physical iPhone and Android devices.
- Confirm no layout clipping on target devices.
- Confirm refreshed controls still drive real mask connect/control/text/RAVE flows.

## Overclaim check

- No Vision or Experimental items are presented as proven capability.
- Firmware and custom firmware changes are excluded.
- Real-mask validation gaps remain named in `docs/progress.md`.
- No Apple Watch behavior is added or claimed.

## Measured outcome

- Changes made: Shared neon shell, SVG assets/icons, action/state converters, redesigned feature pages, React filters, Text length/count, tests, and docs.
- Commands run: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`; Roslyn diagnostics; `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`; `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`; `git diff --check`.
- Result: 92 core tests passed; Roslyn diagnostics reported 0 warnings/errors; iOS and Android builds passed with 0 warnings/errors; diff whitespace check passed.
- Remaining risk: Physical device and real-mask behavior are unverified.

## Next slice candidate

- Physical iPhone validation for connect, control, text upload, RAVE BLACKOUT, and built-in sends.
