# Performance and Power Validation

This is the reproducible validation procedure for sustained MaskApp use. It is
not a record of simulated physical results. CPU, memory, thermal, radio, and
battery claims require a signed Release build on a supported iPhone and a real
mask; GitHub Actions and the Windows simulator cannot supply that evidence.

## Automated bounds before device profiling

Run from a clean checkout:

```powershell
dotnet restore MaskApp.slnx
dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj -c Release --no-restore
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios -c Release --no-restore
dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android -c Release --no-restore
```

The focused scheduler pressure check is:

```powershell
dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~SustainedCoalescing
```

It holds a bulk upload open while submitting 2,000 coalescible audio frames and
proves that superseded operations are removed from the physical priority queue
and retain only one matching wake signal, not merely hidden from the pending
count. Stop and Blackout also release cancelled queue nodes and wake signals
immediately. Other automated limits cover a 64-operation scheduler queue, 120
source frames per animation, bounded editor
history, 32 MB media input, decoded pixel/duration limits, 32 MB MaskPack input,
64 MB uncompressed MaskPack content, and bounded audio latest-buffer processing.

## Device and build record

For each run, record:

- app commit, display version, and build number;
- iPhone model, iOS version, battery health/starting charge, and power source;
- mask identity/profile, observed transport mode, and firmware observation if
  available;
- ambient temperature and whether Low Power Mode is enabled;
- exact show/import/audio fixture and its content counts;
- whether Xcode Instruments was attached and its version.

Use the signed Release IPA produced by `.github/workflows/ios-ipa.yml`. Before
and after every scenario, export Device diagnostics so scheduler counts,
latency/cadence, dropped/late frames, readiness, and redacted errors are stored
with the measurement. Do not put mask identifiers or signing material in a
shared report.

## Rehearsal scenarios

Run each scenario from a cold app launch, then repeat it three times in the same
process to expose retained-state growth.

1. **Long prepared show:** prepare the representative maximum show, enter
   locked Stage, and perform for at least 30 minutes with Page/setlist moves,
   favorites, Scenes, Stop, and Blackout.
2. **Rapid animation:** run the fastest physically verified safe animation for
   10 minutes. Record requested cadence, sent/dropped/late frames, maximum
   lateness, scheduler duration, disconnect behavior, and Blackout latency.
3. **Large import:** import, preview, cancel, and then save a near-limit GIF or
   video five times. Repeat with a rejected over-limit input and a near-limit
   MaskPack. Confirm memory is released between attempts and existing content
   remains readable.
4. **Audio Labs:** after the per-mask physical diagnostic is marked Passed, run
   each visualizer mapping for 15 minutes with representative quiet, voice, and
   music input. Record delivered cadence, suppressed flashes, interruptions,
   route changes, thermal state, and battery change. Capture must stop on
   navigation, lock/background, disconnect, Stop, and Blackout.
5. **Lifecycle/reconnect:** perform at least 20 foreground/background,
   lock/unlock, Bluetooth-off/on, and out-of-range/reconnect cycles while work is
   queued. Confirm background loss does not start scanning, restoration does not
   replay output, and an explicit foreground recovery is required.

## Instruments and measurements

When a Mac is available, use Xcode Instruments with the signed Release build:

- **Allocations and Leaks:** record live bytes, persistent bytes, allocation
  rate, leaks, and the high-water mark across all three cycles.
- **Time Profiler:** record average and peak CPU plus the hottest app-owned call
  paths during Stage, import conversion, animation timing, and FFT processing.
- **Energy Log:** record CPU, networking/Bluetooth, display, and overall energy
  impact together with iPhone battery percentage and thermal-state changes.

Take marks at cold launch, post-warm-up, scenario midpoint, immediately after
Stop/Blackout, and two minutes after leaving the feature. A valid result keeps
the exact trace or exported summary with the corresponding Device diagnostic
report.

## Failure classification

Stop release validation and treat the result as critical/high until resolved if
any scenario shows a crash, hang, out-of-memory termination, data loss, unsafe
output after Stop/Blackout, stale output after reconnect/restoration, a queue
that grows beyond its configured bound, or a repeatable memory trend that does
not stabilize across identical cycles. Also stop on sustained serious/critical
thermal state or an audio/animation rate that defeats the safety gates.

CPU, energy, and battery numbers without a previous physical baseline are
measurements, not pass/fail claims. Record them first, compare on the same device
and fixture for later releases, and investigate material regressions before
publishing. Physical results belong in `PLANS.md`; never mark this procedure
itself as proof that a device run passed.
