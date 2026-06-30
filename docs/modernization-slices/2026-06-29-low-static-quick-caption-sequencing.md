# Low-Static Quick Caption Sequencing

## Slice

- Name: Low-static quick-caption sequencing
- Date: 2026-06-29
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions; RAVE / DnB Festival Mode; Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask retest

## Intent

Reduce the static pre-roll seen on every React/RAVE quick-caption send. The
issue was not cold start: the uploaded caption became visible before Blink
`MODE 2` was applied.

## Target user moment

The wearer taps `LOL` or `DROP` and the caption starts blinking immediately, or
noticeably faster than the old roughly 300 ms static period.

## Observer-facing value

Short captions should read as live reactions instead of briefly appearing as a
static sign before the flash starts.

## Final-goal contribution

This improves the fastest caption path used by Control, React, and RAVE, moving
MaskApp toward a reliable wearable face controller without adding new feature
families.

## Capability claims

- Low-static Flash uses centered 44-column Blink text at speed 50.
- The low-static profile pre-arms `SPEED 50` and `MODE 2` before upload.
- After `DATCP`, the low-static profile sends `MODE 2` before `SPEED 50`.
- Low-static Flash skips the per-send `MODE 1` reset and black `BC` reset so
  neither command can delay post-upload Blink.
- Stable Flash remains available as fallback with the black `BC` reset.
- Fast Flash remains unstable/experimental.
- Firmware/custom firmware, Image Studio, custom animation playback, Drop
  Detector, Voice Mouth, audio visualizer, MaskPack import/export, AI, and
  Apple Watch implementation are out of scope.

## User-visible improvement

Quick-caption settings default to Low-static Flash. Quick-action status reports
the exact profile used: Low-static Flash, Stable Flash, or Fast Flash unstable.
React/RAVE captions remain fixed 44-column, centered, Blink `MODE 2`, speed 50,
and non-scrolling by default.

## Current evidence

- Repo files: `TextSendProfile`, `TextUploadOptions`,
  `TextUploadCommandSequence`, `TextSendPackageFactory`,
  `QuickActionDispatcher`, iOS/Android BLE adapters, Home quick-caption
  settings.
- Java evidence: unchanged; stock text command behavior remains based on
  `docs/stock-mask-protocol.md`.
- Existing tests: text package factory and quick-action dispatcher tests cover
  quick-caption layout/profile behavior.
- Existing validation gaps: physical iPhone/mask timing still needs retest.

## Scope

In scope:

- Low-static quick-caption profile.
- Pre-upload speed/mode arming.
- Immediate post-upload mode-first sequencing.
- Skipping/moving black `BC` away from the low-static post-upload Blink path.
- Quick-caption default/status labels.
- Focused tests and docs.

Out of scope:

- New feature families, Labs automation, image/custom animation playback,
  firmware/custom firmware, AI, and Watch work.

## Files and flows

- Core: text profile/options, command sequence builder, package factory, quick
  action settings and dispatcher.
- App UI: Control quick-caption send-mode labels and default copy.
- Platform adapters: iOS and Android upload sequencing.
- Docs: progress and this slice record.

## Test plan

- Unit tests: low-static profile shape, default quick-action profile, stable
  fallback, fast unstable label, centered 44-column quick-caption payloads, and
  pre/post command order.
- Build validation: core tests, iOS app build, Android app build, diff check.
- Browser/simulator/device validation: not applicable locally for MAUI BLE.
- Skipped validation and reason: physical blink timing requires the real mask.

## Deferred validation

- Send `LOL` from React 10 times and confirm Blink starts immediately or much
  faster than before.
- Send `DROP` from RAVE 10 times and confirm the same.
- Compare Stable Flash fallback.
- Confirm Fast Flash remains unstable.
- Confirm Text Composer Centered Blink, intentional scroll, BLACKOUT, and
  built-in face fallback still work.

## Overclaim check

- Low-static Flash is implemented and compile/test validated, but physical
  timing is still Needs real-mask retest.
- Fast Flash remains unstable.
- Colored backgrounds remain disabled from normal UI.
- Firmware/custom firmware changes are excluded.
- Apple Watch remains backlog only.

## Measured outcome

- Changes made: added Low-static Flash, made it the quick-caption default,
  added reusable upload command sequencing, pre-armed speed/mode before upload,
  sent post-upload `MODE 2` before speed, skipped black `BC` for Low-static,
  preserved Stable Flash fallback, and kept Fast Flash unstable.
- Commands run: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`,
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`,
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`, and
  `git diff --check`.
- Result: 137 core tests passed; iOS and Android builds passed with 0 warnings
  and 0 errors; diff check passed.
- Remaining risk: physical iPhone/mask timing is not verified locally.

## Next slice candidate

- Physical iOS retest of Low-static Flash repeated sends against Stable Flash
  fallback.
