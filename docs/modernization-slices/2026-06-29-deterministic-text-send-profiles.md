# Deterministic Text Send Profiles

## Slice

- Name: P0 deterministic text-send profiles
- Date: 2026-06-29
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions; RAVE / DnB Festival Mode; Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Fix inconsistent quick-caption behavior where text could appear centered or
left-aligned, blink or stay solid, depending on previous mask text state and
fast write timing.

## Target user moment

The wearer can tap `LOL`, `DROP`, or another RAVE/React caption and get a
predictable Stable Flash send plan with centered/fitted text, explicit speed,
and explicit blink mode every time.

## Observer-facing value

Short reactions should be readable and high-visibility in social/festival
settings instead of sometimes looking like old, solid, scrolled, or shifted text.

## Final-goal contribution

This moves MaskApp toward the wearable face controller vision by making the
fastest caption path deterministic enough for Control, React, RAVE, and future
Watch intent triggers to share.

## Capability claims

- Quick captions use stock text upload plus explicit `SPEED` and `MODE`.
- Flash/Blink quick captions use protocol `MODE 2`.
- Stable Flash repeats `SPEED` and `MODE` after post-upload delays.
- Background `BC` is implemented only as fail-soft protocol-documented styling
  and remains Needs real-mask test.
- Firmware/custom firmware, image upload, custom animation upload, Drop
  Detector, Voice Mouth, MaskPacks, AI, and Watch work are out of scope.

## User-visible improvement

- Control, React, and RAVE quick captions share one deterministic text send plan.
- Stable Flash is the default quick-caption profile.
- Fast Flash remains available with safe inter-command delays.
- Reliable ACK remains available when notifications exist.
- Text Creator has explicit Composer Scroll and Composer Centered behavior with
  a visible profile summary.
- React/RAVE/Home statuses preserve the exact profile summary returned by the
  dispatcher.

## Current evidence

- Repo files: `TextSendProfile`, `TextSendPackageFactory`,
  `QuickActionDispatcher`, `TextUploadViewModel`, iOS/Android BLE adapters.
- Java evidence: `android/base/DataManager.java` records separate text speed,
  mode, foreground/background state; protocol details are tracked in
  `docs/stock-mask-protocol.md`.
- Existing tests: quick-caption layout, quick-action dispatcher, text profile
  factory, text protocol, Text Creator view model, Home, React, and RAVE tests.
- Existing validation gaps: physical iPhone/mask text mode, background `BC`, and
  ACK/write-only timing remain unverified.

## Scope

In scope:

- Text send profile model and central package builder.
- Deterministic quick-caption profile defaults.
- Post-upload `SPEED` then `MODE` sequencing with stable repeat.
- Fail-soft background command planning.
- Composer layout/mode clarity.
- Focused tests and docs.

Out of scope:

- Custom animation upload, Image Studio, Drop Detector, Voice Mouth, MaskPack,
  AI, firmware/custom firmware, and Watch implementation.

## Files and flows

- Core: text profiles/plans, package factory, upload options, command builders,
  quick-action settings/dispatch, Text Creator view model.
- App UI: Control quick-caption default label, Text Creator layout picker and
  profile summary.
- Platform adapters: iOS and Android post-`DATCP` command sequencing.
- Docs: progress, real-mask validation checklist, and this slice record.

## Test plan

- Unit tests: quick profile layout/mode/speed, stable/fast timing, ACK fallback,
  composer scroll/centered payloads, background fail-soft planning, dispatcher
  status/profile behavior, Text Creator centered send.
- Build validation: core tests, iOS app build, Android app build, diff check.
- Browser/simulator/device validation: not applicable locally for MAUI BLE.
- Skipped validation and reason: physical iPhone/mask checks require hardware.

## Deferred validation

- Run the deterministic text profile checklist in
  `docs/real-mask-validation.md`.
- Record whether Stable Flash or Fast Flash is the best festival profile.
- Confirm background `BC` behavior before treating it as product-ready styling.

## Overclaim check

- No Vision or Experimental item is presented as physically verified.
- Firmware/custom firmware changes are excluded.
- Real-mask validation gaps are named in `docs/progress.md`.
- Apple Watch remains future companion remote only.

## Measured outcome

- Changes made: added shared text send profiles/plans, centralized package
  creation, defaulted quick captions to Stable Flash, routed quick actions
  through the profile builder, made composer layout explicit, added fail-soft
  background command planning, and changed iOS/Android adapters to send
  `SPEED` then `MODE` after post-upload delay with stable repeat.
- Commands run: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`,
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`,
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`,
  and `git diff --check`.
- Result: 133 core tests passed; iOS and Android builds passed with 0 warnings
  and 0 errors; diff check passed.
- Remaining risk: physical mask timing and `BC` background rendering still need
  real-mask validation.

## Next slice candidate

- Physical iPhone/mask validation of Stable Flash, Fast Flash, Reliable ACK,
  composer centered, composer scroll, and fail-soft background behavior.
