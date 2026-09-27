# Cœurs Libretro retenus

Ce tableau est la source de décision avant de créer les adaptateurs dans les
modules. Les identifiants et extensions seront recopiés dans les contrats du
module uniquement lorsqu'un cœur Windows x64 est sélectionné et testé.

| Famille | Cœur | Machines couvertes | Médias à prévoir | État |
|---|---|---|---|---|
| Sega | Gearsystem | SG-1000, Master System, Game Gear | ROM, Sega Card | À intégrer |
| Sega | Genesis Plus GX / PicoDrive | Mega Drive/Genesis, Mega-CD, 32X | ROM, BIN/CUE, CHD, M3U | À intégrer |
| Sega | Beetle Saturn | Saturn | CUE/CCD/CHD/M3U | À intégrer |
| Sega | Flycast | Dreamcast | CDI, GDI, CHD, CUE/BIN, M3U | À intégrer |
| Nintendo | Nestopia ou Mesen | NES/Famicom, Famicom Disk System | NES, FDS, UNIF/UNF | À intégrer |
| Nintendo | Snes9x ou bsnes | SNES/Super Famicom | SFC, SMC, SWC | À intégrer |
| Nintendo | Gambatte ou mGBA | Game Boy, Color, Advance | GB, GBC, GBA | À intégrer |
| Nintendo | Mupen64Plus-Next | Nintendo 64, 64DD | N64, Z64, V64, NDD | À intégrer |
| Nintendo | melonDS | Nintendo DS/DSi | NDS | À intégrer |
| Nintendo | Citra | Nintendo 3DS | 3DS, 3DSX, CIA, CCI/CXI | À intégrer |
| Nintendo | Dolphin | GameCube, Wii | ISO, WBFS, RVZ, CISO | À intégrer |
| Nintendo | Cemu | Wii U | WUD, WUX, RPX/RPL, directory | À valider |
| Sony | Beetle PSX / SwanStation | PlayStation/PS one | BIN/CUE, CHD, PBP, M3U | À intégrer |
| Sony | LRPS2 / PCEE2 / Play! | PlayStation 2 | ISO, CHD, CUE/BIN, CSO | À intégrer |
| Sony | PPSSPP | PSP, Slim/Lite/Go | ISO, CSO, PBP, CHD | À intégrer |
| NEC | Beetle PCE FAST / Beetle PCE | PC Engine, CoreGrafx, Duo, CD | PCE, CUE/BIN, CHD, M3U | À intégrer |
| NEC | Beetle SGX | SuperGrafx | PCE, HuCard | À intégrer |
| NEC | Beetle PC-FX | PC-FX | CUE/BIN, CHD | À intégrer |
| Microsoft | — | Xbox, Xbox 360 | — | Aucun cœur Libretro vérifié ; étudier xemu/Xenia |

Les éventuels Game & Watch, Virtual Boy, Switch, Vita, PS3, PS4 et PS5 seront
ajoutés après sélection d'un cœur réellement redistribuable et après définition
de leurs médias et de leurs exigences firmware. Aucun fichier de BIOS ou de
jeu n'est copié dans le dépôt.

Références vérifiées :

- https://docs.libretro.com/guides/core-list/
- https://docs.libretro.com/library/flycast/
- https://docs.libretro.com/library/beetle_saturn/
- https://docs.libretro.com/library/citra/
- https://docs.libretro.com/library/dolphin/
- https://docs.libretro.com/library/ppsspp/
- https://docs.libretro.com/library/lrps2/
