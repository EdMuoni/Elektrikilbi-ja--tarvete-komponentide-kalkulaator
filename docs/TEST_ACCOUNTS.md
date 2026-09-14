# Test accounts

Every account you can sign in with, what each one is for, and how they are created.

> ## ⚠️ These passwords are public
>
> The demo passwords below are written in plain text in this file **and** in
> `IdentitySeeder.cs`. Anyone who can read the repository knows them.
>
> They are created **only when the application runs in the Development environment**. That guard is
> the entire safety mechanism — without it, publishing this site would hand an administrator account
> to anyone who found the repository.
>
> **Never** reuse these passwords anywhere real, and **never** remove the environment check.

---

## The accounts

| Role | Email | Password | Created |
|---|---|---|---|
| **Administrator** | `admin@demo.local` | `Admin123` | Automatically, Development only |
| **Customer** | `klient@demo.local` | `Klient123` | Automatically, Development only |
| **Administrator (real)** | *from User Secrets* | *from User Secrets* | Every environment, if configured |

They appear the first time the application starts. Nothing to run by hand.

### On Edgar's machine

A real administrator also exists, seeded from User Secrets rather than from this file, so its
password is not in the repository:

```bash
cd ElektriKalkulaator/ElektriKalkulaator
dotnet user-secrets list        # shows AdminUser:Email and AdminUser:Password
```

---

## What each account is for

### Administrator — `admin@demo.local`

Can do everything a customer can, **plus** manage the catalogue:

| Page | Purpose |
|---|---|
| `/Products/Create` | Add a product, including an image upload |
| `/Products/Edit/{id}` | Change a product or replace its image |
| `/Products/Delete/{id}` | Remove a product |
| `/Products/Categories` | Manage categories |

A **⚙️ Halda** link appears in the navigation bar when signed in as an administrator. That link is
only presentation — the real protection is `[Authorize(Roles = "Admin")]` on the controller, so
typing the URL directly still gets you nowhere.

### Customer — `klient@demo.local`

An ordinary registered user. **Use this to check what a non-administrator actually sees**, which is
easy to get wrong when you are always logged in as an admin.

Opening an admin page as this user gives **Access Denied**, not the login page — the distinction
matters: they are authenticated, just not authorised, so asking them to log in again would be a
dead end.

### Anonymous — no account

The calculator, the product catalogue and the cart all work **without signing in**, deliberately:
the calculator is the product, and putting it behind a login would defeat the point. Worth testing
signed out too.

---

## Trying it

```bash
cd ElektriKalkulaator
dotnet run --project ElektriKalkulaator      # http://localhost:8080
```

The console prints the demo credentials at startup, so they are visible without opening this file.

A five-minute pass that exercises the whole permission model:

1. **Signed out** — open `/Calculator`, run a calculation, add something to the cart. All should work.
2. Open `/Products/Create` while signed out → redirected to the login page.
3. **Sign in as the customer.** Repeat step 2 → **Access Denied**, not the login page.
4. Add something to the cart, then sign out → the cart is empty. It lives in the session, and
   signing out clears it so the next person on a shared computer does not inherit it.
5. **Sign in as the administrator.** `/Products/Create` now opens. Add a product with an image.
6. Try uploading a `.txt` file renamed to `.jpg` → refused, with a message on the form rather than
   an error page.

---

## How this is created and kept safe

`ElektriKalkulaator.Data/IdentitySeeder.cs` runs at startup, after migrations:

1. Creates the `Admin` and `Customer` roles if missing — **always**.
2. Creates the configured administrator from `AdminUser:Email` / `AdminUser:Password` — **always**,
   if those are set.
3. Creates the two demo accounts — **only when `app.Environment.IsDevelopment()`**.

Existing accounts are never touched, so restarting does not duplicate anyone or reset a password you
changed while testing.

### Tested, not assumed

`DemoAccountSeedingTests` covers this, and the most important test is the negative one:

| Test | What it protects |
|---|---|
| `DemoAccounts_AreNotCreated_OutsideDevelopment` | **The safety guard.** A protection nobody has tested is one nobody should trust. |
| `DemoAccounts_AreCreated_InDevelopment` | The accounts genuinely appear when they should |
| `DemoCustomer_IsInTheCustomerRole_AndNotAnAdmin` | The customer account is useless for its purpose if it is accidentally an admin |
| `DemoPasswords_ActuallyWork` | The passwords in this file match the seeded ones — stops the docs drifting |
| `PasswordsAreStoredHashed_NeverAsPlainText` | Passwords are hashed in the database |
| `RunningTheSeederTwice_DoesNotDuplicateAnyone` | The seeder runs on every start, so this is the normal case |
| `AChangedPassword_IsNotResetByRestarting` | Changing a password while testing is not silently undone |

---

## Before deploying anywhere public

- [ ] Confirm the server does **not** run with `ASPNETCORE_ENVIRONMENT=Development`. That single
      variable is what stands between you and a public administrator account.
- [ ] Set a real `AdminUser:Email` / `AdminUser:Password` through environment variables or a secret
      store — never in `appsettings.json`.
- [ ] Sign in as the real administrator once and confirm it works before relying on it.
- [ ] Consider deleting the demo accounts from the production database if the site was ever started
      in Development mode against it.
