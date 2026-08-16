# GW GUI Gebruikersgids

GW GUI is een Windows-applicatie voor het lezen, schrijven, converteren, inspecteren en emuleren van floppy-disk afbeeldingen. Het kan controleren Greaseweazle hardware, werken met disk-image bestanden via de interne motor, en draaien opgeslagen emulated-machine configuraties.

Deze handleiding beschrijft de Engelse interface weergegeven in de huidige versie van de toepassing. Het is geschreven als de bron van de afdrukbare gebruikershandleiding: screenshots illustreren de controles, terwijl de omliggende tekst uitlegt wat te kiezen, waarom te kiezen, en hoe het resultaat te verifiëren.

> **Belangrijk:** Een schijf lezen is niet destructief. Schrijven, wissen, firmware bijwerken, en sommige hardware tools kunnen media of hardware wijzigen. Lees de waarschuwing bij de betreffende procedure voordat u klikt ** Uitvoeren**.

### Hoe deze handleiding te gebruiken

Als dit uw eerste gebruik is GW GUI, compleet [Aan de slag](#getting-started), volg dan [Een schijf lezen](#reading-a-disk). Als de toepassing al is geconfigureerd, ga dan direct naar het hoofdstuk voor de bewerking die u wilt uitvoeren. De opties hoofdstukken dienen als een referentie wanneer een procedure vraagt u een aandrijving, motor, profiel, of emuleerde-machine instelling te veranderen.

Interface namen worden getoond in **vet**. Bestandsnamen, paden, commando's en letterlijke waarden worden weergegeven als `code`. Opmerkingen verklaren normaal gedrag; waarschuwingen identificeren handelingen die een schijf, controller of opgeslagen configuratie kunnen veranderen.

## Inhoud

1. [Begrip van de workflow](#understanding-the-workflow)
2. [Aan de slag](#getting-started)
3. [Hoofdvenster](#main-window)
4. [Een schijf lezen](#reading-a-disk)
5. [Een schijf schrijven](#writing-a-disk)
6. [Schijfafbeeldingen converteren](#converting-disk-images)
7. [Een schijfafbeelding weergeven](#visualizing-a-disk-image)
8. [Verkennen van schijfinhoud](#exploring-disk-contents)
9. [Met behulp van het gereedschap](#using-the-tools)
10. [Emulatie](#emulation)
11. [Toepassingsopties](#application-options)
12. [Emulatieopties](#emulation-options)
13. [Amiga configuratie](#amiga-configuration)
14. [Hardwarediagnostiek en onderhoud](#hardware-diagnostics-and-maintenance)
15. [Logs en operatiegeschiedenis](#logs-and-operation-history)
16. [Toepassingsgegevens en draagbaar gebruik](#application-data-and-portable-use)
17. [Aanbevolen werkstromen](#recommended-workflows)
18. [Veiligheidscontrolelijst](#safety-checklist)
19. [Probleemoplossing](#troubleshooting)
20. [Glossary](#glossary)
21. [Snelle referentie](#quick-reference)

## De workflow begrijpen

GW GUI scheidt fysieke-schijf bewerkingen van image-bestand bewerkingen:

| Doel | Invoer | Uitvoer | Aanbevolen pagina |
|---|---|---|---|
| Een diskette bewaren | Fysische schijf | Afbeeldingsbestand | **Gelezen** |
| Een diskette opnieuw aanmaken | Afbeeldingsbestand | Fysische schijf | **Schrijven** |
| Afbeeldingsformaat wijzigen | Afbeeldingsbestand | Een of meer afbeeldingsbestanden | **Omrekening** |
| Tracks en anomalieën controleren | Afbeeldingsbestand | Visuele analyse | **Visualisatie** |
| Bestanden die in een afbeelding zijn opgeslagen doorbladeren | Ondersteund image/bestandssysteem | Bestanden en mappen | **Disk Explorer** |
| Diagnose van een schijf of controller | Greaseweazle hardware | Metingen of status | **Hulpmiddelen** |
| Voer een opgeslagen virtuele machine uit | Opgeslagen machineconfiguratie | Emulatiesessie | **Emulatie** |

Voor het behoud, maak eerst een ruwe vangst en houd het onveranderd als een meester. Maak geconverteerde of gerepareerde werkkopieën van die meester. Dit voorkomt het herhalen van een fysieke lezing en bewaart informatie die een sectorgebonden formaat mogelijk niet bewaart.

## Beginnen

### Vereisten

- Vensters met de Microsoft .NET Desktop Runtime vereist door de toepassing.
- A Greaseweazle controller voor fysieke diskettes.
- Een ingesteld pad naar `gw.exe` bij gebruik van de Greaseweazle Host Tools motor.
- Juridisch verkregen ROM bestanden wanneer een emuleerde machine ze nodig heeft.

De toepassing controleert de vereiste .NET runtime bij het opstarten. Als het ontbreekt, volg dan de installatieprompt en herstart GW GUI.

### Voor het verbinden van hardware

Controleer het volgende voordat u een fysieke schijfoperatie uitvoert:

1. Sluit de Greaseweazle controller naar een stal USB Bakboord.
2. Sluit de floppy kabel aan met de juiste oriëntatie.
3. Sluit de aandrijving voeding voor het invoegen van waardevolle media.
4. Bevestig dat de schijfgrootte en dichtheid overeenkomen met de schijf.
5. Schrijf-bescherm de bronschijf indien mogelijk.

GW GUI kan schade veroorzaakt door verkeerde bekabeling, ongeschikt vermogen of een mechanisch onveilige aandrijving niet voorkomen. Test onbekende hardware eerst met een vervangbare schijf.

### Eerste lancering

1. Open `gwgui.exe`.
2. Open **Opties**.
3. In **Controllers en aandrijvingen**, scannen op de controller en configureren van de drive.
4. Verifiëren of selecteren van pad `gw.exe`.
5. In **Motoren**, kies welke motor elke verrichting moet uitvoeren.
6. Keer terug naar het hoofdvenster en selecteer het gewenste tabblad.

### Bevestigen dat de installatie klaar is

Een werkende setup moet de controller tonen en rijden in de statusbalk, bijvoorbeeld een schijfnummer, grootte, dichtheid, en COM Bakboord. In **Opties > Controllers en aandrijvingen **, moet de controller worden gemarkeerd ** Beschikbaar ** en de drive ** Geconfigureerd **Rennen. ** Controllerinformatie** voordat u waardevolle media leest als u communicatie wilt verifiëren zonder een schijf te wijzigen.

### Een motor kiezen

GW GUI kan meer dan één implementatie voor sommige operaties blootleggen. De **Greaseweazle Host Tools** motor roept de geconfigureerde `gw.exe`; de interne GW GUI motor behandelt ondersteunde handelingen binnen de toepassing. Motor selectie is expliciet en onafhankelijk voor het lezen, schrijven, conversie, en Disk Explorer. Als een bewerking niet wordt ondersteund door de geselecteerde motor, GW GUI meldt die toestand in plaats van automatisch van motor te veranderen.

## Hoofdvenster

Het hoofdvenster groepeert de belangrijkste bewerkingen in zeven tabbladen:

- **Gelezen** maakt een afbeelding van een fysieke schijf.
- **Schrijven** schrijft een afbeelding naar een fysieke schijf.
- **Omrekening** converteert een disk-image formaat in een of meer uitvoerformaten.
- **Visualisatie** toont tracks en flux of gedecodeerde gegevens.
- **Disk Explorer** Bladert door ondersteunde bestandssystemen en schijfinhoud.
- **Hulpmiddelen** biedt hardware onderhoud en kenmerkende opdrachten.
- **Emulatie** beheert en draait opgeslagen emuleerde machines.

De console onderaan toont het commando dat wordt uitgevoerd en de uitvoer ervan. De statusbalk rapporteert het geselecteerde station, profiel en huidige status.

### De interface lezen

De meeste bewerkingspagina's volgen hetzelfde patroon:

1. **Bron of bestemming** knoppen identificeren van de schijf, afbeelding of map.
2. **Formaatregeling** selecteer automatische detectie of een expliciete machine en formaat.
3. **Profielcontroles** herbruikbare instellingen toepassen.
4. **Geavanceerde instellingen** parameters blootleggen die normaal optioneel zijn.
5. **Uitvoeren** start de operatie.
6. De **console** toont de gegenereerde opdracht, voortgang, waarschuwingen en fouten.

De **Uitvoeren** knop impliceert niet dat alle waarden veilig zijn voor de ingevoegde schijf. Bekijk altijd de bestemming en geselecteerde schijf voordat een schrijf- of onderhoudsoperatie.

### Statusbalk en console

De linkerkant van de statusbalk identificeert de actieve fysieke aandrijving. Het centrum toont het actieve profiel wanneer deze geselecteerd is. De staat indicator meldt of de toepassing is klaar of druk. De console is niet alleen kenmerkend: het is de gezaghebbende record van het commando dat naar de geselecteerde motor wordt gestuurd. Gebruik de kopie control wanneer u dat commando moet behouden of delen.

## Een schijf lezen

Open de **Gelezen** tabblad om een fysieke diskette als afbeelding vast te leggen.

<p align="center"><img src="images/main-read-en.png" alt="Tabblad lezen" width="78%"></p>

### Basisprocedure

1. Plaats de bronschijf in het geconfigureerde station.
2. Kies het afbeeldingstype:
   - **Raw image (SCP)** bewaart informatie op fluxniveau.
   - **Bekende schijfindeling** maakt een afbeelding met behulp van een geselecteerde machine en formaat.
3. Kies de doelmap.
4. Voer de uitvoerbestandsnaam in.
5. Selecteer een profiel indien nodig.
6. Klik **Uitvoeren**.

De console toont de exacte opdracht en voortgang. De schijf niet verwijderen of de controller loskoppelen totdat de bewerking is voltooid.

### Het uitvoertype kiezen

Gebruik **Raw image (SCP)** wanneer het doel archival capture, analyse, recovery of latere conversie is. Een ruwe afbeelding registreert timing informatie en meerdere revoluties, die nuttig is voor ongewone formaten, zwakke sectoren, beschermingssystemen en beschadigde media.

Gebruik **Bekende schijfindeling** wanneer je de diskfamilie al kent en een direct bruikbaar sectorimage nodig hebt. Deze keuze kan kleiner en gemakkelijker te openen in andere software, maar het vertegenwoordigt het gedecodeerde resultaat in plaats van elk detail waargenomen door de schijf.

Wanneer onzeker, maak dan eerst de ruwe afbeelding. U kunt het later converteren zonder de schijf opnieuw te lezen.

### Map, bestandsnaam en profiel

De **Map ** is de doelmap. De ** Bestandsnaam** moet de schijf identificeren zonder alleen te vertrouwen op het fysieke label. Een handige archivale naam bevat de titel, schijfnummer of zijde, en een voorwaarde noot indien van toepassing. Voeg geen formaatextensie toe die strijdig is met het geselecteerde uitvoerformaat.

A **Profiel ** past een opgeslagen set leesparameters toe. Selecteer er maar één als je weet wat het bevat. De ** Standaard** profiel is geschikt voor een normale eerste poging; een gespecialiseerd herstelprofiel kan bewust meer omwentelingen of een ander spoorbereik lezen en dus langer duren.

### Geavanceerde instellingen

Uitvouwen **Geavanceerde instellingen** toegang tot formatspecifieke of deskundige parameters. Laat deze waarden ongewijzigd tenzij de schijf een bepaalde track range, revolutie count, of controller optie vereist.

Gemeenschappelijke geavanceerde waarden omvatten:

| Instellingen | Betreft | Wanneer moet ik het veranderen? |
|---|---|---|
| Trackbereik | Beperkt de cilinders en koppen te lezen | Enkelzijdige media, ongewone geometrie of een gerichte recovery pas |
| Revoluties | Bepaalt hoeveel rotaties worden bemonsterd | Verhoging voor instabiele of beschermde sporen; alleen voor snelheid verlagen indien van toepassing |
| Opmerkingen van deskundigen | Passeert extra motorparameters | Alleen wanneer dit is gedocumenteerd Greaseweazle richtsnoeren |

### Een succesvol lezen verifiëren

Vertrouw niet alleen op het ontbreken van een foutdialoog. Nadat het commando is voltooid:

1. Bevestig dat het uitvoerbestand bestaat en niet leeg is.
2. Lees de laatste consoleregels voor mislukte of ontbrekende nummers.
3. Afbeelding openen in **Visualisatie** om te controleren of beide zijden en het verwachte spoorbereik gegevens bevatten.
4. Open het in **Disk Explorer** wanneer het bestandssysteem wordt ondersteund.
5. Hou het logboek bij met belangrijke archiefopnamen.

Als herhaalde lezingen verschillen, behoud elke ruwe vangst in plaats van het overschrijven van de eerste. Verschillen kunnen nuttig zijn tijdens het herstel.

## Een schijf schrijven

Open de **Schrijven** tabblad om een bestaande afbeelding naar een fysieke diskette te schrijven.

<p align="center"><img src="images/main-write-en.png" alt="Tabblad schrijven" width="78%"></p>

### Basisprocedure

1. De doelschijf invoegen.
2. Selecteer de bron afbeelding met **Bladeren**.
3. Bevestig het gedetecteerde formaat.
4. Selecteer een profiel indien nodig.
5. Klik **Uitvoeren**.

Schrijven vervangt gegevens op de bestemmingsschijf. Controleer de geselecteerde schijf en afbeelding voordat u start.

> **Waarschuwing:** Schrijven is destructief. Het vervangt magnetische gegevens op de bestemmingsschijf. Gebruik waar mogelijk een schrijf-beschermd bronarchief en een aparte bestemmingsschijf.

### Alvorens te schrijven

Controleer vier items voordat u klikt **Uitvoeren**:

1. **Afbeelding:** het geselecteerde pad is de beoogde bronafbeelding.
2. **Schijf:** de schijf in de schijf kan veilig worden overschreven.
3. **Schijf:** de geconfigureerde grootte en dichtheid passen bij het bestemmingsmedium.
4. **Formaat:** automatische detectie of het handmatig geselecteerde formaat komt overeen met de afbeelding.

Als de bronafbeelding niet is getest, open deze dan in **Visualisatie ** of ** Disk Explorer** Eerst. Een succesvol schrijven kan geen onvolledige bronafbeelding repareren.

### Inspectie en wijziging van het spoor

Nadat een afbeelding is geselecteerd, **Tracks visualiseren ** opent zijn spoorweergave. ** Wijzigen** onthult de ondersteunde afbeelding wijzigingen voordat u schrijft. De beschikbare acties zijn afhankelijk van het geselecteerde formaat en de motor.

### Een geschreven schijf verifiëren

Wanneer de motor verificatie ondersteunt, gebruiken voor belangrijke media. Lees anders de geschreven schijf terug naar een nieuwe afbeelding en vergelijk de gedecodeerde inhoud ervan of controleer deze in **Visualisatie**. Houd de verificatie capture gescheiden van de oorspronkelijke afbeelding zodat het origineel nooit wordt overschreven.

Als schrijven mislukt op consistente tracks, controleer schijfconditie, dichtheid, schijf reinheid, en schijfconfiguratie. Als er willekeurig fouten optreden, controleer USB stabiliteit en communicatie met de controller.

## Schijfafbeeldingen converteren

De **Omrekening** tab converteert een bronafbeelding naar één of meerdere bestemmingsformaten.

<p align="center"><img src="images/main-conversion-en.png" alt="Tabblad conversie" width="78%"></p>

### Basisprocedure

1. Selecteer de bron afbeelding.
2. Geef optioneel uitvoernamen.
3. Kies een machinefamilie.
4. Selecteer een of meer uitvoerformaten en extensies.
5. Inschakelen **Tags toevoegen** als bestandsnamen het geconfigureerde tagpatroon moeten gebruiken.
6. Klik **Uitvoeren**.

De **Geselecteerd ** paneel toont de gewenste outputs. ** Bestandsmigratie** biedt de specifieke workflow voor het migreren van ondersteunde bestanden in plaats van het uitvoeren van een standaard beeldconversie.

### Formaten selecteren

De **Machine ** lijst filtert de getoonde formaten in de ** Formaat** paneel. Een formaatnaam beschrijft de logische schijfindeling; de extensie beschrijft de uitvoercontainer. Sommige formaten kunnen worden vertegenwoordigd door meer dan één extensie, en sommige containers kunnen niet elk kenmerk van een ruwe bron behouden.

Selecteer alleen outputs die je echt nodig hebt. Meerdere formaten zijn handig bij het maken van een archival master, een emulator-compatibele kopie, en een kopie voor een andere analyse tool in een operatie.

### Uitvoernaamgeving en tags

**Uitvoernamen ** kunt u de basisnamen gegenereerd voor geselecteerde formaten. ** Tags toevoegen ** past het bestandsnaampatroon toe dat is ingesteld in ** Opties > Algemeen**. Tags kunnen familie, formaat, uitbreiding, datum of tijd coderen. Bekijk het voorbeeld in Opties voordat u een grote batch converteren, zodat bestanden consequent worden genoemd.

### Conversieresultaten controleren

Voor elke gewenste uitvoer:

1. Bevestig dat er een bestand is aangemaakt.
2. Controleer de console op tracks of sectoren die niet gedecodeerd konden worden.
3. Open het resultaat in **Disk Explorer** als het een ondersteund bestandssysteem bevat.
4. Vergelijk de verwachte schijfcapaciteit en inhoud met de broncode.

Een conversie kan worden voltooid tijdens het rapporteren van informatieverlies dat inherent is aan het bestemmingsformaat. Behoud de originele ruwe afbeelding, zelfs wanneer de geconverteerde afbeelding correct lijkt.

## Een schijfimage visualiseren

De **Visualisatie** tabblad toont de structuur en gegevensverdeling van een afbeelding.

<p align="center"><img src="images/main-visualization-en.png" alt="tabblad Visualisatie" width="78%"></p>

1. Klik **Een schijfafbeelding openen**.
2. Bewaar **Automatische detectie** ingeschakeld, of selecteer de machine en formatteer handmatig.
3. Gebruik **Link zoom** om beide zijden op hetzelfde zoomniveau te houden.
4. Gebruik **Reset** om de eerste weergave te herstellen.
5. Open **Inspecteur** voor gedetailleerde informatie over de geselecteerde regio.

De legende onderscheidt normale flux, korte en lange overgangen, headers, gedecodeerde gegevens en gedetecteerde afwijkingen. Een ruwe afbeelding kan gegevens bevatten die niet in een bekend bestandssysteem kunnen worden gedecodeerd, maar hier nog steeds kunnen worden geïnspecteerd.

### De weergave interpreteren

Elk groot rond paneel vertegenwoordigt één schijfzijde. Het centrum identificeert de zijde en de huidige gegevenstoestand; concentrische posities komen overeen met sporen. Kleuren classificeren de gedetecteerde gebieden volgens de legende. De visualizer is bedoeld om vragen te beantwoorden zoals:

- Bevat de afbeelding gegevens aan één of beide zijden?
- Zijn de verwachte sporen aanwezig?
- Zijn anomalieën geïsoleerd of herhaald op de schijf?
- Heeft automatische detectie een plausibele machine en formaat geïdentificeerd?

Een anomaliekleur is een reden om het gebied te inspecteren, geen bewijs dat de schijf onbruikbaar is. Kopieerbeveiliging, niet-standaardopmaak, een zwakke opname en een beschadigde sector kunnen verschillende structuren produceren die contextuele interpretatie vereisen.

### Aanbevolen inspectiesequentie

Begin met gekoppelde zoom ingeschakeld om beide zijden op dezelfde schaal te vergelijken. Selecteer een verdachte regio, open **Inspecteur**, en te vergelijken met naburige tracks. Als het resultaat blijkt te zijn een detectie probleem, schakel automatische detectie en kies een bekende machine en formaat. Terug naar automatische detectie na de test zodat een gedwongen instelling niet per ongeluk wordt gebruikt voor een andere afbeelding.

## Schijfinhoud onderzoeken

De **Disk Explorer** tabblad bladert ondersteunde schijfafbeeldingen als een bestandshiërarchie.

<p align="center"><img src="images/main-disk-explorer-en.png" alt="Disk Explorer tab" width="78%"></p>

1. Open een bestaande afbeelding of lees een schijf.
2. Bewaar **Automatische detectie** ingeschakeld tenzij u een machine of formaat moet forceren.
3. Bekijk de volume informatie: systeem, bescherming, bestandssysteem, capaciteit, vrije ruimte, en item tellen.
4. Blader door mappen in het linkerpaneel.
5. Selecteer een item om de details in het rechterpaneel te bekijken.

Als het afbeeldingsformaat of bestandssysteem niet wordt ondersteund, gebruik **Visualisatie** in plaats daarvan de ruwe structuur te inspecteren.

### Inzicht in de panelen

De bovenste samenvatting beschrijft de aangekoppelde afbeelding en gedetecteerd volume. Het linkeronderpaneel bevat de maphiërarchie. De centrale tabel bevat items in de geselecteerde map met naam, wijzigingsdatum, type en grootte. Het rechterpaneel toont details voor het geselecteerde item.

Disk Explorer betekent niet dat elke rauwe track perfect is gedecodeerd. Gebruik de volumesamenvatting en het item tellen als een snelle plausibiliteitscontrole, open dan representatieve bestanden of vergelijk ze met een bekende directory lijst wanneer het behoud van nauwkeurigheid van belang is.

### Als er niets verschijnt

Bevestig eerst dat het beeldpad correct is. Controleer dan de gedetecteerde machine en formaat. Een geldige afbeelding kan een niet-ondersteund of beschadigd bestandssysteem bevatten, in welk geval de explorer leeg kan blijven, hoewel **Visualisatie** toont geregistreerde gegevens. Het bronbestand niet overschrijven of weggooien op basis van een lege verkenner.

## Gebruik van het gereedschap

De **Hulpmiddelen** tabgroepen Greaseweazle onderhoudswerkzaamheden.

<p align="center"><img src="images/main-tools-en.png" alt="Tabblad Hulpmiddelen" width="78%"></p>

Selecteer een commando uit de lijst links, bekijk de parameters en klik vervolgens op **Uitvoeren**. Destructieve of hardware wisselende commando's mogen alleen worden gebruikt na verificatie van de geselecteerde controller en drive.

De meeste gereedschapsdialogen bevatten drie gebieden: parameters bovenaan, een status- en rauw-outputgebied in het midden en het gegenereerde commando onderaan. De opdrachtvoorbeeldwijzigingen als opties zijn ingeschakeld. Een niet-aangevinkte parameter betekent normaal gesproken dat u deze waarde niet wijzigt, terwijl een aangevinkte parameter deze waarde in het commando opneemt.

De individuele kenmerkende dialogen worden beschreven in [Hardware diagnostiek en onderhoud](#hardware-diagnostics-and-maintenance).

## Emulatie

### Een opgeslagen machine openen

De **Emulatie ** tabbladlijsten opgeslagen configuraties. Selecteer één en klik ** Open**. Elke draaiende machine verschijnt in zijn eigen tabblad.

<p align="center"><img src="images/main-emulation-welcome-en.png" alt="Emulatie welkom scherm" width="78%"></p>

Machines aanmaken en bewerken in **Opties > Emulatie > Instellingen ** en ** Opties > Emulatie > Amiga**.

Als er geen configuratie verschijnt, maak dan eerst een in Opties. Een opgeslagen configuratie combineert het machinemodel, emulator versie, ROM, geheugen, video, audio, opslag en input mappings. Een configuratie opslaan start niet; terug naar de hoofdmap **Emulatie ** tab en klik ** Open**.

### Controles op de werking van de machine

<p align="center"><img src="images/main-emulation-running-en.png" alt="Geïmuleerde machine draaien" width="78%"></p>

De werkbalk van de loopmachine biedt stroom, pauze, reset, save-state, load-state, capture en display controls. Het toont ook:

- de geconfigureerde snel- en snellaadsnelkoppelingen;
- de actieve renderer, zoals Direct3D 11;
- de sneltoetsen op het volledige scherm en de muis;
- audio-, controller- en muisstatus;
- de huidige resolutie, refresh rate en frame rate.

De schijfstrip onderaan het emulatiescherm beheert verwijderbare media voor elke emuleerde schijf. Toetsenbordopdrachten kunnen worden gewijzigd in **Opties > Emulatie > Sneltoetsen**, terwijl emuleerde toetsenbord, muis, en controller mappings zijn geconfigureerd in de overeenkomstige Amiga tabs.

### Werkbalkreferentie

| Controlegroep | Betreft |
|---|---|
| Vermogen en pauze | Start, stopt, pauzeert of hervat de emuleerde machine |
| Controles resetten | Voert de ingestelde zachte of harde resetactie uit |
| Nationale controles | Bespaart of laadt een emulator toestand voor snelle voortzetting |
| Opname | Bewaart een afbeelding van het nagebootste scherm |
| Beeldscherm | De weergave wijzigen of volledig scherm invoeren |
| Sneltoetsherinnering | Toont de actieve sneltoetsen voor opslaan/laden |
| Render | Rapporteert de actieve videobackend |
| Invoerherinnering | Toont sneltoetsen voor volledig scherm en muis |
| Apparaatindicatoren | Rapporteert audio, controller en muisstatus |
| Prestaties | Rapporteert uitvoergrootte, herhalingsfrequentie en framesnelheid |

### Volledig scherm verlaten of de muis vrijgeven

De werkbalk toont de momenteel toegewezen sleutels. In de geïllustreerde configuratie, **Alt+ Terugkeer ** schakelt volledig scherm en ** F12** Laat de muis los. Behandel de weergegeven waarden als gezaghebbend omdat sneltoetsen opnieuw kunnen worden toegewezen.

### Gebruik van floppy media

De aandrijfstrip identificeert elke emuleerde aandrijving, zoals `DF0:`Gebruik de media om een afbeelding in te voegen, te vervangen of uit te werpen. Het vervangen van media verandert alleen de draaiende machine

## Toepassingsopties

Open **Opties** vanuit het hoofdvenster om de toepassing te configureren.

### Algemeen

<p align="center"><img src="images/options-general-en.png" alt="Algemene opties" width="72%"></p>

De **Algemeen** tabblad bevat:

- de standaard disk-imagemap;
- interfacetaal en -thema;
- filename-tag-generatie voor conversies;
- vooraf gedefinieerde en recente aangepaste tagpatronen;
- een live bestandsnaam voorbeeld.

Tag variabelen omvatten de bronnaam, familie, formaat, uitbreiding, datum en tijd. Gebruik de reset knop om het standaard patroon te herstellen.

De bestandsnaam voorbeeld updates voordat bestanden worden gemaakt. Gebruik het om dubbele scheidingstekens, ontbrekende extensies of dubbelzinnige namen te detecteren. Recente aangepaste patronen bieden snelle toegang tot eerdere namenschema's zonder de huidige voorinstelling te vervangen.

### Logs

<p align="center"><img src="images/options-logs-en.png" alt="Logopties" width="72%"></p>

Loggen kan voor elke bewerking onafhankelijk worden geconfigureerd. Kies voor elke categorie of logs worden opgeslagen, stel een maximale bestandsgrootte in en bepaal of vorige logs moeten worden bewaard. Een grootte van `0` betekent onbeperkt. **Map openen** opent de huidige logmap.

Inschakelen **Vorige logs bewaren** voor conservering en diagnose werk waar de geschiedenis van verschillende pogingen van belang is. Schakel het uit wanneer alleen het meest recente resultaat nuttig is. Maximale groottelimieten gelden voor logopslag, niet voor opgenomen schijfafbeeldingen.

### Controllers en aandrijvingen

<p align="center"><img src="images/options-controllers-and-drives-en.png" alt="Controllers en aandrijvingen" width="72%"></p>

Gebruik dit tabblad om:

- scannen op aangesloten controllers;
- schijfconfiguraties toevoegen en verwijderen;
- selecteer de grootte, dichtheid en snelheid van de aandrijving;
- hardware-instellingen opslaan;
- kiezen of automatisch vinden `gw.exe`;
- controleren en downloaden Greaseweazle Host Tools actualiseringen;
- herstel een eerder geconfigureerd uitvoerbaar pad.

Opgeslagen hardware-instellingen blijven beschikbaar wanneer een schijf tijdelijk wordt losgekoppeld.

#### Een schijf toevoegen

1. Klik **Scannen** en wachten tot aangesloten controllers verschijnen.
2. Klik **Een schijf toevoegen** als de vereiste aandrijving nog niet is vermeld.
3. Selecteer zijn logische aandrijving nummer, fysieke grootte, registratie dichtheid, en rotatie snelheid.
4. Bewaar de rij.
5. Bevestig dat het toont **Beschikbaar ** en ** Geconfigureerd**.

Gebruik de prullenbak om alleen de opgeslagen configuratie te verwijderen; de hardware wordt niet verbroken. Als dezelfde controller verschijnt op een andere COM port later, scan opnieuw voordat u ervan uitgaat dat de opgeslagen poort nog geldig is.

#### Beheer Greaseweazle Host Tools

**Zoeken gw.exe ** zoekopdrachten naar bekende locaties. ** Kies ** selecteert een specifiek uitvoerbaar bestand. ** Controleren op updates ** queries beschikbaar versies zonder vervanging van de geïnstalleerde. ** Laatste versie downloaden ** installeert het geselecteerde huidige pakket, en ** Vorig pad gebruiken ** herstelt de eerder geconfigureerde locatie. Na het wijzigen van het uitvoerbare bestand, uitvoeren ** Controllerinformatie** om te bevestigen dat de geselecteerde versie kan communiceren met de controller.

### Motoren

<p align="center"><img src="images/options-engines-en.png" alt="Motorselectie" width="72%"></p>

Kies de motor onafhankelijk voor het lezen, schrijven, conversie, en Disk Explorer. De geselecteerde motor wordt strikt gebruikt: als hij de gevraagde werking niet kan uitvoeren, GW GUI meldt de beperking in plaats van stil schakelen van motoren.

Deze onafhankelijkheid is opzettelijk. Bijvoorbeeld, fysieke lezingen kunnen gebruiken Greaseweazle Host Tools terwijl beeldconversie en exploratie gebruik maken van de interne motor. Registreer motorkeuzes in een profiel of projectnota wanneer reproduceerbaarheid van belang is.

### Profielen

<p align="center"><img src="images/options-profiles-en.png" alt="Profielen" width="72%"></p>

Profielen slaan herbruikbare instellingen op voor lezen, schrijven en omzetten. Selecteer de relevante categorie om de profielen te beheren. Een geselecteerd profiel wordt getoond in de statusbalk van het hoofdvenster en in operatieschermen.

Gebruik profielen voor herhaalbare workflows in plaats van onverklaarbare collecties van deskundige vlaggen. Geef elk profiel een doelspecifieke naam, zoals een bepaalde schijf, schijffamilie of herstelmethode. Bekijk een profiel na het bijwerken van de onderliggende motor omdat ondersteunde opties kunnen veranderen.

## Emulatie-opties

De **Emulatie** opties bevatten algemene opslaginstellingen, globale sneltoetsen, opgeslagen configuraties en machinespecifieke instellingen.

### Algemene emulatiemappen

<p align="center"><img src="images/options-emulation-general-en.png" alt="Algemene emulatieopties" width="72%"></p>

Stel de gedeelde emulatie-opslagmap en de standaardmappen in voor het vastleggen en opslaan van toestanden. **Map openen** opent de gedeelde locatie in File Explorer.

Houd vangt en opgeslagen staten in aparte mappen. Een capture is een gewone afbeelding; een opgeslagen staat bevat emulator-specifieke machinestatus en kan afhangen van de emulator versie en configuratie die het gemaakt. Back-up configuratie en media naast belangrijke opgeslagen staten.

### Wereldwijde sneltoetsen

<p align="center"><img src="images/options-emulation-shortcuts-en.png" alt="Sneltoetsen voor emulatie" width="72%"></p>

Zoeken naar een actie of sleuteltoewijzing, sneltoetsen toewijzen of verwijderen, standaardinstellingen herstellen en conflicten wissen. De status kolom identificeert geldige en tegenstrijdige opdrachten.

Om een snelkoppeling te wijzigen, vindt u de actie, klik **Toewijzen **, en druk op de gewenste toetsencombinatie. Controleer de status voor het sluiten van Opties. ** Conflicten wissen ** verwijdert tegenstrijdige opdrachten; het herstelt de standaard mapping niet. Gebruik ** Standaardinstellingen herstellen** wanneer u aangepaste opdrachten wilt vervangen door de standaard set.

### Opgeslagen configuraties

<p align="center"><img src="images/options-emulation-configurations-en.png" alt="Opgeslagen emulatieconfiguraties" width="72%"></p>

Deze pagina toont opgeslagen machines. Selecteer een configuratie om het te bewerken in de **Amiga** tab. U kunt de lijst vernieuwen of de geselecteerde configuratie verwijderen.

Een configuratie verwijderen verwijdert de opgeslagen machinedefinitie. Het mag niet worden gebruikt als een manier om media uitwerpen of sluiten van een lopende machine. Alvorens te verwijderen, let op alle ROM, harddisk image, en status bestanden geassocieerd met de configuratie.

## Amiga configuratie

De huidige interface biedt gedetailleerde Amiga configuratiepagina's. Dezelfde instellingen structuur kan worden uitgebreid voor andere emuleerde systemen zonder de belangrijkste workflow te veranderen.

### Algemeen

<p align="center"><img src="images/options-amiga-general-en.png" alt="Amiga algemene instellingen" width="72%"></p>

Kies de Amiga model, sla de configuratie op, installeer of vervang de emulatorversie, en definieer standaardmappen voor harde schijven en andere media. **Zoekversies** vraagt de officiële emulator-versie bron.

Begin met het model omdat het later pagina's beperkt. Het wijzigen kan de beschikbare wijzigen CPU, geheugen, ROM, chipset, en opslag keuzes. Na het selecteren van een emulator versie, sla de configuratie voordat u het start vanuit het hoofdvenster. Het installeren van een andere emulator versie vervangt de versie gebruikt door die configuratie; het maakt geen tweede kopie van de machine.

### CPU

<p align="center"><img src="images/options-amiga-cpu-en.png" alt="Amiga CPU instellingen" width="72%"></p>

De CPU pagina toont de processor geselecteerd door het machinemodel en biedt compatibele precisie, FPU, en snelheid keuzes. Opties die niet van toepassing zijn op het geselecteerde model blijven uitgeschakeld.

- **CPU model** de emuleerde processor identificeert.
- **Precisie** controleert het tijdmodel. Cyclus-exacte modi zijn bevorderlijk voor hardwarecompatibiliteit, maar vereisen meer hostverwerking.
- **FPU** maakt een compatibele floating-point unit mogelijk indien ondersteund.
- **CPU snelheid** selecteert originele timing of een versnelde modus.

Voor een basisconfiguratie, het model afgeleid houden CPU en originele snelheid. Verander acceleratie pas nadat de machine correct opstart bij de standaardinstellingen.

### RAM

<p align="center"><img src="images/options-amiga-ram-en.png" alt="Amiga RAM instellingen" width="72%"></p>

Spaander instellen RAM, Langzaam RAM, Snel RAM, en ondersteund uitbreiding geheugen. Compatibiliteitsberichten verklaren beperkingen voor de geselecteerde machine, en het totale geconfigureerde geheugen wordt onderaan weergegeven.

**Chip RAM ** is toegankelijk voor de aangepaste chips en is vereist door het platform. ** Langzaam RAM ** staat voor compatibele uitbreiding geheugen gebruikt door gemeenschappelijke configuraties. ** Snel RAM ** is processor-georiënteerd uitbreidingsgeheugen. ** Zorro III RAM** geldt alleen voor modellen die die uitbreidingsarchitectuur ondersteunen. De compatibiliteitsberichten en de bediening voor gehandicapten verhinderen combinaties die het geselecteerde model niet kan vertegenwoordigen.

### ROM

<p align="center"><img src="images/options-amiga-rom-en.png" alt="Amiga ROM instellingen" width="72%"></p>

Selecteer het systeem Kickstart ROM, facultatief verlengd ROMen ROM sleutel. De gedetecteerde-ROM lijst toont namen, herzieningen en compatibiliteit met het geselecteerde model. Een gedetecteerde selecteren ROM en klik **Gebruik**, of bladeren naar een bestand handmatig.

ROM bestanden worden niet geleverd door GW GUIGebruik ROM's die je wettelijk mag gebruiken.

De gedetecteerde lijst heeft de voorkeur boven het raden van een bestandsnaam: het rapporteert de ROM identiteit en herziening en evalueert de compatibiliteit met het geselecteerde model. **Compatibel ** de normale keuze is; ** Gedeeltelijk compatibel ** wijst erop dat de ROM kan opstarten maar komt niet precies overeen met de machine. ** Verversen ** herscant de geconfigureerde ROM locaties. ** Gebruik** wijst de geselecteerde gedetecteerde ROM naar de configuratie.

### Video

<p align="center"><img src="images/options-amiga-video-en.png" alt="Amiga video-instellingen" width="72%"></p>

Configureren van video standaard, aspect ratio, resolutie, lijn modus, rand verzamelen, renderer, kleurdiepte, frame overslaan, gamma, en flicker vaststelling. Extra chipset instellingen zijn beschikbaar verderop de pagina wanneer ondersteund door het geselecteerde model.

| Instellingen | Praktisch effect |
|---|---|
| Videostandaard | Selecteert PAL of NTSC timing en verwacht refreshgedrag |
| Verhouding | Bepaalt hoe het geëmulgeerde beeld wordt geschaald |
| Resolutie | Selecteer automatische of expliciete uitvoer detail |
| Regelmodus | Bedient de behandeling van interlaced of line-doubled output |
| Graangrenzen | Verwijdert ongebruikte overscan alleen indien ingeschakeld |
| Renderen | Kies de grafische backend |
| Kleurdiepte | Selecteer uitvoerkleurprecisie |
| Frame overslaan | Vermindert weergegeven frames indien ingeschakeld |
| Gamma | Past helderheidsrespons aan |
| Flickerfixer | Processeert modi die anders zichtbaar zouden flikkeren |

Verander één weergave instelling tegelijk. Als het emulatievenster leeg of instabiel wordt, keer dan terug naar automatische resolutie, uitgeschakelde frame overslaan, neutrale gamma, en de eerder werkende renderer.

### Geluid

<p align="center"><img src="images/options-amiga-audio-en.png" alt="Amiga audio-instellingen" width="72%"></p>

Inschakelen of uitschakelen van audio, kies het uitvoerapparaat en latency, dan configureren interpolatie, Amiga filteren, filter type, stereo scheiding, floppy-drive geluid, en CD-audio volume.

Lagere latentie vermindert vertraging maar kan drop-outs veroorzaken op een drukke computer. Verhoog het als audio kraakt. Interpolatie en de Amiga audiofilter verandert de geluidsweergave in plaats van de programmalogica. Drive-geluid volume regelt het gesimuleerde mechanische geluid gescheiden van normaal Amiga audio.

### Opslag

<p align="center"><img src="images/options-amiga-storage-en.png" alt="Amiga opslaginstellingen" width="72%"></p>

De opslagpagina bevat apparaatidentificaties, typen, modellen, bijbehorende media en beschikbare acties. Toevoegen, configureren of verwijderen van apparaten hier. Floppy disks en CD's kunnen direct van een draaiende machine worden geplaatst of vervangen.

De **apparaatidentificatie ** is hoe het emuleerde systeem het apparaat benadert. ** Type ** onderscheidt floppy, harde schijf, optische en andere ondersteunde apparaten. ** Model ** beschrijft de emuleerde hardware, terwijl ** Geassocieerde media** identificeert de momenteel toegewezen afbeelding. Configureer het apparaat voordat u waardevolle beschrijfbare media associeert en reservekopieën van harde schijfafbeeldingen bewaart.

### Toetsenbord

<p align="center"><img src="images/options-amiga-keyboard-en.png" alt="Amiga toetsenbordinstellingen" width="72%"></p>

Zoeken Amiga sleutels en host opdrachten, toewijzen van nieuwe sleutels, verwijderen mappen, herstellen van defaults, of duidelijk conflicten. De statuskolom rapporteert of elke toewijzing geldig is.

De linker kolom noemt de emuleerde Amiga sleutel; **Associatie** toont de host sleutel combinatie. Een geldige mapping kan nog steeds lastig zijn als Windows of de toepassing dezelfde sneltoets reserveert, dus test kritieke combinaties binnen de lopende machine. Vermijd het toewijzen van de muis-release of fullscreen snelkoppeling naar een sleutel die de emuleerde software vaak nodig heeft.

### Muis

<p align="center"><img src="images/options-amiga-mouse-en.png" alt="Amiga muisinstellingen" width="72%"></p>

Stel fysieke muissnelheid in, kies welke analoge stick de muis bestuurt, pas de analoge dode zone en snelheid aan en configureer muis-actie mappings. Defaults herstellen of conflicten in kaart brengen indien nodig.

Verhoog de dode zone als een controller drift veroorzaakt. Stel de linkse en rechtse snelheid onafhankelijk aan als beide sticks ingeschakeld zijn. De lagere mapping tabel associeert host ingangen met muisknoppen of acties; inspecteer de conflictstatus na het veranderen van controller mappings elders.

### Controllers

<p align="center"><img src="images/options-amiga-controllers-en.png" alt="Amiga controllerinstellingen" width="72%"></p>

Detecteer aangesloten controllers, wijs apparaten en controller types aan Amiga poorten, en configureren controller mappings en turbo-brand instellingen. De beschikbare keuzes zijn afhankelijk van gedetecteerde hardware en de geselecteerde machine.

Port 1 en Port 2 zijn onafhankelijk geconfigureerd. **Automatisch** controller type is een verstandig startpunt, maar software verwacht een bepaalde joystick of muis kan een expliciet type vereisen. Voer detectie uit voordat een nieuw aangesloten controller wordt toegewezen. Turbofire activeert herhaaldelijk een in kaart gebrachte ingang en moet uitgeschakeld blijven tenzij het spel of de toepassing ervan profiteert.

## Hardwarediagnostiek en onderhoud

Deze dialoogvensters worden geopend vanuit de **Hulpmiddelen ** tab. Elk dialoogvenster toont de gegenereerde Greaseweazle Commando. Bekijk het voordat u klikt ** Uitvoeren**.

### Controllerinformatie

<p align="center"><img src="images/tool-controller-information-en.png" alt="Controllerinformatie" width="62%"></p>

Toont de door de geselecteerde controller gerapporteerde informatie. Uitvouwen **Ruwe output** wanneer u het volledige commando antwoord nodig heeft.

Gebruik dit als het eerste diagnostische commando. Een succesvol antwoord bevestigt dat GW GUI kan het geconfigureerde Host Tools uitvoerbaar starten en communiceren met het geselecteerde apparaat. Neem de firmware en hardware informatie op voordat u een update uitvoert.

### USB bandbreedte

<p align="center"><img src="images/tool-usb-bandwidth-en.png" alt="USB bandbreedte" width="62%"></p>

De beschikbare maatregelen USB communicatiebandbreedte. Gebruik het om instabiele overdrachten of een ongeschikte diagnose USB verbinding.

Sluit andere software met behulp van de controller voor het testen. Herhaal de meting na het veranderen van de USB poort, kabel, of hub. Vergelijk de resultaten onder vergelijkbare omstandigheden in plaats van een enkele meting te behandelen als een absolute garantie.

### Rijsnelheid

<p align="center"><img src="images/tool-drive-speed-en.png" alt="Rijsnelheid" width="62%"></p>

Meet de draaisnelheid. Verhoog het aantal metingen wanneer u een representatiever resultaat nodig heeft.

Een enkele meting is een snelle controle; uit verschillende metingen blijkt of de snelheid stabiel is. Laat de aandrijving de normale snelheid bereiken alvorens het resultaat te interpreteren. Een onverwachte waarde kan wijzen op een verkeerde geconfigureerde snelheid, een mechanisch probleem, of een meetopstelling probleem.

### Zoekkop

<p align="center"><img src="images/tool-seek-head-en.png" alt="Zoekkop" width="62%"></p>

Verplaatst de aandrijfkop naar een geselecteerde cilinder. **Extreme cilinders toestaan ** normaal beperkte posities toestaat, en ** Motor actief houden** laat de motor draaien tijdens de operatie. Gebruik extreme posities alleen wanneer de hardware procedure ze expliciet vereist.

Normaal zoeken is nuttig voor het bevestigen van hoofdbeweging of positionering voor een diagnose. Luister naar abnormale herhaalde botsingen en stop als de gevraagde cilinder ongeschikt is voor de aandrijving. Deze tool leest of valideert geen gegevens op de bestemmingscilinder.

### Uitlijningsdiagnose van de aandrijving

<p align="center"><img src="images/tool-drive-alignment-en.png" alt="Uitlijningsdiagnose van de aandrijving" width="62%"></p>

Runs herhaalde leest voor drive-alignment analyse. Het ondersteunt track selectie, revolutie en lezen telt, decoderen formaat, ruwe flux, index, snelheid, PLL, dichtheid-pin, harde sector, TG43, en reverse-data opties. Voor aanpassingswerk is passende referentiemedia en hardwarekennis vereist.

Begin met een bekende referentieschijf en de kleinste set overrides. **Andere nummers ** de bemonsterde sporen en koppen; ** Revoluties per spoor ** controleert elke duur van het monster; ** Aantal lezingen** bepaalt herhaling. Schakel een aangepaste diskdefinitie of decoderen formaat alleen in als het overeenkomt met de referentiemedia. Opties zoals nepindex, harde sectoren, PLL overritten, dichtheidspennen, en TG43 zijn hardware- of formaat-specifiek en kan een vergelijking ongeldig maken wanneer onjuist gebruikt.

### Hardwarepennen

<p align="center"><img src="images/tool-hardware-pins-en.png" alt="Hardwarepennen" width="62%"></p>

Leest of wijzigt een ondersteunde controller pin. Selecteer de pin, inschakelen **Pin wijzigen ** alleen bij het schrijven van een waarde, en selecteer ** Hoog niveau** indien vereist door de beoogde hardwarebewerking.

Met **Pin wijzigen** uitgeschakeld, het commando vraagt de pin. Dit is de veiligere standaard. Een niveau wijzigen beïnvloedt direct controller I/O en mag alleen met de juiste Greaseweazle hardware documentatie en aangesloten-drive bedrading.

### Reset controller

<p align="center"><img src="images/tool-reset-controller-en.png" alt="Reset controller" width="62%"></p>

Stelt de Greaseweazle controller. Gebruik dit wanneer de controller wordt gedetecteerd maar niet meer normaal reageert.

Wacht tot elke actieve schijfbewerking voltooid is voordat het opnieuw ingesteld wordt. Daarna de controller opnieuw scannen als de verbindingsstatus niet automatisch herstelt. Een reset herstelt geen fout `gw.exe` pad of een afgesloten pad USB apparaat.

### Vertraging

<p align="center"><img src="images/tool-delays-en.png" alt="Vertraging regelgever" width="62%"></p>

Leest of verandert controller timing waarden, met inbegrip van selectie, kop stap, afwikkeling, motor, automatische deselectatie, schrijf timing, en index masker vertragingen. Activeer alleen de waarden die u wilt wijzigen.

Onaangevinkte velden laten de corresponderende controllerwaarde ongewijzigd. Neem voor het bewerken de bestaande waarden op. Timing veranderingen kunnen elke volgende fysieke werking beïnvloeden, dus test met vervangbare media en herstel bekende-goede waarden als gedrag onbetrouwbaar wordt.

### Firmware

<p align="center"><img src="images/tool-firmware-en.png" alt="Firmware-update" width="62%"></p>

Updates controller firmware. **Opstartlader bijwerken** is uitdrukkelijk als riskant aangemerkt en moet uitgeschakeld blijven, tenzij de officiële firmwareprocedure dit vereist. De controller tijdens een update niet loskoppelen.

Voor het bijwerken, bevestig de verbonden controller met **Controllerinformatie**, gebruik een stabiele direct USB verbinding, en sluit andere software die toegang tot het. Na voltooiing, opnieuw verbinden of opnieuw scannen van de controller en lees zijn informatie opnieuw om de gerapporteerde firmware versie te verifiëren.

## Logs en operatiegeschiedenis

Open de operatiegeschiedenis om opgeslagen logs te inspecteren door operatie.

<p align="center"><img src="images/operation-history-en.png" alt="Operatiegeschiedenis" width="68%"></p>

Selecteer een log aan de linkerkant om de inhoud weer te geven. **Uitvoer** bewaart een kopie voor diagnostiek of ondersteuning. Paden en commandoregels kunnen persoonlijke mapnamen bevatten, dus controleer geëxporteerde logs voordat ze worden gedeeld.

De live console in het hoofdvenster toont het huidige commando en de recente uitvoer. De kopieerknop kopieert de weergegeven tekst.

### Een logboek lezen

Een handige kenmerkende log bevat het gegenereerde commando, tijdstempels, motor uitgang, en de uiteindelijke status. Werk vanaf de onderkant omhoog: identificeer de laatste fout, lokaliseer dan de eerste waarschuwing of het mislukte spoor dat eraan vooraf ging. Een latere generieke fout is vaak alleen het gevolg van een eerdere, meer specifieke boodschap.

Bij het vergelijken van twee pogingen, controleer of de controller, aandrijving, motor, profiel, bronpad, output formaat en expert argumenten identiek waren. Anders kan een ander resultaat de gewijzigde instellingen weerspiegelen in plaats van schijf instabiliteit.

## Toepassingsgegevens en draagbaar gebruik

GW GUI houdt gebruikersgegevens gescheiden van programma binaire bestanden. Afhankelijk van het geselecteerde pakket en modus, worden instellingen, logs, gedownloade tools, emulatorcomponenten, captures, statuss en machineconfiguraties opgeslagen in de toepassing `Data` directory of in de geconfigureerde locatie van gebruikersgegevens.

Voor het vervangen of verplaatsen van een draagbare installatie, houd de volledige toepassingsmap samen en een back-up van de `Data` map. Individuele bestanden niet verplaatsen van `lib`, omdat de toepassing lost zijn eigen en derden bibliotheken uit die structuur.

### Voorgestelde back-up inhoud

Back-up van het volgende wanneer ze belangrijk zijn voor uw workflow:

- toepassingsinstellingen en -profielen;
- definities van besturing en aandrijving;
- emulatieconfiguraties;
- ROM paden en legaal gehouden ROM back-ups;
- harde schijf- en verwijderbare mediabeelden;
- vangt en redt staten;
- exploitatielogboeken gebruikt als bewaargegevens.

Schijfafbeeldingen kunnen veel groter zijn dan instellingen. Store archiefmeesters alleen-lezen indien mogelijk, en werk aan kopieën.

## Aanbevolen werkstromen

### Een onbekende schijf archiveren

1. Controleer en reinig de aandrijving volgens een passende onderhoudsprocedure.
2. Schrijf-bescherm de schijf indien mogelijk.
3. Selecteren **Lees > Raw image (SCP)**.
4. Gebruik een beschrijvende bestandsnaam en lees het normale trackbereik met meerdere omwentelingen.
5. Bekijk de console en bewaar log.
6. Inspecteer beide kanten in **Visualisatie**.
7. Een kopie omzetten naar waarschijnlijk sectorformaten.
8. Test de omgezette kopieën in **Disk Explorer** of geschikte software.
9. Bewaar de ruwe meester, log en noten samen.

### Een schijf van een afbeelding aanmaken

1. Controleer de afbeelding en bevestig de verwachte familie en formaat.
2. Voeg een vervangbare of opzettelijk schrijfbare schijf van de juiste grootte en dichtheid.
3. Open **Schrijven** en selecteer de afbeelding.
4. Bevestig de geconfigureerde schijf en gedetecteerd formaat.
5. Schrijf de schijf.
6. Lees het terug naar een aparte verificatie afbeelding.
7. Vergelijk gedecodeerde inhoud en bekijk verdachte sporen visueel.

### Een nabootsing aanmaken Amiga

1. Open **Opties > Emulatie > Instellingen** en maak of selecteer een machine.
2. In **Amiga > Algemeen**, kies het model en emulator versie.
3. Een verenigbaar, wettelijk verkregen ROM.
4. De standaardwaarden voor het model behouden CPU en RAM Op de eerste laars.
5. Video en audio configureren met conservatieve automatische instellingen.
6. Voeg opslagapparaten toe en associeer gekopieerde mediaafbeeldingen.
7. Beoordeling toetsenbord, muis, en controller opdrachten.
8. Sla de configuratie op.
9. Terug naar **Emulatie **, selecteer het, en klik ** Open**.
10. Pas na een succesvolle basisstart, verander acceleratie of geavanceerde instellingen één voor één.

## Veiligheidschecklist

Voor **Gelezen**:

- de bronschijf zich in de juiste schijf bevindt;
- de bron is waar mogelijk schrijfbeschermd;
- het uitvoerpad zal geen bestaande master overschrijven;
- het profiel en het spoorbereik komen overeen met de schijf.

Voor **Schrijven ** of ** Wissen**:

- de bestemmingsschijf mag worden vernietigd;
- het beeld en de aandrijving correct zijn;
- schijfgrootte en -dichtheid compatibel zijn;
- Er wordt geen archiefmeester gebruikt als bestemming.

Voor een hardware-veranderende tool:

- er wordt geen andere bewerking uitgevoerd;
- de juiste controller is geselecteerd;
- de huidige waarden zijn geregistreerd;
- de controller stabiel is en USB verbinding;
- de actie wordt ondersteund door de hardware documentatie.

## Problemen oplossen

### De controller is niet vermeld

1. Sluit de controller direct aan op de computer.
2. Open **Opties > Controllers en aandrijvingen**.
3. Klik **Scannen**.
4. Controleer de controllerstatus en de schijfconfiguratie.
5. Uitvoeren **Controllerinformatie** als detectie slaagt maar commando's falen.

Als het nog steeds niet verschijnt, probeer een andere direct USB Poort en kabel, dan opnieuw scannen. Controleer Windows Device Manager voor een nieuw gedetecteerd seriële apparaat. Een controller zichtbaar voor Windows, maar afwezig van GW GUI meestal wijst naar een drukke poort, oude configuratie, of Host Tools probleem; een controller afwezig uit Windows wijst naar USB, power, driver, of hardware.

### `gw.exe` kan niet gevonden worden

Open **Opties > Controllers en aandrijvingen **, dan gebruiken ** Zoeken gw.exe **, ** Kies **of ** Laatste versie downloaden**. Bevestigen dat het gedetecteerde pad wijst naar de beoogde Greaseweazle installatie.

Na het selecteren, uitvoeren **Controllerinformatie**. Als dat niet lukt voordat u contact opneemt met hardware, controleer dan het logboek voor een ongeldig uitvoerbaar pad, ontbrekende bestanden of een versie die niet kan starten.

### Een bewerking gebruikt de verkeerde motor

Open **Opties > Motoren** en controleer de aan die exacte werking toegewezen motor. GW GUI valt niet stil terug op de andere motor.

Motorinstellingen zijn gescheiden: het veranderen van de conversiemotor verandert niet lezen, schrijven, of Disk Explorer. Open de mislukte bewerking na het opslaan van de optie en bevestig het gegenereerde commando in de console.

### Een afbeelding wordt niet herkend

Schakel automatische detectie alleen uit als u de juiste machine en formaat kent. Anders, probeer de **Visualisatie** tabblad om de afbeelding op een lager niveau te inspecteren.

Controleer of de bron een raw flux capture is, een sector image, een gecomprimeerde container, of een niet-verbonden bestand met een misleidende extensie. Nooit een uitbreiding hernoemen om de detectie te forceren; conversie moet de bronstructuur correct interpreteren.

### Emulatie start niet

Controleer de opgeslagen configuratie, geïnstalleerde emulatorversie, geselecteerd ROM, opslagpaden en modelcompatibiliteit. Bekijk het programmalogboek voor de volledige foutgegevens.

Tijdelijke terugkeer CPU, RAM, video, en opslag naar een eenvoudige model-compatibele basislijn. Als de basislijn start, herstel één aangepaste instelling tegelijk. Een opgeslagen toestand gemaakt met een andere emulator versie of machine definitie kan ook falen, zelfs wanneer een schone boot werkt.

### Een sneltoets of invoer werkt niet

Controleer zowel de globale **Emulatie > Sneltoetsen** pagina en de machine-specifieke toetsenbord, muis, of controller pagina. Los elke opdracht op die als tegenstrijdig wordt aangemerkt.

Als de muis wordt gevangen, gebruik dan de sneltoets in de werkbalk van de loopmachine. Als een controller werd aangesloten nadat Opties werd geopend, voer controller detectie opnieuw voordat het toewijzen.

### Een opdracht mislukt onverwacht

1. Lees de live console uitgang.
2. Open **Operatiegeschiedenis** voor het volledige opgeslagen logboek.
3. Bevestig de geselecteerde controller, aandrijving, profiel, motor en bestandspaden.
4. Exporteer het relevante logboek als het moet worden gedeeld voor diagnose.

### Audiokrakers of pauzes

Verhoog emulatie audio latentie, sluit CPU-intensieve toepassingen, en terug te keren video frame overslaan en versnelling naar hun vorige waarden. Controleer of het beoogde Windows-audioapparaat is geselecteerd. Verander één instelling tegelijk zodat de effectieve correctie herkenbaar is.

### Het emulatiescherm is leeg of traag

Resolutie en regelmodus terug naar **Automatisch**, schakel frame overslaan en flicker vaststelling tijdelijk, en probeer de eerder werkende renderer. Bevestigen dat de geconfigureerde ROM en opstartmedia zijn geldig. De FPS indicator helpt een rendering-prestatie probleem te onderscheiden van een machine die gewoon niet is opgestart.

### Een gelezene bevat instabiele nummers

Herhaal de read naar een nieuwe bestandsnaam, verhoog revoluties waar nodig, en vergelijk de betrokken tracks. Reinig de drive heads met behulp van een juiste procedure en controleer de schijf voor fysieke schade. Lees niet herhaaldelijk zichtbaar afstoten of beschadigde media, omdat verdere passen kunnen verergeren.

## Woordenlijst

| Termijn | Betekenis GW GUI |
|---|---|
| Controller | De Greaseweazle hardware interface aangesloten over USB |
| Rijden | De fysieke diskette aan de controller bevestigd |
| Motor | De implementatie geselecteerd om een bewerking uit te voeren |
| Flux | Timing informatie voor magnetische overgangen gelezen van een schijf |
| Raw afbeelding | Een capture met laag-niveau schijfinformatie, zoals SCP |
| Sectorafbeelding | Een gedecodeerde vertegenwoordiging georganiseerd in logische sectoren |
| Revolutie | Een volledige rotatie bemonsterd tijdens het lezen van een track |
| Cilinder | Een radiale hoofdpositie; één cilinder kan aan elke zijde een spoor bevatten |
| Hoofd | De schijfzijde geselecteerd door de fysieke schijf |
| Profiel | Een herbruikbare set instellingen voor een bewerking |
| ROM | Firmware-afbeelding vereist door een emuleerde machine |
| Opgeslagen status | Een momentopname van een draaiende emulator |
| Render | De grafische backend waarmee emulatie-uitvoer wordt weergegeven |

## Snelle referentie

| Als je wilt... | Ga... |
|---|---|
| Een fysieke schijf behouden | **Gelezen** |
| Een afbeelding terugzetten op een schijf | **Schrijven** |
| Een ander afbeeldingsformaat aanmaken | **Omrekening** |
| Tracks of flux-anomalieën controleren | **Visualisatie** |
| Bestanden in een afbeelding doorbladeren | **Disk Explorer** |
| Controleer de communicatie met de controller | **Hulpmiddelen > Controllerinformatie** |
| Meetritrotatie | **Hulpmiddelen > Rijsnelheid** |
| Een vorige opdracht bekijken | **Operatiegeschiedenis** |
| hardware configureren | **Opties > Controllers en aandrijvingen** |
| Selecteer implementaties | **Opties > Motoren** |
| Een emuleerde machine aanmaken of bewerken | **Opties > Emulatie** |
| Een opgeslagen machine starten | **Emulatie** |
