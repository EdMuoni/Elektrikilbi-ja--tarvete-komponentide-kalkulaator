# Kuidas see kood töötab — selgitus oma sõnadega

> **Kellele:** sulle, et sa saaksid kaitsmisel oma töö tööpõhimõtet ise seletada,
> ilma et peaksid koodi ette lugema.
>
> **Kuidas lugeda:** iga peatükk algab **ühe lausega**, mille saad õppida ütlema.
> Selle all on pikem selgitus ja kõige lõpus on osa „Kui juhendaja küsib…“
> koos vastustega.
>
> **Reegel:** ära õpi seda pähe. Loe läbi, ava kood kõrvale ja jälgi ühte päringut
> algusest lõpuni. Kui oskad seletada, mis juhtub ühe nupuvajutusega, oskad
> seletada kogu rakendust.

---

## 1. Suur pilt ühe lausega

> **„Rakendus on jagatud nelja ossa, kus iga osa teab ainult neist, mis on
> temast allpool. Nii saab äriloogikat testida ilma veebiserverit käivitamata.“**

```
Core  ←  Data  ←  ApplicationServices  ←  ElektriKalkulaator (veeb)
```

Nool tähendab „sõltub sellest“. Loe paremalt vasakule: veebiprojekt teab
teenustest, teenused teavad andmebaasist, andmebaas teab domeeniolemitest.
**Tagurpidi ei tea keegi kedagi.**

| Osa | Mis seal on | Lihtne seletus |
|---|---|---|
| **Core** | `Product`, `ProductCategory`, `CalculationRule`, DTO-d, liidesed | Mõisted. Siin ei ole ühtegi rida, mis midagi teeks. |
| **Data** | `ElektriKalkulaatorContext`, migratsioonid, algandmed | Andmebaasiga rääkimine. |
| **ApplicationServices** | `CalculatorServices`, `ProductServices` | **Siin elab mõtlemine.** Arvutus toimub siin. |
| **ElektriKalkulaator** | Kontrollerid, Razor vaated, CSS, JS | Veebileht. Võtab päringu vastu, küsib teenuselt, kuvab tulemuse. |

**Miks nii?** Kui arvutusloogika oleks kontrolleris, saaks seda testida ainult
veebiserverit käivitades. Nüüd saab `CalculatorServices` klassi testis otse
välja kutsuda. Just seetõttu on projektis 212 testi, mis jooksevad 3 sekundiga.

**Reegel, mida ei tohi rikkuda:** Core ei sõltu kunagi millestki ja Data ei viita
kunagi veebiprojektile. Kui see katki läheb, kaob testitavus.

---

## 2. Mis juhtub, kui kasutaja vajutab „Näita, mida vaja läheb“

See on kõige tähtsam osa. Kui oskad selle sammhaaval ära seletada, oskad
seletada kogu rakendust.

### Samm 1 — brauser saadab vormi serverisse

Kasutaja täidab vormi ja vajutab nuppu. Brauser saadab **HTTP POST** päringu
aadressile `/Calculator`. Kaasa lähevad väljade väärtused ja **antiforgery
token** (turvamärgis, vt peatükk 6).

### Samm 2 — ASP.NET leiab õige meetodi

ASP.NET vaatab aadressi ja meetodit (POST) ning leiab `CalculatorController`
klassist meetodi, mis on märgitud `[HttpPost]`. Seda nimetatakse **marsruutimiseks
(routing)**.

### Samm 3 — andmed pannakse objektiks (model binding)

ASP.NET võtab vormiväljad ja täidab nendega automaatselt `CalculatorInputDto`
objekti. Väli `RoomCount` vormis läheb omadusse `RoomCount` objektis — **nimede
kokkulangevuse järgi**.

Seejärel kontrollitakse **valideerimisreegleid**, mis on DTO klassis
märgenditena, näiteks `[Range(1, 100)]`. Kui midagi on valesti, ei jõuta
arvutuseni: kasutajale näidatakse sama vorm koos veateatega.

### Samm 4 — kontroller küsib teenuselt

Kontroller **ei arvuta ise midagi**. Ta kutsub välja:

```csharp
var bom = await _calculatorServices.Calculate(input);
```

`_calculatorServices` anti kontrollerile konstruktoris ette — see on
**sõltuvuste süstimine (dependency injection)**. Kontroller ei tea, milline
klass seda liidest täidab; ta teab ainult, et keegi oskab arvutada.

**Miks see hea on:** testis saab sama kontrollerile anda võltsteenuse. Ja
arvutust saab testida ilma kontrollerita.

### Samm 5 — teenus teeb tegeliku töö

`CalculatorServices.Calculate()` teeb järgmist:

1. **Loeb andmebaasist arvutusreeglid** selle hoone tüübi kohta.
2. **Iga reegli kohta arvutab ahelate arvu:**
   `ahelaid = ülespoole ümardatud (tarbijate arv ÷ reegli jagaja)`
   Näiteks 12 valgustit ÷ 8 = 1,5 → **2 ahelat**.
3. **Otsib andmebaasist sobiva kaitselüliti** — õige kategooria ja nimivooluga,
   laos olev, odavaim.
4. **Otsib sobiva kaabli** ja arvutab meetrid.
5. **Lisab kilbi ja rikkevoolukaitse** — üks kummastki.
6. **Salvestab arvutuse** ajalukku.
7. **Tagastab nimekirja** `BOMItemDto` objektidest.

### Samm 6 — vaade kuvab tulemuse

Kontroller annab nimekirja Razor vaatele, mis genereerib HTML-i. Vaade
**ainult kuvab** — ta ei arvuta midagi.

### Samm 7 — brauser saab HTML-i tagasi

Valmis leht saadetakse tagasi ja kasutaja näeb tabelit.

> **Ütle seda nii:** „Vorm läheb kontrollerisse, kontroller annab andmed
> teenusele, teenus loeb reeglid andmebaasist ja arvutab, tulemus läheb vaatesse,
> vaade teeb HTML-i. Kontroller ise ei arvuta midagi — see on meelega, sest siis
> saab arvutust eraldi testida.“

---

## 3. Kuidas rakendus andmebaasiga räägib

> **„Me ei kirjuta SQL-i. Me kirjutame C#-i ja Entity Framework Core tõlgib selle
> SQL-iks.“**

### 3.1. Mis on ORM

**ORM** = *Object-Relational Mapping*. C# klass `Product` vastab tabelile
`Products`. Klassi omadus `Price` vastab veerule `Price`. EF Core hoiab neid
kahte kooskõlas.

Näiteks see C#:

```csharp
var tooted = await _context.Products
    .Where(p => p.Brand == "ABB")
    .OrderBy(p => p.Price)
    .ToListAsync();
```

muutub ligikaudu selliseks SQL-iks:

```sql
SELECT * FROM Products WHERE Brand = 'ABB' ORDER BY Price
```

### 3.2. Kõige tähtsam detail: millal päring käivitub

See on koht, kus tehakse kõige rohkem vigu, ja hea koht, kust kaitsmisel punkte
saada.

`.Where(...)` ja `.OrderBy(...)` **ei käivita veel midagi**. Need ainult
koostavad päringut. Päring läheb andmebaasi alles siis, kui kutsutakse
`.ToListAsync()`.

**Miks see oluline on:**

```csharp
// HEA — andmebaas filtreerib ja tagastab ainult vajaliku
var tulemus = await _context.Products.Where(p => p.Brand == "ABB").ToListAsync();

// HALB — andmebaas tagastab KÕIK tooted, siis viskame enamiku minema
var tulemus = (await _context.Products.ToListAsync()).Where(p => p.Brand == "ABB");
```

Selles projektis rakendatakse kõik filtrid ja sorteerimine **enne**
`ToListAsync()` väljakutset. Seda on näha `ProductServices.Search()` meetodis.

> **Ütle seda nii:** „Filtreerimine ja sorteerimine käivad andmebaasis, mitte
> mälus. Ma ehitan päringu valmis ja alles `ToListAsync` saadab selle andmebaasi.
> Nii ei tule üle võrgu ridu, mida ma kohe ära viskan.“

### 3.3. DbContext

`ElektriKalkulaatorContext` on klass, mis esindab andmebaasi. Iga tabel on
selles üks omadus:

```csharp
public DbSet<Product> Products { get; set; }
public DbSet<ProductCategory> ProductCategories { get; set; }
public DbSet<CalculationRule> CalculationRules { get; set; }
```

Teenused saavad selle konstruktoris ette (jälle sõltuvuste süstimine).

### 3.4. Migratsioonid

Kui muudad C# klassi — näiteks lisad `Product`-ile uue välja — ei muutu
andmebaas iseenesest. Tuleb luua **migratsioon**:

```bash
dotnet ef migrations add LisaUusVali
dotnet ef database update
```

Migratsioon on genereeritud fail, mis kirjeldab muudatust. See läheb
versioonihaldusse koos koodiga.

**Miks nii:** kui muudaksid andmebaasi käsitsi SSMS-is, oleks muudatus ainult
sinu arvutis. Järgmine inimene saaks koodi, mis eeldab uut välja, ja andmebaasi,
kus seda ei ole. Migratsioonidega saab igaüks käivitada ühe käsu ja olla samas
seisus.

Selles projektis on migratsioonid kaustas `ElektriKalkulaator.Data/Migrations/`.

### 3.5. Algandmed (seed data)

Kategooriad, tooted ja arvutusreeglid on koodis kirjas ja laaditakse
andmebaasi käivitumisel. See tähendab, et rakenduse saab nullist tööle panna
ilma andmeid käsitsi sisestamata.

> **Kaitsmisel hea näide:** algandmete ja koodi lahknemine põhjustas selles
> projektis päris vea. `.gitignore` välistas tootepildid versioonihaldusest,
> aga algandmed viitasid neile. Minu masinas oli kõik korras, sest failid olid
> kohapeal. Värskelt alla laaditud koopias oli 10 katkist pilti. Selle peale
> kirjutasin testi, mis kontrollib, et iga algandmete pildifail ka päriselt
> olemas on.

---

## 4. Andmemudel — millised tabelid on ja miks

| Tabel | Mida hoiab | Tähtis detail |
|---|---|---|
| `Products` | Tooted: nimi, tootja, hind, laoseis, nimivool, ristlõige | `CategoryId` viitab kategooriale |
| `ProductCategories` | Kategooriad | |
| `CalculationRules` | Arvutusreeglid | **Ei ole võtmega seotud toodetega** — vt allpool |
| `PowerboxCalculations` | Salvestatud arvutused | |
| `PowerboxComponents` | Salvestatud arvutuse read | Salvestab **hinna arvutuse hetkel** |
| `AspNetUsers`, `AspNetRoles` jne | Kasutajad ja rollid | Loob ASP.NET Core Identity ise |

### 4.1. Miks CalculationRule ei ole tootega seotud

See tundub veana, aga on meelega.

`CalculationRule` ütleb: *„korterelamu valgustusahelas on 1 ahel 8 valgusti
kohta, kaabel 1,5 mm², kaitse 10 A“*. See **ei ütle**, milline toode osta.

Toode otsitakse arvutuse ajal kategooria ja nimivoolu järgi. Nii saab:

- lisada uue tootja kaitselüliti ilma reegleid puutumata;
- muuta reeglit ilma tootekataloogi puutumata;
- valida alati odavaima laos oleva sobiva toote.

Kui reegel viitaks konkreetsele tootele võtmega, tuleks iga toote lisamisel
reegleid muuta ja laost otsa saanud toode blokeeriks arvutuse.

### 4.2. Miks PowerboxComponents salvestab hinna

Salvestatud arvutuse read hoiavad `UnitPrice` välja — hinda **sellisena, nagu
see arvutuse hetkel oli**.

Kui hind hiljem muutub, ei muutu vana arvutuse summa tagantjärele.

**Miks see oluline on:** kogu töö lubadus on, et arvutust saab kontrollida. See
lubadus ei kehti, kui esmaspäeval antud hinnapakkumine näitab reedel teist
summat. Sama põhimõte kehtib igas päris e-poes.

> **Ütle seda nii:** „Tellimus salvestab hinna, millega müüdi, mitte viite
> praegusele hinnale. Muidu muutuks vana arvutus tagantjärele ja seda ei saaks
> enam kontrollida.“

---

## 5. Kuidas kataloogi otsing töötab

`ProductServices.Search()` võtab neli argumenti: kategooria, otsisõna, tootja ja
sorteerimisjärjekorra. Kõik on valikulised.

Oluline nüanss, mille kohta tasub küsimust oodata:

| Argument | Vastavus | Miks |
|---|---|---|
| `searchTerm` | **osaline** — „ABB“ leiab „ABB S201-B32“ | See on otsingukast |
| `brand` | **täpne** — „ABB“ leiab ainult tooted, mille tootja on täpselt „ABB“ | See on filter valitud nimekirjast |

**Miks vahe on tähtis:** kui tootjafilter oleks osaline, võiks ühe tootja filter
tagastada ka teise tootja tooteid, mille nimes see sõna juhtub esinema. Tulemus
näeks usutav välja ja keegi ei märkaks, kuni klient tellib vale osa.

Sellele on eraldi test, mis kontrollib, et tootja nime **osa** ei leia midagi.

---

## 6. Turvalisus — mida ja miks

| Kaitse | Mille vastu | Kuidas |
|---|---|---|
| **Autentimine ja rollid** | Võõras muudab tooteid | ASP.NET Core Identity, `[Authorize(Roles = "Admin")]` |
| **Antiforgery token** | Võõras leht teeb sinu nimel tegevusi (CSRF) | `[ValidateAntiForgeryToken]` + peidetud väli vormis |
| **`Url.IsLocalUrl`** | Link suunab kasutaja võõrale lehele | Kontrollitakse, et tagasisuunamise aadress on meie oma |
| **Koguse vahemik 1–999** | Negatiivne kogus teeb ostukorvi summa miinusesse | Kontroll kontrolleris |
| **Faili algusbaitide kontroll** | `.exe` ümber nimetatud `.jpg`-ks | Loetakse faili esimesed baidid, mitte laiendit |
| **Sessiooni tühjendamine** | Ühiskasutatavas arvutis näeb järgmine eelmise ostukorvi | Väljalogimisel |
| **Razori automaatne escaping** | XSS | Razor põgeneb `<` ja `>` märgid ise |

> **Kaitsmisel aus vastus:** „Esimeses versioonis polnud autentimist üldse, kuigi
> `Program.cs` sisaldas `UseAuthorization()`. See jättis mulje, et kaitse on
> olemas. Turvaülevaatusel selgus, et igaüks pääses administraatori lehtedele.
> See õpetas, et turvalisust ei saa lõppu jätta, sest see puudutab arhitektuuri.“

---

## 7. Testimine ühe lausega

> **„Test, mis ei suuda kunagi läbi kukkuda, on halvem kui testi puudumine.“**

Iga oluline test on läbinud **mutatsioonitestimise**: kood rikutakse meelega ja
kontrollitakse, kas test läheb punaseks. Kui ei lähe, ei kontrolli test midagi.

Kaks päris näidet sellest projektist:

1. Turvakontroll kontrollis ainult, et ümbersuunamine **ei lähe** ründaja lehele.
   Tühi vastus rahuldas selle tingimuse — kontroll oli roheline, kuigi ei
   kontrollinud midagi.
2. Test, mis kontrollis avalehel kuvatavat numbrit, otsis seda kogu failist.
   Number esines ka kommentaaris, mistõttu test läks läbi ka siis, kui nähtav
   number oli vale.

---

## 8. Kui juhendaja küsib…

**„Miks ASP.NET Core, mitte midagi muud?“**
Õppekavas läbitud tehnoloogia, seega töö näitab omandatud oskusi. Rakendus on
vorm ja tulemus, mitte interaktiivne rakendus — serveripoolne renderdamine
sobib, eraldi API ja JavaScripti raamistik lisaksid keerukust ilma kasuta.

**„Miks neli projekti, mitte üks?“**
Et äriloogikat saaks testida ilma veebiserverit käivitamata. Kui arvutus oleks
kontrolleris, tuleks iga testi jaoks käivitada terve rakendus.

**„Kus arvutus päriselt toimub?“**
`ElektriKalkulaator.ApplicationServices/Services/CalculatorServices.cs`, meetod
`Calculate()`.

**„Miks ümardad ülespoole?“**
Kui valgusteid on 12 ja üks ahel kannab 8, on tulemus 1,5 ahelat. Allapoole
ümardamine annaks ühe ülekoormatud ahela.

**„Kust arvutusreeglid tulevad?“**
Andmebaasist, tabelist `CalculationRules`. Seetõttu saab reeglit muuta ilma
koodi muutmata ja rakendust uuesti ehitamata.

**„Kas see vastab EVS-HD 60364 standardile?“**
Vt `docs/EVS_ALLIKAD.md` — vasta täpselt nii, nagu seal kirjas. **Ära ütle
lihtsalt „jah“.** Osa reegleid tuleneb standardi põhimõtetest, osa on
projekteerimistava.

**„Mis on kõige nõrgem koht?“**
Arvutusreeglite juures ei ole veel standardi punktiviiteid. Ja tellimust ei
salvestata — `Checkout()` tühjendab ostukorvi, kuid `Order` olemit veel ei ole.

**„Mida sa uuesti teisiti teeksid?“**
Turvalisuse ja autentimise lisaksin alguses, mitte lõpus. Ja kirjutaksin testid
varem — mitu viga, mille testid hiljem leidsid, olid koodis juba nädalaid.

---

## 9. Failid, mida tasub kaitsmisel avada

| Fail | Miks |
|---|---|
| `ApplicationServices/Services/CalculatorServices.cs` | Siin on arvutus |
| `ApplicationServices/Services/ProductServices.cs` | Siin on otsing ja filtreerimine |
| `Data/ElektriKalkulaatorContext.cs` | Siin on andmemudel ja algandmed |
| `Core/Dto/BOMItemDto.cs` | Siin on tulemuse kuju |
| `ElektriKalkulaator/Controllers/CalculatorController.cs` | Siin on näha, et kontroller ei arvuta ise |
| `ElektriKalkulaator.Tests/` | 212 testi |
