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

### Formats de cartouche spécifiques (à réaliser avant l'exposition utilisateur)

- [x] Déclarer les formats de cartouche par famille, sans identifiant générique visible.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` pour ajouter les identifiants Nintendo, Sega et NEC correspondant aux extensions réelles.
  - [x] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs` pour exposer les mêmes identifiants au moteur.
  - [x] Créer les lecteurs et writers de cartouches dans `src/GWGUI.MediaEngine/Images/Formats/Cartridge/Console/`, avec un identifiant et des extensions propres à chaque famille.
  - [x] Créer l’explorateur des banques dans `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/`, sans lecteur « Raw » générique.
  - [x] Supprimer les anciens lecteurs de cartouche génériques et vérifier qu’aucune référence à cette ancienne structure ne reste dans `src`, `tests` ou la documentation.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/ImageFormatCatalog.cs` pour afficher les noms de formats console réels, jamais « Raw console cartridge ».
  - [x] Modifier `src/GWGUI.App/Resources/00-Base/Formats.resx` pour localiser les formats spécifiques ajoutés dans la base commune.
  - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` avec un test autonome et nettoyage des fichiers temporaires dans `finally`.
  - [x] Compiler MediaEngine, MediaFileSystems et MediaAnalysis puis exécuter les tests ciblés.

### Tranche en cours (à terminer avant tout commit)

- [x] Corriger les ressources de modèles copiées depuis Amstrad.
  - [x] Modifier les cinq `Resources/00-Base/Emulation.resx` pour supprimer les clés de modèles d’une autre famille et écrire celles correspondant aux `ModelCatalog` Sega, Nintendo, Sony, Microsoft et NEC.
  - [x] Reproduire ces clés dans les cultures existantes de chaque module sans conserver de valeur Amstrad résiduelle.
  - [x] Rechercher les identifiants `464`, `664`, `6128`, `GX4000` et les descriptions Amstrad résiduelles dans ces ressources, puis compiler les cinq modules.
- [ ] Finaliser les cinq modules sans laisser de façade non exécutable.
  - [ ] Créer `src/GWGUI.Emulation.Nec/GWGUI.Emulation.Nec.csproj`, `module.json`, `Modules/NecEmulationModuleFactory.cs` et le catalogue PC Engine.
  - [ ] Ajouter dans chaque module un adaptateur concret sous `Emulators/<Cœur>` qui implémente `IEmulatorAdapter`, son installation, son protocole et la création de `Machine`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs`, `src/GWGUI.Emulation.Sony/Common/Dictionaries/EmulatorCatalog.cs`, `src/GWGUI.Emulation.Microsoft/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nec/Common/Dictionaries/EmulatorCatalog.cs` pour refuser explicitement une machine tant qu’aucun adaptateur concret ne la prend en charge.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de refus explicite des machines sans adaptateur.
  - [ ] Relier `CreateRuntimeAsync` de chaque module à l’adaptateur sélectionné et refuser explicitement uniquement les machines sans cœur installé.
  - [ ] Ajouter les lecteurs/writers MediaEngine et les représentations MediaFileSystems/MediaAnalysis réellement nécessaires aux supports déclarés.
  - [ ] Ajouter les ressources de traduction de chaque nouveau libellé dans la base commune existante.
  - [ ] Ajouter les tests autonomes de découverte, configuration, adaptateur, média et nettoyage des artefacts temporaires.
  - [ ] Exécuter la build complète avec tous les modules et vérifier l’exécutable résultant.
- [ ] Stage et commit uniques de cette tranche seulement après réussite de toutes les sous-tâches ci-dessus.

### Constantes de l’explorateur de cartouches

- [x] Supprimer les littéraux techniques du lecteur de banques.
  - [x] Créer `src/GWGUI.MediaFileSystems/Constants/ConsoleCartridgeMetadataConstants.cs` avec les clés, attributs, type d’entrée et limites utilisés par l’explorateur.
  - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/ConsoleCartridgeFileSystemReader.cs` pour consommer uniquement ces constantes et le format de nom de banque défini.
  - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour vérifier les entrées et métadonnées produites après cette centralisation.
  - [x] Compiler MediaFileSystems et exécuter le test média ciblé.
- [ ] Stage et commit de cette correction seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Sega Saturn

- [x] Ajouter un adaptateur Sega Saturn concret avec le protocole déjà utilisé par Sega.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Sega/Emulators/Yabause/` en reprenant les contrats, services et hôte du cœur Sega existant, puis remplacer les identifiants par ceux de Yabause.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` et `src/GWGUI.Emulation.Sega/Resources/00-Base/Emulation.resx` pour rattacher Saturn au nouvel adaptateur et à sa description.
  - [x] Ajouter `src/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de découverte de l’adaptateur Saturn.
  - [x] Modifier `src/GWGUI.Emulation.Sega/EmulationGlobalUsings.cs` et les fichiers de `Emulators/GenesisPlusGX` et `Emulators/Yabause` pour importer explicitement leurs namespaces propres sans ambiguïté entre cœurs.
  - [x] Compiler le module Sega et exécuter les tests d’architecture.
- [x] Stage et commit de l’adaptateur Sega Saturn seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo NES/Famicom Disk System

- [x] Ajouter un adaptateur Nintendo NES/Famicom Disk System concret avec le protocole propre au cœur Mesen.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Mesen/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Mesen.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `Nes` et `FamicomDisk` à Mesen.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de sélection de Mesen pour NES et Famicom Disk System.
  - [x] Relier `NintendoEmulationModule.CreateRuntimeAsync` et `TryHandleHostCommand` au moteur et à l’adaptateur Mesen.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo NES/Famicom Disk System seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo SNES

- [x] Ajouter un adaptateur Nintendo SNES concret avec le protocole propre au cœur Snes9x.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Snes9x/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Snes9x.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `Snes` à Snes9x.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Snes9x pour SNES.
  - [x] Relier le moteur Nintendo à l’adaptateur Snes9x par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo SNES seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur NEC PC Engine / SuperGrafx

- [x] Ajouter un adaptateur NEC PC Engine concret avec le protocole propre au cœur Beetle PCE FAST.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nec/Emulators/BeetlePce/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Beetle PCE FAST.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nec/Resources/00-Base/Emulation.resx` pour rattacher uniquement `PcEngine`, `CoreGrafx`, `SuperGrafx`, `PcEngineDuo` et `TurboExpress` à Beetle PCE FAST.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de sélection de Beetle PCE FAST pour les modèles PC Engine.
  - [x] Relier `NecEmulationModule.CreateRuntimeAsync` et `TryHandleHostCommand` au moteur et à l’adaptateur Beetle PCE FAST.
  - [x] Compiler le module NEC et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur NEC PC Engine / SuperGrafx seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Sony PlayStation

- [x] Ajouter un adaptateur Sony PlayStation concret avec le protocole propre au cœur SwanStation.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Sony/Emulators/SwanStation/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de SwanStation.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Sony/Resources/00-Base/Emulation.resx` pour rattacher uniquement `PlayStation` à SwanStation.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de SwanStation pour PlayStation.
  - [x] Relier `SonyEmulationModule.CreateRuntimeAsync` et `TryHandleHostCommand` au moteur et à l’adaptateur SwanStation.
  - [x] Compiler le module Sony et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Sony PlayStation seulement après réussite de toutes les sous-tâches ci-dessus.

- [x] Créer les cinq projets de modules, leurs manifestes et leurs factories.
  - [x] Ajouter la façade `IEmulationModule` et le catalogue de machines Sega.
- [x] Ajouter la façade `IEmulationModule` et le catalogue de machines Nintendo.
- [x] Ajouter la façade `IEmulationModule` et le catalogue de machines Sony.
- [x] Ajouter la façade `IEmulationModule` et le catalogue de machines Microsoft.
- [x] Ajouter la façade `IEmulationModule` et le catalogue de machines NEC.
- [ ] Brancher les cœurs, un adaptateur à la fois, dans `Emulators/<Cœur>`.
  - [ ] Documenter pour chaque adaptateur son identifiant, sa version et ses médias.
  - [ ] Refuser explicitement une machine dont aucun adaptateur n'est installé.
- [ ] Compléter les formats communs de `MediaEngine`.
  - [ ] Ajouter les lecteurs/writers nommés des cartouches console et leurs représentations par banques.
  - [ ] Ajouter les lecteurs de banques nommés par famille à `MediaFileSystems`.
  - [ ] Ajouter les règles de contenu ROM pour les extensions de cartouches et les banques extraites à `MediaAnalysis`.
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
- [x] Créer `src/GWGUI.Emulation.Nintendo/GWGUI.Emulation.Nintendo.csproj`, `module.json` et la factory.
- [x] Créer `src/GWGUI.Emulation.Nintendo/Common/Machines/MachineCatalog.cs` et les configurations Nintendo.
- [ ] Créer les adaptateurs concrets `src/GWGUI.Emulation.Nintendo/Emulators/<Cœur>/*` pour NES/FDS, SNES, GB/GBA, N64, DS, 3DS, GameCube/Wii et Wii U.
- [x] Créer `src/GWGUI.Emulation.Sony/GWGUI.Emulation.Sony.csproj`, `module.json` et la factory.
- [x] Créer `src/GWGUI.Emulation.Sony/Common/Machines/MachineCatalog.cs` et les configurations Sony.
- [ ] Créer les adaptateurs concrets `src/GWGUI.Emulation.Sony/Emulators/<Cœur>/*` pour PS1, PS2 et PSP.
- [x] Créer `src/GWGUI.Emulation.Nec/GWGUI.Emulation.Nec.csproj`, `module.json` et la factory.
- [x] Créer `src/GWGUI.Emulation.Nec/Common/Machines/MachineCatalog.cs` et les configurations PC Engine.
- [ ] Créer les adaptateurs concrets `src/GWGUI.Emulation.Nec/Emulators/<Cœur>/*` pour PCE, SGX et PC-FX.
- [x] Créer `src/GWGUI.Emulation.Microsoft/GWGUI.Emulation.Microsoft.csproj`, `module.json` et la factory.
- [ ] Créer les lecteurs MediaEngine pour FDS, Sega Card/HuCard et CDI/GDI/CHD multi-pistes.

Ce document ne prétend pas qu'un cœur ou un format est déjà implémenté : chaque
case sera cochée seulement après le fichier et le comportement correspondants.
