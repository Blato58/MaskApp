# MaskApp Product Vision

Last updated: 2026-06-27

MaskApp should be a wearable face controller. It is not just a BLE packet tool
or settings panel.

The intended user moment is:

> Open the app, pick a mood, reaction, caption, image, beat mode, or prepared
> pack, preview it, and make the mask become an expressive face within seconds.

The app should feel like a fast reaction deck, meme generator, pixel-art editor,
party remote, audio visualizer, and performance tool. The best version is useful
to the wearer and funny or memorable to people looking at the mask.

Protocol capability claims should be checked against
`docs/stock-mask-protocol.md` before BLE, Text, Image, Rhythm, or RAVE work. That
document summarizes community reverse-engineered stock-firmware evidence; it is
not manufacturer documentation.

## Product Promise

- Emotional promise: the wearer can react to the room without speaking.
- Practical promise: the mask shows what the user expects, and the app explains
  what happened when it does not.
- Fun promise: observers can understand the joke, reaction, or visual moment
  quickly.

## Product Pillars

### Instant Reactions

The fastest path in the app should be one-tap reactions: short captions,
built-in looks, blackout, last look, and random/favorite reactions.

Examples: `LOL`, `NOPE`, `SUS`, `BRUH`, `VIBE CHECK`, `BUFFERING`,
`SEND HELP`, `MAIN CHARACTER MODE`.

### Creative Composition

The app should hide protocol details behind creation tools: text composer,
image cropper, pixel preview, reaction builder, rhythm designer, preset library,
and later AI-assisted caption or pack generation.

### Performance Modes

The mask is worn around people, in motion, in dark spaces, at conventions,
parties, festivals, Halloween events, videos, and stage-like settings. The app
should support semi-automatic or prepared modes such as mood loops, random
reaction queues, rhythm modes, observer games, and party packs.

### RAVE / DnB Festival Mode

RAVE Mode is a dedicated product pillar. It should make the mask feel like a
wearable visualizer, hype board, and crowd-reaction machine for dark, loud, and
chaotic environments.

RAVE Mode must be manual-first and offline-first until real-mask validation
proves more advanced live behavior. The festival experience should not depend on
internet, AI, long uploads, or perfect audio detection.

### Reliability As A Feature

The fun fails if the mask fails silently. Every send path should answer:

- Did the app send it?
- Did the mask ACK it, where ACK is supported?
- What is likely showing now?
- Is this built-in, uploaded, live, or app-only?
- Can the user recover quickly?

## Capability Confidence Model

Use these labels whenever a roadmap item or slice describes mask capability.

| Confidence | Meaning |
| --- | --- |
| Vision | Desired product behavior; not yet tied to known protocol or implementation. |
| Protocol-documented | Documented by source, reverse-engineering notes, or Java evidence, but not necessarily implemented here. |
| Implemented | Implemented in this repo and compile/test validated where possible. |
| Physically verified | Tested on a real mask and recorded in `docs/progress.md` or a slice record. |
| Experimental | Plausible app-layer composition or lab feature, but physical behavior, timing, reliability, or UX is not proven. |
| Out of scope | Not part of this product-planning slice or not planned for the stock-firmware product. |

Do not describe Experimental items as guaranteed product capability. Phrase them
as Labs features until a real-mask test proves the behavior.

## Current Capability Map

| Capability | Confidence | Notes |
| --- | --- | --- |
| BLE scan/connect app flow | Implemented | iOS and Android compile; physical validation remains open. |
| Brightness, dim/safe-off, built-in image command, built-in animation command | Implemented | Command builders and UI exist; physical command validation remains open. |
| Text upload with ACK mode and write-only compatibility | Implemented | Core tests and platform builds pass; physical text upload validation remains open. |
| Built-in static image and animation galleries | Protocol-documented | Planned scanner/favorites feature; not yet implemented as a product flow. |
| Custom RGB image upload and DIY playback | Protocol-documented | Stock protocol reference documents upload and `PLAY`; must be mapped and tested before product claims. |
| Audio visualizer protocol | Protocol-documented | Stock protocol reference documents encrypted visualizer packets; Rhythm/RAVE plans can target it, but physical behavior is not yet proven here. |
| Reaction Deck, Mask Packs, Party Director, observer games | Vision | App-layer product compositions that should use proven lower-level operations. |
| Drop Detector | Experimental | Keep in Labs until audio detection and resulting mask behavior are proven on a real mask. |
| Voice Mouth | Experimental | Keep in Labs until microphone, visualizer, DIY playback, and timing are proven on a real mask. |
| Bass Face | Experimental | Keep in Labs until visualizer and image/DIY composition reliability are proven. |
| GIF-ish playback | Experimental | Treat as app-layer frame/DIY sequencing until real upload/playback timing is proven. |
| Fast DIY sequencing | Experimental | Do not promise until real-mask slot playback timing and reliability are verified. |
| Real-time effects | Experimental | Avoid overclaiming frame rate or latency before physical tests. |
| Apple Watch Quick Deck and Mode Switcher | Vision | Future companion remote only; no watchOS implementation in the current roadmap slice. |
| Watch microphone input | Experimental | Possible Labs input for music-wave detection, voice commands, or AI dictation; iPhone microphone remains preferred for RAVE MVP. |
| Firmware changes and custom firmware | Out of scope | Stock-firmware app planning only. |

## Product Structure

The app should not be organized primarily around `Connect / Text / Settings`.
That feels like a utility. The target structure is:

- Home / Control Room: connection, current look, brightness, blackout,
  reconnect, recent reactions, random reaction, last look, and resume mode.
- React: one-tap reaction cards grouped by use case.
- Create: text, image, pixel art, meme, rhythm, and later AI caption tools with
  real mask previews.
- Party / RAVE: manual-first live controls, prepared packs, visualizer entry
  points, and safety controls.
- Library: saved reactions, text presets, built-in favorites, image presets,
  rhythm presets, party packs, imports, and exports.
- Settings / Diagnostics: compatibility mode, BLE diagnostics, permissions,
  export logs, reset state, and advanced protocol lab.

## Festival-Ready RAVE MVP

The first RAVE implementation must be reliable in a crowd before it is clever.
It is manual-first, offline-first, and built from short, proven operations.

RAVE MVP includes:

- Big DnB reaction buttons.
- Always-visible blackout.
- Brightness cap.
- Reconnect/resume.
- Short offline captions.
- Haptic/send feedback.
- Low-bandwidth mode.
- Avoiding long uploads during the event.

RAVE MVP should prefer instant commands, built-in looks, short text, preloaded
DIY slots only after validation, and live visualizer control only after basic
physical testing. Automatic Drop Detector, Voice Mouth, and Bass Face stay in
Labs/Experimental until visualizer and DIY playback reliability are proven on a
real mask.

For RAVE FAST implementation guidance, use `docs/stock-mask-protocol.md`:
instant encrypted commands first, short text after physical validation,
preloaded DIY slots only after slot playback is proven, and audio visualization
only in Labs until deterministic real-mask tests pass.

## Future Apple Watch Companion

Apple Watch support is a future backlog item and must not delay Text validation,
Control Room, Reaction Deck, RAVE MVP, presets, image, or rhythm work.

The Watch is a companion remote only:

- iPhone remains the BLE controller, upload engine, preset manager, AI provider,
  and reliability layer.
- Watch sends high-level intents to iPhone.
- Watch must not directly control the mask over BLE.
- Standalone Watch operation is out of scope.

Future Watch core use:

- Quick Deck: a small deck of favorite actions such as `DROP`, `WHEEL UP`,
  `RELOAD`, `BASS FACE`, `HYDRATE`, `VIBE CHECK`, and `BLACKOUT`.
- Deck families: RAVE, Social, Meme, Safety/Welfare, Party, and Favorites.
- Mode Switcher: wrist switching for RAVE, React, Party Loop, Visualizer, Voice
  Mouth, Quiet/Dim, and Blackout.
- Mode switching updates the active iPhone mode/deck and runs required app-side
  start/stop behavior.

Design implication for the phone app:

- Quick actions and modes should eventually be represented as stable intent IDs,
  not only button click handlers.
- Future shared commands should map to `TriggerQuickAction(actionId)` and
  `SwitchMode(modeId)` at the app layer.
- The iPhone RAVE UI, Reaction Deck, Party Director, and future Watch remote
  should share the same command model.

Watch microphone remains Labs/Experimental. Possible future uses include
auxiliary music-wave input, voice commands, and AI prompt dictation. iPhone
microphone remains preferred for RAVE MVP.

Out of scope:

- Direct Watch-to-mask BLE control.
- Watch-based text or image editing.
- Watch-based firmware, custom firmware, or diagnostics.
- Standalone Watch operation.

## Next Product Priorities

1. Text validation/fix on a physical mask.
2. Control Room + Reaction Deck MVP.
3. RAVE MVP entry point.
4. Built-in Gallery Scanner.
5. Preset Library and Mask Packs.
6. Image Studio and DIY slot management.
7. Rhythm, Voice, and RAVE Labs.
8. AI-assisted caption and pack generation.
9. Apple Watch Quick Deck and Mode Switcher.
