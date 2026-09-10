# -*- coding: utf-8 -*-
# Draws the ERD of the model that was actually BUILT, as a vector figure.
#
# The old ERD came from the pre-project React draft: it showed tables that were
# never created and was a screenshot, so it blurred at print size. This one is
# generated from the real EF Core model in ElektriKalkulaatorContext.cs, laid out
# for a 580 pt wide page, and rendered at 3x so it stays sharp on paper.
#
# Only the columns that carry meaning are listed. A thesis figure that reprints
# every CreatedAt/ModifiedAt column is unreadable at page width, and those
# columns say nothing about the design.
import io
import os

W, H = 580, 424
COL = [10, 206, 402]        # x of the three columns
BW = 168                    # box width
HDR = 18.0                  # header band height
ROW = 12.6                  # one column row
PAD = 5.0

INK      = "#1D2724"        # box outline + header fill
INK_SOFT = "#5C6B65"        # relationship lines
TEXT     = "#1A211F"
MUTED    = "#6E7C77"
PK       = "#B4501C"        # primary key marker
FK       = "#2F6B57"        # foreign key marker
FONT     = "Segoe UI, Calibri, Arial, sans-serif"

out = []


def esc(t):
    return (t.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;"))


def box(x, y, title, subtitle, rows):
    """rows: list of (marker, name, type) where marker is '' | 'PK' | 'FK'."""
    h = HDR + PAD + len(rows) * ROW + PAD
    out.append(
        '<rect x="%.1f" y="%.1f" width="%.1f" height="%.1f" rx="3.5" '
        'fill="#FFFFFF" stroke="%s" stroke-width="0.9"/>' % (x, y, BW, h, INK))
    out.append(
        '<path d="M%.1f %.1f h%.1f v%.1f a3.5 3.5 0 0 0 -3.5 -3.5 h-%.1f '
        'a3.5 3.5 0 0 0 -3.5 3.5 z" fill="%s"/>'
        % (x, y + HDR, BW, -(HDR - 3.5), BW - 7, INK))
    out.append(
        '<text x="%.1f" y="%.1f" font-family="%s" font-size="8.2" '
        'font-weight="600" fill="#FFFFFF">%s</text>'
        % (x + 7, y + 12.4, FONT, esc(title)))
    if subtitle:
        out.append(
            '<text x="%.1f" y="%.1f" text-anchor="end" font-family="%s" '
            'font-size="6.2" fill="#B8C6C0">%s</text>'
            % (x + BW - 7, y + 12.2, FONT, esc(subtitle)))

    ty = y + HDR + PAD + 8.6
    for marker, name, typ in rows:
        if marker:
            colour = PK if marker == "PK" else FK
            out.append(
                '<text x="%.1f" y="%.1f" font-family="%s" font-size="5.6" '
                'font-weight="700" fill="%s">%s</text>'
                % (x + 7, ty - 0.4, FONT, colour, marker))
        out.append(
            '<text x="%.1f" y="%.1f" font-family="%s" font-size="7.3" '
            'fill="%s"%s>%s</text>'
            % (x + 25, ty, FONT, TEXT,
               ' font-weight="600"' if marker else "", esc(name)))
        if typ:
            out.append(
                '<text x="%.1f" y="%.1f" text-anchor="end" font-family="%s" '
                'font-size="6.4" fill="%s">%s</text>'
                % (x + BW - 7, ty, FONT, MUTED, esc(typ)))
        ty += ROW
    return h


def line(pts, dashed=False):
    d = "M" + " L".join("%.1f %.1f" % p for p in pts)
    out.append('<path d="%s" fill="none" stroke="%s" stroke-width="0.9"%s/>'
               % (d, INK_SOFT, ' stroke-dasharray="3 2.5"' if dashed else ""))


def card(x, y, txt):
    out.append(
        '<text x="%.1f" y="%.1f" text-anchor="middle" font-family="%s" '
        'font-size="7" font-weight="700" fill="%s">%s</text>'
        % (x, y, FONT, INK_SOFT, txt))


def note(x, y, txt, anchor="middle", size=6.4, style=""):
    out.append(
        '<text x="%.1f" y="%.1f" text-anchor="%s" font-family="%s" '
        'font-size="%.1f" fill="%s"%s>%s</text>'
        % (x, y, anchor, FONT, size, MUTED, style, esc(txt)))


# --------------------------------------------------------------- boxes
Y1 = 16.0
h_users = box(COL[0], Y1, "AspNetUsers", "Identity", [
    ("PK", "Id", "nvarchar"),
    ("", "Email", "nvarchar"),
    ("", "PasswordHash", "nvarchar"),
    ("", "… Identity väljad", ""),
])

h_calc = box(COL[1], Y1, "PowerboxCalculation", "arvutuse päis", [
    ("PK", "Id", "uniqueidentifier"),
    ("FK", "UserId", "null lubatud"),
    ("", "Status", "nvarchar"),
    ("", "TotalCost", "decimal(10,2)"),
    ("", "CreatedAt", "datetime2"),
])

h_req = box(COL[2], Y1, "PowerboxRequirements", "sisestatud andmed", [
    ("PK", "Id", "uniqueidentifier"),
    ("FK", "CalculationId", "unique"),
    ("", "BuildingType", "nvarchar"),
    ("", "RoomCount", "int"),
    ("", "SocketCount / LightCount", "int"),
    ("", "HasElectricStove", "bit"),
])

Y2 = 158.0
h_rule = box(COL[0], Y2, "CalculationRules", "EVS-HD 60364", [
    ("PK", "Id", "uniqueidentifier"),
    ("", "BuildingType", "nvarchar"),
    ("", "CircuitType", "nvarchar"),
    ("", "WireCrossSectionMm2", "decimal(5,2)"),
    ("", "BreakerAmperes", "int"),
    ("", "EvsReference", "nvarchar(50)"),
])

h_comp = box(COL[1], Y2, "PowerboxComponents", "materjalirida", [
    ("PK", "Id", "uniqueidentifier"),
    ("FK", "CalculationId", "uniqueidentifier"),
    ("FK", "ProductId", "uniqueidentifier"),
    ("", "Quantity", "int"),
    ("", "UnitPrice", "decimal(10,2)"),
    ("", "TotalPrice", "decimal(10,2)"),
])

h_prod = box(COL[2], Y2, "Products", "tootekataloog", [
    ("PK", "Id", "uniqueidentifier"),
    ("FK", "CategoryId", "uniqueidentifier"),
    ("", "Name / Brand", "nvarchar"),
    ("", "Price", "decimal(10,2)"),
    ("", "RatedCurrent", "decimal(10,2)"),
    ("", "WireCrossSectionMm2", "null lubatud"),
    ("", "StockQuantity", "int"),
])

Y3 = 302.0
h_cat = box(COL[2], Y3, "ProductCategories", "kategooria", [
    ("PK", "Id", "uniqueidentifier"),
    ("", "Name", "nvarchar"),
    ("", "Description", "nvarchar"),
])

# ---------------------------------------------------------- relations
R = COL[0] + BW          # 178   right edge of column A
L2 = COL[1]              # 206   left  edge of column B
R2 = COL[1] + BW         # 374
L3 = COL[2]              # 402
MIDB = COL[1] + BW / 2   # 290
MIDC = COL[2] + BW / 2   # 486

# 1:N  user -> calculation
y = Y1 + 34
line([(R, y), (L2, y)])
card(R + 6, y - 3, "1")
card(L2 - 6, y - 3, "N")

# 1:1  calculation -> requirements
line([(R2, y), (L3, y)])
card(R2 + 6, y - 3, "1")
card(L3 - 6, y - 3, "1")

# 1:N  calculation -> components
line([(MIDB, Y1 + h_calc), (MIDB, Y2)])
card(MIDB - 7, Y1 + h_calc + 10, "1")
card(MIDB - 7, Y2 - 5, "N")

# 1:N  product -> components
yc = Y2 + 70
line([(L3, yc), (R2, yc)])
card(L3 - 6, yc - 3, "1")
card(R2 + 6, yc - 3, "N")

# 1:N  category -> product
line([(MIDC, Y3), (MIDC, Y2 + h_prod)])
card(MIDC + 7, Y3 - 5, "1")
card(MIDC + 7, Y2 + h_prod + 10, "N")

# CalculationRules has no foreign key - joined in C# by BuildingType.
ygap = 133.0
# Drawn in two runs with a gap around x = MIDB so it reads as passing UNDER the
# 1:N line above it, rather than touching it.
line([(COL[0] + BW / 2, Y2), (COL[0] + BW / 2, ygap), (MIDB - 8, ygap)], dashed=True)
line([(MIDB + 8, ygap), (MIDC, ygap), (MIDC, Y1 + h_req)], dashed=True)
note(COL[0] + BW / 2 + 6, ygap - 5,
     "seos tekib koodis, mitte andmebaasis", anchor="start")
note(MIDC - 6, ygap - 5, "BuildingType", anchor="end")

# ------------------------------------------------------------- legend
ly = 372.0
out.append('<line x1="10" y1="%.1f" x2="570" y2="%.1f" stroke="#D8DEDB" '
           'stroke-width="0.8"/>' % (ly, ly))
out.append('<text x="10" y="%.1f" font-family="%s" font-size="6.6" '
           'fill="%s"><tspan font-weight="700" fill="%s">PK</tspan>'
           '<tspan fill="%s"> primaarvõti  </tspan>'
           '<tspan font-weight="700" fill="%s">FK</tspan>'
           '<tspan fill="%s"> võõrvõti  </tspan>'
           '<tspan fill="%s">1–N üks mitmele  1–1 üks ühele</tspan>'
           '</text>' % (ly + 11, FONT, MUTED, PK, MUTED, FK, MUTED, MUTED))
note(570, ly + 11,
     "Katkendjoon tähendab, et andmebaasis võõrvõtit ei ole.",
     anchor="end")
note(10, ly + 24,
     "Joonis on koostatud EF Core mudelist (ElektriKalkulaatorContext.cs). "
     "Ajatemplid CreatedAt/ModifiedAt on igas tabelis ja on loetavuse huvides "
     "välja jäetud.", anchor="start", size=6.2)

svg = ('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 %d %d" '
       'width="%d" height="%d">'
       '<rect width="%d" height="%d" fill="#FFFFFF"/>%s</svg>'
       % (W, H, W * 3, H * 3, W, H, "".join(out)))

HERE = os.path.dirname(os.path.abspath(__file__))
svg_path = os.path.join(HERE, "erd.svg")
io.open(svg_path, "w", encoding="utf-8").write(svg)

html = ('<!doctype html><meta charset="utf-8">'
        '<style>html,body{margin:0;padding:0;background:#fff}</style>' + svg)
io.open(os.path.join(HERE, "erd.html"), "w", encoding="utf-8").write(html)
print("svg %d bytes, canvas %dx%d -> render %dx%d" % (len(svg), W, H, W * 3, H * 3))
