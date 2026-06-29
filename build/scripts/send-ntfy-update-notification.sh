#!/usr/bin/env bash
set -euo pipefail

require_env() {
  local name="$1"
  if [ -z "${!name:-}" ]; then
    echo "::error::$name is required." >&2
    exit 1
  fi
}

warn_missing_topic() {
  echo "::warning::NTFY_UPDATE_TOPIC_URL is not configured. Skipping update notification."
}

if [ -z "${NTFY_UPDATE_TOPIC_URL:-}" ]; then
  warn_missing_topic
  exit 0
fi

require_env DISPLAY_VERSION
require_env BUILD_NUMBER
require_env RELEASE_NOTES
require_env RELEASE_URL
require_env IPA_DOWNLOAD_URL

if ! command -v curl >/dev/null 2>&1; then
  echo "::error::curl is required to send the ntfy update notification." >&2
  exit 1
fi

click_url="$RELEASE_URL"
if [ "${PUBLISH_PAGES:-false}" = "true" ] && [ "${PAGES_AVAILABLE:-false}" = "true" ] && [ -n "${INSTALL_PAGE_URL:-}" ]; then
  click_url="$INSTALL_PAGE_URL"
fi

message="v${DISPLAY_VERSION} (${BUILD_NUMBER}) is ready: ${RELEASE_NOTES}"
curl_args=(
  --fail
  --show-error
  --silent
  --request POST
  --header "Title: Shining Mask update ready"
  --header "Priority: high"
  --header "Tags: iphone,rocket"
  --header "Click: ${click_url}"
  --data-binary "$message"
)

if [ -n "${NTFY_UPDATE_TOKEN:-}" ]; then
  curl_args+=(--header "Authorization: Bearer ${NTFY_UPDATE_TOKEN}")
fi

curl "${curl_args[@]}" "$NTFY_UPDATE_TOPIC_URL"
echo "Sent ntfy update notification for ${DISPLAY_VERSION} (${BUILD_NUMBER})."
