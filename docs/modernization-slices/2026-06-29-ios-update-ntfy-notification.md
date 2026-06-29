# iOS Update ntfy Notification

## Slice

- Name: iOS update ntfy notification
- Date: 2026-06-29
- Status: validated
- Owner: Codex
- Product pillar: Reliability As A Feature
- Capability confidence: Out of scope
- Physical validation status: Docs-only

## Intent

Notify the update channel when a release-backed iOS build is published, so the
phone-side install flow is easier to notice without adding in-app push
infrastructure.

## Target user moment

After the iOS workflow publishes a new signed IPA, the subscribed phone or
channel receives a concise update notification with a tap target for the install
page or GitHub Release.

## Observer-facing value

No mask-facing behavior changes. This is release-operations UX for the app
owner/tester.

## Final-goal contribution

The wearable face controller needs fast feedback when new test builds are ready.
This reduces friction in the iPhone validation loop without changing mask
protocol behavior.

## Capability claims

- This is webhook-only ntfy notification from GitHub Actions.
- No APNs/FCM app push, notification entitlement, device token registration,
  backend, Firebase, Azure Notification Hubs, mask protocol, BLE, firmware, or
  custom firmware behavior is added.

## User-visible improvement

Release-backed iOS updates can notify a subscribed ntfy topic after publish.
The notification opens the Pages install page when available, otherwise the
GitHub Release page.

## Current evidence

- Repo files: `.github/workflows/ios-ipa.yml`,
  `build/scripts/send-ntfy-update-notification.sh`,
  `docs/ios-ci-distribution.md`, `docs/progress.md`.
- Java evidence: none; this is release automation only.
- Existing tests: workflow helper syntax and diff checks.
- Existing validation gaps: live ntfy delivery requires configured GitHub
  Secrets and a GitHub Actions workflow run.

## Scope

In scope:

- Manual workflow input for update notification control.
- Optional ntfy topic URL and bearer token secrets.
- GitHub Actions notification after release-backed IPA publication.
- Docs and progress tracking.

Out of scope:

- In-app push notifications.
- APNs, FCM, Firebase, Azure Notification Hubs, or backend registration.
- MAUI app code, platform entitlements, notification permissions, or UI.
- Android release notifications.

## Files and flows

- Core: no changes.
- App UI: no changes.
- Platform adapters: no changes.
- Docs: iOS CI distribution setup, progress ledger, and this slice record.

## Test plan

- Unit tests: not applicable; no app code changed.
- Build validation: not applicable; no MAUI app code changed.
- Workflow validation: syntax-check the ntfy helper script and run
  `git diff --check`.
- Skipped validation and reason: live ntfy delivery requires repository secrets
  and a GitHub Actions release run.

## Deferred validation

- Add `NTFY_UPDATE_TOPIC_URL` and optional `NTFY_UPDATE_TOKEN` secrets.
- Run `Build iOS IPA` with `publish_release=true`, `publish_pages=true`, and
  `send_update_notification=true`.
- Confirm one ntfy notification arrives and opens the install page.
- Confirm a missing topic URL warns without failing a release run.

## Overclaim check

- No Vision or Experimental mask capability is presented as implemented.
- Firmware/custom firmware changes are excluded.
- Real-mask validation gaps are unchanged in `docs/progress.md`.
- Apple Watch is not implemented or changed.

## Measured outcome

- Changes made: added ntfy workflow input, release/install click URL
  environment, ntfy helper script, documentation, and progress tracking.
- Commands run: `bash -n build/scripts/send-ntfy-update-notification.sh`,
  `git diff --check`.
- Result: local workflow helper syntax and diff whitespace validation passed.
- Remaining risk: live ntfy delivery is unverified until GitHub Secrets are set
  and the iOS IPA workflow runs in GitHub Actions.

## Next slice candidate

- Run the iOS workflow with ntfy secrets configured and record the live delivery
  result in `docs/progress.md`.
