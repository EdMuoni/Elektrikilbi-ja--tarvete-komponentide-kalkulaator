# Changelog — ElektriKalkulaator

Every code change to this project gets an entry here. **Append to the top, never rewrite history.**
This file exists so that a future developer — human or AI — can answer "why is this code like
this?" without having to guess, and without needing the chat conversation that produced it.

`PROJECT_ROADMAP.md` says *where the project is going*. This file says *what actually happened and
why*. Read both.

---

## How to write an entry

Copy this template. Keep it factual. **"Why" is the most important field** — the code itself
already shows *what* changed; only a human/AI writing at the time knows *why*.

```markdown
## YYYY-MM-DD — Short title of the change

**Type:** feature | bugfix | refactor | test | docs | chore | security
**Author:** who or what made the change (e.g. "Edgar", "Claude (Sonnet 5) + Edgar")

**What changed**
- Bullet list of the actual edits, with file paths.

**Why**
- The reasoning. What problem did this solve? What would break or stay broken without it?

**How it was verified**
- Build passed / tests run / manually tested in browser / etc. Be specific — "it compiles" is
  not the same as "it works".

**Follow-ups or known limitations**
- Anything deliberately left undone, and why. Write "none" if there genuinely are none.
```

**Rules of thumb**
- One entry per logical change, not per file touched.
- If you fix a bug, say what the bug *was* — the symptom, not just the fix.
- If you make a decision with a non-obvious rationale, also add a row to the decision ledger in
  `PROJECT_ROADMAP.md` (§C3). The changelog records events; the ledger records standing decisions.
- Never delete or edit an old entry to make the history look tidier. If an old entry turns out to
  be wrong, add a new entry correcting it.

---

## 2026-09-12 — Removed a committed admin session cookie

**Type:** security
**Author:** Claude (Opus 5) + Edgar

**What changed**
- Deleted `admin.txt` from the repository root.
- `.gitignore` now excludes `admin.txt`, `*.cookies`, `cookies*.txt` and
  `.security-check-cookies.tmp`.

**Why**
- `admin.txt` was not a note. It was a curl cookie jar holding a live
  `.AspNetCore.Identity.Application` cookie — a logged-in **administrator** session — plus an
  antiforgery cookie. It was committed in `44773b3` on 2026-09-09 and pushed to the public
  GitHub repository on branch `feat/conversion-ux`.
- The practical risk is low: the cookie is scoped to `localhost` and is signed with
  data-protection keys that exist only on Edgar's machine, and there is no deployed server it
  could be replayed against. It is still an authentication token in a public repository of a
  thesis whose subject includes security, and a reviewer opening the repo would find it.
- It did not come from `scripts/security-check.sh`, which writes its jar to
  `.security-check-cookies.tmp`. It was most likely a manual `curl -c admin.txt` login. That jar
  was not ignored either, hence the broader pattern.

**Not done, deliberately**
- **Git history was not rewritten.** The cookie remains readable in commit `44773b3` on GitHub.
  Removing it from history means rewriting and force-pushing a branch that already has an open
  pull request; Edgar approved removal and push, not a history rewrite. The cleaner fix is to
  make the token worthless: changing the local admin password rotates the Identity security
  stamp, which invalidates every cookie issued before it.

**How it was verified**
- `git ls-files admin.txt` returns nothing after the commit; `git check-ignore admin.txt`
  confirms the new rule matches.

---

## 2026-09-12 — One generator for both ERDs; removed the in-figure source note

**Type:** docs
**Author:** Claude (Opus 5) + Edgar

**What changed**
- `scripts/generate-erd.py` — restructured into shared drawing routines that emit **two**
  diagrams: `ERD_ElektriKalkulaator` (7 tables, the built model) and `ERD_Kavandatud`
  (12 tables, the planning-stage model).
- Removed the grey two-line source note from the figure footer. The notation key stays.
- Thesis `Elektrikilbi_v8.docx`: both figures replaced, Joonis 1 at 435×300 pt and
  Joonis 10 at 435×442 pt.

**Why**
- Joonis 10 had been drawn by a different session and merely resembled Joonis 1. Two diagrams
  of the same subject in one document that only nearly match look like an oversight. Generating
  both from the same routines means they cannot diverge again.
- The in-figure note said which file the diagram came from and that CreatedAt/ModifiedAt were
  omitted. The caption underneath already identifies the figure, so it was duplication printed
  inside the image, where a reader cannot skip it.
- The designed model shows field names **without types** while the built one shows types. Not an
  inconsistency: 12 tables share the same 435 pt page width, and adding types pushes the text
  below legible size on paper. Names alone carry what that figure exists to show.

**How it was verified**
- Both PNGs inspected at full render size. The first pass had the PRODUCT/PRODUCT_SPEC
  cardinality reversed — a product has zero or one spec, a spec belongs to exactly one product
  — and 70 pt of dead vertical space between rows. Both fixed and re-rendered.
- v8 measured through Word: **37 pages, 6 223 words, 10 figures, 16 tables, zero stranded
  headings, no blank pages.**

---

## 2026-09-11 — Redrew the ERD in crow's foot notation

**Type:** docs
**Author:** Claude (Opus 5) + Edgar

**What changed**
- `scripts/generate-erd.py` — rewritten. Each table now has a coloured header, a PK/FK gutter
  with cell borders, the primary key underlined on a tinted first row, and `name (type)` fields.
  Cardinality is carried by crow's foot connectors (`foot()`) instead of "1"/"N" text: bar for
  exactly one, hollow circle for optional, crow's foot for many. A legend explains the notation.
- Column gap widened from 15 pt to 35 pt and box width reduced 178 → 166 pt.

**Why**
- Edgar showed a reference ERD in the conventional crow's foot style and asked why the diagram
  could not look like that. It could; the first version was simply drawn in a plainer style.
  Crow's feet also carry more information than "1"/"N" labels — they distinguish *zero* or many
  from *one* or many, which matters here because `PowerboxCalculation.UserId` is nullable and the
  diagram should show that a calculation can exist with no user attached.
- The 35 pt column gap is not cosmetic. Two cardinality symbols meet in every horizontal gap and
  each needs about 15 pt of clear line; at the original 15 pt they overlapped into an unreadable
  blob, which is what the first render showed.
- The reference diagram Edgar supplied is the *planned* model — it has `name_et` / `name_en` /
  `name_rus`, `rule_logic (json)` and `component_name_et`, none of which exist in the database.
  The style was copied; the column names remain the real ones from the context class.

**How it was verified**
- Rendered and inspected at 1740 × 1248 px. The first render had the user–calculation symbols
  colliding; fixed by widening the gap and re-rendered.
- Rebuilt the document: **36 pages, 5 970 words, 10 figures**, unchanged by the swap.
- Word reports the ERD at 435 × 312 pt on page 15, full text-column width (column is 453 pt).

---

## 2026-09-10 — Removed the school appendices, redrew the ERD from the real EF Core model

**Type:** docs
**Author:** Claude (Opus 5) + Edgar

**What changed**
- `scripts/build-thesis-docx.js` — deleted the entire LISAD section (Lisa A–E, ~7 000 characters).
- `scripts/generate-erd.py` — new. Generates the entity-relationship diagram as SVG from the
  model declared in `ElektriKalkulaatorContext.cs`, then it is rendered to PNG at 3x
  (`Pictures/Diagrams/ERD_ElektriKalkulaator.png`, also kept as `.svg`).
- `scripts/build-thesis-docx.js` — chapter 1.3 now shows that generated ERD instead of the
  draft screenshot; chapter 3 gained two new sections, "Kavandatud ja realiseeritud andmemudeli
  vahe" (which is where the wide draft ERD now lives, correctly labelled as a planning artefact)
  and "Soovitused sarnase töö tegijale".

**Why**
- Edgar asked whether the appendices were actually required by the school template. They are not.
  The template's LISA A–E are the e-CF competency table the student is *assessed against*, the
  assessment criteria, and a referencing guide — reference material inside the template, not
  content to reproduce. Kalle Olumets' exam work, the model for this document, has no appendices;
  his contents list ends at "5. KASUTATUD ALLIKAD". Lisa A also duplicated the ERD already shown
  in 1.3 and Lisa B repeated the algorithm already described in 1.4. They were there to reach a
  page count, which is the wrong reason to put anything in a thesis.
- JÄRELDUSED JA SOOVITUSED, which Edgar questioned in the same breath, **is** required: it is a
  Pealkiri1 heading in `LÕPUTÖÖ_TEMPLATE (1).docx` and appears in Kalle's contents as
  section 3. It stays.
- The old ERD was a screenshot from the pre-project React draft. It showed tables that were never
  built (AI_SERVICE, PRODUCT_TRANSLATION, PRODUCT_SPEC, BRAND) and blurred at print size, so the
  thesis was illustrating its own data model with a picture of a different data model. Generating
  the figure from the context class means it cannot drift from the code the way a hand-drawn
  diagram does.
- The draft ERD was not discarded: shown against the built model it demonstrates the gap between
  what was planned and what fitted into 156 hours, which is a genuine finding and belongs in the
  conclusions chapter.

**How it was verified**
- Rebuilt the document; Word COM reports **36 pages, 5 970 words, 10 figures** (was 39 / 6 301 / 10
  with the appendices).
- Every figure measured through Word: all render at 420–435 pt against a 453 pt text column, so
  none is cropped or shrunk. The new ERD occupies the full column at 435 × 318 pt on page 15.
- The generated PNG was inspected directly; a first render had the dashed "no foreign key" line
  running through its own label, which was fixed by breaking the path around the crossing.

---

## 2026-09-09 — Fixed the white BOM table, added component photos, centred the navigation

**Type:** bugfix + feature
**Author:** Claude (Opus 5) + Edgar

**Bugs fixed**
1. **The calculator's results table rendered as a white block in dark mode.** Bootstrap's `.table`
   sets `--bs-table-bg` to `var(--bs-body-bg)` — *Bootstrap's* body background, which is white and
   knows nothing about our theme — and then paints every cell with it. It looked right in light
   mode purely by coincidence. `.table` now points every `--bs-table-*` variable at our tokens and
   makes the cell background transparent so the card behind shows through.
2. **The BOM total row had silently lost its highlight.** Its tint and accent top border were set
   on the `<tr>`, but Bootstrap paints the **cells**, so both were drawn over. Verified in the
   browser: the row computed as fully transparent with an ordinary grey border. Moved to
   `.bom-total-row > td`, which now computes as the amber tint with the orange border.
3. **Number inputs began with "0" and typing appended to it,** so entering 2 gave "02" — easy to
   submit without noticing. `site.js` clears a value of exactly `0` on focus and restores the
   field's own `min` on blur if it was left empty. Only `0` is cleared, so correcting 12 to 13
   still works normally.

**What changed**
- **Component photographs in the BOM.** `BOMItemDto` carries `ImagePath` and the results table
  shows a thumbnail beside each part. "ABB S201-B32" means nothing to a non-electrician; a picture
  of a breaker does, and it lets a professional confirm at a glance that the right kind of part was
  chosen. All 8 rows of the standard example resolve to a real photo.
- **Navigation restructured into three groups:** brand left, page links **centred** via `mx-auto`,
  account controls and the theme switch right. The switch is now last, at the far right edge — it
  is a setting, not a destination, so it does not belong among the page links.
- **Dark surfaces returned to a deep green-charcoal** (~158°) at Edgar's request, keeping the
  orange accent and promoting yellow to a real secondary accent. Light mode keeps its warm bone —
  the two are deliberately different families, because a green-tinted *white* reads as clinical
  rather than warm.
- Removed the dead `.table-dark` rule; `ThemeTokenTests` forbids that class in views.

**How it was verified**
- All four fixes confirmed **in the browser against the running app**, not just in code: cell
  background transparent, total-row cell `rgba(242,118,75,0.14)` with an orange top border, page
  `rgb(15,21,19)`, 8 thumbnails present, nav toggle last inside the right-hand list.
- The zero-clearing was exercised as a person would: focus emptied the field, typing produced "3"
  rather than "03", and blurring an emptied field restored its `min`.
- `scripts/check-contrast.py`: **0 below AA**, elevation 1.47 dark / 1.14 light.
- Build clean with `-warnaserror`; **210/210 tests pass.**

**Follow-ups or known limitations**
- **No test covers any of the three bugs.** They were all visual or interaction defects, which is
  the gap `docs/TESTING.md` Part 7 already names as the biggest one. A test asserting that no
  Bootstrap component variable is left at its default would have caught bugs 1 and 2.
- Product photos have white backgrounds, so the thumbnails read as bright squares on the dark
  table. Real product photography on a transparent or neutral background would fix it.
- **iStock images were requested and not used** — they are licensed stock, watermarked and sold
  per image, so they cannot be downloaded and shipped. Free alternatives are noted in
  `docs/IMAGE_CREDITS.md`.

## 2026-09-09 — Brand filtering, breadcrumbs and filter chips, from studying electromaterial.com

**Type:** feature + bugfix + test
**Author:** Claude (Opus 5) + Edgar

**What changed**
- **Brand filtering**, new through the whole stack: `IProductServices.Search` gained a `brand`
  argument and a `GetBrands()` companion; the controller accepts `?brand=`; the catalogue shows a
  brand chip row.
- **Breadcrumbs** on the catalogue (`Avaleht › Tooted › <filter>`).
- **Filter chips** replace the category dropdown. Category and brand are now both visible rows.
- The page subtitle is now **generated from the brands that exist** rather than typed by hand.

**Why**
- Studying electromaterial.com showed the one navigation pattern we were missing: their category
  pages are browsed **by brand**, not just by category. That is how the trade actually shops — an
  electrician often knows they want Schneider before they know which category the part is filed
  under, because the brand is what is already installed in the building. We stored `Brand` on every
  product and offered no way to filter by it.
- `brand` is an **exact** match while the existing `searchTerm` stays partial. A partial brand
  filter still returns plausible-looking results with other brands mixed in, and nobody notices
  until someone orders the wrong part.
- Chips over a dropdown: a closed `<select>` hides both what is available and what is selected.
- What was deliberately **not** copied from them: prices are shown at every level here, where they
  make you click twice more; and their long unstructured sidebar suits ~1,900 categories, not 5.

**Bug found and fixed**
- The catalogue subtitle and a home page feature card both advertised **"ABB, Schneider ja Hager"**.
  The seeded catalogue contains **ABB, Draka and Schneider** — there is no Hager product, and Draka,
  which supplies every cable, went unmentioned. Same class of error as the 160 m cable figure: copy
  that contradicts the data. The catalogue subtitle is now generated from `GetBrands()` so it cannot
  drift again; the home page card was corrected by hand.

**How it was verified**
- 7 new tests in `CatalogueBrandFilterTests`, **mutation-tested**: turning the exact match into
  `Contains`, ignoring the brand filter entirely, and removing `Distinct()` from `GetBrands()` each
  turned the matching test red, and all passed again after reverting.
- Exercised against the running app: `brand=ABB` → 5, `Draka` → 3, `Schneider` → 2, unfiltered → 10
  (they add up), and `brand=AB` → 0, confirming the exact match live rather than only in a test.
- Seen rendered. Build clean with `-warnaserror`; **210/210 tests pass.**

**Follow-ups or known limitations**
- Product photos have white backgrounds and letterbox against the dark card. Known tradeoff from
  the `object-fit: contain` decision; real product photography would fix it properly.
- `Views/Products/Details.cshtml` still has no breadcrumb.
- No test asserts the home page names only brands that exist — the catalogue subtitle is now
  generated, but that one card is still hand-written.

## 2026-09-09 — Warm palette in both themes, and the theme generator moved into the repo

**Type:** feature + chore
**Author:** Claude (Opus 5) + Edgar

**What changed**
- Both palettes are now **warm neutrals** instead of cool greys. Surfaces share one hue (~20–30°,
  a warm taupe) and lose saturation as they get lighter. Light mode went from a cool grey-green
  `#F2F5F3` to a warm bone `#F2EEE8`; dark went from green-charcoal to a warm charcoal `#141210`.
- Dark-mode accents are **lighter and less saturated** than their light-mode counterparts
  (`#F79A76` vs `#A8481D` for accent text), and neither theme uses pure black or pure white.
- Elevation restored after the hue change: dark card/page **1.44**, light **1.14**.
- `scripts/generate-theme.py` — **the generator now lives in the repo.** The previous entry claimed
  theme.css was "generated from one source", but that generator sat in a temp folder and was gone
  by the next session, which made the claim false and the file hand-editable again. It now refuses
  to run if the two palette tables disagree on token names.
- `scripts/check-contrast.py` — measures every text/background pairing and exits non-zero below AA.
- Feature cards: a full accent border on all four sides became **one accent edge along the top**.

**Why**
- Edgar asked for both modes to feel warm and inviting rather than cold, and specifically for it
  not to feel overwhelming.
- **Warm vs cool** is the difference between reading as paper and linen versus screens and
  hospitals. Current practice has moved the same way — bone and sand replacing pure white and cool
  grey. For a tool used while planning work in a building, warm is the right register.
- **Dark mode** follows established accessibility guidance rather than taste: pure black against
  light text causes halation and hides elevation (you cannot see a shadow on black), and fully
  saturated accents visibly vibrate on dark backgrounds.
- **The accent reduction** applies the 60-30-10 guideline (~10% accent). Four outlined cards put
  roughly four times that on screen, and the row competed with the primary button — which is
  supposed to be the only thing shouting. That is the concrete cause of "overwhelming".

**How it was verified**
- `scripts/check-contrast.py`: 34 pairings across both themes, **0 below WCAG AA**; lowest 4.55.
- Both palettes byte-identical to their explicit-choice duplicates; same 36 token names.
- **Seen rendered this time.** Screenshots worked in this session (they did not previously), so
  both themes were viewed in a browser rather than only measured. The toggle was clicked and
  verified: `data-theme="dark"`, `localStorage` = `dark`, band computed as `rgb(243,238,231)` —
  warm cream on a dark page, so the inversion works.
- Build clean with `-warnaserror`; **203/203 tests pass.**

**Follow-ups or known limitations**
- Light-mode card/page separation is 1.14, relying on the shadow. Normal for a light theme.
- The browser pane could not screenshot scrolled content, so the band and lower sections were
  confirmed by computed style rather than by eye.
- `docs/DESIGN_GUIDE.md` Part 2's reference palette lists the previous green values and is now
  stale again. It points at theme.css as the source, so it misleads rather than breaks.

## 2026-08-14 — Full redesign against real industry references, and a false figure on the landing page

**Type:** feature + bugfix + test
**Author:** Claude (Opus 5) + Edgar

**What changed**

*The two-genre principle (the main design decision)*
- Edgar supplied six reference sites. They split cleanly into **two opposite genres**, and that
  distinction drove everything else:
  - **Marketing** (Nesta Sites): dark, one huge headline, one accent, large empty areas.
  - **Catalogue** (SupplyHouse, Electrical2Go, AutomationDirect, Proelectro): light, dense,
    photo-led grids, everything scannable, almost no empty space.
- This project is **both**. The home page sells the calculator; the catalogue serves someone who
  already knows what a B16 is. Applying one genre to the other is what makes a site feel wrong —
  a dense grid on a landing page looks cluttered, an airy hero on a parts list wastes the screen.
- So the home page follows the marketing genre, `/Products` follows the trade genre, and they
  share the palette, the type scale and the buttons. Written up at the top of the redesign
  section in `site.css`.

*Palette*
- `theme.css` is now **generated from one source** (both palettes come from one table), so the
  two can no longer drift apart. Surfaces moved from blue-black to a **green-tinted charcoal**:
  blue-black is the default of every developer tool and reads as generic, while a warm dark
  green-charcoal is closer to workshop equipment. Accent moved to a coral-orange `#F2764B`.
- Card-vs-page elevation **1.20 → 1.43**. Surface hues now sit in a 156–169° band.
- **New: `--bg-band`,** the one token that *inverts* between themes — cream on dark, near-black
  on light. This is the single biggest reason the references look designed rather than assembled:
  a long page of one background reads as flat however good the components on it are. Contrast
  against the page is **15.9**.

*Components*
- New in `site.css`: hero with display type, floating proof card, stat strip, accent-bordered
  feature cards, the contrast band with numbered steps, catalogue toolbar, product grid, pill
  buttons. All from tokens; no literal colours.
- Home page rebuilt. `/Products` rebuilt as a trade catalogue. Calculator, cart, nav and footer
  restyled, and copy moved onto `docs/VOICE_AND_PERSONALITY.md` wording.

**Bugs found and fixed while doing it**

1. **The landing page advertised a figure the calculator does not produce.** It claimed a worked
   example of *"160 m paigalduskaablit"* costing *"504,10 €"*. Running the calculator with the
   inputs that example describes returns **120 m and 348,90 €**. The numbers were hand-typed into
   the view and had drifted from the code.

   On most sites that is a typo. Here it is the worst bug on the page: the whole claim of the
   project is that its quantities come from EVS-HD 60364 and can be audited line by line.
2. **`.page-title { color: white }` in `site.css`** — the *same* invisible-in-light-mode bug as
   the `text-white` one fixed earlier, one file further down, where the view tests could not see
   it. Every page heading on the site was white-on-white in light mode.
3. **Admin controls rendered for everyone** on `/Products` — "Lisa toode" and "Halda
   kategooriaid" were shown to anonymous visitors, who got bounced to a login page.
4. **The print stylesheet hid a class that no longer existed** (`.hero-section`), so the hero
   would have printed.
5. Two `box-shadow`s and the BOM total row still carried the **old amber** as a literal `rgba`,
   so the button glow was a different orange from the button.
6. Dead CSS: the entire old hero block was unreferenced after the rebuild.

**New tests (199 → 203)**
- `LandingPageFiguresTests` (3) — runs the **real calculator** and asserts the landing page shows
  the total, the cable length and the line count it actually returns. This is the fix for bug 1:
  the figures drifted because nothing checked them.
- `ThemeTokenTests.SiteCss_DoesNotHardCodeColours_OutsideTheDeclaredExceptions` — closes the gap
  bug 2 hid in. Two exceptions stay allowed: the print stylesheet (paper is always white) and a
  photo scrim (must stay dark in both themes), and the second must be justified in a comment so
  a future literal cannot quietly claim the same excuse.

**How it was verified**
- Contrast recomputed for **30 pairings** across both themes: **0 WCAG AA failures.** Parity
  checked: both dark blocks identical, both light blocks identical, both themes define the same
  36 token names.
- App run and exercised over HTTP. All seven pages return 200. A **real calculation was posted**
  with an antiforgery token and returned 8 BOM lines totalling 348.90 € — which is how bug 1 was
  found, and what the corrected figures were taken from.
- `scripts/security-check.sh`: **all 18 checks pass.**
- All six new tests **mutation-tested**. Build clean with `-warnaserror`; **203/203 pass.**

**Two mistakes worth recording, because both produced a green result that meant nothing**
- A mutation run reported *nothing at all* because Visual Studio held a lock on the build output,
  so every build failed and no test executed. Silence read as success.
- More seriously: `TheWorkedExampleCableLength` **passed when mutated**. The test searched the
  raw `.cshtml`, and the explanatory comment above the worked example contains the phrase
  "120 m", so it matched inside the comment while the number a visitor actually sees was wrong.
  Fixed by stripping Razor comments before searching. The same trap had already appeared in the
  CSS test earlier the same day — **a test that reads a file must ignore its comments.**

**Follow-ups or known limitations**
- **Still not visually confirmed.** Every check is numerical or structural. Whether it now looks
  good is Edgar's call.
- Light-mode card/page separation is 1.10, relying on the shadow rather than contrast. Standard
  for a light theme, but it is the first thing to revisit if light mode looks flat.
- `Views/Products/Details.cshtml`, `Categories.cshtml` and the Create/Edit/Delete admin forms
  were **not** restyled — they inherit the palette and buttons but keep their old layout.
- The seeded prices are still ~9.20 € against ~5.78 € real market (`docs/RESEARCH_LOG.md`), and
  `Pricing:PricesIncludeVat` remains unverified. The worked example is now *internally* correct;
  whether the underlying prices are right is a separate open question.

## 2026-08-14 — Rebuilt the colour palette; fixed headings that vanished in light mode

**Type:** bugfix + docs
**Author:** Claude (Opus 5) + Edgar

**What changed**
- `wwwroot/css/theme.css` — replaced every surface, text and accent value in both themes.
  Dark surfaces now sit on **one hue (216–224°)** instead of drifting across 215–240°, and
  saturation **falls as lightness rises** (29% → 16%) the way real materials behave under light.
  Page went `#0f0f1a → #0A0C10`, cards `#16213e → #242833`.
- Same file — card shadow gained a 1px inset light rim; a drop shadow alone cannot show elevation
  on a near-black background because there is nothing darker to cast onto.
- Same file — the tint fills still referenced the *old* accent RGB values, so badge backgrounds
  no longer matched the accent they were meant to echo. Rederived all ten from the new accents.
- `Views/**/*.cshtml` — replaced **38 uses of Bootstrap's `text-white`** across 14 views with
  `text-strong-custom`.
- `wwwroot/css/site.css` — defined `.text-strong-custom`, plus a defensive `.text-white` override.
- `Views/Products/Delete.cshtml` — hard-coded `#ef4444` → `var(--accent-red)`.
- `docs/DESIGN_GUIDE.md` — its `:root` block still listed the old colours and told the reader to
  put them in `site.css`. Now points at `theme.css` as the single source and is marked
  reference-only.
- `CLAUDE.md`, `README.md`, `docs/TEST_ACCOUNTS.md`, `scripts/security-check.sh` — corrected the
  documented port from **5250 to 8080**.
- **New `ElektriKalkulaator.Tests/ThemeTokenTests.cs`** (3 tests) so this cannot happen again:
  no view may use a Bootstrap colour utility, no inline style may contain a literal colour, and
  both palettes must define the same token names.
- `docs/TESTING.md` — corrected "174 tests" to 199 and **deleted the "No CI" item**, which had
  been false since CI landed on 08-11; registered the new test file; rewrote Part 7 around the
  gap this session actually exposed.
- `docs/PROJECT_ROADMAP.md` — its header said "Revision 3" while the revision history below
  already recorded rev. 5. Now rev. 6.

**Why**
- Edgar said the site "looks very poor in colours and attractiveness". Measuring the palette rather
  than guessing found three specific causes:
  1. **Hue drift.** Surfaces ranged 215°–240°. Backgrounds that do not share a hue do not read as
     one material — the eye interprets the mismatch as muddiness, not as depth.
  2. **Saturation outlier.** The card sat at 48% saturation while every other surface was 23–33%.
     That single oversaturated navy panel is what produced the "cheap" look; a surface should be
     nearly neutral and let accents carry the colour.
  3. **No elevation.** Card-to-page contrast was **1.20** — cards barely separated from the page,
     so the layout read as flat regardless of the spacing around it. Now **1.33**.
- The `text-white` bug is more serious than the palette: `text-white` hard-codes `#fff`, so every
  heading using it became **white text on a white card** the moment anyone switched to the light
  theme. The light mode shipped in the 2026-08-11 entry was therefore partly unusable, and the
  numbers-only verification done then could not have caught it — the tokens were all correct; the
  markup was bypassing them. This is the identical bug to the `navbar-dark` one fixed earlier,
  which should have prompted a search for the rest of the family at the time. It did not.
- The port was wrong in four places including the security script's default, so the command
  `bash scripts/security-check.sh` as documented in `CLAUDE.md` would have connected to nothing
  and reported failures unrelated to security.

**How it was verified**
- Contrast computed for **22 foreground/background pairs** across both themes: **0 WCAG AA
  failures**. Lowest is `--accent-red` on a dark card at 4.64; text-strong reaches 13.63 (dark)
  and 19.32 (light).
- Verified the two dark blocks are byte-identical to each other and the two light blocks likewise
  (28 variables each), and that both themes define the **same variable names** — a variable present
  in one theme only would silently inherit the other theme's value.
- App run and exercised over HTTP on :8080. Confirmed the server sends the new values, that
  `/`, `/Calculator`, `/Products`, `/Cart`, `/Account/Login` all return 200, and that
  **zero `text-white` occurrences remain** in any rendered page.
- `scripts/security-check.sh`: **all 18 checks pass** against the running app on the corrected port.
- The three new tests were **mutation-tested**, per Part 2 of `TESTING.md`: adding `text-white`
  back to a view, adding an inline `#ff0000`, and deleting one token from the light palette each
  made the matching test **fail**, and all three passed again after reverting.
- Worth recording: the **first** mutation run reported nothing at all, because the app was still
  running from the HTTP testing above and held a lock on `ElektriKalkulaator.exe`, so every build
  failed and no test executed. Silence read as success. This is the same trap as the earlier
  `-warnaserror` "proof" that proved nothing — **a test run that produces no failure output has
  not necessarily run.** Stopped the app, confirmed a clean baseline build, then re-ran.
- Build clean with `-warnaserror`; **199/199 tests pass**.

**Follow-ups or known limitations**
- **Not visually confirmed.** Every check above is numerical or structural — contrast ratios, hue
  angles, served bytes. I could not render the page this session, so whether it now *looks* good
  is Edgar's call, not a verified claim.
- Light-mode card/page separation is **1.11**, much lower than dark's 1.33. That is intentional and
  conventional — white cards on a light-grey page rely on the shadow for elevation, not contrast —
  but it is the pairing to revisit first if light mode still looks flat.
- The 38 replacements were mechanical. Each rendered page was checked for *absence* of the old
  class, but the individual headings were not inspected one by one.
- `ThemeTokenTests` checks colours written in the **markup**. Nothing tests whether the palette
  itself looks good — that still needs a person. See `docs/TESTING.md` Part 7 item 2.

---

## 2026-08-11 — All colours in one file, and a working light/dark switch

**Type:** feature
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- New **`wwwroot/css/theme.css`** holds every colour on the site. `site.css` keeps component styles
  and now contains **zero hard-coded colours** outside the print block, where fixed black-on-white
  is the point.
- New **`wwwroot/js/theme.js`** — a light/dark toggle in the navigation, remembered in
  `localStorage`.
- `_Layout.cshtml` loads `theme.css` **before** `site.css`, and carries a small inline script in
  `<head>`.

**Three states, not two**
`system` (no attribute — the OS decides, and is the default), `light`, and `dark`. The user's choice
must beat the system preference **in both directions**, so the media query is written
`:root:not([data-theme="dark"])` rather than plain `:root`. Without that guard a light OS setting
would keep overriding someone who explicitly asked for dark.

**Why the duplicated inline script**
The `<head>` script repeats logic that also lives in `theme.js`. That is deliberate: it has to run
**before the stylesheets load**, or every page paints in the default colours and then switches — a
visible flash on each navigation. Both copies are wrapped in `try/catch`, since `localStorage`
throws in private browsing and under some corporate policies.

**Light is not an inversion**
Several colours are genuinely different values. The clearest case is amber: `#F5A623` is comfortable
on a dark card but fails contrast as small text on white. So there are now two tokens —
`--accent-amber` for button **fills** (a dark label supplies the contrast) and `--accent-amber-ink`
for **text and borders**, which becomes `#9a5b00` in light mode. Backgrounds are a soft grey-blue
rather than pure white, which glares in daylight — relevant for a tool used on site.

**A bug found while doing this:** the navigation carried Bootstrap's `navbar-dark` class, which
hard-codes light text. In light mode that would have produced white text on a white bar. Removed,
and the styling it provided (including the hamburger icon, whose colour Bootstrap bakes into an
inline SVG) reimplemented with tokens.

**How it was verified**
- Build clean with `-warnaserror`; **196/196 tests passing**.
- `theme.css` and `theme.js` serve with HTTP 200, and the stylesheet order is correct.
- **Switching tested in a real browser**, checking computed styles rather than just the attribute:
  | | `data-theme` | `--bg-primary` | `--accent-amber-ink` | saved |
  |---|---|---|---|---|
  | initial (OS = light) | *none* | `#f4f6f8` | `#9a5b00` | — |
  | after 1st click | `dark` | `#0f0f1a` | `#F5A623` | `dark` |
  | after 2nd click | `light` | `#f4f6f8` | `#9a5b00` | `light` |
- **Contrast measured on the light theme**, since that is where it could realistically fail. Every
  pair passes WCAG AA: body text 13.55, muted text 5.83, amber-ink 5.43, green 5.48, red 6.57,
  blue 6.70, button label on amber 7.24.

**Behaviour change worth knowing**
Visitors whose operating system prefers light will now see the **light** theme by default, where
previously everyone saw dark. That is the correct, accessible behaviour, and the toggle overrides
it — but it does mean the site no longer looks the same to everyone on first visit.

**Follow-ups or known limitations**
- The choice is stored per browser, not per account, so it does not follow a signed-in user between
  devices. Deliberate: someone may want dark on a phone and light on a bright workshop laptop.
- The light theme has not been reviewed by eye at every page, only measured for contrast.

---

## 2026-08-11 — Voice guide, and a written spec for the supplier-sync model

**Type:** docs
**Author:** Claude (Sonnet 5) + Edgar

Two documents, no code.

**`docs/VOICE_AND_PERSONALITY.md`** — how to make the site memorable and warm.

It opens by resolving an apparent contradiction with `DESIGN_GUIDE.md`, which concluded that
plainness wins. Both hold, because they govern different layers: **structure, speed and information
stay ruthlessly plain; language, tone and small moments are where personality lives** — and those
cost nothing to load.

The central argument is that this project's distinctiveness already exists and only needs saying
out loud. It is a shop that shows its working, cites a national safety standard, and can be audited
line by line. Nobody expects that, so no invented quirk is needed — and manufactured quirk on a
safety-relevant tool would be actively worse than plainness.

Contains: a one-line voice rule (write as an electrician explaining something to a colleague), a
before/after table of real Estonian strings, warmth at the points where sites are usually coldest
(errors, empty states, waiting), five "weird bits worth doing" — the strongest being a reasoning
line under each BOM row, which turns a price list into a teaching tool and is only possible because
the calculation is rule-based — and an explicit do-not list. International references (Oatly,
McMaster-Carr, Basecamp, Patagonia, Duolingo, Stripe) each with what to take and what to leave.

One deliberate exception is called out: the login error stays vague, because a friendlier message
would reveal which email addresses have accounts. **Warmth never overrides safety.**

**`docs/SUPPLIER_SYNC_SPEC.md`** — the dropshipping model, written down and clearly marked
**NOT BUILT**.

The honest framing is that the scheduled price-sync job is the *easy* part, and the things that
decide whether the model works are commercial and legal:
- Estonian retailers have **no ordering API**, and automating their checkout would breach terms of
  service and be indistinguishable from an attack. Recommended order is manual relay → reseller
  agreement → automation.
- Taking payment makes this site the **seller of record** in the EU: 14-day withdrawal, 2-year
  conformity guarantee, and a supplier's stock error becomes our late delivery.
- **25 % must be stored as data, not `* 1.25` in code** — it has to vary by supplier and category,
  and must absorb payment fees, returns and price drift.

Also specifies the schema (`Supplier`, `SupplierProduct`, `PriceHistory`), why price history is
append-only (a quote given Monday must still be explainable Friday — the same auditability argument
the whole project rests on), sync safety rules, and a seven-phase order in which **most of the value
arrives at phase 4**, before any automation.

**Follow-ups**
- Four open questions for Edgar at the end of the spec, including whether 25 % is researched or a
  starting guess.
- Nothing in the voice guide is implemented; the cheapest items are copy edits of about 30 minutes.

---

## 2026-08-11 — Session close: roadmap brought up to date

**Type:** docs
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- `docs/PROJECT_ROADMAP.md` updated to **rev. 5**, matching reality after a long session:
  - **§B4 (what works)** rewritten. It still claimed 16 tests, no authentication, and listed
    already-fixed rough edges. Now records authentication, 196 tests, CI, sorting, the print view
    and the design-token system — and lists the *current* rough edges honestly: dead
    `RoomsFrom`/`RoomsTo` fields, `RulesApplied` storing names instead of rule IDs, `UserId` still
    unpopulated, N+1 queries, and the hard-coded homepage figures.
  - **§D2 (Phase 1)** restructured into **done / needs Edgar / optional**. Fifteen items are now
    complete; what remains is mostly **decisions rather than code**.
  - **§D1 maturity map** re-scored: CI/CD and Security move from 🟡 to 🟢; Error Tracking notes
    that startup now fails fast.

**Why**
- A status document that overstates progress is worse than none: the next session trusts it, skips
  work it thinks is done, and redoes work it thinks is not. Accuracy matters more here than
  optimism.
- Written so that context loss between AI sessions costs nothing. Everything verified this session
  lives in files — tests that re-run, a CI pipeline that enforces them, and documents that explain
  the reasoning — rather than in a conversation that will be summarised away.

**How it was verified**
- Clean `--no-incremental` Release build with `-warnaserror`: **0 warnings, 0 errors**.
- **196/196 tests passing.**
- Working tree clean, branch pushed, four PRs open and stacked.

**State at session close**
- Branch `feat/conversion-ux`, 15 commits ahead of `main`, everything pushed.
- **PRs #1 → #2 → #3 → #4 are stacked and must be merged in that order.**
- Outstanding items needing Edgar rather than code: confirm the VAT assumption, fill in real
  EVS-HD 60364 clause numbers, decide whether to re-base the seeded prices, merge the PRs.

---

## 2026-08-11 — Demo admin and customer accounts, created only in Development

**Type:** feature / docs
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- `IdentitySeeder` now also creates two demo accounts — an administrator and a customer — **only
  when the application runs in the Development environment**. Credentials are printed to the console
  at startup.
- New **`docs/TEST_ACCOUNTS.md`** listing every account, what each is for, a five-minute manual pass
  that exercises the whole permission model, and a pre-deployment checklist.
- New `DemoAccountSeedingTests` (12 tests). **196 tests total.**
- The user-creation logic was extracted into one `CreateUserAsync` helper, so the configured admin
  and the demo accounts follow identical rules — including deleting the account if the role
  assignment fails, rather than leaving someone signed in with no role.

**Why**
- There was a seeded administrator but **no customer account**, so the most important thing to test —
  what a non-administrator actually sees — required registering by hand every time the database was
  reset. It is also the case most easily got wrong, precisely because development is always done
  while logged in as an admin.

**The safety question, and how it is answered**
- These passwords are in the source code and in the documentation, so they are public to anyone who
  reads the repository. Creating them on a real server would hand out an administrator account.
- The **only** thing preventing that is `app.Environment.IsDevelopment()`. Because that is the whole
  safety mechanism, the first test written was the negative one —
  `DemoAccounts_AreNotCreated_OutsideDevelopment`. A guard nobody has tested is a guard nobody
  should trust.
- `docs/TEST_ACCOUNTS.md` opens with that warning rather than burying it, and ends with a
  pre-deployment checklist whose first item is confirming the server is not running in Development.

**How it was verified**
- Build clean with `-warnaserror`; **196/196 tests passing**.
- Both accounts exercised against the running application over real HTTP:
  | Check | Admin | Customer |
  |---|---|---|
  | Login | 302 ✓ | 302 ✓ |
  | `/Products/Create` | 200 | **302 → /Account/AccessDenied** |
  | `/Calculator`, `/Products` | 200 | 200 |
  | Admin link in nav | shown | hidden |
  - Wrong password still refused.
- Startup log confirms both accounts seeded and the credentials printed.
- Tests also cover: repeated seeding not duplicating anyone, a password changed during testing not
  being silently reset, passwords stored hashed, and the customer **not** accidentally holding the
  Admin role — which would make the account useless for its only purpose.

**Two mistakes made while doing this, both mine**
- The first version of `AChangedPassword_IsNotResetByRestarting` used a password-reset token, which
  needs token providers the deliberately minimal test setup does not register. Switched to
  `ChangePasswordAsync`, which tests the same thing without that dependency.
- The first manual verification reported HTTP 400 for both logins and looked like a real failure. It
  was the trap already documented in `scripts/security-check.sh`: with `MSYS_NO_PATHCONV=1`, Windows
  `curl` cannot write a cookie jar at an absolute `/tmp` path, so the antiforgery cookie was lost.
  Relative paths fixed it. Worth remembering — I had written that warning myself and still hit it.

**Follow-ups or known limitations**
- If the site is ever started in Development against a production database, the demo accounts would
  be created there and must be deleted manually. Noted in the checklist.

---

## 2026-08-11 — Implement the design guide: tokens, sorting, print, spec table

**Type:** feature
**Author:** Claude (Sonnet 5) + Edgar

All nine steps of `docs/DESIGN_GUIDE.md` §6, implemented and verified. **184 tests passing.**

**What changed**

*Foundation (steps 1–3)*
- **Spacing and type scales** added as CSS tokens. The project had colour tokens but neither of
  these, which is why 19 inline `style="…"` attributes and 13 distinct font sizes had accumulated —
  each an isolated decision.
- **Keyboard focus states.** There were none, so the site could not be operated by keyboard at all.
  Uses `:focus-visible`, which shows the ring for keyboard users without outlining every mouse
  click. Also added `prefers-reduced-motion` support.
- **Colour roles fixed.** Amber previously marked prices, primary buttons, the logo and badges at
  once — when everything is the accent, nothing is. Prices are no longer amber; they carry emphasis
  through size, weight and tabular figures instead, leaving amber to mean "the next action".

*Catalogue (steps 4, 7)*
- **Sorting**, which did not exist. New `ProductSortOrder` enum (an enum rather than loose strings,
  so a typo is a compile error instead of silently falling back to the default order) with five
  options. Applied to the `IQueryable` before `ToListAsync`, so it becomes SQL `ORDER BY`. Every
  branch has a name-based tiebreaker so equally-priced products cannot swap places between page
  loads.
- **Result count above the list** as well as below — people check the count before deciding whether
  to scan or narrow first.
- **Price per unit on cards.** Cable showed a bare `1.20 €`, reading as the price of a whole reel.
  Now `1.20 €/m`, matching the fix already made in the calculator's BOM.
- **Empty state now says what to do next** — a "show all products" button rather than a dead end.

*Calculator (step 5)*
- **Print stylesheet.** An electrician's real workflow is calculate → show the client → order.
  Printing previously produced a dark page with navigation and buttons. It now prints as a
  black-on-white quote: chrome hidden, table borders drawn, header row repeated across pages,
  rows kept from splitting.

*Product page (step 6)*
- **Specification table** replacing the grid of stat cards. Technical buyers compare specs between
  products, and a two-column table reads far faster down the page than boxes across a grid. Rows
  render only when the product has that value.

*Homepage (step 8)*
- **Worked example** showing a real calculator result (3-room apartment → 504,10 €) rather than
  describing what the calculator does. Showing the output is more persuasive than describing the
  input.

*Cleanup (step 9)*
- Homepage inline styles replaced with token classes; the CSS file header and its remaining
  Estonian comments translated to English.

**How it was verified**
- Build clean with `-warnaserror`; **184/184 tests** (8 new in `CatalogueSortingTests`).
- Checked over real HTTP: sorting by price ascending returns the 1.20 € cable first and descending
  the 42.00 € RCD first; stock sorting returns the 5000-unit cable first; the result count renders
  above the list; cable cards show `1.20 €/m`; the spec table renders on the details page; the
  worked example renders on the homepage.
- The sorting tests check more than the endpoints: that the whole sequence is genuinely ordered,
  that sorting **combines with** filtering rather than replacing it (a common mistake that quietly
  returns the entire catalogue in the right order), and that repeated identical requests return an
  identical order.

**Follow-ups or known limitations**
- The worked example's figures are hard-coded. If seeded prices change, that calculation must be
  re-run and the numbers updated — noted in a comment in the view.
- Multi-select filters (guide §3.1 D) deliberately **not** done: at 10 products a dropdown is
  genuinely adequate and checkboxes would be over-engineering. Revisit past ~20 products.
- Manufacturer part codes and datasheet links (guide §3.2) need new `Product` fields and are a
  schema change, so they are left for a separate piece of work.

---

## 2026-08-11 — Design guide: what "good design" means for this project, and how to implement it

**Type:** docs
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- New **`docs/DESIGN_GUIDE.md`** — a design system and page-by-page instruction set, written to be
  implemented from directly rather than admired.
- `CLAUDE.md` and `README.md` document tables updated to list it and `TESTING.md`.
- Two stale claims in `CLAUDE.md` corrected: `Tests` now references all four projects (not three),
  and the VAT question is no longer "open" — it is configuration, though the *value* remains an
  unconfirmed assumption.

**Why, and what the research changed**
- The obvious approach would have been to copy whatever looks impressive on award-winning sites.
  Researching it produced the opposite conclusion, and that conclusion is the guide's central point:
  **the users are not shopping, they are looking something up.** An electrician pricing a job wants
  the answer, not an experience.
- The strongest evidence is [McMaster-Carr](https://www.mcmaster.com), widely regarded as one of the
  best-designed commerce sites in existence — by engineers rather than by design awards. It is
  deliberately plain, server-rendered, and almost JavaScript-free, because its users need a part
  *now*. **That is the same architecture this project already has**, which reframes the current
  design as a strength to sharpen rather than something to replace.

**What the guide contains**
- A spacing scale and type scale as CSS tokens. The project had colour tokens but neither of these,
  which is why there are 19 inline `style="…"` attributes and 13 distinct font sizes — each an
  isolated decision.
- **A colour rule the project currently breaks:** amber is used for prices, buttons, the logo and
  badges simultaneously. When everything is the accent, nothing is. Prices should carry emphasis
  through size and weight, not hue; amber should mean "the next action" and appear roughly once per
  screen.
- Page-by-page instructions with the problem, the fix, and how to verify it — catalogue sorting
  (still missing), a specification table on product pages, a **print stylesheet** so a BOM becomes a
  quote an electrician can hand to a client, and a worked example on the homepage.
- Universal rules: focus states (currently absent, so the site cannot be used by keyboard), minimum
  readable font size, tables scrolling inside themselves, empty states that say what to do next.
- A "what NOT to do" section covering fake scarcity, countdown timers and invented "was" prices —
  regulated as unfair commercial practices in the EU, and directly contradictory to a project whose
  whole argument is that its output is auditable.
- A nine-step implementation order, smallest and highest-value first, plus four ways to check a
  design change actually worked (squint test, 5-second test, keyboard-only, 375px width).

**How it was verified**
- No code changed, so nothing to test. Existing token values were read from `site.css` and the
  inline-style and font-size counts measured, so the guide describes the project as it actually is
  rather than in the abstract.
- Every external claim is cited and recorded with its source.

**Follow-ups or known limitations**
- Nothing in the guide is implemented yet — it is the plan, not the work. Step 1 is adding the
  tokens.
- The colour-role change (prices no longer amber) will visibly alter several pages and is worth
  reviewing on screen before committing.

---

## 2026-08-11 — Zero build warnings, and CI now fails on any new one

**Type:** bugfix / chore
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- `PowerboxCalculation.Components` changed from `ICollection<PowerboxComponents>?` to a
  non-nullable `ICollection<PowerboxComponents>` initialised to an empty list.
- `Views/_ViewImports.cshtml` — removed a duplicate `@using ElektriKalkulaator` (added by me when
  wiring up the VAT notice; the namespace was already imported at the top of the file). Its
  Estonian comments translated to English while there.
- `.github/workflows/ci.yml` — the build step now passes `-warnaserror`.

**Why**
- Two warnings had been present on every build:
  - **CS8620** in `CalculatorServices.GetHistory`. EF Core's `ThenInclude` expects a non-nullable
    collection, and `Components` was declared nullable. Beyond the warning, nullable was the wrong
    description: a calculation always *has* components — possibly none, which an empty list
    expresses perfectly well. Nullable said "this list might not exist", a different and less
    useful idea that forced null checks on something never really null. It now matches how
    `ProductCategory.Products` was already declared.
  - **CS0105**, a duplicate using directive. Harmless, but noise.
- With the count at zero, `-warnaserror` keeps it there. A warning is usually the compiler noticing
  something genuinely wrong; once a few are tolerated they stop being read at all. Both of these
  were small, and one of them was a real nullability mismatch.

**How it was verified**
- Clean Release build: **0 Warning(s), 0 Error(s)** — previously 2 warnings.
- **176/176 tests passing.**
- `dotnet ef migrations has-pending-model-changes` reports **no model changes**, confirming the
  navigation-property change does not affect the database schema and needs no migration.
- **`-warnaserror` was proved to work**, which took two attempts:
  - The first attempt injected a duplicate using and the build still succeeded — because
    `--no-restore` with an unchanged project meant the compiler never ran again, so no warning was
    re-emitted. The test proved nothing.
  - Re-run with `--no-incremental` to force a real recompile: the warning became
    `error CS0105` and the build **FAILED**, then passed again once reverted. On CI this is
    academic — every run starts from a clean machine and always compiles — but a flag nobody has
    seen fail is a flag nobody should trust.

**Follow-ups or known limitations**
- If a warning ever has to be allowed, suppress that specific rule with a comment explaining why,
  rather than removing the flag.
- EF Core still emits two runtime *model validation* warnings about decimal precision on
  `Product.Voltage` and `PowerboxRequirements.TotalAreaM2`. Those come from EF at startup, not the
  compiler, so `-warnaserror` does not cover them. Worth fixing separately with `HasPrecision`.

---

## 2026-08-11 — Continuous integration: tests now run automatically on every push

**Type:** chore
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- `.github/workflows/ci.yml` — GitHub Actions builds the project and runs all 176 tests on every
  push to any branch, on every pull request into `main`, and on demand from the Actions tab.
- The repository already had an empty, untracked `.github/workflows/` folder. It is now used.

**Why**
- This was the largest remaining gap after the test work. **176 tests only protect the project if
  something actually runs them**, and depending on a person to remember means that eventually they
  will not be run. CI removes that dependency.
- It also makes a pull request self-verifying: a reviewer can see the tests passed rather than
  taking the author's word for it.

**Design decisions worth knowing**
- **No database service in the workflow.** Every test — including the integration tests that boot
  the real application — uses an in-memory database, so the runner needs only the .NET SDK. No SQL
  Server, no connection string, no secrets.
- **Targets the test `.csproj`, not the `.slnx` solution.** Building the test project pulls in all
  four other projects through its references anyway, and this avoids depending on the newer `.slnx`
  format being supported by whatever SDK the runner has.
- **Restore, build and test are three separate steps**, so a failure names itself in the GitHub UI
  instead of hiding in one long log.
- **Test results are uploaded even when tests fail** (`if: always()`), since that is exactly when
  they are worth reading.
- `-warnaserror` is deliberately *not* enabled yet: the project still has one known EF Core
  nullability warning in `CalculatorServices.GetHistory`, and turning it on now would fail every
  build until that is addressed.

**How it was verified**
- The exact three commands the workflow runs were executed locally **in Release configuration**
  (the tests had only ever been run in Debug): restore, `build --no-restore`, `test --no-build`.
  Build clean, **176/176 passing**, and the `.trx` file was produced at the path the upload step
  expects.
- Confirmed `TestResults/` is already covered by `.gitignore`, so CI output cannot be committed by
  accident.

**Follow-ups or known limitations**
- The workflow does not run `scripts/security-check.sh`, which needs a started application. Its
  checks are already covered by the integration tests; the script remains useful only for pointing
  at a deployed server.
- No deployment step — the project is not hosted anywhere yet.

---

## 2026-08-11 — Real HTTP integration tests, and two more bugs found with them

**Type:** test / bugfix
**Author:** Claude (Sonnet 5) + Edgar

**134 → 176 tests.** The security checks that previously needed a manually started application and
a shell script now run as part of `dotnet test`.

**What changed**

*Integration test infrastructure*
- `Integration/TestWebAppFactory.cs` — boots the **real** `Program.cs` in memory using
  `WebApplicationFactory`: same DI, same middleware order, same attributes. Only the database is
  swapped for an in-memory one.
- `Integration/HttpTestHelpers.cs` — fetching antiforgery tokens, logging in, reading redirect paths.
- `Integration/AuthorizationTests.cs` (21) — anonymous, customer and admin against every admin-only
  and public page, plus wrong password, unknown email and logout.
- `Integration/RequestSecurityTests.cs` (19) — antiforgery on all five POST endpoints, four
  varieties of hostile `returnUrl`, quantity limits, unknown product IDs.
- `Program.cs` — startup now uses `EnsureCreated` when the provider is not relational, so the same
  startup code works under test instead of the test host needing its own copy. Also gained an empty
  `public partial class Program` so the factory can reference it.

*Two bugs the new tests exposed*
1. **The shopping cart survived logout.** The cart lives in the session, not against the account,
   so signing out left it behind. On a shared computer the next person would inherit it.
   `Logout` now calls `HttpContext.Session.Clear()`. **A test was written first and failed**,
   confirming the bug before the fix.
2. **`AddToRoleAsync` was not checked during registration.** If it failed the account existed with
   no role at all — signed in, but treated as though never registered. The result is now checked
   and the half-created account removed.

*Housekeeping*
- The last Estonian comments (in all four `.csproj` files) translated to English, per the house rule.

*Documentation*
- `docs/TESTING.md` rewritten as a full methodology guide in seven parts: what a test is for a
  beginner, the "a test must be able to fail" rule with this project's real examples, the five
  kinds of test and when each applies, what must be tested and what must not, conventions, an
  add-a-test checklist, and an honest list of what is still missing.

**How it was verified**
- Build clean, **176/176 tests passing**.
- The integration tests were **mutation-tested three ways**, and all three were caught:
  | Mutation | Tests that failed |
  |---|---|
  | removed `[Authorize]` from `ProductsController` | 10 |
  | removed the `Url.IsLocalUrl` guard | 4 |
  | removed one `[ValidateAntiForgeryToken]` | 1 |
- Getting the factory working required fixing a real trap: `AddDbContext` in .NET 9 also registers
  an `IDbContextOptionsConfiguration<T>`, and removing only `DbContextOptions` left the SQL Server
  provider attached — EF then refused to start with two providers registered.

**Follow-ups or known limitations**
- **No CI is now the biggest gap.** 176 tests that only run when someone remembers will eventually
  be ignored.
- `scripts/security-check.sh` is now redundant with the integration tests, but is kept because it
  can be pointed at a deployed server, which `dotnet test` cannot.
- Cart prices are read at display time, so a price change between adding and viewing is reflected
  immediately. Arguably correct; worth a deliberate decision once orders are real.

---

## 2026-08-11 — Fix two display bugs, and make the calculator's value visible

**Type:** bugfix / feature
**Author:** Claude (Sonnet 5) + Edgar

**Two bugs found while doing UI work**

*1. Cable was labelled in pieces instead of metres*
- Every BOM row printed "tk" (pieces). Cable is calculated and priced **per metre**, so a 40-metre
  run displayed as "40 tk" — reading as forty separate cables — and its price showed as "1.20 €"
  as though that were the whole run rather than one metre.
- Anyone using the estimate to order materials would have been badly misled, which undermines the
  one thing this tool exists to do.
- `BOMItemDto` gained a `Unit` property; `CalculatorServices` sets `"m"` for cable and `"tk"` for
  everything else, from named constants. The table now shows `80 m` and `1.20 €/m`.

*2. The entire page body was rendered inside an HTML comment*
- `_Layout.cshtml` had `<!-- Põhisisu ala — @RenderBody() = ... -->`. **Razor does not treat HTML
  comments as comments**, so it executed the `@RenderBody()` written inside one.
- Consequences on every page: 544 lines of content emitted inside an unterminated comment, the
  `<main>` element rendered **empty**, and the comment's trailing words appeared as visible stray
  text (`= siin renderdatakse iga lehe sisu -->`). Content only displayed at all because a nested
  `-->` inside the page happened to close the comment early.
- Fixed by making it a Razor comment, which is stripped before anything inside it runs. Content is
  now inside `<main>` (548 lines) where it belongs.

**Conversion work — the site's strongest argument was invisible**
- **Trust strip** under the calculation result: quantities follow EVS-HD 60364, every line comes
  from a rule rather than an estimate, prices are the cheapest in-stock option. All three are
  literally true of this application — that is what separates them from marketing badges. Green
  accent, not the amber used for prices, so it reads as reassurance rather than another button.
- **VAT is now stated** next to the total. A cost estimate that does not say whether tax is
  included is ambiguous by more than 20%, and every Estonian retailer states it. The value comes
  from a new `Pricing` section in `appsettings.json` (`PricesIncludeVat`, `VatRatePercent`), since
  it is a business fact that may change without code changing. **Assumption: prices include VAT** —
  Edgar should confirm, as it is only a display statement and must match how the prices were
  actually recorded.
- **Homepage hero** now uses the real photograph downloaded earlier (and until now unused) instead
  of a faded emoji. Hidden below `lg` so phones do not download a large decorative image.

**How it was verified**
- Build clean, **134/134 tests** (6 new in `BomUnitOfMeasureTests`), `security-check.sh` **18/18**.
- Checked in a real browser and via page-text extraction: cable rows read `80 m` at `1.20 €/m`
  totalling `96.00 €`; breakers read `2 tk` at `7.90 €/tk`; trust strip and VAT line both render.
- The stray-text bug was confirmed fixed on `/`, `/Products`, `/Calculator` and `/Cart`, and
  `<main>` verified to contain the page content.
- Note: the first attempt at the layout fix **broke the build** — the explanatory comment itself
  contained the literal sequence that ends a Razor comment, closing it early. Rewritten to
  describe the calls by name instead.

**Follow-ups or known limitations**
- The VAT setting only changes what the site *says*; it does not convert any price.
- Catalogue and cart pages do not yet show the VAT notice or per-unit pricing — only the
  calculator does.

---

## 2026-08-11 — Broaden test coverage to 128 tests, and document the testing approach

**Type:** test / docs
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- **`docs/TESTING.md`** — how this project is tested, what each test file covers, the conventions,
  and an honest list of what is still missing. Written so the next person (or model) knows where a
  new test belongs and what "good" looks like here.
- **55 → 128 tests**, adding three kinds of coverage that did not exist:
  - **`SeedDataIntegrityTests`** (13) — asserts things about the *data* rather than the code. Every
    seeded product has an image file **that actually exists on disk**, belongs to a real category,
    has a positive price and non-negative stock; every category the calculator matches by name
    exists and has in-stock products; every building type has rules; every rule has a matching
    breaker and cable in the catalogue; all seeded IDs are unique.
  - **`CalculatorEdgeCaseTests`** (~30) — boundaries rather than happy paths: 0/1/7/8/9/16/17/500
    lights, 0/1/5/6/7/12/500 sockets, all three building types, a commercial building with the
    stove box ticked (no stove rule exists, so nothing must be added), totals matching the sum of
    lines, every line referencing a real product, and the 50-entry history limit.
  - **`FormValidationTests`** (~30) — the `[Required]`, `[Range]` and `[Compare]` attributes on
    every form DTO, including both sides of each boundary and the password-confirmation mismatch.

**Why**
- The suite covered the code but never the **data**. A renamed category silently breaks the
  calculator (it matches by exact string), and an `ImagePath` pointing at an uncommitted file shows
  broken images to everyone who clones the repo — **a bug this project actually shipped**, which no
  existing test could have caught.
- Boundary cases are where "one circuit per 8 lights" goes wrong. Previously only 8 and 9 were
  covered; now both sides of every multiple are.
- Validation attributes are one deleted line away from disappearing with nothing failing to
  compile.

**How it was verified**
- **128/128 passing**, `scripts/security-check.sh` **18/18 passing**, build clean.
- Both new test categories were **mutation-tested**, per the rule now written into
  `docs/TESTING.md`:
  - a seeded image file was temporarily moved away → `SeedDataIntegrityTests` failed with
    *"These seeded products reference image files that do not exist"*;
  - the socket divisor was changed from 6 to 5 → 3 boundary tests failed.
  - Both reverted; full suite green again.

**Follow-ups or known limitations**
Listed in `docs/TESTING.md`, honestly: no HTTP-level integration tests
(`WebApplicationFactory`) — the biggest gap, and it would fold the 18 security checks into
`dotnet test`; no controller tests; `CartController` has no automated coverage at all (session
dependency makes it awkward); no UI tests; no CI running any of this on push.

---

## 2026-08-11 — Code review: fixed an image-deletion ordering bug and fail-fast on startup

**Type:** bugfix
**Author:** Claude (Sonnet 5) + Edgar

A review of the whole codebase for bugs, weak logic and architectural problems. Two clear bugs
fixed here; the rest are recorded as findings in `PROJECT_ROADMAP.md` rather than changed, because
they need decisions rather than code.

**What changed**

*1. `ProductsController.Edit` deleted the old image before the update was confirmed*
- The old code deleted the previous image file, then called `Update`. If `Update` returned `null`
  (the product had been deleted by someone else in the meantime), the image was already gone and
  the newly uploaded one was orphaned. The comment above it even claimed the deletion happened
  "once the new one is confirmed saved" — it did not.
- It also decided **which file to delete from `dto.ImagePath`**, a hidden form field the browser
  controls. A stale tab or an edited form could therefore delete a different product's image.
- Now: the current path is read from the database, the update runs first, and only then is the old
  file removed. Admin-only, so severity was low, but the ordering was simply wrong.

*2. Startup continued after a failed migration*
- `Program.cs` logged a migration failure and carried on. The app then served pages against a
  missing or outdated schema, producing confusing errors all over the site instead of one clear
  one. It now rethrows, so the app stops where the real cause was logged.

*3. New tests*
- `ProductImageLifecycleTests` — 7 tests covering how `ImagePath` behaves across create, update
  and delete, including updating a product that no longer exists (the failure path the old code
  handled badly). **48 → 55 tests.**

**How it was verified**
- Build clean, **55/55 tests passing**, `scripts/security-check.sh` **18/18 passing**.
- The Edit flow was re-tested end to end against the running app as an admin:
  - edit with no new file → image preserved;
  - edit with a new file → new image saved, old file removed from disk;
  - edit submitting a **forged** `ImagePath` pointing at a shared seeded photo → forged value
    ignored, `breaker.jpg` still present.
- Catalogue returned to 10 products, uploads folder empty, all 4 seeded photos intact.
- Note: three apparent failures during testing turned out to be a broken product-ID extraction in
  the test commands (one grep captured the image filename GUID instead of the product ID). The
  application behaved correctly throughout; the IDs were eventually read from the database instead
  of scraped from HTML.

**Findings NOT changed — recorded for decision**
- `CalculationRule.RoomsFrom` / `RoomsTo` are **dead fields**: declared, seeded with 1/999, and
  read by nothing. The comment "999 = no upper limit" implies range logic that does not exist.
- `RulesApplied` stores circuit-type names (`"lighting, socket"`), but the ERD specification says
  it should hold the rule **IDs** as JSON "for auditing". As stored it cannot tell you which rule
  produced a given BOM — a weakness given the project's auditability claim.
- `PowerboxCalculation.UserId` is still never populated, although Identity now exists to fill it.
- `CalculatorServices.Calculate` issues two queries per rule (up to 8 per calculation), and
  `CartController.Index` one per cart line. Fine at this size, wrong at scale.

---

## 2026-08-11 — Move documentation into docs/

**Type:** chore
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- `PROJECT_ROADMAP.md`, `CHANGELOG.md`, `RESEARCH_LOG.md`, `IMAGE_CREDITS.md` and `PROMPTS.md`
  moved into **`docs/`** with `git mv`, so their history is preserved.
- Every reference updated: `CLAUDE.md`, and the code comments in `ProductsController`,
  `CartController`, `CategoryServices` and `ElektriKalkulaatorContext`.
- `README.md` rewritten — it was a single line containing only the repo name. It now explains what
  the project is, how to run and test it, the project structure, and links to every document.

**Why**
- The repository root had seven markdown files competing for attention. Anyone opening the repo saw
  a wall of documents instead of the project.
- **`CLAUDE.md` deliberately stayed at the root.** It is only loaded automatically from there, so
  moving it into `docs/` would have quietly disabled the mechanism that makes every future AI
  session self-orienting — the opposite of what it is for.
- `README.md` also stays at the root because that is what GitHub renders on the repository page.

**How it was verified**
- Searched the whole repository for references to the five moved filenames: none remain without the
  `docs/` prefix.
- Confirmed every path linked from `README.md` and `CLAUDE.md` exists on disk.
- `dotnet build` clean, **48/48 tests passing** — the moved files are referenced from code comments,
  so a bad rename would not break the build; the searches above are what actually proves it.

**Follow-ups or known limitations**
- None.

---

## 2026-08-11 — Make the project survive AI context loss: CLAUDE.md, tests, security script

**Type:** docs / test
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- **`CLAUDE.md`** — loaded automatically at the start of every AI session. Holds the rules
  (changelog every change, English comments, `.jpg` only, test against the running app, never
  invent EVS clause numbers), the commands, and a "things that look like bugs but are deliberate"
  list.
- **`PROMPTS.md`** — ready-made prompts for future sessions with the reasoning behind each, plus a
  table of vague prompts and what to say instead.
- **32 new tests** (16 → **48 total**):
  - `ImageUploadValidationTests` — 16 tests. Accepts real JPEG/PNG/GIF/WEBP, refuses disallowed
    extensions, oversized files, and — the important one — an executable or plain text renamed to
    `.jpg`/`.png`.
  - `CatalogueSearchTests` — 9 tests pinning the SQL-side search: category filter, brand and name
    matching, filters combined with AND not OR, whitespace ignored, terms trimmed, `Category`
    still eager-loaded.
  - `CategoryServicesTests` — 7 tests, the only coverage of `CategoryServices.Delete` and its new
    `CategoryDeleteResult`.
- `ValidateImageFile` / `HasValidImageSignature` changed from `private` to `internal`, with
  `InternalsVisibleTo` in the web `.csproj`, so tests can reach them without making them public
  (which would wrongly suggest other code should call them).
- **`scripts/security-check.sh`** — re-runs all 20 checks from the security review against a
  running app. Exits non-zero on failure so it can go into a build pipeline later.
- `PROJECT_ROADMAP.md` — recorded the **conversion-focused redesign** as planned work (consumer
  and B2B), grounded in the competitor research already in `RESEARCH_LOG.md`.

**Why**
- AI sessions lose detail when their context fills and gets summarised. Documents survive that;
  conversations do not. But a document only records a *claim* about the past — "verified: admin
  pages return 302". A test is a *continuously enforced fact*: remove `[Authorize]` and it goes
  red, while the paragraph stays smugly true.
- So the aim was to convert everything verified by hand on 2026-08-11 into something executable.
  The split is deliberate: xUnit covers logic that needs no web server; the shell script covers
  what only exists once the app is running (auth redirects, antiforgery, HTTP status codes).

**How it was verified**
- `dotnet build` clean; **48/48 tests passing**.
- The security script was **proved able to fail**, which matters more than it passing:
  - pointed at a dead port → exits 1 with a clear message;
  - the `Url.IsLocalUrl` guard was deliberately removed, the app rebuilt, and the script correctly
    reported `FAIL external returnUrl was followed → https://evil.example.com/phish` and exited 1;
  - guard restored, rebuilt, all 20 checks pass and it exits 0.
- Two bugs in the script itself were found and fixed during that process:
  1. the cookie jar used an absolute `/tmp` path, which Windows `curl` cannot write while
     `MSYS_NO_PATHCONV=1` is set — so every request went out session-less and real checks failed
     for the wrong reason;
  2. the open-redirect check only asserted "did not go to evil.example.com", which an empty
     response satisfies trivially — a check that could never fail. It now asserts the redirect
     goes to `/Cart`.

**Follow-ups or known limitations**
- The security script must be run manually against a running app; it is not part of `dotnet test`.
  Proper integration tests (`WebApplicationFactory`) would fold these into the normal test run and
  are the natural next step.
- No test yet covers the role split at HTTP level (anonymous vs customer vs admin) — that is
  script-only for the same reason.

---

## 2026-08-11 — Authentication and roles (ASP.NET Core Identity)

**Type:** security / feature
**Author:** Claude (Sonnet 5) + Edgar

Closes the **critical** finding from the security review: the application had no authentication at
all, so anyone who knew a URL could create, edit and delete products.

**What changed**
- `ApplicationUser` (extends `IdentityUser`) with `FullName`, `Language` and `CreatedAt`.
  `UserRoles` holds the two role names as constants, so a typo becomes a compile error rather than
  a silent hole.
- `ElektriKalkulaatorContext` now inherits `IdentityDbContext<ApplicationUser>`, with
  `base.OnModelCreating` called first. Migration `AddIdentityTables` creates the seven `AspNet*`
  tables in the same database.
- `Program.cs`: `AddIdentity` + `AddRoles`, password rules (8+ chars, upper, lower, digit),
  lockout after 5 failed attempts for 5 minutes, unique email, and cookie paths for login and
  access-denied. **`app.UseAuthentication()` added before `app.UseAuthorization()`** — its absence
  was the root cause, since authorization has nothing to check without it.
- `AccountController` with Login, Register, Logout and AccessDenied. Logout is POST-only so
  another site cannot sign users out.
- `IdentitySeeder` creates both roles and the first admin at startup. Credentials come from
  configuration (User Secrets), never from constants — no password is committed.
- `ProductsController` carries `[Authorize(Roles = Admin)]` at class level, with `[AllowAnonymous]`
  on `Index` and `Details` only. Secure-by-default: a new action is protected unless explicitly
  opened.
- Login/Register/AccessDenied views styled to match the existing dark theme; `_Layout` shows the
  signed-in user, an admin-only link, and login/register or logout.

**Why this design**
- **Identity rather than a hand-written login.** Password hashing, lockout and token handling are
  exactly the things not to write yourself.
- **Same database and DbContext** — one connection string, one migration chain, and it finally
  gives the orphaned `PowerboxCalculation.UserId` a real table to reference, as the ERD spec
  intended.
- **Hand-written MVC views rather than scaffolded Identity Razor Pages** — the project is MVC with
  a custom theme; scaffolding would add dozens of inconsistent files.
- **Admins are seeded, never self-registered.** Everyone registering through the form gets
  `Customer`; otherwise anyone could grant themselves catalogue deletion rights.

**How it was verified**

Build clean, `dotnet test` **16/16 passing**, plus the full flow exercised over real HTTP:

| Check | Result |
|---|---|
| Anonymous → `/Products/Create`, `/Edit`, `/Delete`, `/Categories` | all `302` → `/Account/Login` (were `200`) |
| Anonymous → `/`, `/Products`, `/Calculator`, `/Cart`, `/Products/Details` | all `200` — public pages unaffected |
| Admin login | `302`, then admin pages return `200`, nav shows the account |
| Wrong password | rejected, *"Vale e-post või parool."*, still blocked |
| Customer registration | `302`, auto-signed-in |
| **Logged-in customer → admin pages** | `302` → `/Account/AccessDenied` (authenticated but wrong role — correct behaviour) |
| Customer → calculator / products / cart | all `200` |
| Weak password `abc` | refused, no account created |
| Logout | `302`, admin pages blocked again |

Database confirmed directly: two users with correct roles, and `PasswordHash` stored as a hash,
not plain text. Test customer account deleted afterwards; only the seeded admin remains.

**Follow-ups or known limitations**
- `PowerboxCalculation.UserId` still is not populated when a signed-in user runs a calculation —
  the table now exists to point at, but the wiring is not done.
- No email confirmation, password reset or "remember me" hardening. Fine for a thesis demo.
- Cart is still session-based, so it is lost on logout and not tied to the account.
- Admin credentials for this machine are in User Secrets. Anyone else cloning the repo must set
  `AdminUser:Email` and `AdminUser:Password` themselves or no admin is created (a warning is logged).

---

## 2026-08-11 — Security hardening: CSRF, open redirect, input validation, config

**Type:** security
**Author:** Claude (Sonnet 5) + Edgar

Fixes eight of the ten findings from the security review. The two remaining — missing
authentication, and stock never being reserved/decremented — are tracked separately: authentication
is its own piece of work, and stock belongs with the real `Order` entity.

**What changed**

*Cross-site request forgery*
- `[ValidateAntiForgeryToken]` added to `CartController.Add/Remove/Clear/Checkout` and
  `CalculatorController.Index [HttpPost]`. `ProductsController` already had it; these five were
  missed. The forms were already sending the token — it simply was never checked.

*Open redirect*
- `CartController.Add` passed `returnUrl` straight to `Redirect()`. New private `SafeRedirect`
  helper only redirects when `Url.IsLocalUrl(returnUrl)` is true, otherwise falls back to the cart.

*Cart input validation*
- Quantity must now be between `MinQuantity` (1) and `MaxQuantity` (999); repeated adds are clamped
  with `Math.Min` so they cannot creep past the maximum.
- The product must exist — `Add` looks it up and returns `NotFound()` otherwise.
- `Add` became `async` to allow that lookup.

*Upload content verification*
- New `HasValidImageSignature` checks the file's leading "magic bytes" against JPEG, PNG, GIF and
  WEBP signatures. Extension checking alone accepted any renamed file.

*Search performance*
- New `IProductServices.Search(categoryId, searchTerm)` applies both filters to the `IQueryable`
  **before** `ToListAsync()`, so they become part of the SQL `WHERE` clause.
  `ProductsController.Index` previously fetched every product and filtered the list in C#.
- Dropped `ToLower()` — SQL Server's default collation is already case-insensitive, and omitting it
  lets the database use an index rather than transforming every row.

*Consistency*
- `CategoryServices.Delete` no longer throws bare `Exception`s. It returns a new
  `CategoryDeleteResult` enum (`Deleted` / `NotFound` / `StillHasProducts`).
  **Note: this method is currently unreachable** — no controller calls it and no view offers
  category deletion. Kept and corrected because the planned admin area will need it.

*Configuration*
- `appsettings.json` now ships a portable `(localdb)\MSSQLLocalDB` default so a fresh clone runs.
  The real connection string moved to **User Secrets**, which live outside the project folder and
  are never committed. Edgar's machine keeps working because the secret was set locally.

*Language*
- Cart messages converted from English to Estonian to match the rest of that UI.

**How it was verified**

Build clean, `dotnet test` **16/16 passing**, plus every exploit from the review re-run against the
running app. Each fix was tested **with a valid antiforgery token**, so no result is hidden behind
another guard:

| Exploit | Before | After |
|---|---|---|
| `returnUrl=https://evil.example.com/phish` | `302 → evil.example.com` | `302 → /Cart` |
| Legitimate `returnUrl=/Products` | worked | still works (no over-blocking) |
| POST with no token (Add / Clear / Calculator) | `302 / 302 / 200` | `400 / 400 / 400` |
| `quantity=-5` | cart showed `-3 tk / -27.60 €` | rejected, cart unchanged |
| Nonexistent product ID | `302` accepted | `404` |
| Text file renamed `.jpg` | accepted | rejected, *"Fail ei ole korrektne pildifail."* |
| Real JPEG | accepted | still accepted |

Search correctness after moving to SQL: `ABB`→5, `Schneider`→2, `kaabel`→3, category *Juhtmed*→3,
category+term→3, nonsense term→"not found". User Secrets confirmed connecting to the real database
(all 10 seeded products present). Test data removed; catalogue back to 10 products, uploads empty.

**Follow-ups or known limitations**
- **Authentication is still absent** — the critical finding. Next piece of work.
- Stock is still never reserved or decremented; belongs with real order persistence.
- `TempData` messages in `ProductsController` remain in English while the cart is now Estonian.

---

## 2026-08-11 — Upload problems now show as form messages instead of crashing

**Type:** bugfix
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- New `ProductsController.ValidateImageFile(IFormFile?)` — returns `null` when a file is
  acceptable (including when no file was chosen, since the image is optional), or an
  Estonian-language message explaining the problem.
- `Create` and `Edit` POST actions call it first and pass any message to
  `ModelState.AddModelError("imageFile", …)`, so a rejected file behaves like any other failed
  form validation.
- `SaveProductImage` no longer throws for validation; it now assumes the file was already checked
  and only does the saving.
- `Views/Products/Create.cshtml` and `Edit.cshtml`: added
  `@Html.ValidationMessage("imageFile", …)` beneath the file input so the message is visible.

**Why**
- Previously `SaveProductImage` threw `InvalidOperationException` for a wrong file type or an
  oversized file. Nothing caught it, so the user got a **blank HTTP 500 error page**, lost
  everything they had typed into the form, and were given no clue what was wrong. This was
  recorded as a known limitation when the upload feature was added; this change closes it.
- Splitting "is this file acceptable?" from "save this file" is what makes the fix possible — the
  controller can now ask the question *before* committing to anything, and report the answer in
  the normal way.
- Messages are in Estonian because every label on that form is in Estonian. Note the codebase is
  inconsistent here: `TempData` success messages are still in English. Worth unifying later.

**How it was verified**
- `dotnet build` clean; `dotnet test` **16/16 passing**.
- Three scenarios exercised against the running app over real HTTP (antiforgery token and session
  cookie included, exactly as a browser would):
  1. **Wrong type** (`.txt`) → HTTP **200**, message *"Sobimatu failitüüp. Lubatud on: .jpg,
     .jpeg, .png, .gif, .webp."* rendered in a `field-validation-error` span, the typed product
     name preserved in the redisplayed form, **no product created**.
  2. **Oversized** (6 MB) → HTTP **200**, message *"Pilt on liiga suur (suurim lubatud maht on
     5 MB)."*, **no product created**.
  3. **Valid JPEG** → HTTP **302** redirect, product created, file written to
     `wwwroot/images/uploads/` (confirming the earlier folder split), and removed again when the
     product was deleted.
- Test data created during verification was deleted; catalogue is back to its 10 seeded products
  and the uploads folder is empty.

**Follow-ups or known limitations**
- File *content* is still not inspected — a renamed executable with a `.jpg` extension would pass.
  Acceptable for a local thesis demo, must be revisited before any public deployment.
- Success/error message language is inconsistent across the app (English `TempData`, Estonian
  validation).

---

## 2026-08-11 — Fix: seeded product photos were excluded from git; separate uploads from shipped images

**Type:** bugfix
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- Uploaded images now save to **`wwwroot/images/uploads/`** instead of `wwwroot/images/products/`
  (`ProductsController.SaveProductImage`). New constants `UploadsFolderName` / `UploadsWebPath`.
- `DeleteProductImageFile` now deletes **only** files under the uploads folder, and resolves the
  target by filename rather than by joining the raw database path.
- `.gitignore`: ignores `wwwroot/images/uploads/*` instead of `wwwroot/images/products/*`.
- The four seeded photos (`breaker.jpg`, `cable.jpg`, `rcd.jpg`, `enclosure.jpg`) are now **tracked
  in git**. `.gitkeep` moved from `products/` to `uploads/`.

**Why — two real bugs, both introduced by the previous two changes**

1. **The shipped photos were never committed.** The `.gitignore` rule added with the upload
   feature excluded everything in `wwwroot/images/products/`, with an exception for `*.svg`. When
   the SVG illustrations were later replaced by `.jpg` photos, that exception stopped matching.
   The result: `main` contained seed data pointing at `/images/products/breaker.jpg` while the
   file itself was not in the repository — **a fresh clone would render ten broken images.**
   Confirmed by inspecting the commit: it added the two hero images but none of the four product
   photos.
2. **Deleting one product could destroy another product's image.** All five breakers share
   `breaker.jpg`. The old delete logic removed whatever file `Product.ImagePath` pointed at, so
   deleting a single breaker would have deleted the image still displayed by the other four.
   Splitting shipped assets from uploads fixes this by construction: shipped files are never
   deleted at runtime.

   The same change also removes a path-traversal foothold — the old code joined a database string
   directly onto `WebRootPath`, so a malformed value such as `../../appsettings.json` would have
   been deleted. Deletion is now confined to one known folder.

**How it was verified**
- `dotnet build` clean; `dotnet test` **16/16 passing**.
- `git check-ignore` confirms all four seeded photos are now trackable and that a probe file in
  `uploads/` is still ignored.
- **Regression test against the running app:** deleted seeded product `…0001` (one of the five
  breakers) and confirmed `breaker.jpg` still exists on disk afterwards, with the remaining four
  breakers still rendering it. Under the previous code this deletion removed the shared file.
- The seeded row deleted during that test was restored via SQL; the catalogue is back to 10
  products.

**Follow-ups or known limitations**
- Uploaded images are still deleted immediately with no undo.
- There is no cleanup for uploads orphaned by an error between saving the file and saving the row.

---

## 2026-08-11 — Real product photography for the seeded catalogue

**Type:** feature
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- Added four freely-licensed photographs to `wwwroot/images/products/`: `breaker.jpg`,
  `cable.jpg`, `rcd.jpg`, `enclosure.jpg`. Added two hero images to `wwwroot/images/hero/`.
  **All images are `.jpg` — one format across the whole catalogue.**
- `ElektriKalkulaatorContext.SeedProducts`: all ten seeded products now have an `ImagePath`.
- New migrations `SeedProductImagePaths` and `UseRealProductPhotos`.
- `site.css`: `.product-card-img` changed from `object-fit: cover` to `contain`.
- New `IMAGE_CREDITS.md` at the repo root — required attribution for the CC BY-SA image.
- New `RESEARCH_LOG.md` at the repo root — a separate log for externally gathered data.
- `.gitignore`: uploaded images stay ignored, but images shipped with the seed data are tracked.

**Why**
- Product images were the largest remaining visual gap. Baymard Institute research (see
  `RESEARCH_LOG.md`) finds product thumbnails to be a primary decision aid in list views, and all
  three comparable Estonian retailers show a photo on every list item.
- **Manufacturer photos (ABB, Schneider) are copyrighted and were deliberately not used.** A real
  distributor receives a media kit; a student project has no such agreement, and this thesis will
  be publicly defended and archived. Images come from Wikimedia Commons (CC0 / CC BY-SA) and
  Unsplash / Pexels instead.
- An earlier version of this change used in-house SVG illustrations. Those were replaced with real
  photos on request, and because mixing SVG with uploaded JPGs would mean two formats in one
  catalogue.
- The chosen `breaker.jpg` shows **single-pole** DIN-rail breakers, matching the 1-pole form
  factor of the seeded S201 products. An official ABB photo (CC BY-SA) was available but is
  **3-pole**, so it was rejected as technically misleading.

**How it was verified**
- `dotnet build` clean; `dotnet test` **16/16 passing**.
- App run locally: all ten product cards render an `<img>` (zero placeholders remaining), and all
  four images return `HTTP 200 image/jpeg` with correct byte sizes.
- Verified the rendered `src` attributes contain **no stray whitespace** — an intermediate `sed`
  edit had accidentally placed alignment padding *inside* the string literals, which would have
  been written into the database. Caught and fixed before the migration was generated.

**Follow-ups or known limitations**
- The photos show the correct component *type* but not the exact SKU. The UI does not yet label
  them as illustrative, which Estonian retailers normally do — worth adding.
- `breaker.jpg` is CC BY-SA, so the attribution in `IMAGE_CREDITS.md` must be reachable from the
  running site, not only from the repository, before any public deployment.
- Hero images are downloaded but **not yet used** by any view.

---

## 2026-08-10 — Added automated test project (`ElektriKalkulaator.Tests`)

**Type:** test
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- New project `ElektriKalkulaator/ElektriKalkulaator.Tests/` (xUnit + EF Core InMemory),
  registered in `ElektriKalkulaator.slnx`.
- `TestBase.cs` — sets up a throwaway dependency-injection container per test class, backed by an
  in-memory database seeded from the real `ElektriKalkulaatorContext` model.
- `CalculatorServicesTests.cs` — 11 tests covering the calculator's core rules: ceiling division
  for lighting (8 lights → 1 circuit, 9 lights → 2), sockets (6 per circuit), the conditional
  stove circuit, "enclosure + RCD are always added", empty result for an unknown building type,
  the cheapest-in-stock product selection, that out-of-stock products are skipped even when
  cheaper, wire-length scaling by room count, and that `SaveCalculation` writes all three tables.
- `ProductServicesTests.cs` — 5 tests, including direct regression tests for the nullable bug
  fixed in the entry below.

**Why**
- The project had **zero** automated tests. This was the single most exploitable gap for a thesis
  defense — especially since the project's main argument is that a deterministic rule engine beats
  an LLM precisely *because* it's testable. Making that claim without tests undercuts it.
- `CalculatorServices.Calculate()` is pure arithmetic over seeded data — the cheapest possible
  thing to test and the most expensive to get silently wrong (it produces a shopping list for
  electrical work).
- Pattern copied from Edgar's own prior coursework (`Veebiarendus/Car_TARge24/TARge24_Car_Test`),
  so it uses concepts already familiar from the course rather than introducing anything new.

**How it was verified**
- `dotnet test` → **16/16 passing**.
- **Mutation-tested**: deliberately changed `Math.Ceiling` to `Math.Floor` in
  `CalculatorServices.cs` and re-ran — the 9-lights test failed as it should
  (`Expected: 2, Actual: 1`), then the change was reverted and all 16 passed again. This proves
  the tests actually detect regressions rather than passing vacuously.
- The first run also caught a genuine mistake in the *test* code (an assertion assumed breaker BOM
  lines have a null `WireCrossSectionMm2`, but `Calculate()` copies the rule's value onto both the
  breaker and the cable line). Fixed the assertions, not the app.

**Follow-ups or known limitations**
- No tests for `CategoryServices` or the controllers yet.
- No UI/end-to-end tests. `Tarkvarasüsteemide_Testimine/SeleniumShopUITestSampleTARge24-main` is
  the available template if those are ever wanted.

---

## 2026-08-10 — Product image upload and display

**Type:** feature
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- `ProductsController.cs`: injected `IWebHostEnvironment`; `Create`/`Edit` POST actions now accept
  an `IFormFile? imageFile`; added private helpers `SaveProductImage()` and
  `DeleteProductImageFile()`.
- `Views/Products/Create.cshtml` + `Edit.cshtml`: added `enctype="multipart/form-data"` and a file
  input. `Edit` also renders the current image and round-trips the existing path through a hidden
  field, so saving the form without picking a new file keeps the old image.
- `Views/Products/Index.cshtml`: product cards now show the image, or a 📦 placeholder box when a
  product has none.
- `Views/Products/Details.cshtml`: shows the image when present.
- `wwwroot/css/site.css`: added `.product-card-image`, `.product-card-img`,
  `.product-card-img-placeholder`, matching the existing dark-navy/amber theme.
- `.gitignore`: uploaded images are user content, not source — the folder is kept via `.gitkeep`
  but its contents are ignored.

**Why**
- `Product.ImagePath` had existed in the domain model since the project began but **no view ever
  rendered it**, and no form ever populated it. The field was dead weight.
- This was also the single biggest visual gap versus the React/TypeScript design draft
  (`elecpro-components-&-calculator`), whose `ProductCard.tsx` is image-led.
- Files are saved into `wwwroot/images/products/` rather than a folder outside `wwwroot`, because
  `wwwroot` is already served by the default static-file middleware — no extra
  `StaticFileOptions`/`PhysicalFileProvider` registration needed. (`JustShop2` uses the
  outside-`wwwroot` approach and consequently needs that extra setup; `AdvancedAjax` uses the
  simpler one adopted here.)

**Security hardening applied in the same change**
- The first working version accepted **any** file type. Since this controller has no
  authentication, that meant anyone reaching the form could write arbitrary files to the server.
  Added: an extension allow-list (`.jpg .jpeg .png .gif .webp`) and a 5 MB size cap.
- Saved filenames are freshly generated GUIDs, so an upload can never collide with or overwrite
  another product's image, and the untrusted client-supplied filename is discarded apart from its
  extension.
- Old image files are now deleted from disk when a product is deleted, or when Edit replaces an
  existing image — otherwise every re-upload would leave an orphaned file behind forever.

**How it was verified**
- Ran the app locally and exercised the real HTTP endpoints with `curl` (antiforgery token and
  session cookie included, i.e. the same path a browser takes):
  - upload a PNG on Create → file written to disk, `<img>` rendered on `/Products`, image served
    with `HTTP 200 image/png`;
  - upload a `.txt` → rejected, no product created, no file written;
  - edit a product **without** touching the file input → existing image preserved;
  - edit **with** a new file → new image saved, old file removed from disk;
  - delete the product → its image file removed from disk.
- Two apparent test failures during this work turned out to be flaws in the test commands, not the
  app: Git Bash rewrites leading-`/` arguments into Windows paths (fixed with
  `MSYS_NO_PATHCONV=1`), and one `curl` invocation omitted the hidden `ImagePath` field that a real
  browser form always sends.
- Test data created during verification was deleted afterwards; the catalogue is back to its 10
  seeded products.

**Follow-ups or known limitations**
- Validation failures throw `InvalidOperationException`, which surfaces as a raw 500 error page
  rather than a friendly "wrong file type" message on the form. Should become a `ModelState` error.
- Extension is checked, but file *content* is not — a renamed executable with a `.png` extension
  would still be stored. Acceptable for a local thesis demo; **must** be revisited before any
  public deployment.
- No image resizing/compression; a 5 MB image is served as-is.

---

## 2026-08-10 — Added `EvsReference` field to `CalculationRule`

**Type:** feature
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- `CalculationRule.cs`: new nullable `string? EvsReference` property.
- `ElektriKalkulaatorContext.cs`: `HasMaxLength(50)` for the new column.
- New migration `20260810182640_AddEvsReferenceToCalculationRule`.

**Why**
- Edgar's own written database specification (`ERD_Loogiline_Seletus.docx`, §3.4) defines an
  `evs_reference` field on `CALCULATION_RULES` — "Viide EVS-HD 60364 standardile (nt
  'EVS-HD 60364-4-41')" — but the implemented entity never had it. This closes a documented
  design-vs-code gap.
- It directly strengthens the project's best defence argument: that the calculation is
  *auditable and traceable to a published standard*, unlike an LLM's guess. With this field
  populated, each BOM line can eventually cite the exact clause it came from.

**How it was verified**
- `dotnet build` clean; `dotnet ef migrations add` generated the expected `AddColumn` +
  nullable seed updates; app starts and applies migrations without error.

**Follow-ups or known limitations**
- **The field is intentionally left empty in seed data.** Real EVS-HD 60364 clause numbers were
  not invented — a fabricated standard citation in a thesis about standards compliance would be
  considerably worse than a blank field. Edgar needs to fill these in from the actual standard
  text, then the values can be surfaced in the calculator's output.

---

## 2026-08-10 — Fixed nullable-annotation lie and inconsistent not-found handling

**Type:** bugfix
**Author:** Claude (Sonnet 5) + Edgar

**What changed**
- `IProductServices` / `ICategoryServices`: `GetById` now returns `Task<Product?>` /
  `Task<ProductCategory?>`; `IProductServices.Update` and `Delete` now return `Task<Product?>`.
- `ProductServices.cs` / `CategoryServices.cs`: removed the null-forgiving `!` operators;
  `Update` and `Delete` now return `null` when the record doesn't exist instead of throwing a
  bare `Exception`.
- `ProductsController.cs`: the `Edit` POST and `DeleteConfirmed` actions now check for `null` and
  return `NotFound()`.

**Why**
- **The bug:** `GetById` was declared as returning a non-nullable `Product`, but its body used
  `(await ...FirstOrDefaultAsync(...))!` — the `!` silences the compiler while the method can
  still return `null` at runtime. The type signature was actively lying, so any future caller
  trusting it (`product.Name` without a null check) would get a `NullReferenceException`.
- **The inconsistency:** within the same file, `GetById` returned `null` for "not found" while
  `Update`/`Delete` threw a generic `Exception` for exactly the same condition. Two different
  conventions for one concept. The thrown exception also produced a raw 500 error page instead of
  a proper 404, e.g. if two people deleted the same product at once.
- The null-return convention was chosen (rather than a custom exception) to match ShopTARge24 —
  the course reference project this codebase is explicitly modelled on — and because
  `ProductsController` was already written expecting it.

**How it was verified**
- `dotnet build` clean.
- Live check against the running app: `/Products/Details/<nonexistent-guid>` and
  `/Products/Delete/<nonexistent-guid>` both return **HTTP 404** (previously a 500 for the
  delete/update paths).
- Later covered by automated regression tests — see the test entry above.

**Follow-ups or known limitations**
- `ProductServices.Update` still throws for a genuinely missing `dto.Id`. That's a different case
  (a malformed request, not a missing record) and is left as-is deliberately.
- `ICategoryServices.Delete` still throws when a category still has products attached — also a
  different case (a rule violation with a message meant for the user), left as-is.
