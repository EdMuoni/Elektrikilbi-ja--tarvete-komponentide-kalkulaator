# Research log — external data gathered

Everything gathered from outside sources: market prices, competitor design analysis, UX research
findings, and image sourcing. **Append to the top; date every entry and always record the source
URL**, so any figure in the thesis can be traced back and re-checked later.

`CHANGELOG.md` records changes to the code. **This file records facts learned from the outside
world** — the two are separate because external facts go stale on their own schedule (prices
change, sites redesign) while code changes don't.

> **Warning about staleness:** prices and website designs recorded here were true on the date
> shown. Re-verify anything before quoting it in the final thesis.

---

## 2026-08-11 — Real Estonian market prices vs. the seeded prices

**Why this was researched:** the calculator's entire selling point is producing a realistic cost
estimate that replaces a *eelarvestaja* (cost estimator). If the seeded prices are wrong, every
number the calculator outputs is wrong, and that undermines the thesis's central claim.

### Findings — ABB S201 series 16 A single-pole breaker

| Source | Price | VAT | Notes |
|---|---|---|---|
| [Esvika](https://pood.esvika.ee/kilbidkapidkomponendid/kaitseluliti-1p-b16-a-6ka-s201-abb) | **5.78 €** customer / **8.26 €** list | **incl. VAT** | Exact match: ABB S201 **B16**, product code 4100460, EAN 4016779578639, mfr code 2CDS251001R1165 |
| [elektrikaup.ee](https://www.elektrikaup.ee/1-faasilised-moodulkaitselulitid-abb/873/moodulkaitseluliti-1-faasiline-c-16a-abb-s201-c16-2cds251001r0164.html) | **2.50 €** | **excl. VAT** | S201-**C16** (C-curve, not B). Seller states they are not VAT-registered — likely clearance or parallel import, so treat as an outlier, not a market rate. |
| **This project's seed data** | **9.20 €** | **unspecified** | `ElektriKalkulaatorContext.SeedProducts` |

### What this means

1. **The seeded 9.20 € is at or slightly above the highest observed retail list price** (8.26 €
   incl. VAT at Esvika). It is not absurd, but it is not a typical selling price either — a real
   customer pays closer to **5.78 €**. The calculator therefore currently **overestimates** the
   cost of a panel build.
2. **The bigger problem is that the seed data never states whether prices include VAT.** In
   Estonia that is a >20% swing on the final figure. A cost estimate that doesn't declare its VAT
   basis is ambiguous, and a reviewer may well ask. Retailers are explicit about it: Esvika says
   *"Hinnad sisaldavad käibemaksu"*, elektrikaup.ee says *"Hind ei sisalda käibemaksu"*.
3. Real shops list **two prices** (customer price and list price). The current `Product.Price` is
   a single field with no concept of a discount or a customer-specific rate.

### Recommended actions (not yet implemented)

- [ ] Decide and **document** whether `Product.Price` is with or without VAT; state it in the UI
      next to the total ("hinnad sisaldavad käibemaksu" or the reverse).
- [ ] Consider re-basing seeded prices on observed market rates so the demo produces a believable
      total.
- [ ] Mention in the thesis that prices are a static snapshot — this motivates the supplier
      price-feed work already planned in `PROJECT_ROADMAP.md` §D3.

---

## 2026-08-11 — Competitor / comparable site design analysis

**Why:** to ground UI decisions in what the Estonian market actually does, rather than guessing.

### Esvika — [pood.esvika.ee](https://pood.esvika.ee)
The **closest functional model** for this project. Notable features:
- **Dual pricing** — discounted customer price shown alongside the standard price.
- **Per-branch stock levels** — Tallinn (Mustamäe) 21, Paide 8, Tartu 13, Jõhvi 9, Tallinn
  (Lasnamäe) 6, Pärnu 0. Far more useful to a tradesperson than a single stock number.
- **Four separate identifiers per product** — internal code, EAN, manufacturer, manufacturer code.
- **Datasheet and declaration-of-conformity downloads** per product.
- Packaging quantities (1/10/120) — trade buyers order in boxes.

### Onninen — [onninen.ee/epood](https://www.onninen.ee/epood)
Pure B2B wholesale. Registration restricted to company representatives.
- Personalised contract pricing per account.
- Saved and **shareable shopping lists** (colleagues collaborate on one order).
- Invoice download, multi-user account management.
- "Click and Collect" with ~2-hour pickup.
- Deep category hierarchy (Automation, Electrical materials, Heating/Cooling, Pumps, Ventilation).

### Elektrikaubad.ee — [elektrikaubad.ee](https://www.elektrikaubad.ee/en)
Schneider Electric specialist. Consumer-facing, minimal.
- Clean white layout, eight top-level categories in a horizontal menu.
- Category cards each show **three stacked product images** — conveys range at a glance.
- **Four languages** (EN / ET / RU / FR) — confirms multilingual support is a genuine market
  expectation in Estonia, not an optional extra. Reinforces the i18n gap noted in the roadmap.
- Trust badges above the fold: *fast delivery (2–3 business days)*, *secure payments*,
  *customer support*.
- A product **comparison** feature with a counter in the header.

### Cross-cutting observations

| Pattern | Seen at | Present in ElektriKalkulaator? |
|---|---|---|
| Product photo on every list item | all three | ✅ added 2026-08-11 |
| Manufacturer + product code visible | Esvika, Onninen | ⚠️ brand only, no codes |
| Stock as a number, not a yes/no | Esvika | ✅ `StockQuantity` |
| Explicit VAT statement | Esvika, elektrikaup | ❌ **missing** |
| Datasheet / documentation links | Esvika, Onninen | ❌ |
| Multi-language | Elektrikaubad (4), Onninen | ❌ Estonian only |
| Trust badges above the fold | Elektrikaubad | ❌ |
| Product comparison | Elektrikaubad | ❌ |
| Saved lists / reorder | Onninen | ❌ (no accounts) |

---

## 2026-08-11 — UX and design-psychology research

**Why:** to justify design decisions with published evidence rather than personal taste — which is
also what makes them defensible in a thesis.

| Finding | Figure | Source |
|---|---|---|
| Users form an aesthetic judgement of a page in ~50 ms, and it rarely changes with more time | 50 milliseconds | Lindgaard et al. (2006), *Attention Web Designers: You have 50 milliseconds to make a good first impression* |
| When asked why they trusted or distrusted a site, people cited **visual design** first, ahead of content | ~46 % | Stanford Web Credibility Project |
| Desktop e-commerce sites with "mediocre or worse" product-list UX | 58 % | [Baymard Institute](https://baymard.com/research/ecommerce-product-lists) |
| Mobile sites with "poor to mediocre" product-list UX | 78 % | Baymard Institute |
| Baymard's product-list research base | 25 rounds of moderated testing, 4,400+ sessions | Baymard Institute |

### Baymard guidelines most relevant to this project
- Show the **total item count** above and below the list. *(partially present — "Näidatakse N toodet" appears below only)*
- Prefer a **"Load more" button** over infinite scroll or pure pagination.
- Always use **checkboxes** for filters; allow combining multiple values. *(currently single-select dropdown)*
- Provide **5 essential filter types** and **4 sort types** (price, rating, best-selling, newest).
  *(currently: category + text search, no sorting at all)*
- Show **applied filters in an overview** so users can see and remove them.
- Use **font weight, spacing and line breaks** to separate product name / brand / specs so the
  list can be scanned. *(already done reasonably well)*
- Aim for **~10 subcategories max**, and at least 10 products at the deepest level.
  *(currently 5 categories, 10 products — within guidance but thin)*
- Display **price per unit** where volumes vary — directly relevant, since cable is priced per
  metre while breakers are priced per piece.

---

## 2026-08-11 — Image sourcing

**Why:** the catalogue needed real product photography, but manufacturer images are copyrighted.

- **Rejected:** scraping ABB / Schneider / Hager product photos. Copyrighted; a real distributor
  gets a media kit, a student project does not.
- **Rejected:** in-house SVG illustrations (built first, then removed). They were accurate and
  copyright-free, but real photographs are more convincing, and mixing SVG with uploaded JPGs
  would mean two formats in one catalogue.
- **Chosen:** freely-licensed photographs from **Wikimedia Commons** (CC0 and CC BY-SA) for
  products, and **Unsplash** / **Pexels** for hero imagery. All stored as `.jpg`.
- Full attribution is recorded in **`IMAGE_CREDITS.md`** — required by the CC BY-SA licence on
  `breaker.jpg`.
- Practical note: Wikimedia's thumbnail service only serves **certain widths**. Requesting an
  arbitrary width (800 px) returns **HTTP 400**; the widths advertised by the API (e.g. 960 px)
  work. It also rate-limits — space requests ~2 s apart and send a descriptive `User-Agent`.
- Also found but **not used**: an official **ABB** photo of a *Miniature Circuit Breaker S203 B16*
  released under CC BY-SA 3.0 on Commons. Rejected only because it is a **3-pole** breaker while
  the catalogue's S201 items are **1-pole** — the chosen Chint photo has the correct form factor.
  Worth revisiting if a 3-pole product is ever added.
