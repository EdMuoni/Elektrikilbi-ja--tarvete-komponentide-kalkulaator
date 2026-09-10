# -*- coding: utf-8 -*-
# Draws the ERD of the model that was actually BUILT, as a vector figure.
#
# Style follows the conventional ERD look Edgar asked for: a coloured header per
# table, a PK/FK gutter, the primary key underlined on a tinted first row, and
# crow's foot connectors carrying the cardinality instead of "1"/"N" labels.
#
# The figure is GENERATED from the model declared in ElektriKalkulaatorContext.cs
# rather than drawn by hand, so it cannot drift away from the code the way the
# old draft diagram did. Rendered at 3x (see the msedge command at the bottom of
# this file) so it stays sharp when printed at 580 pt page width.
#
# Only the columns that carry meaning are listed. Reprinting every
# CreatedAt/ModifiedAt column makes the figure unreadable at page width and says
# nothing about the design.
import io
import os

W, H = 580, 416
COL = [6, 207, 408]         # x of the three columns
BW = 166                    # box width
# The 35 pt gap between columns is deliberate: two cardinality symbols meet
# in each gap and each needs about 15 pt of clear line to be legible.
GUT = 22                    # PK/FK gutter width
HDR = 17.0                  # header band height
ROW = 13.5                  # one column row

GRID = "#C9CFCC"            # cell separators
TEXT = "#12100E"
MUTED = "#6E7C77"
LINK = "#3B4744"            # relationship lines
FONT = "Segoe UI, Calibri, Arial, sans-serif"

# One colour per table. Chosen to stay distinguishable in greyscale print too:
# the greens/teal are darker than the orange, which is darker than the blue.
C_USER = "#2E7D32"
C_CALC = "#0277BD"
C_REQ = "#C2185B"
C_RULE = "#5E35B1"
C_COMP = "#E65100"
C_PROD = "#00838F"
C_CAT = "#37474F"

out = []


def esc(t):
    return t.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def tint(hexcolour, amount=0.86):
    """Blend a colour towards white — used for the primary-key row."""
    r = int(hexcolour[1:3], 16)
    g = int(hexcolour[3:5], 16)
    b = int(hexcolour[5:7], 16)
    mix = lambda c: int(c + (255 - c) * amount)
    return "#%02X%02X%02X" % (mix(r), mix(g), mix(b))


def box(x, y, title, colour, rows):
    """rows: list of (marker, text) where marker is '' | 'PK' | 'FK'."""
    h = HDR + len(rows) * ROW

    # primary-key row gets a light wash of the header colour
    for i, (marker, _) in enumerate(rows):
        if marker == "PK":
            out.append('<rect x="%.1f" y="%.1f" width="%.1f" height="%.1f" '
                       'fill="%s"/>'
                       % (x, y + HDR + i * ROW, BW, ROW, tint(colour)))

    out.append('<rect x="%.1f" y="%.1f" width="%.1f" height="%.1f" fill="%s"/>'
               % (x, y, BW, HDR, colour))
    out.append('<text x="%.1f" y="%.1f" text-anchor="middle" font-family="%s" '
               'font-size="8" font-weight="700" fill="#FFFFFF" '
               'letter-spacing="0.3">%s</text>'
               % (x + BW / 2, y + 11.8, FONT, esc(title)))

    # cell grid
    for i in range(1, len(rows)):
        yy = y + HDR + i * ROW
        out.append('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" '
                   'stroke-width="0.6"/>' % (x, yy, x + BW, yy, GRID))
    out.append('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" '
               'stroke-width="0.6"/>'
               % (x + GUT, y + HDR, x + GUT, y + h, GRID))
    out.append('<rect x="%.1f" y="%.1f" width="%.1f" height="%.1f" fill="none" '
               'stroke="%s" stroke-width="1"/>' % (x, y, BW, h, colour))

    ty = y + HDR + 9.4
    for marker, text in rows:
        if marker:
            out.append('<text x="%.1f" y="%.1f" text-anchor="middle" '
                       'font-family="%s" font-size="5.8" font-weight="700" '
                       'fill="%s">%s</text>'
                       % (x + GUT / 2, ty - 0.3, FONT, MUTED, marker))
        deco = ' text-decoration="underline"' if marker == "PK" else ""
        weight = ' font-weight="600"' if marker == "PK" else ""
        out.append('<text x="%.1f" y="%.1f" font-family="%s" font-size="7.2" '
                   'fill="%s"%s%s>%s</text>'
                   % (x + GUT + 5, ty, FONT, TEXT, deco, weight, esc(text)))
        ty += ROW
    return h


# ------------------------------------------------- crow's foot connectors
def foot(x, y, side, kind):
    """Cardinality symbol at (x, y) on the given side of a box.

    side  'L' 'R' 'T' 'B' — which edge of the box the line leaves from.
    kind  'one'       exactly one          — a single bar
          'zero_one'  zero or one          — a bar plus a hollow circle
          'many'      one or many          — a crow's foot
          'zero_many' zero or many         — a crow's foot plus a hollow circle
    The prongs of a crow's foot always touch the box, as the notation requires.
    """
    horiz = side in ("L", "R")
    s = -1 if side in ("L", "T") else 1        # direction away from the box

    def stroke(x1, y1, x2, y2):
        out.append('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" '
                   'stroke-width="0.9" stroke-linecap="round"/>'
                   % (x1, y1, x2, y2, LINK))

    if kind in ("many", "zero_many"):
        ax, ay = (x + s * 10, y) if horiz else (x, y + s * 10)
        for off in (-5, 0, 5):
            px, py = (x, y + off) if horiz else (x + off, y)
            stroke(ax, ay, px, py)
        cd = 15
    else:
        d = 7
        bx, by = (x + s * d, y) if horiz else (x, y + s * d)
        if horiz:
            stroke(bx, by - 4.5, bx, by + 4.5)
        else:
            stroke(bx - 4.5, by, bx + 4.5, by)
        cd = 13

    if kind in ("zero_one", "zero_many"):
        cx, cy = (x + s * cd, y) if horiz else (x, y + s * cd)
        out.append('<circle cx="%.1f" cy="%.1f" r="2.6" fill="#FFFFFF" '
                   'stroke="%s" stroke-width="0.9"/>' % (cx, cy, LINK))


def link(x1, y1, x2, y2, a, b, side_a, side_b):
    out.append('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" '
               'stroke-width="0.9"/>' % (x1, y1, x2, y2, LINK))
    foot(x1, y1, side_a, a)
    foot(x2, y2, side_b, b)


def dashed(pts):
    d = "M" + " L".join("%.1f %.1f" % p for p in pts)
    out.append('<path d="%s" fill="none" stroke="%s" stroke-width="0.9" '
               'stroke-dasharray="3 2.5"/>' % (d, LINK))


def note(x, y, txt, anchor="middle", size=6.3):
    out.append('<text x="%.1f" y="%.1f" text-anchor="%s" font-family="%s" '
               'font-size="%.1f" fill="%s">%s</text>'
               % (x, y, anchor, FONT, size, MUTED, esc(txt)))


# --------------------------------------------------------------- boxes
Y1 = 14.0
h_user = box(COL[0], Y1, "ASPNETUSERS", C_USER, [
    ("PK", "Id (string)"),
    ("", "Email (string)"),
    ("", "PasswordHash (string)"),
    ("", "… Identity väljad"),
])

h_calc = box(COL[1], Y1, "POWERBOX_CALCULATION", C_CALC, [
    ("PK", "Id (GUID)"),
    ("FK", "UserId (GUID, null)"),
    ("", "Status (string)"),
    ("", "TotalCost (decimal)"),
    ("", "CreatedAt (datetime)"),
])

h_req = box(COL[2], Y1, "POWERBOX_REQUIREMENTS", C_REQ, [
    ("PK", "Id (GUID)"),
    ("FK", "CalculationId (GUID)"),
    ("", "BuildingType (string)"),
    ("", "RoomCount (int)"),
    ("", "SocketCount / LightCount (int)"),
    ("", "HasElectricStove (bool)"),
])

Y2 = 160.0
h_rule = box(COL[0], Y2, "CALCULATION_RULES", C_RULE, [
    ("PK", "Id (GUID)"),
    ("", "BuildingType (string)"),
    ("", "CircuitType (string)"),
    ("", "WireCrossSectionMm2 (decimal)"),
    ("", "BreakerAmperes (int)"),
    ("", "EvsReference (string)"),
])

h_comp = box(COL[1], Y2, "POWERBOX_COMPONENTS", C_COMP, [
    ("PK", "Id (GUID)"),
    ("FK", "CalculationId (GUID)"),
    ("FK", "ProductId (GUID)"),
    ("", "Quantity (int)"),
    ("", "UnitPrice (decimal)"),
    ("", "TotalPrice (decimal)"),
])

h_prod = box(COL[2], Y2, "PRODUCT", C_PROD, [
    ("PK", "Id (GUID)"),
    ("FK", "CategoryId (GUID)"),
    ("", "Name (string)"),
    ("", "Brand (string)"),
    ("", "Price (decimal)"),
    ("", "RatedCurrent (decimal)"),
    ("", "StockQuantity (int)"),
])

Y3 = 305.0
h_cat = box(COL[2], Y3, "PRODUCT_CATEGORY", C_CAT, [
    ("PK", "Id (GUID)"),
    ("", "Name (string)"),
    ("", "Description (string)"),
])

# ---------------------------------------------------------- relations
R1 = COL[0] + BW
L2, R2 = COL[1], COL[1] + BW
L3 = COL[2]
MIDB = COL[1] + BW / 2
MIDC = COL[2] + BW / 2

# A calculation belongs to zero or one user (UserId is nullable);
# a user may have zero or many calculations.
y = Y1 + 40
link(R1, y, L2, y, "zero_one", "zero_many", "R", "L")

# One calculation has exactly one set of requirements, and vice versa.
link(R2, y, L3, y, "one", "one", "R", "L")

# One calculation has zero or many component rows.
link(MIDB, Y1 + h_calc, MIDB, Y2, "one", "zero_many", "B", "T")

# One product appears on zero or many component rows.
yc = Y2 + 62
link(L3, yc, R2, yc, "one", "zero_many", "L", "R")

# One category holds zero or many products.
link(MIDC, Y3, MIDC, Y2 + h_prod, "one", "zero_many", "T", "B")

# CalculationRules has NO foreign key. The rule is matched to the entered
# building type in C# at run time, which is why this line is dashed. Drawn in
# two runs with a gap so it reads as passing under the line above it.
ygap = 136.0
dashed([(COL[0] + BW / 2, Y2), (COL[0] + BW / 2, ygap), (MIDB - 9, ygap)])
dashed([(MIDB + 9, ygap), (MIDC, ygap), (MIDC, Y1 + h_req)])
note(COL[0] + BW / 2 + 6, ygap - 5, "seos tekib koodis, mitte andmebaasis",
     anchor="start")
note(MIDC - 6, ygap - 5, "BuildingType", anchor="end")

# ------------------------------------------------------------- legend
ly = 378.0
out.append('<line x1="8" y1="%.1f" x2="572" y2="%.1f" stroke="%s" '
           'stroke-width="0.7"/>' % (ly, ly, GRID))

lx = 8.0
for kind, label in (("one", "täpselt üks"),
                    ("zero_one", "null või üks"),
                    ("zero_many", "null või mitu")):
    out.append('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" '
               'stroke-width="0.9"/>' % (lx, ly + 13, lx + 26, ly + 13, LINK))
    foot(lx + 26, ly + 13, "L", kind)
    note(lx + 32, ly + 15.4, label, anchor="start")
    lx += 32 + len(label) * 3.4 + 24

note(8, ly + 27,
     "PK — primaarvõti (allajoonitud), FK — võõrvõti. "
     "Katkendjoon tähendab, et andmebaasis võtit ei ole.",
     anchor="start")
note(572, ly + 15.4,
     "Joonis on koostatud EF Core mudelist (ElektriKalkulaatorContext.cs).",
     anchor="end")
note(572, ly + 27,
     "Ajatemplid CreatedAt/ModifiedAt on igas tabelis ja on välja jäetud.",
     anchor="end")

svg = ('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 %d %d" '
       'width="%d" height="%d"><rect width="%d" height="%d" fill="#FFFFFF"/>'
       '%s</svg>' % (W, H, W * 3, H * 3, W, H, "".join(out)))

HERE = os.path.dirname(os.path.abspath(__file__))
DEST = os.path.join(HERE, "..", "..", "Pictures", "Diagrams")
DEST = os.path.abspath(DEST)
if not os.path.isdir(DEST):
    os.makedirs(DEST)

io.open(os.path.join(DEST, "ERD_ElektriKalkulaator.svg"), "w",
        encoding="utf-8").write(svg)
io.open(os.path.join(DEST, "erd.html"), "w", encoding="utf-8").write(
    '<!doctype html><meta charset="utf-8">'
    '<style>html,body{margin:0;padding:0;background:#fff}</style>' + svg)

print("SVG: %s" % DEST)
print("canvas %dx%d -> render %dx%d" % (W, H, W * 3, H * 3))
print("")
print("Render to PNG with:")
print('  msedge --headless=new --disable-gpu --hide-scrollbars \\')
print('    --screenshot="%s\\ERD_ElektriKalkulaator.png" \\' % DEST)
print('    --window-size=%d,%d "file:///%s/erd.html"'
      % (W * 3, H * 3, DEST.replace(chr(92), "/")))
