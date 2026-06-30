# Custom Faces And Smileys

## Slice

- Name: Custom faces and smileys
- Date: 2026-06-30
- Status: validated
- Owner: Codex
- Product pillar: Creative Composition, Instant Reactions
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

The app had stock built-in face commands, but no way for the wearer to create
or upload their own expressions. This slice adds a v1 Face Studio for static
36x12 DIY faces and provides a starter set of generated smiley emotions.

## Target user moment

The wearer opens Face Studio from Library/Add, picks a smiley, edits the LED
grid, imports or captures an image, saves the face, selects DIY slot 1-20, and
uploads/plays it on the mask.

## Observer-facing value

People looking at the mask can see a recognizable simple mood such as happy,
sad, angry, surprised, meh, or wink, instead of only text or stock firmware
presets.

## Final-goal contribution

This moves MaskApp toward the wearable face controller vision by making
app-owned expressions editable, reusable, and sendable from the same Library
and Pages flows as captions and quick actions.

## Capability claims

- Static DIY faces use the Java-backed 36x12 payload shape from
  `UCropActivity` and `BitmapUtils.getBitmapData`: 432 RGB triplets ordered
  column-first (`x`, then `y`) with no packed LED-byte prefix.
- Upload uses `DATS`, chunked frames, `DATCP`, and immediate `PLAY` through
  the existing BLE command/image-upload characteristics.
- `CHEC`, `DELE`, slot capacity behavior, fast sequencing, and visual output
  are not physically verified yet.
- Custom animation, GIF-ish playback, MaskPack playback, and firmware changes
  remain out of scope.

## User-visible improvement

- Face Studio provides a touch pixel editor with color, erase, clear, mirror,
  slot selection, save/copy/delete, upload/play, and diagnostics.
- Six generated built-in smileys seed the face library: Happy, Sad, Angry,
  Surprised, Meh, and Wink.
- Photo import and camera capture convert images into the editable 36x12 grid.
- Saved faces appear in Gallery and can be added to Pages.

## Current evidence

- Repo files: `src/MaskApp.Core/Features/Faces`, `src/MaskApp.App/Features/Faces`, BLE adapters, Gallery/Pages catalog.
- Java evidence: `UCropActivity`, `BitmapUtils.getBitmapData`, `DiyImageFragment`, `LedViewDiy`, `DiyAgreement`, and `DiyMutiAgreement` for 36x12 editing, 20 slots, column-major RGB image data, `DATS`/frames/`DATCP`, and `PLAY`.
- Existing tests: new Core face generator, image import, and upload protocol tests.
- Existing validation gaps: no successful real iPhone/mask or Android/mask run has confirmed the corrected visual output.

## Scope

In scope:

- Static 36x12 DIY face model, built-in smiley generation, import transform,
  JSON storage, MAUI editor, MediaPicker import/camera path, BLE upload/play,
  Gallery/Pages projection, tests, and docs.

Out of scope:

- Custom animations, MaskPack playback/import, fast DIY sequencing, `CHEC` UI,
  delete-slot UI, firmware/custom firmware work, and physical hardware claims.

## Files and flows

- Core: face patterns, store state, image transform, upload protocol, upload transport contract, Face Studio view model.
- App UI: Face Studio page, `GraphicsView` editor, route registration, Library/Add route, JSON store.
- Platform adapters: iOS/Android image decoders, Android media permissions, iOS/Android face upload transport implementation.
- Docs: progress tracker and this slice record.

## Test plan

- Unit tests: generated smiley count/emotions, 36x12 shape, Java bit packing,
  Java-compatible column-major RGB payload length/color bytes, frame count,
  command plaintexts, ACK parsing, and image import transform.
- Build validation: core tests, iOS target build, Android target build, diff check.
- Browser/simulator/device validation: not performed; this is a MAUI mobile app
  and physical mask checks require hardware.
- Skipped validation and reason: real mask visual confirmation and picker/camera
  UX were unavailable in this environment.

## Deferred validation

- On physical iPhone, import a photo, capture a photo, draw a face, save it,
  upload slot 1-20, and confirm the mask displays the expected LED pattern.
- Confirm slot overwrite behavior, `PLAYOK`, `CHEC`, `DELEOK`, and app status
  after reconnect.
- Repeat on Android after iOS behavior is understood.

## Overclaim check

- Face upload is marked Implemented but Needs real-mask test.
- Advanced DIY sequencing, custom animation, GIF-ish playback, and MaskPacks
  remain future/Labs.
- Firmware and custom firmware remain out of scope.
- No Apple Watch behavior was added.

## Measured outcome

- Changes made: Face Studio, six smiley generators, face JSON store, image
  import/camera path, DIY upload/play protocol, iOS/Android transport wiring,
  Gallery/Pages projection, tests, and docs.
- Commands run: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`, `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`, `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`, `git diff --check`.
- Result: 216 core tests passed; iOS and Android target builds passed with 0 warnings and 0 errors.
- Post-validation correction: real-mask feedback showed the first upload
  encoder produced split white/yellow output with black patches. The static
  DIY payload was corrected to Java-compatible RGB-only column-major bytes.
- Remaining risk: physical DIY upload behavior, rendered Face Studio ergonomics,
  camera/photo picker UX, slot overwrite, `CHEC`, and `DELE`.

## Next slice candidate

- Physical iOS validation of Face Studio DIY upload/play, then add `CHEC` and
  delete-slot UI only after the mask behavior is confirmed.
