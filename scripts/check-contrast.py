#!/usr/bin/env python3
"""
Reports the WCAG contrast ratio of every text/background pairing the site uses.

    python scripts/check-contrast.py

Exits non-zero if any pairing falls below WCAG AA (4.5:1 for normal text), so it
can be wired into CI later.

WHY THIS EXISTS SEPARATELY FROM THE TESTS
-----------------------------------------
ThemeTokenTests proves the two palettes define the same token NAMES. It says
nothing about whether the VALUES are readable. Those are different failures:
a complete palette can still be unreadable.

This is also the evidence for the accessibility claim in the thesis. "The colours
were chosen carefully" is an opinion; "30 pairings measured, lowest ratio 4.6:1,
zero below AA" is a fact somebody can re-check.

READING THE OUTPUT
------------------
  elevation  card vs page - how clearly a card separates from the page behind it.
      This is NOT a WCAG requirement (it is surface against surface, not text),
      so it has no pass mark. Below ~1.2 the layout starts to read as flat.
  rhythm     band vs page - the contrast band. Deliberately huge; that is the point.
  everything else - real text pairings, which must reach 4.5.
"""
import io
import os
import re
import sys
import colorsys


def parse_block(css, pattern, label):
    m = re.search(pattern, css, re.S)
    if not m:
        print("Could not find the %s block in theme.css." % label)
        sys.exit(2)
    return {k: re.sub(r"\s+", " ", v.split("/*")[0]).strip()
            for k, v in re.findall(r"(--[\w-]+):\s*([^;]+);", m.group(1))}


def rgb(value):
    value = value.lstrip("#")
    return tuple(int(value[i:i + 2], 16) / 255 for i in (0, 2, 4))


def luminance(value):
    def channel(c):
        return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4
    r, g, b = map(channel, rgb(value))
    return 0.2126 * r + 0.7152 * g + 0.0722 * b


def ratio(a, b):
    la, lb = luminance(a), luminance(b)
    return (max(la, lb) + 0.05) / (min(la, lb) + 0.05)


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    path = os.path.normpath(os.path.join(
        here, "..", "ElektriKalkulaator", "ElektriKalkulaator", "wwwroot", "css", "theme.css"))
    css = io.open(path, encoding="utf-8-sig").read()

    dark = parse_block(css, r"\n:root \{(.*?)\n\}", "dark")
    light = parse_block(css, r':root\[data-theme="light"\] \{(.*?)\n\}', "light")
    dark2 = parse_block(css, r':root\[data-theme="dark"\] \{(.*?)\n\}', "explicit dark")
    light2 = parse_block(
        css,
        r'@media \(prefers-color-scheme: light\).*?:root:not\(\[data-theme="dark"\]\) \{(.*?)\n    \}',
        "light media query")

    print("PARITY  dark == explicit dark :", dark == dark2)
    print("PARITY  light == media light  :", light == light2)
    print("PARITY  same token names      :", set(dark) == set(light), "(%d)" % len(dark))

    failures = []
    for name, t in (("DARK", dark), ("LIGHT", light)):
        page, card, band = t["--bg-primary"], t["--bg-card"], t["--bg-band"]
        print("\n== %s ==" % name)
        print("  elevation  card vs page : %5.2f   (no pass mark; below ~1.2 reads flat)"
              % ratio(card, page))
        print("  rhythm     band vs page : %5.2f   (the contrast break)" % ratio(band, page))

        pairs = [
            ("text-strong  on card", t["--text-strong"], card),
            ("text-primary on card", t["--text-primary"], card),
            ("text-muted   on card", t["--text-muted"], card),
            ("accent ink   on card", t["--accent-amber-ink"], card),
            ("blue         on card", t["--accent-blue"], card),
            ("green        on card", t["--accent-green"], card),
            ("red          on card", t["--accent-red"], card),
            ("warning      on card", t["--accent-warning"], card),
            ("text-strong  on page", t["--text-strong"], page),
            ("text-primary on page", t["--text-primary"], page),
            ("text-muted   on page", t["--text-muted"], page),
            ("text-primary on input", t["--text-primary"], t["--bg-input"]),
            ("text-primary on raised", t["--text-primary"], t["--bg-card-raised"]),
            ("label on accent button", t["--on-accent"], t["--accent-amber"]),
            ("band-ink    on band", t["--band-ink"], band),
            ("band-muted  on band", t["--band-muted"], band),
            ("band-ink on band card", t["--band-ink"], t["--band-card"]),
        ]
        for label, fg, bg in pairs:
            value = ratio(fg, bg)
            ok = value >= 4.5
            if not ok:
                failures.append("%s / %s = %.2f" % (name, label.strip(), value))
            print("  %-23s %5.2f  %s" % (label, value, "AA" if ok else "*** BELOW AA ***"))

    print("\n== DARK surface hue consistency (one family, saturation falling) ==")
    for key in ("--bg-nav", "--bg-primary", "--bg-card", "--bg-card-raised",
                "--bg-input", "--border-color", "--border-strong"):
        r, g, b = rgb(dark[key])
        h, l, s = colorsys.rgb_to_hls(r, g, b)
        print("  %-18s %s  hue %3.0f  sat %2.0f%%  light %2.0f%%"
              % (key, dark[key], h * 360, s * 100, l * 100))

    print("\nBELOW AA: %d" % len(failures))
    for f in failures:
        print("  ", f)
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
