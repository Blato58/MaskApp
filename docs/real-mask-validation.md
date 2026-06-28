# Real-Mask Validation Checklist

Last updated: 2026-06-27

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
| RAVE command fallbacks | Not tested |  |
| Festival Lock keeps BLACKOUT | Not tested |  |
| Reconnect after RAVE | Not tested |  |
| Useful built-in IDs | Not tested |  |
