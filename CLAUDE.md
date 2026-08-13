# CLAUDE.md — instructions for AI assistants working on this project

This file is loaded automatically at the start of every session. Read it, then read
`PROJECT_ROADMAP.md` before doing anything else.

---

## What this project is

**ElektriKalkulaator** — Edgar Muoni's diploma thesis (LÕPUTÖÖ) at Tallinna Tööstushariduskeskus,
supervised by Kalle Olumets. An ASP.NET Core 9 MVC + EF Core 9 web app that calculates a **priced
Bill of Materials** for an electrical panel from the Estonian standard **EVS-HD 60364**, using a
real product catalogue.

The calculator is the point. The shop exists to serve it.

## The four documents

| File | What it holds | When to update |
|---|---|---|
| `PROJECT_ROADMAP.md` | Architecture, status, decisions, plans | When status or plans change |
| `CHANGELOG.md` | What changed in the code and **why** | **Every code change** |
| `RESEARCH_LOG.md` | Facts gathered from outside (prices, competitor design, UX research) | When you research something external |
| `IMAGE_CREDITS.md` | Licence and attribution for every image | When images change |

## Rules — follow these without being asked

1. **Write a `CHANGELOG.md` entry for every code change.** The template is at the top of that file.
   The *why* matters more than the *what* — code shows what changed, only you know why.
2. **Tick the matching checkbox** in `PROJECT_ROADMAP.md` §D2/§D3 when you complete a planned item.
3. **All code comments in English**, written so a beginner can follow the reasoning. Some older
   files still have Estonian comments — convert them when you touch those files.
4. **Images: `.jpg` only.** One format for the whole catalogue.
5. **Test against the running application, not just the compiler.** "It builds" is not "it works".
   Start it with `dotnet run`, exercise the real HTTP endpoints, and say what you actually observed.
6. **Clean up test data.** The dev database should end a session with 10 seeded products and only
   the seeded admin user.
7. **Never invent EVS-HD 60364 clause numbers.** `CalculationRule.EvsReference` is deliberately
   empty until Edgar verifies them against the real standard. A fabricated citation in a thesis
   about standards compliance is worse than a blank field.
8. **Never commit secrets.** Connection string and admin credentials live in User Secrets.

## Commands

```bash
# from ElektriKalkulaator/
dotnet build ElektriKalkulaator.slnx
dotnet test ElektriKalkulaator.Tests/ElektriKalkulaator.Tests.csproj
dotnet run --project ElektriKalkulaator            # http://localhost:5250

# re-check the app is still locked down (app must be running)
bash scripts/security-check.sh
```

Local secrets already set on Edgar's machine (`dotnet user-secrets list` from the web project):
`ConnectionStrings:DefaultConnection`, `AdminUser:Email`, `AdminUser:Password`.

## Architecture in one line

`Core` (domain, DTOs, interfaces) ← `Data` (DbContext, migrations, seed) ← `ApplicationServices`
(service implementations) ← `ElektriKalkulaator` (web). `Tests` references the first three.

Never make `Core` depend on anything. Never make `Data` reference the web project.

## Things that look like bugs but are deliberate

- `CalculationRule` has **no foreign key** — C# joins it by `BuildingType` string at runtime. This
  is stated in Edgar's own ERD specification.
- `CartController.Checkout()` **saves nothing**. There is no `Order` entity yet; this is scoped
  future work, not an oversight.
- `ICategoryServices.Delete` is **unreachable** — nothing calls it. Kept for the planned admin area.
- `Product.Price` **does not declare whether it includes VAT.** This is a known open question, not
  something to guess at. See `RESEARCH_LOG.md`.

## Working style Edgar has asked for

- Small changes, one at a time, tested as you go — not large batches.
- Explain what you did and what you found, including problems.
- Use branches and pull requests, not commits straight to `main`.
- Say when something is uncertain or unverified rather than sounding confident.
