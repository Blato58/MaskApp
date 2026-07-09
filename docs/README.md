# Docs

This folder intentionally keeps only durable references needed for development.
Historical progress logs, modernization slice records, and UI concept mockups
were removed to keep coding-agent context small.

- `stock-mask-protocol.md` is the source of truth for BLE topology, encrypted
  command shape, text/image upload behavior, ACK parsing, and unverified mask
  capabilities.
- `android-source-map.md` summarizes the Java source snapshot used as migration
  evidence.
- `setup.md` records local setup and build prerequisites.
- `ios-ci-distribution.md` explains GitHub Actions IPA builds, signing secrets,
  Releases, Pages, and Feather/AltStore-style updates.
- `maskpack-format.md` describes the manifest format implemented by the
  MaskPack parser/validator.
- `icon-sources.md` records source and licensing notes for vendored shortcut
  icons.
- `builtin-preview-sources.md` records stock preview provenance, exact ID/frame
  mapping, regeneration, and validation.

Update these files only when their durable facts change. Do not add per-slice
status records or broad planning docs unless the user explicitly asks for them.
