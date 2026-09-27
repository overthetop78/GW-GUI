# Familles de consoles et supports média

Ce chantier est réalisé sur la branche `feature/console-families-media`, créée
depuis `main` après la fusion de `amstrad` (`0ecd3512`). Il suit les frontières
déjà utilisées par les modules Amiga, Atari et Amstrad :

- [x] Fusionner `amstrad` dans `main` (`0ecd3512`).
- [x] Créer et sélectionner `feature/console-families-media`.
- [x] Écrire ce cadrage avant toute création de module ou de format.

```text
GWGUI.App -> GWGUI.Emulation -> GWGUI.Emulation.<Famille>/Common
                              -> Emulators/<Cœur>
GWGUI.App -> GWGUI.MediaEngine -> GWGUI.MediaFileSystems -> GWGUI.MediaAnalysis
```

## Périmètre des modules

Les identifiants de modules seront `sega`, `nintendo`, `sony` et `microsoft`.
Chaque module aura son propre `module.json`, son projet `net10.0` x64, une
factory publique `IEmulationModuleFactory`, un catalogue de machines et un
adaptateur par cœur réellement intégré. Aucun projet ne sera ajouté à
`GWGUI.App` : la découverte existante charge les modules depuis leur manifeste.

Le module supplémentaire `nec` couvrira PC Engine/TurboGrafx-16, CoreGrafx,
SuperGrafx, PC Engine Duo/TurboDuo et les variantes portables concernées.

### Sega

- SG-1000, SC-3000 et Mark III/Master System ;
- Mega Drive/Genesis, Mega-CD, 32X ;
- Game Gear ;
- Saturn ;
- Dreamcast.

### Nintendo

- Game & Watch ;
- NES/Famicom, SNES/Super Famicom ;
- Virtual Boy ;
- Nintendo 64 ;
- Game Boy, Game Boy Color, Game Boy Advance ;
- Nintendo DS/DSi, 3DS ;
- GameCube, Wii, Wii U ;
- Switch si un cœur compatible et redistribuable est effectivement intégré.

### Sony

- PlayStation/PS one ;
- PlayStation 2/PStwo ;
- PSP, PSP Slim/Lite et PSP Go ;
- PlayStation Vita ;
- PlayStation 3, 4 et 5 lorsqu'un cœur ou un adaptateur local vérifiable existe.

### Microsoft

- Xbox ;
- Xbox 360.

### NEC

- PC Engine/TurboGrafx-16 et CoreGrafx ;
- SuperGrafx ;
- PC Engine Duo/TurboDuo ;
- PC Engine GT/TurboExpress et PC Engine LT lorsqu'un cœur les prend en charge.

Les ordinateurs non compatibles IBM PC x86/x64 peuvent être ajoutés uniquement
comme machines d'une famille lorsque leur cœur et leurs médias sont réellement
disponibles. Aucun ordinateur IBM PC générique ne sera ajouté à ces modules.

## Cœurs ouverts retenus pour l'intégration

Les adaptateurs restent propres à chaque module et suivent les contrats déjà
utilisés par Amiga, Atari et Amstrad. Aucune couche Libretro commune n'est
ajoutée : un cœur n'est déclaré qu'après l'ajout de son adaptateur concret,
de son installation et de son protocole avec le module.

Les cœurs et leurs versions seront inscrits dans les catalogues propres aux
modules uniquement avec leur adaptateur concret, leur protocole, leur
installation et leurs médias vérifiés. Aucun nom de cœur n'est injecté dans
`GWGUI.Emulation` ou partagé entre les familles.

Les extensions sont dérivées des capacités déclarées par chaque adaptateur, et non
d'une liste générique : FDS, Sega Card/HuCard, CD/GD-ROM et les formats
multi-disques doivent donc être ajoutés avec leur lecteur MediaEngine et leur
préparation de contenu correspondants.

## Supports média

Les formats de disquette, cassette, flux, cartouche, disque optique et disque
dur restent dans `GWGUI.MediaEngine`. Les lecteurs et Writers spécialisés
retournent les représentations déjà consommées par la visualisation et la
conversion. `GWGUI.MediaFileSystems` reçoit la représentation décodée pour
l'exploration ; `GWGUI.MediaAnalysis` classe les entrées. Les modules
d'émulation ne recopient aucun lecteur de média.

Les formats à traiter sont inventoriés avant leur implémentation :

| Support | Exemples de formats | Propriétaire |
|---|---|---|
| Cartouche | ROM, BIN, NES, FDS (Famicom Disk System), SNES/SFC, N64, GBA, GB/GBC, NDS, 3DS/CIA, Sega Card et HuCard/PCE, CPR | MediaEngine ; MediaFileSystems seulement si un système de fichiers est démontré |
| Disquette | DSK/EDSK, ST, ADF, G64, D64/D71/D81, XDF et variantes propres aux machines | MediaEngine + MediaFileSystems |
| Cassette/bande | TAP, TZX, CDT, VOC, WAV, FLAC, MP3, AAC, M4A, HXCSTREAM | MediaEngine pour le flux et les blocs ; MediaFileSystems pour les contenus décodés |
| Optique | ISO, BIN/CUE, CHD, CCD/MDS, CDI, GDI (Dreamcast) et images multi-pistes | MediaEngine + systèmes ISO/UDF existants |
| Disque dur | IMG, VHD, VDI, VMDK, QCOW2, CHD | MediaEngine + systèmes de fichiers existants |

Une extension n'est déclarée comme lisible, visualisable, explorable ou
convertible qu'après l'ajout d'un Reader/Writer et d'une représentation réelle.
Une simple ligne d'extension dans un catalogue ne constitue pas un support.

## Ordre d'implémentation

- [ ] Créer les cinq projets de modules, leurs manifestes et leurs factories.
  - [ ] Ajouter la façade `IEmulationModule` et le catalogue de machines Sega.
  - [ ] Ajouter la façade `IEmulationModule` et le catalogue de machines Nintendo.
  - [ ] Ajouter la façade `IEmulationModule` et le catalogue de machines Sony.
  - [ ] Ajouter la façade `IEmulationModule` et le catalogue de machines Microsoft.
  - [ ] Ajouter la façade `IEmulationModule` et le catalogue de machines NEC.
- [ ] Brancher les cœurs, un adaptateur à la fois, dans `Emulators/<Cœur>`.
  - [ ] Documenter pour chaque adaptateur son identifiant, sa version et ses médias.
  - [ ] Refuser explicitement une machine dont aucun adaptateur n'est installé.
- [ ] Compléter les formats communs de `MediaEngine`.
  - [x] Ajouter le lecteur/Writer brut de cartouches console et sa représentation par banques de 16 Kio.
  - [x] Ajouter le lecteur de banques de cartouche à `MediaFileSystems`.
  - [x] Ajouter les règles de contenu ROM pour les extensions de cartouches et les banques extraites à `MediaAnalysis`.
  - [ ] Ajouter les Readers, Writers, représentations visuelles et conversions des autres supports listés.
- [ ] Localiser les nouveaux libellés avec la base commune et Argos.
- [ ] Ajouter les tests utiles, puis exécuter le build Debug avec tous les modules.

## Fichiers de réalisation

Les feuilles suivantes sont les actions concrètes restantes ; chaque case ne
sera cochée qu'après écriture du fichier et compilation du comportement associé.

- [x] Créer `src/GWGUI.Emulation.Sega/GWGUI.Emulation.Sega.csproj` avec la référence SDK.
- [x] Créer `src/GWGUI.Emulation.Sega/module.json` et `Modules/SegaEmulationModuleFactory.cs`.
- [x] Créer `src/GWGUI.Emulation.Sega/Common/Machines/MachineCatalog.cs` et les configurations Sega.
- [x] Créer les contrats, interfaces et services `Common`/`Common/Machines/Common` de Sega en reprenant les frontières des modules existants.
- [ ] Créer les adaptateurs concrets `src/GWGUI.Emulation.Sega/Emulators/<Cœur>/*` pour les cœurs Sega retenus.
- [ ] Créer `src/GWGUI.Emulation.Nintendo/GWGUI.Emulation.Nintendo.csproj`, `module.json` et la factory.
- [ ] Créer `src/GWGUI.Emulation.Nintendo/Common/Machines/MachineCatalog.cs` et les configurations Nintendo.
- [ ] Créer les adaptateurs concrets `src/GWGUI.Emulation.Nintendo/Emulators/<Cœur>/*` pour NES/FDS, SNES, GB/GBA, N64, DS, 3DS, GameCube/Wii et Wii U.
- [ ] Créer `src/GWGUI.Emulation.Sony/GWGUI.Emulation.Sony.csproj`, `module.json` et la factory.
- [ ] Créer `src/GWGUI.Emulation.Sony/Common/Machines/MachineCatalog.cs` et les configurations Sony.
- [ ] Créer les adaptateurs concrets `src/GWGUI.Emulation.Sony/Emulators/<Cœur>/*` pour PS1, PS2 et PSP.
- [ ] Créer `src/GWGUI.Emulation.Nec/GWGUI.Emulation.Nec.csproj`, `module.json` et la factory.
- [ ] Créer `src/GWGUI.Emulation.Nec/Common/Machines/MachineCatalog.cs` et les configurations PC Engine.
- [ ] Créer les adaptateurs concrets `src/GWGUI.Emulation.Nec/Emulators/<Cœur>/*` pour PCE, SGX et PC-FX.
- [ ] Créer `src/GWGUI.Emulation.Microsoft/GWGUI.Emulation.Microsoft.csproj`, `module.json` et la factory après validation xemu/Xenia.
- [ ] Créer les lecteurs MediaEngine pour FDS, Sega Card/HuCard et CDI/GDI/CHD multi-pistes.

Ce document ne prétend pas qu'un cœur ou un format est déjà implémenté : chaque
case sera cochée seulement après le fichier et le comportement correspondants.
