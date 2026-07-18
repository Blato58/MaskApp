# MaskPack v2 Archive Format

Last updated: 2026-07-17

MaskPack is MaskApp's offline show-transfer format. The app can export a
`.maskpack.zip` archive, send it through the platform share sheet, Files, or
AirDrop, inspect an archive without extracting it, preview conflicts, and
transactionally import it. No server or account is involved.

MaskPack keeps two display geometries distinct:

- DIY faces and animation frames use the physical `46x58` art canvas.
- Text presets target the verified `44x58` text region used by the protocol.

Import/export does not prove how content looks or performs on a physical mask.
DIY timing, color fidelity, and text layout still require real-device checks.

## Archive Shape

A schema-v2 package contains one root manifest and only the payload files named
by that manifest:

```text
festival-show.maskpack.zip
  manifest.json
  content/face/000-opening-face.json
  content/animation/001-intro-loop.json
  content/textpreset/002-welcome.json
  content/page/003-main-page.json
  content/scene/004-opening.json
  content/setlist/005-main-set.json
  content/appearance/006-appearance.json
```

Directory entries, unlisted files, duplicate paths, and a second manifest are
not accepted. Import reads entries as bounded streams and never extracts the
archive to the file system.

## Schema-v2 Manifest

```json
{
  "schemaVersion": 2,
  "packName": "Festival Show",
  "author": "MaskApp",
  "source": "maskapp-export",
  "artDisplay": {
    "width": 46,
    "height": 58
  },
  "textDisplay": {
    "width": 44,
    "height": 58
  },
  "contents": [
    {
      "id": "opening-face",
      "type": "face",
      "name": "Opening Face",
      "path": "content/face/000-opening-face.json",
      "sha256": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
      "formatVersion": 1
    }
  ]
}
```

| Field | Contract |
| --- | --- |
| `schemaVersion` | `2` for current archives. Version `1` is accepted only through the migration path below. |
| `packName` | Required display name, at most 160 characters. |
| `author` | Optional creator/source name, at most 160 characters. |
| `source` | Optional provenance, at most 240 characters. MaskApp exports use `maskapp-export`. |
| `artDisplay` | Exactly `46x58`. |
| `textDisplay` | Exactly `44x58`. |
| `contents` | Between 1 and 256 typed entries. A type/id pair and an archive path must be unique. |
| `contents[].id` | Stable, case-sensitive content id, at most 128 characters. |
| `contents[].type` | `face`, `animation`, `textPreset`, `page`, `scene`, `setlist`, or `appearance`. |
| `contents[].name` | Required display name, at most 160 characters. |
| `contents[].path` | Relative, slash-separated payload path, at most 240 characters. |
| `contents[].sha256` | Lower- or upper-case 64-digit SHA-256 digest of the exact payload bytes. |
| `contents[].formatVersion` | Currently `1` for every typed payload. |

The manifest type/id must match the stable id inside the decoded payload. An
`appearance` archive can occur at most once and uses the stable id
`appearance` in MaskApp exports.

## Typed Payloads

All v2 payloads are UTF-8 JSON. Enum values use camel-case names. The current
payload encoding version is `1`.

| Type | Included data | Deliberately excluded state |
| --- | --- | --- |
| `face` | Stable id, display name, emotion, preferred DIY slot, favorite flag, and row-major 46x58 pixels. Pixels are base64-packed as four bytes per cell: lit flag, red, green, blue. | Upload time/status, active-mask slot installation, and built-in artwork. Imported faces are user content. |
| `animation` | Stable project/frame ids, name, favorite flag, loop mode/count, optional BPM, per-frame duration, and packed 46x58 pixels. | Prepared-slot fingerprints, playback position, runtime metrics, and flash-safety acknowledgement. |
| `textPreset` | Caption and mask-safe text, name, pack/tags, visibility, color, layout, bold, display mode, speed, send profile, reset preference, and notes. | Seed ownership, last-send time, and last-send result. |
| `page` | Stable Page/shortcut ids, labels, icons, colors, ordering, and Library item references. | Fast prepared slot, content fingerprint, and prepared timestamp, because those belong to one physical mask profile. |
| `scene` | Stable Scene/step ids, typed bounded steps, references, waits/repeats, failure policy, color, and favorite flag. | Created/updated timestamps and live execution state. |
| `setlist` | Stable setlist/cue ids, labels, ordering, and Scene references. | Created/updated timestamps, current cue, and active-setlist selection. Import never activates a show automatically. |
| `appearance` | Safe Library item/group ordering only. | Device, connection, permission, theme, signing, secret, and mask-profile settings. |

Package semantics are validated as a whole. Page and Scene references must
resolve to package content, existing local content, or an exact known built-in,
app-animation, or quick-action id. Setlist cues must resolve to a package or
existing Scene. Animation projects must compile within the production DIY-slot
budget.

Current semantic limits include 50 faces, 50 animations, 150 text presets, 50
Pages, 50 Scenes, and 20 setlists per package. A Page can contain at most 64
shortcuts, an animation at most 120 source frames, a Scene at most 32 source
steps/128 expanded steps, and a setlist at most 64 cues. The domain validators
apply their tighter timing, repeat, text, and unique-slot rules as well.

## Archive Safety Limits

Inspection fails before import when any of these limits or rules is violated:

- compressed archive: 32 MiB maximum;
- all uncompressed files: 64 MiB maximum;
- one file: 8 MiB maximum;
- `manifest.json`: 1 MiB maximum;
- files in the ZIP: 300 maximum, including the manifest;
- entries larger than 1 MiB: compression ratio no greater than 1000:1;
- no absolute paths, drive/URI separators, backslashes, empty segments, `.` or
  `..` segments, directory entries, or case-insensitive duplicate paths;
- every v2 payload must be listed exactly once and match its SHA-256 digest;
- malformed JSON, unknown enum/type/version values, missing ids, null required
  collections, excessive strings/counts, invalid pixels, and dangling
  references are rejected.

These limits are defense in depth. The importer never invokes content from an
archive and never writes archive-controlled paths.

## Conflict Preview and Import Semantics

Inspection compares canonical payload hashes with current local content and
shows every colliding type/id before import. Each conflict can use one action:

- `Merge`: exact matches keep the local copy. Different Pages add shortcuts
  that do not already exist; appearance ordering preserves existing entries
  and adds missing imported entries. Other different content is safely renamed.
- `Rename`: import under a unique `-imported` id and update package Page, Scene,
  and setlist references to that id.
- `Skip`: keep the local item and map package references to it.
- `Replace`: overwrite that local id only after explicit confirmation. A
  built-in face can never be replaced.

Import rechecks the local hashes immediately before applying the preview. If
content changed after inspection, the import stops and asks for a new preview.

Before the first store write, MaskApp persists a recovery journal containing
the original text, face, animation, Page/ordering, Scene, and setlist stores. A
mid-import failure restores every original store. If rollback itself cannot
finish, the journal is retained. App startup checks for a retained journal,
restores it, and clears it only after all stores succeed. Corrupt journals are
preserved for manual recovery rather than silently discarded.

## Schema-v1 Migration

MaskApp still accepts the legacy metadata-and-PNG contract:

- `schemaVersion` is `1`;
- `targetDisplay` is exactly `44x58`;
- assets are `staticImage` or `animation` with one to 120 PNG frame paths;
- frame durations are positive.

Legacy PNG frames must be bounded, non-interlaced, 44x58, 8-bit images using a
supported grayscale, RGB, indexed, grayscale-alpha, or RGBA color type. PNG
chunk ordering, sizes, filters, palette indexes, decompressed length, and CRCs
are validated. Migration adds one off column on the left and right to produce a
46x58 physical-art canvas. Static assets become user faces; animations retain
their per-frame/default durations and loop intent as native animation projects.

Version 1 has no payload hashes. Inspection therefore displays a warning, and
the migrated data is validated as native v2 content before the conflict preview.
Exports always use schema version 2.

## Physical Validation Status

Automated tests cover v2 round trips, maximum repeated-frame animation export,
v1 migration, content/reference remapping, all conflict policies, stale
previews, rollback/recovery, corrupt journals, and hostile ZIP/PNG inputs. They
do not establish physical-mask color, orientation, text layout, DIY-slot
persistence, animation cadence, or sustained performance. Record those results
in the physical-device evidence checklist in `PLANS.md`.
