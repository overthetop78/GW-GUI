# Uživatelská příručka GW GUI

GW GUI je aplikace pro Windows určená ke čtení, zápisu, převodu, kontrole a emulaci obrazů disků. Dokáže ovládat hardware Greaseweazle, pracovat se soubory obrazů disků prostřednictvím interního enginu a spouštět uložené konfigurace emulovaných počítačů.

Tato příručka popisuje anglické rozhraní uvedené v aktuální verzi aplikace. Je zapsán jako zdroj manuálu pro tisk: screenshoty ilustrují ovládání, zatímco okolní text vysvětluje, co si vybrat, proč si vybrat a jak ověřit výsledek.

> **Důležité:** Čtení disku je nedestruktivní. Psaní, mazání, aktualizace firmwaru a některé hardwarové nástroje mohou změnit média nebo hardware. Před kliknutím na ** Execute** si přečtěte varování připojené k příslušnému postupu.

### Jak se tento návod používá

Pokud je to poprvé, co používáte GW GUI, kompletní [Getting started ](#getting-started), pak následovat [Čtení disku ](#reading-a-disk). Pokud je aplikace již nakonfigurována, přejděte přímo do kapitoly pro operaci, kterou chcete provést. Kapitola možností slouží jako reference, pokud vás procedura požádá o změnu nastavení pohonu, motoru, profilu nebo emulovaného stroje.

Jména rozhraní jsou uvedena v **bold**. Jména souborů, cesty, příkazy a doslovné hodnoty jsou zobrazeny jako `code`. Poznámky vysvětlují normální chování; varování identifikují operace, které mohou změnit disk, regulátor nebo uloženou konfiguraci.

## Obsah

1. [Pochopení pracovního postupu ](#understanding-the-workflow)
2. [Začínáme ](#getting-started)
3. [Hlavní okno ](#main-window)
4. [Čtení disku ](#reading-a-disk)
5. [Psaní disku ](#writing-a-disk)
6. [Převod diskových obrázků ](#converting-disk-images)
7. [Vizualizace diskového obrazu ](#visualizing-a-disk-image)
8. [Průzkum obsahu disku ](#exploring-disk-contents)
9. [Použití nástrojů ](#using-the-tools)
10. [Emulace ](#emulation)
11. [Možnosti aplikace ](#application-options)
12. [Možnosti emulace ](#emulation-options)
13. [konfigurace Amiga ](#amiga-configuration)
14. [Hardwarová diagnostika a údržba ](#hardware-diagnostics-and-maintenance)
15. [Záznamy a provozní historie ](#logs-and-operation-history)
16. [Údaje o aplikaci a přenosné použití ](#application-data-and-portable-use)
17. [Doporučené pracovní toky ](#recommended-workflows)
18. [Bezpečnostní kontrolní seznam ](#safety-checklist)
19. [Řešení problémů ](#troubleshooting)
20. [Glosář ](#glossary)
21. [Rychlý odkaz ](#quick-reference)

## Pochopení pracovního postupu

GW GUI odděluje operace s fyzickým diskem od operací se soubory obrazů:

| Cíl | Vstup | Výstup | Doporučená karta |
|---|---|---|---|
| Zachovat disketu | Fyzický disk | Soubor obrázků | **Čtení** |
| Obnovit disketu | Soubor obrázků | Fyzický disk | **Zapsat** |
| Změnit formát obrázku | Soubor obrázků | Jeden nebo více obrazových souborů | **Převod** |
| Zkontrolovat stopy a anomálie | Soubor obrázků | Vizuální analýza | **Vizualizace** |
| Procházet soubory uložené v obrazu | Podporovaný obraz nebo souborový systém | Soubory a adresáře | **Disk Explorer** |
| Diagnostikovat disk nebo regulátor | Greaseweazle hardware | Měření nebo stav | **Nástroje** |
| Spustit uložený virtuální stroj | Konfigurace uloženého stroje | Emulační relace | **Emulace** |

Pro zachování, nejprve udělat syrové zachycení a udržet ji beze změny jako mistr. Vytvořte převedené nebo opravené pracovní kopie od tohoto mistra. To zabraňuje opakovanému fyzickému čtení a zachovává informace, které nemusí být zachovány v odvětvovém formátu.

## Začínáme

### Požadavky

- Windows s verzí Microsoft .NET Desktop Runtime vyžadovanou aplikací.
- Regulátor Greaseweazle pro fyzické floppy- diskové operace.
- Nakonfigurovaná cesta do `gw.exe` při použití Greaseweazle Host Tools motoru.
- Právně získané soubory ROM, když je emulovaný stroj vyžaduje.

Aplikace kontroluje požadovaný .NET runtime při spuštění. Pokud chybí, postupujte podle volby instalace a poté restartujte GW GUI.

### Před připojením hardwaru

Před spuštěním operace fyzického disku zkontrolujte následující:

1. Připojte ovladač Greaseweazle ke stabilnímu portu USB.
2. Spojte disketový kabel se správnou orientací.
3. Připojte napájení pohonu před vložením cenných médií.
4. Potvrďte, že velikost a hustota pohonu odpovídají disku.
5. Pokud je to možné, chraňte zdrojový disk proti zápisu.

GW GUI nemůže zabránit poškození způsobenému nesprávným kabeláží, nevhodným výkonem nebo mechanicky nebezpečným pohonem. Nejprve otestujte neznámé hardware s postradatelným diskem.

### První start

1. Otevřít `gwgui.exe`.
2. Otevřete **Options**.
3. V **Controllers a pohony**, skenovat pro regulátor a konfigurovat disk.
4. Ověřte nebo vyberte cestu do `gw.exe`.
5. V **motory**, vyberte, který motor by měl provádět každý provoz.
6. Vraťte se do hlavního okna a vyberte požadovanou záložku operace.

### Potvrzuji, že nastavení je připraveno.

Pracovní nastavení by mělo ukázat regulátor a řídit v stavové liště, např. číslo pohonu, velikost, hustota a port COM. V možnostech **> Regulátory a pohony **, regulátor by měl být označen ** k dispozici ** a pohon ** Configured **. Spustit ** Řídicí informace** před čtením hodnotných médií, pokud chcete ověřit komunikaci bez změny disku.

### Výběr motoru

GW GUI může pro některé operace odhalit více než jednu implementaci. Motor **Greaseweazle Host Tools ** se odvolává na konfigurované **; vnitřní ovládání motoru GW GUI podporuje provoz uvnitř aplikace. Výběr motoru je explicitní a nezávislý pro čtení, psaní, konverzi a Disk Explorer. Pokud není provoz podporován zvoleným motorem, GW GUI tuto podmínku hlásí místo automatické výměny motorů.

## Hlavní okno

Hlavní okno zařazuje hlavní operace do sedmi karet:

- **Čtení** vytváří obraz z fyzického disku.
- **Write** napíše obrázek na fyzický disk.
- **Conversion** konvertuje jeden formát obrazu na jeden nebo více výstupních formátů.
- **Vizualizace** zobrazuje stopy a data o toku nebo dekódování.
- **Disk Explorer** prohlíží podporované souborové systémy a obsah disku.
- **Tools** poskytuje hardwarové a diagnostické příkazy.
- **Emulace** spravuje a běží uložené emulované stroje.

Konzola dole zobrazí příkaz, který se provádí, a jeho výstup. Stavová lišta hlásí zvolený disk, profil a aktuální stav.

### Čtení rozhraní

Většina provozních stránek se řídí stejným vzorem:

1. **Zdroj nebo cíl** ovládá identifikovat disk, obrázek nebo složku.
2. **Ovládání formátu** zvolte automatickou detekci nebo explicitní stroj a formát.
3. **Ovládání profilu** aplikuje opakovaně použitelné nastavení.
4. **Pokročilá nastavení** zobrazí parametry, které jsou obvykle volitelné.
5. **Spustit** zahájí operaci.
6. Deska **** ukazuje generovaný příkaz, pokrok, varování a chyby.

Tlačítko **Execute** neznamená, že všechny hodnoty jsou bezpečné pro vložený disk. Před operací zápisu nebo údržby vždy zkontrolujte cíl a zvolený disk.

### Stavebnice a konzole

Levá strana stavové lišty identifikuje aktivní fyzický pohon. Střed zobrazuje aktivní profil při výběru. Indikátor stavu uvádí, zda je aplikace připravena nebo obsazená. Konzola není pouze diagnostická: je to autoritativní záznam příkazu zaslaného do vybraného motoru. Použijte jeho ovládání kopírování, když potřebujete zachovat nebo sdílet tento příkaz.

## Čtení disku

Otevřete kartu **Read** pro zachycení fyzického disketového disku jako obrazu.

<p align="center"><img src="images/main-read-en.png" alt="Přečíst kartu" width="78%"></p>

### Základní postup

1. Vložte zdrojový disk do nakonfigurovaného disku.
2. Vyberte typ obrázku:
   - **Syrový obraz (SCP)** uchovává informace o hladině flux- level.
   - **Známý formát disku** vytvoří obrázek pomocí vybraného stroje a formátu.
3. Vyberte cílovou složku.
4. Zadejte výstupní název souboru.
5. V případě potřeby vyberte profil.
6. Klikněte na **Execute**.

Konzole ukazuje přesný příkaz a pokrok. Neodstraňujte disk nebo odpojte ovladač, dokud operace neskončí.

### Výběr typu výstupu

Použít **Syrový obraz (SCP)**, pokud cílem je zachycení archivu, analýza, obnova nebo pozdější konverze. Syrový obraz zaznamenává informace o načasování a více otáček, které jsou užitečné pro neobvyklé formáty, slabé sektory, ochranné systémy a poškozená média.

Použít **Známý formát disku**, když už znáte rodinu disků a potřebujete přímo použitelný obrázek sektoru. Tato volba může být menší a jednodušší otevřít v jiném softwaru, ale představuje dekódovaný výsledek spíše než každý detail pozorovaný na disku.

Pokud si nejste jisti, vytvořte nejprve syrový obrázek. Můžete jej převést později, aniž byste si znovu přečetli disk.

### Složka, název souboru a profil

Složka **** je cílový adresář. Název souboru **** by měl identifikovat disk, aniž by se spoléhal pouze na jeho fyzické označení. Užitečné archivní jméno obsahuje název, číslo disku nebo stranu a případně poznámku o stavu. Nepřidávejte rozšíření formátu, které je v rozporu se zvoleným formátem výstupu.

Profil **** používá uloženou sadu parametrů čtení. Vyberte jednu, pouze pokud víte, co obsahuje. Výchozí profil **** je vhodný pro normální první pokus; specializovaný profil obnovy může záměrně číst více otáček nebo jiný rozsah dráhy, a proto trvá déle.

### Pokročilá nastavení

Expand **Pokročilá nastavení** pro přístup k formátově specifickým nebo odborným parametrům. Zanechte tyto hodnoty beze změny, pokud disk nevyžaduje určitou volbu rozsahu dráhy, počtu otáček nebo regulátoru.

Společné pokročilé hodnoty zahrnují:

| Nastavení | Účel | Kdy to změnit |
|---|---|---|
| Rozsah koleje | Omezuje čtení válců a hlav | Jednostranná média, neobvyklá geometrie, nebo cílený průkaz na obnovu |
| Revoluce | Kontroluje, kolik rotací je vzorkováno | Zvýšení nestabilních nebo chráněných kolejí; případně snížení pouze rychlosti |
| Odborné argumenty | Předává dodatečné parametry motoru | Pouze po zdokumentovaných pokynech Greaseweazle |

### Ověření úspěšného čtení

Nespoléhejte pouze na absenci chybového dialogu. Po dokončení příkazu:

1. Potvrďte, že výstupní soubor existuje a není prázdný.
2. Přečtěte si poslední konzolové řádky pro neúspěšné nebo chybějící skladby.
3. Otevřete obrázek v **Vizualizace** pro kontrolu, zda obě strany a očekávaný rozsah dráhy obsahují data.
4. Otevřete jej v **Disk Explorer** při podpoře souborového systému.
5. Udržujte operační záznamy s důležitými archivními záznamy.

Pokud se opakované čtení liší, zachovat každý surový zachycení spíše než přepsat první. Rozdíly mohou být užitečné během zotavení.

## Psaní disku

Otevřete kartu **Write** pro zápis stávajícího obrazu na fyzický disketový disk.

<p align="center"><img src="images/main-write-en.png" alt="Záložka" width="78%"></p>

### Základní postup

1. Vložte cílový disk.
2. Vyberte zdrojový obrázek s **Procházet**.
3. Potvrďte zjištěný formát.
4. V případě potřeby vyberte profil.
5. Klikněte na **Execute**.

Zápis nahrazuje data na cílovém disku. Před startem ověřte zvolený disk a obrázek.

> Varování **:** Psaní je destruktivní. Nahrazuje magnetická data na cílovém disku. Použijte archiv chráněný spisy a oddělený cílový disk, kdykoli je to možné.

### Před psaním

Před kliknutím na ** Zkontrolovat čtyři položky:

1. **Image:** zvolená cesta je zamýšleným zdrojem obrazu.
2. **Disk:** disk v disku může být bezpečně přepsán.
3. **Drive:** nakonfigurovaná velikost a hustota vyhovují cílovému médiu.
4. **Formát:** automatická detekce nebo ručně vybraný formát odpovídá obrázku.

Pokud není zdrojový obraz testován, nejprve jej otevřete v Placeholdera Visualization **nebo** Disk Explorer Disk Explorer. Úspěšný zápis nemůže opravit neúplný zdrojový obraz.

### Kontrola a změna dráhy

Po výběru obrázku se zobrazí skladba **Visualize **. ** Modify** zobrazuje podporované úpravy obrazu před zápisem. Dostupné akce závisí na zvoleném formátu a motoru.

### Ověření písemného disku

Pokud motor podporuje ověření, použijte jej pro důležitá média. V opačném případě přečti písemný disk zpět na nový obrázek a porovnej jeho dekódovaný obsah nebo jej zkontroluj v **Visualizace**. Ponechat záznam ověření odděleně od původního obrazu tak, aby originál nebyl nikdy přepsán.

Pokud psaní selže na konzistentních tratích, zkontrolujte stav disku, hustotu, čistotu pohonu a konfiguraci pohonu. Pokud dojde k selhání náhodně, zkontrolujte stabilitu a komunikaci regulátoru USB.

## Převod diskových obrázků

Záložka **Conversion** převádí zdrojový obrázek do jednoho nebo několika formátů cíle.

<p align="center"><img src="images/main-conversion-en.png" alt="Převod karty" width="78%"></p>

### Základní postup

1. Vyberte zdrojový obrázek.
2. Volitelně uveďte výstupní názvy.
3. Vyberte si rodinu strojů.
4. Vyberte jeden nebo více výstupních formátů a rozšíření.
5. Povolit **Přidat značky**, pokud by názvy souborů měly používat nakonfigurovaný vzor značky.
6. Klikněte na **Execute**.

Vybraný panel **uvádí požadované výstupy.** Migrace souborů ** poskytuje vyhrazený pracovní tok pro migraci podporovaných souborů spíše než standardní konverzi obrazu.

### Výběr formátů

Seznam **** filtruje formáty uvedené ve formátu ****. Název formátu popisuje logické rozložení disku; rozšíření popisuje výstupní kontejner. Některé formáty mohou být zastoupeny více než jedním rozšířením a některé kontejnery nemohou zachovat všechny vlastnosti zdroje.

Vyberte pouze výstupy, které skutečně potřebujete. Vícenásobné formáty jsou užitečné při vytváření archiválního master, emulator- kompatibilní kopie, a kopie pro jiný analytický nástroj v jedné operaci.

### Výstupní názvy a značky

**Výstupní názvy ** vám umožní ovládat základní jména generovaná pro vybrané formáty. ** Přidat značky ** používá model názvu souboru nakonfigurovaný v Možnosti **> Obecné**. Značky mohou kódovat rodinu, formát, rozšíření, datum nebo čas. Náhled na příklad v Možnosti před konverzí velké dávky tak, že soubory jsou pojmenovány konzistentně.

### Kontrola výsledků konverze

Pro každý požadovaný výstup:

1. Potvrďte, že byl vytvořen soubor.
2. Zkontrolujte konzoli pro stopy nebo sektory, které nelze dekódovat.
3. Otevřete výsledek v **Disk Explorer**, pokud obsahuje podporovaný souborový systém.
4. Porovnejte očekávanou kapacitu a obsah disku se zdrojem.

Přepočet může být dokončen při vykazování ztráty informací, která je součástí formátu určení. Zachovat původní surový obrázek, i když převrácený obraz vypadá správně.

## Vizualizace obrazu disku

Záložka **Vizualizace** zobrazuje strukturu a distribuci dat obrazu.

<p align="center"><img src="images/main-visualization-en.png" alt="Záložka Vizualizace" width="78%"></p>

1. Klikněte na **Otevřít obrázek disku**.
2. Udržujte **Automatická detekce** povolena, nebo vyberte stroj a formát ručně.
3. Použít **Link zoom** pro udržení obou stran na stejné úrovni zoom.
4. Pro obnovení původního zobrazení použijte **Reset**.
5. Podrobné informace o vybrané oblasti zobrazíte otevřením **Inspector**.

Legenda rozlišuje normální tok, krátké a dlouhé přechody, hlavičky, dekódovaná data a zjištěné anomálie. Syrový obrázek může obsahovat data, která nelze dekódovat do známého souborového systému, ale lze je zde ještě zkontrolovat.

### Tlumočení pohledu

Každý velký kruhový panel představuje jednu stranu disku. Střed identifikuje stranu a její současný stav dat; soustředné polohy odpovídají kolejím. Barvy klasifikují zjištěné oblasti podle legendy. Vizualizér je určen k zodpovězení otázek, jako jsou:

- Obsahuje obrázek data na jedné straně nebo obojí?
- Jsou tu očekávané stopy?
- Jsou anomálie izolovány nebo se opakují přes disk?
- Identifikovala automatická detekce možný stroj a formát?

Barva anomálie je důvodem pro kontrolu regionu, ne důkaz, že disk je nepoužitelný. Kopírování ochrany, nestandardní formátování, slabý záznam a poškozený sektor mohou vytvářet různé struktury, které vyžadují kontextovou interpretaci.

### Doporučená inspekční sekvence

Začněte s připojeným zoomem umožňujícím porovnání obou stran ve stejném měřítku. Vyberte podezřelý region, otevřete **Inspektor** a porovnejte jej se sousedními tratěmi. Pokud se výsledek jeví jako problém s detekcí, vypněte automatickou detekci a vyberte známý stroj a formát. Po zkoušce se vraťte k automatické detekci, aby nebylo omylem použito vynucené nastavení pro jiný obrázek.

## Zkoumání obsahu disku

Záložka **Disk Explorer** zobrazuje podporované obrázky disků jako hierarchii souborů.

<p align="center"><img src="images/main-disk-explorer-en.png" alt="Tabka Disk Explorer" width="78%"></p>

1. Otevřete existující obrázek nebo si přečtěte disk.
2. Udržujte **Automatická detekce** povolena, pokud nepotřebujete vynutit stroj nebo formát.
3. Prohlédněte si informace o objemu: systém, ochrana, souborový systém, kapacita, volný prostor a počet položek.
4. Procházení adresářů v levém panelu.
5. Vyberte položku pro zobrazení jeho detailů v pravém panelu.

Pokud je formát obrázku nebo souborový systém nepodporován, použijte pro kontrolu surové struktury místo toho vizualizaci **.

### Pochopení panelů

Horní přehled popisuje namontovaný obraz a detekovaný objem. Lower-left panel obsahuje hierarchii adresáře. Centrální tabulka uvádí položky ve zvoleném adresáři s názvem, datem změny, typem a velikostí. Pravý panel zobrazuje detaily pro vybranou položku.

Disk Explorer neznamená, že každá syrová stopa byla dokonale dekódována. Použijte přehled objemu a počet položek jako rychlou kontrolu věrohodnosti, poté otevřete reprezentativní soubory nebo je porovnejte se známým seznamem adresářů, pokud jde o přesnost uchovávání.

### Když se nic neobjeví

Nejprve potvrďte, že cesta obrazu je správná. Pak zkontrolujte detekovaný stroj a formát. Platná fotografie může obsahovat nepodporovaný nebo poškozený souborový systém, v takovém případě může průzkumník zůstat prázdný, i když **Visualizace** zobrazuje zaznamenaná data. Nepřepisujte nebo nezbavujte zdrojový obraz pouze na základě prázdného průzkumníka.

## Použití nástrojů

Tabulky **Tools** skupiny Greaseweazle údržby.

<p align="center"><img src="images/main-tools-en.png" alt="Karta Nástroje" width="78%"></p>

Zvolte příkaz ze seznamu vlevo, zkontrolujte jeho parametry a potom klikněte na **Execute**. Příkazy k destruktivnímu nebo pevnému uložení by měly být použity pouze po ověření vybraného regulátoru a řízení.

Většina dialogů nástrojů obsahuje tři oblasti: parametry nahoře, stav a raw- výstupní oblast ve středu a generovaný příkaz dole. Náhled příkazů se mění, protože jsou povoleny volby. Nekontrolovaný parametr obvykle znamená "neměnit tuto hodnotu", zatímco zatržený parametr tuto hodnotu v příkazu obsahuje.

Jednotlivé diagnostické dialogy jsou popsány v [Hardwarová diagnostika a údržba ](#hardware-diagnostics-and-maintenance).

## Emulace

### Otevřít uložený stroj

Záložka **Emulace ** obsahuje uložené konfigurace. Vyberte jednu a klikněte na ** Open**. Každý běžící stroj se objeví na své vlastní kartě.

<p align="center"><img src="images/main-emulation-welcome-en.png" alt="Emulační vítací obrazovka" width="78%"></p>

Vytvořit a upravovat stroje v Možnosti **> Emulace > Konfigurace ** a ** Možnosti > Emulace > Amiga Amiga.

Pokud se neobjeví žádná konfigurace, nejprve vytvořte jednu v Možnosti. Uložená konfigurace kombinuje model stroje, emulátor verze, ROM, paměť, video, audio, úložiště a vstupní mapování. Uložení konfigurace není spuštěn; návrat do hlavní karty **Emulace ** a klepněte na tlačítko ** Open**.

### Ovládání běžeckých strojů

<p align="center"><img src="images/main-emulation-running-en.png" alt="Spouštěcí emulovaný stroj" width="78%"></p>

Nástrojová lišta running- machine poskytuje napájení, pauzu, reset, save- state, načítání, zachycení a ovládání displeje. Také ukazuje:

- konfigurované zkratky pro rychlé ukládání a rychlé zatížení;
- aktivní zprostředkovatel, jako je Direct3D 11;
- zkratky na celou obrazovku a na myši;
- stav zvuku, regulátoru a myši;
- aktuální rozlišení, obnovovací a snímkovací frekvence.

Diskový proužek v dolní části emulačního displeje spravuje odnímatelná média pro každý emulovaný pohon. Přidělení klávesnic lze změnit v možnostech **> Emulace > Zkratky**, zatímco emulované mapování klávesnice, myši a regulátoru jsou nakonfigurovány v odpovídajících záložkách Amiga.

### Odkaz na nástrojové lišty

| Kontrolní skupina | Účel |
|---|---|
| Výkon a pauza | Spustí, zastaví, zastaví, nebo obnoví emulovaný stroj |
| Obnovit ovládání | Provádí nakonfigurovanou akci soft nebo hard reset |
| Státní kontroly | Šetří nebo zatěžuje stav emulátoru pro rychlé pokračování |
| Zachycení | Uloží obraz emulovaného displeje |
| Zobrazit | Mění prezentaci displeje nebo zadává celou obrazovku |
| Upomínka na rychlý stav | Zobrazuje aktivní zkratky uložení / zatížení |
| Návratnost | Hlásí aktivní video backend |
| Upomínka vstupu | Zobrazuje zkratky na celou obrazovku a na myši |
| Ukazatele zařízení | Hlásí stav zvuku, regulátoru a myši |
| Výkonnost | Zprávy o velikosti výstupu, obnovovací frekvenci a frekvenci snímků |

### Opuštění celé obrazovky nebo uvolnění myši

V nástrojové liště se zobrazí aktuálně přidělené klíče. V ilustrované konfiguraci **Alt + Return ** přepíná celou obrazovku a ** F12** uvolňuje myš. Zacházejte se zobrazenými hodnotami jako autoritativními, protože zkratky lze přeřadit.

### Použití disketových médií

Pohon identifikuje každý emulovaný pohon, jako je `DF0:`. Pomocí ovládání médií vložte, nahraďte nebo vysuňte obrázek. Výměna médií mění pouze vložený disk běžícího stroje; nemění definici skladovacího zařízení v uloženém stroji, pokud není tato akce výslovně uložena.

## Možnosti použití

Konfiguraci aplikace otevřete pomocí **Options** v hlavním okně.

### Obecné

<p align="center"><img src="images/options-general-en.png" alt="Obecné možnosti" width="72%"></p>

Záložka **General** obsahuje:

- výchozí složka disk-image;
- jazyk a téma rozhraní;
- generace filename- tag pro konverzi;
- předdefinované a nedávné vzory vlastních značek;
- živý příklad názvu souboru.

Proměnné označení zahrnují název zdroje, rodinu, formát, prodloužení, datum a čas. Pomocí tlačítka reset obnovíte výchozí vzor.

Název souboru se před vytvořením souborů aktualizuje. Použijte to k detekci duplikovaných oddělovačů, chybějících rozšíření nebo nejasných jmen. Nedávné vlastní vzory poskytují rychlý přístup k dřívějším schématům pojmenování bez nahrazení aktuálního přednastaveného.

### Záznamy

<p align="center"><img src="images/options-logs-en.png" alt="Možnosti záznamu" width="72%"></p>

Záznamy lze konfigurovat nezávisle pro každou operaci. Pro každou kategorii zvolte, zda uložit protokoly, nastavit maximální velikost souboru, a rozhodnout, zda předchozí protokoly by měly být zachovány. Velikost `0` znamená neomezenou. **Otevřená složka** otevře aktuální adresář záznamu.

Povolit **Mějte předchozí protokoly** pro konzervační a diagnostické práce, kde historie několika pokusů záleží. Zakázat to, když je užitečný pouze nejnovější výsledek. Maximální limity velikosti platí pro ukládání záznamů, nikoli pro zachycené obrázky disků.

### Regulátory a pohony

<p align="center"><img src="images/options-controllers-and-drives-en.png" alt="Regulátory a pohony" width="72%"></p>

Použijte tuto kartu na:

- skenování připojených ovladačů;
- přidat a odstranit konfigurace pohonu;
- zvolit velikost, hustotu a rychlost pohonu;
- uložit nastavení hardwaru;
- zvolit nebo automaticky najít `gw.exe`;
- zkontrolovat a stáhnout aktualizace Greaseweazle Host Tools;
- obnovit dříve nakonfigurovanou spustitelnou cestu.

Při dočasném odpojení disku zůstává k dispozici uložené nastavení hardwaru.

#### Přidání pohonu

1. Klikněte na **Scan** a počkejte, až se objeví připojené regulátory.
2. Klikněte na **Přidejte disk**, pokud požadovaná jednotka není již uvedena.
3. Vyberte logické číslo pohonu, fyzikální velikost, hustotu záznamu a rychlost otáčení.
4. Šetři si to.
5. Potvrďte, že ukazuje **k dispozici ** a ** konfigurované**.

Ovládání koše lze použít pouze k odstranění uložené konfigurace; neodpojuje hardware. Pokud se stejný ovladač objeví na jiném portu COM později, skenujte znovu před předpokládáním, že uložený port je stále platný.

#### Správa Greaseweazle Host Tools

**Najít gw.exe ** vyhledávání známých míst. ** Vyberte ** vybere konkrétní spustitelný. ** Zkontrolujte aktualizace ** dotazy dostupné verze bez nahrazení nainstalovaného. ** Stáhněte si nejnovější verzi ** nainstaluje vybraný aktuální balíček a ** Použijte předchozí cestu ** obnoví dřívější konfigurované umístění. Po změně spusťte funkci ** Controller informace** pro potvrzení, že vybraná verze může komunikovat s regulátorem.

### Motory

<p align="center"><img src="images/options-engines-en.png" alt="Výběr motoru" width="72%"></p>

Zvolte nezávisle motor pro čtení, zápis, konverzi a Disk Explorer. Vybraný motor se používá striktně: pokud nemůže provést požadovaný provoz, ohlásí GW GUI omezení místo tichého spínání motorů.

Tato nezávislost je úmyslná. Například fyzické údaje mohou používat Greaseweazle Host Tools, zatímco konverzi obrazu a průzkum využívají vnitřní motor. Zaznamenejte volbu motoru do profilu nebo poznámky k projektu, pokud jde o reprodukovatelnost.

### Profily

<p align="center"><img src="images/options-profiles-en.png" alt="Profily" width="72%"></p>

Profiles ukládá opakovaně použitelné nastavení pro operace čtení, zápisu a konverze. Pro správu profilů vyberte příslušnou kategorii. Zvolený profil je zobrazen ve stavové liště hlavního okna a v provozních obrazovkách.

Používejte profily pro opakovatelné pracovní toky spíše než jako nevysvětlitelné sbírky znaleckých vlajek. Každému profilu uveďte konkrétní jméno, jako je konkrétní jednotka, rodina disků nebo metoda obnovy. Překontrolujte profil po aktualizaci základního motoru, protože podporované možnosti se mohou změnit.

## Možnosti emulace

Možnosti emulace ** obsahují obecné nastavení úložiště, globální zkratky, uložené konfigurace a nastavení specifická pro stroje.

### Obecné emulační složky

<p align="center"><img src="images/options-emulation-general-en.png" alt="Obecné možnosti emulace" width="72%"></p>

Nastavte sdílenou emulační složku a výchozí složky pro zachytávání a ukládání stavů. **Otevřená složka** otevře sdílenou polohu v File Exploreru.

Udržujte záznamy a uložené stavy v samostatných složkách. Pochycení je běžný obraz; uložený stav obsahuje stav stroje specifický pro emulátor a může záviset na verzi emulátoru a konfiguraci, která jej vytvořila. Podporovat konfiguraci a média spolu s důležitými uloženými stavy.

### Globální zkratky

<p align="center"><img src="images/options-emulation-shortcuts-en.png" alt="Emulační zkratky" width="72%"></p>

Hledat akci nebo klíčové zadání, přiřadit nebo odstranit zkratky, obnovit chyby a jasné konflikty. Sloupec stavu označuje platné a protichůdné úkoly.

Chcete-li změnit zkratku, najděte akci, klikněte na **Přiřazení ** a stiskněte požadovanou kombinaci klíče. Zkontrolujte stav před zavřením Možnosti. ** Jasné konflikty ** odstraňuje konfliktní úkoly; neobnovuje výchozí mapování. Používejte ** Obnovit vady**, pokud chcete nahradit vlastní úkoly standardní sadou.

### Uložené konfigurace

<p align="center"><img src="images/options-emulation-configurations-en.png" alt="Uložené konfigurace emulace" width="72%"></p>

Tato stránka zobrazuje uložené stroje. Vyberte konfiguraci pro její úpravu v záložce **Amiga**. Seznam můžete obnovit nebo smazat vybranou konfiguraci.

Smazání konfigurace odstraní uloženou definici stroje. Neměl by být použit jako způsob, jak vysunout média nebo zavřít běžící stroj. Před vymazáním si poznamenejte jakékoli ROM, harddisk image a state soubory spojené s konfigurací.

## Nastavení Amiga

Aktuální rozhraní poskytuje podrobné konfigurační stránky Amiga. Stejnou strukturu nastavení lze rozšířit i pro jiné emulované systémy, aniž by se změnil hlavní pracovní postup.

### Obecné

<p align="center"><img src="images/options-amiga-general-en.png" alt="Amiga obecná nastavení" width="72%"></p>

Vyberte model Amiga, uložte konfiguraci, nainstalujte nebo nahraďte emulátor a definujte výchozí složky pro pevné disky a jiná média. **Vyhledávání verze** dotazy oficiální emulator- version zdroj.

Začněte s modelem, protože omezuje pozdější stránky. Změna může změnit dostupné CPU, paměť, ROM, chipset a možnosti ukládání. Po výběru verze emulátoru uložte konfiguraci před jejím spuštěním z hlavního okna. Instalace jiné verze emulátoru nahrazuje verzi použitou touto konfigurací; nevytváří druhou kopii stroje.

### CPU

<p align="center"><img src="images/options-amiga-cpu-en.png" alt="Nastavení Amiga CPU" width="72%"></p>

Stránka CPU zobrazuje procesor vybraný modelem stroje a poskytuje kompatibilní přesnost, FPU a volbu rychlosti. Volby, které se nevztahují na vybraný model, zůstávají zakázány.

- **CPU model** identifikuje emulovaný procesor.
- **Precision** ovládá časový model. Cycle- přesné režimy podporují hardwarovou kompatibilitu, ale vyžadují více zpracování hostitelů.
- **FPU** umožňuje kompatibilní plovoucí bodovou jednotku při podpoře.
- **CPU rychlost** vybere původní načasování nebo zrychlený režim.

Pro základní konfiguraci si ponechte model odvozený od CPU a původní rychlost. Zrychlení změňte až poté, co stroj správně nastaví.

### RAM

<p align="center"><img src="images/options-amiga-ram-en.png" alt="Nastavení Amiga RAM" width="72%"></p>

Nastavte Chip RAM, Slow RAM, Fast RAM a podporovanou rozšiřující paměť. Zprávy o kompatibilitě vysvětlují omezení pro vybraný stroj a celková nastavená paměť je zobrazena dole.

**Chip RAM ** je přístupný pro vlastní čipy a je vyžadován platformou. ** Pomalé ** představuje kompatibilní rozšiřující paměť používaná běžnými konfiguracemi. ** Fast RAM ** je procesně orientovaná rozšiřující paměť. ** Zorro III RAM** se vztahuje pouze na modely, které podporují tuto expanzivní architekturu. Zprávy o kompatibilitě a vypnuté ovládání zabraňují kombinacím, které vybraný model nemůže reprezentovat.

### ROM

<p align="center"><img src="images/options-amiga-rom-en.png" alt="Nastavení Amiga ROM" width="72%"></p>

Vyberte systém Kickstart ROM, volitelný rozšířený ROM a ROM klíč. Detekovaný seznam ROM zobrazuje názvy, revize a kompatibilitu se zvoleným modelem. Zvolte detekovaný **a klikněte na ** Použít **, nebo navštivte soubor ručně.

Soubory s příponou ROM nejsou dodávány společností GW GUI. Používejte ROM, které můžete legálně používat.

Detekovaný seznam je vhodnější než hádat z názvu souboru: zobrazí identitu a revizi ROM a vyhodnotí kompatibilitu se zvoleným modelem. **Kompatibilní ** je normální volba; ** Částečně kompatibilní ROM naznačuje, že ROM může spustit, ale není přesně shodný stroj.** Refresh ** obnovuje konfigurované polohy ROM.** Použití ** přiřazuje zvolený detekovaný ROM do konfigurace.

### Video

<p align="center"><img src="images/options-amiga-video-en.png" alt="Nastavení videa Amiga" width="72%"></p>

Konfigurovat video standard, poměr stran, rozlišení, řádkový režim, střih hranic, renderer, barevná hloubka, přeskakování rámů, gama, a nastavení blikání. Další nastavení čipset jsou k dispozici níže po stránce, pokud je podporována zvoleným modelem.

| Nastavení | Praktický účinek |
|---|---|
| Video standard | Vybere PAL nebo NTSC načasování a očekávané obnovovací chování |
| Poměr stran | Kontroluje, jak se emulovaný obraz stupňuje |
| Usnesení | Vybere automatický nebo explicitní detail výstupu |
| Traťový režim | Ovládání zpracování prokládaného nebo lineabilního výstupu |
| Obilné hranice | Odstraní nevyužité přeskenování pouze pokud je povoleno |
| Překládání | Výběr pozadí grafiky |
| Hloubka barev | Vybere výstupní přesnost barev |
| Přeskočení rámu | Snižuje vykreslené rámy, je-li povoleno |
| Gamma | Nastavuje odpověď na jas |
| Upínač zapalovačů | Proces režimů, které by jinak viditelně blikat |

Změnit jedno nastavení displeje najednou. Pokud se emulační okno stane prázdným nebo nestabilním, vraťte se k automatickému rozlišení, vypnutému snímkování, neutrálnímu gamma a dříve pracujícímu přejímači.

### Audio

<p align="center"><img src="images/options-amiga-audio-en.png" alt="Nastavení zvuku Amiga" width="72%"></p>

Povolit nebo zakázat zvuk, zvolit výstupní zařízení a latenci, pak konfigurovat interpolaci, filtrování Amiga, typ filtru, stereo separace, floppy- drive zvuk a CD- audio hlasitost.

Nižší latence snižuje zpoždění, ale může způsobit výpadky na rušném počítači. Zvyšte to, když se zvuk rozbije. Interpolace a zvukový filtr Amiga spíše mění reprodukci zvuku než emulaci logiky programu. Hlasitý zvuk řídí simulovaný mechanický zvuk odděleně od normálního zvuku Amiga.

### Skladování

<p align="center"><img src="images/options-amiga-storage-en.png" alt="Nastavení úložiště Amiga" width="72%"></p>

Ukládací stránka uvádí identifikátory zařízení, typy, modely, související média a dostupné akce. Přidat, konfigurovat, nebo odstranit zařízení zde. Disky a CD lze vložit nebo vyměnit přímo z běžícího stroje.

Identifikátor zařízení **** je způsob, jakým emulovaný systém zařízení oslovuje. ** Type ** rozlišuje disková, pevnostní, optická a další podporovaná zařízení. ** Model ** popisuje emulovaný hardware, zatímco ** Associated média** identifikuje aktuálně přiřazený obraz. Nastavit zařízení před přiřazením hodnotných zapisovatelných médií a udržet zálohování obrázků na pevném disku.

### Klávesnice

<p align="center"><img src="images/options-amiga-keyboard-en.png" alt="Nastavení klávesnice Amiga" width="72%"></p>

Hledat klávesy Amiga a úkoly hostitele, přiřadit nové klíče, odstranit mapování, obnovit chyby, nebo jasné konflikty. Sloupec stavu uvádí, zda je každý úkol platný.

Levý sloupec pojmenuje emulovaný klíč Amiga; **Association** ukazuje kombinaci kláves pro hostitele. Platné mapování může být stále nepohodlné, pokud si Windows nebo aplikace vyhradí stejnou zkratku, takže otestujte kritické kombinace uvnitř běžícího stroje. Vyhněte se přiřazení myši-uvolnění nebo fullscreen zkratky na klíč, který emulovaný software potřebuje často.

### Myš

<p align="center"><img src="images/options-amiga-mouse-en.png" alt="Nastavení myši Amiga" width="72%"></p>

Nastavte fyzickou rychlost myši, vyberte, která analogová hůl ovládá myš, upravte analogově mrtvou zónu a rychlost a nastavte mapování akce myši. Obnovit závady nebo jasné mapování konfliktů v případě potřeby.

Zvyšte mrtvou zónu, pokud regulátor způsobí směrování ukazovátka. Při zapnutém obou klackách nastavte rychlost levého a pravého hole nezávisle. Nižší mapovací tabulka spojuje hostitelské vstupy s tlačítky myši nebo akce; kontroluje stav konfliktu po změně mapování ovladačů jinde.

### Kontroloři

<p align="center"><img src="images/options-amiga-controllers-en.png" alt="Nastavení regulátoru Amiga" width="72%"></p>

Detekovat připojené regulátory, přiřadit zařízení a typy regulátorů do portů Amiga a konfigurovat mapování regulátorů a nastavení turbodmychadla. Dostupné volby závisí na zjištěném hardwaru a zvoleném stroji.

Port 1 a Port 2 jsou konfigurovány nezávisle. **Automatický řadič** je rozumným výchozím bodem, ale software očekávající určitý joystick nebo myš může vyžadovat explicitní typ. Spustit detekci před přidělením nově připojeného regulátoru. Turbo oheň opakovaně aktivuje mapovaný vstup a měl by zůstat zakázán, pokud z něj hra nebo aplikace nevytěží.

## Diagnostika a údržba hardwaru

Tyto dialogy jsou otevřeny z karty **Tools **. Každé dialogové okno zobrazuje generovaný příkaz Greaseweazle. Před kliknutím na ** Execute**.

### Informace o ovladačích

<p align="center"><img src="images/tool-controller-information-en.png" alt="Informace o ovladačích" width="62%"></p>

Zobrazí informace hlášené zvoleným ovladačem. Expand **Syrový výstup**, když potřebujete kompletní odpověď na příkaz.

Použijte to jako první diagnostický příkaz. Úspěšná odpověď potvrzuje, že GW GUI může spustit nakonfigurovaný spustitelný Host Tools a komunikovat s vybraným zařízením. Před provedením aktualizace zaznamenejte informace o firmwaru a hardwaru.

### šířka pásma USB

<p align="center"><img src="images/tool-usb-bandwidth-en.png" alt="šířka pásma USB" width="62%"></p>

Měří dostupnou komunikační šířku USB. Použijte ji k diagnostice nestabilních přenosů nebo nevhodného spojení USB.

Zavřít jiný software pomocí regulátoru před testováním. Po změně portu USB, kabelu nebo náboje opakujte měření. Srovnejte výsledky za podobných podmínek spíše než jako absolutní záruku zacházet s jedním měřením.

### Rychlost diskové mechaniky

<p align="center"><img src="images/tool-drive-speed-en.png" alt="Rychlost diskové mechaniky" width="62%"></p>

Měří rychlost otáčení pohonu. Zvyšte počet měření, pokud potřebujete reprezentativnější výsledek.

Jedno měření je rychlá kontrola; několik měření ukazuje, zda je rychlost stabilní. Před interpretací výsledku nechte pohon dosáhnout normální rychlosti. Nečekaná hodnota může naznačovat špatně nastavenou rychlost, mechanickou otázku nebo problém s nastavením měření.

### Pohyb hlavy

<p align="center"><img src="images/tool-seek-head-en.png" alt="Pohyb hlavy" width="62%"></p>

Přesune hlavu pohonu do zvoleného válce. **Povolit extrémní tlakové láhve ** umožňuje normálně omezené polohy, a ** Keep motor aktivní** opustí motor běží během provozu. Použijte extrémní pozice pouze tehdy, když je hardware výslovně vyžaduje.

Normální vyhledávání je užitečné pro potvrzení pohybu hlavy nebo umístění před diagnostikou. Poslouchat abnormální opakované dopady a zastavit, pokud je požadovaný válec nevhodné pro pohon. Tento nástroj nečte ani neověřuje data v cílovém válci.

### Diagnostika seřízení pohonu

<p align="center"><img src="images/tool-drive-alignment-en.png" alt="Diagnostika seřízení pohonu" width="62%"></p>

Provádí opakované čtení pro analýzu zarovnání. Podporuje výběr trati, počet otáček a čtení, dekódování formátu, surový tok, index, rychlost, PLL, density- pin, hard-sector, TG43 a reverse- data. Práce na sladění vyžaduje odpovídající referenční média a znalosti hardwaru.

Začněte se známým referenčním diskem a nejmenší sadou přejezdů. **Střídavé koleje ** definuje stopy a hlavy, z nichž byly odebrány vzorky; ** Otáčky na kolej ** kontrolují každou dobu trvání vzorku; ** Počet čtení** určuje opakování. Povolit vlastní definici disku nebo dekódování formátu pouze v případě, že odpovídá referenčním médiím. Volby, jako je falešný index, tvrdé sektory, PLL overrides, hustota kolíky, a TG43 jsou pevně uskladněné nebo formát- specifické a mohou neplatnost srovnání, pokud se používá nesprávně.

### Hardwarové kolíky

<p align="center"><img src="images/tool-hardware-pins-en.png" alt="Hardwarové kolíky" width="62%"></p>

Čte nebo mění podporovaný řadicí pin. Vyberte pin, zapněte **Změnit pin ** pouze při zápisu hodnoty, a vyberte ** na vysoké úrovni**, pokud to vyžaduje zamýšlená hardwarová operace.

S **Změnit pin** deaktivován, příkaz dotaz pin. Tohle je bezpečnější výchozí hodnota. Změna úrovně přímo ovlivňuje regulátor I / O a měla by být provedena pouze se správnou hardwarovou dokumentací Greaseweazle a připojenou elektroinstalací.

### Resetovat regulátor

<p align="center"><img src="images/tool-reset-controller-en.png" alt="Resetovat regulátor" width="62%"></p>

Resetuje řadič Greaseweazle. Používejte toto, když je kontroler detekován, ale již neodpovídá normálně.

Vyčkejte, až bude veškerá aktivní operace na disku dokončena před resetováním. Poté znovu proskenujte ovladač, pokud se stav jeho spojení automaticky neobnoví. Reset neopravuje špatnou cestu `gw.exe` nebo odpojené zařízení USB.

### Zpoždění

<p align="center"><img src="images/tool-delays-en.png" alt="Zpoždění kontrolorů" width="62%"></p>

Čte nebo mění hodnoty času regulátoru, včetně výběru, stepu hlavy, vypořádání, motoru, automatického vyřazení, načasování zápisu a zpoždění indexové masky. Povolit pouze hodnoty, které hodláte upravit.

Nezaškrtnutá pole ponechají odpovídající hodnotu regulátoru beze změny. Před úpravou zaznamenejte existující hodnoty. Časové změny mohou ovlivnit každý následující fyzický provoz, takže test s postradatelnými médii a obnovit známé-dobré hodnoty, pokud se chování stane nespolehlivým.

### Firmware

<p align="center"><img src="images/tool-firmware-en.png" alt="Aktualizace firmwaru" width="62%"></p>

Aktualizuje regulátor firmware. **Aktualizovat bootloader** je výslovně označen jako riskantní a měl by zůstat zakázán, pokud to nevyžaduje oficiální firmware. Během aktualizace neodpojte ovladač.

Před aktualizací potvrďte připojený ovladač s informacemi o ovladači ****, použijte stabilní přímé spojení USB a uzavřete další software, který k němu může přístup. Po dokončení, znovu připojit nebo rescan regulátoru a znovu si jeho informace pro ověření hlášené verze firmware.

## Záznamy a historie operací

Otevřete historii operace pro kontrolu uložených záznamů pomocí operace.

<p align="center"><img src="images/operation-history-en.png" alt="Historie operací" width="68%"></p>

Pro zobrazení obsahu zvolte záznam vlevo. **Export** ukládá kopii pro diagnostiku nebo podporu. Cesty a příkazové řádky mohou obsahovat osobní jména adresářů, takže před jejich sdílením zkontrolujte vyvážené protokoly.

Živá konzole v hlavním okně ukazuje aktuální příkaz a nedávný výstup. Jeho kopírovací tlačítko kopíruje zobrazený text.

### Čtení deníku

Užitečný diagnostický deník obsahuje generovaný příkaz, časové razítka, výkon motoru a konečný stav. Práce zdola nahoru: identifikovat konečnou chybu, pak najít první varování nebo neúspěšnou stopu, která mu předcházela. Pozdní obecné selhání je často jen důsledkem dřívější, konkrétnější zprávy.

Při porovnávání dvou pokusů zkontrolujte, zda byl ovladač, pohon, motor, profil, zdrojová cesta, výstupní formát a odborné argumenty shodné. Jinak může jiný výsledek odrážet spíše změněné nastavení než nestabilitu disku.

## Údaje o aplikaci a přenosné použití

GW GUI udržuje uživatelská data oddělená od aplikačních binárů. V závislosti na zvoleném balíku a režimu jsou nastavení, protokoly, stažené nástroje, emulační komponenty, zachycení, stavy a konfigurace stroje uloženy buď v adresáři `Data` aplikace, nebo v konfigurovaných uživatelsky-datových lokalitách.

Před nahrazením nebo přesunutím přenosné instalace držte kompletní složku aplikace pohromadě a zálohujte složku `Data`. Nepřenášejte jednotlivé soubory z `lib`, protože aplikace z této struktury řeší své vlastní knihovny a knihovny třetích stran.

### Navrhl záložní obsah

Zálohovat následující, pokud jsou důležité pro váš pracovní postup:

- nastavení a profily aplikací;
- řízení a definice pohonu;
- konfigurace emulace;
- ROM cesty a právně držené ROM zálohy;
- obrázky z pevného disku a odnímatelných médií;
- zachytávání a ukládání;
- provozní záznamy používané jako záznamy o uchování.

Obrázky disků mohou být mnohem větší než nastavení. Uložení archiválních mistrů read- pouze pokud je to možné, a pracovat na kopie.

## Doporučené pracovní postupy

### Archivace neznámého disku

1. Prohlédněte a vyčistěte pohon vhodným postupem údržby.
2. Write - chránit disk, pokud je to možné.
3. Vyberte **Přečtěte si > Syrový obraz (SCP)**.
4. Použijte deskriptivní název souboru a přečtěte si normální rozsah dráhy s více otáčkami.
5. Zkontrolujte konzoli a uložený záznam.
6. Zkontrolujte obě strany v Placeholdera Vizualizace **.
7. Převést kopii do pravděpodobných sektorových formátů.
8. Otestujte převedené kopie v **Disk Explorer** nebo vhodném softwaru.
9. Zachovat syrového mistra, log a poznámky dohromady.

### Obnovení disku z obrazu

1. Prohlédněte obraz a potvrďte jeho očekávanou rodinu a formát.
2. Vložte postradatelný nebo záměrně zapisovatelný disk správné velikosti a hustoty.
3. Otevřete **Napište** a vyberte obrázek.
4. Potvrďte nakonfigurovaný disk a detekovaný formát.
5. Napište disk.
6. Přečti si to zpět na samostatný ověřovací obrázek.
7. Porovnejte dekódovaný obsah a zkontrolujte podezřelé stopy vizuálně.

### Vytvoření emulovaného Amiga

1. Otevřete **Options > Emulation > Configurations** a vytvořte nebo vyberte počítač.
2. V **Amiga > General** zvolte verzi modelu a emulátoru.
3. Přiřadit kompatibilní, legálně získané ROM.
4. Při prvním startu ponechejte vady modelu CPU a RAM.
5. Nastavit video a audio s konzervativním automatickým nastavením.
6. Přidat paměťová zařízení a připojit zkopírované mediální obrázky.
7. Recenze klávesnice, myši, a řídící úkoly.
8. Uložit konfiguraci.
9. Zpět na **Emulace **, vyberte jej a klikněte na ** Open**.
10. Pouze po úspěšném startu, změně zrychlení nebo pokročilém nastavení.

## Bezpečnostní kontrolní seznam

Před **Přečtěte si**:

- zdrojový disk je na správném disku;
- zdroj je pokud možno chráněn spisem;
- výstupní cesta nepřepíše existující master;
- profil a rozsah dráhy odpovídají disku.

Před **napsat ** nebo ** Vymazat**:

- cílový disk může být zničen;
- obrázek a pohon jsou správné;
- velikost a hustota disku jsou kompatibilní;
- jako cíl se nepoužívá žádný archivář.

Před nástrojem pro výměnu pevného skladu:

- žádný jiný provoz není spuštěn;
- je vybrán správný ovladač;
- byly zaznamenány současné hodnoty;
- regulátor má stabilní výkon a konektivitu USB;
- akce je podporována hardwarovou dokumentací.

## Řešení problémů

### Regulátor není uveden

1. Nastavit ovladač přímo do počítače.
2. Otevřete **Options > Controllers and drives**.
3. Klikněte na **Scan**.
4. Ověřte stav regulátoru a konfiguraci pohonu.
5. Spustit **Řídicí informace**, pokud detekce uspěje, ale příkazy selžou.

Pokud se stále neobjeví, zkuste jiný přímý port USB a kabel, pak rescan. Zkontrolujte Windows Device Manager pro nově detekované sériové zařízení. Regulátor viditelný pro Windows, ale chybějící v GW GUI obvykle ukazuje na rušný port, stalou konfiguraci nebo problém s Host Tools; regulátor chybějící ve Windows ukazuje na USB, výkon, ovladač nebo hardware.

### `gw.exe` nelze nalézt

Otevřete **Options > Controllers and drives** a použijte **Find gw.exe**, **Choose...** nebo **Download latest version**. Ověřte, že nalezená cesta ukazuje na požadovanou instalaci Greaseweazle.

Po jeho výběru spusťte informace o ovladači **. Pokud to selže před kontaktováním hardwaru, zkontrolujte záznam pro neplatnou spustitelnou cestu, chybějící soubory nebo verzi, která nemůže spustit.

### Operace používá špatný motor

Otevřete **Options > Engines** a zkontrolujte engine přiřazený dané operaci. GW GUI bez upozornění nepřepne na jiný engine.

Nastavení motoru je oddělené: změna konverzního motoru nemění čtení, zápis ani Disk Explorer. Znovu otevřete chybnou operaci po uložení volby a potvrďte generovaný příkaz v konzoli.

### Obrázek není rozpoznán

Zakázat automatickou detekci pouze tehdy, pokud znáte správný stroj a formát. V opačném případě zkuste kartu **Vizualizace** pro kontrolu obrazu na nižší úrovni.

Zkontrolujte, zda je zdrojem zachycení surového toku, obraz odvětví, stlačený kontejner nebo nesouvisející soubor s zavádějícím rozšířením. Nikdy nepřejmenujte rozšíření pouze na detekci síly; převod musí interpretovat strukturu zdroje správně.

### Emulace nezačne

Ověřte uloženou konfiguraci, nainstalovanou verzi emulátoru, zvolenou ROM, skladovací cesty a kompatibilitu modelu. Překontrolujte záznam aplikace pro kompletní podrobnosti o chybách.

Dočasně vrátit CPU, RAM, video a úložiště do jednoduché modelově kompatibilní výchozí hodnoty. Pokud začne základní nastavení, obnovte jedno nastavení najednou. Uložený stav vytvořený jinou verzí emulátoru nebo definováním stroje může selhat i v případě, že funguje čistý kufr.

### Zkratka nebo vstup nefunguje

Zkontrolujte jak globální emulaci **> Shortcut** page and the machine- specific klávesnice, myš, or controller page. Vyřešte všechny úkoly označené jako protichůdné.

Pokud je myš zachycena, použijte zkratku pro uvolnění zobrazenou v nástrojové liště running- machine. Pokud byl ovladač připojen po otevření Options, spusťte znovu detekci regulátoru před jeho přidělením.

### Příkaz nečekaně selže.

1. Přečtěte si výstup živé konzole.
2. Otevřete **Operation history** a zobrazte úplný uložený záznam.
3. Potvrďte vybraný ovladač, pohon, profil, motor a cesty souborů.
4. Exportovat příslušný záznam, pokud musí být sdílen pro diagnostiku.

### Zvukové praskání nebo pauzy

Zvyšte zvukovou latenci emulace, intenzivní aplikace CPU a vraťte přeskoky a zrychlení obrazu na jejich předchozí hodnoty. Ověřte, zda je vybráno zamýšlené zvukové zařízení Windows. Změňte jedno nastavení najednou, aby byla účinná oprava identifikovatelná.

### Displej emulace je prázdný nebo pomalý

Vrátit rozlišení a režim linky do **Automatic**, dočasně zakázat přeskakování rámů a nastavování blikačů a vyzkoušet dříve pracující přenašeč. Potvrďte, že nastavená a vložená bootovací média ROM jsou platná. Ukazatel FPS pomáhá rozlišovat problém s dosažením výkonnostního výkonu od stroje, který jednoduše nezabooted.

### Čtení obsahuje nestabilní stopy

Opakujte čtení do nového názvu souboru, případně zvyšte otáčky a porovnejte postižené stopy. Očistěte hlavice pohonu správným postupem a zkontrolujte, zda disk není fyzicky poškozen. Nečtěte opakovaně viditelně prolévající nebo poškozená média, protože další průchody mohou zhoršit.

## Glosář

| Termín | Význam v GW GUI |
|---|---|
| Regulátor | Hardwarové rozhraní Greaseweazle připojené přes USB |
| Jeď. | Fyzický disketový pohon připojený k regulátoru |
| Motor | Implementace vybraná k provedení operace |
| Flux | Časové informace představující magnetické přechody čteny z disku |
| Syrový obrázek | Zachycení uchovávající informace o disku na nízké úrovni, jako je SCP |
| Obrázek sektoru | Dekódované zastoupení organizované do logických sektorů |
| Revoluce | Jedna kompletní rotace vzorkované při čtení trati |
| Válec | Radiální poloha hlavy; jeden válec může obsahovat stopu na každé straně |
| Hlava | Strana disku vybraná fyzickým pohonem |
| Profil | Znovu použitelný soubor nastavení pro operaci |
| ROM | Obraz firmwaru požadovaný emulovaným strojem |
| Uložený stav | Snímek stavu stroje na ovládání emulátoru |
| Návratnost | Grafický backend používaný k zobrazení emulačního výstupu |

## Rychlý odkaz

| Jestli chceš... | Jdi... |
|---|---|
| Zachovat fyzický disk | **Čtení** |
| Vrať obrázek na disk. | **Zapsat** |
| Vytvořit jiný formát obrazu | **Převod** |
| Zkontrolovat stopy nebo anomálie toku | **Vizualizace** |
| Procházet soubory uvnitř obrazu | **Disk Explorer** |
| Kontrola komunikace regulátoru | **Nástroje > Informace o ovladačích** |
| Měření otáček mechaniky | **Tools > Drive speed** |
| Přezkum příkazu z minulosti | **Historie operací** |
| Nastavit hardware | **Možnosti > Regulátory a pohony** |
| Vybrat implementace | **Možnosti > Motory** |
| Vytvořit nebo upravit emulovaný stroj | **Možnosti > Emulace** |
| Spustit uložený stroj | **Emulace** |
