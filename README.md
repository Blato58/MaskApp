# MaskApp

MaskApp is a .NET 10 MAUI app for controlling Shining Mask LED face masks over Bluetooth Low Energy. iOS is the primary target and Android is maintained as a secondary target. The implementation replaces the original Java application while keeping the local Android snapshot as behavioral evidence.

## Features

- **Library** — browse stock faces and animations, create custom faces and animations, compose text, and manage portable MaskPack archives.
- **Stage** — organize pages and scenes, run preflight checks, and use a focused performance surface.
- **Device** — discover and reconnect masks, adjust live controls, and inspect redacted diagnostics.

The mask protocol is community-reverse-engineered and device firmware can vary. Read [the protocol reference](docs/stock-mask-protocol.md) before changing BLE commands, uploads, or capability claims.

## Repository Layout

- `src/MaskApp.App/` — MAUI UI, feature slices, platform services, and resources.
- `src/MaskApp.Core/` — platform-neutral protocol, models, persistence, and view-model logic.
- `tests/MaskApp.Core.Tests/` — xUnit regression tests organized by feature.
- `android/` and local `decompiled-app/` — read-only migration evidence.
- `docs/` — setup, protocol, distribution, archive-format, and asset-provenance references.
- `build/scripts/` — preview generation and iOS distribution helpers.

## Prerequisites

- The .NET SDK version selected by `global.json` (`10.0.300` baseline).
- The .NET MAUI mobile workload:

```powershell
dotnet workload install maui-mobile
```

- Android SDK/JDK for Android builds.
- A Mac build host with Xcode for iOS builds and device deployment.

See [docs/setup.md](docs/setup.md) for platform-specific configuration.

## Build and Test

Restore the solution and run the platform-neutral test suite:

```powershell
dotnet restore MaskApp.slnx
dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj
```

Build an individual MAUI target:

```powershell
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios
```

Use Visual Studio, Rider, or platform tooling for simulator and physical-device deployment. The iOS command requires an available Mac build host.

To verify that tracked stock previews match their generator inputs:

```powershell
python build\scripts\generate-builtin-previews.py --check
```

Regeneration prerequisites and source mapping are documented in [docs/builtin-preview-sources.md](docs/builtin-preview-sources.md).

## CI and Distribution

[`.github/workflows/ios-ipa.yml`](.github/workflows/ios-ipa.yml) restores the solution, runs core tests, and builds the mobile targets. With Apple signing secrets configured, it can publish a signed IPA, GitHub Release, install page, and AltStore-compatible feed. See [docs/ios-ci-distribution.md](docs/ios-ci-distribution.md); never commit signing assets or credentials.

## Contributing

Read [AGENTS.md](AGENTS.md) for repository structure, coding conventions, testing expectations, and pull request guidance. Keep protocol behavior in `MaskApp.Core`, platform APIs in `MaskApp.App`, and accompany behavioral changes with focused tests.
