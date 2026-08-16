# GW GUI Brugervejledning

GW GUI er et Windows-program til at læse, skrive, konvertere, inspicere og efterligne floppy- disk billeder. Det kan styre Greaseweazle hardware, arbejde med diskbilledfiler gennem sin interne motor, og køre gemt emuleret maskine konfigurationer.

Denne vejledning beskriver den engelske grænseflade, der vises i den aktuelle version af programmet. Det er skrevet som kilden til den printbare brugermanual: screenshots illustrerer kontrollerne, mens den omgivende tekst forklarer, hvad man skal vælge, hvorfor man skal vælge det, og hvordan man kontrollerer resultatet.

> **Vigtigt:** At læse en disk er ikke destruktivt. Skrivning, sletning, firmware opdatering, og nogle hardware værktøjer kan ændre medier eller hardware. Læs advarslen vedhæftet til den relevante procedure, før du klikker ** Kør**.

### Hvordan denne vejledning anvendes

Hvis dette er din første gang ved hjælp af GW GUI, komplet [Kom i gang ](#getting-started), så følg [Læsning af en disk ](#reading-a-disk). Hvis programmet allerede er konfigureret, gå direkte til kapitlet for den operation, du ønsker at udføre. De muligheder kapitler tjener som reference, når en procedure beder dig om at ændre et drev, motor, profil, eller emuleret-maskine indstilling.

Interface navne vises i **fed**. Filnavne, stier, kommandoer og bogstavelige værdier vises som `code`. Noter forklarer normal adfærd; advarsler identificerer operationer, der kan ændre en disk, controller eller lagret konfiguration.

## Indhold

1. [Forståelse af arbejdsgangen ](#understanding-the-workflow)
2. [Kom i gang ](#getting-started)
3. [Hovedvinduet ](#main-window)
4. [Læsning af en disk ](#reading-a-disk)
5. [Skrivning af en disk ](#writing-a-disk)
6. [Konverterer disk billeder ](#converting-disk-images)
7. [Visualizing a disk image ](#visualizing-a-disk-image)
8. [Udforsker diskindhold ](#exploring-disk-contents)
9. [Brug af værktøjerne ](#using-the-tools)
10. [Emulerings](#emulation)
11. [Ansøgningsmuligheder ](#application-options)
12. [Emulationsmuligheder ](#emulation-options)
13. [Amiga konfiguration ](#amiga-configuration)
14. [Hardware diagnostik og vedligeholdelse ](#hardware-diagnostics-and-maintenance)
15. [Logs og driftshistorik ](#logs-and-operation-history)
16. [Ansøgningsdata og bærbart brug ](#application-data-and-portable-use)
17. [Anbefalede arbejdsgange ](#recommended-workflows)
18. [Sikkerhedscheckliste ](#safety-checklist)
19. [Fejlfinding ](#troubleshooting)
20. [Ordliste ](#glossary)
21. [Hurtig reference ](#quick-reference)

## Forståelse af arbejdsgangen

GW GUI adskiller fysisk-disk operationer fra image- file operationer:

| Målsætning | Input | Output | Anbefalet side |
|---|---|---|---|
| Bevar en diskette | Fysisk disk | Billedfil | **Læs** |
| Genopret en diskette | Billedfil | Fysisk disk | **Skriv** |
| Ændr billedformat | Billedfil | En eller flere billedfiler | **Konvertering** |
| Undersøg spor og anomalier | Billedfil | Visuel analyse | **Visualisering** |
| Gennemse filer gemt i et billede | Understøttede billede / filsystem | Filer og mapper | **Disk Explorer** |
| Diagnose en drev eller controller | Greaseweazle hardware | Målinger eller status | **Værktøjer** |
| Kør en gemt virtuel maskine | Gemt maskinkonfiguration | Emulationssession | **Emulering** |

For konservering, først gøre en rå fangst og holde det uændret som en mester. Opret konverterede eller reparerede arbejdskopier fra denne master. Dette undgår at gentage en fysisk læse og bevarer oplysninger, som en sektor-baseret format kan ikke bevare.

## Kom i gang

### Krav

- Windows med Microsoft .NET Desktop Runtime kræves af programmet.
- En Greaseweazle controller til fysiske floppy- disk operationer.
- En konfigureret sti til `gw.exe`, når du bruger Greaseweazle Host Tools-motoren.
- Juridisk opnåede ROM filer, når en emuleret maskine kræver dem.

Ansøgningen kontrollerer sin krævede .NET runtime ved opstart. Hvis det mangler, skal du følge installationsprompten og derefter genstarte GW GUI.

### Før tilslutning af hardware

Kontroller følgende, før du kører en fysisk-disk operation:

1. Tilslut Greaseweazle controlleren til en stabil USB port.
2. Tilslut disketten med den rigtige retning.
3. Tilslut drevet strømforsyning, før du indsætter værdifulde medier.
4. Bekræft, at drevet størrelse og tæthed matcher disken.
5. Skriv-beskytte kildedisken, når det er muligt.

GW GUI kan ikke forhindre skader forårsaget af ukorrekt kabelføring, uhensigtsmæssig kraft eller en mekanisk usikker drev. Test ukendt hardware med en mulig disk først.

### Første lancering

1. Åbn `gwgui.exe`.
2. Åbn **muligheder**.
3. I **Controllers og drev**, scanne for controlleren og konfigurere drevet.
4. Verificer eller vælg stien til `gw.exe`.
5. I **Motorer**, vælge hvilken motor skal udføre hver operation.
6. Vend tilbage til hovedvinduet og vælg den ønskede operation fanen.

### Bekræfter at opsætningen er klar

En arbejdsopsætning skal vise controlleren og køre i statuslinjen, for eksempel et drev nummer, størrelse, tæthed, og COM port. I **muligheder > Kontrollører og drev **, skal controlleren være mærket ** Tilgængelig ** og drevet ** Confied **. Kør ** Controller information** før du læser værdifulde medier, hvis du ønsker at kontrollere kommunikation uden at ændre en disk.

### Valg af motor

GW GUI kan afsløre mere end én implementering for nogle operationer. **Greaseweazle Host Tools** motoren påkalder den konfigurerede `gw.exe`; den interne GW GUI motor håndterer understøttede operationer inde i programmet. Motorvalg er eksplicit og uafhængig til læsning, skrivning, konvertering og Disk Explorer. Hvis en operation ikke understøttes af den valgte motor, indberetter GW GUI denne tilstand i stedet for automatisk at skifte motor.

## Hovedvindue

Hovedvinduet grupperer de vigtigste operationer i syv faneblade:

- **Læs** skaber et billede fra en fysisk disk.
- **Skriv** skriver et billede til en fysisk disk.
- **Konvertering** konverterer en disk- billedformat til en eller flere outputformater.
- **Visualisering** viser spor og flux eller dekodede data.
- **Disk Explorer** gennemser understøttede filsystemer og diskindhold.
- **Tools** giver hardware vedligeholdelse og diagnostiske kommandoer.
- **Emulation** administrerer og kører gemte emulerede maskiner.

Konsollen nederst viser kommandoen der udføres og dens output. Statuslinjen rapporterer det valgte drev, profil og nuværende tilstand.

### Læsning af grænsefladen

De fleste driftssider følger samme mønster:

1. **Kilde eller destination** kontrol identificere disk, billede, eller mappe.
2. **Formatstyring** vælge automatisk detektering eller en eksplicit maskine og format.
3. **Profilkontrol** anvender genanvendelige indstillinger.
4. **Avancerede indstillinger** afsløre parametre, der normalt er valgfrie.
5. **Execute** starter operationen.
6. **konsollen** viser den genererede kommando, fremskridt, advarsler og fejl.

**Execute** knappen betyder ikke, at alle værdier er sikre for den indsatte disk. Altid gennemgå destinationen og valgte drev før en skrive eller vedligeholdelse operation.

### Statuslinje og konsol

Venstre side af statuslinjen identificerer det aktive fysiske drev. Centret viser den aktive profil, når man vælger. Den statslige indikator rapporterer, om programmet er klar eller optaget. Konsollen er ikke blot diagnostisk: det er autoritativ registrering af kommandoen sendt til den valgte motor. Brug sin kopikontrol, når du har brug for at bevare eller dele denne kommando.

## Læsning af en disk

Åbn **Læs** fanen for at fange en fysisk diskette som et billede.

<p align="center"><img src="images/main-read-en.png" alt="Læs faneblad" width="78%"></p>

### Grundlæggende procedure

1. Indsæt kildedisken i det indstillede drev.
2. Vælg billedtype:
   - **Rå billede (SCP)** bevarer oplysninger på fluxniveau.
   - **Kendte disk format** skaber et billede ved hjælp af en valgt maskine og format.
3. Vælg destinationsmappen.
4. Indtast output- filnavnet.
5. Vælg om nødvendigt en profil.
6. Klik på **Execute**.

Konsollen viser præcis kommando og fremskridt. Fjern ikke disken eller frakoble controlleren, før operationen er afsluttet.

### Valg af outputtype

Brug **Rå billede (SCP)**, når målet er arkivoptagelse, analyse, nyttiggørelse eller senere konvertering. Et råt billede registrerer timing oplysninger og flere revolutioner, som er nyttige for usædvanlige formater, svage sektorer, beskyttelsesordninger, og beskadigede medier.

Brug **Kendte disk format**, når du allerede kender disk familie og har brug for en direkte anvendelig sektor billede. Dette valg kan være mindre og lettere at åbne i andre software, men det repræsenterer dekodet resultat snarere end alle detaljer observeret af drevet.

Når usikker, skal du oprette den rå billede først. Du kan konvertere det senere uden at læse disken igen.

### Mappe, filnavn og profil

**Mappe ** er destinationsmappen. ** Filnavn** bør identificere disken uden kun at stole på dens fysiske etiket. Et nyttigt arkivnavn indeholder titel, disknummer eller side, og en betingelse note, når det er relevant. Må ikke tilføje et format udvidelse, der strider med det valgte outputformat.

En **Profil ** anvender et gemt sæt læseparametre. Vælg kun én når du ved hvad den indeholder. ** Standard** profilen er velegnet til et normalt første forsøg; en specialiseret recovery profil kan bevidst læse flere revolutioner eller et andet sporområde og derfor tage længere tid.

### Avancerede indstillinger

Udvid **Avancerede indstillinger** at få adgang til formatspecifikke eller ekspert parametre. Efterlad disse værdier uændret, medmindre disken kræver et bestemt sporområde, revolutionstæller eller controller.

Fælles avancerede værdier omfatter:

| Indstilling | Formål | Hvornår du skal ændre det |
|---|---|---|
| Sporvidde | Begrænser flaskerne og hovederne til at læse | Enkeltsidede medier, usædvanlig geometri, eller en målrettet opsving pass |
| Revolutioner | Kontrollerer hvor mange rotationer der udtages prøver af | Forøg for ustabile eller beskyttede spor; reducer kun for hastighed, når det er relevant |
| Ekspertargumenter | Passerer yderligere motorparametre | Kun når du følger dokumenteret Greaseweazle vejledning |

### Kontrol af en vellykket læsning

Stol ikke kun på fraværet af en fejldialog. Efter at kommandoen er fuldført:

1. Bekræft at uddatafilen eksisterer og ikke er tom.
2. Læs de endelige konsollinjer for mislykkede eller manglende spor.
3. Åbn billedet i **Visualisering** for at kontrollere, at begge sider og det forventede sporområde indeholder data.
4. Åbn den i **Disk Explorer**, når filsystemet er understøttet.
5. Hold operationsloggen med vigtige arkivoptagelser.

Hvis gentagne læsning er forskellige, bevare hver rå fangst snarere end at overskrive den første. Forskelle kan være nyttige under inddrivelse.

## Skrivning af en disk

Åbn **Skriv** fanebladet for at skrive et eksisterende billede til en fysisk diskette.

<p align="center"><img src="images/main-write-en.png" alt="Skriv faneblad" width="78%"></p>

### Grundlæggende procedure

1. Indsæt destinationsdisken.
2. Vælg kildebilledet med **Gennemse**.
3. Bekræft det fundne format.
4. Vælg om nødvendigt en profil.
5. Klik på **Execute**.

Skrivning erstatter data på destinationsdisken. Verificer det valgte drev og billede før start.

> **Advarsel:** Skrivning er destruktiv. Det erstatter magnetiske data på destinationsdisken. Brug et skrivebeskyttet kildearkiv og en separat destinationsdisk når det er muligt.

### Før du skriver

Tjek fire elementer før du klikker **Kør**:

1. **Billede:** den valgte sti er den tiltænkte kildebillede.
2. **Disk:** disken i drevet kan sikkert overskrives.
3. **Drive:** den konfigurerede størrelse og tæthed passer til destinationsmediet.
4. **Format:** automatisk detektering eller det manuelt valgte format matcher billedet.

Hvis kildebilledet ikke er blevet testet, åbnes det i **Visualisering ** eller ** Disk Explorer** først. En vellykket skrive kan ikke reparere en ufuldstændig kilde billede.

### Sporinspektion og -ændring

Når et billede er valgt, åbner **Visualize spor ** sin sporrepræsentation. ** Ændr** udsætter de understøttede billedændringer før skrivning. Tilgængelige handlinger afhænger af det valgte format og motor.

### Verifikation af en skriftlig disk

Når motoren understøtter verifikation, skal du bruge den til vigtige medier. Ellers læse den skrevne disk tilbage til et nyt billede og sammenligne dens dekodede indhold eller inspicere det i **Visualisering**. Hold verifikationsoptagelsen adskilt fra det oprindelige billede, så originalen aldrig overskrives.

Hvis skrivning mislykkes på konsekvent spor, kontrollere disk tilstand, tæthed, drev renlighed, og drev konfiguration. Hvis svigt opstår tilfældigt, skal du kontrollere USB stabilitet og controller kommunikation.

## Konverterer diskaftryk

**Konvertering** fanen konverterer en kilde billede til en eller flere destination formater.

<p align="center"><img src="images/main-conversion-en.png" alt="Konverteringsfaneblad" width="78%"></p>

### Grundlæggende procedure

1. Vælg kildebilledet.
2. Angiv outputnavne.
3. Vælg en maskinfamilie.
4. Vælg en eller flere outputformater og udvidelser.
5. Aktivér **Tilføj tags** hvis filnavne skal bruge det indstillede tagmønster.
6. Klik på **Execute**.

**Udvalgte ** panel viser de ønskede udgange. ** File migration** giver dedikeret workflow til migrering understøttede filer i stedet for at udføre en standard billedkonvertering.

### Valg af formater

**Machine ** listen filtrerer de formater, der vises i ** Format** panel. Et formatnavn beskriver det logiske disklayout; forlængelsen beskriver udgangsbeholderen. Nogle formater kan repræsenteres af mere end én udvidelse, og nogle beholdere kan ikke bevare alle funktioner i en rå kilde.

Vælg kun udgange du faktisk har brug for. Flere formater er nyttige, når du opretter en arkivmaster, en emulator- kompatibel kopi, og en kopi til en anden analyse værktøj i én operation.

### Uddata navngivning og tags

**Output navne ** kan du styre grundnavne genereret til valgte formater. ** Tilføj tags ** anvender filnavnet mønster konfigureret i ** muligheder > General**. Tags kan indkode familie, format, udvidelse, dato, eller tid. Vis eksemplet i Indstillinger, før du konverterer en stor batch, så filerne er navngivet konsekvent.

### Kontrol af konverteringsresultater

For hver ønsket output:

1. Bekræft at en fil blev oprettet.
2. Tjek konsollen for spor eller sektorer, der ikke kunne afkodes.
3. Åbn resultatet i **Disk Explorer**, hvis det indeholder et understøttet filsystem.
4. Sammenlign den forventede diskkapacitet og indhold med kilden.

En konvertering kan fuldføre, mens rapportering af tab af oplysninger, der er iboende til destinationsformatet. Behold det oprindelige rå billede, selv når det konverterede billede synes korrekt.

## Visualisering af et disk- billede

Fanebladet **Visualisering** viser strukturen og datafordelingen af et billede.

<p align="center"><img src="images/main-visualization-en.png" alt="Fanebladet Visualisering" width="78%"></p>

1. Klik **Åbn et disk billede**.
2. Behold **Automatisk detektering** aktiveret, eller vælg maskinen og format manuelt.
3. Brug **Link zoom** at holde begge sider på samme zoom niveau.
4. Brug **Nulstil** til at gendanne den oprindelige visning.
5. Åbn **Inspektør** for detaljeret information om den valgte region.

Legenden adskiller normal flux, korte og lange overgange, headers, dekodede data, og opdagede anomalier. Et råt billede kan indeholde data, der ikke kan afkodes til et kendt filsystem, men stadig kan inspiceres her.

### Fortolkning af synspunktet

Hvert stort cirkulært panel repræsenterer en disk side. Centret identificerer siden og dens nuværende datatilstand; koncentriske positioner svarer til spor. Farver klassificere de fundne regioner ifølge legenden. Visualizer er beregnet til at besvare spørgsmål såsom:

- Indeholder billedet data på den ene eller begge sider?
- Er de forventede spor til stede?
- Er anomalier isoleret eller gentaget på tværs af disken?
- Identificerede automatisk detektering en plausibel maskine og format?

En anomali farve er en grund til at inspicere regionen, ikke bevis for, at disken er ubrugelig. Kopier beskyttelse, ikke-standard formatering, en svag optagelse, og en beskadiget sektor kan producere forskellige strukturer, der kræver kontekstuel fortolkning.

### Anbefalet inspektionssekvens

Start med forbundet zoom aktiveret til at sammenligne begge sider på samme skala. Vælg en mistænkelig region, åben **Inspektør**, og sammenligne det med nærliggende spor. Hvis resultatet synes at være et detektionsproblem, deaktivere automatisk detektering og vælge en kendt maskine og format. Vend tilbage til automatisk detektering efter prøvningen, så en tvungen indstilling ikke ved et uheld bruges til et andet billede.

## Udforskning af diskindhold

**Disk Explorer** fanen gennemser understøttede disk-billeder som et filhierarki.

<p align="center"><img src="images/main-disk-explorer-en.png" alt="Fanebladet Disk Explorer" width="78%"></p>

1. Åbn et eksisterende billede eller læs en disk.
2. Behold **Automatisk detektering** aktiveret, medmindre du har brug for at tvinge en maskine eller format.
3. Gennemgå volumen information: system, beskyttelse, filsystem, kapacitet, ledig plads, og post tæller.
4. Gennemse mapper i venstre panel.
5. Vælg et element for at se dets detaljer i højre panel.

Hvis billedformatet eller filsystemet ikke understøttes, skal du bruge **Visualisering** til at inspicere den rå struktur i stedet.

### Forståelse af panelerne

Den øverste oversigt beskriver det monterede billede og detekteret volumen. Den nederste venstre panel indeholder mappen hierarki. Den centrale tabel viser punkter i den valgte mappe med navn, ændringsdato, type og størrelse. Det rigtige panel viser detaljer for det valgte punkt.

Disk Explorer betyder ikke, at hver rå spor blev afkodet perfekt. Brug volumen resumé og element tæller som en hurtig sandsynlighedskontrol, derefter åbne repræsentative filer eller sammenligne dem med en kendt mappe liste, når bevarelse nøjagtighed spørgsmål.

### Når intet dukker op

Bekræft først, at billedstien er korrekt. Tjek derefter den detekterede maskine og format. Et gyldigt billede kan indeholde et uunderstøttet eller beskadiget filsystem, i hvilket tilfælde opdageren kan forblive tom, selvom **Visualisering** viser registrerede data. Du må ikke overskrive eller kassere kildebilledet udelukkende baseret på en tom opdagelsesrejsende.

## Brug af værktøjerne

**Værktøjer** fanen grupper Greaseweazle vedligeholdelse operationer.

<p align="center"><img src="images/main-tools-en.png" alt="Værktøjstabel" width="78%"></p>

Vælg en kommando fra listen til venstre, gennemse dens parametre, og klik derefter på **Kør**. Destruktive eller hardware- skiftende kommandoer bør kun bruges efter kontrol af den valgte controller og drev.

De fleste værktøjsdialoger indeholder tre områder: parametre øverst, et status- og raw- outputområde i midten og den genererede kommando nederst. Kommandoen forhåndsvisning ændringer som tilvalg er aktiveret. En umarkeret parameter betyder normalt "ændrer ikke denne værdi", mens en markeret parameter inkluderer denne værdi i kommandoen.

De individuelle diagnostiske dialoger er beskrevet i [Hardware diagnostik og vedligeholdelse ](#hardware-diagnostics-and-maintenance).

## Emulering

### Åbning af en gemt maskine

**Emulation ** fanen lister gemte konfigurationer. Vælg en og klik ** Åbn**. Hver kørende maskine vises i sin egen fane.

<p align="center"><img src="images/main-emulation-welcome-en.png" alt="Emulationsvelkomstskærm" width="78%"></p>

Opret og rediger maskiner i **muligheder > Emulation > Konfigurationer ** og ** tilvalg > emulering > Amiga**.

Hvis ingen konfiguration vises, skal du oprette en i Indstillinger først. En gemt konfiguration kombinerer maskine model, emulator version, ROM, hukommelse, video, lyd, opbevaring og input tilknytninger. Lagring af en konfiguration ikke starte det; vende tilbage til de vigtigste **Emulation ** fanen og klik ** Open**.

### Styring af køremaskine

<p align="center"><img src="images/main-emulation-running-en.png" alt="Kører emuleret maskine" width="78%"></p>

Værktøjslinjen for running- machine giver power, pause, nulstilling, save- state, load- state, capture og display-styring. Det viser også:

- de konfigurerede genveje til hurtig lagring og hurtig lastning
- den aktive formidler, såsom Direct3D 11
- genveje med fuld skærm og muse-release
- audio, controller og mus tilstand;
- den aktuelle opløsning, opdateringshastighed og rammehastighed.

Disketten i bunden af emuleringsdisplayet håndterer flytbare medier for hvert emuleret drev. Tastaturopgaver kan ændres i **tilvalg > Emulation > Genveje**, mens emulerede tastatur, mus og controller tilknytninger er konfigureret i de tilsvarende Amiga faner.

### Reference for værktøjslinje

| Kontrolgruppe | Formål |
|---|---|
| Effekt og pause | Starter, stopper, pauser eller genoptager den emulerede maskine |
| Nulstil kontrol | Udfører den konfigurerede soft or hard reset handling |
| Statslig kontrol | Gemmer eller lader en emulator tilstand for hurtig fortsættelse |
| Fangst | Gemmer et billede af det emulerede display |
| Vis | Ændrer visningspræsentationen eller går ind i fuldskærm |
| Hurtig påmindelse | Viser de aktive spar / load genveje |
| Renderer | Rapporterer den aktive videomotor |
| Input- påmindelse | Viser genveje med fuld skærm og udgivelsesbilleder |
| Enhedsindikatorer | Rapporter lyd, controller og muse tilstand |
| Resultater | Rapporter output størrelse, opdater frekvens, og frame rate |

### Forlader fuld skærm eller frigive musen

Værktøjslinjen viser de aktuelt tildelte nøgler. I den illustrerede konfiguration, **Alt + Return ** skifter til fuld skærm og ** F12** frigiver musen. Behandl de viste værdier som autoritative, fordi genveje kan overføres.

### Brug af diskette medier

Drev striben identificerer hver emuleret drev, såsom `DF0:`. Brug mediekontrollen til at indsætte, erstatte eller skubbe et billede ud. Udskiftning af medier ændrer kun den kørende maskines indsatte disk; det ændrer ikke definitionen af storage- enhed i den gemte maskine, medmindre denne handling er eksplicit gemt.

## Programindstillinger

Åbn **Indstillinger** fra hovedvinduet for at indstille programmet.

### Generelt

<p align="center"><img src="images/options-general-en.png" alt="Generelle valgmuligheder" width="72%"></p>

Fanebladet **General** indeholder:

- standarddisk- billedmappen;
- grænsefladesprog og -tema
- filename- mærkegenerering til konverteringer
- foruddefinerede og seneste brugerdefinerede mærkemønstre
- et levende filnavn eksempel.

Mærkevariabler omfatter kildenavn, familie, format, udvidelse, dato og tid. Brug nulstil knappen til at gendanne standardmønstret.

Filnavnet forhåndsvisning opdateringer, før nogen filer er oprettet. Brug det til at opdage duplikerede separatorer, manglende udvidelser, eller tvetydige navne. Nylige brugerdefinerede mønstre giver hurtig adgang til tidligere navngivning ordninger uden at erstatte den nuværende forudindstillet.

### Logge

<p align="center"><img src="images/options-logs-en.png" alt="Logtilvalg" width="72%"></p>

Logning kan konfigureres uafhængigt for hver operation. For hver kategori, skal du vælge, om du skal gemme logfiler, indstille en maksimal filstørrelse, og beslutte, om tidligere logfiler skal opbevares. En størrelse `0` betyder ubegrænset. **Åbn mappe** åbner den aktuelle logmappe.

Aktivér **Hold tidligere logs** for bevarelse og diagnostisk arbejde, hvor historien om flere forsøg betyder noget. Deaktivér den når kun det seneste resultat er nyttigt. Maksimale størrelsesgrænser gælder for loglagring, ikke for optagne disk-billeder.

### Styremaskiner og -apparater

<p align="center"><img src="images/options-controllers-and-drives-en.png" alt="Styremaskiner og -apparater" width="72%"></p>

Brug dette faneblad til:

- scanning af tilsluttede controllere
- tilføje og fjerne drevkonfigurationer
- vælge drev størrelse, tæthed og hastighed
- gemme hardwareindstillinger
- vælge eller automatisk finde `gw.exe`
- kontrollere og downloade Greaseweazle Host Tools-opdateringer
- gendanne en tidligere konfigureret eksekverbar sti.

Gemte hardwareindstillinger forbliver tilgængelige, når et drev afbrydes midlertidigt.

#### Tilføjelse af et drev

1. Klik på **Scan** og vent på at tilsluttede controllere vises.
2. Klik **Tilføj et drev**, hvis det nødvendige drev ikke allerede er angivet.
3. Vælg dens logiske drev nummer, fysisk størrelse, optagelsestæthed, og rotationshastighed.
4. Gem rækken.
5. Bekræft, at det viser **Tilgængelig ** og ** Confied**.

Brug kun affaldsstyringen til at fjerne den gemte konfiguration; den afbryder ikke hardware. Hvis den samme controller vises på en anden COM port senere, scanne igen, før du antager, at den gemte port er stadig gyldig.

#### Forvaltning af Greaseweazle Host Tools

**Find gw.exe ** søger kendte steder. ** Vælg ** vælger en bestemt eksekverbar. ** Tjek for opdateringer ** forespørgsler tilgængelige versioner uden at erstatte den installerede. ** Download nyeste version ** installerer den valgte nuværende pakke, og ** Brug tidligere sti ** genopretter den tidligere konfigurerede placering. Efter ændring af den eksekverbare, køre ** Controller oplysninger** at bekræfte, at den valgte version kan kommunikere med controlleren.

### Motorer

<p align="center"><img src="images/options-engines-en.png" alt="Motorvalg" width="72%"></p>

Vælg motoren uafhængigt til læsning, skrivning, konvertering og Disk Explorer. Den valgte motor anvendes strengt: Hvis den ikke kan udføre den ønskede operation, indberetter GW GUI begrænsningen i stedet for stille at skifte motor.

Denne uafhængighed er bevidst. For eksempel kan fysiske læsning bruge Greaseweazle Host Tools, mens billede konvertering og efterforskning bruge den interne motor. Registrer motorvalg i en profil eller projektnote, når reproducerbarhed er vigtig.

### Profiler

<p align="center"><img src="images/options-profiles-en.png" alt="Profiler" width="72%"></p>

Profiler gemme genanvendelige indstillinger til læse, skrive og konvertering operationer. Vælg den relevante kategori til at administrere sine profiler. En valgt profil vises i statuslinjen for hovedvinduet og i arbejdsskærme.

Brug profiler til gentagelige arbejdsgange i stedet for som uforklarlige samlinger af ekspertflag. Giv hver profil et formål-specifikt navn, såsom et bestemt drev, disk familie, eller recovery metode. Gennemgå en profil efter opdatering af den underliggende motor, fordi understøttede muligheder kan ændre sig.

## Emulationsindstillinger

**Emulation** muligheder indeholder generelle lagringsindstillinger, globale genveje, gemte konfigurationer, og maskinspecifikke indstillinger.

### Generelle emuleringsmapper

<p align="center"><img src="images/options-emulation-general-en.png" alt="Generelle emuleringsmuligheder" width="72%"></p>

Indstil den delte emulering lagermappe og standardmapper til indfangning og gemte tilstande. **Åbn mappe** åbner den delte placering i File Explorer.

Hold fanger og gemte tilstande i separate mapper. En optagelse er et almindeligt billede; en gemt tilstand indeholder emulator- specifik maskine tilstand og kan afhænge af emulator version og konfiguration, der skabte det. Sikkerhedskopiering og medier ved siden af vigtige gemte stater.

### Globale genveje

<p align="center"><img src="images/options-emulation-shortcuts-en.png" alt="Emulationsgenveje" width="72%"></p>

Søg efter en handling eller nøgletildeling, tildele eller fjerne genveje, gendanne standardværdier, og klare konflikter. Status kolonne identificerer gyldige og modstridende opgaver.

For at ændre en genvej, skal du finde handlingen, klikke på **Tildel **, og tryk på den ønskede tastekombination. Tjek status før lukning Indstillinger. ** Ryd konflikter ** fjerner modstridende opgaver; det ikke gendanne standard kortlægning. Brug ** Gendanne standard**, når du ønsker at erstatte brugerdefinerede opgaver med standardsættet.

### Gemte konfigurationer

<p align="center"><img src="images/options-emulation-configurations-en.png" alt="Gemte emuleringskonfigurationer" width="72%"></p>

Denne side viser gemte maskiner. Vælg en indstilling til at redigere den i fanebladet **Amiga**. Du kan opdatere listen eller slette den valgte indstilling.

Sletning af en konfiguration fjerner den gemte maskine definition. Det bør ikke bruges som en måde at skubbe medier ud eller lukke en kørende maskine. Før sletning, bemærk enhver ROM, harddisk billede, og stat filer, der er forbundet med konfigurationen.

## Amiga-konfiguration

Den aktuelle grænseflade indeholder detaljerede Amiga konfigurationssider. Den samme indstillingsstruktur kan udvides til andre emulerede systemer uden at ændre hovedarbejdsgangen.

### Generelt

<p align="center"><img src="images/options-amiga-general-en.png" alt="Amiga generelle indstillinger" width="72%"></p>

Vælg Amiga model, gemme konfigurationen, installere eller erstatte emulator version, og definere standard mapper til harddiske og andre medier. **Søg versioner** spørger den officielle emulator- version kilde.

Start med modellen, fordi den begrænser senere sider. Ændring af det kan ændre de tilgængelige CPU, hukommelse, ROM, chipset, og opbevaring valg. Når du har valgt en emulator version, gemme konfigurationen, før du lancerer det fra hovedvinduet. Installation af en anden emulator version erstatter den version, der anvendes af denne konfiguration; det skaber ikke en anden kopi af maskinen.

### CPU

<p align="center"><img src="images/options-amiga-cpu-en.png" alt="Amiga CPU indstillinger" width="72%"></p>

CPU-siden viser den processor, der er valgt af maskinmodellen, og giver kompatibel præcision, FPU og hastighedsvalg. Indstillinger, der ikke gælder for den valgte model, forbliver deaktiverede.

- **CPU model** identificerer den emulerede processor.
- **Præcision** styrer timing model. Cycle- eksakte tilstande favorisere hardware kompatibilitet, men kræver mere host behandling.
- **FPU** muliggør en kompatibel flydepunktsenhed når den understøttes.
- **CPU hastighed** vælger oprindelige timing eller en accelereret tilstand.

For en basiskonfiguration, holde den modelafledte CPU og oprindelige hastighed. Skift kun acceleration efter maskinen støvler korrekt i sine standardindstillinger.

### RAM

<p align="center"><img src="images/options-amiga-ram-en.png" alt="Amiga RAM indstillinger" width="72%"></p>

Indstil Chip RAM, Slow RAM, Fast RAM, og understøttet ekspansionshukommelse. Kompatibilitetsmeddelelser forklarer begrænsninger for den valgte maskine, og den samlede konfigurerede hukommelse vises nederst.

**Chip RAM ** er tilgængelig for brugerdefinerede chips og er påkrævet af platformen. ** Slow RAM ** repræsenterer kompatibel ekspansion hukommelse, der anvendes af fælles konfigurationer. ** Fast RAM ** er procesorienteret ekspansionshukommelse. ** Zorro III RAM** gælder kun for modeller, der understøtter denne ekspansion arkitektur. Kompatibilitetsmeddelelser og deaktiveret kontrol forhindrer kombinationer, som den valgte model ikke kan repræsentere.

### ROM

<p align="center"><img src="images/options-amiga-rom-en.png" alt="Amiga ROM indstillinger" width="72%"></p>

Vælg systemet Kickstart ROM, valgfri udvidet ROM, og ROM nøgle. Listen detekteret - ROM viser navne, revisioner og kompatibilitet med den valgte model. Vælg en opdaget ROM og klik **Brug**, eller gennemse til en fil manuelt.

ROM filer leveres ikke af GW GUI. Brug ringmekanismer, du er lovligt tilladt at bruge.

Den opfangede liste er at foretrække frem for at gætte på et filnavn: Den rapporterer ROM-identiteten og reviderer og vurderer kompatibiliteten med den valgte model. **Kompatibel ** er det normale valg; ** Delvis kompatibel ** indikerer, at ROM kan starte, men ikke præcist matcher maskinen. ** Genopfrisk ** genopretter de konfigurerede ROM steder. ** Brug** tildeler den valgte detekterede ROM til konfigurationen.

### Video

<p align="center"><img src="images/options-amiga-video-en.png" alt="Amiga videoindstillinger" width="72%"></p>

Indstil videostandard, aspektforhold, opløsning, linjetilstand, grænsebeskæring, renderer, farvedybde, frame skipping, gamma og flicker fastsættelse. Yderligere chipset indstillinger er tilgængelige længere nede på siden, når understøttet af den valgte model.

| Indstilling | Praktisk virkning |
|---|---|
| Videostandard | Vælger PAL eller NTSC timing og forventet opdateringsadfærd |
| Orienteringsforhold | Kontrollerer hvordan det emulerede billede skaleres |
| Opløsning | Vælger automatisk eller eksplicit output- detalje |
| Linjetilstand | Kontrollerer behandling af interlaced eller line- fordoblet output |
| Afgrødegrænser | Fjerner kun ubrugt overscan når det er aktiveret |
| Rendering | Vælger den grafiske motor |
| Farvedybde | Vælger farvepræcision |
| Frame skip | Reducerer afleverede rammer når det er aktiveret |
| Gamma | Justerer lysstyrkens respons |
| Flicker fixer | Processer tilstande, der ellers synligt flimmer |

Ændr en visningsindstilling ad gangen. Hvis emuleringsvinduet bliver tomt eller ustabilt, skal du vende tilbage til automatisk opløsning, deaktiveret frame skip, neutral gamma, og den tidligere arbejder renderer.

### Lyd

<p align="center"><img src="images/options-amiga-audio-en.png" alt="Amiga lydindstillinger" width="72%"></p>

Aktivér eller deaktivér lyd, vælg output- enheden og latency, og indstil interpolation, Amiga filtrering, filtertype, stereo separation, floppy- drev lyd og CD- lydstyrke.

Lavere latency reducerer forsinkelse, men kan forårsage drop- outs på en travl computer. Øg den, hvis lyden rammer. Interpolation og Amiga lydfilter ændre lyd reproduktion snarere end emuleret program logik. Drivlydstyrken styrer den simulerede mekaniske lyd adskilt fra normal Amiga lyd.

### Opbevaring

<p align="center"><img src="images/options-amiga-storage-en.png" alt="Amiga lagerindstillinger" width="72%"></p>

Lagringssiden viser enhedsidentifikatorer, typer, modeller, tilknyttede medier og tilgængelige handlinger. Tilføj, konfigurér eller fjern enheder her. Disketter og cd 'er kan indsættes eller udskiftes direkte fra en løbemaskine.

**enhedsidentifikator ** er, hvordan det emulerede system behandler enheden. ** Type ** adskiller floppy, hard- disk, optiske og andre understøttede enheder. ** Model ** beskriver den emulerede hardware, mens ** Associated media** identificerer det aktuelt tildelte billede. Indstil enheden, før du forbinder værdifulde skrivbare medier, og holde sikkerhedskopier af harddiskbilleder.

### Tastatur

<p align="center"><img src="images/options-amiga-keyboard-en.png" alt="Amiga tastaturindstillinger" width="72%"></p>

Søg Amiga nøgler og vært opgaver, tildele nye nøgler, fjerne tilknytninger, gendanne standard, eller klare konflikter. Status kolonne rapporterer, om hver overdragelse er gyldig.

Den venstre kolonne navngiver den emulerede Amiga nøgle; **Association** viser værtsnøglekombinationen. En gyldig kortlægning kan stadig være ubelejlig, hvis Windows eller programmet forbeholder den samme genvej, så test kritiske kombinationer inde i løbemaskinen. Undgå at tildele muse- release eller fuldskærm genvej til en nøgle, som den emulerede software har brug for ofte.

### mus

<p align="center"><img src="images/options-amiga-mouse-en.png" alt="Amiga museindstillinger" width="72%"></p>

Indstil fysisk musehastighed, vælg hvilken analog stick styrer musen, juster den analog døde zone og hastighed, og konfigurér muse- action tilknytninger. Gendanne standardværdier eller klare kortlægningskonflikter, når det er nødvendigt.

Forøg den døde zone, hvis en controller forårsager pointrør drift. Justere venstre-og højre-stick hastighed uafhængigt, når begge pinde er aktiveret. De lavere mapping tabel associerede vært input med museknapper eller handlinger; inspicer sin konfliktstatus efter ændring af controller tilknytninger andre steder.

### Kontrollører

<p align="center"><img src="images/options-amiga-controllers-en.png" alt="Amiga controllerindstillinger" width="72%"></p>

Detektere tilsluttede controllere, tildele enheder og controller typer til Amiga porte, og konfigurere controller tilknytninger og turbo- brand indstillinger. Tilgængelige valg afhænger af detekteret hardware og den valgte maskine.

Port 1 og Port 2 konfigureres uafhængigt. **Automatic** controller type er et fornuftigt udgangspunkt, men software forventer en bestemt joystick eller mus kan kræve en eksplicit type. Kør detektion før tildele en nyligt tilsluttet controller. Turbo brand gentagne gange aktiverer en kortlagt input og bør forblive deaktiveret, medmindre spillet eller programmet nyder godt af det.

## Hardwarediagnostik og -vedligeholdelse

Disse dialoger åbnes fra fanebladet **Tools **. Hver dialog viser den genererede Greaseweazle- kommando. Gennemgå det før du klikker ** Kør**.

### Kontrollørens oplysninger

<p align="center"><img src="images/tool-controller-information-en.png" alt="Kontrollørens oplysninger" width="62%"></p>

Viser information rapporteret af den valgte controller. Udvid **Raw output** når du har brug for den komplette kommando respons.

Brug dette som den første diagnostiske kommando. En vellykket respons bekræfter, at GW GUI kan starte den konfigurerede Værtsværktøjer eksekverbar og kommunikere med den valgte enhed. Registrer firmware- og hardwareinformation, før du udfører en opdatering.

### USB båndbredde

<p align="center"><img src="images/tool-usb-bandwidth-en.png" alt="USB båndbredde" width="62%"></p>

Måler den tilgængelige USB kommunikationsbåndbredde. Brug det til at diagnosticere ustabile overførsler eller en uegnet USB forbindelse.

Luk anden software ved hjælp af controlleren før test. Gentag målingen efter ændring af USB port, kabel eller hub. Sammenlign resultater under lignende betingelser i stedet for at behandle en enkelt måling som en absolut garanti.

### Kørselshastighed

<p align="center"><img src="images/tool-drive-speed-en.png" alt="Kørselshastighed" width="62%"></p>

Måler drejningshastigheden. Øg antallet af målinger, når du har brug for et mere repræsentativt resultat.

En enkelt måling er en hurtig kontrol; flere målinger viser, om hastigheden er stabil. Lad drevet nå normal hastighed før fortolkning af resultatet. En uventet værdi kan indikere en forkert konfigureret hastighed, en mekanisk problem, eller en måling setup problem.

### Søgehoved

<p align="center"><img src="images/tool-seek-head-en.png" alt="Søgehoved" width="62%"></p>

Flytter drevhovedet til en valgt cylinder. **Tillad ekstreme flasker ** tillader normalt begrænsede positioner, og ** Hold motoren aktiv** forlader motoren kører under operationen. Brug kun ekstreme positioner, når hardwareproceduren udtrykkeligt kræver dem.

Normal søgning er nyttig til bekræftelse af hoved bevægelse eller positionering før en diagnose. Lyt til unormale gentagne påvirkninger og stop, hvis den ønskede cylinder er uhensigtsmæssig for drevet. Dette værktøj læser eller validerer ikke data ved destinationsflasken.

### Diagnosticering af køreindstilling

<p align="center"><img src="images/tool-drive-alignment-en.png" alt="Diagnosticering af køreindstilling" width="62%"></p>

Kører gentagne læser til drive- justering analyse. Det understøtter spor valg, revolution og læse tællinger, afkodning format, rå flux, indeks, hastighed, PLL, densitypin, hard- sektor, TG43, og returdata muligheder. Justeringsarbejde kræver passende referencemedier og hardware viden.

Begynd med en kendt reference disk og det mindste sæt af overrides. **Alternerende spor ** definerer de spor og hoveder, der udtages prøver af; ** Revolutioner pr. spor ** styrer hver prøvevarighed; ** Antal aflæsninger** bestemmer gentagelse. Aktivér kun en brugerdefineret diskdefinition eller afkodningsformat, når den matcher referencemedierne. Muligheder som falsk indeks, hårde sektorer, PLL tilsidesætter, tæthed pins, og TG43 er hardware- eller format- specifikke og kan ugyldiggøre en sammenligning, når de anvendes forkert.

### Hardware-stifter

<p align="center"><img src="images/tool-hardware-pins-en.png" alt="Hardware-stifter" width="62%"></p>

Læser eller ændrer en understøttet controller pin. Vælg den nål, aktivere **Skift pin ** kun, når du skriver en værdi, og vælg ** Højt niveau**, når det kræves af den planlagte hardware operation.

MED **Skift pin** deaktiveret, kommandoen spørger stiften. Dette er den sikrere standard. Ændring af et niveau direkte påvirker controller I / O og bør kun ske med den korrekte Greaseweazle hardware dokumentation og attached- drev ledninger.

### Nulstil controller

<p align="center"><img src="images/tool-reset-controller-en.png" alt="Nulstil controller" width="62%"></p>

Nulstiller Greaseweazle controller. Brug dette, når controlleren detekteres, men ikke længere reagerer normalt.

Vent på en aktiv disk operation til at afslutte før nulstilling. Derefter scanne controlleren igen, hvis dens tilslutningsstatus ikke gendannes automatisk. En nulstilling reparerer ikke en forkert `gw.exe`- sti eller en frakoblet USB- enhed.

### Forsinkelser

<p align="center"><img src="images/tool-delays-en.png" alt="Kontrollør-forsinkelser" width="62%"></p>

Læser eller ændrer controller timing værdier, herunder valg, hoved trin, bundfald, motor, automatisk fravalg, skrive timing, og indeks maske forsinkelser. Aktivér kun de værdier som du agter at ændre.

Ukontrollerede felter efterlader den tilsvarende controllerværdi uændret. Før redigering, registrere de eksisterende værdier. Timing ændringer kan påvirke hver efterfølgende fysisk drift, så test med undgåelige medier og genoprette kendte-gode værdier, hvis adfærd bliver upålidelig.

### Firmware

<p align="center"><img src="images/tool-firmware-en.png" alt="Firmware opdatering" width="62%"></p>

Opdaterer controller firmware. **Opdater bootloader** er udtrykkeligt markeret som risikabel og bør forblive deaktiveret, medmindre den officielle firmware procedure kræver det. Afbryd ikke forbindelsen til controlleren under en opdatering.

Før opdatering, skal du bekræfte den tilsluttede controller med **Controller oplysninger**, bruge en stabil direkte USB forbindelse, og lukke anden software, der kunne få adgang til det. Efter afslutning, gentilslutte eller genkan controlleren og læse sine oplysninger igen for at kontrollere den rapporterede firmware version.

## Logs og driftshistorik

Åbn operationens historik for at inspicere gemte logfiler ved operation.

<p align="center"><img src="images/operation-history-en.png" alt="Operationshistorik" width="68%"></p>

Vælg en log til venstre for at vise indholdet. **Export** gemmer en kopi til diagnostik eller support. Stier og kommandolinjer kan indeholde personlige mappenavne, så gennemgå eksporterede logs, før de deles.

Den levende konsol i hovedvinduet viser den aktuelle kommando og seneste output. Knappen kopierer den viste tekst.

### Læsning af en log

En nyttig diagnostisk log indeholder den genererede kommando, tidsstempler, motor output, og den endelige status. Arbejde fra bunden opad: identificere den endelige fejl, derefter finde den første advarsel eller mislykkedes spor, der gik forud for det. En senere generisk fiasko er ofte kun en konsekvens af en tidligere, mere specifik meddelelse.

Ved sammenligning af to forsøg, kontrollere, at controller, drev, motor, profil, kilde sti, output format, og ekspert argumenter var identiske. Ellers kan et andet resultat afspejle ændrede indstillinger snarere end disk ustabilitet.

## Ansøgningsdata og bærbar anvendelse

GW GUI holder brugerdata adskilt fra applikationsbinære filer. Afhængig af den valgte pakke og tilstand gemmes indstillinger, logs, downloadede værktøjer, emulatorkomponenter, indfangning, tilstande og maskinkonfigurationer enten i applikationen `Data` eller i de konfigurerede brugerdataplaceringer.

Før du erstatter eller flytter en bærbar installation, holde den komplette programmappe sammen og sikkerhedskopiere `Data` mappe. Flyt ikke individuelle filer fra `lib`, fordi programmet løser sine egne og tredjeparts biblioteker fra denne struktur.

### Foreslåede sikkerhedskopi indhold

Sikkerhedskopiér følgende når de er vigtige for din arbejdsgang:

- anvendelsesindstillinger og -profiler
- definition af controller og drev
- emuleringskonfigurationer
- ROM stier og lovligt ejede ROM backup;
- hard- disk og removable-media billeder;
- fanger og gemte tilstande
- driftsjournaler, der anvendes som opbevaringsoptegnelser.

Disk billeder kan være meget større end indstillinger. Lagre arkivmastere read- kun når det er muligt, og arbejde på kopier.

## Anbefalede arbejdsgange

### Arkivering af ukendt disk

1. Undersøg og rens drevet ved hjælp af en passende vedligeholdelsesprocedure.
2. Skriv-beskytte disken, hvis det er muligt.
3. Vælg **Læs > Rå billede (SCP)**.
4. Brug et beskrivende filnavn og læs det normale sporområde med flere omdrejninger.
5. Gennemgå konsollen og gemt log.
6. Undersøg begge sider i **Visualisering**.
7. Konverter en kopi til sandsynlige sektor formater.
8. Test de konverterede kopier i **Disk Explorer** eller passende software.
9. Bevar den rå master, log og noter sammen.

### Gendannelse af en disk fra et billede

1. Undersøg billedet og bekræft dets forventede familie og format.
2. Indsæt en spillelig eller bevidst skrivbar disk af den korrekte størrelse og tæthed.
3. Åbn **Skriv** og vælg billedet.
4. Bekræft det konfigurerede drev og detekteret format.
5. Skriv disken.
6. Læs det tilbage til et separat verifikationsbillede.
7. Sammenlign dekodet indhold og gennemgå mistænkelige spor visuelt.

### Oprettelse af en emuleret Amiga

1. Åbn **Indstillinger > Emulation > Indstillinger** og oprette eller vælge en maskine.
2. I **Amiga > Generelt**, vælg model og emulator version.
3. Tildel en kompatibel, lovligt opnået ROM.
4. Hold modelstandarderne for CPU og RAM på den første støvle.
5. Indstil video og lyd med konservative automatiske indstillinger.
6. Tilføj lagerenheder og associere kopierede mediebilleder.
7. Gennemgå tastatur, mus og controller opgaver.
8. Gem konfigurationen.
9. Retur til **Emulation **, vælg det, og klik på ** Open**.
10. Først efter en vellykket baseline boot, ændre acceleration eller avancerede indstillinger en ad gangen.

## Sikkerhedscheckliste

Før **Læs**:

- kildedisken er i det korrekte drev
- Kilden er så vidt muligt beskyttet mod skrift.
- udgangsstien ikke overskriver en eksisterende master
- profilen og sporvidde matcher disken.

Før **Skriv ** eller ** Slet**:

- destinationsdisken kan destrueres
- billedet og drevet er korrekt
- diskstørrelse og -tæthed er kompatible
- ingen arkivmaster bliver brugt som destination.

Før et hardware- skiftende værktøj:

- ingen anden operation kører
- den korrekte styreenhed vælges
- der er registreret aktuelle værdier
- controlleren har stabil strøm og USB-forbindelse
- aktionen understøttes af hardwaredokumentationen.

## Fejlfinding

### Den registeransvarlige er ikke opført på listen

1. Genindstil controlleren direkte til computeren.
2. Åbn **Indstillinger > Styrere og driver**.
3. Klik på **Scan**.
4. Verificer controllerens status og drevkonfiguration.
5. Kør **Kontrollør information**, hvis detektering lykkes, men kommandoer mislykkes.

Hvis det stadig ikke vises, prøv en anden direkte USB port og kabel, derefter rescan. Tjek Windows Enhedshåndtering for en nyligt opdaget serieenhed. En controller synlig for Windows, men fraværende fra GW GUI normalt peger på en travl port, gammel konfiguration, eller Værtsværktøjer problem; en controller fraværende fra Windows punkter til USB, magt, driver, eller hardware.

### `gw.exe` kan ikke findes

Åbn **Indstillinger > Styrere og drev **, derefter bruge ** Find gw.exe **, ** Vælg **, eller ** Download nyeste version**. Bekræft, at den fundne sti peger på den planlagte Greaseweazle installation.

Efter valg, køre **Controller oplysninger**. Hvis det mislykkes, før du kontakter hardware, inspicer loggen for en ugyldig eksekverbar sti, manglende filer, eller en version, der ikke kan starte.

### En operation bruger den forkerte motor

Åbn **Indstillinger > Motorer** og kontrollere den motor, der er tildelt den pågældende operation. GW GUI falder ikke stille tilbage til den anden motor.

Motorens indstillinger er separate: Ændring af konverteringsmotoren ændrer ikke læsning, skrivning eller Disk Explorer. Åbn den svigtende operation igen efter at have gemt indstillingen og bekræft den genererede kommando i konsollen.

### Et billede genkendes ikke

Deaktivér kun automatisk detektering, hvis du kender den korrekte maskine og format. Ellers prøv **Visualisering** fanen for at inspicere billedet på et lavere niveau.

Kontroller, om kilden er en raw flux capture, en sektor billede, en komprimeret beholder, eller en ikke-relateret fil med en vildledende udvidelse. Omdøb aldrig en udvidelse blot til at tvinge detektering; konvertering skal fortolke kildestrukturen korrekt.

### Emulering starter ikke

Verificer den gemte konfiguration, installerede emulator version, valgte ROM, lagerstier, og model kompatibilitet. Gennemgå programloggen for de fuldstændige fejldetaljer.

Midlertidigt returnere CPU, RAM, video, og opbevaring til en simpel model- kompatibel baseline. Hvis baseline starter, gendanne en brugerdefineret indstilling ad gangen. En gemt tilstand skabt med en anden emulator version eller maskine definition kan også mislykkes, selv når en ren boot virker.

### En genvej eller indgang virker ikke

Tjek både den globale **emulering > Genveje** side og maskinspecifikke tastatur, mus, eller controller side. Løs alle opgaver, der er markeret som modstridende.

Hvis musen er fanget, skal du bruge genvejen som vises i værktøjslinjen for running- machine. Hvis en controller blev tilsluttet efter Indstillinger blev åbnet, køre controller detektion igen, før du tildeler det.

### En kommando mislykkes uventet

1. Læs direkte konsol output.
2. Åbn **Operation historie** for den komplette gemte log.
3. Bekræft den valgte styreenhed, drev, profil, motor og filstier.
4. Eksportér den relevante log, hvis den skal deles med henblik på diagnosticering.

### Audio crackles eller pauser

Øge emulering lyd latency, tæt CPU-intensive applikationer, og returnere video ramme skipper og acceleration til deres tidligere værdier. Verificer at den tiltænkte Windows-lydenhed er valgt. Ændr en indstilling ad gangen, så den effektive korrektion kan identificeres.

### Emuleringsdisplayet er tomt eller langsomt

Returnér opløsning og linjetilstand til **Automatic**, deaktivér frame skipping og flicker fastsættelse midlertidigt, og prøv den tidligere arbejder renderer. Bekræft at de konfigurerede ROM og indsatte boot medier er gyldige. FPS indikatoren hjælper med at skelne en rendering- performance problem fra en maskine, der simpelthen ikke har startet.

### En læsning indeholder ustabile spor

Gentag aflæsningen til et nyt filnavn, øge revolutioner, hvor det er relevant, og sammenligne de berørte spor. Rens drevhovederne ved hjælp af en korrekt procedure og inspicer disken for fysiske skader. Læs ikke gentagne gange synligt aflevering eller beskadigede medier, fordi yderligere passerer kan forværre det.

## Ordliste

| Betegnelse | Betydning i GW GUI |
|---|---|
| Kontrollør | Greaseweazle hardware interface forbundet over USB |
| Kør | Den fysiske diskette drev knyttet til controlleren |
| Motor | Implementeringen udvalgt til at udføre en operation |
| Flux | Tidsdata, der repræsenterer magnetiske overgange, der læses fra en disk |
| Rå billede | En optagelse holder lav-niveau disk information, såsom SCP |
| Sektorbillede | En afkodet repræsentation organiseret i logiske sektorer |
| Revolution | En komplet rotation, der udtages prøver af, mens en bane aflæses |
| Cylinder | En radial hovedposition; en cylinder kan indeholde et spor på hver side |
| Ansvarlig | Disksiden valgt af det fysiske drev |
| Profil | Et genanvendeligt sæt indstillinger til en operation |
| ROM | Firmware billede kræves af en emuleret maskine |
| Reddet tilstand | Et øjebliksbillede af en kørende emulators maskintilstand |
| Renderer | Grafikkomponenten der bruges til at vise emuleringsoutput |

## Hurtig reference

| Hvis du vil... | Gå til... |
|---|---|
| Bevar en fysisk disk | **Læs** |
| Sæt et billede tilbage på en disk | **Skriv** |
| Fremstil et andet billedformat | **Konvertering** |
| Undersøg spor eller fluxanomalier | **Visualisering** |
| Gennemse filer inde i et billede | **Disk Explorer** |
| Kontrollér controllerkommunikation | **Værktøjer > Kontrollørens oplysninger** |
| Mål drevrotation | **Værktøjer > Kørselshastighed** |
| Gennemgå en tidligere kommando | **Operationshistorik** |
| Indstil hardware | **Indstillinger > Styremaskiner og -apparater** |
| Vælg implementeringer | **Indstillinger > Motorer** |
| Opret eller rediger en emuleret maskine | **Indstillinger > Emulering** |
| Start en gemt maskine | **Emulering** |
