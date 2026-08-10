# ElektriKalkulaator — Project Roadmap & AI Context Document

> **Purpose of this file.** This is a self-contained briefing for any AI model (or human) picking
> up this project without prior conversation history. It covers what the project is, how it got
> here, what's built, what's missing, what's deliberately deferred (and why), and what to do next.
> It is a **living document** — append to it, don't just read it. See "How to keep this document
> updated" at the end before you finish a work session on this project.
>
> Last updated: **2026-08-10 (rev. 2)**, by Claude (Sonnet 5), in collaboration with Edgar. Rev. 2
> incorporates the official thesis proposal form and the author's own written ERD specification
> (both previously unread `.docx` files), plus a targeted scan of five other coursework projects
> for reusable patterns.

---

## 1. Project identity

| | |
|---|---|
| **Project name** | ElektriKalkulaator (Estonian repo name: `Elektrikilbi-ja--tarvete-komponentide-kalkulaator`) |
| **Official thesis title** | "Elektrikilbi ja -tarvete komponentide kalkulaator" — confirmed verbatim in the signed-off proposal form |
| **Author** | Edgar Muoni |
| **Type** | LÕPUTÖÖ — diploma/graduation thesis project |
| **Institution** | Tallinna Tööstushariduskeskus, Infotehnoloogia osakond (IT department) — confirmed independently in both the ERD spec doc and the proposal form |
| **Supervisor (juhendaja)** | Kalle Olumets — same, confirmed in both documents |
| **Study group (õpperühm)** | TARge24 |
| **Thesis plan submission** | The proposal form has two dates that don't agree: the form's own printed instruction says thesis plans ("lõputöö kava") are due **9. veebruar 2026**, but the field Edgar filled in under "Lõputöö plaani esitamise kuupäev" says **31.05.2026**. Not resolved — see §13 Open Questions. |
| **Defense date** | Not stated in any document read so far. Unknown — see §13. |
| **Repo root (this file's location)** | `C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\Elektrikilbi-ja--tarvete-komponentide-kalkulaator\` |
| **Git status as of writing** | Single branch `main`, 4 commits, solo author (Edgar), Apr 26 – May 7 2026 |

**Important scope note for any AI reading this:** the repo map in §3 below includes paths *outside*
this git repository, on Edgar's local machine (a much larger personal school-work repo at
`C:\Users\Jazztime\Desktop\TARge24\`). If you are only given a clone of *this* repo, those paths
won't exist for you — treat that material as background you're being told about, not files you can
open. If you do have access to the full `TARge24` tree, it's genuinely useful reference material
(see §7–§9).

---

## 2. Elevator pitch & vision

**The official pitch**, quoted from the submitted proposal form (translated from Estonian):

> "Build a calculator that helps clients assemble electrical supplies and panel components for
> apartment buildings [korterelamud] and commercial buildings [ärihooned]. The calculator would
> show different components, their prices, and help clients make suitable choices."
>
> Goal: "A web-based application whose core component is the calculator algorithm."
>
> Practical value: "The innovation is that the automatic calculator uses Estonia's most common
> electrical supply components, and assembles a panel and electrical system according to the
> client's construction site's parameters. It produces a list with an estimated cost, removing the
> need to use a cost-estimator's [eelarvestaja] service."

**Scope note**: the official proposal names only *korterelamu* (apartment building) and *ärihoone*
(commercial building) as target building types. The actual seeded `CalculationRule` data and the
`eramu` (private house) option in the UI go beyond that — a reasonable, low-risk extension, but
worth having a one-sentence answer ready ("I extended coverage to private houses since the same
EVS-HD 60364 rules apply, it cost nothing extra to include") rather than being caught off guard.

**Now (thesis deliverable):** a working ASP.NET Core web app where the user enters a building type
+ room/socket/light counts, and gets back a priced Bill of Materials (BOM) — computed
deterministically from **EVS-HD 60364** (the Estonian electrical installation standard), matched
against a real seeded product catalogue with real prices and stock levels.

**Officially documented future work** (from the author's own ERD specification, §7 of that
document — see §12 below for the full list): CAD floor-plan import, user accounts, live supplier
price feeds, PDF export.

**Additionally discussed with Claude, not yet in any written thesis document (2026-07-27):**
grow this into a real client-facing e-commerce/dropshipping site post-thesis, where the calculator's
BOM becomes an actual customer order relayed to suppliers rather than fulfilled from owned stock.
This is a broader commercial vision than the officially documented future work above — the two
overlap (both want a `Supplier`-integration layer) but aren't identical, and only the
CAD/accounts/pricing/PDF list has been formally written down by Edgar himself. Treat the
dropshipping vision as Edgar's stated intent, not yet a committed academic scope item.

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
in ASP.NET session, 30-min idle timeout), no authentication anywhere. There is currently **no test
project** in the solution.

### 3.2 Planning / reference material (outside this repo, same machine)

All under `C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\Elektrikilbi ja -tarvete kalk draft\` unless
noted. All of these have now been read (as of rev. 2 of this document).

| File/folder | What it is | Key takeaway |
|---|---|---|
| `elecpro-components-&-calculator/` (React+TS+Vite) | Earliest artifact (2026-01-29): AI-estimator-based UI/UX prototype (dark slate/yellow theme, `en`/`et`/`ru` i18n, calls Gemini for the BOM, no pricing). | Visual/UX north star, calculation approach explicitly rejected. See §8. |
| `whole-building-calculator-erd.mermaid`, `powerbox-calculator-erd.mermaid`, `Whole_Building_Electrical_ERD.png` | ERD explorations (2026-02-02), still AI-service-oriented and US-centric (NEC, AWG, 120V/240V). | Superseded by the Estonia-localized ERD below. |
| `powerbox-no-ai-visual-explanation.html` | Written AI-vs-rule-based pivot decision (2026-02-03). | Decision record for the project's most defensible architecture choice. |
| `Elektrikilbi ja -tarvete komponentide kalkulaator .drawio/.png/.svg`, `elektrikomponentide-kalkulaator-erd.html`, `tabelite-seosed-seletus.html` | Final Estonia-localized ERD diagrams/explanations (2026-03-08/09). | Matches `ERD_Loogiline_Seletus.docx` below almost exactly; the `.docx` is the authoritative prose version, cite that one. |
| **`ERD_Loogiline_Seletus.docx`** | The author's own formal, written database design specification — meant as an appendix to the thesis's theoretical section. **Read in full for rev. 2 — see §4.1, §9, and §12.** | This is the single most authoritative planning document for the data model. Where the real C# code disagrees with it, that's a documented gap, not a guess. |
| **`Lõputöö kavand_VORM.docx`** | The official submitted thesis proposal form. **Read in full for rev. 2.** | Source of §1's identity facts, §2's official pitch, and the tech-stack reconciliation in §6. |

### 3.3 Reference codebases from Edgar's broader coursework (same machine, `C:\Users\Jazztime\Desktop\TARge24\`)

| Project | Path | Relevance |
|---|---|---|
| **ShopTARge24** (Edgar's own prior coursework) | `Veebiarendus/ShopTARge24/ShopTARge24/` | The architectural template this thesis's 4-project layering was explicitly modeled on. Full comparison in §7. |
| **ShopTARge24_opetaja** | `Veebiarendus/ShopTARge24_opetaja/ShopTARge24/` | Teacher's reference solution for the same project. |
| **ReactCRUD** | `Veebiarendus/ReactCRUD/` | Working React+TypeScript-over-C# solution (VS "ASP.NET Core with React" template). Directly answers "how do I wire the future frontend to this backend." See §9.1. |
| **AdvancedAjax** | `Veebiarendus/AdvancedAjax/` | jQuery/AJAX cascading-dropdown and live-image-preview patterns. See §9.3. |
| **Car_TARge24** | `Veebiarendus/Car_TARge24/` | Ships a ready-to-clone xUnit + EF-InMemory test project. See §9.4. |
| **JustShop2** | `Agiilsed tarkvaraarenduse metoodikad/JustShop/JustShop2/` | Same layered architecture; has two working image-upload/render implementations. See §9.5. |
| **Bitcoin-kalkulaator** | `Bitcoin-kalkulaator/` | Edgar's earlier, simpler calculator (WinForms). Checked, not useful beyond a "past mistake already fixed" contrast — see §9.2. |
| **SeleniumShopUITestSampleTARge24** | `Tarkvarasüsteemide_Testimine/SeleniumShopUITestSampleTARge24-main/` | UI-level Selenium testing template for ShopTARge24, applicable to Calculator/Cart flows. |

---

## 4. Domain model (full detail)

All entities in `ElektriKalkulaator.Core/Domain/`, `Guid` primary keys, `CreatedAt`/`ModifiedAt`
timestamps set by the service layer via `DateTime.Now`.

- **`ProductCategory`** — `Id`, `Name`, `Description`. 1→N `Products`. Seeded categories (Estonian):
  Kaitselülitid (breakers), Juhtmed (cables), RCD / Rikkevoolukaitsmeid, Klemmid, Kilbi korpused
  (enclosures).
- **`Product`** — `Id`, `CategoryId` (FK), `Name`, `Brand`, `RatedCurrent`, `Voltage`, `Price`
  (decimal 10,2), `StockQuantity` (int), `ImagePath` (nullable — **exists but is never rendered in
  any view**, see §4.1/§9.5), `WireCrossSectionMm2` (nullable, only for cables), `Description`.
- **`CalculationRule`** — `Id`, `RuleName`, `BuildingType` (string exact-match join key:
  `korterelamu`/`eramu`/`ärihoone`), `WireCrossSectionMm2`, `BreakerAmperes`, `CircuitType`
  (`lighting`/`socket`/`stove`), `RoomsFrom`/`RoomsTo`. 8 seeded rules encoding EVS-HD 60364.
- **`PowerboxCalculation`** — `Id`, `UserId` (nullable Guid, unused, no `User` table exists),
  `Status`, `RulesApplied`, `TotalCost`. 1↔1 `Requirements`, 1→N `Components`.
- **`PowerboxRequirements`** — `Id`, `CalculationId`, `BuildingType`, `RoomCount`, `SocketCount`,
  `LightCount`, `SwitchCount` (informational only), `HasElectricStove`, `FloorCount`/`TotalAreaM2`
  (nullable, currently unused by the calculation itself).
- **`PowerboxComponents`** — `Id`, `CalculationId`, `ProductId`, `Quantity`, `UnitPrice`,
  `TotalPrice`, `CircuitType`, `WireCrossSectionMm2`.

### DTOs (`ElektriKalkulaator.Core/Dto/`)
- `CalculatorInputDto` — validated form model for `/Calculator`.
- `BOMItemDto` — one BOM output row.
- `ProductDto` — form model for Create/Edit product pages.

### 4.1 Actual implementation vs. the officially documented ERD — known gaps

`ERD_Loogiline_Seletus.docx` specifies a 7-table schema (`USER`, `POWERBOX_CALCULATION`,
`POWERBOX_REQUIREMENTS`, `CALCULATION_RULES`, `POWERBOX_COMPONENTS`, `PRODUCT`,
`PRODUCT_CATEGORY`) that maps closely onto the real EF Core model above — but not perfectly. These
are genuine, citable differences between the *approved design* and the *built implementation*,
useful to know before a committee member who has read the same appendix asks about them:

| Documented in the ERD spec | Actually implemented | Note |
|---|---|---|
| `USER` table with `UserID` (PK), `language` (et/en/ru), `location`, `created_at` | No `User` entity exists at all. `PowerboxCalculation.UserId` is a bare nullable `Guid` with no backing table or FK. | Documented as supporting guest sessions either way — the "no login required" behavior matches intent, but the table itself was never built. |
| `PRODUCT.name_et`/`name_en`/`name_rus`, `PRODUCT_CATEGORY.name_et`/`name_en`/`name_rus`, `USER.language` | `Product.Name`, `ProductCategory.Name` are single strings, Estonian only. | **This is more than a deferred nice-to-have** — multilingual text fields are listed explicitly as one of the database's stated "peamised disainipõhimõtted" (main design principles) in §8 of the spec doc: *"Mitmekeelsus: kõik kasutajale nähtavad tekstiväljad on saadaval eesti, inglise ja vene keeles."* The real schema doesn't have the columns for it yet. Be ready to say this plainly (time constraints during the build phase) rather than reframe it as intentional scoping — the design doc itself treats it as core, not optional. |
| `CALCULATION_RULES.room_type` (`üldine`/`köök`/`vannituba`/`esik` — general/kitchen/bathroom/hallway) | `CalculationRule` has no room-type granularity — only `BuildingType` + `CircuitType`. | The real rule set is coarser than documented. Simplification made during implementation, not previously justified in writing — worth a one-line rationale in the thesis ("per-room-type rules were designed but simplified to per-circuit-type for the initial release; the schema's `RuleName`/`CircuitType` fields leave room to reintroduce room-level granularity later"). |
| `CALCULATION_RULES.evs_reference` (e.g. `"EVS-HD 60364-4-41"`) | No such field on `CalculationRule`. | **Cheap, high-value fix**: this is a single nullable string column plus seed-data values. It would let the BOM output cite the exact standard clause per component — directly strengthens the "auditable, standards-traceable" argument that's already the project's best defense talking point (§8). Worth doing even outside the formal roadmap phases below, it's nearly free. |
| `POWERBOX_COMPONENTS.calculation_source` (`"rule_based"`, future `"cad_import"`) | No such field on `PowerboxComponents`. | Low priority — only matters once CAD import (§12) is real. |
| `PRODUCT.in_stock` (BIT) | `Product.StockQuantity` (INT) | The real implementation is **better** than the documented design here — quantity instead of a boolean. No action needed, just note it as a positive deviation if asked. |
| Design principle: *"only two required inputs — building type and room count, everything else derived"* (ERD spec §8) | `CalculatorInputDto` requires `BuildingType`, `RoomCount`, **and** `SocketCount`, `LightCount` (both `[Range(0,500)]`, not optional), plus `HasElectricStove` | The built form asks for more explicit input than the approved design says it should. Defensible (more accurate than guessing sockets/lights per room), but if asked "your own design doc says two inputs, why does the form have five fields," the honest answer is that requiring the extra counts turned out to give better real-world accuracy than deriving them from room count alone — say that, don't pretend the doc said something else. |

---

## 5. Core algorithm — `CalculatorServices.Calculate()`

File: `ElektriKalkulaator.ApplicationServices/Services/CalculatorServices.cs`

1. Load all `CalculationRule` rows where `BuildingType == input.BuildingType`. Empty BOM if none
   match.
2. Compute circuit counts: `lightingCircuits = ⌈LightCount / 8⌉`, `socketCircuits = ⌈SocketCount /
   6⌉`, `stoveCircuits = HasElectricStove ? 1 : 0`.
3. Rough wire estimate: `wireLengthPerCircuit = RoomCount × 8` metres.
4. Per matching rule: cheapest in-stock breaker matching `RatedCurrent == rule.BreakerAmperes` in
   "Kaitselülitid" → one BOM line. Cheapest in-stock cable matching `WireCrossSectionMm2` in
   "Juhtmed" → one BOM line, quantity = `circuitCount × wireLengthPerCircuit` metres.
5. Always append one enclosure ("Kilbi korpused") and one RCD ("RCD / Rikkevoolukaitsmeid"),
   quantity 1 each.
6. `SaveCalculation()` persists across `PowerboxCalculation`/`PowerboxRequirements`/
   `PowerboxComponents`.
7. `GetHistory()` returns the last 50 calculations, eager-loaded, for `/Calculator/History`.

Pure, deterministic, side-effect-light logic (excluding DB reads) — see §11 (decision ledger) for
why this matters, and §9.4 for the cheapest path to actually testing it.

---

## 6. Design & planning history (chronological, dated from real file timestamps)

1. **2026-01-29** — React/TypeScript/Vite UI prototype, calculator powered by a live Gemini call,
   no pricing, "consult a licensed electrician" disclaimer.
2. **2026-02-02** — Two ERD explorations, still `AI_ESTIMATOR`/`AI_SERVICE`-centered and
   **US-centric** (NEC, 120V/240V/600V, AWG wire gauge, 100A/200A/400A service ratings).
3. **2026-02-03** — `powerbox-no-ai-visual-explanation.html`: explicit written AI-vs-rule-based
   comparison, concludes rule-based is correct, notes "can customize for Estonian regulations."
4. **2026-03-08/09** — Final, Estonia-localized ERD diagrams **and** `ERD_Loogiline_Seletus.docx`
   (the formal written spec — see §4.1): building types switch to Estonian terms, standard
   reference switches to `evs_reference` (EVS-HD 60364), 7-table schema with multilingual product
   fields and per-room-type rules specified.
5. **2026-04-24** — `Lõputöö kavand_VORM.docx` (official thesis proposal) touched/finalized.
6. **2026-04-26 to 2026-05-07** — Actual implementation, 4 git commits, culminating in a single
   ~4300-line commit that built out most of the app. Some design-doc details (i18n columns,
   room-level rules, `evs_reference`, the `User` table) didn't make it into this implementation
   pass — see §4.1.

**Tech stack reconciliation.** The official proposal's technology list is: *"MSQL, C#, .NET, HTML,
javascript, Python, Typescript."* Checked against what actually exists:

| Listed | Status |
|---|---|
| MSQL (MSSQL) | ✅ SQL Server, as built |
| C#, .NET | ✅ as built |
| HTML | ✅ Razor views |
| JavaScript | ✅ jQuery/Bootstrap JS already present (`site.js`, jquery-validation) |
| TypeScript | Used in the `elecpro-components-&-calculator` prototype only; not yet in the production app — matches this document's Phase 2 (React frontend) plan, see §12. |
| **Python** | **Not used anywhere yet**, prototype or production. Best-fit explanation: `ERD_Loogiline_Seletus.docx` §7 (future work) describes CAD floor-plan import (parsing DXF/DWG files to auto-detect outlet/light/switch positions) — a natural Python task (geometry/CAD file-format libraries are Python's strength, e.g. `ezdxf`), much more so than a general C# or TypeScript job. Treat Python as reserved for that one specific future feature, not a general-purpose addition to the current stack — this doesn't change the Phase 2 backend/frontend recommendation in §12. |

**Net effect**: ~5 weeks of design/planning (including a real pivot away from an AI API call, a
real US→Estonia localization pass, and a formally written 7-table spec), followed by a
concentrated ~2-week implementation that built the core system but left several documented details
(i18n columns, room-level rule granularity, the `User` table, `evs_reference`) for later.

---

## 7. Relationship to ShopTARge24 (condensed — see the standalone comparison report for detail)

A full file-by-file comparison report exists as a previously-published Claude artifact ("ElektriKalkulaator —
Thesis Comparison Report"); if unavailable, regenerate by diffing conventions against
`Veebiarendus/ShopTARge24/ShopTARge24/`.

**Matches**: 4-project layering, manual DI registration style, domain model shape, naming
conventions, async patterns.

**Deliberate, defensible deviations**: two DTO layers instead of three; separate
`Create.cshtml`/`Edit.cshtml` instead of combined `CreateUpdate.cshtml`; `ProductsController.Index()`
correctly uses the service layer where ShopTARge24's `RealEstateController.Index()` bypasses its
own.

**Real inconsistency** (fix in Phase 1, §12): `ProductServices.GetById`/`CategoryServices.GetById`
are typed non-nullable but can return `null` via a suppressed `!` warning; `Update`/`Delete` throw
raw `Exception` on not-found instead of following ShopTARge24's null-return convention.

**Missing entirely**: automated tests — see §9.4 for the concrete fix (a better template than
ShopTARge24's own test projects, found in Car_TARge24).

---

## 8. Relationship to the TypeScript/React design draft (condensed)

`elecpro-components-&-calculator` is a visual/UX reference only, calculator calls Gemini instead of
computing anything deterministically, `CalculatedComponent` has no price field. **Already
implemented in the real app**: `wwwroot/css/site.css` has a bespoke dark-navy/amber theme layered
over Bootstrap that closely mirrors the draft's palette — this was a deliberate, successful
adaptation. **Still missing**: product images, full i18n (now known to be a documented design
principle, not just a draft aspiration — see §4.1), broader filtering.

**The single most important finding**: the draft's AI-estimator has no price and an explicit
"consult a licensed electrician" disclaimer. The real app's value proposition is the opposite — a
priced, auditable, testable BOM. Lead talking point for the defense.

---

## 9. Ideas mined from other sibling coursework projects

Scope: read-only scan of five other projects under `C:\Users\Jazztime\Desktop\TARge24\`
(ShopTARge24 excluded, covered separately in §7). Goal was concrete, transferable patterns, not
another architecture audit.

### 9.1 ReactCRUD — the exact wiring recipe for Phase 2

`Veebiarendus/ReactCRUD/` is the Visual Studio "ASP.NET Core with React" template: a `.esproj`
client project referenced from the C# server project with `ReferenceOutputAssembly=false`, plus
the `Microsoft.AspNetCore.SpaProxy` package. In dev, Vite proxies `/api` calls straight to the
ASP.NET Core HTTPS port; in prod, `Program.cs` just does
`app.UseDefaultFiles(); app.MapStaticAssets(); app.MapFallbackToFile("/index.html");` and serves
the built SPA as static files from the same origin as the API. **Result: no CORS configuration
anywhere.** React side is plain — `fetch()`, `useState`/`useEffect`, react-router-dom, hand-typed
TS interfaces mirroring C# view models (PascalCase → camelCase via
`JsonSerializerOptions.PropertyNamingPolicy`), no AutoMapper, no shared-type codegen.

**Action**: when Phase 2 starts, clone this structure directly instead of researching SPA-hosting
options from scratch.

### 9.2 Bitcoin-kalkulaator — checked, not useful

Edgar's earlier calculator (WinForms, not web). Input validation is an empty-string check only; a
numeric-format check was attempted and abandoned (commented out); unguarded `float.Parse` throws
on bad input. No history/persistence. Nothing to adopt — noted only because ElektriKalkulaator's
own `CalculatorInputDto` (model binding + `[Required]`/`[Range]` + `ModelState.IsValid` +
persisted `History()`) is already more mature than this earlier attempt.

### 9.3 AdvancedAjax — cascading dropdowns and live previews, zero new dependencies

`Veebiarendus/AdvancedAjax/` demonstrates two jQuery-AJAX patterns (jQuery is already loaded in
`_Layout.cshtml` for unobtrusive validation, so these cost nothing new):

- **Cascading dropdown**: an `onchange` handler calls `$.getJSON('/Controller/Action', {...},
  callback)` against a `[HttpGet] JsonResult` action that returns a filtered `SelectListItem` list;
  the callback repopulates a second `<select>`.
- **Live image preview**: `imgElement.src = window.URL.createObjectURL(fileInput.files[0])` on a
  file input's `change` event, no upload required to preview.

**Action**: `Views/Calculator/Index.cshtml` currently full-page POSTs and reloads to show the BOM
table. A `Recalculate` JSON action + this same `$.getJSON` pattern could swap in a `_BomResults`
partial without a reload — nice UX polish, not required for the defense, low effort. The
live-preview snippet is also a direct drop-in for the product image upload work below.

### 9.4 Car_TARge24 — the test-project template to actually copy

`Veebiarendus/Car_TARge24/TARge24_Car_Test/` ships a working xUnit + EF Core InMemory test
project:

```csharp
// TestBase.cs
public abstract class TestBase {
    protected IServiceProvider serviceProvider { get; set; }
    protected TestBase() {
        var services = new ServiceCollection();
        services.AddScoped<ICarServices, CarServices>();
        services.AddDbContext<Car_TARge24Context>(x => x.UseInMemoryDatabase("TEST"));
        serviceProvider = services.BuildServiceProvider();
    }
    protected T Svc<T>() => serviceProvider.GetService<T>();
}
```
Tests call the real `ApplicationServices` layer through its interface (`Svc<ICarServices>().Create(dto)`
etc.) against the in-memory DB — integration-style, not mocked. (Its own assertions are a bit thin
in places, but the harness itself is solid and directly copyable.)

**Action** (Phase 1, highest priority): add `ElektriKalkulaator.Tests`, same package set
(`Microsoft.EntityFrameworkCore.InMemory`, `xunit`, `xunit.runner.visualstudio`,
`Microsoft.NET.Test.Sdk`), same `TestBase`/`Svc<T>()` shape, pointed at `ElektriKalkulaatorContext`.
Cover `CalculatorServices.Calculate()` first.

### 9.5 JustShop2 — two working image upload/render implementations

`Agiilsed tarkvaraarenduse metoodikad/JustShop/JustShop2/` solves the exact
`Product.ImagePath`-never-rendered gap two ways: (a) save to a disk folder with a GUID-prefixed
filename, store the relative path, render with `<img src="~/path/@Model.FilePath">`; (b) store
`byte[]` directly in the DB, render as a base64 data URI. Option (a) matches `Product.ImagePath`'s
existing `string?` shape.

**Action** (Phase 1): the *cheapest* real working version of option (a) is actually in
`AdvancedAjax/CustomerController` — saving straight into `wwwroot/images` needs no extra
`StaticFileOptions`/`PhysicalFileProvider` registration (unlike JustShop2's approach, which saves
outside `wwwroot` and needs one). Add `<input asp-for="ImageFile" type="file">` to
`Views/Products/Create.cshtml`/`Edit.cshtml` (currently has no file input at all — confirmed), save
into `wwwroot/images/products/` with a GUID-prefixed filename, store the relative path in
`Product.ImagePath`, add one `<img>` to `Views/Products/Index.cshtml`'s product card.

### Prioritized shortlist across all five projects

1. **xUnit test project from Car_TARge24's pattern** (§9.4) — highest value, low effort, closes
   the most visible thesis gap.
2. **Product image upload/render via AdvancedAjax's simple disk-save pattern** (§9.5) — closes the
   other explicitly-flagged gap, no new middleware needed.
3. **ReactCRUD's SPA-proxy wiring, bookmarked for Phase 2** (§9.1) — not needed pre-defense, but
   removes all the guesswork later.
4. **AJAX-ify the calculator submit** (§9.3) — medium value UX polish, low effort.
5. **Cascading dropdowns on the Products form** (§9.3) — lower priority, essentially free once #4
   is done.

---

## 10. Current implementation status (as of 2026-08-10)

**Built and working**: full `Product`/`ProductCategory` CRUD, calculator form → BOM → persisted
history, session-based cart (checkout is cosmetic — clears session, writes nothing to DB),
automatic EF Core migration on startup, custom dark-navy/amber theme.

**Not built / deferred**: automated tests (§9.4), auth (`PowerboxCalculation.UserId` unused, no
`User` table despite being in the ERD spec), real order persistence/payment, i18n (§4.1 — this is
now known to be a documented design principle that wasn't implemented, not a deferred nice-to-have),
product images in the catalogue view, `evs_reference` traceability field, room-level rule
granularity, CAD import, PDF export, supplier price feeds.

**Known bugs/inconsistencies**: the `GetById` nullable-annotation lie and mixed
throw/null-return convention (§7).

---

## 11. Explicit design decisions & rationale ledger

| Decision | Rationale |
|---|---|
| Rule-based (`CalculationRule` table + arithmetic) instead of an LLM call | Explicitly reasoned through in `powerbox-no-ai-visual-explanation.html` (2026-02-03): auditability, zero ongoing cost, 100% predictability, offline capability, guaranteed standard compliance. |
| Two DTO layers, not three like ShopTARge24 | ShopTARge24's third layer existed for multi-entity forms with file uploads; this project's single CRUD entity doesn't need it. |
| `CalculationRule` joined to `PowerboxRequirements` by string equality, no formal FK | Deliberate — stated explicitly in both `tabelite-seosed-seletus.html` and `ERD_Loogiline_Seletus.docx` §4.6: rules are a general lookup table read by business logic, not a relational entity tied to one specific requirements row. |
| Estonian-only strings, no i18n yet | **Correction from rev. 1 of this document**: this was originally framed as a smart scoping decision. It's more accurate to say i18n was a *stated core design principle* in the approved ERD spec (§4.1) that wasn't implemented during the compressed build phase — still explainable (time constraints), but don't claim it was intentionally deferred as good scoping when the design doc itself calls it a main principle. |
| Separate `Create.cshtml`/`Edit.cshtml` instead of ShopTARge24's combined view | Legitimate alternative convention — no runtime branching, and the two forms genuinely diverge slightly. |
| Session-only cart, cosmetic checkout | Matches the thesis's scope: demonstrating the calculator and catalogue, not a production checkout flow. First real build target in a post-thesis commercial phase. |
| Rules keyed on `BuildingType` + `CircuitType` only, not per-room-type as documented | Simplification made during implementation, not previously written down anywhere — now recorded here (§4.1) so it reads as a known, explainable scope reduction rather than an oversight if raised in the defense. |

---

## 12. Roadmap

Status legend: `[ ]` not started · `[~]` in progress · `[x]` done.

### Phase 0 — Workflow setup
- [x] Cross-session memory of project context established.
- [x] This document created and revised (rev. 2, 2026-08-10) with the official thesis proposal,
      the formal ERD spec, and sibling-project research folded in.
- [ ] `CLAUDE.md` at repo root — optional, not yet requested.
- [ ] `.claude/agents/{architect,coder,tester}.md` — discussed, not yet built.

### Phase 1 — Pre-defense fixes, ranked by usefulness × cheapness
- [ ] **Add `ElektriKalkulaator.Tests`** — xUnit + EF InMemory, cloned from
      `Car_TARge24/TARge24_Car_Test`'s `TestBase`/`Svc<T>()` pattern (§9.4). Cover
      `CalculatorServices.Calculate()` first: circuit-count math, stove conditional, RCD+enclosure
      always present, empty result for unmatched `BuildingType`.
- [ ] **Render product images** — file-upload input on `Products/Create.cshtml`/`Edit.cshtml`,
      save to `wwwroot/images/products/` (AdvancedAjax-style, no extra middleware, §9.5), display
      in `Products/Index.cshtml`.
- [ ] Fix `ProductServices.GetById`/`CategoryServices.GetById` nullable-annotation lie; make
      `Update`/`Delete` not-found handling consistent (§7).
- [ ] **Nearly-free bonus**: add `EvsReference` (nullable string) to `CalculationRule`, seed real
      EVS-HD 60364 clause citations (§4.1) — directly strengthens the project's best defense
      talking point ("auditable, standards-traceable calculation") for one column and some seed
      data.
- [ ] *Optional, time permitting*: AJAX-ify the calculator submit (§9.3); broaden `/Products`
      filtering (price range, brand).
- [ ] Write the thesis's "future work" / limitations section using §4.1 (documented-vs-built gaps),
      §8 (i18n structural cost), and §12 Phase 3/4 below.

### Phase 2 — Post-defense: TypeScript/React frontend
- [ ] Add a Web API surface (`[ApiController]`) alongside/replacing the MVC controllers —
      `Core`/`Data`/`ApplicationServices` untouched.
- [ ] Clone ReactCRUD's SPA-proxy wiring (§9.1) — `.esproj` client project, `SpaProxy` package,
      Vite dev-proxy, same-origin static serving in prod, no CORS code.
- [ ] Turn `elecpro-components-&-calculator` into a real app wired to the new API instead of
      Gemini.
- [ ] Do real i18n here — add the `name_et`/`name_en`/`name_rus` columns and `User.language` that
      the ERD spec already calls for (§4.1), replace Estonian string join-keys
      (`CalculationRule.BuildingType`, `Product.Category.Name`) with stable codes + a translation
      table, since the API contract is already being touched.

### Phase 3 — Officially documented future work (from `ERD_Loogiline_Seletus.docx` §7)
- [ ] **CAD floor-plan import**: parse uploaded DXF/DWG files to auto-detect outlet/light/switch
      counts, populating `POWERBOX_REQUIREMENTS` automatically instead of manual entry. Extends
      `PowerboxRequirements` with `OutletsCount`/`LightsCount`/`SwitchesCount` and
      `PowerboxComponents` with a `CalculationSource` flag (`"rule_based"` vs `"cad_import"`).
      **This is the most plausible home for Python** in the stack (§6) — CAD/geometry parsing
      libraries are a Python strength; likely shape is a small dedicated parsing service the C#
      backend calls, not a rewrite of anything existing.
- [ ] **User accounts**: build the `User` table the ERD spec already defines (email, password
      hash, roles), wire it to the existing (currently unused) `PowerboxCalculation.UserId`, so
      calculation history can be tied to a real account instead of only session-scoped guests.
- [ ] **Live supplier price feeds**: extend `Product` with a `SupplierId` FK to a new `Supplier`
      table; real-time price queries against ABB/Schneider/Hager wholesaler feeds.
- [ ] **PDF export**: export a calculation's BOM as a PDF (component list, quantities, prices,
      EVS-HD 60364 references) — usable as a cost-estimation document for building-permit
      applications.

### Phase 4 — Dropshipping/marketplace vision (discussed with Claude, not yet a written thesis commitment)
- [ ] `Order`/`OrderLine` entities + real checkout persistence (replaces the cosmetic
      `CartController.Checkout()`).
- [ ] `Supplier` entity — overlaps with Phase 3's supplier-price-feed item; if both get built,
      design them as one entity, not two.
- [ ] Payment processing (Stripe.net).
- [ ] Supplier order relay — start manual/semi-automated, add per-supplier API integration only
      where volume justifies it.

**Stack recommendation** (unchanged from 2026-08-10, now reconciled against the official proposal
in §6): C#/.NET backend, TypeScript/React frontend once past the defense. Python is officially
declared but has no clear role yet except the CAD-import feature above — don't add it anywhere
else without a specific reason.

---

## 13. Open questions — don't assume, ask Edgar

- **Conflicting thesis-plan dates**: the proposal form's printed deadline says 9 February 2026;
  Edgar's own filled-in field says 31 May 2026. Which is real? Is there a separate, later *defense*
  date that isn't recorded in either document?
- **Python's actual intended role**: this document's best guess is CAD-import parsing (§6, §12
  Phase 3). Confirm, since it's currently inferred, not stated outright anywhere read so far.
- **Scope beyond the official pitch**: the proposal names only korterelamu + ärihoone; the built
  app and seed data also cover eramu. Intentional expansion — worth a one-line note in the thesis,
  or worth trimming back to match the proposal exactly? (This document's recommendation: keep it
  and mention it, don't trim.)
- **The "two required inputs" design principle vs. the five-ish-field form** (§4.1): explain as an
  accuracy-driven refinement in the thesis text, or treat as a gap? No action needed either way,
  just needs a chosen framing before the defense.
- Whether Edgar wants the Phase 0 items (`.claude/agents/*`, standalone `CLAUDE.md`) built is still
  open.

---

## 14. How to keep this document updated

Whoever (human or AI) does work on this project next:
1. Update the checkboxes in §12 as items move from not-started → in-progress → done.
2. If you make an architectural decision with a non-obvious rationale, add a row to §11.
3. Add a dated bullet to the Change Log below — specific, not just "updated code."
4. If you discover the repo has drifted from what this document claims, fix that section in place.
5. **Don't cite ephemeral file paths** (temp/scratchpad directories from a particular AI session)
   inside this document — inline the actual facts instead, since those paths won't exist for
   whoever reads this next.

## 15. Change log

- **2026-08-10 (rev. 1)** — Initial version. Synthesized from direct code reading, a background-agent
  diff against ShopTARge24/ShopTARge24_opetaja, the `elecpro-components-&-calculator` TS draft, and
  the `.mermaid`/`.html` ERD planning docs. `Lõputöö kavand_VORM.docx` and
  `ERD_Loogiline_Seletus.docx` explicitly flagged as unread.
- **2026-08-10 (rev. 2)** — Read both previously-flagged `.docx` files in full; corrected §1
  (institution/supervisor now doubly-confirmed, added proposal-date discrepancy), §2 (added the
  official pitch, quoted), added §4.1 (documented-ERD-vs-built-implementation gap table — most
  significantly, reframed i18n from "smart scoping" to "documented principle not yet built"),
  reconciled the official tech-stack list against reality in §6 (Python's likely role identified as
  CAD-import), added §9 (five sibling projects mined for concrete reusable patterns — test-project
  template, image upload pattern, React/C# wiring recipe, AJAX patterns), reworked §12's roadmap
  into four phases with sibling-sourced concrete action items and the officially-documented future
  work separated from the Claude-discussed dropshipping vision, expanded §13's open questions.
