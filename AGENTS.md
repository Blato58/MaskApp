# Repository Guidelines

## Project Structure

- `android/` contains the original Java Android source snapshot. Treat it as read-only migration evidence unless the user explicitly asks to edit it.
- `src/MaskApp.App` contains the .NET MAUI app. iOS is the primary target; Android is secondary.
- `src/MaskApp.Core` contains platform-neutral code ported from Java, especially protocol, BLE parsing, feature state, models, and data transformations.
- `tests/MaskApp.Core.Tests` contains xUnit tests for migrated core behavior.
- `docs/` contains migration notes, source maps, and setup documentation.
- `.github/workflows/ios-ipa.yml` builds signed iOS IPA artifacts on GitHub-hosted macOS runners using GitHub Secrets.
- `build/scripts/` contains CI helper scripts for Apple signing setup, AltStore-compatible source generation, and install-page generation.

## Build, Test, and Run Commands

```powershell
dotnet restore MaskApp.slnx
dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android
```

The MAUI app requires the MAUI mobile workload:

```powershell
dotnet workload install maui-mobile
```

iOS build/run requires a Mac build host with Xcode and Apple signing/provisioning configured. Android build/run requires Android SDK/JDK setup.

Use `.github/workflows/ios-ipa.yml` for CI-based signed iOS IPA distribution. See `docs/ios-ci-distribution.md` for required GitHub Secrets and workflow inputs.

## Coding Conventions

- Use C# nullable reference types and implicit usings.
- Keep migrated protocol and byte-level behavior in `MaskApp.Core` with focused tests.
- Use vertical feature slices in `src/MaskApp.App`, such as `Features/Connect`, `Features/Settings`, `Features/Text`, `Features/Image`, `Features/Rhythm`, and `Features/Microphone`.
- Keep platform APIs, permissions, app lifecycle, BLE adapters, camera/media picker, audio capture, and resources in `src/MaskApp.App` platform folders or app infrastructure.
- Preserve original package/app identity unless the user asks to change it: `cn.com.heaton.shiningmask`, version `1.2.6`, code `126`.

## Testing Guidelines

- Add tests for each ported Java algorithm before using it from MAUI UI code.
- Prefer small tests around byte protocols, parsing, image transformations, and persistence mapping.
- For MAUI changes, run the relevant target build when tooling is available: iOS first, Android second.
- If mobile tooling or a Mac build host is unavailable, still run `dotnet test` for core changes and report exactly which platform build/device validation was not performed.

## Architecture Rules

- Do not directly translate every Java class one-to-one when a smaller C# domain model is clearer.
- This migration is meant to improve UI/UX and functionality, not mechanically copy the old Java structure. Every slice should preserve required behavior while making the user flow clearer, more reliable, easier to validate, or better aligned with iOS/MAUI platform conventions.
- A migration slice is not done just because code was moved. Record the migrated behavior and the UI/UX or functionality improvement in `docs/progress.md`.
- Keep BLE scanning, camera, audio, runtime permissions, storage, and platform lifecycle code out of `MaskApp.Core`.
- Shared app services should represent real platform boundaries. Do not add broad shared abstractions just to mirror Java base classes.
- Use the Java files as behavioral evidence. When source is decompiled or unclear, document the assumption in `docs/`.

## Generated Code and Migrations

- Do not edit build outputs, `bin/`, `obj/`, generated Android resource files, or decompiled synthetic lambda files as source of truth.
- Do not add database migrations or storage schema replacements until the Java DAO/data model has been mapped.

## Security and Configuration

- Do not commit signing keys, keystores, passwords, API secrets, or production endpoints.
- Do not commit Apple signing assets. `.p12`, `.mobileprovision`, keychain, keystore, and `certificate_*/` paths must stay ignored and out of tracked history.
- Keep iOS permissions explicit in `src/MaskApp.App/Platforms/iOS/Info.plist`.
- Keep Android permissions explicit in `src/MaskApp.App/Platforms/Android/AndroidManifest.xml`.
- Treat Bluetooth, camera, microphone, and storage permissions as user-visible product behavior.

## Agent Notes

- Start from `docs/android-source-map.md` and the relevant Java files before porting a feature.
- Check `docs/progress.md` before starting a slice and update it when the slice, platform adapter, or validation status changes.
- Prefer `.NET MAUI` over legacy Xamarin project formats because Xamarin support ended on May 1, 2024.
