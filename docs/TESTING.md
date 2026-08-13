# Testing guide

How this project is tested, and what to do when you add a test. Written for whoever works on this
next — human or AI.

**Current state: 128 automated tests + 18 live security checks, all passing.**

```bash
cd ElektriKalkulaator
dotnet test ElektriKalkulaator.Tests/ElektriKalkulaator.Tests.csproj   # the 128

# the 18 — needs the app running in another shell (dotnet run --project ElektriKalkulaator)
bash scripts/security-check.sh
```

---

## The one rule

**A test must be able to fail.** A test that passes no matter what the code does is worse than no
test, because it creates false confidence and nobody looks at it again.

Two real examples from this project:

- `Calculate_With9Lights_RoundsUpToTwoLightingCircuits` was proved by changing `Math.Ceiling` to
  `Math.Floor` in the real code and watching it go red, then reverting.
- The security script originally checked only that a redirect "did not go to evil.example.com" —
  which an *empty* response satisfies. It looked green while checking nothing. It now asserts the
  redirect goes to `/Cart`.

**When you add a test that matters, break the code on purpose, watch the test fail, then put the
code back.** If it does not fail, the test is not testing what you think.

---

## What lives where

| File | Covers | Needs |
|---|---|---|
| `TestBase.cs` | Shared setup: DI container + in-memory database, seeded | — |
| `CalculatorServicesTests.cs` | Core calculator behaviour: circuit maths, cheapest in-stock pick, saving | DB |
| `CalculatorEdgeCaseTests.cs` | Boundaries: 7/8/9 lights, all building types, totals, history limits | DB |
| `ProductServicesTests.cs` | Product CRUD, and not-found returning `null` | DB |
| `ProductImageLifecycleTests.cs` | `ImagePath` across create/update/delete | DB |
| `CatalogueSearchTests.cs` | Category + text search, combined with AND | DB |
| `CategoryServicesTests.cs` | Categories, and `CategoryDeleteResult` | DB |
| `SeedDataIntegrityTests.cs` | Seed data consistency, **including that image files exist on disk** | DB + files |
| `ImageUploadValidationTests.cs` | Upload rules, magic-byte checks | nothing |
| `FormValidationTests.cs` | `[Required]`, `[Range]`, `[Compare]` on the DTOs | nothing |
| `scripts/security-check.sh` | Auth, antiforgery, open redirect, cart input — over real HTTP | running app |

### Why the split

xUnit can test anything that does not need a live web server. It **cannot** easily test
authentication redirects, antiforgery enforcement, or HTTP status codes, because those only exist
once the whole pipeline is running. That is why the security checks are a script.

This is a known gap, not a design ideal — see "What is missing" below.

---

## The kinds of test used here

**Unit** — pure logic, no database. `ImageUploadValidationTests`, `FormValidationTests`. Fast, and
they fail for exactly one reason.

**Integration** — real services against a real (in-memory) database. Most of the suite. These
exercise the actual `CalculatorServices`/`ProductServices` code, so a genuine bug in them shows up.

**Data integrity** — `SeedDataIntegrityTests`. An underused and very cheap category: it asserts
things about the *data* rather than the code. It catches a class of bug the compiler never will —
a renamed category silently breaking the calculator's string matching, a rule needing a 25 A
breaker nobody stocks, or an `ImagePath` pointing at a file that was never committed. **That last
one is a real bug this project shipped**, and this is the test that would have caught it.

**Regression** — tests written *because* something broke, named so the reason is obvious:
`ExecutableRenamedAsJpg_IsRejected`, `Update_OfAProductThatWasDeleted_ReturnsNullAndChangesNothing`.

**End-to-end** — `scripts/security-check.sh`. Real HTTP against a running app.

---

## Conventions

- **Name the behaviour, not the method.** `LightingCircuits_AreOnePerEightLights` beats
  `TestCalculate2`. When it fails, the name alone should tell you what broke.
- **Arrange / Act / Assert**, in that order, with a blank line between.
- **One reason to fail per test.** If two asserts could fail independently for unrelated reasons,
  write two tests.
- **`[Theory]` with `[InlineData]` for boundaries.** Cheaper than eight near-identical `[Fact]`s
  and makes the boundary obvious.
- **Comment *why* a test exists** when it is not self-evident — especially regression tests. Say
  what broke.
- **Do not test the framework.** EF Core saving a row and `[Required]` rejecting empty text are
  Microsoft's tests, not yours. (`FormValidationTests` is a deliberate exception: it verifies *our
  attributes are still present and configured*, not that validation works.)
- **English, and explain the reasoning** — matches the rest of the codebase.

## Test isolation

`TestBase` gives every test class its own in-memory database named with a fresh `Guid`, so tests
never see each other's data and can run in any order. `EnsureCreated()` then applies the seed data
from `OnModelCreating`, so every test starts from the same known 10 products, 5 categories and 8
rules.

**Never write a test that depends on another test having run first.**

---

## What is missing — the honest list

Pick these up if you are looking for high-value work:

1. **HTTP-level integration tests (`WebApplicationFactory`).** The biggest gap. It would fold the
   18 security checks into `dotnet test` so they run automatically instead of needing a running app
   and a manual command. It would also allow proper role tests — anonymous vs customer vs admin
   hitting the same URL.
2. **Controller tests.** No controller is covered directly. The validation helpers are, and the
   services are, but the actions that glue them together are only exercised by the script.
3. **`CartController` has no automated tests at all.** Quantity clamping and the redirect guard are
   script-only. The session dependency makes it awkward, which is why it was deferred — but it is
   doable with a mocked `ISession`.
4. **No UI tests.** `Tarkvarasüsteemide_Testimine/SeleniumShopUITestSampleTARge24-main` in the
   wider coursework repo is a working Selenium template if this is ever wanted.
5. **No CI.** Nothing runs the tests automatically on push. `ShopTARge24` has a
   `.github/workflows` folder worth copying — and with 128 tests it would now be genuinely useful.

---

## Adding a test — checklist

1. Put it in the file that matches what you are testing, or add a new one with a clear name.
2. Follow the naming and structure conventions above.
3. **Prove it can fail** — break the code, watch it go red, restore.
4. Run the whole suite, not just yours: `dotnet test`.
5. If the test exists because of a bug, say so in a comment and add the entry to
   `docs/CHANGELOG.md`.
6. Update the counts at the top of this file if they changed.
