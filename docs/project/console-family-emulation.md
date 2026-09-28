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

### Support Microsoft Xbox XDVDFS/XISO

- [x] Ajouter le format nommé `microsoft.xbox.xdvdfs` sans masquer les ISO 9660 existantes.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` et `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs` pour déclarer l'identifiant XDVDFS.
  - [x] Conserver `.iso` comme extension optique existante et ajouter uniquement la constante `.xbe` nécessaire à l'analyse des exécutables Xbox.
  - [x] Créer le Reader/Writer XDVDFS dans `src/GWGUI.MediaEngine/Images/Formats/Optical/Xdvdfs/` avec validation de la signature du secteur 32, des bornes et des entrées.
  - [x] Enregistrer le Reader/Writer dans les compositions MediaEngine avant le lecteur ISO générique, sans modifier le comportement ISO 9660.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Xbox/Xdvdfs/` pour parcourir l'arbre binaire des répertoires et lire les fichiers.
  - [x] Ajouter la règle de contenu des exécutables Xbox `.xbe` dans `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` avec la signature `XBEH`.
  - [x] Ajouter les tests de lecture, exploration, conversion et nettoyage dans `tests/GWGUI.Tests/Media/XdvdfsMediaFormatTests.cs`.
  - [x] Compiler MediaEngine, MediaFileSystems et MediaAnalysis puis exécuter les tests ciblés.
- [x] Stage et commit de la tranche XDVDFS seulement après réussite de toutes les sous-tâches ci-dessus.

## Ordre d'implémentation

### Formats de cartouche spécifiques (à réaliser avant l'exposition utilisateur)

- [x] Déclarer les formats de cartouche par famille, sans identifiant générique visible.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` pour ajouter les identifiants Nintendo, Sega et NEC correspondant aux extensions réelles.
  - [x] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs` pour exposer les mêmes identifiants au moteur.
  - [x] Créer les lecteurs et writers de cartouches dans `src/GWGUI.MediaEngine/Images/Formats/Cartridge/Console/`, avec un identifiant et des extensions propres à chaque famille.
  - [x] Créer l’explorateur des banques dans `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/`, avec des formats console identifiés explicitement.
  - [x] Supprimer les anciens lecteurs de cartouche génériques et vérifier qu’aucune référence à cette ancienne structure ne reste dans `src`, `tests` ou la documentation.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/ImageFormatCatalog.cs` pour afficher le nom du format console réel.
  - [x] Modifier `src/GWGUI.App/Resources/00-Base/Formats.resx` pour localiser les formats spécifiques ajoutés dans la base commune.
  - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` avec un test autonome et nettoyage des fichiers temporaires dans `finally`.
  - [x] Compiler MediaEngine, MediaFileSystems et MediaAnalysis puis exécuter les tests ciblés.
  - [x] Retirer le libellé historique de cartouche générique de la documentation.

### Tranche en cours (à terminer avant tout commit)

- [x] Corriger les ressources de modèles copiées depuis Amstrad.
  - [x] Modifier les cinq `Resources/00-Base/Emulation.resx` pour supprimer les clés de modèles d’une autre famille et écrire celles correspondant aux `ModelCatalog` Sega, Nintendo, Sony, Microsoft et NEC.
  - [x] Reproduire ces clés dans les cultures existantes de chaque module sans conserver de valeur Amstrad résiduelle.
  - [x] Rechercher les identifiants `464`, `664`, `6128`, `GX4000` et les descriptions Amstrad résiduelles dans ces ressources, puis compiler les cinq modules.

### Identifiants interprocessus des hôtes de cœur

- [x] Supprimer les préfixes `gwgui-amstrad-*` restés dans les hôtes Sega et Nintendo.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Core/Constants/ProcessCoreConstants.cs` pour utiliser les préfixes `gwgui-sega-genesisplusgx-*`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Mesen/Core/Constants/ProcessCoreConstants.cs` pour utiliser les préfixes `gwgui-nintendo-mesen-*`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Snes9x/Core/Constants/ProcessCoreConstants.cs` pour utiliser les préfixes `gwgui-nintendo-snes9x-*`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/GameWatch/Core/Constants/ProcessCoreConstants.cs` pour utiliser les préfixes `gwgui-nintendo-gw-*`.
  - [x] Rechercher les anciennes chaînes dans les cinq modules, compiler les modules concernés et exécuter les tests d’architecture.
- [x] Stage et commit de cette correction seulement après réussite des vérifications.

### Plan d’intégration Wii U sans couche générique

- [x] Remplacer la référence Cemu non vérifiée par un contrat d’intégration natif.
  - [x] Modifier `docs/project/console-family-emulation.md` pour retirer l’ancien paquet générique non vérifié de la checklist.
  - [x] Décrire dans cette section le binaire Cemu natif, son protocole de lancement et la capture vidéo/input nécessaires avant toute création de fichier sous `src/GWGUI.Emulation.Nintendo/Emulators/Cemu/`.
- [ ] Finaliser les cinq modules sans laisser de façade non exécutable.
  - [ ] Créer `src/GWGUI.Emulation.Nec/GWGUI.Emulation.Nec.csproj`, `module.json`, `Modules/NecEmulationModuleFactory.cs` et le catalogue PC Engine.
  - [ ] Ajouter dans chaque module un adaptateur concret sous `Emulators/<Cœur>` qui implémente `IEmulatorAdapter`, son installation, son protocole et la création de `Machine`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs`, `src/GWGUI.Emulation.Sony/Common/Dictionaries/EmulatorCatalog.cs`, `src/GWGUI.Emulation.Microsoft/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nec/Common/Dictionaries/EmulatorCatalog.cs` pour refuser explicitement une machine tant qu’aucun adaptateur concret ne la prend en charge.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de refus explicite des machines sans adaptateur.
  - [ ] Relier `CreateRuntimeAsync` de chaque module à l’adaptateur sélectionné et refuser explicitement uniquement les machines sans cœur installé.
  - [ ] Ajouter les lecteurs/writers MediaEngine et les représentations MediaFileSystems/MediaAnalysis réellement nécessaires aux supports déclarés.
  - [ ] Ajouter les ressources de traduction de chaque nouveau libellé dans la base commune existante.
  - [ ] Ajouter les tests autonomes de découverte, configuration, adaptateur, média et nettoyage des artefacts temporaires.
  - [x] Exécuter la build complète avec tous les modules et vérifier l’exécutable résultant (`F:\GW GUI\build\Debug\GW GUI\gwgui.exe`).
- [ ] Stage et commit uniques de cette tranche seulement après réussite de toutes les sous-tâches ci-dessus.

### Alignement des constantes Common

- [x] Supprimer l’écart introduit dans le fichier `MediaConstants` d’Amiga sans modifier les autres familles.
  - [x] Ajouter `DiskChangeDelayMilliseconds` au contrat commun `src/GWGUI.Emulation/Constants/EmulationMediaSlotConstants.cs`.
  - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Services/Machine.Lifecycle.cs` pour utiliser la constante commune.
  - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Constants/MediaConstants.cs` pour retrouver exactement la structure des autres familles.
  - [x] Exécuter le test `FamilyCommonConstantFilesAndMembersAreIdentical`.
- [x] Stage et commit de cette correction seulement après réussite du test d’architecture.

### Image optique Nintendo GameCube/Wii

- [x] Reconnaître les images `.gcm` qui utilisent le profil ISO sectoriel existant.
  - [x] Ajouter la constante d’extension dans `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs`.
  - [x] Ajouter `.gcm` aux extensions du profil `IsoFormat` pour la lecture et l’écriture lossless.
  - [x] Ajouter `.gcm` aux extensions d’analyse et à l’icône optique existante.
  - [x] Ajouter un test autonome de lecture, conversion et nettoyage dans `tests/GWGUI.Tests/Media/GameCubeIsoMediaFormatTests.cs`.
  - [x] Compiler les projets concernés et exécuter le test ciblé.
- [x] Stage et commit de cette tranche seulement après réussite du test média.

### Exploration FST Nintendo GameCube/Wii

- [x] Exposer l'arbre FST des images `.gcm` sans les traiter comme un ISO9660 générique.
  - [x] Ajouter `src/GWGUI.MediaFileSystems/Definitions/FileSystemIds.cs` avec l'identifiant technique FST Nintendo.
  - [x] Ajouter `src/GWGUI.MediaEngine/Images/Formats/Optical/Iso/IsoReader.cs` avec l'extension source reconnue dans les métadonnées du document.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/GameCube/GameCubeFstFileSystemConstants.cs` avec les offsets, limites et attributs FST.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/GameCube/GameCubeFstFileSystemReader.cs` pour lire la table FST big-endian, reconstruire les dossiers, lire les fichiers et refuser les bornes invalides.
  - [x] Enregistrer le lecteur dans `src/GWGUI.MediaFileSystems/Exploration/MediaExplorer.cs` avant les lecteurs ISO/UDF génériques.
  - [x] Ajouter `tests/GWGUI.Tests/Media/GameCubeFstFileSystemReaderTests.cs` avec une image synthétique, l'exploration et le nettoyage des artefacts temporaires dans `finally`.
  - [x] Exécuter le test ciblé.
  - [x] Exécuter la suite complète.
- [x] Stage et commit de cette tranche seulement après réussite de toutes les sous-tâches ci-dessus.

### Détection des ISO GameCube par en-tête

- [x] Reconnaître les dumps GameCube `.iso` par leur en-tête réel, sans détourner les ISO 9660 ordinaires.
  - [x] Ajouter dans `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/GameCube/GameCubeFstFileSystemConstants.cs` les constantes du mot magique et de son offset.
  - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/GameCube/GameCubeFstFileSystemReader.cs` pour autoriser `.iso` uniquement après validation du mot magique GameCube.
  - [x] Ajouter dans `tests/GWGUI.Tests/Media/GameCubeFstFileSystemReaderTests.cs` un cas `.iso` et conserver le cas `.gcm`.
  - [x] Exécuter le test ciblé puis la suite complète.
- [x] Stage et commit de cette tranche seulement après réussite de toutes les sous-tâches ci-dessus.

### Analyse des exécutables Xbox 360

- [x] Classer les fichiers Xbox 360 `.xex` à partir de leur signature `XEX2`.
  - [x] Ajouter la signature `XEX2` dans `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs`.
  - [x] Ajouter la règle commune `.xex` signée dans `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` sans supprimer la règle Atari 8-bit.
  - [x] Ajouter un test de classification signé dans `tests/GWGUI.Tests/Media/MediaContentRecognitionCatalogScenarios.cs`.
  - [x] Compiler MediaAnalysis et exécuter le test média ciblé.
- [x] Stage et commit de cette tranche seulement après réussite du test.

### Constantes de l’explorateur de cartouches

- [x] Supprimer les littéraux techniques du lecteur de banques.
  - [x] Créer `src/GWGUI.MediaFileSystems/Constants/ConsoleCartridgeMetadataConstants.cs` avec les clés, attributs, type d’entrée et limites utilisés par l’explorateur.
  - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/ConsoleCartridgeFileSystemReader.cs` pour consommer uniquement ces constantes et le format de nom de banque défini.
  - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour vérifier les entrées et métadonnées produites après cette centralisation.
  - [x] Compiler MediaFileSystems et exécuter le test média ciblé.
- [x] Stage et commit de cette correction seulement après réussite de toutes les sous-tâches ci-dessus.

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
- [x] Stage et commit de l'adaptateur Nintendo NES/Famicom Disk System seulement après réussite de toutes les sous-tâches ci-dessus.

### Support MediaEngine du Famicom Disk System

- [x] Ajouter le format nommé Famicom Disk System sans identifiant de cartouche générique.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` et `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs` pour ajouter `nintendo.fds`.
  - [x] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs` pour ajouter l’extension `.fds`.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Floppy/FamicomFds/FamicomFdsConstants.cs` avec la signature, la taille d’une face et les clés de métadonnées.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Floppy/FamicomFds/FamicomFdsReader.cs` pour lire les images FDS avec ou sans en-tête et exposer les faces comme blocs adressables.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Floppy/FamicomFds/FamicomFdsWriter.cs` pour réécrire les faces sans ajouter de remplissage inventé.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Reading/MediaRecognitionComposition.cs` et `src/GWGUI.MediaEngine/Images/Writing/MediaWritingComposition.cs` pour enregistrer ce lecteur et ce writer.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/ImageFormatCatalog.cs` pour publier le format `.fds` dans la famille Nintendo.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/FamicomDisk/FamicomDiskFileSystemReader.cs` et l’enregistrer dans `src/GWGUI.MediaFileSystems/Exploration/MediaExplorer.cs` pour explorer les fichiers FDS réellement décodables.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` pour classer `.fds` comme image disque.
  - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` avec un test média autonome et suppression des fichiers temporaires dans `finally`.
- [x] Compiler les projets concernés et exécuter le test ciblé avant le commit de cette tranche.

### Support optique Dreamcast GDI

- [x] Ajouter le descripteur GDI nommé et ses constantes sans réintroduire de format générique.
  - [x] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs` et `src/GWGUI.MediaEngine/Constants/OpticalImageFormatIds.cs` pour déclarer `.gdi` et `optical-gdi`.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/Gdi/GdiConstants.cs` et `GdiDescriptorReader.cs` pour valider les pistes et leurs fichiers associés.
- [x] Ajouter la lecture et l’écriture des pistes GDI dans MediaEngine.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/Gdi/GdiReader.cs` avec une représentation `OpticalMediaImageRepresentation` fidèle aux secteurs déclarés.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/Gdi/GdiWriter.cs` pour écrire le descripteur et les données de piste sans compléter artificiellement les fichiers.
  - [x] Enregistrer le lecteur et le writer dans `MediaRecognitionComposition` et `MediaWritingComposition`.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/ImageFormatCatalog.cs` pour afficher « Dreamcast GDI ».
- [x] Exposer GDI dans les couches d’exploration et d’analyse.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` et `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour l’identifiant et l’extension GDI.
  - [x] Ajouter le classement `.gdi` dans `CommonMediaContentRecognitionTable`; l’exploration optique existante consomme les pistes GDI décodées.
- [x] Ajouter le test autonome GDI, compiler les projets concernés et exécuter les tests avant le commit de cette tranche.

### Cartouches Sega My Card et NEC SuperGrafx

- [x] Reconnaître `.mv` comme variante SG-1000 / Sega My Card dans les constantes, l’analyse, l’icône, le catalogue et le lecteur de cartouches existants.
- [x] Ajouter le format nommé `nec.supergrafx` pour `.sgx`, avec lecture, écriture, visualisation par banques et exploration via les couches déjà utilisées par les autres cartouches.
- [x] Ajouter le test de round-trip et d’exploration des deux extensions.
- [ ] Vérifier les images My Card et SuperGrafx réelles du corpus utilisateur.

### Descripteurs optiques déjà décodés

- [x] Publier `.cue`, `.ccd`, `.mds` et `.chd` dans les constantes d’analyse et la classification disque.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` avec les extensions optiques manquantes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` pour classer ces extensions comme images disque.
- [x] Ajouter l’icône optique pour `.chd` dans `src/GWGUI.MediaEngine/Constants/MediaIconExtensions.cs` et `src/GWGUI.MediaEngine/Images/Visualization/MediaIconSelector.cs`.
- [x] Ajouter un test autonome de publication des extensions optiques et nettoyer ses artefacts temporaires.

### Pistes audio WAVE dans BIN/CUE

- [x] Lire les feuilles CUE qui référencent des fichiers WAVE PCM Red Book.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Reading/Sources/WavePcmRandomAccessData.cs` pour exposer uniquement le bloc `data` PCM d’un fichier RIFF/WAVE validé.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/BinCue/WavePcmConstants.cs` avec les signatures, paramètres PCM et tailles de secteurs audio.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/Optical/BinCue/BinCueFormat.cs` pour accepter `.wav` comme fichier associé d’une feuille CUE.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/Optical/BinCue/BinCueReader.cs` pour lire les pistes WAVE uniquement en mode `AUDIO` et les exposer dans la représentation optique commune.
  - [x] Ajouter `tests/GWGUI.Tests/Media/BinCueWaveMediaFormatTests.cs` avec une feuille CUE/WAVE synthétique, une conversion BIN/CUE et la suppression des artefacts dans `finally`.
- [x] Compiler MediaEngine et exécuter le test média ciblé avant le commit de cette tranche.

### Adaptateur Nintendo Virtual Boy

- [x] Ajouter un adaptateur Nintendo Virtual Boy concret avec le protocole propre au cœur Beetle VB.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/BeetleVb/` en reprenant les contrats et l’hôte déjà utilisés par les cœurs Nintendo, puis adapter l’installation, les identifiants et le média `.vb`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `VirtualBoy` à Beetle VB.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Beetle VB pour Virtual Boy.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de configuration.
- [x] Stage et commit de l’adaptateur Nintendo Virtual Boy seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo Game & Watch

- [x] Ajouter l’adaptateur nommé `gw` et son cœur Windows x64 dans `src/GWGUI.Emulation.Nintendo/Emulators/GameWatch/`, en reprenant le protocole d’un adaptateur Nintendo existant sans créer de couche générique.
  - [x] Relier la factory exclusivement au modèle `GameWatch` et à la commande hôte Nintendo dédiée.
  - [x] Déclarer la description et les messages du cœur dans les ressources Nintendo.
  - [x] Compiler le module Nintendo et vérifier la sélection du cœur par un test d’architecture.
- [x] Ajouter le format nommé `.mgw` dans `MediaEngine`, `MediaFileSystems` et `MediaAnalysis`, avec lecture, écriture, exploration par banques et icône cartouche.
- [x] Ajouter les URL de catalogue de mise à jour aux cinq manifestes de modules afin que le build avec modules puisse les valider.
- [ ] Vérifier avec un fichier `.mgw` réel fourni par l’utilisateur.

### Adaptateur Nintendo 3DS (Citra)

- [x] Ajouter l’adaptateur nommé `citra` dans `src/GWGUI.Emulation.Nintendo/Emulators/Citra/`, avec le téléchargement x64 du cœur Citra et une factory limitée au modèle `Nintendo3Ds`.
- [x] Déclarer les messages d’erreur et la description Citra dans les ressources Nintendo.
- [x] Ajouter les extensions Citra `.3ds`, `.3dsx`, `.elf`, `.axf`, `.cci`, `.cxi` et `.app` au format nommé `nintendo.3ds` pour lecture, écriture, visualisation et exploration par banques.
- [x] Ajouter le test de sélection de Citra pour Nintendo 3DS et compiler le module Nintendo.
- [ ] Vérifier le rendu matériel Citra avec un fichier 3DS réel fourni par l’utilisateur.

### Adaptateur Nintendo GameCube/Wii (Dolphin)

- [x] Ajouter l’adaptateur nommé `dolphin` dans `src/GWGUI.Emulation.Nintendo/Emulators/Dolphin/`, avec le cœur Windows x64 et une factory limitée aux modèles `GameCube` et `Wii`.
- [x] Déclarer le rendu logiciel par défaut et les messages d’erreur/description Dolphin dans les ressources Nintendo.
- [x] Ajouter le test de sélection de Dolphin pour GameCube et Wii et compiler le module Nintendo.
- [ ] Vérifier un jeu GameCube et un jeu Wii réels avec les fichiers système Dolphin requis.

### Adaptateur Nintendo Wii U (Cemu)

L’adaptation devra utiliser la distribution native de Cemu, sans DLL générique.
Avant toute création de l’adaptateur, il faut établir le contrat réel de
lancement de Cemu, le chargement du jeu, le transport des images vidéo,
les entrées et la libération du processus ; tant que ces points ne sont pas
vérifiés, aucun fichier Cemu ne doit être ajouté au module.

- [ ] Étendre l’hôte de cœur Nintendo avec un contexte graphique matériel Vulkan/OpenGL partagé, requis par Cemu.
  - [ ] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Cemu/Core/Services/ExternalHostCallbacks.Environment.cs` pour fournir `SetHwRender` et les callbacks de contexte.
  - [ ] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Cemu/Core/Services/ProcessCore.cs` et le protocole d’hôte pour transférer les frames matérielles vers la surface vidéo existante.
  - [ ] Ajouter le test d’initialisation et de libération du contexte matériel dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs`.
- [ ] Ajouter l’adaptateur Cemu dans `src/GWGUI.Emulation.Nintendo/Emulators/Cemu/` avec le protocole de cœur déjà utilisé par les adaptateurs Nintendo.
  - [ ] Déclarer la distribution Windows x64 de Cemu natif, son exécutable et sa commande hôte après vérification du protocole.
  - [ ] Relier uniquement le modèle `WiiU` à Cemu et laisser le cœur fournir ses options vidéo et système.
  - [ ] Ajouter les ressources de description et d’erreur Cemu dans les ressources Nintendo.
  - [ ] Ajouter le test d’architecture de sélection Cemu pour `WiiU`.
- [ ] Compiler le module Nintendo et exécuter le test d’architecture avant le commit.

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

### Adaptateur Nintendo Game Boy

- [x] Ajouter un adaptateur Nintendo Game Boy / Game Boy Color concret avec le protocole propre au cœur Gambatte.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Gambatte/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Gambatte.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `GameBoy` et `GameBoyColor` à Gambatte.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de sélection de Gambatte pour Game Boy et Game Boy Color.
  - [x] Relier le moteur Nintendo à l’adaptateur Gambatte par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo Game Boy seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo Game Boy Advance

- [x] Ajouter un adaptateur Nintendo Game Boy Advance concret avec le protocole propre au cœur mGBA.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Mgba/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de mGBA.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `GameBoyAdvance` à mGBA.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de mGBA pour Game Boy Advance.
  - [x] Relier le moteur Nintendo à l’adaptateur mGBA par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo Game Boy Advance seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo 64

- [x] Ajouter un adaptateur Nintendo 64 concret avec le protocole propre au cœur Mupen64Plus-Next.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Mupen64PlusNext/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Mupen64Plus-Next.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Dictionaries/ModelCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `Nintendo64` à Mupen64Plus-Next.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Mupen64Plus-Next pour Nintendo 64.
  - [x] Relier le moteur Nintendo à l’adaptateur Mupen64Plus-Next par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo 64 seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo DS

- [x] Ajouter un adaptateur Nintendo DS / DSi concret avec le protocole propre au cœur melonDS.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/MelonDs/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de melonDS.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Dictionaries/ModelCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `NintendoDs` à melonDS.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de melonDS pour Nintendo DS.
  - [x] Relier le moteur Nintendo à l’adaptateur melonDS par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo DS seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateurs Sony PlayStation 2 et PSP

- [x] Ajouter des adaptateurs Sony PlayStation 2 et PSP concrets avec les protocoles propres aux cœurs PCSX2 et PPSSPP.
  - [x] Créer `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/` et `src/GWGUI.Emulation.Sony/Emulators/Ppsspp/` en reprenant les contrats, services et hôtes Sony déjà intégrés, puis remplacer les identifiants, téléchargements et médias.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Resources/00-Base/Emulation.resx` pour ajouter les erreurs et descriptions localisées des deux cœurs.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de sélection PCSX2 pour `PlayStation2` et PPSSPP pour `Psp`.
  - [x] Relier le moteur Sony aux deux adaptateurs par sa découverte d’adaptateurs.
  - [x] Compiler le module Sony et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit des adaptateurs Sony PlayStation 2 et PSP seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur NEC PC-FX

- [x] Ajouter un adaptateur NEC PC-FX concret avec le protocole propre au cœur Beetle PC-FX.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nec/Emulators/BeetlePcfx/` en reprenant le protocole d’hôte NEC déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux du cœur PC-FX.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Resources/00-Base/Emulation.resx` pour ajouter les erreurs et la description localisées de Beetle PC-FX.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Beetle PC-FX pour `PcFx`.
  - [x] Relier le moteur NEC à l’adaptateur Beetle PC-FX par sa découverte d’adaptateurs.
  - [x] Compiler le module NEC et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur NEC PC-FX seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Sega Dreamcast

- [x] Ajouter un adaptateur Sega Dreamcast concret avec le protocole propre au cœur Flycast.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Sega/Emulators/Flycast/` en reprenant le protocole d’hôte Sega déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Flycast.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/Emulation.resx` pour ajouter les erreurs et la description localisées de Flycast.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Flycast pour `Dreamcast`.
  - [x] Relier le moteur Sega à l’adaptateur Flycast par sa découverte d’adaptateurs.
  - [x] Compiler le module Sega et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Sega Dreamcast seulement après réussite de toutes les sous-tâches ci-dessus.

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
- [x] Créer les adaptateurs concrets `src/GWGUI.Emulation.Sega/Emulators/<Cœur>/*` pour les cœurs Sega retenus.
- [x] Créer `src/GWGUI.Emulation.Nintendo/GWGUI.Emulation.Nintendo.csproj`, `module.json` et la factory.
- [x] Créer `src/GWGUI.Emulation.Nintendo/Common/Machines/MachineCatalog.cs` et les configurations Nintendo.
- [ ] Créer les adaptateurs concrets `src/GWGUI.Emulation.Nintendo/Emulators/<Cœur>/*` pour NES/FDS, SNES, GB/GBA, N64, DS, 3DS, GameCube/Wii et Wii U.
- [x] Créer `src/GWGUI.Emulation.Sony/GWGUI.Emulation.Sony.csproj`, `module.json` et la factory.
- [x] Créer `src/GWGUI.Emulation.Sony/Common/Machines/MachineCatalog.cs` et les configurations Sony.
- [x] Créer les adaptateurs concrets `src/GWGUI.Emulation.Sony/Emulators/<Cœur>/*` pour PS1, PS2 et PSP.
- [x] Créer `src/GWGUI.Emulation.Nec/GWGUI.Emulation.Nec.csproj`, `module.json` et la factory.
- [x] Créer `src/GWGUI.Emulation.Nec/Common/Machines/MachineCatalog.cs` et les configurations PC Engine.
- [x] Créer les adaptateurs concrets `src/GWGUI.Emulation.Nec/Emulators/<Cœur>/*` pour PCE, SGX et PC-FX.
- [x] Créer `src/GWGUI.Emulation.Microsoft/GWGUI.Emulation.Microsoft.csproj`, `module.json` et la factory.
- [ ] Créer les lecteurs MediaEngine pour FDS, Sega Card/HuCard et CDI/GDI/CHD multi-pistes.

Ce document ne prétend pas qu'un cœur ou un format est déjà implémenté : chaque
case sera cochée seulement après le fichier et le comportement correspondants.
