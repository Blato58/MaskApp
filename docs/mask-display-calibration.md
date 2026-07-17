# Mask Display Calibration

Last updated: 2026-07-17

## Purpose

This document maps MaskApp's logical 46x58 DIY image canvas to the LEDs that are
actually visible on the physical mask. The map was registered from two
straight-on photographs of the active calibration face on 2026-07-17.

Use this map when placing eyes, borders, and other alignment-sensitive details
in new faces. Do not infer the final visible bounds from the mask's plastic
outline or from an unregistered photograph.

## Logical Coordinate System

- Canvas size: 46 columns by 58 rows.
- Coordinates are zero-based `(x, y)`.
- Logical origin: `(0, 0)` at the upper-left of the app preview.
- Logical range: `x = 0..45`, `y = 0..57`.
- In a front view of the mask, `x` increases from the viewer's left to right and
  `y` increases from top to bottom. The app preview is not mirrored.

## Test Face

Find `Mask Calibration · Color Anchors` in the Library and upload it as a static
DIY face. Its built-in id is `built-in-face-mask-calibration`.

The pattern assigns all 2668 logical positions. Most of the canvas uses a dim
gray `#404040` field so visible LEDs can be detected without washing out the
saturated landmarks. The nine anchor centers intentionally send black
`#000000`; those known dark points distinguish the center of each color ring.

### Orientation rails

| Logical edge | Color |
| --- | --- |
| Top, `y = 0` | Red `#FF0000` |
| Right, `x = 45`, excluding corners | Green `#00FF00` |
| Bottom, `y = 57` | Blue `#0000FF` |
| Left, `x = 0`, excluding corners | Yellow `#FFFF00` |

### Registration anchors

Each anchor is a 3x3 colored ring with a black center at the listed coordinate.
The center gives a precise logical point even when the photograph is curved or
slightly off-axis.

| Position | Center `(x, y)` | Ring color |
| --- | --- | --- |
| Upper-left | `(5, 5)` | Red `#FF0000` |
| Upper-center | `(23, 5)` | Green `#00FF00` |
| Upper-right | `(40, 5)` | Blue `#0000FF` |
| Middle-left | `(5, 29)` | Yellow `#FFFF00` |
| Center | `(23, 29)` | White `#FFFFFF` |
| Middle-right | `(40, 29)` | Cyan `#00FFFF` |
| Lower-left | `(5, 52)` | Orange `#FF8000` |
| Lower-center | `(23, 52)` | Magenta `#FF00FF` |
| Lower-right | `(40, 52)` | Lime `#80FF00` |

### Eye-region ruler

The eye ruler spans logical rows `11..23`. White vertical ticks are located at
`x = 5, 10, 15, 20, 25, 30, 35, 40`; the magenta center tick is at `x = 23`.
Colored horizontal rails identify these rows:

| Row | Color |
| --- | --- |
| `y = 12` | Red `#FF0000` |
| `y = 14` | Orange `#FF8000` |
| `y = 16` | Yellow `#FFFF00` |
| `y = 18` | Green `#00FF00` |
| `y = 20` | Cyan `#00FFFF` |
| `y = 22` | Blue `#0000FF` |

Vertical ticks are drawn over the horizontal rails. Missing or clipped tick
segments in the photograph therefore identify the affected logical columns;
the rail color identifies the corresponding row.

## Capture Guidance

1. Display the calibration face at a steady, moderate brightness.
2. Photograph the whole mask straight-on at the highest practical resolution.
3. Keep the camera level and include the complete outer mask frame.
4. Avoid exposure that turns the dim gray field fully white or blooms adjacent
   LEDs together.
5. If possible, also capture one slightly darker exposure when the anchor colors
   are clearer than in the main photograph.

## Physical Visibility Map

Status: **Physically mapped from `IMG_9180.JPEG` and `IMG_9181.JPEG`.**

The two 4344x5792 captures use slightly different exposure and camera position.
Registering both against the calibration rails produced the same 58 logical
rows, orientation, eye openings, and row envelope. The original photographs
remain outside the repository; this coordinate map is the durable result.

The physical mask exposes 2062 positions inside its outer LED envelope. Of
those, 79 positions are behind the eye apertures, leaving 1983 usable LEDs. The
remaining 606 positions on the rectangular 46x58 canvas are outside the
physical display.

### Outer LED envelope

The bounds below are inclusive. A coordinate inside the bounds is usable unless
it is listed in the eye-aperture table. Rows with the same bounds are grouped.

| Logical row `y` | First visible `x` | Last visible `x` |
| --- | ---: | ---: |
| `0` | 18 | 27 |
| `1` | 13 | 32 |
| `2` | 11 | 34 |
| `3` | 9 | 36 |
| `4` | 8 | 37 |
| `5` | 7 | 38 |
| `6..7` | 6 | 39 |
| `8` | 5 | 40 |
| `9..10` | 4 | 41 |
| `11..13` | 3 | 42 |
| `14..15` | 2 | 43 |
| `16` | 2 | 44 |
| `17` | 1 | 43 |
| `18..33` | 1 | 44 |
| `34..37` | 2 | 43 |
| `38..39` | 3 | 42 |
| `40..42` | 4 | 41 |
| `43..44` | 5 | 40 |
| `45..46` | 6 | 39 |
| `47` | 7 | 38 |
| `48` | 8 | 37 |
| `49..50` | 9 | 36 |
| `51` | 10 | 35 |
| `52` | 11 | 34 |
| `53` | 12 | 33 |
| `54` | 13 | 32 |
| `55` | 15 | 30 |
| `56` | 17 | 28 |
| `57` | 21 | 24 |

### Eye apertures

The physical eye openings cover exactly rows `16..19`. These logical positions
are hidden even when the artwork assigns a lit color:

| Row `y` | Viewer-left eye: hidden `x` | Viewer-right eye: hidden `x` |
| --- | --- | --- |
| `16` | `5..15` | `30..40` |
| `17` | `6..17` | `28..38` |
| `18` | `7..17` | `28..38` |
| `19` | `9..14` | `31..36` |

Rows `15` and `20` are continuous across the face. Artwork intended to meet the
eye openings should therefore use all four aperture rows rather than assuming a
rectangular or three-row cutout.

### Complete coordinate mask

Each character represents one logical pixel. The leftmost character is `x = 0`;
the row label is `y`. `#` is a visible/usable LED, `E` is hidden behind an eye
opening, and `.` is outside the physical LED envelope.

```text
00 ..................##########..................
01 .............####################.............
02 ...........########################...........
03 .........############################.........
04 ........##############################........
05 .......################################.......
06 ......##################################......
07 ......##################################......
08 .....####################################.....
09 ....######################################....
10 ....######################################....
11 ...########################################...
12 ...########################################...
13 ...########################################...
14 ..##########################################..
15 ..##########################################..
16 ..###EEEEEEEEEEE##############EEEEEEEEEEE####.
17 .#####EEEEEEEEEEEE##########EEEEEEEEEEE#####..
18 .######EEEEEEEEEEE##########EEEEEEEEEEE######.
19 .########EEEEEE################EEEEEE########.
20 .############################################.
21 .############################################.
22 .############################################.
23 .############################################.
24 .############################################.
25 .############################################.
26 .############################################.
27 .############################################.
28 .############################################.
29 .############################################.
30 .############################################.
31 .############################################.
32 .############################################.
33 .############################################.
34 ..##########################################..
35 ..##########################################..
36 ..##########################################..
37 ..##########################################..
38 ...########################################...
39 ...########################################...
40 ....######################################....
41 ....######################################....
42 ....######################################....
43 .....####################################.....
44 .....####################################.....
45 ......##################################......
46 ......##################################......
47 .......################################.......
48 ........##############################........
49 .........############################.........
50 .........############################.........
51 ..........##########################..........
52 ...........########################...........
53 ............######################............
54 .............####################.............
55 ...............################...............
56 .................############.................
57 .....................####.....................
```

### Interpretation and design rules

- Filling the entire 46x58 canvas is safe. Pixels marked `.` or `E` simply do
  not contribute visible light on this mask.
- Use the exact `E` contour for eye-aligned artwork. In particular, do not place
  critical eye or cross edges only on rows `16..18`; row `19` is also cut out.
- Keep small focal details at least one pixel inside the outer envelope. The
  first or last LED on a curved edge can appear dimmer or more occluded as the
  viewing angle changes.
- The five unlit calibration centers inside the envelope -- `(23, 5)`,
  `(5, 29)`, `(23, 29)`, `(40, 29)`, and `(23, 52)` -- were intentional black
  test pixels. Their one-pitch gaps and surrounding rings confirm that these
  are usable positions, not missing LEDs.
- No additional permanent holes or dead-pixel regions were reproducible across
  both photographs. Apparent edge brightness differences were treated as
  curvature and exposure effects, not as missing logical positions.
