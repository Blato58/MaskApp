# MaskPack Import Format

Last updated: 2026-06-27

MaskPack is the planned archive format for future app-owned or generated mask
artwork. This slice defines metadata and validation only. It does not implement
custom image upload, DIY slot playback, video playback, frame extraction from
stock built-ins, AI generation, or festival-time animation playback.

Built-in mask faces remain stock firmware content addressed by `IMAG` and
`ANIM` command IDs. The app can save names, tags, notes, status, favorites, and
send history for those IDs, but it cannot extract raw built-in frames from the
mask.

## Package Shape

Future packages use a zip file with a `.maskpack.zip` extension:

```text
my-faces.maskpack.zip
  manifest.json
  preview.png
  frames/frame-000.png
  frames/frame-001.png
```

`manifest.json` is the source of truth for import metadata.

## Manifest Example

```json
{
  "schemaVersion": 1,
  "packName": "Festival Faces",
  "author": "MaskApp",
  "source": "manual",
  "targetDisplay": {
    "width": 44,
    "height": 58
  },
  "assets": [
    {
      "id": "smile",
      "type": "staticImage",
      "name": "Smile",
      "tags": [ "happy", "safe" ],
      "notes": "One-frame high-contrast face.",
      "frames": [
        { "path": "frames/frame-000.png" }
      ],
      "frameDurationMs": 250,
      "loop": false,
      "generatedBy": "",
      "sourcePrompt": "",
      "safetyNotes": "Readable at distance."
    }
  ]
}
```

## Fields

| Field | Meaning |
| --- | --- |
| `schemaVersion` | Manifest schema version. Current version is `1`. |
| `packName` | Human-readable pack name. |
| `author` | Creator or source name. |
| `source` | Manual, imported, generated, or other provenance. |
| `targetDisplay.width` | Must be `44`. |
| `targetDisplay.height` | Must be `58`. |
| `assets[].id` | Stable asset id inside the pack. |
| `assets[].type` | `staticImage` or `animation`. |
| `assets[].name` | Human-readable asset name. |
| `assets[].tags` | Search and grouping tags. |
| `assets[].notes` | User notes or import notes. |
| `assets[].frames` | Relative frame paths inside the zip. |
| `assets[].frameDurationMs` | Positive frame duration for preview/future playback. |
| `assets[].loop` | Whether preview/future playback should loop. |
| `assets[].generatedBy` | Optional generator/tool name. |
| `assets[].sourcePrompt` | Optional source prompt for generated assets. |
| `assets[].safetyNotes` | Readability and safety notes. |

## Validation Rules

- Width must be `44`.
- Height must be `58`.
- Each asset must include an id, name, and at least one frame.
- Frame duration must be positive.
- One-frame assets are valid static images.
- Frame counts above 24 warn that stock-firmware playback is unverified.
- Frame counts above 120 are rejected by the safe metadata validator.
- Upload and playback remain Labs until physical mask validation proves the
  image upload, DIY slot IDs, and `PLAY` timing.

## Capability Status

MaskPack is metadata/model support in this slice. Importing a package, previewing
frames, uploading frames, assigning DIY slots, and playing animations are future
work. The app must keep custom animation playback Experimental until validated
on a real mask.
