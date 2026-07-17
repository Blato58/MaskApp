#!/usr/bin/env python3
"""Decrypt the two known TR1906 OTA firmware containers.

The published key is a 128-byte repeating XOR stream represented as 16-bit
words. The bytes in each displayed word must be swapped for these files. The
16-byte OTA header is plaintext; XOR starts at file offset 0x10 while the key
phase remains based on the absolute file offset.
"""

from __future__ import annotations

import argparse
import hashlib
import struct
from dataclasses import dataclass
from pathlib import Path


OTA_HEADER_SIZE = 0x10
INNER_HEADER_SIZE = 0x08
IMAGE_OFFSET = OTA_HEADER_SIZE + INNER_HEADER_SIZE
IMAGE_LOAD_ADDRESS = 0x00010000

PUBLISHED_WORD_KEY_HEX = (
    "2776639913bbb1cc89dd58e6c46e2cf3"
    "62379679b11bcb3cd88d659eecc6324f"
    "76639927bbb1cc13dd58e6896e2cf3c4"
    "379679621bcb3cb18d659ed8c6324fec"
    "63992776b1cc13bb58e689dd2cf3c46e"
    "96796237cb3cb11b659ed88d324fecc6"
    "99277663cc13bbb1e689dd58f3c46e2c"
    "796237963cb11bcb9ed88d654fecc632"
)


@dataclass(frozen=True)
class KnownFirmware:
    name: str
    decrypted_sha256: str
    image_sha256: str
    version: bytes


KNOWN_FIRMWARE = {
    "36a3b4a1144ada273e03e08c91d6cb1b7fdb9f35": KnownFirmware(
        name="TR1906R04-10_OTA.bin",
        decrypted_sha256="70da370a61f74a8d9aef20370f75b83eabe2e390e40150bba1ec25e161f2f556",
        image_sha256="083ae84b313e641037a7ec42d003b6b0965b4dbe4b04231b087a12cdb7be537f",
        version=b"TR1906R04-10",
    ),
    "f0f38c1faacf3fc2730b0809d381aecdd56566e2": KnownFirmware(
        name="TR1906R04-1-10_OTA.bin",
        decrypted_sha256="503557e2ed6aba141b5e90834590bd2e40811a80ab8b5285f2acba3d4334a177",
        image_sha256="f53a8cb6619cb605244355932d49dc3b032c8679bf7f5b5a299d24814b22e1de",
        version=b"TR1906R04-01-10",
    ),
}


def sha1(data: bytes) -> str:
    return hashlib.sha1(data).hexdigest()


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def effective_xor_key() -> bytes:
    published = bytes.fromhex(PUBLISHED_WORD_KEY_HEX)
    if len(published) != 128:
        raise ValueError(f"Expected a 128-byte key, got {len(published)} bytes")

    return bytes(
        byte
        for index in range(0, len(published), 2)
        for byte in (published[index + 1], published[index])
    )


def decrypt_container(encrypted: bytes) -> bytes:
    if len(encrypted) < IMAGE_OFFSET + 8:
        raise ValueError("Input is too short to contain the OTA and image headers")

    key = effective_xor_key()
    decrypted = bytearray(encrypted)
    for offset in range(OTA_HEADER_SIZE, len(decrypted)):
        decrypted[offset] ^= key[offset % len(key)]
    return bytes(decrypted)


def validate_container(encrypted: bytes, decrypted: bytes) -> tuple[bytes, list[str]]:
    errors: list[str] = []
    image = decrypted[IMAGE_OFFSET:]

    encrypted_body_length = struct.unpack_from("<I", encrypted)[0]
    if encrypted_body_length != len(encrypted) - OTA_HEADER_SIZE:
        errors.append(
            "outer header length is "
            f"{encrypted_body_length}, expected {len(encrypted) - OTA_HEADER_SIZE}"
        )

    initial_sp, entry_candidate = struct.unpack_from("<II", image)
    if initial_sp & 0xFFF00000 != 0x20000000 or initial_sp & 0x3:
        errors.append(f"implausible initial stack pointer 0x{initial_sp:08x}")

    entry_address = entry_candidate & ~1
    image_end = IMAGE_LOAD_ADDRESS + len(image)
    if entry_candidate & 1 == 0 or not IMAGE_LOAD_ADDRESS <= entry_address < image_end:
        errors.append(f"implausible Thumb entry candidate 0x{entry_candidate:08x}")

    input_sha1 = sha1(encrypted)
    known = KNOWN_FIRMWARE.get(input_sha1)
    if known is not None:
        if sha256(decrypted) != known.decrypted_sha256:
            errors.append("decrypted container SHA-256 does not match the known result")
        if sha256(image) != known.image_sha256:
            errors.append("raw image SHA-256 does not match the known result")
        if known.version not in image:
            errors.append(f"expected version string {known.version.decode()} was not found")

    return image, errors


def output_paths(input_path: Path, output_dir: Path) -> tuple[Path, Path]:
    return (
        output_dir / f"{input_path.name}.decrypted.bin",
        output_dir / f"{input_path.name}.image.bin",
    )


def write_file(path: Path, data: bytes, force: bool) -> None:
    if path.exists() and not force:
        raise FileExistsError(f"Refusing to overwrite {path}; pass --force to replace it")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)


def decrypt_file(input_path: Path, output_dir: Path, force: bool) -> None:
    encrypted = input_path.read_bytes()
    input_sha1 = sha1(encrypted)
    known = KNOWN_FIRMWARE.get(input_sha1)
    decrypted = decrypt_container(encrypted)
    image, errors = validate_container(encrypted, decrypted)

    if errors:
        details = "\n  - ".join(errors)
        raise ValueError(f"Validation failed for {input_path}:\n  - {details}")

    decrypted_path, image_path = output_paths(input_path, output_dir)
    write_file(decrypted_path, decrypted, force)
    write_file(image_path, image, force)

    label = known.name if known is not None else "unknown TR1906-compatible input"
    initial_sp, entry_candidate = struct.unpack_from("<II", image)
    print(f"{input_path}: {label}")
    print(f"  encrypted SHA-1:       {input_sha1}")
    print(f"  decrypted SHA-256:     {sha256(decrypted)}")
    print(f"  raw image SHA-256:     {sha256(image)}")
    print(f"  image mapping:         file +0x{IMAGE_OFFSET:x} -> 0x{IMAGE_LOAD_ADDRESS:08x}")
    print(f"  initial SP / entry:    0x{initial_sp:08x} / 0x{entry_candidate:08x}")
    print(f"  decrypted container:   {decrypted_path}")
    print(f"  raw Cortex-M image:    {image_path}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("inputs", nargs="+", type=Path, help="encrypted TR1906 OTA .bin file")
    parser.add_argument(
        "--output-dir",
        type=Path,
        help="output directory (defaults to each input file's directory)",
    )
    parser.add_argument("--force", action="store_true", help="replace existing output files")
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    for input_path in args.inputs:
        if not input_path.is_file():
            raise SystemExit(f"Input does not exist or is not a file: {input_path}")
        output_dir = args.output_dir or input_path.parent
        try:
            decrypt_file(input_path, output_dir, args.force)
        except (FileExistsError, ValueError) as error:
            raise SystemExit(str(error)) from error


if __name__ == "__main__":
    main()
