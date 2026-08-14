# Design guide

What "good design" means **for this project specifically**, and exactly how to implement it.

Written for a beginner programmer and for AI models. Every instruction is concrete enough to act on
without further research, and every rule says *why* — a rule you do not understand is a rule you
will break the first time it is inconvenient.

---

# Part 1 — The governing principle

## Your users are not shopping. They are looking something up.

An electrician pricing a job, or a company buying panel components, is not browsing for pleasure.
They arrive with a specific question — *what do I need, and what will it cost?* — and they judge the
site on how fast it answers.

This single fact decides every choice below, and it means **most "beautiful website" advice is wrong
for you**. Large hero videos, scroll animations, lifestyle photography and clever transitions all
add time between the question and the answer.

### The strongest evidence for this

**[McMaster-Carr](https://www.mcmaster.com)** sells industrial parts and is widely considered one of
the best-designed commerce sites in existence — by engineers, not by design awards. Its design is
deliberately plain, almost wireframe-like. It is server-rendered HTML with very little JavaScript,
because its users need to find a part *now*. It has no AI recommendations and no featured products.

Read that as permission. Your project is already server-rendered Razor with no framework overhead,
which is the same architecture for the same reason. **You do not need to become a fashion site to be
well designed.** You need to become the fastest, clearest way to answer an electrician's question.

### The three questions every page must answer instantly

1. **What is this?**
2. **What does it cost, and is that price complete?**
3. **What do I do next?**

If a page element does not serve one of those, it is decoration, and decoration costs attention.

---

# Part 2 — The design system

Design consistency is not an aesthetic preference; it is how a user learns your interface once
instead of relearning it on every page. The way to get it is to **stop inventing values** and pick
from a fixed set.

The project already has colour tokens. It has **no spacing scale and no type scale**, which is why
there are currently 19 inline `style="…"` attributes in the views and 13 different font sizes in the
CSS — each one a small decision made in isolation.

## 2.1 Add these tokens

Put this in `wwwroot/css/site.css`, extending the existing `:root` block.

```css
:root {
    /* ── EXISTING COLOURS — do not change, they are used everywhere ── */
    --bg-primary:   #0f0f1a;
    --bg-card:      #16213e;
    --bg-input:     #1f2937;
    --bg-nav:       #0d0d1a;
    --accent-amber: #F5A623;
    --accent-blue:  #3B82F6;
    --accent-green: #10B981;
    --text-primary: #e5e7eb;
    --text-muted:   #9ca3af;
    --border-color: #2d3748;

    /* ── SPACING SCALE ──────────────────────────────────────────────
       Every margin, padding and gap uses one of these. Six options is
       enough for any layout; unlimited options is what produces a page
       where nothing quite lines up. Based on 4px because it divides
       evenly at every common screen size. */
    --space-1: 4px;    /* hairline gaps, icon-to-text */
    --space-2: 8px;    /* inside small elements, badge padding */
    --space-3: 16px;   /* the default — between related things */
    --space-4: 24px;   /* inside cards, between form fields */
    --space-5: 40px;   /* between sections of a page */
    --space-6: 64px;   /* above and below major page regions */

    /* ── TYPE SCALE ─────────────────────────────────────────────────
       Sizes step by roughly 1.25x. Anything outside this list is a
       decision that has to be justified, which is the point. */
    --text-xs:   0.75rem;   /* 12px — badges, table captions */
    --text-sm:   0.875rem;  /* 14px — secondary text, help text */
    --text-base: 1rem;      /* 16px — body. NEVER smaller for reading text */
    --text-lg:   1.125rem;  /* 18px — card titles */
    --text-xl:   1.5rem;    /* 24px — page titles */
    --text-2xl:  2rem;      /* 32px — hero */

    /* ── OTHER ──────────────────────────────────────────────────── */
    --radius:     8px;      /* one corner radius everywhere */
    --radius-lg:  12px;     /* cards and panels only */
    --border:     1px solid var(--border-color);
    --transition: 0.15s ease;   /* fast enough to feel instant */
}
```

## 2.2 The colour rule that is currently being broken

Right now amber is used for **prices, primary buttons, the logo, badges and highlights** all at
once. When everything is the accent colour, nothing is.

Give each colour one job:

| Token | Means, and ONLY means |
|---|---|
| `--accent-amber` | **The next action.** The primary button on a page, and nothing else. |
| `--accent-green` | Success, "in stock", trust. Never a button. |
| `--accent-blue` | Information, links, technical badges. |
| red (`#ef4444`) | Errors and destructive actions only. |
| `--text-primary` / `--text-muted` | All ordinary text and all prices. |

**Prices should not be amber.** A price is information, not an action. Making it the same colour as
the "Add to cart" button teaches the eye to stop distinguishing them. Prices should be
`--text-primary`, larger and bolder than surrounding text — weight and size carry the emphasis, not
hue.

**Rule of thumb: one amber element per screen.** If you can see two primary buttons at once, one of
them is not primary.

---

# Part 3 — Page-by-page instructions

Ordered by impact. Each item states the problem, the fix, and how to tell it worked.

## 3.1 Catalogue (`/Products`) — highest impact

### A. Add sorting *(missing entirely)*

**Problem:** the catalogue cannot be sorted. Price is the deciding factor for most buyers, and they
currently cannot order by it. Baymard lists sorting among the essentials of a product list.

**Fix:** add a `sort` parameter to `IProductServices.Search` and a dropdown next to the search box:

```
Sorteeri:  [ Nimi A–Z ▾ ]   Hind: odavaim enne   Hind: kallim enne   Laoseis
```

Apply it inside the existing `IQueryable` **before** `ToListAsync()` so it becomes SQL `ORDER BY`,
exactly as the filtering already does.

**Verify:** sorting by price ascending puts the €1.20 cable first and the €42.00 RCD last.

### B. Show the result count above the list, not only below

Currently "Näidatakse 10 toodet" appears only underneath. Users check the count *before* scanning,
to decide whether to narrow the filter first.

### C. Show price per unit

The calculator now says `1.20 €/m`. The catalogue still says `1.20 €` for the same cable, which
reads as the price of a whole reel. Reuse `BOMItemDto.Unit`'s logic: cable is per metre.

### D. Multi-select filters instead of one dropdown

A single-select category dropdown means a user can never ask for "breakers **and** cables". Replace
with checkboxes once the catalogue outgrows ~20 products — **not before**, since at 10 products a
dropdown is genuinely fine and checkboxes would be over-engineering.

## 3.2 Product page (`/Products/Details`)

**The pattern to copy from McMaster-Carr:** core facts above the fold, everything else in
accordions.

Above the fold: name, image, price, stock, and a **specification table**. Technical buyers read
tables, not prose. You already store the values — present them as data:

| | |
|---|---|
| Nimivool | 16 A |
| Nimipinge | 230 V |
| Juhtme ristlõige | 2.5 mm² |
| Tootja | ABB |

**Add two fields the catalogue is missing** and every real supplier shows (see `RESEARCH_LOG.md`):
- **Manufacturer part code** — how a tradesperson actually identifies a component
- **Datasheet link** — what makes the page usable for specification work

## 3.3 Calculator (`/Calculator`) — your differentiator

The trust strip and per-unit pricing are done. Two things remain:

### A. Make the result printable

An electrician's real workflow is: calculate → **show the client** → order. A printable BOM is the
single most valuable missing feature, and it is nearly free:

```css
@media print {
    .navbar, .footer, .trust-strip, form, .btn { display: none; }
    body { background: #fff; color: #000; }
    .card { border: 1px solid #ccc; }
}
```

That alone turns the page into a quote document. Later, a real PDF export
(`docs/PROJECT_ROADMAP.md` §D3 already plans it).

### B. Keep the input form visible beside the result

After calculating, the user's next thought is usually *"what if it were 6 rooms?"*. If the form is
still on screen they can iterate; if they must scroll back up, they often will not.

## 3.4 Homepage

The hero photo is in place. What is missing is **proof it works**. Replace one of the three feature
cards with a worked example — real numbers from the real calculator:

> **3-room apartment → €504.10**
> 2 × B10 breakers, 2 × B16, 1 × B32, 160 m cable, RCD, enclosure — calculated to EVS-HD 60364.

Showing the output is more persuasive than describing the input.

---

# Part 4 — Rules that apply everywhere

## 4.1 Never use a font size below 14px for reading text

`--text-xs` (12px) is for badges and table captions only. Body text at 12px is a common
"looks tidy in the mockup" mistake that makes a site unusable for anyone over about 40 — a
significant share of working electricians.

## 4.2 Every interactive element needs a visible focus state

Currently missing. Without it the site cannot be used by keyboard, which also fails accessibility
requirements a thesis reviewer may well ask about.

```css
:is(a, button, input, select, textarea):focus-visible {
    outline: 2px solid var(--accent-amber);
    outline-offset: 2px;
}
```

## 4.3 Tables must scroll inside themselves, not push the page sideways

The BOM table is wide. On a phone it currently makes the whole page scroll horizontally, which
breaks every other element. Wrap wide content:

```css
.table-responsive { overflow-x: auto; }   /* Bootstrap already provides this — use it consistently */
```

## 4.4 Empty states must say what to do next

"Tooteid ei leitud" is a dead end. Add the action: *"Proovi laiemat otsingut või vaata kõiki
tooteid"* with a link that clears the filter.

## 4.5 Buttons must say what happens

"Salvesta" is fine. "OK" is not. The label should complete the sentence *"When I click this, the
system will…"*.

---

# Part 5 — What NOT to do

These will actively damage this project, and some are illegal in the EU.

| Do not | Why |
|---|---|
| Fake scarcity ("2 left!" when stock is 200) | Unfair commercial practice under EU consumer law. Also destroys the auditability claim the whole project rests on. |
| Countdown timers that reset | Same. |
| Invented "was" prices | Same. Estonia requires the prior price to be genuine. |
| Hide shipping cost until checkout | The single largest cause of cart abandonment, and it contradicts "we tell you the real cost". |
| Pre-ticked add-ons | Prohibited for extra payments under EU rules. |
| Carousels / auto-rotating banners | Users ignore them; they push real content below the fold. |
| Custom scrollbars, scroll hijacking, cursor effects | Break the platform behaviour people rely on. |
| Low-contrast grey-on-grey text | Fails WCAG AA and is unreadable on a workshop laptop in daylight. |

**The deeper reason:** this project's entire argument is that its output is trustworthy *because it
is auditable*. A manufactured-urgency banner next to a standards-compliance calculation does not
just look tacky — it contradicts the thesis.

---

# Part 6 — Implementation order

Do these in order. Each is small, testable, and independently valuable.

| # | Task | Effort | Why this order |
|---|---|---|---|
| 1 | Add spacing + type tokens (§2.1) | 30 min | Everything after this uses them |
| 2 | Focus states (§4.2) | 10 min | Accessibility, trivially cheap |
| 3 | Fix the colour roles — prices stop being amber (§2.2) | 1 h | Biggest visual improvement per hour |
| 4 | Catalogue sorting (§3.1 A) | 1–2 h | Most-missed functional gap |
| 5 | Print stylesheet for the BOM (§3.3 A) | 30 min | Turns the result into a quote |
| 6 | Specification table on the product page (§3.2) | 1–2 h | What technical buyers actually read |
| 7 | Result count above list, price per unit (§3.1 B, C) | 1 h | Small, visible polish |
| 8 | Worked example on the homepage (§3.4) | 30 min | Proof beats description |
| 9 | Replace inline `style="…"` with token classes | 2 h | Cleanup once tokens exist |

**Do not do all of this at once.** One item per commit, tested, with a `docs/CHANGELOG.md` entry —
same rule as any other change to this project.

## How to tell it worked

Design changes are easy to argue about and hard to measure. Use these checks:

- **The squint test.** Blur your eyes at a page. The most prominent thing should be the single most
  important thing. If three things compete, the hierarchy is wrong.
- **The 5-second test.** Show the calculator result to someone who has not seen it. Ask what it
  cost and whether tax is included. If they cannot answer, the page has failed.
- **Keyboard only.** Unplug the mouse and try to complete a calculation and add to cart.
- **Phone width.** At 375px nothing should scroll sideways.

---

## Sources

- [McMaster-Carr](https://www.mcmaster.com) — the reference for technical-parts commerce
- [9 Best Ecommerce UX Practices](https://medusajs.com/blog/9-best-ecommerce-ux-practices-with-examples/) — analysis of McMaster-Carr's patterns
- [Baymard Institute](https://baymard.com/research/ecommerce-product-lists) — product-list research
- Estonian competitor analysis and the UX research figures are recorded in `RESEARCH_LOG.md`
