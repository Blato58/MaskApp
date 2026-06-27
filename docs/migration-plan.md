# Migration Plan

## Direction

Use `.NET MAUI` as the migration target, with iOS as the primary production platform and Android as a secondary target. Legacy Xamarin SDKs are out of support, so the app project should stay SDK-style, use `<UseMaui>true</UseMaui>`, and target `net10.0-ios` plus `net10.0-android`.

The app code is organized by vertical slices. Each feature owns its page, view model, commands, state, and slice-specific abstractions. Shared services are reserved for platform boundaries such as BLE, permissions, storage, camera/media picker, audio capture, navigation, dialogs, and logging.

The migration is not a one-to-one Java port. Every migrated slice should improve the product's UI/UX, reliability, or functional capability while preserving required behavior from the source app. If a slice only copies old structure without improving the user flow, maintainability, or platform fit, it should be redesigned before implementation.

## Phases

1. Baseline app identity, iOS permissions, source areas, and MAUI build setup.
2. Port platform-neutral protocol logic into `MaskApp.Core` with tests.
3. Build the `Connect` slice first with MAUI XAML, a testable view model, and iOS CoreBluetooth adapter.
4. Add Android BLE adapter after the iOS connect flow is stable.
5. Port persistence from the Java DAO layer into a documented .NET storage approach.
6. Port UI slices incrementally: settings, text, image, rhythm, microphone.
7. Rebuild custom LED/image widgets with maintainable MAUI controls or platform handlers.
8. Add device validation for iOS permissions, BLE, camera, microphone, storage, and app restart recovery.

## Current Migration Targets

- BLE advertisement product matching from `android/base/app/BleConfig.java`.
- App package/version identity from `android/BuildConfig.java`.
- Connect vertical slice: scan status, discovered mask list, connect/disconnect state.
- iOS permission baseline from observed source areas: Bluetooth, camera, photo library, and microphone.

Track migration completion in `docs/progress.md`. Update that file whenever a feature slice, platform adapter, core migration item, or validation gate changes state.

For each update, record both the migrated behavior and the UI/UX or functionality improvement delivered by the slice.

## Validation Gates

- Core logic: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
- iOS compile through Mac build host: `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`
- Android secondary compile: `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
- Device validation: BLE scan/connect, camera image capture/crop, microphone rhythm mode, local persistence, app restart recovery.

## Open Questions

- Whether Apple provisioning is ready for the MAUI app id `com.blato58.maskapp`.
- Exact minimum Android API level required by product/support policy.
- Exact minimum iOS version required by product/support policy.
- Whether the Java source snapshot has original XML layouts/assets elsewhere or only generated binding/decompiled Java output.
- Replacement strategy for GreenDAO data and the external BLE/library dependencies.
