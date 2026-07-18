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
| S00 - Baseline and release protection | Implemented | Living ledger plus PR/release validation that gates signing on restore, tests, iOS build, and Android build. A hosted workflow run is the remaining verification gate. |
| S01 - Unified BLE scheduler | Implemented | One DI-owned bounded priority scheduler serializes all three transport facades, with cancellation/coalescing, timeouts, generation invalidation, diagnostics, and deterministic fake-transport tests. Stop cancels without output; BLACKOUT cancels active/queued visuals and bypasses backpressure. Physical disconnect/late-ACK behavior remains. |
| S02 - Mask profiles and preflight | Implemented | Hashed per-mask identity, capability snapshots, ACK/write-only state, isolated slot ledgers, safe one-time legacy migration, collision/capacity analysis, one-Page/whole-show reports, and scheduler-backed DIY preparation are production-wired. Physical identity/capability checks and real latency/cadence values remain. |
| S03 - Animation engine and safety | Implemented | Existing Holy Priest playback now uses one bounded production engine with per-frame monotonic deadlines, finite/continuous loops, pause/resume, dropped/late metrics, stable-look restore, hold release, disconnect cancellation, scheduler Stop/Blackout, BPM/tap tempo, unique-frame deduplication, slot limits, load/cadence checks, conservative flash analysis, and persisted exact-revision overrides. Physical timing/safety rehearsal remains. |
| S04 - Stage Mode | Implemented | Reachable locked surface provides giant, 2×2, and dense layouts; Page swipe/arrows; prepared-only tiles; current/next cue state; keep-awake; haptics; explicit readiness; hold-to-play/restore; hold-to-exit; persistent Stop/Blackout; and connection-loss recovery without replay. Scene/setlist tile sources join in S06. Physical festival ergonomics remain. |
| S05 - Studio and import | Implemented | Reachable Animation Studio provides a selected-frame 46x58 editor, timeline, native drag/drop plus move controls, onion skin, duration/loop/BPM/tap-tempo controls, live unique-slot and exact-revision safety status, explicit save/delete, and prepare/preview through the production scheduler/engine. GIF/video Files import is bounded before and after native decode, samples frames, preserves bounded timing, removes consecutive duplicates, supports crop/fit/position/palette/dither, and remains an unsaved preview until confirmed. Saved projects flow through Library, Pages, Stage readiness, and Preflight. Physical decoder/fidelity and mask playback checks remain. |
| S06 - Scenes and setlists | Implemented | Reachable Scene Studio provides bounded typed brightness/speed/face/text/animation/wait/repeat/restore/Stop/Blackout steps, validation, native drag/drop plus accessible ordering, duplication, rehearsal, exact partial-failure policy, atomic persistence, Library/Pages projection, nested Preflight, prepared Stage tiles, and persistent setlist cue position with explicit Pages/setlist switching. Physical rehearsal remains. |
| S07 - MaskPack v2 | Implemented | Reachable backend-free Files/share/AirDrop workflow exports and inspection-first imports faces, animations/timing, text presets, Pages, Scenes, setlists, and safe Library ordering. V2 separates 46x58 art from 44x58 text, hashes every typed payload, bounds hostile ZIP/JSON input, migrates CRC-checked v1 PNGs, previews per-item merge/rename/skip/confirmed-replace conflicts, remaps references, rejects stale previews, and journals cross-store rollback/startup recovery. Physical transfer and mask-content rehearsal remain. |
| S08 - Audio Labs | Not Started | Permission-aware bounded audio pipeline, configurable smoothing/sensitivity, spectrum and beat modes, explicit experimental labels, graceful interruption, and no-audio fallback. |
| S09 - Navigation, diagnostics, recovery | Not Started | Coherent reachable information architecture, bounded privacy-safe event log, connection/scheduler metrics, exportable support bundle, and guided recovery actions. |
| S10 - Face Studio, accessibility, ordering, localization | Not Started | Preserve and harden Face Studio; consistent ordering and semantics; Dynamic Type, VoiceOver, contrast, reduced motion, localization-ready strings, and reachable controls. |
| S11 - Apple Watch companion | Not Started | Minimal native companion for next/previous/go/stop/blackout and state display, phone-owned BLE execution, stale-state safety, and CI build/signing path. |
| S12 - Lifecycle, performance, migration, release hardening | Not Started | Bounded memory/CPU/battery behavior, interruption recovery, migration fixtures, clean restore/test/build, automated release gates, and completed evidence audit. |

## Feature ledger

`Implemented` means the code path exists but its required evidence is incomplete. `Verified` requires recorded automated or physical evidence appropriate to the row. `Externally Blocked` is reserved for a check that genuinely requires unavailable hardware, credentials, or service state. `Evidence-Excluded` requires a written product or protocol reason and is not a shortcut for unfinished work.

| Capability | Status | Evidence or next gate |
|---|---|---|
| Core protocol/parsing/scheduler/profile/preflight/animation/Stage/Studio/Scene/MaskPack test suite | Verified | 412 tests passed after the MaskPack v2 slice. |
| iOS compile path | Verified | Local `net10.0-ios` build passed with zero warnings at the baseline snapshot; signed-device execution remains separate. |
| Android compile path | Verified | Serial `net10.0-android` build passed with zero warnings at the baseline snapshot. |
| Library catalog, search, grouping, favorites, ordering | Implemented | View-model coverage exists; rendered accessibility and regression checks remain. |
| Pages and prepared shortcuts | Implemented | Persistence/coordinator tests exist; real-mask preparation and recovery remain. |
| Device connection and foreground auto-connect | Implemented | Physical reconnect, weak-link, and interruption matrix remains. |
| Text composer and editable presets | Implemented | Core behavior tests exist; real-mask style and three-line checks remain. |
| Stock faces and previews | Implemented | Catalog/send paths exist; device-specific command mapping remains physically unverified. |
| Face Studio and 46x58 DIY upload | Implemented | Editing/upload code and tests exist; physical image fidelity and slot behavior remain. |
| Production DIY animation engine | Implemented | The old fixed-delay loop is removed. Controllable-clock tests cover deadlines, pause/resume, finite/continuous playback, dropped frames, disconnect/no replay, cancellation, hold restore, stable-look restore, safety blocking, Stop, and Blackout. Real-mask cadence remains. |
| CI validation before signing | Implemented | Restore, 412 tests, and both platform builds pass locally; hosted `macos-26` execution remains. |
| Unified BLE command ownership | Implemented | All app consumers resolve one scheduler; 8 deterministic tests prove serialization, priority, coalescing, bounded backpressure, Stop/BLACKOUT preemption, cancellation, timeout recovery, and connection invalidation. Real-mask behavior remains. |
| Per-mask profiles and compatible slot migration | Implemented | Two-fake-mask tests prove no slot leakage; legacy global slots migrate once as unverified and unreadable profile data is not overwritten. Real iOS/Android identity stability remains. |
| Readiness/preflight report | Implemented | Reachable Pages workflow analyzes one Page or the whole show, deduplicates/allocates slots, blocks collisions/capacity errors, explains recovery, and prepares eligible DIY content. Scenes, permissions, safety, and measured physical cadence join as their slices land. |
| Safe animation engine and exact-revision override | Implemented | Playback and Preflight share conservative full-sequence/loop-boundary analysis. Unsafe revisions are blocked until an explicit photosensitivity warning is accepted; acknowledgement is persisted, reversible, and invalidated by content, timing, or BPM changes. |
| Stage Mode | Implemented | Pages exposes a locked full-screen route. Deterministic tests cover layouts, prepared-only triggering, typed success/failure feedback, hold restore, hold-to-unlock boundary, disconnect/reconnect with no replay, and distinct Stop/Blackout emergency preemption. Scene/setlist projection remains coupled to S06. |
| Consolidated Studio/import workflow | Implemented | Twenty-one new tests cover compilation/deduplication, exact revisions, compact atomic persistence and corrupt fallback, hostile import limits/signatures/timing, 46x58 conversion modes, unsaved preview semantics, editor operations, catalog projection, Preflight, and prepared replay. Native GIF/video decoders compile for iOS and Android; physical Files/import fidelity remains. |
| Scenes and setlists | Implemented | Seventeen focused tests cover bounded repeat/type validation, deterministic ordering/restore/partial failure, wait cancellation, concurrent-scene rejection, terminal Stop/Blackout, atomic/corrupt-store behavior, persistent cue position, catalog/readiness projection, nested Preflight deduplication, and Studio save/duplicate/setlist activation. Physical mask rehearsal remains. |
| MaskPack v2 round-trip | Implemented | Thirty-four focused tests cover complete-show and maximum-repeated-animation round trips, v1 44x58 PNG migration, typed payload/device-state stripping, semantic references, all conflict actions, stale previews, transactional rollback/startup recovery, corrupt-journal preservation, and hostile path/hash/CRC/ZIP/size/count/compression/null inputs. Physical Files/AirDrop transfer and real-mask rehearsal remain. |
| Audio-reactive Labs | Not Started | S08. |
| Diagnostics/support bundle/recovery | Not Started | S09. |
| Consolidated navigation and dormant-screen decision | Not Started | S09 after Stage/Studio routes are real. |
| Accessibility/localization/order hardening | Not Started | S10. |
| Apple Watch companion | Not Started | S11. |
| Lifecycle/performance/release evidence audit | Not Started | S12. |

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

## Physical-device evidence checklist

These checks are deliberately separate from simulated and automated evidence. Record date, app revision, phone/OS, mask identity/firmware observation, result, and any captured diagnostic bundle for every run.

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

### Platform, accessibility, and companion

- [ ] Dynamic Type, VoiceOver, contrast, reduced motion, touch targets, orientation, and safe-area behavior on supported iPhones.
- [ ] Camera/photo permissions, microphone denial/interruption, phone call/audio-route interruption, and no-audio fallback.
- [ ] Sustained CPU, memory, thermal, and battery behavior during a rehearsal-length foreground performance.
- [ ] Android connect/send/animation/recovery smoke test on supported hardware.
- [ ] Watch install, stale/disconnected state, next/previous/go/stop/blackout, phone relaunch, and delayed message behavior.
- [ ] Signed IPA install/upgrade over existing user data, launch, reconnect, and rollback/recovery documentation.

## Remaining risks and external gates

- A real compatible mask and iPhone are required for protocol, timing, reconnection, safety, and sustained-performance claims. Automated fakes can prove ordering and failure policy, not radio or firmware behavior.
- Two-mask isolation cannot be fully verified without two distinguishable devices; one-mask compatibility still must be proven first.
- Apple Watch and signed iPhone validation require matching Apple signing entitlements/profiles and physical devices.
- Audio Labs evidence depends on microphone permission and representative ambient/music sources; it remains Experimental until checked.
- The reverse-engineered protocol may not expose storage, battery, acknowledgement, or transition capabilities uniformly. Unsupported behavior must be surfaced honestly and may become `Evidence-Excluded` only with recorded protocol evidence.
- Separate JSON stores do not provide a native cross-file transaction. MaskPack now journals and tests rollback/recovery across its five owned stores, but abrupt power-loss durability and upgrade behavior still require device-level validation.

## Completion rule

The roadmap is complete only when every in-scope ledger row is `Verified` or has a justified `Externally Blocked`/`Evidence-Excluded` state, all clean-checkout restore/test/build gates pass, migrations preserve existing data, simulated failure matrices pass, independent review findings are resolved, and the physical checklist contains evidence or an explicit user-owned external gate. A passing build alone is not completion.
