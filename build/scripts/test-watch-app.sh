#!/usr/bin/env bash
set -euo pipefail

project_path="${1:-src/MaskApp.Watch/MaskApp.Watch.xcodeproj}"
scheme="${2:-MaskApp Watch}"

device_ids="$({ xcrun simctl list devices available -j || true; } | python3 -c '
import json, sys
data = json.load(sys.stdin)
watch = None
phone = None
for runtime, devices in sorted(data.get("devices", {}).items(), reverse=True):
    if watch is None and "watchOS" in runtime:
        watch = next((d["udid"] for d in devices if d.get("isAvailable")), None)
    if phone is None and "iOS" in runtime and "watchOS" not in runtime:
        phone = next((d["udid"] for d in devices if d.get("isAvailable") and "iPhone" in d.get("name", "")), None)
print(watch or "")
print(phone or "")
')"

watch_id="$(printf '%s\n' "$device_ids" | sed -n '1p')"
phone_id="$(printf '%s\n' "$device_ids" | sed -n '2p')"

if [ -z "$watch_id" ]; then
  echo "::error::No available watchOS simulator was found." >&2
  exit 1
fi

if [ -n "$phone_id" ]; then
  xcrun simctl pair "$watch_id" "$phone_id" >/dev/null 2>&1 || true
  xcrun simctl boot "$phone_id" >/dev/null 2>&1 || true
fi
xcrun simctl boot "$watch_id" >/dev/null 2>&1 || true
xcrun simctl bootstatus "$watch_id" -b

xcodebuild test \
  -project "$project_path" \
  -scheme "$scheme" \
  -configuration Debug \
  -destination "platform=watchOS Simulator,id=$watch_id" \
  CODE_SIGNING_ALLOWED=NO
