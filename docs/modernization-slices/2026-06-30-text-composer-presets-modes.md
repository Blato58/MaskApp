# Text Composer Presets And Modes

## Slice

- Name: Text Composer presets and modes
- Date: 2026-06-30
- Status: validated
- Owner: Codex
- Product pillar: Creative Composition
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Make Text Composer useful as a reusable caption workspace instead of a
single-draft sender with picker-based preset management.

## Target user moment

The wearer can open a saved caption, edit it, send it immediately without
saving, save changes when intended, or send a preset directly from the list.

## Observer-facing value

Captions can be prepared and reused quickly, including compact three-line
messages and bolder text that should read more clearly on the mask if physical
testing confirms it.

## Final-goal contribution

This moves MaskApp toward a wearable face controller by making text presets a
fast creative deck, not just a protocol upload screen.

## Capability claims

- Manual 3-line centered captions and bold glyphs are app-rendered text payload
  variants over the existing text upload protocol.
- No new firmware, custom firmware, background BLE, image, DIY-slot, or audio
  behavior is claimed.
- Physical mask behavior for bold readability and 3-line captions remains
  unverified.

## User-visible improvement

- Text Composer shows a real saved-preset list with open, send, copy, and delete
  actions.
- `Send Draft` sends current editor state without saving.
- Presets persist bold style and manual 3-line centered layout.

## Current evidence

- Repo files: `TextUploadViewModel`, `QuickCaptionLayout`,
  `TextGlyphRasterizer`, `TextPresetStyle`, `TextPage.xaml`.
- Java evidence: not required; this is an app-layer layout/style extension over
  existing text upload.
- Existing tests: text upload, quick-caption layout, preset store, and Composer
  view-model tests.
- Existing validation gaps: physical iOS/Android mask checks.

## Scope

In scope:

- Composer preset list/open/direct-send/edit/copy/delete behavior.
- Manual-line-break 3-line centered payloads.
- Bold glyph rasterization and JSON preset persistence.

Out of scope:

- Colored backgrounds, image upload, DIY slots, rhythm/audio, import/export, and
  physical capability promotion.

## Files and flows

- Core: text raster/layout/package factory, preset style normalization, Composer
  view model, JSON preset persistence tests.
- App UI: Text Composer page preset list and bold/layout controls.
- Platform adapters: unchanged.
- Docs: progress status and this slice record.

## Test plan

- Unit tests: core Text tests cover bold bytes, 3-line layout, Composer draft
  send, preset open/save/direct-send, and JSON persistence.
- Build validation: iOS and Android target builds.
- Browser/simulator/device validation: skipped; MAUI device UI and real mask are
  not available in this local pass.
- Skipped validation and reason: physical mask validation remains required to
  confirm readability/timing.

## Deferred validation

- Test bold and 3-line centered captions on physical iOS first.
- Repeat on Android after iOS behavior is understood.

## Overclaim check

- No Vision or Experimental item is presented as physically verified.
- Firmware/custom firmware changes are excluded.
- Real-mask validation gaps are named in `docs/progress.md`.
- Apple Watch is not part of this slice.

## Measured outcome

- Changes made: preset-list Composer workflow, manual 3-line centered layout,
  bold glyph style, and persistence/tests/docs.
- Commands run:
  - `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore`
  - `git diff --check`
- Result: all passed; core test count is 182.
- Remaining risk: physical mask rendering/readability of bold and compact
  three-line text is still unverified.

## Next slice candidate

- Physical iOS validation of Low-static Flash, preset colors, bold text, and
  manual 3-line centered captions.
