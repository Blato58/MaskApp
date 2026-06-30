# Text Presets And Czech Packs

## Slice

- Name: Text presets and Czech starter packs
- Date: 2026-06-30
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions, Creative Composition, RAVE / DnB Festival Mode
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Replace hardcoded-only quick captions with editable local text presets that carry
their own style, visibility, and mask-safe text.

## Target user moment

The wearer can open React, RAVE, Control, or Text Composer, pick or create a
Czech caption, save it, favorite it, and send it again without retyping or
changing one global quick-caption style.

## Observer-facing value

People looking at the mask should see short readable Czech captions, RAVE cues,
or neutral satire text that matches the selected per-preset style.

## Final-goal contribution

This moves MaskApp closer to a wearable face controller by making text reactions
reusable, editable, local, and visible across the main live-use decks.

## Capability claims

- Text presets use the existing deterministic text upload path and current
  Low-static Flash default.
- Czech diacritics are transliterated to mask-safe ASCII before sending.
- Political/Satire content is local editable starter text, not current or remote
  political data.
- No firmware, custom firmware, AI, audio visualizer, Drop Detector, Voice
  Mouth, Image Studio, custom animation playback, MaskPack import/export, or
  Apple Watch implementation was added.

## User-visible improvement

- Text Composer can save, load, edit, duplicate, delete, favorite, and show
  presets in React, RAVE, or Control.
- React shows Czech Basic, Czech Meme, Czech Political/Satire, Czech RAVE, and
  Custom preset groups.
- RAVE shows RAVE favorites and Czech RAVE presets without hiding BLACKOUT,
  brightness, favorite faces, or command fallbacks.
- Control shows favorite text presets using each preset's own style.

## Current evidence

- Repo files: `src/MaskApp.Core/Features/TextPresets`, Text/React/RAVE/Home view
  models, MAUI XAML pages, and app JSON storage.
- Java evidence: not needed for new local preset library; send path still uses
  existing text protocol implementation.
- Existing tests: text upload, quick-action dispatch, React/RAVE/Home, and new
  text preset tests.
- Existing validation gaps: physical iPhone/mask confirmation remains open.

## Scope

In scope:

- Local JSON text preset persistence.
- Czech starter packs and transliteration.
- Per-preset style, category, favorite, and visibility.
- Text Composer save/edit/delete/duplicate.
- React, RAVE, and Control preset decks.

Out of scope:

- Remote content updates, analytics, network dependency, AI, Image Studio,
  custom animation playback, audio visualizer, Drop Detector, Voice Mouth,
  MaskPack import/export, firmware/custom firmware, and Apple Watch code.

## Files and flows

- Core: text preset models, seed catalog, store state, normalizer, dispatcher,
  and view-model preset surfaces.
- App UI: Text Composer, React, RAVE, and Control XAML.
- Platform adapters: unchanged; preset sends use existing `ITextUploadTransport`.
- Docs: progress and this slice record.

## Test plan

- Unit tests: Czech transliteration, seed packs, JSON store fallback/roundtrip,
  preset dispatch, composer save, and React/RAVE/Control preset surfaces.
- Build validation:
  - `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore`
  - `git diff --check`
- Browser/simulator/device validation: not run; this is a MAUI mobile app and
  physical mask validation remains separate.
- Skipped validation and reason: physical iPhone/mask checks were not performed
  in this slice.

## Deferred validation

- Confirm Czech Basic `AHOJ`, Czech Meme `TY VOLE`, and Czech RAVE `KDE JE
  VODA` display correctly on the physical mask.
- Confirm saved presets persist after app restart and appear in Control
  favorites when favorited.
- Confirm Political/Satire presets are editable local content on-device.

## Overclaim check

- No Vision or Experimental features are presented as proven hardware behavior.
- Firmware/custom firmware changes are excluded.
- Real-mask validation gaps remain named in `docs/progress.md`.
- Apple Watch remains backlog only with iPhone-owned BLE control.

## Measured outcome

- Changes made: local text preset library, Czech seed packs, transliteration,
  JSON persistence, preset dispatcher, Text Composer save/edit controls, and
  React/RAVE/Control preset decks.
- Commands run:
  - `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore`
  - `git diff --check`
- Result: tests and both platform builds passed.
- Remaining risk: physical text rendering, color appearance, and persistence
  restart behavior still need real iPhone/mask validation.

## Next slice candidate

- Record preset send behavior on the physical mask.
