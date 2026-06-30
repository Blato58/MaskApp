# Built-In Faces Deck UX

## Slice

- Name: Built-in Faces deck UX
- Date: 2026-06-27
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions, RAVE / DnB Festival Mode, Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

The old Built-ins archive was too editor-heavy for live use. This slice makes
the stock IMAG/ANIM archive usable as a fast Faces deck while keeping scanner
and edit flows available for physical validation notes.

## Target user moment

The wearer can open Faces, scroll favorite or working stock looks, and tap one
card once to send the command to the mask.

## Observer-facing value

People looking at the mask should see a known useful face or animation quickly
instead of waiting while the wearer edits metadata or hunts through protocol
IDs.

## Final-goal contribution

This moves MaskApp toward the wearable face controller vision by turning
stock-firmware looks into reusable reaction assets for Faces, React, and RAVE.

## Capability claims

- Built-in Face means stock IMAG/ANIM ID metadata only.
- No stock frames are extracted from the mask.
- Custom Static Face, Custom Animation, MaskPack import, DIY PLAY, upload,
  video, AI generation, Drop Detector, Voice Mouth, and real-time animation
  remain future or Labs work.

## User-visible improvement

- The tab is labeled Faces.
- The page opens with a Favorite Faces deck.
- Favorite or Working archive records appear in the deck.
- Card tap sends the saved IMAG/ANIM command immediately.
- Scanner status controls are one-tap and autosave: Favorite, Works, Bad,
  Weird.
- Detailed name, tags, and notes remain available through edit/save.
- React and RAVE can surface favorite/working built-ins as command-only actions.
- BLACKOUT remains visible on Faces and RAVE.

## Current evidence

- Repo files: `src/MaskApp.Core/Features/BuiltIns`,
  `src/MaskApp.App/Features/BuiltIns`, React, RAVE, quick actions.
- Java evidence: original app stock-image/animation behavior remains migration
  evidence only.
- Existing tests: archive, quick actions, React, RAVE, command builders.
- Existing validation gaps: physical iPhone/mask command behavior remains
  unverified.

## Scope

In scope:

- Faces deck query and sorting.
- One-tap scanner status/favorite autosave.
- Deck card IMAG/ANIM send.
- React/RAVE favorite face integration.
- Docs and physical validation notes.

Out of scope:

- Custom image upload.
- DIY PLAY or custom animation playback.
- Video playback.
- AI generation.
- Firmware or custom firmware.
- Apple Watch.

## Files and flows

- Core: archive deck query, built-in card model, BuiltIns/React/RAVE view models.
- App UI: Faces page, React label, RAVE favorite faces label, shell tab title.
- Platform adapters: unchanged.
- Docs: progress tracker and this slice record.

## Test plan

- Unit tests: deck query inclusion/sorting, one-tap autosave, deck sends,
  empty archive, React/RAVE favorite-face loading.
- Build validation: core tests plus iOS and Android MAUI target builds.
- Browser/simulator/device validation: not applicable locally for this MAUI
  slice.
- Skipped validation and reason: real mask testing requires physical hardware.

## Deferred validation

- Validate Faces on iPhone with the physical mask.
- Record useful IMAG/ANIM IDs and failures in `docs/progress.md`.

## Overclaim check

- Vision or Experimental items are not presented as proven capability.
- Firmware/custom firmware changes are excluded.
- Real-mask validation gaps remain named.
- Apple Watch remains backlog only.

## Measured outcome

- Changes made: Faces deck, one-tap autosave controls, command-only React/RAVE
  face actions, docs.
- Commands run: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`;
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`;
  `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`;
  `git diff --check`.
- Result: passed; core test run passed 101 tests; iOS and Android builds
  completed with 0 warnings and 0 errors.
- Remaining risk: physical mask behavior for specific IDs is still untested.

## Next slice candidate

- Physical iOS validation of Faces deck sends and useful stock IDs.
