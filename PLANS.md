# MaskApp Performance Roadmap

This is the single living execution ledger for preparing MaskApp for sustained live-performance use. It records frozen scope, architecture decisions, slice order, evidence, physical-device checks, and remaining risk. Update this file as evidence changes; do not create parallel progress trackers.

## Product goal and boundaries

Deliver an iOS-first .NET MAUI app that can prepare, rehearse, and perform reliable mask shows while preserving the current stock-mask protocol, existing user data, and already-working Library, Pages, Device, text, stock-face, and DIY-face workflows.

Frozen boundaries:

- The stock mask and its reverse-engineered BLE protocol remain the hardware contract. Custom firmware and backend services are out of scope.
- iOS is primary and Android remains supported. Apple Watch may use a native SwiftUI companion built by CI.
- `android/` remains read-only migration evidence.
- Rapid animation playback is foreground-only until real-device evidence supports a broader lifecycle policy.
- No capability is called physically verified without dated evidence from a real mask. Labs audio features stay experimental until that evidence exists.
- Existing JSON data must migrate in place. Destructive persistence replacement is not allowed.

## Baseline snapshot

| Fact | Evidence |
|---|---|
| Snapshot | 2026-07-17 at commit `b26691f1233dd4ba57574efd2116c6dd5873e7d2` |
| Checkout | Detached worktree, clean before roadmap edits |
| SDK and app | .NET 10 / MAUI; `net10.0-ios` and `net10.0-android` |
| Automated baseline | Restore passed; 291 Core tests passed; iOS and serial Android builds passed with zero warnings |
| Reachable root UI | Library, Pages, Device |
| Existing but unreachable UI | Home, React, and Rave are DI-registered but absent from Shell navigation |
| Transport baseline | Feature-specific locks exist, but platform transports write directly and no app-wide command scheduler owns the BLE link |
| Persistence baseline | Separate JSON stores for gallery layout, text presets, faces/DIY slots, built-ins, quick text, and auto-connect |
| Distribution baseline | The signed IPA workflow did not run tests or unsigned platform builds before signing |

## Locked architecture decisions

1. **One BLE owner.** All mask commands will pass through one connection-scoped scheduler. Feature locks may coordinate feature state, but must never bypass the scheduler or write concurrently to the link.
2. **Explicit command policy.** Work is classified by priority, cancellation/supersession key, retry safety, timeout, and connection generation. Control and blackout preempt queued bulk work; frame streams coalesce stale frames; non-idempotent commands are never replayed blindly.
3. **Connection generations.** Disconnect invalidates queued and active work from the old connection. Reconnect starts a new generation; automatic replay is opt-in only for commands proven safe.
4. **Safe stop semantics.** Stop restores the last stable look when one exists. If it does not, pending playback is cancelled without inventing a fallback image. Blackout is a distinct explicit control.
5. **Per-mask identity.** Slot assignments, capability observations, and last-known stable state are keyed by a derived mask identity. The current global slot ledger will migrate compatibly.
6. **Dimensions remain domain-specific.** DIY image content stays 46x58. The protocol's 44-column text/display payload must not be used to reject valid DIY content.
7. **Exact revision acknowledgement.** A risky animation revision can run only after the user acknowledges the exact content revision hash; changing content invalidates the acknowledgement.
8. **Prepared-first performance.** Stage execution prefers validated, preloaded content and deterministic cues. It exposes readiness and degradation rather than hiding missing assets or a weak connection.
9. **Pages are an input, not a scene model.** Existing Pages/shortcuts seed Stage, but scenes and setlists get explicit persistent models with cue and transition semantics.
10. **Bounded runtime work.** Animation decoding, frame buffers, queues, logs, and caches have explicit limits. Lifecycle cancellation is cooperative and observable.
11. **Compatibility before consolidation.** New stores use versioned envelopes and migrate current JSON in place. Existing user content is preserved before dormant or duplicate screens are removed.
12. **Evidence gates release.** Signed or published artifacts depend on restore, tests, and unsigned platform builds. Physical verification remains separately recorded because CI cannot prove mask behavior.

## Dependency-ordered slices

Allowed status values are `Not Started`, `In Progress`, `Implemented`, `Verified`, `Externally Blocked`, and `Evidence-Excluded`.

| Slice | Status | Deliverable and exit evidence |
|---|---|---|
| S00 - Baseline and release protection | Externally Blocked | The ledger and pre-signing restore/test/unsigned-iOS/Android gate are complete and pass locally. A hosted `macos-26` workflow run is required to verify the service-side gate. |
| S01 - Unified BLE scheduler | Externally Blocked | One DI-owned bounded priority scheduler serializes all transport facades, with cancellation/coalescing, timeouts, generation invalidation, bounded nodes/wake signals, diagnostics, and deterministic tests. Real-radio disconnect, late-ACK, Stop, and Blackout behavior require a compatible mask. |
| S02 - Mask profiles and preflight | Externally Blocked | Hashed per-mask identity, capability snapshots, isolated slot ledgers, safe legacy migration, collision/capacity analysis, reports, and scheduler-backed preparation are complete. Physical identity stability, capability observation, and measured cadence require one or two real masks. |
| S03 - Animation engine and safety | Externally Blocked | The bounded deadline engine, restore/hold policy, metrics, scheduler cancellation, BPM/tap tempo, slot checks, flash analysis, and exact-revision overrides are complete. Timing, appearance, and safety rehearsal require a real mask. |
| S04 - Stage Mode | Externally Blocked | The reachable locked surface, layouts, prepared-only cues, keep-awake/haptics, readiness, emergency controls, and no-replay recovery are complete. Festival ergonomics and sustained rehearsal require a signed iPhone build and mask. |
| S05 - Studio and import | Externally Blocked | The editor, bounded history/timeline, safety/readiness integration, native bounded GIF/video decode, conversion controls, persistence, and prepared preview path are complete. Physical Files decode fidelity and mask playback require device fixtures. |
| S06 - Scenes and setlists | Externally Blocked | Typed bounded steps, validation, ordering, rehearsal, failure policy, atomic persistence, catalog/readiness projection, and persistent cue position are complete. End-to-end mask rehearsal remains a physical gate. |
| S07 - MaskPack v2 | Externally Blocked | The backend-free v2 archive, typed hashes and bounds, v1 migration, inspection/conflicts, reference remap, rollback journal, and startup recovery are complete. Files/AirDrop transfer and imported show rehearsal require signed devices and a mask. |
| S08 - Audio Labs | Externally Blocked | The reachable finite diagnostic, per-mask evidence gate, four mappings, bounded FFT/latest-buffer path, cadence and flash gates, native capture, scheduler coalescing, and interruption cancellation are complete. `960B` output, microphone sources, safety, thermal, and battery behavior require physical evidence. |
| S09 - Navigation, diagnostics, recovery | Externally Blocked | Library/Stage/Device navigation, Stage ownership, observed diagnostics, bounded histories, redacted report, scoped reset, and guided recovery are complete. Weak-link, interruption, and recovery checks require physical devices. |
| S10 - Face Studio, accessibility, ordering, localization | Externally Blocked | Face Studio, shared tokens/controls, color-independent state, target sizing, semantics, reduced motion, accessible ordering, and English/Czech entry points are complete. Dynamic Type, VoiceOver, contrast, safe-area, and secondary-screen checks require supported iPhones. |
| S11 - Apple Watch companion | Evidence-Excluded | The production phone boundary is implemented: strict typed JSON, bounded payload/state, stale/duplicate/out-of-order rejection, pre-dispatch replay marking, serialized ordinary actions, immediate Scene/scheduler-backed Stop/Blackout, Page/setlist position, favorites, brightness, readiness, connection state, reply haptics, and an iOS Watch Connectivity bridge. The installable SwiftUI companion is evidence-excluded because the pinned .NET 10/MAUI toolchain exposes iOS Watch Connectivity bindings but no watchOS target/workload or reference pack. Exact impact and enabling change are recorded below. |
| S12 - Lifecycle, performance, migration, release hardening | Externally Blocked | The reopened corrective pass is locally closed: post-upload emergency cancellation, transactional profile switching and live measurements, Preflight permission/multi-Page coverage, stale navigation, Face Studio tools and ordering gestures, English/Czech localization, lifecycle handling, and focused regression coverage are complete. Signed-device, physical-mask, Instruments, upgrade, accessibility, and hosted-workflow evidence remain external. |

## Feature ledger

`Implemented` means the code path exists but its required evidence is incomplete. `Verified` requires recorded automated or physical evidence appropriate to the row. `Externally Blocked` is reserved for a check that genuinely requires unavailable hardware, credentials, or service state. `Evidence-Excluded` requires a written product or protocol reason and is not a shortcut for unfinished work.

| Capability | Status | Evidence or next gate |
|---|---|---|
| Core protocol/parsing/scheduler/profile/preflight/animation/Stage/Studio/Scene/MaskPack/Audio/Watch/lifecycle test suite | Verified | 538 Release tests pass, including Watch, Audio, lifecycle, navigation, scheduler, transactional profile, migration, MaskPack, failure, and cancellation coverage. |
| iOS compile path | Verified | Local `net10.0-ios` simulator build passed with zero warnings on 2026-07-19; signed-device execution remains separate. |
| Android compile path | Verified | Local `net10.0-android` build passed with zero warnings on 2026-07-19. |
| Library catalog, search, grouping, favorites, ordering | Externally Blocked | View-model behavior is covered; rendered accessibility and regression checks require a supported phone. |
| Pages and prepared shortcuts | Externally Blocked | Persistence and coordinator tests pass; real-mask preparation, output, and recovery remain physical gates. |
| Device connection and foreground auto-connect | Externally Blocked | Seventeen tests cover exact/name identity, foreground-only scanning, pending-scan shutdown, connect races, manual disconnect, and reconnect policy. Weak-link, restoration, and Bluetooth interruption require real devices. |
| Text composer and editable presets | Externally Blocked | Core behavior is covered; physical alignment, bold, color, scrolling, and three-line output remain. |
| Stock faces and previews | Externally Blocked | Catalog, preview, and scheduled send paths are complete; device-specific command mapping needs mask evidence. |
| Face Studio and 46x58 DIY upload | Externally Blocked | Editing, migration, upload, and slot tests pass; physical crop/orientation/color/slot fidelity remains. |
| Production DIY animation engine | Externally Blocked | Controllable-clock tests cover deadlines, loops, dropped frames, disconnect/no replay, cancellation, restores, safety, Stop, and Blackout. Real-mask cadence and appearance remain. |
| CI validation before signing | Externally Blocked | Restore, 538 tests, strict unsigned iOS Release, and Android Release pass locally; a hosted `macos-26` run is required to prove the service-side pre-signing gate. |
| Unified BLE command ownership | Externally Blocked | All app consumers resolve one scheduler; 11 tests prove serialization, priority, bounded backpressure, 2,000-frame physical-node/wake-signal coalescing, immediate cancellation cleanup, emergency preemption, timeouts, and connection invalidation. Radio/firmware behavior remains. |
| Per-mask profiles and compatible slot migration | Externally Blocked | Fake-mask isolation, legacy slot migration, corrupt-store protection, and a pre-Audio schema-1 fixture pass without data loss. Signed install/upgrade and stable platform identity require devices. |
| Readiness/preflight report | Externally Blocked | Page/show/setlist analysis, allocation, blockers/warnings, observed evidence, recovery, and preparation are covered. Physical cadence, storage, and identity rehearsal remain. |
| Safe animation engine and exact-revision override | Externally Blocked | Shared conservative analysis and exact-revision acknowledgement behavior are covered; physical photosensitivity/cadence rehearsal remains. |
| Stage Mode | Externally Blocked | Prepared-only triggering, hold restore/unlock, no-replay recovery, persistent delivery, and emergency preemption are covered. Festival ergonomics and sustained rehearsal require iPhone/mask hardware. |
| Consolidated Studio/import workflow | Externally Blocked | Compiler, persistence, hostile-input, conversion, editing, catalog, Preflight, and prepared-replay tests pass; physical Files decoder fidelity remains. |
| Scenes and setlists | Externally Blocked | Seventeen focused tests cover validation, ordering, restore/failure policy, waits, concurrency, persistence, cue position, projection, and activation. Physical transitions and rehearsal remain. |
| MaskPack v2 round-trip | Externally Blocked | Thirty-four focused tests cover v2/v1 round trips, typed content, conflicts, references, stale previews, rollback/recovery, and hostile archives. Files/AirDrop and mask rehearsal remain. |
| Audio-reactive Labs | Externally Blocked | Twenty-two focused tests cover encrypted fixtures, framing/modes, finite diagnostics, evidence integrity, per-mask isolation, FFT/mappings, Drop/flash gates, bounded frames, cancellable permission/start and calibration boundaries, and producer cancellation. `960B`, native microphone, visual, cadence, thermal, and battery checks remain physical. |
| Diagnostics/support bundle/recovery | Externally Blocked | Observed capability/transport/scheduler/animation state, bounded histories, redaction, scoped reset, and guided recovery are complete. Physical failure/recovery rehearsal remains. |
| Consolidated navigation and dormant-screen decision | Verified | Root navigation is Library/Stage/Device; Pages, editors, Scene Studio, and Audio Labs compile as reachable child routes. Dormant Home/React/Rave registrations remain deliberately preserved. |
| Accessibility/localization/order hardening | Externally Blocked | Semantics, color-independent state, contrast tokens, reduced motion, target sizing, native plus accessible ordering, and 502-key English/Czech resource parity compile. Assistive-technology and secondary-screen checks require supported iPhones. |
| Apple Watch companion | Evidence-Excluded | No installable Watch UI can be produced by the pinned toolchain. The iPhone now owns a tested Watch Connectivity contract for Page/setlist position, triggerable favorites, previous/next/current cue, Crown brightness, readiness, connection status, result haptics, Stop, and immediate Blackout; it never delegates BLE ownership. |
| Lifecycle/performance/release evidence audit | Externally Blocked | Local audit and reproducible procedure are complete; signed device/Instruments measurements, upgrade rehearsal, and hosted workflow evidence remain external. |

### S11 Apple Watch tooling boundary

- **Evidence:** SDK `10.0.300` with workload set `10.0.301.1` has `android` and `maui-mobile`; `dotnet workload search watchos` returns no workload, `dotnet workload search ios` returns only `ios` and `maui-ios`, and installed packs include Microsoft.iOS but no Watch/watchOS pack. Microsoft's [.NET MAUI supported-platform list](https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms?view=net-maui-10.0) names Android, iOS, Mac Catalyst, and Windows, not watchOS; current [dotnet/macios releases](https://github.com/dotnet/macios/releases) list iOS, tvOS, macOS, Mac Catalyst, Android, and MAUI workload IDs, not watchOS.
- **User impact:** this repository cannot currently build, sign, install, render, or physically validate a Watch screen, Digital Crown UI, or Watch haptic. The iPhone bridge therefore reports `CompanionNotInstalled` truthfully and no Watch action can reach mask hardware without the phone app receiving and validating it.
- **Implemented foundation:** versioned `sendMessageData` JSON actions and `stateJson` application context; strict 16 KiB bounds; required fields and string-only enums; ten-second stale/future guards; per-sender sequence and message replay protection; bounded favorites/state; existing Stage, Scene, scheduler, safety, and BLE ownership; explicit foreground/staleness metadata; iOS reachability/paired/install status; and 28 deterministic tests.
- **Smallest enabling change:** add a native SwiftUI watchOS companion target with its own bundle identifier and Watch provisioning to the Apple build environment, decode `stateJson`, send the existing action envelope through `WCSession.sendMessageData`, map result haptics, and add a signed Watch build/device job. No protocol, phone BLE, scheduler, or persistence redesign is needed.

## Verification evidence

Add evidence only after a command or physical check completes. Preserve failure evidence when it changes a design decision.

| Date | Slice | Evidence | Result |
|---|---|---|---|
| 2026-07-17 | Baseline | `dotnet restore MaskApp.slnx` | Passed. |
| 2026-07-17 | Baseline | `dotnet test tests/MaskApp.Core.Tests/MaskApp.Core.Tests.csproj` | Passed, 291 tests. |
| 2026-07-17 | Baseline | `dotnet build src/MaskApp.App/MaskApp.App.csproj -f net10.0-ios` | Passed, zero warnings/errors. |
| 2026-07-17 | Baseline | `dotnet build src/MaskApp.App/MaskApp.App.csproj -f net10.0-android --no-restore -m:1 -nodeReuse:false` | Passed, zero warnings/errors; a prior concurrent timeout was not reproducible. |
| 2026-07-17 | S00/S01 | Scheduler-focused Core test filter | Passed, 8 tests. |
| 2026-07-17 | S00/S01 | Full Core test suite after scheduler integration | Passed, 298 tests. |
| 2026-07-17 | S00/S01 | iOS and serial Android builds after DI integration | Passed, zero warnings/errors. |
| 2026-07-17 | S02 | Profile and Preflight focused suites | Passed, 16 tests including two-mask isolation, legacy migration, corrupt-store protection, allocation, readiness, and scheduler-backed preparation. |
| 2026-07-17 | S02 | Full Core suite after profile/Preflight integration | Passed, 314 tests. |
| 2026-07-17 | S03 | Animation/flash/Preflight/coordinator focused suites | Passed, 34 tests with a controllable monotonic clock and fake transports. |
| 2026-07-17 | S03 | Full Core suite after production animation migration and flash-safety wiring | Passed, 333 tests. |
| 2026-07-17 | S03 | iOS simulator and serial Android builds plus `git diff --check` | Passed with zero warnings/errors and no whitespace errors. |
| 2026-07-17 | S04 | Stage state-machine focused suite | Passed, 10 tests covering layouts, prepared-only action gating, cue state, haptic outcome, hold restore/unlock, disconnect recovery without replay, and Blackout preemption during blocked work. |
| 2026-07-17 | S04 | Full Core suite, iOS simulator build, serial Android build, and `git diff --check` | Passed, 343 tests and zero build warnings/errors. |
| 2026-07-17 | S05 | Studio/import/compiler/store/catalog/Preflight/coordinator focused suites | Passed, 21 new deterministic tests covering bounded hostile input, project editing and persistence, exact-revision safety, custom-animation projection, and prepared replay. |
| 2026-07-17 | S05 | Full Core suite after Studio/import integration | Passed, 364 tests. One deadline-loop test was late during the first parallel run, then passed alone and in the complete rerun; no deterministic failure remained. |
| 2026-07-17 | S05 | iOS simulator and serial Android builds after native GIF/video decoder and route integration | Passed with zero warnings/errors. |
| 2026-07-17 | S06 | Scene/setlist/Preflight/Stage focused suites | Passed, 17 new tests; the combined focused run with Stage regressions passed 27 tests. |
| 2026-07-17 | S06 | Full Core suite after Scene/setlist production wiring | Passed, 381 tests. One pre-existing assertion expected the stable `content-upload-required` issue code; compatibility was restored and the full rerun passed. |
| 2026-07-17 | S06 | iOS simulator and serial Android builds plus `git diff --check` | Passed with zero warnings/errors and no whitespace errors. |
| 2026-07-17 | S07 | MaskPack parser/payload/archive/conflict/recovery/view-model focused suites | Passed, 34 tests covering v2 and v1 migration, full typed-content round trips, conflict/reference semantics, journal rollback/recovery, and hostile archives. |
| 2026-07-17 | S07 | Full Core suite after MaskPack v2 production wiring | Passed, 412 tests. Two initial full runs exposed an old exact-alternation assertion that contradicted the deadline engine's intentional late-frame dropping; the isolated test passed, its assertion was narrowed to the actual loop/stop/reuse contract, and the complete rerun passed. |
| 2026-07-17 | S07 | iOS simulator and serial Android builds plus `git diff --check` | Passed with zero warnings/errors and no whitespace errors. |
| 2026-07-18 | S09/S10 | Full Core suite after `master` integration, primary-surface redesign, and truthful-state hardening | Passed, 441 tests. |
| 2026-07-18 | S09/S10 | Source XAML parse, iOS simulator build, Android build, and Roslyn diagnostics | Passed; both target builds and semantic diagnostics reported zero warnings/errors. |
| 2026-07-19 | S08 | Audio protocol/diagnostic/processor/engine plus scheduler/profile focused suites | Passed, 18 dedicated Audio tests plus scheduler and profile coverage for coalescing, producer cancellation, capability observation, and exact-mask evidence isolation. |
| 2026-07-19 | S08 | Full Core suite after Audio Labs production wiring | Passed, 463 tests. The first full run exposed a shutdown scheduling race under parallel load; the processing loop was started synchronously to its first await and the complete rerun passed. |
| 2026-07-19 | S08 | iOS simulator build, Android build, and Roslyn diagnostics | Passed with zero warnings/errors. Native microphone and `960B` physical-mask execution remain external. |
| 2026-07-19 | S11 | Pinned SDK/workload/reference-pack spike plus official platform inventories | No `watchos` workload or pack exists in the pinned .NET 10/MAUI toolchain; the iOS pack does expose Watch Connectivity, so the phone boundary was implemented and the companion UI was evidence-excluded with a concrete enabling change. |
| 2026-07-19 | S11 | Watch contract/processor/coordinator focused suite | Passed, 23 tests covering strict decoding, payload bounds, stale/future/duplicate/out-of-order rejection, failed-action replay prevention, state/favorite filtering, Stage routing, and Blackout bypass during an active cue. |
| 2026-07-19 | S11 | Full Core suite after Watch phone-boundary wiring | Passed, 486 tests. |
| 2026-07-19 | S11 | iOS simulator and Android builds | Passed with zero warnings/errors; the iOS build compiles the native Watch Connectivity bridge. No Watch target exists to build under the pinned toolchain. |
| 2026-07-19 | S12 | Independent lifecycle/concurrency review | Resolved retained scheduler nodes and wake debt, late/pending foreground scan and connect races, queued Watch foreground/cancellation reply races, Audio diagnostic/calibration/start cancellation, and background shutdown cascade isolation. Regression filters pass: 11 scheduler, 17 auto-connect, 22 Audio, and 28 Watch tests. |
| 2026-07-19 | S12 | Migration compatibility fixture | A pre-Audio schema-1 profile document loads in place with its prepared slot intact and safe `Unknown`/false defaults. Signed install/upgrade over actual app data remains external. |
| 2026-07-19 | S12 | Strict XAML compilation audit | The first strict Release compile surfaced 84 `XC0022`/`XC0025` binding warnings. Source-compiled bindings, explicit item/root types, and warnings-as-errors were added; final iOS and Android Release builds report zero warnings/errors. |
| 2026-07-19 | S12 | Clean automated release matrix | Forced restore passed; all 501 Core Release tests passed; `net10.0-ios` and `net10.0-android` Release builds passed with zero warnings/errors. The release workflow now runs restore, tests, unsigned iOS, and Android before signing on every event; hosted execution remains external. |
| 2026-07-19 | S12 | Android AOT repeatability | A final incremental Windows cross-AOT run failed first in `llvm-mc` for unchanged framework assemblies, and an interrupted diagnostic rerun left stale generated state that produced `XAGNM7009`. After the orphaned linker exited and MSBuild cleaned only Android Release intermediates, the serialized two-ABI Release build passed from a clean state with zero warnings/errors in 4m31s. No app-source, managed compile, linker, XAML, or package configuration defect remained; the hosted Android gate remains authoritative. |
| 2026-07-19 | S12 | Completion-contract audit reopened local work | Two independent read-only audits found locally actionable gaps despite the earlier external-only classification: upload quiet periods delayed emergency cancellation; failed profile switches were not transactional and live latency/cadence was not persisted; Preflight omitted runtime permission state and arbitrary selected Pages; retained Home/Rave callers still targeted `//connect`; Face Studio lacked required editing tools and most ordering surfaces lacked native gestures; localization plus route/lifecycle/accessibility-sensitive test coverage was incomplete. S12 remains the sole integrated slice in progress until these findings are resolved and revalidated. |
| 2026-07-19 | S12 | Static and source integrity checks | Roslyn refreshed 417 files/6,308 declarations and reported zero compiler diagnostics; 32 source XAML/XML/plist files parsed; scoped `dotnet format` verification, unfinished-marker/secret scans, and `git diff --check` passed. Repository-wide formatting also reported older drift in untouched files, which was deliberately not folded into this roadmap diff. |
| 2026-07-19 | S12 | Sustained performance procedure | `docs/performance-validation.md` records the exact signed-build metadata, 30-minute Stage, 10-minute animation, repeated near-limit imports, 15-minute-per-mode Audio, 20 lifecycle cycles, Instruments traces, stop conditions, and evidence ownership. No physical result is claimed. |
| 2026-07-19 | S12 | Corrective hardening closeout | Emergency cancellation, native-style transactional profile activation, measured profile data, runtime-aware multi-Page Preflight, route cleanup, complete Face Studio tools, native and accessible ordering, lifecycle coordination, and reachable English/Czech UI were independently reviewed with no remaining P0-P2 finding. The profile seam passes six focused tests, including a deterministic `Connected`-then-`Disconnected` blocked-save race. |
| 2026-07-19 | S12 | Final local release matrix after corrective pass | All 538 Core Release tests pass; strict iOS simulator and serialized two-ABI Android Release builds pass with zero warnings/errors; scoped formatting verification passes; 32 source XAML/XML/plist files parse; English/Czech resources have 502/502 key parity, matching format tokens, and no missing referenced key; `git diff --check` reports no whitespace error. |

## Physical-device evidence checklist

These checks are deliberately separate from simulated and automated evidence. Record date, app revision, phone/OS, mask identity/firmware observation, result, and any captured diagnostic bundle for every run.

### User-owned external verification batch

Run this as one batch when Apple signing credentials, a supported iPhone, and a
compatible mask are available:

1. Run the hosted `ios-ipa.yml` workflow for this exact revision and retain the
   successful `validate` and signed IPA job links.
2. Install the signed Release IPA over an existing MaskApp installation. Record
   the revision, app/build version, iPhone model, iOS version, mask/profile,
   whether existing content survived, and rollback/recovery outcome.
3. Execute every unchecked item below and the sustained scenarios in
   `docs/performance-validation.md` in the same device campaign. Export the
   redacted Device diagnostic before and after each scenario and retain the
   Allocations/Leaks, Time Profiler, and Energy Log evidence.
4. Return the workflow links, device/build record, checklist results, diagnostic
   exports, and Instruments summaries so the affected rows can move from
   `Externally Blocked` to `Verified` without repeating local checks.

The Apple Watch install/UI/Crown/haptic campaign is not part of this batch; it
remains `Evidence-Excluded` until the S11 watchOS enabling change exists.

### Connection and command safety

- [ ] Connect, disconnect, foreground/background, Bluetooth toggle, and app relaunch on a real iPhone and mask.
- [ ] Reconnect during queued text, static image, and animation work without stale-command replay.
- [ ] Control/blackout preemption during bulk upload; prove no concurrent writes or protocol corruption.
- [ ] Weak signal, out-of-range, powered-off mask, and mid-command disconnect recovery.
- [ ] Two-mask profile isolation and slot-ledger behavior, if a second compatible mask is available.

### Content and performance

- [ ] Stock face commands and previews match the physical mask.
- [ ] Text alignment, color, bold, scrolling, and three-line payloads match the composer.
- [ ] 46x58 DIY static output preserves crop, orientation, color, and intended slot.
- [ ] GIF and video import through Files preserves crop/fit/position, palette/dither choice, sampled timing, orientation, duplicate removal, preview, and actionable rejection behavior.
- [ ] Animation timing, looping, stop-to-last-stable, blackout, cancellation, and dropped-frame behavior.
- [ ] Flash/rate/battery safety warnings and exact-revision acknowledgement on the acknowledged content only.
- [ ] Stage current/next cue, rapid cueing, accidental-action guards, degraded state, and recovery during a rehearsal-length run.
- [ ] Scene/setlist preparation, transitions, resume, reorder, duplicate, relaunch persistence, and failure reporting.
- [ ] MaskPack import/export round-trip with current, legacy, corrupt, duplicate, and conflicting packages.
- [ ] Discover `960B`, run the finite Audio Labs sequence on each intended mask/framing/mode profile, confirm return-to-dark, and record palette, orientation, stability, write cadence, and failure behavior before marking that mask Passed.
- [ ] Rehearse Spectrum, Bass Face, Voice Mouth, and Drop Detector with quiet-room calibration and representative ambient/music/voice sources; verify sensitivity, threshold, smoothing, the two-per-second Drop limit, live flash suppression, Stop, and Blackout.
- [ ] Lock/background, navigate away, disconnect, deny/revoke microphone permission, receive a call/audio interruption, and change the input route; prove capture stops and never resumes without an explicit Start.

### Platform, accessibility, and companion

- [ ] Dynamic Type, VoiceOver, contrast, reduced motion, touch targets, orientation, and safe-area behavior on supported iPhones.
- [ ] Camera/photo permissions, microphone denial/interruption, phone call/audio-route interruption, and no-audio fallback.
- [ ] Sustained CPU, memory, thermal, and battery behavior during a rehearsal-length foreground performance.
- [ ] Android connect/send/animation/recovery smoke test on supported hardware.
- Evidence-excluded under the pinned toolchain: Watch install/UI/Crown/haptic/device checks. After the S11 enabling change, verify stale/disconnected state, next/previous/go/stop/blackout, phone relaunch, and delayed-message behavior on signed iPhone and Watch hardware.
- [ ] Signed IPA install/upgrade over existing user data, launch, reconnect, and rollback/recovery documentation.

## Remaining risks and external gates

- A real compatible mask and iPhone are required for protocol, timing, reconnection, safety, and sustained-performance claims. Automated fakes can prove ordering and failure policy, not radio or firmware behavior.
- Two-mask isolation cannot be fully verified without two distinguishable devices; one-mask compatibility still must be proven first.
- The installable Apple Watch companion is evidence-excluded until a watchOS-capable native build target and Watch provisioning are added; signed iPhone validation still requires matching Apple signing assets and physical devices.
- Audio Labs evidence depends on microphone permission and representative ambient/music sources; it remains Experimental until checked.
- The reverse-engineered protocol may not expose storage, battery, acknowledgement, or transition capabilities uniformly. Unsupported behavior must be surfaced honestly and may become `Evidence-Excluded` only with recorded protocol evidence.
- Separate JSON stores do not provide a native cross-file transaction. MaskPack now journals and tests rollback/recovery across its five owned stores, but abrupt power-loss durability and upgrade behavior still require device-level validation.

## Completion rule

The roadmap is complete only when every in-scope ledger row is `Verified` or has a justified `Externally Blocked`/`Evidence-Excluded` state, all clean-checkout restore/test/build gates pass, migrations preserve existing data, simulated failure matrices pass, independent review findings are resolved, and the physical checklist contains evidence or an explicit user-owned external gate. A passing build alone is not completion.
