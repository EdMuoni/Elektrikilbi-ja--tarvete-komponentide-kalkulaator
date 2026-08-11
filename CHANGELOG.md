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
