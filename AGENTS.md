# Repository Guidelines

## Project Structure

- `android/` contains the original Java Android source snapshot. Treat it as read-only migration evidence unless the user explicitly asks to edit it.
- `src/MaskApp.App` contains the .NET MAUI app. iOS is the primary target; Android is secondary.
- `src/MaskApp.Core` contains platform-neutral code ported from Java, especially protocol, BLE parsing, feature state, models, and data transformations.
- `tests/MaskApp.Core.Tests` contains xUnit tests for migrated core behavior.
- `docs/` contains durable development references only: mask protocol notes,
  Java source mapping, setup/distribution runbooks, MaskPack format notes, and
  icon attribution.
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
- The MAUI app uses `app.turquoise6409.green2444`, version `1.2.6`, code `126`, matching the current iOS provisioning profile used by CI. The original Android snapshot still records `cn.com.heaton.shiningmask` as migration evidence only.

## Testing Guidelines

- Add tests for each ported Java algorithm before using it from MAUI UI code.
- Prefer small tests around byte protocols, parsing, image transformations, and persistence mapping.
- For MAUI changes, run the relevant target build when tooling is available: iOS first, Android second.
- If mobile tooling or a Mac build host is unavailable, still run `dotnet test` for core changes and report exactly which platform build/device validation was not performed.

## Architecture Rules

- Do not directly translate every Java class one-to-one when a smaller C# domain model is clearer.
- Port/migration changes should preserve required behavior while making the
  user flow clearer, more reliable, easier to validate, or better aligned with
  iOS/MAUI platform conventions.
- Update docs only when a durable fact changes. Do not add per-slice progress
  logs, modernization records, or UI concept dumps unless the user explicitly
  asks for them.
- Keep BLE scanning, camera, audio, runtime permissions, storage, and platform lifecycle code out of `MaskApp.Core`.
- Shared app services should represent real platform boundaries. Do not add broad shared abstractions just to mirror Java base classes.
- Use the Java files as behavioral evidence. When source is decompiled or
  unclear, document the assumption in the relevant durable doc or code comment.

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

- Before protocol, BLE, Text, Image, Rhythm, RAVE, or DIY-slot work, read `docs/stock-mask-protocol.md` and treat it as community reverse-engineered evidence, not manufacturer documentation.
- Start from `docs/android-source-map.md` and the relevant Java files before porting or correcting Java-derived behavior.
- Before MaskPack manifest/import work, read `docs/maskpack-format.md`.
- Before changing the Pages icon catalog or vendored icon assets, read `docs/icon-sources.md`.
- Keep old progress trackers, modernization slice records, and UI concept mockups out of the repo unless the user explicitly asks to bring them back.
- State user-visible behavior changes and physical validation status in the final response instead of maintaining a repo progress log.
- Do not overclaim mask capability. Treat Drop Detector, Voice Mouth, Bass Face, GIF-ish playback, fast DIY sequencing, and real-time effects as Labs/Experimental until physically verified on a real mask.
- Firmware and custom firmware work are out of scope unless the user explicitly requests it.
- Prefer `.NET MAUI` over legacy Xamarin project formats because Xamarin support ended on May 1, 2024.
