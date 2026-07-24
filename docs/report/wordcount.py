"""Counts the report's words the way a word processor does, so the total can be
checked against the 4,000 word ceiling.

Hyphenated words count as one, as they do in Word. Figure captions and notes to
the authors are written as blockquotes and are excluded, as are fenced code
blocks. Tables are included, since the guideline excludes only the reference list,
bibliography and appendices.
"""

import pathlib
import re

BASE = pathlib.Path(__file__).parent
CEILING = 4000


def count_words(markdown: str) -> int:
    text = markdown
    text = re.sub(r"^>.*$", "", text, flags=re.MULTILINE)          # captions, notes
    text = re.sub(r"```.*?```", "", text, flags=re.DOTALL)         # code blocks
    text = re.sub(r"^\|[-: |]+\|$", "", text, flags=re.MULTILINE)  # table rules
    text = text.replace("|", " ")
    text = re.sub(r"[#*`]", "", text)                              # markdown syntax
    text = re.sub(r"\[([^\]]*)\]\([^)]*\)", r"\1", text)           # links keep their text

    # A word processor treats a hyphenated compound as one word, so hyphens are
    # kept inside a token rather than used as separators.
    return len(re.findall(r"[A-Za-z0-9][A-Za-z0-9'./-]*", text))


def main() -> None:
    total = 0
    for path in sorted(BASE.glob("chapter-*.md")):
        words = count_words(path.read_text(encoding="utf-8"))
        total += words
        print(f"  {path.name:<45}{words:>5}")

    print(f"  {'-' * 50}")
    print(f"  {'TOTAL':<45}{total:>5}")
    print(f"  {'Ceiling':<45}{CEILING:>5}")
    label = "Headroom" if total <= CEILING else "OVER BY"
    print(f"  {label:<45}{abs(CEILING - total):>5}")


if __name__ == "__main__":
    main()
