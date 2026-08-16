# GW GUI Felhasználói útmutató

GW GUI egy Windows alkalmazás olvasásra, írásra, konvertálásra, ellenőrzésre, és a floppy- disk képek emulálására. Képes irányítani. Greaseweazle hardver, dolgozz a lemez-kép fájlokat a belső motor, és fuss mentett emulated-gép konfigurációk.

Ez az útmutató az alkalmazás aktuális verziójában látható angol felületet írja le. A nyomtatható felhasználói kézikönyv forrásaként van megírva: a képernyőképek illusztrálják a vezérlőket, míg a környező szöveg elmagyarázza, hogy mit válasszunk, miért válasszunk, és hogyan ellenőrizzük az eredményt.

> **Fontos:** A lemez olvasása nem romboló. Írás, törlés, firmware frissítés, és néhány hardver eszközök módosíthatja a média vagy hardver. A kattintás előtt olvassa el a megfelelő eljáráshoz mellékelt figyelmeztetést ** Végrehajtás**.

### Hogyan kell alkalmazni ezt az útmutatót?

Ha ez az első alkalom, hogy GW GUI, teljes [Kezdődik](#getting-started), majd kövesse [Egy lemez olvasása](#reading-a-disk)Ha az alkalmazás már be van konfigurálva, menjen közvetlenül a művelet fejezetéhez. Az opciók fejezetei referenciaként szolgálnak, amikor az eljárás arra kéri Önt, hogy változtasson meg egy meghajtót, motort, profilt vagy emulatált gépet.

Az interfész nevei a következők: **félkövér**. Filenames, ösvények, parancsok, és szó szerinti értékek jelennek meg, mint `code`. Megjegyzés magyarázza a normál viselkedést; figyelmeztetések azonosítja műveletek, amelyek megváltoztathatják a lemez, vezérlő, vagy tárolt konfiguráció.

## Tartalom

1. [A munkafolyamat megértése](#understanding-the-workflow)
2. [Kezdődik](#getting-started)
3. [Fő ablak](#main-window)
4. [Lemezolvasás](#reading-a-disk)
5. [Lemezírás](#writing-a-disk)
6. [Lemezképek konvertálása](#converting-disk-images)
7. [Disk kép megjelenítése](#visualizing-a-disk-image)
8. [A lemez tartalmának feltárása](#exploring-disk-contents)
9. [A szerszámok használata](#using-the-tools)
10. [Emuláció](#emulation)
11. [Alkalmazási lehetőségek](#application-options)
12. [Emulációs lehetőségek](#emulation-options)
13. [Amiga konfiguráció](#amiga-configuration)
14. [Hardware diagnosztika és karbantartás](#hardware-diagnostics-and-maintenance)
15. [Bejelentkezések és műveleti előzmények](#logs-and-operation-history)
16. [Alkalmazási adatok és hordozható használat](#application-data-and-portable-use)
17. [Ajánlott munkafolyamatok](#recommended-workflows)
18. [Biztonsági ellenőrző lista](#safety-checklist)
19. [Hibaelhárítás](#troubleshooting)
20. [Glosszárium](#glossary)
21. [Gyors hivatkozás](#quick-reference)

## A munkafolyamat megértése

GW GUI elválasztja a fizikai lemezen végzett műveleteket az image-file műveletektől:

| Cél | Bemenet | Kimenet | Ajánlott oldal |
|---|---|---|---|
| Tartson fenn egy floppy lemezt | Fizikai lemez | Képfájl | **Olvassa el** |
| Egy floppy lemez helyreállítása | Képfájl | Fizikai lemez | **Írás** |
| Képformátum módosítása | Képfájl | Egy vagy több képfájl | **Átalakítás** |
| Vizsgálópályák és anomáliák | Képfájl | Vizuális analízis | **Visualization** |
| A képben tárolt fájlok böngészése | Támogatott kép- / fájlrendszer | Fájlok és könyvtárak | **Disk Explorer** |
| A meghajtó vagy vezérlő diagnosztizálása | Greaseweazle hardver | Mérések vagy állapotok | **Szerszámok** |
| Mentett virtuális gép futtatása | Mentett gépkonfiguráció | Emulációs munkamenet | **Emuláció** |

A megőrzés, először, hogy egy nyers fogás, és tartsa változatlan, mint egy mester. Hozzon létre átalakított vagy javított munkapéldányokat a mestertől. Ez elkerüli a fizikai olvasást, és megőrzi azokat az információkat, amelyeket az ágazati formátum nem őrizhet meg.

## Kezdődik

### Követelmények

- Ablakok a Microsoft .NET Asztali futásidő szükséges az alkalmazás.
- A Greaseweazle vezérlő a fizikai floppy- lemez műveletek.
- A beállított út `gw.exe` amikor a Greaseweazle Host Tools motor.
- Jogilag előállított ROM akták, amikor egy emulált gépnek szüksége van rájuk.

Az alkalmazás ellenőrzi a szükséges .NET futási idő indításakor. Ha hiányzik, kövesse a telepítési parancsot, majd indítsa újra GW GUI.

### A hardver csatlakoztatása előtt

A fizikai lemezen végzett művelet előtt ellenőrizze a következőket:

1. Csatlakoztassa a Greaseweazle Vezérlő egy stabil USB kikötő.
2. Csatlakoztassa a floppy kábelt a megfelelő tájolással.
3. Csatlakoztassa a meghajtó energiaellátását, mielőtt értékes médiát helyez be.
4. Erősítse meg, hogy a meghajtó mérete és sűrűsége megegyezik a lemezzel.
5. Írás- védje a forráslemezt, ha lehetséges.

GW GUI nem akadályozhatja meg a hibás kábelezés, a nem megfelelő teljesítmény vagy a mechanikusan nem biztonságos hajtás okozta károkat. Először teszteljünk ismeretlen hardvert feláldozható lemezzel.

### Első kilövés

1. Megnyitás `gwgui.exe`.
2. Megnyitás **Opciók**.
3. In **Vezérlő és meghajtó** Keresse meg a vezérlőt, és állítsa be a meghajtót.
4. Az út ellenőrzése vagy kiválasztása `gw.exe`.
5. In **Motorok**, válassza ki, melyik motort kell üzemeltetni.
6. Térjen vissza a fő ablakhoz, és válassza ki a szükséges műveleti lapot.

### Megerősítve, hogy a beállítás kész

A munkabeállítás kell mutatni a vezérlő és a meghajtó az állapotsorban, például a meghajtó száma, mérete, sűrűsége, és COM kikötő. In **Opciók > Vezérlő és meghajtó **, az adatkezelő kell jelölni ** Rendelkezésre álló ** és a meghajtó ** Beállítások **Fuss! ** Adatkezelő** az értékes média elolvasása előtt, ha a kommunikációt korong megváltoztatása nélkül szeretné ellenőrizni.

### Motor kiválasztása

GW GUI egyes műveletek esetében egynél több végrehajtást fedhet fel. A **Greaseweazle Host Tools** a motor a beállított `gw.exe`; a belső GW GUI az alkalmazáson belüli támogatott műveletek motorkezelői. A motor kiválasztása egyértelmű és független az olvasáshoz, íráshoz, átalakításhoz, és Disk Explorer. Ha egy műveletet a kiválasztott motor nem támogat, GW GUI Ezt az állapotot a motor automatikus módosítása helyett jelenti.

## Fő ablak

A fő ablak csoportosítja a fő műveletek hét fülek:

- **Olvassa el** létrehoz egy képet egy fizikai lemezről.
- **Írás** egy képet ír egy fizikai lemezre.
- **Átalakítás** egy vagy több kimeneti formátumot alakít át egy lemezképformátumba.
- **Visualization** pályák és fluxus vagy dekódolt adatok megjelenítése.
- **Disk Explorer** a támogatott fájlrendszerek és lemeztartalom böngészése.
- **Szerszámok** hardver karbantartási és diagnosztikai parancsokat biztosít.
- **Emuláció** kezeli és futtatja a mentett emulált gépeket.

Az alsó konzol mutatja a végrehajtásra kerülő parancsot és annak kimenetét. Az állapotsor jelenti a kiválasztott meghajtót, profilt és aktuális állapotot.

### Az interfész olvasása

A legtöbb művelet oldal ugyanazt a mintát követi:

1. **Forrás vagy rendeltetési hely** a vezérlő azonosítja a lemezt, képet vagy mappát.
2. **Formátum-vezérlés** Válassza ki az automatikus felismerést vagy egy explicit gépet és formátumot.
3. **Profilvezérlés** újra használható beállításokat alkalmazzon.
4. **Speciális beállítások** a normál körülmények között választható paraméterek feltárása.
5. **Végrehajtás** Megkezdjük a műveletet.
6. A **konzol** mutatja a generált parancsot, előrehaladást, figyelmeztetéseket és hibákat.

A **Végrehajtás** a gomb nem jelenti azt, hogy minden érték biztonságos a beillesztett lemezre. Mindig ellenőrizze a cél és a kiválasztott meghajtó előtt írási vagy karbantartási művelet.

### Status bar és konzol

Az állapotsor bal oldala azonosítja az aktív fizikai meghajtót. A központ az aktív profilt mutatja, amikor kiválasztják. Az állami mutató jelenti, hogy az alkalmazás készen áll vagy foglalt. A konzol nem csupán diagnosztikai: ez a hiteles feljegyzése a parancs küldött a kiválasztott motor. Használd a másolat vezérlését, ha meg kell őrizned vagy meg kell osztanod a parancsot.

## Egy lemez olvasása

A **Olvassa el** tab, hogy rögzítse a fizikai floppy lemez, mint egy kép.

<p align="center"><img src="images/main-read-en.png" alt="A lap olvasása" width="78%"></p>

### Alapeljárás

1. Helyezze be a forráslemezt a beállított meghajtóba.
2. Válassza ki a kép típusát:
   - **Nyers kép (SCP)** megőrzi a fluxszintű információkat.
   - **Ismert lemezformátum** létrehoz egy képet egy kiválasztott gép és formátum.
3. Válassza ki a célmappát.
4. Adja meg a kimeneti fájlnevet.
5. Válassza ki a profilt, ha szükséges.
6. Kattintson ide **Végrehajtás**.

A konzol mutatja a pontos parancsot és haladást. Ne távolítsa el a lemezt, és ne távolítsa el a vezérlőt, amíg a művelet be nem fejeződött.

### A kimeneti típus kiválasztása

Felhasználás **Nyers kép (SCP)** ha a cél az archívum rögzítése, elemzése, visszanyerése vagy későbbi átalakítása. A nyers kép rögzíti időzítése információk és többszörös forradalmak, amely hasznos a szokatlan formátumok, gyenge ágazatok, védelmi rendszerek, és sérült média.

Felhasználás **Ismert lemezformátum** ha már ismeri a lemez család, és szüksége van egy közvetlenül használható ágazati képet. Ez a választás lehet kisebb és könnyebb megnyitni más szoftver, de ez képviseli a dekódolt eredményt, nem minden részlet megfigyelt a meghajtó.

Ha bizonytalan, először készítsd el a nyers képet. Később átalakíthatja anélkül, hogy újra elolvasná a lemezt.

### Mappa, fájlnév és profil

A **Mappa ** a célkönyvtár. A ** Fájlnév** azonosítania kell a lemezt anélkül, hogy kizárólag a fizikai címkéjére támaszkodna. A hasznos archiválási név tartalmazza a címet, a lemezszámot vagy az oldalt, és adott esetben egy feltételjegyzetet. Ne adjon hozzá olyan formátumkiterjesztést, amely ütközik a kiválasztott kimeneti formátummal.

A **Profil ** a mentett olvasási paramétereket alkalmazza. Csak akkor válasszon egyet, ha tudja, mit tartalmaz. A ** Alapértelmezés** a profil megfelelő a normál első kísérlethez; a speciális helyreállítási profil szándékosan több fordulatot vagy más pályatartományt is leolvashat, és így tovább tart.

### Speciális beállítások

Kibontás **Speciális beállítások** a formátspecifikus vagy szakértői paraméterekhez való hozzáférés. Ezeket az értékeket változatlanul hagyjuk, hacsak a lemez nem igényel egy adott sávtartományt, forradalmi számot vagy vezérlő opciót.

Gyakori fejlett értékek:

| Beállítás | Cél | Mikor kell megváltoztatni? |
|---|---|---|
| Nyomtáv | Korlátozza a hengerek és fejek olvasni | Egyoldalas média, szokatlan geometria vagy célzott helyreállítási engedély |
| Források | Ellenőrzi, hogy hány rotációból vesznek mintát | Instabil vagy védett vágányokra vonatkozó növelés; szükség esetén csak a sebességre vonatkozó csökkentés |
| Szakértői érvek | További motorparaméterek | Csak akkor, ha dokumentálva van Greaseweazle iránymutatás |

### A sikeres olvasmány ellenőrzése

Ne csak a hibaablak hiányára hagyatkozzon. A parancs befejezése után:

1. Ellenőrizze, hogy a kimeneti fájl létezik, és nem üres.
2. Olvassa el a végső konzol vonalak nem sikerült vagy hiányzik számokat.
3. A kép megnyitása **Visualization** annak ellenőrzése, hogy mindkét oldal és a várt pályatartomány tartalmaz-e adatokat.
4. Nyisd ki! **Disk Explorer** ha a fájlrendszer támogatott.
5. Tartsa a műveleti napló fontos archívum rögzítések.

Ha az ismételt olvasatok eltérnek egymástól, akkor az egyes nyers fogásokat az első átírás helyett meg kell őrizni. A különbségek hasznosak lehetnek a felépülés során.

## Lemezírás

A **Írás** A lap egy létező képet ír egy fizikai floppy lemezre.

<p align="center"><img src="images/main-write-en.png" alt="A lap írása" width="78%"></p>

### Alapeljárás

1. Helyezze be a céllemezt.
2. Válassza ki a forrás képet **Böngészés**.
3. Erősítse meg az észlelt formátumot.
4. Válassza ki a profilt, ha szükséges.
5. Kattintson ide **Végrehajtás**.

Az írás helyettesíti a céllemez adatait. Az indítás előtt ellenőrizze a kijelölt meghajtót és képet.

> **Figyelem:** Az írás pusztító. Mágneses adatokat helyettesít a céllemezen. Lehetőség szerint írásvédett forrásarchívumot és külön céllemezt használjon.

### Írás előtt

Négy elem ellenőrzése a kattintás előtt **Végrehajtás**:

1. **Kép:** a kijelölt útvonal a tervezett forráskép.
2. **Lemez:** a meghajtóban lévő lemez biztonságosan felülírható.
3. **Indítás:** a beállított méret és sűrűség megfelel a célközegnek.
4. **Formátum:** automatikus felismerés vagy a manuálisan kiválasztott formátum megfelel a képnek.

Ha a forrás képet nem tesztelték, nyissa ki **Visualization ** vagy ** Disk Explorer** Először. Egy sikeres írás nem javít meg egy hiányos forrásképet.

### A vágány vizsgálata és módosítása

A kép kiválasztása után **A számok megjelenítése ** megnyitja a pálya ábrázolását. ** Módosítás** A támogatott képmódosítást az írás előtt teszi közzé. A rendelkezésre álló műveletek a kiválasztott formátumtól és motortól függnek.

### Írásos lemez ellenőrzése

Amikor a motor támogatja a hitelesítést, használja a fontos média. Ellenkező esetben olvassa vissza az írott lemezt egy új képre, és hasonlítsa össze dekódolt tartalmát, vagy vizsgálja meg **Visualization**. Tartsa a hitelesítést elkülönítve az eredeti képtől, hogy az eredeti ne legyen felülírva.

Ha az írás nem sikerül a konzisztens síneken, ellenőrizze a lemez állapotát, sűrűségét, a meghajtó tisztaságát és a meghajtó konfigurációját. Ha a hiba véletlenszerűen történik, ellenőrizze USB stabilitás és vezérlő kommunikáció.

## Lemezképek konvertálása

A **Átalakítás** A lap a forrásképet egy vagy több célformátummá alakítja.

<p align="center"><img src="images/main-conversion-en.png" alt="Átalakító lap" width="78%"></p>

### Alapeljárás

1. Válassza ki a forrás képet.
2. Opcionálisan adja meg a kimeneti neveket.
3. Válassz egy gépcsaládot.
4. Válasszon ki egy vagy több kimeneti formátumot és kiterjesztést.
5. Beállítás **Címkék hozzáadása** ha a fájlneveknek a beállított címkemintát kell használniuk.
6. Kattintson ide **Végrehajtás**.

A **Kiválasztott ** A panel felsorolja a kért kimeneteket. ** Fájl migráció** biztosítja a támogatott fájlok átviteléhez szükséges munkafolyamatokat, ahelyett, hogy standard képátalakítást végezne.

### A formátumok kiválasztása

A **Gép ** list szűrők a formátumok látható a ** Formátum** panel. A formátum neve a logikai lemez elrendezését írja le; a kiterjesztés a kimeneti tartályt írja le. Egyes formátumokat egynél több kiterjesztés is képviselhet, és egyes konténerek nem képesek megőrizni a nyers forrás minden jellemzőjét.

Válassza ki a ténylegesen szükséges kimeneteket. Több formátum hasznos, ha létrehoz egy archívum mester, emulátorkompatibilis másolat, és egy másolat egy másik elemzési eszköz egy művelet.

### Kimeneti név és címkék

**Kimeneti nevek ** lehetővé teszi a kiválasztott formátumokhoz létrehozott alapnevek ellenőrzését. ** Címkék hozzáadása ** alkalmazza a megadott fájlnév mintát ** Opciók > Általános**. Címkék lehet kódolni család, formátum, kiterjesztés, dátum, vagy idő. Preview the example in Options before converting a large batch so that files are named continually.

### A konverziós eredmények ellenőrzése

Minden egyes kimenet esetében:

1. Erősítse meg, hogy létrehoztak egy fájlt.
2. Ellenőrizze a konzolt olyan sávok vagy szektorok után, amelyeket nem lehet dekódolni.
3. Az eredmény megnyitása **Disk Explorer** ha támogatott fájlrendszert tartalmaz.
4. Hasonlítsa össze a várt lemezt és tartalmát a forrással.

Az átalakítás a célformátumból eredő adatvesztés bejelentése közben is elvégezhető. Tartsa meg az eredeti nyers képet akkor is, ha az átalakított kép helyesnek tűnik.

## A lemezkép megjelenítése

A **Visualization** A lap megjeleníti a kép szerkezetét és adateloszlását.

<p align="center"><img src="images/main-visualization-en.png" alt="Visualization lap" width="78%"></p>

1. Kattintson ide **Lemezkép megnyitása**.
2. Tartsa **Automatikus észlelés** engedélyezése, vagy válassza ki a gép és a formátum kézzel.
3. Felhasználás **Link zoom** mindkét oldalt azonos zoom szinten tartani.
4. Felhasználás **Újraindítás** visszaállítani az eredeti nézetet.
5. Megnyitás **Felügyelő** részletes információk a kiválasztott régióról.

A legenda különbséget tesz a normál fluxus, rövid és hosszú átmenetek, fejlécek, dekódolt adatok, és észlelt anomáliák. A nyers kép tartalmazhat olyan adatokat, amelyeket nem lehet dekódolni egy ismert fájlrendszerbe, de itt még mindig ellenőrizhető.

### A nézet értelmezése

Minden nagy kör alakú panel egy lemezoldalt jelöl. A központ azonosítja az oldalt és annak aktuális adatállapotát; a koncentrikus pozíciók megfelelnek a pályáknak. A színek a felfedezett régiókat a legenda szerint osztályozzák. A vizualizáló az alábbi kérdésekre kíván válaszolni:

- A kép az egyik vagy mindkét oldalon tartalmaz adatokat?
- A várt nyomok jelen vannak?
- Az anomáliák elkülönülnek vagy ismétlődnek a lemezen?
- Az automatikus felismerés azonosított egy hihető gépet és formátumot?

Az anomália színe ok a régió ellenőrzésére, nem bizonyíték arra, hogy a lemez használhatatlan. Másolat védelem, nem szabványos formázás, gyenge rögzítés, és a sérült szektor képes különböző struktúrákat létrehozni, amelyek kontextuális értelmezést igényelnek.

### Ajánlott vizsgálati sorozat

Kezdje a kapcsolódó zoom lehetővé teszi, hogy hasonlítsa össze mindkét oldalon azonos skálán. Válassza ki a gyanús régió, nyitott **Felügyelő**, és hasonlítsa össze a szomszédos pályákkal. Ha az eredmény észlelési problémának tűnik, tiltsa le az automatikus észlelést, és válasszon egy ismert gépet és formátumot. A vizsgálat után térjen vissza az automatikus érzékeléshez, így az erőltetett beállítást nem használják véletlenül más képhez.

## A lemez tartalmának feltárása

A **Disk Explorer** A tab böngésző fájlhierarchiaként támogatott lemezképeket.

<p align="center"><img src="images/main-disk-explorer-en.png" alt="Disk Explorer tab" width="78%"></p>

1. Meglévő kép megnyitása vagy lemez elolvasása.
2. Tartsa **Automatikus észlelés** engedélyezve, hacsak nem kell kényszeríteni egy gépet vagy formátumot.
3. Tekintse át a volumenre vonatkozó információkat: rendszer, védelem, fájlrendszer, kapacitás, szabad tér és tételszám.
4. Böngészés könyvtárak a bal panel.
5. Válasszon ki egy elemet, hogy megtekinthesse annak részleteit a jobb panelben.

Ha a képformátum vagy fájlrendszer nem támogatott, használja **Visualization** a nyers szerkezet vizsgálata helyett.

### A panelek megértése

A felső összegzés leírja a szerelt képet és az észlelt térfogatot. A bal alsó panel tartalmazza a könyvtár hierarchiáját. A központi táblázat felsorolja a kiválasztott könyvtárba tartozó tételeket név, módosítási dátum, típus és méret szerint. A jobb oldali panel a kiválasztott elem részleteit mutatja.

Disk Explorer nem jelenti azt, hogy minden nyers vágányt tökéletesen dekódoltak. Használja a kötet összefoglalót és a tétel száma, mint egy gyors valószínűség ellenőrzés, majd nyissa meg a reprezentatív fájlokat, vagy hasonlítsa össze őket egy ismert könyvtár felsorolás, ha a megőrzés pontossága számít.

### Amikor semmi sem jelenik meg

Először is erősítsd meg, hogy a képút helyes. Ezután ellenőrizze az észlelt gép és formátum. Egy érvényes kép tartalmazhat egy nem támogatott vagy sérült fájlrendszert, amely esetben a felfedező üres maradhat, még akkor is, ha **Visualization** mutatja a rögzített adatokat. Ne írja felül vagy dobja el a forrás képet csak egy üres felfedező.

## Az eszközök használata

A **Szerszámok** Tabletta csoportok Greaseweazle karbantartási műveletek.

<p align="center"><img src="images/main-tools-en.png" alt="Szerszámlap" width="78%"></p>

Válasszon egy parancsot a bal oldali listáról, vizsgálja felül a paramétereket, majd kattintson a **Végrehajtás**. Destruktív vagy hardver-változó parancsok csak a kiválasztott vezérlő és meghajtó ellenőrzése után használhatók.

A legtöbb eszköz párbeszédalgák három terület: paraméterek a tetején, egy állapot és raw- kimeneti terület a központban, és a generált parancs az alján. Az opciók engedélyezésével a parancs előnézete változik. Az ellenőrizetlen paraméter általában azt jelenti, hogy "ne módosítsa ezt az értéket", míg az ellenőrzött paraméter ezt az értéket tartalmazza a parancsban.

Az egyéni diagnosztikai párbeszédek leírása [Hardware diagnosztika és karbantartás](#hardware-diagnostics-and-maintenance).

## Emuláció

### Mentett gép megnyitása

A **Emuláció ** tab listák mentett konfigurációk. Válasszon egyet és kattintson ** Megnyitás**Minden futó gép a saját fülében jelenik meg.

<p align="center"><img src="images/main-emulation-welcome-en.png" alt="Emulációs üdvözlő képernyő" width="78%"></p>

Gépek létrehozása és szerkesztése **Opciók > Emuláció > Beállítások ** és ** Opciók > Emuláció > Amiga**.

Ha nincs konfigurálás, akkor először készíts egyet a Beállítások mezőben. A mentett konfiguráció kombinálja a gép modell, emulátor verzió, ROM, memória, videó, hang, tárolás, és bemenet feltérképezések. A konfiguráció mentése nem indul el; visszatérés a fő **Emuláció ** tab és kattintás ** Megnyitás**.

### Futómű vezérlése

<p align="center"><img src="images/main-emulation-running-en.png" alt="Mozgásszimulátor" width="78%"></p>

A Running- machine eszköztár áramellátást, szünetet, reset, save-state, load-state, capture és kijelző vezérlőket biztosít. Azt is mutatja, hogy:

- a beállított quick-save és quick-load rövidítések;
- az aktív renderelő, mint például Direct3D 11;
- a teljes képernyő és a szájfelszabadulási rövidítések;
- audió, vezérlő és egér állapot;
- a jelenlegi felbontás, frissítési sebesség és keretarány.

Az emulációs kijelző alján lévő lemezcsík minden egyes emulált meghajtáshoz eltávolítható adathordozót kezel. Billentyűzet feladatok lehet változtatni **Opciók > Emuláció > Rövidítések**, míg az emulált billentyűzet, egér, és vezérlő térképek vannak konfigurálva a megfelelő Amiga Tabs.

### Eszköztár hivatkozási száma

| Ellenőrzési csoport | Cél |
|---|---|
| Energia és szünet | Indítja, állítja, szüneti vagy újraindítja az emulált gépet |
| A kezelőszervek újraindítása | A beállított puha vagy kemény újraindítási művelet végrehajtása |
| Állami ellenőrzések | Az emulátor állapotának megőrzése a gyors folytatáshoz |
| Fogás | Az emulált kijelző képének mentése |
| Megjelenítés | A kijelző megjelenítésének módosítása vagy a teljes képernyőre való bejutás |
| Gyors állapot emlékeztető | Az aktív mentési / betöltési parancsfájl megjelenítése |
| Rendező | Jelentés az aktív videó backend |
| Bemeneti emlékeztető | Teljes képernyő és egérkioldó rövidítések megjelenítése |
| Eszközmutatók | Jelentések audio, vezérlő, és egér állapot |
| Teljesítmény | Jelentések kimeneti méret, frissítési gyakoriság és képkocka sebesség |

### Teljes képernyő elhagyása vagy az egér elengedése

Az eszköztár a kijelölt billentyűket jeleníti meg. Az illusztrált konfigurációban **Alt + Visszatérés ** a teljes képernyőre és ** F12** kiadja az egeret. A megjelenített értékeket tekintélyesnek kell tekinteni, mert a rövidítések áthelyezhetők.

### Floppy média használata

A meghajtó szalag azonosítja az egyes emulált meghajtókat, mint például `DF0:`. Használja a média vezérlők behelyezni, pótolni, vagy katapult egy képet. A média helyettesítése csak a futó gép beépített lemezét változtatja meg; a mentett gépben nem változtatja meg a storage- eszköz meghatározását, kivéve, ha ezt a műveletet kifejezetten elmentik.

## Alkalmazási lehetőségek

Megnyitás **Opciók** a főablaktól az alkalmazás beállításához.

### Általános

<p align="center"><img src="images/options-general-en.png" alt="Általános lehetőségek" width="72%"></p>

A **Általános** A lap tartalma:

- az alapértelmezett lemezkép mappa;
- interfész nyelve és témája;
- átkonvertáláshoz használt filename- tag generáció;
- előre meghatározott és friss egyedi címkeminták;
- egy élő fájlnév példa.

Tag változók közé tartozik a forrás neve, család, formátum, kiterjesztés, dátum és idő. Az alapértelmezett minta visszaállításához használja az újraindító gombot.

A fájlnév preview frissítések, mielőtt bármilyen fájl jön létre. A kettős elválasztók, hiányzó kiterjesztések vagy kétértelmű nevek észlelésére használja. A legújabb egyedi minták gyors hozzáférést biztosítanak a korábbi elnevezési rendszerekhez anélkül, hogy helyettesítenék a jelenlegi beállítást.

### Naplók

<p align="center"><img src="images/options-logs-en.png" alt="Naplóbejegyzések" width="72%"></p>

A naplózást minden művelethez önállóan lehet beállítani. Minden kategória esetében válassza ki, hogy mentse-e a naplókat, állítsa be a maximális fájlméretet, és határozza meg, hogy a korábbi naplókat meg kell-e őrizni. A méret `0` Azt jelenti, korlátlan. **A mappa megnyitása** megnyitja az aktuális naplókönyvtárat.

Beállítás **Korábbi naplók vezetése** megőrzési és diagnosztikai munkálatok, ahol a történelem több kísérlet számít. Hatástalanítsa, ha csak a legújabb eredmény hasznos. A maximális mérethatárok a log tárolásra vonatkoznak, nem a rögzített lemezképekre.

### Vezérlő és meghajtó

<p align="center"><img src="images/options-controllers-and-drives-en.png" alt="Vezérlő és meghajtó" width="72%"></p>

Ezt a lapot használja:

- összekapcsolt vezérlők keresése;
- a meghajtó konfigurációinak hozzáadása és eltávolítása;
- Válassza ki a hajtómű méretét, sűrűségét és sebességét;
- a hardverbeállítások mentése;
- kiválasztja vagy automatikusan megtalálja `gw.exe`;
- ellenőrzés és letöltés Greaseweazle Host Tools frissítések;
- visszaállítja a korábban beállított futtatható útvonalat.

A elmentett hardverbeállítások akkor is elérhetőek maradnak, ha a meghajtó átmenetileg le van kapcsolva.

#### Drive hozzáadása

1. Kattintson ide **Keresés** és várjuk meg, hogy megjelenjenek az összekötött vezérlők.
2. Kattintson ide **Egy meghajtó hozzáadása** ha az előírt hajtómű még nincs felsorolva.
3. Válassza ki a logikai hajtási számot, a fizikai méretet, a felvételi sűrűséget és a forgási sebességet.
4. Mentsd meg a sort.
5. Erősítse meg, hogy megmutatja **Rendelkezésre álló ** és ** Beállítások**.

A hulladékkezelő csak a mentett konfiguráció eltávolítására használható; nem távolítja el a hardvert. Ha ugyanaz a vezérlő jelenik meg egy másik COM port később, scan újra feltételezve, hogy a tárolt port még érvényes.

#### Kezelés Greaseweazle Host Tools

**Keresés gw.exe ** Kutassák át az ismert helyeket. ** Válasszon ** Kiválaszt egy adott végrehajthatót. ** A frissítések ellenőrzése ** lekérdezések a rendelkezésre álló verziók helyett a telepített. ** A legújabb verzió letöltése ** beállítja a kiválasztott aktuális csomagot, és ** Korábbi útvonal használata ** visszaállítja a korábbi beállított helyet. A programfájl módosítása után fusson ** Adatkezelő** annak megerősítése, hogy a kiválasztott verzió kommunikálni tud a vezérlő.

### Motorok

<p align="center"><img src="images/options-engines-en.png" alt="A motor kiválasztása" width="72%"></p>

Válassza ki a motort függetlenül olvasás, írás, átalakítás, és Disk Explorer. A kiválasztott motort szigorúan használják: ha nem tudja végrehajtani a kért műveletet, GW GUI a korlátozást jelenti, ahelyett, hogy csendben kapcsolná a motorokat.

Ez a függetlenség szándékos. Például a fizikai olvasatok használhatják Greaseweazle Host Tools míg a képátalakítás és a feltárás a belső motort használja. A motorválasztásokat profilba vagy projektfeljegyzésbe kell rögzíteni, ha a reprodukálhatóság számít.

### Profilok

<p align="center"><img src="images/options-profiles-en.png" alt="Profilok" width="72%"></p>

Profilok tároló újra használható beállítások olvasási, írási és konverziós műveletek. Válassza ki a megfelelő kategóriát a profilok kezeléséhez. A kiválasztott profil megjelenik a főablak állapotsorában és az operációs képernyőkben.

Használjon profilokat megismételhető munkafolyamatokhoz, nem pedig a szakértői zászlók megmagyarázhatatlan gyűjteményeként. Minden profilnak adjon egy célzott nevet, például egy bizonyos meghajtót, lemezcsaládot vagy helyreállítási módszert. Felülvizsgálni egy profilt a mögöttes motor frissítése után, mert a támogatott opciók változhatnak.

## Emulációs lehetőségek

A **Emuláció** Az opciók általános tárolási beállításokat, globális rövidítéseket, elmentett konfigurációkat és gépspecifikus beállításokat tartalmaznak.

### Általános emulációs mappák

<p align="center"><img src="images/options-emulation-general-en.png" alt="Általános emulációs lehetőségek" width="72%"></p>

Állítsa be a megosztott emuláció tároló mappát és az alapértelmezett mappákat a rögzítéshez és a mentéshez. **A mappa megnyitása** megnyitja a megosztott helyet File Explorer.

Tartsa a fogások és mentett államok külön mappák. A rögzítés egy közönséges kép; a mentett állapot tartalmaz emulátor-specifikus gép állapotát, és függhet az emulátor változata és konfigurációja, amely létrehozta. Erősítse a konfigurációt és a médiát a fontos mentett államok mellett.

### Globális rövidítések

<p align="center"><img src="images/options-emulation-shortcuts-en.png" alt="Emulációs rövidítések" width="72%"></p>

Keresés egy művelet vagy kulcsfontosságú hozzárendelés, hozzárendelése vagy eltávolítása rövidítések, visszaállítja az alapértelmezett, és egyértelmű konfliktusok. A státus oszlop azonosítja az érvényes és ellentmondásos megbízásokat.

A rövidítés megváltoztatásához keresse meg a műveletet, kattintson a **Kijelölés **, és nyomja meg a kívánt kulcskombinációt. Ellenőrizze az állapotot a Beállítások lezárása előtt. ** Egyértelmű konfliktusok ** Eltávolítja az egymásnak ellentmondó feladatokat; nem állítja vissza az alapértelmezett feltérképezést. Felhasználás ** Alapértelmezések visszaállítása** amikor az egyéni feladatokat a standard készletre szeretné cserélni.

### Mentett konfigurációk

<p align="center"><img src="images/options-emulation-configurations-en.png" alt="Mentett emulációs konfigurációk" width="72%"></p>

Ez az oldal a mentett gépeket sorolja fel. Válassza ki a beállítást szerkeszteni a **Amiga** Tab. Frissítheti a listát vagy törölheti a kijelölt konfigurációt.

A konfiguráció törlése eltávolítja a mentett gép definícióját. Nem használható média kilövésére vagy futtató gép bezárására. A törlés előtt fel kell jegyezni ROM, merevlemezes kép, és a konfigurációhoz kapcsolódó fájlokat.

## Amiga konfiguráció

Az aktuális interfész részletes Amiga konfigurációs oldalak. Ugyanez a beállítási struktúra kiterjeszthető más emulált rendszerekre a fő munkafolyamat megváltoztatása nélkül.

### Általános

<p align="center"><img src="images/options-amiga-general-en.png" alt="Amiga általános beállítások" width="72%"></p>

Válassza ki a Amiga modell, mentse a konfiguráció, telepítse vagy cserélje az emulátor verziót, és határozza meg az alapértelmezett mappák a merevlemezek és más média. **Keresés** lekérdezi a hivatalos emulátor-verzió forrást.

Kezdje a modellel, mert később már nem fog menni. Módosítása megváltoztathatja a rendelkezésre álló CPU, memória, ROMa chipset és a tárolási lehetőségek. Az emulátor verziójának kiválasztása után mentse el a konfigurációt, mielőtt elindítaná a főablakból. Egy másik emulátor verzió telepítése helyettesíti a konfiguráció által használt verziót; nem hoz létre egy második másolatot a gép.

### CPU

<p align="center"><img src="images/options-amiga-cpu-en.png" alt="Amiga CPU beállítások" width="72%"></p>

A CPU az oldal a gépmodell által kiválasztott processzort mutatja, és kompatibilis precizitást biztosít, FPUés gyors döntéseket. A kiválasztott modellre nem alkalmazandó lehetőségek továbbra is tiltva maradnak.

- **CPU modell** azonosítja az emulált processzort.
- **Pontosság** irányítja az időzítési modellt. Cycle- pontos módok előnyben részesítik a hardver kompatibilitást, de több host feldolgozást igényelnek.
- **FPU** lehetővé teszi a kompatibilis lebegéspont egység, ha támogatott.
- **CPU sebesség** Válassza ki az eredeti időzítést vagy a gyorsított üzemmódot.

Alapkonfigurációhoz tartsa meg a modelszármazékot CPU és eredeti sebesség. A gyorsulást csak azután kell megváltoztatni, hogy a gép a normál beállítások mellett megfelelően bakancsol.

### RAM

<p align="center"><img src="images/options-amiga-ram-en.png" alt="Amiga RAM beállítások" width="72%"></p>

Chip beállítása RAMLassan. RAM, RAM, és támogatta bővítési memória. A kompatibilitási üzenetek magyarázzák a kiválasztott gép korlátozásait, és az összes beállított memória alul jelenik meg.

**Chip RAM ** elérhető az egyéni zsetonokhoz, és a platform igényli. ** Lassú RAM ** a közös konfigurációkban használt kompatibilis bővítési memória. ** Gyors RAM ** a processzor- orientált bővítési memória. ** Zorro III RAM** csak azokra a modellekre vonatkozik, amelyek támogatják ezt a bővítési struktúrát. A kompatibilitási üzenetek és a fogyatékkal élő kezelőszervek megakadályozzák a kiválasztott modell által nem képviselhető kombinációkat.

### ROM

<p align="center"><img src="images/options-amiga-rom-en.png" alt="Amiga ROM beállítások" width="72%"></p>

Válassza ki a Kickstart rendszert ROM, opcionális kiterjesztés ROM, és ROM Kulcs. A felfedezett...ROM list megjeleníti nevek, revíziók, és kompatibilitás a kiválasztott modell. Kijelölés ROM és kattintson **Felhasználás** vagy kézzel böngészhet egy fájlba.

ROM A fájlokat nem a GW GUI. Használjon ROM-ot, amit legálisan használhat.

Az észlelt lista jobb, mint találgatni egy fájlnév: jelenti a ROM azonosítás és felülvizsgálat, valamint a kiválasztott modellel való kompatibilitás értékelése. **Kompatibilis ** a szokásos választás; ** Részben kompatibilis ** jelzi, hogy ROM lehet boot, de nem pontosan illeszkedik a gép. ** Frissítés ** a beállított ROM helyszínek. ** Felhasználás** kijelöli a kiválasztott észleltet ROM a konfigurációhoz.

### Videó

<p align="center"><img src="images/options-amiga-video-en.png" alt="Amiga videobeállítások" width="72%"></p>

Beállítja a videó szabványt, a képarány, felbontás, vonal mód, szegélyvágás, renderer, színmélység, képkocka ugrás, gamma, és flicker javítás. További chipset beállítások is elérhetők az oldalon, ha a kiválasztott modell támogatja.

| Beállítás | Gyakorlati hatás |
|---|---|
| Videoszabvány | Válogatás PAL vagy NTSC az időzítés és a várt frissítési viselkedés |
| Szemrevételezési arány | Az emulált kép méretezésének ellenőrzése |
| Felbontás | Automatikus vagy explicit kimeneti részlet kiválasztása |
| Vonalüzemmód | Ellenőrzi az interlaced vagy line-duplateljesítmény kezelését |
| Növényhatárok | Csak akkor távolítja el a fel nem használt overscan-t, ha engedélyezve van |
| Termelés | A grafikus háttér kiválasztása |
| Színmélység | Kimeneti színpontosság kiválasztása |
| Keretkilépés | A kiolvasztott keret csökkentése, ha be van kapcsolva |
| Gamma | A fényerő-válasz beállítása |
| Flicker fixer | Olyan feldolgozási módok, amelyek egyébként láthatóan villognának |

Egyszerre egy kijelző beállítása. Ha az emulációs ablak üres vagy instabil lesz, térjen vissza az automatikus felbontáshoz, a kikapcsolt képkockához, a semleges gamma-hoz, és a korábban dolgozó adatszolgáltatóhoz.

### Hang

<p align="center"><img src="images/options-amiga-audio-en.png" alt="Amiga hangbeállítások" width="72%"></p>

Hang engedélyezése vagy letiltása, a kimeneti eszköz és a késleltetés kiválasztása, majd interpoláció beállítása, Amiga szűrés, szűrő típus, sztereó szétválasztás, floppy- meghajtó hang, és CD- audio hangerő.

Az enyhébb késleltetés csökkenti a késést, de egy forgalmas számítógépen kiugrást okozhat. Növelje, ha a hang ropog. Interpoláció és Amiga Az audió szűrő a hangreprodukciót változtatja meg, nem pedig a program logikáját. Drive- sound hangerő szabályozza a szimulált mechanikai hang külön a normál Amiga audio.

### Tárolás

<p align="center"><img src="images/options-amiga-storage-en.png" alt="Amiga tárolási beállítások" width="72%"></p>

A tároló oldal felsorolja az eszközök azonosítóit, típusait, modelljeit, a kapcsolódó médiát és a rendelkezésre álló intézkedéseket. Itt lehet összeadni, beállítani vagy eltávolítani az eszközöket. A floppy lemezeket és CD-ket közvetlenül egy futtató gépről lehet behelyezni vagy kicserélni.

A **eszközazonosító ** az emulált rendszer hogyan kezeli a készüléket. ** Típus ** megkülönbözteti a floppy, merevlemez, optikai és más támogatott eszközök. ** Minta ** leírja az emulált hardvert, míg ** Kapcsolódó média** azonosítja a jelenleg kijelölt képet. Állítsa be az eszközt, mielőtt értékes írható médiát kapcsolna össze, és tartsa meg a merevlemezes képek mentéseit.

### Billentyűzet

<p align="center"><img src="images/options-amiga-keyboard-en.png" alt="Amiga billentyűzetbeállítások" width="72%"></p>

Keresés Amiga kulcsok és host megbízások, új billentyűk hozzárendelése, a térképek eltávolítása, alapértelmezések helyreállítása vagy egyértelmű konfliktusok. A státus oszlop azt jelenti, hogy minden megbízás érvényes-e.

A bal oszlop az emulált nevet adja Amiga kulcs; **Egyesület** mutatja a gazdatest kulcskombinációját. Az érvényes térképezés még mindig kényelmetlen lehet, ha a Windows vagy az alkalmazás ugyanazt a rövidítést tartja fenn, így tesztelje a kritikus kombinációkat a futtató gépen belül. Kerülje a egérkioldást vagy a teljes képernyős rövidítést egy olyan kulcshoz, amelyre az emulált szoftvernek gyakran szüksége van.

### Egér

<p align="center"><img src="images/options-amiga-mouse-en.png" alt="Amiga egér beállítások" width="72%"></p>

Állítsa be a fizikai egér sebességét, válassza ki, melyik analóg stick vezérli az egeret, állítsa be az analóg halott zóna és sebesség, és állítsa be a Mouse-action feltérképezések. Ha szükséges, az alapértelmezések vagy egyértelmű feltérképezési konfliktusok visszaállítása.

Növelje a halott zónát, ha a vezérlő irányít. A bal - és jobb-stick sebességet egymástól függetlenül kell beállítani, ha mindkét bot be van kapcsolva. Az alacsonyabb feltérképezési táblázat tárolja bemenetek egérgombokkal vagy akciók; vizsgálja meg a konfliktus állapotát megváltoztatását vezérlő feltérképezések máshol.

### Vezérlők

<p align="center"><img src="images/options-amiga-controllers-en.png" alt="Amiga Vezérlőbeállítások" width="72%"></p>

Kapcsolt vezérlők, eszközök és vezérlők kijelölése Amiga portok, és konfigurálja vezérlő térképek és turbo- tűz beállítások. A rendelkezésre álló lehetőségek az észlelt hardvertől és a kiválasztott géptől függenek.

Az 1-es és a 2-es port önállóan van beállítva. **Automatikus** A vezérlő típusa ésszerű kiindulópont, de a szoftver egy adott joystick vagy egér lehet szükség explicit típus. Futtasd le a felderítést, mielőtt kijelölsz egy újonnan csatlakoztatott vezérlőt. A turbótűz többször aktiválja a feltérképezett bemenetet, és kikapcsolva kell maradnia, hacsak a játék vagy alkalmazás nem részesül belőle.

## Hardverdiagnosztika és karbantartás

Ezek a párbeszédek a **Szerszámok ** Tab. Minden párbeszédablak előnézeti a generált Greaseweazle parancs. Ellenőrizze, mielőtt rákattint ** Végrehajtás**.

### Adatkezelő

<p align="center"><img src="images/tool-controller-information-en.png" alt="Adatkezelő" width="62%"></p>

A kijelölt vezérlő által jelentett információk megjelenítése. Kibontás **Nyersteljesítmény** amikor a teljes parancs válaszra van szükséged.

Használja ezt az első diagnosztikai parancsként. A sikeres válasz megerősíti, hogy GW GUI elindíthatja a beállított Host Tools programot, és kommunikálhat a kiválasztott eszközzel. A frissítést megelőzően rögzítse a firmware és hardver adatokat.

### USB sávszélesség

<p align="center"><img src="images/tool-usb-bandwidth-en.png" alt="USB sávszélesség" width="62%"></p>

A rendelkezésre álló intézkedések USB kommunikációs sávszélesség. Instabil transzferek diagnosztizálására vagy alkalmatlan USB kapcsolat.

Más szoftverek bezárása a vezérlő segítségével a vizsgálat előtt. Ismételje meg a mérést a USB Kikötő, kábel vagy hub. Az eredményeket hasonló körülmények között hasonlítsuk össze ahelyett, hogy egyetlen mérést abszolút garanciaként kezelnénk.

### Hajtási sebesség

<p align="center"><img src="images/tool-drive-speed-en.png" alt="Hajtási sebesség" width="62%"></p>

Méri a forgási sebességet. Növelje a mérések számát, ha reprezentatívabb eredményre van szüksége.

Egy mérés gyors ellenőrzés; több mérés is mutatja, hogy a sebesség stabil-e. Hagyjuk, hogy a meghajtó elérje a normál sebességet az eredmény értelmezése előtt. Egy váratlan érték rossz beállított sebességet, mechanikai problémát vagy mérési beállítási problémát jelezhet.

### Kereső fej

<p align="center"><img src="images/tool-seek-head-en.png" alt="Kereső fej" width="62%"></p>

A meghajtó fejet egy kiválasztott hengerre mozgatja. **Szélső hengerek engedélyezése ** általában korlátozott pozíciókat engedélyez, és ** A motor maradjon aktív** hagyja a motort a művelet során. Csak akkor használjon szélsőséges pozíciókat, ha a hardveres eljárás kifejezetten előírja.

Normál keresés hasznos megerősítése fej mozgás vagy helymeghatározás előtt diagnosztika. Hallgassa meg a rendellenes ismétlődő hatásokat, és hagyja abba, ha a kért henger nem alkalmas a meghajtásra. Ez az eszköz nem olvassa el és nem érvényesíti az adatokat a célpalackon.

### Hajtásbeállító diagnosztika

<p align="center"><img src="images/tool-drive-alignment-en.png" alt="Hajtásbeállító diagnosztika" width="62%"></p>

Megismételte a beállítási elemzést. Támogatja a pályák kiválasztását, a forradalmi és olvasási számokat, dekódolási formátumot, nyers fluxust, indexet, sebességet, PLL, density- pin, hard- sector, TG43, és vissza-adatok opciók. Az összehangolás megfelelő referenciaprofil és hardver ismereteket igényel.

Kezdje egy ismert referencia lemez és a legkisebb felülírás. **Váltóvágányok ** meghatározza a kiválasztott vágányokat és fejeket; ** A vágányonkénti fordulatszám ** az egyes minták időtartamának ellenőrzése; ** Olvasások száma** meghatározza az ismétlést. Csak akkor adjon meg egyedi lemezmeghatározást vagy dekódolási formátumot, ha az megfelel a referenciaadathordozónak. Olyan lehetőségek, mint a hamis index, a kemény szektorok, PLL felüljárók, sűrűségi szögek, és TG43 kemény- vagy formátspecifikus, és érvénytelenítheti az összehasonlítást, ha helytelen.

### Hardvercsapok

<p align="center"><img src="images/tool-hardware-pins-en.png" alt="Hardvercsapok" width="62%"></p>

A támogatott vezérlőpin olvasása vagy módosítása. Válassza ki a pin, engedélyezése **Pin módosítása ** csak akkor, ha egy értéket ír, és válassza ki ** Magas szint** ha a tervezett hardverművelet megköveteli.

A **Pin módosítása** kikapcsolva, a parancs lekérdezi a kitűzőt. Ez a biztonságosabb alapértelmezés. A szint megváltoztatása közvetlenül érinti a vezérlő I / O, és meg kell tenni csak a megfelelő Greaseweazle hardver dokumentáció és attasé-meghajtó vezetékek.

### Irányító újraindítása

<p align="center"><img src="images/tool-reset-controller-en.png" alt="Irányító újraindítása" width="62%"></p>

A Greaseweazle Irányító. Ezt használja, ha a vezérlő észleli, de már nem reagál rendesen.

Várjuk meg, amíg bármilyen aktív lemezművelet befejeződik az újraindítás előtt. Ezt követően, ha a kapcsolat állapota nem áll vissza automatikusan, akkor ismét ellenőrizze a vezérlőt. A reset nem javít meg egy hibát `gw.exe` útvonal vagy kikapcsolt USB készülék.

### Késedelmek

<p align="center"><img src="images/tool-delays-en.png" alt="Ellenőrzési késedelmek" width="62%"></p>

Beolvassa vagy módosítja a vezérlő időzítési értékeit, beleértve a kiválasztást, a fejlépést, a települést, a motort, az automatikus választásokat, az írási időzítést és az indexmaszkot. Csak azokat az értékeket adja meg, amelyeket módosítani kíván.

Az ellenőrizetlen mezők változatlanul hagyják a megfelelő vezérlőértéket. A szerkesztés előtt rögzítse a meglévő értékeket. Az időzítési változtatások minden későbbi fizikai műveletet befolyásolhatnak, ezért teszteljék fel feláldozható adathordozókkal, és állítsák helyre az ismert jó értékeket, ha a viselkedés megbízhatatlanná válik.

### Firmware

<p align="center"><img src="images/tool-firmware-en.png" alt="A szoftverek frissítése" width="62%"></p>

Frissíti a vezérlő firmware. **A bootloader frissítése** kifejezetten kockázatosnak minősül, és mozgáskorlátozottnak kell maradnia, kivéve, ha a hivatalos firmware eljárás megköveteli. Frissítés közben ne bontsa le a vezérlőt.

Frissítés előtt erősítse meg a csatlakoztatott vezérlő **Adatkezelő** stabil közvetlen USB kapcsolat, és zárja le más szoftver, hogy hozzáférhet. A befejezés után csatlakoztassa újra az adatkezelőt, vagy újra olvassa el az adatait, hogy ellenőrizze a jelentett firmware verziót.

## Bejelentések és műveleti előzmények

Nyissa meg a művelet előzményeit, hogy ellenőrizze mentett naplók művelet.

<p align="center"><img src="images/operation-history-en.png" alt="Műveleti előzmények" width="68%"></p>

Válasszon ki egy logot a bal oldalon a tartalom megjelenítéséhez. **Export** elmentett egy másolatot diagnosztikához vagy támogatáshoz. A paths és a parancssorok tartalmazhatnak személyes mappa neveket, ezért az exportált naplók megosztása előtt felülvizsgálhatják azokat.

Az élő konzol a fő ablakban mutatja a jelenlegi parancsot és a legújabb kimenetet. A másoló gomb lemásolja a megjelenített szöveget.

### A napló olvasása

Egy hasznos diagnosztikai napló tartalmazza a generált parancsot, időbélyegzőket, motor kimenetet és a végleges állapotot. Alulról felfelé haladva: azonosítsa a végső hibát, majd keresse meg az azt megelőző első figyelmeztetést vagy sikertelen pályát. A későbbi általános kudarc gyakran csak egy korábbi, konkrétabb üzenet következménye.

Két kísérlet összehasonlításakor ellenőrizze, hogy a vezérlő, a meghajtó, a motor, a profil, a forrásútvonal, a kimeneti formátum és a szakértői érvek azonosak voltak-e. Ellenkező esetben egy másik eredmény a megváltozott beállításokat tükrözheti a lemez instabilitása helyett.

## Alkalmazási adatok és hordozható használat

GW GUI a felhasználói adatokat elkülöníti az alkalmazásikontrolloktól. A kiválasztott csomagtól és módtól függően a beállítások, naplók, letöltött eszközök, emulátor alkatrészek, rögzítések, állapotok és gépkonfigurációk az alkalmazásban is tárolhatók `Data` könyvtár vagy a beállított user- adatok helyein.

A hordozható installáció cseréje vagy mozgatása előtt a teljes alkalmazás mappát együtt kell tartani, és a `Data` mappa. Ne mozgassa az egyes fájlokat `lib`, mert az alkalmazás megszünteti a saját és harmadik fél könyvtárak ebből a szerkezetből.

### Javasolt tartalék tartalom

Erősítse meg a következőket, amikor azok fontosak a munkafolyamathoz:

- alkalmazásbeállítások és profilok;
- az adatkezelő és a meghajtó meghatározása;
- emulációs konfigurációk;
- ROM Útvonalakat és törvényesen fenntartott ROM mentések;
- merevlemez és eltávolítható médiaképek;
- fogások és mentett államok;
- a megőrzési nyilvántartásként használt műveleti naplók.

A lemezképek sokkal nagyobbak lehetnek, mint a beállítások. Tárolja archiválási mesterek read- csak ha lehetséges, és dolgozzon másolatok.

## Ajánlott munkafolyamatok

### Ismeretlen lemez archiválása

1. Vizsgálja meg és tisztítsa meg a meghajtót egy megfelelő karbantartási eljárással.
2. Írjon - védje meg a lemezt, ha lehetséges.
3. Kijelölés **Olvassa el > Nyers kép (SCP)**.
4. Használjon egy leíró fájlnevet, és olvassa el a normál pályatartomány több fordulat.
5. Tekintse át a konzolt és mentett napló.
6. Vizsgáljuk meg mindkét oldalt **Visualization**.
7. Átalakít egy másolatot a valószínű ágazati formátumokra.
8. Az átalakított másolatok vizsgálata **Disk Explorer** vagy megfelelő szoftvert.
9. Őrizzék meg a nyers mestert, a naplót és a jegyzeteket együtt.

### Lemez visszaállítása képből

1. Vizsgálja meg a képet, és erősítse meg a várt család és formátum.
2. Helyezzen be egy megfelelő méretű és sűrűségű feláldozható vagy szándékosan írható lemezt.
3. Megnyitás **Írás** és válassza ki a képet.
4. A konfigurált meghajtó és az érzékelt formátum megerősítése.
5. Írd meg a lemezt.
6. Olvasd vissza egy külön ellenőrző képre.
7. Hasonlítsa össze a dekódolt tartalmakat és nézze át a gyanús nyomokat vizuálisan.

### Emulált Amiga

1. Megnyitás **Opciók > Emuláció > Beállítások** és hozzon létre vagy válasszon ki egy gépet.
2. In **Amiga > Általános**, válassza ki a modell és emulátor verzió.
3. Összeegyeztethető, jogszerűen megszerzett ROM.
4. A modell alapértelmezett CPU és RAM Az első csizma.
5. Videó és audió beállítása konzervatív automatikus beállításokkal.
6. Tároló eszközök hozzáadása és kapcsolódó másolt média képek.
7. Áttekintés billentyűzet, egér, és vezérlő feladatok.
8. Mentse a konfigurációt.
9. Vissza a **Emuláció **, válassza ki, és kattintson ** Megnyitás**.
10. Csak a sikeres alapbeállítás után váltsunk gyorsulást vagy haladó beállításokat egyesével.

## Biztonsági ellenőrző lista

Korábban **Olvassa el**:

- a forráslemez a megfelelő meghajtóban van;
- a forrást lehetőség szerint írásban védik;
- a kimeneti útvonal nem ír felül egy meglévő parancsnokot;
- a profil és a sáv tartománya megegyezik a lemezzel.

Korábban **Írás ** vagy ** Törlés**:

- a céllemez megsemmisíthető;
- a kép és a meghajtás helyes;
- a lemez mérete és sűrűsége kompatibilis;
- semmilyen levéltári mestert nem használnak rendeltetésként.

A hardver-változó eszköz előtt:

- nincs más művelet;
- a megfelelő vezérlő kiválasztása;
- az aktuális értékeket rögzítették;
- az adatkezelő stabil energiával rendelkezik, és USB kapcsolat;
- a műveletet a hardverdokumentáció támogatja.

## Hibaelhárítás

### Az adatkezelő nincs felsorolva

1. Csatlakoztassa a vezérlőt közvetlenül a számítógéphez.
2. Megnyitás **Opciók > Vezérlő és meghajtó**.
3. Kattintson ide **Keresés**.
4. Ellenőrizze a vezérlő állapotát és a meghajtó konfigurációját.
5. Futás! **Adatkezelő** ha az észlelés sikerrel jár, de a parancs nem.

Ha még mindig nem jelenik meg, próbálja meg egy másik közvetlen USB Kikötő és kábel, aztán rescan. Ellenőrizze a Windows Device Manager egy újonnan felfedezett soros eszköz. A vezérlő látható a Windows, de hiányzik GW GUI általában mutat egy forgalmas port, állott konfiguráció, vagy Host Tools probléma; egy vezérlő hiányzik a Windows pontok USB, teljesítmény, vezető, vagy hardver.

### `gw.exe` nem található

Megnyitás **Opciók > Vezérlő és meghajtó ** használat **Keresés gw.exe **, ** Válasszon **vagy ** A legújabb verzió letöltése**. Erősítse meg, hogy az észlelt útvonal a tervezett Greaseweazle telepítés.

Miután kiválasztottad, fuss. **Adatkezelő**. Ha ez nem sikerül, mielőtt kapcsolatba lépne a hardverrel, ellenőrizze a naplót egy érvénytelen futtatható elérési út, hiányzó fájlok, vagy egy verzió, amely nem indul.

### Egy művelet rossz motort használ

Megnyitás **Opciók > Motorok** és ellenőrizze az adott művelethez rendelt motort. GW GUI nem esik vissza csendben a másik motorra.

A motor beállítása különálló: a konverziós motor megváltoztatása nem változtat az olvasáson, íráson vagy Disk Explorer. Újra megnyitni a hiba művelet mentése után az opciót, és ellenőrizze a generált parancs a konzolban.

### A kép nem ismert

Automatikus érzékelés kikapcsolása csak akkor, ha ismeri a megfelelő gépet és formátumot. Ellenkező esetben próbálja ki a **Visualization** lap, hogy ellenőrizze a képet egy alacsonyabb szinten.

Ellenőrizzük, hogy a forrás egy nyers fluxus rögzítés, egy ágazati kép, egy tömörített tartály, vagy egy független fájl félrevezető kiterjesztéssel. Soha ne nevezz át egy kiterjesztést pusztán az észlelés kényszerítésére; az átalakításnak helyesen kell értelmeznie a forrásszerkezetet.

### Emuláció nem indul

A mentett konfiguráció ellenőrzése, telepített emulátor verzió, kiválasztva ROMa tárolási utak és a modell kompatibilitás. Az alkalmazás naplójának felülvizsgálata a teljes hibaadatok tekintetében.

Ideiglenes visszatérés CPU, RAM, videó és tárolás egy egyszerű model-kompatibilis alapvonal. Ha az alapvonal elkezdődik, egyszerre csak egy beállítást kell visszaállítani. Egy másik emulátor verzióval vagy gép meghatározással létrehozott mentett állapot akkor is sikerülhet, ha a tiszta csizma működik.

### Rövidítés vagy bemenet nem működik

Ellenőrizze mind a globális **Emuláció > Rövidítések** oldal és a gépspecifikus billentyűzet, egér, vagy vezérlő oldal. Oldja meg az ellentmondásként megjelölt feladatokat.

Ha az egér rögzítve van, használja a Running- machine eszköztárában látható kioldási gyorsítást. Ha a kezelőt a Beállítások megnyitása után csatlakoztatták, futtassa újra a vezérlő észlelését, mielőtt kijelölné.

### Egy parancs váratlanul megbukik.

1. Olvassa el az élő konzol kimenetét.
2. Megnyitás **Műveleti előzmények** a teljes mentett napló.
3. A kijelölt vezérlő, meghajtó, profil, motor és fájlutak megerősítése.
4. Exportálja a vonatkozó naplót, ha meg kell osztani a diagnózis.

### Hangmorzsa vagy -szünet

Növelje emuláció audio latency, szoros CPU-intenzív alkalmazások, és vissza videó keret ugrás és gyorsulás a korábbi értékeket. Ellenőrizze, hogy a kívánt Windows audio eszköz kiválasztásra került-e. Egyszerre egy beállítást kell megváltoztatni, így a tényleges korrekció azonosítható.

### Az emulációs kijelző üres vagy lassú

A felbontás és a vonal üzemmódjának visszaállítása **Automatikus**, tiltsa le a keret kiugró és flicker rögzítés ideiglenesen, és próbálja ki a korábban működő rendező. Igazolja, hogy a beállított ROM és a beépített boot média érvényes. A FPS indikátor segít megkülönböztetni a rendering- teljesítmény probléma egy gép, amely egyszerűen nem booted.

### Az olvasás instabil nyomokat tartalmaz

Ismételje meg az olvasást egy új fájlnévre, növelje a forradalmakat, ahol szükséges, és hasonlítsa össze az érintett számokat. Tisztítsa meg a meghajtó fejeket a helyes eljárással, és ellenőrizze a lemezt fizikai károsodás. Ne olvassa el újra és újra a látható szóródás vagy sérült média, mert további passz ronthatja azt.

## Glosszárium

| kifejezés | Vagyis: GW GUI |
|---|---|
| Vezérlő | A Greaseweazle hardver interfész csatlakoztatva USB |
| Indítás | A kezelőhöz csatlakoztatott fizikai floppy meghajtó |
| Motor | A művelet végrehajtásához kiválasztott végrehajtás |
| Flux | A lemezen olvasott mágneses átmeneteket ábrázoló időzítő adatok |
| Nyers kép | A rögzítés megőrzi alacsony szintű lemez információk, mint például SCP |
| Ágazati kép | A logikai szektorokba szervezett dekódolt képviselet |
| Forradalom | A vágány olvasása során vett teljes forgás |
| Henger | Radiális fejhelyzet; egy henger mindkét oldalán tartalmazhat vágányt |
| Fej | A fizikai meghajtó által kiválasztott lemezoldal |
| Profil | A művelet újrahasználható beállításai |
| ROM | Emulált gép által megkövetelt firmware kép |
| Mentett állam | A pillanatfelvétel a futó emulátor gép állapota |
| Rendező | A grafikus backend használt megjelenítésére emulációs kimenetet |

## Gyors hivatkozás

| Ha akarod... | Menj... |
|---|---|
| Fizikai lemez megőrzése | **Olvassa el** |
| Helyezzen vissza egy képet egy lemezre | **Írás** |
| Más képformátum készítése | **Átalakítás** |
| Vizsgálópályák vagy fluxusanomáliák | **Visualization** |
| Fájlok böngészése a képben | **Disk Explorer** |
| A vezérlő kommunikációjának ellenőrzése | **Szerszámok > Adatkezelő** |
| Mérési forgás | **Szerszámok > Hajtási sebesség** |
| Előző parancs felülvizsgálata | **Műveleti előzmények** |
| A hardver beállítása | **Opciók > Vezérlő és meghajtó** |
| A végrehajtás kiválasztása | **Opciók > Motorok** |
| Emulált gép létrehozása vagy szerkesztése | **Opciók > Emuláció** |
| Mentett gép indítása | **Emuláció** |
