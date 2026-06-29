# Text Profile Physical Feedback

## Slice

- Name: Text profile physical feedback follow-up
- Date: 2026-06-29
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions; RAVE / DnB Festival Mode; Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask retest

## Intent

Apply real-mask feedback from deterministic text profile testing without adding
new product scope.

## Target user moment

The wearer sends quick captions and Text Creator captions through the profile
that actually behaved best on the physical mask: centered 44-column Blink at
speed 50 with a black background.

## Observer-facing value

Captions should stay readable and predictable instead of becoming left-aligned,
solid, or visually harmed by background styling.

## Final-goal contribution

This keeps MaskApp focused on reliability as a feature before adding more RAVE,
image, animation, AI, or Watch work.

## Capability claims

- Fast Flash failed physical testing and is marked unstable.
- Text background styling failed physical testing and is disabled/black.
- Centered 44-column Blink at speed 50 is the best observed profile.
- Firmware/custom firmware, custom animation upload, Image Studio, Drop
  Detector, Voice Mouth, MaskPacks, AI, and Watch work remain out of scope.

## User-visible improvement

- Quick captions default to speed 50.
- Text Creator opens on Centered 44-column + Blink + speed 50.
- Quick caption settings no longer present colored background controls.
- Background style commands are skipped so text stays on black/off background.
- Fast Flash remains labeled unstable.

## Current evidence

- Repo files: `TextSendProfile`, `TextSendPackageFactory`,
  `QuickActionTextSettings`, `HomeViewModel`, `TextUploadViewModel`.
- Java evidence: not changed in this follow-up.
- Existing tests: text profile factory, quick-action dispatcher, Text Creator
  view model, and protocol tests.
- Existing validation gaps: the updated defaults need one more real-mask check.

## Scope

In scope:

- Apply physical feedback to defaults and UI labels.
- Keep text backgrounds black.
- Update docs and tests.

Out of scope:

- Further fast write transport experiments.
- New text styling features.
- Image, animation, Drop Detector, Voice Mouth, MaskPack, AI, or Watch work.

## Files and flows

- Core: text send profiles, quick-action settings, package factory, Text Creator
  defaults.
- App UI: quick-caption settings copy and background-control removal.
- Platform adapters: unchanged.
- Docs: progress, real-mask checklist, and this slice record.

## Test plan

- Unit tests: profile speed defaults, skipped background style command, Text
  Creator default profile.
- Build validation: core tests, iOS app build, Android app build, diff check.
- Browser/simulator/device validation: not applicable locally for MAUI BLE.
- Skipped validation and reason: physical retest requires the real mask.

## Deferred validation

- Re-run `LOL` and `DROP` with updated Stable Flash speed 50 defaults.
- Confirm background remains black.
- Keep Fast Flash failed unless a later pacing change fixes it.

## Overclaim check

- Fast Flash is not presented as reliable.
- Background styling is not presented as working.
- Firmware/custom firmware changes are excluded.
- Apple Watch remains future companion remote only.

## Measured outcome

- Changes made: changed quick-caption and composer centered defaults to speed
  50, opened Text Creator on centered Blink, skipped background style commands,
  removed colored background controls from the quick settings UI, and recorded
  physical results.
- Commands run: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`,
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`,
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`,
  and `git diff --check`.
- Result: 133 core tests passed; iOS and Android builds passed with 0 warnings
  and 0 errors; diff check passed.
- Remaining risk: physical retest of the updated defaults still needs the real
  mask.

## Next slice candidate

- Physical retest of updated Stable Flash speed 50 quick captions on iOS.
