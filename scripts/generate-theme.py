#!/usr/bin/env python3
"""
Generates ElektriKalkulaator/wwwroot/css/theme.css.

    python scripts/generate-theme.py

WHY A GENERATOR AND NOT A HAND-WRITTEN FILE
-------------------------------------------
theme.css contains the SAME palette four times: the dark defaults on :root, the
light palette inside a prefers-color-scheme media query, and then both again
under [data-theme="light"] / [data-theme="dark"] so an explicit choice beats the
operating system in both directions.

Four hand-maintained copies drift. A token added to one and forgotten in another
silently inherits the wrong theme's value, which produces the worst kind of bug:
invisible text that only appears on one page, in one theme, long after the change
that caused it. Generating all four from the two tables below makes that
impossible by construction. ThemeTokenTests then checks the result.

Edit the tables here, re-run, and commit both this file and theme.css.

THE COLOUR REASONING
--------------------
Both palettes are WARM neutrals, not cool greys. Warm neutrals read as paper,
linen and unfinished wood; cool greys read as screens and hospitals. For a tool
someone uses while planning work in a building, the first is the right feeling,
and it is also the direction current design practice has moved - bone and sand
replacing pure white and cool grey.

Surfaces share one hue (~28 degrees, a warm taupe) and get LESS saturated as they
get lighter, which is how real materials behave under light.

Dark mode follows the established accessibility guidance:
  - never pure black - #000 against light text causes halation, and grey shows
    elevation because you cannot see a shadow on black
  - never pure white text, for the same halation reason
  - accents are LIGHTER and LESS saturated than their light-mode counterparts;
    a fully saturated colour vibrates against a dark background

--bg-band is the one token that deliberately INVERTS between themes: warm cream
on the dark theme, warm near-black on the light one. It paints a single section
the opposite value to the rest of the page, which is what stops a long page
reading as flat. Because it flips per theme, that rhythm survives the switch.
"""
import io
import os
import sys

# ---------------------------------------------------------------------------
# The dark palette, in the order it should appear in the file.
# (token, dark value, comment shown in the CSS)
# Comments are shared by BOTH themes, so they must not describe one theme's
# values - "page background", never "warm near-black".
# ---------------------------------------------------------------------------
GROUPS = [
    ("SURFACES, furthest back to nearest front", [
        ("--bg-primary",     "#141210", "page background"),
        ("--bg-card",        "#36302B", "cards and panels"),
        ("--bg-card-raised", "#453D38", "a card sitting on top of another card"),
        ("--bg-input",       "#3C3631", "form fields"),
        ("--bg-nav",         "#0D0B0A", "navigation bar"),
        ("--bg-subtle",      "rgba(255, 255, 255, 0.045)", "table headers, hover fills"),
    ]),
    ("THE CONTRAST BAND - inverts between themes, see the module docstring", [
        ("--bg-band",     "#F3EEE7", "the band surface"),
        ("--band-ink",    "#1A1511", "headings inside the band"),
        ("--band-muted",  "#5E544B", "secondary text inside the band"),
        ("--band-card",   "#FFFFFF", "a card sitting on the band"),
        ("--band-border", "#DED6CA", "borders inside the band"),
    ]),
    ("ACCENTS - each has exactly one job, see docs/DESIGN_GUIDE.md", [
        ("--accent-amber",       "#F2764B", "THE next action. Button fills."),
        ("--accent-amber-ink",   "#F79A76", "the same accent used as TEXT rather than as a fill"),
        ("--accent-amber-hover", "#FF8F63", ""),
        ("--on-accent",          "#20100A", "text placed ON an accent button"),
        ("--accent-blue",        "#7FB6EE", "information, links, technical badges"),
        ("--accent-green",       "#62C79E", "success, in stock, trust"),
        ("--accent-red",         "#F08E80", "errors and destructive actions only"),
        ("--accent-warning",     "#E9B96A", "low stock"),
    ]),
    ("TEXT", [
        ("--text-strong",  "#F6F4F1", "headings and figures that must dominate"),
        ("--text-primary", "#DAD5D0", "body text and prices"),
        ("--text-muted",   "#ACA49D", "secondary text"),
    ]),
    ("BORDERS", [
        ("--border-color",  "#534C46", ""),
        ("--border-strong", "#696059", "used when a border must actually be seen"),
    ]),
    ("CIRCUIT TYPES in the BOM table", [
        ("--circuit-lighting", "#7FB6EE", ""),
        ("--circuit-socket",   "#62C79E", ""),
        ("--circuit-stove",    "#F08E80", ""),
        ("--circuit-rcd",      "#BFA6F5", ""),
        ("--circuit-panel",    "#F79A76", ""),
    ]),
    ("TINTED FILLS behind badges and the trust strip", [
        ("--tint-amber",   "rgba(242, 118, 75, 0.14)", ""),
        ("--tint-blue",    "rgba(127, 182, 238, 0.13)", ""),
        ("--tint-green",   "rgba(98, 199, 158, 0.12)", ""),
        ("--tint-red",     "rgba(240, 142, 128, 0.13)", ""),
        ("--tint-warning", "rgba(233, 185, 106, 0.13)", ""),
    ]),
    ("SHADOWS", [
        ("--shadow-card",  "0 1px 0 rgba(255,255,255,0.04) inset, 0 10px 30px rgba(0,0,0,0.45)",
         "elevation"),
        ("--shadow-float", "0 20px 52px rgba(0,0,0,0.55)", "cards that overlap the hero photo"),
    ]),
]

# ---------------------------------------------------------------------------
# The light palette. Every token above must appear here or the script refuses
# to run - that check is the whole point of the file.
#
# Not an inversion: several colours are genuinely different values, because a
# colour readable on near-black is often unreadable on near-white. The clearest
# case is the orange accent - #F2764B stays as a BUTTON FILL (dark label on top)
# but has to darken to #A8481D to be readable as TEXT on a light surface.
# ---------------------------------------------------------------------------
LIGHT = {
    "--bg-primary": "#F2EEE8", "--bg-card": "#FEFDFB", "--bg-card-raised": "#FFFFFF",
    "--bg-input": "#FEFDFB", "--bg-nav": "#FEFDFB",
    "--bg-subtle": "rgba(26, 21, 17, 0.04)",

    "--bg-band": "#1F1A16", "--band-ink": "#F6F4F1", "--band-muted": "#B3A99F",
    "--band-card": "#2C2723", "--band-border": "#3D352E",

    "--accent-amber": "#F2764B", "--accent-amber-ink": "#A8481D",
    "--accent-amber-hover": "#DC6135", "--on-accent": "#20100A",
    "--accent-blue": "#1D5BB8", "--accent-green": "#0A6B4D",
    "--accent-red": "#A83226", "--accent-warning": "#85560C",

    "--text-strong": "#1A1511", "--text-primary": "#3F362F", "--text-muted": "#756A61",

    "--border-color": "#E7E1DA", "--border-strong": "#D2C9C1",

    "--circuit-lighting": "#1D5BB8", "--circuit-socket": "#0A6B4D",
    "--circuit-stove": "#A83226", "--circuit-rcd": "#6D3FC4", "--circuit-panel": "#A8481D",

    "--tint-amber": "rgba(242, 118, 75, 0.15)", "--tint-blue": "rgba(29, 91, 184, 0.10)",
    "--tint-green": "rgba(10, 107, 77, 0.10)", "--tint-red": "rgba(168, 50, 38, 0.10)",
    "--tint-warning": "rgba(133, 86, 12, 0.12)",

    "--shadow-card": "0 1px 2px rgba(26,21,17,0.05), 0 8px 24px rgba(26,21,17,0.07)",
    "--shadow-float": "0 20px 52px rgba(26,21,17,0.16)",
}


def block(theme, indent):
    """Render one complete palette at the given indent."""
    pad = " " * indent
    out = []
    for title, items in GROUPS:
        out.append(pad + "/* " + title + " */")
        for name, dark_value, note in items:
            value = dark_value if theme == "dark" else LIGHT[name]
            line = (pad + name + ":").ljust(len(pad) + 22) + value + ";"
            if note:
                line = line.ljust(len(pad) + 82) + "/* " + note + " */"
            out.append(line.rstrip())
        out.append("")
    out.append(pad + "color-scheme: " + theme + ";")
    out.append("")
    return "\n".join(out)


HEADER = '''/* =============================================================================
   theme.css - ALL COLOURS FOR THE WHOLE SITE
   =============================================================================
   GENERATED FILE. Do not edit by hand - your changes will be overwritten.
       python scripts/generate-theme.py
   The reasoning behind every value is in that script's docstring.

   Every colour used anywhere lives in this file and nowhere else. site.css and
   the views refer to them by name - var(--bg-card) - and never write a colour
   value of their own. ThemeTokenTests enforces that.

   -- HOW THE LIGHT / DARK SWITCH WORKS ---------------------------------------
   Three states, not two:

     1. "system"  - no data-theme attribute. The OS decides, via the
                    prefers-color-scheme media query. This is the default.
     2. "dark"    - the visitor chose dark:  <html data-theme="dark">
     3. "light"   - the visitor chose light: <html data-theme="light">

   The visitor's choice must beat the system preference in BOTH directions,
   which is why the media query is written as :root:not([data-theme="dark"])
   rather than plain :root. wwwroot/js/theme.js sets the attribute.

   -- WHY EVERYTHING IS WARM ---------------------------------------------------
   Both palettes are warm neutrals rather than cool greys. Warm neutrals read as
   paper, linen and unfinished wood; cool greys read as screens and hospitals.
   For a tool someone uses while planning work in a building, the first is the
   right feeling. Surfaces share one hue (~28deg) and lose saturation as they
   get lighter, which is how real materials behave under light.

   Dark mode follows established accessibility guidance: never pure black
   (halation against light text, and you cannot see a shadow on black), never
   pure white text for the same reason, and accents that are lighter and less
   saturated than their light-mode versions, because a saturated colour vibrates
   against a dark background.

   -- THE CONTRAST BAND -------------------------------------------------------
   --bg-band is the one token that INVERTS between themes: warm cream on dark,
   warm near-black on light. It paints one section the opposite value to the
   rest of the page, which is what stops a long page reading as flat. Because it
   flips per theme, that rhythm survives the switch.

   Anything drawn inside the band MUST use --band-ink / --band-muted /
   --band-card, never --text-primary, or it will be invisible in one theme.
   ============================================================================= */


/* -- DARK: the default palette ----------------------------------------------
   Dark is this project's identity, so it is the base. A visitor whose system
   asks for light gets the light palette below; everyone else sees this. */
:root {
'''

LIGHT_NOTE = '''/* -- LIGHT -------------------------------------------------------------------
   Not an inversion of the above. Several colours are genuinely different
   values, because a colour readable on near-black is often unreadable on
   near-white. The orange accent stays #F2764B as a BUTTON FILL, where a dark
   label sits on top, but darkens to #A8481D when used as TEXT.

   Backgrounds are a warm bone rather than pure white: pure white glares in
   daylight and reads as colder than the rest of the palette. */
@media (prefers-color-scheme: light) {
    /* :not([data-theme="dark"]) = "unless the visitor explicitly chose dark". */
    :root:not([data-theme="dark"]) {
'''


def main():
    declared = [name for _, items in GROUPS for name, _, _ in items]

    missing = [n for n in declared if n not in LIGHT]
    extra = [n for n in LIGHT if n not in declared]
    if missing or extra:
        print("Palette tables disagree - refusing to generate.")
        for n in missing:
            print("  missing from LIGHT:", n)
        for n in extra:
            print("  in LIGHT but not declared in GROUPS:", n)
        return 1

    css = "".join([
        HEADER, block("dark", 4), "}\n\n",
        LIGHT_NOTE, block("light", 8), "    }\n}\n\n",
        '/* The visitor explicitly chose light. Same values as the media query\n'
        '   above, but applied regardless of what the operating system prefers. */\n'
        ':root[data-theme="light"] {\n', block("light", 4), "}\n\n",
        '/* The visitor explicitly chose dark. Repeats the defaults so that\n'
        '   choosing dark on a light-preferring system works. */\n'
        ':root[data-theme="dark"] {\n', block("dark", 4), "}\n",
    ])

    here = os.path.dirname(os.path.abspath(__file__))
    target = os.path.join(here, "..", "ElektriKalkulaator", "ElektriKalkulaator",
                          "wwwroot", "css", "theme.css")
    target = os.path.normpath(target)
    io.open(target, "w", encoding="utf-8").write(css)
    print("wrote %s (%d tokens per palette)" % (target, len(declared)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
