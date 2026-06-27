# AI Animation Import Guidance

Last updated: 2026-06-27

This document describes a future workflow only. This slice adds no OpenAI/API
code, no internet requirement, no generator UI, and no custom animation upload or
playback.

## Intended Future Flow

1. The user provides a photo, video, sketch, or prompt to an external generator.
2. The generator produces a `.maskpack.zip` package with `manifest.json`, frame
   PNG files, and an optional preview image.
3. MaskApp imports and validates the manifest and frames.
4. MaskApp previews the frames locally.
5. After physical validation, MaskApp may upload a static frame or use DIY slots.

## Generator Constraints

- Output must target a `44x58` display.
- Prefer high contrast and simple facial shapes.
- Avoid tiny details that will disappear on the LED grid.
- Avoid long text; short captions belong in the Text slice.
- Keep frame counts small.
- Pre-generate packs before a festival.
- Do not require internet at the event.
- Keep safety and readability notes in the MaskPack manifest.

## Product Boundary

AI-assisted generation belongs to future creative tooling. It must not be
presented as a proven mask capability until the app can import, preview, upload,
and physically validate the resulting frames on stock firmware.
