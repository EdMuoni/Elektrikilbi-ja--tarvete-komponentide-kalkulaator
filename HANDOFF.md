# HANDOFF — read this before doing anything else

**Version 6** · updated **2026-09-12** · version 5 was written 2026-09-11
**Project:** Elektrikilbi ja -tarvete komponentide kalkulaator (LÕPUTÖÖ)
**For:** the next AI model, and for Edgar when he opens a new chat.

This file lives in the **repository root** so that anything which clones the repo can see it.
Version 5 lived in `LÕPUTÖÖ/Lõputöö_Dokumendi_variandid/`, outside the repo, where no clone
could reach it. That copy is superseded by this one.

**Update section 4 (the ledger) and section 5 (Kalle's comments) whenever a fact changes.**
That is the whole point of the file. A fact that lives only in a chat window is gone the
moment the chat ends.

---

## 0. STOP — four facts that will make you wrong if you skip them

**0.1 — The GitHub repository is NOT the current project.**

| | GitHub (what a clone sees) | Edgar's desktop (the truth) |
|---|---|---|
| Commits | **10**, HEAD `46ebffa`, 2026-08-11 | **49**, HEAD `bda9089`, 2026-09-12 |
| `TESTING.md`, `DESIGN_GUIDE.md`, `EVS_ALLIKAD.md`, `KOODI_SELGITUS.md` | absent | exist |
| ~218 automated tests, GitHub Actions CI, authentication | not present | exist |

Before making ANY claim about the code, run `git log --oneline -3` and compare the top commit
date with today. If the gap is more than a few days, say so and ask Edgar to push. This has
been the single costliest mistake on this project.

**0.2 — The thesis is a Word document, not a file in this repo.**
The current thesis is `LÕPUTÖÖ/Lõputöö_Dokumendi_variandid/Elektrikilbi_v6_KALLE.docx` (v5 plus
Kalle's review corrections; v5 is kept untouched as the fallback).
**Never draft thesis text before reading the actual document.** It already contains most of
what you would be tempted to write, and your draft will contradict it. This happened on
2026-09-11 and the draft was thrown away.

**0.3 — `docs/LOPUTOO_MUSTAND.md` is NOT the thesis any more.**
It is a stale markdown draft from before 2026-09-10: it still contains the LISAD section
(29 references), lacks chapter 2.6, and lacks the chapter-3 sections added on 2026-09-11.
`CLAUDE.md` used to describe it as "the thesis itself"; that row was corrected on 2026-09-12
to say so. **Read the .docx, never the markdown.**

**0.4 — There is no memory between sessions.**
Chat history does not travel between Claude Desktop, Claude Code, and any new chat. This
repository and its `.md` files are the only channel.

---

## 1. The documents and how they relate

All under `C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\`.

| Document | What it is | Status |
|---|---|---|
| `LÕPUTÖÖ_TEMPLATE (1).docx` | **The school's official template.** Its rules live in six instruction IMAGES (`word/media/image1–6.png`), not in its text | Reference only — never edit |
| `Lõputöö kavand_VORM.docx` | **The submitted proposal.** The authority on what the work promised | Reference only — never edit |
| `Näited/Eksamitöö_Kalle_Olumets_Cyber_Plan (1).docx` | **The supervisor's own thesis**, the worked example | Reference only — never edit |
| `Lõputöö_Dokumendi_variandid/Elektrikilbi_v6_KALLE.docx` | **THE CURRENT THESIS.** v5 plus 15 of Kalle's 18 review comments — see §5. 35 pages, 5 897 words | **This is the one to edit** |
| `Lõputöö_Dokumendi_variandid/Elektrikilbi_v5.docx` | 35 pages, 5 885 words, mina-vorm. Superseded by v6 but kept as the fallback | Do not edit |
| `Elektrikilbi ja -tarvete komponentide kalkulaator_v2_UUS.docx` | Generator output, 36 pages. **Superseded** | Do not edit |
| `Pictures/Lõputöö_märkused/1–14.PNG` | **Kalle's review**, 18 comments as screenshots | The work list |

**Supervisor:** Kalle Olumets. **Edgar defends this work himself.**

### 1a. The generator is retired — read this before running it

`scripts/build-thesis-docx.js` produced `…_v2_UUS.docx`. v5 descends from that output (it
still carries the chapter-3 sections the generator added) but was then **hand-edited in
Word**: converted to mina-vorm, MÕISTED glossary moved, introduction rewritten around the
proposal's aim, 7 TÄITA notes removed, Joonis 10 replaced, 15 tables captioned.

**None of those edits exist in the generator.** Running it overwrites nothing by itself, but
treating its output as current silently discards all of them. The generator is kept for one
purpose only: regenerating figures. Use `scripts/generate-erd.py` for those, and edit the
.docx directly for text.

### 1b. Files an earlier handoff promised that DO NOT exist

Searched the whole `Desktop\TARge24` tree on 2026-09-12:

| File | Status |
|---|---|
| `Elektrikilbi_v5_LOPLIK.docx` | renamed to `Elektrikilbi_v5.docx` |
| `TEHA_NIMEKIRI.md` — Edgar's 7 TÄITA items | **gone** |
| `loputoo_tekstid.md` — **drafted AI-usage text** | **gone** |
| `joonis10.png` / `joonis10.html` | **gone** (the PNG is embedded in the .docx and is recoverable from `word/media/`) |

The lost AI-usage draft is the text Kalle now explicitly asks for (§5, comment 18). It has
to be written again.

---

## 2. Project identity

| | |
|---|---|
| Thesis title | Elektrikilbi ja -tarvete komponentide kalkulaator |
| Type | LÕPUTÖÖ, noorem tarkvaraarendaja (TAR), EKR tase 4 |
| Author | Edgar Muoni · Institution: Tallinna Tööstushariduskeskus (TTHK), group TARge24 |
| Supervisor | Kalle Olumets |
| Repo | `EdMuoni/Elektrikilbi-ja--tarvete-komponentide-kalkulaator` |
| Stack | ASP.NET Core 9 MVC (C#), EF Core 9, SQL Server, xUnit, Bootstrap 5 |
| Solution | `.Core` ← `.Data` ← `.ApplicationServices` ← web · plus `.Tests` referencing all four |
| What it does | User enters building parameters → app returns a priced component list (BOM) derived from **EVS-HD 60364**, showing the working, not just the total |
| Language rules | Code, comments, repo docs: **English**. Thesis: **Estonian, mina-vorm** |

---

## 3. School formatting rules — read from the template's own instruction images

**Read-level** facts extracted from images inside the template. They override anything an AI
model remembers about "how theses are usually written".

**Person form — Pilt 2 "Keel ja stiil":**
> "Tekstis kasutatakse **kogu tööd läbivalt** kas **umbisikulist** (töös on uuritud,
> mõõtmiseks kasutati) või **mina-vormi** (töös uurisin, mõõtmiseks kasutasin)."

→ **Mina-vorm is permitted.** The binding constraint is *kogu tööd läbivalt* — consistently
throughout. Mixing the two is what breaks the rule. The thesis uses mina-vorm.

**Citation — Pilt 3 "Viitamine ja allikad":**
- All positions taken from other authors must be referenced, **including tables, figures and
  photographs**.
- **"Viitamata tekst loetakse autori loominguks"** — unreferenced text counts as the author's own.
- Prefer literature from the last five years.

**Other rules read from the template:**
- Theory section **20–30 pages** plus appendices. The project is a **156-hour** project.
- `SISSEJUHATUS` is **not numbered** and must not contain results or conclusions.
- Reference list: alphabetical, not numbered, initials only, year in round brackets.
- Internet sources: access date; publication date not the footer copyright date; `n.d.` if undatable.
- **LISA A–E in the template are NOT content to copy.** They are the e-CF competency table the
  student is assessed against, the assessment criteria, and a referencing guide. Kalle's own
  thesis has no appendices; his contents end at "5. KASUTATUD ALLIKAD". Settled 2026-09-10.

---

## 3a. What the submitted proposal promised

From `Lõputöö kavand_VORM.docx`. The thesis must not drift from it.

- **Teema:** "Koostada kalkulaator, mis abistaks **klientidele** elektritarvikute ja
  kilbikomponentide koostamist **korterelamutes ja ärihoonetes**."
- **Väärtus:** "…**eemaldades vajadust kasutada eelarvestaja teenust**."
- **Planned technology:** MSQL, C#, .NET, HTML, JavaScript, **Python**.

**Two mismatches still unresolved as of 2026-09-12:**
1. The proposal names only *korterelamu* and *ärihoone*; the app also supports *eramu*.
   A defensible expansion — state it in one sentence rather than be caught out.
2. The proposal lists **Python**; the solution contains none. Explain the change or drop it.

---

## 4. LEDGER — as of 2026-09-12

### Confirmed (claim → evidence)

| Claim | Evidence |
|---|---|
| Desktop repo: **49 commits**, HEAD `bda9089`, working tree clean | `git log`, `git rev-list --count HEAD` |
| GitHub `origin/main` still at `46ebffa` — **desktop is 49 ahead, unpushed** | `git log --oneline origin/main -1` |
| `docs/CHANGELOG.md` holds **30 entries**: 23 "Claude (Sonnet 5) + Edgar", 7 "Claude (Opus 5) + Edgar" | `grep '^\*\*Author' docs/CHANGELOG.md \| sort \| uniq -c` |
| v5: 35 pages, 5 885 words, 10 figures, 16 tables, mina-vorm, 0 TÄITA notes | Word COM `ComputeStatistics` |
| v2_UUS: 36 pages, 5 970 words, umbisikuline, 2 TÄITA notes remaining | same |
| v5 descends from the generator — it contains "Kavandatud ja realiseeritud andmemudeli vahe" and "Soovitused sarnase töö tegijale" | full-text match in both files |
| **Reference list has SIX sources**, not seven: Bootstrap, EVS, Microsoft n.d.-a, Microsoft n.d.-b, OWASP, W3C | enumerated every "Kättesaadav aadressil" paragraph in both .docx |
| **No heading in v5 has `pageBreakBefore` or `keepWithNext`** — all report 0 | walked every bold numbered paragraph via Word COM |
| v5 has **10 figure captions and 0 in-text figure references** | counted paragraphs matching `^Joonis \d+\.` vs other "Joonis" mentions |
| `Joonis 1` (lk 14, 435×312 pt) is the generated ERD of the BUILT model | `scripts/generate-erd.py`, rendered 2026-09-11 |
| `Joonis 10` (lk 31, 435×320 pt) is the DESIGN-PHASE model, 12 tables at the same page width → type drops to ~3 pt | measured; box width falls 166 → ~62 pt |
| Joonis 10's cardinalities are **inferred from foreign keys**, not read from Edgar's design | earlier session's own caveat |
| Diagram sources now DO exist: `docs/joonised/` (2 SVG), `scripts/generate-erd.py`, `Pictures/Diagrams/` | filesystem |
| The standard's own Estonian title is "Madalpingelised **elektripaigaldised**" | `docs/EVS_ALLIKAD.md` line 25 |
| Calculator rules: lighting 1 circuit / 8 lights (1.5 mm², 10 A); sockets 1 / 6 (2.5 mm², 16 A); stove 1 dedicated (6 mm², 32 A) | `CalculatorServices.Calculate()` |
| Cable quantity is a **declared heuristic, not a standard**: 8 m per room per circuit | `CalculatorServices.cs`, "Rough wire estimate" |
| The BOM picks the **cheapest in-stock** product matching each rule, plus one enclosure | `CalculatorServices.Calculate()` |
| Appendices are NOT required — removed 2026-09-10 | template's LISA A–E are assessment material; Kalle's thesis has none |
| No OWASP ZAP or equivalent scanner has ever been run | `docs/TESTING.md` Part 7 lists what is missing; no scanner appears anywhere |

### Open (question → the exact thing that closes it)

| Question | How to close it |
|---|---|
| **How long should the AI-usage section be?** Short honest paragraph, or a full methodology subsection | Edgar's decision. Asked 2026-09-12, not yet answered |
| Exact current test count. Docs say 203; attribute count is 135 `[Fact]` + 22 `[Theory]` + 61 `[InlineData]` + MemberData rows | Run `dotnet test` and read the summary line |
| Does Edgar have EVS-HD 60364 access yet? `CalculationRule.EvsReference` is still empty | Edgar asks the school library; A1:2025 amendment alone is €12.40 |
| Should the document use the template's own styles (`Pealkiri1/2/3`) instead of Word's built-in `Heading1/2/3`? | Ask Kalle Olumets |
| The thesis says **44 commits**; the repo now has **49** | Either update the number or state a cut-off date ("seisuga 10.09.2026") |
| Python appears in the proposal but nowhere in the solution (§3a) | One sentence, Edgar's wording |
| The proposal names only korterelamu + ärihoone; the app also does eramu (§3a) | Same |
| Is a foreign-language resümee required? | The school's "Kirjalike tööde koostamise juhend" (2023), never seen |

### Dead — ruled out, do not re-propose

| Killed idea | What killed it |
|---|---|
| "The pushed repo reflects the current project" | 49 vs 10 commits |
| "`docs/LOPUTOO_MUSTAND.md` is the thesis" | It predates 2026-09-10; the .docx is 6 revisions ahead |
| "The appendices are required by the template" | The template's LISA A–E are assessment material; Kalle's thesis has none |
| "The ugly ellipse ERD was produced by a Claude Code session" | It came from the pre-project React draft folder; verified by reading the file |
| "Five months of design, then a two-week build" | Contradicts the thesis's own four active months / 156 hours |
| "Changing the Word theme will fix the blue headings" | The colours were hard-coded hex in `styles.xml`; already fixed to black |
| Using an LLM at runtime to do the calculation | Edgar built it (Jan 2026 prototype, Gemini) and rejected it in writing 2026-02-03. **This is the project's strongest defence point.** Never suggest re-adding it |

---

## 5. Kalle Olumets's review — the active work list

Source: `Pictures/Lõputöö_märkused/1–14.PNG`, 18 comments left 2026-09-12 09:17–09:34 against
a copy of the thesis. **Kalle challenged no technical claim** — not the architecture, the
algorithm, the data model, the security analysis, the test results or the sources. 15 of 18
are formatting and Estonian usage.

Status key: ☐ not started · ◐ in progress · ☑ done

| # | Where | Kalle's comment | Verdict | Status |
|---|---|---|---|---|
| 1 | lk 6, "1. TEOREETILINE TAUST" | numbered chapters always start on a new page | Do | ☑ |
| 2 | lk 7 | elektrikomponentide → **elektri komponentide** | Do | ☑ |
| 3 | lk 8 | veebirünnete → **veebi rünnakute** | Do | ☑ |
| 4a | lk 9, Tabel 2 | paigaldist → **paigaldus** | **Push back.** The standard's own title is "Madalpingelised elektripaigaldised". *Paigaldis* = the installation; *paigaldus* = the act of installing. Edgar's call | ☐ |
| 4b | lk 9, Tabel 2 | valikutööriistad → **valiktööriistad** / valiku tööriistad | Do | ☑ |
| 5 | lk 9, "Teema olulisus" | orphan heading at page foot — start a new page | Do (same fix as 1) | ☑ |
| 6 | lk 10 | üheleherakendus → **üheleheline veebirakendus (SPA)** | Do, good point | ☑ |
| 7 | lk 10 | kompileerimisel → **koodi kompileerimisel** | Do | ☑ |
| 8 | lk 11 | versioonitud → versioniseeritud | **Rewrite the sentence instead** — both are clumsy | ☑ |
| 9 | lk 12, Tabel 4 | commit'id → kommitmendid **or** sisestused koodi repositooriumisse | **Take the second option.** "Kommitment" in Estonian reads as *pühendumus* | ☑ |
| 10 | lk 12 | neljakuulisest → **nelja kuulisest** | Do | ☑ |
| 11 | lk 12, "1.3." | new page | Do (same fix as 1) | ☑ |
| 12 | lk 13 | "Kaks teadlikku otsust väärivad selgitust." → **bold** | Do | ☑ |
| 13 | lk 14 | **refer to figures in the body text** ("nagu on näha Jooniselt 1") | **Do — real gap.** 10 captions, 0 references | ☐ |
| 14 | lk 15, "1.4." | new page | Do (same fix as 1) | ☑ |
| 15 | lk 16 | "Arendus jagunes kolme ossa…" → **numbered or bulleted list** | Do | ☑ |
| 16 | lk 18, "Turvalisus" | orphan heading — new line/page | Do (same fix as 1) | ☑ |
| 17 | lk 23, "Mis jäi tegemata" | if no OWASP ZAP or other security test was run, **write that it is planned** | **Do.** Kalle means OWASP **ZAP**. We have `security-check.sh` (18 checks) + integration tests, **no scanner** | ☐ |
| 18 | lk 25, §1.6 | **"Kindlasti pange kirja AI ja agentraamistike kasutus"** | **The most important comment in the set.** §1.6 says "Meeskond: Edgar Muoni (üksinda)" — incomplete | ☐ |

### Applied 2026-09-12 → `Elektrikilbi_v6_KALLE.docx`

15 of 18 done. Still open: **4a** (paigaldis — Edgar's call), **13** (figure references), **17** (security paragraph), **18** (AI section — scope not yet decided).

| Measured | v5 | v6 |
|---|---|---|
| Headings left stranded at the foot of a page | 10 | **1** |
| Numbered chapters starting at the top of a page | 0 of 5 | **5 of 5** |
| Language items from Kalle's list still present | 6 | **0** |
| Blank pages | none | none |
| Pages / words / figures / tables | 35 / 5 885 / 10 / 16 | 35 / 5 897 / 10 / 16 |

The one remaining stranded heading is "Kasutajaliidese kavandamine" (lk 14). `KeepWithNext` **is** set on it and no page break follows it — Word simply does not honour it there. Forcing `PageBreakBefore` would leave a visible gap on page 14, so it was left alone. Revisit if Kalle raises it.

Two things fixed that Kalle did not flag: **"ehitati" and "analüüsiti"** were impersonal forms surviving in a mina-vorm document, which breaks the template's *kogu tööd läbivalt* rule. Both now read "ehitasin" / "analüüsisin".

**Comments 1, 5, 11, 14 and 16 are one bug, not five:** no heading in the document carries
`pageBreakBefore` or `keepWithNext`. One pass over the heading styles closes all five.

**On comment 18 — the evidence, and the limit of it.** `docs/CHANGELOG.md` records 30 changes,
every one marked "Claude (…) + Edgar". That marker records **assisted** changes, not AI
authorship. Edgar has corrected an earlier session for describing his largest development
phase as "the AI one". Write what the record supports: he did the work, specific named tasks
had help. Do not overstate it, and do not understate it either — Kalle asked for it directly,
which means he already knows.

---

## 6. Next steps, in order

1. **Apply Kalle's comments** in the order in §5: page breaks → AI section → figure
   references → security paragraph → language fixes → Joonis 10. Work on a **new version
   file**, never on v5 itself — Edgar's standing instruction.
2. **Edgar pushes his desktop work.** `git push -u origin main`. Nothing a clone sees is
   reliable until this happens.
3. **Fix `CLAUDE.md`** — its table still calls `docs/LOPUTOO_MUSTAND.md` "the thesis itself".
4. **Refresh the stale docs**: `PROJECT_ROADMAP.md` (revision 7, 2026-08-14, says "4 commits"),
   `TESTING.md` (says 203 tests).
5. **Rework Joonis 10** — show only the six planned-but-unbuilt entities, not all twelve.
6. **Resolve the two proposal mismatches** in §3a.
7. **Ask Kalle** whether the document must use the template's own styles.

---

## 7. How to work with Edgar — these are not optional

He is a junior frontend developer and a self-described beginner, writing a diploma thesis he
will defend in person. A wrong fact costs him more than slowness does.

1. **Never state as fact anything you have not read.** Cite file and line.
2. **Never make a negative claim** ("there is no X") unless you have searched every file where
   it could live. A truncated extract is not a search.
3. **Three evidence tiers, visible in the wording.**
   **Read** — you read the lines → state it plainly.
   **Inferred** — follows from what you read but depends on something you did not → "this
   should mean X, if [assumption] holds", with the assumption named in the same sentence.
   **Unread** — you have not seen the deciding file → write it as a **question**, never a finding.
4. **Deliverables inherit their tier.** Thesis text, a PR comment, a ticket — the evidence
   level survives the reformatting.
5. **Put a numbered gaps list above every deliverable**: what is unconfirmed, what is missing,
   what he could not defend if questioned.
6. **Explain every technical term the first time it appears**, in one sentence, right where it
   appears — including ones that seem too basic. Give the searchable name ("this is crow's-foot
   notation", "this is a Word style, not direct formatting").
7. **Answer first, reasoning underneath.** No preamble. If the honest first line is "I can't
   tell without seeing X", that is the first line.
8. Never write "as you know", "simply", "just", "obviously". No emoji headers, no praise
   openers, no closing summaries.
9. **Correct yourself in the first line** when wrong, exactly as far as the new evidence
   reaches, and no further.
10. **Do not overstate the role of AI assistance in his work.** See §5, comment 18.
11. **No new tests unless he asks.** He stopped per-change test writing on 2026-09-09 —
    it costs time and tokens. Run the existing suite; verify against the running app.
12. **Always work on a new version file** of the thesis. Never edit the previous one in place.
13. Thesis text: **Estonian, mina-vorm**. Code, comments, repo docs: **English**.
14. **Never write that the cable figure is "arbitrary" (suvaline).** It is a declared
    simplifying assumption with a stated reason and a named fix, which is defensible.
    "Arbitrary" invites an examiner to discard the whole cost estimate.

---

## 8. Command reference

Run on **Edgar's desktop**, in the project folder — not in a cloud clone.

```bash
git log --oneline -3                     # top commit date vs today = how stale any clone is
git rev-list --count HEAD                # commit count, for the thesis's own number
git push -u origin main                  # publish so a clone can actually see it
grep -c '^\*\*Author' docs/CHANGELOG.md  # how many recorded changes
dotnet test ElektriKalkulaator/ElektriKalkulaator.Tests/ElektriKalkulaator.Tests.csproj
python scripts/generate-erd.py           # regenerate the ERD SVG, then render it with msedge
```

Reading a `.docx` without Word: it is a zip. `word/document.xml` is the text, `word/styles.xml`
the styles, `word/media/` the images. **The school template's rules live in its images**, not
its text — a model that only reads the text will miss all of them.

Word COM is available on this machine and is the reliable way to measure a .docx:
`$w = New-Object -ComObject Word.Application` → `$d.ComputeStatistics(2)` is pages,
`(0)` is words. Always open read-only (`$w.Documents.Open($path,$false,$true)`) and
`$d.Close(0)` so nothing is written back by accident.

---

## 9. Paste this at the start of a new chat

> I am Edgar, writing my diploma thesis "Elektrikilbi ja -tarvete komponentide kalkulaator"
> (ASP.NET Core MVC, C#, EF Core) at TTHK. I will be defending it myself.
>
> Before you answer anything:
>
> 1. Read `HANDOFF.md` in the repository root, top to bottom. Then `CLAUDE.md`.
> 2. Run `git log --oneline -3` and tell me the date of the top commit.
> 3. The thesis is a Word file on my desktop, not in the repo — read the .docx, never
>    `docs/LOPUTOO_MUSTAND.md`.
> 4. Follow the working rules in `HANDOFF.md` §7 — especially the three evidence tiers.
> 5. Before this chat ends, update `HANDOFF.md` §4 and §5.
>
> Then tell me what you have read and what you are still missing, before doing any work.

---

## 10. A warning about this file

Do not trust it blindly. Verify a file still exists before recommending changes to it, and
verify a date before quoting it. Version 5 of this file listed four deliverables that no
longer existed anywhere on disk (§1b) — including a drafted section the supervisor later
asked for.

When a fact in §4 changes, **edit §4**. When a comment in §5 is done, **tick it there**.
Do not append a correction elsewhere and leave the old claim standing.
