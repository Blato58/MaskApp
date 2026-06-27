# RAVE FAST MVP

## Slice

- Name: RAVE FAST MVP
- Date: 2026-06-27
- Status: validated
- Owner: Codex parent integrator with Control Room, React, and RAVE slice workers
- Product pillar: Instant Reactions, RAVE / DnB Festival Mode, Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Make MaskApp usable in less than a week as a manual, offline festival reaction
remote instead of a migration roadmap.

## Target user moment

Open the app, connect or recover the mask, hit `BLACKOUT`, send a short
reaction, and switch to RAVE for large DnB controls.

## Observer-facing value

People looking at the mask can read quick reactions such as `LOL`, `DROP`,
`WHEEL UP`, `HYDRATE`, and `TOO MUCH BASS` without the wearer speaking.

## Final-goal contribution

Establishes the phone-side command model that future React, RAVE, Library, and
Apple Watch companion remote surfaces can share through stable app-layer intent
IDs.

## Capability claims

All RAVE FAST actions are manual/offline short captions or brightness commands.
Drop Detector, Voice Mouth, automatic Bass Face, microphone visualizer,
real-time video, GIF-ish playback, Image Studio, AI, watchOS, firmware, and
custom firmware remain out of scope. `BASS FACE` is a manual caption only.

## User-visible improvement

- Home is now Control Room.
- React is a one-tap deck grouped by use case.
- RAVE is a manual-first party deck with large buttons, blackout, brightness
  cap, Festival Lock, connect affordance, and honest send/readiness status.

## Current evidence

- Repo files: Home, React, RAVE, QuickActions, Text, MaskControl, BLE adapters.
- Java evidence: protocol-facing behavior remains tied to existing command/text
  builders and migration docs.
- Existing tests: Home, React, RAVE, QuickActions, Text, and MaskControl tests.
- Existing validation gaps: no real-mask physical test in this run.

## Scope

In scope:

- Control Room dashboard, Reaction Deck, RAVE manual deck, stable quick-action
  IDs, dispatcher, Shell/DI registration, and docs.

Out of scope:

- Persistence, presets library, Image Studio, Drop Detector, Voice Mouth,
  automatic Bass Face, audio visualizer, AI, Apple Watch code, firmware, and
  custom firmware.

## Files and flows

- Core: `Features/Home`, `Features/React`, `Features/Rave`,
  `Features/QuickActions`
- App UI: Home, React, RAVE pages and Shell tabs
- Platform adapters: only protocol characteristic routing changed
- Docs: progress tracker and slice records

## Test plan

- Unit tests: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`
- Build validation: `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`
  and `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`
- Browser/simulator/device validation: not performed; this is a MAUI app and no
  simulator/device run was available in this turn.
- Skipped validation and reason: physical mask validation requires user hardware.

## Deferred validation

- Real iOS device: connect, blackout, brightness cap, `LOL`, `DROP`, `WHEEL UP`,
  React random, RAVE write-only fallback, reconnect.
- Android physical parity after iOS behavior is known.

## Overclaim check

- RAVE is labeled manual/offline and low-bandwidth.
- Labs/Experimental features are not presented as implemented behavior.
- Apple Watch is only enabled by shared intent IDs, with no watchOS code.

## Measured outcome

- Changes made: new quick-action model/dispatcher, Control Room, React deck,
  RAVE deck, Shell/DI registration, protocol routing, and docs.
- Commands run: core tests, Roslyn diagnostics, iOS build, Android build.
- Result: all passed locally.
- Remaining risk: physical BLE behavior and exact mask display timing still need
  real-mask testing.

## Next slice candidate

- Run the physical real-mask checklist and record results before adding gallery
  scanner or persistence.
