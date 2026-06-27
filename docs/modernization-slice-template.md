# Modernization Slice Template

Copy this file to `docs/modernization-slices/YYYY-MM-DD-short-name.md` when a
slice starts.

## Slice

- Name:
- Date:
- Status: planned | in progress | validated | blocked
- Owner:
- Product pillar:
- Capability confidence: Vision | Protocol-documented | Implemented | Physically verified | Experimental | Out of scope
- Physical validation status: Docs-only | Simulator only | Protocol expected | Needs real-mask test | Tested on real mask | Failed on real mask | Blocked by unknown hardware behavior

## Intent

What user problem does this slice solve?

## Target user moment

What should the wearer be able to do?

## Observer-facing value

What should people looking at the mask understand, laugh at, or react to?

## Final-goal contribution

How does this move MaskApp toward the wearable face controller vision?

## Capability claims

List any protocol, hardware, timing, audio, DIY-slot, or real-time behavior this
slice depends on. Mark advanced features as Labs/Experimental unless physically
verified.

## User-visible improvement

What can a user do better after this slice?

## Current evidence

- Repo files:
- Java evidence:
- Existing tests:
- Existing validation gaps:

## Scope

In scope:

- 

Out of scope:

- 

## Files and flows

- Core:
- App UI:
- Platform adapters:
- Docs:

## Test plan

- Unit tests:
- Build validation:
- Browser/simulator/device validation:
- Skipped validation and reason:

## Deferred validation

- 

## Overclaim check

- Are any Vision or Experimental items presented as proven capability?
- Are firmware/custom firmware changes excluded unless explicitly requested?
- Are real-mask validation gaps named in `docs/progress.md`?
- If Apple Watch is mentioned, is it kept as a future companion remote with
  iPhone-owned BLE control and no standalone/watch-to-mask behavior?

## Measured outcome

Record after implementation:

- Changes made:
- Commands run:
- Result:
- Remaining risk:

## Next slice candidate

- 
