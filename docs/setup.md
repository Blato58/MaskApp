# Setup

## Installed Baseline

The repository was initialized against .NET SDK `10.0.300`.

Install the MAUI mobile workload before building or running `src/MaskApp.App`:

```powershell
dotnet workload install maui-mobile
```

iOS builds require access to Apple's build tools on a Mac with Xcode installed and its license accepted. From Windows, use Visual Studio Pair to Mac or pass the Mac host properties to `dotnet build`.

Depending on the machine, Android SDK/JDK setup may also be required for the secondary Android target.

On this machine the dependency target installed them here:

```powershell
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
$env:JAVA_HOME = "$env:LOCALAPPDATA\Microsoft\Jdk-android"
```

## Validate Core Code

Core code can be restored, built, and tested without mobile platform tooling:

```powershell
dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj
```

## Validate MAUI App

Build the primary iOS target through a Mac build host:

```powershell
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios
```

Build the secondary Android target locally:

```powershell
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
$env:JAVA_HOME = "$env:LOCALAPPDATA\Microsoft\Jdk-android"
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android
```

Use Visual Studio, Rider, or platform tooling for simulator/device deployment.

## CI iOS Distribution

Use `.github/workflows/ios-ipa.yml` to build a signed IPA on a GitHub-hosted
macOS runner without a local Mac. The workflow uses GitHub Secrets for the
certificate, provisioning profile, and signing identity.

See `docs/ios-ci-distribution.md` for the full setup and iPhone install/update
flow.
