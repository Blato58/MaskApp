# Built-In Archive And MaskPack Format

## Slice

- Name: Built-in Archive and MaskPack Format
- Date: 2026-06-27
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions, Creative Composition, RAVE / DnB Festival Mode
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

Let the wearer save useful stock built-in IDs while scanning the mask, then reuse
favorite IDs from React and RAVE without pretending the app can extract stock
firmware frames.

## Target user moment

Stand near the mask, send the next `IMAG` or `ANIM` ID, inspect the result,
mark it Working, Weird, Bad, or Favorite, name it, tag it, save it, and later
send it from React or RAVE.

## Observer-facing value

Good stock faces can be found once and reused quickly during social or festival
moments.

## Final-goal contribution

This turns protocol ID scanning into a small reusable face archive and starts
the future MaskPack metadata path for app-owned creative content.

## Capability claims

- Built-in archive stores metadata only: type, ID, hex ID, name, tags, notes,
  status, favorite flag, last tested timestamp, and last send status.
- React and RAVE favorite built-ins send `IMAG`/`ANIM` command IDs only.
- MaskPack models and docs describe future custom/generated content but do not
  upload, play, or generate frames.
- Custom animation playback remains Experimental and Needs real-mask test.

## User-visible improvement

The Built-ins page can now save, favorite, and reload tested IDs. React and RAVE
can show saved favorite built-ins when archive records exist.

## Current evidence

- Repo files: `src/MaskApp.Core/Features/BuiltIns`, `src/MaskApp.App/Features/BuiltIns`, React/RAVE view models and pages.
- Java evidence: stock command behavior is represented through the protocol reference rather than new Java porting in this slice.
- Existing tests: built-in scanner, quick actions, React, RAVE, and command builder tests.
- Existing validation gaps: physical mask behavior for useful built-in IDs is still untested.

## Scope

In scope:

- Built-in archive domain model.
- JSON-backed local metadata persistence.
- Built-ins page archive editing controls.
- Favorite built-ins in React and RAVE.
- MaskPack manifest models, parser, validation tests, and docs.

Out of scope:

- Raw built-in frame extraction.
- Image upload, DIY `PLAY`, `DELE`, `CHEC` product UI.
- Video playback, Drop Detector, Voice Mouth, real-time animation.
- AI generation or OpenAI/API integration.
- Firmware or custom firmware work.

## Files and flows

- Core: Built-in archive models/store contracts, favorite action models, MaskPack models/parser.
- App UI: Built-ins metadata editor/list, React favorite built-ins, RAVE favorite built-ins.
- Platform adapters: no BLE/platform adapter changes.
- Docs: MaskPack format, AI import guidance, progress tracker, this slice record.

## Test plan

- Unit tests: archive range/default/update behavior and MaskPack parser/validator.
- Build validation: core tests, iOS build, Android build.
- Browser/simulator/device validation: no browser validation; MAUI physical mask testing remains manual.
- Skipped validation and reason: physical iPhone/mask validation cannot be performed from this environment.

## Deferred validation

- Connect to the physical mask on iPhone.
- Scan `IMAG`/`ANIM` IDs and record which IDs are useful.
- Reopen the app and confirm the JSON archive persists.
- Send favorite built-ins from React and RAVE.

## Overclaim check

- No Vision or Experimental item is presented as proven playback capability.
- Firmware/custom firmware is excluded.
- Real-mask validation gaps remain named in `docs/progress.md`.
- Apple Watch remains backlog only.

## Measured outcome

Record after implementation:

- Changes made: archive metadata, JSON persistence, React/RAVE favorite built-ins, MaskPack docs/models/tests.
- Commands run: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj`; `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios`; `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android`; `git diff --check`; Roslyn diagnostics.
- Result: 86 core tests passed; iOS and Android target builds passed with 0 warnings and 0 errors; whitespace check passed; Roslyn diagnostics reported 0 warnings and 0 errors.
- Remaining risk: physical mask behavior, useful built-in IDs, and mobile app JSON persistence need iPhone/device confirmation.

## Next slice candidate

- Real-mask built-in ID scan and archive validation on iPhone.
