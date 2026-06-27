#!/usr/bin/env python3
import argparse
import json
import os
import plistlib
import shutil
import subprocess
import sys
import tempfile
import urllib.error
import urllib.request
import zipfile
from datetime import datetime, timezone
from pathlib import Path, PurePosixPath


STANDARD_ENTITLEMENTS = {
    "application-identifier",
    "com.apple.developer.team-identifier",
    "com.apple.security.application-groups",
    "get-task-allow",
    "keychain-access-groups",
}


def fail(message: str) -> None:
    raise SystemExit(message)


def load_existing_source(url: str) -> dict:
    if not url:
        return {}

    try:
        with urllib.request.urlopen(url, timeout=20) as response:
            status = getattr(response, "status", None) or response.getcode() or 200
            if status >= 400:
                return {}
            return json.loads(response.read().decode("utf-8"))
    except (urllib.error.URLError, TimeoutError, json.JSONDecodeError):
        return {}


def find_info_plist(ipa_path: Path) -> tuple[str, dict]:
    with zipfile.ZipFile(ipa_path) as archive:
        names = archive.namelist()
        candidates = [
            name
            for name in names
            if name.startswith("Payload/")
            and name.endswith(".app/Info.plist")
            and len(PurePosixPath(name).parts) == 3
        ]

        if not candidates:
            candidates = [
                name
                for name in names
                if name.startswith("Payload/") and name.endswith("/Info.plist") and ".app/" in name
            ]

        if not candidates:
            fail(f"No Payload/*.app/Info.plist found in {ipa_path}")

        plist_name = sorted(candidates)[0]
        with archive.open(plist_name) as stream:
            return plist_name, plistlib.load(stream)


def extract_entitlements(ipa_path: Path, app_relative_path: str) -> list[str]:
    if shutil.which("codesign") is None:
        return []

    app_dir_name = app_relative_path.split("/Info.plist", 1)[0]
    with tempfile.TemporaryDirectory() as temp_dir:
        with zipfile.ZipFile(ipa_path) as archive:
            for member in archive.namelist():
                if member.startswith(app_dir_name + "/"):
                    archive.extract(member, temp_dir)

        app_path = Path(temp_dir) / app_dir_name
        if not app_path.exists():
            return []

        result = subprocess.run(
            ["codesign", "-d", "--entitlements", ":-", str(app_path)],
            check=False,
            stdout=subprocess.PIPE,
            stderr=subprocess.DEVNULL,
        )
        if result.returncode != 0 or not result.stdout:
            return []

        try:
            entitlements = plistlib.loads(result.stdout)
        except Exception:
            return []

    return sorted(key for key in entitlements.keys() if key not in STANDARD_ENTITLEMENTS)


def privacy_usage_descriptions(info: dict) -> dict:
    privacy = {}
    for key, value in info.items():
        if key.endswith("UsageDescription") and isinstance(value, str):
            privacy[key] = value
    return dict(sorted(privacy.items()))


def existing_app(source: dict, bundle_id: str) -> dict:
    for app in source.get("apps", []):
        if app.get("bundleIdentifier") == bundle_id:
            return app
    return {}


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate an AltStore-compatible source JSON.")
    parser.add_argument("--ipa-path", required=True)
    parser.add_argument("--ipa-url", required=True)
    parser.add_argument("--existing-source-url", default="")
    parser.add_argument("--output-dir", required=True)
    parser.add_argument("--source-name", required=True)
    parser.add_argument("--developer-name", required=True)
    parser.add_argument("--release-notes", default="")
    parser.add_argument("--min-ios-version", default="")
    parser.add_argument("--icon-url", default="")
    parser.add_argument("--category", default="utilities")
    parser.add_argument("--subtitle", default="Internal iOS builds")
    parser.add_argument("--localized-description", default="")
    parser.add_argument("--tint-color", default="#0A84FF")
    parser.add_argument("--website", default="")
    args = parser.parse_args()

    ipa_path = Path(args.ipa_path)
    if not ipa_path.exists():
        fail(f"IPA does not exist: {ipa_path}")

    app_relative_info_path, info = find_info_plist(ipa_path)
    bundle_id = info.get("CFBundleIdentifier")
    name = info.get("CFBundleDisplayName") or info.get("CFBundleName")
    version = info.get("CFBundleShortVersionString")
    build_version = info.get("CFBundleVersion")

    if not bundle_id:
        fail("CFBundleIdentifier is missing from Info.plist")
    if not name:
        fail("CFBundleDisplayName or CFBundleName is missing from Info.plist")
    if not version:
        fail("CFBundleShortVersionString is missing from Info.plist")
    if not build_version:
        fail("CFBundleVersion is missing from Info.plist")

    min_os_version = args.min_ios_version or info.get("MinimumOSVersion", "")
    release_notes = args.release_notes or f"{name} {version} ({build_version})"
    localized_description = args.localized_description or f"Internal signed builds for {name}."
    app_size = ipa_path.stat().st_size
    privacy = privacy_usage_descriptions(info)
    entitlements = extract_entitlements(ipa_path, app_relative_info_path)

    existing_source = load_existing_source(args.existing_source_url)
    prior_app = existing_app(existing_source, bundle_id)
    prior_versions = prior_app.get("versions", [])

    new_version = {
        "version": str(version),
        "buildVersion": str(build_version),
        "date": datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z"),
        "localizedDescription": release_notes,
        "downloadURL": args.ipa_url,
        "size": app_size,
    }
    if min_os_version:
        new_version["minOSVersion"] = str(min_os_version)

    versions = [new_version]
    for prior in prior_versions:
        if (
            str(prior.get("version", "")) == str(version)
            and str(prior.get("buildVersion", "")) == str(build_version)
        ):
            continue
        versions.append(prior)

    app = {
        "name": name,
        "bundleIdentifier": bundle_id,
        "developerName": args.developer_name,
        "subtitle": args.subtitle,
        "localizedDescription": localized_description,
        "iconURL": args.icon_url or prior_app.get("iconURL", ""),
        "tintColor": args.tint_color,
        "category": args.category,
        "downloadURL": args.ipa_url,
        "versions": versions,
        "appPermissions": {
            "entitlements": entitlements,
            "privacy": privacy,
        },
    }

    source = {
        "name": args.source_name,
        "subtitle": args.subtitle,
        "description": localized_description,
        "website": args.website or existing_source.get("website", ""),
        "apps": [app],
        "news": existing_source.get("news", []),
    }

    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    output_path = output_dir / "apps.json"
    output_path.write_text(json.dumps(source, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"Wrote {output_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
