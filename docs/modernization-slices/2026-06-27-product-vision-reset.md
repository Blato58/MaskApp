# Product Vision Reset

## Slice

- Name: Product vision reset
- Date: 2026-06-27
- Status: validated
- Owner: Codex
- Product pillar: Reliability as a Feature
- Capability confidence: Vision
- Physical validation status: Docs-only

## Intent

Make the wearable face controller vision the planning source of truth and add
guardrails so future slices do not overclaim unverified mask capability.

## Target user moment

Future sessions should choose work that helps the user send expressive reactions,
captions, looks, or performance moments quickly and reliably.

## Observer-facing value

Future features should make the mask understandable and funny to people looking
at it, not just technically controllable by the wearer.

## Final-goal contribution

This slice aligns the roadmap, progress tracker, and agent guidance around
MaskApp as a wearable face controller.

## Capability claims

- RAVE, Drop Detector, Voice Mouth, Bass Face, GIF-ish playback, fast DIY
  sequencing, and real-time effects are not claimed as proven capability.
- Firmware and custom firmware work are out of scope for this planning slice.

## User-visible improvement

No app behavior changes. Future work now has a clear product target and physical
validation vocabulary.

## Current evidence

- Repo files:
  - `docs/product-vision.md`
  - `docs/modernization-execplan.md`
  - `docs/progress.md`
  - `docs/modernization-slice-template.md`
  - `AGENTS.md`
- Java evidence:
  - Not changed in this docs-only slice.
- Existing tests:
  - Existing Text repair validation remains unchanged.
- Existing validation gaps:
  - Physical mask validation remains open as recorded in `docs/progress.md`.

## Scope

In scope:

- Product vision docs.
- Roadmap and slice template updates.
- AGENTS.md planning/process guidance.
- Docs-only validation.

Out of scope:

- UI/code features.
- App behavior changes.
- RAVE, AI, Reaction Deck, firmware, or custom firmware implementation.

## Files and flows

- Core:
  - None.
- App UI:
  - None.
- Platform adapters:
  - None.
- Docs:
  - Product vision, execution plan, progress tracker, docs index, slice
    template, and agent notes.

## Test plan

- Unit tests:
  - Not required; docs-only slice.
- Build validation:
  - Not required; docs-only slice.
- Browser/simulator/device validation:
  - Not required; docs-only slice.
- Skipped validation and reason:
  - No code or app behavior changed.

## Deferred validation

- Physical Text validation remains the next implementation priority.

## Overclaim check

- Vision and Experimental items are not presented as proven mask capability.
- Firmware/custom firmware changes are excluded.
- Real-mask validation gaps remain named in `docs/progress.md`.

## Measured outcome

- Changes made:
  - Added product vision and capability-confidence model.
  - Added Festival-Ready RAVE MVP definition.
  - Added Apple Watch Quick Deck + Mode Switcher as a future lowest-priority
    companion remote and captured the stable intent ID implication.
  - Updated execution plan, slice template, docs index, progress tracker, and
    agent notes.
- Commands run:
  - `git diff --check`
- Result:
  - Passed.
- Remaining risk:
  - None for docs formatting. Product assumptions still need future physical
    validation before implementation claims.

## Next slice candidate

- Text validation/fix on a physical mask.
