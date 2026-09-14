# ElektriKalkulaator

**Elektrikilbi ja -tarvete komponentide kalkulaator**

Veebirakendus, mis koostab hoone põhiandmete põhjal elektripaigaldise **hinnaga materjalide
loendi**. Loendis on kaitselülitid, kaabli kogus, rikkevoolukaitse ja kilbi korpus. Arvutus lähtub
Eesti standardist **EVS-HD 60364** ja hinnad võetakse päris tootekataloogist. Sama tööd teeb
tavaliselt eelarvestaja.

Kasutaja sisestab hoone tüübi, tubade, pistikute ja valgustite arvu ning märgib, kas hoones on
elektripliit. Rakendus jagab tarbijad ahelateks ja valib igale ahelale odavaima laos oleva sobiva
kaitselüliti ja kaabli. Iga materjali rea juures on näha ahela tüüp ja kaabli ristlõige, nii et
tulemust saab kontrollida.

**Lõputöö:** Edgar Muoni, rühm TARge24, eriala noorem tarkvaraarendaja, Tallinna Tehnoloogiakolledž
(Techno TLN)
**Juhendaja:** Kalle Olumets

---

## Autor ja tehisintellekti kasutamine

Rakenduse autor on **Edgar Muoni** ja töö on tehtud üksinda.

Arenduse ajal kasutasin abivahendina Anthropicu keelemudelit Claude, peamiselt agentkeskkonnas
Claude Code. Kasutasin seda eelkõige vigade ja nende põhjuste otsimisel, koodi ülevaatamisel ning
automaattestide kirjutamisel. Lisaks kasutasin seda dokumentatsiooni ja lõputöö teksti koostamisel.

Seepärast on GitHubi kaastööliste (ingl *Contributors*) loendis ka `claude`. Sisestuse
(ingl *commit*) kirjelduse lõpus olev rida `Co-Authored-By: Claude` tähistab muudatusi, mille juures
kasutasin tehisintellekti abi. Muudatuste päevikus [docs/CHANGELOG.md](docs/CHANGELOG.md) on see
märgitud iga kirje juures (väli *Author*).

Nõuded, arhitektuuri valikud, andmemudel ja arvutusreeglid on minu otsused. Iga soovituse
kontrollisin ise töötava rakenduse ja testidega. Tehisintellekti kasutamist kirjeldan täpsemalt
lõputöö peatükis 1.6.

Rakenduses endas tehisintellekti ei kasutata. Arvutus põhineb andmebaasis olevatel reeglitel ja annab
sama sisendi korral alati sama tulemuse.

---

## Käivitamine

Vaja on .NET 9 SDK-d ja SQL Serverit. Sobib ka LocalDB, mis paigaldatakse koos Visual Studioga.

```bash
cd ElektriKalkulaator
dotnet run --project ElektriKalkulaator
```

Seejärel ava brauseris <http://localhost:8080>. Esimesel käivitamisel luuakse andmebaas
automaatselt ning sinna lisatakse 10 näidistoodet ja arvutusreeglid.

### Valikuline seadistus

`appsettings.json` kasutab vaikimisi LocalDB-d, nii et äsja kloonitud projekt käivitub ilma
seadistamata. Kui soovid kasutada teist SQL Serverit või luua administraatori konto, kasuta
User Secrets hoidlat. **Ära muuda versioonihalduses olevat faili** ja ära kirjuta sinna kunagi
parooli:

```bash
cd ElektriKalkulaator/ElektriKalkulaator
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<sinu ühendusstring>"
dotnet user-secrets set "AdminUser:Email" "sina@example.com"
dotnet user-secrets set "AdminUser:Password" "<tugev parool>"
```

Rakendus töötab ka ilma administraatori seadeteta. Sel juhul administraatorit ei looda ja logisse
kirjutatakse selle kohta hoiatus.

## Testimine

```bash
cd ElektriKalkulaator
dotnet test ElektriKalkulaator.Tests/ElektriKalkulaator.Tests.csproj
```

Rakendust katab 218 automaattesti. GitHub Actions käivitab need pärast iga sisestust automaatselt
(vt vahekaart *Actions*).

Lisaks on olemas turvakontrolli skript, mis kontrollib töötavat rakendust. Käivita kõigepealt
rakendus ja seejärel repositooriumi juurkaustas:

```bash
bash docs/scripts/security-check.sh
```

Skript kordab kõiki turvaülevaatuses leitud rünnaku katseid, mis puudutavad autentimist,
CSRF-kaitset (ingl *antiforgery*), avatud ümbersuunamist ja sisendi kontrolli. Kui mõni kontroll
ebaõnnestub, lõpetab skript töö veakoodiga.

## Ülesehitus

```
ElektriKalkulaator/
  ElektriKalkulaator.Core                  domeenimudelid, DTO-d, teenuste liidesed  (ei sõltu millestki)
  ElektriKalkulaator.Data                  DbContext, migratsioonid, algandmed       (sõltub Core'ist)
  ElektriKalkulaator.ApplicationServices   teenuste realisatsioonid                  (Core + Data)
  ElektriKalkulaator                       veebirakendus                             (kõik eelnevad)
  ElektriKalkulaator.Tests                 automaattestid                            (viitab kõigile neljale)
docs/                                      dokumentatsioon ja abiskriptid
.github/workflows/                         automaatne ehitamine ja testimine (GitHub Actions)
```

Tehnoloogiad: ASP.NET Core 9 MVC, Entity Framework Core 9, SQL Server, *Bootstrap* 5 ja
ASP.NET Core Identity.

## Teadaolevad piirangud

- Arvutusreeglite juures ei ole veel EVS-HD 60364 punktiviiteid ja ahelate jaotus põhineb
  projekteerimistaval, mitte standardi nõudel. Vt [docs/EVS_ALLIKAD.md](docs/EVS_ALLIKAD.md).
- Kaabli pikkus on hinnanguline, mitte arvutatud.
- Ostukorv on olemas, kuid tellimust ei salvestata. Tellimuse vormistamine on edasiarenduse töö.
- Rakendus töötab praegu ainult kohalikus arenduskeskkonnas.

## Dokumentatsioon

Kõik dokumendid ja abiskriptid on kaustas [`docs/`](docs/). Enamik dokumente on inglise keeles.

| Dokument | Sisu |
|---|---|
| [KOODI_SELGITUS.md](docs/KOODI_SELGITUS.md) | Koodi tööpõhimõtte selgitus eesti keeles |
| [EVS_ALLIKAD.md](docs/EVS_ALLIKAD.md) | Standardi allikad ja hinnang, mil määral rakendus seda järgib |
| [CHANGELOG.md](docs/CHANGELOG.md) | Iga koodimuudatus ja selle põhjus |
| [PROJECT_ROADMAP.md](docs/PROJECT_ROADMAP.md) | Arhitektuur, otsused ja plaanid (viimati uuendatud 14.08.2026) |
| [TESTING.md](docs/TESTING.md) | Testimise metoodika |
| [DESIGN_GUIDE.md](docs/DESIGN_GUIDE.md) | Disainisüsteem ja lehtede kujunduse juhised |
| [RESEARCH_LOG.md](docs/RESEARCH_LOG.md) | Väljastpoolt kogutud faktid: turuhinnad, konkurentide analüüs, kasutajaliidese uuring |
| [IMAGE_CREDITS.md](docs/IMAGE_CREDITS.md) | Iga pildi päritolu ja litsents |
| [TEST_ACCOUNTS.md](docs/TEST_ACCOUNTS.md) | Näidiskontod (administraator ja klient) rakenduse proovimiseks |
| [VOICE_AND_PERSONALITY.md](docs/VOICE_AND_PERSONALITY.md) | Veebilehe tekstide toon ja stiil |
| [SUPPLIER_SYNC_SPEC.md](docs/SUPPLIER_SYNC_SPEC.md) | Plaan tarnijate hindade automaatseks sünkroonimiseks (ei ole realiseeritud) |
| [CLAUDE.md](docs/CLAUDE.md) | Projekti juhend tehisintellektile (Claude Code): reeglid, käsud ja tahtlikud lahendused, mis võivad näida vigadena |
| [joonised/](docs/joonised/) | Lõputöö arhitektuuriskeemi ja vooskeemi lähtefailid (SVG) |
| [scripts/](docs/scripts/) | Turvakontrolli skript ning jooniste ja värviteema genereerimise skriptid |

## Litsents ja pildiallikad

Tootefotod on vaba litsentsiga (Wikimedia Commons CC0 / CC BY-SA, Unsplash, Pexels) ja iga pildi
allikas on kirjas failis [docs/IMAGE_CREDITS.md](docs/IMAGE_CREDITS.md). Tootjate fotosid ei
kasutata; põhjus on kirjas samas failis.
