# Référentiel technique pour l’émulation

## Objet et périmètre

Ce document prépare l’émulation interne des familles de machines dont GW GUI sait déjà reconnaître, lire, écrire, reconstruire ou cataloguer les images de disquettes. Il ne décrit pas l’interface utilisateur.

Il sert à répondre à quatre questions :

1. quels composants matériels doivent être reproduits ;
2. quelles machines partagent réellement ces composants ;
3. quels modèles d’une même famille diffèrent matériellement ;
4. quels codes sources existants permettent de vérifier leur comportement.

La liste des machines vient de [la référence des formats de médias](media-formats.md). Les listes d’utilisation des puces sont exhaustives **dans ce périmètre**, puis complétées par les consoles courantes utilisant exactement la même puce ou la même famille. « Compatible » ne signifie pas toujours identique : fréquence, révision, bus, temporisation et composants périphériques restent propres à chaque machine.

Les liens privilégient du code d’émulation, des tests ou des descriptions de registres directement exploitables. Le dépôt [MAME](https://github.com/mamedev/mame) sert d’index transversal : ses composants réutilisables sont sous [`src/devices`](https://github.com/mamedev/mame/tree/master/src/devices) et ses cartes machines sous [`src/mame`](https://github.com/mamedev/mame/tree/master/src/mame).

## Sommaire

- [1. Inventaire transversal des composants](#1-inventaire-transversal-des-composants)
  - [1.1 Processeurs](#11-processeurs)
  - [1.2 Vidéo, graphisme et coprocesseurs](#12-vidéo-graphisme-et-coprocesseurs)
  - [1.3 Son](#13-son)
  - [1.4 Contrôleurs de disquette et E/S indispensables](#14-contrôleurs-de-disquette-et-es-indispensables)
- [2. Fiches par marque et famille](#2-fiches-par-marque-et-famille)
  - [Acorn](#21-acorn) · [Amstrad](#22-amstrad) · [Apple](#23-apple) · [Atari](#24-atari) · [Commodore](#25-commodore)
  - [DEC](#26-dec) · [Epson](#27-epson) · [Fujitsu](#28-fujitsu) · [IBM PC](#29-ibm-pc-et-compatibles)
  - [Kaypro/Osborne](#210-kaypro-et-osborne) · [MSX](#211-msx) · [NEC](#212-nec) · [NorthStar/S-100](#213-northstar-et-s-100cpm)
  - [Oric](#214-oric) · [Sharp](#215-sharp) · [Sinclair](#216-sinclair) · [Tandy/Dragon](#217-tandy--radio-shack-et-dragon)
  - [Texas Instruments](#218-texas-instruments) · [Thomson](#219-thomson) · [Victor](#220-victor--sirius) · [Heath/Zenith](#221-heath--zenith)
  - [SAM Coupé](#222-sam-coupé) · [Enterprise](#223-enterprise) · [Apricot](#224-apricot) · [Sord](#225-sord)
  - [Data General](#226-data-general) · [Micral](#227-micral)
- [3. Règles de conception](#3-règles-de-conception-déduites-des-matériels)
- [4. Sources transversales](#4-sources-transversales-à-exploiter-en-priorité)
- [5. Points restant à confirmer](#5-points-restant-à-confirmer-avant-implémentation)
- Dossiers spécialisés : [Amiga avec PUAE/Libretro](../tasks/emulation/amiga.md) · [Atari avec les cœurs Libretro retenus](../tasks/emulation/atari.md)

## 1. Inventaire transversal des composants

### 1.1 Processeurs

| Puce ou famille | Machines du périmètre et autres machines/consoles qui l’utilisent | Sources de fonctionnement et d’émulation |
|---|---|---|
| MOS 6502 / 6502A / 6502B | Apple II/II+/IIe, Apple III, Acorn BBC Micro/Electron, Commodore PET/VIC-20 ; consoles Atari 2600/5200 (variantes 6507/6502C), NES (Ricoh 2A03 dérivé), PC Engine (HuC6280 dérivé) | [MAME m6502](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m6502), [Visual6502 transistor-level](https://github.com/trebonian/visual6502), [perfect6502](https://github.com/mist64/perfect6502) |
| WDC 65C02 | Apple IIc, IIc Plus et certaines cartes Apple II ; BBC Master (65SC12 proche), Acorn Communicator ; consoles/machines embarquées diverses | [MAME 65C02](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m6502), [fake6502](https://github.com/omarandlorraine/fake6502) |
| WDC 65C816 | Apple IIgs ; Super Nintendo/Super Famicom utilise le Ricoh 5A22 dérivé | [MAME 65C816](https://github.com/mamedev/mame/tree/master/src/devices/cpu/g65816), [65816-tests](https://github.com/TomHarte/ProcessorTests/tree/main/65816) |
| MOS 6510 / 8500 | Commodore 64/C64C/C64G ; C64GS ; famille 6502 avec port d’E/S intégré | [VICE 6510](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/6510), [MAME 6510](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m6502) |
| MOS 7501 / 8501 | Commodore 16, C116, Plus/4 | [VICE Plus/4](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/plus4), [MAME m6502](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m6502) |
| MOS 8502 | Commodore 128/C128D/C128DCR | [VICE C128](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/c128), [MAME m6502](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m6502) |
| CSG 65CE02 / 4510 | Commodore 65 ; dérivé dans le MEGA65 | [MAME m6502/4510](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m6502), [MEGA65 core](https://github.com/MEGA65/mega65-core) |
| Zilog Z80 / compatibles | Amstrad CPC/Plus et PCW, Sinclair Spectrum/+3, MSX1/2/2+, Enterprise, SAM Coupé, Osborne, Kaypro, Epson QX-10, Sharp MZ/X1, NEC PC-8001/8801, Sord M5, beaucoup de CP/M ; consoles ColecoVision, Sega Master System/Game Gear, Neo Geo (audio), Game Boy dérivé | [MAME Z80](https://github.com/mamedev/mame/tree/master/src/devices/cpu/z80), [redcode Z80 tests](https://github.com/redcode/Z80), [FUSE Z80](https://github.com/speccytools/fuse/tree/master/cpu) |
| Z180 / HD64180 | MSX Turbo R comme processeur secondaire R800/Z80 compatible selon modèle, certains systèmes CP/M tardifs ; Amstrad NC et machines embarquées hors périmètre | [MAME Z180](https://github.com/mamedev/mame/tree/master/src/devices/cpu/z180) |
| Motorola 6800 | Systèmes S-100/industriels particuliers ; famille fondatrice, peu présente directement dans le catalogue | [MAME m6800](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m6800) |
| Motorola 6809 / Hitachi 6309 | Dragon 32/64, Tandy Color Computer, Thomson TO/MO (6809E), Fujitsu FM-7/FM-77 ; consoles Vectrex et certains systèmes d’arcade | [MAME m6809](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m6809), [6809 tests](https://github.com/TomHarte/ProcessorTests/tree/main/6809) |
| Motorola 68000 | Amiga 1000/500/500+/600/2000, Atari ST/STF/STFM/STE/Mega ST, Apple Lisa, Macintosh 128K à Plus/SE, Sharp X68000 initial, Sinclair QL (68008), Commodore 900 non concerné ; consoles Mega Drive/Genesis, Neo Geo, Mega-CD et nombreuses bornes | [Musashi](https://github.com/kstenerud/Musashi), [MAME m68000](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m68000), [WinUAE CPU](https://github.com/tonioni/WinUAE/tree/master), [68000 tests](https://github.com/TomHarte/ProcessorTests/tree/main/68000) |
| Motorola 68020/68030/68040 | Amiga 1200 (68EC020), 3000 (68030), 4000 (68030/040), Atari TT/Falcon (68030), Macintosh II/IIx/IIcx/IIci/SE/30/LC/Quadra/Centris selon modèle, X68030 ; consoles CD32 (68EC020) | [Musashi](https://github.com/kstenerud/Musashi), [MAME m68000](https://github.com/mamedev/mame/tree/master/src/devices/cpu/m68000), [WinUAE](https://github.com/tonioni/WinUAE), [Hatari CPU](https://github.com/hatari/hatari/tree/main/src/cpu) |
| ARM2 / ARM3 / ARM6+ | Acorn Archimedes A300/A400/A3000 (ARM2), A4/A5000 (ARM3), Risc PC (ARM6/7 et options StrongARM) ; 3DO utilise ARM60, Game Boy Advance ARM7TDMI, nombreuses consoles ultérieures ARM | [MAME ARM](https://github.com/mamedev/mame/tree/master/src/devices/cpu/arm), [Arculator](https://github.com/sarah-walker-pcem/arculator), [RPCEmu](https://github.com/rpcemu/rpcemu) |
| Intel 8080 / 8085 | Micral et S-100/CP/M selon carte, premiers systèmes industriels ; bornes d’arcade 8080 et Altair/IMS​​AI | [MAME i8085](https://github.com/mamedev/mame/tree/master/src/devices/cpu/i8085), [8080 exerciser](https://github.com/superzazu/8080) |
| Intel 8086 / 8088 | IBM PC 5150/XT, compatibles dont Amstrad PC1512/1640, Epson Equity, Tandy 1000, Apricot PC/Xen selon modèle, Victor 9000 utilise 8088 | [MAME i86](https://github.com/mamedev/mame/tree/master/src/devices/cpu/i86), [8088MPH reenigneering](https://github.com/reenigne/reenigne/tree/master/8088) |
| Intel 80186/286/386/486 et Pentium | IBM PC AT/PS2 et compatibles, Amstrad PC2086/2286/2386, Apricot Xen tardifs, FM Towns (386+), PC-98 selon génération | [MAME x86](https://github.com/mamedev/mame/tree/master/src/devices/cpu/i386), [86Box CPU](https://github.com/86Box/86Box/tree/master/src/cpu), [Bochs CPU](https://github.com/bochs-emu/Bochs/tree/master/bochs/cpu) |
| NEC V20/V30 | PC-98 anciens, certains compatibles PC et consoles/arcades ; WonderSwan utilise V30MZ dérivé | [MAME V30](https://github.com/mamedev/mame/tree/master/src/devices/cpu/nec) |
| Zilog Z8000 (Z8001/Z8002) | Commodore 900 (Z8001), Onyx et autres stations Unix ; certaines bornes Namco utilisent des variantes Z8002 | [MAME Z8000](https://github.com/mamedev/mame/tree/master/src/devices/cpu/z8000), [MAME Commodore 900](https://github.com/mamedev/mame/tree/master/src/mame/commodore) |
| NEC V60/V70 | Stations et systèmes industriels NEC, Sega System 32 et autres bornes ; aucune machine actuellement cataloguée par GW GUI ne doit être supposée V60 sans identification explicite | [MAME V60](https://github.com/mamedev/mame/tree/master/src/devices/cpu/v60) |
| TI TMS9900 | TI-99/4A ; TI-990 et variantes industrielles | [MAME TMS9900](https://github.com/mamedev/mame/tree/master/src/devices/cpu/tms9900), [js99er CPU](https://github.com/Rasmus-M/js99er-angular/tree/master/src/app/emulator/classes) |
| DEC PDP-8 | PDP-8 et MINC-8 selon configuration | [SIMH PDP-8](https://github.com/simh/simh/blob/master/PDP8/pdp8_cpu.c), [MAME PDP-8](https://github.com/mamedev/mame/tree/master/src/devices/cpu/pdp8) |
| DEC PDP-11 / VAX | PDP-11, MINC-11, MicroVAX/VAX ; consoles non concernées | [SIMH PDP-11](https://github.com/simh/simh/tree/master/PDP11), [SIMH VAX](https://github.com/simh/simh/tree/master/VAX), [MAME T11](https://github.com/mamedev/mame/tree/master/src/devices/cpu/t11) |
| Data General Nova / Eclipse | Nova, SuperNOVA, Eclipse et systèmes associés | [SIMH Nova](https://github.com/simh/simh/tree/master/NOVA), [MAME Nova](https://github.com/mamedev/mame/tree/master/src/devices/cpu/nova) |
| National Semiconductor NS32016 | BBC Micro avec coprocesseur 32016, Acorn Cambridge Workstation ; certains S-100/Unix hors périmètre | [MAME NS32000](https://github.com/mamedev/mame/tree/master/src/devices/cpu/ns32000) |

### 1.2 Vidéo, graphisme et coprocesseurs

| Puce | Machines utilisant la puce | Sources de code utiles |
|---|---|---|
| Motorola 6845 et compatibles | Amstrad CPC, PC1512/1640, BBC Micro/Master, IBM CGA/MDA, Tandy compatibles, Apricot, systèmes CP/M et terminaux | [MAME mc6845](https://github.com/mamedev/mame/tree/master/src/devices/video), [Caprice32 CRTC/Gate Array](https://github.com/ColinPitrat/caprice32/tree/master/src) |
| Motorola 6847 VDG | Dragon 32/64, Tandy CoCo 1/2 ; consoles non concernées | [MAME mc6847](https://github.com/mamedev/mame/tree/master/src/devices/video), [XRoar video](https://github.com/stahta01/xroar) |
| Motorola 68486 / MC6847 + SAM | CoCo/Dragon utilisent le couple VDG 6847 et SAM 6883 | [MAME SAM](https://github.com/mamedev/mame/tree/master/src/devices/machine), [XRoar](https://github.com/stahta01/xroar) |
| Hitachi HD63484 ACRTC | Sharp X68000 (avec autres circuits), stations graphiques et cartes PC japonaises | [MAME HD63484](https://github.com/mamedev/mame/tree/master/src/devices/video) |
| NEC µPD7220 GDC | NEC PC-9801 anciens, DEC Rainbow option graphique, Epson QX-10/Valdocs, cartes graphiques PC | [MAME upd7220](https://github.com/mamedev/mame/tree/master/src/devices/video), [MAME Rainbow](https://github.com/mamedev/mame/blob/master/src/mame/dec/rainbow.cpp) |
| TI TMS9918/9928/9929 VDP | MSX1, TI-99/4A, ColecoVision, SG-1000, Coleco Adam, Spectravideo | [MAME TMS9928A](https://github.com/mamedev/mame/tree/master/src/devices/video), [openMSX VDP](https://github.com/openMSX/openMSX/tree/master/src/video) |
| Yamaha V9938 | MSX2 ; Yamaha CX7M2 et machines compatibles | [openMSX VDP](https://github.com/openMSX/openMSX/tree/master/src/video), [MAME V9938](https://github.com/mamedev/mame/tree/master/src/devices/video) |
| Yamaha V9958 | MSX2+ et certains MSX Turbo R | [openMSX VDP](https://github.com/openMSX/openMSX/tree/master/src/video), [MAME V9938/V9958](https://github.com/mamedev/mame/tree/master/src/devices/video) |
| Atari ANTIC + GTIA/CTIA | Atari 400/800/XL/XE/XEGS ; Atari 5200 réutilise ANTIC/GTIA | [MAME Atari 8-bit](https://github.com/mamedev/mame/tree/master/src/mame/atari), [Altirra reference source](https://github.com/gianlucarenzi/Altirra) |
| Atari Shifter + GLUE | Atari ST/Mega ST ; STE ajoute Shifter amélioré et GST MCU ; TT/Falcon remplacent/complètent par TT Shifter/Videl | [Hatari vidéo](https://github.com/hatari/hatari/tree/main/src), [MAME Atari ST](https://github.com/mamedev/mame/tree/master/src/mame/atari) |
| Atari BLiTTER | Mega ST avec option, STE/Mega STE/TT/Falcon | [Hatari blitter](https://github.com/hatari/hatari/blob/main/src/blitter.c), [MAME blitter ST](https://github.com/mamedev/mame/tree/master/src/mame/atari) |
| Amiga Agnus/Alice | Amiga OCS/ECS : Agnus/Fat Agnus ; AGA : Alice. DMA, Copper, Blitter, adressage Chip RAM | [WinUAE custom chipset](https://github.com/tonioni/WinUAE), [vAmiga Agnus](https://github.com/dirkwhoffmann/vAmiga/tree/master/Core/Components/Agnus), [MAME Amiga](https://github.com/mamedev/mame/tree/master/src/mame/amiga) |
| Amiga Denise/Lisa | Amiga OCS/ECS : Denise/Super Denise ; AGA : Lisa. Plans de bits, sprites, couleurs et sortie vidéo | [vAmiga Denise](https://github.com/dirkwhoffmann/vAmiga/tree/master/Core/Components/Denise), [WinUAE](https://github.com/tonioni/WinUAE), [ScriptedAmigaEmulator](https://github.com/naTmeg/ScriptedAmigaEmulator) |
| Commodore VIC / VIC-II | VIC-20 : VIC 6560/6561 ; C64/C128 mode C64 : VIC-II 6567/6569 et 8562/8565 | [VICE VIC-II](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/vicii), [re-enigne VIC-II](https://github.com/mist64/re-vicii) |
| Commodore TED 7360/8360 | C16/C116/Plus4 : vidéo, son, timers et rafraîchissement mémoire intégrés | [VICE TED](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/ted), [MAME TED](https://github.com/mamedev/mame/tree/master/src/devices/video) |
| Commodore VDC 8563/8568 | C128/C128D/C128DCR, affichage 80 colonnes indépendant du VIC-II | [VICE VDC](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/vdc), [MAME MOS8563](https://github.com/mamedev/mame/tree/master/src/devices/video) |
| Apple II logique vidéo discrète | Apple II/II+/IIe/IIc ; accès vidéo partagé avec le 6502, modes texte, lores, hires et double-hires selon modèle | [AppleWin vidéo](https://github.com/AppleWin/AppleWin/tree/master/source), [MAME Apple II](https://github.com/mamedev/mame/tree/master/src/mame/apple) |
| Apple IIgs VGC | Apple IIgs ; modes Apple II compatibles et modes Super Hi-Res | [MAME apple2gs](https://github.com/mamedev/mame/tree/master/src/mame/apple), [GSplus](https://github.com/ivanizag/gsplus) |
| Macintosh/Lisa vidéo discrète | Lisa et premiers Macintosh utilisent framebuffer en RAM et logique discrète ; Macintosh II ajoute cartes NuBus, modèles ultérieurs contrôleurs intégrés | [LisaEm](https://github.com/rayarachelian/lisaem), [MAME Macintosh](https://github.com/mamedev/mame/tree/master/src/mame/apple), [Basilisk II](https://github.com/cebix/macemu) |
| Acorn Video ULA | BBC Micro, Electron (ULA spécifique), Master ; modes vidéo et palette | [BeebEm](https://github.com/stardot/beebem-windows), [MAME BBC](https://github.com/mamedev/mame/tree/master/src/mame/acorn) |
| Acorn VIDC / VIDC20 | Archimedes et A-series : VIDC1 ; Risc PC : VIDC20. Vidéo et audio DMA | [Arculator](https://github.com/sarah-walker-pcem/arculator), [RPCEmu](https://github.com/rpcemu/rpcemu), [MAME Archimedes](https://github.com/mamedev/mame/tree/master/src/mame/acorn) |
| Amstrad Plus ASIC | CPC 464+/6128+ et GX4000 : sprites, palette 4096 couleurs, DMA audio et fonctions de cartouche | [Caprice32 Plus](https://github.com/ColinPitrat/caprice32/tree/master/src), [MAME Amstrad](https://github.com/mamedev/mame/tree/master/src/mame/amstrad) |
| Enterprise DAVE/NICK | Enterprise 64/128 : NICK vidéo, DAVE son/mémoire/interruptions | [ep128emu](https://github.com/istvan-v/ep128emu), [MAME Enterprise](https://github.com/mamedev/mame/tree/master/src/mame/enterprise) |
| SAM Coupé ASIC | SAM Coupé : vidéo, mémoire, clavier et interface système autour du Z80B | [SimCoupe](https://github.com/simonowen/simcoupe), [MAME SAM](https://github.com/mamedev/mame/tree/master/src/mame/samcoupe) |
| Fujitsu MB61VH010 / Towns vidéo | FM Towns : contrôleurs graphiques propriétaires, sprites sur certains modèles, modes planaires et packed-pixel | [Tsugaru](https://github.com/captainys/TOWNSEMU), [MAME FM Towns](https://github.com/mamedev/mame/tree/master/src/mame/fujitsu) |
| Sharp X68000 custom video | Contrôleur CRT, graphic VRAM, text VRAM et sprite controller ; modèles tardifs conservent l’architecture | [MAME X68000](https://github.com/mamedev/mame/blob/master/src/mame/sharp/x68k.cpp), [px68k](https://github.com/libretro/px68k-libretro) |

### 1.3 Son

| Puce | Machines et consoles | Sources de code utiles |
|---|---|---|
| General Instrument AY-3-8910/8912/8913 | Amstrad CPC, MSX, Oric, ZX Spectrum 128/+2/+3, SAM Coupé (SAA1099 à la place, voir plus bas), Apple II via cartes Mockingboard ; consoles Intellivision, Vectrex (8912), arcade | [MAME AY8910](https://github.com/mamedev/mame/blob/master/src/devices/sound/ay8910.cpp), [AYumi](https://github.com/true-grue/ayumi), [jt49 HDL](https://github.com/jotego/jt49) |
| Yamaha YM2149F | Atari ST/STE/TT/Falcon, variantes dans certains MSX et machines japonaises ; compatible fonctionnel AY avec différences de diviseur/enveloppe | [Hatari PSG](https://github.com/hatari/hatari/tree/main/src), [ym2149_audio](https://github.com/dnotq/ym2149_audio), [sndh-player](https://github.com/arnaud-carre/sndh-player/tree/main/AtariAudio) |
| Yamaha YM2203/2608/2612/2151 | PC-88 (YM2203/2608 selon modèle), PC-98 (YM2203/2608), Sharp X1/X68000 (YM2151 pour X68000), FM Towns (YM2612) ; Mega Drive (YM2612), nombreuses arcades | [MAME Yamaha FM](https://github.com/mamedev/mame/tree/master/src/devices/sound), [Nuked-OPN2](https://github.com/nukeykt/Nuked-OPN2), [ymfm](https://github.com/aaronsgiles/ymfm) |
| Yamaha OPLL YM2413 | MSX-MUSIC et certains MSX2+/Turbo R ; Sega Master System japonais | [emu2413](https://github.com/digital-sound-antiques/emu2413), [openMSX sound](https://github.com/openMSX/openMSX/tree/master/src/sound) |
| Yamaha YMF278/OPL4 | MSX-AUDIO évolué et extensions ; non standard sur tous les MSX | [openMSX YMF278](https://github.com/openMSX/openMSX/tree/master/src/sound), [MAME ymf278b](https://github.com/mamedev/mame/tree/master/src/devices/sound) |
| MOS SID 6581/8580 | Commodore 64/C64C/C64G, C128 en mode C64 ; cartes SID externes pour autres machines | [reSID](https://github.com/daglem/reSID), [VICE SID](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/sid), [reSID-fp](https://github.com/daglem/reSID-fp) |
| Commodore TED audio | C16/C116/Plus4, intégré au TED | [VICE TED](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/ted) |
| Amiga Paula | Tous les Amiga classiques A1000 à A4000, CDTV/CD32 ; quatre canaux PCM DMA et fonctions disquette/série/interruptions associées | [vAmiga Paula](https://github.com/dirkwhoffmann/vAmiga/tree/master/Core/Components/Paula), [WinUAE audio](https://github.com/tonioni/WinUAE/blob/master/audio.cpp), [MAME Amiga](https://github.com/mamedev/mame/tree/master/src/mame/amiga) |
| Atari POKEY | Atari 400/800/XL/XE/XEGS et Atari 5200 ; son, clavier, potentiomètres et série | [MAME POKEY](https://github.com/mamedev/mame/tree/master/src/devices/sound), [Altirra POKEY](https://github.com/gianlucarenzi/Altirra) |
| SN76489 / compatibles | TI-99/4A, BBC Micro (SN76489), machines MSX selon extension ; consoles ColecoVision, SG-1000, Master System, Game Gear, Mega Drive en complément | [MAME SN76496](https://github.com/mamedev/mame/tree/master/src/devices/sound), [Nuked-PSG](https://github.com/nukeykt/Nuked-PSG) |
| Philips SAA1099 | SAM Coupé ; cartes Creative CMS/Game Blaster pour IBM PC | [MAME SAA1099](https://github.com/mamedev/mame/tree/master/src/devices/sound), [SimCoupe sound](https://github.com/simonowen/simcoupe) |
| Acorn VIDC audio | Archimedes/A-series/Risc PC, audio DMA intégré au contrôleur vidéo | [Arculator](https://github.com/sarah-walker-pcem/arculator), [RPCEmu](https://github.com/rpcemu/rpcemu) |
| PC Speaker / PIT 8253/8254 | IBM PC/XT/AT/PS2 et compatibles, Amstrad PC, Epson Equity, Tandy, Apricot ; le Tandy 1000 ajoute SN76489, certains modèles DAC | [MAME PIT8253](https://github.com/mamedev/mame/tree/master/src/devices/machine), [DOSBox-X mixer/hardware](https://github.com/joncampbell123/dosbox-x/tree/master/src) |
| Covox/Sound Blaster/AdLib | Extensions PC, non garanties par le modèle de base ; OPL2 YM3812, OPL3 YMF262, DSP Sound Blaster | [DOSBox-X hardware](https://github.com/joncampbell123/dosbox-x/tree/master/src/hardware), [Nuked-OPL3](https://github.com/nukeykt/Nuked-OPL3), [86Box sound](https://github.com/86Box/86Box/tree/master/src/sound) |
| Thomson 1-bit / DAC | TO7/MO5 anciens : buzzer 1 bit ; TO8/TO9 et extensions ajoutent DAC/son selon modèle | [Dcmoto](https://github.com/danielcoulom/dcmoto), [MAME Thomson](https://github.com/mamedev/mame/tree/master/src/mame/thomson) |

### 1.4 Contrôleurs de disquette et E/S indispensables

| Composant | Machines principales | Sources de code utiles |
|---|---|---|
| WD1770/1772/179x | Atari ST/STE, Acorn BBC/Master selon interface, Dragon/CoCo, MSX, Oric, SAM Coupé, Enterprise, nombreux CP/M | [MAME WD FDC](https://github.com/mamedev/mame/tree/master/src/devices/machine), [Hatari FDC](https://github.com/hatari/hatari/tree/main/src), [openMSX WD2793](https://github.com/openMSX/openMSX/tree/master/src/fdc) |
| NEC µPD765 / Intel 8272 | Amstrad CPC/PCW, IBM PC et compatibles, MSX2 selon contrôleur, PC-98, FM Towns, Spectrum +3 | [MAME upd765](https://github.com/mamedev/mame/tree/master/src/devices/machine), [DOSBox-X floppy](https://github.com/joncampbell123/dosbox-x/tree/master/src/hardware), [Caprice32 FDC](https://github.com/ColinPitrat/caprice32/tree/master/src) |
| Intel 8271 | BBC Micro avec DFS d’origine | [MAME i8271](https://github.com/mamedev/mame/tree/master/src/devices/machine), [BeebEm disc](https://github.com/stardot/beebem-windows) |
| Apple Disk II state machine | Apple II/II+/IIe/IIc ; Woz Machine et IWM/SWIM sur IIc/IIgs/Mac/Lisa selon génération | [AppleWin Disk II](https://github.com/AppleWin/AppleWin/tree/master/source), [MAME Apple floppy](https://github.com/mamedev/mame/tree/master/src/devices/machine), [CLK floppy](https://github.com/TomHarte/CLK/tree/master/Components) |
| Apple IWM / SWIM | Apple IIc/IIgs, Lisa 2, Macintosh 128K à modèles 1,44 Mio ; SWIM ajoute MFM 1,44 Mio | [MAME Apple SWIM](https://github.com/mamedev/mame/tree/master/src/devices/machine), [Basilisk II](https://github.com/cebix/macemu), [Shoebill](https://github.com/pruten/shoebill) |
| Amiga Paula + mécanique trackdisk | Tous les Amiga classiques ; Paula gère le flux DMA brut, le logiciel reconnaît le format | [WinUAE disk](https://github.com/tonioni/WinUAE/blob/master/disk.cpp), [vAmiga DiskController](https://github.com/dirkwhoffmann/vAmiga/tree/master/Core/Components/Paula), [SAE disk](https://github.com/naTmeg/ScriptedAmigaEmulator) |
| Commodore 2040/4040/1541/1571/1581 | Ordinateur et lecteur sont deux machines reliées par IEEE-488 ou IEC ; le lecteur possède CPU, RAM, ROM, VIA/CIA et contrôleur propre | [VICE drive](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/drive), [MAME CBM drives](https://github.com/mamedev/mame/tree/master/src/devices/bus/cbmiec), [1541 Ultimate FPGA](https://github.com/GideonZ/1541ultimate) |
| Atari SIO + lecteurs 810/1050/XF551 | Atari 8-bit : contrôleur principal POKEY/SIO, intelligence et FDC dans le lecteur externe | [Altirra](https://github.com/gianlucarenzi/Altirra), [Atari800](https://github.com/atari800/atari800), [MAME Atari floppy](https://github.com/mamedev/mame/tree/master/src/mame/atari) |
| MOS 6522 VIA / 6526 CIA | VIA : PET/VIC-20/1541/Apple III/Macintosh selon fonction ; CIA : C64/C128/Amiga, temporisateurs, ports et série | [MAME 6522](https://github.com/mamedev/mame/tree/master/src/devices/machine), [VICE CIA](https://github.com/VICE-Team/svn-mirror/tree/main/vice/src/cia), [vAmiga CIA](https://github.com/dirkwhoffmann/vAmiga/tree/master/Core/Components/CIA) |
| Motorola 68901 MFP | Atari ST/STE/TT/Falcon et Sharp X68000 ; timers, interruptions, série | [Hatari MFP](https://github.com/hatari/hatari/blob/main/src/mfp.c), [MAME 68901](https://github.com/mamedev/mame/tree/master/src/devices/machine) |
| Z80 PIO/SIO/CTC/DMA | Osborne, Kaypro, Epson, PCW et nombreux CP/M ; combinaisons variables | [MAME Z80 peripherals](https://github.com/mamedev/mame/tree/master/src/devices/machine), [RunCPM](https://github.com/MockbaTheBorg/RunCPM) |

## 2. Fiches par marque et famille

Chaque fiche indique le matériel minimal à reproduire, les différences entre modèles et les sources principales. Les ROM système doivent être fournies séparément quand aucun remplacement libre n’existe.

### 2.1 Acorn

**Matériel commun BBC/Electron :** famille 6502, Video ULA, mémoire partagée, clavier, son SN76489 sur BBC, cassette et extension disquette. **Sources :** [BeebEm](https://github.com/stardot/beebem-windows), [B-em](https://github.com/stardot/b-em), [MAME Acorn](https://github.com/mamedev/mame/tree/master/src/mame/acorn), [CLK](https://github.com/TomHarte/CLK).

- **BBC Model A** : 6502A 2 MHz, 16 Kio, Video ULA, 6845, moins d’E/S et de sockets ROM que le B.
- **BBC Model B** : 32 Kio, VIA système et utilisateur, SN76489, ADC, Tube, Econet/1 MHz bus optionnels ; Intel 8271 ou WD177x selon interface disquette.
- **BBC Master 128** : 65SC12, 128 Kio, MOS 3.x, ACRTC/ULA révisés, DFS/ADFS, contrôleur WD1770 intégré, RTC CMOS.
- **Master Compact** : Master simplifié, clavier séparé, lecteur 3,5 pouces et vidéo/connectique différentes.
- **Electron** : 6502A, ULA très intégrée, pas de 6845 ni SN76489 standard ; contention mémoire importante ; disquette via Plus 3/AP6.
- **Archimedes A300/A400/A3000** : ARM2, MEMC1, VIDC1, IOC, 1–4 Mio selon modèle ; contrôleur WD1772 ; A3000 intègre clavier/boîtier.
- **A4/A5000** : ARM3 avec cache, MEMC1a, VIDC/IOC, davantage de RAM ; IDE et contrôleur disquette HD selon modèle.
- **Risc PC** : ARM610/710/StrongARM selon carte CPU, VIDC20, IOMD, RAM modulaire, architecture à cartes CPU, lecteur HD.

### 2.2 Amstrad

**Sources CPC :** [Caprice32](https://github.com/ColinPitrat/caprice32), [Sugarbox](https://github.com/TFE-Developers/sugarbox), [MAME Amstrad](https://github.com/mamedev/mame/tree/master/src/mame/amstrad). **Sources PCW :** [MAME PCW](https://github.com/mamedev/mame/tree/master/src/mame/amstrad), [JOYCE](https://www.seasip.info/Unix/Joyce/). **PC compatibles :** [86Box](https://github.com/86Box/86Box), [DOSBox-X](https://github.com/joncampbell123/dosbox-x).

- **CPC 464** : Z80A 4 MHz, 64 Kio, 6845 + Gate Array, AY-3-8912, cassette ; DDI-1 ajoute µPD765 et lecteur 3 pouces.
- **CPC 664** : même base, lecteur 3 pouces intégré, ROM BASIC/AMSDOS différente.
- **CPC 6128** : 128 Kio avec bancs mémoire, lecteur intégré.
- **464 Plus / 6128 Plus** : ASIC Plus, sprites, palette 4096 couleurs, DMA audio, cartouches ; 64/128 Kio et cassette/lecteur selon modèle.
- **PCW 8256** : Z80A, 256 Kio, vidéo monochrome pilotée en logiciel, µPD765, lecteur CF2 ; imprimante et clavier font partie de la plateforme.
- **PCW 8512** : 512 Kio et second lecteur CF2DD.
- **PCW 9512** : 512 Kio, lecteur CF2DD et imprimante marguerite, boîtier/ROM révisés.
- **PCW 9256 / 9512+** : lecteur 3,5 pouces, contrôleur et ROM adaptés ; 256/512 Kio.
- **PC1512 / PC1640** : 8086, chipset compatible PC propriétaire ; PC1512 CGA amélioré, PC1640 EGA/MDA selon écran, 360 Kio.
- **PC2086 / 2286 / 2386** : respectivement 8086, 80286, 80386 ; VGA intégré, contrôleurs 720 Kio/1,44 Mio selon révision.

### 2.3 Apple

**Sources :** [AppleWin](https://github.com/AppleWin/AppleWin), [MAME Apple](https://github.com/mamedev/mame/tree/master/src/mame/apple), [GSplus](https://github.com/ivanizag/gsplus), [LisaEm](https://github.com/rayarachelian/lisaem), [IDLE Lisa](https://github.com/ParagPatil/IDLE), [Basilisk II/SheepShaver](https://github.com/cebix/macemu), [Mini vMac](https://github.com/zydeco/minivmac), [Shoebill](https://github.com/pruten/shoebill).

- **Apple II** : 6502 1 MHz, 4–48 Kio, vidéo discrète, Integer BASIC, Disk II optionnel.
- **Apple II+** : ROM Applesoft BASIC, généralement 48 Kio, autostart ROM.
- **Apple IIe** : 64 Kio minimum, MMU/IOU intégrés, 80 colonnes et double-hires avec carte auxiliaire.
- **Apple IIc** : 65C02, 128 Kio, fonctions cartes intégrées, lecteur 5,25, IWM, ports série ; révisions ROM ajoutent SmartPort/3,5 pouces.
- **Apple IIc Plus** : 65C02 à 4 MHz, lecteur 3,5 pouces intégré, alimentation interne, accélération mémoire.
- **Apple IIgs** : 65C816, Mega II pour compatibilité Apple II, VGC Super Hi-Res, Ensoniq 5503 DOC, ADB, IWM ; ROM 00/01/03 et mémoire diffèrent.
- **Apple III / III+** : 6502A 2 MHz, 128–512 Kio, architecture mémoire et E/S incompatibles avec Apple II hors mode émulation, SOS, lecteur 5,25 ; III+ corrige matériel/horloge et porte 256 Kio standard.
- **Lisa 1** : 68000 5 MHz, MMU propriétaire, 1 Mio, vidéo monochrome, deux lecteurs Twiggy 5,25 très spécifiques.
- **Lisa 2 / 2/5** : lecteur Sony 3,5 400 Kio, Profile 5 Mio selon configuration ; Lisa 2/5 conserve châssis disque externe.
- **Lisa 2/10** : disque dur interne Widget 10 Mio et E/S stockage modifiées.
- **Macintosh 128K** : 68000 7,8336 MHz, 128 Kio, framebuffer monochrome, IWM, lecteur 400 Kio, SCC, VIA, son PWM.
- **Macintosh 512K** : même logique avec 512 Kio.
- **512Ke / Plus** : ROM 128 Kio et lecteur 800 Kio ; Plus ajoute SCSI et 1–4 Mio.
- **Macintosh SE** : slots PDS, deux lecteurs ou disque dur, ventilateur ; variantes 800 Kio.
- **SE FDHD / SE/30** : SWIM 1,44 Mio ; SE/30 passe au 68030, MMU et architecture Macintosh II compacte.
- **Macintosh II/IIx/IIcx/IIci** : 68020 puis 68030, couleur via NuBus/intégrée, MMU/FPU variables, SWIM selon révision.
- **Classic / LC** : gamme compacte/économique, 68000 ou 68020/030, vidéo intégrée, bus PDS ; nombreuses sous-révisions.
- **Centris / Quadra** : 68040/68LC040, vidéo intégrée ou NuBus, contrôleurs stockage et fréquences variables.
- **Power Macintosh avec disquette** : PowerPC 601/603/604/G3 selon génération, contrôleurs Apple custom, compatibilité 68k logicielle ; SWIM jusqu’à disparition du lecteur.

### 2.4 Atari

**Sources 8-bit :** [Atari800](https://github.com/atari800/atari800), [Altirra source mirror](https://github.com/gianlucarenzi/Altirra), [MAME Atari](https://github.com/mamedev/mame/tree/master/src/mame/atari). **Sources ST :** [Hatari](https://github.com/hatari/hatari), [MAME Atari ST](https://github.com/mamedev/mame/tree/master/src/mame/atari), [Steem SSE](https://github.com/steem-engine/steem-engine).

- **Atari 400 / 800** : 6502C, ANTIC, CTIA/GTIA selon révision, POKEY, PIA ; 400 clavier membrane/extension limitée, 800 slots et clavier complet.
- **600XL / 800XL** : boîtier XL, OS révisé, BASIC intégré, 16/64 Kio, Freddie sur révisions tardives.
- **1200XL** : 64 Kio, clavier/fonctions console spécifiques, ports différents et compatibilité périphérique imparfaite.
- **65XE / 130XE** : 64/128 Kio ; 130XE ajoute EMMU et mémoire étendue ; boîtiers XE.
- **XEGS** : base 65XE orientée console, clavier détachable, ROM Missile Command.
- **ST / 520ST / 1040ST** : 68000 8 MHz, MMU, GLUE, Shifter, YM2149, MFP, ACIA, WD1772 ; RAM et lecteur externe/interne selon modèle.
- **STF / STFM** : lecteur intégré ; STFM ajoute modulateur RF ; RAM 512 Kio/1 Mio typique.
- **Mega ST** : boîtier clavier séparé, 1–4 Mio, horloge, BLiTTER selon révision.
- **520STE / 1040STE** : palette étendue, scrolling matériel, BLiTTER, audio DMA stéréo, ports joystick analogiques, SIMM selon modèle.
- **Mega STE** : 68000 8/16 MHz avec cache, VME, SCSI/ACSI, boîtier séparé ; lecteur HD sur certaines unités.
- **TT030** : 68030 32 MHz, FPU, MMU, TT-RAM, TT Shifter, VME/SCSI, architecture 32 bits.
- **Falcon 030** : 68030 16 MHz, DSP 56001, Videl, audio DMA/codec, IDE/SCSI, BLiTTER ; bus mémoire et compatibilité ST particuliers.

### 2.5 Commodore

**Sources 8-bit :** [VICE](https://github.com/VICE-Team/svn-mirror), [MAME Commodore](https://github.com/mamedev/mame/tree/master/src/mame/commodore). **Amiga :** [WinUAE](https://github.com/tonioni/WinUAE), [vAmiga](https://github.com/dirkwhoffmann/vAmiga), [Scripted Amiga Emulator](https://github.com/naTmeg/ScriptedAmigaEmulator), [MAME Amiga](https://github.com/mamedev/mame/tree/master/src/mame/amiga). **C900 :** [MAME Commodore 900](https://github.com/mamedev/mame/tree/master/src/mame/commodore).

- **PET/CBM 2001–4000** : 6502, CRTC absent sur premiers PET puis 6545, PIA/VIA, vidéo texte, IEEE-488 ; RAM, clavier et écran diffèrent. Lecteurs 2040/3040/4040 sont des ordinateurs doubles 6502 avec GCR.
- **CBM 8000 + 8050/8250** : CRTC, 80 colonnes ; lecteurs 8050 simple face haute capacité, 8250 double face.
- **VIC-20** : 6502, VIC 6560/6561, VIA x2, 5 Kio, cartouches ; lecteur 1540/1541 externe.
- **C16 / C116 / Plus/4** : 7501/8501 + TED ; 16/16/64 Kio, claviers/connectique différents ; Plus/4 possède ROM bureautique.
- **C64 / C64C / C64G** : 6510/8500, VIC-II, SID, CIA x2 ; révisions de carte, VIC/SID PAL/NTSC et 6581/8580 changent.
- **C128** : 8502 + Z80, VIC-IIe + VDC, SID, 128 Kio ; modes C64/CP-M/C128.
- **C128D / DCR** : lecteur 1571 intégré ; DCR emploie VDC 8568, davantage de VRAM et carte révisée.
- **C65** : 4510, VIC-III, deux SID, 3,5 pouces intégré, 128 Kio extensible ; prototypes aux révisions différentes.
- **Amiga 1000** : 68000, OCS, 256 Kio puis extension, Kickstart chargé depuis disquette, WCS et Agnus initial.
- **Amiga 500** : 68000, OCS/ECS partiel selon révision, 512 Kio Chip + extension trapdoor, Kickstart ROM.
- **Amiga 500+** : ECS complet, 1 Mio Chip, horloge, Kickstart 2.x.
- **Amiga 600** : ECS, 68000, IDE, PCMCIA, 1 Mio Chip, pas de pavé numérique.
- **Amiga 2000** : OCS/ECS selon révision, slots Zorro II, vidéo/genlock et cartes CPU/stockage.
- **Amiga 3000** : 68030, ECS, Zorro III, SCSI, Ramsey/Gary/DMAC, mémoire 32 bits, scandoubler Amber.
- **Amiga 1200** : 68EC020, AGA (Alice/Lisa), Gayle IDE/PCMCIA, 2 Mio Chip.
- **Amiga 4000** : 68030/040 selon modèle, AGA, Zorro III, IDE, Ramsey/Gary révisés ; A4000T ajoute SCSI et tour.
- **CDTV** : base A500/2000 OCS avec CD-ROM, télécommande et contrôleurs spécifiques.
- **CD32** : base A1200 AGA, 68EC020, CD-ROM, Akiko, manettes spécifiques.
- **Commodore 900** : Zilog Z8001, MMU segmentée, vidéo bitmap pilotée par un second Z8001 sur les configurations graphiques, contrôleur GCR propriétaire et COHERENT ; vérifier le driver MAME et les schémas avant implémentation, car les prototypes et cartes diffèrent.

### 2.6 DEC

**Sources :** [SIMH](https://github.com/simh/simh), [MAME DEC](https://github.com/mamedev/mame/tree/master/src/mame/dec).

- **PDP-8 + RX01** : CPU PDP-8 12 bits selon modèle, contrôleur RX8E, RX01 FM simple densité.
- **PDP-8 + RX02** : contrôleur RX8E/RX28 selon système, encodage RX02 particulier.
- **PDP-11 + RX02** : CPU PDP-11 variable, contrôleur RX211/RXV21 et bus Unibus/Q-bus.
- **MINC** : PDP-11 ou PDP-8 selon génération, interfaces laboratoire et contrôleur RX02 ; l’émulation doit sélectionner la vraie base.
- **PDP-11/VAX + RX50** : contrôleurs et bus différents ; RX50 5,25 pouces 10 secteurs/piste.
- **RX33/RX23** : lecteurs compatibles MFM haute/double densité sur MicroVAX/DECstations selon contrôleur.
- **MicroVAX/VAX** : famille CPU VAX, Q-bus, contrôleurs stockage variés ; impossible d’en faire une machine unique générique.

### 2.7 Epson

**Sources :** [MAME Epson](https://github.com/mamedev/mame/tree/master/src/mame/epson), [MAME QX-10](https://github.com/mamedev/mame/tree/master/src/mame/epson), [86Box](https://github.com/86Box/86Box).

- **QX-10** : Z80A, µPD7220 vidéo, banques RAM, contrôleurs Z80 périphériques, µPD765, clavier intelligent ; TPM, CP/M et Valdocs utilisent des géométries/ROM différentes.
- **Equity I/II/III et compatibles** : générations PC/XT/AT ; 8088/8086/286 selon modèle, vidéo et chipset variables. Les lecteurs 360 Kio, 1,2 Mio, 720 Kio et 1,44 Mio dépendent du modèle exact.

### 2.8 Fujitsu

**Sources :** [MAME Fujitsu](https://github.com/mamedev/mame/tree/master/src/mame/fujitsu), [XM7](https://github.com/Artanejp/XM7-for-SDL), [Tsugaru FM Towns](https://github.com/captainys/TOWNSEMU).

- **FM-7** : deux 6809 (principal et graphique), sous-système vidéo indépendant, AY-3-8910 ; lecteur optionnel.
- **FM-77** : mémoire/stockage étendus, lecteurs intégrés selon version.
- **FM-77AV** : vidéo analogique 4096 couleurs, YM2203, sous-CPU et modes vidéo révisés ; nombreuses variantes AV20/40.
- **FM Towns** : 386/486/Pentium selon génération, graphiques propriétaires, YM2612 + RF5C68 PCM, CD-ROM, contrôleur disquette 1,2 Mio ; Marty est la variante console.

### 2.9 IBM PC et compatibles

**Sources :** [86Box](https://github.com/86Box/86Box), [PCem](https://github.com/sarah-walker-pcem/pcem), [DOSBox-X](https://github.com/joncampbell123/dosbox-x), [Bochs](https://github.com/bochs-emu/Bochs), [MAME PC](https://github.com/mamedev/mame/tree/master/src/mame/pc).

- **PC 5150** : 8088 4,77 MHz, 5150 motherboard, 8259/8237/8253/8255, CGA/MDA, contrôleur µPD765 ; RAM et BIOS par révision, lecteurs 160–360 Kio.
- **XT 5160** : 8088, huit slots, disque dur XT, BIOS/carte mère révisés, 360 Kio.
- **AT 5170** : 80286, bus AT 16 bits, 8042 clavier, RTC CMOS, second PIC/DMA, 1,2 Mio.
- **PS/2** : gamme et non modèle unique : 8086 à 486, MCA sur plusieurs modèles, VGA/MCGA, contrôleurs 720 Kio/1,44/2,88 Mio.
- **XT/AT compatibles** : chaque chipset, BIOS, contrôleur vidéo et son doit être décrit par une configuration de carte mère ; ne pas supposer un PC générique universel.
- **286/386/486/Pentium** : caches, MMU, FPU, chipsets ISA/EISA/VLB/PCI, BIOS et temporisations distincts.
- **DMF/XDF/2M** : formats logiciels de disquette, pas de nouvelles machines ; ils exigent surtout un µPD765 suffisamment précis et une géométrie variable.

### 2.10 Kaypro et Osborne

**Sources :** [MAME Kaypro](https://github.com/mamedev/mame/tree/master/src/mame/kaypro), [MAME Osborne](https://github.com/mamedev/mame/tree/master/src/mame/osborne), [RunCPM](https://github.com/MockbaTheBorg/RunCPM).

- **Kaypro II** : Z80, 64 Kio, vidéo texte, SIO/PIO, WD179x, deux lecteurs simple face.
- **Kaypro 4 / 4/84** : double face, carte mère et vidéo/RTC révisées ; 4/84 ajoute modem/horloge selon configuration.
- **Kaypro 10** : disque dur intégré et logique associée.
- **Osborne 1** : Z80, 64 Kio bank-switchée, vidéo 52×24 affichée dans fenêtre 5 pouces, µPD765, lecteurs simple face.
- **Osborne Executive** : écran/vidéo 80 colonnes, davantage de mémoire et lecteurs double densité.

### 2.11 MSX

**Sources :** [openMSX](https://github.com/openMSX/openMSX), [blueMSX](https://github.com/libretro/blueMSX-libretro), [MAME MSX](https://github.com/mamedev/mame/tree/master/src/mame/msx).

- **MSX1** : Z80A, TMS9918/28/29, AY-3-8910, PPI 8255 ; contrôleur disquette et mapper propres au constructeur.
- **MSX2** : V9938, RTC, mémoire vidéo accrue, memory mapper fréquent, WD2793/µPD765 selon machine.
- **MSX2+** : V9958, modes YJK/YAE, souvent OPLL MSX-MUSIC et mapper plus grand.
- **Turbo R** : R800 + Z80 compatible, V9958, PCM, MSX-MUSIC, firmware et commutation CPU spécifiques ; modèles FS-A1ST/GT diffèrent en RAM/MIDI.

### 2.12 NEC

**Sources :** [MAME NEC PC](https://github.com/mamedev/mame/tree/master/src/mame/nec), [QUASI88](https://github.com/libretro/quasi88-libretro), [Neko Project II](https://github.com/AZO234/NP2kai).

- **PC-8001** : Z80, vidéo texte/graphique simple, extensions disquette externes et variantes mkII.
- **PC-8801** : Z80, vidéo couleur, YM2203/2608 selon génération, contrôleurs disquette ; SR/MR/FR/MA/FA/MC ont des modes et fréquences différents.
- **PC-9801** : 8086/V30 puis 286/386, µPD7220 double GDC, bus C-Bus, son optionnel, contrôleurs 2D/2DD/2HD.
- **PC-9821** : 386/486/Pentium, architecture 98 modernisée, VGA-like/PEG C-Bus/PCI selon modèle, 1,2 et 1,44 Mio.

### 2.13 NorthStar et S-100/CP/M

**Sources :** [MAME S-100](https://github.com/mamedev/mame/tree/master/src/mame), [SIMH AltairZ80](https://github.com/simh/simh/tree/master/AltairZ80), [RunCPM](https://github.com/MockbaTheBorg/RunCPM).

- **NorthStar Horizon** : S-100, Z80/8080 selon carte CPU, contrôleur North Star MDS hard-sectored, configurations vidéo/terminal externes.
- **NorthStar Advantage** : Z80A 4 MHz, terminal intégré, bitmap, contrôleur disquette double densité.
- **S-100 générique** : ce n’est pas une machine unique. Il faut choisir CPU, carte mémoire, terminal, contrôleur (Tarbell, North Star, Cromemco…) et géométrie CP/M.

### 2.14 Oric

**Sources :** [Oricutron](https://github.com/pete-gordon/oricutron), [CLK](https://github.com/TomHarte/CLK), [MAME Oric](https://github.com/mamedev/mame/tree/master/src/mame/tangerine).

- **Oric-1** : 6502A, 48 Kio typique, ULA vidéo, AY-3-8912, cassette ; Microdisc externe ajoute WD1793 et ROM.
- **Atmos** : clavier et ROM BASIC corrigés, matériel central proche.
- **Telestrat** : deux VIA, banques/cartouches, contrôleur Microdisc et ports télécom intégrés.

### 2.15 Sharp

**Sources :** [MAME Sharp](https://github.com/mamedev/mame/tree/master/src/mame/sharp), [EmuZ-700](https://github.com/EtchedPixels/EmulatorKit), [px68k](https://github.com/libretro/px68k-libretro).

- **MZ-80** : Z80, vidéo texte, mémoire et moniteur ROM selon K/A/B ; disquette externe variable.
- **MZ-700** : Z80, texte/couleurs, pas de bitmap standard, extensions disquette.
- **MZ-800** : compatibilité MZ-700 plus bitmap 320×200/640×200 et mémoire accrue.
- **Sharp X1** : Z80, sous-CPU E/S, CRTC, PSG puis FM selon Turbo, lecteur 5,25.
- **X68000** : 68000 puis 68030, contrôleurs vidéo/sprites, YM2151 + OKI MSM6258, MFP, DMA, SCC, deux lecteurs 1,2 Mio ; ACE/PRO/SUPER/XVI/Compact/030 diffèrent en CPU, fréquence, SCSI et format physique.

### 2.16 Sinclair

**Sources :** [Fuse](https://github.com/speccytools/fuse), [MAME Sinclair](https://github.com/mamedev/mame/tree/master/src/mame/sinclair), [QL emulators Q68](https://github.com/mist-devel/mist-board/tree/master/cores/ql).

- **Spectrum 48K** : Z80A, ULA, 48 Kio, beeper 1 bit ; Beta Disk ajoute WD1793 et TR-DOS.
- **Spectrum 128K** : 128 Kio bank-switchée, AY-3-8912, ULA/ROM révisées.
- **+2** : cassette intégrée ; modèles gris proches 128K, +2A/+2B proches +3 avec ASIC.
- **+3** : ASIC Amstrad, µPD765 et lecteur 3 pouces, ROM +3DOS ; temporisations différentes.
- **Sinclair QL** : 68008, ZX8301/ZX8302, 128 Kio, vidéo, son et microdrives ; interfaces floppy sont tierces et doivent être configurées par modèle de contrôleur.

### 2.17 Tandy / Radio Shack et Dragon

**Sources :** [MAME Tandy](https://github.com/mamedev/mame/tree/master/src/mame/trs), [sdltrs](https://github.com/TimothyPMann/sdltrs), [XRoar](https://github.com/stahta01/xroar), [86Box Tandy](https://github.com/86Box/86Box).

- **TRS-80 Model I** : Z80, 4–48 Kio, vidéo texte, Expansion Interface et WD1771 pour disquette.
- **Model II** : Z80, bus/vidéo 80 colonnes, lecteurs 8 pouces, architecture incompatible Model I.
- **Model III** : intégration du Model I, WD179x, vidéo/ROM révisées.
- **Model 4** : Z80A 4 MHz, 64/128 Kio, mode 80×24 et compatibilité Model III.
- **CoCo 1/2** : 6809E, SAM + 6847, PIA, cassette ; contrôleur disquette WD17xx externe.
- **CoCo 3** : 6809E, GIME remplaçant SAM/VDG, jusqu’à 512 Kio et nouveaux modes vidéo.
- **Dragon 32/64** : 6809E, SAM + 6847, PIA ; ROM/clavier/E/S différents du CoCo, Dragon 64 ajoute RAM et port série.
- **Tandy 1000** : compatible PCjr, 8088 puis 286 selon modèle, vidéo Tandy, SN76489, DMA/IRQ et contrôleurs disquette variant entre EX/HX/SX/TL/RL.

### 2.18 Texas Instruments

**Sources :** [js99er](https://github.com/Rasmus-M/js99er-angular), [Classic99](https://github.com/tursilion/classic99), [MAME TI](https://github.com/mamedev/mame/tree/master/src/mame/ti).

- **TI-99/4A** : TMS9900, TMS9918A/9929A, SN76489, TMS9901, 16 Kio VRAM ; Peripheral Expansion Box et contrôleur disquette TMS9900/WD1771 selon carte.
- **TI Professional Computer** : 8088, vidéo et bus propriétaires, contrôleur disquette ; compatibilité IBM PC limitée.

### 2.19 Thomson

**Sources :** [Dcmoto](https://github.com/danielcoulom/dcmoto), [MAME Thomson](https://github.com/mamedev/mame/tree/master/src/mame/thomson), [Theodore](https://github.com/Zlika/theodore).

- **TO7** : 6809E, 8/22 Kio selon extension, vidéo Thomson, crayon optique, clavier membrane.
- **TO7/70** : 64 Kio, vidéo/palette améliorée et BASIC cartouche.
- **MO5** : 6809E, 48 Kio, architecture mémoire/ROM différente des TO, clavier gomme.
- **MO6** : 128 Kio, modes vidéo enrichis, BASIC intégré et compatibilité MO5.
- **TO8 / TO8D** : 256 Kio, nombreux modes vidéo, contrôleur mémoire ; TO8D intègre le lecteur.
- **TO9 / TO9+** : orientation professionnelle, clavier séparé, 128/512 Kio selon modèle, lecteurs et ROM distincts.

### 2.20 Victor / Sirius

**Sources :** [MAME Victor 9000](https://github.com/mamedev/mame/tree/master/src/mame/victor), [FluxEngine Victor](https://github.com/davidgiven/fluxengine/tree/master/src/formats).

- **Victor 9000 / Sirius 1** : 8088, jusqu’à 896 Kio, vidéo haute résolution, VIA/SIO, lecteurs GCR à vitesse variable commandés par microcontrôleur ; configurations simple/double lecteur et régions ROM différentes.

### 2.21 Heath / Zenith

**Sources :** [MAME Heathkit](https://github.com/mamedev/mame/tree/master/src/mame/heathkit), [heathkit H89 emulator](https://github.com/mamedev/mame/tree/master/src/mame/heathkit), [MAME Z100](https://github.com/mamedev/mame/tree/master/src/mame/zenith).

- **H8** : Intel 8080A, bus Heath, console octale ; H17 hard-sectored contrôlé par logiciel.
- **H89 / Z-89** : Z80, terminal H19 intégré, contrôleurs H17 puis soft-sectored selon carte.
- **Zenith Z-100** : 8085 + 8088, bus S-100, vidéo couleur bitmap, µPD765 ; non compatible IBM PC au niveau matériel.

### 2.22 SAM Coupé

**Sources :** [SimCoupe](https://github.com/simonowen/simcoupe), [MAME SAM Coupé](https://github.com/mamedev/mame/tree/master/src/mame/samcoupe).

- **SAM Coupé** : Z80B 6 MHz, ASIC SAM, 256/512 Kio, SAA1099, WD1772, lecteurs 3,5 pouces ; révisions ASIC/ROM et extensions mémoire/Atom HDD à représenter séparément.

### 2.23 Enterprise

**Sources :** [ep128emu](https://github.com/istvan-v/ep128emu), [MAME Enterprise](https://github.com/mamedev/mame/tree/master/src/mame/enterprise).

- **Enterprise 64** : Z80A 4 MHz, 64 Kio, NICK, DAVE, AY absent car DAVE produit le son.
- **Enterprise 128** : 128 Kio, même chipset, ROM EXOS/BASIC selon région.
- **EXDOS** : extension WD1772 avec ROM et lecteur 3,5 ; doit être modélisée comme périphérique.

### 2.24 Apricot

**Sources :** [MAME Apricot](https://github.com/mamedev/mame/tree/master/src/mame/apricot), [MAME PC](https://github.com/mamedev/mame/tree/master/src/mame/pc).

- **Apricot PC** : 8086, architecture non totalement IBM-compatible, écran/clavier intelligents, lecteurs 3,5 315 Kio.
- **F1/F2** : modèles plus compacts, stockage et mémoire différents ; F2 double lecteur selon configuration.
- **Xen** : 286 puis générations ultérieures, bus/vidéo propriétaires avant convergence PC-compatible ; sélectionner le modèle exact.

### 2.25 Sord

**Sources :** [MAME Sord](https://github.com/mamedev/mame/tree/master/src/mame/sord), [MAME computers](https://github.com/mamedev/mame/tree/master/src/mame).

- **Sord M5** : Z80, TMS9918A, SN76489, 4 Kio ; extension disquette et DOS selon contrôleur.
- **Sord M23** : Z80, CP/M, vidéo/contrôleur disquette propriétaires.
- **Sord M68** : famille 68000 orientée Unix/CP-M 68K, configurations terminal et stockage variables.

### 2.26 Data General

**Sources :** [SIMH Nova](https://github.com/simh/simh/tree/master/NOVA), [MAME Data General](https://github.com/mamedev/mame/tree/master/src/mame/dg).

- **Nova** : CPU 16 bits à accumulateurs, nombreuses générations de châssis et contrôleurs.
- **Eclipse** : extensions d’instructions/mémoire, modèles 16/32 bits ; contrôleur disquette à sélectionner par système. Une « DLL Data General » devra exposer plusieurs cartes CPU et contrôleurs, pas une machine fixe.

### 2.27 Micral

**Sources :** [MAME Micral](https://github.com/mamedev/mame/tree/master/src/mame), [Micral N FPGA/software references](https://github.com/search?q=Micral+N+emulator&type=repositories).

- **Micral N** : Intel 8008, bus Pluribus, cartes mémoire/E/S et console ; les configurations à disquette sont postérieures ou associées à la famille Micral.
- **Micral 80-20/90-20 et familles CP/M** : 8080/Z80 selon modèle, contrôleurs 8/5,25 pouces variables. Il faut identifier le modèle depuis la géométrie/corpus avant de choisir la carte émulée.

### 2.28 NEC/Sharp/Fujitsu et autres machines japonaises : règle de variantes

Les désignations PC-88, PC-98, X1, X68000, FM-7 et FM Towns couvrent chacune de nombreuses révisions incompatibles sur les fréquences, le son, la vidéo et le contrôleur de disquette. Une configuration d’émulation doit conserver au minimum : modèle exact, révision ROM, fréquence CPU, VDP/GDC, carte sonore, FDC, type et rotation du lecteur.

## 3. Règles de conception déduites des matériels

1. **Une famille de fichiers n’identifie pas toujours une machine.** Une image FAT12 peut venir d’un IBM PC, d’un Atari ST ou d’un MSX ; une image CP/M ne choisit ni CPU ni contrôleur.
2. **Le lecteur peut être une machine autonome.** C’est obligatoire pour les Commodore IEC/IEEE-488 et utile pour Atari SIO.
3. **Le même CPU ne garantit aucun partage de boucle temporelle.** Le 68000 d’un Amiga subit le DMA des custom chips ; celui d’un ST partage le bus avec le Shifter ; celui d’un Macintosh suit une autre logique vidéo.
4. **Les révisions de puces sont des paramètres de compatibilité.** SID 6581/8580, VIC-II PAL/NTSC, Agnus/Denise, GTIA/CTIA, CRTC Amstrad et ROM système changent le résultat.
5. **Les composants critiques d’une machine doivent rester dans la même DLL.** Les appels CPU/bus/chipset ont lieu à très haute fréquence. Les frontières publiques portent sur des trames vidéo, tampons audio, entrées, médias et états.
6. **Les sources spécialisées valident MAME, et réciproquement.** Pour chaque machine, confronter au moins deux implémentations lorsque c’est possible.
7. **Les tests doivent partir de ROMs de diagnostic et de traces.** Les tests unitaires CPU ne suffisent pas ; il faut des tests de bus, d’interruptions, de DMA, de vidéo par ligne et de contrôleur de disquette.

## 4. Sources transversales à exploiter en priorité

- [MAME](https://github.com/mamedev/mame) : composants et cartes de presque toutes les machines du catalogue.
- [Clock Signal / CLK](https://github.com/TomHarte/CLK) : Apple II, Macintosh, Atari ST, Amstrad CPC, MSX, Oric, Electron et contrôleurs de disquette avec architecture moderne.
- [TomHarte ProcessorTests](https://github.com/TomHarte/ProcessorTests) : vecteurs de tests CPU, notamment 6502, 68000, 6809 et Z80 selon dossiers disponibles.
- [SIMH](https://github.com/simh/simh) : DEC, Data General, Altair/S-100 et mini-ordinateurs.
- [FluxEngine formats](https://github.com/davidgiven/fluxengine/tree/master/src/formats) : comportement de nombreux formats physiques rares.
- [Greaseweazle disk definitions](https://github.com/keirf/greaseweazle/tree/master/src/greaseweazle/data) : géométries et formats déjà liés à GW GUI.
- [86Box](https://github.com/86Box/86Box) : cartes mères, chipsets, contrôleurs, vidéo et son IBM PC compatibles.
- [DOSBox-X](https://github.com/joncampbell123/dosbox-x) : périphériques PC et contrôleur disquette, à confronter à 86Box pour la précision matérielle.

## 5. Points restant à confirmer avant implémentation

- modèle exact de Commodore 900 visé et CPU de chaque prototype ;
- modèles Micral réellement représentés par les formats déjà décodés ;
- cartes contrôleur utilisées par NorthStar, Sord, S-100 et CP/M générique pour chaque corpus ;
- sous-modèles PC-88/98, X1/X68000, FM-7/FM Towns et Apricot à prioriser ;
- révisions de ROM et de chipset nécessaires pour démarrer chaque image de validation ;
- disponibilité de corpus de diagnostic redistribuables ou générables pour chaque famille.

Ces inconnues ne bloquent pas l’architecture, mais elles interdisent de prétendre qu’un seul « modèle générique » reproduira toutes les images d’une marque.
