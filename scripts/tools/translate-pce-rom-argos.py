#!/usr/bin/env python3
"""Find reviewable text in a HuCard and build a fixed-size French copy.

Usage:
  python scripts/tools/translate-pce-rom-argos.py scan GAME.pce candidates.json
  python scripts/tools/translate-pce-rom-argos.py scan-ascii GAME.pce candidates.json
  # Inspect candidates.json and set accepted=true only for confirmed game text.
  python scripts/tools/translate-pce-rom-argos.py translate candidates.json proposals.json
  # Review/shorten each translation and set pad_byte if the game's format needs it.
  python scripts/tools/translate-pce-rom-argos.py build proposals.json translated.pce

Offsets include any copier header. The original ROM is never modified. This tool
only handles text represented by the chosen character encoding; custom tile fonts,
compressed scripts, pointer relocation and missing French glyphs require game-
specific reverse engineering.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re


DEFAULT_ENCODING = "cp932"
MIN_JAPANESE_CHARACTERS = 4
MAX_CANDIDATE_BYTES = 192
MIN_JAPANESE_RATIO = 0.6
DEFAULT_PAD_BYTE = "20"
JAPANESE_CHARACTER = re.compile(r"[\u3040-\u30ff\u3400-\u9fff]")
ASCII_CANDIDATE = re.compile(rb"[A-Z][A-Z 0-9!?.-]{3,}")


def digest(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def japanese_pair(data: bytes, offset: int, encoding: str) -> str | None:
    if offset + 1 >= len(data):
        return None
    lead, trail = data[offset : offset + 2]
    if not (0x81 <= lead <= 0x9F or 0xE0 <= lead <= 0xFC):
        return None
    if not (0x40 <= trail <= 0x7E or 0x80 <= trail <= 0xFC):
        return None
    try:
        character = data[offset : offset + 2].decode(encoding)
    except UnicodeDecodeError:
        return None
    return character if JAPANESE_CHARACTER.fullmatch(character) else None


def candidates(data: bytes, encoding: str, minimum: int) -> list[dict]:
    found: list[dict] = []
    offset = 0
    while offset < len(data):
        if japanese_pair(data, offset, encoding) is None:
            offset += 1
            continue
        start = offset
        characters: list[str] = []
        japanese_count = 0
        while offset < len(data) and offset - start < MAX_CANDIDATE_BYTES:
            character = japanese_pair(data, offset, encoding)
            if character is not None:
                characters.append(character)
                japanese_count += 1
                offset += 2
            elif 0x20 <= data[offset] <= 0x7E:
                characters.append(chr(data[offset]))
                offset += 1
            else:
                break
        if japanese_count >= minimum and japanese_count / len(characters) >= MIN_JAPANESE_RATIO:
            original = data[start:offset]
            found.append({
                "offset": start,
                "original_hex": original.hex(),
                "original": "".join(characters),
                "accepted": False,
                "translation": None,
                "pad_byte": DEFAULT_PAD_BYTE,
            })
        if offset == start:
            offset += 1
    return found


def ascii_candidates(data: bytes) -> list[dict]:
    return [
        {
            "offset": match.start(),
            "original_hex": match.group().hex(),
            "original": match.group().decode("ascii"),
            "accepted": False,
            "translation": None,
            "pad_byte": DEFAULT_PAD_BYTE,
        }
        for match in ASCII_CANDIDATE.finditer(data)
        if len(match.group()) <= MAX_CANDIDATE_BYTES
        and sum(65 <= byte <= 90 for byte in match.group()) >= MIN_JAPANESE_CHARACTERS
    ]


def save_json(path: Path, value: dict) -> None:
    if path.exists():
        raise ValueError(f"Output already exists: {path}")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def load_manifest(path: Path) -> dict:
    manifest = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(manifest, dict) or not isinstance(manifest.get("entries"), list):
        raise ValueError("Invalid candidate manifest")
    return manifest


def scan(args: argparse.Namespace) -> None:
    rom = args.rom.resolve()
    data = rom.read_bytes()
    manifest = {
        "rom": str(rom),
        "sha256": digest(data),
        "encoding": args.encoding,
        "source_language": "ja",
        "entries": candidates(data, args.encoding, args.min_japanese),
    }
    save_json(args.output, manifest)
    print(f"{len(manifest['entries'])} candidates to review: {args.output}")


def scan_ascii(args: argparse.Namespace) -> None:
    rom = args.rom.resolve()
    data = rom.read_bytes()
    manifest = {
        "rom": str(rom),
        "sha256": digest(data),
        "encoding": "ascii",
        "source_language": "en",
        "entries": ascii_candidates(data),
    }
    save_json(args.output, manifest)
    print(f"{len(manifest['entries'])} candidates to review: {args.output}")


def translate(args: argparse.Namespace) -> None:
    manifest = load_manifest(args.manifest)
    selected = [entry for entry in manifest["entries"] if entry.get("accepted") is True]
    if not selected:
        raise ValueError("No accepted entries; review the scan before translating")
    os.environ.setdefault(
        "XDG_CONFIG_HOME",
        str(Path(__file__).resolve().parents[2] / "artifacts" / "pce-translation" / "argos-config"),
    )
    from argostranslate import package
    import ctranslate2

    installed_packages = package.get_installed_packages()
    source_language = manifest.get("source_language", "ja")
    untranslated = list(dict.fromkeys(
        entry["original"] for entry in selected if not entry.get("translation")
    ))

    def translate_batch(texts: list[str], source_code: str, target_code: str) -> list[str]:
        model = next((item for item in installed_packages
                      if item.from_code == source_code and item.to_code == target_code), None)
        if model is None:
            raise ValueError(f"Missing Argos model {source_code} -> {target_code}")
        translator = ctranslate2.Translator(str(model.package_path / "model"))
        results = translator.translate_batch(
            [model.tokenizer.encode(value) for value in texts], beam_size=4
        )
        return [model.tokenizer.decode(result.hypotheses[0]).strip() for result in results]

    if not untranslated or source_language == "fr":
        proposals = untranslated
    elif any(item.from_code == source_language and item.to_code == "fr"
             for item in installed_packages):
        proposals = translate_batch(untranslated, source_language, "fr")
    else:
        intermediate = translate_batch(untranslated, source_language, "en")
        proposals = translate_batch(intermediate, "en", "fr")
    translated = dict(zip(untranslated, proposals))
    for entry in selected:
        if not entry.get("translation"):
            entry["translation"] = translated[entry["original"]]
    save_json(args.output, manifest)
    print(f"{len(selected)} proposals to review: {args.output}")


def build(args: argparse.Namespace) -> None:
    manifest = load_manifest(args.manifest)
    source = Path(manifest["rom"]).resolve()
    output = args.output.resolve()
    if output == source:
        raise ValueError("The output must not overwrite the original ROM")
    original = source.read_bytes()
    if digest(original) != manifest["sha256"]:
        raise ValueError("Source ROM checksum differs from the scan")
    selected = sorted(
        (entry for entry in manifest["entries"] if entry.get("accepted") is True),
        key=lambda entry: entry["offset"],
    )
    if not selected:
        raise ValueError("No accepted entries to patch")
    patched = bytearray(original)
    previous_end = 0
    for entry in selected:
        offset = entry["offset"]
        if not isinstance(offset, int) or offset < previous_end:
            raise ValueError(f"Invalid or overlapping span at {offset}")
        source_bytes = bytes.fromhex(entry["original_hex"])
        end = offset + len(source_bytes)
        if end > len(original):
            raise ValueError(f"Invalid or overlapping span at {offset}")
        if original[offset:end] != source_bytes:
            raise ValueError(f"Original bytes differ at {offset:#x}")
        if not isinstance(entry.get("translation"), str) or not entry["translation"]:
            raise ValueError(f"Missing French text at {offset:#x}")
        replacement = entry["translation"].encode(manifest["encoding"])
        if len(replacement) > len(source_bytes):
            raise ValueError(
                f"French text at {offset:#x} needs {len(replacement)} bytes; "
                f"only {len(source_bytes)} are available"
            )
        pad = bytes.fromhex(entry.get("pad_byte", DEFAULT_PAD_BYTE))
        if len(pad) != 1:
            raise ValueError(f"pad_byte must be one byte at {offset:#x}")
        patched[offset:end] = replacement + pad * (len(source_bytes) - len(replacement))
        previous_end = end
    save_rom(output, bytes(patched))
    print(f"Patched {len(selected)} spans without changing ROM size: {output}")


def save_rom(path: Path, data: bytes) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("xb") as stream:
        stream.write(data)


def main() -> None:
    parser = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter
    )
    commands = parser.add_subparsers(dest="command", required=True)
    scan_command = commands.add_parser("scan", help="Find possible CP932 Japanese strings")
    scan_command.add_argument("rom", type=Path)
    scan_command.add_argument("output", type=Path)
    scan_command.add_argument("--encoding", default=DEFAULT_ENCODING)
    scan_command.add_argument("--min-japanese", type=int, default=MIN_JAPANESE_CHARACTERS)
    scan_command.set_defaults(action=scan)
    ascii_command = commands.add_parser("scan-ascii", help="Find possible Latin-letter strings")
    ascii_command.add_argument("rom", type=Path)
    ascii_command.add_argument("output", type=Path)
    ascii_command.set_defaults(action=scan_ascii)
    translate_command = commands.add_parser("translate", help="Translate accepted text with Argos")
    translate_command.add_argument("manifest", type=Path)
    translate_command.add_argument("output", type=Path)
    translate_command.set_defaults(action=translate)
    build_command = commands.add_parser("build", help="Write a size-preserving ROM copy")
    build_command.add_argument("manifest", type=Path)
    build_command.add_argument("output", type=Path)
    build_command.set_defaults(action=build)
    args = parser.parse_args()
    try:
        args.action(args)
    except (OSError, ValueError, UnicodeError, KeyError, TypeError) as error:
        parser.exit(1, f"Error: {error}\n")


if __name__ == "__main__":
    main()
