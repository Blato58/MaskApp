# Android Source Map

The `android/` folder contains 302 Java files from the existing app. It appears to be a decompiled or exported Java source snapshot rather than a complete Gradle project.

## App Identity

From `android/BuildConfig.java`:

- Application id: `cn.com.heaton.shiningmask`
- Flavor: `google`
- Version name: `1.2.6`
- Version code: `126`
- Build type: `release`

## Main Areas

| Area | Purpose |
| --- | --- |
| `android/base` | Application bootstrap, base MVP classes, crash handling, update, permissions, music/recording helpers. |
| `android/base/app` | BLE protocol constants, commands, language helpers, background player and upload manager. |
| `android/ble` | BLE device abstraction around `HeartBeatDevice`. |
| `android/model` | Data manager, HTTP helpers, preferences, bean/data models. |
| `android/dao` | GreenDAO-style generated database classes and DAO beans. |
| `android/ui/activity` | User flows for connect, rhythm, microphone, image editing, camera, settings, splash, and text editing. |
| `android/ui/widget` | Custom LED, image, carousel, color picker, camera, and scrolling widgets. |
| `android/databinding` | Generated binding classes. Do not treat these as hand-authored UI source. |
| `android/sevice` | Audio/FFT listener services. The folder name is misspelled in the source snapshot. |

## First Ported Behavior

`android/base/app/BleConfig.java` contains the BLE manufacturer advertisement matcher. The product signature has been ported to `MaskApp.Core.Bluetooth.BleAdvertisementMatcher` with tests.

Original signature:

```text
FF 54 52 00 4A
```

The leading `FF` is the BLE manufacturer-specific AD type. The remaining bytes come from `BROADCAST_SPECIFIC1_5_PRODUCT`.
