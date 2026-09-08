# Catalogue global des ordinateurs et consoles

## Objet

Ce catalogue sert à choisir les futures familles de GW GUI. Il couvre les principales machines commerciales et regroupe dans une même ligne les variantes utilisant la même architecture. Les variantes devront néanmoins rester des modèles distincts dans l'application lorsque leurs ROM, fréquences, claviers ou périphériques diffèrent.

La colonne « Recréation interne » mesure uniquement la difficulté pour Codex d'écrire notre propre émulateur : elle ne mesure ni l'installation ni l'intégration d'un moteur existant. Chaque ligne a été contrôlée le 8 septembre 2026 dans les sources ou la documentation primaire du moteur cité. Une famille est scindée dès que ses machines ne disposent pas de sources au même niveau d'achèvement.

| Niveau | Critère de recréation |
|---|---|
| Facile | Sources complètes en C# |
| Faisable | Sources complètes en C ou C++ ne demandant pas d'adaptation complexe |
| Intermédiaire | Sources complètes dans un autre langage et demandant une adaptation |
| Un peu plus dur | Sources complètes demandant une grosse adaptation, ou plusieurs sources complètes à réunir |
| Complexe | Sources incomplètes, mais suffisantes pour obtenir un émulateur fonctionnel |
| Très complexe | Sources incomplètes et insuffisantes pour obtenir directement un émulateur fonctionnel |
| Extrêmement complexe | Aucune source exploitable, ou seulement très peu de sources |

## Ordinateurs

| Famille et machines | Cœurs libretro utilisables | Émulateurs externes adaptables en DLL | EXE pilotable | Recréation interne |
|---|---|---|---|---|
| Acorn Atom | MAME Current | MAME | MAME | Un peu plus dur — pilote complet à extraire du framework MAME |
| Acorn BBC Micro et Master | b2, MAME Current | [BeebEm](https://github.com/stardot/beebem-windows), MAME | BeebEm | Faisable — sources C/C++ complètes de BeebEm |
| Acorn Electron | MAME Current | [Elkulator](https://github.com/dmcoles/elkulator), MAME | Elkulator | Faisable — sources C/C++ complètes d'Elkulator ; la liste officielle Libretro ne donne `b2` que pour BBC Micro |
| Acorn Archimedes et Risc PC | MAME Current | RPCEmu, Arculator, MAME | RPCEmu, Arculator | Un peu plus dur — RPCEmu et Arculator fournissent déjà les composants complexes |
| Amstrad CPC464, 664, 6128, 464+/6128+ | Caprice32, ep128emu, MAME Current | Caprice32, CPCEC | Caprice32, Retro Virtual Machine, JavaCPC | Faisable — sources C/C++ complètes et directement adaptables |
| Amstrad PCW 8256/8512/9256/9512/9512+/10 | MAME Current | Joyce, MAME | Joyce, MAME | Faisable — sources complètes disponibles |
| Amstrad PcW16 | MAME Current partiel | MAME | MAME | Complexe — sources incomplètes, mais le pilote MAME fournit une base fonctionnelle |
| Amstrad NC100/150/200 | MAME Current | nc100em, MAME | nc100em, MAME | Faisable — sources C disponibles |
| Amstrad PC1512/1640/PC200/PPC512/640 | MAME Current, VirtualXT, DOSBox Pure | MAME, PCem, VirtualXT | PCem, MAME | Un peu plus dur — MAME et PCem implémentent déjà l'essentiel du matériel |
| Amstrad PC2086 et PC3086 | DOSBox Pure en approximation | [PCem](https://github.com/sarah-walker-pcem/pcem) | PCem | Un peu plus dur — les deux modèles sont explicitement implémentés dans les sources complètes de PCem, mais leur extraction demande une grosse adaptation |
| Amstrad PC2386 | MAME Current | [pilote PC2386 de MAME](https://github.com/mamedev/mame/blob/master/src/mame/pc/at.cpp) | MAME | Un peu plus dur — pilote C++ complet, mais fortement intégré au framework MAME |
| Amstrad PC2286 et PC3286 | DOSBox Pure en approximation | PCem/86Box générique, sans modèle exact vérifié | PCem, 86Box | Complexe — les composants PC sont complets, mais l'implémentation exacte de ces deux Amstrad reste à écrire |
| Apple I | MAME Current | Pom1, MAME | Pom1 | Faisable — sources C complètes |
| Apple II/II+/IIe | AppleWin, MAME Current | [AppleWin](https://github.com/AppleWin/AppleWin), MAME | AppleWin, MAME | Faisable — sources C++ complètes d'AppleWin |
| Apple IIc/IIc+ | MAME Current | MAME | MAME | Un peu plus dur — AppleWin ne les prend explicitement pas en charge ; pilote MAME à extraire |
| Apple IIGS | MAME Current | GSport, GSplus, MAME | GSport, GSplus | Faisable — émulateurs C/C++ complets distincts d'AppleWin |
| Apple III | MAME Current | MAME | MAME | Un peu plus dur — pilote complet à extraire du framework MAME |
| Apple Lisa | MAME Current | [LisaEm](https://github.com/rayarachelian/lisaem), MAME | LisaEm, MAME | Faisable — LisaEm fournit un émulateur entièrement fonctionnel et ses sources C/C++ |
| Macintosh 68k | Mini vMac, MAME Current | [Mini vMac et Basilisk II](https://github.com/cebix/macemu), MAME | Mini vMac, Basilisk II | Un peu plus dur — sources complètes, mais deux moteurs couvrent des générations différentes |
| Macintosh PowerPC classique | MAME Current partiel | [SheepShaver](https://github.com/cebix/macemu), DingusPPC, QEMU | SheepShaver, QEMU | Un peu plus dur — sources complètes et fonctionnelles, mais adaptation volumineuse |
| Atari 400/800/XL/XE/XEGS | Atari800, MAME Current | Atari800, Altirra | Altirra | Faisable — sources C/C++ complètes |
| Atari ST/STF/STFM/Mega ST | Hatari, hatariB, MAME Current | Hatari, Steem SSE | Hatari, Steem SSE | Faisable — sources C/C++ complètes |
| Atari STE/Mega STE/TT/Falcon | Hatari, hatariB, MAME Current | Hatari | Hatari | Faisable — Hatari fournit des sources C complètes |
| Atari Portfolio | MAME Current | MAME | MAME | Un peu plus dur — pilote complet, mais à extraire du framework MAME |
| Camputers Lynx 48/96/128 | MAME Current | Jynx, MAME | Jynx | Faisable — sources C/C++ complètes |
| Coleco Adam | MAME Current, blueMSX partiel | MAME, AdamEm | MAME | Faisable — sources C/C++ complètes |
| Commodore KIM/PET/VIC/C64/C128/264/CBM-II/C65/LCD/PC/TV Game | VICE, MAME Current, DOSBox Pure selon famille | VICE, Xemu, PCem, 86Box | VICE, Xemu, PCem | Voir le catalogue Commodore détaillé |
| Dragon 32/64 et Tano Dragon | XRoar, MAME Current | XRoar, MAME | XRoar | Faisable — sources C/C++ complètes |
| Enterprise 64/128 | ep128emu, MAME Current | ep128emu | ep128emu | Faisable — sources C++ complètes |
| Fujitsu FM-7/FM-77/FM77AV | MAME Current | XM7, MAME | XM7 | Un peu plus dur — XM7 et MAME contiennent les implémentations |
| Fujitsu FM Towns/Marty | MAME Current | Tsugaru, MAME | Tsugaru, UNZ | Un peu plus dur — Tsugaru fournit déjà x86, CD, audio et vidéo |
| IBM PC/XT/AT et compatibles | DOSBox Pure/Core, VirtualXT, MAME Current | VirtualXT, PCem, 86Box, MAME | PCem, 86Box, DOSBox-X | Un peu plus dur — nombreuses bases complètes réutilisables |
| IBM PCjr et PS/2 | MAME Current | MAME, 86Box | MAME, 86Box | Un peu plus dur — sources complètes réparties entre MAME et 86Box |
| Jupiter Ace | MAME Current | EightyOne, MAME | EightyOne | Faisable — sources C/C++ complètes |
| Laser/VTech VZ200/300 et Laser 200/310 | MAME Current | MAME, JVZ200 | MAME | Un peu plus dur — plusieurs sources à réunir ou pilote MAME à extraire |
| Mattel Aquarius I/II | MAME Current | MAME, Virtual Aquarius | MAME | Faisable — sources C/C++ complètes |
| Memotech MTX500/512/RS128 | MAME Current | MEMU, MAME | MEMU | Faisable — sources C/C++ complètes |
| MSX1/MSX2/MSX2+/turboR | blueMSX, fMSX, MAME Current | openMSX, blueMSX | openMSX | Un peu plus dur — openMSX et blueMSX implémentent déjà les extensions |
| NEC PC-6001/6601 | MAME Current | PC6001VX, MAME | PC6001VX | Faisable — sources C/C++ complètes |
| NEC PC-8001/8801 | QUASI88, MAME Current | QUASI88, MAME | QUASI88 | Faisable — sources C/C++ complètes |
| NEC PC-9801 | Neko Project II Kai, MAME Current | NP2kai, DOSBox-X, MAME | DOSBox-X, Neko Project | Un peu plus dur — moteurs complets disponibles |
| Oric-1/Atmos/Telestrat | Oricutron, MAME Current | Oricutron | Oricutron | Faisable — sources C/C++ complètes |
| Philips P2000 | M2000, MAME Current | M2000, MAME | MAME | Faisable — sources C/C++ complètes |
| Radio Shack TRS-80 | MAME Current | MAME | MAME | Un peu plus dur — pilotes complets à extraire du framework MAME |
| Tandy Color Computer 1/2/3 et MC-10 | XRoar, MAME Current | [XRoar](https://github.com/stahta01/xroar), MAME | XRoar | Faisable — sources C complètes de XRoar |
| SAM Coupé | SimCoupe, MAME Current | SimCoupe | SimCoupe | Faisable — sources C++ complètes |
| Sega SC-3000 | MAME Current, blueMSX partiel | MAME | MAME | Un peu plus dur — sources complètes à extraire et adapter depuis MAME |
| Sharp MZ-80 et variantes MZ-80K/A/B | MAME Current | MAME, EmuZ | MAME, EmuZ | Un peu plus dur — pilotes fonctionnels à extraire du framework MAME |
| Sharp MZ-700 | MAME Current | [pilote MZ-700 de MAME](https://github.com/mamedev/mame/blob/master/src/mame/sharp/mz700.cpp), [mz800emu](https://github.com/michalhucik/mz800emu) | MAME, mz800emu | Faisable — mz800emu fournit des sources C/C++ autonomes et fonctionnelles |
| Sharp MZ-800 et MZ-1500 | MAME Current non fonctionnel | [pilote MAME incomplet](https://github.com/mamedev/mame/blob/master/src/mame/sharp/mz700.cpp), [mz800emu](https://github.com/michalhucik/mz800emu) | mz800emu | Faisable — malgré le pilote MAME incomplet, mz800emu fournit une autre implémentation C/C++ fonctionnelle complète |
| Sharp MZ-2500 | MAME Current | [pilote MZ-2500 de MAME](https://github.com/mamedev/mame/blob/master/src/mame/sharp/mz2500.cpp), [EmuZ-2500](https://github.com/SHARPENTIERS/Common-Source-Code-Project) | MAME, EmuZ-2500 | Faisable — l'incomplétude du pilote MAME n'impose pas la note : EmuZ-2500 apporte une seconde implémentation C/C++ autonome et fonctionnelle |
| Sharp X1/X1 Turbo | X Millennium, MAME Current | X Millennium, MAME | X Millennium | Faisable — sources C/C++ complètes |
| Sharp X68000 | PX68k, MAME Current | PX68k, XM6 TypeG, MAME | XM6 TypeG | Un peu plus dur — chipset déjà présent dans plusieurs sources |
| Sinclair ZX80/ZX81 | EightyOne, MAME Current | EightyOne, MAME | EightyOne | Faisable — sources C/C++ complètes |
| Sinclair ZX Spectrum 16/48/128, +2/+3 | Fuse, ep128emu, MAME Current | Fuse, EightyOne | Fuse, Retro Virtual Machine | Faisable — sources C/C++ complètes |
| Sinclair QL | MAME Current | Q-emuLator, MAME | Q-emuLator | Un peu plus dur — sources complètes à adapter depuis plusieurs projets |
| Spectravideo SVI-318/328/728 | blueMSX, MAME Current | openMSX, MAME | openMSX | Faisable — sources C/C++ complètes et réutilisables |
| Tandy 1000 | MAME Current, DOSBox Pure en approximation | 86Box, PCem, MAME | 86Box, PCem | Un peu plus dur — modèles et vidéo/audio déjà codés dans PCem/86Box/MAME |
| Texas Instruments TI-99/4 et 4A | MAME Current | Classic99, MAME | Classic99 | Faisable — sources C/C++ complètes |
| Thomson MO5/MO6/TO7/TO8/TO9 | Theodore, MAME Current | DCMOTO, MAME | DCMOTO | Faisable — sources complètes disponibles |
| Timex Sinclair 1000/1500/2068 | Fuse, EightyOne, MAME Current | Fuse, EightyOne | Fuse | Faisable — sources C/C++ complètes |
| Videoton TVC | ep128emu, MAME Current | ep128emu | ep128emu | Faisable — sources C++ complètes |

## Consoles de salon

| Famille et machines | Cœurs libretro utilisables | Émulateurs externes adaptables en DLL | EXE pilotable | Recréation interne |
|---|---|---|---|---|
| 3DO | Opera, 4DO | Opera/FreeDO | 4DO | Un peu plus dur — Opera/FreeDO fournit déjà les ASIC émulés |
| Amstrad GX4000 | Caprice32, MAME Current | Caprice32 | Retro Virtual Machine | Faisable — sources C/C++ complètes |
| Atari 2600 | Stella | Stella | Stella | Faisable — sources C++ complètes |
| Atari 5200 | Atari800, MAME Current | Atari800 | Atari800 | Faisable — sources C complètes |
| Atari 7800 | ProSystem, A7800, MAME Current | ProSystem, A7800 | A7800 | Faisable — sources C/C++ complètes |
| Atari Jaguar et Jaguar CD | Virtual Jaguar | [Virtual Jaguar](https://github.com/libretro/virtualjaguar-libretro) | Virtual Jaguar | Faisable — le même cœur C/C++ libre couvre explicitement cartouches et CD |
| Atari Pong, Pong Doubles et jeux TTL disposant d'une netlist MAME active | MAME Current | [netlists MAME Atari](https://github.com/mamedev/mame/blob/master/src/mame/atari/pong.cpp) | MAME | Un peu plus dur — circuits complets à extraire et adapter depuis MAME |
| Atari jeux TTL encore désactivés ou sans netlist complète, dont Spike/Volleyball | | sources MAME partielles | | Très complexe — les sources existent, mais le pilote reste explicitement non fonctionnel ou désactivé |
| Bandai Playdia | MAME Current partiel | MAME | MAME | Très complexe — sources incomplètes et émulation non fonctionnelle complète |
| Casio PV-1000 | MAME Current | [MAME, pilote PV-1000](https://github.com/mamedev/mame/blob/master/src/mame/casio/pv1000.cpp) | MAME | Un peu plus dur — pilote C++ fonctionnel à extraire de MAME |
| Casio Loopy | MAME Current | [MAME, pilote Loopy](https://github.com/mamedev/mame/blob/master/src/mame/casio/casloopy.cpp) | MAME | Un peu plus dur — le pilote C++ actuel est fonctionnel, mais fortement lié à MAME |
| ColecoVision | Gearcoleco, blueMSX, [JollyCV](https://github.com/libretro/jollycv), MAME Current | Gearcoleco, JollyCV, MAME | MAME | Faisable — JollyCV est une base C11 autonome annonçant 100 % de compatibilité avec la bibliothèque commerciale |
| Coleco Telstar Arcade/Gemini | | futur moteur MOS 7600/7601 | | Extrêmement complexe — très peu de sources exploitables ; ROM, PLA et CPU restant à décoder |
| Commodore C64GS | VICE x64sc, MAME Current | VICE | VICE | Faisable — sources C de VICE complètes |
| Commodore TV Game 2000K/3000H | | futur moteur MOS 7600/7601 | | Extrêmement complexe — aucun moteur complet et très peu de sources exploitables |
| Fairchild Channel F | FreeChaF, MAME Current | FreeChaF, MAME | MAME | Faisable — sources C/C++ complètes |
| GCE Vectrex | vecx, MAME Current | vecx, MAME | ParaJVE | Faisable — sources C/C++ complètes |
| Interton VC 4000 et compatibles | AmiArcadia, MAME Current | MAME | MAME | Faisable — sources C/C++ complètes |
| Magnavox Odyssey originale | | [ODYEMU](https://github.com/henrydm/MagnaVox-Odyssey-Emulator-2.0) | ODYEMU | Complexe — sources incomplètes, mais ancien émulateur partiellement fonctionnel disponible |
| Magnavox/Philips Pong dédiées avec netlist MAME complète | MAME Current | netlists MAME | MAME | Un peu plus dur — sources complètes, mais extraction importante depuis MAME |
| Magnavox/Philips Pong dédiées sans netlist complète | | | | Extrêmement complexe — aucune source d'émulation complète exploitable |
| Magnavox Odyssey²/Philips Videopac | O2EM, MAME Current | O2EM, MAME | MAME | Faisable — sources C/C++ complètes |
| Mattel Intellivision/ECS | FreeIntv, MAME Current | jzIntv, MAME | jzIntv | Faisable — sources C/C++ complètes |
| Microsoft Xbox | DirectXbox | [xemu](https://github.com/xemu-project/xemu) | xemu | Un peu plus dur — xemu et QEMU fournissent CPU, chipset et GPU émulés ; DirectXbox est bien répertorié par Libretro |
| Microsoft Xbox 360 | | Xenia | Xenia | Un peu plus dur — Xenia contient déjà PowerPC, GPU et système ; base très volumineuse |
| Microsoft Xbox One/Series | | | | Extrêmement complexe — aucune base complète réutilisable |
| NEC PC Engine/TurboGrafx/SuperGrafx/CD | Beetle PCE/Fast | Mednafen | Mednafen | Faisable — sources C/C++ complètes |
| NEC PC-FX | Beetle PC-FX, MAME Current partiel | Mednafen | Mednafen | Faisable — sources C/C++ complètes dans Mednafen |
| Nintendo Color TV-Game 6 et 15 | | | | Extrêmement complexe — aucun émulateur complet ni pilote MAME officiel trouvé ; seulement des schémas et travaux matériels |
| Nintendo Color TV-Game Racing 112 | | | | Extrêmement complexe — aucun code d'émulation complet trouvé pour son circuit dédié |
| Nintendo Color TV-Game Block Kuzushi | | | | Extrêmement complexe — aucun code d'émulation complet trouvé pour son circuit dédié |
| Nintendo Computer TV-Game | | | | Extrêmement complexe — aucun code d'émulation complet trouvé pour cette version domestique de Computer Othello |
| Nintendo NES/Famicom/FDS | Mesen, Nestopia UE, FCEUmm | Mesen, Nestopia | Mesen | Faisable — nombreuses sources C/C++ complètes |
| Nintendo SNES/Super Famicom | bsnes, Snes9x, Mesen-S | bsnes, Snes9x | bsnes | Faisable — sources C/C++ complètes, coprocesseurs inclus |
| Nintendo 64/64DD | Mupen64Plus-Next, ParaLLEl N64 | Mupen64Plus, ares | ares | Un peu plus dur — plusieurs moteurs complets fournissent déjà le RCP |
| Nintendo GameCube/Wii | Dolphin | [Dolphin](https://github.com/dolphin-emu/dolphin) | Dolphin | Un peu plus dur — sources C++ complètes, mais moteur volumineux à découpler |
| Nintendo Wii U | | [Cemu](https://github.com/cemu-project/Cemu) | Cemu | Un peu plus dur — sources C/C++ complètes et majorité des jeux jouables, mais adaptation volumineuse |
| Nintendo Switch | | Ryujinx et dérivés dont les sources complètes sont disponibles | émulateur externe | Un peu plus dur — base complète existante, grosse adaptation ; l'état de maintenance ne change pas la complétude du code disponible |
| Philips CD-i | SAME CDi, MAME Current | MAME | MAME | Un peu plus dur — sources complètes disponibles, mais liées à plusieurs composants MAME |
| RCA Studio II et clones | MAME Current | MAME | MAME | Un peu plus dur — pilote complet à extraire et adapter depuis MAME |
| Sega SG-1000/Master System/Game Gear | Genesis Plus GX, Gearsystem, MAME Current | Gearsystem, Genesis Plus GX | MAME | Faisable — sources C/C++ complètes |
| Sega Mega Drive/Mega-CD/32X | Genesis Plus GX, BlastEm, PicoDrive | mêmes projets | BlastEm | Faisable — sources C/C++ complètes couvrant déjà le CD et le 32X |
| Sega Saturn | Beetle Saturn, Kronos, Yabause | Mednafen, Kronos, Yabause | Mednafen | Un peu plus dur — la synchronisation est déjà implémentée dans plusieurs sources |
| Sega Dreamcast | Flycast | Flycast | Flycast | Faisable — Flycast est déjà un cœur réutilisable complet |
| SNK Neo Geo AES/CD | FinalBurn Neo, MAME Current, NeoCD | FBNeo, MAME | MAME | Faisable — sources C/C++ complètes |
| Sony PlayStation | Beetle PSX/HW, SwanStation, PCSX ReARMed | DuckStation, Mednafen | DuckStation | Faisable — plusieurs sources complètes et mûres |
| Sony PlayStation 2 | LRPS2/PCSX2 | [PCSX2](https://github.com/PCSX2/pcsx2) | PCSX2 | Un peu plus dur — sources C/C++ complètes couvrant déjà Emotion Engine, VU, GS, IOP et périphériques |
| Sony PlayStation 3 | | [RPCS3](https://github.com/RPCS3/rpcs3) | RPCS3 | Un peu plus dur — sources C++ complètes, mais très volumineuses à découpler |
| Sony PlayStation 4 | | shadPS4 | shadPS4 | Complexe — sources publiques et émulateur fonctionnel, mais couverture encore incomplète |
| Sony PlayStation 5 | | | | Extrêmement complexe — aucune source d'émulateur PS5 complet exploitable |
| VTech CreatiVision | [JollyCV](https://github.com/libretro/jollycv), MAME Current | JollyCV, MAME, FunnyMu | MAME | Faisable — JollyCV fournit une source C11 portable et annonce 100 % de compatibilité avec la bibliothèque commerciale, hors lecteur de cassette BASIC |
| Nichibutsu My Vision | [JollyCV](https://github.com/libretro/jollycv), MAME Current | JollyCV, MAME | MAME | Faisable — JollyCV fournit une source C11 complète et annonce 100 % de compatibilité avec tous les jeux dumpés |

## Consoles portables

| Famille et machines | Cœurs libretro utilisables | Émulateurs externes adaptables | EXE pilotable | Recréation interne |
|---|---|---|---|---|
| Atari Lynx/Lynx II | Beetle Lynx, Handy, Holani | Handy, Mednafen | Mednafen | Faisable — sources C/C++ complètes |
| Bandai WonderSwan/Color/SwanCrystal | Beetle Cygne, MAME Current | Mednafen | Mednafen | Faisable — sources C/C++ complètes |
| Nintendo Game & Watch | GW, MAME Current | GW, MAME | MAME | Un peu plus dur — sources complètes, mais plusieurs configurations matérielles à réunir |
| Nintendo Game Boy/Color | SameBoy, Gambatte, mGBA, Mesen | SameBoy, mGBA | SameBoy | Faisable — sources C/C++ complètes |
| Nintendo Game Boy Advance/Micro | mGBA, VBA-M, gpSP | mGBA | mGBA | Faisable — mGBA fournit une base complète et propre |
| Nintendo DS/DSi | melonDS DS, DeSmuME | melonDS, DeSmuME | melonDS | Un peu plus dur — les deux CPU, la 3D et les périphériques sont déjà implémentés |
| Nintendo 3DS/2DS | Citra, Citra 2018, Citra Canary | [Azahar](https://github.com/azahar-emu/azahar) | Azahar | Un peu plus dur — les cœurs Citra restent répertoriés officiellement et la base C++ actuelle d'Azahar est complète, mais volumineuse |
| Sega Nomad | Genesis Plus GX, PicoDrive | Genesis Plus GX | | Faisable — sources C/C++ complètes de la Mega Drive |
| SNK Neo Geo Pocket/Color | Beetle NeoPop, RACE, MAME Current | Mednafen | Mednafen | Faisable — sources C/C++ complètes |
| Sony PocketStation | MAME Current | [MAME, pilote PocketStation](https://github.com/mamedev/mame/blob/master/src/mame/sony/pockstat.cpp) | MAME | Un peu plus dur — pilote C++ fonctionnel sans marqueur `MACHINE_NOT_WORKING`, mais lié à MAME |
| Sony PSP/PSP Go | PPSSPP | PPSSPP | PPSSPP | Faisable — PPSSPP est complet, mûr et déjà structuré pour plusieurs frontends |
| Sony PlayStation Vita/TV | | Vita3K | Vita3K | Complexe — sources disponibles mais émulation encore incomplète |
| Watara Supervision | Potator, MAME Current | MAME | MAME | Faisable — sources C/C++ complètes |
| Hartung Game Master | MAME Current | MAME | MAME | Un peu plus dur — pilote C++ fonctionnel à extraire de MAME |
| Tiger Game.com | MAME Current | [MAME, pilote Game.com](https://github.com/mamedev/mame/blob/master/src/mame/tiger/gamecom.cpp) | MAME | Un peu plus dur — pilote C++ fonctionnel à extraire de MAME |

## Cas à ne pas confondre

- DOSBox fait fonctionner des logiciels PC, mais ne recrée pas automatiquement le matériel exact d'un PC Amstrad, Commodore ou Tandy.
- Un EXE pilotable peut être lancé et configuré par GW GUI, mais il ne fournit pas automatiquement les trames, l'audio et les entrées comme une DLL libretro.
- MAME Current contient les anciens pilotes MESS et peut donc émuler des ordinateurs ; les anciens cœurs MAME arcade ne les couvrent pas nécessairement.
- Une famille avec plusieurs variantes d'un même dépôt, comme VICE, doit partager un seul code d'intégration.
- Les machines construites par Amstrad mais vendues sous la marque Sinclair restent dans la famille Sinclair.

## Priorités suggérées

| Rang | Famille | Raisons techniques |
|---:|---|---|
| 1 | Amstrad CPC/GX4000 | Une DLL Caprice32 couvre six machines et leurs principaux médias |
| 2 | Commodore VICE | Un moteur partagé couvre la majorité des Commodore 8 bits |
| 3 | Sinclair ZX Spectrum | Fuse couvre presque toute la famille |
| 4 | Thomson | Theodore offre une base compacte pour une famille française |
| 5 | MSX | Très grande couverture mais catalogue de firmwares plus lourd |
| 6 | Acorn BBC/Electron | Sources et cœurs disponibles |
| 7 | PCW/NC et PC de marque | Dépendance plus forte à MAME, PCem ou 86Box |
| 8 | MOS 7600/7601 et autres Pong | Recréation interne intéressante mais rétro-ingénierie incomplète |

## Sources principales

- [Liste actuelle des cœurs Libretro](https://docs.libretro.com/guides/core-list/)
- [MAME Current pour les ordinateurs et consoles](https://docs.libretro.com/guides/softwarelist-getting-started/)
- [Sources MAME](https://github.com/mamedev/mame)
- [Caprice32 libretro](https://github.com/libretro/libretro-cap32)
- [VICE libretro](https://github.com/libretro/vice-libretro)
- [DOSBox Pure](https://github.com/schellingb/dosbox-pure)
- [PCem](https://github.com/sarah-walker-pcem/pcem)
- [86Box](https://github.com/86Box/86Box)
- [Xemu](https://github.com/lgblgblgb/xemu)

Ce catalogue doit rester évolutif : avant d'ajouter une machine à GW GUI, la présence exacte du modèle, son état d'émulation, ses firmwares, ses médias et la disponibilité d'une DLL Windows x64 doivent être contrôlés dans la version du moteur retenue.
