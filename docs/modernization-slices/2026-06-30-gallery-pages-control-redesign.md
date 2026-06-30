# Gallery, Pages, And Slim Control Redesign

## Slice

- Name: Gallery, Pages, and slim Control redesign
- Date: 2026-06-30
- Status: validated
- Owner: Codex
- Product pillar: Instant Reactions, Creative Composition, Reliability As A Feature
- Capability confidence: Implemented
- Physical validation status: Needs real-mask test

## Intent

The app had separate React, RAVE, Faces, and Connect tabs, which made sendable
content feel scattered and pushed connection controls into a utility page. This
slice turns text presets, quick actions, and built-in face/animation records
into one Gallery and adds a page organizer for live-use decks.

## Target user moment

The wearer opens Gallery, searches or groups existing text, face, and animation
items, sends one, manages it, or places it onto a custom page of shortcuts. The
Control tab stays focused on scan/connect/disconnect/auto-connect and
brightness recovery.

## Observer-facing value

The wearer can trigger recognizable captions, faces, and animations faster,
with fewer mode decisions while wearing the mask.

## Final-goal contribution

This moves MaskApp toward the wearable face controller shape: content lives in a
single reusable library and live pages behave like prepared reaction decks,
while BLE recovery stays obvious and separate.

## Capability claims

- Text preset sends use the existing text preset dispatcher and remain
  implemented but still need repeated physical iOS validation.
- Built-in faces and animations remain command-only IMAG/ANIM metadata and
  still need real-mask confirmation.
- Custom images, custom animations, and MaskPack playback are shown only as
  unavailable future/Labs add options.
- No firmware or custom firmware behavior was added.

## User-visible improvement

- Root tabs are now Gallery, Pages, and Control.
- Gallery supports search, favorites filtering, grouping by manual group,
  favorites, type, pack/group, and recently sent state.
- Gallery item and group order can be changed while filtered/grouped through
  explicit move controls.
- Pages provides multiple color-coded shortcut pages with icon/color cycling,
  add/remove, reorder, and send support.
- Control contains only connection, foreground auto-connect, remembered mask,
  brightness, BLACKOUT, and restore controls.

## Current evidence

- Repo files: `src/MaskApp.Core/Features/Gallery`, `src/MaskApp.App/Features/Gallery`, `src/MaskApp.App/AppShell.xaml.cs`
- Java evidence: not used; this is an app information-architecture slice.
- Existing tests: text preset, quick action, built-in archive, connect, and mask-control tests.
- Existing validation gaps: physical iOS/Android mask validation for sends, scan/connect, auto-connect, and brightness remains open.

## Scope

In scope:

- Shared gallery projection, ordering, page layout, storage contract, and send routing.
- MAUI Gallery and Pages tabs.
- Slim Control tab.
- Hidden manage routes for Text Composer and built-in scanner/archive.
- Focused unit tests and progress docs.

Out of scope:

- Custom image upload, custom animation upload, DIY playback, import/export UI,
  and native drag/drop.
- Real-device BLE or mask display validation.

## Files and flows

- Core: gallery item projection, grouping, ordering, page layout, Pages and Gallery view models, explicit brightness blackout/restore commands.
- App UI: Gallery page, Pages page, slim Control page, shell tab order, gallery layout JSON store.
- Platform adapters: unchanged.
- Docs: progress entry and this slice record.

## Test plan

- Unit tests: gallery projection, search/grouping, filtered reorder, page shortcut add/remove/reorder/icon/color behavior, send routing, blackout/restore.
- Build validation: core tests, iOS target build, Android target build.
- Browser/simulator/device validation: not performed; this is a MAUI app and physical mask checks require hardware.
- Skipped validation and reason: real iPhone/mask and Android/mask checks were not available in this environment.

## Deferred validation

- Open the app on iPhone and confirm root tabs render as Gallery, Pages, Control.
- Send a text preset, quick action, built-in face, and built-in animation from Gallery and Pages.
- Verify scan/connect/disconnect, foreground-only auto-connect, brightness,
  BLACKOUT, and restore on real mask hardware.

## Overclaim check

- Future image/animation/import options are disabled and labeled future/Labs.
- Built-in IMAG/ANIM content still says it needs real-mask confirmation.
- Firmware and custom firmware remain out of scope.
- No Apple Watch behavior was added.

## Measured outcome

- Changes made: unified Gallery, page organizer, slim Control tab, JSON layout storage, tests, docs.
- Commands run: `dotnet test tests\MaskApp.Core.Tests\MaskApp.Core.Tests.csproj --no-restore`, `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-ios --no-restore`, `dotnet build src\MaskApp.App\MaskApp.App.csproj -f net10.0-android --no-restore`, `git diff --check`.
- Result: 191 core tests passed; iOS and Android target builds passed with 0 warnings and 0 errors.
- Remaining risk: physical BLE/mask behavior and rendered mobile layout need device validation.

## Next slice candidate

- Physical iOS validation of Gallery and Pages sends, then refine item/card
  density based on real phone ergonomics.
