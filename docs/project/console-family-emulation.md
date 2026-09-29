# Plan ordonné — familles de consoles et médias

Ce fichier est la liste de travail active. Il est volontairement détaillé :
point principal, groupe, tâche, sous-tâche, puis action concrète sur un fichier.
Une feuille ne peut être cochée qu'après l'écriture effective du fichier, la
vérification de son comportement et les tests utiles. Une lecture seule n'est
jamais une feuille terminale.

## 0. Règles d'exécution obligatoires

- [x] Respecter `.codex/config.toml` pour toute la réalisation.
  - [x] Conserver la hiérarchie et l'ordre des cases dans ce document.
    - [x] Écrire chaque dernière case comme une action `Créer`, `Modifier`,
      `Déplacer`, `Renommer` ou `Supprimer` avec un chemin précis.
    - [x] Ajouter une nouvelle action après la dernière case cochée avant de
      la réaliser si un besoin apparaît.
  - [x] Ne pas faire de revue globale avant de modifier un fichier.
    - [x] Ouvrir uniquement les fichiers nécessaires à la feuille courante.
    - [x] Ne corriger les lignes cassées que dans les fichiers effectivement
      modifiés, sauf demande distincte.
- [x] Préserver les frontières du projet.
  - [x] Conserver `GWGUI.App -> GWGUI.Emulation -> GWGUI.Emulation.<famille>/Common`.
    - [x] Reprendre les noms et le contenu des fichiers `Common` d'Amiga,
      Atari et Amstrad au lieu d'inventer une seconde architecture.
  - [x] Conserver `GWGUI.App -> GWGUI.MediaEngine -> GWGUI.MediaFileSystems -> GWGUI.MediaAnalysis`.
    - [x] Laisser la logique de média dans ces trois bibliothèques; App ne
      fait qu'afficher et appeler les contrats existants.
  - [x] Conserver `Emulators/<émulateur>/` sans niveau `Core`.
    - [x] Ne jamais créer `Emulators/<émulateur>/Core` ni une couche Libretro.
- [x] Préserver les données utilisateur.
  - [x] Ne pas modifier les médias de `F:\Retro`.
    - [x] Pour un ZIP, extraire dans `<dossier du ZIP>\<nom sans extension>`
      seulement si ce dossier n'existe pas, sans supprimer l'archive.
  - [x] Ne pas stocker de secret, de clé ou de jeton dans le dépôt.
- [ ] Appliquer les règles de fin de tranche.
  - [ ] Vérifier les tests ciblés après toutes les feuilles du groupe.
    - [ ] Modifier le fichier de test de la tranche pour couvrir uniquement le
      comportement ajouté et supprimer ses fichiers temporaires.
  - [ ] Créer un commit seulement après code, tests et traductions terminés.
    - [ ] Modifier ce document dans le même commit que la fonctionnalité,
      jamais dans un commit de documentation seul.

## 1. Périmètre fonctionnel à conserver

- [ ] Réaliser les familles demandées sans en ajouter d'autre.
  - [ ] Sega : SG-1000, SC-3000, Mark III, Master System I/II, Game Gear,
    Mega Drive/Genesis, Saturn et Dreamcast.
    - [ ] Modifier le catalogue Sega pour une seule machine Master System et
      une seule machine Mega Drive; les variantes restent des options.
  - [ ] Nintendo : Game & Watch, NES/Famicom, Famicom Disk, SNES/SFC, Virtual
    Boy, N64, GB/GBC/GBA, DS/DSi, 3DS, GameCube, Wii, Wii U et Switch vérifiable.
    - [ ] Modifier le catalogue Nintendo et ses adaptateurs un cœur à la fois.
  - [ ] Sony : PlayStation/PS one, PS2/PStwo, PSP Slim/Lite/Go, Vita, PS3,
    PS4 et PS5 vérifiables.
    - [ ] Modifier le catalogue Sony et ses adaptateurs uniquement pour les
      cœurs réellement intégrés.
  - [ ] Microsoft : Xbox et Xbox 360 vérifiables.
    - [ ] Modifier le catalogue Microsoft et ses adaptateurs uniquement pour
      les cœurs réellement intégrés.
  - [ ] NEC : PC Engine/TurboGrafx-16, CoreGrafx, SuperGrafx, Duo/TurboDuo,
    GT/TurboExpress, LT et PC-FX vérifiables.
    - [ ] Modifier le catalogue NEC et ses adaptateurs uniquement pour les
      cœurs réellement intégrés.
  - [ ] Ajouter un ordinateur non-x86/x64 IBM uniquement avec un cœur et des
    médias réellement intégrés.
    - [ ] Modifier le catalogue de la famille concernée avec le modèle et le
      cœur vérifiés, sans créer de famille supplémentaire.

## 2. Sega — catalogue et rattachement des cœurs

- [x] Terminer le catalogue de modèles et ses contrats.
  - [x] Décrire les caractéristiques matérielles vérifiées.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ModelContracts.cs` pour CPU, VDP/GPU, audio, RAM/ROM, ports et supports.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` pour SG-1000, SC-3000, Mark III, MasterSystem, MegaDrive, GameGear, Saturn et Dreamcast.
  - [x] Publier les machines avec le nom existant du projet.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/MachineCatalog.cs` pour conserver `ModelCatalog.All`; ce chemin est identique à Amiga, Atari, Amstrad, Nintendo, Sony, Microsoft et NEC.
- [x] Rattacher les cœurs aux machines réelles.
  - [x] Filtrer les définitions inconnues du catalogue.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Dictionaries/EmulatorCatalog.cs` pour ignorer toute définition dont un identifiant n'est pas dans `ModelCatalog.All`.
  - [x] Vérifier chaque factory existante.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour exiger un adaptateur pour chaque machine publiée et refuser Mega-CD/32X comme machines.
    - [x] Conserver `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Factories/GenesisPlusGXMachineFactory.cs` limité à SG-1000, SC-3000, Mark III, MasterSystem, MegaDrive et GameGear.
    - [x] Conserver `src/GWGUI.Emulation.Sega/Emulators/Yabause/Factories/YabauseMachineFactory.cs` limité à Saturn.
    - [x] Conserver `src/GWGUI.Emulation.Sega/Emulators/Flycast/Factories/FlycastMachineFactory.cs` limité à Dreamcast.

## 3. Sega — périphériques et entrée

- [x] Recenser les périphériques indépendamment des capacités d'un cœur.
  - [x] Déclarer les catégories officielles.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Enums/ControllerEnums.cs` avec manettes SMS, Control Stick, Light Phaser, Mega Drive 3/6 boutons, Mega Mouse, Menacer, Saturn et Dreamcast.
  - [x] Relier chaque modèle à ses ports.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ControllerCatalog.cs` pour retourner tous les modèles officiels, y compris ceux absents d'un cœur.
- [x] Afficher correctement le périphérique choisi.
  - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier que chaque périphérique publié par Sega possède une clé de ressource d'affichage non vide et que son dossier temporaire est supprimé dans `finally`.
- [x] Conserver les choix acceptés par la machine.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Settings.cs` pour normaliser le type et le `VisualId` sans remplacer un périphérique officiel par `Joystick`.
- [ ] Relier les visuels déjà existants.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Visuals.cs` pour Mega Drive 3/6 boutons avec `EmulationControllerVisualIds.MegaDrive3/MegaDrive6`.
  - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier la normalisation du visuel 3 boutons, les choix 3/6 boutons et le Light Phaser catalogué.
    - [ ] Modifier `src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs` uniquement si un visuel générique existant manque réellement; ne pas inventer de fichier d'image.
  - [x] Préserver les mappings.
    - [x] Vérifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/InputSnapshotDictionary.cs` avec `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` : le runtime applique le `DeviceId` et les boutons, tandis que `MachineConfiguration` conserve le type, le port et le visuel lors de la sauvegarde/relecture.
  - [x] Relier GameInput au cœur.
  - [x] Choisir l'identifiant exposé par le cœur.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` pour rechercher le nom/ID du périphérique déclaré par le cœur, mapper souris/pointeur pour Light Phaser/Menacer et ne jamais imposer `JoypadDevice`.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalHostCallbacks.Input.cs` pour publier les coordonnées, le déclenchement et l'état hors écran du périphérique Light Phaser via le pointeur commun.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier qu'un périphérique absent du cœur reste non mappé et que le pointeur alimente le protocole Light Phaser.
  - [x] Fermer les ressources empruntées.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Services/Machine.Commands.cs` pour envoyer une entrée vide au cœur avant son arrêt, réinitialiser l'état du pointeur et conserver l'arrêt du cœur/audio dans le `finally` existant.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour vérifier qu'un arrêt normal et un arrêt après erreur relâchent l'entrée avant la destruction du cœur et suppriment le dossier de session temporaire.

## 4. Sega — options matérielles, ports et addons

- [x] Conserver le second slot cartouche générique.
  - [x] Ajouter le protocole stable.
    - [x] Modifier `src/GWGUI.Emulation/Constants/EmulationMediaSlotConstants.cs` avec la valeur du second slot.
    - [x] Modifier `src/GWGUI.Emulation/Contracts/EmulationMediaSlot.cs` avec `Cartridge1` et son aller-retour protocolaire.
- [ ] Persister les options de la machine Mega Drive.
  - [ ] Ajouter les clés techniques dans les contrats existants.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ConfigurationContracts.cs` pour conserver le modèle Mega-CD, son activation désactivée par défaut et la présence du 32X sans ajouter de machine.
  - [ ] Déclarer les valeurs constantes.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/SettingsConstants.cs` avec les clés d'activation et de modèle Mega-CD, activation 32X, région et fréquence.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/ModelConstants.cs` avec les identifiants techniques des deux addons, sans texte utilisateur.
  - [ ] Afficher les choix dans les blocs existants.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` pour le modèle Mega-CD I/II, sa coche d'activation désactivée par défaut et la coche 32X.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/SettingsHelpDictionary.cs` pour les clés d'aide de ces champs.
    - [x] Modifier les catalogues `src/GWGUI.Emulation.Sega/Resources/<culture>/` pour les libellés traduits avec Argos.
  - [ ] Valider les combinaisons matérielles.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/ConfigurationFunctions.cs` pour refuser un CD Mega-CD désactivé, une cartouche `.32x` sans 32X et une Sega Card verrouillée par SMS II/3-D Glasses.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/StorageFunctions.cs` pour n'ajouter le lecteur CD Mega-CD qu'après sa coche d'activation et exposer les extensions Mega Drive/32X.
  - [ ] Transmettre seulement ce que connaît le cœur.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Functions/GenesisPlusGXOptionFunctions.cs` pour filtrer les options persistées sur le catalogue renvoyé par le cœur, puis appliquer ce filtre après l'initialisation du cœur.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` pour refuser proprement chaque extension non supportée, sans construire ni accepter de playlist.
  - [ ] Tester les ports et les addons.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour les deux slots SMS, Mega-CD activé/désactivé, 32X activé/désactivé et le rejet des incompatibilités.

## 5. Sega — firmwares et ROM système

- [x] Déclarer les profils vérifiés.
  - [x] Ajouter les identifiants sans texte brut.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/FirmwareConstants.cs` avec les empreintes vérifiées Genesis Plus GX pour Master System, Game Gear, Mega Drive et Mega-CD; laisser les profils non vérifiés (BIOS 32X, BIOS custom et autres révisions) non sélectionnables.
  - [x] Relier un fichier réellement fourni.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` pour reconnaître les empreintes connues, conserver les noms système attendus par le cœur et laisser les fichiers inconnus sans profil utilisable.
  - [x] Exposer le profil sélectionné.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Modules/SegaEmulationModule.cs` pour scanner le dossier Firmware, exposer les candidats compatibles et persister le chemin sélectionné dans les options de la configuration.
  - [x] Appliquer le profil au cœur.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` pour identifier l'empreinte sélectionnée et copier le fichier sous les noms système attendus, y compris plusieurs alias régionaux.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Yabause/Services/ExternalCore.cs` et `Flycast/Services/ExternalCore.cs` pour transmettre uniquement un firmware accepté par chacun de ces cœurs.
  - [x] Vérifier les profils.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier les empreintes connues, leurs modèles et leurs noms système attendus, ainsi que le refus d'une empreinte fictive.
  - [x] Ajouter les profils vérifiés des cœurs optiques Sega.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/FirmwareConstants.cs` avec les noms et empreintes vérifiés du BIOS Saturn Yabause et du BIOS Dreamcast Flycast, sans ajouter les BIOS non vérifiés.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` pour associer ces deux profils aux modèles Saturn et Dreamcast et à leurs chemins système exacts.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier les deux profils optiques et leurs fichiers système.

## 6. Sega — validation réelle avant Nintendo

- [ ] Valider le fonctionnement réel de Sega avant d'ouvrir la tranche Nintendo.
  - [ ] Préparer un corpus de validation séparé du dépôt.
    - [x] Utiliser `F:\Retro\Sega` uniquement comme corpus externe de validation sans modifier ni versionner ses médias; `F:\Retro\Sega\Homebrew\RTsLastMinuteDemo-SMS-1.00\last-minute.sms` a été extrait et l'archive ZIP est conservée.
    - [x] Référencer le corpus Sega externe déjà disponible sous `F:\Retro\Sega\Roms` (jeux, BIOS et ROM système) sans le modifier ni le versionner; les installations de cœurs et de ROM système de l'application restent dans les chemins gérés par le module, comme Amiga, Atari et Amstrad.
    - [x] Conserver les sources Archive.org fournies pour les essais Sega : `https://archive.org/download/CentralArquivista-SegaCD32x`, `https://archive.org/download/pack-roms-sega-cd-cd-32x`, `https://archive.org/download/pack-roms-sega-chihiro-jeux-arcade`, `https://archive.org/download/sega-model-2_202312/Sega%20Model%202%20Emu%201.1a%20and%20Full%20Romset/Model%202%20Romset%20%28Merged%29/` et `https://archive.org/download/sega_model3/Sega%20Model%203/`.
    - [ ] Compléter la validation Sega avec un média et, si le cœur l'exige, un firmware pour chaque famille Sega restante, en conservant les archives et les fichiers source dans leurs sous-dossiers.
  - [ ] Tester chaque chaîne cœur/média avec le corpus Sega.
    - [ ] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour couvrir les formats réellement acceptés par chaque cœur, la sélection de firmware et les erreurs de média, avec suppression des dossiers temporaires dans `finally`.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs`, `SettingsFunctions.Builders.cs` et `SettingsHelpDictionary.cs` pour afficher un chemin de ROM système externe pour Saturn, Dreamcast et Mega Drive avec Mega-CD activé, en réutilisant l'aide générique de l'App.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier ces trois champs de firmware et supprimer le dossier temporaire dans `finally`.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger `last-minute.sms` avec le cœur Genesis Plus GX, vérifier une frame vidéo et libérer le cœur ainsi que le dossier de session dans `finally`.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger une ROM Mega Drive `.md` du corpus externe avec Genesis Plus GX, vérifier une frame vidéo et supprimer le dossier de session dans `finally`.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger une ROM SG-1000 `.sg` et une ROM SC-3000 `.sc` du corpus externe avec Genesis Plus GX, vérifier une frame vidéo pour chaque modèle et supprimer chaque session dans `finally`.
  - [ ] Corriger les défauts révélés par les essais sans modifier les contrats d'Amiga, Atari, Amstrad ou App.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Constants/GenesisPlusGXConstants.cs` pour utiliser l'identité `Genesis Plus GX` annoncée par le cœur, puis valider le chargement avec le test Sega réel.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Flycast/Services/ExternalHostCallbacks.Environment.cs`, `GenesisPlusGX/Services/ExternalHostCallbacks.Environment.cs` et `Yabause/Services/ExternalHostCallbacks.Environment.cs` pour traiter `SET_CORE_OPTIONS` et `SET_CORE_OPTIONS_INTL` avec la structure legacy, tout en conservant le second champ de `retro_core_options_v2` comme pointeur des définitions v2.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour vérifier qu'une définition legacy annoncée par Flycast est publiée et reçue avant le chargement du média, avec libération des allocations natives et du dossier temporaire dans `finally`.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` et `GenesisPlusGX/Constants/GenesisPlusGXConstants.cs` pour adapter une ROM SC-3000 `.sc` vers une copie temporaire `.sg` acceptée par le cœur, sans modifier le fichier source.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/Flycast/Services/ExternalCore.cs` après reproduction avec le BIOS Dreamcast connu et le média Dreamcast local, afin que le cœur accepte un support réellement compatible et libère toujours sa session après refus.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour conserver un scénario Flycast uniquement lorsqu'un média Dreamcast réellement accepté est disponible dans le corpus externe, avec nettoyage dans `finally`.
  - [ ] Ajouter les cœurs Sega manquants explicitement demandés.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/ModelConstants.cs` pour ajouter l'identifiant technique vérifié de Sega Pico.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` pour publier Sega Pico avec ses caractéristiques matérielles vérifiées et sans port manette manuel.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/Machine.resx` pour ajouter le nom invariant Sega Pico dans la base commune.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Factories/GenesisPlusGXMachineFactory.cs` pour rattacher Sega Pico au cœur Genesis Plus GX qui l'annonce comme système pris en charge.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger une ROM Pico du corpus externe avec Genesis Plus GX et vérifier une frame vidéo avec nettoyage dans `finally`.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/ModelConstants.cs` pour ajouter les identifiants techniques vérifiés NAOMI et NAOMI 2; les autres familles arcade restent en attente d'un cœur confirmé.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` pour publier NAOMI et NAOMI 2 avec leur SH-4, PowerVR2, AICA, RAM et supports cartouche/GD-ROM vérifiés.
    - [ ] Créer ou modifier `src/GWGUI.Emulation.Sega/Emulators/<cœur-arcade>/` pour l'adaptateur du cœur Sega arcade réellement retenu, avec les fichiers directement dans le dossier du cœur et sans dossier `Core`.
    - [ ] Créer ou modifier `src/GWGUI.Emulation.Sega/Emulators/<cœur-pico>/` pour l'adaptateur Sega Pico réellement retenu, avec les formats et périphériques vérifiés.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Flycast/Factories/FlycastMachineFactory.cs` et `Services/ExternalCore.cs` pour rattacher NAOMI/NAOMI 2 à Flycast et installer les BIOS `dc/naomi.zip` et `dc/naomi2.zip` sous leurs noms attendus; aucun adaptateur parallèle n'est créé.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour exiger le rattachement de NAOMI et NAOMI 2 à Flycast, vérifier leurs chemins BIOS et libérer le dossier temporaire dans `finally`.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger un média du corpus local par nouveau cœur et libérer cœur, processus, ressources graphiques et dossier temporaire dans `finally`.

## 7. MediaEngine — formats et représentations

- [x] Ajouter les constantes de formats réels.
  - [x] Déclarer les extensions.
    - [x] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs`, `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs`, le lecteur/écrivain console et le catalogue de reconnaissance pour `.mdx`, `.smd`, `.bms`, `.sgd` et `.68k`, extensions ROM Mega Drive reconnues par Genesis Plus GX; vérifier leur round-trip et leur icône dans le test média.
    - [x] Vérifier `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs` pour `.fds`, Sega Card/My Card, HuCard/PCE, cartouches et optiques démontrés, puis compléter `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` avec les extensions SG-1000/My Card et PCE; le test round-trip couvre désormais les formats démontrés.
    - [x] Vérifier `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs` et `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` : les identifiants nommés SG-1000/My Card, PC Engine/HuCard, FDS et GDI Dreamcast sont utilisés; aucune constante `RawCartridge` n'est ajoutée.
    - [x] Vérifier `src/GWGUI.MediaEngine/Constants/MediaImageWriterIds.cs` et l'enregistrement du writer `ConsoleCartridge`; un seul writer partagé traite les identifiants nommés de chaque cartouche sans introduire `RawCartridge`.
  - [x] Lire et écrire les cartouches par banques.
    - [x] Vérifier `src/GWGUI.MediaEngine/Images/Formats/Cartridge/Console/ConsoleCartridgeReader.cs` et `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/ConsoleCartridgeFileSystemReader.cs` : les lecteurs publient les banques réelles et leurs tailles.
    - [x] Vérifier `src/GWGUI.MediaEngine/Images/Formats/Cartridge/Console/ConsoleCartridgeWriter.cs` : l'écriture relit exactement les données stockées et échoue sur une banque absente au lieu d'écrire des blocs noirs.
    - [x] Vérifier et supprimer toute occurrence de `src/GWGUI.MediaEngine/Images/Formats/Cartridge/Raw/` ou de `RawCartridge`; aucune occurrence ne subsiste dans `src/` ni `tests/`.
  - [x] Préserver les optiques multipistes.
    - [x] Vérifier `src/GWGUI.MediaEngine/Images/Formats/Optical/Gdi/` pour le descripteur GD-ROM Dreamcast et les lecteurs CUE/BIN, CloneCD, MDS, CHD et ISO pour les supports Mega-CD/Saturn sans inventer d'identifiant machine quand le conteneur ne l'expose pas.
  - [x] Enregistrer chaque composant.
    - [x] Vérifier `src/GWGUI.MediaEngine/Images/Reading/MediaRecognitionComposition.cs` : readers cartouche, FDS et GDI sont enregistrés.
    - [x] Vérifier `src/GWGUI.MediaEngine/Images/Writing/MediaWritingComposition.cs` : writers cartouche, FDS et GDI sont enregistrés.
    - [x] Vérifier `src/GWGUI.MediaEngine/Images/Formats/CapabilityAwareImageFormatCatalog.cs` : les capacités sont dérivées des lecteurs/writers réellement enregistrés.

## 8. MediaFileSystems et MediaAnalysis

- [x] Exposer les volumes démontrés.
  - [x] Ajouter les identifiants nommés.
    - [x] Vérifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` pour Sega Card/My Card, HuCard, FDS et banques console réelles.
  - [x] Lire les entrées sans inventer de système de fichiers.
    - [x] Vérifier `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/` et `FileSystems/Nintendo/FamicomDisk/`; seuls les blocs/bandes réellement décodés sont exposés.
    - [x] Vérifier les lecteurs existants : noms et tailles réels des banques/blocs sont retournés.
- [x] Reconnaître les contenus.
  - [x] Déclarer les extensions.
    - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour les extensions ajoutées dans MediaEngine.
  - [x] Déclarer les signatures et contenus.
    - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` pour distinguer flux, image sectorielle, cartouche et optique.
  - [x] Vérifier les fichiers extraits.
    - [x] Vérifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs`; les artefacts sont créés puis supprimés dans `finally`.
  - [x] Rendre chaque ROM Sega cartouche utilisable dans les quatre parcours de l'application.
    - [x] Conserver `src/GWGUI.MediaEngine/Images/Visualization/` comme représentation par blocs des banques réellement décodées, sans bloc noir fabriqué.
    - [x] Conserver `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/` pour exposer chaque banque réelle avec sa taille et son nom.
    - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/ImageFormatCatalog.cs` pour publier toutes les extensions Mega Drive déjà prises en charge par le lecteur et le writer.
    - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour vérifier, avec suppression dans `finally`, la visualisation, l'exploration et la conversion/export des ROM Sega SG-1000, Master System, Mega Drive, Game Gear et 32X.
  - [ ] Valider séparément les quatre parcours de chaque ROM Sega réellement prise en charge avant de clore la famille Sega.
    - [ ] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour ouvrir chaque ROM Sega dans le visualiseur, lire ses banques dans l'explorateur, convertir vers chaque format de sortie compatible et exporter le résultat vers ce format, avec un artefact temporaire par cas supprimé dans `finally`.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour couvrir les mêmes ROM Sega avec le cœur correspondant, vérifier une image décodée et libérer le cœur, les ressources graphiques et le dossier de session dans `finally`.

## 9. Nintendo, Sony, Microsoft et NEC — même ordre par famille

- [ ] Ne commencer aucune autre famille avant la clôture complète de la section Sega.
  - [ ] Modifier ce document pour cocher la clôture Sega uniquement après validation des cœurs, médias, firmware, options, visualisation, exploration, conversion, export et tests réels dans MediaEngine, MediaFileSystems et MediaAnalysis.
  - [ ] Modifier ce document pour exécuter ensuite une seule famille à la fois dans l'ordre Nintendo, NEC, Sony, Microsoft, en reproduisant la structure et les contrôles validés pour Sega, y compris la visualisation, l'exploration, la conversion et l'export de leurs ROM.

- [ ] Terminer Nintendo avant Sony.
  - [x] Terminer le catalogue avant les cœurs.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Contracts/ModelContracts.cs`, `Common/Constants/ModelConstants.cs` et `Common/Dictionaries/ModelCatalog.cs` pour publier les quinze modèles Nintendo, leurs identifiants backend Nintendo et leurs champs CPU, vidéo et audio.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier l'ordre du catalogue Nintendo et refuser les identifiants backend Sega copiés.
  - [ ] Terminer un adaptateur à la fois.
    - [ ] Créer/modifier `src/GWGUI.Emulation.Nintendo/Emulators/<cœur>/` avec les fichiers directement dans le dossier du cœur.
    - [x] Modifier les fabriques `src/GWGUI.Emulation.Nintendo/Emulators/*/Factories/*MachineFactory.cs` pour rattacher chaque cœur Nintendo existant aux constantes du catalogue, sans identifiant machine en dur.
  - [ ] Tester le rattachement.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier la sélection Mesen sur NES et Famicom Disk System.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour refuser qu'un adaptateur Nintendo publie un modèle absent du catalogue.
    - [ ] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour modèle, cœur et média de chaque adaptateur restant.
- [ ] Terminer Sony avant Microsoft.
  - [ ] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Contracts/ModelContracts.cs` puis `Dictionaries/ModelCatalog.cs` pour Vita, PS3, PS4 et PS5 vérifiables.
    - [ ] Conserver PS1, PS2 et PSP déjà présents sans créer de variantes comme machines indépendantes.
  - [ ] Créer/modifier `src/GWGUI.Emulation.Sony/Emulators/<cœur>/` pour chaque cœur réellement intégré.
    - [ ] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour chaque sélection de cœur et support.
- [ ] Terminer Microsoft.
  - [ ] Modifier `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Contracts/ModelContracts.cs` puis `Dictionaries/ModelCatalog.cs` pour Xbox et Xbox 360 vérifiables.
    - [ ] Créer/modifier `src/GWGUI.Emulation.Microsoft/Emulators/<cœur>/` directement sans `Core`.
  - [ ] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour les machines et leurs médias.
- [ ] Terminer NEC.
  - [ ] Modifier `src/GWGUI.Emulation.Nec/Common/Machines/Common/Contracts/ModelContracts.cs` puis `Dictionaries/ModelCatalog.cs` pour les variantes réellement supportées.
    - [ ] Créer/modifier `src/GWGUI.Emulation.Nec/Emulators/<cœur>/` directement sans `Core`.
  - [ ] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour les machines et leurs médias.

## 10. Cassette, flux et progressions communes

- [ ] Ajouter les flux audio/cassette dans MediaEngine.
  - [ ] Décoder seulement les formats disponibles.
    - [ ] Modifier `src/GWGUI.MediaEngine/Images/Formats/Cassette/` pour CDT, TAP, TZX, TSX, VOC, WAV, FLAC, MP3, AAC, M4A et RAW audio réellement décodables.
    - [ ] Modifier `src/GWGUI.MediaEngine/Images/Formats/Flux/` pour SCP, HxCSTREAM et KryoFlux en conservant leurs révolutions/pistes réelles.
  - [ ] Afficher les blocs.
    - [ ] Modifier les représentations existantes dans `src/GWGUI.MediaEngine/Images/` pour découper la bande et publier le nombre réel de blocs.
    - [ ] Modifier `src/GWGUI.App/` uniquement si le contrat commun de progression ne suffit pas; ne jamais ajouter une seconde barre générique.
- [ ] Corriger conversion et erreurs.
  - [ ] Continuer un lot après erreur.
    - [ ] Modifier le service de conversion existant pour capturer l'erreur par fichier, continuer les fichiers suivants et conserver le détail technique dans la console.
  - [ ] Afficher un message traduit court.
    - [ ] Modifier les ressources de chaque culture avec le message d'erreur court, puis modifier le contrôleur existant seulement pour l'affichage rouge dans la console.
- [ ] Mémoriser la console par onglet.
  - [ ] Persister l'état et la hauteur dans le modèle déjà propriétaire des onglets.
    - [ ] Modifier uniquement les fichiers existants de l'état des onglets après identification de leur propriétaire; ne pas déplacer cette logique dans MediaEngine.

## 11. Traductions, tests et commits

- [ ] Traduire toute nouvelle clé.
  - [ ] Ajouter la clé anglaise dans la base commune du module.
    - [ ] Modifier les catalogues `src/GWGUI.Emulation.<famille>/Resources/00-Base/*.resx` avec les noms réels, `Cartouche`, Sega Card/My Card, périphériques et erreurs.
  - [ ] Synchroniser les cultures avec Argos.
    - [ ] Modifier chaque catalogue `Resources/<culture>/*.resx` correspondant et vérifier qu'aucune clé `[... ]` n'est affichée.
    - [ ] Refaire les ressources Sega à partir de la base canonique.
      - [ ] Remplacer les messages d'erreur répétés des trois cœurs Sega par une fabrique commune et les clés génériques d'erreur de cœur externe.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Exceptions/MachineExceptions.cs` pour contenir aussi la fabrique `ExternalCoreExceptions`, puis modifier les trois familles de services pour l'utiliser sans injecter le nom d'un cœur dans les phrases génériques.
      - [x] Répartir les clés de base dans `Resources/00-Base/*.resx`, notamment `Error.resx` et `Help.resx`, sans fichier monolithique ni dossier redondant.
      - [x] Conserver les valeurs invariantes dans `Resources/00-Base/Machine.resx` uniquement et retirer leur copie des cultures.
      - [x] Aligner la clé `Emulation.Emulator.genesisplusgx.Description` de toutes les cultures sur `GenesisPlusGXConstants.DescriptionResourceKey` afin d'éviter l'affichage de la clé brute.
    - [x] Créer les mêmes catalogues (`Controller.resx`, `Core.resx`, `Error.resx`, `Firmware.resx`, `Help.resx`, `Machine.resx`, `Option.resx`, `Video.resx`) directement sous `Resources/en-US/` et sous les 28 cultures, puis supprimer les fichiers monolithiques.
    - [x] Modifier `src/GWGUI.Emulation.Sega/GWGUI.Emulation.Sega.csproj` pour embarquer les catalogues imbriqués avec leurs cultures.
    - [x] Modifier `src/GWGUI.Emulation/Services/EmulationModuleLocalization.cs` pour charger tous les catalogues d'un module, en conservant le fonctionnement des modules à fichier unique.
    - [x] Retirer de `Resources/en-US/Controller.resx` et des 28 cultures les noms officiels Sega invariants (`Light Phaser`, `Mega Mouse`, `Menacer`, `Virtua Gun`, etc.) et les conserver uniquement dans `Resources/00-Base/Controller.resx`.
    - [x] Traduire chaque catalogue avec `scripts/tools/translate-resx-argos.py --retranslate-all`, puis exécuter l'audit : 28 cultures, 8 catalogues, 4088 entrées localisées; les valeurs invariantes restent uniquement dans `00-Base`.
    - [x] Modifier `scripts/tools/translate-resx-argos.py` pour protéger les noms techniques invariants des cœurs et des machines pendant la traduction et l'audit.
- [ ] Vérifier chaque tranche.
  - [ ] Tester sans média utilisateur permanent.
    - [ ] Modifier le fichier de test ciblé pour créer puis supprimer ses fichiers temporaires dans `finally`.
  - [x] Auditer toutes les suites de tests, pas uniquement le test de la tranche.
    - [x] Modifier `docs/project/console-family-emulation.md` pour consigner les 1067 tests réussis et l'absence de processus `vstest`, `testhost` ou GW GUI résiduel après l'exécution; les fenêtres WPF, dispatchers, cœurs, threads et dossiers temporaires existants sont libérés par leurs `finally` ou leurs fixtures partagées.
  - [ ] Compiler la tranche.
    - [x] Exécuter les tests ciblés après les feuilles de la tranche; noter le résultat dans ce document : 35 tests réussis, 0 échec.
    - [ ] Exécuter `scripts\local-building.cmd --building=debug --modules=0` seulement après une tranche complète et vérifier `build/Debug/GW GUI/gwgui.exe`.
- [ ] Commiter une tranche complète.
  - [ ] Contrôler le contenu du commit.
    - [ ] Modifier ce document pour cocher les feuilles réellement terminées.
    - [ ] Créer un seul commit contenant code, tests, traductions et plan; ne jamais créer un commit documentaire seul.

- [x] Refaire intégralement les traductions Sega avec le vocabulaire matériel correct.
  - [x] Reconstituer les catalogues source et traduisibles avant toute traduction automatique.
    - [x] Modifier les catalogues `src/GWGUI.Emulation.Sega/Resources/00-Base/Machine.resx` et les cultures correspondantes pour ne garder les invariants que dans la base commune.
  - [x] Traduire toutes les cultures depuis `en-US` avec Argos et protéger les noms techniques invariants.
    - [x] Modifier `scripts/tools/translate-resx-argos.py` pour protéger les noms de consoles, extensions, modules et périphériques Sega, conserver les fragments de ponctuation et appliquer les corrections terminologiques par culture.
    - [x] Modifier les 28 jeux de catalogues `Resources/<culture>/*.resx` avec `--retranslate-all`; résultat : 28 cultures, 8 catalogues, 4088 entrées localisées, après retrait des valeurs invariantes.
  - [x] Corriger le vocabulaire matériel français puis auditer les catalogues.
    - [x] Modifier les catalogues `src/GWGUI.Emulation.Sega/Resources/fr-FR/` avec les termes Dreamcast, Mega-CD, 32X, extension, activation et messages d'erreur corrects, puis exécuter l'audit Argos.

## 12. Cœurs Sega à comparer avant toute nouvelle intégration

- [ ] Choisir un cœur par modèle et par extension sur des capacités vérifiées.
  - [ ] Documenter les capacités confirmées de Genesis Plus GX.
    - [ ] Modifier `docs/project/console-family-emulation.md` pour conserver Genesis Plus GX comme candidat principal des SG-1000, Mark III, Master System I/II, Game Gear, Mega Drive/Genesis et Mega-CD, sans lui attribuer le 32X qu'il ne supporte pas.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Dictionaries/EmulatorCatalog.cs` pour conserver ce rattachement uniquement sur les modèles effectivement publiés par le cœur.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Functions/GenesisPlusGXOptionFunctions.cs` pour exposer les options de système, région, BIOS, CD et lock-on réellement renvoyées par le cœur.
  - [ ] Ajouter le candidat PicoDrive pour les extensions que Genesis Plus GX ne couvre pas.
    - [ ] Créer `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Constants/`, `Contracts/`, `Factories/`, `Functions/` et `Services/` avec les mêmes fichiers et noms que GenesisPlusGX, directement sous le dossier de l'émulateur.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Dictionaries/EmulatorCatalog.cs` pour rattacher PicoDrive au 32X et aux autres modèles seulement après vérification de ses options et extensions.
    - [ ] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour verrouiller la matrice modèle/extension/cœur et refuser une combinaison non supportée.
  - [ ] Évaluer les alternatives DLL sans inventer de contrat.
    - [ ] Modifier `docs/project/console-family-emulation.md` pour consigner BizHawk comme candidat C# à DLL de cœur (`IEmulator`, `IVideoProvider`, `ISoundProvider`) à vérifier dans une version récupérable, sans ajouter de DLL fictive au dépôt.
    - [ ] Modifier `docs/project/console-family-emulation.md` pour consigner VirtualGens (`gens.dll`) comme candidat natif à API C, avec son thread, ses entrées et son tampon vidéo à vérifier avant tout adaptateur.
    - [ ] Modifier `docs/project/console-family-emulation.md` pour consigner Exodus comme candidat modulaire cycle-accurate (M68000, Z80, VDP) à vérifier avant tout adaptateur.
    - [ ] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour exiger qu'un candidat sans binaire/API vérifié reste marqué indisponible au lieu d'être présenté comme fonctionnel.
  - [ ] Préparer un cœur GW GUI lorsque les candidats existants ne couvrent pas une machine.
    - [ ] Modifier `docs/project/console-family-emulation.md` pour conserver les sources ouvertes BizHawk, VirtualGens et Exodus comme références d'implémentation future, sans copier leur code ni créer un émulateur incomplet dans cette tranche.
    - [ ] Créer le futur adaptateur sous `src/GWGUI.Emulation.Sega/Emulators/<cœur>/` avec les contrats Common existants uniquement après définition vérifiée du matériel, des médias, des entrées et des états exposés.

## 13. BIOS et firmware Sega par machine et par cœur

- [ ] Décrire les BIOS uniquement avec des fichiers et empreintes vérifiés.
  - [ ] Déclarer les profils Genesis Plus GX vérifiés.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/FirmwareConstants.cs` avec les identifiants BIOS Master System (`bios_E.sms`, `bios_U.sms`, `bios_J.sms`), Game Gear (`bios.gg`), Mega Drive (`bios_MD.bin`) et Mega-CD (`bios_CD_E.bin`, `bios_CD_U.bin`, `bios_CD_J.bin`) et leurs empreintes documentées, sans inventer de BIOS 32X.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` pour associer chaque fichier, empreinte, modèle et cœur compatible, en laissant le 32X indisponible pour Genesis Plus GX.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` pour conserver les révisions régionales et les BIOS custom inconnus comme entrées non sélectionnables tant que leur empreinte, leur modèle et leur nom système attendu ne sont pas vérifiés.
  - [ ] Déclarer les profils PicoDrive vérifiés séparément.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/FirmwareConstants.cs` avec les identifiants Mega-CD PicoDrive réellement distincts lorsque leurs empreintes diffèrent, sans mélanger les profils des deux cœurs.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` pour associer les BIOS au cœur qui les accepte et refuser silencieusement les fichiers d'un autre profil.
  - [ ] Transmettre les BIOS sélectionnés aux adaptateurs.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` pour préparer le répertoire système et les noms attendus par le cœur sans copier de fichier absent.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Services/ExternalCore.cs` pour appliquer la même règle lorsque l'adaptateur PicoDrive existe et expose l'option correspondante.
    - [ ] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier l'association BIOS/modèle/cœur et le refus d'un BIOS non vérifié.

## Historique conservé

- `0ecd3512` : fusion `amstrad` et création de la branche dédiée.
- `62fe7071` : aplatissement des répertoires des cœurs.
- `e2e954d7d` : première tranche Sega (catalogue matériel, slots et
  périphériques officiels).

Le commit `6b78f82e3` a été supprimé. Ses changements sont conservés comme
modifications de travail afin d'être réintégrés dans une vraie tranche, avec
les tests et le comportement complet correspondant.
