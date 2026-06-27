# Text Repair And Planning Baseline

## Slice

- Name: Text repair and planning baseline
- Date: 2026-06-27
- Status: validated
- Owner: Codex

## Intent

Make the Text tab react to real BLE readiness after connection and support masks
where ACK notifications are missing or unreliable. Create the long-term planning
system before broader feature implementation.

## User-visible improvement

The Text composer now distinguishes ACK-confirmed upload from write-only
compatibility upload and shows the current transport mode, status, progress, and
diagnostics.

## Current evidence

- Repo files:
  - `src/MaskApp.Core/Features/Text`
  - `src/MaskApp.App/Features/Text`
  - `src/MaskApp.App/Platforms/iOS/IosBleAdapter.cs`
  - `src/MaskApp.App/Platforms/Android/AndroidBleAdapter.cs`
- Java evidence:
  - Text protocol remains mapped through existing text upload protocol tests.
- Existing tests:
  - Text rasterizer, upload protocol, and view model tests.
- Existing validation gaps:
  - Physical iOS and Android text upload are still open.

## Scope

In scope:

- Text transport state events.
- ACK capability reporting.
- ACK-required and write-only text upload options.
- Composer status and compatibility mode UI.
- Core tests for readiness and upload mode selection.
- Long-term modernization planning docs.

Out of scope:

- Preset library implementation.
- AI composer implementation.
- Control dashboard navigation rewrite.
- Image, rhythm, microphone implementation.
- Physical device validation without a device session.

## Files and flows

- Core:
  - `ITextUploadTransport`
  - `TextUploadTransportState`
  - `TextUploadTransportStateChangedEventArgs`
  - `TextUploadOptions`
  - `TextUploadViewModel`
- App UI:
  - `TextPage.xaml`
- Platform adapters:
  - iOS CoreBluetooth text upload path
  - Android Bluetooth LE text upload path
- Docs:
  - `docs/modernization-execplan.md`
  - `docs/modernization-slice-template.md`
  - `docs/modernization-slices/`
  - `docs/progress.md`

## Test plan

- Unit tests:
  - `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
- Build validation:
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
- Skipped validation and reason:
  - Physical mask validation requires device access.

## Deferred validation

- iOS physical text upload with ACK notifications.
- iOS physical text upload in write-only compatibility mode.
- Android physical text upload with ACK notifications.
- Android physical text upload in write-only compatibility mode.

## Measured outcome

- Changes made:
  - Added text transport state events, ACK capability reporting, and
    ACK-required/write-only upload options.
  - Updated the Text composer to refresh readiness after transport events, show
    ACK/write-only mode, show send progress, and expose compatibility mode.
  - Updated iOS and Android BLE adapters to report text readiness separately
    from control readiness and support write-only compatibility upload.
  - Added the modernization execution plan, slice template, and slice records
    folder.
- Commands run:
  - `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`
  - `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
- Result:
  - Core tests passed: 40 passed.
  - iOS build passed with 0 warnings and 0 errors.
  - Android build passed with 0 warnings and 0 errors.
- Remaining risk:
  - Physical iOS and Android mask validation is still required for both
    ACK-required and write-only text upload paths.

## Next slice candidate

- Convert Home into a Control-first dashboard after physical Text validation is
  attempted or the current blocker is documented.
