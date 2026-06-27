# iOS CI Distribution

This repository can build a signed iOS `.ipa` on a GitHub-hosted macOS runner.
The workflow is intended for Windows/Rider development without a local Mac or
Visual Studio.

The workflow file is `.github/workflows/ios-ipa.yml`. It builds
`src/MaskApp.App/MaskApp.App.csproj` for `net10.0-ios`, uploads the IPA as a
workflow artifact, can publish it to a GitHub Release, and can publish a
Feather/AltStore-style update source to GitHub Pages.

## Apple Prerequisites

- Paid Apple Developer account.
- App ID / bundle identifier matching the MAUI app `ApplicationId`:
  `app.turquoise6409.green2444`.
- iPhone UDID registered in the Apple Developer portal.
- Ad Hoc provisioning profile for this app and device, or another profile that
  fits the chosen distribution method.
- Distribution certificate exported as `.p12` with the private key.
- `.mobileprovision` file downloaded from Apple Developer.

Use an Ad Hoc provisioning profile for the first physical iPhone install path.
That profile must include the iPhone UDID and must be issued for
`app.turquoise6409.green2444`.

## GitHub Secrets

Create these repository or organization secrets:

```text
IOS_BUILD_CERTIFICATE_BASE64
IOS_P12_PASSWORD
IOS_PROVISION_PROFILE_BASE64
IOS_KEYCHAIN_PASSWORD
IOS_CODESIGN_KEY
```

`IOS_BUILD_CERTIFICATE_BASE64` is the Base64 text of the `.p12` file.
`IOS_PROVISION_PROFILE_BASE64` is the Base64 text of the `.mobileprovision`
file. `IOS_CODESIGN_KEY` is the full signing identity name, usually shaped like:

```text
Apple Distribution: Your Name (TEAMID)
```

The workflow prints available code signing identities if the provided name is
wrong. It does not print secret values.

## Convert Files To Base64 On Windows

Copy the `.p12` content to the clipboard:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\path\certificate.p12")) | Set-Clipboard
```

Copy the provisioning profile content to the clipboard:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\path\profile.mobileprovision")) | Set-Clipboard
```

Create `IOS_KEYCHAIN_PASSWORD` as a random password. It is used only for the
temporary GitHub Actions keychain.

## Run The Workflow

1. Open the repository in GitHub.
2. Go to Actions.
3. Select `Build iOS IPA`.
4. Click `Run workflow`.
5. Leave `project_path` as `src/MaskApp.App/MaskApp.App.csproj`.
6. Optionally set `display_version` and `build_number`.
7. Enable `publish_release` to create or update a GitHub Release.
8. Enable `publish_pages` to update `apps.json` and the install page.

If `build_number` is empty, the workflow uses the GitHub Actions run number.
If `publish_pages` is enabled, the workflow also publishes a release because the
phone source needs a stable IPA URL.

Tag pushes also run the workflow for tags matching:

```text
ios-v*
v*
```

Tag runs publish a release asset using the pushed tag as the release tag.

## Install Or Update On iPhone

After a successful run with `publish_release` and `publish_pages` enabled:

1. Open the generated GitHub Pages install page on the iPhone.
2. Add the source to Feather or an AltStore-style installer.
3. Install the latest build.
4. For updates, run the workflow again with a higher version or build number.
5. The source JSON keeps the newest IPA first so update checks see the latest
   build.

The Pages output contains:

- `apps.json`
- `index.html`
- `manifest.plist`

The primary supported path is the signed IPA plus the Feather/AltStore-compatible
source JSON. The `manifest.plist` is only a convenience fallback because OTA
installation behavior depends on the Apple account/profile type and installer.

## Private Repository Warning

If the repository or release assets are private, Feather and AltStore-style
installers may not be able to download the IPA directly from the iPhone. In that
case:

- download the IPA manually from GitHub Actions or Releases
- send or open it on the iPhone
- import it into Feather or another sideloading tool

For the smoothest update flow, host the IPA and source JSON somewhere reachable
from the iPhone, or use a separate public distribution repository that contains
only release assets and source JSON.

## Optional Source Metadata

Copy `build/ios-distribution.example.json` to `build/ios-distribution.json` to
customize source metadata such as the source name, developer name, category,
tint color, icon URL, and website.

Do not put secrets or signing files in this JSON file.

## Security Notes

- Never commit `.p12`, `.mobileprovision`, passwords, Base64 secrets, keychains,
  keystores, or production signing material.
- Do not upload signing certificates to random third-party signing services.
- Use GitHub repository or organization secrets for signing inputs.
- If signing material was ever committed, removing it from current history
  reduces repository exposure but does not prove the material was never fetched,
  cached, cloned, or forked.
