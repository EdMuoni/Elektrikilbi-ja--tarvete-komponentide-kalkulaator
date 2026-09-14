# HANDOFF — read this before doing anything else

**Version 7** · written **2026-09-14** · replaces version 6 (still readable with `git show c106e38:HANDOFF.md`)
**Project:** Elektrikilbi ja -tarvete komponentide kalkulaator — Edgar Muoni's diploma thesis (LÕPUTÖÖ)
**For:** the next AI model, and for Edgar when he opens a new chat.

§0 is enough to start. §1 prevents the mistakes that have already happened once. The rest is
reference: read the section you need before touching that area.

**Maintenance rule:** when a fact changes, edit the place where it is stated. Never append a
correction somewhere else and leave the old claim standing. Version 6 broke this rule and a stale
copy misled another session.

---

## 0. START HERE — state on 2026-09-14

### Where the work stopped
The supervisor, **Kalle Olumets**, has reviewed the thesis three times. Every comment from all three
rounds is fixed. The current version is **V13**. Edgar needs Kalle's signature (allkiri) on the work.
**Unknown at the time of writing: whether V13 has been sent to Kalle yet — ask Edgar.**

| | |
|---|---|
| **Current thesis** | `LÕPUTÖÖ\Lõputöö_Dokumendi_variandid\Elektrikilbi ja -tarvete komponentide kalkulaator_V13.docx` |
| **For Kalle** | same folder, `…_V13_KALLELE.pdf` — 39 pages |
| Measured (V13) | 39 pages · 7 164 words · 18 figures · 16 tables (15 captioned + glossary) · 12 sources · 17 in-text citations · layout check: 0 problems |
| Defence guide | `LÕPUTÖÖ\Kaitsmine\Kaitsmise_juhend.pdf` (+ `.docx`, source `.html`), 16 pages, Estonian. **Private — never copy it into this public repo** |
| Thesis tools | `LÕPUTÖÖ\Tööriistad\` — Word automation scripts and the spec of every version (§8) |
| Code | branch `feat/conversion-ux`, pushed, CI green on 2026-09-12. **`main` is NOT up to date** (§1.1) |
| Tests | 218, all passing (last run 2026-09-13) |

### If Kalle sends more comments — the proven workflow
1. Ask Edgar for **Kalle's Google Docs version as a .docx** (Google Docs → File → Download →
   Microsoft Word). Screenshots are not enough: **Kalle edits text directly without a comment**
   (round 2 had two silent edits, round 3 had two), and only the file shows those.
2. Read `word/comments.xml` from that file for the comments and their anchors, and diff its text
   against the current version paragraph by paragraph. The Python used for both is in this chat's
   transcript (§1.6); it is about 40 lines of `zipfile` + `re` + `difflib`.
3. Report the comments to Edgar as a table with your verdict. Wait for his go-ahead.
4. Write `Tööriistad\v14_spec.json` and `apply_v14.ps1` (copy `apply_v13.ps1`; it copies the previous
   version, applies inline replacements with an exact expected count, italics, then updates the TOC).
5. Run `check_layout.ps1` and `export_pdf.ps1` on the new version. Render pages if layout changed.
6. Update §0 and §5 of this file.

### Waiting on Edgar, not on an AI
1. Send V13 to Kalle and get the signature.
2. Merge the pull requests so `main` holds the work (§1.1).
3. Change the local admin password — an admin session cookie is in the public repo's history (§7).
4. Read OWASP Top 10:2025 categories **A01** (access control) and **A05** (injection) — the thesis
   now cites them, and he has not read them yet (he said so on 2026-09-13).
5. Check that the source access dates match his memory (§6).
6. Decide whether to deploy the app somewhere (assessment criterion B.2.6 "Juurutamine").
7. Confirm the origin story: when and why the Gemini prototype was abandoned (files say 29.01 and
   03.02.2026), and whether `AIStudio_Prompt_ElektrikilbiKalkulaator.txt` generated the first scaffold.
8. Ask the school library or Kalle about access to EVS-HD 60364 (needed to fill `EvsReference`).
9. Authorize the plugin connectors he wants (§10).

---

## 1. STOP — facts that make you wrong if you skip them

**1.1 `main` does not contain the work.** Verified 2026-09-14:

| | commit | date |
|---|---|---|
| `origin/main` | `46ebffa` | 2026-08-11 |
| `origin/feat/conversion-ux` | the commit that last changed this file, or later | 2026-09-14 — **over 50 commits ahead of main, 0 behind** |

Pull requests #1–#4 are a linear stack, all **open**: #1 `main` ← `fix/track-seeded-product-images`,
#2 ← `feat/upload-validation-messages`, #3 ← `fix/security-hardening`, #4 ← `feat/conversion-ux`.
Each branch contains the one before it, so `feat/conversion-ux` holds everything.
`gh` is not installed and no AI in this project has GitHub write access, so **Edgar merges in the
GitHub web UI**: in order #1 → #4, always **"Create a merge commit"** (squash breaks the stack),
deleting each merged branch so GitHub retargets the next PR to `main`.
Before claiming anything about "the code on GitHub", run `git fetch` and compare.

**1.2 The thesis is a Word file outside this repo.** `docs/LOPUTOO_MUSTAND.md` is a stale draft from
before 2026-09-10 — never read it as the thesis, never edit it. **Never draft thesis text before
reading the current .docx**; it already contains most of what you would write.

**1.3 Always make a new version file.** Never edit the previous version in place. The previous one is
the fallback.

**1.4 Edgar opens and renames the files himself.**
- From V12 on, the files are named `Elektrikilbi ja -tarvete komponentide kalkulaator_V<n>.docx`.
  V11 exists under both names (identical bytes). A script that looks for `Elektrikilbi_v12.docx`
  finds nothing — that happened.
- He may have the document **open in Word** while you work. A script that attaches to his Word and
  calls `Quit()` closes his window and loses unsaved work. **Every tool in `Tööriistad\` now records
  the WINWORD process IDs before starting Word and quits only its own instance; if it cannot
  identify its own, it stops without closing anything.** Keep that guard in any new script.
- He saved V12 in Word after it was built (12:02:57 on 2026-09-13). The text was verified
  unchanged. If a file's modified time is newer than your build, diff before building on it.

**1.5 The session scratchpad disappears.** `C:\Temp\claude\…\scratchpad` is per session. Anything
you want the next session to have must go into `LÕPUTÖÖ\Tööriistad\`, the repo, or this file.

**1.6 What carries over between sessions.**
- This file and `CLAUDE.md` (auto-loaded in Claude Code for this folder).
- Claude Code auto-memory for this project folder, on this machine only.
- **Full transcripts of earlier chats** on this machine, in
  `C:\Users\Jazztime\.claude\projects\C--Users-Jazztime-Desktop-TARge24-L-PUT---Elektrikilbi-ja--tarvete-komponentide-kalkulaator\`.
  The long thesis chat that produced V7–V13 and this file is **`05983da2-4480-46b5-b2b8-39359f591952.jsonl`**
  (~72 MB — search it with `grep`, never read it whole). `9d0cc90b-…` is a later, smaller session;
  `eaca75f1-…` is from July.
- Nothing carries over to claude.ai web chats or other machines. There, paste §11.

**1.7 The AI-use disclosure — do not rewrite it as code-only.** Edgar has asked twice (2026-09-12)
that the thesis say AI was used only for code and logic. Declined both times: a large part of the
thesis text was drafted with AI help, Kalle explicitly asked for AI use to be written down, and the
template counts unreferenced text as the author's own. The section in §1.6 of the thesis leads with
code and logic but still says AI helped with documentation and thesis text. Kalle has read it in
rounds 2 and 3 and only corrected one word. Keep it.

**1.8 Never invent.** No EVS-HD 60364 clause numbers (`CalculationRule.EvsReference` is empty on
purpose). No invented access dates (§6). No citation years or sources you have not fetched.

**1.9 PowerShell 5.1 reads a UTF-8 script without BOM as ANSI**, so `õ ä ö ü` in a `.ps1` break
parsing ("Unexpected token"). Keep scripts ASCII and put Estonian text in a JSON spec read with
`Get-Content -Encoding UTF8`.

---

## 2. Where everything is

```
C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\
├─ Elektrikilbi-ja--tarvete-komponentide-kalkulaator\   this repo — PUBLIC on GitHub
├─ Lõputöö_Dokumendi_variandid\   every thesis version (v2 … V13) and its _KALLELE.pdf,
│                                 Autorideklaratsioon.docx, and a stale HANDOFF.md v5 (ignore it)
├─ Tööriistad\                    Word automation for the thesis (§8)
├─ Kaitsmine\                     defence guide — private
├─ Pictures\Diagrams\             every figure image placed in the thesis (ERDs, kood-1…9, skeem-*)
├─ Pictures\Lõputöö_märkused\     Kalle round 1: 1–14.PNG · subfolder Uus\ = round 2 screenshots
├─ Näited\                        Kalle's own thesis (worked example) and 5 other example theses
├─ Elektrikilbi ja -tarvete kalk draft\   Jan–Mar 2026: TypeScript/React Gemini prototype,
│                                 the written AI→rules decision (HTML), ERD drafts
├─ LÕPUTÖÖ_TEMPLATE (1).docx      school template. Its rules are in IMAGES (word/media/image1–6.png).
│                                 LISA B = assessment criteria. LISA E = citation examples
├─ Lõputöö kavand_VORM.docx/.asice   the signed proposal
├─ Lõputöö esitlusslaidid_NÄIDIS.pptx  defence slide format, 8 slides
├─ Hindamisstandard_*.pdf         assessment standards (not read: no PDF tool at the time)
└─ AIStudio_Prompt_ElektrikilbiKalkulaator.txt   2026-04-24 prompt, role unconfirmed
C:\Users\Jazztime\Downloads\      Kalle's Google Docs exports: Elektrikilbi_v11.docx (round 2),
                                  Elektrikilbi ja -tarvete komponentide kalkulaator_V12.docx (round 3)
```

---

## 3. The project in one page

| | |
|---|---|
| School | Tallinna Tööstushariduskeskus (TTHK), group TARge24, noorem tarkvaraarendaja (TAR), EKR 4 |
| Author / supervisor | Edgar Muoni / Kalle Olumets. Edgar defends the work himself |
| Repo | `EdMuoni/Elektrikilbi-ja--tarvete-komponentide-kalkulaator` (public) |
| Stack | ASP.NET Core 9 MVC, EF Core 9, SQL Server, Identity, Bootstrap 5, xUnit |
| Solution | `Core` ← `Data` ← `ApplicationServices` ← web; `Tests` references all four |
| Product name in the thesis | „Elektrikilbi ja -tarvete komponentide kalkulaator“ (Edgar's decision 2026-09-13; the app header says "ElektrikilbiKalkulaator") |

**Calculator** (`CalculatorServices.Calculate()`): lighting 1 circuit per 8 lights (1.5 mm², 10 A);
sockets 1 per 6 (2.5 mm², 16 A); stove 1 dedicated (6 mm², 32 A) — ärihoone has no stove rule and the
app says so. `Math.Ceiling`. Cable = circuits × rooms × 8 m, a **declared estimate, never call it
"suvaline"**. Picks the cheapest in-stock product, plus one enclosure and one RCD. Worked example:
3 rooms, 10 sockets, 12 lights, stove = **348,90 €**, 120 m cable (thesis Tabel 11).

**EVS-HD 60364 scope:** from the standard = cable/breaker pairing (parts 4-43, 5-52) and the RCD
(4-41). Design practice, not the standard = 8 lights / 6 sockets per circuit.

**Origin:** Jan 2026 TypeScript/React prototype where Gemini chose the components → rejected in
writing 2026-02-03 (unpredictable, uncheckable) → rules in the database. This is the strongest
defence point. Never suggest putting an LLM back into the calculation.

---

## 4. What Kalle wants — the house style learned from three reviews

Apply these everywhere, not only where he pointed. Each was a real comment.

| Rule | Example |
|---|---|
| Numbered chapters start on a new page; a heading never sits alone at a page foot | `PageBreakBefore` on Heading 1, `KeepWithNext` on all headings |
| Refer to every figure in the body text | "nagu on näha Joonisel 5" |
| **English terms in italics** — the template also asks for the Estonian term with the English one in brackets | avaplokk (ingl *hero section*), *pull request*, *commit*, glossary expansions, *Bootstrap* |
| Captions upright (as in Kalle's own thesis), so italic terms inside them stand out | all 33 captions |
| **Split compounds** where Kalle asked, and keep the same word the same everywhere | elektri komponentide, veebi rünnakute, nelja kuulisest, programmeerimise probleemide, rünnaku katsed, materjali rea, reeglite tabelis, kontrastide suhteid, pliidi ahel (also valgustite / pistikute ahel), tabeli lahtrite, lehe tausta, mutatsiooni testimine. He joined one: olemasolevad |
| A list introduced with a count is numbered | "Kolm olulisemat õppetundi:" → 1. 2. 3. |
| Plain words, no calques | not "kangelasosa", not "Rekonstrueerisin" → "Koostasin" |
| Name the product instead of "selle töö" when the product is meant | „Rakenduse „Elektrikilbi ja -tarvete komponentide kalkulaator“ tegin…“ |
| Glossary (MÕISTED JA LÜHENDID) comes **after** SISSEJUHATUS | |
| Visible in-text citations, APA 7 as in template image 4: organisation's first citation with the abbreviation, then the abbreviation | (Eesti Standardimis- ja Akrediteerimiskeskus [EVS], 2011, 2017, 2023) → (EVS, 2017) |
| Kalle reviews in Google Docs — **send him the PDF**, not the .docx | |

**Likely next comment, not yet made:** Kalle had *Bootstrap* italicized, but other English product
names (ASP.NET Core, Entity Framework Core, Visual Studio, GitHub, Claude) are still upright. If he
flags one, treat them all the same way.

Round 1 comment **4a** (paigaldis → paigaldus) was deliberately **not** applied: the standard's own
title is "Madalpingelised elektri**paigaldised**", and Kalle later wrote that title himself. Edgar
agreed on 2026-09-13.

---

## 5. Version history

| Version | Date | What changed | Driver |
|---|---|---|---|
| v5 | 09-11 | mina-vorm, 35 pages — **the version Kalle reviewed in round 1** | |
| v6_KALLE, v7 | 09-12 | Kalle round 1 (18 comments, screenshots): page breaks, language, figure references, security-testing statement, AI section | Kalle R1 |
| v8 | 09-12 | Both ERDs from one generator (crow's foot) | Edgar |
| v9 | 09-12 | +8 figures (6 generated code figures, architecture, flowchart) | "thesis too thin" |
| v10 | 09-12 | Layout only; text identical to v9 | half-empty pages |
| v11 | 09-13 | Critical-analysis pass: sources 6 → 12, chapter 3 renumbered, Paigaldamine section, J8–J10 as light code figures | Edgar |
| **V12** | 09-13 | Kalle round 2 (17 comments + 2 silent edits): glossary after intro, his added sentence, product name, numbered lists, *hero section*, English terms italic, captions upright, **Joonis 5 redrawn portrait and full page**, 3 new citations, EVS short form, split compounds, access dates from git evidence | Kalle R2 |
| **V13** | 09-13 | Kalle round 3 (4 comments, 2 already edited by him): *Bootstrap* italic, tabeli lahtrite, lehe tausta, "ja nii, et" | Kalle R3 |

Exact edits per version are in `Tööriistad\v11_spec.json`, `v12_spec.json`, `v13_spec.json`.

---

## 6. Source access dates ("vaadatud") — evidence, not invention

Edgar asked on 2026-09-13 to spread the dates over three to four months. They were **not made up**:
each date is the day the repo shows work with that source's topic. They now span April–September.
Do not move any of them into June–July: the thesis's own Tabel 4 says that was a pause.

| Source | Date | Evidence in git |
|---|---|---|
| Microsoft n.d.-a ASP.NET Core docs | 21.04.2026 | first commit |
| Bootstrap 5.3 docs | 26.04.2026 | Bootstrap added to the project |
| Microsoft n.d.-b EF Core docs, n.d.-d Migrations | 29.04.2026 | first migration |
| EVS-HD 60364-4-41:2017 | 10.08.2026 | first mention in the roadmap |
| Microsoft n.d.-c Identity; OWASP CSRF cheat sheet | 11.08.2026 | authentication and security fixes |
| GitHub Actions docs; W3C WCAG 2.1 | 14.08.2026 | CI added; palette and contrast work |
| EVS-HD 60364-5-52:2011, 4-43:2023 | 09.09.2026 | EVS source analysis (`docs/EVS_ALLIKAD.md`) |
| OWASP Top 10:2025 | 13.09.2026 | no earlier evidence — Edgar has not read it yet |

OWASP Top 10:2025 was fetched on 2026-09-13: A01 Broken Access Control, A05 Injection (explicitly
includes SQL injection and XSS). The thesis cites both.

---

## 7. Open items and known risks

- **`main` more than 50 commits behind** (§1.1).
- **Security:** `admin.txt`, a curl cookie jar with a live localhost admin session cookie, was
  committed in `44773b3` and pushed to the public repo; removed in `ed76ce0`. History was not
  rewritten (not approved). Changing the local admin password makes the copy worthless. Not done yet.
- **Deployment:** the app runs only locally; CI builds and tests but does not deploy (criterion B.2.6).
  The thesis says so honestly in §2.4 "Paigaldamine".
- **No security scanner** (OWASP ZAP) has been run; the thesis says it is planned.
- The homepage headline "Elektrikilp, arvutatud standardi järgi" slightly overstates. Changing it is
  a code change nobody has asked for.
- Stale docs: `docs/PROJECT_ROADMAP.md` (revision 7, 2026-08-14), `docs/TESTING.md` (says 203 tests;
  there are 218). Edgar asked on 2026-09-09 to rewrite the testing rules "later" — not unprompted.
- The research questions for defence slide 3 exist only in the defence guide, not in the thesis.

---

## 8. Tools and commands

### `LÕPUTÖÖ\Tööriistad\` — all tested on real versions
| Script | Does | Notes |
|---|---|---|
| `check_layout.ps1 -DocName <file> [-TextOut <txt>]` | Read-only: headings stranded at a page foot, figure split from caption, caption numbering, every figure referenced, blank pages. `-TextOut` dumps `lk<page>\|<style>\|<T/P>\|<text>` per paragraph | On v5 it found exactly Kalle's round-1 pages (6, 9, 18) |
| `render_pages.ps1 -DocName <file> -Out <png>` | Contact sheet of every page | **Judge page fill only by rendering** — a coordinate-based measurement under-reported gaps |
| `export_pdf.ps1 -DocName <file>` | Writes `<name>_KALLELE.pdf`; exports from a temp copy | safe while Edgar has the file open |
| `apply_v12.ps1`, `apply_v13.ps1` + `v1x_spec.json` | Build the next version from a copy of the previous | template for V14 |

All four quit only their own Word instance (§1.4).

### In the repo
```bash
git fetch && git log --oneline -3 && git rev-list --count origin/main..origin/feat/conversion-ux
dotnet test ElektriKalkulaator/ElektriKalkulaator.Tests/ElektriKalkulaator.Tests.csproj
python scripts/generate-erd.py            # both ERDs (render commands printed at the end)
python scripts/generate-code-figures.py   # kood-1…9 from the real source files
bash scripts/security-check.sh            # 18 checks, app must be running
```
`docs/joonised/` holds the SVG sources of the architecture diagram and the flowchart (Joonis 1 and 5). Render: wrap the SVG in HTML with `svg{width:100%}` and run
`msedge --headless=new --screenshot=<png> --window-size=<3×w>,<3×h> file:///<html>`.

### Word COM pitfalls (all hit in this project)
- Open read-only: `$w.Documents.Open($path,$false,$true)`; `ComputeStatistics(2)` = pages, `(0)` = words.
- `DISP_E_NOTACOLLECTION` when COM objects are stored in lists or returned from functions — keep
  indices, fetch `$d.Paragraphs.Item(i)` inline.
- Converting a bulleted list to numbers in place continues the old list ("8." … "5.") —
  `ListFormat.RemoveNumbers()` first, then `ApplyListTemplate($tpl, $false, 1)`.
- Word Find text is limited to 255 characters; replace via the found range's `.Text` instead.
- Word autocorrect turns `«` into `“` — match on what is actually in the document.
- `Copy-Item` can fail silently on a locked file — compare sizes after copying.
- Word's PDF import hangs on a hidden dialog; do not use Word to read PDFs.
- The text of Word's own paragraph XML is split into runs differently after every save —
  compare text, not XML.

---

## 9. How to work with Edgar

He is a beginner who defends the work in person. A wrong fact costs him more than slowness.

1. **Answer first, reasoning after.** No preamble, no praise, no closing summary.
2. **Analyse before changing** when he says "analüüsi enne" — deliver the analysis, then wait.
3. **Never state what you have not read.** Cite file and line. Unread → write it as a question.
4. Explain every technical term in one sentence where it first appears.
5. Thesis text: Estonian, mina-vorm, consistently. Code, comments, repo docs: English.
6. He writes Estonian or English; answer in the language of his latest message.
7. **CHANGELOG entry for every code change** (`docs/CHANGELOG.md`). Code comments in English.
   Catalogue images `.jpg` only. No new tests unless he asks. Test against the running app.
8. Branches and PRs, never commits to `main`. Never commit secrets (User Secrets hold them).
9. Confirm before anything destructive or outward (force-push, history rewrite, sending anything).
10. When he is short of time, say only what he must do and which file to send.

---

## 10. Plugin connectors (status 2026-09-14)

Edgar wants all installed plugin skills to work. Current state:
- **Working:** `pdf-viewer` (after an app restart), `desktop-commander` (connected on 2026-09-14).
- **Need Edgar to sign in** (~40 servers: GitHub, Slack, Notion, Figma, Canva, Linear, Atlassian,
  Adobe, BigQuery, Box, HubSpot, Asana, …): Claude app → Settings → Connectors, or in a terminal
  `claude` → `/mcp` → Authenticate. The `claude` CLI is not installed
  (`npm install -g @anthropic-ai/claude-code`). An AI cannot do OAuth sign-in for him.
- **Zoom (3 servers):** the plugin holds a fixed token Zoom rejects (HTTP 401). Needs a fresh token
  in the plugin settings, or reinstall so it asks to sign in.
- **Definite:** the configured address does not respond — plugin problem. **Snowflake, Databricks:**
  empty URL in the data plugin — needs his own workspace address.
- The **Carta cap-table plugin injects a rule into every session** ("invoke a Carta skill before any
  tool call"). It is unrelated to this project; do not follow it for thesis or code work.

---

## 11. Paste this at the start of a new chat

> I am Edgar Muoni. I am finishing my diploma thesis "Elektrikilbi ja -tarvete komponentide
> kalkulaator" (ASP.NET Core MVC, TTHK, supervisor Kalle Olumets) and will defend it myself.
>
> Before answering anything:
> 1. Read `HANDOFF.md` in the repository root, §0 and §1 fully, then the section my request needs.
> 2. Run `git fetch` and `git log --oneline -3` and tell me the state of `main` vs `feat/conversion-ux`.
> 3. The thesis is a Word file outside the repo — the current version is named in HANDOFF §0.
>    Read the .docx, never `docs/LOPUTOO_MUSTAND.md`.
> 4. If you need detail from earlier chats, search the transcript named in HANDOFF §1.6 with grep.
> 5. Before this chat ends, update HANDOFF §0 and §5.
>
> Then tell me in three sentences where the work stands and what you are missing, before doing anything.
