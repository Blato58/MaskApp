# Text Crash/Freeze And Quick Caption Layout

## Slice

- Name: P0 text crash/freeze and quick-caption layout hotfix
- Date: 2026-06-29
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions; RAVE / DnB Festival Mode; Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Stop text-related tap crashes/freezes and make Home, React, and RAVE quick
captions use fast centered Flash/Blink text instead of slow scrolling.

## Target user moment

The wearer can tap `LOL`, `NOPE`, or `DROP` and get an immediate-feeling mask
caption send path without the app crashing. Text Creator remains responsive
while typing.

## Observer-facing value

Short captions should appear as readable, centered reactions that observers can
understand quickly in a social or festival setting.

## Final-goal contribution

This moves MaskApp toward the wearable face controller goal by making the
fastest reaction paths reliable enough to build Control Room, React, and RAVE
flows on top of them.

## Capability claims

- Quick captions use stock text upload plus `MODE 2` and `SPEED 100` by default.
- Quick-caption bitmap layout is app-owned and fixed to 44 visible columns.
- Text upload remains Needs real-mask test for ACK mode and write-only mode.
- Background color remains protocol-documented only; no FC/BC command path was
  added in this hotfix.
- Firmware/custom firmware, image upload, Drop Detector, Voice Mouth, AI, and
  Apple Watch work are out of scope.

## User-visible improvement

- Async command failures are contained and reported instead of crashing taps.
- Text Creator preview replaces one list per edit instead of mutating hundreds
  of cells on every keystroke, and a follow-up debounce prevents synchronous
  preview rebuilds while typing.
- Text send failures and cancellations show concise status text.
- Home, React, and RAVE quick text uses centered/fitted 44-column payloads with
  paced Fast write-only sends.
- Long quick captions are shortened before upload instead of overflowing.

## Current evidence

- Repo files: `AsyncRelayCommand`, `TextUploadViewModel`,
  `QuickActionDispatcher`, `QuickCaptionLayout`, Home/React/RAVE view models.
- Java evidence: no new Java behavior was ported in this slice.
- Existing tests: text protocol, text view model, quick actions, Home, React,
  RAVE, built-ins, and mask control tests.
- Existing validation gaps: physical iPhone/mask text display and timing remain
  untested.

## Scope

In scope:

- Async command crash containment.
- UI-bound await cleanup in bindable view models.
- Text Creator preview responsiveness.
- Quick-caption fixed-width centered layout and default Blink/Flash behavior.
- Focused regression tests and docs.

Out of scope:

- Built-in Archive continuation.
- Custom animation/image import.
- MaskPack product flows.
- AI, Drop Detector, Voice Mouth, firmware, custom firmware, and Watch work.
- New FC/BC background command implementation.

## Files and flows

- Core: command execution, Text upload view model, quick-caption layout,
  quick-action dispatcher, Home/React/RAVE status flows.
- App UI: Text preview binding now consumes a replacement list.
- Platform adapters: no platform adapter behavior changed.
- Docs: progress, real-mask checklist, and this slice record.

## Test plan

- Unit tests: command exception containment, text upload exception/cancellation,
  diagnostic truncation, preview list replacement, quick-caption 44-column
  layout, quick-action defaults and failures.
- Build validation: core tests, iOS app build, Android app build, diff check.
- Browser/simulator/device validation: not applicable to this MAUI core hotfix
  in the local environment.
- Skipped validation and reason: physical iPhone/mask checks require hardware.

## Deferred validation

- Run the real-mask checklist in `docs/real-mask-validation.md` on iPhone.
- Compare Fast write-only and Reliable ACK send modes on the physical mask.
- Confirm centered/fitted text display and Flash/Blink mode visually.

## Overclaim check

- No Vision or Experimental capability is presented as physically verified.
- Firmware/custom firmware changes are excluded.
- Real-mask validation remains named in `docs/progress.md`.
- Apple Watch remains backlog only and is not implemented here.

## Measured outcome

- Changes made: fixed async command crash behavior, removed UI-bound
  `ConfigureAwait(false)` usage, made text send fail-soft, capped payload hex,
  replaced preview collection mutation, debounced Text Creator preview refresh,
  added 44-column quick-caption layout, and routed quick actions through a
  centered/fitted paced Fast write-only package path.
- Commands run: Roslyn diagnostics, `dotnet test
  tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`, `dotnet build
  src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`, `dotnet build
  src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`, and `git diff
  --check`.
- Result: 116 core tests passed; Roslyn diagnostics found 0 warnings/errors;
  iOS and Android app builds succeeded with 0 warnings/errors; diff check
  passed.
- Remaining risk: physical mask behavior, ACK/write-only timing, and visual
  centering still need real iPhone/mask validation. After physical feedback,
  the zero-delay path was replaced with 20 ms frame pacing because the real mask
  did not render text properly after unpaced writes. If quick captions are still
  visibly slow, the next decision is whether RAVE quick actions should prefer
  command-only built-in looks over fresh text upload for true instant use.

## Next slice candidate

- Physical iPhone/mask validation for Text ACK mode, write-only mode, and RAVE
  quick-caption timing.
