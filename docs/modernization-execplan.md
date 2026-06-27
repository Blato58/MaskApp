# MaskApp Modernization Execution Plan

Last updated: 2026-06-27

This document is the long-term product plan for MaskApp. It complements
`docs/progress.md`: this file explains where the product is going and how work
is selected, while `docs/progress.md` records the current state of the repo and
validation gates.

## Product Direction

MaskApp is being built as a new, user-friendly MAUI product, not as a mechanical
Java port. The old Android source remains behavioral evidence. New work should
preserve the required mask protocol behavior while making the app easier to use,
easier to validate, and more capable.

Current product facts:

- Connect works well enough to reach real hardware, but physical validation is
  still tracked explicitly.
- Text is the first repair slice because it is the visible creation flow that
  currently fails on real hardware.
- iOS is the primary target. Android remains secondary and should compile when
  touched.
- Signing secrets, API keys, database migrations, backend services, commits,
  and pushes are out of scope unless requested explicitly.

## Operating Model

Use this model for continuation prompts and autonomous work.

### Continue MaskApp modernization

Use when one bounded slice should be completed.

1. Read `AGENTS.md`, `docs/progress.md`, this file, and the relevant Java source
   map or Java files.
2. Pick the next smallest slice that improves real user behavior.
3. Write or update a slice record under `docs/modernization-slices/` from
   `docs/modernization-slice-template.md`.
4. Implement only that slice.
5. Run proportional validation:
   - Core: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
   - iOS build when MAUI app code or platform contracts changed:
     `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`
   - Android build when Android or shared platform contracts changed:
     `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
6. Update `docs/progress.md` with the user-visible improvement and validation
   result.
7. Stop with a concise summary, explicit deferred validation, and the next
   slice candidate.

### Don't stop until MaskApp is ready

Use only when the user explicitly wants repeated slices.

1. Work through the readiness checklist in this document from top to bottom.
2. Complete one coherent slice at a time.
3. Run broad builds/device validation after coherent slices, not after every
   small rewrite.
4. Stop only when the readiness checklist passes or a real blocker is recorded
   in `docs/progress.md` and the relevant slice record.

## Slice Record Rules

Every modernization slice should record:

- Intent: what user problem this slice addresses.
- User-visible improvement: what the user can do better after the slice.
- Files and flows: the affected MAUI pages, Core services, platform adapters,
  Java evidence, and docs.
- Tests: unit tests, builds, simulator/device checks, and skipped validation.
- Deferred validation: usually physical iOS/Android mask checks.
- Measured outcome: what changed and how it was verified.

Use `docs/modernization-slice-template.md` and save completed records under
`docs/modernization-slices/YYYY-MM-DD-short-name.md`.

## Readiness Checklist

The app is not product-ready until these are true:

- Control dashboard is the first useful screen.
- Connect, reconnect, disconnect, and known-device recovery are clear.
- Text send works on physical iOS hardware, with ACK mode and write-only
  compatibility mode both understood.
- Preset library exists for text and control looks, including favorites,
  history, duplicate, edit, delete, import, export, and one-tap send.
- Image creator supports import/capture, crop, LED preview, transform settings,
  save, and upload.
- Rhythm and microphone modes handle permission denial, level preview,
  sensitivity, smoothing, stop, and recovery.
- AI composer has offline templates and an OpenAI Responses API provider path
  that does not depend on automating the ChatGPT web UI.
- Settings contains diagnostics, device test checklist, compatibility mode,
  permissions, data export/import, and app/about information.
- iOS physical validation passes for connect, control, text, image, rhythm, and
  microphone.
- Android parity is compiled and tracked, with physical validation where the
  shared protocol changed.

## Roadmap

### Phase 1: Text repair and planning baseline

Goal: make Text usable enough to validate on hardware and create the long-term
execution system.

Slices:

- Text readiness events after connection and characteristic discovery.
- ACK-required upload vs write-only compatibility upload.
- Text composer diagnostics: active transport, ACK mode, send progress, payload,
  frame count, and clear failure text.
- Planning docs, slice template, and progress tracking.

Exit criteria:

- Core tests cover readiness changes, unavailable transport, ACK-required send,
  write-only compatibility send, empty text, and diagnostics.
- iOS and Android builds pass after shared transport changes.
- Physical text upload gates remain explicit until tested on devices.

### Phase 2: Control-first home

Goal: replace roadmap-style Home with a real operational dashboard.

Target user experience:

- Connection state and current device at the top.
- Clear Connect action.
- Power/dim and brightness controls.
- Last sent preset or last command result.
- Quick text send.
- Current transport status and diagnostics.
- Recovery actions when the device disconnects.

Implementation notes:

- `Connect` becomes an action/page reachable from Control and Settings, not a
  primary dead-end tab.
- Navigation becomes `Control`, `Create`, `Library`, `Live`, `Settings`.
- Keep the existing Connect page available until the dashboard fully owns the
  workflow.

Exit criteria:

- A user can connect and perform common mask actions from the first tab.
- The old roadmap presentation is moved to docs/progress, not the main app UI.

### Phase 3: Text Pro Composer

Goal: turn Text from a repair slice into a complete creation tool.

Target features:

- Text, color, speed, animation, duration, preview, send, retry, and cancel.
- Font/glyph packs.
- Scrolling direction.
- Emoji/glyph fallback with mask-safe warnings.
- Preview scaling.
- Mask-length validation before send.
- Per-letter or gradient color where protocol support is proven.
- Save as preset.

Exit criteria:

- Text creation is usable without guessing protocol limits.
- Hardware upload status is visible and recoverable.

### Phase 4: Preset Library

Goal: make the app useful for repeated use instead of one-off sends.

Target features:

- Saved presets for text, brightness, image, animation, rhythm, and mixed looks.
- Favorites.
- History.
- Duplicate, edit, delete.
- Import/export JSON.
- One-tap send.
- Send-result recording.

Implementation notes:

- Add platform-neutral recipe models in Core when this phase starts:
  `MaskPreset`, `TextRecipe`, `ImageRecipe`, `RhythmRecipe`, `AiPromptRecipe`,
  `MaskPalette`, and `MaskSendResult`.
- Add JSON-backed storage in App infrastructure first.
- Do not replace GreenDAO with a database until the Java DAO/data model is
  intentionally mapped.

Exit criteria:

- Presets survive app restart.
- Import/export works with versioned JSON.
- Send history explains what was sent and whether it succeeded.

### Phase 5: Image Creator

Goal: make image upload understandable before sending data to the mask.

Target features:

- Import/capture image.
- Crop to mask ratio.
- Threshold/dither controls.
- Palette and brightness selection.
- LED output preview.
- Save as preset.
- Upload with the same progress and diagnostics model as Text.

Exit criteria:

- Image transform math is tested.
- User sees the LED result before upload.
- Upload progress and failures are explicit.

### Phase 6: Rhythm and Microphone

Goal: rebuild audio-reactive modes with explicit permission and recovery states.

Target features:

- Permission request and permission-denied UI.
- Input level preview.
- Sensitivity and smoothing controls.
- Palette selection.
- Stop/recover behavior.
- Saved rhythm presets.

Exit criteria:

- Permission failure is understandable.
- Audio mode can be stopped reliably.
- Physical iOS microphone validation is complete.

### Phase 7: AI Composer

Goal: provide creative suggestions without making the app depend on ChatGPT web
automation.

Target features:

- AI panel for emotions, jokes, party messages, compliments, warnings, short
  captions, and mood-based presets.
- Offline template fallback.
- OpenAI Responses API provider using typed request/response models.
- Prompt builders kept in code near the AI feature.
- User-provided OpenAI API key or future backend proxy.

Rules:

- Do not automate the ChatGPT web UI from the mobile app.
- A ChatGPT subscription path can be planned later as a ChatGPT App/MCP
  companion, not as mobile API authentication.
- Follow official OpenAI guidance when implemented:
  - https://developers.openai.com/api/docs/guides/text#version-prompts-in-code
  - https://developers.openai.com/apps-sdk/quickstart#next-steps
  - https://developers.openai.com/api/docs/mcp

Exit criteria:

- Offline suggestions work without network/API keys.
- API errors fall back without exposing secrets.
- Output is glyph-safe and length-limited.

### Phase 8: Device reliability

Goal: make the app dependable with real BLE hardware.

Target features:

- Known-device memory.
- Reconnect.
- Command queue visibility.
- Upload progress.
- BLE diagnostics.
- Compatibility mode for uncertain ACK behavior.
- Device test checklist inside Settings.

Exit criteria:

- The user can recover from disconnects.
- Diagnostics are understandable enough to validate real masks without guessing.

## Backlog Priority

1. Finish Text physical validation on iOS.
2. Convert Home into Control dashboard.
3. Add Settings diagnostics and device test checklist.
4. Add JSON preset storage and text presets.
5. Expand Text Pro Composer.
6. Add Image Creator transform pipeline.
7. Add Rhythm/Microphone permission and preview flow.
8. Add AI Composer.
9. Complete Android parity validation.

## Deferred Decisions

- Long-term Apple Developer identity and app id ownership.
- Minimum supported iOS version.
- Minimum supported Android API level.
- Whether a backend proxy is needed for AI keys.
- Whether preset storage remains JSON or later moves to a mapped database.
- Exact protocol support for per-letter and gradient text colors.
