# Library Pages Device Concept Adaptation

## Slice

- Name: Library / Pages / Device concept adaptation
- Date: 2026-06-30
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions, Creative Composition, Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Make the app match the new iPhone concept around Library, Pages, and Device
without starting unimplemented media/protocol features.

## Target user moment

The wearer can open the app, find or arrange reusable reactions in Library,
trigger prepared shortcuts from Pages, and recover or tune the mask from Device.

## Observer-facing value

Prepared captions, quick actions, and built-in looks remain fast to trigger, so
people looking at the mask see deliberate reactions instead of setup friction.

## Final-goal contribution

This moves MaskApp away from a migration utility and closer to a wearable face
controller with clear content, launcher, and hardware areas.

## Capability claims

No new mask protocol capability is claimed. Image import, DIY playback, rhythm,
firmware, battery, and last-sync telemetry remain absent or Labs-only until
their implementation and physical validation exist.

## User-visible improvement

The root tabs now present the concept mental model: Library, Pages, Device.
Library has Browse/Arrange modes, filter/group/add/manage sheets, and larger
cards. Pages has Use/Manage modes, page editing/add-item/delete-confirmation
surfaces, and larger shortcut tiles. Device has a dashboard, real BLE state,
scan/connect controls, auto-connect settings, and brightness presets.

## Current evidence

- Repo files: `src/MaskApp.App`, `src/MaskApp.Core/Features/Gallery`,
  `src/MaskApp.Core/Features/Connect`, `src/MaskApp.Core/Features/MaskControl`
- Java evidence: not needed; this slice changes MAUI UX around existing features.
- Existing tests: Gallery, Pages, Connect, and MaskControl view-model tests.
- Existing validation gaps: rendered iPhone ergonomics and physical mask BLE/text
  behavior still require device validation.

## Scope

In scope:

- Library/Pages/Device tab naming and concept UI.
- Mode and sheet state for implemented item types.
- Brightness preset commands.
- Tests and progress docs.

Out of scope:

- Real image import, DIY slot upload, rhythm/microphone, firmware, battery, and
  last-sync telemetry.
- Native drag-and-drop and full modal sheet navigation.

## Files and flows

- Core: Gallery, Pages, Connect, and MaskControl view models.
- App UI: Shell, Library, Pages, Device XAML, shared styles, tab icons.
- Platform adapters: unchanged.
- Docs: progress and this slice record.

## Test plan

- Unit tests: Library mode/sheet state, Pages use/manage and delete confirmation,
  Device dashboard text, brightness presets.
- Build validation: core tests, iOS build, Android build, diff whitespace check.
- Browser/simulator/device validation: not run.
- Skipped validation and reason: physical iPhone/mask validation requires hardware.

## Deferred validation

- iOS rendered ergonomics on device or simulator.
- iOS physical BLE scan/connect, remembered-mask recovery, brightness, and sends.
- Android physical parity after iOS behavior is understood.

## Overclaim check

- No Vision or Experimental items are presented as proven capability.
- Firmware and custom firmware remain out of scope.
- Real-mask validation gaps are named in `docs/progress.md`.
- Apple Watch is not touched.

## Measured outcome

- Changes made: adapted the visible MAUI concept around Library, Pages, and
  Device, reusing current stores and send paths.
- Commands run:
  - `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore`
- Result: all passed with 0 warnings/errors where reported.
- Remaining risk: visual polish and physical mask behavior still need real
  device validation.

## Next slice candidate

- Rendered iOS smoke test and ergonomic polish for the new Library/Pages/Device
  layout.
