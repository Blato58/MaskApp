#!/usr/bin/env python3
import argparse
import html
import json
import plistlib
import sys
from pathlib import Path
from urllib.parse import quote


def fail(message: str) -> None:
    raise SystemExit(message)


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate a static iOS install/update page.")
    parser.add_argument("--source-json-path", required=True)
    parser.add_argument("--source-json-url", required=True)
    parser.add_argument("--ipa-url", required=True)
    parser.add_argument("--output-dir", required=True)
    parser.add_argument("--release-notes", default="")
    args = parser.parse_args()

    source_path = Path(args.source_json_path)
    if not source_path.exists():
        fail(f"Source JSON does not exist: {source_path}")

    source = json.loads(source_path.read_text(encoding="utf-8"))
    apps = source.get("apps") or []
    if not apps:
        fail("Source JSON has no apps.")

    app = apps[0]
    versions = app.get("versions") or []
    if not versions:
        fail("Source app has no versions.")

    latest = versions[0]
    app_name = app.get("name", "iOS App")
    bundle_id = app.get("bundleIdentifier", "")
    version = latest.get("version", "")
    build = latest.get("buildVersion", "")
    release_notes = args.release_notes or latest.get("localizedDescription", "")
    source_url = args.source_json_url
    encoded_source_url = quote(source_url, safe="")
    feather_url = f"feather://source/{source_url}"
    altstore_url = f"altstore://source?url={encoded_source_url}"
    manifest_url = source_url.rsplit("/", 1)[0].rstrip("/") + "/manifest.plist"
    direct_install_url = f"itms-services://?action=download-manifest&url={quote(manifest_url, safe='')}"

    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    title = html.escape(app_name)
    escaped_notes = html.escape(release_notes).replace("\n", "<br>")
    escaped_source_url = html.escape(source_url)
    escaped_ipa_url = html.escape(args.ipa_url)
    escaped_direct_install_url = html.escape(direct_install_url)
    escaped_bundle_id = html.escape(bundle_id)

    index_html = f"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{title} iOS Builds</title>
  <style>
    :root {{
      color-scheme: light dark;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
      background: #f6f7f9;
      color: #14171f;
    }}
    body {{
      margin: 0;
      padding: 32px 18px;
    }}
    main {{
      max-width: 760px;
      margin: 0 auto;
    }}
    h1 {{
      margin: 0 0 8px;
      font-size: 34px;
      line-height: 1.1;
    }}
    h2 {{
      margin-top: 32px;
      font-size: 20px;
    }}
    p, li {{
      line-height: 1.55;
    }}
    .meta {{
      color: #566070;
      margin: 0 0 24px;
    }}
    .actions {{
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
      margin: 24px 0;
    }}
    a.button {{
      display: inline-block;
      padding: 12px 16px;
      border-radius: 8px;
      color: white;
      background: #0a84ff;
      text-decoration: none;
      font-weight: 650;
    }}
    a.secondary {{
      background: #273142;
    }}
    code {{
      word-break: break-all;
      background: rgba(127, 127, 127, 0.14);
      border-radius: 6px;
      padding: 2px 5px;
    }}
    .warning {{
      border-left: 4px solid #ff9f0a;
      padding: 10px 14px;
      background: rgba(255, 159, 10, 0.12);
    }}
    @media (prefers-color-scheme: dark) {{
      :root {{
        background: #101217;
        color: #eef1f6;
      }}
      .meta {{
        color: #a6afbd;
      }}
    }}
  </style>
</head>
<body>
  <main>
    <h1>{title}</h1>
    <p class="meta">Version {html.escape(str(version))} ({html.escape(str(build))}) &middot; {escaped_bundle_id}</p>

    <div class="actions">
      <a class="button" href="{escaped_direct_install_url}">Install signed app</a>
      <a class="button" href="{escaped_ipa_url}">Download IPA</a>
      <a class="button secondary" href="{html.escape(feather_url)}">Add source to Feather</a>
      <a class="button secondary" href="{html.escape(altstore_url)}">Add source to AltStore</a>
    </div>

    <h2>Release Notes</h2>
    <p>{escaped_notes}</p>

    <h2>Source URL</h2>
    <p><code>{escaped_source_url}</code></p>

    <h2>Install Or Update</h2>
    <ol>
      <li>Open this page on your iPhone.</li>
      <li>Use <strong>Install signed app</strong> first to install the CI-signed IPA without re-signing.</li>
      <li>If direct install is blocked on the device, add the source to Feather or an AltStore-style installer.</li>
      <li>Future builds should appear as updates after this source JSON changes.</li>
    </ol>

    <p class="warning">The IPA URL and source JSON must be reachable from the iPhone. Private GitHub repositories or private release assets may not work for direct Feather or AltStore-style updates unless the phone app can access the URL.</p>

    <h2>Signing</h2>
    <p>This IPA is signed in GitHub Actions with the provisioning profile stored in GitHub Secrets. Feather installs through its own signing flow, so use the direct signed install path when re-signing causes a launch crash.</p>

    <h2>Manual Fallback</h2>
    <p>Download the IPA from the workflow artifact or GitHub Release, send it to the iPhone, and open it in your sideloading tool.</p>
  </main>
</body>
</html>
"""

    (output_dir / "index.html").write_text(index_html, encoding="utf-8")

    manifest = {
        "items": [
            {
                "assets": [
                    {
                        "kind": "software-package",
                        "url": args.ipa_url,
                    }
                ],
                "metadata": {
                    "bundle-identifier": bundle_id,
                    "bundle-version": str(build),
                    "kind": "software",
                    "title": app_name,
                },
            }
        ]
    }
    (output_dir / "manifest.plist").write_bytes(plistlib.dumps(manifest, sort_keys=False))
    print(f"Wrote {output_dir / 'index.html'}")
    print(f"Wrote {output_dir / 'manifest.plist'}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
