from __future__ import annotations

import concurrent.futures
import argparse
import html
import json
import os
import re
import sysconfig
import time
import urllib.parse
import urllib.request
from pathlib import Path


DEFAULT_CACHE = Path("docs/user-guide/translation-data/google-translate-cache.json")


TOKEN_PATTERN = re.compile(
    r"`[^`]+`|<[^>]+>|\]\([^)]+\)|\[|\]|\*\*|\||ZXQLINE\d+QXZ|"
    r"\b(?:GW GUI|Greaseweazle Host Tools|Greaseweazle|Disk Explorer|SuperCard Pro|"
    r"AmigaDOS|Amiga|IBM PC|Atari ST|Apple II|Apple Macintosh|Microsoft \.NET|"
    r"gw\.exe|gwgui\.exe|diskdefs\.cfg|SCP|ADF|IMA|IMG|MSA|D64|HFE|MFM|FM|GCR|"
    r"PLL|TG43|USB|RPM|CRC|ROM|RAM|CPU|FPU|COM\d*|PAL|NTSC|Direct3D 11|WASAPI|"
    r"KiB|MiB|Hz|MHz|FPS)\b",
    re.IGNORECASE,
)


def configure_argos_cuda() -> None:
    site_packages = Path(sysconfig.get_paths()["purelib"])
    cuda_directories = (
        site_packages / "nvidia" / "cublas" / "bin",
        site_packages / "nvidia" / "cuda_runtime" / "bin",
    )
    existing = os.environ.get("PATH", "")
    os.environ["PATH"] = os.pathsep.join(
        [*(str(path) for path in cuda_directories if path.is_dir()), existing]
    )
    os.environ["ARGOS_DEVICE_TYPE"] = "cuda"
    os.environ["ARGOS_COMPUTE_TYPE"] = "float16"
    os.environ.setdefault("XDG_CONFIG_HOME", str(Path("tmp/argos-config").resolve()))


def protect(text: str) -> tuple[str, dict[str, str]]:
    tokens: dict[str, str] = {}

    def replace(match: re.Match[str]) -> str:
        number = len(tokens)
        letters = ""
        while True:
            letters = chr(ord("A") + number % 26) + letters
            number = number // 26 - 1
            if number < 0:
                break
        # Private-use delimiters keep Markdown tokens opaque to the Argos
        # language models without introducing a translatable word.
        marker = "\ue000" + letters + "\ue001"
        tokens[marker] = match.group(0)
        return f" {marker} "

    return TOKEN_PATTERN.sub(replace, text), tokens


def translate_to(text: str, target_language: str) -> str:
    protected, tokens = protect(text)
    body = urllib.parse.urlencode(
        {"client": "gtx", "sl": "en", "tl": target_language, "dt": "t", "q": protected}
    ).encode("utf-8")
    url = "https://translate.googleapis.com/translate_a/single"
    error: Exception | None = None
    for attempt in range(3):
        try:
            request = urllib.request.Request(url, data=body, headers={"User-Agent": "GWGUI-doc-builder/1.0"})
            with urllib.request.urlopen(request, timeout=20) as response:
                payload = json.loads(response.read().decode("utf-8"))
            result = "".join(segment[0] for segment in payload[0])
            result = html.unescape(result).strip()
            for marker, value in tokens.items():
                result = result.replace(marker, value)
            return result
        except Exception as exc:
            error = exc
            time.sleep(attempt + 1)
    raise RuntimeError(f"Translation failed: {text[:80]!r}") from error


def line_parts(line: str) -> tuple[str, str]:
    match = re.match(r"^(#{1,6}\s+|>\s*|\s*[-*+]\s+|\s*\d+\.\s+)(.*)$", line)
    return (match.group(1), match.group(2)) if match else ("", line)


def load_cache(path: Path) -> dict[str, str]:
    if not path.exists():
        return {}
    return json.loads(path.read_text(encoding="utf-8"))


def save_cache(path: Path, cache: dict[str, str]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(path.suffix + ".tmp")
    temporary.write_text(json.dumps(cache, ensure_ascii=False, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    temporary.replace(path)


def make_batches(jobs: list[tuple[int, str, str, str]], maximum_characters: int = 3800):
    batches: list[list[tuple[int, str, str, str]]] = []
    current: list[tuple[int, str, str, str]] = []
    size = 0
    for job in jobs:
        required = len(job[2]) + 24
        if current and size + required > maximum_characters:
            batches.append(current)
            current = []
            size = 0
        current.append(job)
        size += required
    if current:
        batches.append(current)
    return batches


def translate_batch(batch: list[tuple[int, str, str, str]], target_language: str, provider: str) -> list[str]:
    payload = "\n".join(f"ZXQLINE{position}QXZ {job[2]}" for position, job in enumerate(batch))
    if provider == "google":
        translated = translate_to(payload, target_language)
    elif provider == "argos":
        from argostranslate import translate
        translated = translate.translate(payload, "en", target_language)
    else:
        import translators
        translated = translators.translate_text(
            payload, translator=provider, from_language="en", to_language=target_language
        )
    pieces = re.split(r"ZXQLINE(\d+)QXZ\s*", translated)
    results: dict[int, str] = {}
    for offset in range(1, len(pieces), 2):
        results[int(pieces[offset])] = pieces[offset + 1].strip()
    if len(results) != len(batch):
        raise RuntimeError(f"Translation provider returned {len(results)} lines for a {len(batch)}-line batch.")
    return [results[position] for position in range(len(batch))]


def translate_argos_table(line: str, local_translation) -> str:
    cells = line.strip().strip("|").split("|")
    translated_cells: list[str] = []
    for raw_cell in cells:
        value = raw_cell.strip()
        if not value or re.fullmatch(r":?-{3,}:?", value):
            translated_cells.append(value)
            continue
        bold = value.startswith("**") and value.endswith("**")
        if bold:
            value = value[2:-2].strip()
        result = translate_argos_preserving_tokens(value, local_translation)
        result = re.sub(r"\s+", " ", result).strip()
        translated_cells.append(f"**{result}**" if bold else result)
    return "| " + " | ".join(translated_cells) + " |"


def translate_argos_preserving_tokens(text: str, local_translation) -> str:
    """Translate prose while keeping every Markdown/technical token byte-for-byte."""
    pieces: list[str] = []
    position = 0

    def translate_fragment(fragment: str) -> str:
        if not fragment or not any(character.isalpha() for character in fragment):
            return fragment
        leading = fragment[: len(fragment) - len(fragment.lstrip())]
        trailing = fragment[len(fragment.rstrip()) :]
        core = fragment.strip()
        return leading + local_translation.translate(core).strip() + trailing

    for match in TOKEN_PATTERN.finditer(text):
        pieces.append(translate_fragment(text[position : match.start()]))
        pieces.append(match.group(0))
        position = match.end()
    pieces.append(translate_fragment(text[position:]))
    return "".join(pieces)


def main(source: Path, destination: Path, target_language: str, cache_path: Path, provider: str, refresh: bool) -> None:
    if provider == "argos":
        configure_argos_cuda()

    lines = source.read_text(encoding="utf-8").splitlines()
    jobs: list[tuple[int, str, str, str]] = []
    translated = list(lines)
    in_fence = False

    for index, line in enumerate(lines):
        stripped = line.strip()
        if stripped.startswith("```"):
            in_fence = not in_fence
            continue
        if in_fence or not stripped or stripped in {"---", "***", "___"}:
            continue
        if stripped.startswith("<p ") and "<img " in stripped:
            alt = re.match(r'^(.*\balt=")([^"]+)(".*)$', line)
            if alt:
                jobs.append((index, alt.group(1), alt.group(2), alt.group(3)))
            continue
        if re.fullmatch(r"\|?[\s|:-]+\|?", stripped):
            continue
        prefix, content = line_parts(line)
        if content.strip():
            jobs.append((index, prefix, content, ""))

    cache = load_cache(cache_path)
    pending: list[tuple[int, str, str, str]] = []
    for index, prefix, content, suffix in jobs:
        key = f"{provider}\u241f{target_language}\u241f{content}"
        if key in cache and not refresh:
            translated[index] = prefix + cache[key] + suffix
        else:
            pending.append((index, prefix, content, suffix))

    if provider == "argos":
        from argostranslate import translate
        source_language = next(language for language in translate.get_installed_languages() if language.code == "en")
        destination_language = next(language for language in translate.get_installed_languages() if language.code == target_language)
        local_translation = source_language.get_translation(destination_language)
        print(f"Using {len(jobs) - len(pending)} cached translations; translating {len(pending)} lines locally", flush=True)
        for done, (index, prefix, content, suffix) in enumerate(pending, 1):
            result = translate_argos_preserving_tokens(content, local_translation).strip()
            translated[index] = prefix + result + suffix
            cache[f"{provider}\u241f{target_language}\u241f{content}"] = result
            if done % 10 == 0 or done == len(pending):
                save_cache(cache_path, cache)
                print(f"Translated {done}/{len(pending)} new lines", flush=True)
        table_jobs = [job for job in jobs if job[2].strip().startswith("|")]
        for index, prefix, content, suffix in table_jobs:
            result = translate_argos_table(content, local_translation)
            translated[index] = prefix + result + suffix
            cache[f"{provider}\u241f{target_language}\u241f{content}"] = result
        save_cache(cache_path, cache)
    else:
        batches = make_batches(pending)
        print(f"Using {len(jobs) - len(pending)} cached translations; requesting {len(pending)} lines in {len(batches)} batches", flush=True)
        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as executor:
            futures = {
                executor.submit(translate_batch, batch, target_language, provider): batch
                for batch in batches
            }
            done = 0
            for future in concurrent.futures.as_completed(futures):
                batch = futures[future]
                results = future.result()
                for (index, prefix, content, suffix), result in zip(batch, results):
                    translated[index] = prefix + result + suffix
                    cache[f"{provider}\u241f{target_language}\u241f{content}"] = result
                    done += 1
                save_cache(cache_path, cache)
                print(f"Translated {done}/{len(pending)} new lines", flush=True)

    output = "\n".join(translated) + "\n"
    output = re.sub(r"\*\*\s*([^*\n]+?)\s*\*\*", lambda match: f"**{match.group(1).strip()}**", output)
    output = re.sub(r"(?<=\w)(\*\*[^*\n]+\*\*)", r" \1", output)
    output = re.sub(r"(\*\*[^*\n]+\*\*)(?=\w)", r"\1 ", output)
    destination.write_text(output, encoding="utf-8")
    print(f"Written {destination} ({len(jobs)} translated lines)")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Translate a Markdown guide through Google Translate.")
    parser.add_argument("source", type=Path)
    parser.add_argument("destination", type=Path)
    parser.add_argument("target_language")
    parser.add_argument("--cache", type=Path, default=DEFAULT_CACHE)
    parser.add_argument("--provider", choices=("google", "bing", "argos"), default="google")
    parser.add_argument("--refresh", action="store_true", help="Ignore cached entries for this run.")
    arguments = parser.parse_args()
    main(arguments.source, arguments.destination, arguments.target_language, arguments.cache, arguments.provider, arguments.refresh)
