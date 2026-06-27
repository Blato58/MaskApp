# Protocol Text Stabilizer

## Slice

- Name: Protocol/Text stabilizer
- Date: 2026-06-27
- Status: validated
- Owner: Codex parent integrator
- Product pillar: Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Correct obvious stock-protocol mismatches before fast reaction and RAVE controls
depend on text upload.

## Target user moment

The wearer can send short captions for real-mask testing without the app using
known-wrong text mode commands or ambiguous BLE routing.

## Observer-facing value

Short text reactions such as `LOL`, `DROP`, and `WHEEL UP` have a clearer path
to appear on the mask during physical validation.

## Final-goal contribution

Moves MaskApp from a migration workbench toward a reliable wearable face
controller by making the low-level text path more protocol-honest.

## Capability claims

Encrypted commands target the command characteristic. Text frames prefer the
text/image upload characteristic. ACK subscription prefers the notification
characteristic. Compatibility fallbacks remain when exact characteristics are
missing. Physical behavior is not claimed until tested on a real mask.

## User-visible improvement

Text UI mode labels now match documented modes, and ACK/write-only statuses can
recognize more real-world responses.

## Current evidence

- Repo files: `MaskBleProtocol`, `MaskCommandBuilder`, `TextUploadProtocol`,
  iOS and Android BLE adapters.
- Java evidence: original Android snapshot remains migration evidence; exact
  protocol details are recorded in `docs/stock-mask-protocol.md`.
- Existing tests: text protocol and quick-action tests.
- Existing validation gaps: physical iOS/Android mask send remains unverified.

## Scope

In scope:

- UUID constants, text mode command, ACK parsing, text frame characteristic
  routing, and focused tests.

Out of scope:

- Firmware changes, custom firmware, image upload, audio visualizer, automatic
  RAVE features, and physical-device claims.

## Files and flows

- Core: `src/MaskApp.Core/Features/MaskControl`, `src/MaskApp.Core/Features/Text`
- App UI: Text mode labels
- Platform adapters: iOS and Android BLE write/notify characteristic selection
- Docs: `docs/stock-mask-protocol.md`, `docs/progress.md`

## Test plan

- Unit tests: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
- Build validation: iOS and Android MAUI builds
- Browser/simulator/device validation: not applicable in this run
- Skipped validation and reason: real mask testing requires user hardware

## Deferred validation

- iOS real-mask ACK-required text upload
- iOS real-mask write-only text upload
- Android real-mask parity after iOS behavior is known

## Overclaim check

- No physical text behavior is marked as verified.
- Firmware/custom firmware is excluded.
- Apple Watch remains backlog only.

## Measured outcome

- Changes made: protocol constants, `MODE` text command, broader ACK parsing,
  exact characteristic preferences with fallback, and tests.
- Commands run: core tests, Roslyn diagnostics, iOS build, Android build.
- Result: all passed locally.
- Remaining risk: real mask may still require timing or DATS argument changes.

## Next slice candidate

- Physical iOS text upload checklist execution on real hardware.
