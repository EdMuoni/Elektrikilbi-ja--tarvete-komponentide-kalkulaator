# Image credits and licences

Every image shipped with this project, where it came from, and under what licence. **CC BY-SA
images legally require attribution**, so this file is not optional documentation — it is the
attribution itself, and it must ship with the project.

All product images are stored as **`.jpg`** in `ElektriKalkulaator/wwwroot/images/products/`.
One format is used across the whole catalogue for consistency.

## The two routes, and where each one stands (checked 2026-09-09)

Edgar asked for manufacturer photography **and** free stock photography. They are at different
stages, so this section records both honestly.

### Route 1 — manufacturer media kits: NOT AVAILABLE YET

ABB, Schneider Electric and Draka/Prysmian all publish product imagery, and manufacturers
generally **want** their parts pictured by the people selling them. But their image libraries sit
behind a **distributor or reseller agreement**, not a public licence. A search of their public
material found distributor portals and contact routes, and no published terms that permit a third
party to redistribute product photography.

So this route is not blocked by principle — it is blocked by not having the agreement yet. What it
needs, in order:

1. Approach one supplier. `SUPPLIER_SYNC_SPEC.md` recommends **Onninen** first, because they are
   already B2B-oriented and reseller-friendly.
2. Ask specifically for a **reseller/dealer agreement including product media**. Media rights are
   normally granted as part of it rather than separately.
3. The same agreement is what unlocks phases 5–7 of `SUPPLIER_SYNC_SPEC.md` — real prices, real
   stock data, and eventually automated ordering. **The photographs are a side benefit of the
   agreement the shop needs anyway**, which is why it is worth doing properly rather than scraping.

Until that exists, using their photographs would be a copyright violation in a document that is
going to be publicly defended and archived.

**iStock and similar paid libraries are not a shortcut.** Those images are licensed per-use and
sold; the previews are watermarked. They are the one case where the question is not arguable.

### Route 2 — free-licence photography: IN USE NOW

Everything currently shipped comes from Wikimedia Commons, Unsplash and Pexels, all under licences
that permit commercial use. Details in the tables below.

---

## Why not manufacturer photos?

ABB, Schneider Electric and Hager product photography is **copyrighted**. A real distributor gets
permission or a media kit from the manufacturer; a student project has no such agreement. Using
those images would be a copyright violation in a document that is going to be publicly defended
and archived.

Instead, every image below is a genuine photograph of the **correct component type**, published
under a free licence. They are not photos of the exact SKU in the catalogue row. This is normal
retail practice — Estonian e-shops routinely mark catalogue photos as illustrative — and the UI
labels them accordingly.

## Product images

| File | Depicts | Source | Author | Licence |
|---|---|---|---|---|
| `breaker.jpg` | Two single-pole DIN-rail miniature circuit breakers (Chint NB1-63, C2 and B13). Matches the 1-pole form factor of the seeded S201 breakers. | [Chint circuit breakers.JPG](https://commons.wikimedia.org/wiki/File:Chint_circuit_breakers.JPG) | Wikimedia Commons contributor | CC BY-SA 3.0 |
| `cable.jpg` | NYM-J installation cable, including a length printed "NYM 3x2,5" — the exact cable type in the catalogue. | [NYM cable and clones.JPG](https://commons.wikimedia.org/wiki/File:NYM_cable_and_clones.JPG) | Wikimedia Commons contributor | CC0 (public domain dedication) |
| `rcd.jpg` | Two-pole residual current device, 16 A / 0.03 A, on a dark background. | [Residual current device (RCD).jpg](https://commons.wikimedia.org/wiki/File:Residual_current_device_(RCD).jpg) | Wikimedia Commons contributor | CC0 (public domain dedication) |
| `enclosure.jpg` | Installed consumer unit populated with ABB 16 A breakers and RCDs. | [ABB 230V 16A fuses in fuse box in German shop, 2024.jpg](https://commons.wikimedia.org/wiki/File:ABB_230V_16A_fuses_in_fuse_box_in_German_shop,_2024.jpg) | Wikimedia Commons contributor | CC0 (public domain dedication) |

## Hero / decorative images

Stored in `ElektriKalkulaator/wwwroot/images/hero/`.

| File | Depicts | Source | Licence |
|---|---|---|---|
| `electrician.jpg` | Electrician in a hard hat working on a wall-mounted electrical box. | [Unsplash](https://unsplash.com) | Unsplash Licence — free for commercial and non-commercial use, no permission needed, attribution appreciated but not required. |
| `panel.jpg` | Close-up of a consumer unit with breakers, RCDs and coloured wiring. | [Pexels](https://www.pexels.com) | Pexels Licence — free to use, attribution not required. |

## Before publishing this project publicly

- [ ] Verify each Commons file page still shows the licence recorded above (licences can be
      corrected or files re-tagged after upload).
- [ ] For the CC BY-SA image (`breaker.jpg`), keep this attribution visible or linked from the
      site itself, not only in the repository — that is what "BY" requires.
- [ ] If real manufacturer photos are ever wanted, obtain them through the reseller agreement
      described above and in `SUPPLIER_SYNC_SPEC.md` — that agreement carries distribution rights
      for the imagery as well as for the price and stock data.

## A presentation note that matters more than the photographs

Product photography is shot on **white**. Placing those cut-outs directly on a dark surface makes
each one read as a glowing rectangle, because the photo carries its own background with it.

The fix is not a better photograph — it is the `--product-tile` token, which is deliberately
**white in both themes**. The photo sits on a white tile whatever the page around it is doing,
because the tile belongs to the product rather than to the theme. Every trade catalogue in
`RESEARCH_LOG.md` does exactly this.
