#!/usr/bin/env python3
"""Add missing RESX keys using the locally installed offline Argos models."""

from __future__ import annotations

import argparse
import re
from pathlib import Path
from xml.sax.saxutils import escape

from argostranslate import translate


LANGUAGE_CODES = {
    "ar-SA": "ar", "cs-CZ": "cs", "da-DK": "da", "de-DE": "de", "el-GR": "el",
    "es-ES": "es", "fi-FI": "fi", "fr-FR": "fr", "he-IL": "he", "hu-HU": "hu",
    "id-ID": "id", "it-IT": "it", "ja-JP": "ja", "ko-KR": "ko", "nb-NO": "nb",
    "nl-NL": "nl", "pl-PL": "pl", "pt-BR": "pt", "pt-PT": "pt", "ro-RO": "ro",
    "ru-RU": "ru", "sv-SE": "sv", "th-TH": "th", "tr-TR": "tr", "uk-UA": "uk",
    "vi-VN": "vi", "zh-Hans": "zh", "zh-Hant": "zh",
}


def insert(path: Path, key: str, value: str, replace_existing: bool = False) -> None:
    text = path.read_text(encoding="utf-8")
    encoded_key = escape(key)
    encoded_value = escape(value)
    pattern = re.compile(
        rf'  <data name="{re.escape(encoded_key)}"><value>.*?</value></data>'
    )
    if pattern.search(text):
        if replace_existing:
            replacement = f'  <data name="{encoded_key}"><value>{encoded_value}</value></data>'
            path.write_text(pattern.sub(replacement, text, count=1), encoding="utf-8", newline="")
        return
    newline = "\r\n" if "\r\n" in text else "\n"
    entry = f'  <data name="{encoded_key}"><value>{encoded_value}</value></data>{newline}'
    marker = f"</root>{newline}" if text.endswith(f"</root>{newline}") else "</root>"
    path.write_text(text.replace(marker, entry + marker, 1), encoding="utf-8", newline="")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("resource")
    parser.add_argument("key")
    parser.add_argument("english")
    parser.add_argument("--replace", action="store_true",
        help="replace the value when the key already exists")
    args = parser.parse_args()
    root = Path("src/GWGUI.App/Resources")
    insert(root / "00-Base" / args.resource, args.key, args.english, args.replace)
    insert(root / "en-US" / args.resource, args.key, args.english, args.replace)
    installed = {language.code: language for language in translate.get_installed_languages()}
    source = installed["en"]
    for culture, language_code in LANGUAGE_CODES.items():
        destination = installed[language_code]
        translation = source.get_translation(destination)
        if translation is None:
            raise RuntimeError(f"Missing Argos model en -> {language_code}")
        value = translation.translate(args.english).strip()
        insert(root / culture / args.resource, args.key, value, args.replace)
        print(culture, flush=True)


if __name__ == "__main__":
    main()
