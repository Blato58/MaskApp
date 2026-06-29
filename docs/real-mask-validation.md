# Real-Mask Validation Checklist

Last updated: 2026-06-30

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
4. Confirm Quick Caption Send mode defaults to Low-static Flash.
5. Scan finds mask.
6. Connect succeeds.
7. Disconnection/reconnect path is understandable.
8. BLACKOUT sends `LIGHT 1`.
9. Brightness cap sends expected brightness.
10. Send `LOL` from React.
11. Send `DROP` from RAVE.
12. Confirm quick captions do not slow right-to-left scroll by default.
13. Text short caption `LOL` sends in ACK mode if notifications work.
14. Text short caption sends in write-only mode if ACK is missing.
15. React page sends `NOPE`, `LOL`, and `SUS`.
16. RAVE page sends `DROP`, `WHEEL UP`, and `HYDRATE`.
17. RAVE command fallbacks work.
18. RAVE keeps BLACKOUT, Connect, Text Composer, brightness, Favorite Faces,
    and fallbacks visible without Festival Lock.
19. Reconnect does not break RAVE UI.
20. `IMAG 1` changes the mask.
21. `ANIM 1` changes the mask.
22. Write down which built-in image/animation IDs are good.

## Festival Live Polish Checklist

1. Manual connect to the mask.
2. Confirm the mask appears as the last known mask in Control and Connect.
3. Close and reopen the app.
4. Confirm auto-connect searches for the remembered mask while the app is open.
5. Confirm auto-connect connects when the mask is nearby.
6. Disable auto-connect and confirm the app does not auto-connect.
7. Re-enable auto-connect and confirm Connect now starts foreground search.
8. Forget mask and confirm remembered mask is cleared.
9. Set global quick-caption text color to Cyan.
10. Send `LOL` from React and confirm Cyan appears.
11. Set global quick-caption text color to Pink.
12. Send `DROP` from RAVE and confirm Pink appears.
13. Open Text Composer and confirm the selected/default color follows the
    global setting.
14. Manually choose a different Text Composer color and confirm it is used for
    that send.
15. Send BLACKOUT from RAVE body controls.
16. Send BLACKOUT from the RAVE sticky footer.
17. Send a RAVE Favorite Face.
18. Confirm Connect page manual scan still works after forget/reconnect.

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
bad, so colored text backgrounds should stay disabled. Follow-up feedback showed
the remaining issue is not cold start: every quick-caption send had about 300 ms
of static display before Blink began. Low-static Flash now avoids the per-send
display reset/black `BC` delay, pre-arms `SPEED 50` and `MODE 2`, and sends
post-upload `MODE 2` immediately.

1. Open app and connect.
2. Select or confirm Low-static Flash profile.
3. Send `LOL` from React 10 times.
4. Confirm all 10 are centered.
5. Confirm Blink begins immediately or much faster than the old ~300 ms static
   pre-roll.
6. Send `DROP` from RAVE 10 times.
7. Confirm all 10 are centered.
8. Confirm Blink begins immediately or much faster than the old ~300 ms static
   pre-roll.
9. Compare Stable Flash fallback with `LOL` and `DROP`.
10. Confirm Stable Flash remains centered/blink if Low-static is less reliable.
11. Switch to Fast Flash unstable.
12. Repeat `LOL` and `DROP` 5 times.
13. Record if any send becomes solid or left-aligned.
14. Open Text Composer.
15. Choose Centered 44-column + Blink.
16. Send `TEST`.
17. Confirm centered/blink.
18. Choose Scroll right-to-left.
19. Send `TEST SCROLL`.
20. Confirm scrolling is intentional.
21. Send BLACKOUT.
22. Confirm BLACKOUT still works.
23. Send a built-in face fallback.
24. Confirm the built-in face fallback still works.
25. Record the best festival profile.

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
- Confirm RAVE shows Favorite Faces without a Festival Lock toggle.
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
| Low-static Flash repeated quick sends | Not tested | Confirm blink starts immediately or noticeably faster than the old ~300 ms static pre-roll. |
| Fast Flash repeated quick sends | Failed on real mask | Still produced left-aligned and solid text. Keep unstable. |
| Text Creator centered 44-column + Blink | Passed | Best observed profile at speed 50. |
| Text Creator Scroll right-to-left | Passed | Checklist otherwise passed. |
| Background `BC` fail-soft style | Needs retest | Colored background looked bad; recheck explicit black reset clears stale background state. |
| Text Composer fast typing | Not tested |  |
| RAVE command fallbacks | Not tested |  |
| Lock-free RAVE controls | Not tested | Confirm BLACKOUT, Connect, Text Composer, brightness, Favorite Faces, and fallbacks stay visible. |
| Auto-connect remembered mask | Not tested | Foreground/app-open only. |
| Auto-connect disabled | Not tested | Confirm disabled toggle prevents app-open auto-connect. |
| Forget remembered mask | Not tested | Confirm known mask clears in Control and Connect. |
| Global text color React | Not tested | Confirm selected color appears for `LOL`. |
| Global text color RAVE | Not tested | Confirm selected color appears for `DROP`. |
| Text Composer global default | Not tested | Confirm composer default follows global color and manual override still works. |
| Reconnect after RAVE | Not tested |  |
| Useful built-in IDs | Not tested |  |
