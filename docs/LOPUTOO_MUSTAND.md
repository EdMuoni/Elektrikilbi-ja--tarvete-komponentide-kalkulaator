# LÕPUTÖÖ MUSTAND — Elektrikilbi ja -tarvete komponentide kalkulaator

> **See fail on tööversioon, mitte lõplik lõputöö.**
>
> Siin on kogu tekst eesti keeles, valmis kopeerimiseks Wordi malli
> `LÕPUTÖÖ_TEMPLATE (1).docx`. Struktuur järgib kooli malli TAR-i (noorem
> tarkvaraarendaja) haru: peatükid 1 ja 3, mitte ITS-i omad.
>
> **Kuidas seda faili lugeda:**
>
> | Märgend | Tähendus |
> |---|---|
> | `[EDGAR: ...]` | Siia on vaja **sinu** otsust või infot, mida mina ei tea. Ära jäta neid sisse. |
> | `[KONTROLLI: ...]` | Väide, mille pead enne esitamist üle kontrollima. |
> | `[JOONIS n]` | Siia kuulub pilt. Loetelu on peatükis „Jooniste plaan“ selle faili lõpus. |
>
> **Maht:** näidistööd `Näited/` kaustas on 35–62 lehekülge ja 5 700–8 500 sõna.
> See mustand on ligikaudu 7 000 sõna, mis koos joonistega annab umbes 40–45
> lehekülge. Sinu senine tekst oli ~2 000 sõna.
>
> **Kõige tähtsam reegel:** ära kirjuta siia ühtegi EVS-HD 60364 punktinumbrit,
> mida sa pole standardist endast üle kontrollinud. Vale viide standardile töös,
> mis räägib standardile vastavusest, on hullem kui viite puudumine.

---

## MÕISTED JA LÜHENDID

| Mõiste | Selgitus |
|---|---|
| **ASP.NET Core** | Microsofti avatud lähtekoodiga veebiraamistik. Selles töös versioon 9. |
| **MVC** | *Model-View-Controller* — arhitektuurimuster, kus andmed (Model), kasutajaliides (View) ja päringute käsitlemine (Controller) on eraldatud. |
| **Entity Framework Core (EF Core)** | Objekt-relatsioonvastendus (ORM): C# klassid vastendatakse andmebaasitabeliteks, nii et SQL-i ei pea käsitsi kirjutama. |
| **Migratsioon** | Genereeritud skript, mis viib andmebaasi struktuuri vastavusse muudetud C# mudeliga. |
| **DTO** | *Data Transfer Object* — lihtne klass andmete liigutamiseks kihtide vahel, ilma andmebaasi olemeid välja andmata. |
| **DI** | *Dependency Injection* — sõltuvused antakse klassile konstruktoris ette, selle asemel et klass need ise looks. See teeb koodi testitavaks. |
| **BOM** | *Bill of Materials* — materjalide loend. Selle töö peamine väljund. |
| **RCD** | *Residual Current Device*, eesti k. rikkevoolukaitse. Katkestab vooluahela lekkevoolu tuvastamisel. |
| **MCB** | *Miniature Circuit Breaker*, kaitselüliti. |
| **NYM-J** | Levinud paigalduskaabli tüüp, nt NYM-J 3×2,5 mm². |
| **EVS-HD 60364** | Eesti standardikogum madalpingelektripaigaldiste kohta. Selle töö arvutusreeglite alus. |
| **xUnit** | C# testiraamistik. `[Fact]`-märgendiga meetodid käivitatakse automaatselt. |
| **CI** | *Continuous Integration* — automaatne ehitamine ja testimine iga muudatuse järel. |
| **WCAG** | *Web Content Accessibility Guidelines* — veebi ligipääsetavuse juhised. |
| **Antiforgery token** | Vormiga kaasa saadetav ühekordne märgis, mis tõendab, et päring tuli meie enda lehelt. |

---

## SISSEJUHATUS

Hoonete elektrivarustuse projekteerimine on vastutusrikas valdkond, kus iga otsus
peab vastama kehtivatele ohutusnõuetele. Kaitselüliti nimivool, kaabli ristlõige
ja rikkevoolukaitse valik ei ole maitseküsimused — need tulenevad standardist, ja
vale valik võib põhjustada kaabli ülekuumenemise või jätta inimese elektrilöögi
eest kaitseta.

Väikeste objektide puhul — korter, eramu, väike äripind — on nende arvutuste
tegemine siiski ebaproportsionaalselt tülikas. Elektrik või tellija peab kas
palkama eelarvestaja või tegema arvutused käsitsi, otsima iga komponendi hinna
eraldi kokku ja lootma, et kusagil ei tekkinud viga. Tulemus on ajakulu, mis on
objekti maksumusega võrreldes suur.

Turul on olemas spetsialiseeritud tarkvaralahendused, näiteks ABB ja Schneider
Electricu tootjaspetsiifilised valikutööriistad. Need on aga suunatud
professionaalsele projekteerijale, seotud ühe tootja tootevalikuga ja eeldavad
kasutajalt erialast ettevalmistust. Lihtsat, eestikeelset ja tootjaneutraalset
vahendit, mis annaks väikeobjektile kohe hinnastatud komponentide nimekirja,
autor ei leidnud.

Käesoleva lõputöö eesmärk on luua veebirakendus **„Elektrikilbi ja -tarvete
komponentide kalkulaator“**, mis võtab sisendiks hoone lihtsad parameetrid —
hoone tüüp, tubade arv, pistikute ja valgustite arv, elektripliidi olemasolu —
ning tagastab EVS-HD 60364 alusel arvutatud komponentide nimekirja koos
hindadega.

Töö keskne idee ei ole ainult arvutamine, vaid **arvutuskäigu nähtavaks
tegemine**. Rakendus ei ütle üksnes „kaks kaitselülitit“, vaid näitab, mitmest
ahelast see arv tuleb ja millisest jaotusreeglist. See eristab tööd nii
kaubanduslikest e-poodidest, mis müüvad komponente ilma põhjenduseta, kui ka
professionaalsetest projekteerimisvahenditest, mis eeldavad, et kasutaja oskab
tulemust ise kontrollida.

Lõputöö on üles ehitatud järgmiselt. **Esimeses peatükis** käsitletakse
teoreetilist tausta: töö eesmärki, projekti kavandamist, kasutatud tehnoloogiate
valikut ja rakenduse kavandamist. **Teises peatükis** kirjeldatakse valminud
lahendust: arhitektuuri, andmemudelit, arvutusalgoritmi, kasutajaliidest,
turvalisust ja testimist. **Kolmandas peatükis** esitatakse tulemused ja nende
vastavus algsele eesmärgile. **Neljandas peatükis** tehakse järeldused ja
kirjeldatakse edasiarendamise võimalusi.

---

# 1. TEOREETILINE OSA

## 1.1. Töö eesmärk, teema valiku põhjendus ja olulisus

### 1.1.1. Teema valiku põhjendus

Teema valik on ajendatud praktilisest vajadusest. Elektripaigaldise
komponentide valik väikeobjektil on korduv ülesanne, mille lahendamine käib
alati sama loogika järgi: loe kokku tarbijad, jaga need ahelateks, vali igale
ahelale kaitselüliti ja kaabli ristlõige, lisa rikkevoolukaitse ja kilp. See on
täpselt selline ülesanne, mille automatiseerimine annab kohe mõõdetava kasu —
reeglid on olemas, need lihtsalt rakendatakse käsitsi.

Teema on ühtlasi sobiv noorema tarkvaraarendaja lõputööks, sest see nõuab
korraga mitut oskust: andmemudeli kavandamist, äriloogika kirjutamist,
veebiliidese loomist, andmebaasi kasutamist ja turvalisuse arvestamist.

### 1.1.2. Töö eesmärk

Töö peamine eesmärk on välja töötada toimiv veebirakendus, mis:

1. **kogub sisendandmed** lihtsa vormi kaudu, mida saab täita ka inimene, kes ei
   ole elektrik;
2. **arvutab ahelate arvu ja komponendid** EVS-HD 60364 põhimõtete alusel,
   kasutades andmebaasis hoitavaid arvutusreegleid;
3. **koostab hinnastatud materjalide loendi (BOM)**, kasutades tootekataloogi
   reaalseid tooteid ja hindu;
4. **näitab arvutuskäiku**, nii et iga rea juures on näha, kust arv tuleb;
5. **võimaldab tulemuse ostukorvi lisada**, valmistades ette hilisemat
   e-kaubanduse funktsionaalsust.

### 1.1.3. Teema olulisus

Olulisus seisneb kolmes asjas. **Esiteks aja kokkuhoid** — arvutus, mis käsitsi
võtab tunde, valmib minutiga. **Teiseks vigade vältimine** — automaatne arvutus
ei unusta rikkevoolukaitset ega vali liiga õhukest kaablit, sest reegel
rakendatakse alati. **Kolmandaks läbipaistvus** — kuna rakendus näitab
arvutuskäiku, saab tulemust kontrollida, mitte lihtsalt uskuda.

> `[EDGAR: kui sul on juhendajaga kokku lepitud konkreetne mõõdetav eesmärk —
> näiteks „arvutus valmib alla 5 sekundi“ või „kaetud on kolm hoonetüüpi“ —
> lisa see siia. Mõõdetav eesmärk teeb peatüki 3 (Tulemused) kirjutamise
> lihtsamaks, sest siis on millegi vastu võrrelda.]`

---

## 1.2. Projekti kavandamine

### 1.2.1. Lähteülesanne

Lähteülesanne sõnastati esitatud lõputöö kavandis järgmiselt: luua kalkulaator,
mis aitab klientidel kokku panna elektritarvikud ja kilbi komponendid korter- ja
ärihoonetele, näidates komponente, nende hindu ja aidates teha sobivaid valikuid.

Valminud rakendus toetab lisaks ka **eramut**. See on kavandi suhtes väike
laiendus, mis on põhjendatud: arvutusloogika on sama ja eramu on väikeobjektina
sama sagedane kasutusjuht.

### 1.2.2. Nõuded

**Funktsionaalsed nõuded:**

| Nr | Nõue |
|---|---|
| F1 | Kasutaja saab sisestada hoone tüübi, tubade, pistikute, valgustite ja lülitite arvu ning märkida elektripliidi olemasolu. |
| F2 | Süsteem arvutab ahelate arvu iga ahelatüübi kohta. |
| F3 | Süsteem valib igale ahelale kaitselüliti ja kaabli andmebaasi toodete hulgast. |
| F4 | Süsteem lisab rikkevoolukaitse ja jaotuskilbi. |
| F5 | Süsteem kuvab materjalide loendi koos ühiku-, rea- ja kogumaksumusega. |
| F6 | Kasutaja saab BOM-i read ostukorvi lisada. |
| F7 | Kasutaja saab sirvida tootekataloogi, filtreerida kategooria ja tootja järgi ning otsida nime järgi. |
| F8 | Administraator saab tooteid lisada, muuta ja kustutada. |
| F9 | Arvutused salvestatakse ja neid saab hiljem vaadata. |

**Mittefunktsionaalsed nõuded:**

| Nr | Nõue |
|---|---|
| M1 | Kasutajaliides on eestikeelne. |
| M2 | Rakendus töötab tavalises veebibrauseris ilma paigalduseta. |
| M3 | Administraatori funktsioonid on kaitstud autentimisega. |
| M4 | Rakendus kaitseb end levinud veebirünnete eest (CSRF, avatud ümbersuunamine, failiüleslaadimine). |
| M5 | Tekst vastab WCAG AA kontrastinõuetele. |
| M6 | Koodi katab automaatne testikomplekt. |

### 1.2.3. Tegevus- ja ajakava

Töö maht on **156 tundi**. Arendusprotsessis kasutati agiilset lähenemist:
töö jaotati väikesteks, iseseisvalt testitavateks muudatusteks, mitte üheks
suureks etapiks.

| Etapp | Maht | Sisu |
|---|---|---|
| Analüüs ja nõuete kogumine | ~30 h | Standardi põhimõtete uurimine, turuülevaade, nõuete sõnastamine |
| Disain ja arhitektuur | ~40 h | Andmemudel, ERD, kihiline arhitektuur, kasutajaliidese kavand |
| Arendus | ~60 h | Andmebaas, äriloogika, kasutajaliides, autentimine |
| Testimine ja dokumenteerimine | ~26 h | Automaattestid, turvakontroll, lõputöö |

> `[EDGAR: kontrolli, kas 156 tundi on jätkuvalt õige arv ja kas jaotus vastab
> tegelikult kulunud ajale. Peatükis 3 tuleb võrrelda plaani tegelikkusega —
> see võrdlus on malli järgi kohustuslik ja aus vahe on parem kui ilus vale.]`

---

## 1.3. Ülevaade kasutatud tehnoloogiatest ja vahenditest

### 1.3.1. Programmeerimiskeel ja raamistik

**C# ja ASP.NET Core 9 MVC.** Valik tugineb kahele põhjusele. Esiteks on C# ja
ASP.NET Core õppekavas läbitud tehnoloogiad, mis tähendab, et lõputöö
demonstreerib omandatud oskusi, mitte iseõpitud kõrvalteed. Teiseks sobib
serveripoolne renderdamine (server-side rendering) selle rakenduse iseloomuga:
kalkulaator teeb ühe arvutuse ja kuvab tulemuse: siin ei ole vaja
üheleherakendust (SPA) ega eraldi JavaScripti raamistikku.

MVC-muster eraldab andmed, kuvamise ja päringukäsitluse. See ei ole ainult
korrastatuse küsimus — see on eeldus testitavusele, sest äriloogika ei sõltu
sellest, kas seda kutsub veebipäring või test.

**Kaalutud alternatiivid.** Enne valikut vaadeldi kolme muud varianti:

| Alternatiiv | Miks ei valitud |
|---|---|
| React või Angular + eraldi API | Nõuab kahe eraldi rakenduse haldamist. Selle töö kasutajaliides ei ole interaktiivne rakendus, vaid vorm ja tulemus — SPA lisaks keerukust ilma kasuga. |
| Blazor | Huvitav ja kaasaegne, kuid õppekavas läbimata. Lõputöö peaks demonstreerima omandatud oskusi. |
| PHP (Laravel) või Python (Django) | Mõlemad sobiksid tehniliselt, kuid tähendaksid uue keele õppimist lõputöö ajal, mis suurendaks riski. |

Valik langes tuttavale tehnoloogiale teadlikult. Lõputöö riskikoht ei ole
tehnoloogia uudsus, vaid see, kas töö saab tähtajaks valmis ja töötab.

### 1.3.2. Andmebaas ja andmete käsitlemine

**Microsoft SQL Server** koos **Entity Framework Core 9-ga**. EF Core vastendab
C# klassid tabeliteks, mistõttu SQL-i ei kirjutata käsitsi. Andmebaasi struktuuri
muudatused hoitakse **migratsioonides**, mis on versioonihalduses — nii saab
andmebaasi igal masinal samasse seisu viia ühe käsuga.

Oluline detail: kõik filtreerimised ja sorteerimised rakendatakse `IQueryable`-le
**enne** `ToListAsync()` väljakutset. See tähendab, et need muutuvad SQL-päringu
osaks ja andmebaas tagastab ainult vajalikud read õiges järjekorras. Kui
sorteerida alles mälus, tuleks andmebaasist tuua kõik read, et enamik neist kohe
ära visata.

### 1.3.3. Kasutajaliides

**Razor vaated ja Bootstrap 5.** Razor võimaldab kirjutada HTML-i sisse C#-i,
mis sobib serveripoolse renderdamisega. Bootstrap annab valmis ruudustiku ja
komponendid.

Värvid ei tule siiski Bootstrapist, vaid ühest failist `theme.css`, kus on
kirjeldatud kaks täielikku värvipaletti (hele ja tume). Selle põhjus on
kirjeldatud peatükis 2.4.

### 1.3.4. Testimine

**xUnit** koos EF Core **InMemory** pakiga ühik- ja integratsioonitestideks ning
**WebApplicationFactory**-ga HTTP-testideks, mis käivitavad kogu rakenduse mällu
ja saadavad sellele päris HTTP-päringuid.

### 1.3.5. Abivahendid

| Vahend | Kasutus |
|---|---|
| Visual Studio 2022 | Peamine arenduskeskkond |
| Git ja GitHub | Versioonihaldus, harud ja pull request'id |
| GitHub Actions | Automaatne ehitamine ja testimine iga muudatuse järel |
| draw.io | ERD ja arhitektuuriskeemid |
| Claude (AI) | Koodiülevaatus, testide kirjutamise abi, dokumentatsioon |

> `[EDGAR: AI kasutamise mainimine on kooli praktikas tavaline ja aus, aga
> kontrolli juhendajaga, kas ja kuidas ta soovib seda kirjeldatuna näha. Kui
> soovid, saab siia lisada lühikese lõigu selle kohta, mida AI tegi ja mida
> sina — see on hindajale sageli huvitav.]`

---

## 1.4. Rakenduse kavandamine

### 1.4.1. Arhitektuur

Rakendus on jaotatud **nelja projekti**, kus iga projekt sõltub ainult
„allpool“ olevatest. Sama kihiline struktuur on kasutusel autori varasemas
kursusetöös `ShopTARge24`, mis oli teadlik valik: tuttav struktuur vähendab vigu.

```
Core  ←  Data  ←  ApplicationServices  ←  Web (ElektriKalkulaator)
 ↑                                          ↑
 └──────────── Tests viitab kõigile ────────┘
```

| Projekt | Vastutus |
|---|---|
| **Core** | Domeeniolemid, DTO-d, teenuseliidesed. Ei sõltu millestki. |
| **Data** | `DbContext`, migratsioonid, algandmed. |
| **ApplicationServices** | Teenuste teostused — siin elab äriloogika. |
| **ElektriKalkulaator (Web)** | Kontrollerid, Razor vaated, staatilised failid. |
| **Tests** | Viitab kõigile neljale. |

Reegel, mida ei tohi rikkuda: **Core ei sõltu kunagi millestki ja Data ei viita
kunagi veebiprojektile.** Kui see reegel murdub, muutub äriloogika testimine
võimatuks ilma veebiserverit käivitamata.

`[JOONIS 1]` — Arhitektuuriskeem (kihid ja sõltuvused)

### 1.4.2. Andmemudel

Peamised olemid:

| Olem | Sisu |
|---|---|
| `Product` | Toode: nimi, tootja, hind, laoseis, nimivool, kaabli ristlõige, pildi tee. |
| `ProductCategory` | Tootekategooria (kaitselülitid, juhtmed, kilbi korpused, RCD). |
| `CalculationRule` | Arvutusreegel: hoone tüüp, ahelatüüp, jagaja, kaabli ristlõige, nimivool. |
| `PowerboxCalculation` | Salvestatud arvutus koos sisendandmetega. |
| `PowerboxComponents` | Salvestatud arvutuse üksikread. |

`[JOONIS 2]` — Andmemudeli ERD

**Teadlik erisus:** `CalculationRule` ei ole `Product`-iga võtmeseosega seotud.
Reegel seotakse hoone tüübiga **sõne järgi** ja sobiv toode otsitakse arvutuse
ajal kategooria ja nimivoolu põhjal. See on kirjeldatud autori algses
ERD-spetsifikatsioonis ja on tahtlik: nii saab reegleid muuta ilma
tootekataloogi puutumata.

**Teine teadlik otsus:** `PowerboxComponents` salvestab `UnitPrice` välja, st
hinna sellisena, nagu see arvutuse hetkel oli. Kui hind hiljem muutub, ei muutu
vana arvutuse summa tagantjärele. See on auditeeritavuse eeldus — töö keskne
lubadus on, et arvutust saab kontrollida, ja see ei kehti, kui numbrid muutuvad
tagantjärele.

### 1.4.3. Kasutusjuhud

| Kasutaja | Kasutusjuht |
|---|---|
| Külastaja | Arvutab komponendid, sirvib katalooogi, lisab ostukorvi |
| Registreeritud kasutaja | Lisaks: näeb oma arvutuste ajalugu |
| Administraator | Lisaks: haldab tooteid ja kategooriaid |

`[JOONIS 3]` — Kasutusjuhtude diagramm

---

## 1.5. Olemasolevate lahenduste võrdlus

| Lahendus | Tugevus | Nõrkus selle töö kontekstis |
|---|---|---|
| ABB / Schneider valikutööriistad | Täpsed, tootja poolt hooldatud | Ühe tootja tooted, eeldavad erialast ettevalmistust, ingliskeelsed |
| Eesti e-poed (Esvika, Onninen, Elektrikaubad) | Reaalsed hinnad ja laoseis | Müüvad komponente, ei arvuta koguseid |
| Käsitsi arvutamine tabelarvutuses | Paindlik | Aeganõudev, vigu ei märka keegi |
| **Käesolev töö** | Arvutab kogused JA näitab arvutuskäiku, eestikeelne, tootjaneutraalne | Väiksem tootevalik, hinnad ei uuene automaatselt |

Võrdlusest nähtub, et turul on olemas nii arvutusvahendid kui ka poed, kuid
väikeobjektile suunatud eestikeelset lahendust, mis ühendab mõlemad ja **näitab
arvutuskäiku**, ei ole.

> `[KONTROLLI: enne esitamist vaata need konkurendid uuesti üle — veebipoed
> muutuvad. Kirjuta juurde kuupäev, millal võrdlus tehti.]`

---

# 2. PRAKTILINE OSA — LOODUD LAHENDUSE KIRJELDUS

## 2.1. Arendusprotsess ja töökorraldus

Töö tehti üksinda, mistõttu meeskonnajaotust ei ole. Töökorraldus järgis
järgmisi põhimõtteid:

- **Väikesed muudatused ükshaaval.** Iga muudatus ehitati, testiti ja alles siis
  liideti. Suur muudatuste pakk teeb vea allika leidmise raskeks.
- **Haru ja pull request, mitte otse `main`-i.** Iga tööpakett tehti eraldi
  harus ja liideti pull request'i kaudu.
- **Muudatuste päevik.** Iga koodimuudatuse kohta kirjutati kirje faili
  `docs/CHANGELOG.md`, kus on kirjas **miks**, mitte ainult mis. Kood näitab
  ise, mis muutus; põhjus on ainuke asi, mida hiljem enam kuskilt ei leia.

### 2.1.1. Töövoog praktikas

Iga muudatus läbis sama tsükli:

1. **Uus haru** kirjeldava nimega, näiteks `fix/security-hardening`.
2. **Väike muudatus**, mis on iseseisvalt mõttekas.
3. **Ehitus ja testid lokaalselt.** Hoiatused on seatud vigadeks, seega
   hoiatustega kood ei lähe edasi.
4. **Käivitatud rakenduse vastu kontrollimine.** „See ehitub“ ei ole „see
   töötab“ — mitu selle töö vigadest olid kompilaatorile täiesti nähtamatud.
5. **Kirje muudatuste päevikusse** koos põhjendusega.
6. **Pull request** ja liitmine.

Töö käigus tekkis neli üksteise peale ehitatud haru, mis liidetakse
järjekorras. See on tavaline viis hoida ühte suurt tööd väiksemateks
ülevaadatavateks tükkideks jaotatuna.

### 2.1.2. Dokumentatsioon arenduse osana

Projektis on eraldi dokumentatsioonikaust `docs/`, kus muu hulgas:

| Fail | Sisu |
|---|---|
| `CHANGELOG.md` | Iga koodimuudatus ja selle **põhjus**. Ei kirjutata kunagi ümber. |
| `PROJECT_ROADMAP.md` | Praegune seis ja plaanid. |
| `TESTING.md` | Testimise metoodika, kirjutatud nii algajale kui AI-mudelile. |
| `DESIGN_GUIDE.md` | Disainisüsteem ja selle põhjendused. |
| `RESEARCH_LOG.md` | Väljastpoolt kogutud faktid: turuhinnad, konkurentide analüüs. |
| `IMAGE_CREDITS.md` | Iga pildi päritolu ja litsents. |

Selline dokumentatsioon ei ole bürokraatia. Praktikas oli selle väärtus
konkreetne: kui projekti juurde naasti nädalaid hiljem, oli võimalik lugeda,
**miks** mingi lahendus on selline, selle asemel et seda koodist tagasi
tuletada — või, mis halvem, valesti tuletada ja „ära parandada“ midagi, mis oli
tahtlik.

`[JOONIS 4]` — GitHubi pull request'ide ja commit'ide vaade (tõestusmaterjal)

## 2.2. Kalkulaatori algoritm

Algoritm on rakenduse tuum. See töötab järgmiselt:

1. **Loe arvutusreeglid** andmebaasist antud hoone tüübi kohta.
2. **Iga reegli kohta arvuta ahelate arv.** Ahelate arv leitakse tarbijate arvu
   jagamisel reegli jagajaga ja tulemuse **ülespoole ümardamisel**.
3. **Vali kaitselüliti** — sobiva kategooria ja nimivooluga odavaim laos olev
   toode.
4. **Vali kaabel** — sobiva ristlõikega odavaim laos olev toode; kogus arvutatakse
   meetrites.
5. **Lisa rikkevoolukaitse ja jaotuskilp** — üks kummastki paigaldise kohta.
6. **Arvuta summad** ja tagasta BOM.

Ümardamine ülespoole on oluline detail. Kui valgusteid on 12 ja üks ahel kannab
8 valgustit, siis 12 ÷ 8 = 1,5 ahelat, mis ümardatakse **kaheks**. Allapoole
ümardamine tähendaks ülekoormatud ahelat.

**Näide.** Kolmetoaline korter, 10 pistikut, 12 valgustit, elektripliit:

| Komponent | Kogus | Ühikuhind | Kokku |
|---|---|---|---|
| Schneider Easy9 B10A (valgustus) | 2 tk | 7,90 € | 15,80 € |
| NYM-J 3×1,5 mm² kaabel | 48 m | 1,20 €/m | 57,60 € |
| Schneider Easy9 B16A (pistikud) | 2 tk | 8,50 € | 17,00 € |
| NYM-J 3×2,5 mm² kaabel | 48 m | 1,85 €/m | 88,80 € |
| ABB S201-B32 (pliit) | 1 tk | 12,80 € | 12,80 € |
| NYM-J 3×6 mm² kaabel | 24 m | 3,60 €/m | 86,40 € |
| ABB Mistral41F 12 mooduli kilp | 1 tk | 28,50 € | 28,50 € |
| ABB F202 AC-40/0.03 RCD 40A | 1 tk | 42,00 € | 42,00 € |
| **Kokku** | | | **348,90 €** |

Valgustusahelaid on kaks, sest 12 ÷ 8 = 1,5 → 2. Pistikuahelaid on kaks, sest
10 ÷ 6 = 1,67 → 2. Kaablit on kokku 120 m, sest ahelaid on viis ja iga ahela
kohta arvestatakse 24 m.

`[JOONIS 5]` — Kalkulaatori vooskeem

> `[KONTROLLI: see tabel on rakendusest tegelikult saadud väljund. Kui muudad
> algandmete hindu või reegleid, käivita arvutus uuesti ja uuenda tabelit.
> Töös, mille lubadus on auditeeritavus, ei tohi näidistabel tegelikkusest
> erineda.]`

## 2.3. Andmebaas, migratsioonid ja algandmed

Andmebaasi struktuuri muudetakse **ainult migratsioonide kaudu**. Käsitsi
tehtud muudatus ühes andmebaasis ei jõua kunagi teistesse ja põhjustab vea, mis
avaldub alles siis, kui keegi teine projekti avab.

Algandmed (kategooriad, tooted, arvutusreeglid) laaditakse rakenduse
käivitumisel. Need on osa projektist, mitte käsitsi sisestatud, mis tähendab, et
rakenduse saab igal masinal nullist tööle panna ühe käsuga.

**Praktiline õppetund algandmete kohta.** Töö käigus juhtus viga, mis on hea
näide sellest, kuidas andmete ja koodi lahknemine avaldub. Failis `.gitignore`
oli reegel, mis välistas tootepildid versioonihaldusest, samal ajal kui
algandmed viitasid neile failidele. Arendaja enda masinas oli kõik korras, sest
failid olid kohapeal olemas — kuid värskelt alla laaditud koopias oli kümme
katkist pilti. Ei kompilaator ega ükski test seda ei märganud.

Lahenduseks lisati **andmeterviklikkuse test**, mis kontrollib, et iga algandmete
tootepilt eksisteerib ka tegelikult kettal. See on odav test, mis välistab terve
klassi vigu.

## 2.4. Arvutusreeglid ja standard

Arvutusreeglid hoitakse andmebaasis, mitte koodis. See tähendab, et reegli
muutmiseks ei ole vaja rakendust uuesti ehitada.

| Ahelatüüp | Jagaja | Kaabli ristlõige | Nimivool |
|---|---|---|---|
| Valgustus | 1 ahel / 8 valgustit | 1,5 mm² | 10 A |
| Pistikud | 1 ahel / 6 pistikut | 2,5 mm² | 16 A |
| Elektripliit | eraldi ahel | 6,0 mm² | 32 A |

### Mis tuleneb standardist ja mis mitte

Siin tuleb teha vahet, mida kaitsmisel kindlasti küsitakse.

**Standardi põhimõtetest tulenev:** kaabli ristlõike ja kaitselüliti nimivoolu
paar (1,5 mm² → 10 A, 2,5 mm² → 16 A, 6,0 mm² → 32 A). Alus on liigvoolukaitse
põhimõte — kaitseseade peab rakenduma enne, kui juht üle kuumeneb — mida
käsitlevad EVS-HD 60364 osad **4-43** (kaitse liigvoolu eest) ja **5-52**
(juhistikud). Rikkevoolukaitsme vajadus tuleneb osast **4-41**.

**Projekteerimistava, mitte standardi nõue:** ahelate jaotus, st „üks ahel
kaheksa valgusti kohta“ ja „üks ahel kuue pistiku kohta“. Standard **ei sätesta**
punktide arvu ahelas. Standard nõuab, et ahela koormus mahuks kaabli ja kaitse
piiridesse; mitu punkti see tähendab, sõltub punktide võimsusest — kümme
5 W LED-valgustit ja kümme 150 W valgustit on täiesti erinev koormus.

Seetõttu hoitakse neid arve **andmebaasis, mitte koodis**: need on eeldused,
mida saab muuta ilma rakendust uuesti ehitamata. Täielik allikaanalüüs on failis
`docs/EVS_ALLIKAD.md`.

`CalculationRule` olemil on väli `EvsReference`, mis on **teadlikult tühi**.
Sinna kuuluvad standardi punktinumbrid, kuid neid ei tohi sisestada enne, kui
need on standardist endast üle kontrollitud.

> `[EDGAR — SEE ON KÕIGE TÄHTSAM PUNKT KOGU TÖÖS: hangi ligipääs EVS-HD 60364
> standardile ja kirjuta iga ülaltoodud reegli juurde täpne punktinumber. Kuni
> seda ei ole, kirjuta töös „standardi põhimõtetel“, mitte „standardi punkti
> X.Y järgi“.
>
> Väljamõeldud viide standardile töös, mille teema on standardile vastavus,
> on tõsisem viga kui viite puudumine — ja kaitsmisel on see esimene asi,
> mida küsitakse.]`

## 2.5. Kasutajaliides ja disain

### 2.5.1. Kaks lehetüüpi

Kasutajaliidese kavandamisel analüüsiti kuut rahvusvahelist erialast veebilehte.
Analüüsist selgus, et need jagunevad **kaheks vastandlikuks tüübiks**:

| | Turunduslehed | Kataloogilehed |
|---|---|---|
| Näited | Nesta Sites | SupplyHouse, Electrical2Go, AutomationDirect, Proelectro |
| Taust | Tume | Hele |
| Tihedus | 3–4 elementi ekraanil | Kõik korraga nähtav |
| Eesmärk | Veenda proovima | Aidata osa leida |

Käesolev rakendus on mõlemat: **avaleht** peab veenma kalkulaatorit proovima,
**kataloog** peab aitama leida konkreetse komponendi. Seetõttu järgib avaleht
turunduslehe ja kataloog kataloogilehe loogikat. Ühe tüübi rakendamine teisele
on kõige levinum põhjus, miks veebileht „tundub vale“, kuigi iga üksik element
on korras.

`[JOONIS 6]` — Avaleht
`[JOONIS 7]` — Tootekataloog filtritega

### 2.5.2. Värvisüsteem ja hele/tume režiim

Kõik värvid on ühes failis `theme.css` nimetatud muutujatena. Ükski vaade ega
teine laaditabel ei kirjuta värvi väärtust otse.

Selle reegli väärtus tuli välja praktikas. Töö käigus avastati, et vaadetes oli
**38 kohta**, kus kasutati Bootstrapi klassi `text-white`, mis tähendab
sõna-sõnalt valget. Tumedal taustal on see õige, heledal aga nähtamatu — seega
oli hele režiim osaliselt kasutuskõlbmatu, ilma et keegi oleks seda märganud.
Värvimuutujad olid õiged; viga oli selles, et märgistus läks neist mööda.

Pärast parandust lisati automaattest, mis kontrollib, et ükski vaade ega
laaditabel ei sisaldaks otsest värviväärtust. See on hea näide üldisest
põhimõttest: **reegel, mida miski ei kontrolli, ei ole reegel.**

Värvipaletid genereeritakse skriptiga `scripts/generate-theme.py`, sest sama
palett esineb failis neli korda (tume vaikimisi, hele, ja mõlemad uuesti
kasutaja selgesõnalise valiku jaoks). Neli käsitsi hooldatavat koopiat
lahknevad paratamatult.

`[JOONIS 8]` — Sama leht heledas ja tumedas režiimis kõrvuti

### 2.5.3. Ligipääsetavus

Kontrastisuhted mõõdeti skriptiga `scripts/check-contrast.py`. Kõik teksti ja
tausta paarid mõlemas režiimis **ületavad WCAG AA nõude 4,5:1**.

See on oluline vahe: „värvid valiti hoolikalt“ on arvamus, „mõõdeti 34 paari,
madalaim 4,55, ükski ei jää alla AA nõude“ on kontrollitav fakt.

## 2.6. Turvalisus

Turvalisus ei olnud algselt piisav ja seda parandati teadlikult. Leitud ja
parandatud probleemid:

| Probleem | Tagajärg | Lahendus |
|---|---|---|
| Autentimine puudus täielikult | Igaüks pääses administraatori lehtedele | ASP.NET Core Identity, rollid Admin/Customer |
| Avatud ümbersuunamine | Link sai kasutaja suunata võõrale lehele | `Url.IsLocalUrl` kontroll |
| CSRF-kaitse puudus 5 otspunktil | Võõras leht sai kasutaja nimel tegevusi teha | `[ValidateAntiForgeryToken]` |
| Negatiivne kogus ostukorvis | Ostukorvi summa läks miinusesse | Koguse vahemik 1–999 |
| Failiüleslaadimise nõrk kontroll | `.exe` sai ümber nimetada `.jpg`-ks | Faili sisu algusbaitide kontroll |
| Ostukorv säilis väljalogimisel | Ühiskasutatavas arvutis nägi järgmine kasutaja eelmise korvi | Sessiooni tühjendamine |

Turvakontrollide kordamiseks on skript `scripts/security-check.sh`, mis
käivitab kõik rünnakukatsed töötava rakenduse vastu. Kõik **18 kontrolli**
läbivad.

`[JOONIS 9]` — Turvakontrolli skripti väljund

## 2.7. Testimine

### 2.7.1. Testide arv ja liigid

Rakendust katab **212 automaattesti**, mis jagunevad:

| Liik | Mida kontrollib | Näide |
|---|---|---|
| Ühiktestid | Puhas loogika, ilma andmebaasita | Failiüleslaadimise kontroll |
| Integratsioonitestid | Teenus + andmebaas | Kataloogi otsing ja sorteerimine |
| HTTP-testid | Kogu rakendus, päris päringutega | Autentimine, CSRF, ümbersuunamised |
| Andmeterviklikkuse testid | Algandmete korrektsus | Iga toote pilt on olemas |
| Regressioonitestid | Konkreetne varasem viga | Kaabli ühik on meetrites, mitte tükkides |

### 2.7.2. Põhireegel: test peab suutma läbi kukkuda

Projekti keskne testimispõhimõte on, et **test, mis ei suuda kunagi läbi kukkuda,
on halvem kui testi puudumine**, sest see loob teenimatut kindlustunnet.

Seetõttu kontrolliti iga olulist testi **mutatsioonitestimisega**: koodi rikuti
meelega, veenduti, et test läks punaseks, ja seejärel taastati kood.

Töö käigus tabati kaks juhtumit, kus test **näis** töötavat, aga ei töötanud:

1. Turvaskript kontrollis algselt ainult, et ümbersuunamine **ei lähe** võõrale
   lehele. Tühi vastus rahuldas selle tingimuse — kontroll oli roheline, kuigi
   ei kontrollinud midagi.
2. Test, mis kontrollis avalehel kuvatavat kaabli pikkust, otsis numbrit kogu
   failist. Number esines ka **kommentaaris**, mistõttu test läks läbi ka siis,
   kui nähtav number oli vale.

Mõlemad on kirjas failis `docs/TESTING.md`, sest need on üldistatavad õppetunnid,
mitte ühekordsed apsakad.

`[JOONIS 10]` — Testide käivitamise väljund (212 testi)

### 2.7.3. Automaatne testimine (CI)

GitHub Actions käivitab iga muudatuse järel ehituse ja kõik testid.
Hoiatused on seatud vigadeks (`-warnaserror`), mis tähendab, et hoiatustega kood
ei lähe läbi. Testikomplekt, mis käivitub ainult siis, kui keegi mäletab seda
käivitada, jääb varem või hiljem käivitamata.

## 2.8. Suurimad väljakutsed

Näidistöödes on eraldi peatükk suurimatest väljakutsetest. Alljärgnev ei ole
loetelu ebaõnnestumistest, vaid neljast probleemist, mille lahendamine muutis
lahendust kõige rohkem.

### 2.8.1. Vead, mida kompilaator ei näe

Kõige raskem klass vigu olid need, mille puhul kood oli **süntaktiliselt
korrektne ja loogiliselt vale**. Kolm näidet:

**Terve leht kommentaari sees.** Failis `_Layout.cshtml` oli `@RenderBody()`
väljakutse HTML-kommentaari sees. Razor ei käsitle HTML-kommentaare
kommentaaridena — kood nende sees käivitub —, mistõttu iga lehe sisu renderdati
lõpetamata kommentaari sisse ja `<main>` element jäi tühjaks. Rakendus ehitus
veatult.

**Valge tabel tumedal taustal.** Bootstrapi `.table` seab muutuja
`--bs-table-bg` väärtuseks *Bootstrapi enda* lehetausta, mis on valge ja ei tea
midagi meie teemast. Tulemuseks oli kalkulaatori tulemuste tabel valge plokina
tumedal lehel. Heledas režiimis nägi see juhuslikult õige välja, mistõttu viga
jäi kaua märkamata.

**Nähtamatud pealkirjad heledas režiimis.** Vaadetes oli 38 kohta klassiga
`text-white`, mis on sõna-sõnalt valge. Kui hele režiim lisati, muutusid kõik
need pealkirjad valgeks valgel taustal.

**Õppetund:** kompilaator kontrollib süntaksit, mitte tähendust. Rakendust tuleb
vaadata töötavana, ja seda tuleb teha mõlemas režiimis.

### 2.8.2. Test, mis ei suutnud läbi kukkuda

Turvakontroll ümbersuunamiste kohta kontrollis algselt ainult, et
ümbersuunamine **ei lähe** ründaja lehele. Tühi vastus rahuldab selle tingimuse
täielikult — kontroll oli roheline, kuigi ei kontrollinud midagi.

Sarnane juhtum kordus hiljem: test, mis kontrollis avalehel kuvatavat kaabli
pikkust, otsis numbrit kogu failist. Number esines ka **selgitavas
kommentaaris**, mistõttu test läks läbi ka pärast seda, kui nähtav number oli
meelega valeks muudetud.

**Õppetund:** testi, mis pole kunagi punast näinud, ei saa usaldada. Iga oluline
test tuleb läbida mutatsioonitestimisega — rikkuda kood meelega ja veenduda, et
test seda märkab.

### 2.8.3. Number, mida rakendus ei tootnud

Avaleht reklaamis näidisarvutust: „160 m paigalduskaablit“ maksumusega
„504,10 €“. Kui sama sisendiga arvutus tegelikult käivitati, tagastas rakendus
**120 m ja 348,90 €**. Numbrid olid kunagi käsitsi lehele kirjutatud ja koodist
lahknenud.

Enamikul veebilehtedel oleks see trükiviga. Selles töös on see kõige halvem
võimalik viga, sest töö keskne väide on, et selle arvud on standardist tuletatud
ja kontrollitavad.

Lahendus ei olnud ainult numbri parandamine, vaid **test, mis käivitab päris
kalkulaatori** ja võrdleb tulemust sellega, mida avaleht väidab. Nüüd ei saa
need enam lahku minna.

**Õppetund:** number, mis on kahes kohas, läheb varem või hiljem lahku. Kas
arvuta see või testi seda.

### 2.8.4. Turvalisus kui järelmõte

Rakenduse esimeses versioonis puudus autentimine täielikult, kuigi
`Program.cs` sisaldas `UseAuthorization()` väljakutset. See jättis mulje, et
kaitse on olemas. Tegelikult pääses igaüks tooteid lisama ja kustutama.

**Õppetund:** turvalisust ei saa lõppu jätta, sest see puudutab arhitektuuri.
Rollipõhise ligipääsu lisamine tähendas kontrollerite, vaadete ja andmemudeli
muutmist korraga.

---

# 3. TULEMUSED

## 3.1. Nõuetele vastavus

| Nõue | Täidetud | Märkus |
|---|---|---|
| F1 sisendandmed | ✅ | Vorm valideerib vahemikud |
| F2 ahelate arvutus | ✅ | Ülespoole ümardamine |
| F3 komponentide valik | ✅ | Odavaim laos olev sobiv toode |
| F4 RCD ja kilp | ✅ | Üks kummastki |
| F5 hinnastatud BOM | ✅ | Ühiku-, rea- ja kogusumma |
| F6 ostukorv | ✅ | Tellimust veel ei salvestata |
| F7 kataloog | ✅ | Kategooria, tootja, otsing, sorteerimine |
| F8 administreerimine | ✅ | Rollipõhine ligipääs |
| F9 arvutuste ajalugu | ✅ | |
| M1 eestikeelne | ✅ | |
| M2 brauseripõhine | ✅ | |
| M3 autentimine | ✅ | Identity, kaks rolli |
| M4 turvalisus | ✅ | 18 kontrolli |
| M5 WCAG AA | ✅ | Mõõdetud, 0 puudujääki |
| M6 testid | ✅ | 212 testi, CI |

## 3.2. Mis jäi tegemata

Aus loetelu on hindaja jaoks väärtuslikum kui täiuslikkuse väide:

- **Tellimust ei salvestata.** `Checkout()` ei loo `Order` olemit — seda ei ole
  veel olemas. See on teadlikult järgmise etapi töö.
- **`EvsReference` on tühi.** Vt peatükk 2.4.
- **Hinnad ei ole turuhindadega võrreldud.** Algandmete hinnad on ligikaudsed.
- **Käibemaksu eeldus on kontrollimata.** Seadistus `Pricing:PricesIncludeVat`
  on `true`, kuid keegi ei ole kinnitanud, et algandmete hinnad tõesti sisaldavad
  käibemaksu. See mõjutab ainult seda, mida rakendus **väidab**, mitte arvutust.
- **Mobiilivaade on vähe testitud.**

> `[EDGAR: kui jõuad enne esitamist mõne neist ära teha, tõsta see ülemisse
> tabelisse. Kui ei jõua, jäta siia — see loetelu näitab, et sa tead oma töö
> piire, ja see on kaitsmisel tugevus, mitte nõrkus.]`

## 3.3. Võrdlus algse ajakavaga

> `[EDGAR: täida see peatükk ise — mina ei tea, kui palju aega tegelikult kulus.
> Mall nõuab võrdlust plaaniga. Kirjuta, milline etapp võttis kauem kui plaanitud
> ja miks. Näiteks: turvalisuse parandamine ei olnud algses plaanis eraldi
> etapina, aga võttis reaalselt aega, sest vead leiti alles ülevaatusel.]`

---

# 4. JÄRELDUSED JA EDASIARENDAMISE VÕIMALUSED

## 4.1. Järeldused

Töö eesmärk sai täidetud: valmis toimiv veebirakendus, mis arvutab
elektripaigaldise komponendid ja hinna hoone lihtsate parameetrite põhjal ning
näitab arvutuskäiku.

Kolm olulisemat õppetundi:

1. **Reegel, mida miski ei kontrolli, ei ole reegel.** Värvimuutujate süsteem oli
   õigesti kavandatud, aga 38 kohta läks sellest mööda, sest ükski test ei
   kontrollinud seda.
2. **Test peab suutma läbi kukkuda.** Kaks testi, mis näisid töötavat, ei
   kontrollinud tegelikult midagi.
3. **„See ehitub“ ei ole „see töötab“.** Mitu viga — valge tabel tumedas režiimis,
   avalehe vale number — olid kompilaatorile nähtamatud ja tulid välja alles
   töötavat rakendust vaadates.

## 4.2. Edasiarendamise võimalused

**Lähim etapp:**
1. `Order` ja `OrderLine` olemid ning tellimuse salvestamine.
2. `EvsReference` täitmine kontrollitud standardiviidetega.
3. Makselahendus.

**Kaugem eesmärk — tarnijaga sidumine.** Rakendus võiks tooteid, hindu ja pilte
uuendada automaatselt tarnija andmete põhjal, lisades marginaali. See eeldab
edasimüügilepingut, mis annab ühtlasi õiguse kasutada tootjate tootepilte.
Tehniline pool — ajastatud töö, mis hindu uuendab — on lihtsam osa; keerulisem
on juriidiline ja ärialane pool. Plaan on kirjeldatud failis
`docs/SUPPLIER_SYNC_SPEC.md`.

**Muud võimalused:** mitmekeelsus, kolmefaasiliste paigaldiste tugi, PDF-i
eksport, CAD-plaani import.

---

# KOKKUVÕTE

Käesoleva lõputöö raames valmis veebirakendus „Elektrikilbi ja -tarvete
komponentide kalkulaator“, mis arvutab hoone lihtsate parameetrite põhjal
elektripaigaldise komponentide nimekirja koos maksumusega.

Rakendus on ehitatud ASP.NET Core 9 MVC ja Entity Framework Core 9 baasil,
kasutades neljakihilist arhitektuuri. Arvutusreeglid hoitakse andmebaasis,
mistõttu neid saab muuta rakendust uuesti ehitamata. Lahendust katab 212
automaattesti ja iga muudatust kontrollib automaatne CI.

Töö eristub olemasolevatest lahendustest selle poolest, et **näitab
arvutuskäiku**: iga rea juures on näha, mitmest ahelast kogus tuleb. See muudab
hinnakirja kontrollitavaks tööriistaks.

Töö peamine piirang on, et arvutusreeglite juures ei ole veel täpseid
EVS-HD 60364 punktiviiteid. Need tuleb lisada enne, kui rakendust saab pidada
standardile vastavaks, ja see on esimene edasiarenduse samm.

> `[EDGAR: kokkuvõte peab olema umbes 1 lehekülg ja seda loetakse sageli
> esimesena. Loe see valjusti läbi — kui mõni lause ei kõla nagu sinu oma,
> kirjuta ümber.]`

---

# SUMMARY

**Electrical Panel and Supplies Component Calculator**

This diploma thesis presents a web application that calculates the components
and cost of a low-voltage electrical installation for small buildings —
apartments, private houses and small commercial premises — based on the
Estonian standard EVS-HD 60364.

The user enters simple building parameters: building type, number of rooms,
sockets, lights and switches, and whether an electric stove is present. The
application divides the load into circuits, selects a circuit breaker and cable
cross-section for each, adds a residual current device and an enclosure, and
returns a priced bill of materials.

The distinguishing feature is that the application **shows its reasoning**. Each
line states how many circuits the quantity comes from and which division rule
produced it, which turns a price list into something the user can verify rather
than merely trust.

The application is built with ASP.NET Core 9 MVC and Entity Framework Core 9 on
a four-layer architecture. Calculation rules are stored in the database rather
than in code, so they can be changed without rebuilding. The solution is covered
by 212 automated tests, and every change is built and tested automatically.

The main limitation is that the calculation rules do not yet carry verified
clause references to EVS-HD 60364. Adding them is the first step of further
development, together with order persistence and payment.

**Keywords:** electrical installation, circuit breaker, bill of materials,
ASP.NET Core, EVS-HD 60364, web application

> `[EDGAR: kontrolli juhendajaga, kas ingliskeelne kokkuvõte on nõutav ja kui
> pikk see peab olema. Näidistöödes on see enamasti olemas, tavaliselt
> 150–250 sõna. Kui see on kohustuslik, kontrolli ka võtmesõnade nõuet.]`


---

# JOONISTE PLAAN

| Nr | Joonis | Kust saada |
|---|---|---|
| 1 | Arhitektuuriskeem (4 kihti) | **VALMIS:** `docs/joonised/joonis-1-arhitektuur.svg` |
| 2 | Andmemudeli ERD | **Sul on olemas:** `Elektrikilbi ja -tarvete kalk draft/Elektrikilbi ja -tarvete komponentide kalkulaator .svg` |
| 3 | Kasutusjuhtude diagramm | Joonista draw.io-s |
| 4 | GitHubi pull request'ide vaade | Kuvatõmmis GitHubist |
| 5 | Kalkulaatori vooskeem | **VALMIS:** `docs/joonised/joonis-5-arvutuse-vooskeem.svg` |
| 6 | Avaleht | Kuvatõmmis rakendusest |
| 7 | Tootekataloog | Kuvatõmmis rakendusest |
| 8 | Hele ja tume režiim kõrvuti | Kaks kuvatõmmist |
| 9 | Turvakontrolli väljund | Terminali kuvatõmmis |
| 10 | Testide väljund | Terminali kuvatõmmis |

**Nõuanne joonistele:** näidistöödes on iga joonise all allkiri kujul
„Joonis 1. Rakenduse arhitektuur“ ja igale joonisele viidatakse tekstis
(„nagu on näha jooniselt 1…“). Joonis, millele tekstis ei viidata, mõjub
kaunistusena.

---

# KASUTATUD ALLIKAD

> `[EDGAR: allikad tuleb vormistada kooli malli LISA E („Viitamise näited“)
> järgi. Allpool on nimekiri sellest, millele töö tegelikult tugineb — vormista
> need õigesse kujju ja lisa vaatamise kuupäevad.]`

1. EVS-HD 60364 madalpingelektripaigaldiste standardikogum. Eesti Standardikeskus.
2. Microsoft. ASP.NET Core dokumentatsioon. https://learn.microsoft.com/aspnet/core
3. Microsoft. Entity Framework Core dokumentatsioon. https://learn.microsoft.com/ef/core
4. Bootstrap 5 dokumentatsioon. https://getbootstrap.com
5. xUnit.net dokumentatsioon. https://xunit.net
6. W3C. Web Content Accessibility Guidelines (WCAG) 2.1. https://www.w3.org/TR/WCAG21/
7. Material Design. Dark theme. https://m2.material.io/design/color/dark-theme.html
8. OWASP. Cross-Site Request Forgery Prevention Cheat Sheet. https://cheatsheetseries.owasp.org

> `[EDGAR: lisa siia ka need allikad, mida sa ise kasutasid ja mida mina ei tea —
> eriti elektrotehnika-alased. Kui kasutasid mõnda õpikut või juhendaja
> materjali, kuulub see kindlasti siia.]`

---

# LISAD

| Lisa | Sisu |
|---|---|
| Lisa A | Andmemudeli täielik ERD |
| Lisa B | Kalkulaatori algoritmi pseudokood |
| Lisa C | Testide täielik nimekiri |
| Lisa D | Kuvatõmmised kõigist vaadetest |
| Lisa E | Noorem tarkvaraarendaja kompetentsinõuded (malli kohustuslik lisa) |
| Lisa F | Eksamitöö hindamiskriteeriumid (malli kohustuslik lisa) |

---

## KIRJUTAMISE JÄRJEKORD — soovitus

1. **Täida kõik `[EDGAR: ...]` kohad.** Neid on 8. Ilma nendeta ei ole töö sinu oma.
2. **Kontrolli kõik `[KONTROLLI: ...]` väited.** Neid on 3.
3. **Tee joonised** — vt „Jooniste plaan“. Kõige tähtsamad on 1, 2, 5, 6.
4. **Kopeeri tekst Wordi malli** ja rakenda malli laadid (Pealkiri1, Pealkiri2).
5. **Genereeri sisukord** Wordis automaatselt.
6. **Kirjuta kokkuvõte viimasena**, kui ülejäänu on paigas.
7. **Loe valjusti läbi.** Iga lause, mida sa ei ütleks juhendajale näost näkku,
   kirjuta ümber.
