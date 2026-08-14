# Testing guide

How this project is tested, why it is tested that way, and exactly what to do when you add a test.
Written for two readers: a **beginner programmer** who has not written tests before, and an **AI
model** picking this project up with no memory of previous sessions.

**Current state: 199 automated tests, all passing, run by CI on every push.**

```bash
cd ElektriKalkulaator
dotnet test ElektriKalkulaator.Tests/ElektriKalkulaator.Tests.csproj
```

There is also `scripts/security-check.sh`, which runs the same security checks against a *manually
started* app. It is now **redundant** — everything it does is covered by the integration tests
below — but it is kept because it can be pointed at a deployed server, which `dotnet test` cannot.

---

# Part 1 — For a beginner: what a test actually is

A test is a small program that runs your real code and then checks the answer. If the answer is
wrong, the test fails and tells you where.

```csharp
[Fact]                                    // "[Fact]" marks a method as a test
public async Task EightLights_NeedOneCircuit()
{
    var input = new CalculatorInputDto { BuildingType = "korterelamu", LightCount = 8 };  // Arrange

    var bom = await Svc<ICalculatorServices>().Calculate(input);                          // Act

    Assert.Equal(1, bom.Count(b => b.CircuitType == "lighting" && !b.ProductName.Contains("kaabel")));  // Assert
}
```

Three steps, always in this order:

| Step | Means | In the example |
|---|---|---|
| **Arrange** | Set up the situation | a building with 8 lights |
| **Act** | Run the thing being tested | call `Calculate` |
| **Assert** | State what must be true | exactly 1 lighting circuit |

`Assert.Equal(expected, actual)` — **expected comes first**. Getting this backwards makes failure
messages read backwards, which is confusing at 2am.

### Why bother

You already test your code — by opening the browser and clicking. That works once. The problem is
the *second* change: you fix the cart and unknowingly break the calculator, and you will not click
through the calculator again because you were not thinking about it.

A test is that click, written down, run automatically, forever.

---

# Part 2 — The one rule that matters

> **A test must be able to fail.**

A test that passes no matter what the code does is *worse* than no test, because it creates
confidence that is not earned, and nobody looks at it again.

**Two real examples from this project:**

1. `scripts/security-check.sh` originally checked only that a redirect *"did not go to
   evil.example.com"*. An **empty** response satisfies that. The check was green while verifying
   nothing. It now asserts the redirect goes to `/Cart`.
2. An early test asserted a breaker's `WireCrossSectionMm2` was `null` to tell breakers from
   cables. It was not — `Calculate` copies the rule's value onto both. The test was wrong, not the
   code, and it only surfaced because it failed on the first run.

### How to prove a test can fail — do this every time

```bash
# 1. Break the real code on purpose (change Math.Ceiling to Math.Floor, delete an [Authorize], ...)
# 2. Run the test
dotnet test --filter "FullyQualifiedName~YourTestName"
# 3. Confirm it FAILS, and that the message points at the right thing
# 4. Put the code back
# 5. Run again, confirm it passes
```

This is called **mutation testing**. Every important test in this project has been through it. Some
that were verified this way:

| Mutation applied | Tests that went red |
|---|---|
| `Math.Ceiling` → `Math.Floor` | 1 |
| socket divisor `6` → `5` | 3 |
| moved a seeded image file away | 1 |
| removed `[Authorize]` from `ProductsController` | 10 |
| removed the `Url.IsLocalUrl` guard | 4 |
| removed one `[ValidateAntiForgeryToken]` | 1 |

---

# Part 3 — The kinds of test, and when to use each

## 1. Unit tests — pure logic, nothing else

No database, no web server. Fastest, and they fail for exactly one reason.

**Use when:** the code is a calculation or a decision that takes values in and gives values out.

**Here:** `ImageUploadValidationTests`, `FormValidationTests`.

```csharp
// No TestBase inheritance — nothing external is needed
public class ImageUploadValidationTests
{
    [Fact]
    public void ExecutableRenamedAsJpg_IsRejected()
    {
        var exe = new byte[] { 0x4D, 0x5A, /* "MZ" - every Windows .exe starts with this */ };
        var error = ProductsController.ValidateImageFile(FakeUpload("payload.jpg", exe));
        Assert.Contains("ei ole korrektne pildifail", error);
    }
}
```

## 2. Integration tests (service + database)

Real services against a real — but in-memory — database. Slower, but they exercise the code the
application actually runs, including the EF Core queries.

**Use when:** the behaviour involves storing, loading or querying data.

**Here:** most of the suite. Inherit `TestBase`, then call `Svc<IProductServices>()`.

## 3. HTTP integration tests — the whole application

`WebApplicationFactory` boots the **real** `Program.cs` in memory: the same DI, the same middleware
order, the same attributes. Only the database is swapped. Tests then send real HTTP requests.

**Use when:** the behaviour comes from middleware or attributes rather than from your own methods —
authentication, authorization, antiforgery, redirects, status codes. **Nothing else can test these.**

**Here:** `Integration/AuthorizationTests`, `Integration/RequestSecurityTests`.

```csharp
public class AuthorizationTests : IClassFixture<TestWebAppFactory>   // one app for the whole class
{
    [Theory]
    [MemberData(nameof(AdminOnlyPages))]
    public async Task AnonymousVisitor_IsSentToTheLoginPage(string url)
    {
        var client = _factory.CreateNonRedirectingClient();   // must NOT follow redirects
        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Account/Login", response.RedirectPath());
    }
}
```

**Two traps worth knowing before you write one:**

- **The client must not follow redirects.** A normal `HttpClient` follows the 302 to the login page
  and reports `200`, so a test checking "anonymous users are blocked" passes even when they are
  not. Always use `CreateNonRedirectingClient()`.
- **POSTs need an antiforgery token.** Fetch the page first and read the hidden field —
  `PostFormAsync` in `HttpTestHelpers` does this for you.

## 4. Data integrity tests — check the *data*, not the code

An underused and very cheap category. It asserts things about the seeded data rather than about any
method.

**Use when:** code depends on data being shaped a certain way. This project matches product
categories **by exact string**, so a renamed category silently breaks the calculator with no
compiler error anywhere.

**Here:** `SeedDataIntegrityTests`. It caught a bug this project genuinely shipped — `ImagePath`
values pointing at image files that a `.gitignore` rule had excluded from the repository, so a
fresh clone rendered ten broken images.

## 5. Regression tests — written because something broke

Not a separate technique; a separate *reason*. Name them so the reason is obvious and comment what
broke.

**Here:** `ExecutableRenamedAsJpg_IsRejected`, `Update_OfAProductThatWasDeleted_ReturnsNullAndChangesNothing`,
`CableLines_AreMeasuredInMetres`.

---

# Part 4 — What must be tested

Ordered by how much damage an untested failure would do.

### Always, without exception

| What | Why | Where here |
|---|---|---|
| **Money and quantity arithmetic** | Wrong numbers are the product being wrong. A cable priced per piece instead of per metre misleads someone ordering materials. | `CalculatorServicesTests`, `BomUnitOfMeasureTests` |
| **Who can reach which page** | The worst bug this project ever had: every admin page open to everyone. | `Integration/AuthorizationTests` |
| **Boundaries** | Off-by-one lives here. Test *both sides*: 7, 8, 9 — not just 8. | `CalculatorEdgeCaseTests` |
| **Anything accepting a file or a URL** | Uploads write to disk; redirects send users away. Both are attacker-controlled. | `ImageUploadValidationTests`, `RequestSecurityTests` |
| **Every bug you fix** | Otherwise it comes back and nobody notices. | throughout |

### Usually worth it

- **Not-found paths** — asking for something deleted should give 404, not a crash.
- **Data integrity** — see Part 3.4.
- **Validation attributes** — one deleted line removes a rule with nothing failing to compile.

### Do not test

- **The framework.** EF Core saving a row, `[Required]` rejecting empty text — those are
  Microsoft's tests. (`FormValidationTests` is a deliberate exception: it checks *our attributes
  are still present and configured*, not that validation works.)
- **Exact wording of user-facing text**, unless the wording is the point. It changes often and the
  test adds nothing.
- **Private methods directly.** Test them through the public behaviour that uses them. If that is
  impossible, the method probably belongs somewhere else.

---

# Part 5 — Conventions in this project

- **Name the behaviour, not the method.** `LightingCircuits_AreOnePerEightLights`, not `TestCalc2`.
  A failing name alone should tell you what broke.
- **Arrange / Act / Assert**, blank line between each.
- **One reason to fail per test.** Two independent asserts → two tests.
- **`[Theory]` + `[InlineData]` for boundaries.** Cheaper than eight near-identical `[Fact]`s and
  the boundary becomes visible at a glance.
- **Comment *why* a test exists** when it is not obvious — especially regression tests. Say what
  broke.
- **English, explaining the reasoning** — same rule as the rest of the codebase.
- **Never depend on another test having run first.** `TestBase` gives every test class its own
  database named with a fresh `Guid`, so order never matters. Keep it that way.

## Where things live

| File | Covers | Needs |
|---|---|---|
| `TestBase.cs` | DI container + seeded in-memory database | — |
| `CalculatorServicesTests.cs` | Core calculator behaviour | DB |
| `CalculatorEdgeCaseTests.cs` | Boundaries, all building types, history limits | DB |
| `BomUnitOfMeasureTests.cs` | Pieces vs metres on each BOM line | DB |
| `ProductServicesTests.cs` | Product CRUD, not-found returning `null` | DB |
| `ProductImageLifecycleTests.cs` | `ImagePath` across create/update/delete | DB |
| `CatalogueSearchTests.cs` | Category + text search combined with AND | DB |
| `CategoryServicesTests.cs` | Categories, `CategoryDeleteResult` | DB |
| `SeedDataIntegrityTests.cs` | Seed data consistency, image files existing | DB + files |
| `ThemeTokenTests.cs` | Views use colour tokens, not literals; both palettes match | files |
| `ImageUploadValidationTests.cs` | Extension, size and magic-byte checks | — |
| `FormValidationTests.cs` | `[Required]`, `[Range]`, `[Compare]` on DTOs | — |
| `Integration/TestWebAppFactory.cs` | Boots the real app for HTTP tests | — |
| `Integration/HttpTestHelpers.cs` | Antiforgery tokens, login, redirect paths | — |
| `Integration/AuthorizationTests.cs` | Anonymous / customer / admin access | full app |
| `Integration/RequestSecurityTests.cs` | Antiforgery, open redirect, cart input | full app |

---

# Part 6 — Adding a test: the checklist

1. Decide **which kind** it is (Part 3). Wrong kind = slow, brittle, or unable to see the bug.
2. Put it in the matching file, or create one with a clear name.
3. Follow the naming and structure conventions (Part 5).
4. **Prove it can fail** (Part 2). Break the code, watch it go red, restore.
5. Run the **whole** suite, not just yours: `dotnet test`.
6. If it exists because of a bug, comment what broke and add a `docs/CHANGELOG.md` entry.
7. Update the test count at the top of this file.

---

# Part 7 — What is still missing

An honest list. Good places to start if you are looking for work.

1. **`CartController` has no unit tests.** It is covered at HTTP level, but its session handling is
   untested in isolation. Awkward because of the session dependency; doable by mocking `ISession`.
2. **No UI tests, and no way to catch a purely visual defect.** **This is now the biggest gap.**
   Nothing verifies that pages *look* right or that JavaScript works.

   What this costs is documented: on 2026-08-14 we found 38 uses of Bootstrap's `text-white` across
   14 views. `text-white` means literally `#fff`, so those headings were **white text on a white
   card** in light mode — the light theme shipped three days earlier was partly unusable, and 196
   passing tests said nothing. `ThemeTokenTests` now catches that specific family of bug, but note
   its limit: it checks colours written in the *markup*, not whether the palette itself is any good.
   **Judging whether a design looks good still needs a person looking at a screen.**

   `Tarkvarasüsteemide_Testimine/SeleniumShopUITestSampleTARge24-main` in the wider coursework repo
   is a working Selenium template if this is ever wanted.
3. **No performance tests.** `Calculate` issues two queries per rule and the cart one per line.
   Fine at this size; nothing would warn you when it stops being fine.
4. **No test of the migration path.** Tests use `EnsureCreated` against an in-memory database, so a
   broken SQL Server migration would not be caught until someone runs the app.
