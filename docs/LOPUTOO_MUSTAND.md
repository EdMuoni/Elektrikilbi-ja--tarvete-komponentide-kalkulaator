# LÕPUTÖÖ MUSTAND — Elektrikilbi ja -tarvete komponentide kalkulaator

> **Tööversioon, mitte lõplik töö.** Kogu tekst on eesti keeles, valmis
> kopeerimiseks Wordi malli `LÕPUTÖÖ_TEMPLATE (1).docx`.
>
> **Struktuur järgib Kalle Olumetsa eksamitööd** (`Eksamitöö_Kalle_Olumets_Cyber_Plan.docx`),
> mis omakorda järgib kooli malli TAR-i haru. Peatükid 1.1–1.6 ja 2.1–2.5 on
> täpselt samas järjekorras ja samade pealkirjadega.
>
> | Märgend | Tähendus |
> |---|---|
> | `[EDGAR: ...]` | Vajab **sinu** otsust või infot, mida mina ei tea. Ära jäta sisse. |
> | `[KONTROLLI: ...]` | Väide, mille pead enne esitamist üle kontrollima. |
> | `[JOONIS n]` | Siia kuulub pilt. Loetelu on faili lõpus. |
>
> **Maht:** Kalle töö on ~6 060 sõna, sellest 1. peatükk ~4 570 sõna. Näidistööd
> on 35–62 lk ja 5 700–8 500 sõna. See mustand on samas suurusjärgus.
>
> **Kõige tähtsam reegel:** ära kirjuta ühtegi EVS-HD 60364 punktinumbrit, mida
> sa pole standardist endast üle kontrollinud. Vt `docs/EVS_ALLIKAD.md`.

---

## SISUKORD

```
SISSEJUHATUS
1. TEOREETILINE TAUST
   1.1. Töö eesmärk, teema valiku põhjendus, olulisus
   1.2. Projekti kavandamine
   1.3. Rakenduse kavandamine
   1.4. Ülevaade praktilise töö teostamisest
   1.5. Tulemused
   1.6. Töö teostamiseks meeskonna koosseis ja ülesannete jaotus
2. LOODUD LAHENDUSE KIRJELDUS
   2.1. Tehtud tööde maht ja arendusplaan
   2.2. Valitud töövahendid
   2.3. Valitud meetodid ja töövõtted
   2.4. Arendusprotsess / etapid ja dokumentatsioon
   2.5. Valminud lahenduse testimine ja kirjeldamine
3. JÄRELDUSED JA SOOVITUSED
4. KOKKUVÕTE
5. KASUTATUD ALLIKAD
LISAD
```

---

## MÕISTED JA LÜHENDID

| Mõiste | Selgitus |
|---|---|
| **ASP.NET Core** | Microsofti avatud lähtekoodiga veebiraamistik. Selles töös versioon 9. |
| **MVC** | *Model-View-Controller* — muster, kus andmed, kasutajaliides ja päringute käsitlemine on eraldatud. |
| **EF Core** | *Entity Framework Core* — objekt-relatsioonvastendus: C# klassid vastendatakse tabeliteks, SQL-i ei kirjutata käsitsi. |
| **Migratsioon** | Genereeritud skript, mis viib andmebaasi struktuuri vastavusse muudetud C# mudeliga. |
| **DTO** | *Data Transfer Object* — lihtne klass andmete liigutamiseks kihtide vahel. |
| **DI** | *Dependency Injection* — sõltuvused antakse klassile konstruktoris ette. Teeb koodi testitavaks. |
| **BOM** | *Bill of Materials* — materjalide loend. Selle töö peamine väljund. |
| **RCD** | Rikkevoolukaitse. Katkestab vooluahela lekkevoolu tuvastamisel. |
| **MCB** | *Miniature Circuit Breaker* — kaitselüliti. |
| **NYM-J** | Levinud paigalduskaabli tüüp, nt NYM-J 3×2,5 mm². |
| **EVS-HD 60364** | Eesti standardikogum madalpingeelektripaigaldiste kohta. |
| **xUnit** | C# testiraamistik. |
| **CI** | *Continuous Integration* — automaatne ehitamine ja testimine iga muudatuse järel. |
| **WCAG** | Veebi ligipääsetavuse juhised. |

---

## SISSEJUHATUS

Hoonete elektrivarustuse projekteerimine on vastutusrikas valdkond, kus iga
otsus peab vastama kehtivatele ohutusnõuetele. Kaitselüliti nimivool, kaabli
ristlõige ja rikkevoolukaitse valik ei ole maitseküsimused — vale valik võib
põhjustada kaabli ülekuumenemise või jätta inimese elektrilöögi eest kaitseta.

Väikeobjektidel — korter, eramu, väike äripind — on nende arvutuste tegemine
siiski ebaproportsionaalselt tülikas. Elektrik või tellija peab kas palkama
eelarvestaja või tegema arvutused käsitsi, otsima iga komponendi hinna eraldi
kokku ja lootma, et kusagil ei tekkinud viga.

Turul on olemas spetsialiseeritud tarkvaralahendused, näiteks ABB ja Schneider
Electricu tootjaspetsiifilised valikutööriistad. Need on suunatud
professionaalsele projekteerijale, seotud ühe tootja tootevalikuga ja eeldavad
kasutajalt erialast ettevalmistust. Lihtsat, eestikeelset ja tootjaneutraalset
vahendit, mis annaks väikeobjektile kohe hinnastatud komponentide nimekirja,
autor ei leidnud.

Käesoleva lõputöö eesmärk on luua veebirakendus **„Elektrikilbi ja -tarvete
komponentide kalkulaator“**, mis võtab sisendiks hoone lihtsad parameetrid ning
tagastab arvutatud komponentide nimekirja koos hindadega.

Töö keskne idee ei ole ainult arvutamine, vaid **arvutuskäigu nähtavaks
tegemine**. Rakendus ei ütle üksnes „kaks kaitselülitit“, vaid näitab, mitmest
ahelast see arv tuleb. See eristab tööd nii kaubanduslikest e-poodidest, mis
müüvad komponente ilma põhjenduseta, kui ka professionaalsetest
projekteerimisvahenditest, mis eeldavad, et kasutaja oskab tulemust ise
kontrollida.

Lõputöö on üles ehitatud järgmiselt. Esimeses peatükis käsitletakse teoreetilist
tausta, projekti ja rakenduse kavandamist ning tulemusi. Teises peatükis
kirjeldatakse valminud lahendust: töövahendeid, meetodeid, arendusprotsessi ja
testimist. Kolmandas peatükis tehakse järeldused ja soovitused.

---

# 1. TEOREETILINE TAUST

Et arendada veebirakendus, mis aitaks väikeobjekti elektripaigaldise komponendid
kiiresti ja kontrollitavalt kokku panna, tuli lahendada järgmised ülesanded:

- **Analüüsida** väikeobjekti elektrikomponentide valiku senist käiku ja
  kaardistada loodava rakenduse funktsionaalsed ning arhitektuursed nõuded.
- **Disainida** andmemudel, mis eraldab arvutusreeglid tootekataloogist, nii et
  kumbagi saaks muuta teist puutumata.
- **Realiseerida** arvutusalgoritm, mis jaotab tarbijad ahelateks ning valib
  igale ahelale kaitselüliti ja kaabli.
- **Koostada** kasutajaliides, mis näitab lisaks tulemusele ka arvutuskäiku.
- **Tagada** rakenduse turvalisus: autentimine, rollipõhine ligipääs ja kaitse
  levinud veebirünnete vastu.
- **Testida** loodud lahenduse funktsionaalsust, turvalisust ja vastavust
  püstitatud eesmärkidele ning dokumenteerida tulemus.

## 1.1. Töö eesmärk, teema valiku põhjendus, olulisus

Eesti ehitusturul tehakse igal aastal suur hulk väikesemahulisi
elektritöid — korterite renoveerimine, eramute ehitus, väikeste äripindade
ümberehitus. Igaüks neist nõuab elektripaigaldise komponentide valikut, ja igaüks
neist on liiga väike, et projekteerimisbüroo teenus oleks majanduslikult
mõistlik. Praktikas tähendab see, et arvutused teeb elektrik ise või jäävad need
üldse tegemata ja komponendid valitakse kogemuse põhjal.

Käesoleva tarkvaraarenduse (TAR) lõputöö teemavalik tulenebki otseselt sellest
vajadusest.

### Kellele lahendus on mõeldud

Rakendusel on kolm eristatavat kasutajarühma, kelle vajadused on erinevad:

**Elektrik või paigaldaja.** Teab, mida ta teeb, kuid tahab säästa aega. Talle
on oluline, et arvutus oleks kiire ja tulemus **kontrollitav** — ta peab nägema,
kust kogus tuleb, sest vastutus paigaldise eest jääb temale. Teda ei huvita ilus
liides, teda huvitab, kas number on õige.

**Tellija või korteriomanik.** Ei tunne elektrotehnikat ja tahab teada, mida töö
ligikaudu maksab, enne kui ta kelleltki pakkumist küsib. Talle on oluline, et
vormi saaks täita ilma erialaste teadmisteta ja et tulemus oleks arusaadav.

**Väikeettevõte või ehitaja.** Vajab kiiret hinnangut mitme objekti kohta. Talle
on oluline, et arvutused säiliksid ja neid saaks hiljem uuesti vaadata.

Nende kolme ühisosa määras rakenduse kuju: **lihtne sisend, selge tulemus ja
nähtav arvutuskäik.**

### Teema valiku põhjendus

Praegune käik on manuaalne. Elektrik või tellija loeb kokku valgustid ja
pistikud, jagab need peast või paberil ahelateks, valib igale ahelale
kaitselüliti ja kaabli ristlõike ning otsib seejärel iga komponendi hinna eraldi
mõne e-poe otsingust. Tulemus kirjutatakse tabelisse või paberile.

Sellel käigul on kolm konkreetset probleemi:

1. **Ajakulu ei ole proportsioonis objekti suurusega.** Sama arvutuskäik tuleb
   korrata iga objekti kohta uuesti, kuigi loogika on identne.
2. **Vigu ei märka keegi.** Käsitsi tehtud arvutuses ei ole kontrollmehhanismi.
   Unustatud rikkevoolukaitse või liiga õhuke kaabel ei anna endast märku enne
   paigaldust — halvimal juhul mitte kunagi.
3. **Tulemus ei ole kontrollitav.** Valmis nimekiri ei näita, kust kogused
   tulid. Tellija peab lihtsalt uskuma.

`[JOONIS 1]` — Näide senisest käsitsi koostatud komponentide nimekirjast

### Olemasolevad lahendused ja nende puudused

Enne arendamist kaardistati, mis turul juba olemas on.

| Lahendus | Tugevus | Puudus selle ülesande jaoks |
|---|---|---|
| **Tootjapõhised valikutööriistad** (ABB, Schneider Electric) | Täpsed, tootja hooldatud | Seotud ühe tootja tootevalikuga. Suunatud professionaalile ja eeldavad erialast ettevalmistust. Ingliskeelsed. Ei anna hinda. |
| **Eesti elektrikaupade e-poed** (Esvika, Onninen, Elektrikaubad) | Reaalsed hinnad ja laoseis, eestikeelsed | Müüvad komponente, ei arvuta koguseid. Kasutaja peab ise teadma, mida ja kui palju osta. |
| **Üldised kaabliarvutuse kalkulaatorid** | Tasuta, kiired | Arvutavad ühe parameetri, mitte tervet paigaldist. Ei anna nimekirja ega hinda. |
| **Käsitsi arvutamine tabelarvutuses** | Täielikult paindlik | Aeganõudev, vigu ei märka keegi. |
| **Projekteerimisbüroo teenus** | Professionaalne, vastutusega | Väikeobjektil ebaproportsionaalselt kallis. |

Võrdlusest nähtub kaks tühimikku. Esiteks: **arvutamise ja ostmise vahel ei ole
silda** — tööriistad, mis arvutavad, ei tea hindu, ja poed, mis teavad hindu, ei
arvuta. Teiseks: **ükski lahendus ei näita arvutuskäiku** viisil, mis lubaks
mitteprofessionaalil tulemust kontrollida.

Käesolev töö asub täpselt nendesse tühimikesse.

> `[KONTROLLI: vaata konkurendid enne esitamist uuesti üle ja kirjuta juurde
> kuupäev, millal võrdlus tehti. Veebipoed ja tootjate tööriistad muutuvad.]`

> `[EDGAR: Kalle kasutab siin Joonis 1-na väljavõtet päris tabelist, millega
> tema kliendid seni tööd tegid. See on väga tugev võte, sest näitab probleemi
> selle asemel et seda kirjeldada.
>
> Kui sul on kuskil päris näide — elektriku tehtud materjalinimekiri, sinu enda
> katse see käsitsi kokku panna, või kasvõi Exceli tabel, mille sa selle töö
> alguses tegid — pane see siia. Kui ei ole, tee üks: võta kolmetoaline korter
> ja kirjuta käsitsi välja, mis sinna vaja läheb. See võtab 20 minutit ja annab
> peatükile 1.1 kõige veenvama joonise kogu töös.]`

### Töö eesmärk

Töö peamine eesmärk on välja töötada toimiv veebirakendus, mis:

1. **kogub sisendandmed** lihtsa vormi kaudu, mida saab täita ka inimene, kes ei
   ole elektrik;
2. **arvutab ahelate arvu ja komponendid** andmebaasis hoitavate
   arvutusreeglite alusel;
3. **koostab hinnastatud materjalide loendi (BOM)** tootekataloogi reaalsete
   toodete ja hindade põhjal;
4. **näitab arvutuskäiku**, nii et iga rea juures on näha, kust arv tuleb;
5. **võimaldab tulemuse ostukorvi lisada**, valmistades ette hilisemat
   e-kaubanduse funktsionaalsust.

### Teema olulisus

Olulisus seisneb kolmes asjas.

**Aja kokkuhoid.** Arvutus, mis käsitsi võtab tunde, valmib minutiga. Kuna
loogika on iga objekti puhul sama, on automatiseerimise tulu vahetu.

**Vigade vältimine.** Automaatne arvutus ei unusta rikkevoolukaitset ega vali
liiga õhukest kaablit, sest reegel rakendatakse alati ja ühtemoodi.

**Läbipaistvus.** Kuna rakendus näitab arvutuskäiku, saab tulemust kontrollida.
See on töö kõige olulisem eristaja: e-pood müüb komponente ilma põhjenduseta,
projekteerimistarkvara eeldab, et kasutaja oskab tulemust ise üle kontrollida,
ja käesolev lahendus asub nende vahel.

> `[EDGAR: kui juhendajaga on kokku lepitud mõõdetav eesmärk — näiteks „kaetud
> on kolm hoonetüüpi“ või „arvutus valmib alla 5 sekundi“ — lisa see siia.
> Mõõdetav eesmärk teeb peatüki 1.5 (Tulemused) kirjutamise palju lihtsamaks.]`

## 1.2. Projekti kavandamine

### Lähteülesanne

Käesoleva tarkvaraarendusprojekti lähteülesanne sõnastati esitatud lõputöö
kavandis: luua kalkulaator, mis aitab klientidel kokku panna elektritarvikud ja
kilbi komponendid korter- ja ärihoonetele, näidates komponente, nende hindu ja
aidates teha sobivaid valikuid.

Loodav süsteem peab tehniliselt toetama arvutusreeglite hoidmist andmebaasis
(mitte koodis), tootekataloogi haldamist administraatori poolt, arvutuste
salvestamist ja hilisemat vaatamist ning tulemuse ostukorvi lisamist.

Valminud rakendus toetab lisaks kavandis nimetatud korter- ja ärihoonele ka
**eramut**. See on väike ja põhjendatud laiendus: arvutusloogika on sama ja
eramu on väikeobjektina sama sagedane kasutusjuht.

### Ülevaade kasutatud tehnoloogiatest ja vahenditest

Tehnoloogiate valikul eelistati õppekavas läbitud, hästi dokumenteeritud ja
serveripoolseks renderdamiseks sobivaid vahendeid. Lõputöö riskikoht ei ole
tehnoloogia uudsus, vaid see, kas töö saab tähtajaks valmis ja töötab.

- **Kasutajaliides ja server: ASP.NET Core 9 MVC.** Serveripoolne renderdamine
  sobib selle rakenduse iseloomuga — kalkulaator teeb ühe arvutuse ja kuvab
  tulemuse. Eraldi üheleherakendus (SPA) ja JavaScripti raamistik lisaksid
  keerukust ilma kasuta. MVC eraldab andmed, kuvamise ja päringukäsitluse, mis
  on eeldus testitavusele.
- **Programmeerimiskeel: C#.** Õppekavas läbitud keel, staatiliselt tüübitud,
  mis püüab suure osa vigadest kinni juba kompileerimisel.
- **Andmebaas: Microsoft SQL Server.** Relatsiooniline andmebaas, mis sobib
  andmemudelile, kus tooted, kategooriad ja arvutusreeglid on omavahel seotud.
- **Andmebaasi abstraktsioonikiht (ORM): Entity Framework Core 9.** Vastendab
  C# klassid tabeliteks, mistõttu SQL-i ei kirjutata käsitsi. Filtreerimine ja
  sorteerimine rakendatakse `IQueryable`-le enne päringu käivitamist, nii et
  need muutuvad SQL-päringu osaks.
- **Andmebaasi migratsioonid: EF Core Migrations.** Tagavad andmebaasi skeemi
  muudatuste versioonitud ja korratava rakendamise igas keskkonnas.
- **Autentimine ja õigused: ASP.NET Core Identity.** Valmis lahendus
  kasutajate, paroolide räsimise ja rollide haldamiseks. Kaks rolli:
  administraator ja klient.
- **Kujundus: Bootstrap 5 ja oma CSS-i muutujad.** Bootstrap annab ruudustiku
  ja komponendid; kõik värvid on ühes failis muutujatena, mis teeb hele/tume
  režiimi võimalikuks.
- **Testimine: xUnit, EF Core InMemory, WebApplicationFactory.** Võimaldavad
  testida nii puhast loogikat, teenuseid koos andmebaasiga kui ka tervet
  rakendust päris HTTP-päringutega.
- **Versioonihaldus ja CI: Git, GitHub, GitHub Actions.** Iga muudatus
  ehitatakse ja testitakse automaatselt.
- **Arenduskeskkond: Visual Studio 2022.** Diagrammide joonistamiseks draw.io.

### Praktilise töö tegevus- ja ajakava

Projekt on kavandatud katma **156 töötunni** nõuet ning on jaotatud agiilsete
põhimõtete järgi lühikesteks, iseseisvalt testitavateks faasideks. Suur
muudatuste pakk teeb vea allika leidmise raskeks; väike muudatus ei tee.

| Faas | Sisu | Maht |
|---|---|---|
| **Faas 0** | Analüüs ja vundament. Nõuete kogumine, elektrotehniliste põhimõtete uurimine, turuülevaade, projekti struktuuri loomine. | ~30 h |
| **Faas 1** | Andmemudel ja andmebaas. Olemite kavandamine, ERD, migratsioonid, algandmed. | ~25 h |
| **Faas 2** | Arvutusalgoritm ja äriloogika. Ahelate jaotus, komponentide valik, BOM-i koostamine. | ~35 h |
| **Faas 3** | Kasutajaliides. Kalkulaatori vorm, tulemuste tabel, tootekataloog, ostukorv. | ~30 h |
| **Faas 4** | Turvalisus ja autentimine. Identity, rollid, CSRF-kaitse, failiüleslaadimise kontroll. | ~16 h |
| **Faas 5** | Testimine ja dokumenteerimine. Automaattestid, turvakontroll, lõputöö. | ~20 h |

> `[EDGAR: kontrolli, kas 156 tundi on õige arv ja kas jaotus vastab tegelikult
> kulunud ajale. Peatükis 1.5 tuleb võrrelda plaani tegelikkusega — see võrdlus
> on malli järgi kohustuslik ja aus vahe on parem kui ilus vale.]`

## 1.3. Rakenduse kavandamine

### Arhitektuur

Rakendus on jaotatud **nelja projekti**, kus iga projekt sõltub ainult
„allpool“ olevatest. Sama kihiline struktuur on kasutusel autori varasemas
kursusetöös `ShopTARge24` — tuttav struktuur vähendab vigu.

```
Core  ←  Data  ←  ApplicationServices  ←  ElektriKalkulaator (veeb)
 ↑                                          ↑
 └──────────── Tests viitab kõigile ────────┘
```

| Projekt | Vastutus |
|---|---|
| **Core** | Domeeniolemid, DTO-d, teenuseliidesed. Ei sõltu millestki. |
| **Data** | `DbContext`, migratsioonid, algandmed. |
| **ApplicationServices** | Teenuste teostused. **Siin elab äriloogika.** |
| **ElektriKalkulaator** | Kontrollerid, Razor vaated, staatilised failid. |
| **Tests** | Viitab kõigile neljale. |

Reegel, mida ei tohi rikkuda: **Core ei sõltu kunagi millestki ja Data ei viita
kunagi veebiprojektile.** Kui see reegel murdub, muutub äriloogika testimine
võimatuks ilma veebiserverit käivitamata.

`[JOONIS 2]` — Rakenduse arhitektuur

### Andmemudel

| Olem | Sisu |
|---|---|
| `Product` | Toode: nimi, tootja, hind, laoseis, nimivool, kaabli ristlõige, pildi tee |
| `ProductCategory` | Tootekategooria |
| `CalculationRule` | Arvutusreegel: hoone tüüp, ahelatüüp, jagaja, ristlõige, nimivool |
| `PowerboxCalculation` | Salvestatud arvutus koos sisendandmetega |
| `PowerboxComponents` | Salvestatud arvutuse üksikread |

`[JOONIS 3]` — Andmemudeli ERD

**Kaks teadlikku otsust, mis väärivad selgitust:**

**`CalculationRule` ei ole `Product`-iga võtmeseoses.** Reegel seotakse hoone
tüübiga sõne järgi ja sobiv toode otsitakse arvutuse ajal kategooria ja
nimivoolu põhjal. Nii saab lisada uue tootja kaitselüliti reegleid puutumata,
muuta reeglit kataloogi puutumata ja valida alati odavaima laos oleva sobiva
toote. Kui reegel viitaks konkreetsele tootele, blokeeriks laost otsa saanud
toode arvutuse.

**`PowerboxComponents` salvestab `UnitPrice`,** st hinna sellisena, nagu see
arvutuse hetkel oli. Kui hind hiljem muutub, ei muutu vana arvutuse summa
tagantjärele. Kogu töö lubadus on, et arvutust saab kontrollida, ja see lubadus
ei kehti, kui esmaspäeval antud hinnapakkumine näitab reedel teist summat.

### Kasutajaliidese kavandamine

Kasutajaliidese kavandamisel analüüsiti kuut rahvusvahelist erialast
veebilehte. Analüüsist selgus, et need jagunevad kaheks vastandlikuks tüübiks:
**turunduslehed** (tume taust, suur pealkiri, vähe elemente, eesmärk veenda) ja
**kataloogilehed** (hele taust, tihe, fotopõhine, eesmärk aidata osa leida).

Käesolev rakendus on mõlemat: avaleht peab veenma kalkulaatorit proovima,
kataloog peab aitama leida konkreetse komponendi. Ühe tüübi rakendamine teisele
on kõige levinum põhjus, miks veebileht „tundub vale“, kuigi iga üksik element
on korras.

Kavandati kaks täielikku värvipaletti — hele ja tume — mille vahel kasutaja saab
valida. Kõik värvid koondati **ühte faili nimetatud muutujatena**, nii et ükski
vaade ei kirjuta värvi väärtust otse. See otsus osutus hiljem oluliseks (vt
peatükk 3, õppetund 1).

### Kasutusjuhud

| Kasutaja | Kasutusjuht |
|---|---|
| Külastaja (sisse logimata) | Arvutab komponendid, sirvib kataloogi, lisab ostukorvi |
| Registreeritud kasutaja | Lisaks: näeb oma arvutuste ajalugu |
| Administraator | Lisaks: haldab tooteid ja kategooriaid |

Teadlik otsus oli, et **kalkulaator on kasutatav ilma kontota**. Registreerumise
nõudmine enne, kui kasutaja on näinud, kas tööriist on üldse kasulik, on kindel
viis kasutajaid kaotada. Konto annab lisaväärtust (ajalugu), mitte ligipääsu.

### Turvalisuse kavandamine

Kuna rakendusel on administraatori funktsioonid ja kasutajate andmed, kavandati
juba alguses:

- **rollipõhine ligipääs** — administraatori lehed on ligipääsmatud ka aadressi
  käsitsi sisestades;
- **CSRF-kaitse** kõigil andmeid muutvatel vormidel;
- **failiüleslaadimise kontroll** toote pildi lisamisel;
- **sisendi valideerimine** nii kliendi kui serveri poolel.

Kui hästi see kavand tegelikkuses realiseerus, on kirjeldatud peatükis 1.4.

## 1.4. Ülevaade praktilise töö teostamisest

### Väljatöötamine

Töö tehti üksinda. Iga muudatus läbis sama tsükli: eraldi haru → väike
muudatus → ehitus ja testid → kontroll töötava rakenduse vastu → kirje
muudatuste päevikusse → pull request.

Töö käigus tekkis neli üksteise peale ehitatud haru, mis liidetakse
järjekorras. See on tavaline viis hoida ühte suurt tööd väiksemateks
ülevaadatavateks tükkideks jaotatuna.

### Arvutusalgoritmi realiseerimine

Algoritm on rakenduse tuum ja töötab järgmiselt:

1. Loe andmebaasist arvutusreeglid antud hoone tüübi kohta.
2. Iga reegli kohta arvuta ahelate arv: tarbijate arv jagatakse reegli jagajaga
   ja tulemus **ümardatakse ülespoole**.
3. Vali kaitselüliti — sobiva kategooria ja nimivooluga odavaim laos olev toode.
4. Vali kaabel — sobiva ristlõikega odavaim laos olev toode; kogus meetrites.
5. Lisa rikkevoolukaitse ja jaotuskilp — üks kummastki paigaldise kohta.
6. Arvuta summad, salvesta arvutus ja tagasta BOM.

Ümardamine ülespoole on oluline detail. Kui valgusteid on 12 ja üks ahel kannab
8 valgustit, on tulemus 1,5 ahelat, mis ümardatakse kaheks. Allapoole
ümardamine tähendaks ülekoormatud ahelat.

`[JOONIS 4]` — Kalkulaatori algoritmi vooskeem

### Arvutusreeglid ja standard

Arvutusreeglid hoitakse andmebaasis, mitte koodis, mistõttu reegli muutmiseks ei
ole vaja rakendust uuesti ehitada.

| Ahelatüüp | Jagaja | Kaabli ristlõige | Nimivool |
|---|---|---|---|
| Valgustus | 1 ahel / 8 valgustit | 1,5 mm² | 10 A |
| Pistikud | 1 ahel / 6 pistikut | 2,5 mm² | 16 A |
| Elektripliit | eraldi ahel | 6,0 mm² | 32 A |

**Siin tuleb teha vahe, mida kaitsmisel kindlasti küsitakse.**

*Standardi põhimõtetest tulenev:* kaabli ristlõike ja kaitselüliti nimivoolu
paar. Alus on liigvoolukaitse põhimõte — kaitseseade peab rakenduma enne, kui
juht üle kuumeneb — mida käsitlevad EVS-HD 60364 osad **4-43** ja **5-52**.
Rikkevoolukaitsme vajadus tuleneb osast **4-41**.

*Projekteerimistava, mitte standardi nõue:* ahelate jaotus, st „üks ahel kaheksa
valgusti kohta“. Standard ei sätesta punktide arvu ahelas — see nõuab, et ahela
koormus mahuks kaabli ja kaitse piiridesse, ja mitu punkti see tähendab, sõltub
punktide võimsusest. Kümme 5 W LED-valgustit ja kümme 150 W valgustit on
täiesti erinev koormus.

Just seetõttu hoitakse neid arve andmebaasis: need on **eeldused**, mida saab
muuta. Täielik allikaanalüüs on failis `docs/EVS_ALLIKAD.md`.

`CalculationRule` väli `EvsReference` on **teadlikult tühi**. Sinna kuuluvad
standardi punktinumbrid, kuid neid ei tohi sisestada enne, kui need on
standardist endast üle kontrollitud.

### Turvalisus

Turvalisus ei olnud algselt piisav ja seda parandati teadlikult:

| Probleem | Tagajärg | Lahendus |
|---|---|---|
| Autentimine puudus | Igaüks pääses administraatori lehtedele | Identity, rollid Admin/Customer |
| Avatud ümbersuunamine | Link sai kasutaja suunata võõrale lehele | `Url.IsLocalUrl` kontroll |
| CSRF-kaitse puudus 5 otspunktil | Võõras leht sai kasutaja nimel tegevusi teha | `[ValidateAntiForgeryToken]` |
| Negatiivne kogus ostukorvis | Summa läks miinusesse | Vahemik 1–999 |
| Nõrk failikontroll | `.exe` sai ümber nimetada `.jpg`-ks | Faili algusbaitide kontroll |
| Ostukorv säilis väljalogimisel | Järgmine kasutaja nägi eelmise korvi | Sessiooni tühjendamine |

### Juurutamine

Rakendus töötab arenduskeskkonnas aadressil `http://localhost:8080`. Andmebaas
luuakse ja täidetakse algandmetega esmakäivitusel automaatselt, mistõttu
rakenduse saab igal masinal tööle panna ühe käsuga.

> `[EDGAR: kui jõuad rakenduse kuhugi avalikult üles panna (nt Azure’i
> tasuta astmele), kirjuta see siia — mall nimetab juurutamist eraldi. Kui ei
> jõua, kirjuta aus lause: „Rakendust ei ole avalikku keskkonda juurutatud;
> see on kavandatud edasiarendusena.“]`

## 1.5. Tulemused

### Nõuetele vastavus

| Nõue | Täidetud | Märkus |
|---|---|---|
| Sisendandmete kogumine | ✅ | Vorm valideerib vahemikud |
| Ahelate arvutus | ✅ | Ülespoole ümardamine |
| Komponentide valik | ✅ | Odavaim laos olev sobiv toode |
| RCD ja kilp | ✅ | Üks kummastki |
| Hinnastatud BOM | ✅ | Ühiku-, rea- ja kogusumma |
| Arvutuskäigu näitamine | ✅ | Ahelatüüp ja ristlõige iga rea juures |
| Ostukorv | ✅ | Tellimust veel ei salvestata |
| Tootekataloog | ✅ | Kategooria, tootja, otsing, sorteerimine |
| Administreerimine | ✅ | Rollipõhine ligipääs |
| Arvutuste ajalugu | ✅ | |
| Eestikeelne liides | ✅ | |
| Turvalisus | ✅ | 18 kontrolli läbib |
| WCAG AA kontrast | ✅ | Mõõdetud, 0 puudujääki |
| Automaattestid | ✅ | 218 testi, CI |

### Loodud lahenduse praktiline väärtus

Näidisarvutus kolmetoalise korteri kohta (3 tuba, 10 pistikut, 12 valgustit,
elektripliit) annab 8-realise nimekirja kogumaksumusega **348,90 €**. Arvutus
valmib sekundi murdosaga; sama töö käsitsi tähendaks komponentide valikut ja
kaheksa hinna eraldi otsimist.

`[JOONIS 5]` — Kalkulaatori tulemus

Arvutuse täielik väljund on järgmine:

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
10 ÷ 6 = 1,67 → 2. Kaablit on kokku 120 m: viis ahelat, iga ahela kohta 24 m.

> `[KONTROLLI: see tabel on rakendusest tegelikult saadud väljund. Kui muudad
> hindu või reegleid, käivita arvutus uuesti ja uuenda tabelit. Töös, mille
> lubadus on auditeeritavus, ei tohi näidistabel tegelikkusest erineda.]`

### Testimise tulemused

Rakendust katab 218 automaattesti, mis kõik läbivad. Lisaks kontrolliti
funktsionaalsust käsitsi töötava rakenduse vastu:

| Kontrollitud | Tulemus |
|---|---|
| Kõik kolm hoonetüüpi annavad BOM-i | korterelamu 8 rida, eramu 8 rida, ärihoone 6 rida |
| Piirjuhud | 0 tuba annab valideerimisvea; 100 tuba / 200 pistikut arvutab korrektselt |
| SQL-i süstimine ja XSS | ei anna tulemust, kataloog jääb terveks |
| Vigased identifikaatorid | 404 |
| Ostukorvi piirid | kogus 0 ja 99999 lükatakse tagasi |
| Administraatori lehed ilma sisselogimiseta | suunatakse sisselogimislehele |
| Vale parool | ei paljasta, kas konto on olemas |
| Arvutuse salvestamine ajalukku | toimib |
| Turvaskript (18 kontrolli) | kõik läbivad |
| WCAG AA kontrast (34 paari) | ükski ei jää alla nõude |

Testimise käigus leiti üks tõeline viga: **elektripliidi märkeruut kuvati kõigi
hoonetüüpide juures**, kuigi ärihoone jaoks ei olnud pliidiahela arvutusreeglit
määratud. Selle märkimine ärihoone puhul ei lisanud pliidiahelat ega andnud
kasutajale ka mingit teadet — vorm lubas vaikimisi midagi, mida ta ei täitnud.

Viga parandati nii, et rakendus **ütleb selle välja**: kui pliidiahelat ei saa
lisada, kuvatakse tulemuse juures põhjendus. Väljamõeldud reegli lisamine oleks
olnud halvem, sest ärihoonel ei pruugigi olla kodust pliidiahelat. Kalkulaator,
mille mõte on arvutuskäigu näitamine, peab näitama ka seda, mida ta **ei
teinud**.

### Võrdlus esialgse tegevus- ja ajakavaga

> `[EDGAR: täida see ise — mina ei tea, kui palju aega tegelikult kulus. Mall
> nõuab võrdlust plaaniga. Kirjuta, milline faas võttis kauem kui plaanitud ja
> miks. Aus näide: turvalisuse parandamine ei olnud algses plaanis eraldi
> faasina, aga võttis reaalselt aega, sest vead leiti alles ülevaatusel.]`

### Mis jäi tegemata

Aus loetelu on hindaja jaoks väärtuslikum kui täiuslikkuse väide:

- **Tellimust ei salvestata.** `Checkout()` tühjendab ostukorvi, kuid `Order`
  olemit veel ei ole. Teadlikult järgmise etapi töö.
- **`EvsReference` on tühi.** Vt eespool.
- **Hinnad ei ole turuhindadega võrreldud.** Algandmete hinnad on ligikaudsed.
- **Käibemaksu eeldus on kontrollimata.** Seadistus `Pricing:PricesIncludeVat`
  on `true`, kuid keegi ei ole kinnitanud, et algandmete hinnad tõesti
  sisaldavad käibemaksu. See mõjutab ainult seda, mida rakendus **väidab**.
- **Mobiilivaade on vähe testitud.**
- **Ärihoone tüübil puudub pliidiahela reegel.** Rakendus teatab sellest
  kasutajale, kuid reeglit ennast ei ole.

## 1.6. Töö teostamiseks meeskonna koosseis ja ülesannete jaotus

**Meeskond:** Edgar Muoni (üksinda).

**Ülesanded:** nõuete analüüs, andmemudeli ja arhitektuuri kavandamine,
arvutusalgoritmi realiseerimine, kasutajaliidese kujundus ja programmeerimine,
turvalisuse tagamine, automaattestide kirjutamine, dokumentatsiooni koostamine
ja lõputöö kirjutamine.

---

# 2. LOODUD LAHENDUSE KIRJELDUS

## 2.1. Tehtud tööde maht ja arendusplaan

Töö kogumaht on 156 tundi, jaotatuna kuueks faasiks (vt peatükk 1.2).
Arendusplaan järgis põhimõtet, et iga faasi lõpus peab rakendus olema
töötavas seisus — mitte, et kõik osad valmivad korraga ja liidetakse lõpus.

Praktikas tähendas see näiteks, et andmemudel ja algandmed valmisid enne
arvutusalgoritmi, ja arvutusalgoritm enne kasutajaliidest. Nii sai iga osa
testida siis, kui see valmis, mitte alles siis, kui kõik oli koos.

Arendus toimus **haruderivatsiooni põhimõttel**: iga tööpakett tehti eraldi
harus ja liideti pull request'i kaudu, mitte otse peaharusse. Töö käigus tekkis
neli üksteise peale ehitatud haru:

| Haru | Sisu |
|---|---|
| `fix/track-seeded-product-images` | Algandmete piltide parandus |
| `feat/upload-validation-messages` | Failiüleslaadimise veateated vormil |
| `fix/security-hardening` | Autentimine, rollid, CSRF, sisendi valideerimine |
| `feat/conversion-ux` | Kasutajaliides, kataloog, kujundus |

Selline jaotus ei ole formaalsus. Iga haru on eraldi ülevaadatav tervik, ja kui
mõnes neist ilmneb hiljem viga, on selge, milline muudatuste kogum selle
põhjustas. Üks suur muudatuste pakk seda ei võimalda.

`[JOONIS 6]` — GitHubi pull request'ide ja commit'ide vaade

## 2.2. Valitud töövahendid

| Vahend | Otstarve |
|---|---|
| Visual Studio 2022 | Peamine arenduskeskkond |
| SQL Server Management Studio | Andmebaasi vaatamine ja kontroll |
| Git ja GitHub | Versioonihaldus, harud, pull request'id |
| GitHub Actions | Automaatne ehitamine ja testimine |
| draw.io | ERD ja arhitektuuriskeemid |
| Veebibrauseri arendajatööriistad | Kasutajaliidese ja kontrastide kontroll |
| Claude (tehisintellekt) | Koodiülevaatus, testide kirjutamise abi, dokumentatsioon |

**Töövahendite valiku põhimõte** oli, et vahend peab olema kas õppekavas
läbitud või laialt kasutatav ja hästi dokumenteeritud. Lõputöö ajal uue vahendi
õppimine suurendab riski, et töö ei valmi tähtajaks.

Eraldi väärib mainimist **automaatne ehitamine ja testimine (CI)**. GitHub
Actions käivitab iga muudatuse järel ehituse ja kõik testid. Hoiatused on seatud
vigadeks, mis tähendab, et hoiatustega kood ei lähe läbi. See ei ole
mugavusvahend, vaid distsipliini tagamise vahend: käsitsi käivitatav
testikomplekt jääb varem või hiljem käivitamata.

> `[EDGAR: Kalle mainib oma töös tehisintellekti kasutamist avalikult ja
> põhjendatult. Kontrolli juhendajaga, kuidas ta soovib seda kirjeldatuna
> näha — ja kirjuta aus lause selle kohta, mida tegi AI ja mida sina.]`

## 2.3. Valitud meetodid ja töövõtted

**Agiilne, väikeste sammudega arendus.** Töö jaotati väikesteks, iseseisvalt
testitavateks muudatusteks. Iga muudatus ehitati, testiti ja alles siis liideti.

**Kihiline arhitektuur.** Neli projekti, sõltuvused ainult ühes suunas. See ei
ole korrastatuse küsimus, vaid eeldus testitavusele.

**Andmepõhised reeglid.** Arvutusreeglid on andmebaasis, mitte koodis.

**Muudatuste päevik.** Iga koodimuudatuse kohta kirjutati kirje, kus on kirjas
**miks**, mitte ainult mis. Kood näitab ise, mis muutus; põhjus on ainuke asi,
mida hiljem enam kuskilt ei leia.

**Testimine kui osa arendusest, mitte lõpuetapp.** Vt peatükk 2.5.

**Kontrollimine töötava rakenduse vastu.** Projektis kehtib reegel, et „see
ehitub“ ei ole „see töötab“. Iga muudatus kontrolliti käivitatud rakenduse
vastu, mitte üksnes kompilaatori abil. See ei ole liigne ettevaatus: mitu selle
töö vigadest — valge tabel tumedas režiimis, avalehel kuvatud vale number,
märkeruut, mis midagi ei teinud — olid kompilaatorile täiesti nähtamatud.

**Andmete ja koodi kooskõla kontrollimine.** Kuna algandmed (tooted,
kategooriad, arvutusreeglid) elavad koodis ja viitavad välistele failidele,
lisati kontroll, et need viited ka päriselt kehtivad. Selle vajadus tuli
praktikast: versioonihalduse reegel välistas tootepildid, samal ajal kui
algandmed neile viitasid, mistõttu värskelt alla laaditud koopias oli kümme
katkist pilti.

## 2.4. Arendusprotsess / etapid ja dokumentatsioon

Projektis on eraldi dokumentatsioonikaust `docs/`:

| Fail | Sisu |
|---|---|
| `CHANGELOG.md` | Iga koodimuudatus ja selle põhjus. Ei kirjutata kunagi ümber. |
| `PROJECT_ROADMAP.md` | Praegune seis ja plaanid |
| `TESTING.md` | Testimise metoodika |
| `DESIGN_GUIDE.md` | Disainisüsteem ja põhjendused |
| `RESEARCH_LOG.md` | Väljastpoolt kogutud faktid: turuhinnad, konkurentide analüüs |
| `IMAGE_CREDITS.md` | Iga pildi päritolu ja litsents |
| `EVS_ALLIKAD.md` | Standardi allikad ja aus hinnang vastavusele |
| `KOODI_SELGITUS.md` | Koodi selgitus eesti keeles |

Selline dokumentatsioon ei ole bürokraatia. Selle väärtus oli konkreetne: kui
projekti juurde naasti nädalaid hiljem, sai lugeda, **miks** mingi lahendus on
selline, selle asemel et seda koodist tagasi tuletada — või valesti tuletada ja
„ära parandada“ midagi, mis oli tahtlik.

Projektis on eraldi loetelu asjadest, mis **näevad välja nagu vead, kuid on
tahtlikud** — näiteks see, et arvutusreegel ei ole tootega võtmeseoses, ja et
ostukorvi vormistamine ei salvesta veel tellimust. Ilma sellise loeteluta
„parandab“ järgmine arendaja need ära ja rikub sellega läbimõeldud lahenduse.

### Arendusetapid praktikas

Arendus järgis peatükis 1.2 kirjeldatud faase, kuid tegelikkuses ei olnud
üleminekud järsud. Näiteks turvalisuse faas (Faas 4) algas alles siis, kui
kasutajaliides oli suures osas valmis — ja just see järjekord osutus veaks. Kui
selgus, et autentimine puudus täielikult, tuli muuta korraga kontrollereid,
vaateid ja andmemudelit. Turvalisus puudutab arhitektuuri, mistõttu selle
lõppu jätmine tähendab hilisemat ümbertegemist.

See on üks konkreetsemaid õppetunde kogu tööst ja on kirjas ka peatükis 3.

## 2.5. Valminud lahenduse testimine ja kirjeldamine

### Testide arv ja liigid

Rakendust katab **218 automaattesti**:

| Liik | Mida kontrollib |
|---|---|
| Ühiktestid | Puhas loogika, ilma andmebaasita |
| Integratsioonitestid | Teenus koos andmebaasiga |
| HTTP-testid | Kogu rakendus päris päringutega: autentimine, CSRF, ümbersuunamised |
| Andmeterviklikkuse testid | Algandmete korrektsus, nt iga toote pilt on olemas |
| Regressioonitestid | Konkreetne varasem viga |

`[JOONIS 7]` — Testide käivitamise väljund

### Põhireegel: test peab suutma läbi kukkuda

Projekti keskne testimispõhimõte on, et **test, mis ei suuda kunagi läbi
kukkuda, on halvem kui testi puudumine**, sest see loob teenimatut
kindlustunnet.

Seetõttu kontrolliti iga olulist testi **mutatsioonitestimisega**: koodi rikuti
meelega, veenduti, et test läks punaseks, ja seejärel taastati kood.

Töö käigus tabati kaks juhtumit, kus test näis töötavat, aga ei töötanud:

1. Turvakontroll kontrollis algselt ainult, et ümbersuunamine **ei lähe**
   ründaja lehele. Tühi vastus rahuldas selle tingimuse — kontroll oli
   roheline, kuigi ei kontrollinud midagi.
2. Test, mis kontrollis avalehel kuvatavat kaabli pikkust, otsis numbrit kogu
   failist. Number esines ka **kommentaaris**, mistõttu test läks läbi ka siis,
   kui nähtav number oli vale.

### Turvalisuse testimine

Turvakontrollide kordamiseks on skript `scripts/security-check.sh`, mis
käivitab kõik rünnakukatsed töötava rakenduse vastu. Kõik **18 kontrolli**
läbivad.

Lisaks kontrolliti käsitsi töötava rakenduse vastu: SQL-i süstimise ja XSS-i
katsed ei anna tulemust ja kataloog jääb terveks, vigased identifikaatorid
annavad 404, ostukorv keeldub kogusest 0 ja 99999, vale parool ei paljasta,
kas konto on olemas.

`[JOONIS 8]` — Turvakontrolli skripti väljund

### Ligipääsetavus

Kontrastisuhted mõõdeti skriptiga `scripts/check-contrast.py`. Kõik teksti ja
tausta paarid mõlemas režiimis ületavad WCAG AA nõude 4,5:1.

„Värvid valiti hoolikalt“ on arvamus; „mõõdeti 34 paari, madalaim 4,55, ükski
ei jää alla AA nõude“ on kontrollitav fakt.

### Automaatne testimine (CI)

GitHub Actions käivitab iga muudatuse järel ehituse ja kõik testid. Hoiatused on
seatud vigadeks, mis tähendab, et hoiatustega kood ei lähe läbi.

---

# 3. JÄRELDUSED JA SOOVITUSED

## Järeldused

Töö eesmärk sai täidetud: valmis toimiv veebirakendus, mis arvutab
elektripaigaldise komponendid ja hinna hoone lihtsate parameetrite põhjal ning
näitab arvutuskäiku.

Kolm olulisemat õppetundi:

1. **Reegel, mida miski ei kontrolli, ei ole reegel.** Värvimuutujate süsteem oli
   õigesti kavandatud, aga 38 kohta läks sellest mööda, sest ükski test ei
   kontrollinud seda.
2. **Test peab suutma läbi kukkuda.** Kaks testi, mis näisid töötavat, ei
   kontrollinud tegelikult midagi.
3. **„See ehitub“ ei ole „see töötab“.** Mitu viga — valge tabel tumedas
   režiimis, avalehe vale number — olid kompilaatorile nähtamatud ja tulid
   välja alles töötavat rakendust vaadates.

## Soovitused ja edasiarendamise võimalused

**Lähim etapp:**
1. `Order` ja `OrderLine` olemid ning tellimuse salvestamine.
2. `EvsReference` täitmine kontrollitud standardiviidetega.
3. Makselahendus.

**Kaugem eesmärk — tarnijaga sidumine.** Rakendus võiks tooteid, hindu ja pilte
uuendada automaatselt tarnija andmete põhjal, lisades marginaali. See eeldab
edasimüügilepingut, mis annab ühtlasi õiguse kasutada tootjate tootepilte.
Tehniline pool on lihtsam osa; keerulisem on juriidiline ja ärialane pool.

**Muud võimalused:** mitmekeelsus, kolmefaasiliste paigaldiste tugi, PDF-eksport,
CAD-plaani import.

---

# 4. KOKKUVÕTE

Käesoleva lõputöö raames valmis veebirakendus „Elektrikilbi ja -tarvete
komponentide kalkulaator“, mis arvutab hoone lihtsate parameetrite põhjal
elektripaigaldise komponentide nimekirja koos maksumusega.

Rakendus on ehitatud ASP.NET Core 9 MVC ja Entity Framework Core 9 baasil,
kasutades neljakihilist arhitektuuri. Arvutusreeglid hoitakse andmebaasis,
mistõttu neid saab muuta rakendust uuesti ehitamata. Lahendust katab 218
automaattesti ja iga muudatust kontrollib automaatne CI.

Töö eristub olemasolevatest lahendustest selle poolest, et **näitab
arvutuskäiku**: iga rea juures on näha, mitmest ahelast kogus tuleb. See muudab
hinnakirja kontrollitavaks tööriistaks.

Töö peamine piirang on, et arvutusreeglite juures ei ole veel täpseid
EVS-HD 60364 punktiviiteid, ja et osa arvutusreegleid on projekteerimistava,
mitte standardi nõue. Mõlemad on töös selgelt välja toodud, ja punktiviidete
lisamine on esimene edasiarenduse samm.

---

# 5. KASUTATUD ALLIKAD

> `[EDGAR: vormista kooli malli LISA E („Viitamise näited“) järgi ja lisa
> vaatamise kuupäevad. Kalle kasutab allviiteid (footnote) — kontrolli, kas
> juhendaja eelistab seda või lõpuloetelu.]`

1. Eesti Standardimis- ja Akrediteerimiskeskus. EVS-HD 60364 standardikogum.
   <https://www.evs.ee>
2. EVS-HD 60364-5-52. Madalpingelised elektripaigaldised. Osa 5-52:
   Elektriseadmete valik ja paigaldamine. Juhistikud.
3. Microsoft. ASP.NET Core dokumentatsioon. <https://learn.microsoft.com/aspnet/core>
4. Microsoft. Entity Framework Core dokumentatsioon. <https://learn.microsoft.com/ef/core>
5. Bootstrap 5 dokumentatsioon. <https://getbootstrap.com>
6. xUnit.net dokumentatsioon. <https://xunit.net>
7. W3C. Web Content Accessibility Guidelines (WCAG) 2.1. <https://www.w3.org/TR/WCAG21/>
8. OWASP. Cross-Site Request Forgery Prevention Cheat Sheet.
   <https://cheatsheetseries.owasp.org>

---

# LISAD

| Lisa | Sisu |
|---|---|
| Lisa A | Andmemudeli täielik ERD |
| Lisa B | Kalkulaatori algoritmi pseudokood |
| Lisa C | Kuvatõmmised kõigist vaadetest |
| Lisa D | Noorem tarkvaraarendaja kompetentsinõuded (malli kohustuslik lisa) |
| Lisa E | Eksamitöö hindamiskriteeriumid (malli kohustuslik lisa) |

---

# JOONISTE PLAAN

| Nr | Joonis | Kust saada |
|---|---|---|
| 1 | Näide senisest käsitsi tehtud nimekirjast | **Tee ise** — vt märkus peatükis 1.1. Kõige veenvam joonis kogu töös. |
| 2 | Rakenduse arhitektuur | **VALMIS:** `docs/joonised/joonis-1-arhitektuur.svg` |
| 3 | Andmemudeli ERD | **Sul on olemas:** `Elektrikilbi ja -tarvete kalk draft/…kalkulaator .svg` |
| 4 | Kalkulaatori algoritmi vooskeem | **VALMIS:** `docs/joonised/joonis-5-arvutuse-vooskeem.svg` |
| 5 | Kalkulaatori tulemus | Kuvatõmmis rakendusest |
| 6 | GitHubi pull request'ide vaade | Kuvatõmmis GitHubist |
| 7 | Testide väljund | Terminali kuvatõmmis |
| 8 | Turvakontrolli väljund | Terminali kuvatõmmis |

**Nõuanne:** iga joonise alla käib allkiri kujul „Joonis 1 – Rakenduse
arhitektuur“ ja igale joonisele viidatakse tekstis. Joonis, millele tekstis ei
viidata, mõjub kaunistusena.

---

## KIRJUTAMISE JÄRJEKORD

1. **Täida kõik `[EDGAR: ]` kohad.** Ilma nendeta ei ole töö sinu oma.
2. **Tee Joonis 1** — käsitsi tehtud nimekirja näide. See on kõige tähtsam.
3. **Kontrolli `[KONTROLLI: ]` väited.**
4. **Kopeeri tekst Wordi malli** ja rakenda malli laadid (Pealkiri1, Pealkiri2).
5. **Genereeri sisukord** Wordis automaatselt.
6. **Kirjuta kokkuvõte viimasena.**
7. **Loe valjusti läbi.** Iga lause, mida sa ei ütleks juhendajale näost näkku,
   kirjuta ümber.
