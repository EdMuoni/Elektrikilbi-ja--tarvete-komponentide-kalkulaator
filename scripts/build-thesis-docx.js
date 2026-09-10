/*
 * Builds a NEW version of Edgar's thesis as .docx.
 * The original "Elektrikilbi ja -tarvete komponentide kalkulaator.docx" is never touched.
 *
 * Formatting follows LÕPUTÖÖ_TEMPLATE (1).docx: 12 pt body, H1 20 pt, H2 16 pt,
 * 2.5 cm margins. Structure follows Eksamitöö_Kalle_Olumets_Cyber_Plan.docx.
 */
const fs = require("fs");
const path = require("path");
const {
  Document, Packer, Paragraph, TextRun, HeadingLevel, AlignmentType,
  ImageRun, PageBreak, Table, TableRow, TableCell, WidthType, ShadingType,
  BorderStyle, TableOfContents, Footer, PageNumber, convertMillimetersToTwip,
} = require("docx");

const PIC = String.raw`C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\Pictures`;
const DRAFT = String.raw`C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\Elektrikilbi ja -tarvete kalk draft`;
const OUT = String.raw`C:\Users\Jazztime\Desktop\TARge24\LÕPUTÖÖ\Elektrikilbi ja -tarvete komponentide kalkulaator_v2_UUS.docx`;

const FONT = "Times New Roman";
const SZ = 24;            // 12 pt (half-points)
const LINE = 360;         // 1.5 line spacing

// ---------- helpers -------------------------------------------------------
const p = (text, opts = {}) =>
  new Paragraph({
    spacing: { line: LINE, after: opts.after ?? 120 },
    alignment: opts.align,
    indent: opts.indent,
    bullet: opts.bullet,
    numbering: opts.numbering,
    children: [new TextRun({ text, font: FONT, size: opts.size ?? SZ, bold: opts.bold, italics: opts.italics, color: opts.color })],
  });

// A paragraph built from parts, so a single line can mix bold and normal text.
const rich = (parts, opts = {}) =>
  new Paragraph({
    spacing: { line: LINE, after: opts.after ?? 120 },
    alignment: opts.align,
    bullet: opts.bullet,
    children: parts.map((x) =>
      typeof x === "string"
        ? new TextRun({ text: x, font: FONT, size: SZ })
        : new TextRun({ text: x.t, font: FONT, size: SZ, bold: x.b, italics: x.i })),
  });

const h1 = (text) =>
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    spacing: { before: 360, after: 200, line: LINE },
    children: [new TextRun({ text, font: FONT, size: 40, bold: true })],
  });

const h2 = (text) =>
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    spacing: { before: 300, after: 160, line: LINE },
    children: [new TextRun({ text, font: FONT, size: 32, bold: true })],
  });

const h3 = (text) =>
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    spacing: { before: 240, after: 120, line: LINE },
    children: [new TextRun({ text, font: FONT, size: 26, bold: true })],
  });

const bullet = (text) => p(text, { bullet: { level: 0 }, after: 60 });

// A note for Edgar. Deliberately loud so it cannot be left in by accident.
const todo = (text) =>
  new Paragraph({
    spacing: { line: LINE, before: 120, after: 160 },
    shading: { type: ShadingType.CLEAR, fill: "FFF2CC" },
    children: [new TextRun({ text: "TÄITA — " + text, font: FONT, size: 22, italics: true, color: "7F6000" })],
  });

// Figure + caption. Width is capped so a wide screenshot still fits the page.
let figNo = 0;
function figure(relPath, caption, maxWidth = 580, root) {
  const file = path.join(root || PIC, relPath);
  const data = fs.readFileSync(file);
  const dim = pngSize(data);
  let w = Math.min(maxWidth, dim.w);
  let h = Math.round((dim.h / dim.w) * w);
  const MAX_H = 620;                 // keeps a tall screenshot on one page
  if (h > MAX_H) { w = Math.round((dim.w / dim.h) * MAX_H); h = MAX_H; }
  figNo += 1;
  return [
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { before: 200, after: 60 },
      children: [new ImageRun({ data, type: "png", transformation: { width: w, height: h } })],
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { after: 200 },
      children: [new TextRun({ text: `Joonis ${figNo}. ${caption}`, font: FONT, size: 20, italics: true })],
    }),
  ];
}

// Minimal PNG header reader — avoids pulling in an image library.
function pngSize(buf) {
  return { w: buf.readUInt32BE(16), h: buf.readUInt32BE(20) };
}

// Table with the dual widths docx requires.
function table(rows, widths) {
  const total = widths.reduce((a, b) => a + b, 0);
  return new Table({
    columnWidths: widths,
    width: { size: total, type: WidthType.DXA },
    rows: rows.map((cells, r) =>
      new TableRow({
        tableHeader: r === 0,
        children: cells.map((c, i) =>
          new TableCell({
            width: { size: widths[i], type: WidthType.DXA },
            shading: r === 0 ? { type: ShadingType.CLEAR, fill: "E8E8E8" } : undefined,
            margins: { top: 60, bottom: 60, left: 100, right: 100 },
            children: [new Paragraph({
              spacing: { line: 240, after: 0 },
              children: [new TextRun({ text: String(c), font: FONT, size: 20, bold: r === 0 })],
            })],
          })),
      })),
  });
}

const children = [];
const add = (...xs) => xs.flat().forEach((x) => children.push(x));

// ======================= TIITELLEHT ======================================
add(
  p("Tallinna Tööstushariduskeskus", { align: AlignmentType.CENTER, bold: true }),
  p("Noorem tarkvaraarendaja", { align: AlignmentType.CENTER }),
  p("TARge24", { align: AlignmentType.CENTER, after: 1200 }),
  new Paragraph({ spacing: { after: 600 }, children: [] }),
  p("ELEKTRIKILBI JA -TARVETE KOMPONENTIDE KALKULAATOR",
    { align: AlignmentType.CENTER, bold: true, size: 32, after: 200 }),
  p("Lõputöö", { align: AlignmentType.CENTER, italics: true, after: 1600 }),
  new Paragraph({ spacing: { after: 1200 }, children: [] }),
  p("Koostaja: Edgar Muoni", { align: AlignmentType.CENTER }),
  p("Juhendaja: Kalle Olumets", { align: AlignmentType.CENTER, after: 1200 }),
  p("Tallinn 2026", { align: AlignmentType.CENTER }),
  new Paragraph({ children: [new PageBreak()] }),
);

// ======================= AUTORIDEKLARATSIOON =============================
add(
  h1("AUTORIDEKLARATSIOON"),
  p("Deklareerin, et käesolev lõputöö, mis on minu iseseisva töö tulemus, on esitatud Tallinna Tööstushariduskeskuse lõputunnistuse taotlemiseks noorem tarkvaraarendaja erialal."),
  p("Lõputöö alusel ei ole varem eriala lõputunnistust taotletud.", { after: 400 }),
  p("Autor: Edgar Muoni\t\t(allkirjastatud digitaalselt)", { after: 400 }),
  p("Töö vastab kehtivatele nõuetele.", { after: 400 }),
  p("Juhendaja: Kalle Olumets\t\t(allkirjastatud digitaalselt)"),
  new Paragraph({ children: [new PageBreak()] }),
);

// ======================= SISUKORD ========================================
add(
  h1("SISUKORD"),
  new TableOfContents("Sisukord", { hyperlink: true, headingStyleRange: "1-3" }),
  todo("Sisukord genereeritakse Wordis automaatselt: klõpsa siia ja vali Viited → Uuenda tabelit → Uuenda kogu tabel."),
  new Paragraph({ children: [new PageBreak()] }),
);

// ======================= MÕISTED =========================================
add(
  h1("MÕISTED JA LÜHENDID"),
  table([
    ["Mõiste", "Selgitus"],
    ["ASP.NET Core", "Microsofti avatud lähtekoodiga veebiraamistik. Selles töös versioon 9."],
    ["MVC", "Model-View-Controller — muster, kus andmed, kasutajaliides ja päringute käsitlemine on eraldatud."],
    ["EF Core", "Entity Framework Core — objekt-relatsioonvastendus: C# klassid vastendatakse andmebaasitabeliteks."],
    ["Migratsioon", "Genereeritud skript, mis viib andmebaasi struktuuri vastavusse muudetud C# mudeliga."],
    ["DTO", "Data Transfer Object — lihtne klass andmete liigutamiseks kihtide vahel."],
    ["DI", "Dependency Injection — sõltuvused antakse klassile konstruktoris ette. Teeb koodi testitavaks."],
    ["BOM", "Bill of Materials — materjalide loend. Selle töö peamine väljund."],
    ["RCD", "Rikkevoolukaitse. Katkestab vooluahela lekkevoolu tuvastamisel."],
    ["MCB", "Miniature Circuit Breaker — kaitselüliti."],
    ["NYM-J", "Levinud paigalduskaabli tüüp, nt NYM-J 3×2,5 mm²."],
    ["EVS-HD 60364", "Eesti standardikogum madalpingeelektripaigaldiste kohta."],
    ["CSRF", "Cross-Site Request Forgery — rünne, kus võõras leht saadab päringu kasutaja nimel."],
    ["xUnit", "C# testiraamistik."],
    ["CI", "Continuous Integration — automaatne ehitamine ja testimine iga muudatuse järel."],
    ["WCAG", "Web Content Accessibility Guidelines — veebi ligipääsetavuse juhised."],
  ], [2200, 6800]),
  new Paragraph({ children: [new PageBreak()] }),
);

// ======================= SISSEJUHATUS ====================================
add(
  h1("SISSEJUHATUS"),
  p("Hoonete elektrivarustuse projekteerimine on vastutusrikas valdkond, kus iga otsus peab vastama kehtivatele ohutusnõuetele. Kaitselüliti nimivool, kaabli ristlõige ja rikkevoolukaitse valik ei ole maitseküsimused — vale valik võib põhjustada kaabli ülekuumenemise või jätta inimese elektrilöögi eest kaitseta."),
  p("Väikeobjektidel — korter, eramu, väike äripind — on nende arvutuste tegemine siiski ebaproportsionaalselt tülikas. Elektrik või tellija peab kas palkama eelarvestaja või tegema arvutused käsitsi, otsima iga komponendi hinna eraldi kokku ja lootma, et kusagil ei tekkinud viga."),
  p("Turul on olemas spetsialiseeritud tarkvaralahendused, näiteks ABB ja Schneider Electricu tootjaspetsiifilised valikutööriistad. Need on suunatud professionaalsele projekteerijale, seotud ühe tootja tootevalikuga ja eeldavad kasutajalt erialast ettevalmistust. Lihtsat, eestikeelset ja tootjaneutraalset vahendit, mis annaks väikeobjektile kohe hinnastatud komponentide nimekirja, autor ei leidnud."),
  rich([{ t: "Käesoleva lõputöö eesmärk on luua veebirakendus " }, { t: "„Elektrikilbi ja -tarvete komponentide kalkulaator“", b: true }, { t: ", mis võtab sisendiks hoone lihtsad parameetrid ning tagastab arvutatud komponentide nimekirja koos hindadega." }]),
  rich([{ t: "Töö keskne idee ei ole ainult arvutamine, vaid " }, { t: "arvutuskäigu nähtavaks tegemine", b: true }, { t: ". Rakendus ei ütle üksnes „kaks kaitselülitit“, vaid näitab, mitmest ahelast see arv tuleb. See eristab tööd nii kaubanduslikest e-poodidest, mis müüvad komponente ilma põhjenduseta, kui ka professionaalsetest projekteerimisvahenditest, mis eeldavad, et kasutaja oskab tulemust ise kontrollida." }]),
  p("Lõputöö on üles ehitatud järgmiselt. Esimeses peatükis käsitletakse teoreetilist tausta: töö eesmärki, projekti ja rakenduse kavandamist, praktilise töö teostamist ning tulemusi. Teises peatükis kirjeldatakse valminud lahendust: tööde mahtu, töövahendeid, meetodeid, arendusprotsessi ja testimist. Kolmandas peatükis tehakse järeldused ja soovitused."),
  new Paragraph({ children: [new PageBreak()] }),
);

// ======================= 1. TEOREETILINE TAUST ===========================
add(
  h1("1. TEOREETILINE TAUST"),
  p("Et arendada veebirakendus, mis aitaks väikeobjekti elektripaigaldise komponendid kiiresti ja kontrollitavalt kokku panna, tuli lahendada järgmised ülesanded:"),
  bullet("Analüüsida väikeobjekti elektrikomponentide valiku senist käiku ja kaardistada loodava rakenduse funktsionaalsed ning arhitektuursed nõuded."),
  bullet("Disainida andmemudel, mis eraldab arvutusreeglid tootekataloogist, nii et kumbagi saaks muuta teist puutumata."),
  bullet("Realiseerida arvutusalgoritm, mis jaotab tarbijad ahelateks ning valib igale ahelale kaitselüliti ja kaabli."),
  bullet("Koostada kasutajaliides, mis näitab lisaks tulemusele ka arvutuskäiku."),
  bullet("Tagada rakenduse turvalisus: autentimine, rollipõhine ligipääs ja kaitse levinud veebirünnete vastu."),
  bullet("Testida loodud lahenduse funktsionaalsust ja turvalisust ning dokumenteerida tulemus."),
);

// ---- 1.1 ----------------------------------------------------------------
add(
  h2("1.1. Töö eesmärk, teema valiku põhjendus, olulisus"),
  p("Eesti ehitusturul tehakse igal aastal suur hulk väikesemahulisi elektritöid — korterite renoveerimine, eramute ehitus, väikeste äripindade ümberehitus. Igaüks neist nõuab elektripaigaldise komponentide valikut, ja igaüks neist on liiga väike, et projekteerimisbüroo teenus oleks majanduslikult mõistlik. Praktikas tähendab see, et arvutused teeb elektrik ise või jäävad need üldse tegemata ja komponendid valitakse kogemuse põhjal."),
  p("Käesoleva tarkvaraarenduse (TAR) lõputöö teemavalik tulenebki otseselt sellest vajadusest."),

  h3("Kellele lahendus on mõeldud"),
  p("Rakendusel on kolm eristatavat kasutajarühma, kelle vajadused on erinevad."),
  rich([{ t: "Elektrik või paigaldaja.", b: true }, { t: " Teab, mida ta teeb, kuid tahab säästa aega. Talle on oluline, et arvutus oleks kiire ja tulemus kontrollitav — ta peab nägema, kust kogus tuleb, sest vastutus paigaldise eest jääb temale." }]),
  rich([{ t: "Tellija või korteriomanik.", b: true }, { t: " Ei tunne elektrotehnikat ja tahab teada, mida töö ligikaudu maksab, enne kui ta kelleltki pakkumist küsib. Talle on oluline, et vormi saaks täita ilma erialaste teadmisteta ja et tulemus oleks arusaadav." }]),
  rich([{ t: "Väikeettevõte või ehitaja.", b: true }, { t: " Vajab kiiret hinnangut mitme objekti kohta. Talle on oluline, et arvutused säiliksid ja neid saaks hiljem uuesti vaadata." }]),
  rich([{ t: "Nende kolme ühisosa määras rakenduse kuju: " }, { t: "lihtne sisend, selge tulemus ja nähtav arvutuskäik.", b: true }]),

  h3("Teema valiku põhjendus"),
  p("Praegune käik on manuaalne. Elektrik või tellija loeb kokku valgustid ja pistikud, jagab need peast või paberil ahelateks, valib igale ahelale kaitselüliti ja kaabli ristlõike ning otsib seejärel iga komponendi hinna eraldi mõne e-poe otsingust. Tulemus kirjutatakse tabelisse või paberile."),
  p("Sellel käigul on kolm konkreetset probleemi."),
  rich([{ t: "Ajakulu ei ole proportsioonis objekti suurusega.", b: true }, { t: " Sama arvutuskäik tuleb korrata iga objekti kohta uuesti, kuigi loogika on identne." }], { bullet: { level: 0 } }),
  rich([{ t: "Vigu ei märka keegi.", b: true }, { t: " Käsitsi tehtud arvutuses ei ole kontrollmehhanismi. Unustatud rikkevoolukaitse või liiga õhuke kaabel ei anna endast märku enne paigaldust — halvimal juhul mitte kunagi." }], { bullet: { level: 0 } }),
  rich([{ t: "Tulemus ei ole kontrollitav.", b: true }, { t: " Valmis nimekiri ei näita, kust kogused tulid. Tellija peab lihtsalt uskuma." }], { bullet: { level: 0 } }),
  p("J\u00e4rgnev tabel n\u00e4itab, kuidas selline nimekiri praegu tekib. See on n\u00e4idis k\u00e4sitsi koostatud materjalide loendist kolmetoalise korteri kohta \u2014 t\u00e4pselt selline, nagu elektrik selle paberile v\u00f5i tabelarvutusse kirjutab."),
  table([
    ["Komponent", "Kogus", "Hind", "M\u00e4rkus"],
    ["Kaitsel\u00fcliti B10", "2", "?", "vist 2, tuleb \u00fcle vaadata"],
    ["Kaitsel\u00fcliti B16", "2", "?", ""],
    ["Kaitsel\u00fcliti B32", "1", "12,80", "pliidi jaoks"],
    ["Kaabel 1,5", "~50 m", "1,20/m", "pikkus umbkaudne"],
    ["Kaabel 2,5", "~50 m", "1,85/m", ""],
    ["Kaabel 6", "~25 m", "3,60/m", ""],
    ["Kilp", "1", "28,50", ""],
    ["Rikkevoolukaitse", "1", "42,00", "kas 40 A v\u00f5i 63 A?"],
    ["KOKKU", "", "\u2248 350 \u20ac", "hinnad eri poodidest, osa puudu"],
  ], [2600, 1400, 1800, 3200]),
  new Paragraph({
    alignment: AlignmentType.CENTER,
    spacing: { after: 200 },
    children: [new TextRun({ text: "Tabel 1. N\u00e4idis k\u00e4sitsi koostatud materjalide nimekirjast", font: FONT, size: 20, italics: true })],
  }),
  p("Tabel n\u00e4itab k\u00f5iki kolme eespool nimetatud probleemi korraga. Koguste juures on k\u00fcsim\u00e4rgid, sest ahelate arv on peast arvutatud. Kaabli pikkused on hinnangulised. Kaks hinda on puudu, sest need tuleks eraldi otsida. Ja mitte \u00fckski rida ei \u00fctle, MIKS kogus selline on \u2014 kui keegi hiljem k\u00fcsib, miks kaitsel\u00fcliteid on kaks, ei ole vastust kuskilt v\u00f5tta."),
  todo("Kui sul on olemas P\u00c4RIS n\u00e4ide \u2014 elektriku tehtud nimekiri, sinu enda k\u00e4sitsi katse v\u00f5i Exceli tabel projekti algusest \u2014 asenda \u00fclaltoodud tabel selle pildiga. P\u00e4ris artefakt on veenvam kui n\u00e4idis. Kalle kasutab oma t\u00f6\u00f6s t\u00e4pselt seda v\u00f5tet: v\u00e4ljav\u00f5tet tabelist, millega tema kliendid p\u00e4riselt t\u00f6\u00f6tasid."),

  h3("Töö eesmärk"),
  p("Töö peamine eesmärk on välja töötada toimiv veebirakendus, mis:"),
  bullet("kogub sisendandmed lihtsa vormi kaudu, mida saab täita ka inimene, kes ei ole elektrik;"),
  bullet("arvutab ahelate arvu ja komponendid andmebaasis hoitavate arvutusreeglite alusel;"),
  bullet("koostab hinnastatud materjalide loendi (BOM) tootekataloogi reaalsete toodete ja hindade põhjal;"),
  bullet("näitab arvutuskäiku, nii et iga rea juures on näha, kust arv tuleb;"),
  bullet("võimaldab tulemuse ostukorvi lisada, valmistades ette hilisemat e-kaubanduse funktsionaalsust."),

  h3("Olemasolevad lahendused ja nende puudused"),
  p("Enne arendamist kaardistati, mis turul juba olemas on."),
  table([
    ["Lahendus", "Tugevus", "Puudus selle ülesande jaoks"],
    ["Tootjapõhised valikutööriistad (ABB, Schneider Electric)", "Täpsed, tootja hooldatud", "Seotud ühe tootja tootevalikuga. Eeldavad erialast ettevalmistust. Ingliskeelsed. Ei anna hinda."],
    ["Eesti elektrikaupade e-poed", "Reaalsed hinnad ja laoseis, eestikeelsed", "Müüvad komponente, ei arvuta koguseid. Kasutaja peab ise teadma, mida ja kui palju osta."],
    ["Üldised kaabliarvutuse kalkulaatorid", "Tasuta, kiired", "Arvutavad ühe parameetri, mitte tervet paigaldist. Ei anna nimekirja ega hinda."],
    ["Käsitsi arvutamine tabelarvutuses", "Täielikult paindlik", "Aeganõudev, vigu ei märka keegi."],
    ["Projekteerimisbüroo teenus", "Professionaalne, vastutusega", "Väikeobjektil ebaproportsionaalselt kallis."],
  ], [2600, 2600, 3800]),
  p("Võrdlusest nähtub kaks tühimikku. Esiteks: arvutamise ja ostmise vahel ei ole silda — tööriistad, mis arvutavad, ei tea hindu, ja poed, mis teavad hindu, ei arvuta. Teiseks: ükski lahendus ei näita arvutuskäiku viisil, mis lubaks mitteprofessionaalil tulemust kontrollida. Käesolev töö asub täpselt nendesse tühimikesse.", { after: 200 }),

  h3("Teema olulisus"),
  rich([{ t: "Aja kokkuhoid.", b: true }, { t: " Arvutus, mis käsitsi võtab tunde, valmib minutiga. Kuna loogika on iga objekti puhul sama, on automatiseerimise tulu vahetu." }]),
  rich([{ t: "Vigade vältimine.", b: true }, { t: " Automaatne arvutus ei unusta rikkevoolukaitset ega vali liiga õhukest kaablit, sest reegel rakendatakse alati ja ühtemoodi." }]),
  rich([{ t: "Läbipaistvus.", b: true }, { t: " Kuna rakendus näitab arvutuskäiku, saab tulemust kontrollida. See on töö kõige olulisem eristaja." }]),
);

// ---- 1.2 ----------------------------------------------------------------
add(
  h2("1.2. Projekti kavandamine"),
  h3("Lähteülesanne"),
  p("Käesoleva tarkvaraarendusprojekti lähteülesanne sõnastati esitatud lõputöö kavandis: luua kalkulaator, mis aitab klientidel kokku panna elektritarvikud ja kilbi komponendid korter- ja ärihoonetele, näidates komponente, nende hindu ja aidates teha sobivaid valikuid."),
  p("Loodav süsteem peab tehniliselt toetama arvutusreeglite hoidmist andmebaasis, tootekataloogi haldamist administraatori poolt, arvutuste salvestamist ja hilisemat vaatamist ning tulemuse ostukorvi lisamist."),
  rich([{ t: "Valminud rakendus toetab lisaks kavandis nimetatud korter- ja ärihoonele ka " }, { t: "eramut", b: true }, { t: ". See on väike ja põhjendatud laiendus: arvutusloogika on sama ja eramu on väikeobjektina sama sagedane kasutusjuht." }]),

  h3("Ülevaade kasutatud tehnoloogiatest ja vahenditest"),
  p("Tehnoloogiate valikul eelistati õppekavas läbitud, hästi dokumenteeritud ja serveripoolseks renderdamiseks sobivaid vahendeid. Lõputöö riskikoht ei ole tehnoloogia uudsus, vaid see, kas töö saab tähtajaks valmis ja töötab."),
  rich([{ t: "Kasutajaliides ja server: ASP.NET Core 9 MVC.", b: true }, { t: " Serveripoolne renderdamine sobib selle rakenduse iseloomuga — kalkulaator teeb ühe arvutuse ja kuvab tulemuse. Eraldi üheleherakendus ja JavaScripti raamistik lisaksid keerukust ilma kasuta. MVC eraldab andmed, kuvamise ja päringukäsitluse, mis on eeldus testitavusele (Microsoft, n.d.-a)." }], { bullet: { level: 0 } }),
  rich([{ t: "Programmeerimiskeel: C#.", b: true }, { t: " Õppekavas läbitud keel, staatiliselt tüübitud, mis püüab suure osa vigadest kinni juba kompileerimisel." }], { bullet: { level: 0 } }),
  rich([{ t: "Andmebaas: Microsoft SQL Server.", b: true }, { t: " Relatsiooniline andmebaas, mis sobib andmemudelile, kus tooted, kategooriad ja arvutusreeglid on omavahel seotud." }], { bullet: { level: 0 } }),
  rich([{ t: "ORM: Entity Framework Core 9.", b: true }, { t: " Vastendab C# klassid tabeliteks, mistõttu SQL-i ei kirjutata käsitsi. Filtreerimine ja sorteerimine rakendatakse päringule enne selle käivitamist, nii et need muutuvad SQL-päringu osaks (Microsoft, n.d.-b)." }], { bullet: { level: 0 } }),
  rich([{ t: "Migratsioonid: EF Core Migrations.", b: true }, { t: " Tagavad andmebaasi skeemi muudatuste versioonitud ja korratava rakendamise igas keskkonnas." }], { bullet: { level: 0 } }),
  rich([{ t: "Autentimine: ASP.NET Core Identity.", b: true }, { t: " Valmis lahendus kasutajate, paroolide räsimise ja rollide haldamiseks. Kaks rolli: administraator ja klient." }], { bullet: { level: 0 } }),
  rich([{ t: "Kujundus: Bootstrap 5 ja oma CSS-i muutujad.", b: true }, { t: " Bootstrap annab ruudustiku ja komponendid; kõik värvid on ühes failis muutujatena, mis teeb hele- ja tumeda režiimi võimalikuks." }], { bullet: { level: 0 } }),
  rich([{ t: "Testimine: xUnit ja WebApplicationFactory.", b: true }, { t: " Võimaldavad testida nii puhast loogikat kui ka tervet rakendust päris HTTP-päringutega." }], { bullet: { level: 0 } }),
  rich([{ t: "Versioonihaldus ja CI: Git, GitHub, GitHub Actions.", b: true }, { t: " Iga muudatus ehitatakse ja testitakse automaatselt." }], { bullet: { level: 0 } }),

  h3("Praktilise töö tegevus- ja ajakava"),
  rich([{ t: "Projekt on kavandatud katma " }, { t: "156 töötunni", b: true }, { t: " nõuet ning on jaotatud agiilsete põhimõtete järgi lühikesteks, iseseisvalt testitavateks faasideks. Suur muudatuste pakk teeb vea allika leidmise raskeks; väike muudatus ei tee." }]),
  table([
    ["Faas", "Sisu", "Maht"],
    ["Faas 0", "Analüüs ja vundament. Nõuete kogumine, elektrotehniliste põhimõtete uurimine, turuülevaade, projekti struktuuri loomine.", "~30 h"],
    ["Faas 1", "Andmemudel ja andmebaas. Olemite kavandamine, ERD, migratsioonid, algandmed.", "~25 h"],
    ["Faas 2", "Arvutusalgoritm ja äriloogika. Ahelate jaotus, komponentide valik, BOM-i koostamine.", "~35 h"],
    ["Faas 3", "Kasutajaliides. Kalkulaatori vorm, tulemuste tabel, tootekataloog, ostukorv.", "~30 h"],
    ["Faas 4", "Turvalisus ja autentimine. Identity, rollid, CSRF-kaitse, failiüleslaadimise kontroll.", "~16 h"],
    ["Faas 5", "Testimine ja dokumenteerimine. Automaattestid, turvakontroll, lõputöö.", "~20 h"],
  ], [1200, 6400, 1400]),
  h3("Projekti kestus: neli aktiivset t\u00f6\u00f6kuud"),
  p("Projekti aktiivne t\u00f6\u00f6maht jaotus \u00fcle NELJA T\u00d6\u00d6KUU: aprill, mai, august ja september 2026. Need neli kuud jagunesid kahte etappi, mille vahele j\u00e4i suvine paus. Kavandamine ja anal\u00fc\u00fcs algasid varem, veebruaris, kuid programmeerimist nendel kuudel veel ei toimunud."),
  p("Alljärgnev tabel on rekonstrueeritud versioonihalduse ajaloost ja dokumentide kuup\u00e4evadest, mitte m\u00e4lu j\u00e4rgi. Iga rea juures on n\u00e4idatud t\u00f5end, mille alusel see on kirja pandud."),
  table([
    ["Aeg", "Tegevus", "T\u00f5end"],
    ["Veebruar 2026", "L\u00f5put\u00f6\u00f6 kavandi koostamine ja esitamine. Teema piiritlemine, lähteülesande sõnastamine.", "Kavandi fail 06.02.2026"],
    ["M\u00e4rts 2026", "Andmemudeli kavandamine. Olemi-suhte diagrammide joonistamine, tabelite seoste l\u00e4bim\u00f5tlemine.", "ERD-failid 08.\u201309.03.2026"],
    ["Aprill 2026", "Arenduse algus. Projekti struktuuri loomine, nelja projekti eraldamine, esimesed kontrollerid, DTO-d, vaated ja teenused.", "Esimene commit 21.04.2026"],
    ["Mai 2026", "Esimese versiooni viimistlemine ja seadistamine.", "Commit 07.05.2026"],
    ["Juuni\u2013juuli 2026", "PAUS \u2014 muud \u00f5ppet\u00f6\u00f6 kohustused. Neid kuid ei arvestata t\u00f6\u00f6mahu sisse.", "commit\u2019e ei ole"],
    ["August 2026", "Suurim arendusetapp: arvutusalgoritmi viimistlemine, turvalisus ja autentimine, automaattestid, kasutajaliidese \u00fcmberkujundamine.", "Aktiivsed commit'id alates 10.08.2026"],
    ["September 2026", "Kataloogi t\u00e4iendamine, standardi allikate uurimine, dokumentatsioon ja l\u00f5put\u00f6\u00f6 kirjutamine.", "Commit'id kuni 10.09.2026"],
  ], [1800, 4600, 2600]),
  rich([{ t: "Kokku on versioonihalduses 44 commit\u2019it. Aktiivse t\u00f6\u00f6 kestus oli " }, { t: "neli kuud", b: true }, { t: " \u2014 aprill ja mai 2026 ning august ja september 2026 \u2014 kokku 156 t\u00f6\u00f6tundi. Juuni ja juuli olid paus, mille p\u00f5hjustasid muud \u00f5ppet\u00f6\u00f6 kohustused." }]),
  p("Kui arvestada ka kavandamise ja anal\u00fc\u00fcsi etappi veebruarist m\u00e4rtsini, h\u00f5lmab projekt kalendris veebruarist septembrini. T\u00f6\u00f6maht \u2014 156 tundi \u2014 tekkis siiski nende nelja aktiivse kuu jooksul, mist\u00f5ttu on korrektne rääkida neljakuulisest projektist."),
  rich([{ t: "Miks paus ei ole puudus. ", b: true }, { t: "Kahe etapi vahele j\u00e4\u00e4nud paus osutus kasulikuks: augustis projekti juurde naastes tuli lugeda oma enda koodi n\u00e4dalatepikkuse vahega, mis paljastas kohad, kus lahendus ei olnud ilma selgituseta arusaadav. Just sellest kogemusest kasvas v\u00e4lja dokumentatsioonikaust ja n\u00f5ue kirjutada iga muudatuse juurde selle p\u00f5hjus." }]),
  todo("Kuup\u00e4evad on \u00f5iged \u2014 v\u00f5etud git-ajaloost ja failide kuup\u00e4evadest. T\u00e4psusta tegevuste kirjeldusi oma m\u00e4lu j\u00e4rgi. KAITSMISEL: kui k\u00fcsitakse, miks kuup\u00e4evad ulatuvad veebruarist septembrini, aga t\u00f6\u00f6 kestis neli kuud, on vastus: kavandamine algas veebruaris, aktiivne arendus toimus neljal kuul kahes etapis, suvine paus vahel."),
);

// ---- 1.3 ----------------------------------------------------------------
add(
  h2("1.3. Rakenduse kavandamine"),
  h3("Arhitektuur"),
  p("Rakendus on jaotatud nelja projekti, kus iga projekt sõltub ainult „allpool“ olevatest. Sama kihiline struktuur on kasutusel autori varasemas kursusetöös — tuttav struktuur vähendab vigu."),
  table([
    ["Projekt", "Vastutus"],
    ["Core", "Domeeniolemid, DTO-d, teenuseliidesed. Ei sõltu millestki."],
    ["Data", "DbContext, migratsioonid, algandmed."],
    ["ApplicationServices", "Teenuste teostused. Siin elab äriloogika."],
    ["ElektriKalkulaator (veeb)", "Kontrollerid, Razor vaated, staatilised failid."],
    ["Tests", "Viitab kõigile neljale."],
  ], [2600, 6400]),
  rich([{ t: "Reegel, mida ei tohi rikkuda: " }, { t: "Core ei sõltu kunagi millestki ja Data ei viita kunagi veebiprojektile.", b: true }, { t: " Kui see reegel murdub, muutub äriloogika testimine võimatuks ilma veebiserverit käivitamata." }]),

  h3("Andmemudel"),
  table([
    ["Olem", "Sisu"],
    ["Product", "Toode: nimi, tootja, hind, laoseis, nimivool, kaabli ristlõige, pildi tee"],
    ["ProductCategory", "Tootekategooria"],
    ["CalculationRule", "Arvutusreegel: hoone tüüp, ahelatüüp, jagaja, ristlõige, nimivool"],
    ["PowerboxCalculation", "Salvestatud arvutus koos sisendandmetega"],
    ["PowerboxComponents", "Salvestatud arvutuse üksikread"],
  ], [2600, 6400]),
  p("Kaks teadlikku otsust väärivad selgitust."),
  rich([{ t: "CalculationRule ei ole Product-iga võtmeseoses.", b: true }, { t: " Reegel seotakse hoone tüübiga sõne järgi ja sobiv toode otsitakse arvutuse ajal kategooria ja nimivoolu põhjal. Nii saab lisada uue tootja kaitselüliti reegleid puutumata, muuta reeglit kataloogi puutumata ja valida alati odavaima laos oleva sobiva toote. Kui reegel viitaks konkreetsele tootele, blokeeriks laost otsa saanud toode arvutuse." }]),
  rich([{ t: "PowerboxComponents salvestab hinna arvutuse hetkel.", b: true }, { t: " Kui hind hiljem muutub, ei muutu vana arvutuse summa tagantjärele. Kogu töö lubadus on, et arvutust saab kontrollida, ja see lubadus ei kehti, kui esmaspäeval antud hinnapakkumine näitab reedel teist summat." }]),
  figure("Elektrikilbi ja -tarvete komponentide kalkulaator .png",
         "Andmemudeli olemi-suhte diagramm (ERD). Tooted, kategooriad ja arvutusreeglid ning salvestatud arvutuste seosed.",
         470, DRAFT),

  h3("Kasutusjuhud"),
  table([
    ["Kasutaja", "Kasutusjuht"],
    ["Külastaja (sisse logimata)", "Arvutab komponendid, sirvib kataloogi, lisab ostukorvi"],
    ["Registreeritud kasutaja", "Lisaks: näeb oma arvutuste ajalugu"],
    ["Administraator", "Lisaks: haldab tooteid ja kategooriaid"],
  ], [3000, 6000]),
  rich([{ t: "Teadlik otsus oli, et " }, { t: "kalkulaator on kasutatav ilma kontota", b: true }, { t: ". Registreerumise nõudmine enne, kui kasutaja on näinud, kas tööriist on üldse kasulik, on kindel viis kasutajaid kaotada. Konto annab lisaväärtust (ajalugu), mitte ligipääsu." }]),

  h3("Kasutajaliidese kavandamine"),
  p("Kasutajaliidese kavandamisel analüüsiti rahvusvahelisi erialaseid veebilehti. Analüüsist selgus, et need jagunevad kaheks vastandlikuks tüübiks: turunduslehed (tume taust, suur pealkiri, vähe elemente, eesmärk veenda) ja kataloogilehed (hele taust, tihe, fotopõhine, eesmärk aidata osa leida)."),
  p("Käesolev rakendus on mõlemat: avaleht peab veenma kalkulaatorit proovima, kataloog peab aitama leida konkreetse komponendi. Ühe tüübi rakendamine teisele on kõige levinum põhjus, miks veebileht „tundub vale“, kuigi iga üksik element on korras."),
  figure(String.raw`Website\1.PNG`, "Rakenduse avaleht. Ülal kangelasosa koos peamise tegevusnupuga, all mõõdetavad näitajad ja rakenduse põhiomadused."),
  todo("KUVAT\u00d5MMIST TULEB UUENDADA. Sellel pildil seisab veel vana tekst „Kogused tulevad EVS-HD 60364 n\u00f5uetest“. Rakenduses on see n\u00fc\u00fcdseks parandatud: „Kaabli ja kaitsel\u00fcliti valik l\u00e4htub EVS-HD 60364 p\u00f5him\u00f5tetest. Ahelate jaotus p\u00f5hineb projekteerimistaval.“ Tee avalehest ja ostukorvist uued kuvat\u00f5mmised ning asenda failid Pictures kaustas: Website 1.PNG ja 5.PNG. Muidu on joonisel kirjas v\u00e4ide, mille t\u00f6\u00f6 tekst ise \u00fcmber l\u00fckkab — seda k\u00fcsitakse kaitsmisel."),
  p("Kavandati kaks täielikku värvipaletti — hele ja tume — mille vahel kasutaja saab valida. Kõik värvid koondati ühte faili nimetatud muutujatena, nii et ükski vaade ei kirjuta värvi väärtust otse. Kontrastisuhted mõõdeti ja need vastavad WCAG AA nõudele (W3C, 2025)."),

  h3("Turvalisuse kavandamine"),
  p("Kuna rakendusel on administraatori funktsioonid ja kasutajate andmed, kavandati juba alguses rollipõhine ligipääs, CSRF-kaitse kõigil andmeid muutvatel vormidel, failiüleslaadimise kontroll ning sisendi valideerimine nii kliendi kui serveri poolel (OWASP, n.d.)."),
);

// ---- 1.4 ----------------------------------------------------------------
add(
  h2("1.4. Ülevaade praktilise töö teostamisest"),
  h3("Väljatöötamine"),
  p("Töö tehti üksinda. Iga muudatus läbis sama tsükli: eraldi haru, väike muudatus, ehitus ja testid, kontroll töötava rakenduse vastu, kirje muudatuste päevikusse ning liitmine pull request'i kaudu."),
  p("Selline ts\u00fckkel ei ole formaalsus. Selle m\u00f5te on, et iga muudatus oleks eraldi \u00fclevaadatav ja tagasip\u00f6\u00f6ratav. Kui rakenduses ilmneb hiljem viga, saab vaadata, milline v\u00e4ike muudatuste kogum selle p\u00f5hjustas, selle asemel et otsida seda sadade ridade seast."),
  p("Arendus jagunes praktikas kolme selgesti eristuvasse ossa. Esimene oli andmete ja loogika osa \u2014 andmemudel, algandmed ja arvutusalgoritm \u2014 mille valmimiseni ei olnud rakendusel kasutajaliidest ega \u00fchtegi vaadet. Teine oli kasutajaliides, mis ehitati valmis loogika peale. Kolmas oli turvalisus ja testimine."),
  p("See j\u00e4rjekord osutus \u00fches osas veaks, mida on kirjeldatud peat\u00fckis 2.4: turvalisuse j\u00e4tmine kolmandaks t\u00e4hendas, et selle lisamisel tuli muuta juba valmis kontrollereid ja vaateid."),

  h3("Arvutusalgoritmi realiseerimine"),
  p("Algoritm on rakenduse tuum ja töötab järgmiselt:"),
  bullet("Loe andmebaasist arvutusreeglid antud hoone tüübi kohta."),
  bullet("Iga reegli kohta arvuta ahelate arv: tarbijate arv jagatakse reegli jagajaga ja tulemus ümardatakse ülespoole."),
  bullet("Vali kaitselüliti — sobiva kategooria ja nimivooluga odavaim laos olev toode."),
  bullet("Vali kaabel — sobiva ristlõikega odavaim laos olev toode; kogus meetrites."),
  bullet("Lisa rikkevoolukaitse ja jaotuskilp — üks kummastki paigaldise kohta."),
  bullet("Arvuta summad, salvesta arvutus ja tagasta materjalide loend."),
  p("Ümardamine ülespoole on oluline detail. Kui valgusteid on 12 ja üks ahel kannab 8 valgustit, on tulemus 1,5 ahelat, mis ümardatakse kaheks. Allapoole ümardamine tähendaks ülekoormatud ahelat."),
  figure(String.raw`Code\CalculatorController1.PNG`, "Kalkulaatori kontroller. Teenus antakse konstruktoris ette (sõltuvuste süstimine), mistõttu kontroller ise ei arvuta midagi ja arvutust saab testida eraldi.", 560),
  figure(String.raw`Code\CalculatorController2.PNG`, "Kalkulaatori POST-meetod. Märgend [ValidateAntiForgeryToken] nõuab, et päring kannaks meie enda vormi peidetud märgist — ilma selleta saaks võõras leht vormi kasutaja nimel esitada.", 560),

  h3("Arvutusreeglid ja standard"),
  p("Arvutusreeglid hoitakse andmebaasis, mitte koodis, mistõttu reegli muutmiseks ei ole vaja rakendust uuesti ehitada."),
  table([
    ["Ahelatüüp", "Jagaja", "Kaabli ristlõige", "Nimivool"],
    ["Valgustus", "1 ahel / 8 valgustit", "1,5 mm²", "10 A"],
    ["Pistikud", "1 ahel / 6 pistikut", "2,5 mm²", "16 A"],
    ["Elektripliit", "eraldi ahel", "6,0 mm²", "32 A"],
  ], [2200, 2800, 2200, 1800]),
  rich([{ t: "Siin tuleb teha vahe, mida kaitsmisel kindlasti küsitakse.", b: true }]),
  rich([{ t: "Standardi põhimõtetest tulenev:", b: true }, { t: " kaabli ristlõike ja kaitselüliti nimivoolu paar. Alus on liigvoolukaitse põhimõte — kaitseseade peab rakenduma enne, kui juht üle kuumeneb — mida käsitlevad EVS-HD 60364 osad 4-43 ja 5-52. Rikkevoolukaitsme vajadus tuleneb osast 4-41 (Eesti Standardimis- ja Akrediteerimiskeskus, 2011)." }]),
  rich([{ t: "Projekteerimistava, mitte standardi nõue:", b: true }, { t: " ahelate jaotus, st „üks ahel kaheksa valgusti kohta“. Standard ei sätesta punktide arvu ahelas — see nõuab, et ahela koormus mahuks kaabli ja kaitse piiridesse, ja mitu punkti see tähendab, sõltub punktide võimsusest. Kümme 5 W LED-valgustit ja kümme 150 W valgustit on täiesti erinev koormus." }]),
  p("Just seetõttu hoitakse neid arve andmebaasis: need on eeldused, mida saab muuta. Väli EvsReference on teadlikult tühi — sinna kuuluvad standardi punktinumbrid, kuid neid ei tohi sisestada enne, kui need on standardist endast üle kontrollitud."),
  todo("KÕIGE TÄHTSAM ÜLESANNE: hangi ligipääs standardile EVS-HD 60364 (evs.ee, osa 5-52 muudatus maksab 12,40 €; küsi enne koolist, kas ligipääs on olemas) ja kirjuta iga reegli juurde täpne punktinumber. Väljamõeldud viide standardile on tõsisem viga kui viite puudumine."),

  h3("Turvalisus"),
  p("Turvalisus ei olnud algselt piisav ja seda parandati teadlikult."),
  table([
    ["Probleem", "Tagajärg", "Lahendus"],
    ["Autentimine puudus", "Igaüks pääses administraatori lehtedele", "Identity, rollid Admin/Customer"],
    ["Avatud ümbersuunamine", "Link sai kasutaja suunata võõrale lehele", "Url.IsLocalUrl kontroll"],
    ["CSRF-kaitse puudus", "Võõras leht sai kasutaja nimel tegevusi teha", "[ValidateAntiForgeryToken]"],
    ["Negatiivne kogus ostukorvis", "Summa läks miinusesse", "Vahemik 1–999"],
    ["Nõrk failikontroll", "Programm sai ümber nimetada pildiks", "Faili algusbaitide kontroll"],
    ["Ostukorv säilis väljalogimisel", "Järgmine kasutaja nägi eelmise korvi", "Sessiooni tühjendamine"],
  ], [2600, 3400, 3000]),
  figure(String.raw`Code\AccountController.PNG`, "Autentimise kontroller. Paroolide räsimise ja kontrollimise teeb ASP.NET Core Identity — rakendus ise ei näe ega salvesta parooli avatekstina.", 560),
);

// ---- 1.5 ----------------------------------------------------------------
add(
  h2("1.5. Tulemused"),
  h3("Nõuetele vastavus"),
  table([
    ["Nõue", "Täidetud", "Märkus"],
    ["Sisendandmete kogumine", "Jah", "Vorm valideerib vahemikud"],
    ["Ahelate arvutus", "Jah", "Ülespoole ümardamine"],
    ["Komponentide valik", "Jah", "Odavaim laos olev sobiv toode"],
    ["Rikkevoolukaitse ja kilp", "Jah", "Üks kummastki"],
    ["Hinnastatud materjalide loend", "Jah", "Ühiku-, rea- ja kogusumma"],
    ["Arvutuskäigu näitamine", "Jah", "Ahelatüüp ja ristlõige iga rea juures"],
    ["Ostukorv", "Jah", "Tellimust veel ei salvestata"],
    ["Tootekataloog", "Jah", "Kategooria, tootja, otsing, sorteerimine"],
    ["Administreerimine", "Jah", "Rollipõhine ligipääs"],
    ["Arvutuste ajalugu", "Jah", ""],
    ["Eestikeelne liides", "Jah", ""],
    ["Turvalisus", "Jah", "18 kontrolli läbib"],
    ["WCAG AA kontrast", "Jah", "Mõõdetud, 0 puudujääki"],
    ["Automaattestid", "Jah", "218 testi, CI"],
  ], [3400, 1600, 4000]),

  h3("Valminud rakendus"),
  p("Kalkulaator võtab sisendiks hoone parameetrid ja tagastab hinnastatud materjalide loendi. Iga rea juures on näha ahelatüüp ja kaabli ristlõige, mis muudab tulemuse kontrollitavaks."),
  figure(String.raw`Website\4.PNG`, "Kalkulaatori vorm ja arvutuse tulemus. Vasakul sisendandmed ja arvutamise reeglid, paremal materjalide loend koos ühiku- ja kogumaksumusega."),
  p("Tootekataloog järgib erialaste hulgimüüjate loogikat: filtreerimine kategooria ja tootja järgi, otsing nime järgi ning sorteerimine. Iga filtri juures on näha, mitu toodet selle taha jääb."),
  figure(String.raw`Website\2.PNG`, "Tootekataloog. Filtririba näitab kategooriaid ja tootjaid koos toodete arvuga; tühja tulemusega filtreid ei pakuta."),
  figure(String.raw`Website\3.PNG`, "Tootekataloogi tooted. Iga kaardi juures on tehnilised näitajad, laoseis ja hind koos õige ühikuga — kaablil meetri, ülejäänutel tüki kohta."),
  p("Ostukorv koondab valitud tooted ja näitab kogusummat. Tellimuse vormistamine tühjendab korvi, kuid tellimust veel ei salvestata — see on teadlikult järgmise etapi töö."),
  figure(String.raw`Website\5.PNG`, "Ostukorv koos kokkuvõttega."),

  h3("Näidisarvutus"),
  p("Kolmetoalise korteri kohta (3 tuba, 10 pistikut, 12 valgustit, elektripliit) annab rakendus kaheksarealise nimekirja kogumaksumusega 348,90 €."),
  table([
    ["Komponent", "Kogus", "Ühikuhind", "Kokku"],
    ["Schneider Easy9 B10A (valgustus)", "2 tk", "7,90 €", "15,80 €"],
    ["NYM-J 3×1,5 mm² kaabel", "48 m", "1,20 €/m", "57,60 €"],
    ["Schneider Easy9 B16A (pistikud)", "2 tk", "8,50 €", "17,00 €"],
    ["NYM-J 3×2,5 mm² kaabel", "48 m", "1,85 €/m", "88,80 €"],
    ["ABB S201-B32 (pliit)", "1 tk", "12,80 €", "12,80 €"],
    ["NYM-J 3×6 mm² kaabel", "24 m", "3,60 €/m", "86,40 €"],
    ["ABB Mistral41F 12 mooduli kilp", "1 tk", "28,50 €", "28,50 €"],
    ["ABB F202 AC-40/0.03 RCD 40A", "1 tk", "42,00 €", "42,00 €"],
    ["KOKKU", "", "", "348,90 €"],
  ], [3600, 1400, 2000, 2000]),
  p("Valgustusahelaid on kaks, sest 12 ÷ 8 = 1,5 ja tulemus ümardatakse üles. Pistikuahelaid on kaks, sest 10 ÷ 6 = 1,67. Kaablit on kokku 120 m: viis ahelat, iga ahela kohta 24 m."),

  h3("Testimise tulemused"),
  p("Rakendust katab 218 automaattesti, mis kõik läbivad. Lisaks kontrolliti funktsionaalsust käsitsi töötava rakenduse vastu."),
  table([
    ["Kontrollitud", "Tulemus"],
    ["Kõik kolm hoonetüüpi annavad materjalide loendi", "korterelamu 8 rida, eramu 8 rida, ärihoone 6 rida"],
    ["Piirjuhud", "0 tuba annab valideerimisvea; 100 tuba ja 200 pistikut arvutab korrektselt"],
    ["SQL-i süstimine ja XSS", "ei anna tulemust, kataloog jääb terveks"],
    ["Vigased identifikaatorid", "404"],
    ["Ostukorvi piirid", "kogus 0 ja 99999 lükatakse tagasi"],
    ["Administraatori lehed ilma sisselogimiseta", "suunatakse sisselogimislehele"],
    ["Vale parool", "ei paljasta, kas konto on olemas"],
    ["Turvaskript (18 kontrolli)", "kõik läbivad"],
    ["WCAG AA kontrast (34 paari)", "ükski ei jää alla nõude"],
  ], [4200, 4800]),
  p("Testimise käigus leiti üks tõeline viga: elektripliidi märkeruut kuvati kõigi hoonetüüpide juures, kuigi ärihoone jaoks ei olnud pliidiahela arvutusreeglit määratud. Selle märkimine ärihoone puhul ei lisanud pliidiahelat ega andnud kasutajale ka mingit teadet."),
  p("Viga parandati nii, et rakendus ütleb selle välja: kui pliidiahelat ei saa lisada, kuvatakse tulemuse juures põhjendus. Väljamõeldud reegli lisamine oleks olnud halvem lahendus, sest ärihoonel ei pruugigi olla kodust pliidiahelat. Kalkulaator, mille mõte on arvutuskäigu näitamine, peab näitama ka seda, mida ta ei teinud."),

  h3("Võrdlus esialgse tegevus- ja ajakavaga"),
  todo("Täida see peatükk ise — ainult sina tead, kui palju aega tegelikult kulus. Mall nõuab võrdlust plaaniga. Kirjuta, milline faas võttis kauem kui plaanitud ja miks. Aus näide: turvalisuse parandamine ei olnud algses plaanis eraldi faasina, aga võttis reaalselt aega, sest vead leiti alles ülevaatusel."),

  h3("Mis jäi tegemata"),
  bullet("Tellimust ei salvestata. Ostukorvi vormistamine tühjendab korvi, kuid Order-olemit veel ei ole. Teadlikult järgmise etapi töö."),
  bullet("Arvutusreeglite juures ei ole veel standardi punktiviiteid."),
  bullet("Algandmete hinnad on ligikaudsed ega ole turuhindadega võrreldud."),
  bullet("Käibemaksu eeldus on kontrollimata: seadistus näitab, et hinnad sisaldavad käibemaksu, kuid seda ei ole kinnitatud."),
  bullet("Mobiilivaade on vähe testitud."),
  bullet("Ärihoone tüübil puudub pliidiahela reegel. Rakendus teatab sellest kasutajale, kuid reeglit ennast ei ole."),
);

// ---- 1.6 ----------------------------------------------------------------
add(
  h2("1.6. Töö teostamiseks meeskonna koosseis ja ülesannete jaotus"),
  rich([{ t: "Meeskond: ", b: true }, { t: "Edgar Muoni (üksinda)." }]),
  rich([{ t: "Ülesanded: ", b: true }, { t: "nõuete analüüs, andmemudeli ja arhitektuuri kavandamine, arvutusalgoritmi realiseerimine, kasutajaliidese kujundus ja programmeerimine, turvalisuse tagamine, automaattestide kirjutamine, dokumentatsiooni koostamine ja lõputöö kirjutamine." }]),
  new Paragraph({ children: [new PageBreak()] }),
);

// ======================= 2. LOODUD LAHENDUSE KIRJELDUS ===================
add(
  h1("2. LOODUD LAHENDUSE KIRJELDUS"),

  h2("2.1. Tehtud tööde maht ja arendusplaan"),
  p("Töö kogumaht on 156 tundi, jaotatuna kuueks faasiks (vt peatükk 1.2). Arendusplaan järgis põhimõtet, et iga faasi lõpus peab rakendus olema töötavas seisus — mitte, et kõik osad valmivad korraga ja liidetakse lõpus."),
  p("Praktikas tähendas see, et andmemudel ja algandmed valmisid enne arvutusalgoritmi ning arvutusalgoritm enne kasutajaliidest. Nii sai iga osa testida siis, kui see valmis."),
  p("Arendus toimus haruderivatsiooni põhimõttel: iga tööpakett tehti eraldi harus ja liideti pull request'i kaudu, mitte otse peaharusse. Selline jaotus ei ole formaalsus — iga haru on eraldi ülevaadatav tervik, ja kui mõnes neist ilmneb hiljem viga, on selge, milline muudatuste kogum selle põhjustas."),

  h2("2.2. Valitud töövahendid"),
  table([
    ["Vahend", "Otstarve"],
    ["Visual Studio 2022", "Peamine arenduskeskkond"],
    ["SQL Server Management Studio", "Andmebaasi vaatamine ja kontroll"],
    ["Git ja GitHub", "Versioonihaldus, harud, pull request'id"],
    ["GitHub Actions", "Automaatne ehitamine ja testimine"],
    ["draw.io", "ERD ja arhitektuuriskeemid"],
    ["Veebibrauseri arendajatööriistad", "Kasutajaliidese ja kontrastide kontroll"],
    ["Tehisintellekt (Claude)", "Koodiülevaatus, testide kirjutamise abi, dokumentatsioon"],
  ], [3400, 5600]),
  p("Töövahendite valiku põhimõte oli, et vahend peab olema kas õppekavas läbitud või laialt kasutatav ja hästi dokumenteeritud. Lõputöö ajal uue vahendi õppimine suurendab riski, et töö ei valmi tähtajaks."),
  p("Eraldi väärib mainimist automaatne ehitamine ja testimine. GitHub Actions käivitab iga muudatuse järel ehituse ja kõik testid; hoiatused on seatud vigadeks, mis tähendab, et hoiatustega kood ei lähe läbi. See ei ole mugavusvahend, vaid distsipliini tagamise vahend: käsitsi käivitatav testikomplekt jääb varem või hiljem käivitamata."),
  todo("Kontrolli juhendajaga, kuidas ta soovib tehisintellekti kasutamist kirjeldatuna näha. Malli LISA E annab viitamise vormi: OpenAI. (2024). ChatGPT (v. 4.0) [Suur keelemudel]. Kättesaadav aadressil https://www.openai.com/chatgpt"),

  h2("2.3. Valitud meetodid ja töövõtted"),
  rich([{ t: "Agiilne, väikeste sammudega arendus.", b: true }, { t: " Töö jaotati väikesteks, iseseisvalt testitavateks muudatusteks. Iga muudatus ehitati, testiti ja alles siis liideti." }]),
  rich([{ t: "Kihiline arhitektuur.", b: true }, { t: " Neli projekti, sõltuvused ainult ühes suunas. See ei ole korrastatuse küsimus, vaid eeldus testitavusele." }]),
  rich([{ t: "Andmepõhised reeglid.", b: true }, { t: " Arvutusreeglid on andmebaasis, mitte koodis, mistõttu neid saab muuta rakendust uuesti ehitamata." }]),
  rich([{ t: "Muudatuste päevik.", b: true }, { t: " Iga koodimuudatuse kohta kirjutati kirje, kus on kirjas MIKS, mitte ainult mis. Kood näitab ise, mis muutus; põhjus on ainuke asi, mida hiljem enam kuskilt ei leia." }]),
  rich([{ t: "Kontrollimine töötava rakenduse vastu.", b: true }, { t: " Projektis kehtib reegel, et „see ehitub“ ei ole „see töötab“. Mitu selle töö vigadest olid kompilaatorile täiesti nähtamatud." }]),

  h2("2.4. Arendusprotsess / etapid ja dokumentatsioon"),
  p("Projektis on eraldi dokumentatsioonikaust, mis on osa lähtekoodist ja liigub sellega kaasa."),
  table([
    ["Fail", "Sisu"],
    ["CHANGELOG.md", "Iga koodimuudatus ja selle põhjus. Ei kirjutata kunagi ümber."],
    ["PROJECT_ROADMAP.md", "Praegune seis ja plaanid"],
    ["TESTING.md", "Testimise metoodika"],
    ["DESIGN_GUIDE.md", "Disainisüsteem ja põhjendused"],
    ["RESEARCH_LOG.md", "Väljastpoolt kogutud faktid: turuhinnad, konkurentide analüüs"],
    ["IMAGE_CREDITS.md", "Iga pildi päritolu ja litsents"],
    ["EVS_ALLIKAD.md", "Standardi allikad ja aus hinnang vastavusele"],
    ["KOODI_SELGITUS.md", "Koodi selgitus eesti keeles"],
  ], [3000, 6000]),
  p("Selline dokumentatsioon ei ole bürokraatia. Selle väärtus oli konkreetne: kui projekti juurde naasti nädalaid hiljem, sai lugeda, miks mingi lahendus on selline, selle asemel et seda koodist tagasi tuletada — või valesti tuletada ja „ära parandada“ midagi, mis oli tahtlik."),
  p("Projektis on eraldi loetelu asjadest, mis näevad välja nagu vead, kuid on tahtlikud — näiteks see, et arvutusreegel ei ole tootega võtmeseoses. Ilma sellise loeteluta „parandab“ järgmine arendaja need ära ja rikub läbimõeldud lahenduse."),
  p("Arendus järgis peatükis 1.2 kirjeldatud faase, kuid tegelikkuses ei olnud üleminekud järsud. Turvalisuse faas algas alles siis, kui kasutajaliides oli suures osas valmis — ja just see järjekord osutus veaks. Kui selgus, et autentimine puudus täielikult, tuli muuta korraga kontrollereid, vaateid ja andmemudelit. Turvalisus puudutab arhitektuuri, mistõttu selle lõppu jätmine tähendab hilisemat ümbertegemist."),

  h2("2.5. Valminud lahenduse testimine ja kirjeldamine"),
  h3("Testide arv ja liigid"),
  p("Rakendust katab 218 automaattesti."),
  table([
    ["Liik", "Mida kontrollib"],
    ["Ühiktestid", "Puhas loogika, ilma andmebaasita"],
    ["Integratsioonitestid", "Teenus koos andmebaasiga"],
    ["HTTP-testid", "Kogu rakendus päris päringutega: autentimine, CSRF, ümbersuunamised"],
    ["Andmeterviklikkuse testid", "Algandmete korrektsus, nt iga toote pilt on olemas"],
    ["Regressioonitestid", "Konkreetne varasem viga"],
  ], [3000, 6000]),

  h3("Põhireegel: test peab suutma läbi kukkuda"),
  rich([{ t: "Projekti keskne testimispõhimõte on, et " }, { t: "test, mis ei suuda kunagi läbi kukkuda, on halvem kui testi puudumine", b: true }, { t: ", sest see loob teenimatut kindlustunnet." }]),
  p("Seetõttu kontrolliti iga olulist testi mutatsioonitestimisega: koodi rikuti meelega, veenduti, et test läks punaseks, ja seejärel taastati kood."),
  p("Töö käigus tabati kaks juhtumit, kus test näis töötavat, aga ei töötanud. Esiteks kontrollis turvakontroll algselt ainult, et ümbersuunamine ei lähe ründaja lehele — tühi vastus rahuldas selle tingimuse, mistõttu kontroll oli roheline, kuigi ei kontrollinud midagi. Teiseks otsis test, mis kontrollis avalehel kuvatavat kaabli pikkust, numbrit kogu failist; number esines ka kommentaaris, mistõttu test läks läbi ka siis, kui nähtav number oli vale."),

  h3("Turvalisuse testimine"),
  p("Turvakontrollide kordamiseks on skript, mis käivitab kõik rünnakukatsed töötava rakenduse vastu. Kõik 18 kontrolli läbivad. Lisaks kontrolliti käsitsi, et SQL-i süstimise ja XSS-i katsed ei anna tulemust ning kataloog jääb terveks."),

  h3("Ligipääsetavus"),
  p("Kontrastisuhted mõõdeti skriptiga. Kõik teksti ja tausta paarid mõlemas režiimis ületavad WCAG AA nõude 4,5:1 (W3C, 2025). „Värvid valiti hoolikalt“ on arvamus; „mõõdeti 34 paari, madalaim 4,55, ükski ei jää alla nõude“ on kontrollitav fakt."),
  new Paragraph({ children: [new PageBreak()] }),
);

// ======================= 3-5 =============================================
add(
  h2("2.6. Suurimad v\u00e4ljakutsed"),
  p("J\u00e4rgnev ei ole loetelu eba\u00f5nnestumistest, vaid neljast probleemist, mille lahendamine muutis lahendust k\u00f5ige rohkem. K\u00f5ik neli on p\u00e4ris vead, mis selles projektis tegelikult esinesid."),

  h3("Vead, mida kompilaator ei n\u00e4e"),
  p("K\u00f5ige raskem klass vigu olid need, mille puhul kood oli s\u00fcntaktiliselt korrektne ja loogiliselt vale."),
  p("Esimene n\u00e4ide: p\u00f5hivaate failis oli lehe sisu kuvamise k\u00e4sk HTML-kommentaari sees. Razor ei k\u00e4sitle HTML-kommentaare kommentaaridena \u2014 kood nende sees k\u00e4ivitub \u2014 mist\u00f5ttu iga lehe sisu renderdati l\u00f5petamata kommentaari sisse ja p\u00f5hiala j\u00e4i t\u00fchjaks. Rakendus ehitus veatult."),
  p("Teine n\u00e4ide: Bootstrapi tabelistiil seab muutuja, mis m\u00e4\u00e4rab tabelilahtrite tausta, Bootstrapi enda lehetausta v\u00e4\u00e4rtuseks. See on valge ega tea midagi meie teemast. Tulemuseks oli kalkulaatori tulemuste tabel valge plokina tumedal lehel. Heledas re\u017eiimis n\u00e4gi see juhuslikult \u00f5ige v\u00e4lja, mist\u00f5ttu viga j\u00e4i kauaks m\u00e4rkamata."),
  p("Kolmas n\u00e4ide: vaadetes oli 38 kohta, kus kasutati Bootstrapi klassi, mis t\u00e4hendab s\u00f5na-s\u00f5nalt valget teksti. Kui hele re\u017eiim lisati, muutusid k\u00f5ik need pealkirjad valgeks valgel taustal."),
  rich([{ t: "\u00d5ppetund: ", b: true }, { t: "kompilaator kontrollib s\u00fcntaksit, mitte t\u00e4hendust. Rakendust tuleb vaadata t\u00f6\u00f6tavana, ja seda tuleb teha m\u00f5lemas re\u017eiimis." }]),

  h3("Test, mis ei suutnud l\u00e4bi kukkuda"),
  p("Turvakontroll \u00fcmbersuunamiste kohta kontrollis algselt ainult, et \u00fcmbersuunamine ei l\u00e4he r\u00fcndaja lehele. T\u00fchi vastus rahuldab selle tingimuse t\u00e4ielikult \u2014 kontroll oli roheline, kuigi ei kontrollinud midagi."),
  p("Sarnane juhtum kordus hiljem: test, mis kontrollis avalehel kuvatavat kaabli pikkust, otsis numbrit kogu failist. Number esines ka selgitavas kommentaaris, mist\u00f5ttu test l\u00e4ks l\u00e4bi ka p\u00e4rast seda, kui n\u00e4htav number oli meelega valeks muudetud."),
  rich([{ t: "\u00d5ppetund: ", b: true }, { t: "testi, mis pole kunagi punast n\u00e4inud, ei saa usaldada. Iga oluline test tuleb l\u00e4bida mutatsioonitestimisega \u2014 rikkuda kood meelega ja veenduda, et test seda m\u00e4rkab." }]),

  h3("Number, mida rakendus ei tootnud"),
  p("Avaleht reklaamis n\u00e4idisarvutust: 160 m paigalduskaablit maksumusega 504,10 \u20ac. Kui sama sisendiga arvutus tegelikult k\u00e4ivitati, tagastas rakendus 120 m ja 348,90 \u20ac. Numbrid olid kunagi k\u00e4sitsi lehele kirjutatud ja koodist lahknenud."),
  p("Enamikul veebilehtedel oleks see tr\u00fckiviga. Selles t\u00f6\u00f6s on see k\u00f5ige halvem v\u00f5imalik viga, sest t\u00f6\u00f6 keskne v\u00e4ide on, et selle arvud on tuletatud ja kontrollitavad."),
  rich([{ t: "\u00d5ppetund: ", b: true }, { t: "number, mis on kahes kohas, l\u00e4heb varem v\u00f5i hiljem lahku. Kas arvuta see v\u00f5i testi seda." }]),

  h3("Turvalisus kui j\u00e4relm\u00f5te"),
  p("Rakenduse esimeses versioonis puudus autentimine t\u00e4ielikult, kuigi rakenduse seadistuses oli volituste kontrolli k\u00e4sk olemas. See j\u00e4ttis mulje, et kaitse on olemas. Tegelikult p\u00e4\u00e4ses iga\u00fcks tooteid lisama ja kustutama."),
  rich([{ t: "\u00d5ppetund: ", b: true }, { t: "turvalisust ei saa l\u00f5ppu j\u00e4tta, sest see puudutab arhitektuuri. Rollip\u00f5hise ligip\u00e4\u00e4su lisamine t\u00e4hendas kontrollerite, vaadete ja andmemudeli muutmist korraga." }]),
  new Paragraph({ children: [new PageBreak()] }),

  h1("3. JÄRELDUSED JA SOOVITUSED"),
  h2("Järeldused"),
  p("Töö eesmärk sai täidetud: valmis toimiv veebirakendus, mis arvutab elektripaigaldise komponendid ja hinna hoone lihtsate parameetrite põhjal ning näitab arvutuskäiku."),
  p("Kolm olulisemat õppetundi:"),
  rich([{ t: "Reegel, mida miski ei kontrolli, ei ole reegel.", b: true }, { t: " Värvimuutujate süsteem oli õigesti kavandatud, aga 38 kohta läks sellest mööda, sest ükski test ei kontrollinud seda." }], { bullet: { level: 0 } }),
  rich([{ t: "Test peab suutma läbi kukkuda.", b: true }, { t: " Kaks testi, mis näisid töötavat, ei kontrollinud tegelikult midagi." }], { bullet: { level: 0 } }),
  rich([{ t: "„See ehitub“ ei ole „see töötab“.", b: true }, { t: " Mitu viga olid kompilaatorile nähtamatud ja tulid välja alles töötavat rakendust vaadates." }], { bullet: { level: 0 } }),

  h2("Soovitused ja edasiarendamise võimalused"),
  p("Lähim etapp:"),
  bullet("Order- ja OrderLine-olemid ning tellimuse salvestamine."),
  bullet("Standardi punktiviidete lisamine arvutusreeglite juurde."),
  bullet("Makselahendus."),
  p("Kaugem eesmärk on tarnijaga sidumine: rakendus võiks tooteid, hindu ja pilte uuendada automaatselt tarnija andmete põhjal, lisades marginaali. See eeldab edasimüügilepingut, mis annab ühtlasi õiguse kasutada tootjate tootepilte. Tehniline pool on lihtsam osa; keerulisem on juriidiline ja ärialane pool."),
  p("Muud võimalused: mitmekeelsus, kolmefaasiliste paigaldiste tugi, PDF-eksport ja CAD-plaani import."),
  new Paragraph({ children: [new PageBreak()] }),

  h1("4. KOKKUVÕTE"),
  p("Käesoleva lõputöö raames valmis veebirakendus „Elektrikilbi ja -tarvete komponentide kalkulaator“, mis arvutab hoone lihtsate parameetrite põhjal elektripaigaldise komponentide nimekirja koos maksumusega."),
  p("Rakendus on ehitatud ASP.NET Core 9 MVC ja Entity Framework Core 9 baasil, kasutades neljakihilist arhitektuuri. Arvutusreeglid hoitakse andmebaasis, mistõttu neid saab muuta rakendust uuesti ehitamata. Lahendust katab 218 automaattesti ja iga muudatust kontrollib automaatne ehitus."),
  p("Töö eristub olemasolevatest lahendustest selle poolest, et näitab arvutuskäiku: iga rea juures on näha, mitmest ahelast kogus tuleb. See muudab hinnakirja kontrollitavaks tööriistaks."),
  p("Töö peamine piirang on, et arvutusreeglite juures ei ole veel täpseid EVS-HD 60364 punktiviiteid ja et osa arvutusreegleid on projekteerimistava, mitte standardi nõue. Mõlemad on töös selgelt välja toodud ning punktiviidete lisamine on esimene edasiarenduse samm."),
  new Paragraph({ children: [new PageBreak()] }),

  h1("5. KASUTATUD ALLIKAD"),
  p("Bootstrap. (n.d.). Bootstrap 5.3 documentation (v. 5.3.8). K\u00e4ttesaadav aadressil https://getbootstrap.com/docs/5.3/ (vaadatud 10.09.2026)."),
  p("Eesti Standardimis- ja Akrediteerimiskeskus. (2011). EVS-HD 60364-5-52:2011. Madalpingelised elektripaigaldised. Osa 5-52: Elektriseadmete valik ja paigaldamine. Juhistikud. K\u00e4ttesaadav aadressil https://www.evs.ee/et/evs-hd-60364-5-52-2011-a1-2025 (vaadatud 10.09.2026)."),
  p("Microsoft. (n.d.-a). ASP.NET Core documentation. K\u00e4ttesaadav aadressil https://learn.microsoft.com/aspnet/core (vaadatud 10.09.2026)."),
  p("Microsoft. (n.d.-b). Entity Framework Core documentation. K\u00e4ttesaadav aadressil https://learn.microsoft.com/ef/core (vaadatud 10.09.2026)."),
  p("OWASP. (n.d.). Cross-Site Request Forgery Prevention Cheat Sheet. K\u00e4ttesaadav aadressil https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html (vaadatud 10.09.2026)."),
  p("W3C. (2025). Web Content Accessibility Guidelines (WCAG) 2.1. W3C Recommendation, 6. mai 2025. K\u00e4ttesaadav aadressil https://www.w3.org/TR/WCAG21/ (vaadatud 10.09.2026)."),
  p("M\u00e4rkus allikate kohta: k\u00f5ik kuus URL-i kontrolliti 10.09.2026 ja need avanevad. Tehnilise dokumentatsiooni puhul, millel avaldamisaastat m\u00e4rgitud ei ole, kasutatakse APA 7 j\u00e4rgi t\u00e4hist (n.d.) ja lisatakse vaatamise kuup\u00e4ev.", { italics: true }),
  todo("Allikaid on kuus, mall n\u00f5uab v\u00e4hemalt viit. K\u00f5ik URL-id on kontrollitud ja avanevad. Aastaarvud on parandatud: varem seisis siin mitu v\u00e4lja m\u00f5eldud aastat. Kui lisad omalt poolt allikaid \u2014 eriti elektrotehnika-alaseid \u2014 vormista need samas stiilis ja kontrolli malli LISA E n\u00e4idete j\u00e4rgi."),
  new Paragraph({ children: [new PageBreak()] }),

  h1("LISAD"),
  p("K\u00e4esolevas peat\u00fckis on esitatud l\u00f5put\u00f6\u00f6d t\u00e4iendavad materjalid."),
  table([
    ["Lisa", "Sisu"],
    ["Lisa A", "Andmemudeli t\u00e4ielik olemi-suhte diagramm (ERD)"],
    ["Lisa B", "Kalkulaatori algoritmi pseudokood"],
    ["Lisa C", "Kuvat\u00f5mmised k\u00f5igist rakenduse vaadetest"],
    ["Lisa C", "Noorem tarkvaraarendaja kompetentsin\u00f5uded (malli kohustuslik lisa)"],
    ["Lisa D", "Eksamit\u00f6\u00f6 hindamiskriteeriumid \u2013 TAR (malli kohustuslik lisa)"],
  ], [1800, 7200]),


  h2("Lisa A. Andmemudeli t\u00e4ielik ERD"),
  figure("Whole_Building_Electrical_ERD.png",
         "Hoone elektripaigaldise t\u00e4ielik olemi-suhte diagramm.", 580, DRAFT),

  h2("Lisa B. Kalkulaatori algoritmi pseudokood"),
  p("SISEND: hoone t\u00fc\u00fcp, tubade arv, pistikute arv, valgustite arv, elektripliidi olemasolu", { italics: true }),
  p("1. reeglid := loe andmebaasist arvutusreeglid, mille hoone t\u00fc\u00fcp vastab sisendile", { italics: true }),
  p("2. IGA reegli KOHTA:", { italics: true }),
  p("      tarbijaid := vali sisendist reegli ahelat\u00fc\u00fcbile vastav arv", { italics: true }),
  p("      ahelaid   := \u00dcLESPOOLE \u00dcMARDA (tarbijaid / reegli jagaja)", { italics: true }),
  p("      KUI ahelaid > 0 SIIS", { italics: true }),
  p("          kaitse := odavaim laos olev toode, mille kategooria = kaitsel\u00fclitid JA nimivool = reegli nimivool", { italics: true }),
  p("          kaabel := odavaim laos olev toode, mille kategooria = juhtmed JA ristl\u00f5ige = reegli ristl\u00f5ige", { italics: true }),
  p("          lisa loendisse kaitse koguses (ahelaid)", { italics: true }),
  p("          lisa loendisse kaabel koguses (ahelaid \u00d7 tubade arv \u00d7 8 m)", { italics: true }),
  p("3. lisa loendisse jaotuskilp koguses 1", { italics: true }),
  p("4. lisa loendisse rikkevoolukaitse koguses 1", { italics: true }),
  p("5. IGA rea KOHTA: rea summa := kogus \u00d7 \u00fchikuhind", { italics: true }),
  p("6. salvesta arvutus koos sisendandmetega ajalukku", { italics: true }),
  p("V\u00c4LJUND: materjalide loend koos \u00fchiku-, rea- ja kogumaksumusega", { italics: true }),
  todo("Kontrolli pseudokood koodi vastu \u00fcle \u2014 eriti kaabli pikkuse valem. Rakenduses on kaabli pikkus ahela kohta seotud tubade arvuga: 1 tuba andis 8 m ahela kohta, 3 tuba 24 m."),

  new Paragraph({ children: [new PageBreak()] }),
  h2("Lisa C. Noorem tarkvaraarendaja kompetentsin\u00f5uded"),
  p("Alljärgnev on kopeeritud kooli lõputöö mallist ja on kohustuslik lisa."),
  table([
    ["Kood", "Kompetents", "Sisu"],
    ["B.2.1", "Toote või projekti kavandamine (e-CF kompetents A.4.)", "1. annab projekti kavandamiseks vajaliku sisendi aja ja muu ressursi vajaduste osas; 2. osaleb tehnoloogiate ja töövahendite valiku protsessis."],
    ["B.2.2", "Rakenduse projekteerimine (e-CF kompetents A.6.)", "1. osaleb arhitektuuri planeerimisel, lähtudes süsteemi arhitektuuri nõuetest (jõudlus, hooldatavus, laiendatavus, mastaabitavas, kättesaadavus, turvalisus ja juurdepääsetavus); 2. kasutab oma töös testimisest ja prototüüpimisest saadud sisendit; 3. osaleb kasutajaliidese kavandamisel."],
    ["B.2.3", "Tehnoloogia arengu jälgimine (e-CF kompetents A.7.)", "1. hoiab end kursis IKT uusimate tehnoloogiliste saavutustega, kasutades asjakohaseid informatsiooniallikaid."],
    ["B.2.4", "Kavandamine ja väljatöötamine (e-CF kompetents B.1. ja B.2)", "1. hindab vastuvõtu tingimuste realiseeritavust kooskõlas olemasolevate piirangutega, 2. kavandab oma töö, lähtudes vastuvõtu tingimustest; 3. töötab välja ja integreerib tarkvarakomponente, lähtudes ettevõttes kasutusel olevast metoodikatest ja parimatest praktikatest (sh koodistandardid, agiilsed "],
    ["B.2.5", "Testimine (e-CF kompetents B.3.)", "1. kirjutab (automaat)teste enda kirjutatud/kirjutatavale koodile; 2. testib enda loodud tarkvarakomponentide põhifunktsionaalsust ja nõuetele vastavust, kasutades sobivat ja efektiivset testimise metoodikat."],
    ["B.2.6", "Lahenduse juurutamine/paigaldamine/kasutuselevõtt (e-CF kompetents B.4", "1. tagab, et loodud tarkvarakomponendid on paigaldatavad (sh kasutades automaatpaigaldussüsteeme); 2. paigaldab loodud tarkvarakomponendid nõutavasse keskkonda (sh test-, eeltootmis- ja tootmiskeskkond) vastavalt ettevõttes kasutatavale reliisiprotsessile; 3. osaleb juurutusprotsessis."],
    ["B.2.7", "Dokumentatsiooni koostamine (e-CF kompetents B.5.)", "1. tagab dokumentatsiooni olemasolu ja ajakohasuse kogu loodud lahenduse elutsükli jooksul; 2. dokumenteerimisel lähtub üldlevinud parimatest praktikatest (sh programmeerimiskeelte dokumenteerimis-standardid ja vahendid) ja ettevõttes kehtestatud nõuetest."]
  ], [1200, 2800, 5000]),

  new Paragraph({ children: [new PageBreak()] }),
  h2("Lisa D. Eksamit\u00f6\u00f6 hindamiskriteeriumid \u2013 TAR"),
  p("Alljärgnev on kopeeritud kooli lõputöö mallist ja on kohustuslik lisa."),
  p("Hindamiskriteeriumid"),
  p("Nõutud kompetentsid"),
  p("Vastavus teemale ja erialale"),
  p("(min kriteerium: töö peab olema seotud tarkvara arendusega)"),
  p("B.2.1 Projekti kavandamine"),
  p("Praktiline kasutatavus"),
  p("(min kriteerium: konkreetse sihtgrupi või konkreetse kliendi olemasolu, töö baseerub reaalsel vajadusel)"),
  p("B.2.1 Projekti kavandamine"),
  p("Töö maht, töövahendid ja -võtted"),
  p("(min kriteerium: maht vähemalt 156 tundi, teostatud sobivate vahendite ja töövõtetega)"),
  p("B.2.2 Rakenduse projekteerimine"),
  p("B.2.4 Kavandamine ja väljatöötamine"),
  p("B.2.5 Testimine"),
  p("B.2.6 Juurutamine"),
  p("Teoreetilise osa sisu ja vormistus"),
  p("(min kriteerium: teoreetiline osa on loogiline struktuuriga, töö osad ja vormistus vastavad min nõuetele)"),
  p("B.2.4 Kavandamine ja väljatöötamine"),
  p("B.2.7 Dokumenteerimine"),
  p("Erialane terminoloogia ja keelekasutus"),
  p("(min kriteerium: kasutatud on arusaadavat erialast terminoloogiat, dokumenteerimine vastab minimaalsetele nõutele)"),
  p("B.2.7 Dokumenteerimine"),
  p("Kasutatud allikad"),
  p("(min kriteerium: vähemalt 5 asjakohast allikat)"),
  p("B.2.1 Kavandamine"),
  p("Praktilise lahenduse kvaliteet"),
  p("(min kriteerium: lahenduse töö on demonstreeritud, lahendus on osaliselt testitud, üldjoontes vastab parimatele praktikatele)"),
  p("B.2.4 Kavandamine ja väljatöötamine"),
  p("B.2.5 Testimine"),
  p("B.2.6 Juurutamine"),
  p("Praktilise lahenduse jätkusuutlikus, edasiarendamise võimalused"),
  p("(min kriteerium: selgitatud vastavust kaasaegsetele tehnoloogiatele, esitatud arendusvõimalused)"),
  p("B.2.3 Tehnoloogia arengu jälgimine"),
  p("Retsensendi arvamus"),
  p("(min kriteerium: retsensent peab olema eriala spetsialist)"),
  p("Üldoskused"),
  p("Töö kaitsmine"),
  p("(min kriteerium: oskab selgitada lahendust ja selle väljatöötamist, vastab rahuldavalt enamikule komisjoni küsimustele)"),
  p("Üldoskused"),
);

// ======================= BUILD ===========================================
const doc = new Document({
  styles: {
    default: {
      document: { run: { font: FONT, size: SZ }, paragraph: { spacing: { line: LINE } } },
    },
  },
  sections: [{
    properties: {
      page: { margin: { top: convertMillimetersToTwip(25), right: convertMillimetersToTwip(25), bottom: convertMillimetersToTwip(25), left: convertMillimetersToTwip(25) } },
    },
    footers: {
      default: new Footer({
        children: [new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ children: [PageNumber.CURRENT], font: FONT, size: 20 })],
        })],
      }),
    },
    children,
  }],
});

Packer.toBuffer(doc).then((buf) => {
  fs.writeFileSync(OUT, buf);
  console.log("KIRJUTATUD:", OUT);
  console.log("suurus:", Math.round(buf.length / 1024), "KB");
  console.log("jooniseid:", figNo);
});
