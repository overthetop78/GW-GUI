from __future__ import annotations

import concurrent.futures
import html
import json
import re
import sys
import time
import urllib.parse
import urllib.request
from pathlib import Path


TOKEN_PATTERN = re.compile(
    r"`[^`]+`|<[^>]+>|\]\([^)]+\)|"
    r"\b(?:GW GUI|Greaseweazle Host Tools|Greaseweazle|Disk Explorer|SuperCard Pro|"
    r"AmigaDOS|Amiga|IBM PC|Atari ST|Apple II|Apple Macintosh|Microsoft \.NET|"
    r"gw\.exe|gwgui\.exe|diskdefs\.cfg|SCP|ADF|IMA|IMG|MSA|D64|HFE|MFM|FM|GCR|"
    r"PLL|TG43|USB|RPM|CRC|ROM|RAM|CPU|FPU|COM\d*|PAL|NTSC|Direct3D 11|WASAPI|"
    r"KiB|MiB|Hz|MHz|FPS)\b",
    re.IGNORECASE,
)


def protect(text: str) -> tuple[str, dict[str, str]]:
    tokens: dict[str, str] = {}

    def replace(match: re.Match[str]) -> str:
        marker = f"ZXQ{len(tokens)}QXZ"
        tokens[marker] = match.group(0)
        return marker

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


def main(source: Path, destination: Path, target_language: str) -> None:
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

    with concurrent.futures.ThreadPoolExecutor(max_workers=8) as executor:
        futures = {
            executor.submit(translate_to, content, target_language): (index, prefix, suffix)
            for index, prefix, content, suffix in jobs
        }
        done = 0
        for future in concurrent.futures.as_completed(futures):
            index, prefix, suffix = futures[future]
            translated[index] = prefix + future.result() + suffix
            done += 1
            if done % 50 == 0:
                print(f"Translated {done}/{len(jobs)} lines", flush=True)

    output = "\n".join(translated) + "\n"
    output = re.sub(r"\*\*\s+", "**", output)
    output = re.sub(r"\s+\*\*", "**", output)
    destination.write_text(output, encoding="utf-8")
    print(f"Written {destination} ({len(jobs)} translated lines)")


if __name__ == "__main__":
    if len(sys.argv) != 4:
        raise SystemExit("Usage: google-translate-markdown.py SOURCE.md DESTINATION.md TARGET_LANGUAGE")
    main(Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3])
