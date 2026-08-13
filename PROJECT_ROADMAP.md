# ElektriKalkulaator — Project Roadmap & Context Document

**Revision 3** · last updated **2026-08-11** · maintained by Edgar Muoni with Claude

---

## §0 — Read this first

### What this document is
A self-contained briefing on the ElektriKalkulaator project, written so that **someone who has
never seen this codebase** — a new AI model with no chat history, a classmate, a reviewer, or
Edgar himself in six months — can understand what it is, why it's built this way, and what to do
next, without guessing.

### Its two companion files
| File | Purpose |
|---|---|
| `PROJECT_ROADMAP.md` (this file) | **Where things stand and where they're going.** Living document — sections get rewritten as reality changes. |
| `CHANGELOG.md` | **What actually happened to the code, and why.** Append-only log, newest first. Never rewritten. |
| `RESEARCH_LOG.md` | **Facts gathered from outside the project** — market prices, competitor design analysis, UX research, image sourcing. Separate from the changelog because external facts go stale on their own schedule. |
| `IMAGE_CREDITS.md` | Attribution and licences for every shipped image. Legally required for the CC BY-SA one. |
| `CLAUDE.md` | **Loaded automatically at the start of every AI session.** Rules, commands, and the things that look like bugs but are deliberate. |
| `PROMPTS.md` | Ready-made prompts for future AI sessions, with the reasoning behind each. |
| `scripts/security-check.sh` | Re-runs every exploit from the security review against the running app. `bash scripts/security-check.sh` |
| `README.md` | Currently just the repo title. Low priority. |

### House rules for code
- **All code comments must be written in English.** Some older files still contain Estonian
  comments (`*.csproj`, `site.css`, several views) — convert them when you next touch those files.
- Comments should explain code so that a **beginner programmer** can follow the reasoning, not
  just restate what the line does.
- **Images: one format only — `.jpg`.** Applies to seeded catalogue images and uploads alike.

### If you are an AI model picking this up cold
1. Read this file top to bottom — it's designed to be your whole context.
2. Read `CHANGELOG.md` for recent history and the reasoning behind current code.
3. **After any code change**, add a `CHANGELOG.md` entry (template is in that file) and tick the
   relevant checkbox in §D2 below. This is not optional bookkeeping — it's the mechanism that
   keeps the next model from having to re-derive everything from scratch.
4. Don't trust this document blindly on details — verify a file still exists before recommending
   changes to it. Documents drift; code is the truth.

### If you are a beginner programmer
The project is an **ASP.NET Core MVC web application** in C#. Terms you'll meet below:
- **MVC** — Model (data), View (HTML pages), Controller (handles requests). ASP.NET's standard way
  of organising a website.
- **EF Core (Entity Framework Core)** — translates C# classes into database tables so you rarely
  write SQL by hand.
- **Migration** — a versioned, generated script that updates the database schema when you change
  a C# model class.
- **DTO (Data Transfer Object)** — a simple class for moving data between layers (e.g. from a web
  form into a service) without exposing the database entities directly.
- **DI (Dependency Injection)** — instead of a class creating its own dependencies, they're handed
  to it in its constructor. Configured in `Program.cs`. It's what makes the code testable.
- **xUnit** — the testing framework. Methods marked `[Fact]` are run automatically by
  `dotnet test`.

Start by reading `§B2` (domain model) and `§B3` (the calculator algorithm). Those two sections are
the heart of the project; everything else is scaffolding around them.

---

# PART A — Identity and purpose

## §A1 — Project identity

| | |
|---|---|
| **Thesis title** | "Elektrikilbi ja -tarvete komponentide kalkulaator" (Electrical panel and supplies component calculator) |
| **Type** | LÕPUTÖÖ — diploma thesis |
| **Author** | Edgar Muoni |
| **Institution** | Tallinna Tööstushariduskeskus (TTHK), Infotehnoloogia osakond |
| **Study group** | TARge24 |
| **Supervisor (juhendaja)** | Kalle Olumets |
| **Repo root** | `C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\Elektrikilbi-ja--tarvete-komponentide-kalkulaator\` |
| **Git** | branch `main`, 4 commits (Apr 26 – May 7 2026), solo author. Work from 2026-08-10 onward is **uncommitted** as of this revision. |

**Administrative record** (from the TTHK student portal, screenshot reviewed 2026-08-11 — recorded
because it's the only dated administrative trail available, and dates matter for thesis planning):

| Application (avaldus) | Submitted | Approved |
|---|---|---|
| Akadeemiline puhkus (academic leave) | 16.01.2024 | 05.02.2024 |
| Rakenduskava muutmine (curriculum plan change) | 09.07.2024 | 09.07.2024 |
| Välisõpilaseks vormistamine (registration as an exchange/foreign student) | 02.04.2026 | 10.04.2026 |

Two things worth noting from that record: an academic leave in 2024 explains the gap between
course enrolment (TARge24) and thesis work in 2026, and exchange-student status was approved on
10.04.2026 — about two weeks before the implementation commits began. **Neither the portal nor any
document read so far states a thesis defence date.** See §D4.

## §A2 — What it does (and what it will do)

**The official pitch**, from the submitted proposal form `Lõputöö kavand_VORM.docx`, translated:

> "Build a calculator that helps clients assemble electrical supplies and panel components for
> apartment buildings and commercial buildings. The calculator would show different components,
> their prices, and help clients make suitable choices."
>
> Goal: "A web-based application whose core component is the calculator algorithm."
>
> Value: "…the automatic calculator uses Estonia's most common electrical supply components and
> assembles a panel and electrical system according to the client's construction site parameters.
> It produces a list with an estimated cost, removing the need to use a cost-estimator's
> (*eelarvestaja*) service."

**In plain terms:** the user says what kind of building they have and how many rooms, sockets and
lights it needs. The app returns a **priced shopping list** (Bill of Materials) of breakers, cable,
an RCD and an enclosure — calculated from the Estonian electrical standard **EVS-HD 60364**, using
real products with real prices from its own catalogue.

**Scope note:** the proposal names only *korterelamu* (apartment building) and *ärihoone*
(commercial building). The built app also supports *eramu* (private house). That's a small,
defensible expansion — keep it, and mention it in one sentence rather than being caught out by it.

**Future direction (post-thesis, not an academic commitment):** grow into a real client-facing
shop where the calculator's BOM becomes an actual order, and the site acts as a middleman relaying
orders to suppliers rather than holding stock (dropshipping). The calculator stays the core
feature. See §D3.

---

# PART B — The system as it is today

## §B1 — Architecture and file map

Four projects, each depending only on the ones "below" it. This is the same layered structure as
Edgar's earlier coursework (`ShopTARge24`), deliberately.

```
Core  ←  Data  ←  ApplicationServices  ←  Web
 ↑                                        ↑
 └────────── Tests references these ──────┘
```

```
Elektrikilbi-ja--tarvete-komponentide-kalkulaator/     ← git root
├── PROJECT_ROADMAP.md                                  ← this file
├── CHANGELOG.md                                        ← what changed and why
└── ElektriKalkulaator/
    ├── ElektriKalkulaator.slnx                          ← solution file (lists all 5 projects)
    ├── ElektriKalkulaator.Core/                         ← no dependencies on anything
    │   ├── Domain/       Product, ProductCategory, CalculationRule,
    │   │                 PowerboxCalculation, PowerboxRequirements, PowerboxComponents
    │   ├── Dto/          CalculatorInputDto, BOMItemDto, ProductDto
    │   └── ServiceInterface/  ICalculatorServices, IProductServices, ICategoryServices
    ├── ElektriKalkulaator.Data/                         ← depends on Core
    │   ├── ElektriKalkulaatorContext.cs                  (DbContext + all seed data)
    │   └── Migrations/                                   (InitialCreate, AddEvsReference…)
    ├── ElektriKalkulaator.ApplicationServices/          ← depends on Core + Data
    │   └── Services/     CalculatorServices, ProductServices, CategoryServices
    ├── ElektriKalkulaator.Tests/                        ← depends on Core + Data + AppServices
    │   ├── TestBase.cs, CalculatorServicesTests.cs, ProductServicesTests.cs
    └── ElektriKalkulaator/                              ← the website; depends on all three
        ├── Program.cs                                    (DI wiring, middleware, auto-migrate)
        ├── Controllers/  Calculator, Cart, Products, Home
        ├── Views/        Calculator/, Cart/, Products/, Home/, Shared/
        └── wwwroot/      css/site.css (custom dark theme), images/products/ (uploads), lib/
```

**Stack:** ASP.NET Core 9 MVC · EF Core 9 · SQL Server · Razor views · Bootstrap 5 with a custom
dark-navy/amber theme · session-based cart · **no authentication**.

## §B2 — Domain model

All entities use `Guid` primary keys and `CreatedAt`/`ModifiedAt` timestamps set by the service layer.

- **`ProductCategory`** — `Name`, `Description`. Seeded (Estonian): Kaitselülitid (breakers),
  Juhtmed (cables), RCD / Rikkevoolukaitsmeid, Klemmid (terminals), Kilbi korpused (enclosures).
- **`Product`** — `CategoryId`, `Name`, `Brand`, `RatedCurrent` (used to match 10A/16A/32A
  breakers), `Voltage`, `Price`, `StockQuantity`, `ImagePath`, `WireCrossSectionMm2` (cables only:
  1.5 / 2.5 / 6.0 mm²), `Description`. 10 seeded ABB/Schneider/Draka products.
- **`CalculationRule`** — the standard, encoded as data: `BuildingType`
  (`korterelamu`/`eramu`/`ärihoone`), `CircuitType` (`lighting`/`socket`/`stove`),
  `WireCrossSectionMm2`, `BreakerAmperes`, `RoomsFrom`/`RoomsTo`, `EvsReference` *(added
  2026-08-10, currently unpopulated — see §D2)*. 8 seeded rules.
- **`PowerboxCalculation`** — one row per "Calculate" click. `UserId` (nullable, **unused** — no
  user table exists), `Status`, `RulesApplied`, `TotalCost`.
- **`PowerboxRequirements`** — 1:1 with a calculation; stores exactly what the user typed.
- **`PowerboxComponents`** — 1:N with a calculation; one row per BOM line
  (`ProductId`, `Quantity`, `UnitPrice`, `TotalPrice`, `CircuitType`, `WireCrossSectionMm2`).

**Important design note:** `CalculationRule` has **no foreign key** to anything. C# code joins it
at runtime by matching `BuildingType` strings. This is deliberate and is stated explicitly in
Edgar's own ERD document: rules are a general lookup table, not data belonging to one calculation.

### Documented design vs. built code — known gaps
Edgar's written spec (`ERD_Loogiline_Seletus.docx`) defines a 7-table schema. The built code
differs in these ways. A reviewer who read that appendix may ask about them:

| Spec says | Code actually has | Verdict |
|---|---|---|
| `USER` table (id, language, location) | No user entity at all; `PowerboxCalculation.UserId` is an orphan nullable field | Not built. Guest-only behaviour matches intent, but the table doesn't exist. |
| `name_et` / `name_en` / `name_rus` on products & categories; `USER.language` | Single Estonian `Name` field | **Not built — and the spec calls multilinguality a *main design principle*, not an extra.** Explain as a time constraint; don't dress it up as deliberate scoping. |
| `CALCULATION_RULES.room_type` (üldine/köök/vannituba/esik) | No room-type granularity | Simplified during implementation. Worth one honest sentence in the thesis. |
| `CALCULATION_RULES.evs_reference` | ✅ Added 2026-08-10 — but values still empty | Needs real clause numbers from the standard. |
| `POWERBOX_COMPONENTS.calculation_source` | Not present | Only matters once CAD import exists. Low priority. |
| `PRODUCT.in_stock` (boolean) | `StockQuantity` (integer) | **Code is better than the spec here.** Positive deviation. |
| "Only two required inputs: building type + room count" | Form also requires socket count, light count, stove checkbox | Honest answer: explicit counts proved more accurate than deriving them from room count. |

## §B3 — The core algorithm

`ElektriKalkulaator.ApplicationServices/Services/CalculatorServices.cs` → `Calculate()`

1. Load all `CalculationRule` rows matching the chosen `BuildingType`. **If none match, return an
   empty list immediately.**
2. Work out how many circuits are needed:
   - lighting: `ceiling(LightCount / 8)` — one circuit per 8 lights
   - sockets: `ceiling(SocketCount / 6)` — one circuit per 6 sockets
   - stove: `1` if the box is ticked, otherwise `0`
3. Estimate cable: `RoomCount × 8` metres per circuit.
4. For each matching rule, pick the **cheapest in-stock** product that matches — a breaker whose
   `RatedCurrent` equals the rule's amperage, and a cable whose cross-section matches.
5. Always add one enclosure and one RCD (fault protection is mandatory under the standard).
6. `SaveCalculation()` writes three tables: the calculation header, the user's inputs, and one row
   per BOM line.
7. `GetHistory()` returns the 50 most recent calculations for `/Calculator/History`.

**Why this matters:** this is pure, deterministic arithmetic over data. Same inputs always give the
same outputs, it's auditable, and it's testable — which is exactly the argument for choosing it
over the AI-based approach in the original prototype. See §C3.

## §B4 — What works right now

✅ **Working:** product & category CRUD · the calculator (form → BOM → saved history) ·
session-based cart (add/remove/clear) · product image upload and display · automatic database
migration on startup · custom dark theme · **16 automated tests, all passing**

⚠️ **Cosmetic only:** `CartController.Checkout()` clears the session and shows a confirmation
page. **It saves nothing to the database.** There is no order.

❌ **Absent:** authentication · i18n · orders/payment · deployment · everything in §D1 marked
"Not started"

🐞 **Known rough edges:** image-upload validation failures throw an exception (raw 500 page)
instead of showing a friendly form error · one EF Core nullability warning (CS8620) in
`CalculatorServices.GetHistory()` · connection string is hard-coded to a specific machine name
(`DESKTOP-KMVSVQK`) in `appsettings.json`

---

# PART C — How it got here

## §C1 — Timeline (reconstructed from file timestamps and git history)

| Date | Event |
|---|---|
| 2026-01-29 | React/TypeScript/Vite UI prototype (`elecpro-components-&-calculator`). Calculator powered by a live **Google Gemini** call. No pricing. Dark slate/yellow theme, 3-language UI. |
| 2026-02-02 | Two ERD drafts — still AI-service-centred, and **US-centric** (NEC code, 120V/240V, AWG wire gauges). |
| 2026-02-03 | `powerbox-no-ai-visual-explanation.html` — a written AI-vs-rules comparison concluding **rule-based is correct**, citing predictability, zero running cost, offline capability and code compliance. |
| 2026-03-08/09 | Final **Estonia-localised** ERD set + `ERD_Loogiline_Seletus.docx`. NEC → EVS-HD 60364, US building types → korterelamu/eramu/ärihoone. |
| 2026-04-10 | Exchange-student status approved (see §A1). |
| 2026-04-24 | Thesis proposal form finalised. |
| 2026-04-26 → 05-07 | Implementation. 4 commits; one ~4,300-line commit contains most of the app. |
| 2026-08-10 | Nullable/error-handling bugfix · `EvsReference` field · product images + upload hardening · **test project added**. All logged in `CHANGELOG.md`. |

**The story this tells** — and it's a genuinely good one for a thesis methodology chapter — is
five weeks of design that included **two real pivots** (AI → deterministic rules, then US standard
→ Estonian standard), followed by a concentrated two-week build.

## §C2 — Reference projects (Edgar's own earlier coursework)

All under `C:\Users\Jazztime\Desktop\TARge24\`. These are on Edgar's machine, not in this repo.

| Project | Path | What it's good for |
|---|---|---|
| **ShopTARge24** | `Veebiarendus/ShopTARge24/` | The architectural template this project copies. Matching it = safe; deviating = needs a reason. |
| ShopTARge24_opetaja | `Veebiarendus/ShopTARge24_opetaja/` | Teacher's reference version of the same. |
| **Car_TARge24** | `Veebiarendus/Car_TARge24/` | Source of the `TestBase` pattern now used in `ElektriKalkulaator.Tests`. |
| **ReactCRUD** | `Veebiarendus/ReactCRUD/` | Working React+TS frontend over a C# backend — the exact recipe for §D3 Phase 2. Single solution, `.esproj` client, `SpaProxy` package, Vite dev-proxy, **no CORS code needed**. |
| **AdvancedAjax** | `Veebiarendus/AdvancedAjax/` | jQuery `$.getJSON` cascading dropdowns; live image preview via `createObjectURL`. Source of the simple `wwwroot` upload pattern. |
| JustShop2 | `Agiilsed tarkvaraarenduse metoodikad/JustShop/` | Two image-upload strategies (disk path vs DB blob). |
| SeleniumShopUITest… | `Tarkvarasüsteemide_Testimine/SeleniumShopUITestSampleTARge24-main/` | Template if browser-level end-to-end tests are ever wanted. |
| Bitcoin-kalkulaator | `Bitcoin-kalkulaator/` | Checked; not useful. WinForms, dead API, unfinished validation. |

**The design draft** — `LÕPUTÖÖ/Elektrikilbi ja -tarvete kalk draft/elecpro-components-&-calculator/`
— is a visual reference only. Its calculator calls Gemini and its output type has **no price
field at all**. The real app's dark-navy/amber theme is already a deliberate adaptation of its
slate/yellow palette.

## §C3 — Decision ledger

When someone asks "why is it like this?", the answer is here. **Add a row whenever you make a
non-obvious decision.**

| Decision | Reasoning |
|---|---|
| **Rule table + arithmetic, not an LLM call** | Written out in full on 2026-02-03: auditable, reproducible, free to run, works offline, and guarantees standard compliance. An LLM can hallucinate an amperage; for electrical work that's unacceptable. **This is the project's strongest defence point — lead with it.** |
| Two DTO layers, not three like ShopTARge24 | ShopTARge24's third (ViewModel) layer served multi-entity forms with uploads. With one CRUD entity here, it would be pure boilerplate. |
| `CalculationRule` joined by string, no FK | Stated in the ERD spec: rules are a general lookup table, not rows owned by one calculation. |
| Estonian-only, no i18n | **Honest framing:** the spec calls multilinguality a main design principle; it wasn't built in the compressed build phase. Also genuinely harder than it looks — `BuildingType` and `Category.Name` are *business-logic join keys*, not just labels, so i18n needs stable codes + a translation table, i.e. a schema change. |
| Separate `Create`/`Edit` views, not one combined | Valid alternative convention; the two forms genuinely differ (Edit carries hidden `Id` and `ImagePath`). |
| Session-only cart, cosmetic checkout | Matches thesis scope: demonstrate the calculator and catalogue, not a payment flow. |
| Not-found → return `null`, not throw | Matches ShopTARge24, and the controllers already assumed it. Fixed 2026-08-10. |
| Uploads into `wwwroot/images/products/` | `wwwroot` is already served by default — no extra middleware needed, unlike storing outside it. |
| `EvsReference` left empty | Inventing standard clause numbers in a thesis about standards compliance would be worse than leaving them blank. Needs Edgar's verification against the real text. |

---

# PART D — Where it's going

## §D1 — Full-stack maturity map

This is the honest answer to "how far is this from a real production system?", mapped against the
thirteen layers of a production web stack.

**Read this the right way.** It is an *orientation map*, not a to-do list. A diploma thesis is
graded on whether it solves its stated problem correctly and is well-engineered — **not** on
whether it has load balancing. Most rows below are correctly absent. The map's value is that you
can now *name* what's missing and say deliberately "out of scope for v1" instead of being caught
unaware. Being able to explain why a layer is absent is a stronger position than having built it.

| # | Layer | Status | Where it stands |
|---|---|---|---|
| 1 | **Frontend** | 🟢 Good | Razor + Bootstrap 5, custom dark theme, responsive card grid. Server-rendered, no SPA. |
| 2 | **APIs & Backend Logic** | 🟡 Half | Backend logic is the project's strength (clean layering, tested service). But there's **no API surface** — controllers return HTML, not JSON. Blocks a React frontend and any mobile client. → Phase 2. |
| 3 | **Database & Storage** | 🟢 Good | EF Core 9 + SQL Server, migrations, seeded reference data, decimal precision configured. File storage = local disk. |
| 4 | **Auth & Permissions** | 🟢 Good | **Closed 2026-08-11.** ASP.NET Core Identity with `Admin` and `Customer` roles; product/category management requires an admin, catalogue and calculator stay public. Passwords hashed, lockout after 5 failed attempts, admin seeded from User Secrets. Remaining: `PowerboxCalculation.UserId` is still not populated on save. |
| 5 | **Hosting & Deployment** | 🔴 None | Runs on `localhost` only. Connection string hard-codes one machine name. Never deployed anywhere. |
| 6 | **Cloud & Compute** | 🔴 None | No cloud resources. Not needed yet. |
| 7 | **CI/CD & Version Control** | 🟡 Half | Git yes — but 4 commits, one branch, and ~a full session of work currently uncommitted. No CI pipeline, though `dotnet test` now makes one genuinely worthwhile, and ShopTARge24 has a `.github/workflows` folder to copy from. |
| 8 | **Security** | 🟡 Half | Present: antiforgery tokens on POSTs, EF Core parameterised queries (no SQL injection), HTTPS redirection, upload allow-list + size cap + GUID filenames. Missing: auth (row 4), content-type verification on uploads, secret management. |
| 9 | **Rate Limiting** | 🔴 None | .NET 9 has `AddRateLimiter` built in — a few lines whenever it's actually needed. |
| 10 | **Caching & CDN** | 🔴 None | Note: `AddDistributedMemoryCache()` in `Program.cs` looks like caching but only backs session state. |
| 11 | **Load Balancing & Scaling** | 🔴 None | Worth knowing: the cart lives in **in-memory** session state, so running two instances would randomly lose carts. A real blocker for the dropshipping vision, not for the thesis. |
| 12 | **Error Tracking & Logs** | 🟡 Half | Default `ILogger`, migration failures logged, `UseExceptionHandler` in production. No structured logging, no aggregation. |
| 13 | **Availability & Recovery** | 🔴 None | No backups, health checks or recovery plan. |

**For the defence:** rows 1–3 are solid, row 4 is the one you should proactively acknowledge, and
rows 5–6 and 9–13 are legitimately beyond a diploma project's scope. Say so plainly.

**For the dropshipping future:** rows 4, 5, 8 and 11 become mandatory the moment real customers and
real money are involved.

## §D2 — Phase 1: before the defence

- [x] **Automated test project** — `ElektriKalkulaator.Tests`, 16 tests passing, mutation-verified. *(2026-08-10)*
- [x] **Fix nullable-annotation lie + inconsistent not-found handling** *(2026-08-10)*
- [x] **Product images** — upload, storage, display, plus security hardening *(2026-08-10)*
- [x] **Add `EvsReference` column** to `CalculationRule` *(2026-08-10)*
- [x] **Real product photography** for all ten seeded products, licensed and attributed *(2026-08-11)*
- [ ] **Decide and display the VAT basis of prices** — `Product.Price` doesn't say whether it
      includes VAT, a >20 % ambiguity in a tool whose purpose is cost estimation. Estonian
      retailers always state it. See `RESEARCH_LOG.md` *(2026-08-11)*
- [ ] Consider re-basing seeded prices on observed market rates — the seeded 9.20 € for an
      ABB S201-B16 is above the ~5.78 € a real customer pays, so the calculator over-estimates
- [ ] Label catalogue photos as illustrative ("pilt on illustratiivne"), as Estonian shops do
- [ ] **Populate `EvsReference`** with real EVS-HD 60364 clause numbers — *needs Edgar; must not be guessed*
- [ ] **Commit the current work** — a session's worth of changes is uncommitted (see §A1)
- [x] **Turn image-upload validation errors into form messages instead of 500 pages** *(2026-08-11)*
- [ ] Write the thesis's own "limitations / future work" section using §B2's gap table, §D1 and §D3
- [ ] **Redesign the site to convert visitors into buyers — both consumers and companies.**
      Edgar's priority (stated 2026-08-11): the site must *psychologically attract* customers, and
      companies as B2B buyers. This is a distinct piece of work from the security and correctness
      fixes done so far, and it is the main thing standing between "a working thesis project" and
      "something a real customer would buy from".

      Research already gathered in `RESEARCH_LOG.md` (competitor teardowns of Esvika, Onninen and
      Elektrikaubad.ee, plus Baymard/Stanford/Lindgaard findings). Highest impact ÷ effort first:
      - [ ] State whether prices include VAT — a cost-estimation tool with a >20 % ambiguity
      - [ ] Trust strip under the calculator result, saying what is actually true and unusually
            strong here: calculated to EVS-HD 60364, live catalogue prices, every rule auditable.
            The project's real differentiator is currently invisible in the interface.
      - [ ] Sorting (price, name) and price-per-metre on cable — Baymard essentials
      - [ ] Homepage hero built around the calculator, using the licensed photography already in
            `wwwroot/images/hero/` (downloaded, still unused)
      - [ ] **B2B specifically** — this is what makes companies buy: saved/shareable BOM lists
            (an electrician sends a calculation to a client for approval), a printable/PDF quote,
            product codes and datasheets on the product page, and eventually account pricing.
            Onninen's whole interface is built around these; see `RESEARCH_LOG.md`.
      - [ ] Never: fake scarcity, countdown timers, invented "was" prices. Regulated as unfair
            commercial practices in the EU, and they would undermine the trustworthiness argument
            the entire project rests on.
- [ ] *Optional:* more test coverage (`CategoryServices`, controllers)
- [ ] *Optional:* AJAX-ify the calculator submit (pattern in `AdvancedAjax`, jQuery already loaded)
- [ ] *Optional:* broaden `/Products` filtering (price range, brand)

## §D3 — Later phases (post-defence)

**Phase 2 — API + React frontend**
- [ ] Add `[ApiController]` JSON endpoints alongside the MVC controllers (Core/Data/Services untouched)
- [ ] Clone ReactCRUD's SPA wiring: `.esproj` client project, `SpaProxy`, Vite dev-proxy, same-origin in production
- [ ] Rebuild the `elecpro-components-&-calculator` draft against the real API instead of Gemini
- [ ] **Do i18n here** — while the API contract is being designed anyway. Replace Estonian string join-keys with stable codes plus a translation table; add the multilingual columns the ERD spec already defines.

**Phase 3 — Edgar's own documented future work** (from `ERD_Loogiline_Seletus.docx` §7)
- [ ] **CAD import** — parse uploaded DXF/DWG floor plans to count outlets/lights/switches automatically. **This is the one place Python genuinely fits** (see §D4), likely as a small separate parsing service the C# backend calls.
- [ ] **User accounts** — build the `USER` table the spec defines; connect it to the already-present `PowerboxCalculation.UserId`. Also closes §D1 row 4.
- [ ] **Live supplier price feeds** — `Supplier` table + `Product.SupplierId`; real-time prices from ABB/Schneider/Hager wholesalers.
- [ ] **PDF export** — BOM as a PDF with quantities, prices and EVS-HD 60364 references; usable for building-permit applications.

**Phase 4 — Dropshipping/marketplace** *(Edgar's stated ambition; not an academic commitment)*

The model, as specified by Edgar on 2026-08-11: list products sourced from other retailers' sites;
when a customer buys here, the system **automatically places the order with the original seller**,
and the customer is charged the **source price + 25 % markup + shipping**. The calculator remains
the reason customers arrive; the shop is how the site earns.

- [ ] `Order`/`OrderLine` entities + real checkout persistence (replaces the cosmetic `Checkout()`)
- [ ] `Supplier` entity — merge with Phase 3's supplier work; build it once, not twice
- [ ] **Pricing engine** — store the supplier's cost separately from the customer-facing price,
      with the markup as *configurable data, not a hard-coded 1.25*. Margins change; shipping is
      not always a flat add-on; and some suppliers forbid resale above a set price.
- [ ] **Automated order relay** to the source retailer. Be aware this is the hardest and riskiest
      part: most retailers have **no public ordering API**, their terms of service often prohibit
      automated purchasing or resale, and scraping a checkout flow is brittle and may be a
      contract breach. Realistic sequencing: start with a **manual/assisted** relay (an email or
      dashboard task per order), then negotiate a proper reseller/affiliate agreement with one or
      two suppliers, and only then automate against a real API.
- [ ] Payment processing (Stripe.net is the standard .NET choice)
- [ ] **Legal/consumer-protection groundwork before taking real money** — as the seller of record
      in the EU, this site would owe the customer a 14-day withdrawal right, a 2-year conformity
      guarantee, and clear delivery-time disclosure, regardless of what the upstream retailer
      offers. Stock and price sync failures become *your* liability, not the supplier's.
- [ ] Revisit §D1 rows 4, 5, 8, 11 — they stop being optional here

**Stack recommendation:** keep **C#/.NET** for the backend; add **TypeScript + React** for the
frontend after the defence. This isn't a new bet — it finishes the prototype Edgar already built.

## §D4 — Open questions

These need Edgar's answer. Don't guess at them.

1. **When is the defence?** No document or portal screenshot read so far states one. The proposal
   form contains two conflicting dates: a printed deadline of 9 February 2026, and Edgar's own
   entry of 31.05.2026 for "Lõputöö plaani esitamise kuupäev". Neither is obviously a defence date.
2. **Does exchange-student status (approved 10.04.2026) affect the thesis timeline** — supervision,
   deadlines, or where the defence takes place?
3. **Python's role.** The proposal lists MSQL, C#, .NET, HTML, JavaScript, **Python** and
   TypeScript. Everything but Python is either used or planned. Best inference is the CAD-import
   feature (§D3 Phase 3) — geometry/DXF parsing is a genuine Python strength. Confirm, since a
   listed technology that never appears may attract a question.
4. **Framing for the eramu scope expansion** and **the "two required inputs" principle vs. the
   five-field form** — both need a chosen sentence before the defence, not new code.

---

# PART E — Working agreements

## §E1 — After every code change

1. **Add a `CHANGELOG.md` entry** using the template at the top of that file. The *why* matters
   more than the *what*.
2. **Tick the checkbox** in §D2 or §D3 if the change completes a planned item.
3. **Add a row to §C3** if you made a decision whose reasoning isn't obvious from the code.
4. **Update §B4** if the "what works / what's broken" picture changed.
5. **Fix any section that has drifted** out of line with reality — don't leave a stale claim
   standing next to a new one.
6. Add a line to §E2 below.

## §E2 — Revision history of this document

- **rev. 1** (2026-08-10) — Created. Built from a full read of the C# solution, a comparison
  against ShopTARge24, the TypeScript design draft, and the `.mermaid`/`.html` ERD planning
  documents. `Lõputöö kavand_VORM.docx` and `ERD_Loogiline_Seletus.docx` flagged as unread.
- **rev. 2** (2026-08-10) — Read both `.docx` files in full. Added the official pitch, the
  documented-vs-built gap table, the tech-stack reconciliation, and findings from five sibling
  coursework projects. **Corrected the i18n framing** from "smart scoping" to "documented
  principle not yet built".
- **rev. 3** (2026-08-11) — Restructured into Parts A–E for readability by newcomers. Added §0
  orientation and beginner glossary; added §D1 full-stack maturity map (thirteen layers, honestly
  scored); recorded TTHK portal administrative dates in §A1; **introduced `CHANGELOG.md`** and the
  §E1 update protocol; ticked the four Phase 1 items completed on 2026-08-10.
- **rev. 4** (2026-08-11) — Added `RESEARCH_LOG.md` and `IMAGE_CREDITS.md` to §0 and house rules
  (English-only comments, single `.jpg` image format). Recorded the real-product-photography work
  and three new price/VAT follow-ups in §D2. Expanded Phase 4 with the **25 % markup dropshipping
  specification**, plus the supplier-API, pricing-engine and EU consumer-law realities it implies.
