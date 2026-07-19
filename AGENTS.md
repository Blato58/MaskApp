# Repository Guidelines

## Project Structure & Module Organization

- `src/MaskApp.App/` is the .NET MAUI client (`net10.0-ios` primary, `net10.0-android` secondary). Keep screens in vertical `Features/` slices, shared UI in `Controls/`, platform implementations in `Platforms/`, and app assets in `Resources/`.
- `src/MaskApp.Core/` contains platform-neutral protocol, state, persistence, and transformation logic. Keep Bluetooth, permissions, camera, audio, and lifecycle APIs out of this project.
- `tests/MaskApp.Core.Tests/` mirrors core feature folders and contains xUnit tests. `android/` and `decompiled-app/` are read-only migration evidence.
- `docs/` holds durable references; `docs/stock-mask-protocol.md` is authoritative for BLE and upload behavior. `build/scripts/` contains asset and distribution generators.

## Build, Test, and Development Commands

```powershell
dotnet restore MaskApp.slnx
dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios
python build\scripts\generate-builtin-previews.py --check
```

Install the pinned .NET SDK from `global.json` and the MAUI workload with `dotnet workload install maui-mobile`. Android needs a configured SDK/JDK; iOS needs a Mac build host with Xcode. Deploy through Visual Studio, Rider, or platform tooling after building the desired target.

## Coding Style & Naming Conventions

Use four-space indentation, file-scoped namespaces, nullable reference types, and implicit usings. `Directory.Build.props` enables latest analyzers and treats warnings as errors. Use `PascalCase` for types and members, `camelCase` for locals and fields, `I` prefixes for interfaces, and `Async` suffixes for asynchronous methods. Preserve feature boundaries and add abstractions only for real platform seams.

## Testing Guidelines

Add focused regression tests for protocol bytes, parsing, transformations, persistence, and view-model behavior. Name test classes `*Tests` and methods `Operation_ExpectedBehavior`. No coverage threshold is enforced; cover changed branches and failure paths. For UI changes, build each affected platform and report any simulator or physical-mask validation not performed.

## Commit & Pull Request Guidelines

Recent commits use short, imperative subjects such as `Fix Connect page XAML bindings`; follow that style and keep unrelated work separate. Pull requests should explain user-visible behavior, link an issue when applicable, list validation commands, and include before/after screenshots for UI changes. Call out platform, signing, or physical-device validation gaps.

## Security & Generated Assets

Never commit certificates, provisioning profiles, keystores, secrets, or device identifiers. Do not hand-edit `bin/`, `obj/`, generated preview files, or CI output; update the generator/source and rerun its check instead.
