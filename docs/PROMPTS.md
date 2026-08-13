# Prompt notebook — ElektriKalkulaator

Ready-made prompts for future AI sessions on this project, with an explanation of *why* each one
is written the way it is. Copy a prompt, paste it, adjust the details in `<angle brackets>`.

---

## How to get good results — the short version

* **Start every new session with the orientation prompt (§1).** It costs almost nothing and stops
  the model rediscovering the project from scratch.
* **One task per prompt.** "Add sorting and fix VAT and redesign the homepage" produces three
  half-finished things. Ask for one, review it, then ask for the next.
* **Say how you want it verified.** Without this you often get "it builds" as the entire proof.
  With it you get evidence.
* **Ask for the reasoning, not just the code.** "Explain why" turns a black box into something you
  can defend in front of the committee.
* **Say when you do NOT want something built.** Models tend to build. "Investigate and report
  back, do not change any code yet" is a legitimate and useful instruction.
* **Ask what it is unsure about.** Models sound equally confident whether they are certain or
  guessing. Asking directly is the cheapest way to find the soft spots.

---

## 1. Starting a new session

> Read `CLAUDE.md` and `PROJECT_ROADMAP.md`, then give me a three-sentence summary of where the
> project stands and what the next planned task is. Do not change any code yet.

**Why:** `CLAUDE.md` loads automatically, but explicitly asking for a summary proves the model has
actually understood the state rather than pattern-matching on the file names. The "do not change
code yet" stops it charging into the first thing it sees.

---

## 2. Asking for a feature or fix

> Implement `<the task>`.
>
> Work in small steps and test each one before moving on. Comment the code so a beginner
> programmer can follow the reasoning, in English. When you are done: add a `CHANGELOG.md` entry,
> tick the matching checkbox in `PROJECT_ROADMAP.md`, and tell me what you changed, how you
> verified it, and anything you are unsure about.

**Why:** this single prompt carries every standing rule. The last sentence is the important part —
asking for uncertainty up front surfaces problems while they are still cheap.

---

## 3. Verifying something actually works

> Start the application and test `<the feature>` against the real running app, not just the
> compiler. Show me the actual requests and responses. If you cannot verify something, say so
> rather than assuming it works.

**Why:** "Build succeeded" means the code compiles, not that it does the right thing. Several real
bugs in this project passed a clean build. Demanding observed evidence is what catches them.

---

## 4. Security review

> Review `<area>` for security problems. For each finding, reproduce it against the running
> application and show me the evidence, rate its severity, and explain the fix. Do not fix anything
> yet — report first.

**Why:** separating "find" from "fix" lets you decide what is worth doing. It also prevents a long
unreviewable change. This is exactly how the ten findings of 2026-08-11 were produced.

---

## 5. After the model claims something is done

> Re-read what you just changed and review it as if someone else had written it. Are there bugs,
> weak logic, inconsistencies with the rest of the codebase, or things you did not test?

**Why:** reviewing with fresh eyes reliably surfaces problems the model missed while writing. In
this project that exact prompt caught an upload endpoint that accepted any file type.

---

## 6. Design and UX work

> Compare our `<page>` against real Estonian competitors and published usability research
> (Baymard, Nielsen Norman Group). Recommend concrete changes ranked by impact ÷ effort, and cite
> your sources. Distinguish legitimate persuasion from manipulative dark patterns — I do not want
> fake scarcity or countdown timers.

**Why:** the last sentence matters. Asked for "conversion optimisation", models will happily
suggest fake urgency, which is regulated as an unfair commercial practice in the EU and would
undermine a project whose selling point is trustworthiness.

---

## 7. Writing thesis text

> Using `PROJECT_ROADMAP.md`, `CHANGELOG.md` and `RESEARCH_LOG.md`, draft the `<section>` section
> of my thesis in Estonian. Base every claim on what is actually in those documents. Where
> something is uncertain or unverified, mark it clearly instead of writing around it.

**Why:** grounding the text in the project's own records stops invented detail. Marking gaps means
you can see what still needs your input rather than discovering it during the defence.

---

## 8. When something is broken

> `<Describe what you did and what happened.>` Find the root cause before proposing a fix, and
> tell me whether this is a real bug in the application or a problem with how it was tested.

**Why:** the second sentence is worth the whole prompt. Several apparent failures in this project
turned out to be flaws in the test commands (Git Bash rewriting paths, a missing form field).
Fixing working code because a test lied is a genuinely bad outcome.

---

## 9. Before finishing a session

> Summarise what changed this session. Make sure `CHANGELOG.md` and `PROJECT_ROADMAP.md` are up to
> date, all tests pass, no test data is left in the database, and nothing is uncommitted. List
> anything left unfinished.

**Why:** the conversation is forgotten; the files are not. This is what makes the next session
start strong instead of confused.

---

## 10. Preparing for the defence

> Based on the project documents, what are the five questions a committee is most likely to ask,
> and what is the honest answer to each? Include the ones that are uncomfortable.

**Why:** "include the uncomfortable ones" prevents a reassuring, useless answer. Better to meet the
awkward question at your desk than in the room.

---

## Prompts to avoid, and what to say instead

| Instead of | Say |
|---|---|
| "Make it better" | "Improve `<specific thing>` so that `<specific outcome>`" |
| "Fix all the bugs" | "Review `<file>` and report bugs with evidence. Do not fix yet." |
| "Is this good?" | "What is wrong with this, and what would you change first?" |
| "Add tests" | "Add tests covering `<behaviours>`, then prove they work by deliberately breaking the code and showing the test fails" |
| "Make it look professional" | "Compare against `<real sites>` and apply the three highest-impact differences" |

The pattern: **specific target, specific outcome, specific proof.**

---

## Standing context worth pasting when a model seems lost

> This is a diploma thesis, not a commercial product. Scope decisions favour "correct and
> explainable" over "feature-complete". The calculator is the core; the shop exists to serve it.
> Electrical calculations follow EVS-HD 60364 and must never be guessed at — if a standard
> reference is unknown, leave it blank and tell me.
