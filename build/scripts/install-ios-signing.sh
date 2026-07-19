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

file_size_bytes() {
  local path="$1"

  if stat -f '%z' "$path" >/dev/null 2>&1; then
    stat -f '%z' "$path"
    return 0
  fi

  stat -c '%s' "$path"
}

import_certificate() {
  local path="$1"

  security import "$path" -P "$IOS_P12_PASSWORD" -A -t cert -f pkcs12 -k "$keychain_path"
}

import_certificate_with_fallback() {
  if import_certificate "$certificate_path"; then
    return 0
  fi

  echo "::warning::macOS security could not import the p12 directly. Retrying with an OpenSSL-repacked p12."

  if ! command -v openssl >/dev/null 2>&1; then
    echo "::error::OpenSSL is not available to repack the p12 after security import failed." >&2
    return 1
  fi

  local pem_path="$RUNNER_TEMP/ios-build-certificate.pem"
  local compatible_certificate_path="$RUNNER_TEMP/ios-build-certificate-compatible.p12"
  local pkcs12_export_args=(-export -in "$pem_path" -out "$compatible_certificate_path" -passout "pass:$IOS_P12_PASSWORD")

  if openssl pkcs12 -help 2>&1 | grep -q -- '-legacy'; then
    pkcs12_export_args=(-export -legacy -in "$pem_path" -out "$compatible_certificate_path" -passout "pass:$IOS_P12_PASSWORD")
  fi

  if ! openssl pkcs12 -in "$certificate_path" -passin "pass:$IOS_P12_PASSWORD" -nodes -out "$pem_path" >/dev/null 2>&1; then
    echo "::error::OpenSSL could not read the decoded p12 with IOS_P12_PASSWORD." >&2
    rm -f "$pem_path" "$compatible_certificate_path"
    return 1
  fi

  if ! openssl pkcs12 "${pkcs12_export_args[@]}" >/dev/null 2>&1; then
    echo "::error::OpenSSL could not repack the p12 for macOS security import." >&2
    rm -f "$pem_path" "$compatible_certificate_path"
    return 1
  fi

  chmod 600 "$pem_path" "$compatible_certificate_path"
  if ! import_certificate "$compatible_certificate_path"; then
    rm -f "$pem_path" "$compatible_certificate_path"
    return 1
  fi

  rm -f "$pem_path" "$compatible_certificate_path"
}

require_env IOS_BUILD_CERTIFICATE_BASE64
require_env IOS_P12_PASSWORD
require_env IOS_PROVISION_PROFILE_BASE64
require_env WATCHOS_PROVISION_PROFILE_BASE64
require_env IOS_KEYCHAIN_PASSWORD
require_env RUNNER_TEMP
require_env GITHUB_ENV

certificate_path="$RUNNER_TEMP/ios-build-certificate.p12"
profile_path="$RUNNER_TEMP/ios-build-profile.mobileprovision"
profile_plist_path="$RUNNER_TEMP/ios-build-profile.plist"
watch_profile_path="$RUNNER_TEMP/watchos-build-profile.mobileprovision"
watch_profile_plist_path="$RUNNER_TEMP/watchos-build-profile.plist"
keychain_path="$RUNNER_TEMP/ios-build.keychain-db"
profile_install_dir="$HOME/Library/MobileDevice/Provisioning Profiles"

decode_base64_to_file "$IOS_BUILD_CERTIFICATE_BASE64" "$certificate_path"
decode_base64_to_file "$IOS_PROVISION_PROFILE_BASE64" "$profile_path"
decode_base64_to_file "$WATCHOS_PROVISION_PROFILE_BASE64" "$watch_profile_path"
chmod 600 "$certificate_path" "$profile_path" "$watch_profile_path"

certificate_size="$(file_size_bytes "$certificate_path")"
profile_size="$(file_size_bytes "$profile_path")"
watch_profile_size="$(file_size_bytes "$watch_profile_path")"

if [ "$certificate_size" -le 0 ]; then
  echo "::error::Decoded p12 certificate file is empty." >&2
  exit 1
fi

if [ "$profile_size" -le 0 ]; then
  echo "::error::Decoded provisioning profile file is empty." >&2
  exit 1
fi

if [ "$watch_profile_size" -le 0 ]; then
  echo "::error::Decoded watchOS provisioning profile file is empty." >&2
  exit 1
fi

echo "Decoded p12 certificate size: $certificate_size bytes"
echo "Decoded provisioning profile size: $profile_size bytes"
echo "Decoded watchOS provisioning profile size: $watch_profile_size bytes"

security create-keychain -p "$IOS_KEYCHAIN_PASSWORD" "$keychain_path"
security set-keychain-settings -lut 21600 "$keychain_path"
security unlock-keychain -p "$IOS_KEYCHAIN_PASSWORD" "$keychain_path"
security list-keychains -d user -s "$keychain_path"
import_certificate_with_fallback
security set-key-partition-list -S apple-tool:,apple:,codesign: -s -k "$IOS_KEYCHAIN_PASSWORD" "$keychain_path"

security cms -D -i "$profile_path" > "$profile_plist_path"
security cms -D -i "$watch_profile_path" > "$watch_profile_plist_path"
profile_uuid="$(/usr/libexec/PlistBuddy -c 'Print UUID' "$profile_plist_path")"
profile_name="$(/usr/libexec/PlistBuddy -c 'Print Name' "$profile_plist_path")"
team_id="$(/usr/libexec/PlistBuddy -c 'Print TeamIdentifier:0' "$profile_plist_path" 2>/dev/null || true)"
watch_profile_uuid="$(/usr/libexec/PlistBuddy -c 'Print UUID' "$watch_profile_plist_path")"
watch_profile_name="$(/usr/libexec/PlistBuddy -c 'Print Name' "$watch_profile_plist_path")"
watch_team_id="$(/usr/libexec/PlistBuddy -c 'Print TeamIdentifier:0' "$watch_profile_plist_path" 2>/dev/null || true)"

if [ -z "$profile_uuid" ] || [ -z "$profile_name" ]; then
  echo "::error::Could not extract UUID and Name from the provisioning profile." >&2
  exit 1
fi

if [ -z "$watch_profile_uuid" ] || [ -z "$watch_profile_name" ]; then
  echo "::error::Could not extract UUID and Name from the watchOS provisioning profile." >&2
  exit 1
fi

if [ -n "$team_id" ] && [ -n "$watch_team_id" ] && [ "$team_id" != "$watch_team_id" ]; then
  echo "::error::The iOS and watchOS provisioning profiles belong to different Apple teams." >&2
  exit 1
fi

mkdir -p "$profile_install_dir"
installed_profile_path="$profile_install_dir/$profile_uuid.mobileprovision"
installed_watch_profile_path="$profile_install_dir/$watch_profile_uuid.mobileprovision"
cp "$profile_path" "$installed_profile_path"
cp "$watch_profile_path" "$installed_watch_profile_path"

{
  echo "IOS_PROFILE_UUID=$profile_uuid"
  echo "IOS_PROFILE_NAME=$profile_name"
  echo "IOS_PROFILE_PATH=$installed_profile_path"
  echo "WATCHOS_PROFILE_UUID=$watch_profile_uuid"
  echo "WATCHOS_PROFILE_NAME=$watch_profile_name"
  echo "WATCHOS_PROFILE_PATH=$installed_watch_profile_path"
  echo "IOS_KEYCHAIN_PATH=$keychain_path"
  if [ -n "$team_id" ]; then
    echo "IOS_TEAM_ID=$team_id"
  fi
} >> "$GITHUB_ENV"

echo "Installed provisioning profile: $profile_name ($profile_uuid)"
echo "Installed watchOS provisioning profile: $watch_profile_name ($watch_profile_uuid)"
if [ -n "$team_id" ]; then
  echo "Provisioning team id: $team_id"
fi
