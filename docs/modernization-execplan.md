# MaskApp Modernization Execution Plan

Last updated: 2026-06-27

This document explains how to move MaskApp toward the product vision in
`docs/product-vision.md`. It complements `docs/progress.md`: this file guides
slice selection, while `docs/progress.md` records current implementation and
validation status.

## Product Direction

MaskApp is being built as a wearable face controller: a fast way to send
reactions, captions, images, beat modes, and prepared performance moments to the
mask. It should not regress into a BLE packet utility or migration checklist.

Current product facts:

- iOS is the primary target. Android remains secondary and should compile when
  touched.
- Connect, mask control MVP, and Text have implementation work in place, but
  physical validation remains explicit.
- Text validation/fix is still the next implementation priority.
- RAVE, Voice Mouth, Drop Detector, Bass Face, fast DIY sequencing, GIF-ish
  playback, and real-time effects are Labs/Experimental until tested on a real
  mask.
- Apple Watch support is a lowest-priority future companion remote. It must not
  delay Text validation, Control Room, Reaction Deck, RAVE MVP, presets, image,
  or rhythm work.
- Firmware and custom firmware are out of scope for this planning track.
- Signing secrets, API keys, database migrations, backend services, commits,
  and pushes are out of scope unless requested explicitly.

## Operating Model

Use this model for continuation prompts and autonomous work.

### Continue MaskApp modernization

Use when one bounded slice should be completed.

1. Read `AGENTS.md`, `docs/product-vision.md`, `docs/progress.md`, this file,
   and the relevant Java source map or Java files.
2. Pick the next smallest slice that improves real user behavior.
3. Identify the product pillar, target user moment, observer-facing value,
   capability confidence, and physical validation status.
4. Write or update a slice record under `docs/modernization-slices/` from
   `docs/modernization-slice-template.md`.
5. Implement only that slice.
6. Run proportional validation:
   - Core: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
   - iOS build when MAUI app code or platform contracts changed:
     `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`
   - Android build when Android or shared platform contracts changed:
     `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
7. Update `docs/progress.md` with the user-visible improvement and validation
   result.
8. Stop with a concise summary, explicit deferred validation, and the next
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
- Product pillar: which product pillar from `docs/product-vision.md` it moves.
- Target user moment: what the wearer should be able to do.
- Observer-facing value: what people looking at the mask should understand.
- Capability confidence: Vision, Protocol-documented, Implemented, Physically
  verified, Experimental, or Out of scope.
- Physical validation status: Docs-only, Simulator only, Protocol expected,
  Needs real-mask test, Tested on real mask, Failed on real mask, or Blocked by
  unknown hardware behavior.
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

- Control Room is the first useful screen.
- Connect, reconnect, disconnect, and known-device recovery are clear.
- Text send works on physical iOS hardware, with ACK mode and write-only
  compatibility mode both understood.
- React has a fast one-tap deck for short captions and proven built-in looks.
- RAVE MVP provides manual-first, offline-first festival controls without long
  uploads during the event.
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

### Phase 1: Text validation/fix

Goal: make Text reliable on a physical mask before building more features on top
of text upload.

Slices:

- Validate ACK-required text upload on iOS.
- Validate write-only compatibility upload on iOS.
- Record physical behavior and adjust transport/status handling if needed.
- Repeat Android validation after iOS path is understood.

Exit criteria:

- Real-mask text status is recorded as tested, failed, or blocked by unknown
  hardware behavior.
- Any required Text fix is covered by tests and platform builds.

### Phase 2: Control Room + Reaction Deck MVP

Goal: make the app feel useful and fun immediately after opening.

Target user experience:

- Connection state and current device at the top.
- Clear Connect action.
- Power/dim and brightness controls.
- Last sent preset or last command result.
- Quick text send and one-tap short reactions.
- Recent reactions and random reaction.
- Blackout and last look.
- Current transport status and diagnostics.
- Recovery actions when the device disconnects.

Implementation notes:

- `Connect` becomes an action/page reachable from Control and Settings, not a
  primary dead-end tab.
- The target navigation shape is `Control`, `React`, `Create`, `Party/RAVE`,
  `Library`, and `Settings`.
- Keep the existing Connect page available until the dashboard fully owns the
  workflow.
- Use only proven or clearly labeled operations. Do not present Experimental
  ideas as available mask capability.

Exit criteria:

- A user can connect and perform common mask actions from the first tab.
- A user can send a short reaction in seconds.
- The old roadmap presentation is moved to docs, not the main app UI.

### Phase 3: RAVE MVP Entry Point

Goal: provide a manual-first, offline-first DnB festival mode that is useful
before Labs automation exists.

Target features:

- Big DnB reaction buttons.
- Always-visible blackout.
- Brightness cap.
- Reconnect/resume.
- Short offline captions.
- Haptic/send feedback.
- Low-bandwidth mode.
- Avoid long uploads during the event.

Exit criteria:

- The RAVE entry point can be used without internet or AI.
- The mode prefers instant commands, short text, built-in looks, and previously
  validated upload paths.
- Automatic Drop Detector, Voice Mouth, and Bass Face remain Labs/Experimental.

### Phase 4: Built-in Gallery Scanner

Goal: unlock useful stock-firmware content already on the mask.

Target features:

- Scan/send built-in static image IDs and animation IDs.
- Favorite, rename, tag, and hide discovered looks.
- Save useful built-in looks into the Reaction Deck and Library.

Exit criteria:

- The user can discover and reuse built-in content without knowing protocol IDs.

### Phase 5: Text Pro Composer

Goal: turn Text from a repaired sender into a complete caption system.

Target features:

- Accurate preview, scroll preview, speed preview, direction, blink, foreground
  color, background color where proven, safe width limits, upload progress,
  retry/cancel, and save as preset.
- Advanced color/effect claims must stay Protocol-documented or Experimental
  until verified.

Exit criteria:

- Text creation is usable without guessing protocol limits.
- Hardware upload status is visible and recoverable.

### Phase 6: Preset Library And Mask Packs

Goal: make the app useful for repeated use and sharing.

Target features:

- Saved presets for reactions, text, brightness, built-in images, built-in
  animations, image looks, rhythm settings, and party/RAVE packs.
- Favorites, history, duplicate, edit, delete, import/export JSON, and one-tap
  send.
- Send-result recording.

Exit criteria:

- Presets survive app restart.
- Import/export works with versioned JSON.
- Send history explains what was sent and whether it succeeded.

### Phase 7: Image Studio And DIY Slots

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
- DIY slot behavior is physically tested before features rely on fast slot
  playback or sequencing.

### Phase 8: Rhythm, Voice, And RAVE Labs

Goal: explore performance modes without overclaiming capability.

Target features:

- Permission request and permission-denied UI.
- Input level preview.
- Sensitivity and smoothing controls.
- Palette selection.
- Stop/recover behavior.
- Saved rhythm presets.
- Labs for Drop Detector, Voice Mouth, Bass Face, GIF-ish playback, fast DIY
  sequencing, and real-time effects.

Exit criteria:

- Permission failure is understandable.
- Audio mode can be stopped reliably.
- Physical visualizer and DIY playback behavior is recorded before Labs features
  move into the main product.

### Phase 9: AI Composer

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

### Phase 10: Device Reliability

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

### Backlog: Apple Watch Companion Remote

Goal: eventually let the wearer trigger prepared actions and switch app modes
from the wrist without moving BLE control away from the iPhone.

Priority:

- Lowest priority / backlog.
- Must not delay Text validation, Control Room, Reaction Deck, RAVE MVP,
  presets, image, or rhythm work.
- No watchOS code should be added until the phone-side command model is stable.

Architecture:

- Apple Watch is a companion remote only.
- iPhone remains the BLE controller, upload engine, preset manager, AI provider,
  and reliability layer.
- Watch sends high-level intents to iPhone.
- Watch must not directly control the mask over BLE.

Future core use:

- Quick Deck: favorite actions such as `DROP`, `WHEEL UP`, `RELOAD`,
  `BASS FACE`, `HYDRATE`, `VIBE CHECK`, and `BLACKOUT`.
- Decks: RAVE, Social, Meme, Safety/Welfare, Party, and Favorites.
- Mode Switcher: RAVE, React, Party Loop, Visualizer, Voice Mouth, Quiet/Dim,
  and Blackout.
- Mode switching updates the active iPhone mode/deck and executes any required
  app-side start/stop behavior.

Design implication:

- Future quick actions and modes should be represented as stable intent IDs, not
  only button click handlers.
- The app-layer command model should be able to expose
  `TriggerQuickAction(actionId)` and `SwitchMode(modeId)`.
- iPhone RAVE UI, Reaction Deck, Party Director, and future Watch remote should
  share that model.

Labs/Experimental:

- Watch microphone input for auxiliary music-wave input, voice commands, or AI
  prompt dictation.
- iPhone microphone remains preferred for RAVE MVP.

Out of scope:

- Direct Watch-to-mask BLE control.
- Watch-based text/image editing.
- Watch-based firmware, custom firmware, or diagnostics.
- Standalone Watch operation.

## Backlog Priority

1. Finish Text physical validation on iOS.
2. Fix Text based on physical validation if needed.
3. Build Control Room + Reaction Deck MVP.
4. Add RAVE MVP entry point.
5. Add Built-in Gallery Scanner.
6. Add Settings diagnostics and device test checklist.
7. Add JSON preset storage and text presets.
8. Expand Text Pro Composer.
9. Add Image Studio and DIY slot management.
10. Add Rhythm, Voice, and RAVE Labs after physical validation.
11. Add AI Composer.
12. Complete Android parity validation.
13. Apple Watch Quick Deck + Mode Switcher.

## Deferred Decisions

- Long-term Apple Developer identity and app id ownership.
- Minimum supported iOS version.
- Minimum supported Android API level.
- Whether a backend proxy is needed for AI keys.
- Whether preset storage remains JSON or later moves to a mapped database.
- Exact protocol support for per-letter and gradient text colors.
- Exact phone-side quick-action and mode intent ID scheme for future Watch,
  RAVE UI, Reaction Deck, and Party Director sharing.
