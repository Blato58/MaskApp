# Apple Watch Companion

The Apple Watch companion is a native SwiftUI watchOS app in
`src/MaskApp.Watch`. It sends remote-control requests to the .NET MAUI iPhone
app through WatchConnectivity. The iPhone remains the only device that talks to
the mask over Bluetooth.

## User Flow

1. Install the signed iPhone IPA and its embedded watch app.
2. Open Shining Mask on iPhone and connect the mask.
3. Leave the iPhone app in the foreground.
4. Open Shining Mask on Apple Watch.
5. Use cue navigation, favorites, emergency controls, or the Digital Crown
   brightness control.

Commands are immediate and are never queued. If the iPhone is unreachable or
the relevant mask transport is not ready, the watch reports that state and does
not reconnect or send later. This preserves the app's foreground-only BLE
connection policy.

## Controls

- STOP and BLACKOUT emergency actions
- Previous, trigger current, and next cue
- Brightness from 1 through 100 with the Digital Crown, plus Restore 60%
- Up to 12 triggerable Gallery favorites published by the iPhone
- Live iPhone reachability, foreground, readiness, setlist, and mask state

STOP and BLACKOUT remain available while the phone app is reachable. Other
actions require the phone app to be foregrounded. Favorites are selected and
validated by `WatchRemoteCoordinator`; the watch cannot send arbitrary Gallery
IDs that the phone did not publish.

## Wire Contract

The version 1 contract uses bounded JSON data messages. Each action envelope has
a schema version, message ID, stable watch sender ID, increasing sequence,
timestamp, and typed action. Replies contain the result, haptic intent, and the
latest `WatchRemoteState`.

The iPhone publishes its latest state as `stateJson` with
`updateApplicationContext`. The watch uses `sendMessageData` only when the phone
is reachable. `WatchRemoteActionProcessor` rejects stale, duplicate,
out-of-order, malformed, and unsupported messages before dispatch.

## Build And Test

On a Mac with Xcode and the watchOS simulator installed:

```bash
bash build/scripts/test-watch-app.sh
dotnet build src/MaskApp.App/MaskApp.App.csproj -f net10.0-ios -c Release \
  -p:RuntimeIdentifier=iossimulator-arm64 -p:CodesignKey= -p:CodesignProvision=
```

The MAUI iOS project references `src/MaskApp.Watch/MaskApp.Watch.proj`. That
wrapper builds the Xcode project and returns the generated `.app` bundle to the
.NET iOS SDK, which embeds it under the iPhone app's `Watch` directory.

Signed distribution additionally requires a watchOS provisioning profile for
`app.turquoise6409.green2444.watchkitapp`. See
`docs/ios-ci-distribution.md` for the GitHub secret and IPA verification path.
