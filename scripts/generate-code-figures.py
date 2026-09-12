# -*- coding: utf-8 -*-
# Renders C# snippets from the real source files as thesis figures.
#
# Why not screenshot Visual Studio: a screenshot is fixed at whatever resolution
# the screen had, blurs when Word scales it to the text column, carries the
# editor's dark theme into a printed page, and drifts out of date silently. This
# reads the actual file, so a figure can be regenerated after the code changes
# and is sharp at any print size.
#
# Long lines are soft-wrapped with a hanging indent rather than shrinking the
# font — the font size is what decides whether the figure is readable on paper,
# so it is held constant and the line gives way instead.
#
# Run:  python scripts/generate-code-figures.py
# Then render each .html with the msedge commands it prints.
import io
import os
import re

FONT = "Consolas, 'DejaVu Sans Mono', Menlo, monospace"
UIFONT = "Segoe UI, Calibri, Arial, sans-serif"
FS = 8.6                 # code font size in viewBox units
CW = FS * 0.55           # monospace advance width
LH = 12.2                # line height
GUT = 26                 # line-number gutter
PAD = 9
W = 580
WRAP = 92                # characters before a line is soft-wrapped

BG = "#FBFAF7"
BORDER = "#D6D2CA"
HEADBG = "#2B3632"
NUM = "#A8A399"
CODE = "#1A1A1A"
KW = "#0033B3"
STR = "#A31515"
CMT = "#6B7D6B"
NUMLIT = "#1F7A4D"

KEYWORDS = set("""abstract async await base bool break case catch class const continue decimal
default do double else enum false finally float for foreach get if in int interface internal is
lock long namespace new null object out override params private protected public readonly ref
return sealed set short static string struct switch this throw true try typeof using var virtual
void where while""".split())


def esc(t):
    return t.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def spans(line):
    """Split one line of C# into (text, colour, italic) pieces."""
    out, i, n = [], 0, len(line)
    buf = ""

    def flush():
        # colour the plain-code buffer word by word
        nonlocal buf
        for part in re.split(r"(\w+)", buf):
            if not part:
                continue
            if part in KEYWORDS:
                out.append((part, KW, False))
            elif re.fullmatch(r"\d[\d_.]*m?", part):
                out.append((part, NUMLIT, False))
            else:
                out.append((part, CODE, False))
        buf = ""

    while i < n:
        if line[i] == '"':
            flush()
            j = i + 1
            while j < n and not (line[j] == '"' and line[j - 1] != "\\"):
                j += 1
            out.append((line[i:j + 1], STR, False))
            i = j + 1
        elif line[i] == "/" and i + 1 < n and line[i + 1] == "/":
            flush()
            out.append((line[i:], CMT, True))
            return out
        else:
            buf += line[i]
            i += 1
    flush()
    return out


def wrap(lines):
    """Soft-wrap over-long lines; returns (number_or_None, text) pairs."""
    res = []
    for num, text in lines:
        if len(text) <= WRAP:
            res.append((num, text))
            continue
        indent = len(text) - len(text.lstrip()) + 4
        first, rest = text[:WRAP], text[WRAP:]
        res.append((num, first))
        while rest:
            res.append((None, " " * indent + rest[:WRAP - indent]))
            rest = rest[WRAP - indent:]
    return res


def figure(name, path, start, end, label):
    src = io.open(os.path.join(ROOT, path), encoding="utf-8-sig").read().split("\n")
    raw = [(i, src[i - 1].replace("\t", "    ").rstrip())
           for i in range(start, end + 1)]

    # drop the common leading indentation so the code uses the full width
    bodies = [t for _, t in raw if t.strip()]
    cut = min((len(t) - len(t.lstrip()) for t in bodies), default=0)
    raw = [(i, t[cut:] if len(t) >= cut else t) for i, t in raw]

    lines = wrap(raw)
    head = 15.0
    h = head + PAD + len(lines) * LH + PAD

    o = ['<rect x="0.5" y="0.5" width="%.1f" height="%.1f" rx="3" fill="%s" '
         'stroke="%s" stroke-width="1"/>' % (W - 1, h - 1, BG, BORDER),
         '<path d="M0.5 %.1f v-%.1f a3 3 0 0 1 3 -3 h%.1f a3 3 0 0 1 3 3 v%.1f z" '
         'fill="%s"/>' % (head, head - 3, W - 7, head - 3, HEADBG),
         '<text x="8" y="10.8" font-family="%s" font-size="7.4" fill="#E6ECE9">%s</text>'
         % (UIFONT, esc(path.replace("/", chr(92)))),
         '<text x="%.1f" y="10.8" text-anchor="end" font-family="%s" font-size="7.4" '
         'fill="#9DB0A8">read %d\u2013%d</text>' % (W - 8, UIFONT, start, end),
         '<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" stroke-width="0.7"/>'
         % (GUT, head, GUT, h, BORDER)]

    y = head + PAD + FS
    for num, text in lines:
        if num is not None:
            o.append('<text x="%.1f" y="%.1f" text-anchor="end" font-family="%s" '
                     'font-size="6.8" fill="%s">%d</text>'
                     % (GUT - 5, y, FONT, NUM, num))
        pieces = "".join(
            '<tspan fill="%s"%s>%s</tspan>'
            % (col, ' font-style="italic"' if it else "", esc(t))
            for t, col, it in spans(text))
        o.append('<text x="%.1f" y="%.1f" xml:space="preserve" font-family="%s" '
                 'font-size="%.1f" fill="%s">%s</text>'
                 % (GUT + 6, y, FONT, FS, CODE, pieces))
        y += LH

    hh = int(h + 1)
    svg = ('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 %d %d" width="%d" '
           'height="%d"><rect width="%d" height="%d" fill="#FFFFFF"/>%s</svg>'
           % (W, hh, W * 3, hh * 3, W, hh, "".join(o)))
    io.open(os.path.join(DEST, name + ".svg"), "w", encoding="utf-8").write(svg)
    io.open(os.path.join(DEST, name + ".html"), "w", encoding="utf-8").write(
        '<!doctype html><meta charset="utf-8">'
        '<style>html,body{margin:0;padding:0;background:#fff}</style>' + svg)
    print("  %-22s %3d rida  %dx%d  (%s)" % (name, len(lines), W, hh, label))
    return name, W * 3, hh * 3


HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "ElektriKalkulaator"))
DEST = os.path.abspath(os.path.join(HERE, "..", "..", "Pictures", "Diagrams"))
if not os.path.isdir(DEST):
    os.makedirs(DEST)

S = "ElektriKalkulaator.ApplicationServices/Services/CalculatorServices.cs"
print("Koodijoonised:")
jobs = [
    figure("kood-1-calculationrule",
           "ElektriKalkulaator.Core/Domain/CalculationRule.cs", 3, 33,
           "reeglid elavad andmebaasis"),
    figure("kood-2-algoritm", S, 25, 58, "arvutuse tuum"),
    figure("kood-3-tootevalik", S, 86, 100, "odavaim laos olev toode"),
    figure("kood-4-seosed",
           "ElektriKalkulaator.Data/ElektriKalkulaatorContext.cs", 26, 50,
           "EF Core seoste kirjeldus"),
    figure("kood-5-middleware", "ElektriKalkulaator/Program.cs", 124, 136,
           "autentimine enne autoriseerimist"),
    figure("kood-6-test",
           "ElektriKalkulaator.Tests/CalculatorServicesTests.cs", 59, 73,
           "Arrange / Act / Assert"),
]

EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
print("\nRenderda:")
for name, pw, ph in jobs:
    print('"%s" --headless=new --disable-gpu --hide-scrollbars '
          '--default-background-color=FFFFFFFF --screenshot="%s\\%s.png" '
          '--window-size=%d,%d "file:///%s/%s.html"'
          % (EDGE, DEST, name, pw, ph, DEST.replace(chr(92), "/"), name))
