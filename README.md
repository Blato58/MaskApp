# MaskApp

MaskApp is the migration workspace for rewriting the existing Android Java app into a modern .NET MAUI mobile app.

The original source snapshot is kept in `android/` and should be treated as migration input. New code lives under `src/`.

## Projects

- `src/MaskApp.App` - .NET MAUI app, iOS first and Android secondary, using the app id `app.turquoise6409.green2444`.
- `src/MaskApp.Core` - platform-neutral migration code for BLE protocol, feature state, parsing, and transformations.
- `tests/MaskApp.Core.Tests` - xUnit tests for migrated core behavior.

## App Navigation

The primary Shell destinations are **Library**, **Stage**, and **Device**.
Library owns content discovery and editor entry points. Stage contains Build and
Preflight, with Pages, Scenes, setlists, and the full-screen locked performance
surface underneath it. Device keeps ordinary connection and brightness controls
ahead of progressively disclosed diagnostics and recovery tools.

## Prerequisites

- .NET SDK `10.0.300` or a compatible .NET 10 feature band.
- MAUI mobile workload for building/running the app:

```powershell
dotnet workload install maui-mobile
```

iOS builds require a Mac build host with Xcode and Apple signing/provisioning configured. From Windows, use Visual Studio Pair to Mac or equivalent `dotnet build` Mac host properties.

## Common Commands

Core logic can be validated without a mobile device:

```powershell
dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj
```

Build the MAUI app for the secondary Android target locally:

```powershell
dotnet restore MaskApp.slnx
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android
```

Build the primary iOS target through a Mac build host:

```powershell
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios
```

## iOS CI Distribution

The repository includes a GitHub Actions workflow for building a signed iOS IPA
on a macOS runner without a local Mac or Visual Studio:

```text
.github/workflows/ios-ipa.yml
```

See `docs/ios-ci-distribution.md` for the required Apple signing assets,
GitHub Secrets, first workflow run, GitHub Release publishing, GitHub Pages
install page, and Feather/AltStore-style update source.

The published install page and source feed are available at
https://blato58.github.io/MaskApp/.

## Migration Notes

Microsoft support for legacy Xamarin SDKs ended on May 1, 2024. This repo now targets `.NET MAUI`, with iOS as the primary product target and Android maintained as a secondary target.

Use vertical feature slices in the MAUI app. Keep platform-neutral behavior in `MaskApp.Core` with tests, then wire each slice to platform services under `Platforms/iOS` and `Platforms/Android`.

The goal is to improve UI/UX and functionality, not to mechanically copy the old Java structure. Each slice should preserve required behavior while making the user flow clearer, more reliable, and better aligned with iOS/MAUI conventions.
