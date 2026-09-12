# -*- coding: utf-8 -*-
# Draws BOTH entity-relationship diagrams used in the thesis, from one set of
# drawing routines, so the two figures cannot drift apart in style:
#
#   Joonis 1  — the model that was actually BUILT (7 tables, read from
#               ElektriKalkulaatorContext.cs)
#   Joonis 10 — the model that was DESIGNED during planning (12 tables), shown
#               in chapter 3 against the built one
#
# Style: a coloured header per table, a PK/FK gutter, the primary key underlined
# on a tinted first row, and crow's foot connectors carrying the cardinality.
#
# The built diagram lists column types; the designed one does not. That is not
# an inconsistency — the designed model has 12 tables in the same page width, so
# the types would push the boxes past the point where the text is legible on
# paper. Names alone carry what that figure is there to show.
#
# Run:  python scripts/generate-erd.py
# Then render each .html to .png with the msedge command printed at the end.
import io
import os

BW = 166                    # box width
GUT = 22                    # PK/FK gutter width
HDR = 17.0                  # header band height
ROW = 13.5                  # one column row
COL = [6, 207, 408]         # x of the three columns
# The 35 pt gap between columns is deliberate: two cardinality symbols meet in
# each gap and each needs about 15 pt of clear line to be legible.

GRID = "#C9CFCC"
TEXT = "#12100E"
MUTED = "#6E7C77"
LINK = "#3B4744"
FONT = "Segoe UI, Calibri, Arial, sans-serif"

# One colour per table, kept distinguishable in greyscale print as well.
C_GREEN = "#2E7D32"
C_BLUE = "#0277BD"
C_PINK = "#C2185B"
C_PURPLE = "#5E35B1"
C_ORANGE = "#E65100"
C_TEAL = "#00838F"
C_SLATE = "#37474F"
C_INDIGO = "#283593"
C_BROWN = "#6D4C41"
C_CYAN = "#0097A7"

out = []


def esc(t):
    return t.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def tint(hexcolour, amount=0.86):
    """Blend a colour towards white — used for the primary-key row."""
    r, g, b = (int(hexcolour[i:i + 2], 16) for i in (1, 3, 5))
    mix = lambda c: int(c + (255 - c) * amount)
    return "#%02X%02X%02X" % (mix(r), mix(g), mix(b))


def box(x, y, title, colour, rows):
    """rows: list of (marker, text) where marker is '' | 'PK' | 'FK'."""
    h = HDR + len(rows) * ROW

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
    kind  'one'       exactly one   — a single bar
          'zero_one'  zero or one   — a bar plus a hollow circle
          'many'      one or many   — a crow's foot
          'zero_many' zero or many  — a crow's foot plus a hollow circle
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
        bx, by = (x + s * 7, y) if horiz else (x, y + s * 7)
        if horiz:
            stroke(bx, by - 4.5, bx, by + 4.5)
        else:
            stroke(bx - 4.5, by, bx + 4.5, by)
        cd = 13

    if kind in ("zero_one", "zero_many"):
        cx, cy = (x + s * cd, y) if horiz else (x, y + s * cd)
        out.append('<circle cx="%.1f" cy="%.1f" r="2.6" fill="#FFFFFF" '
                   'stroke="%s" stroke-width="0.9"/>' % (cx, cy, LINK))


def polyline(pts, dash=False):
    d = "M" + " L".join("%.1f %.1f" % p for p in pts)
    out.append('<path d="%s" fill="none" stroke="%s" stroke-width="0.9"%s/>'
               % (d, LINK, ' stroke-dasharray="3 2.5"' if dash else ""))


def link(x1, y1, x2, y2, a, b, side_a, side_b, via=None):
    """Straight link, or an L-shaped one when `via` gives intermediate points."""
    polyline([(x1, y1)] + (via or []) + [(x2, y2)])
    foot(x1, y1, side_a, a)
    foot(x2, y2, side_b, b)


def note(x, y, txt, anchor="middle", size=6.3):
    out.append('<text x="%.1f" y="%.1f" text-anchor="%s" font-family="%s" '
               'font-size="%.1f" fill="%s">%s</text>'
               % (x, y, anchor, FONT, size, MUTED, esc(txt)))


def legend(y, width):
    """Notation key. Deliberately the ONLY footer on the figure — the source
    note that used to sit on the right was removed on Edgar's instruction: the
    caption underneath already says what the figure is."""
    out.append('<line x1="6" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" '
               'stroke-width="0.7"/>' % (y, width - 6, y, GRID))
    lx = 6.0
    for kind, label in (("one", "täpselt üks"),
                        ("zero_one", "null või üks"),
                        ("zero_many", "null või mitu")):
        out.append('<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="%s" '
                   'stroke-width="0.9"/>' % (lx, y + 13, lx + 26, y + 13, LINK))
        foot(lx + 26, y + 13, "L", kind)
        note(lx + 32, y + 15.4, label, anchor="start")
        lx += 32 + len(label) * 3.4 + 24
    note(lx + 4, y + 15.4,
         "PK — primaarvõti (allajoonitud), FK — võõrvõti", anchor="start")


def write(name, width, height, title):
    svg = ('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 %d %d" '
           'width="%d" height="%d"><rect width="%d" height="%d" fill="#FFFFFF"/>'
           '%s</svg>' % (width, height, width * 3, height * 3,
                         width, height, "".join(out)))
    io.open(os.path.join(DEST, name + ".svg"), "w", encoding="utf-8").write(svg)
    io.open(os.path.join(DEST, name + ".html"), "w", encoding="utf-8").write(
        '<!doctype html><meta charset="utf-8">'
        '<style>html,body{margin:0;padding:0;background:#fff}</style>' + svg)
    print("  %-28s %dx%d  ->  render %dx%d   (%s)"
          % (name + ".svg", width, height, width * 3, height * 3, title))
    return name, width * 3, height * 3


HERE = os.path.dirname(os.path.abspath(__file__))
DEST = os.path.abspath(os.path.join(HERE, "..", "..", "Pictures", "Diagrams"))
if not os.path.isdir(DEST):
    os.makedirs(DEST)

jobs = []
print("Koostatud:")

# ===================================================================
# Joonis 1 — the model that was BUILT
# ===================================================================
out = []
W1, H1 = 580, 400

Y1, Y2, Y3 = 14.0, 160.0, 305.0
h_user = box(COL[0], Y1, "ASPNETUSERS", C_GREEN, [
    ("PK", "Id (string)"), ("", "Email (string)"),
    ("", "PasswordHash (string)"), ("", "… Identity väljad")])
h_calc = box(COL[1], Y1, "POWERBOX_CALCULATION", C_BLUE, [
    ("PK", "Id (GUID)"), ("FK", "UserId (GUID, null)"), ("", "Status (string)"),
    ("", "TotalCost (decimal)"), ("", "CreatedAt (datetime)")])
h_req = box(COL[2], Y1, "POWERBOX_REQUIREMENTS", C_PINK, [
    ("PK", "Id (GUID)"), ("FK", "CalculationId (GUID)"),
    ("", "BuildingType (string)"), ("", "RoomCount (int)"),
    ("", "SocketCount / LightCount (int)"), ("", "HasElectricStove (bool)")])
h_rule = box(COL[0], Y2, "CALCULATION_RULES", C_PURPLE, [
    ("PK", "Id (GUID)"), ("", "BuildingType (string)"),
    ("", "CircuitType (string)"), ("", "WireCrossSectionMm2 (decimal)"),
    ("", "BreakerAmperes (int)"), ("", "EvsReference (string)")])
h_comp = box(COL[1], Y2, "POWERBOX_COMPONENTS", C_ORANGE, [
    ("PK", "Id (GUID)"), ("FK", "CalculationId (GUID)"),
    ("FK", "ProductId (GUID)"), ("", "Quantity (int)"),
    ("", "UnitPrice (decimal)"), ("", "TotalPrice (decimal)")])
h_prod = box(COL[2], Y2, "PRODUCT", C_TEAL, [
    ("PK", "Id (GUID)"), ("FK", "CategoryId (GUID)"), ("", "Name (string)"),
    ("", "Brand (string)"), ("", "Price (decimal)"),
    ("", "RatedCurrent (decimal)"), ("", "StockQuantity (int)")])
h_cat = box(COL[2], Y3, "PRODUCT_CATEGORY", C_SLATE, [
    ("PK", "Id (GUID)"), ("", "Name (string)"), ("", "Description (string)")])

R1, L2, R2, L3 = COL[0] + BW, COL[1], COL[1] + BW, COL[2]
MIDB, MIDC = COL[1] + BW / 2, COL[2] + BW / 2

y = Y1 + 40
link(R1, y, L2, y, "zero_one", "zero_many", "R", "L")   # user -> calculation
link(R2, y, L3, y, "one", "one", "R", "L")              # calculation <-> requirements
link(MIDB, Y1 + h_calc, MIDB, Y2, "one", "zero_many", "B", "T")
yc = Y2 + 62
link(L3, yc, R2, yc, "one", "zero_many", "L", "R")      # product -> components
link(MIDC, Y3, MIDC, Y2 + h_prod, "one", "zero_many", "T", "B")

# CalculationRules has NO foreign key — matched in C# at run time, hence dashed.
ygap = 136.0
polyline([(COL[0] + BW / 2, Y2), (COL[0] + BW / 2, ygap), (MIDB - 9, ygap)], dash=True)
polyline([(MIDB + 9, ygap), (MIDC, ygap), (MIDC, Y1 + h_req)], dash=True)
note(COL[0] + BW / 2 + 6, ygap - 5, "seos tekib koodis, mitte andmebaasis",
     anchor="start")
note(MIDC - 6, ygap - 5, "BuildingType", anchor="end")

legend(362.0, W1)
jobs.append(write("ERD_ElektriKalkulaator", W1, H1, "Joonis 1 — realiseeritud"))

# ===================================================================
# Joonis 10 — the model that was DESIGNED during planning
# ===================================================================
out = []
W2, H2 = 580, 590
P1, P2, P3, P4 = 14.0, 143.0, 300.0, 437.0

h_ais = box(COL[0], P1, "AI_SERVICE", C_PURPLE, [
    ("PK", "id"), ("", "name"), ("", "provider"), ("", "api_version")])
h_air = box(COL[1], P1, "AI_REQUEST", C_PURPLE, [
    ("PK", "id"), ("FK", "building_calculation_id"), ("FK", "ai_service_id"),
    ("", "input_payload"), ("", "requested_at")])
h_aip = box(COL[2], P1, "AI_RESPONSE", C_PINK, [
    ("PK", "id"), ("FK", "ai_request_id"), ("", "output_payload"),
    ("", "confidence_score")])

h_usr = box(COL[0], P2, "USER", C_GREEN, [
    ("PK", "id"), ("", "email"), ("", "password_hash"),
    ("", "preferred_language"), ("", "created_at")])
h_bc = box(COL[1], P2, "BUILDING_CALCULATION", C_BLUE, [
    ("PK", "id"), ("FK", "user_id"), ("", "building_type"), ("", "floors"),
    ("", "rooms_per_floor"), ("", "total_area_m2"), ("", "status")])
h_cl = box(COL[2], P2, "COMPONENT_LIST", C_ORANGE, [
    ("PK", "id"), ("FK", "building_calculation_id"), ("", "generated_at")])

h_ctg = box(COL[0], P3, "CATEGORY", C_SLATE, [("PK", "id"), ("", "name")])
h_prd = box(COL[1], P3, "PRODUCT", C_TEAL, [
    ("PK", "id"), ("", "sku"), ("", "price"), ("", "image_url"),
    ("", "is_active")])
h_cli = box(COL[2], P3, "COMPONENT_LIST_ITEM", C_ORANGE, [
    ("PK", "id"), ("FK", "component_list_id"), ("FK", "product_id"),
    ("", "quantity"), ("", "usage_description")])

h_brd = box(COL[0], P4, "BRAND", C_BROWN, [("PK", "id"), ("", "name")])
h_ptr = box(COL[1], P4, "PRODUCT_TRANSLATION", C_CYAN, [
    ("PK", "id"), ("FK", "product_id"), ("", "language"), ("", "name"),
    ("", "description")])
h_psp = box(COL[2], P4, "PRODUCT_SPEC", C_INDIGO, [
    ("PK", "id"), ("FK", "product_id"), ("", "voltage"), ("", "amperage"),
    ("", "material")])

GAP12 = (COL[0] + BW + COL[1]) / 2      # clear lane between columns 1 and 2

ya = P1 + 34
link(R1, ya, L2, ya, "one", "zero_many", "R", "L")          # ai_service -> ai_request
link(R2, ya, L3, ya, "one", "zero_one", "R", "L")           # ai_request -> ai_response
link(MIDB, P2, MIDB, P1 + h_air, "one", "zero_many", "T", "B")   # calc -> ai_request

yb = P2 + 40
link(R1, yb, L2, yb, "one", "zero_many", "R", "L")          # user -> calculation
link(R2, yb, L3, yb, "one", "zero_one", "R", "L")           # calculation -> list
link(MIDC, P2 + h_cl, MIDC, P3, "one", "zero_many", "B", "T")    # list -> list_item

yc2 = P3 + 30
link(R1, yc2, L2, yc2, "one", "zero_many", "R", "L")        # category -> product
link(R2, yc2, L3, yc2, "one", "zero_many", "R", "L")        # product -> list_item

# brand -> product: routed through the lane between columns 1 and 2, because
# CATEGORY sits directly above BRAND and a straight line would cross it.
link(COL[0] + BW, P4 + 24, COL[1], P3 + 64, "one", "zero_many", "R", "L",
     via=[(GAP12, P4 + 24), (GAP12, P3 + 64)])

link(MIDB - 28, P4, MIDB - 28, P3 + h_prd, "one", "zero_many", "T", "B")  # product -> translation
# product -> spec: down out of PRODUCT, across below the third column, then up.
link(MIDC, P4, MIDB + 28, P3 + h_prd, "zero_one", "one", "T", "B",
     via=[(MIDC, P4 - 26), (MIDB + 28, P4 - 26)])

legend(552.0, W2)
jobs.append(write("ERD_Kavandatud", W2, H2, "Joonis 10 — kavandatud"))

print("\nRenderda:")
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
for name, pw, ph in jobs:
    print('  "%s" --headless=new --disable-gpu --hide-scrollbars \\' % EDGE)
    print('    --screenshot="%s\\%s.png" --window-size=%d,%d "file:///%s/%s.html"'
          % (DEST, name, pw, ph, DEST.replace(chr(92), "/"), name))
