# Supplier sync — specification for later

**Status: NOT BUILT. This is a plan, not a description of the system.**
Nothing here exists in the code. It is written down now so the reasoning survives.

Post-thesis work. Do not start it before the defence.

---

## The model Edgar described

> List products sourced from other retailers. When a customer buys here, the system automatically
> places the order with the original seller. The customer pays **source price + 25 % + shipping**.
> Images, prices and product details update themselves.

This is dropshipping with automated fulfilment. The calculator remains the reason customers arrive;
the shop is how the site earns.

---

## Read this before writing any code

The technically interesting part — a scheduled job that refreshes prices — is the **easy** part.
Everything that will actually decide whether this works is commercial and legal.

### 1. Most retailers have no ordering API

Esvika, Onninen and Elektrikaubad (see `RESEARCH_LOG.md`) are **shops**, not platforms. There is no
documented way to place an order programmatically. The options are:

| Approach | Reality |
|---|---|
| Scraping their site and automating checkout | Brittle, breaks on any redesign, and **almost certainly against their terms of service**. Also indistinguishable from an attack: repeated automated logins and orders from one IP. |
| A negotiated reseller / dealer agreement | The normal way this is actually done. Wholesalers *want* resellers. You get real prices, real stock data, and often a genuine feed. |
| Manual relay (a human places the order) | Unglamorous, works on day one, costs nothing to build. |

**Recommended order: manual → agreement → automation.** Automating an unauthorised purchase flow is
the one path that can end the business rather than grow it.

### 2. Terms of service usually forbid exactly this

Retail terms commonly prohibit automated ordering, resale, and bulk data extraction. Breaching them
risks account termination and, if prices and photos are copied wholesale, a copyright claim on the
photography — the same reason `IMAGE_CREDITS.md` exists.

### 3. You become the seller of record

The moment a customer pays **you**, EU consumer law makes **you** responsible — not the upstream
shop:

- **14-day right of withdrawal**, even if the supplier will not take the item back
- **2-year conformity guarantee** on the goods
- **Clear delivery-time disclosure** before purchase
- A supplier's stock error becomes **your** late delivery and **your** refund

Budget for this. It is the single largest hidden cost of the model.

### 4. A 25 % markup is a decision, not a constant

25 % must cover: payment fees (~2–3 %), returns and refunds, currency and price drift between quote
and purchase, support time, and the platform itself. On a €1.20/m cable it is €0.30 — probably less
than the labour of handling one query about it.

**Therefore: store the markup as data, not as `* 1.25` in code.** It will need to vary by supplier,
by category, and by order size.

---

## What has to change in the data model

Today `Product` holds a single `Price` and a `StockQuantity`, and nothing records where either came
from. That is the core gap.

```
Supplier
  Id, Name, Website
  ContactEmail
  DefaultMarkupPercent        decimal   -- e.g. 25.00, overridable per product
  IsActive                    bool
  AgreementStatus             enum      -- None | Requested | Signed
                                        -- automated ordering allowed ONLY when Signed
  SyncMethod                  enum      -- Manual | CsvFeed | Api

SupplierProduct               -- what the supplier sells, as they describe it
  Id, SupplierId
  SupplierSku                 string    -- their code, the key for re-syncing
  SourcePriceExVat            decimal   -- what WE pay
  SourceStock                 int?      -- null when they do not publish it
  SourceUrl                   string
  LastCheckedAt               datetime  -- how stale is this?
  LastChangedAt               datetime
  IsAvailable                 bool

Product                       -- unchanged fields, plus:
  SupplierProductId           Guid?     -- null for own-stock items
  MarkupPercentOverride       decimal?  -- null = use the supplier default
  PriceLastSyncedAt           datetime?

PriceHistory                  -- append-only; never overwrite a price silently
  Id, SupplierProductId, PriceExVat, RecordedAt
```

### Why `PriceHistory` is not optional

The calculator's entire claim is that its output is **auditable**. A quote given on Monday must
still be explainable on Friday, after the supplier changed their price. Overwriting prices in place
destroys that — and it is also what makes disputes unwinnable.

**Rule: an order stores the price it was sold at.** `PowerboxComponents` already does this with
`UnitPrice`, which is the right pattern; keep it.

---

## How the sync would work

```
Scheduled job (hourly or nightly)
  └─ for each active SupplierProduct
       ├─ fetch current price / stock / availability
       ├─ if unchanged → update LastCheckedAt only
       ├─ if changed   → append PriceHistory, update SupplierProduct,
       │                 recalculate Product.Price = source × (1 + markup)
       └─ if unreachable → leave the old value, increment a failure count,
                           and flag after N consecutive failures
```

In .NET the scheduler is a `BackgroundService`, or Hangfire/Quartz.NET if retries and a dashboard
are wanted. **Not** a job that runs inside a web request.

### The rules that stop this becoming dangerous

1. **Never let a sync failure silently show a stale price as current.** If a product has not been
   verified within *N* hours, either hide it or label it *"hind kontrollimata"*.
2. **Guard against absurd changes.** A price moving more than ~30 % in one sync is more likely a
   parsing error than a real change. Flag for review; do not publish.
3. **Never let sync take a site visitor's request with it.** All of this happens in the background.
4. **Rate-limit and identify yourself.** A descriptive `User-Agent` and slow, polite intervals.
   (The Wikimedia work in `IMAGE_CREDITS.md` already hit rate limits — assume everyone has them.)
5. **Log every change.** Who/what changed a price and when is an audit requirement, not a nicety.

### Images

Do **not** hotlink supplier images: it breaks when they move, and it uses their bandwidth without
permission. Download, store, and record the licence — the process `IMAGE_CREDITS.md` already
describes. Real product photography needs the supplier's permission, which a reseller agreement
normally grants.

---

## Suggested phases

| Phase | What | Why this order |
|---|---|---|
| **1** | `Order` / `OrderLine` entities + real checkout persistence | Nothing else matters until an order exists. `Checkout()` currently saves nothing. |
| **2** | `Supplier` + `SupplierProduct` + `PriceHistory`, all filled in **by hand** | Proves the model before automating anything |
| **3** | Payment (Stripe.net), and the consumer-law obligations above | You cannot take money before this |
| **4** | Manual order relay — an email or dashboard task per order | Ships immediately; tests the business, not the code |
| **5** | Reseller agreement with one supplier | The unlock for everything after it |
| **6** | Automated price/stock sync for that one supplier | Now legitimate, and against a real feed |
| **7** | Automated order placement | Last, and only where an agreement permits it |

Most of the value arrives at phase 4. Phases 6–7 are optimisation, not the product.

---

## How it connects to the calculator

The calculator is the differentiator, and it makes this model stronger than a generic dropshipper:

- A BOM is a **bundle**, not one item — bigger baskets, and a reason to buy everything in one place.
- The quantities are **justified by a standard**, so the customer trusts the list.
- The BOM already carries `ProductId`, `Quantity` and `UnitPrice` per line — **structurally already
  an order**. Turning it into one is a small step, which is why phase 1 is cheap.

Keep it that way: whatever the shop becomes, the calculator stays the reason anyone arrives.

---

## Open questions for Edgar

1. Which supplier would you approach first? Onninen is already B2B-oriented and reseller-friendly.
2. Own stock for fast-moving items (breakers, common cable), or pure dropship?
3. Estonia only at first, or the Baltics? Cross-border adds VAT-registration duties.
4. Is 25 % from research, or a starting guess? Worth checking typical Estonian electrical trade
   margins before committing to it publicly.
