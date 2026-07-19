#!/usr/bin/env bash
set -euo pipefail

ipa_path="${1:?IPA path is required}"
expected_phone_bundle_id="${2:-app.turquoise6409.green2444}"
expected_watch_bundle_id="${3:-app.turquoise6409.green2444.watchkitapp}"

verify_root="$(mktemp -d "${RUNNER_TEMP:-/tmp}/maskapp-watch-verify.XXXXXX")"
trap 'rm -rf "$verify_root"' EXIT

ditto -x -k "$ipa_path" "$verify_root"

if [ ! -d "$verify_root/Payload" ]; then
  echo "::error::The IPA does not contain a Payload directory." >&2
  exit 1
fi

phone_app="$(find "$verify_root/Payload" -maxdepth 1 -type d -name '*.app' | head -n 1)"
if [ -z "$phone_app" ] || [ ! -d "$phone_app" ]; then
  echo "::error::The IPA does not contain an iPhone app." >&2
  exit 1
fi

if [ ! -d "$phone_app/Watch" ]; then
  echo "::error::The IPA does not contain an embedded Apple Watch app." >&2
  exit 1
fi

watch_app="$(find "$phone_app/Watch" -maxdepth 1 -type d -name '*.app' | head -n 1)"
if [ -z "$watch_app" ] || [ ! -d "$watch_app" ]; then
  echo "::error::The IPA does not contain an embedded Apple Watch app." >&2
  exit 1
fi

phone_bundle_id="$(/usr/libexec/PlistBuddy -c 'Print CFBundleIdentifier' "$phone_app/Info.plist")"
watch_bundle_id="$(/usr/libexec/PlistBuddy -c 'Print CFBundleIdentifier' "$watch_app/Info.plist")"
companion_bundle_id="$(/usr/libexec/PlistBuddy -c 'Print WKCompanionAppBundleIdentifier' "$watch_app/Info.plist")"

if [ "$phone_bundle_id" != "$expected_phone_bundle_id" ]; then
  echo "::error::Unexpected iPhone bundle id: $phone_bundle_id" >&2
  exit 1
fi
if [ "$watch_bundle_id" != "$expected_watch_bundle_id" ]; then
  echo "::error::Unexpected watch bundle id: $watch_bundle_id" >&2
  exit 1
fi
if [ "$companion_bundle_id" != "$phone_bundle_id" ]; then
  echo "::error::Watch companion bundle id does not match the iPhone app." >&2
  exit 1
fi

codesign --verify --strict "$watch_app"
codesign --verify --deep --strict "$phone_app"
test -f "$watch_app/embedded.mobileprovision"

echo "Verified embedded watch app: $watch_bundle_id"
