#!/usr/bin/env python3
"""Maintain and complete RESX translations with locally installed Argos models."""

from __future__ import annotations

import argparse
import ctypes
from collections import Counter
from contextlib import contextmanager
from functools import lru_cache
import re
from pathlib import Path
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape, quoteattr

from argostranslate import package
import ctranslate2


MAX_TRANSLATION_BATCH_SIZE = 32
TRANSLATION_CPU_THREADS = 4

LANGUAGE_CODES = {
    "ar-SA": "ar", "cs-CZ": "cs", "da-DK": "da", "de-DE": "de", "el-GR": "el",
    "es-ES": "es", "fi-FI": "fi", "fr-FR": "fr", "he-IL": "he", "hu-HU": "hu",
    "id-ID": "id", "it-IT": "it", "ja-JP": "ja", "ko-KR": "ko", "nb-NO": "nb",
    "nl-NL": "nl", "pl-PL": "pl", "pt-BR": "pt", "pt-PT": "pt", "ro-RO": "ro",
    "ru-RU": "ru", "sv-SE": "sv", "th-TH": "th", "tr-TR": "tr", "uk-UA": "uk",
    "vi-VN": "vi", "zh-Hans": "zh", "zh-Hant": "zh",
}
PLACEHOLDER_PATTERN = re.compile(r"\{[^{}\r\n]+\}")
STRUCTURAL_TOKEN_PATTERN = re.compile(
    r"\{[^{}\r\n]+\}|\r\n|\r|\n|\*\.(?:\*|[A-Za-z0-9]+)|\*|\|"
)
PROTECTED_TOKEN_SOURCE = (
    STRUCTURAL_TOKEN_PATTERN.pattern + r"|"
    r"(?<![\w-])GW GUI(?![\w-])|"
    r"\$[0-9A-Fa-f]+|"
    r"\d+(?:[.,]\d+)?\s*(?:Hz|kHz|MHz|GHz|KB|MB|GB)|"
    r"(?<![\w-])(?:[A-Z][A-Za-z0-9-]*\s+)+[IVX]+(?![\w-])|"
    r"(?<![\w.-])[\w-]+\.[A-Za-z0-9]+(?![\w.-])|"
    r"(?<!\w)\.[A-Za-z][A-Za-z0-9]{1,7}\b|(?<!\w)_\d+(?!\w)|"
    r"(?<![A-Za-z])[A-Z][A-Z0-9]+(?:[+.-][A-Z0-9]+)*(?:(?=s\b)|(?![A-Za-z\u00C0-\u024F\u1E00-\u1EFF]))"
)


def protected_token_pattern(root: Path) -> re.Pattern[str]:
    base = root / "00-Base"
    names: set[str] = set()
    family_names: set[str] = set()
    emulator_ids: set[str] = set()
    values: list[str] = []
    manufacturer = (
        root.parent.name.rsplit(".", 1)[-1]
        if root.parent.name.startswith("GWGUI.Emulation.") else None
    )
    if manufacturer:
        names.add(manufacturer)
    names.update(INVARIANT_OPTION_NAMES)
    names.update(INVARIANT_CONTROLLER_NAMES)
    names.add("PlayStation Move")
    names.update({"neGcon", "NeGcon", "MMap", "GunCon", "Konami Gun",
        "Hyper Blaster", "Justifier", "DualShock", "FastMAD", "Doom", "Hexen",
        "Soul Blade", "Pro Pinball", "Saga Frontier", "RetroArch",
        "DualSense", "DualSense Edge", "Dual Analog", "Emotion Engine",
        "Weave", "Bob", "Bob (Offset)"})
    for path in base.rglob("*.resx"):
        for key, value in read_entries(path).items():
            values.append(value)
            if key.startswith("Emulation.Family.") and value:
                family_names.add(value)
            if key.startswith("Emulation.Emulator."):
                emulator_ids.add(key.split(".")[2])
                if re.fullmatch(r"Emulation\.Emulator\.[^.]+", key) and value:
                    names.add(value)
            if path.stem in {"Model", "Machine"} or (
                ".Model." in key and ".Help." not in key
            ):
                for part in value.split(" / "):
                    name = part.strip()
                    if name:
                        names.add(name)
    for emulator_id in emulator_ids:
        pattern = re.compile(r"(?<![\w-])" + re.escape(emulator_id) + r"(?![\w-])", re.IGNORECASE)
        names.update(match.group(0) for value in values for match in pattern.finditer(value))
    if manufacturer:
        names.update(name[len(manufacturer) + 1:] for name in tuple(names)
                     if name.startswith(manufacturer + " "))
    for name in tuple(names):
        for family in family_names:
            if name.startswith(family + " "):
                names.add(name[len(family) + 1:])
    names.update(family_names)
    if not names:
        return re.compile(PROTECTED_TOKEN_SOURCE)

    invariant_pattern = r"(?<!\w)(?:" + "|".join(
        re.escape(name) for name in sorted(names, key=len, reverse=True)
    ) + r")(?!\w)"
    # Texture pack folders and configuration paths are literal core inputs.
    path_pattern = r"(?:<[A-Za-z]+>|[A-Za-z0-9_-]+)(?:/[A-Za-z0-9_.-]+)+/(?![A-Za-z0-9_.-])|<[A-Za-z]+>-texture-[A-Za-z-]+/?"
    literal_paths = {match.group(0) for value in values
        for match in re.finditer(path_pattern, value)}
    protected_paths = "|".join(re.escape(path)
        for path in sorted(literal_paths, key=len, reverse=True))
    return re.compile((protected_paths + r"|" if protected_paths else "")
        + invariant_pattern + r"|" + PROTECTED_TOKEN_SOURCE)


PROTECTED_TOKEN_PATTERN = re.compile(PROTECTED_TOKEN_SOURCE)
RESOURCE_ENTRY_BLOCK_PATTERN = re.compile(
    r"(?P<indent>[ \t]*)<(?P<tag>data|resheader)\b[^>]*>.*?</(?P=tag)>[ \t]*(?P<newline>\r?\n)?",
    re.MULTILINE | re.DOTALL,
)

# These are identifiers and named algorithms, not ordinary interface states.
INVARIANT_OPTION_NAMES = frozenset({
    "Weave", "Bob", "FastMAD", "RetroArch", "neGcon", "NeGcon", "GunCon",
    "PAL", "NTSC", "CPU", "GPU", "RAM", "ROM", "CRT", "LCD", "HDMI", "VGA",
    "Vulkan", "GLideN64", "gln64", "Angrylion", "cxd4", "Bayer", "B-Spline",
    "Catmull-Rom", "Mitchell-Netravali", "ScaleForce", "Anime4K Ultrafast",
    "Dynarec", "jit", "hli", "Sinc", "HuC1", "HuC3", "Bung/EMS", "Hitek",
    "Li Cheng", "Sachen MMC1", "Sachen MMC2", "MemPak", "SummerCart64", "64DD IPL",
    "Dendy", "Nintendo DS", "DSi", "New 3DS", "gba sp", "CANDYPOP!",
    "2Bit DEMICHROME", "Andrade Gameboy", "Kirokaze Gameboy", "Lospec GB",
    "HoneyGB", "SpaceHaze", "RGR-Papercut4", "T-Lollipop", "GB MŒBIUS",
    "Sony CXA2025AS", "Composite Direct FBX", "Magnum FBX", "Smooth V2 FBX",
})
INVARIANT_OPTION_PATTERN = re.compile(
    r"(?:[+-]?\d+(?:[.,]\d+)?\s*(?:%|[kKmMgG]?Hz|[KMGT]?i?B)|"
    r"\d+-bit|0x[0-9A-Fa-f]+|0rgb1555|rgb565|\d+x (?:MSAA|SSAA)|"
    r"\d+:\d+ (?:\(DAR\)|PAR)|(?:[2-6])?xBRZ|xBRZ freescale|Bisqwit [248]x|"
    r"(?:AltWFC|Kaeru WFC) \([\d.]+\)|EEPROM \(\d+kB\)|"
    r"CXA2025AS \((?:JP|US)\)|(?:Super )?Game Boy(?: Color| Advance| DMG| 2)?"
    r"(?: #\d+| (?:NTSC|PAL)| \((?:DMG-CPU B|CPU AGB A|CPU CGB [0A-E]|SGB[12]\.sfc)\))?|"
    r"Super Game Boy2|Super Game Boy Color|[\w-]+\.(?:rom|bin|bmp|wav|sfc)/?)",
    re.IGNORECASE,
)
INVARIANT_CONTROLLER_NAMES = frozenset({
    "PlayStation Move Sharp Shooter",
    "Konami Justifier / Hyper Blaster",
    "Namco GunCon / G-Con 45",
    "Namco GunCon 2",
    "Namco GunCon 3",
})
SONY_CONTROLLER_BUTTON_PATTERN = re.compile(
    r"(?:L[123]|R[123]|Start|Select)"
    r"(?:\s*\+\s*(?:L[123]|R[123]|Start|Select))*"
)


def is_invariant_entry(key: str, value: str) -> bool:
    if key.startswith("Emulation.Sony.Input.Key."):
        return re.fullmatch(r"[A-Z0-9]|F\d+|[^\w\s]", value) is not None
    if re.fullmatch(r"Emulation\.Family\.[^.]+", key):
        return True
    if re.fullmatch(r"Emulation\.Emulator\.[^.]+", key):
        return True
    if re.fullmatch(r"Emulation\.[^.]+\.(?:Model|Machine)\.[^.]+", key):
        return True
    if re.fullmatch(r"Emulation\.[^.]+\.Firmware\..+", key):
        return re.fullmatch(r"[A-Z][A-Z0-9_]*|OpenBIOS|[\w-]+\.(?:rom|bin|PUP)", value) is not None
    if ".Controller." in key and value in INVARIANT_CONTROLLER_NAMES:
        return True
    if ".Value." not in key:
        return False
    # Printed Start/Select and L/R button names are invariant; symbol names and directions are translated.
    if key.startswith("Emulation.Option.Sony.Value.") and SONY_CONTROLLER_BUTTON_PATTERN.fullmatch(value):
        return True
    # Numbered palette names identify presets; palette descriptions remain translatable.
    if re.match(r"(?:TWB64|PixelShift) \d+ - ", value):
        return True
    return value in INVARIANT_OPTION_NAMES or INVARIANT_OPTION_PATTERN.fullmatch(value) is not None


def remove_invariant_copies(root: Path, pattern: str = "*.resx") -> int:
    removed = 0
    for base_path in (root / "00-Base").rglob(pattern):
        keys = {key for key, value in read_entries(base_path).items()
                if is_invariant_entry(key, value)}
        if not keys:
            continue
        relative = base_path.relative_to(root / "00-Base")
        for culture_path in root.iterdir():
            path = culture_path / relative
            if not culture_path.is_dir() or culture_path.name == "00-Base" or not path.exists():
                continue
            with path.open(encoding="utf-8", newline="") as source:
                text = source.read()

            def remove(match: re.Match[str]) -> str:
                nonlocal removed
                if match.group("tag") == "data" and ET.fromstring(match.group(0).strip()).attrib["name"] in keys:
                    removed += 1
                    return ""
                return match.group(0)

            corrected = RESOURCE_ENTRY_BLOCK_PATTERN.sub(remove, text)
            if corrected != text:
                ET.fromstring(corrected)
                path.write_text(corrected, encoding="utf-8", newline="")
    return removed
def read_entries(path: Path) -> dict[str, str]:
    root = ET.parse(path).getroot()
    return {
        node.attrib["name"]: node.findtext("value", default="")
        for node in root.findall("data")
    }


def create_empty_catalog(path: Path, source: Path) -> None:
    if path.exists():
        return
    template = RESOURCE_ENTRY_BLOCK_PATTERN.sub(
        lambda match: "" if match.group("tag") == "data" else match.group(0),
        source.read_text(encoding="utf-8"),
    )
    ET.fromstring(template)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(template, encoding="utf-8", newline="")


def read_catalogs(root: Path, pattern: str = "*.resx") -> dict[str, dict[str, str]]:
    catalogs = {
        path.relative_to(root).as_posix(): read_entries(path)
        for path in sorted(root.rglob(pattern))
    }
    if root.name == "en-US":
        for catalog, entries in catalogs.items():
            base_path = root.parent / "00-Base" / catalog
            base_entries = read_entries(base_path) if base_path.exists() else {}
            catalogs[catalog] = {
                key: value for key, value in entries.items()
                if not is_invariant_entry(key, base_entries.get(key, value))
            }
    return catalogs


def normalize_acronym_plurals(source: str, candidate: str) -> str:
    for acronym in set(re.findall(r"(?<![A-Za-z])([A-Z][A-Z0-9]+)s\b", source)):
        candidate = re.sub(rf"(?<![A-Za-z]){re.escape(acronym)}S(?![A-Za-z])",
            acronym + "s", candidate)
    return candidate


def protected_tokens_preserved(source: str, candidate: str) -> bool:
    candidate = normalize_acronym_plurals(source, candidate)
    # CJK particles may immediately follow a Latin name: regex word boundaries
    # on the translated text would incorrectly treat that name as missing.
    source_tokens = Counter(PROTECTED_TOKEN_PATTERN.findall(source))
    if not source_tokens:
        return True
    # Use the same token definitions on both sides. Here every \w occurs
    # inside a character class; limiting those classes to Latin identifiers
    # allows CJK particles beside names without counting CPU inside CPUs.
    target_pattern = re.compile(PROTECTED_TOKEN_PATTERN.pattern.replace(r"\w", "A-Za-z0-9_"))
    target_tokens = Counter(target_pattern.findall(candidate))
    return all(target_tokens[token] == count for token, count in source_tokens.items())


def translate_around_protected_tokens(
    source: str, tokenizer, translator: ctranslate2.Translator,
    sentence_case: bool = False,
) -> str:
    parts: list[str] = []
    fragments: list[str] = []
    positions: list[int] = []

    def add_fragment(fragment: str) -> None:
        if not any(character.isalnum() for character in fragment):
            parts.append(fragment)
            return
        leading = re.match(r"[^\w]*", fragment)[0]
        trailing = re.search(r"[^\w]*$", fragment)[0]
        parts.append(leading)
        positions.append(len(parts))
        parts.append("")
        parts.append(trailing)
        text = fragment[len(leading):len(fragment) - len(trailing) if trailing else None]
        if sentence_case:
            text = re.sub(r"\bdeadzone\b", "dead zone", text, flags=re.IGNORECASE)
            text = text.lower()
            text = text[:1].upper() + text[1:]
        fragments.append(text)

    position = 0
    for match in PROTECTED_TOKEN_PATTERN.finditer(source):
        add_fragment(source[position:match.start()])
        parts.append(match.group(0))
        position = match.end()
    add_fragment(source[position:])

    if fragments:
        results = translator.translate_batch(
            [tokenizer.encode(fragment) for fragment in fragments],
            max_batch_size=MAX_TRANSLATION_BATCH_SIZE, beam_size=4, repetition_penalty=1.2, no_repeat_ngram_size=3,
        )
        for index, result in zip(positions, results):
            parts[index] = tokenizer.decode(result.hypotheses[0]).strip()
    return "".join(parts)


def translate_preserving_placeholders(
    texts: list[str], tokenizer, translator: ctranslate2.Translator,
    force_context: bool = False,
) -> list[str]:
    encoded_segments: list[list[tuple[str, int | str]]] = []
    source_segments: list[str] = []
    whitespace: list[tuple[str, str]] = []
    for text in texts:
        encoded_text: list[tuple[str, int | str]] = []
        # A technical name must not split a sentence into unrelated fragments.
        # File-filter separators, line breaks and complete sentences delimit labels.
        for part in re.split(r"(\r\n|\r|\n|\||(?<=\.)\s+(?=[A-Z]))", text):
            if not part:
                continue
            remaining = PROTECTED_TOKEN_PATTERN.sub("", part)
            if not any(character.isalnum() for character in remaining):
                encoded_text.append(("literal", part))
                continue
            leading = part[:len(part) - len(part.lstrip())]
            trailing = part[len(part.rstrip()):]
            core = part.strip()
            if not core:
                encoded_text.append(("literal", part))
                continue
            # Do not send punctuation-only fragments to Argos.  Translating a
            # comma, separator, or parenthesis as an isolated sentence makes
            # several language models emit duplicated punctuation or unrelated
            # words.  Keep those fragments exactly as they appear in en-US;
            # only actual text should be translated.
            if not any(character.isalnum() for character in core):
                encoded_text.append(("literal", part))
                continue
            index = len(source_segments)
            source_segments.append(core)
            whitespace.append((leading, trailing))
            encoded_text.append(("translated", index))
        encoded_segments.append(encoded_text)

    source_tokens = [tokenizer.encode(text) for text in source_segments]
    results = translator.translate_batch(
        source_tokens, max_batch_size=MAX_TRANSLATION_BATCH_SIZE, beam_size=4, repetition_penalty=1.2, no_repeat_ngram_size=3
    ) if source_tokens else []
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
        contextual_results = translator.translate_batch(
            contextual_tokens, max_batch_size=MAX_TRANSLATION_BATCH_SIZE, beam_size=4, repetition_penalty=1.2, no_repeat_ngram_size=3
        )
        for index, result in zip(contextual_indexes, contextual_results):
            contextual = tokenizer.decode(result.hypotheses[0]).strip()
            if ":" not in contextual:
                continue
            candidate = contextual.split(":", 1)[1].strip()
            current = translated_segments[index]
            source = source_segments[index]
            if candidate and (
                not contains_untranslated_english_run(source, candidate)
                or contains_untranslated_english_run(source, current)
            ):
                translated_segments[index] = candidate
    for index, (source, translated) in enumerate(zip(source_segments, translated_segments)):
        if source != translated or not PROTECTED_TOKEN_PATTERN.search(source):
            continue
        candidate = translate_around_protected_tokens(source, tokenizer, translator)
        if candidate != source and protected_tokens_preserved(source, candidate):
            translated_segments[index] = candidate
    # Translate the whole sentence again with reversible markers if a model
    # changed a proper name, a hexadecimal value or a formatting parameter.
    masked_indexes: list[int] = []
    masked_sources: list[str] = []
    marker_maps: list[dict[str, str]] = []
    for index, (source, translated) in enumerate(zip(source_segments, translated_segments)):
        if protected_tokens_preserved(source, translated):
            continue
        markers: dict[str, str] = {}

        def mask(match: re.Match[str]) -> str:
            marker = f"987650{len(markers)}09876"
            markers[marker] = match.group(0)
            return marker

        masked_indexes.append(index)
        masked_sources.append(PROTECTED_TOKEN_PATTERN.sub(mask, source))
        marker_maps.append(markers)
    if masked_sources:
        masked_results = translator.translate_batch(
            [tokenizer.encode(source) for source in masked_sources],
            max_batch_size=MAX_TRANSLATION_BATCH_SIZE, beam_size=4, repetition_penalty=1.2, no_repeat_ngram_size=3,
        )
        for index, result, markers in zip(masked_indexes, masked_results, marker_maps):
            candidate = tokenizer.decode(result.hypotheses[0]).strip()
            restored = False
            for attempt in range(3):
                restored_candidate = candidate
                counts = []
                for marker, literal in markers.items():
                    marker_pattern = r"\s*".join(re.escape(character) for character in marker)
                    if marker.startswith("["):
                        digits = r"\s*".join(marker[1:-1])
                        marker_pattern = r"[\[(]?\s*" + digits + r"\s*[\])]?"
                    restored_candidate, count = re.subn(marker_pattern, lambda _: literal, restored_candidate, flags=re.IGNORECASE)
                    counts.append(count)
                if all(count == 1 for count in counts) and protected_tokens_preserved(source_segments[index], restored_candidate):
                    candidate = restored_candidate
                    restored = True
                    break
                if attempt == 2:
                    break
                markers = {}
                def retry_mask(match: re.Match[str]) -> str:
                    marker = f"ZXQ{len(markers)}QXZ" if attempt == 0 else f"[{1000 + len(markers)}]"
                    markers[marker] = match.group(0)
                    return marker
                retry_source = PROTECTED_TOKEN_PATTERN.sub(retry_mask, source_segments[index])
                retry_result = translator.translate_batch(
                    [tokenizer.encode(retry_source)], max_batch_size=MAX_TRANSLATION_BATCH_SIZE, beam_size=4,
                    repetition_penalty=1.2, no_repeat_ngram_size=3,
                )[0]
                candidate = tokenizer.decode(retry_result.hypotheses[0]).strip()
            if not restored:
                candidate = translate_around_protected_tokens(
                    source_segments[index], tokenizer, translator,
                )
                if not protected_tokens_preserved(source_segments[index], candidate):
                    raise RuntimeError(
                        f"Argos changed protected tokens in {source_segments[index]!r}: {candidate!r}"
                    )
            translated_segments[index] = candidate
    for index, (source, translated) in enumerate(zip(source_segments, translated_segments)):
        if not contains_untranslated_english_run(source, translated):
            continue
        # Some Argos models copy title-cased interface text verbatim. Retry
        # the complete sentence in ordinary casing while preserving names.
        ordinary_parts: list[str] = []
        position = 0
        for match in PROTECTED_TOKEN_PATTERN.finditer(source):
            ordinary_parts.extend((source[position:match.start()].lower(), match.group(0)))
            position = match.end()
        ordinary_parts.append(source[position:].lower())
        ordinary_source = "".join(ordinary_parts)
        ordinary_result = translator.translate_batch(
            [tokenizer.encode(ordinary_source)], max_batch_size=MAX_TRANSLATION_BATCH_SIZE, beam_size=4,
            repetition_penalty=1.2, no_repeat_ngram_size=3,
        )[0]
        candidate = tokenizer.decode(ordinary_result.hypotheses[0]).strip()
        if (not contains_untranslated_english_run(source, candidate)
                and protected_tokens_preserved(source, candidate)):
            translated_segments[index] = candidate
            continue
        candidate = translate_around_protected_tokens(
            source, tokenizer, translator, sentence_case=True,
        )
        if (
            not contains_untranslated_english_run(source, candidate)
            and protected_tokens_preserved(source, candidate)
        ):
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
    return [normalize_acronym_plurals(source, translated)
            for source, translated in zip(texts, translated_texts)]


@lru_cache(maxsize=None)
def common_state_labels(culture: str) -> dict[str, str]:
    """Use the application's established switch vocabulary, not isolated ML terms."""
    resources = Path(__file__).resolve().parents[2] / "src/GWGUI.App/Resources"
    values = read_entries(resources / culture / "Emulation/EmulationValue.resx")
    automatic = read_entries(resources / culture / "Visualizer.resx").get("Visual.Automatic")
    labels = {
        "disabled": values["Emulation.Value.Disabled"],
        "enabled": values["Emulation.Value.Enabled"],
        "none": values["Emulation.Value.None"],
    }
    labels.update(off=labels["disabled"], on=labels["enabled"])
    if automatic:
        labels.update(auto=automatic, automatic=automatic)
    return labels


@lru_cache(maxsize=None)
def traditional_chinese(value: str) -> str:
    """Convert Argos Chinese output through Windows Unicode script mapping."""
    if not value:
        return value
    mapping = ctypes.WinDLL("kernel32", use_last_error=True).LCMapStringEx
    mapping.argtypes = [ctypes.c_wchar_p, ctypes.c_uint32, ctypes.c_wchar_p,
        ctypes.c_int, ctypes.c_wchar_p, ctypes.c_int, ctypes.c_void_p,
        ctypes.c_void_p, ctypes.c_ssize_t]
    mapping.restype = ctypes.c_int
    traditional_chinese_flag = 0x04000000
    required = mapping("zh-Hant", traditional_chinese_flag, value, -1,
        None, 0, None, None, 0)
    if not required:
        raise ctypes.WinError(ctypes.get_last_error())
    buffer = ctypes.create_unicode_buffer(required)
    written = mapping("zh-Hant", traditional_chinese_flag, value, -1,
        buffer, required, None, None, 0)
    if not written:
        raise ctypes.WinError(ctypes.get_last_error())
    return buffer.value


def normalize_technical_translation(english: str, translated: str, culture: str) -> str:
    # Several models decorate labels with Markdown emphasis. RESX values do
    # not use Markdown; preserve wildcards/emphasis only when present in source.
    if "*" not in english:
        translated = translated.replace("*", "")
    labels = common_state_labels(culture)
    if re.search(r"\bOff\b", english):
        translated = re.sub(r"\bOff\b", labels["disabled"], translated)
    rendering_terms = {
        "da-DK": {"hardware rendering": "hardwaregengivelse",
            "software rendering": "softwaregengivelse"},
        "nl-NL": {"software renderer": "softwarerenderer",
            "hardware rendering": "hardwarematige weergave",
            "software rendering": "softwarematige weergave"},
    }
    if re.search(r"\brender(?:er|ing)\b", english, re.IGNORECASE):
        for term, replacement in rendering_terms.get(culture, {}).items():
            translated = re.sub(r"\b" + term + r"\b", replacement,
                translated, flags=re.IGNORECASE)
    if english.strip().lower() in labels:
        return labels[english.strip().lower()]
    # Only repair disability words when the source actually describes a disabled
    # setting. Never replace them in unrelated accessibility text.
    if re.search(r"\b(?:disabled|off)\b", english, re.IGNORECASE):
        disability_words = {
            "ar-SA": r"(?<!\w)(?:المعوقين|معاق(?:اً|ا|ة|ون|ين)?)(?!\w)",
            "fr-FR": r"\bhandicap(?:é|ée|és|ées|e|es)?\b",
            "cs-CZ": r"\bhandicapovan\w*\b",
            "da-DK": r"\bhandicap(?:pede)?\b",
            "ro-RO": r"\bhandicap(?:ați|aţi|ati|ate|at)?\b",
            "nl-NL": r"\bgehandicapten\b",
        }
        if culture in disability_words:
            if culture == "cs-CZ":
                translated = re.sub(r"s tímto handicapovaným", "pokud je tato možnost zakázána",
                    translated, flags=re.IGNORECASE)
            elif culture == "ro-RO":
                translated = re.sub(r"cu acest handicap", "cu această opțiune dezactivată",
                    translated, flags=re.IGNORECASE)
            translated = re.sub(disability_words[culture], labels["disabled"], translated,
                flags=re.IGNORECASE)
    if culture == "fr-FR":
        if re.search(r"\bthreads?\b", english, re.IGNORECASE):
            translated = re.sub(r"\bfil(s)?\b(?!\s+d['’]exécution)",
                lambda match: f"fil{match[1] or ''} d’exécution", translated)
        reviewed_rendering_values = {
            "Shooting attachment for PlayStation Move": "Accessoire de tir PlayStation Move",
            "Racing wheel for PlayStation Move": "Volant PlayStation Move",
            "Software": "Logiciel",
            "Disabled (Slower)": "Désactivé (plus lent)",
            "Disabled (Beetle Interpreter)": "Désactivé (interpréteur Beetle)",
        }
        if english in reviewed_rendering_values:
            return reviewed_rendering_values[english]
        crop_help = re.fullmatch(
            r"Pads or crops off lines from the (left|right|top|bottom) of the displayed image\.", english)
        if crop_help:
            edge = {"left": "à gauche", "right": "à droite", "top": "en haut", "bottom": "en bas"}[crop_help[1]]
            return f"Ajoute ou retire des lignes {edge} de l’image affichée."
        if english.lower() == "per-cart":
            return "Par cartouche"
        if english.lower() == "per-game":
            return "Par jeu"
        if english == "When running Sega CD/Mega-CD content, specifies whether to share a single backup ram cart for all games (Per-Cart) or to create a separate backup ram cart for each game (Per-Game).":
            return ("Pour les jeux Sega CD/Mega-CD, choisit entre une cartouche RAM de sauvegarde "
                "commune à tous les jeux (par cartouche) et une cartouche RAM de sauvegarde distincte "
                "pour chaque jeu (par jeu).")
        if english == "Sets the backup ram cart size when running Sega CD/Mega-CD content. Useful when setting the backup ram cart to Per-Game to avoid multiple larger cart sizes.":
            return ("Définit la capacité de la cartouche RAM de sauvegarde pour les jeux Sega CD/Mega-CD. "
                "En mode par jeu, une capacité plus faible évite de multiplier les grandes cartouches de sauvegarde.")
        if english.startswith(("Run the audio pipeline at 44.1 kHz", "Run the entire audio pipeline at 44.1 kHz")):
            return ("Utilise une fréquence audio de 44.1 kHz pour les jeux MSU-1. "
                "Lorsque cette option est désactivée, le flux PCM MSU-1 à 44.1 kHz est ramené "
                "à la fréquence native de la SNES, environ 32 kHz, sans filtre anti-repliement. "
                "Les hautes fréquences peuvent alors produire un léger souffle. Lorsqu’elle est activée, "
                "le flux MSU-1 est transmis sans altération et le son SPC est rééchantillonné à 44.1 kHz. "
                "Cette option n’a aucun effet sur les jeux sans MSU-1 et s’applique au prochain chargement.")
        reviewed_labels = {
            "Do not display identical video frames.": "Ignorer les images vidéo identiques",
            "System Boot ROM": "ROM de démarrage du système",
            "Force VDP Mode": "Mode VDP forcé",
            "CD System BRAM": "Mémoire de sauvegarde du Mega-CD",
            "CD Backup RAM Cart": "Cartouche de sauvegarde RAM du Mega-CD",
            "CD Backup RAM Cart Size": "Capacité de la cartouche de sauvegarde RAM",
            "CD Add-on (MD mode)": "Extension Mega-CD (mode Mega Drive)",
            "Borders": "Bordures",
            "CPU Speed": "Vitesse du CPU",
            "CD Access Time": "Temps d’accès au CD",
            "CD Image Cache": "Cache de l’image CD",
            "Enhanced per-tile vertical scroll": "Défilement vertical amélioré par tuile",
            "Enhanced per-tile vertical scroll limit": "Limite du défilement vertical amélioré par tuile",
        }
        if english in reviewed_labels:
            return reviewed_labels[english]
        plane = re.fullmatch(r"Debug > Disable (Sprite Plane|Window Plane|Plane [AB])", english)
        if plane:
            name = {"Sprite Plane": "plan des sprites", "Window Plane": "plan de fenêtre",
                "Plane A": "plan A", "Plane B": "plan B"}[plane[1]]
            return f"Débogage > Désactiver le {name}"
        plane = re.fullmatch(r"Disable the VDP's (Sprite Plane|Window Plane|Plane [AB])\.", english)
        if plane:
            name = {"Sprite Plane": "plan des sprites", "Window Plane": "plan de fenêtre",
                "Plane A": "plan A", "Plane B": "plan B"}[plane[1]]
            return f"Désactive le {name} du VDP."
        channel = re.fullmatch(r"Disable the (YM2612|SN76496)'s (FM\d|PSG\d|DAC) channel\.", english)
        if channel:
            return f"Désactive le canal {channel[2]} du {channel[1]}."
        if re.search(r"\bplane\b", english, re.IGNORECASE):
            translated = re.sub(r"l['’]avion", "le plan", translated, flags=re.IGNORECASE)
            translated = re.sub(r"\bavions?\b", "plan", translated, flags=re.IGNORECASE)
        if re.search(r"\b(?:cart|cartridge)\b", english, re.IGNORECASE):
            translated = re.sub(r"\b(?:panier|chariot)s?\b", "cartouche", translated, flags=re.IGNORECASE)
            for incorrect, correct in (("du cartouche", "de la cartouche"), ("un seul cartouche", "une seule cartouche"),
                    ("un cartouche", "une cartouche"), ("le cartouche", "la cartouche")):
                translated = translated.replace(incorrect, correct)
        if re.search(r"\bborders?\b", english, re.IGNORECASE):
            translated = re.sub(r"\bfrontières\b", "bordures", translated, flags=re.IGNORECASE)
            translated = re.sub(r"\bfrontière\b", "bordure", translated, flags=re.IGNORECASE)
        if "scroll" in english.lower():
            translated = re.sub(r"\bparchemin\b", "défilement", translated, flags=re.IGNORECASE)
        if "add-on" in english.lower():
            translated = re.sub(r"\badditif\b", "extension", translated, flags=re.IGNORECASE)
    return traditional_chinese(translated) if culture == "zh-Hant" else translated


def translate_entries(
    entries: list[tuple[str, str]], tokenizer, translator: ctranslate2.Translator,
    force_context: bool = False, culture: str | None = None,
) -> list[str]:
    translated = translate_preserving_placeholders(
        [english for _, english in entries], tokenizer, translator,
        force_context=force_context,
    )
    if culture:
        return [normalize_technical_translation(english, value, culture)
            for (_, english), value in zip(entries, translated)]
    return translated


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
    return tuple(sorted(PLACEHOLDER_PATTERN.findall(value)))


def protected_signature(value: str) -> tuple[str, ...]:
    return tuple(sorted(STRUCTURAL_TOKEN_PATTERN.findall(value)))


def contains_untranslated_english_run(english: str, translated: str) -> bool:
    """Detect a meaningful English word sequence left inside a translation."""
    english = PROTECTED_TOKEN_PATTERN.sub(" ", english)
    translated = PROTECTED_TOKEN_PATTERN.sub(" ", translated)
    source_words = [
        word.lower()
        for word in re.findall(r"[A-Za-z][A-Za-z'-]*", english)
    ]
    if len(source_words) < 3:
        return False
    target_words = [
        word.lower() for word in re.findall(r"[A-Za-z][A-Za-z'-]*", translated)
    ]
    source_runs = {
        tuple(source_words[index:index + width])
        for width in (3, 4)
        for index in range(len(source_words) - width + 1)
        if sum(len(word) for word in source_words[index:index + width]) >= 18
    }
    return any(
        tuple(target_words[index:index + width]) in source_runs
        for width in (3, 4)
        for index in range(len(target_words) - width + 1)
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
                elif not protected_tokens_preserved(english, target_entries[key]):
                    errors.append(f"{culture_path.name}/{catalog}: technical names or paths differ for {key}")
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


def replace_existing_values(path: Path, updates: dict[str, str]) -> None:
    """Replace values once per catalog, preserving attributes and untouched XML."""
    if not updates:
        return
    with path.open(encoding="utf-8", newline="") as source:
        text = source.read()

    def replace(match: re.Match[str]) -> str:
        block = match.group(0)
        if match.group("tag") != "data":
            return block
        element = ET.fromstring(block.strip())
        key = element.attrib["name"]
        if key not in updates:
            return block
        return re.sub(r"(<value(?:\s[^>]*)?>).*?(</value>)",
            lambda value: value[1] + encode_element_text(updates[key]) + value[2],
            block, count=1, flags=re.DOTALL)

    corrected = RESOURCE_ENTRY_BLOCK_PATTERN.sub(replace, text)
    ET.fromstring(corrected)
    path.write_text(corrected, encoding="utf-8", newline="")


@contextmanager
def translation_model(installed_package):
    translator = ctranslate2.Translator(
        str(installed_package.package_path / "model"),
        intra_threads=TRANSLATION_CPU_THREADS,
    )
    try:
        yield translator
    finally:
        translator.unload_model()
        del translator


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
    parser.add_argument("--retranslate-all", action="store_true",
        help="retranslate every entry in every RESX catalog from en-US")
    parser.add_argument("--culture", choices=[*LANGUAGE_CODES],
        help="limit --sync-all to one culture")
    parser.add_argument("--from-culture", choices=[*LANGUAGE_CODES],
        help="resume bulk translation starting at this culture")
    parser.add_argument("--catalog",
        help="limit --sync-all to one RESX catalog, for example Visualizer.resx")
    parser.add_argument("--clean-only", action="store_true",
        help="remove duplicate keys and localized entries absent from en-US")
    parser.add_argument("--clean-invariants", action="store_true",
        help="keep technical identifiers only in 00-Base, including removing en-US copies")
    parser.add_argument("--audit", action="store_true",
        help="validate catalogs, translatable keys and format placeholders")
    parser.add_argument("--repair-mixed", action="store_true",
        help="retranslate entries that still contain a substantial English fragment")
    parser.add_argument("--format", action="store_true",
        help="put every RESX data/value translation on one physical XML line")
    parser.add_argument("--root", type=Path, default=Path("src/GWGUI.App/Resources"),
        help="RESX root containing 00-Base and culture directories (application or module)")
    parser.add_argument("--repair-technical", action="store_true",
        help="repair known technical mistranslations using English sources and common labels")
    args = parser.parse_args()
    root = args.root
    global PROTECTED_TOKEN_PATTERN
    PROTECTED_TOKEN_PATTERN = protected_token_pattern(root)

    if args.clean_invariants:
        print(f"Invariant copies removed: {remove_invariant_copies(root, args.catalog or '*.resx')}")
        return

    if args.repair_technical:
        catalogs = read_catalogs(root / "en-US", args.catalog or "*.resx")
        for culture in ([args.culture] if args.culture else LANGUAGE_CODES):
            changes = 0
            for catalog, sources in catalogs.items():
                path = root / culture / catalog
                if not path.exists():
                    continue
                updates: dict[str, str] = {}
                for key, current in read_entries(path).items():
                    if key not in sources:
                        continue
                    corrected = normalize_technical_translation(sources[key], current, culture)
                    if corrected != current:
                        updates[key] = corrected
                        changes += 1
                replace_existing_values(path, updates)
            print(f"{culture}: technical translations corrected={changes}", flush=True)
        return

    if args.audit:
        audit_resources(root)
        return

    if args.format:
        changed = sum(format_resx_data_entries(path)
                      for path in root.rglob(args.catalog or "*.resx"))
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
        selected_cultures = ({args.culture: LANGUAGE_CODES[args.culture]}
            if args.culture else LANGUAGE_CODES)
        for culture, language_code in selected_cultures.items():
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
                            or not protected_tokens_preserved(english, current)
                        )
                    ):
                        pending.append((target_path, key, english))
            if not pending:
                print(f"{culture}: repaired=0", flush=True)
                continue
            with translation_model(installed_package) as translator:
                translated_values = translate_entries(
                    [(key, english) for _, key, english in pending],
                    installed_package.tokenizer,
                    translator,
                    force_context=True,
                    culture=culture,
                )
                for (target_path, key, _), value in zip(pending, translated_values):
                    insert(target_path, key, value, replace_existing=True)
                total += len(pending)
                print(f"{culture}: repaired={len(pending)}", flush=True)
        print(f"Partially untranslated entries repaired: {total}")
        return

    if args.sync_all or args.retranslate_all:
        selected_cultures = (
            {args.culture: LANGUAGE_CODES[args.culture]}
            if args.culture in LANGUAGE_CODES
            else LANGUAGE_CODES
        )
        if args.from_culture:
            cultures = list(selected_cultures)
            if args.from_culture not in cultures:
                parser.error("--from-culture must belong to the selected cultures")
            selected_cultures = {culture: selected_cultures[culture]
                for culture in cultures[cultures.index(args.from_culture):]}
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
            with translation_model(installed_package) as translator:
                tokenizer = installed_package.tokenizer
                pending: list[tuple[Path, str, str]] = []
                for catalog, english_entries in english_catalogs.items():
                    target_path = root / culture / Path(catalog)
                    create_empty_catalog(target_path, root / "en-US" / Path(catalog))
                    target_entries = read_entries(target_path)
                    for key, english in english_entries.items():
                        current = target_entries.get(key)
                        if args.retranslate_all or current is None or current == english:
                            pending.append((target_path, key, english))

                translated_values = translate_entries(
                    [(key, english) for _, key, english in pending], tokenizer, translator, culture=culture
                )
                updates_by_path: dict[Path, list[tuple[str, str]]] = {}
                for (target_path, key, _), value in zip(pending, translated_values):
                    updates_by_path.setdefault(target_path, []).append((key, value))
                for target_path, updates in updates_by_path.items():
                    existing = read_entries(target_path)
                    replace_existing_values(target_path,
                        {key: value for key, value in updates if key in existing})
                    for key, value in updates:
                        if key in existing:
                            continue
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
        if not is_invariant_entry(key, english):
            insert(root / "en-US" / args.resource, key, english, args.replace)
    remove_invariant_copies(root, args.resource)
    translatable_entries = [(key, value) for key, value in entries if not is_invariant_entry(key, value)]
    if not translatable_entries:
        return
    english_entries = read_entries(root / "en-US" / args.resource)
    packages = {(item.from_code, item.to_code): item
        for item in package.get_installed_packages() if item.type == "translate"}
    for culture, language_code in LANGUAGE_CODES.items():
        installed_package = packages.get(("en", language_code))
        if installed_package is None:
            raise RuntimeError(f"Missing Argos model en -> {language_code}")
        with translation_model(installed_package) as translator:
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
