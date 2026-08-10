# ElektriKalkulaator — Project Roadmap & AI Context Document

> **Purpose of this file.** This is a self-contained briefing for any AI model (or human) picking
> up this project without prior conversation history. It covers what the project is, how it got
> here, what's built, what's missing, what's deliberately deferred (and why), and what to do next.
> It is a **living document** — append to it, don't just read it. See "How to keep this document
> updated" at the end before you finish a work session on this project.
>
> Last updated: **2026-08-10**, by Claude (Sonnet 5), in collaboration with Edgar.

---

## 1. Project identity

| | |
|---|---|
| **Project name** | ElektriKalkulaator (Estonian repo name: `Elektrikilbi-ja--tarvete-komponentide-kalkulaator` — "electrical panel and supplies component calculator") |
| **Author** | Edgar Muoni |
| **Type** | LÕPUTÖÖ — diploma/graduation thesis project |
| **Institution** | Tallinna Tööstushariduskeskus (per footer credit found in project planning docs) |
| **Supervisor (juhendaja)** | Kalle Olumets |
| **Course/cohort context** | TARge24 |
| **Repo root (this file's location)** | `C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\Elektrikilbi-ja--tarvete-komponentide-kalkulaator\` |
| **Git status as of writing** | Single branch `main`, 4 commits, solo author (Edgar), Apr 26 – May 7 2026 |

**Important scope note for any AI reading this:** the repo map in §4 below includes paths *outside*
this git repository, on Edgar's local machine (a much larger personal school-work repo at
`C:\Users\Jazztime\Desktop\TARge24\`). If you are only given a clone of *this* repo, those paths
won't exist for you — treat that material as background you're being told about, not files you can
open. If you do have access to the full `TARge24` tree, it's genuinely useful reference material
(see §9–§10).

---

## 2. Elevator pitch & vision

**Now (thesis deliverable):** a working ASP.NET Core web app where an Estonian electrician or
homeowner enters a building type + room/socket/light counts, and gets back a priced Bill of
Materials (BOM) — which breakers, cable, an RCD, and an enclosure they need — computed
deterministically from the Estonian electrical installation standard **EVS-HD 60364**, matched
against a real (seeded) product catalogue with real prices and stock levels.

**Later (post-thesis, explicitly out of scope for the defense):** grow this into a real
client-facing e-commerce site, Shopify-like, where the calculator's BOM becomes an actual
customer order, and the site acts as a **middleman/dropshipping** operation — relaying that order
to suppliers/producers rather than holding its own stock. The calculator stays the core,
differentiating feature of the product; e-commerce/fulfillment is built around it, not the other
way around.

---

## 3. Repository & file map

### 3.1 The actual buildable solution (this repo)

```
Elektrikilbi-ja--tarvete-komponentide-kalkulaator/          ← git root, this file lives here
└── ElektriKalkulaator/
    ├── ElektriKalkulaator.slnx                              ← solution file
    ├── ElektriKalkulaator.Core/                              ← Domain, DTO, ServiceInterface (no deps)
    │   ├── Domain/        Product.cs, ProductCategory.cs, CalculationRule.cs,
    │   │                  PowerboxCalculation.cs, PowerboxRequirements.cs, PowerboxComponents.cs
    │   ├── Dto/           CalculatorInputDto.cs, BOMItemDto.cs, ProductDto.cs
    │   └── ServiceInterface/  ICalculatorServices.cs, IProductServices.cs, ICategoryServices.cs
    ├── ElektriKalkulaator.Data/                              ← EF Core DbContext + migrations
    │   ├── ElektriKalkulaatorContext.cs                       (seed data lives here too)
    │   └── Migrations/    20260429200202_InitialCreate.cs (+ Designer + Snapshot)
    ├── ElektriKalkulaator.ApplicationServices/               ← service implementations
    │   └── Services/      CalculatorServices.cs, ProductServices.cs, CategoryServices.cs
    └── ElektriKalkulaator/                                   ← the ASP.NET Core web project
        ├── Program.cs
        ├── Controllers/   CalculatorController.cs, CartController.cs, ProductsController.cs, HomeController.cs
        ├── Views/          Calculator/, Cart/, Products/, Home/, Shared/
        ├── wwwroot/        css/site.css (custom dark-navy/amber theme over Bootstrap 5), lib/ (bootstrap, jquery)
        └── appsettings.json  (SQL Server connection string, local `DESKTOP-KMVSVQK` server)
```

Stack: **ASP.NET Core 9 MVC + EF Core 9 (SQL Server)**, Razor views + Bootstrap 5 with custom CSS
variables for a dark theme, session-based cart (`Newtonsoft.Json`-serialised `Dictionary<Guid,int>`
in ASP.NET session, 30-min idle timeout), no authentication anywhere.

### 3.2 Reference / planning material (outside this repo, same machine)

All under `C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\Elektrikilbi ja -tarvete kalk draft\` unless
noted:

| File/folder | What it is |
|---|---|
| `elecpro-components-&-calculator/` (React+TS+Vite) | Early **AI-estimator-based** UI/UX prototype (dark slate/yellow theme, `en`/`et`/`ru` i18n, calls Gemini for the BOM). See §10. First artifact created (2026-01-29). |
| `whole-building-calculator-erd.mermaid`, `powerbox-calculator-erd.mermaid`, `Whole_Building_Electrical_ERD.png` | Earliest ERD explorations (2026-02-02), still AI-service-oriented (`AI_ESTIMATOR`/`AI_SERVICE` entities), and **US-centric** (NEC code references, 120V/240V/600V, AWG wire gauge, `service_amperage` 100A/200A/400A) — not yet localized to Estonia. |
| `powerbox-no-ai-visual-explanation.html` | Explicit **pivot document** (2026-02-03): a written AI-vs-rule-based comparison that concludes rule-based is correct for this use case (predictability, zero ongoing cost, offline capability, guaranteed code compliance, "can customize for Estonian regulations"). Still uses NEC/AWG terminology at this point — the reasoning came before the localization. |
| `Elektrikilbi ja -tarvete komponentide kalkulaator .drawio/.png/.svg`, `elektrikomponentide-kalkulaator-erd.html`, `ERD_Loogiline_Seletus.docx`, `tabelite-seosed-seletus.html` | **Final, Estonia-localized** ERD + explanation docs (2026-03-08/09). Entities/fields here (`POWERBOX_CALCULATION`, `POWERBOX_REQUIREMENTS`, `POWERBOX_COMPONENT`, `PRODUCT`, `PRODUCT_CATEGORY`, `CALCULATION_RULES`) map almost 1:1 onto the real C# domain model in §7. Building types are Estonian (`korterelamu`/`eramu`/`ärihoone`), standard reference is `evs_reference` (EVS-HD 60364), not NEC. This is the actual design document the real implementation was built from. |
| `Lõputöö kavand_VORM.docx` | The official thesis proposal/outline form (touched 2026-04-24, right before coding started). **Not yet read by any AI in this project's history** — worth reading before writing the thesis's own introduction/scope chapters, since it's the officially submitted proposal and should match what actually got built. |

### 3.3 Reference codebases from Edgar's broader coursework (same machine, `C:\Users\Jazztime\Desktop\TARge24\`)

| Project | Path | Relevance |
|---|---|---|
| **ShopTARge24** (Edgar's own prior coursework) | `Veebiarendus/ShopTARge24/ShopTARge24/` | The architectural template this thesis's 4-project layering was explicitly modeled on (a code comment in `ProductServices.cs` says so directly). Same `Core`/`Data`/`ApplicationServices`/Web shape. Has 3 xUnit test projects with a reusable `TestBase` (in-memory EF) pattern — directly reusable, since this thesis has zero tests. Full comparison in §9. |
| **ShopTARge24_opetaja** | `Veebiarendus/ShopTARge24_opetaja/ShopTARge24/` | Teacher's reference solution for the same project — useful as the "intended" version if ShopTARge24 itself has inconsistencies. |
| **SeleniumShopUITestSampleTARge24** | `Tarkvarasüsteemide_Testimine/SeleniumShopUITestSampleTARge24-main/` | Shows how ShopTARge24 gets Selenium-tested end-to-end — a template for UI-level testing of the Calculator/Cart flows. |
| **JustShop2** | `Agiilsed tarkvaraarenduse metoodikad/JustShop/` | Same layered architecture, built for the Agile methodologies course. |

---

## 4. Domain model (full detail)

All entities in `ElektriKalkulaator.Core/Domain/`, `Guid` primary keys, `CreatedAt`/`ModifiedAt`
timestamps set by the service layer via `DateTime.Now`.

- **`ProductCategory`** — `Id`, `Name`, `Description`. 1→N `Products`. Seeded categories (Estonian):
  Kaitselülitid (breakers), Juhtmed (cables), RCD / Rikkevoolukaitsmeid, Klemmid, Kilbi korpused
  (enclosures).
- **`Product`** — `Id`, `CategoryId` (FK), `Name`, `Brand`, `RatedCurrent` (decimal, used to match
  the right breaker: 10A/16A/32A), `Voltage`, `Price` (decimal, precision 10,2), `StockQuantity`,
  `ImagePath` (nullable — **exists but is never rendered in any view**, see §12),
  `WireCrossSectionMm2` (nullable decimal, only set for cables: 1.5/2.5/6.0mm²), `Description`.
  Seeded with real-ish ABB/Schneider/Draka products.
- **`CalculationRule`** — `Id`, `RuleName`, `BuildingType` (string: `korterelamu`/`eramu`/`ärihoone`
  — **exact-match join key, not just a label**, see §13), `WireCrossSectionMm2`, `BreakerAmperes`,
  `CircuitType` (string: `lighting`/`socket`/`stove`), `RoomsFrom`/`RoomsTo`. Seeded with 8 rules
  (3 circuit types × korterelamu/eramu, 2 for ärihoone) encoding EVS-HD 60364: lighting =
  1.5mm²/10A, sockets = 2.5mm²/16A, stove = 6.0mm²/32A.
- **`PowerboxCalculation`** — `Id`, `UserId` (nullable Guid — **exists, unused**, no auth system
  exists yet), `Status` (string, always `"completed"` in practice), `RulesApplied` (comma-joined
  circuit types), `TotalCost`. 1↔1 `Requirements`, 1→N `Components`.
- **`PowerboxRequirements`** — `Id`, `CalculationId` (FK, also the 1:1 join key), `BuildingType`,
  `RoomCount`, `SocketCount`, `LightCount`, `SwitchCount` (informational only, doesn't affect
  circuit math), `HasElectricStove`, `FloorCount`/`TotalAreaM2` (nullable, currently unused by the
  calculation itself).
- **`PowerboxComponents`** — one BOM line item: `Id`, `CalculationId` (FK), `ProductId` (FK),
  `Quantity`, `UnitPrice`, `TotalPrice`, `CircuitType`, `WireCrossSectionMm2`.

### DTOs (`ElektriKalkulaator.Core/Dto/`)
- `CalculatorInputDto` — the form model for `/Calculator` (validated with `[Required]`/`[Range]`).
- `BOMItemDto` — one output row (`ProductId`, `ProductName`, `Brand`, `Quantity`, `UnitPrice`,
  `TotalPrice`, `CircuitType`, `WireCrossSectionMm2`).
- `ProductDto` — the form model for Create/Edit product pages.

Note: this is a **two-layer** model (Domain + Dto), not three like ShopTARge24 (which adds a
web-project `ViewModel` layer) — see §13 for why that's a deliberate simplification, not an
oversight.

---

## 5. Core algorithm — `CalculatorServices.Calculate()`

File: `ElektriKalkulaator.ApplicationServices/Services/CalculatorServices.cs`

1. Load all `CalculationRule` rows where `BuildingType == input.BuildingType` (exact Estonian
   string match). Return empty BOM if none match.
2. Compute circuit counts: `lightingCircuits = ⌈LightCount / 8⌉`, `socketCircuits = ⌈SocketCount / 6⌉`,
   `stoveCircuits = HasElectricStove ? 1 : 0`.
3. Rough wire estimate: `wireLengthPerCircuit = RoomCount × 8` metres.
4. For each matching rule: find the cheapest in-stock `Product` with `RatedCurrent ==
   rule.BreakerAmperes` in category "Kaitselülitid" → one BOM line (breaker). Find the cheapest
   in-stock `Product` with `WireCrossSectionMm2 == rule.WireCrossSectionMm2` in category "Juhtmed"
   → one BOM line, quantity = `circuitCount × wireLengthPerCircuit` metres.
5. Always append one enclosure (cheapest in-stock "Kilbi korpused" product) and one RCD (cheapest
   in-stock "RCD / Rikkevoolukaitsmeid" product), quantity 1 each.
6. `SaveCalculation()` persists everything across `PowerboxCalculation` (header + total cost +
   comma-joined circuit types) / `PowerboxRequirements` (the input snapshot) /
   `PowerboxComponents` (each BOM line, one row per line).
7. `GetHistory()` returns the last 50 calculations with `Requirements` and `Components.Product`
   eager-loaded, for the `/Calculator/History` page.

This is pure, deterministic, side-effect-light logic (once you exclude the DB reads) — see §13 for
why this matters as a defensible engineering decision, and §14 for why it's currently untested
despite being the easiest thing in the codebase to test.

---

## 6. Design & planning history (chronological, dated from real file timestamps)

This is a genuinely useful narrative for the thesis's own "methodology" or "development process"
section — it shows deliberate, reasoned pivots, not just "I built the first thing that came to
mind."

1. **2026-01-29** — First artifact: the React/TypeScript/Vite UI prototype
   (`elecpro-components-&-calculator`), calculator powered by a live call to Google Gemini
   (`gemini-3-flash-preview`), generic building types (residential/commercial/workshop), no
   pricing in the output, disclaimer telling users to "consult a licensed electrician."
2. **2026-02-02** — Two ERD explorations (`whole-building-calculator-erd.mermaid`,
   `powerbox-calculator-erd.mermaid`), both still centered on an `AI_ESTIMATOR`/`AI_SERVICE`
   entity, and both **US-centric**: NEC code references, 120V/240V/600V, AWG wire gauges,
   100A/200A/400A service ratings. (Likely inherited from a generic/US-oriented starting template
   — worth being upfront about this in the thesis rather than glossing over it.)
3. **2026-02-03** — `powerbox-no-ai-visual-explanation.html`: an explicit, written **AI vs.
   rule-based comparison** (cost, speed, predictability, offline support, code-compliance
   guarantees) concluding that rule-based is the right call for a standards-driven calculation —
   and explicitly notes "can customize for Estonian regulations" as a stated advantage of going
   rule-based. This is the actual decision record for the architecture's most defensible choice.
4. **2026-03-08/09** — Final, **Estonia-localized** ERD + explanation set (`.drawio`/`.png`/`.svg`
   diagrams, `ERD_Loogiline_Seletus.docx`, `tabelite-seosed-seletus.html`): building types switch
   to `korterelamu`/`eramu`/`ärihoone`, standard reference switches from `nec_reference` to
   `evs_reference` (EVS-HD 60364), and the schema shape (`POWERBOX_CALCULATION` →
   `POWERBOX_REQUIREMENTS`/`POWERBOX_COMPONENT`, `PRODUCT` → `PRODUCT_CATEGORY`,
   `CALCULATION_RULES` joined by `building_type` with **no formal FK, deliberately** — the doc
   states explicitly "Siin EI OLE FK — C# kood ühendab need omavahel loogikaga" / "there is NO FK
   here — C# code joins these via logic") maps almost exactly onto the real EF Core model.
5. **2026-04-24** — `Lõputöö kavand_VORM.docx` (official thesis proposal form) touched.
6. **2026-04-26 to 2026-05-07** — Actual implementation, 4 git commits: "Establishing project" →
   "Creating another necessary project solutions" (the 4-project layering) → "Creating
   controllers, DTO, Views, Services, Domain and ServiceInterface" (the bulk of the app, ~4300
   lines in one commit) → "Changing localhost" (launch-settings tweak).

**Net effect**: ~5 weeks of design/planning exploration (including a real pivot away from an AI
API call, and a real localization pass from a US-oriented template to the Estonian standard),
followed by a concentrated ~2-week implementation. The React/TS prototype's *visual* direction
(dark theme, i18n) was kept as a north star even after its *calculation approach* (Gemini call)
was explicitly rejected — the real app inherited the look, not the AI dependency.

---

## 7. Relationship to ShopTARge24 (condensed — see full report for detail)

A full file-by-file comparison report was produced separately: it should still exist as a
Claude-published artifact from this project's session history (titled "ElektriKalkulaator —
Thesis Comparison Report"); if it's not available, regenerate it by reading
`Veebiarendus/ShopTARge24/ShopTARge24/` and diffing conventions against this repo, section by
section, as itemized below.

**Matches**: 4-project layering, manual DI registration style in `Program.cs`, domain model shape
(POCOs, `Guid` PKs, `CreatedAt`/`ModifiedAt`), naming conventions (`I{Entity}Services`,
`{Entity}Controller`), async patterns throughout.

**Deliberate, defensible deviations**:
- Two DTO layers instead of three (ShopTARge24 adds a web-project `ViewModel`; this project's
  single CRUD entity doesn't need the extra hop).
- Separate `Create.cshtml`/`Edit.cshtml` instead of ShopTARge24's combined `CreateUpdate.cshtml`.
- `ProductsController.Index()` correctly goes through the service layer where ShopTARge24's
  `RealEstateController.Index()` bypasses its own service layer — a genuine improvement.

**Real inconsistency, not a style choice** (fix before defense — see §15 Phase 1):
- `ProductServices.GetById`/`CategoryServices.GetById` are typed `Task<Product>`/`Task<ProductCategory>`
  (non-nullable) but can return `null` at runtime via a suppressed `!` warning — a genuine
  nullable-annotation bug. Meanwhile `Update`/`Delete` throw raw `Exception` on not-found instead
  of following ShopTARge24's null-return + controller-checks convention. Two different
  not-found conventions inside the same two files.

**Missing entirely**: automated tests. ShopTARge24 has 3 xUnit test projects with a reusable
`TestBase` (real DI container + `UseInMemoryDatabase`, not mocks) — directly portable to cover
`CalculatorServices.Calculate()`.

---

## 8. Relationship to the TypeScript/React design draft (condensed)

`elecpro-components-&-calculator` is a **visual/UX reference only** — never wired to the real
backend, its calculator calls Gemini instead of computing anything deterministically, and its
`CalculatedComponent` type has **no price field at all** (matches §6's timeline: this was the
pre-pivot AI-estimator approach).

**Already implemented in the real app**, contrary to how it might look from a first glance at
Bootstrap-based Razor views: `wwwroot/css/site.css` has a bespoke dark-navy (`--bg-primary:
#0f0f1a`) + amber (`--accent-amber: #F5A623`) theme layered over Bootstrap via CSS variables —
closely mirroring the draft's `slate-900`/`yellow-400` Tailwind palette. This was a deliberate,
successful adaptation, not a from-scratch Bootstrap default.

**Still missing vs. the draft**:
- Product images (`Product.ImagePath` exists in the domain model; no view renders it).
- Full i18n (`en`/`et`/`ru` in the draft vs. Estonian-only here) — and this is **structurally
  harder than a resource-file swap**: `CalculationRule.BuildingType` and `Product.Category.Name`
  are exact-string business-logic join keys in Estonian, not just UI labels. Real i18n needs
  stable language-independent codes first, which is a schema change (see §15 Phase 2).
- Broader product filtering (price range, brand, spec checkboxes) — the draft filters an in-memory
  mock array client-side; the real catalogue is SQL-backed and would need incremental `.Where()`
  additions, not a rewrite.

**The single most important finding**: the draft's AI-estimator output has no price and an
explicit "consult a licensed electrician" disclaimer. The real app's entire value proposition is
the *opposite* — a **priced**, auditable, testable BOM computed from a real rules table against a
real in-stock catalogue. This is the strongest, most explicitly-reasoned (see §6, step 3)
engineering decision in the whole project, and should be the lead talking point in the defense.

---

## 9. Current implementation status (as of 2026-08-10)

**Built and working:**
- Full `Product`/`ProductCategory` CRUD (`/Products`, admin-style, no auth gate).
- Calculator form → BOM computation → persisted history (`/Calculator`, `/Calculator/History`).
- Session-based cart (`/Cart`) — add/remove/clear; `Checkout()` is **cosmetic only**, clears the
  session and shows a static confirmation view, writes nothing to the database.
- Automatic EF Core migration on startup (try/catch + logged, not present in ShopTARge24 —
  a genuine small improvement).
- Custom dark-navy/amber theme over Bootstrap 5.

**Not built / explicitly deferred:**
- Any automated tests (highest-priority gap, see §15 Phase 1).
- Authentication/authorization (`PowerboxCalculation.UserId` exists, unused).
- Real order persistence / payment (needed for the dropshipping vision, §16 Phase 3).
- i18n (deferred for structural reasons, §8).
- Product images in the catalogue view (cheap fix, §15 Phase 1).

**Known bugs / inconsistencies** (see §7 for detail): the `GetById` nullable-annotation lie and
the mixed throw/null-return convention in `ProductServices`/`CategoryServices`.

---

## 10. Explicit design decisions & rationale (defense-ready ledger)

Use this table when a committee member (or any future contributor) asks "why does X work this
way" — the answer already exists, don't re-derive it from scratch.

| Decision | Rationale |
|---|---|
| Rule-based (`CalculationRule` table + arithmetic) instead of an LLM call | Explicitly reasoned through in `powerbox-no-ai-visual-explanation.html` (2026-02-03): auditability, zero ongoing cost, 100% predictability, offline capability, guaranteed standard compliance — all essential for a safety-relevant electrical calculation, none of which an LLM call can guarantee. |
| Two DTO layers, not three like ShopTARge24 | The third layer (web-project `ViewModel`) existed in ShopTARge24 because its views needed shapes tailored to multi-entity forms with file uploads. This project's `Dto`s already serve that role directly for its one CRUD entity — a third layer would be pure boilerplate. |
| `CalculationRule` joined to `PowerboxRequirements` by string equality on `BuildingType`, with no formal FK | Deliberate, stated explicitly in `tabelite-seosed-seletus.html`: "there is NO FK here — C# code joins these via logic." Rules are a lookup table read by business logic, not a relational entity tied 1:1 to a specific requirements row. |
| Estonian-only strings, no i18n yet | Business-logic join keys (`BuildingType`, `Category.Name`) are Estonian strings; the thesis's core deliverable targets the Estonian market and the EVS-HD 60364 standard specifically. Correctly scoped out of v1 rather than overlooked — real i18n needs a schema change (stable codes), not a resource-file swap. |
| Separate `Create.cshtml`/`Edit.cshtml` instead of ShopTARge24's combined `CreateUpdate.cshtml` | A legitimate alternative convention — no runtime branching on `Model.Id.HasValue` to decide the heading/action, and the two forms genuinely diverge slightly (Create has no hidden Id field). |
| Session-only cart, cosmetic checkout | Matches the thesis's scope: demonstrating the calculator and catalogue, not a production checkout/payment flow. Explicitly the first thing to build for real in the post-thesis dropshipping phase (§16 Phase 3). |

---

## 11. Roadmap

Status legend: `[ ]` not started · `[~]` in progress · `[x]` done. Update these as work lands —
this is the part of the document that should change most often.

### Phase 0 — Workflow setup
- [x] Cross-session memory of project context established (Claude Code memory files, 2026-08-10).
- [x] This document created (2026-08-10).
- [ ] `CLAUDE.md` at repo root (Claude-Code-specific, can be a thin pointer to this file plus tool
      conventions) — optional, not yet requested as of this doc's writing.
- [ ] `.claude/agents/{architect,coder,tester}.md` custom subagent definitions — discussed,
      not yet built as of this doc's writing.

### Phase 1 — Pre-defense fixes (priority — do before the thesis defense date)
- [ ] Add `ElektriKalkulaator.Tests` (xUnit + `UseInMemoryDatabase`, port ShopTARge24's `TestBase`
      pattern). Minimum coverage: `CalculatorServices.Calculate()` circuit-count math, stove
      conditional, RCD+enclosure always present, empty result for unmatched `BuildingType`.
- [ ] Fix `ProductServices.GetById`/`CategoryServices.GetById` nullable-annotation lie; make
      `Update`/`Delete` not-found handling consistent (recommend: match ShopTARge24's null-return
      + controller-checks convention throughout).
- [ ] Render `Product.ImagePath` in `Views/Products/Index.cshtml`.
- [ ] *Optional if time remains*: broaden `/Products` filtering (price range, brand).
- [ ] Write the thesis's "future work" section from §8 (i18n rationale) + §16 (dropshipping
      roadmap) below.

### Phase 2 — Post-defense: TypeScript/React frontend
- [ ] Add a Web API surface (`[ApiController]`) alongside/replacing the MVC controllers —
      `Core`/`Data`/`ApplicationServices` untouched.
- [ ] Turn `elecpro-components-&-calculator` into a real app wired to the new API instead of
      Gemini.
- [ ] Do real i18n here — replace Estonian string join-keys with stable codes + a translation
      table, since the API contract is already being touched.

### Phase 3 — Post-defense: dropshipping/marketplace features
- [ ] `Order`/`OrderLine` entities + real checkout persistence (replace the cosmetic
      `CartController.Checkout()`).
- [ ] `Supplier` entity — per-supplier cost distinct from customer-facing `Product.Price`.
- [ ] Payment processing (Stripe.net is the standard C# choice).
- [ ] Customer accounts/auth (`PowerboxCalculation.UserId` already exists as a placeholder).
- [ ] Supplier order relay — start manual/semi-automated, add per-supplier API integration only
      where volume justifies it.

**Stack recommendation** (asked and answered 2026-08-10): keep **C#/.NET** for the backend
(EF Core, Web API, Hangfire/Quartz.NET for scheduled supplier orders, Stripe.net for payments are
all mature); add **TypeScript + React** for the frontend once past the defense — not a new bet,
it's finishing the `elecpro-components-&-calculator` prototype. Don't add a third language
(Python/Node) for supplier automation unless a specific supplier genuinely has no API — plain
`HttpClient` covers ordinary REST/webhook integration.

---

## 12. Open questions — don't assume, ask Edgar

- `Lõputöö kavand_VORM.docx` (the official thesis proposal) has not been read by any AI in this
  project's history. It may contain scope commitments, a defense date, or grading-criteria
  language that should override or refine anything in this document — read it before making
  scope decisions on Edgar's behalf.
- No confirmed defense date is recorded anywhere in this document — ask before treating "Phase 1"
  as urgent vs. relaxed.
- Whether Edgar wants the Phase 0 items (`.claude/agents/*`, standalone `CLAUDE.md`) built is
  still open as of this writing.

---

## 13. How to keep this document updated

Whoever (human or AI) does work on this project next:
1. Update the checkboxes in §11 as items move from not-started → in-progress → done.
2. If you make an architectural decision with a non-obvious rationale, add a row to §10 — that
   table exists specifically so the reasoning survives past the conversation that produced it.
3. Add a dated bullet to the Change Log below — one line is enough, but make it specific (what
   changed, and why, not just "updated code").
4. If you discover the actual state of the repo has drifted from what this document says
   (a section describes something that no longer matches the code), fix that section in place —
   don't leave stale claims standing next to new ones.

## 14. Change log

- **2026-08-10** — Initial version created (Claude Sonnet 5 + Edgar). Synthesized from: direct
  code reading of the full C# solution, a background-agent diff against ShopTARge24 +
  ShopTARge24_opetaja, direct reading of the `elecpro-components-&-calculator` TS draft and the
  ERD/planning docs (`.mermaid`, `.html`) in the "Elektrikilbi ja -tarvete kalk draft" folder, and
  real filesystem timestamps used to reconstruct the design-evolution timeline in §6. Not yet
  read: `Lõputöö kavand_VORM.docx`, `ERD_Loogiline_Seletus.docx` (both `.docx`, need the docx
  skill), `elektrikomponentide-kalkulaator-erd.html` (skipped as redundant with
  `tabelite-seosed-seletus.html`).
