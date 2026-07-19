# Docs

This folder intentionally keeps only durable references needed for development.
Historical progress logs, modernization slice records, and UI concept mockups
were removed to keep coding-agent context small.

- `stock-mask-protocol.md` is the source of truth for BLE topology, encrypted
  command shape, text/image upload behavior, ACK parsing, and unverified mask
  capabilities.
- `tr1906-firmware-analysis.md` records reproducible OTA decryption and the
  headless Ghidra annotation/export workflow for the narrower BLE
  command/upload/visualizer profile compiled into the two bundled TR1906
  firmware images.
- `android-source-map.md` summarizes the Java source snapshot used as migration
  evidence.
- `setup.md` records local setup and build prerequisites.
- `ios-ci-distribution.md` explains GitHub Actions IPA builds, signing secrets,
  Releases, Pages, and Feather/AltStore-style updates.
- `performance-validation.md` defines the repeatable signed-iPhone procedure
  for sustained Stage, animation, import, Audio Labs, lifecycle, memory, CPU,
  thermal, and battery evidence.
- `maskpack-format.md` defines the offline MaskPack v2 archive, typed payloads,
  safety limits, conflict/transaction behavior, and schema-v1 migration.
- `icon-sources.md` records source and licensing notes for vendored shortcut
  icons.
- `builtin-preview-sources.md` records stock preview provenance, exact ID/frame
  mapping, regeneration, and validation.

Update these files only when their durable facts change. Do not add per-slice
status records or broad planning docs unless the user explicitly asks for them.
