# Real-Mask RAVE Validation

## Slice

- Name: Real-Mask RAVE Validation
- Date: 2026-06-27
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions, RAVE / DnB Festival Mode, Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Make the first RAVE FAST MVP testable against the physical mask before the
festival by adding command-only fallbacks, a built-in ID scanner, and a concrete
iPhone validation checklist.

## Target user moment

Open the app, connect to the mask, use BLACKOUT for recovery, scan built-in
`IMAG`/`ANIM` IDs, and use RAVE fallback buttons if text upload is slow or not
validated.

## Observer-facing value

The wearer can quickly find stock visuals that read well on the mask and use
them as festival-safe fallbacks instead of depending only on uploaded text.

## Final-goal contribution

Moves MaskApp toward a wearable face controller by turning protocol-documented
stock content into a real-mask validation workflow, while keeping unverified
visual results labeled honestly.

## Capability claims

This slice depends on `LIGHT`, `IMAG`, and `ANIM` command writes plus existing
text-upload behavior. Built-in IDs, text ACK behavior, write-only text behavior,
and reconnection remain `Needs real-mask test`. Drop Detector, Voice Mouth,
automatic Bass Face, GIF-ish playback, real-time effects, AI, Apple Watch, and
firmware/custom firmware are out of scope.

## User-visible improvement

- Built-ins tab sends static image and animation IDs with decimal/hex display,
  safe range notes, previous/next controls, last status, and BLACKOUT.
- React exposes command-only test/fallback built-ins.
- RAVE exposes command fallback buttons when Festival Lock is off while keeping
  BLACKOUT always visible.
- Real-mask validation checklist is available in docs.

## Current evidence

- Repo files: `Features/QuickActions`, `Features/BuiltIns`, `Features/Rave`,
  `Features/React`, `Features/MaskControl`, MAUI Shell/DI, BLE adapters.
- Java evidence: protocol-facing behavior stays tied to existing command/text
  builders and `docs/android-source-map.md`.
- Existing tests: QuickAction, BuiltIns, React, RAVE, MaskControl, and Text
  tests.
- Existing validation gaps: no physical iPhone/mask test in this run.

## Scope

In scope:

- Complete local protocol doc.
- Built-in command quick actions for Image 1, Image 2, Animation 1, Animation 2.
- Built-in scanner/lab UI.
- RAVE command fallback section.
- Real-mask validation checklist.

Out of scope:

- Image Studio, full preset library, persistence/favorites, DIY image upload,
  audio visualizer, Drop Detector, Voice Mouth, automatic Bass Face, GIF-ish
  playback, real-time video, AI, Apple Watch, firmware, and custom firmware.

## Files and flows

- Core: `Features/QuickActions`, `Features/BuiltIns`, `Features/Rave`,
  `Features/React`
- App UI: Built-ins page/tab and RAVE fallback section
- Platform adapters: no contract changes; existing command transport writes all
  `MaskCommand` payloads
- Docs: protocol reference, real-mask checklist, progress tracker, slice record

## Test Plan

- Unit tests: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
- Build validation: iOS and Android target builds
- Browser/simulator/device validation: not performed; physical mask/iPhone is
  required for meaningful BLE validation.
- Skipped validation and reason: physical mask validation requires user hardware.

## Deferred Validation

- iPhone scan/connect.
- BLACKOUT / `LIGHT 1`.
- Brightness cap.
- Built-in `IMAG` and `ANIM` ID quality.
- Text ACK-required and write-only behavior.
- React and RAVE sends on real hardware.
- Reconnect behavior after using RAVE.

## Overclaim Check

- All new built-in flows say `Needs real-mask test`.
- Fallback buttons are labeled as test/fallback, not polished visuals.
- Experimental features remain Labs/out of scope.
- Apple Watch remains backlog only.
- Firmware/custom firmware is excluded.

## Measured Outcome

- Changes made: completed protocol reference, command-capable built-in quick
  actions, Built-ins scanner tab, React built-in fallbacks, RAVE command
  fallback section, checklist docs, and focused tests.
- Commands run:
  - `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj` passed:
    76 tests.
  - Roslyn diagnostics summary passed: 0 warnings, 0 errors.
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios` passed:
    0 warnings, 0 errors.
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
    passed: 0 warnings, 0 errors.
- Result: implementation is compile/test validated and ready for physical
  iPhone/mask validation.
- Remaining risk: built-in visual quality, ACK behavior, write-only reliability,
  and reconnect behavior still require real hardware.

## Next Slice Candidate

- After physical testing, promote useful built-in IDs into named favorites or a
  small festival preset pack.
