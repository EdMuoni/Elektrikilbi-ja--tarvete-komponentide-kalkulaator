# EVS-HD 60364 — allikad ja aus hinnang

> **Uuritud 2026-09-09. Loe see läbi enne kaitsmist.**
>
> See fail vastab kahele küsimusele:
> 1. **Kus standard päriselt on** ja mis see maksab.
> 2. **Kas rakendus järgib seda** — ausalt, ilma ilustamata.
>
> Teine vastus on osaliselt „ei“, ja see on parem teada praegu kui kaitsmisel.

---

## 1. Kus standard on

Standardit müüb **Eesti Standardimis- ja Akrediteerimiskeskus**: <https://www.evs.ee>

**Tähtis arusaam:** EVS-HD 60364 **ei ole üks dokument**. See on
standardikogum, mis koosneb kümnetest eraldi ostetavatest osadest. Lause
„vastab EVS-HD 60364-le“ on seetõttu umbmäärane — tuleb nimetada osa.

### Selle töö jaoks olulised osad

| Osa | Pealkiri (eesti k) | Mida see katab | Miks meile oluline |
|---|---|---|---|
| **EVS-HD 60364-5-52** | Madalpingelised elektripaigaldised. Osa 5-52: Elektriseadmete valik ja paigaldamine. Juhistikud | Kaablid, nende valik ja koormatavus | **Kaabli ristlõike valik** |
| **EVS-HD 60364-4-43** | Kaitse liigvoolu eest | Kaitselüliti ja kaabli kokkusobivus | **Kaitselüliti nimivool vs kaabel** |
| **EVS-HD 60364-4-41** | Kaitse elektrilöögi eest | Rikkevoolukaitsme nõuded | **Miks RCD on vajalik** |
| **EVS-HD 60364-5-54** | Maandus ja kaitsejuhid | Maandus | Taust |
| **EVS-HD 60364-6** | Kontrollimine | Paigaldise kontroll | Taust |

### Hind

Kontrollitud lehelt <https://www.evs.ee/et/evs-hd-60364-5-52-2011-a1-2025>:
**muudatus A1:2025 üksi maksab 12,40 € (koos käibemaksuga)**, PDF või paber.

**Ettevaatust:** see on **ainult muudatus**, mitte terve standard. Terviktekst
(EVS-HD 60364-5-52:2011+A11+A12+A1:2025) on eraldi toode ja kallim.

> `[EDGAR: kontrolli, kas koolil on EVS-i ligipääs. Paljudel kutseõppeasutustel
> on leping, mis annab õpilastele standardid tasuta või soodsalt. Küsi
> raamatukogust või juhendajalt ENNE ostmist — see võib säästa sadu eurosid.]`

---

## 2. Kas rakendus järgib standardit? Aus vastus

Rakenduse reeglid jagunevad **kaheks** ja neid ei tohi ühte patta panna.

### 2.1. See, mis tuleneb standardi põhimõtetest ✅

| Reegel rakenduses | Alus |
|---|---|
| Valgustusahel: 1,5 mm² kaabel + 10 A kaitse | Liigvoolukaitse põhimõte: kaitselüliti nimivool ei tohi ületada kaabli koormatavust. Osad **4-43** ja **5-52**. |
| Pistikuahel: 2,5 mm² kaabel + 16 A kaitse | Sama põhimõte. |
| Pliidiahel: 6,0 mm² kaabel + 32 A kaitse, eraldi ahel | Sama põhimõte; suur püsikoormus eraldatakse. |
| Rikkevoolukaitse paigaldises | Osa **4-41** nõuab lisakaitset elektrilöögi eest. |

Need paarid — 1,5 mm² → 10 A, 2,5 mm² → 16 A — on rahvusvahelises praktikas
laialt kasutusel ja järgivad standardi loogikat: **kaitse peab rakenduma enne,
kui kaabel üle kuumeneb.**

### 2.2. See, mis EI ole standardis ⚠️ — kõige tähtsam osa siin failis

| Reegel rakenduses | Tegelik staatus |
|---|---|
| **1 ahel iga 8 valgusti kohta** | **Ei ole standardi nõue.** See on projekteerimistava. |
| **1 ahel iga 6 pistiku kohta** | **Ei ole standardi nõue.** Sama. |
| **24 m kaablit ahela kohta** | **Ei ole standardi nõue.** See on hinnanguline eeldus. |

Standard **ei ütle**, mitu valgustit tohib ühes ahelas olla. Standard nõuab, et
ahela koormus ei ületaks kaabli ja kaitse võimekust — **mitu punkti see tähendab,
sõltub sellest, kui suured need punktid on.** Kümme 5 W LED-valgustit ja kümme
150 W halogeenvalgustit on täiesti erinev koormus.

Lähim asi standardimaailmas on Briti BS 7671 **informatiivne** lisa 55A, mis
soovitab valgustusahelas maksimaalselt 10 punkti. **Informatiivne tähendab, et
see ei ole nõue**, vaid hea tava soovitus.

### 2.3. Mida see praktikas tähendab

Praegu ütleb rakendus mitmes kohas „EVS-HD 60364 järgi“ ka nende arvude kohta,
mis standardist ei tulene. **See on ülepakkumine ja kaitsmisel ohtlik**, sest
esimene küsimus on täpselt see: „kus standardis see 8 kirjas on?“

**Õige sõnastus:**

| Ära ütle | Ütle |
|---|---|
| „Kogused arvutatakse EVS-HD 60364 järgi“ | „Kaabli ja kaitse valik järgib EVS-HD 60364 põhimõtteid; ahelate jaotus põhineb projekteerimistaval“ |
| „Vastab standardile“ | „Lähtub standardi osadest 4-41, 4-43 ja 5-52“ |
| „1 ahel 8 valgusti kohta (standard)“ | „1 ahel 8 valgusti kohta (eeldus, muudetav andmebaasis)“ |

**See ei nõrgesta tööd.** Vastupidi: teadmine, kus standard lõpeb ja eeldus
algab, on täpselt see, mida elektriprojekteerimises oodatakse. Töö, mis ütleb
„need arvud on eeldused, siin nad on ja neid saab muuta“, on tugevam kui töö,
mis väidab valesti, et kõik tuleb standardist.

---

## 3. Mida teha, kui saad standardi kätte

1. **Ava osa 5-52** ja otsi kaablite koormatavuse tabelid (paigaldusviiside
   kaupa). Kirjuta üles tabeli number ja veerg, mille põhjal 1,5 mm² annab
   vähemalt 10 A ja 2,5 mm² vähemalt 16 A **selle paigaldusviisi juures, mida
   sinu töö eeldab**.
2. **Ava osa 4-43** ja otsi liigvoolukaitse tingimus (kaitseseadme nimivool ≤
   juhi koormatavus). Kirjuta punktinumber üles.
3. **Ava osa 4-41** ja otsi rikkevoolukaitset puudutav nõue.
4. **Täida `CalculationRule.EvsReference`** väli nende punktinumbritega.
5. **Ahelate jaotuse (8 ja 6) juurde ära pane standardiviidet.** Kirjuta
   selle asemel lahtrisse midagi taolist: „projekteerimiseeldus, ei tulene
   standardist“.

---

## 4. Kaitsmisel — valmis vastus

Kui küsitakse „kas see vastab standardile?“, ütle umbes nii:

> „Kaabli ristlõike ja kaitselüliti paar tuleneb EVS-HD 60364 osadest 4-43 ja
> 5-52 — kaitse peab rakenduma enne, kui kaabel üle kuumeneb. Rikkevoolukaitse
> nõue tuleb osast 4-41.
>
> Ahelate jaotus — üks ahel kaheksa valgusti kohta — ei ole standardi nõue, vaid
> projekteerimiseeldus. Standard nõuab, et ahela koormus mahuks kaabli ja kaitse
> piiridesse; kui mitu punkti see tähendab, sõltub punktide võimsusest. Just
> sellepärast hoian ma neid arve andmebaasis, mitte koodis — need on eeldused,
> mida saab muuta ilma rakendust uuesti ehitamata.“

See vastus on aus, näitab, et sa tead vahet nõude ja tava vahel, ja pöörab
võimaliku nõrkuse arhitektuurseks põhjenduseks.

---

## 5. Allikad

| Allikas | Link |
|---|---|
| Eesti Standardimis- ja Akrediteerimiskeskus | <https://www.evs.ee> |
| EVS-HD 60364-5-52 (juhistikud) | <https://www.evs.ee/et/evs-hd-60364-5-52-2011-a1-2025> |
| EVS-HD 60364-1 (üldpõhimõtted) | <https://www.evs.ee/tooted/evs-hd-60364-1-2008+a11-2017> |
| IEC 60364 ülevaade | <https://en.wikipedia.org/wiki/IEC_60364> |
| IEC 60364-5-52 (rahvusvaheline vaste) | <https://webstore.iec.ch/en/publication/1878> |

> **Hoiatus:** ülaltoodud tabelid ja väited põhinevad standardi **avalikel
> kirjeldustel ja tööstuspraktikal**, mitte standardi tekstil endal — mul ei ole
> standardile ligipääsu. Enne töö esitamist tuleb punktid 1–3 peatükis 3 ise üle
> kontrollida. Ükski punktinumber selles failis ei ole välja mõeldud, sest
> ühtegi punktinumbrit siin ei ole.
