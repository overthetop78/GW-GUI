# Émulation Commodore — catalogue et moteurs

## Périmètre

Cette feuille concerne les ordinateurs et consoles Commodore hors Amiga. Amiga reste une famille séparée dans GW GUI avec son propre module et PUAE.

Les variantes régionales et les révisions d'un même matériel peuvent partager une implémentation, mais restent sélectionnables séparément lorsque leur ROM, leur fréquence, leur clavier ou leurs périphériques diffèrent.

## Échelle de complexité

La complexité estime uniquement le travail nécessaire pour que Codex recrée lui-même l'émulateur. Elle ne mesure jamais la difficulté d'installation ou d'intégration d'un cœur existant. Le classement dépend du langage, de l'état complet ou incomplet des sources et du nombre de projets à réunir ; la difficulté théorique du matériel n'augmente pas la note lorsque son implémentation complète existe déjà.

| Niveau | Signification |
|---|---|
| Facile | Sources complètes en C# |
| Faisable | Sources complètes en C ou C++ ne demandant pas d'adaptation complexe |
| Intermédiaire | Sources complètes dans un autre langage et demandant une adaptation |
| Un peu plus dur | Sources complètes demandant une grosse adaptation, ou plusieurs sources complètes à réunir |
| Complexe | Sources incomplètes, mais suffisantes pour obtenir un émulateur fonctionnel |
| Très complexe | Sources incomplètes et insuffisantes pour obtenir directement un émulateur fonctionnel |
| Extrêmement complexe | Aucune source exploitable, ou seulement très peu de sources |

## Catalogue Commodore

| Famille | Machines et variantes | Libretro recommandé | Autres libretro possibles | Émulateurs externes exploitables | Moteur interne si nécessaire | Recréation interne par Codex |
|---|---|---|---|---|---|---|
| KIM | MOS/Commodore KIM-1 | MAME Current | | MAME autonome | Extraire le pilote MAME et réutiliser son 6502 | Un peu plus dur — pilote complet mais lié au framework MAME |
| PET 2000 | PET 2001-4/8/16/32, 2001-N et variantes régionales | VICE xpet | MAME Current | VICE autonome, MAME | Réutiliser les composants des sources VICE | Complexe — VICE est fonctionnel, mais sa documentation confirme encore des fonctions PET non implémentées |
| PET 3000 | CBM/PET 3008, 3016, 3032, 3032B | VICE xpet | MAME Current | VICE autonome, MAME | Réutiliser VICE | Complexe — même moteur PET fonctionnel mais incomplet |
| PET 4000 | PET/CBM 4016, 4032, 4032B et variantes | VICE xpet | MAME Current | VICE autonome, MAME | Réutiliser VICE | Complexe — même moteur PET fonctionnel mais incomplet |
| PET 8000 | PET/CBM 8032, 8096, variantes SK et régionales | VICE xpet | MAME Current | VICE autonome, MAME | Réutiliser VICE | Complexe — même moteur PET fonctionnel mais incomplet |
| PET 8296 | CBM 8296, 8296-D, 8296-GD et variantes | VICE xpet | MAME Current | VICE autonome, MAME | Réutiliser VICE | Complexe — même moteur PET fonctionnel mais incomplet |
| SuperPET | SuperPET, MMF 9000 et variantes | VICE xpet | MAME Current | VICE autonome, MAME | Réutiliser VICE | Complexe — SuperPET fonctionne, mais dépend du moteur PET encore incomplet |
| VIC | VIC-20 NTSC/PAL, VC-20, VIC-1001, variantes régionales | VICE xvic | MAME Current | VICE autonome | Réutiliser VICE | Complexe — fonctionnel, mais VICE signale encore l'entrelacement NTSC absent et le son expérimental |
| VIC étendu | VIC-21/Super VIC | VICE xvic | | VICE autonome | Réutiliser VICE | Faisable — variante déjà présente |
| MAX | MAX Machine, Ultimax, VIC-10 | MAME Current | | MAME autonome ; VICE autonome selon configuration | Extraire le pilote MAME ou adapter VICE | Un peu plus dur — composants déjà codés mais intégration à isoler |
| C64 | C64 PAL/NTSC, japonais et variantes régionales | VICE x64sc | VICE x64, MAME Current | VICE autonome, Denise, Hoxs64, [ViceSharp](https://github.com/sharpninja/vice-sharp) | Réutiliser ViceSharp | Facile — cœur C64 complet en C#/.NET 10, validé en lockstep avec VICE x64sc |
| C64C/G | C64C, C64G et variantes régionales | VICE x64sc | VICE x64, MAME Current | VICE autonome, Denise, Hoxs64, [ViceSharp](https://github.com/sharpninja/vice-sharp) | Réutiliser ViceSharp | Facile — variantes reposant sur le cœur C64 complet en C# |
| C64 portable | SX-64, Executive 64, VIP-64 | VICE x64sc | MAME Current | VICE autonome, [ViceSharp](https://github.com/sharpninja/vice-sharp) | Réutiliser ViceSharp/VICE | Facile — SX-64 et même architecture C64 figurent dans l'itération C# complète |
| C64 portable prototype | DX-64 | MAME Current | | MAME | Extraire le pilote MAME et réutiliser les composants C64 | Un peu plus dur — pas couvert comme machine complète par ViceSharp ; plusieurs composants à réunir |
| C64 éducatif | Educator 64, PET 64, CBM 4064 | VICE x64sc | MAME Current | VICE autonome | Réutiliser VICE | Faisable — variante existante |
| C64 console | Commodore 64 Games System/C64GS | VICE x64sc | MAME Current | VICE autonome | Réutiliser VICE | Faisable — variante existante |
| C64 accéléré | C64 avec CMD SuperCPU | VICE xscpu64 | | VICE autonome | Réutiliser VICE | Faisable — SuperCPU déjà implémentée |
| C64DTV | DTV2 PAL/NTSC, DTV3 PAL/NTSC, Hummer | VICE x64dtv | MAME Current | VICE autonome | Réutiliser VICE | Complexe — moteur fonctionnel, mais VICE le déclare encore en construction avec des modes vidéo inexacts |
| Série 264 | Commodore 16 PAL/NTSC et variantes régionales | VICE xplus4 | MAME Current | VICE autonome, YAPE | Réutiliser VICE/YAPE | Complexe — moteur fonctionnel, mais VICE classe encore Plus/4 parmi les émulations en construction |
| Série 264 | Commodore 116 | VICE xplus4 | MAME Current | VICE autonome, YAPE | Réutiliser VICE | Complexe — même moteur Plus/4 fonctionnel mais incomplet |
| Série 264 | Commodore Plus/4 PAL/NTSC | VICE xplus4 | MAME Current | VICE autonome, YAPE | Réutiliser VICE/YAPE | Complexe — même moteur fonctionnel mais incomplet |
| Série 264 prototypes | C232, C264, V364 | VICE xplus4 | MAME Current | VICE autonome, YAPE | Réutiliser VICE/MAME | Complexe — variantes fonctionnelles reposant sur le moteur Plus/4 incomplet |
| C128 | C128 PAL/NTSC et variantes régionales | VICE x128 | MAME Current | VICE autonome | Réutiliser VICE | Complexe — MMU, VDC, Z80 et mode 2 MHz sont présents, mais VICE classe encore l'ensemble en construction |
| C128D | C128D PAL/NTSC | VICE x128 | MAME Current | VICE autonome | Réutiliser VICE | Complexe — variante fonctionnelle du moteur C128 encore incomplet |
| C128DCR | C128DCR PAL/NTSC et variantes régionales | VICE x128 | MAME Current | VICE autonome | Réutiliser VICE | Complexe — variante fonctionnelle du moteur C128 encore incomplet |
| CBM-II 5x0 | B500, CBM 500/510, P500 | VICE xcbm5x0 | MAME Current | VICE autonome | Réutiliser VICE | Complexe — émulation fonctionnelle, mais le C510/P500 reste annoncé expérimental |
| CBM-II 6x0 | B128, B256, CBM 610/620, variantes HP/Plus | VICE xcbm2 | MAME Current | VICE autonome | Réutiliser VICE | Complexe — fonctionnel, mais VICE classe CBM-II parmi les moteurs en construction |
| CBM-II 7x0 | CBM 710/720/730, variantes HP/Plus et régionales | VICE xcbm2 | MAME Current | VICE autonome | Réutiliser VICE | Complexe — fonctionnel, mais variantes à coprocesseur absentes et moteur encore incomplet |
| C65 | Commodore 65/C64DX prototypes PAL et NTSC | MAME Current | | Xemu | Réunir les sources Xemu et MAME | Complexe — pilote MAME fonctionnel mais encore rempli de parties `TODO` |
| Commodore LCD | Commodore LCD prototype | MAME Current | | Xemu | Réunir les sources Xemu et MAME | Très complexe — sources incomplètes et couverture insuffisante du prototype |
| PC XT antérieurs non implémentés exactement | Commodore PC-1, Colt, PC-10 et PC-10 II | DOSBox Pure/VirtualXT en approximation | DOSBox Core | PCem/86Box générique | Réutiliser le PC générique puis coder les différences propres à chaque modèle | Complexe — base fonctionnelle complète, mais aucun de ces quatre modèles Commodore exacts n'a été vérifié dans les sources ; le `pc1` de MAME est un Atari |
| PC XT III | PC-10 III et PC-20 III | MAME Current pour PC-10 III ; DOSBox Pure/VirtualXT en approximation | | [PCem](https://github.com/sarah-walker-pcem/pcem), MAME | Extraire le modèle PCem et ses périphériques | Un peu plus dur — PCem fournit le modèle et le BIOS 4.41 ; le PC-20 III partage la carte du PC-10 III et ajoute le disque XTA |
| PC AT III implémentés | PC-30 III et PC-40 III | MAME Current | DOSBox Pure en approximation | [MAME](https://github.com/mamedev/mame/blob/master/src/mame/pc/at.cpp) | Extraire les pilotes MAME | Un peu plus dur — les deux modèles sont explicitement présents dans le pilote C++ complet de MAME |
| PC AT III non implémenté exactement | PC-45 III | DOSBox Pure en approximation | | PCem/86Box générique | Réutiliser un PC AT puis ajouter la carte exacte | Complexe — base PC fonctionnelle, mais aucun modèle PC-45 III complet vérifié |
| PC Commodore non implémentés exactement | PC-5, PC-30/40 antérieurs, PC-50/60/70 et variantes 286/386 | DOSBox Pure/Core en approximation | | PCem ou 86Box générique | Partir du PC générique et ajouter chaque carte propre à Commodore | Complexe — sources PC fonctionnelles disponibles, mais implémentations exactes de ces modèles absentes |
| PC 386 implémenté | Commodore SL386SX-25 | DOSBox Pure en approximation | | [PCem](https://github.com/sarah-walker-pcem/pcem) | Extraire le modèle PCem | Un peu plus dur — machine explicitement implémentée avec son BIOS et son contrôleur vidéo AVGA2 dans les sources complètes de PCem |
| Portables PC | C286-LT et autres portables Commodore | | DOSBox Pure en approximation | 86Box générique ; ajout du modèle encore demandé | Partir de PCem/86Box | Complexe — base PC présente, matériel portable exact encore absent |
| Station Unix | Commodore 900 prototype | | | | Développer CPU Z8001, MMU, vidéo, stockage et périphériques à partir des documents disponibles | Extrêmement complexe |
| TV Game | Commodore TV Game 2000K | | | Aucun émulateur complet trouvé | Moteur commun MOS 7600/7601 | Extrêmement complexe — très peu de sources ; ROM, PLA et CPU à décoder |
| TV Game | Commodore TV Game 3000H | | | Aucun émulateur complet trouvé | Même moteur, variante M5601 | Extrêmement complexe — très peu de sources et variante M5601 encore moins documentée |

## Architecture VICE proposée

Les dix DLL VICE ne doivent pas produire dix copies de l'intégration. Elles proviennent du même dépôt et exposent la même ABI libretro.

Le module Commodore doit fournir :

- un hôte VICE commun ;
- un catalogue décrivant la DLL et les options de chaque famille ;
- une gestion commune des disquettes, bandes, cartouches, lecteurs, imprimantes et états ;
- des adaptateurs de configuration propres aux capacités de chaque famille ;
- une seule infrastructure vidéo, audio, clavier et manettes.

`vice_x64` ne doit pas être proposé par défaut : `vice_x64sc` est la variante C64 précise. Il peut rester un moteur alternatif si un besoin de performance est démontré.

## Alternatives hors libretro

| Projet | Forme disponible | Machines utiles | Possibilité d'intégration | Décision |
|---|---|---|---|---|
| VICE officiel | Exécutables Windows et sources C | Toutes les familles VICE | Pas d'API DLL stable équivalente à libretro ; contrôle par processus possible | Employer vice-libretro |
| ViceSharp | Bibliothèque .NET 10 NativeAOT | C64 et 1541 actuellement ; autres familles annoncées | Très bonne architecture pour GW GUI, mais projet encore incomplet | À surveiller, pas moteur initial |
| Denise | Exécutable et sources C++ | C64 | Port en bibliothèque possible mais redondant avec VICE | Repli éventuel C64 |
| Hoxs64 | Exécutable Windows | C64 | Pas de bibliothèque publique stable | Ne pas intégrer |
| YAPE | Exécutable Windows | Plus/4 et C16 | Pas de bibliothèque publique stable | VICE reste préférable |
| Xemu | Exécutables SDL2 et sources C | C65, Commodore LCD, MEGA65 | Adaptateur ou port libretro à écrire | Repli pertinent pour C65/LCD |
| MAME autonome | Exécutable et sources C++ | Presque tout le catalogue documenté | Lancement externe possible, mais MAME Current libretro est préférable | Employer le cœur libretro |
| PCem | Exécutable et sources C/C++ | Plusieurs PC Commodore | Transformation en bibliothèque lourde ; lancement externe possible | Repli pour PC exacts |
| 86Box | Exécutable et sources C | Compatibles PC génériques ; certains modèles demandés manquent | Pilotable par dossier de VM et processus externe, sans ABI vidéo/audio commune | Repli générique uniquement |

## MOS 7600/7601 et TV Game

Les TV Game Commodore utilisent un microcontrôleur de la famille MOS et non le circuit câblé AY-3-8500 :

| Variante | Matériel connu | Programme |
|---|---|---|
| MOS 7600-001 | consoles NTSC compatibles et cartouche 2 Coleco Telstar Arcade | Tennis, hockey/football, handball/squash, cible |
| MOS 7601-001 | Commodore TV Game 2000K PAL | mêmes jeux adaptés au PAL |
| M5601 | Commodore TV Game 3000H | variante simplifiée, vidéo moins colorée |
| MOS 7600-002 | cartouche 1 Coleco Telstar Arcade | course, tennis, tir |
| MOS 7600-003 | cartouche 4 Coleco Telstar Arcade | bataille navale, Speedball, tir |
| MOS 7600-004 | cartouche 3 Coleco Telstar Arcade et Telstar Gemini | flipper et tir |

Le 7600-001 a été décapsulé et photographié. Il contient une ROM masque de 512 mots de 13 bits, des PLA graphiques et un processeur série propriétaire. Aucun dump binaire public complet ni émulateur complet n'a été identifié.

Le moteur interne doit être générique et ne jamais porter un nom limité à Commodore. Sa structure cible est :

- cœur CPU série MOS 7600 ;
- ROM et PLA sélectionnées par variante ;
- temporisation PAL ou NTSC ;
- définition externe des circuits de couleurs et du son ;
- définition des commandes par console ;
- possibilité de réutilisation future pour Coleco, Radofin et les autres machines équipées de la même puce.

Sans sources complètes, même une reproduction fonctionnelle reste très complexe. Une émulation exacte exige en plus l'extraction de la ROM et des PLA depuis les photographies du silicium et devient extrêmement complexe.

## Ordre d'intégration recommandé

1. Créer `GWGUI.Emulation.Commodore` comme module multi-machines et multi-cœurs sur le modèle d'Atari.
2. Intégrer l'hôte commun VICE et ses variantes.
3. Ajouter KIM-1, MAX Machine, C65, LCD et les PC couverts par MAME Current.
4. Tester la faisabilité d'un moteur interne MOS 7600/7601.
5. Traiter les PC non couverts seulement après validation de MAME, PCem et 86Box modèle par modèle.
6. Laisser Commodore 900 en recherche tant qu'aucun moteur exploitable n'existe.

## Sources principales

- [VICE libretro](https://github.com/libretro/vice-libretro)
- [Documentation VICE libretro](https://docs.libretro.com/library/vice/)
- [MAME Current pour les ordinateurs et consoles](https://docs.libretro.com/guides/softwarelist-getting-started/)
- [Sources MAME](https://github.com/mamedev/mame)
- [Xemu : C65 et Commodore LCD](https://github.com/lgblgblgb/xemu)
- [PCem](https://github.com/sarah-walker-pcem/pcem)
- [86Box](https://github.com/86Box/86Box)
- [ViceSharp](https://github.com/sharpninja/vice-sharp)
- [Analyse du MOS 7600/7601](https://oldvcr.blogspot.com/2022/09/confirmed-mos-76007601-pong-chip-is.html)
- [Documentation retrouvée du MOS 7600/7601](https://oldvcr.blogspot.com/2023/10/finally-mos-76007601-video-game-array.html)
