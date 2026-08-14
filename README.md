# ElektriKalkulaator

**Elektrikilbi ja -tarvete komponentide kalkulaator** — an electrical panel and supplies component
calculator.

A web application that turns a building's basic parameters into a **priced Bill of Materials** for
its electrical installation: which circuit breakers, how much cable, an RCD and an enclosure. The
calculation follows the Estonian electrical standard **EVS-HD 60364** and prices the result against
a real product catalogue, replacing work normally done by a cost estimator (*eelarvestaja*).

Diploma thesis (LÕPUTÖÖ) by **Edgar Muoni**, group TARge24, Tallinna Tööstushariduskeskus.
Supervisor: **Kalle Olumets**.

---

## Running it

Requires the .NET 9 SDK and SQL Server (LocalDB is fine — it ships with Visual Studio).

```bash
cd ElektriKalkulaator
dotnet run --project ElektriKalkulaator
```

Then open <http://localhost:5250>. The database is created and seeded automatically on first run,
with 10 demo products and the EVS-HD 60364 calculation rules.

### Optional configuration

`appsettings.json` defaults to LocalDB so a fresh clone runs with no setup. To use a different
SQL Server, or to create an administrator account, use User Secrets — **never edit the committed
file**, and never put a password in it:

```bash
cd ElektriKalkulaator/ElektriKalkulaator
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your connection string>"
dotnet user-secrets set "AdminUser:Email" "you@example.com"
dotnet user-secrets set "AdminUser:Password" "<a strong password>"
```

Without the admin settings the app still runs — it just creates no administrator, and logs a
warning saying so.

## Testing

```bash
cd ElektriKalkulaator
dotnet test ElektriKalkulaator.Tests/ElektriKalkulaator.Tests.csproj
```

There is also a security check that runs against the live application. Start the app first, then:

```bash
bash scripts/security-check.sh
```

It re-runs every vulnerability found in the security review — authentication, antiforgery, open
redirect, input validation — and exits non-zero if any regressed.

## Structure

```
ElektriKalkulaator.Core     domain models, DTOs, service interfaces  (depends on nothing)
ElektriKalkulaator.Data     DbContext, migrations, seed data          (depends on Core)
...ApplicationServices      service implementations                   (Core + Data)
ElektriKalkulaator          the web application                       (all of the above)
ElektriKalkulaator.Tests    48 automated tests
```

Built with ASP.NET Core 9 MVC, EF Core 9, SQL Server, Bootstrap 5 and ASP.NET Core Identity.

## Documentation

Everything is in [`docs/`](docs/):

| Document | What it covers |
|---|---|
| [PROJECT_ROADMAP.md](docs/PROJECT_ROADMAP.md) | Architecture, current status, design decisions and what is planned next. **Start here.** |
| [CHANGELOG.md](docs/CHANGELOG.md) | Every code change and the reasoning behind it |
| [RESEARCH_LOG.md](docs/RESEARCH_LOG.md) | Market prices, competitor analysis and UX research, with sources |
| [IMAGE_CREDITS.md](docs/IMAGE_CREDITS.md) | Licence and attribution for every image |
| [DESIGN_GUIDE.md](docs/DESIGN_GUIDE.md) | Design system and page-by-page UI instructions |
| [TESTING.md](docs/TESTING.md) | How the project is tested, and what to test |
| [TEST_ACCOUNTS.md](docs/TEST_ACCOUNTS.md) | Demo admin and customer logins for trying the site |
| [VOICE_AND_PERSONALITY.md](docs/VOICE_AND_PERSONALITY.md) | How the site should sound to a customer |
| [SUPPLIER_SYNC_SPEC.md](docs/SUPPLIER_SYNC_SPEC.md) | Plan for the future supplier price-sync model (not built) |
| [PROMPTS.md](docs/PROMPTS.md) | Prompts for working on this project with an AI assistant |

[`CLAUDE.md`](CLAUDE.md) sits at the repo root because AI assistants load it automatically from
there; it points at the documents above.

## Licence and attributions

Product photographs are freely licensed (Wikimedia Commons CC0 / CC BY-SA, Unsplash, Pexels) and
individually credited in [docs/IMAGE_CREDITS.md](docs/IMAGE_CREDITS.md). No manufacturer
photography is used — see that file for why.
