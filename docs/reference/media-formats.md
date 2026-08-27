# Formats de médias et couverture SCP

## Familles et formats d’images

### État dans GW GUI

- `✅ Fait en interne` : l’image est ouverte directement par GW GUI et le système de fichiers correspondant peut être exploré.
- `🟡 Partiel` : seule une partie des extensions, variantes ou fonctions indiquées sur la ligne est réalisée ; le détail est précisé.
- `🔵 Flux` : le décodage et l’encodage des pistes sont présents, mais le conteneur ou l’exploration des fichiers reste à compléter.
- Une ligne sans marque n’est pas encore réalisée en interne.

Ces états décrivent le code interne de GW GUI, pas les possibilités fournies séparément par `gw.exe`.

* **Acorn**

  * **BBC Micro Model A, Model B**

    * 5,25" simple face — `.ssd`, `.img` — 🟡 **Partiel : `.ssd` et BBC DFS faits ; `.img` reste à faire**
    * 5,25" double face — `.dsd`, `.img` — 🟡 **Partiel : `.dsd` et BBC DFS faits ; `.img` reste à faire**
  * **BBC Master 128, Master Compact**

    * 5,25" — `.ssd`, `.dsd` — ✅ **Fait en interne : lecture et exploration BBC DFS**
    * 3,5" — `.adf`, `.adl`
  * **Acorn Electron avec interface disquette**

    * 5,25" — `.ssd`, `.dsd` — ✅ **Fait en interne : lecture et exploration BBC DFS**
  * **Archimedes A300, A400, A3000**

    * 3,5" DD 800 Ko — `.adf`, `.adl`
  * **Archimedes A5000, A4, Risc PC**

    * 3,5" HD 1,6 Mo — `.adf`, `.adl`
    * formats DOS compatibles — `.img`, `.ima`

* **Amstrad**

  * **CPC 464 + DDI-1, CPC 664, CPC 6128**

    * 3" CF2 — `.dsk`, `.edsk` — ✅ **Fait en interne : CPCEMU DSK standard/étendu et CP/M**
    * format étendu CPC — `.dsk` — ✅ **Fait en interne**
  * **CPC 464 Plus, CPC 6128 Plus**

    * 3" CF2 — `.dsk`, `.edsk` — ✅ **Fait en interne**
  * **PCW 8256, PCW 8512**

    * 3" CF2 / CF2DD — `.dsk`, `.edsk` — ✅ **Fait en interne : lecture et exploration CP/M**
  * **PCW 9512**

    * 3" CF2DD — `.dsk`, `.edsk` — ✅ **Fait en interne : lecture et exploration CP/M**
  * **PCW 9256, PCW 9512+**

    * 3,5" — `.dsk`, `.img` — ✅ **Fait en interne pour les géométries PCW reconnues**
  * **PC1512, PC1640**

    * 5,25" DD 360 Ko — `.img`, `.ima`, `.dsk` — 🟡 **Partiel : `.img`/`.ima` FAT12 faits ; `.dsk` brut générique reste à faire**
  * **PC2086, PC2286, PC2386**

    * 3,5" DD 720 Ko — `.img`, `.ima` — ✅ **Fait en interne : lecture et exploration FAT12**
    * 3,5" HD 1,44 Mo selon modèle — `.img`, `.ima` — ✅ **Fait en interne : lecture et exploration FAT12**

* **Apple**

  * **Apple II, Apple II+, Apple IIe**

    * 5,25" 13 secteurs — `.d13` — ✅ **Fait en interne : Apple DOS 3.2**
    * 5,25" 16 secteurs / 140 Ko — `.dsk`, `.do`, `.po` — ✅ **Fait en interne : Apple DOS/ProDOS**
    * image nibble — `.nib` — ✅ **Fait en interne**
    * image WOZ — `.woz` — ✅ **Fait en interne**
    * conteneur Apple II — `.2mg`, `.2img` — 🟡 **Partiel : `.2mg` fait ; extension `.2img` reste à ajouter**
  * **Apple IIc**

    * 5,25" 140 Ko — `.dsk`, `.do`, `.po`, `.nib`, `.woz`, `.2mg` — ✅ **Fait en interne**
    * 3,5" 800 Ko avec lecteur compatible — `.po`, `.2mg` — ✅ **Fait en interne : ProDOS**
  * **Apple IIc Plus**

    * 3,5" 800 Ko — `.po`, `.2mg`, `.2img` — 🟡 **Partiel : `.po`/`.2mg` faits ; extension `.2img` reste à ajouter**
  * **Apple IIgs**

    * 5,25" 140 Ko — `.dsk`, `.do`, `.po`, `.nib`, `.woz`, `.2mg` — ✅ **Fait en interne**
    * 3,5" 800 Ko — `.po`, `.2mg`, `.2img` — 🟡 **Partiel : `.po`/`.2mg` faits ; extension `.2img` reste à ajouter**
  * **Apple III, Apple III+**

    * 5,25" — `.dsk`, `.po`, `.2mg` — ✅ **Fait en interne : Apple III SOS**
  * **Lisa 1**

    * 5,25" Twiggy — `.image`, `.dc42` — ✅ **Fait en interne : données/tag Lisa et Lisa Office System**
    * format très spécifique : conserver les dumps physiques originaux lorsqu’ils existent
  * **Lisa 2, Lisa 2/5, Lisa 2/10**

    * 3,5" Sony 400 Ko — `.image`, `.dc42` — ✅ **Fait en interne pour les images DiskCopy reconnues**
  * **Macintosh 128K, Macintosh 512K**

    * 3,5" 400 Ko — `.dsk`, `.image`, `.dc42` — ✅ **Fait en interne : MFS/HFS selon l’image**
  * **Macintosh 512Ke, Macintosh Plus, Macintosh SE**

    * 3,5" 400 Ko — `.dsk`, `.image`, `.dc42` — ✅ **Fait en interne : MFS/HFS selon l’image**
    * 3,5" 800 Ko — `.dsk`, `.image`, `.dc42` — ✅ **Fait en interne : MFS/HFS selon l’image**
  * **Macintosh SE FDHD, Macintosh IIx, IIcx, IIci**

    * 3,5" 800 Ko — `.dsk`, `.image`, `.dc42` — ✅ **Fait en interne**
    * 3,5" HD 1,44 Mo — `.dsk`, `.image`, `.dc42`, `.img` — ✅ **Fait en interne**
  * **Macintosh Classic, LC, Quadra, Centris**

    * 3,5" HD 1,44 Mo — `.dsk`, `.image`, `.dc42`, `.img` — ✅ **Fait en interne**
  * **Power Macintosh avec lecteur de disquette**

    * 3,5" HD 1,44 Mo — `.dsk`, `.image`, `.dc42`, `.img` — ✅ **Fait en interne**

* **Atari**

  * **Atari 400, Atari 800**

    * 5,25" SD — `.atr`, `.xfd` — 🟡 **Partiel : `.atr` et Atari DOS faits ; `.xfd` reste à faire**
    * formats protégés / bas niveau — `.atx`
  * **Atari 600XL, 800XL, 1200XL**

    * 5,25" SD / Enhanced Density — `.atr`, `.xfd`, `.atx` — 🟡 **Partiel : `.atr` et Atari DOS faits ; `.xfd`/`.atx` restent à faire**
  * **Atari 65XE, 130XE, XE Game System**

    * 5,25" SD / ED / DD selon lecteur — `.atr`, `.xfd`, `.atx` — 🟡 **Partiel : `.atr` et Atari DOS faits ; `.xfd`/`.atx` restent à faire**
  * **Atari ST, 520ST, 1040ST, STF, STFM, Mega ST**

    * 3,5" DD 360/720 Ko — `.st`, `.msa` — ✅ **Fait en interne : lecture et exploration Atari TOS FAT12**
    * image protégée / bas niveau — `.stx`
    * préservation — `.ipf`
    * autres images rencontrées — `.dim`
  * **Atari STe, 520STe, 1040STe**

    * 3,5" DD 720 Ko — `.st`, `.msa`, `.stx`, `.ipf`, `.dim` — 🟡 **Partiel : `.st`/`.msa` et Atari TOS FAT12 faits ; autres extensions à faire**
  * **Mega STe**

    * 3,5" DD 720 Ko — `.st`, `.msa`, `.stx`, `.ipf` — 🟡 **Partiel : `.st`/`.msa` faits ; `.stx`/`.ipf` restent à faire**
    * 3,5" HD 1,44 Mo selon configuration — `.st` — ✅ **Fait en interne**
  * **Atari TT030**

    * 3,5" DD 720 Ko — `.st`, `.msa` — ✅ **Fait en interne**
    * 3,5" HD 1,44 Mo — `.st` — ✅ **Fait en interne**
  * **Atari Falcon 030**

    * 3,5" DD 720 Ko — `.st`, `.msa` — ✅ **Fait en interne**
    * 3,5" HD 1,44 Mo — `.st` — ✅ **Fait en interne**

* **Commodore**

  * **PET / CBM avec 2040, 3040, 4040**

    * 5,25" — `.d64`, `.d67` — 🟡 **Partiel : `.d64` et Commodore DOS faits ; `.d67` reste à faire**
    * image GCR — `.g64`
  * **PET / CBM avec 8050**

    * 5,25" — `.d80`
  * **PET / CBM avec 8250**

    * 5,25" double face — `.d82`
  * **VIC-20**

    * lecteur 1540 / 1541, 5,25" — `.d64`, `.g64`, `.p64`, `.nib`, `.nbz` — 🟡 **Partiel : `.d64` et Commodore DOS faits ; autres extensions à faire**
  * **Commodore 16, C116, Plus/4**

    * lecteur 1551 / 1541, 5,25" — `.d64`, `.g64` — 🟡 **Partiel : `.d64` fait ; `.g64` reste à faire**
  * **Commodore 64, C64C, C64G**

    * 1541 / 1541-II, 5,25" — `.d64`, `.x64`, `.g64`, `.p64`, `.nib`, `.nbz` — 🟡 **Partiel : `.d64` et Commodore DOS faits ; autres extensions à faire**
    * 1571, 5,25" double face — `.d71`, `.g71` — 🟡 **Partiel : `.d71` fait ; `.g71` reste à faire**
    * 1581, 3,5" 800 Ko — `.d81` — ✅ **Fait en interne : lecture et exploration Commodore DOS**
  * **Commodore 128, C128D, C128DCR**

    * 1541, 5,25" — `.d64`, `.g64`, `.p64` — 🟡 **Partiel : `.d64` fait ; `.g64`/`.p64` restent à faire**
    * 1571, 5,25" double face — `.d71`, `.g71` — 🟡 **Partiel : `.d71` fait ; `.g71` reste à faire**
    * 1581, 3,5" 800 Ko — `.d81` — ✅ **Fait en interne**
  * **Commodore 65**

    * 3,5" — `.d81` — ✅ **Fait en interne**
  * **Amiga 1000, Amiga 500, 500+, Amiga 600, Amiga 2000**

    * 3,5" DD 880 Ko — `.adf`, `.adz`, `.dms` — 🟡 **Partiel : `.adf` et AmigaDOS faits ; `.adz`/`.dms` restent à faire**
    * préservation / pistes non standard — `.ipf`
    * images bas niveau compatibles — `.fdi`
  * **Amiga 3000**

    * 3,5" DD 880 Ko — `.adf`, `.adz`, `.dms`, `.ipf` — 🟡 **Partiel : `.adf` fait ; autres extensions à faire**
    * HD selon lecteur — `.adf` — ✅ **Fait en interne : AmigaDOS HD**
  * **Amiga 1200, Amiga 4000**

    * 3,5" DD 880 Ko — `.adf`, `.adz`, `.dms`, `.ipf` — 🟡 **Partiel : `.adf` fait ; autres extensions à faire**
    * 3,5" HD 1,76 Mo — `.adf` — ✅ **Fait en interne : AmigaDOS HD**
  * **Commodore 900**

    * 5,25" — `.img` — 🔵 **Flux : décodeur/encodeur C900 GCR et exploration COHERENT présents ; ouverture directe actuellement reconnue via `.bin`, pas encore via `.img`**
    * COHERENT 0.7.3 : plusieurs géométries propres au C900

* **DEC**

  * **PDP-8 avec RX01**

    * 8" RX01 — `.img`, `.dsk`
  * **PDP-8 / PDP-11 avec RX02**

    * 8" RX02 — `.img`, `.dsk` — 🟡 **Partiel : `.img`, décodage RX02 et exploration RT-11 faits ; `.dsk` reste à faire**
  * **DEC MINC avec RX02**

    * 8" RX02 512 512 octets — `.img` — ✅ **Fait en interne**
    * RT-11 — ✅ **Fait en interne : exploration du système de fichiers**
  * **PDP-11 avec RX50**

    * 5,25" — `.img`, `.dsk`
  * **VAX / MicroVAX avec RX50**

    * 5,25" — `.img`, `.dsk`
  * **DEC systèmes avec RX33**

    * 5,25" — `.img`
  * **DEC systèmes avec RX23**

    * 3,5" — `.img`

* **Epson**

  * **Epson QX-10**

    * 5,25" — `.img`, `.dsk` — 🟡 **Partiel : géométries et exploration CP/M QX-10 présentes ; reconnaissance générique de tous les conteneurs à compléter**
    * plusieurs géométries selon TPM, CP/M et Valdocs — 🟡 **Partiel : plusieurs dispositions QX-10 sont déjà cataloguées**
  * **Epson Equity / compatibles PC**

    * 5,25" 360 Ko / 1,2 Mo — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
    * 3,5" 720 Ko / 1,44 Mo — `.img`, `.ima` — ✅ **Fait en interne : FAT12**

* **Fujitsu**

  * **FM-7, FM-77**

    * 5,25" — `.d77`, `.d88`, `.dsk`
  * **FM-77AV**

    * 3,5" / 5,25" selon configuration — `.d77`, `.d88`
  * **FM Towns**

    * 3,5" 1,2 Mo — `.d77`, `.d88`, `.xdf`, `.img`

* **IBM**

  * **IBM PC 5150**

    * 5,25" 160 Ko — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
    * 5,25" 180 Ko — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
    * 5,25" 320 Ko — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
    * 5,25" 360 Ko — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
  * **IBM PC XT 5160**

    * 5,25" 360 Ko — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
  * **IBM PC AT 5170**

    * 5,25" 360 Ko — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
    * 5,25" HD 1,2 Mo — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
  * **IBM PS/2**

    * 3,5" DD 720 Ko — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
    * 3,5" HD 1,44 Mo — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
    * 3,5" ED 2,88 Mo sur modèles compatibles — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
  * **IBM PC compatibles XT / AT / 286 / 386 / 486 / Pentium**

    * 5,25" 160/180/320/360 Ko — `.img`, `.ima`, `.dsk` — 🟡 **Partiel : `.img`/`.ima` FAT12 faits ; `.dsk` brut générique reste à faire**
    * 5,25" 1,2 Mo — `.img`, `.ima`, `.dsk` — 🟡 **Partiel : `.img`/`.ima` faits ; `.dsk` brut générique reste à faire**
    * 3,5" 720 Ko — `.img`, `.ima`, `.dsk` — 🟡 **Partiel : `.img`/`.ima` faits ; `.dsk` brut générique reste à faire**
    * 3,5" 1,44 Mo — `.img`, `.ima`, `.dsk` — 🟡 **Partiel : `.img`/`.ima` faits ; `.dsk` brut générique reste à faire**
    * 3,5" 2,88 Mo — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
    * ImageDisk — `.imd` — 🟡 **Partiel : lecteur de conteneur présent ; exploration selon la géométrie reconnue**
    * TeleDisk — `.td0` — 🟡 **Partiel : images TD0 ordinaires non compressées seulement**
    * CopyQM — `.cqm`
    * CopyIIPC / TransCopy — `.cp2` — ✅ **Fait en interne : lecteur de conteneur**
    * AnaDisk — `.ana`
    * DiskDupe — `.ddi`
    * DiskCopy — `.dsk`
    * WinImage — `.ima`, `.imz` — 🟡 **Partiel : `.ima` fait ; `.imz` reste à faire**
    * formats protégés / structurés — `.fdi`
  * **Microsoft DMF**

    * 3,5" ~1,68 Mo — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
  * **IBM XDF**

    * 3,5" capacité étendue — `.xdf`
  * **2M / 2MGUI**

    * formats haute capacité — `.2m`, `.img`

* **Kaypro**

  * **Kaypro II**

    * 5,25" — `.img`, `.dsk`, `.imd`, `.td0`
  * **Kaypro 4, 4/84**

    * 5,25" — `.img`, `.dsk`, `.imd`, `.td0`
  * **Kaypro 10**

    * 5,25" — `.img`, `.dsk`, `.imd`, `.td0`

* **MSX**

  * **MSX1 avec lecteur de disquette**

    * 3,5" / 5,25" — `.dsk`, `.img` — 🟡 **Partiel : `.dsk` avec signature MSX et MSX-DOS FAT12 faits ; `.img` spécifique reste à consolider**
  * **MSX2**

    * 3,5" 360/720 Ko — `.dsk`, `.img` — 🟡 **Partiel : `.dsk` et MSX-DOS FAT12 faits ; `.img` spécifique reste à consolider**
  * **MSX2+**

    * 3,5" 720 Ko — `.dsk`, `.img` — 🟡 **Partiel : `.dsk` et MSX-DOS FAT12 faits ; `.img` spécifique reste à consolider**
  * **MSX Turbo R**

    * 3,5" 720 Ko — `.dsk`, `.img` — 🟡 **Partiel : `.dsk` et MSX-DOS FAT12 faits ; `.img` spécifique reste à consolider**

* **NEC**

  * **PC-8001**

    * 5,25" — `.d88`, `.d77`, `.dsk`
  * **PC-8801**

    * 5,25" — `.d88`, `.d77`, `.dsk`, `.fdi`
  * **PC-9801**

    * 5,25" 2D / 2DD / 2HD — `.d88`, `.d98`, `.fdi`, `.xdf`, `.hdm`, `.img`
    * 3,5" 2DD / 2HD — `.d88`, `.d98`, `.fdi`, `.xdf`, `.hdm`, `.img`
  * **PC-9821**

    * 3,5" 1,2 Mo / 1,44 Mo selon modèle — `.d88`, `.d98`, `.fdi`, `.xdf`, `.hdm`, `.img`

* **NorthStar**

  * **NorthStar Horizon, Advantage**

    * 5,25" — `.img`, `.dsk`, `.imd` — 🔵 **Flux : décodeur/encodeur NorthStar MFM présents ; conteneurs et exploration restent à compléter**
    * formats hard-sectored selon contrôleur — 🔵 **Flux : couche de pistes présente**

* **Osborne**

  * **Osborne 1**

    * 5,25" simple face — `.img`, `.dsk`, `.imd`, `.td0`
  * **Osborne Executive**

    * 5,25" — `.img`, `.dsk`, `.imd`, `.td0`

* **Oric**

  * **Oric-1 avec Microdisc**

    * 3" / 3,5" selon lecteur — `.dsk`
  * **Oric Atmos avec Microdisc**

    * 3" / 3,5" — `.dsk`
  * **Oric Telestrat**

    * 3" — `.dsk`

* **Sharp**

  * **MZ-80, MZ-700**

    * 5,25" selon extension — `.dsk`, `.d88`
  * **MZ-800**

    * 5,25" — `.dsk`, `.d88`
  * **Sharp X1**

    * 5,25" — `.d88`, `.dsk`
  * **Sharp X68000**

    * 5,25" HD 1,2 Mo — `.xdf`, `.dim`, `.d88`, `.img`

* **Sinclair**

  * **ZX Spectrum 48K / 128K avec Beta Disk**

    * 5,25" — `.trd`, `.scl`, `.fdi`, `.udi`, `.td0`
  * **ZX Spectrum +2 avec interface disquette**

    * selon interface — `.trd`, `.scl`, `.dsk`
  * **ZX Spectrum +3**

    * 3" CF2 — `.dsk`, `.edsk`
  * **Sinclair QL avec extensions floppy**

    * 3,5" — `.img`, `.dsk`

* **Tandy / Radio Shack**

  * **TRS-80 Model I**

    * 5,25" — `.dsk`, `.dmk`, `.jv1`, `.jv3`
  * **TRS-80 Model II**

    * 8" — `.dsk`, `.dmk`, `.imd`
  * **TRS-80 Model III**

    * 5,25" — `.dsk`, `.dmk`, `.jv1`, `.jv3`
  * **TRS-80 Model 4**

    * 5,25" — `.dsk`, `.dmk`, `.jv1`, `.jv3`
  * **TRS-80 Color Computer / CoCo**

    * 5,25" — `.dsk`, `.dmk`, `.jvc`
  * **Tandy 1000**

    * 5,25" 360 Ko — `.img`, `.ima` — ✅ **Fait en interne : FAT12**
    * 3,5" 720 Ko / 1,44 Mo selon modèle — `.img`, `.ima` — ✅ **Fait en interne : FAT12**

* **Texas Instruments**

  * **TI-99/4A**

    * 5,25" — `.dsk`
    * formats V9T9 — `.dsk`
    * formats track-based — `.trk`
    * formats HFE spécifiques possibles pour remplacement de lecteur
  * **TI Professional Computer**

    * 5,25" — `.img`, `.dsk`

* **Thomson**

  * **TO7, TO7/70**

    * 5,25" — `.fd`, `.sap`
  * **MO5, MO6**

    * 5,25" — `.fd`, `.sap` — 🔵 **Flux : décodeur/encodeur QD MO5 MFM présents ; conteneurs et exploration restent à faire**
    * 3,5" selon lecteur — `.fd`, `.sap` — 🔵 **Flux : décodeur/encodeur QD MO5 MFM présents ; conteneurs et exploration restent à faire**
  * **TO8, TO8D**

    * 3,5" — `.fd`, `.sap`
  * **TO9, TO9+**

    * 3,5" — `.fd`, `.sap`

* **Victor**

  * **Victor 9000 / Sirius 1**

    * 5,25" vitesse variable / GCR — `.img` — 🔵 **Flux : décodeur/encodeur Victor 9000 GCR présents ; conteneur et exploration restent à faire**
    * cas particulier : géométrie et vitesse variables, donc les images sectorielles classiques ne représentent pas toujours intégralement le disque

* **Heath / Zenith**

  * **Heath H8**

    * 5,25" hard-sectored — `.img`, `.h8d` — 🔵 **Flux : décodeur/encodeur Heathkit FM présents ; conteneurs et exploration restent à faire**
  * **Heath H89 / Zenith Z-89**

    * 5,25" — `.h17`, `.h8d`, `.img`, `.imd` — 🔵 **Flux : décodeur/encodeur Heathkit FM présents ; prise en charge complète des conteneurs à faire**
  * **Zenith Z-100**

    * 5,25" — `.img`, `.imd`, `.td0`

* **Dragon Data**

  * **Dragon 32, Dragon 64**

    * 5,25" — `.dsk`, `.vdk`, `.dmk`, `.jvc`

* **SAM Coupé**

  * **SAM Coupé**

    * 3,5" 800 Ko — `.dsk`, `.mgt`, `.sad`

* **Enterprise**

  * **Enterprise 64, Enterprise 128**

    * 3,5" avec EXDOS — `.img`, `.dsk`

* **Apricot**

  * **Apricot PC**

    * 3,5" 315 Ko / formats propriétaires selon modèle — `.img`, `.dsk`, `.imd`
  * **Apricot F1, F2**

    * 3,5" — `.img`, `.dsk`
  * **Apricot Xen**

    * 3,5" — `.img`, `.dsk`

* **Sord**

  * **Sord M5 avec extension**

    * formats dépendants du contrôleur — `.dsk`
  * **Sord M23 / M68**

    * 5,25" / 8" — `.img`, `.dsk`, `.imd`

* **Data General**

  * **Nova, Eclipse et systèmes associés**

    * 8" / formats FM — `.img`, `.dsk`, `.imd` — 🔵 **Flux : décodeur/encodeur Data General FM présents ; conteneurs et exploration restent à faire**

* **Micral**

  * **Micral N et familles associées**

    * 8" / 5,25" selon configuration — `.img`, `.dsk`, `.imd` — 🔵 **Flux : décodeur/encodeur Micral N FM présents ; conteneurs et exploration restent à faire**

* **Systèmes CP/M génériques**

  * **Machines S-100 et compatibles CP/M**

    * 8" IBM 3740 — `.img`, `.dsk`, `.imd`, `.td0` — 🟡 **Partiel : lecteurs `.imd` et TD0 non compressé présents ; géométries CP/M génériques à compléter**
    * 8" double densité — `.img`, `.dsk`, `.imd`, `.td0` — 🟡 **Partiel : lecteurs `.imd` et TD0 non compressé présents ; géométries CP/M génériques à compléter**
    * 5,25" — `.img`, `.dsk`, `.imd`, `.td0` — 🟡 **Partiel : lecteurs `.imd` et TD0 non compressé présents ; géométries CP/M génériques à compléter**
    * 3,5" — `.img`, `.dsk`, `.imd` — 🟡 **Partiel : lecteur `.imd` présent ; géométries CP/M génériques à compléter**
    * attention : une même taille de fichier peut correspondre à de nombreuses géométries CP/M différentes

### Formats transversaux à accepter sans les rattacher à une seule marque

* **Images sectorielles brutes**

  * `.img` — 🟡 **Partiel : accepté pour les tailles, signatures et géométries reconnues**
  * `.ima` — 🟡 **Partiel : accepté pour les tailles et géométries reconnues**
  * `.dsk` — 🟡 **Partiel : Apple, Amstrad et MSX reconnus ; pas encore un conteneur brut universel**
  * `.raw` lorsqu’il s’agit réellement de secteurs bruts et non de flux

* **Images avec description de géométrie / secteurs**

  * `.imd` — ImageDisk — ✅ **Fait en interne : lecture du conteneur ; exploration selon le format reconnu**
  * `.td0` — TeleDisk — 🟡 **Partiel : variantes ordinaires non compressées seulement**
  * `.cqm` — CopyQM
  * `.fdi` — Formatted Disk Image
  * `.d88`
  * `.d77`
  * `.d98`

* **Formats de préservation spécifiques à certaines familles**

  * `.ipf`
  * `.stx`
  * `.woz` — ✅ **Fait en interne**
  * `.nib` — ✅ **Fait en interne**
  * `.g64`
  * `.g71`
  * `.p64`
  * `.atx`
  * `.dmk`
  * `.udi`

* **Formats compressés qui représentent toujours une image de disquette**

  * `.adz` — ADF compressé
  * `.dms` — Disk Masher System
  * `.imz` — image WinImage compressée

* **À ne pas classer comme formats machine**

  * `.scp` — 🟡 **Partiel : conteneur, visualisation et nombreuses familles de flux pris en charge ; tous les systèmes de fichiers ne sont pas encore reconstructibles**
  * KryoFlux RAW
  * FluxEngine flux
  * Greaseweazle raw flux
  * autres formats génériques de capture de transitions magnétiques

  Ils restent utiles au décodeur et à la visualisation physique, mais ne caractérisent pas une famille de machine particulière.

## Couverture des décodeurs et encodeurs SCP

Ce tableau distingue volontairement trois niveaux : détection de synchronisation ou de marque, extraction d’identité de secteur, et contrôle d’intégrité. La présence d’un décodeur dans la liste ne signifie donc pas automatiquement que tout son contenu logique est déjà décodé.

| Décodeur | Extraction réalisée | Contrôles appliqués | État de couverture synthétique |
|---|---:|---:|---:|
| ISO MFM — Atari ST / IBM PC | Marques FE/FB/F8, C/H/R/N, taille et données | CRC16-CCITT des en-têtes et des données, avec préfixe A1×3 ; état valide, incorrect ou indisponible | Oui, y compris les données supprimées F8 |
| ISO FM | Marques FE/FB/F8, C/H/R/N, taille et données | CRC16-CCITT des en-têtes et des données ; état valide, incorrect ou indisponible | Oui, y compris les données supprimées F8 |
| NorthStar MFM à secteurs matériels | Enregistrement unique marque/identité/données | Piste, secteur et restitution du bloc de 512 octets | Checksum rotatif du bloc ; état valide, incorrect ou indisponible si tronqué |
| Heathkit FM à secteurs matériels | Marques FD d’en-tête et de données associées | Volume, cylindre, secteur et bloc de 256 octets, avec inversion bit à bit | Checksums rotatifs distincts de l’en-tête et des données ; état valide, incorrect ou indisponible |
| Micral N FM à secteurs matériels | Trois octets nuls suivis de la synchronisation FF | Cylindre, secteur et restitution du bloc de 128 octets | Checksum additif avec retenue de fin autour des données ; état valide, incorrect ou indisponible si tronqué |
| Membrain MFM | Marques en-tête/données associées | Cylindre, face, secteur et bloc de 512 octets | CRC16 `0x8005` de l’en-tête et du bloc de données ; état valide, incorrect ou indisponible |
| AED 6200P MFM | Marque d’en-tête C6 et marques de données C0–C3 associées | Cylindre, secteur et bloc de taille variable | CRC-CCITT de l’en-tête et des données ; état valide, incorrect ou indisponible |
| Amiga MFM | Double synchronisation, identité odd/even, cylindre, face, secteur, secteurs restants et 512 octets | Parités XOR odd/even de l’en-tête/label et des données ; état valide, incorrect ou indisponible | Oui pour l’identité, les données et les deux checksums |
| Apple II GCR 13/16 secteurs | Adresse 4-and-4 avec volume, piste et secteur ; blocs de 256 octets décodés en 5-and-3 (DOS 3.2) ou 6-and-2 (DOS 3.3/ProDOS) | XOR de l’adresse et chaîne XOR des symboles GCR ; état valide, incorrect ou indisponible | Oui pour les deux prologues, l’identité, les données, les deux checksums et l’aller-retour avec l’encodeur |
| Apple II Broderbund RWTS18 | Adresse `D5 9D`, piste, secteur physique et six blocs de 768 octets par piste, chacun restituant trois pages de 256 octets | XOR piste/secteur de l'adresse et chaîne XOR des 1 024 symboles GCR de données ; identifiant de données modifiable accepté | Oui pour les six secteurs, les 4 608 octets de la piste, les checksums et l'aller-retour avec l'encodeur |
| Apple Macintosh GCR | En-tête à cinq symboles 6 bits et bloc 6-and-2 de 524 octets | Cylindre 8 bits, face, secteur, 12 octets de tags ignorés et restitution des 512 octets utiles | XOR des quatre champs d’en-tête et quatre checksums de données ; état valide, incorrect ou indisponible |
| Commodore GCR | Synchronisations, blocs `0x08`/`0x07`, piste, secteur, identifiant disque et 256 octets | XOR des cinq champs d’en-tête et XOR des données avec l’octet stocké ; état valide, incorrect ou indisponible | Oui pour l’identité, les données et les deux checksums |
| QD MO5 MFM | En-tête et bloc de données associés | Numéro de secteur sur 16 bits et restitution des 128 octets | Somme 8 bits du marqueur et des données ; état valide, incorrect ou indisponible ; aucun CRC d’en-tête |
| Centurion MFM | En-tête et bloc de données associés, clé et taille 16 bits | Cylindre, secteur et bloc de taille variable | CRC16 XMODEM de l’en-tête et de `taille + données` ; état valide, incorrect ou indisponible ; seule la clé non chiffrée `0` documentée est décodée |
| E-mu Emulator FM | Cylindre, face, secteur unique et restitution des 3584 octets | CRC16 `0x8005` de l’en-tête et des données ; état valide, incorrect ou indisponible | Oui pour l’identité, la cadence FM quadruplée, les données et les deux CRC |
| TYCOM FM | Cylindre, secteur, marques F8–FB et restitution des 128 octets | CRC-CCITT de l’en-tête et des données ; état valide, incorrect ou indisponible | Oui pour l’identité, les marques, la cadence FM quadruplée, les données et les deux CRC |
| DEC RX02 FM/M²FM | Cylindre, face, secteur, code de taille, marques F8–FD ; restitution de 128 octets FM ou 256 octets M²FM pour F9/FD | CRC-CCITT de l’en-tête et des données ; état valide, incorrect ou indisponible | Oui, y compris les données et la substitution DEC M²FM sur 11 bits |
| Arburg | Bloc FM de 2560 octets et bloc système de 3840 octets ; restitution des charges utiles de 2558 ou 3838 octets | Somme additive 16 bits little-endian ; état valide, incorrect ou indisponible | Oui pour les deux encodages, leurs données et leurs sommes |
| Victor 9000 GCR | En-tête GCR de 6 octets, cylindre, secteur et restitution des 512 octets | Contrôle arithmétique de l’en-tête et somme additive 16 bits little-endian des données ; état valide, incorrect ou indisponible | Oui pour l’identité, les données, l’encodage GCR à demi-cellules et les deux contrôles |
| Flux brut | Impulsions courtes et absences longues | Sans objet | Sans objet |

### Familles Greaseweazle ajoutées

| Décodeur | Extraction réalisée | Contrôles appliqués | État |
|---|---|---|---|
| HP MMFM | Cylindre, face, secteur et charge utile de 256 octets, avec inversion des bits et permutation des octets par mots | CRC-CCITT distinct de l'identité et des données | Décodeur et encodeur validés ensemble |
| Data General 2F | Cylindre, face, secteur matériel et charge utile de 512 octets | Checksum Data General `x^16+x^8+1` | Décodeur et encodeur validés ensemble |
| Micropolis MFM | Cylindre, secteur, dix octets constructeur et charge utile de 256 octets | Somme additive à retenue circulaire | Décodeur et encodeur validés ensemble |

### Couverture des encodeurs

Les 24 décodeurs sectoriels possèdent maintenant un encodeur de piste portant le même identifiant. Le registre vérifie cette parité automatiquement. Un test aller-retour construit une piste, la transforme en intervalles de flux, appelle le décodeur correspondant et compare l'identité, l'intégrité et la charge utile obtenues. Cela couvre ISO MFM/FM, Amiga, Apple II DOS 3.2/3.3, Apple II RWTS18, Apple Macintosh, Apple Lisa FileWare, Commodore, Commodore 900, HP MMFM, Data General, Micropolis, Membrain, AED, QD MO5, Centurion, NorthStar, Heathkit, Micral N, E-mu, TYCOM, DEC RX02, Arburg et Victor 9000.

Pour Apple II, l’encodeur sélectionne automatiquement le 5-and-3 avec prologue `D5 AA B5` pour une géométrie de 13 secteurs, et le 6-and-2 avec prologue `D5 AA 96` pour 16 secteurs. Les deux chemins possèdent un test d’aller-retour secteur par secteur.

Le décodeur `raw` n'a pas d'encodeur sectoriel : un flux brut est déjà une suite d'intervalles et ne contient pas de modèle de secteurs à reconstruire. Sa copie ou son écriture relève du conteneur SCP, pas d'un algorithme d'encodage de format.

### Sources de qualification

Les structures Amiga, NorthStar, Heathkit, Micral N, Membrain, AED 6200P, Apple II et Macintosh 6-and-2, Commodore et Victor 9000 sont alignées sur leurs extracteurs homonymes de libhxcfe. Les tests synthétiques reconstruisent les encodages bit à bit, injectent une intégrité valide puis corrompue et vérifient les champs extraits, y compris les restitutions exactes Amiga, Apple II, Apple Macintosh, Commodore, Micral N, Membrain et AED, les tailles variables et marques C0–C3 AED, ainsi que les échantillonnages GCR Macintosh et Victor. Les 18 fichiers `*_track.c` de cette collection HxC possèdent désormais un décodeur dédié dans GW GUI. Leur validation physique exhaustive demande encore un corpus libre.

Pour Membrain, AED 6200P, QD MO5, Centurion, E-mu, Arburg, Victor 9000, TYCOM et DEC RX02, le test d’intégration synthétique ne s’arrête plus à un appel direct du décodeur : il construit un véritable conteneur SCP avec checksum, le relit par `ScpReader`, force le décodeur dans l’inspecteur, puis vérifie que le rendu Skia contient la superposition attendue. Cette preuve couvre la chaîne logicielle complète, mais reste explicitement distincte d’une capture de disquette physique.

## Références des décodeurs SCP

Les décodeurs rares sont implémentés à partir des caractéristiques de leur encodage, sans intégrer le code de HxC dans GW GUI.

### Référence principale

- Projet officiel : <https://github.com/jfdelnero/HxCFloppyEmulator>
- Copie de contrôle étudiée : branche principale consultée le 5 août 2026.
- Fichiers de référence : `libhxcfe/sources/tracks/track_formats/*_track.c` et `libhxcfe/sources/tracks/encoding/`.

### Correspondances actuellement vérifiées

| Décodeur GW GUI | Référence HxC étudiée | Marques utilisées |
|---|---|---|
| Membrain MFM | `membrain_mfm_track.c` | `44 89 55 54`, `44 89 55 4A` |
| AED 6200P MFM | `aed6200p_track.c` | `50 94`, `A5 08` |
| QD MO5 MFM | `qd_mo5_track.c` | cinq répétitions `A9 14`, puis `44 91` ou `91 44` |
| Centurion MFM | `centurion_mfm_track.c` | `91 22 44 89`, `AA AA AA A9` |
| NorthStar MFM | `northstar_mfm_track.c` | sept octets nuls suivis de `FB`, encodés en MFM |
| Heathkit FM | `heathkit_fm_track.c` | trois octets nuls suivis de `FD` inversé bit à bit, encodés en FM |
| Micral N FM | `micraln_fm_track.c` | trois octets nuls, synchronisation `FF`, secteur, cylindre, 128 octets et checksum à retenue de fin |
| E-mu Emulator FM | `emu_emulator_fm_track.c` | `45 45 55 55 45 54 54 45` |
| TYCOM FM | `tycom_fm_track.c` | `55 11 15 54` et marques de données `55 11 14 xx` |
| DEC RX02 M²FM | `dec_rx02_track.c` | `55 11 15 54` et marque M²FM `55 11 15 45` |
| Arburg | `arburg_track.c` | `44 44 44 44 55 55 55 55` et `55 55 55 55 55 24 92 49` |
| Victor 9000 GCR | `victor9k_gcr_track.c` | Marques `55 55 55 55 55 55 11 11` / `55 55 55 55 55 55 11 04`, table GCR 4/5, en-tête de 6 octets, données de 512 octets et somme additive 16 bits little-endian |
| Apple II GCR | `apple2_gcr_track.c` | Prologues `D5 AA 96` / `D5 AA AD`, adresse 4-and-4, table 6-and-2, reconstruction de 256 octets et chaîne XOR |
| Apple Macintosh GCR | `apple_mac_gcr_track.c` | Prologues échantillonnés `D5 AA 96` / `D5 AA AD`, en-tête 6 bits, dénibblisation de 524 octets, 12 octets de tags, 512 octets utiles et quatre checksums |
| Commodore GCR | `c64_gcr_track.c` | Synchronisations, table GCR 4/5, blocs `0x08`/`0x07`, en-tête de 6 octets, données de 256 octets et checksums XOR |
| Amiga MFM | `amiga_mfm_track.c` | Double sync `4489 4489`, identité et données odd/even, bloc de 512 octets et parités XOR séparées |
| ISO MFM/FM | Greaseweazle `codec/ibm` et conventions WD/IBM | Marques FE/FB/F8, géométrie CHRN, CRC-CCITT avec préfixe A1×3 en MFM et sans préfixe en FM |

La comparaison exhaustive confirme que les 18 extracteurs `*_track.c` possèdent un décodeur dédié. Chaque ajout suit la même règle : signature justifiée par la référence primaire, corpus synthétique ciblé et résultat visuel réellement exploitable.

### Références Greaseweazle complémentaires

La comparaison du catalogue HxC avec les codecs Greaseweazle a identifié trois familles supplémentaires :

| Famille GW GUI | Source Greaseweazle étudiée |
|---|---|
| HP MMFM | `src/greaseweazle/codec/hp/hp_mmfm.py` |
| Data General 2F | `src/greaseweazle/codec/datageneral/datageneral.py` |
| Micropolis MFM | `src/greaseweazle/codec/micropolis/micropolis.py` |

Révisions locales de contrôle utilisées le 7 août 2026 :

- Greaseweazle : `26690f89967d519e0106ab9566019a026b920bb4` ;
- HxCFloppyEmulator : `b1eee4cd73391ceaf2ad4ac57e28bf11c91333ba`.

Les algorithmes ont été réimplémentés en C# dans l'architecture de GW GUI. Le code HxC sous GPL n'est pas copié dans le projet. Les encodeurs possèdent chacun leur propre classe et partagent uniquement les primitives réellement communes : construction de cellules, conversion en intervalles, CRC, checksums et inversion de bits.

Pour la lecture AmigaDOS, les structures de blocs ont été vérifiées dans les fichiers `adf_blk.h`, `adf_raw.c`, `adf_dir.c`, `adf_file.c` et `adf_bitm.c` d’ADFlib inclus dans la révision HxC indiquée ci-dessus. GW GUI utilise une réimplémentation C# indépendante : blocs racine et répertoires, chaînes de hachage, blocs de données OFS/FFS, extensions de fichiers, bitmaps, dates et sommes de contrôle.

Pour Apple, les conteneurs, géométries, ordres de secteurs et structures Apple DOS, ProDOS, SOS, MFS et HFS ont également été confrontés à CiderPress2 (<https://github.com/fadden/CiderPress2>), sous licence Apache 2.0. Les structures Lisa ont été vérifiées avec le manuel de référence Lisa OS et LisaFS (<https://lisa.sunder.net/lisafsh/index.html>). GW GUI conserve une implémentation C# indépendante et ne copie aucun code soumis à une licence incompatible.

## Couverture des commandes Greaseweazle

Cette matrice est vérifiée contre la liste `actions` de `greaseweazle/cli.py` dans le dépôt officiel Greaseweazle. Elle indique le parcours choisi dans GW GUI afin qu’aucune commande ne soit perdue dans un écran général surchargé.

| Commande | Emplacement GW GUI | Présentation |
|---|---|---|
| `info` | Options → Diagnostics → Informations | Dialogue ponctuel |
| `read` | Onglet Lecture | Parcours principal avec profils |
| `write` | Onglet Écriture | Parcours principal avec profils et confirmation |
| `convert` | Onglet Conversion | Conversion simple ou multiple |
| `erase` | Onglet Outils | Action destructive avec confirmation |
| `clean` | Onglet Outils | Maintenance avec confirmation du disque de nettoyage |
| `seek` | Options → Diagnostics → Déplacer la tête | Dialogue ponctuel |
| `delays` | Options → Matériel → Temporisations | Dialogue matériel |
| `update` | Options → Matériel → Firmware | Dialogue matériel avec avertissement bootloader |
| `pin` | Options → Matériel → Broches | Dialogue matériel |
| `reset` | Options → Matériel → Réinitialiser | Dialogue matériel |
| `bandwidth` | Options → Diagnostics → Bande passante USB | Dialogue ponctuel |
| `rpm` | Options → Diagnostics → Vitesse RPM | Dialogue ponctuel |
| `align` | Options → Diagnostics → Alignement du lecteur | Dialogue complet de diagnostic mécanique |

`list_ports_windows.py` et `util.py` sont des modules internes des Host Tools, pas des actions proposées par `gw`; ils ne nécessitent donc pas d’écran propre. La détection des ports Windows est néanmoins utilisée par la configuration matérielle.

### Alignement du lecteur

Le dialogue `align` couvre les paramètres publiés par les Host Tools : contrôleur, lecteur, pistes obligatoires, révolutions, nombre de lectures, format, `diskdefs.cfg`, flux brut, faux index ou secteurs matériels, ajustement de vitesse, PLL, densité ou TG43 et inversion flippy. Les combinaisons mutuellement exclusives et les valeurs structurées sont validées avant activation du bouton Exécuter.
