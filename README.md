# ElektriKalkulaator

**Elektrikilbi ja -tarvete komponentide kalkulaator**

Veebirakendus, mis koostab hoone põhiandmete põhjal elektripaigaldise **hinnaga materjalide
loendi**. Loendis on kaitselülitid, kaabli kogus, rikkevoolukaitse ja kilbi korpus. Arvutus lähtub
Eesti standardist **EVS-HD 60364** ja tulemus hinnastatakse päris tootekataloogi järgi. Sama tööd
teeb tavaliselt eelarvestaja.

Kasutaja sisestab hoone tüübi, tubade, pistikute ja valgustite arvu ning selle, kas on elektripliit.
Rakendus jagab tarbijad ahelateks, valib igale ahelale odavaima laos oleva sobiva kaitselüliti ja
kaabli ning näitab iga materjali rea juures ahelatüüpi ja kaabli ristlõiget, nii et tulemust saab
kontrollida.

**Lõputöö:** Edgar Muoni, rühm TARge24, eriala noorem tarkvaraarendaja, Tallinna Tehnoloogiakolledž
(Techno TLN)
**Juhendaja:** Kalle Olumets

---

## Autor ja tehisintellekti kasutamine

Rakenduse autor on **Edgar Muoni** ja töö on tehtud üksinda.

Arenduse ajal kasutasin abivahendina Anthropicu keelemudelit Claude, peamiselt agentkeskkonnas
Claude Code. Peamiselt kasutasin seda vigade ja nende põhjuste otsimisel, koodi ülevaatamisel ning
automaattestide kirjutamisel. Lisaks kasutasin seda dokumentatsiooni ja lõputöö teksti koostamisel.

Seepärast on GitHubi kaastööliste (ingl *Contributors*) loendis ka `claude`. Sisestuse
(ingl *commit*) lõpus olev rida `Co-Authored-By: Claude` tähistab muudatusi, mille juures kasutasin
tehisintellekti abi. Muudatuste päevikus [docs/CHANGELOG.md](docs/CHANGELOG.md) on see märgitud iga
kirje juures (väli *Author*).

Nõuded, arhitektuuri valikud, andmemudel ja arvutusreeglid on minu otsused. Iga soovituse
kontrollisin ise töötava rakenduse ja testidega. Tehisintellekti kasutamist kirjeldan täpsemalt
lõputöö peatükis 1.6.

Rakenduse enda sees tehisintellekti ei ole. Arvutus põhineb andmebaasis olevatel reeglitel ja annab
sama sisendi korral alati sama tulemuse.

---

## Käivitamine

Vaja on .NET 9 SDK-d ja SQL Serverit. Sobib ka LocalDB, mis tuleb koos Visual Studioga.

```bash
cd ElektriKalkulaator
dotnet run --project ElektriKalkulaator
```

Seejärel ava brauseris <http://localhost:8080>. Esimesel käivitamisel luuakse andmebaas
automaatselt ja sinna lisatakse 10 näidistoodet ning arvutusreeglid.

### Valikuline seadistus

`appsettings.json` kasutab vaikimisi LocalDB-d, nii et värskelt kloonitud projekt käivitub ilma
seadistamata. Teise SQL Serveri kasutamiseks või administraatori konto loomiseks kasuta User
Secrets hoidlat. **Ära muuda versioonihalduses olevat faili** ja ära kirjuta sinna kunagi parooli:

```bash
cd ElektriKalkulaator/ElektriKalkulaator
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<sinu ühendusstring>"
dotnet user-secrets set "AdminUser:Email" "sina@example.com"
dotnet user-secrets set "AdminUser:Password" "<tugev parool>"
```

Ilma administraatori seadeteta töötab rakendus samuti. Administraatorit siis ei looda ja logisse
kirjutatakse selle kohta hoiatus.

## Testimine

```bash
cd ElektriKalkulaator
dotnet test ElektriKalkulaator.Tests/ElektriKalkulaator.Tests.csproj
```

Rakendust katab 218 automaattesti. GitHub Actions käivitab need iga sisestuse järel automaatselt
(vt vahekaart *Actions*).

Lisaks on turvakontrolli skript, mis töötab käivitatud rakenduse vastu. Käivita kõigepealt rakendus
ja seejärel:

```bash
bash scripts/security-check.sh
```

Skript kordab kõiki turvaülevaatuses leitud rünnaku katseid: autentimine, CSRF-kaitse
(ingl *antiforgery*), avatud ümbersuunamine ja sisendi kontroll. Kui mõni neist ei läbi, lõpetab
skript veakoodiga.

## Ülesehitus

```
ElektriKalkulaator.Core     domeenimudelid, DTO-d, teenuste liidesed   (ei sõltu millestki)
ElektriKalkulaator.Data     DbContext, migratsioonid, algandmed        (sõltub Core'ist)
...ApplicationServices      teenuste realisatsioonid                   (Core + Data)
ElektriKalkulaator          veebirakendus                              (kõik eelnevad)
ElektriKalkulaator.Tests    automaattestid                             (kõik neli projekti)
```

Tehnoloogiad: ASP.NET Core 9 MVC, Entity Framework Core 9, SQL Server, *Bootstrap* 5 ja
ASP.NET Core Identity.

## Teadaolevad piirangud

- Arvutusreeglite juures ei ole veel EVS-HD 60364 punktiviiteid ja ahelate jaotus põhineb
  projekteerimistaval, mitte standardi nõudel. Vt [docs/EVS_ALLIKAD.md](docs/EVS_ALLIKAD.md).
- Kaabli pikkus on hinnanguline, mitte arvutatud.
- Tellimust ei salvestata. Ostukorv on olemas, aga vormistamine on edasiarendus.
- Rakendus töötab praegu ainult kohalikus arenduskeskkonnas.

## Dokumentatsioon

Kõik dokumendid on kaustas [`docs/`](docs/). Enamik neist on inglise keeles.

| Dokument | Sisu |
|---|---|
| [KOODI_SELGITUS.md](docs/KOODI_SELGITUS.md) | Koodi tööpõhimõtte selgitus eesti keeles |
| [EVS_ALLIKAD.md](docs/EVS_ALLIKAD.md) | Standardi allikad ja aus hinnang, kui palju rakendus seda järgib |
| [CHANGELOG.md](docs/CHANGELOG.md) | Iga koodimuudatus ja selle põhjus |
| [PROJECT_ROADMAP.md](docs/PROJECT_ROADMAP.md) | Arhitektuur, otsused ja plaanid (viimati uuendatud 14.08.2026) |
| [TESTING.md](docs/TESTING.md) | Testimise metoodika |
| [DESIGN_GUIDE.md](docs/DESIGN_GUIDE.md) | Disainisüsteem ja lehtede kujunduse juhised |
| [RESEARCH_LOG.md](docs/RESEARCH_LOG.md) | Väljast kogutud faktid: turuhinnad, konkurentide analüüs, kasutajaliidese uuring |
| [IMAGE_CREDITS.md](docs/IMAGE_CREDITS.md) | Iga pildi päritolu ja litsents |
| [TEST_ACCOUNTS.md](docs/TEST_ACCOUNTS.md) | Näidiskontod (administraator ja klient) rakenduse proovimiseks |
| [VOICE_AND_PERSONALITY.md](docs/VOICE_AND_PERSONALITY.md) | Kuidas veebileht kasutajaga räägib |
| [SUPPLIER_SYNC_SPEC.md](docs/SUPPLIER_SYNC_SPEC.md) | Tulevase tarnijate hinnasünkrooni plaan (ei ole realiseeritud) |
| [PROMPTS.md](docs/PROMPTS.md) | Juhised selle projekti kallal tehisintellektiga töötamiseks |
| [joonised/](docs/joonised/) | Lõputöö arhitektuuri- ja vooskeemi joonise lähtefailid (SVG) |
| [LOPUTOO_MUSTAND.md](docs/LOPUTOO_MUSTAND.md) | **Vana mustand, mitte lõputöö.** Lõplik töö on Wordi dokument |

[`CLAUDE.md`](CLAUDE.md) asub repositooriumi juurkaustas, sest Claude Code loeb selle sealt
automaatselt. See on juhend tehisintellektile ja viitab ülalolevatele dokumentidele.

## Litsents ja pildiallikad

Tootefotod on vaba litsentsiga (Wikimedia Commons CC0 / CC BY-SA, Unsplash, Pexels) ja iga pildi
allikas on kirjas failis [docs/IMAGE_CREDITS.md](docs/IMAGE_CREDITS.md). Tootjate fotosid ei kasutata;
põhjus on samas failis.
