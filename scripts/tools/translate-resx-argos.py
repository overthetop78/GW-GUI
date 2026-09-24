#!/usr/bin/env python3
"""Maintain and complete RESX translations with locally installed Argos models."""

from __future__ import annotations

import argparse
import re
from pathlib import Path
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape, quoteattr

from argostranslate import package
import ctranslate2


LANGUAGE_CODES = {
    "ar-SA": "ar", "cs-CZ": "cs", "da-DK": "da", "de-DE": "de", "el-GR": "el",
    "es-ES": "es", "fi-FI": "fi", "fr-FR": "fr", "he-IL": "he", "hu-HU": "hu",
    "id-ID": "id", "it-IT": "it", "ja-JP": "ja", "ko-KR": "ko", "nb-NO": "nb",
    "nl-NL": "nl", "pl-PL": "pl", "pt-BR": "pt", "pt-PT": "pt", "ro-RO": "ro",
    "ru-RU": "ru", "sv-SE": "sv", "th-TH": "th", "tr-TR": "tr", "uk-UA": "uk",
    "vi-VN": "vi", "zh-Hans": "zh", "zh-Hant": "zh",
}
CONTEXTUAL_LABEL_SOURCES = {
    "Visual.StartLabel": "Beginning",
    "Explorer.Session": "Disc session",
    "Explorer.Track": "Media track",
    "Explorer.Layers": "Disc layers",
    "Explorer.Faces": "Media recording sides",
}
CONTEXTUAL_LABEL_OVERRIDES = {
    "fr-FR": {
        "Visual.StartLabel": "Début",
        "Explorer.Session": "Session",
        "Explorer.Track": "Piste",
        "Explorer.Layers": "Couches",
        "Explorer.Faces": "Faces",
    },
}

PLACEHOLDER_PATTERN = re.compile(r"\{[^{}\r\n]+\}")
STRUCTURAL_TOKEN_PATTERN = re.compile(
    r"\{[^{}\r\n]+\}(?:\.{1,3}|[,;:!?…])?|\r\n|\r|\n|\*[^|\s]*|\|"
)
# TODO: Keep the term "Scanline" unchanged in every language while still allowing
# Argos to translate and grammatically reorder the complete surrounding label.
# TODO: Review Classification.Machine, Classification.Format and
# Classification.Protection in every language; a valid translation may be
# spelled exactly like the English source and must not be replaced arbitrarily.
# TODO: Review every Common.Representation.* translation in its technical media
# context, especially Flux, Sectors, Blocks, OpticalTracks and Sequential.
PROTECTED_TOKEN_PATTERN = re.compile(
    STRUCTURAL_TOKEN_PATTERN.pattern + r"|"
    r"(?<![\w.-])[\w-]+\.[A-Za-z0-9]+(?![\w.-])|"
    r"(?<![A-Za-z])[A-Z][A-Z0-9+.-]{1,}(?![A-Za-z])"
)
RESOURCE_ENTRY_BLOCK_PATTERN = re.compile(
    r"(?P<indent>[ \t]*)<(?P<tag>data|resheader)\b[^>]*>.*?</(?P=tag)>[ \t]*(?P<newline>\r?\n)?",
    re.MULTILINE | re.DOTALL,
)
def read_entries(path: Path) -> dict[str, str]:
    root = ET.parse(path).getroot()
    return {
        node.attrib["name"]: node.findtext("value", default="")
        for node in root.findall("data")
    }


def read_catalogs(root: Path, pattern: str = "*.resx") -> dict[str, dict[str, str]]:
    return {
        path.relative_to(root).as_posix(): read_entries(path)
        for path in sorted(root.rglob(pattern))
    }


def translate_preserving_placeholders(
    texts: list[str], tokenizer, translator: ctranslate2.Translator,
    force_context: bool = False,
) -> list[str]:
    encoded_segments: list[list[tuple[str, int | str]]] = []
    source_segments: list[str] = []
    whitespace: list[tuple[str, str]] = []
    for text in texts:
        encoded_text: list[tuple[str, int | str]] = []
        for part in re.split(f"({PROTECTED_TOKEN_PATTERN.pattern})", text):
            if not part:
                continue
            if PROTECTED_TOKEN_PATTERN.fullmatch(part):
                encoded_text.append(("literal", part))
                continue
            leading = part[:len(part) - len(part.lstrip())]
            trailing = part[len(part.rstrip()):]
            core = part.strip()
            if not core:
                encoded_text.append(("literal", part))
                continue
            index = len(source_segments)
            source_segments.append(core)
            whitespace.append((leading, trailing))
            encoded_text.append(("translated", index))
        encoded_segments.append(encoded_text)

    source_tokens = [tokenizer.encode(text) for text in source_segments]
    results = translator.translate_batch(source_tokens, beam_size=1) if source_tokens else []
    translated_segments = [tokenizer.decode(result.hypotheses[0]).strip() for result in results]
    contextual_indexes = [
        index for index, (source, translated) in enumerate(zip(source_segments, translated_segments))
        if force_context or source == translated
    ]
    if contextual_indexes:
        contextual_tokens = [
            tokenizer.encode(f"Interface label: {source_segments[index]}")
            for index in contextual_indexes
        ]
        contextual_results = translator.translate_batch(contextual_tokens, beam_size=1)
        for index, result in zip(contextual_indexes, contextual_results):
            contextual = tokenizer.decode(result.hypotheses[0]).strip()
            if ":" not in contextual:
                continue
            candidate = contextual.split(":", 1)[1].strip()
            if candidate:
                translated_segments[index] = candidate
    translated_texts: list[str] = []
    for encoded_text in encoded_segments:
        parts: list[str] = []
        for kind, value in encoded_text:
            if kind == "literal":
                parts.append(str(value))
            else:
                index = int(value)
                leading, trailing = whitespace[index]
                parts.append(leading + translated_segments[index] + trailing)
        translated_texts.append("".join(parts))
    return translated_texts


def translate_entries(
    entries: list[tuple[str, str]], tokenizer, translator: ctranslate2.Translator,
    culture: str | None = None,
    force_context: bool = False,
) -> list[str]:
    sources = [CONTEXTUAL_LABEL_SOURCES.get(key, english) for key, english in entries]
    translated = translate_preserving_placeholders(
        sources,
        tokenizer,
        translator,
        force_context=force_context,
    )
    overrides = CONTEXTUAL_LABEL_OVERRIDES.get(culture or "", {})
    return [overrides.get(key, value) for (key, _), value in zip(entries, translated)]


def remove_entries_not_in_source(path: Path, source_entries: dict[str, str]) -> int:
    if not path.exists():
        return 0
    text = path.read_text(encoding="utf-8")
    target_entries = read_entries(path)
    removed = 0
    for key in target_entries:
        if key in source_entries:
            continue
        pattern = re.compile(
            rf'^[ \t]*<data\s+name="{re.escape(escape(key))}"[^>]*>.*?</data>[ \t]*(?:\r?\n)?',
            re.MULTILINE | re.DOTALL,
        )
        text, count = pattern.subn("", text, count=1)
        removed += count
    if removed:
        path.write_text(text, encoding="utf-8", newline="")
    return removed


def remove_duplicate_keys(path: Path) -> int:
    root = ET.parse(path).getroot()
    counts: dict[str, int] = {}
    for node in root.findall("data"):
        key = node.attrib["name"]
        counts[key] = counts.get(key, 0) + 1
    duplicate_keys = [key for key, count in counts.items() if count > 1]
    if not duplicate_keys:
        return 0
    text = path.read_text(encoding="utf-8")
    removed = 0
    for key in duplicate_keys:
        pattern = re.compile(
            rf'^[ \t]*<data\s+name="{re.escape(escape(key))}"[^>]*>.*?</data>[ \t]*(?:\r?\n)?',
            re.MULTILINE | re.DOTALL,
        )
        matches = list(pattern.finditer(text))
        for match in reversed(matches[:-1]):
            text = text[:match.start()] + text[match.end():]
            removed += 1
    path.write_text(text, encoding="utf-8", newline="")
    return removed


def placeholder_signature(value: str) -> tuple[str, ...]:
    return tuple(sorted(set(PLACEHOLDER_PATTERN.findall(value))))


def protected_signature(value: str) -> tuple[str, ...]:
    return tuple(STRUCTURAL_TOKEN_PATTERN.findall(value))


def contains_untranslated_english_run(english: str, translated: str) -> bool:
    """Detect a meaningful English word sequence left inside a translation."""
    english = PROTECTED_TOKEN_PATTERN.sub(" ", english)
    translated = PROTECTED_TOKEN_PATTERN.sub(" ", translated)
    source_words = [
        word.lower()
        for word in re.findall(r"[A-Za-z][A-Za-z'-]*", english)
    ]
    if len(source_words) < 4:
        return False
    target_words = [
        word.lower() for word in re.findall(r"[A-Za-z][A-Za-z'-]*", translated)
    ]
    source_runs = {
        tuple(source_words[index:index + 4])
        for index in range(len(source_words) - 3)
        if sum(len(word) for word in source_words[index:index + 4]) >= 18
    }
    return any(
        tuple(target_words[index:index + 4]) in source_runs
        for index in range(len(target_words) - 3)
    )


def encode_element_text(value: str) -> str:
    return (
        escape(value)
        .replace("\r\n", "&#xA;")
        .replace("\r", "&#xA;")
        .replace("\n", "&#xA;")
    )


def format_resx_data_entries(path: Path) -> int:
    """Put every RESX resource element on one physical line without changing its value."""
    text = path.read_text(encoding="utf-8")
    changed = 0

    def replace(match: re.Match[str]) -> str:
        nonlocal changed
        node = ET.fromstring(match.group(0).strip())
        tag = node.tag
        value_node = node.find("value")
        value = "" if value_node is None or value_node.text is None else value_node.text
        serialized_attributes: list[str] = []
        for name, attribute_value in node.attrib.items():
            if name == "{http://www.w3.org/XML/1998/namespace}space":
                name = "xml:space"
            serialized_attributes.append(f"{name}={quoteattr(attribute_value)}")
        attributes = " " + " ".join(serialized_attributes) if serialized_attributes else ""
        replacement = (
            f"{match.group('indent')}<{tag}{attributes}><value>{encode_element_text(value)}</value>"
            f"</{tag}>{match.group('newline')}"
        )
        if replacement != match.group(0):
            changed += 1
        return replacement

    formatted = RESOURCE_ENTRY_BLOCK_PATTERN.sub(replace, text)
    formatted = re.sub(r"(?m)^[ \t]*\r?\n", "", formatted)
    if formatted != text:
        path.write_text(formatted, encoding="utf-8", newline="")
    return changed


def audit_resources(root: Path) -> None:
    base_catalogs = read_catalogs(root / "00-Base")
    english_catalogs = read_catalogs(root / "en-US")
    errors: list[str] = []
    cultures = sorted(
        path
        for path in root.iterdir()
        if path.is_dir() and path.name not in {"00-Base", "en-US"}
    )

    for catalog, english_entries in english_catalogs.items():
        base_entries = base_catalogs.get(catalog)
        if base_entries is None:
            errors.append(f"en-US/{catalog}: catalog is missing from 00-Base")
            continue
        for key in english_entries.keys() - base_entries.keys():
            errors.append(f"en-US/{catalog}: unknown {key}")

    for culture_path in cultures:
        target_catalogs = read_catalogs(culture_path)
        for catalog, english_entries in english_catalogs.items():
            target_path = culture_path / Path(catalog)
            if not target_path.exists():
                errors.append(f"{culture_path.name}/{catalog}: missing catalog")
                continue
            nodes = ET.parse(target_path).getroot().findall("data")
            keys = [node.attrib["name"] for node in nodes]
            duplicates = sorted({key for key in keys if keys.count(key) > 1})
            if duplicates:
                errors.append(f"{culture_path.name}/{catalog}: duplicate {', '.join(duplicates)}")
            target_entries = read_entries(target_path)
            for key, english in english_entries.items():
                if key not in target_entries:
                    errors.append(f"{culture_path.name}/{catalog}: missing {key}")
                    continue
                if placeholder_signature(english) != placeholder_signature(target_entries[key]):
                    errors.append(f"{culture_path.name}/{catalog}: placeholders differ for {key}")
                elif protected_signature(english) != protected_signature(target_entries[key]):
                    errors.append(f"{culture_path.name}/{catalog}: protected tokens differ for {key}")
                elif contains_untranslated_english_run(english, target_entries[key]):
                    errors.append(f"{culture_path.name}/{catalog}: partially untranslated {key}")
            for key in target_entries.keys() - english_entries.keys():
                errors.append(f"{culture_path.name}/{catalog}: unknown {key}")
            physical_text = target_path.read_text(encoding="utf-8")
            for match in RESOURCE_ENTRY_BLOCK_PATTERN.finditer(physical_text):
                if "\n" in match.group(0).rstrip("\r\n"):
                    errors.append(
                        f"{culture_path.name}/{catalog}: data entry spans multiple physical lines"
                    )
                    break
        for catalog in target_catalogs.keys() - english_catalogs.keys():
            errors.append(f"{culture_path.name}/{catalog}: catalog is not translatable")
    if errors:
        raise RuntimeError("RESX audit failed:\n" + "\n".join(errors))
    translated = sum(
        len(read_entries(culture_path / Path(catalog)))
        for culture_path in cultures
        for catalog in english_catalogs
    )
    print(
        f"RESX audit passed: {len(cultures)} cultures, {len(english_catalogs)} catalogs, "
        f"{translated} localized entries"
    )


def insert(path: Path, key: str, value: str, replace_existing: bool = False) -> None:
    text = path.read_text(encoding="utf-8")
    encoded_key = escape(key)
    encoded_value = encode_element_text(value)
    pattern = re.compile(
        rf'^[ \t]*<data name="{re.escape(encoded_key)}"[^>]*><value>.*?</value></data>',
        re.MULTILINE | re.DOTALL,
    )
    if pattern.search(text):
        if replace_existing:
            replacement = f'  <data name="{encoded_key}"><value>{encoded_value}</value></data>'
            path.write_text(
                pattern.sub(lambda _: replacement, text, count=1),
                encoding="utf-8",
                newline="",
            )
        return
    newline = "\r\n" if "\r\n" in text else "\n"
    entry = f'  <data name="{encoded_key}"><value>{encoded_value}</value></data>{newline}'
    marker = f"</root>{newline}" if text.endswith(f"</root>{newline}") else "</root>"
    path.write_text(text.replace(marker, entry + marker, 1), encoding="utf-8", newline="")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("resource", nargs="?")
    parser.add_argument("key", nargs="?")
    parser.add_argument("english", nargs="?")
    parser.add_argument("--entry", nargs=2, action="append", default=[],
        metavar=("KEY", "ENGLISH"), help="add another key in the same model-loading pass")
    parser.add_argument("--replace", action="store_true",
        help="replace the value when the key already exists")
    parser.add_argument("--sync-all", action="store_true",
        help="translate every missing or untranslated entry in every RESX catalog")
    parser.add_argument("--culture", choices=[*LANGUAGE_CODES],
        help="limit --sync-all to one culture")
    parser.add_argument("--catalog",
        help="limit --sync-all to one RESX catalog, for example Visualizer.resx")
    parser.add_argument("--clean-only", action="store_true",
        help="remove duplicate keys and localized entries absent from en-US")
    parser.add_argument("--audit", action="store_true",
        help="validate catalogs, translatable keys and format placeholders")
    parser.add_argument("--repair-mixed", action="store_true",
        help="retranslate entries that still contain a substantial English fragment")
    parser.add_argument("--format", action="store_true",
        help="put every RESX data/value translation on one physical XML line")
    parser.add_argument("--root", type=Path, default=Path("src/GWGUI.App/Resources"),
        help="RESX root containing 00-Base and culture directories (application or module)")
    args = parser.parse_args()
    root = args.root

    if args.audit:
        audit_resources(root)
        return

    if args.format:
        changed = sum(format_resx_data_entries(path) for path in root.rglob("*.resx"))
        print(f"RESX entries normalized: {changed}")
        return

    if args.clean_only:
        english_catalogs = read_catalogs(root / "en-US")
        duplicates = 0
        removed = 0
        for culture_path in root.iterdir():
            if not culture_path.is_dir() or culture_path.name in {"00-Base", "en-US"}:
                continue
            for target_path in culture_path.rglob("*.resx"):
                duplicates += remove_duplicate_keys(target_path)
                catalog = target_path.relative_to(culture_path).as_posix()
                removed += remove_entries_not_in_source(
                    target_path, english_catalogs.get(catalog, {})
                )
        print(f"Duplicate entries removed: {duplicates}")
        print(f"Entries absent from en-US removed: {removed}")
        return

    if args.repair_mixed:
        english_catalogs = read_catalogs(root / "en-US")
        packages = {(item.from_code, item.to_code): item
            for item in package.get_installed_packages() if item.type == "translate"}
        total = 0
        for culture, language_code in LANGUAGE_CODES.items():
            installed_package = packages.get(("en", language_code))
            if installed_package is None:
                raise RuntimeError(f"Missing Argos model en -> {language_code}")
            pending: list[tuple[Path, str, str]] = []
            for catalog, english_entries in english_catalogs.items():
                target_path = root / culture / Path(catalog)
                target_entries = read_entries(target_path)
                for key, english in english_entries.items():
                    current = target_entries.get(key)
                    if (
                        current is not None
                        and (
                            contains_untranslated_english_run(english, current)
                            or protected_signature(english) != protected_signature(current)
                        )
                    ):
                        pending.append((target_path, key, english))
            if not pending:
                print(f"{culture}: repaired=0", flush=True)
                continue
            translator = ctranslate2.Translator(str(installed_package.package_path / "model"))
            translated_values = translate_entries(
                [(key, english) for _, key, english in pending],
                installed_package.tokenizer,
                translator,
                culture=culture,
                force_context=True,
            )
            for (target_path, key, _), value in zip(pending, translated_values):
                insert(target_path, key, value, replace_existing=True)
            total += len(pending)
            print(f"{culture}: repaired={len(pending)}", flush=True)
        print(f"Partially untranslated entries repaired: {total}")
        return

    if args.sync_all:
        selected_cultures = (
            {args.culture: LANGUAGE_CODES[args.culture]}
            if args.culture in LANGUAGE_CODES
            else LANGUAGE_CODES
        )
        selected_directories = (
            [root / args.culture]
            if args.culture is not None
            else [
                path
                for path in root.iterdir()
                if path.is_dir() and path.name not in {"00-Base", "en-US"}
            ]
        )
        catalog_pattern = args.catalog or "*.resx"
        duplicate_count = sum(
            remove_duplicate_keys(path)
            for culture_path in selected_directories
            for path in culture_path.rglob(catalog_pattern)
        )
        if duplicate_count:
            print(f"Duplicate entries removed: {duplicate_count}", flush=True)
        english_catalogs = {
            catalog: entries
            for catalog, entries in read_catalogs(root / "en-US").items()
            if Path(catalog).match(catalog_pattern)
        }
        packages = {(item.from_code, item.to_code): item
            for item in package.get_installed_packages() if item.type == "translate"}
        for culture, language_code in selected_cultures.items():
            installed_package = packages.get(("en", language_code))
            if installed_package is None:
                raise RuntimeError(f"Missing Argos model en -> {language_code}")
            translator = ctranslate2.Translator(str(installed_package.package_path / "model"))
            tokenizer = installed_package.tokenizer
            pending: list[tuple[Path, str, str]] = []
            for catalog, english_entries in english_catalogs.items():
                target_path = root / culture / Path(catalog)
                target_entries = read_entries(target_path)
                for key, english in english_entries.items():
                    current = target_entries.get(key)
                    if current is None or current == english:
                        pending.append((target_path, key, english))

            translated_values = translate_entries(
                [(key, english) for _, key, english in pending], tokenizer, translator, culture=culture
            )
            updates_by_path: dict[Path, list[tuple[str, str]]] = {}
            for (target_path, key, _), value in zip(pending, translated_values):
                updates_by_path.setdefault(target_path, []).append((key, value))
            for target_path, updates in updates_by_path.items():
                for key, value in updates:
                    insert(target_path, key, value, replace_existing=True)

            removed = 0
            culture_root = root / culture
            for target_path in culture_root.rglob(catalog_pattern):
                catalog = target_path.relative_to(culture_root).as_posix()
                removed += remove_entries_not_in_source(
                    target_path, english_catalogs.get(catalog, {})
                )
            print(f"{culture}: translated={len(pending)}, obsolete entries removed={removed}", flush=True)
        return

    if not args.resource or not args.key or args.english is None:
        parser.error("resource, key and english are required unless --sync-all is used")
    entries = [(args.key, args.english), *[tuple(entry) for entry in args.entry]]
    for key, english in entries:
        insert(root / "00-Base" / args.resource, key, english, args.replace)
        insert(root / "en-US" / args.resource, key, english, args.replace)
    translatable_entries = entries
    english_entries = read_entries(root / "en-US" / args.resource)
    packages = {(item.from_code, item.to_code): item
        for item in package.get_installed_packages() if item.type == "translate"}
    for culture, language_code in LANGUAGE_CODES.items():
        installed_package = packages.get(("en", language_code))
        if installed_package is None:
            raise RuntimeError(f"Missing Argos model en -> {language_code}")
        translator = ctranslate2.Translator(str(installed_package.package_path / "model"))
        tokenizer = installed_package.tokenizer
        translated_values = translate_entries(
            translatable_entries, tokenizer, translator, culture=culture
        )
        for (key, _), value in zip(translatable_entries, translated_values):
            insert(root / culture / args.resource, key, value, args.replace)
        remove_entries_not_in_source(
            root / culture / args.resource, english_entries
        )
        print(culture, flush=True)


if __name__ == "__main__":
    main()
