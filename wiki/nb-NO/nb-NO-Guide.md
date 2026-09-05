[🌐 Languages / Langues](../Home.md)

# GW GUI Brukerveiledning

GW GUI er et Windows-program for å lese, skrive, konvertere, inspisere og emulere diskett-disk-bilder. Det kan kontrollere Greaseweazle maskinvare, arbeid med disk-bilde-filer gjennom sin interne motor, og kjøre lagret emulert-maskin konfigurasjoner.

Denne guiden beskriver det engelske grensesnittet som er vist i gjeldende versjon av programmet. Den er skrevet som kilden til den utskrivbare brukermanualen: skjermbilder illustrerer kontroller, mens den omkringliggende teksten forklarer hva du skal velge, hvorfor velge den, og hvordan du kan verifisere resultatet.

> **Viktig:** Å lese en disk er ikke-destruktiv. Skrive, slette, firmware oppdatering og noen maskinvareverktøy kan endre medier eller maskinvare. Les advarselen som er knyttet til den aktuelle prosedyren før du klikker ** Kjør**.

### Hvordan bruke denne guiden

Hvis dette er første gang du bruker GW GUI, komplett [Åpne i gang](#getting-started)Følg deretter [Leser en disk](#reading-a-disk)Hvis programmet allerede er konfigurert, gå direkte til kapitlet for operasjonen du vil utføre. Alternativkapittelene tjener som referanse når en prosedyre ber deg om å endre en stasjon, motor, profil eller emulerte maskininnstillinger.

Grensesnittnavn er vist i **dristige**. Filnavn, stier, kommandoer og bokstavelige verdier vises som `code`Noter forklarer normal atferd; advarsler identifiserer operasjoner som kan endre en disk, kontroller eller lagret konfigurasjon.

## Innhold

1. [Forstå arbeidsflyten](#understanding-the-workflow)
2. [Å komme i gang](#getting-started)
3. [hovedvindu](#main-window)
4. [Leser en disk](#reading-a-disk)
5. [Kvar en disk](#writing-a-disk)
6. [Konverter diskbilder](#converting-disk-images)
7. [Visualiserer et diskbilde](#visualizing-a-disk-image)
8. [Utforske diskinnhold](#exploring-disk-contents)
9. [Bruk av verktøyene](#using-the-tools)
10. [Emulering](#emulation)
11. [Applikasjonsalternativer](#application-options)
12. [Emuleringsalternativer](#emulation-options)
13. [Amiga konfigurasjon](#amiga-configuration)
14. [Hardware diagnostikk og vedlikehold](#hardware-diagnostics-and-maintenance)
15. [Logg og operasjonshistorie](#logs-and-operation-history)
16. [Applikasjonsdata og bærbar bruk](#application-data-and-portable-use)
17. [Anbefalte arbeidsflyter](#recommended-workflows)
18. [Sikkerhetskontrollliste](#safety-checklist)
19. [Trobleshooting](#troubleshooting)
20. [Glossary](#glossary)
21. [Kort referanse](#quick-reference)

## Forstå arbeidsflyten

GW GUI separerer fysiske diskoperasjoner fra bildefiloperasjoner:

| Mål | Inngang | Utgang | Anbefalt side |
|---|---|---|---|
| Bevar en diskett | Fysisk disk | Bildefil | **Les** |
| Opprette en diskett på nytt | Bildefil | Fysisk disk | **Skriv** |
| Endre bildeformat | Bildefil | En eller flere bildefiler | **Konvertering** |
| Inspeksjon av spor og avvik | Bildefil | Visuell analyse | **Visualisering** |
| Bla gjennom filer som er lagret i et bilde | Støttet bilde/filsystem | Filer og mapper | **Disk Explorer** |
| Diagnose en stasjon eller kontroller | Greaseweazle maskinvare | Målinger eller status | **Verktøy** |
| Kjør en lagret virtuell maskin | Lagret maskinkonfigurasjon | Emuleringsøkt | **Emulering** |

For bevaring, først gjøre en rå fangst og holde det uendret som en mester. Opprett konverterte eller reparerte arbeidskopier fra masteren. Dette unngår å gjenta en fysisk lesing og bevarer informasjon som et sektorbasert format ikke kan beholde.

## Komme i gang

### Krav

- Windows med Microsoft .NET Skrivebordskjøringstid som kreves av programmet.
- A Greaseweazle kontroller for fysisk diskettdrift.
- En konfigurert bane til `gw.exe` Når du bruker Greaseweazle Host Tools motor.
- Lovlig oppnådd ROM filer når en emulert maskin krever dem.

Programmet kontrollerer den nødvendige .NET-kjøretiden ved oppstart. Hvis det mangler, følg installasjonsprompten og start på nytt GW GUI.

### Før du kobler til maskinvare

Sjekk følgende før du kjører en fysisk-disk-operasjon:

1. Koble til Greaseweazle kontroller til en stabil USB Port.
2. Koble diskettkabelen med riktig orientering.
3. Koble til strømforsyningen før du setter inn verdifulle medier.
4. Bekreft at enhetens størrelse og tetthet samsvarer med disken.
5. Skrivebeskytt kildedisken når det er mulig.

GW GUI kan ikke hindre skader forårsaket av feil cabling, upassende effekt eller en mekanisk utrygg kjøring. Test ukjent maskinvare med en utnyttbar disk først.

### Første lansering

1. Åpne `gwgui.exe`.
2. Åpne **Valg**.
3. I **Styrere og stasjoner**, skanne for kontrolleren og konfigurere stasjonen.
4. Kontroller eller velg banen til `gw.exe`.
5. I **Motorer** Velg hvilken motor som skal utføre hver operasjon.
6. Gå tilbake til hovedvinduet og velg den nødvendige operasjonsfanen.

### Bekrefte at oppsettet er klart

En arbeidsoppsett bør vise kontrolleren og kjøre i statuslinjen, for eksempel et drivnummer, størrelse, tetthet og COM Port. I **Alternativer > Styrere og stasjoner **, kontrolleren bør merkes ** Tilgjengelig ** og stasjonen ** Konfigurert **Løp ** Kontrollørinformasjon** før du leser verdifulle medier hvis du vil verifisere kommunikasjon uten å endre en disk.

### Velg en motor

GW GUI kan utsette mer enn én implementering for enkelte operasjoner. Den **Greaseweazle Host Tools** motor påkaller konfigurert `gw.exe`den interne GW GUI motor håndterer støttede operasjoner inne i applikasjonen. Motorvalg er eksplisitt og uavhengig for lesing, skriving, konvertering og Disk ExplorerHvis en operasjon ikke støttes av den valgte motoren, GW GUI rapporterer at tilstanden i stedet for å endre motorer automatisk.

## Hovedvindu

Hovedvinduet grupperer hovedoperasjonene i syv faner:

- **Les** Oppretter et bilde fra en fysisk disk.
- **Skriv** Skriver et bilde til en fysisk disk.
- **Konvertering** Konverterer ett disk-bildeformat til ett eller flere utgangsformat.
- **Visualisering** Viser spor og flux eller dekodede data.
- **Disk Explorer** Bla gjennom støttede filsystemer og diskinnhold.
- **Verktøy** gir maskinvare vedlikehold og diagnostiske kommandoer.
- **Emulering** håndterer og kjører lagrede emulerte maskiner.

Konsollen nederst viser kommandoen som utføres og dens utgang. Statuslinjen rapporterer den valgte stasjonen, profilen og gjeldende tilstand.

### Lese grensesnittet

De fleste operasjonssider følger samme mønster:

1. **Kilde eller destinasjon** kontroller identifisere disk, bilde eller mappe.
2. **Formater kontroller** Velg automatisk deteksjon eller eksplisitt maskin og format.
3. **Profilkontroller** anvende gjenbrukbare innstillinger.
4. **Avanserte innstillinger** Eksponeringsparametre som normalt er valgfrie.
5. **Kjør** starter operasjonen.
6. Den **konsoll** viser den genererte kommandoen, fremgang, advarsler og feil.

Den **Kjør** knappen betyr ikke at alle verdier er trygge for den innsatte disken. Les alltid destinasjonen og den valgte stasjonen før en skrive- eller vedlikeholdsoperasjon.

### Statuslinje og konsoll

Den venstre siden av statuslinjen identifiserer den aktive fysiske stasjonen. Senteret viser den aktive profilen når en er valgt. Statens indikator rapporterer om søknaden er klar eller opptatt. Konsollen er ikke bare diagnostisk: det er autoritativt register over kommandoen som sendes til den valgte motoren. Bruk kopikontrollen når du trenger å bevare eller dele kommandoen.

## Lese en disk

Åpne **Les** tab for å fange en fysisk diskett som et bilde.

<p align="center"><img src="../images/main-read-en.png" alt="Les faneblad" width="78%"></p>

### Grunnleggende prosedyre

1. Sett inn kildedisken i den konfigurerte enheten.
2. Velg bildetypen:
   - **Råbilde (SCP)** Bevarer flux-nivå informasjon.
   - **Kjent diskformat** oppretter et bilde ved hjelp av en valgt maskin og format.
3. Velg målmappen.
4. Skriv inn utfilnavnet.
5. Velg en profil om nødvendig.
6. Klikk **Kjør**.

Konsollen viser nøyaktig kommando og fremgang. Ikke fjern disken eller frakoble kontrolleren før operasjonen er ferdig.

### Velg utgangstype

Bruk **Råbilde (SCP)** når målet er arkivering fangst, analyse, gjenoppretting eller senere konvertering. Et råbilde registrerer timingsinformasjon og flere revolusjoner, som er nyttig for uvanlige formater, svake sektorer, beskyttelsesordninger og skadede medier.

Bruk **Kjent diskformat** Når du allerede kjenner diskfamilien og trenger et direkte brukbart sektorbilde. Dette valget kan være mindre og lettere å åpne i annen programvare, men det representerer det dekodede resultatet i stedet for alle detaljer observert av stasjonen.

Når det er usikkert, lage det rå bildet først. Du kan konvertere den senere uten å lese disken igjen.

### Mappe, filnavn og profil

Den **Mappe ** er destinasjonskatalogen. Den ** Filnavn** bør identifisere disken uten å stole på dens fysiske etikett. Et nyttig arkivnavn inneholder tittel, disknummer eller side, og en betingelsesnote når det er aktuelt. Ikke legg til en formatutvidelse som er i konflikt med det valgte utdataformatet.

A **Profil ** anvende et lagret sett av leseparametre. Velg kun én når du vet hva den inneholder. Den ** Standard** profil er egnet for et normalt første forsøk; en spesialisert gjenopprettingsprofil kan bevisst lese flere revolusjoner eller et annet sporområde og derfor ta lengre tid.

### Avanserte innstillinger

Utvid **Avanserte innstillinger** å få tilgang til formatspesifikke eller ekspertparametre. La disse verdiene være uendret med mindre disken krever et bestemt sporområde, revolusjonstelling eller kontrollalternativ.

Vanlige avanserte verdier inkluderer:

| Innstilling | Formål | Når å endre det |
|---|---|---|
| Sporområde | Begrenser sylindrene og hodet til å lese | Ensidige medier, uvanlig geometri eller et målrettet gjenopprettingspass |
| Revolusjoner | Kontrollerer hvor mange rotasjoner som prøves | Øke for ustabile eller beskyttede spor; redusere bare for hastighet når det er nødvendig |
| Ekspertargumenter | Passerer ytterligere motorparametre | Bare når du følger dokumentert Greaseweazle Veiledning |

### Bekrefte en vellykket lesing

Ikke bruk bare fraværet av en feildialog. Etter at kommandoen er ferdig:

1. Bekreft at utfilen eksisterer og ikke er tom.
2. Les de endelige konsolllinjene for feil eller manglende spor.
3. Åpne bildet i **Visualisering** å kontrollere at begge sider og det forventede sporområdet inneholder data.
4. Åpne den i **Disk Explorer** Når filsystemet støttes.
5. Behold operasjonsloggen med viktige arkivopptak.

Hvis gjentatte lesere er forskjellige, bevare hver rå fangst i stedet for å overskrive den første. Forskjellene kan være nyttige under utvinning.

## Skrive en disk

Åpne **Skriv** tab for å skrive et eksisterende bilde til en fysisk diskett.

<p align="center"><img src="../images/main-write-en.png" alt="Skriv fane" width="78%"></p>

### Grunnleggende prosedyre

1. Sett inn måldisken.
2. Velg kildebildet med **Bla gjennom**.
3. Bekreft det detekterte formatet.
4. Velg en profil om nødvendig.
5. Klikk **Kjør**.

Skriving erstatter data på destinasjonsdisken. Kontroller den valgte stasjonen og bildet før du starter.

> **Advarsel:** Å skrive er ødeleggende. Det erstatter magnetiske data på destinasjonsdisken. Bruk et skrivebeskyttet kildearkiv og en separat destinasjonsdisk når det er mulig.

### Før du skriver

Sjekk fire elementer før du klikker **Kjør**:

1. **Bilde:** den valgte banen er det tiltenkte kildebildet.
2. **Disk:** Disken i stasjonen kan trygt overskrives.
3. **Kjør:** konfigurert størrelse og tetthet passer til målmediet.
4. **Format:** Automatisk deteksjon eller manuelt valgt format passer til bildet.

Hvis kildebildet ikke er testet, kan du åpne det i **Visualisering ** eller ** Disk Explorer** Først. En vellykket skriving kan ikke reparere et ufullstendig kildebilde.

### Sporkontroll og modifikasjon

Når et bilde er valgt, **Visualiser spor ** Åpner sin banerepresentasjon. ** Endre** Viser de støttede bildeendringene før skriving. Tilgjengelige handlinger avhenger av det valgte formatet og motoren.

### Bekrefte en skriftlig disk

Når motoren støtter verifisering, bruk den for viktige medier. Ellers kan du lese den skriftlige disken tilbake til et nytt bilde og sammenligne dets dekodede innhold eller inspisere det i **Visualisering** Hold verifiseringsopptaket adskilt fra det opprinnelige bildet slik at originalen aldri overskrives.

Hvis skriving mislykkes ved konsistente spor, sjekk disktilstand, tetthet, kjøre renhet og kjørekonfigurasjon. Hvis feil oppstår tilfeldig, sjekk USB stabilitet og styringskommunikasjon.

## Konverterer diskbilder

Den **Konvertering** tab konverterer et kildebilde til ett eller flere destinasjonsformater.

<p align="center"><img src="../images/main-conversion-en.png" alt="Konverteringsfane" width="78%"></p>

### Grunnleggende prosedyre

1. Velg kildebildet.
2. Oppgi utgangsnavn.
3. Velg en maskinfamilie.
4. Velg ett eller flere utdataformater og utvidelser.
5. Slå på **Legg til etiketter** hvis filnavn skal bruke det konfigurerte merkemønsteret.
6. Klikk **Kjør**.

Den **Valgt ** panelet viser de ønskede utgangene. ** Fil migrasjon** gir den dedikerte arbeidsflyten for å overføre støttede filer i stedet for å utføre en standard bildekonvertering.

### Velger formater

Den **Maskin ** liste filtrerer formatene vist i ** Format** panel. Et formatnavn beskriver den logiske diskutformingen; utvidelsen beskriver utgangsbeholderen. Noen formater kan representeres av mer enn én forlengelse, og noen beholdere kan ikke bevare alle funksjoner av en rå kilde.

Velg kun utganger du faktisk trenger. Flere formater er nyttige når du oppretter en arkivmester, en emulatorkompatibel kopi og en kopi for et annet analyseverktøy i én operasjon.

### Utgangsnavn og tagger

**Utgangsnavn ** lar deg styre basisnavnene som genereres for valgte formater. ** Legg til etiketter ** bruke filnavnmønsteret konfigurert i ** Alternativer > Generelt**. Tags kan kode familie, format, forlengelse, dato eller tid. Forhåndsvisning av eksemplet i Alternativer før konvertering av et stort parti slik at filer heter konsekvent.

### Sjekker konverteringsresultater

For hver ønsket utgang:

1. Bekreft at en fil ble opprettet.
2. Sjekk konsollen for spor eller sektorer som ikke kunne dekodes.
3. Åpne resultatet i **Disk Explorer** Hvis det inneholder et støttet filsystem.
4. Sammenlign forventet diskkapasitet og innhold med kilden.

En konvertering kan fullføres samtidig som informasjonstap som er iboende destinasjonsformatet. Behold det opprinnelige råbildet selv når det konverterte bildet vises riktig.

## Visualisere et diskbilde

Den **Visualisering** fanen viser strukturen og datafordelingen av et bilde.

<p align="center"><img src="../images/main-visualization-en.png" alt="Fanen Visualisering" width="78%"></p>

1. Klikk **Åpne et diskbilde**.
2. Behold **Automatisk deteksjon** aktivert, eller velg maskinen og formater manuelt.
3. Bruk **Link zoom** å holde begge sider på samme zoomnivå.
4. Bruk **Nullstill** å gjenopprette den første visningen.
5. Åpne **Inspektør** for detaljert informasjon om det valgte området.

Legenden skiller normal flux, korte og lange overganger, overskrifter, dekodede data og detekterte avvik. Et råbilde kan inneholde data som ikke kan dekodes til et kjent filsystem, men som fortsatt kan inspiseres her.

### Tolker visningen

Hvert stort sirkulært panel representerer én diskside. Senteret identifiserer siden og dens nåværende datatilstand; konsentriske posisjoner tilsvarer spor. Farger klassifiserer detekterte området i henhold til legenden. Visualizeren er ment å svare på spørsmål som:

- Inneholder bildet data på den ene siden eller begge?
- Er de forventede sporene tilstede?
- Er avvik isolert eller gjentatt på tvers av disken?
- Har automatisk deteksjon identifisert en mulig maskin og format?

En anomalisk farge er en grunn til å inspisere regionen, ikke bevis på at disken er ubrukelig. Kopier beskyttelse, ikke-standard formatering, en svak opptak og en skadet sektor kan produsere ulike strukturer som krever kontekstuell tolkning.

### Anbefalt inspeksjonssekvens

Start med koblet zoom aktivert for å sammenligne begge sider i samme skala. Velg et mistenkelig område, åpne **Inspektør** og sammenligne det med nabospor. Hvis resultatet ser ut til å være et deteksjonsproblem, deaktiver automatisk deteksjon og velg en kjent maskin og format. Gå tilbake til automatisk deteksjon etter testen, så en tvangsinnstilling ikke ved et uhell brukes til et annet bilde.

## Utforske diskinnhold

Den **Disk Explorer** fanen bla gjennom støttede diskbilder som et filhierarki.

<p align="center"><img src="../images/main-disk-explorer-en.png" alt="Disk Explorer fanen" width="78%"></p>

1. Åpne et eksisterende bilde eller les en disk.
2. Behold **Automatisk deteksjon** aktivert med mindre du trenger å tvinge en maskin eller format.
3. Se gjennom voluminformasjonen: system, beskyttelse, filsystem, kapasitet, ledig plass og elementtelling.
4. Bla gjennom mapper i det venstre panelet.
5. Velg et element for å vise detaljene i høyre panel.

Hvis bildet eller filsystemet ikke støttes, bruk **Visualisering** å inspisere den rå strukturen i stedet.

### Forstå panelene

Den øverste sammendraget beskriver det monterte bildet og detektert volum. Det nedre venstre panelet inneholder kataloghierarkiet. Den sentrale tabellen viser elementer i den valgte katalogen med navn, endringsdato, type og størrelse. Det høyre panelet viser detaljer for det valgte elementet.

Disk Explorer Det betyr ikke at alle råspor ble dekodet perfekt. Bruk volumsammendraget og elementet teller som en rask plausibilitetssjekk, deretter åpne representative filer eller sammenligne dem med en kjent katalogliste når bevaringsnøyaktigheten er viktig.

### Når ingenting vises

Først bekrefte at bildestien er riktig. Sjekk deretter detektert maskin og format. Et gyldig bilde kan inneholde et ustøtt eller skadet filsystem, i hvilket tilfelle oppdageren kan forbli tom selv om **Visualisering** Viser registrerte data. Ikke overskriv eller kast kildebildet basert på en tom oppdager.

## Bruk av verktøyene

Den **Verktøy** fanegrupper Greaseweazle vedlikehold.

<p align="center"><img src="../images/main-tools-en.png" alt="Verktøyfanen" width="78%"></p>

Velg en kommando fra listen til venstre, se på parametrene og klikk deretter på **Kjør** Destruktive eller maskinvareendringskommandoer bør kun brukes etter å ha verifisert den valgte kontrolleren og stasjonen.

De fleste verktøydialoger inneholder tre områder: parametre øverst, et status- og råutgangsområde i sentrum, og den genererte kommandoen nederst. Kommandoen forhåndsvisning endres som alternativer er aktivert. En ukontrollert parameter betyr normalt «ikke endre denne verdien», mens en kontrollert parameter inkluderer den verdien i kommandoen.

De individuelle diagnostiske dialogene er beskrevet i [Hardware diagnostikk og vedlikehold](#hardware-diagnostics-and-maintenance).

## Emulering

### Åpne en lagret maskin

Den **Emulering ** fanelister lagret konfigurasjoner. Velg ett og klikk ** Åpne**. Hver kjøremaskin vises i sin egen fane.

<p align="center"><img src="../images/main-emulation-welcome-en.png" alt="Emulering velkomstskjerm" width="78%"></p>

Opprette og redigere maskiner i **Alternativer > Emulering > Konfigurasjoner ** og ** Alternativer > Emulering > Amiga**.

Hvis det ikke vises noen konfigurasjon, oppretter du først en i Alternativer. En lagret konfigurasjon kombinerer maskinmodellen, emulatorversjonen, ROM, minne, video, lyd, lagring og inndatakartlegging. Å lagre en konfigurasjon starter ikke; gå tilbake til hovedlinjen **Emulering ** fanen og klikk ** Åpne**.

### Kjøremaskinkontroller

<p align="center"><img src="../images/main-emulation-running-en.png" alt="Kjøre emulert maskin" width="78%"></p>

Kjøremaskinverktøylinjen gir strøm, pause, tilbakestilling, lagringstilstand, lasttilstand, fangst og skjermkontroller. Det viser også:

- de konfigurerte hurtiglagrings- og hurtiglastsnarveiene;
- aktiv render, som Direct3D 11;
- snarveiene for full- og musutgivelse;
- lyd, kontroller og musetilstand;
- den aktuelle oppløsningen, oppdateringshastigheten og rammehastigheten.

Diskstrimmelen i bunnen av emuleringsskjermen administrerer flyttbare medier for hver emulert enhet. Tastaturoppgaver kan endres i **Alternativer > Emulering > Snarveier**, mens emulert tastatur, mus og kontroller kartlegginger er konfigurert i de tilsvarende Amiga faner.

### Verktøylinjereferanse

| Kontrollgruppe | Formål |
|---|---|
| Makt og pause | Starter, stopper, pauser eller gjenopptar den emulerte maskinen |
| Tilbakestill kontroller | Utfører den konfigurerte myk eller harde tilbakestillingshandlingen |
| Statskontroller | Lagrer eller laster en emulatortilstand for rask videreføring |
| Opptak | Lagrer et bilde av den emulerte skjermen |
| Vis | Endrer skjermpresentasjonen eller går inn i fullskjerm |
| Hurtigpåminning | Viser de aktive lagrings-/last-snarveiene |
| Render | Rapporter den aktive videomotoren |
| Inngangspåminnelse | Viser full- og musutgivelsessnarveier |
| Enhetsindikatorer | Rapporterer lyd, kontroller og musetilstand |
| Performance | Rapporter utgangsstørrelse, oppdateringsfrekvens og rammehastighet |

### Forlater fullskjerm eller frigjør musen

Verktøylinja viser de tilordnede tastene. I den illustrerte konfigurasjonen, **Alt+ Tilbake ** slår av fullskjerm og ** F12** frigjør musen. Behandle de viste verdiene som autoritative fordi snarveier kan endres.

### Bruke diskettmedier

Drivstrimmelen identifiserer hver emulert enhet, som `DF0:`. Bruk mediekontrollene til å sette inn, erstatte eller utsette et bilde. Utskifting av medieendringer bare kjørermaskinens innsatte disk; den endrer ikke definisjonen av lagringsenheten i den lagrede maskinen med mindre handlingen eksplisitt lagres.

## Programalternativer

Åpne **Valg** fra hovedvinduet for å konfigurere programmet.

### Generelt

<p align="center"><img src="../images/options-general-en.png" alt="Generelle alternativer" width="72%"></p>

Den **Generelt** Fanen inneholder:

- standard diskbildemappe;
- grensesnittspråk og tema;
- filnavnmerkegenerering for konverteringer;
- forhåndsdefinerte og nylige tilpassede tag mønstre;
- Et eksempel på levende filnavn.

Merkevariabler inkluderer kildenavn, familie, format, forlengelse, dato og klokkeslett. Bruk tilbakestillingsknappen til å gjenopprette standardmønsteret.

Filnavnet forhåndsvisningsoppdateringer før noen filer opprettes. Bruk den til å oppdage dupliserte separatorer, manglende utvidelser eller tvetydige navn. Nylig tilpassede mønstre gir rask tilgang til tidligere navneordninger uten å erstatte gjeldende forhåndsinnstilling.

### Logger

<p align="center"><img src="../images/options-logs-en.png" alt="Loggalternativer" width="72%"></p>

Logging kan konfigureres uavhengig for hver operasjon. For hver kategori velger du om du vil lagre logger, angir en maksimal filstørrelse og bestemmer om tidligere logger skal beholdes. En størrelse på `0` betyr ubegrenset. **Åpne mappe** Åpner loggkatalogen.

Slå på **Behold tidligere logger** for bevaring og diagnostisk arbeid der historien til flere forsøk er viktig. Deaktiver det når det siste resultatet er nyttig. Maksimale størrelsesgrenser gjelder for logglagring, ikke for opptak av diskbilder.

### Styrere og stasjoner

<p align="center"><img src="../images/options-controllers-and-drives-en.png" alt="Styrere og stasjoner" width="72%"></p>

Bruk denne fanen til:

- skann for tilkoblede kontroller;
- legge til og fjerne enhetskonfigurasjoner;
- Velg enhetsstørrelse, tetthet og hastighet;
- lagre maskinvareinnstillinger;
- Velg eller automatisk finne `gw.exe`;
- Sjekk for og last ned Greaseweazle Host Tools oppdateringer;
- gjenopprette en tidligere konfigurert kjørbar bane.

Lagrede maskinvareinnstillinger forblir tilgjengelige når en stasjon er midlertidig frakoblet.

#### Legg til en stasjon

1. Klikk **Skann** og vente på tilkoblede kontroller vises.
2. Klikk **Legg til en stasjon** Hvis den nødvendige enheten ikke allerede er oppført.
3. Velg det logiske drivnummeret, fysisk størrelse, opptakstetthet og rotasjonshastighet.
4. Lagre raden.
5. Bekreft at det viser **Tilgjengelig ** og ** Konfigurert**.

Bruk søppelkontrollen bare til å fjerne den lagrede konfigurasjonen; den kobler ikke fra maskinvaren. Hvis den samme kontrolleren vises på en annen COM port senere, skann igjen før anta at den lagrede porten fortsatt er gyldig.

#### Håndtering Greaseweazle Host Tools

**Finn gw.exe ** Søk kjente steder. ** Velg ** Velger en bestemt kjørbar. ** Sjekk etter oppdateringer ** spørringer tilgjengelige versjoner uten å erstatte den installerte. ** Last ned siste versjon ** installerer den valgte pakken, og ** Bruk forrige sti ** gjenoppretter tidligere konfigurert plassering. Etter å ha endret kjøringen, kjør ** Kontrollørinformasjon** å bekrefte at den valgte versjonen kan kommunisere med kontrolleren.

### Motorer

<p align="center"><img src="../images/options-engines-en.png" alt="Motorvalg" width="72%"></p>

Velg motoren uavhengig for lesing, skriving, konvertering og Disk Explorer. Den valgte motoren brukes strengt: hvis den ikke kan utføre den ønskede operasjonen, GW GUI rapporterer begrensningen i stedet for stille bytte motorer.

Denne uavhengigheten er intensjonell. For eksempel kan fysiske lesere bruke Greaseweazle Host Tools mens bildekonvertering og utforskning bruker den interne motoren. Record motorvalg i en profil eller prosjektnote når reprodusilitet er viktig.

### Profiler

<p align="center"><img src="../images/options-profiles-en.png" alt="Profiler" width="72%"></p>

Profiler lagrer gjenbrukbare innstillinger for lesing, skriving og konverteringsoperasjoner. Velg den relevante kategorien for å administrere profilene. En valgt profil vises i hovedvinduets statuslinje og i driftsskjermer.

Bruk profiler for repeterbare arbeidsflyter i stedet for som uforklarlige samlinger av ekspertflagg. Gi hver profil et formålsbestemt navn, som en bestemt enhet, diskfamilie eller gjenopprettingsmetode. Sjekk en profil etter oppdatering av den underliggende motoren fordi støttede alternativer kan endres.

## Emuleringsalternativer

Den **Emulering** alternativer inneholder generelle lagringsinnstillinger, globale snarveier, lagrede konfigurasjoner og maskinspesifikke innstillinger.

### Generelle emuleringsmapper

<p align="center"><img src="../images/options-emulation-general-en.png" alt="Generelle emuleringsalternativer" width="72%"></p>

Sett den delte emuleringslagringsmappen og standardmappene for fangster og lagrede tilstander. **Åpne mappe** Åpner den delte plasseringen i File Explorer.

Holde opptak og lagrede tilstander i separate mapper. En fangst er et vanlig bilde; en lagret tilstand inneholder emulatorspesifikk maskintilstand og kan avhenge av emulatorversjonen og konfigurasjonen som opprettet den. Sikkerhetskopier konfigurasjon og medier sammen med viktige lagrede stater.

### Globale snarveier

<p align="center"><img src="../images/options-emulation-shortcuts-en.png" alt="Emuleringssnarveier" width="72%"></p>

Søk etter en handling eller nøkkeltildeling, tilordne eller fjerne snarveier, gjenopprette standardinnstillinger og klare konflikter. Statuskolonnen identifiserer gyldige og motstridende oppdrag.

Hvis du vil endre en snarvei, klikker du på handlingen. **Tildel **, og trykk den ønskede tastekombinasjonen. Sjekk status før du avslutter innstillingene. ** Klare konflikter ** fjerner motstridende oppgaver; det gjenoppretter ikke standardkartleggingen. Bruk ** Gjenopprett standardverdier** Når du vil erstatte egendefinerte oppgaver med standardsettet.

### Lagrede konfigurasjoner

<p align="center"><img src="../images/options-emulation-configurations-en.png" alt="Lagrede emuleringskonfigurasjoner" width="72%"></p>

Denne siden lister lagrede maskiner. Velg en konfigurasjon for å redigere den i **Amiga** tab. Du kan oppdatere listen eller slette den valgte konfigurasjonen.

Sletting av en konfigurasjon fjerner den lagrede maskindefinisjonen. Det bør ikke brukes som en måte å utløse medier eller lukke en kjøremaskin. Før sletting, bemerk noen ROM, harddiskbilde og tilstandsfiler tilknyttet konfigurasjonen.

## Amiga konfigurasjon

Det aktuelle grensesnittet gir detaljert Amiga konfigurasjonssider. Den samme innstillingsstrukturen kan forlenges for andre emulerte systemer uten å endre hovedarbeidsflyten.

### Generelt

<p align="center"><img src="../images/options-amiga-general-en.png" alt="Amiga generelle innstillinger" width="72%"></p>

Velg den Amiga modell, lagre konfigurasjonen, installere eller erstatte emulatorversjonen, og definere standardmapper for harddisker og andre medier. **Søk versjoner** spør den offisielle emulator-versjon kilde.

Start med modellen fordi den begrenser senere sider. Endring kan endre tilgjengelig CPU, minne, ROM, chipset og lagringsvalg. Etter å ha valgt en emulatorversjon, lagrer du konfigurasjonen før du starter den fra hovedvinduet. Installerer en annen emulatorversjon erstatter den versjonen som brukes av konfigurasjonen; den oppretter ikke en annen kopi av maskinen.

### CPU

<p align="center"><img src="../images/options-amiga-cpu-en.png" alt="Amiga CPU innstillinger" width="72%"></p>

Den CPU siden viser prosessoren valgt av maskinmodellen og gir kompatibel presisjon, FPUog raske valg. Alternativer som ikke gjelder for den valgte modellen forblir deaktivert.

- **CPU modell** identifiserer den emulerte prosessoren.
- **Precision** Kontrollerer tidsmodellen. Sykkeleksakte moduser favoriserer maskinvarekompatibilitet, men krever mer vertsbehandling.
- **FPU** muliggjør en kompatibel flytpunktenhet når den støttes.
- **CPU hastighet** Velger opprinnelig timing eller akselerert modus.

For en baseline-konfigurasjon, hold modellen avledet CPU Opprinnelig hastighet. Endre akselerasjon bare etter maskinstøvler riktig i standardinnstillingene.

### RAM

<p align="center"><img src="../images/options-amiga-ram-en.png" alt="Amiga RAM innstillinger" width="72%"></p>

Konfigurer Chip RAM, langsom RAM, Rask RAMog støttet utvidelsesminne. Kompatibilitetsmeldinger forklarer restriksjoner for den valgte maskinen, og det totale konfigurerte minnet vises nederst.

**Chip RAM ** er tilgjengelig for egendefinerte chips og kreves av plattformen. ** Langsom RAM ** representerer kompatibel ekspansjonsminne som brukes av vanlige konfigurasjoner. ** Rask RAM ** er prosessorororientert ekspansjonsminne. ** Zorro III RAM** gjelder bare for modeller som støtter den ekspansjonsarkitekturen. Kompatibilitetsmeldingene og deaktiverte kontroller hindrer kombinasjoner som den valgte modellen ikke kan representere.

### ROM

<p align="center"><img src="../images/options-amiga-rom-en.png" alt="Amiga ROM innstillinger" width="72%"></p>

Velg systemet Kickstart ROM, valgfri utvidet ROM, og ROM Nøkkelen. Detektert-ROM liste viser navn, revisjoner og kompatibilitet med den valgte modellen. Velg et detektert ROM og klikk **Bruk**, eller bla til en fil manuelt.

ROM Filene leveres ikke av GW GUI. Bruk ROMs du har lov til å bruke.

Den detekterte listen er foretrukket å gjette fra et filnavn: det rapporterer ROM identitet og revisjon og evaluerer kompatibilitet med den valgte modellen. **Kompatibel ** er det normale valget; ** Delvis kompatibel ** indikerer at ROM kan boot men ikke akkurat matche maskinen. ** Oppdater ** Sjekker om konfigurert ROM beliggenheter. ** Bruk** tilordner den valgte detekterte ROM til konfigurasjonen.

### Video

<p align="center"><img src="../images/options-amiga-video-en.png" alt="Amiga Videoinnstillinger" width="72%"></p>

Konfigurer videostandard, aspektforhold, oppløsning, linjemodus, grensebeskjering, render, fargedybde, ramme hopping, gamma og flimring fikse. Ytterligere chipsetinnstillinger er tilgjengelige lenger ned på siden når støttes av den valgte modellen.

| Innstilling | Praktisk effekt |
|---|---|
| Videostandard | Velger PAL eller NTSC timing og forventet oppdateringsadferd |
| Sideforhold | Kontrollerer hvordan det emulerte bildet skaleres |
| Oppløsning | Velger automatisk eller eksplisitt utgangsdetalj |
| Linjemodus | Kontrollbehandling av interlaced eller linje-dobbelt utgang |
| Beskjær grenser | Fjerner kun ubrukt overscan når aktivert |
| Rendering | Velger grafikkbakstykket |
| Farge dybde | Velger utgangsfarge presisjon |
| Rammeovergang | Reduserer gjengitte rammer når aktivert |
| Gamma | Justerer lysstyrkerespons |
| Flicker fixer | Prosesser som ellers vil bli synlige |

Endre én skjerminnstilling om gangen. Hvis emuleringsvinduet blir tomt eller ustabilt, gå tilbake til automatisk oppløsning, hopp over deaktivert ramme, nøytral gamma og den tidligere fungerende render.

### Lyd

<p align="center"><img src="../images/options-amiga-audio-en.png" alt="Amiga lydinnstillinger" width="72%"></p>

Aktiver eller deaktiver lyd, velg utenheten og latensen, og konfigurer deretter interpolasjon, Amiga filtrering, filtertype, stereoseparasjon, diskettdrevet lyd og CD-audio volum.

Lavere latens reduserer forsinkelser, men kan forårsake drop-outs på en travel datamaskin. Øk det hvis lyden sprekker. Interpolering og Amiga lydfilter endre lyd reproduksjon i stedet for emulert programlogikk. Drivlydvolum kontrollerer den simulerte mekaniske lyden separat fra normal Amiga Lyd.

### Oppbevaring

<p align="center"><img src="../images/options-amiga-storage-en.png" alt="Amiga Oppbevaringsinnstillinger" width="72%"></p>

Lagringssiden viser enhetsidentifikatorer, typer, modeller, tilhørende medier og tilgjengelige handlinger. Legg til, konfigurer eller fjern enheter her. Diskettdisker og CD-er kan settes inn eller erstattes direkte fra en kjøremaskin.

Den **enhetsidentifikator ** Det emulerte systemet adresserer enheten. ** Type ** Skiljer diskett, harddisk, optiske og andre støttede enheter. ** Modell ** beskriver den emulerte maskinvaren, mens ** Tilknyttede medier** identifiserer det tilordnet bildet. Konfigurer enheten før du tilknytter verdifulle skrivbare medier, og hold sikkerhetskopier av harddiskbilder.

### Tastatur

<p align="center"><img src="../images/options-amiga-keyboard-en.png" alt="Amiga tastaturinnstillinger" width="72%"></p>

Søk Amiga nøkler og vertsoppgaver, tilordne nye nøkler, fjerne kartlegginger, gjenopprette standardinnstillinger eller klare konflikter. Statuskolonnen rapporterer om hver oppgave er gyldig.

Den venstre kolonnen navn den emulerte Amiga nøkkel; **Foreningen** Viser vertsnøkkelkombinasjonen. En gyldig kartlegging kan fortsatt være upraktisk hvis Windows eller programmet reserverer den samme snarveien, så test kritiske kombinasjoner inne i kjøremaskinen. Unngå å tilordne musutgivelsen eller fullskjermssnarveien til en nøkkel som den emulerte programvaren trenger ofte.

### Mus

<p align="center"><img src="../images/options-amiga-mouse-en.png" alt="Amiga museinnstillinger" width="72%"></p>

Sett fysisk musehastighet, velg hvilken analog pinne som styrer musen, justere den analoge døde sonen og hastigheten og konfigurere musehandlingskartlegginger. Gjenopprett standard eller klare kartleggingskonflikter når det er nødvendig.

Øk den døde sonen hvis en kontroller forårsaker pekerdrift. Juster hastigheten til venstre og høyre når begge pinne er aktivert. Den nedre kartleggingstabellen forbinder vertsinnganger med museknapper eller handlinger; inspisere konfliktstatusen etter å ha endret kontroller kartlegginger andre steder.

### Styrere

<p align="center"><img src="../images/options-amiga-controllers-en.png" alt="Amiga kontrollerinnstillinger" width="72%"></p>

Oppdag tilkoblede kontroller, tilordne enheter og kontrollertyper til Amiga porter, og konfigurer kontroller kartlegginger og turbo-brann innstillinger. Tilgjengelige valg avhenger av detektert maskinvare og den valgte maskinen.

Port 1 og port 2 konfigureres uavhengig. **Automatisk** kontrollertype er et fornuftig utgangspunkt, men programvare som forventer en bestemt joystick eller mus kan kreve en eksplisitt type. Kjør deteksjon før du tilordner en nytilkoblet kontroller. Turbo brann aktiverer gjentatte ganger en kartlagt inngang og bør forbli deaktivert med mindre spillet eller applikasjonen drar fordel av det.

## Maskinvarediagnostikk og vedlikehold

Disse dialogene åpnes fra **Verktøy ** tab. Hver dialog forhåndsvisning den genererte Greaseweazle kommando. Se den før du klikker ** Kjør**.

### Kontrollørinformasjon

<p align="center"><img src="../images/tool-controller-information-en.png" alt="Kontrollørinformasjon" width="62%"></p>

Viser informasjon rapportert av den valgte kontrolleren. Utvid **Rå utgang** Når du trenger fullstendig kommandorespons.

Bruk dette som den første diagnostiske kommandoen. En vellykket respons bekrefter at GW GUI kan starte den konfigurerte vertsverktøy kjørbare og kommunisere med den valgte enheten. Ta opp firmware- og maskinvareinformasjonen før du utfører en oppdatering.

### USB båndbredde

<p align="center"><img src="../images/tool-usb-bandwidth-en.png" alt="USB båndbredde" width="62%"></p>

Tiltak de tilgjengelige USB kommunikasjon båndbredde. Bruk den til å diagnostisere ustabile overføringer eller upassende USB Tilkobling.

Lukk annen programvare ved hjelp av kontrolleren før testing. Gjenta målingen etter å ha endret USB port, kabel eller nav. Sammenlign resultater under lignende forhold i stedet for å behandle en enkelt måling som en absolutt garanti.

### Drivhastighet

<p align="center"><img src="../images/tool-drive-speed-en.png" alt="Drivhastighet" width="62%"></p>

Måler drivrotasjonshastigheten. Øk antall målinger når du trenger et mer representativt resultat.

En enkelt måling er en rask kontroll; flere målinger avslører om hastigheten er stabil. La stasjonen nå normal hastighet før du tolker resultatet. En uventet verdi kan indikere feil konfigurert hastighet, et mekanisk problem eller et problem med målingsoppsett.

### Søk i hodet

<p align="center"><img src="../images/tool-seek-head-en.png" alt="Søk i hodet" width="62%"></p>

Flytte drivhodet til en valgt sylinder. **Tillat ekstreme sylindere ** tillater normalt begrensede stillinger, og ** Hold motoren aktiv** La motoren kjøre under driften. Bruk ekstreme posisjoner bare når maskinvareprosedyren eksplisitt krever dem.

Normal søk er nyttig for å bekrefte hodebevegelse eller posisjonering før en diagnose. Hør etter unormale gjentatte konsekvenser og stopp hvis den ønskede sylinderen er upassende for stasjonen. Dette verktøyet leser eller validerer ikke data på destinasjonssylinderen.

### Drivjusteringsdiagnostikk

<p align="center"><img src="../images/tool-drive-alignment-en.png" alt="Drivjusteringsdiagnostikk" width="62%"></p>

Kjører gjentatte lesere for drivjusteringsanalyse. Den støtter sporvalg, revolusjon og lesetall, dekodingsformat, råflyt, indeks, hastighet, PLL, tetthetspinn, hard sektor, TG43, og omvendte dataalternativer. Justeringsarbeid krever riktig referansemediene og maskinvarekunnskap.

Begynn med en kjent referansedisk og det minste settet overstyr. **Alternative spor ** definerer sporene og hodene som samples; ** Revolusjoner per spor ** kontrollerer hver prøvevarighet; ** Antall lesninger** bestemme repetisjon. Aktivere en egendefinert diskdefinisjon eller dekodingsformat bare når det samsvarer med referansemediet. Alternativer som falsk indeks, harde sektorer, PLL overstyrer, densitetsstifter, og TG43 er maskinvare- eller formatspesifikk og kan ugyldiggjøre en sammenligning når den brukes feil.

### Hardware pins

<p align="center"><img src="../images/tool-hardware-pins-en.png" alt="Hardware pins" width="62%"></p>

Leser eller endrer en støttet kontrollerpinn. Velg pinne, aktiver **Endre pinne ** Bare når du skriver en verdi og velger ** Høyt nivå** når det kreves av den tiltenkte maskinvareoperasjonen.

Med **Endre pinne** Deaktivert, kommando spør pinnen. Dette er den sikreste standarden. Endring av nivå direkte påvirker kontroller I/O og bør bare gjøres med riktig Greaseweazle maskinvaredokumentasjon og tilhørende ledninger.

### Tilbakestill kontroller

<p align="center"><img src="../images/tool-reset-controller-en.png" alt="Tilbakestill kontroller" width="62%"></p>

Tilbakestiller Greaseweazle kontroller. Bruk dette når kontrolleren oppdages, men ikke lenger reagerer normalt.

Vent til en aktiv diskoperasjon avsluttes før du setter opp igjen. Etterpå skann kontrolleren igjen hvis tilkoblingsstatusen ikke gjenopprettes automatisk. En tilbakestilling reparerer ikke feil `gw.exe` bane eller frakoblet USB Enhet.

### Forsinkelser

<p align="center"><img src="../images/tool-delays-en.png" alt="Kontrollørforsinkelser" width="62%"></p>

Leser eller endrer kontrollerens timingsverdier, inkludert utvalg, hodesteg, bosetting, motor, automatisk avvalg, skrivetid og indeksmaskeforsinkelser. Aktiver bare verdiene du har tenkt å endre.

Usjekkede felt etterlater den tilsvarende kontrollerverdien uendret. Før du redigerer, ta opp eksisterende verdier. Timing endringer kan påvirke hver påfølgende fysisk operasjon, så test med utnyttbare medier og gjenopprette kjente gode verdier hvis oppførselen blir upålitelig.

### Firmware

<p align="center"><img src="../images/tool-firmware-en.png" alt="Firmware oppdatering" width="62%"></p>

Oppdaterer kontrolleren firmware. **Oppdater oppstartslaster** er eksplisitt merket som risikabelt og bør forbli deaktivert med mindre den offisielle fastvareprosedyren krever det. Ikke frakoble kontrolleren under en oppdatering.

Før oppdatering, bekreft den tilkoblede kontrolleren med **Kontrollørinformasjon** Bruk en stabil direkte USB tilkobling, og steng annen programvare som kan få tilgang til den. Etter fullføring, koble til eller skanne kontrolleren på nytt og lese informasjonen igjen for å verifisere den rapporterte firmware-versjonen.

## Logger og operasjonshistorie

Åpne operasjonsloggen for å inspisere lagrede logger etter operasjon.

<p align="center"><img src="../images/operation-history-en.png" alt="Operasjonshistorie" width="68%"></p>

Velg en logg til venstre for å vise innholdet. **Eksporter** sparer en kopi for diagnostikk eller støtte. Stier og kommandolinjer kan inneholde personlige mappenavn, så gjennomgang eksporterte logger før de deler dem.

Live-konsollen i hovedvinduet viser gjeldende kommando og nylig utdata. Dens kopiknapp kopierer den viste teksten.

### Lese en logg

En nyttig diagnostisk logg inneholder den genererte kommandoen, tidsstemplene, motorutgangen og den endelige statusen. Arbeid fra bunnen oppover: identifisere den endelige feilen, deretter finne den første advarselen eller feil spor som var foran den. En senere generisk feil er ofte bare konsekvensen av en tidligere, mer spesifikk melding.

Når du sammenligner to forsøk, sjekk at kontrolleren, stasjonen, motoren, profilen, kildestien, utgangsformatet og ekspertargumentene var identiske. Ellers kan et annet resultat gjenspeile endret innstillinger i stedet for diskustabilitet.

## Søknadsdata og bærbar bruk

GW GUI holde brukerdata adskilt fra applikasjonsbinarer. Avhengig av den valgte pakken og modusen lagres innstillinger, logger, nedlastede verktøy, emulatorkomponenter, opptak, tilstander og maskinkonfigurasjoner enten i programmet `Data` katalog eller på konfigurerte brukerdatasteder.

Før du erstatter eller flytter en bærbar installasjon, hold hele programmappen sammen og sikkerhetskopierer `Data` mappe. Ikke flytt individuelle filer fra `lib`, fordi programmet løser sine egne og tredjeparts biblioteker fra den strukturen.

### Foreslått sikkerhetskopieringsinnhold

Sikkerhetskopiere følgende når de er viktige for arbeidsflyten din:

- applikasjonsinnstillinger og profiler;
- kontroller og driver definisjoner;
- emuleringskonfigurasjoner;
- ROM Veier og lovlige ROM sikkerhetskopier;
- harddisk og flyttbare mediebilder;
- fanger og lagret stater;
- operasjonslogger som brukes som bevaringsregistre.

Diskbilder kan være mye større enn innstillinger. Oppbevar arkivmestere kun når det er mulig, og arbeid på kopier.

## Anbefalte arbeidsflyter

### Arkivere en ukjent disk

1. Inspeksjon og rengjøring av stasjonen ved hjelp av en passende vedlikeholdsprosedyre.
2. Skrivebeskytt disken om mulig.
3. Velg **Les > Råbilde (SCP)**.
4. Bruk et beskrivende filnamn og les det normale sporområdet med flere revolusjoner.
5. Se på konsollen og lagret logg.
6. Sjekk begge sider i **Visualisering**.
7. Konverter en kopi til sannsynlige sektorformater.
8. Test de konverterte kopiene i **Disk Explorer** eller egnet programvare.
9. Bevar råmesteren, loggen og notatene sammen.

### Rekreasjon av en disk fra et bilde

1. Sjekk bildet og bekreft den forventede familien og format.
2. Sett inn en utnyttbar eller med vilje skrivbar disk av riktig størrelse og tetthet.
3. Åpne **Skriv** Velg bildet.
4. Bekreft konfigurert enhet og detektert format.
5. Skriv disk.
6. Les det tilbake til et separat verifikasjonsbilde.
7. Sammenlign avkodet innhold og se mistenkelige spor visuelt.

### Opprette en emulert Amiga

1. Åpne **Alternativer > Emulering > Konfigurasjoner** Opprett eller velg en maskin.
2. I **Amiga > Generelt** Velg modell og emulator versjon.
3. Tilordne en kompatibel, lovlig oppnådd ROM.
4. Behold standardmodellen for CPU og RAM På den første boot.
5. Konfigurer video og lyd med konservative automatiske innstillinger.
6. Legg til lagringsenheter og tilknytte kopierte mediebilder.
7. Gjennomgang tastatur, mus og kontroller oppgaver.
8. Lagre konfigurasjonen.
9. Tilbake til **Emulering ** Velg det, og klikk **Åpne**.
10. Først etter en vellykket baseline oppstart, endre akselerasjon eller avanserte innstillinger én om gangen.

## Sikkerhetskontrollliste

Før **Les**:

- kildedisken er i riktig drift;
- kilden er der det er mulig,
- utgangsstien vil ikke overskrive en eksisterende master;
- Profilen og sporområdet matcher disken.

Før **Skriv ** eller ** Slett**:

- Destinasjonsdisken kan ødelegges,
- Bildet og stasjonen er riktig;
- diskstørrelse og tetthet er kompatible;
- Ingen arkivmester blir brukt som destinasjon.

Før et maskinvareendringsverktøy:

- Ingen annen operasjon kjører;
- den riktige kontrolleren er valgt;
- Nåværende verdier er registrert;
- kontrolleren har stabil makt og USB tilkobling;
- Handlingen støttes av maskinvaredokumentasjonen.

## Feilsøking

### Kontrolløren er ikke oppført

1. Koble kontrolleren direkte til datamaskinen.
2. Åpne **Alternativer > Styrere og stasjoner**.
3. Klikk **Skann**.
4. Kontroller kontrollerstatus og stasjonskonfigurasjon.
5. Kjør **Kontrollørinformasjon** Hvis deteksjonen lykkes, men kommandoene mislykkes.

Hvis det fortsatt ikke vises, prøv en annen direkte USB Port og kabel, deretter skanne. Sjekk Windows Device Manager for en nyoppdaget serieenhet. En kontroller synlig for Windows, men fraværende GW GUI vanligvis peker på en travel port, stange konfigurasjon eller Host Tools problem; en kontroller fraværende fra Windows poeng til USB, strøm, driver eller maskinvare.

### `gw.exe` Finnes ikke

Åpne **Alternativer > Styrere og stasjoner ** Bruk deretter **Finn gw.exe **, ** Velg **, eller ** Last ned siste versjon**. Bekreft at den detekterte banen peker til den tiltenkte Greaseweazle installasjon.

Etter å ha valgt den, løp **Kontrollørinformasjon** Hvis det mislykkes før du kontakter maskinvare, inspisere loggen for en ugyldig kjørbar sti, manglende filer eller en versjon som ikke kan starte.

### En operasjon bruker feil motor

Åpne **Alternativer > Motorer** Sjekk motoren som er tildelt den nøyaktige driften. GW GUI Faller ikke stille tilbake til den andre motoren.

Motorinnstillingene er separate: å endre konverteringsmotoren endrer ikke lesing, skriving eller Disk Explorer. Åpne feiloperasjonen etter å ha lagret alternativet og bekrefte den genererte kommandoen i konsollen.

### Et bilde gjenkjennes ikke

Deaktiver automatisk deteksjon bare hvis du vet riktig maskin og format. Ellers prøver **Visualisering** fanen for å inspisere bildet på et lavere nivå.

Sjekk om kilden er en rå flux-opptak, et sektorbilde, en komprimert beholder eller en ikke-relatert fil med en vildledende utvidelse. Aldri omdøbe en forlengelse bare for å tvinge deteksjon; konvertering må tolke kildestrukturen riktig.

### Emuleringen starter ikke

Bekreft den lagrede konfigurasjonen, installert emulatorversjon, valgt ROM, lagringsstier og modellkompatibilitet. Les programloggen for fullstendige feildetaljer.

Midlertidig tilbake CPU, RAM, video og lagring til en enkel modellkompatibel baseline. Hvis grunnlinjen starter, gjenopprette en egendefinert innstilling om gangen. En lagret tilstand opprettet med en annen emulatorversjon eller maskindefinisjon kan også mislykkes selv når en ren oppstart fungerer.

### En snarvei eller inndata virker ikke

Sjekk både det globale **Emulering > Snarveier** siden og den maskinspesifikke tastatur-, mus- eller kontrollsiden. Løs alle oppgaver merket som motstridende.

Hvis musen er tatt opp, bruk utgivelsessnarveien som vises i kjøringsmaskinverktøylinjen. Hvis en kontroller ble koblet til etter at innstillingene ble åpnet, kjører du kontrollerdeteksjonen igjen før den tilordnes.

### En kommando mislykkes uventet

1. Les live konsollutgangen.
2. Åpne **Operasjonshistorie** for den fullstendig lagrede loggen.
3. Bekreft den valgte kontrolleren, stasjonen, profilen, motoren og filstiene.
4. Eksporter den relevante loggen hvis den må deles for diagnose.

### Lydsprekker eller pauser

Øk emulering lyd latens, lukk CPU-intensive applikasjoner, og returnere videoramme som hopper over og akselerasjon til sine tidligere verdier. Kontroller at den tiltenkte Windows-lydenheten er valgt. Endre én innstilling om gangen slik at den effektive rettelsen er identifiserbar.

### Emuleringsskjermen er tom eller langsom

Return oppløsnings- og linjemodus til **Automatisk**, deaktivere ramme hopping og flimmer fikse midlertidig, og prøv den tidligere fungerende render. Bekreft at konfigureringen ROM og innsatt oppstartsmedier er gyldige. Den FPS indikator bidrar til å skille et gjengivelsesproblem fra en maskin som ikke har startet.

### Lese inneholder ustabile spor

Gjenta lesningen til et nytt filnamn, øke revolusjoner der det er nødvendig, og sammenligne de berørte sporene. Rengjør drivhodene ved hjelp av en riktig prosedyre og inspisere disken for fysisk skade. Ikke les gjentatte ganger synlig shedding eller skadet media, fordi ytterligere passeringer kan forverre det.

## Ordliste

| Term | Betydning i GW GUI |
|---|---|
| Kontrollør | Den Greaseweazle maskinvaregrensesnitt tilkoblet over USB |
| Drive | Den fysiske diskettstasjonen som er festet til kontrolleren |
| Motor | Implementasjonen valgt for å utføre en operasjon |
| Flux | Timing informasjon som representerer magnetiske overganger leses fra en disk |
| Råbilde | En fangst som beholder diskinformasjon på lavt nivå, som SCP |
| Sektorbilde | En dekodet representasjon organisert i logiske sektorer |
| Revolusjon | En komplett rotasjon prøvet mens du leser et spor |
| Sylinder | En radial hodeposisjon; en sylinder kan inneholde et spor på hver side |
| Hoved | Disksiden valgt av den fysiske stasjonen |
| Profil | Et gjenbrukbart sett innstillinger for en operasjon |
| ROM | Firmware-bilde som kreves av en emulert maskin |
| Reddet tilstand | Et øyeblikksbilde av en kjørende emulators maskintilstand |
| Render | Grafisk motor som brukes til å vise emuleringsutdata |

## Rask referanse

| Hvis du vil... | Gå til... |
|---|---|
| Bevar en fysisk disk | **Les** |
| Legg et bilde tilbake på en disk | **Skriv** |
| Produsere et annet bildeformat | **Konvertering** |
| Inspeksjon av spor eller fluxavvik | **Visualisering** |
| Bla gjennom filer inne i et bilde | **Disk Explorer** |
| Sjekk kontrollerkommunikasjon | **Verktøy > Kontrollørinformasjon** |
| Mål drivrotasjon | **Verktøy > Drivhastighet** |
| Se en tidligere kommando | **Operasjonshistorie** |
| Konfigurer maskinvare | **Alternativer > Styrere og stasjoner** |
| Velg implementeringer | **Alternativer > Motorer** |
| Opprett eller rediger en emulert maskin | **Alternativer > Emulering** |
| Start en lagret maskin | **Emulering** |
