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
