#!/usr/bin/env bash
set -euo pipefail

require_env() {
  local name="$1"
  if [ -z "${!name:-}" ]; then
    echo "::error::$name is required." >&2
    exit 1
  fi
}

decode_base64_to_file() {
  local value="$1"
  local output="$2"

  if printf '%s' "$value" | base64 --decode > "$output" 2>/dev/null; then
    return 0
  fi

  printf '%s' "$value" | base64 -D > "$output"
}

require_env IOS_BUILD_CERTIFICATE_BASE64
require_env IOS_P12_PASSWORD
require_env IOS_PROVISION_PROFILE_BASE64
require_env IOS_KEYCHAIN_PASSWORD
require_env RUNNER_TEMP
require_env GITHUB_ENV

certificate_path="$RUNNER_TEMP/ios-build-certificate.p12"
profile_path="$RUNNER_TEMP/ios-build-profile.mobileprovision"
profile_plist_path="$RUNNER_TEMP/ios-build-profile.plist"
keychain_path="$RUNNER_TEMP/ios-build.keychain-db"
profile_install_dir="$HOME/Library/MobileDevice/Provisioning Profiles"

decode_base64_to_file "$IOS_BUILD_CERTIFICATE_BASE64" "$certificate_path"
decode_base64_to_file "$IOS_PROVISION_PROFILE_BASE64" "$profile_path"
chmod 600 "$certificate_path" "$profile_path"

security create-keychain -p "$IOS_KEYCHAIN_PASSWORD" "$keychain_path"
security set-keychain-settings -lut 21600 "$keychain_path"
security unlock-keychain -p "$IOS_KEYCHAIN_PASSWORD" "$keychain_path"
security list-keychains -d user -s "$keychain_path"
security import "$certificate_path" -P "$IOS_P12_PASSWORD" -A -t cert -f pkcs12 -k "$keychain_path"
security set-key-partition-list -S apple-tool:,apple:,codesign: -s -k "$IOS_KEYCHAIN_PASSWORD" "$keychain_path"

security cms -D -i "$profile_path" > "$profile_plist_path"
profile_uuid="$(/usr/libexec/PlistBuddy -c 'Print UUID' "$profile_plist_path")"
profile_name="$(/usr/libexec/PlistBuddy -c 'Print Name' "$profile_plist_path")"
team_id="$(/usr/libexec/PlistBuddy -c 'Print TeamIdentifier:0' "$profile_plist_path" 2>/dev/null || true)"

if [ -z "$profile_uuid" ] || [ -z "$profile_name" ]; then
  echo "::error::Could not extract UUID and Name from the provisioning profile." >&2
  exit 1
fi

mkdir -p "$profile_install_dir"
installed_profile_path="$profile_install_dir/$profile_uuid.mobileprovision"
cp "$profile_path" "$installed_profile_path"

{
  echo "IOS_PROFILE_UUID=$profile_uuid"
  echo "IOS_PROFILE_NAME=$profile_name"
  echo "IOS_PROFILE_PATH=$installed_profile_path"
  echo "IOS_KEYCHAIN_PATH=$keychain_path"
  if [ -n "$team_id" ]; then
    echo "IOS_TEAM_ID=$team_id"
  fi
} >> "$GITHUB_ENV"

echo "Installed provisioning profile: $profile_name ($profile_uuid)"
if [ -n "$team_id" ]; then
  echo "Provisioning team id: $team_id"
fi
