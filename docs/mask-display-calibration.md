# Mask Display Calibration

Last updated: 2026-07-17

## Purpose

This document maps MaskApp's logical 46x58 DIY image canvas to the LEDs that are
actually visible on the physical mask. The logical calibration pattern is
implemented; the physical map remains pending until a straight-on photograph
of the active test face is analyzed.

Use this map when placing eyes, borders, and other alignment-sensitive details
in new faces. Do not infer the final visible bounds from the mask's plastic
outline or from an unregistered photograph.

## Logical Coordinate System

- Canvas size: 46 columns by 58 rows.
- Coordinates are zero-based `(x, y)`.
- Logical origin: `(0, 0)` at the upper-left of the app preview.
- Logical range: `x = 0..45`, `y = 0..57`.
- Physical left/right orientation, hidden pixels, and eye cutouts remain to be
  confirmed from the calibration photograph.

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

Status: **Awaiting calibration photograph.**

The following facts must be filled from the registered photograph before they
become design constraints:

- Physical orientation of logical rows and columns.
- First and last visible logical pixel on each row.
- Left eye opening: exact hidden/clipped coordinates.
- Right eye opening: exact hidden/clipped coordinates.
- Other permanently hidden, clipped, dead, or unusually dim pixels.
- Any nonlinear offset caused by the mask's curvature.

Once populated, this section is the source of truth for alignment-sensitive
face artwork. Keep the raw photo-derived observations separate from artistic
padding so future faces can deliberately choose their own margins.
