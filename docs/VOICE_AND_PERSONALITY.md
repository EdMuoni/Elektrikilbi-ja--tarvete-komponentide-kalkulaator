# Voice and personality

How to make this site **memorable and warm** without making it slower or harder to use.

---

## First, an apparent contradiction — and its answer

`DESIGN_GUIDE.md` concluded that plainness wins: your users are looking something up, not shopping,
and McMaster-Carr is the reference precisely because it strips everything away.

This document asks for the opposite: weird, warm, human.

**Both are right, because they apply to different layers.**

| Layer | Rule |
|---|---|
| **Structure, speed, information** | Ruthlessly plain. Never sacrifice these. |
| **Language, tone, small moments** | This is where personality lives — and it costs nothing. |

Warmth is not animation. It is not a mascot, a gradient, or a cursor trail. **Warmth is what your
software says to a person, and when.** A page can load in 200 ms, contain zero decoration, and
still feel like it was made by a human being who respected the reader.

Every technique below is **words, timing and honesty** — not weight.

---

## The core idea: your weirdness already exists

Most sites bolt personality on because they have nothing distinctive underneath — hence mascots on
insurance sites. You do not have that problem.

**This project's genuine oddity is that it shows its working.** It is a shop that tells you *why*
each item is in your basket, cites a national safety standard, and can be audited line by line.
Nobody expects that from a web shop. It is unusual and it is true.

Everything below is about **saying that out loud** in a human voice, instead of hiding it behind
neutral commercial language.

The trap to avoid: manufactured quirk. A site that jokes about breakers while giving vague numbers
is worse than a plain one. **Your personality has to be your actual character, amplified.** For this
project that character is: *precise, honest, unpretentious, quietly proud of getting it right.*

---

## Part 1 — Voice

### The one rule

> Write as a **good electrician explaining something to a colleague** — not as a company
> addressing a user.

Knowledgeable, direct, no showing off, no salesmanship. Comfortable saying "this is an estimate,
not a design."

### What that changes, concretely

Current copy is *fine*. It is also anonymous — it could belong to any shop. Small rewrites, no
layout change:

| Where | Now | Warmer |
|---|---|---|
| Calculator intro | "Täida hoone andmed" (*Fill in building data*) | **"Räägi meile hoonest — ülejäänu arvutame ise."** (*Tell us about the building — we'll work out the rest.*) |
| Submit button | "Arvuta komponendid" (*Calculate components*) | **"Näita, mida vaja läheb"** (*Show me what's needed*) |
| Results heading | "Komponentide nimekiri" (*Component list*) | **"Selle hoone jaoks läheb vaja:"** (*For this building you'll need:*) |
| Empty cart | "Ostukorv on tühi" (*Cart is empty*) | **"Ostukorv on veel tühi. Alusta kalkulaatorist — see paneb nimekirja kokku sinu eest."** |
| Stock | "Laos: 150 tk" | **"Laos 150 tk — jätkub"** (*enough*) / **"Ainult 6 tk — kontrolli enne suurt tellimust"** |

Note the last one. It says *"check before a big order"* — actively unhelpful to a sales funnel and
exactly what a colleague would say. **That is the whole technique.**

### Words to remove

| Avoid | Because |
|---|---|
| "Meie innovaatiline lahendus" | Nobody describes their own work as innovative except in marketing |
| "Kliendikogemus", "sünergia" | Corporate filler nobody says aloud |
| "Lihtsalt kliki siia!" | Exclamation marks in a tool for professionals read as nervous |
| "Osta kohe!" | You are not selling impulse items. Nobody impulse-buys a 32 A breaker. |

---

## Part 2 — Warmth lives at moments of friction

Anyone can be pleasant on a working page. Personality shows when something **goes wrong, is empty,
or takes time** — which is exactly where most sites are coldest.

### Errors

An error message should do three things: say what happened, say why, say what to do. Blame the
system, never the person.

| Now | Warmer |
|---|---|
| "Sobimatu failitüüp." | **"See ei paista olevat pildifail. Lubatud on .jpg, .png, .gif ja .webp — proovi uuesti."** |
| "Vale e-post või parool." | **"Ei õnnestunud sisse logida. Kontrolli e-posti ja parooli."** (still vague on purpose — see below) |
| "Kogus peab olema vahemikus 1–999." | **"Kogus peab olema 1 ja 999 vahel. Suurema koguse jaoks võta meiega ühendust."** |

**One place where being unhelpful is correct:** the login error stays vague. Saying "no such user"
would let someone discover which email addresses have accounts. Warmth never overrides safety —
but the *tone* can still be kind.

### Empty states

An empty state is someone at a dead end. Give them the next step, not a shrug.
Already done for the catalogue; apply the same to cart, history and search.

### Waiting

The calculator is fast, so no spinner is needed. If a step ever becomes slow (a supplier price
lookup — see `SUPPLIER_SYNC_SPEC.md`), say what is happening: **"Küsime tarnijalt hetkehinda…"**
(*Asking the supplier for the current price…*), not "Laadin…".

---

## Part 3 — The weird bits worth doing

Ranked by effect ÷ risk. All are copy or tiny markup — none costs performance.

### 1. Show the reasoning inline *(the strongest, and unique to you)*

Under each BOM line, one plain sentence saying **why it is there**:

> `2 × B10 kaitselüliti` — *12 valgustit, 8 valgusti ahela kohta → 2 ahelat*
> `40 m kaablit` — *1 ahel × 5 tuba × 8 m*

Nobody else does this, because nobody else can. It converts a price list into a **teaching tool**,
and it is the single most persuasive thing on the page: it proves the number is not invented.

### 2. Be honest about being an estimate

> **"See on hinnang, mitte projekt."** *(This is an estimate, not a design.)*
> Kogused arvutatud EVS-HD 60364 järgi. Enne paigaldust laseb pädev elektrik selle üle vaadata.

Admitting a limit **increases** trust. It also happens to be true and protects you.

### 3. Sign the work

One line in the footer: *"Tehtud Tallinnas. Küsimused: <email>."* A real place and a real person.
Costs nothing; instantly separates you from a template.

### 4. Round numbers honestly

When cable comes out at 40 m, say **"40 m (ostame 50 m rulli)"** — real cable is sold in rolls.
Showing you know that is more convincing than any badge.

### 5. Name things the way tradespeople do

Use the words on the van, not the words in the database. "Kaitselüliti" not "kaitseseade". If
electricians say "automaat", say that too.

### Do NOT do

| | Why |
|---|---|
| Mascot / cartoon electrician | Reads as unserious for safety-relevant work |
| Jokes about electrocution | The audience knows someone it happened to |
| Animated counters on prices | Slows the one number people came for |
| "Hey there! 👋" | Not how anyone speaks Estonian, or speaks at work |
| Personality **instead of** precision | Fatal. Charm never substitutes for a correct number. |

---

## Part 4 — International references, and what to take

| Site | The weird thing | Take | Leave |
|---|---|---|---|
| **Oatly** | Packaging argues with itself; copy admits doubts | Radical honesty as a brand trait | The chaos — you are safety equipment |
| **McMaster-Carr** | Almost aggressively plain, and beloved for it | Speed and predictability as kindness | Nothing — this is your structural model |
| **Basecamp / 37signals** | Opinionated; tells you what *not* to buy | Saying "you don't need this" | The manifesto tone |
| **Patagonia** | "Don't buy this jacket" | Restraint reads as trustworthy | Activism unrelated to your domain |
| **Duolingo** | Enormous personality in microcopy | Character in errors and empty states | The mascot and the guilt-tripping |
| **Stripe docs** | Warmth through *clarity* — a worked example everywhere | Explain by showing | Their scale of design investment |

**The pattern across all of them:** none is warm because of decoration. Each is warm because it
**tells the truth in a recognisable human voice**, including truths that cost it something.

---

## Part 5 — Doing it

Cheapest and highest-effect first. All are copy edits unless noted.

| # | Change | Effort |
|---|---|---|
| 1 | Rewrite the five strings in Part 1's table | 30 min |
| 2 | Rewrite error messages in Part 2 | 30 min |
| 3 | "This is an estimate, not a design" under the BOM | 15 min |
| 4 | Footer signature with a real contact | 10 min |
| 5 | **Reasoning line under each BOM row** *(needs the rule data passed to the view)* | 2 h |
| 6 | Cable rounded to purchasable rolls | 1 h |
| 7 | Stock wording that warns instead of upselling | 20 min |

### How to tell it worked

- **Read it aloud.** Anything you would not say to a colleague, rewrite.
- **Cover the logo.** Could this be any other shop? If yes, there is no personality yet.
- **Find the friction.** Trigger every error and empty state. Cold ones are your to-do list.
- **Ask one electrician** whether it sounds like someone who knows the trade, or like a website.

---

## The line that matters

> Be weird in **what you say**, never in **how it works**.

Navigation, speed and numbers stay boring and predictable. The voice around them can be
unmistakably yours — and for this project, the most memorable thing available is also the most
honest: *we show our working, and we will tell you when we are not sure.*
