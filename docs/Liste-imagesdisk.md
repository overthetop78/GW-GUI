# Familles d’ordinateurs et formats d’images de disquettes

## État dans GW GUI

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

# Formats transversaux à accepter sans les rattacher à une seule marque

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
