# Built-in Preview Sources

## Purpose

MaskApp ships previews for the stock `IMAG` and `ANIM` catalogs so users can
choose content without testing numeric IDs blindly. These images are UI
previews recovered from the original Android app. They are not raw mask
framebuffer data and must not be presented as a guarantee of physical LED
output.

The project owner confirmed permission to redistribute the generated preview
derivatives on 2026-07-09.

## Source Mapping

The local, ignored reverse-engineering input is expected at:

```text
decompiled-app/app/src/main/res/values/arrays.xml
decompiled-app/app/src/main/base.apk/res/mipmap-xxhdpi-v4/
```

The generator reads resource names from `arrays.xml`; it does not infer frame
membership from filenames.

- Static faces use `image_default_new`, producing command IDs `0..69`.
- Static ID `63` therefore uses `image_new_default_63`.
- Animations use arrays `anim0`, `anim1`, `anim2`, `anim3`, and `anim5` through
  `anim45`. Command ID `4` is intentionally absent.
- Command ID `45` uses the `anim_46_new_image_*` resources referenced by
  `anim45`.
- Every source preview is `52x64` pixels.
- Animation frames loop at the original app's `100 ms` cadence.

The generated, tracked outputs are:

```text
src/MaskApp.App/Resources/Images/builtins/builtin_face_00.png ... builtin_face_69.png
src/MaskApp.App/Resources/Images/builtins/builtin_anim_00.gif ... builtin_anim_45.gif
src/MaskApp.Core/Features/BuiltIns/BuiltInPreviewCatalog.Generated.cs
build/builtin-preview-manifest.json
```

The animation set skips `builtin_anim_04.gif`. The manifest records frame
counts and SHA-256 hashes for all 115 assets.

## Regeneration

Install the pinned development-only image dependency, generate outputs, and
then run the non-mutating consistency check:

```powershell
python -m pip install -r build\scripts\requirements-builtin-previews.txt
python build\scripts\generate-builtin-previews.py
python build\scripts\generate-builtin-previews.py --check
```

Do not commit the APK, APKM, or `decompiled-app` tree. They remain ignored local
evidence; only the permission-cleared generated derivatives and their mapping
metadata are shipped.
