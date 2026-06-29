# Real-Mask Validation Checklist

Last updated: 2026-06-29

Use this checklist on iPhone with the physical Shining Mask before the festival.
Record useful built-in IDs and any failures in `docs/progress.md` or the current
slice record.

## Setup

- Use the iOS app build.
- Keep the mask charged and close to the phone.
- Start from a disconnected app state if possible.
- Treat every new command-only built-in as `Needs real-mask test` until it is
  seen on the physical mask.

## Checklist

1. Confirm Faces is visible as a root tab without using More.
2. Confirm Text Composer is reachable from Control, React, and RAVE.
3. Confirm Quick Caption Mode defaults to Flash / Blink.
4. Scan finds mask.
5. Connect succeeds.
6. Disconnection/reconnect path is understandable.
7. BLACKOUT sends `LIGHT 1`.
8. Brightness cap sends expected brightness.
9. Send `LOL` from React.
10. Send `DROP` from RAVE.
11. Confirm quick captions do not slow right-to-left scroll by default.
12. Text short caption `LOL` sends in ACK mode if notifications work.
13. Text short caption sends in write-only mode if ACK is missing.
14. React page sends `NOPE`, `LOL`, and `SUS`.
15. RAVE page sends `DROP`, `WHEEL UP`, and `HYDRATE`.
16. RAVE command fallbacks work.
17. Festival Lock keeps BLACKOUT available.
18. Reconnect does not break RAVE UI.
19. `IMAG 1` changes the mask.
20. `ANIM 1` changes the mask.
21. Write down which built-in image/animation IDs are good.

## P0 Text Crash/Freeze Hotfix Checklist

1. Open app.
2. Tap `LOL` from Control/Home.
3. Confirm app does not crash.
4. Tap `LOL` from React.
5. Confirm app does not crash.
6. Tap `DROP` from RAVE.
7. Confirm app does not crash.
8. Confirm quick caption blinks/flashes instead of slow right-to-left scroll.
9. Confirm text appears centered/fitted.
10. Open Text Composer.
11. Type quickly for 20-30 characters.
12. Confirm no freeze.
13. Send short text from Text Composer.
14. Compare Fast write-only vs ACK mode.
15. Record whether background color works if present; this hotfix does not add
    new FC/BC background commands.

## P0 Deterministic Text Profile Checklist

Physical feedback on 2026-06-29: Stable/centered Blink text passed except Fast
Flash, which still became left-aligned or solid. Text background styling looked
bad, so text backgrounds should stay black. The best observed profile was Text
Creator Centered 44-column + Blink at speed 50.

1. Open app and connect.
2. Select Stable Flash profile.
3. Send `LOL` from React 5 times.
4. Confirm all 5 are centered.
5. Confirm all 5 flash/blink.
6. Send `DROP` from RAVE 5 times.
7. Confirm all 5 are centered.
8. Confirm all 5 flash/blink.
9. Switch to Fast Flash.
10. Repeat `LOL` and `DROP` 5 times.
11. Record if any send becomes solid or left-aligned.
12. Open Text Creator.
13. Choose Centered 44-column + Blink.
14. Send `TEST`.
15. Confirm centered/blink.
16. Choose Scroll right-to-left.
17. Send `TEST SCROLL`.
18. Confirm scrolling is intentional.
19. Test background color if enabled.
20. Record the best festival profile.

## Built-In Scanner Sequence

Start with the safe IDs before exploring wider ranges:

- Scan `IMAG 0` through `IMAG 10`.
- Test `ANIM 0`, `ANIM 1`, `ANIM 2`, `ANIM 3`, `ANIM 4`, `ANIM 5`.
- Record useful IDs manually for now.
- Mark one `IMAG` ID as Works with one tap.
- Mark one `IMAG` ID as Favorite with one tap.
- Confirm the saved `IMAG` appears in Favorite Faces.
- Tap the deck card once and confirm it sends to the mask.
- Restart the app and confirm the favorite remains.
- Repeat the Works/Favorite/deck-send sequence for `ANIM`.
- Confirm RAVE shows Favorite Faces when Festival Lock is off.
- Confirm BLACKOUT remains visible on Faces and RAVE.
- Confirm the same favorite face sends from React.
- Confirm BLACKOUT remains visible after switching between Faces, React, RAVE,
  and Connect.

Protocol evidence suggests `IMAG` may be useful up to about `0x69` and `ANIM`
up to about `0x45`. Higher IDs should stay experimental until tested.

## Record Results

| Item | Result | Notes |
| --- | --- | --- |
| Scan finds mask | Not tested |  |
| Connect succeeds | Not tested |  |
| BLACKOUT / `LIGHT 1` | Not tested |  |
| Brightness cap | Not tested |  |
| `IMAG 1` | Not tested |  |
| `ANIM 1` | Not tested |  |
| Text `LOL` ACK mode | Not tested |  |
| Text `LOL` write-only mode | Not tested |  |
| React `NOPE` / `LOL` / `SUS` | Not tested |  |
| RAVE `DROP` / `WHEEL UP` / `HYDRATE` | Not tested |  |
| Quick caption Flash/Blink mode | Passed with Stable; failed with Fast | Fast Flash could become solid. |
| Quick caption centered/fitted layout | Passed with Stable; failed with Fast | Fast Flash could become left-aligned. |
| Stable Flash repeated quick sends | Passed | Checklist otherwise passed; revalidate speed 50 default after follow-up. |
| Fast Flash repeated quick sends | Failed on real mask | Still produced left-aligned and solid text. Keep unstable. |
| Text Creator centered 44-column + Blink | Passed | Best observed profile at speed 50. |
| Text Creator Scroll right-to-left | Passed | Checklist otherwise passed. |
| Background `BC` fail-soft style | Failed on real mask | Background looked bad; keep text background black. |
| Text Composer fast typing | Not tested |  |
| RAVE command fallbacks | Not tested |  |
| Festival Lock keeps BLACKOUT | Not tested |  |
| Reconnect after RAVE | Not tested |  |
| Useful built-in IDs | Not tested |  |
