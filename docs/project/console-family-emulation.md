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
- [x] Relier les visuels déjà existants.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Visuals.cs` pour Mega Drive 3/6 boutons et les périphériques Sega possédant déjà un visuel App.
  - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier la normalisation du visuel 3 boutons, les choix 3/6 boutons, Master System, Saturn, Dreamcast et le Light Phaser catalogué.
    - [x] Modifier `src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs` pour publier les identifiants des visuels Sega déjà présents, puis modifier `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs` pour relier leurs fichiers et zones de commande existants; ne pas inventer de fichier d'image.
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
- [x] Persister les options de la machine Mega Drive.
  - [x] Ajouter les clés techniques dans les contrats existants.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ConfigurationContracts.cs` pour conserver le modèle Mega-CD, son activation désactivée par défaut et la présence du 32X sans ajouter de machine.
  - [x] Déclarer les valeurs constantes.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/SettingsConstants.cs` avec les clés d'activation et de modèle Mega-CD, activation 32X, région et fréquence.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/ModelConstants.cs` avec les identifiants techniques des deux addons, sans texte utilisateur.
  - [x] Afficher les choix dans les blocs existants.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` pour le modèle Mega-CD I/II, sa coche d'activation désactivée par défaut et la coche 32X.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/SettingsHelpDictionary.cs` pour les clés d'aide de ces champs.
    - [x] Modifier les catalogues `src/GWGUI.Emulation.Sega/Resources/<culture>/` pour les libellés traduits avec Argos.
  - [x] Valider les combinaisons matérielles.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/ConfigurationFunctions.cs` pour refuser un CD Mega-CD désactivé, une cartouche `.32x` sans 32X et une Sega Card verrouillée par SMS II/3-D Glasses.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/StorageFunctions.cs` pour n'ajouter le lecteur CD Mega-CD qu'après sa coche d'activation et exposer les extensions Mega Drive/32X.
  - [x] Transmettre seulement ce que connaît le cœur.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Functions/GenesisPlusGXOptionFunctions.cs` pour filtrer les options persistées sur le catalogue renvoyé par le cœur, puis appliquer ce filtre après l'initialisation du cœur.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` pour refuser proprement chaque extension non supportée, sans construire ni accepter de playlist.
  - [x] Tester les ports et les addons.
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

- [x] Valider le fonctionnement réel du périmètre Sega implémenté avant d'ouvrir la tranche Nintendo.
  - [ ] Préparer un corpus de validation séparé du dépôt.
    - [x] Utiliser `F:\Retro\Sega` uniquement comme corpus externe de validation sans modifier ni versionner ses médias; `F:\Retro\Sega\Homebrew\RTsLastMinuteDemo-SMS-1.00\last-minute.sms` a été extrait et l'archive ZIP est conservée.
    - [x] Référencer le corpus Sega externe déjà disponible sous `F:\Retro\Sega\Roms` (jeux, BIOS et ROM système) sans le modifier ni le versionner; les installations de cœurs et de ROM système de l'application restent dans les chemins gérés par le module, comme Amiga, Atari et Amstrad.
    - [x] Conserver les sources Archive.org fournies pour les essais Sega : `https://archive.org/download/CentralArquivista-SegaCD32x`, `https://archive.org/download/pack-roms-sega-cd-cd-32x`, `https://archive.org/download/pack-roms-sega-chihiro-jeux-arcade`, `https://archive.org/download/sega-model-2_202312/Sega%20Model%202%20Emu%201.1a%20and%20Full%20Romset/Model%202%20Romset%20%28Merged%29/` et `https://archive.org/download/sega_model3/Sega%20Model%203/`.
    - [x] Ajouter le romset Atomiswave externe `https://archive.org/download/atomiswave_20220115/`, conserver son archive et extraire le ZIP interne dans `F:\Retro\Sega\Roms\Atomiswave` sans modifier le fichier source.
    - [x] Compléter la validation Sega pour chaque famille couverte par un cœur vérifié, en conservant les archives et les fichiers source dans leurs sous-dossiers; les arcades Model 2, Model 3, Chihiro et autres sans cœur vérifié restent explicitement documentées en section 21.
  - [ ] Tester chaque chaîne cœur/média avec le corpus Sega.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour couvrir les formats réellement acceptés par chaque cœur, l'ordre des médias optiques, les playlists disquette, leur limite et les erreurs de média, avec suppression des dossiers temporaires dans `finally`.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs`, `SettingsFunctions.Builders.cs` et `SettingsHelpDictionary.cs` pour afficher un chemin de ROM système externe pour Saturn, Dreamcast et Mega Drive avec Mega-CD activé, en réutilisant l'aide générique de l'App.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier ces trois champs de firmware et supprimer le dossier temporaire dans `finally`.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger `last-minute.sms` avec le cœur Genesis Plus GX, vérifier une frame vidéo et libérer le cœur ainsi que le dossier de session dans `finally`.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger une ROM Mega Drive `.md` du corpus externe avec Genesis Plus GX, vérifier une frame vidéo et supprimer le dossier de session dans `finally`.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger une ROM SG-1000 `.sg` et une ROM SC-3000 `.sc` du corpus externe avec Genesis Plus GX, vérifier une frame vidéo pour chaque modèle et supprimer chaque session dans `finally`.
  - [x] Corriger les défauts révélés par les essais sans modifier les contrats d'Amiga, Atari, Amstrad ou App.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Constants/GenesisPlusGXConstants.cs` pour utiliser l'identité `Genesis Plus GX` annoncée par le cœur, puis valider le chargement avec le test Sega réel.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Flycast/Services/ExternalHostCallbacks.Environment.cs`, `GenesisPlusGX/Services/ExternalHostCallbacks.Environment.cs` et `Yabause/Services/ExternalHostCallbacks.Environment.cs` pour traiter `SET_CORE_OPTIONS` et `SET_CORE_OPTIONS_INTL` avec la structure legacy, tout en conservant le second champ de `retro_core_options_v2` comme pointeur des définitions v2.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour vérifier qu'une définition legacy annoncée par Flycast est publiée et reçue avant le chargement du média, avec libération des allocations natives et du dossier temporaire dans `finally`.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` et `GenesisPlusGX/Constants/GenesisPlusGXConstants.cs` pour adapter une ROM SC-3000 `.sc` vers une copie temporaire `.sg` acceptée par le cœur, sans modifier le fichier source.
    - [x] Modifier le chemin d'initialisation Flycast et ses callbacks hôtes après reproduction avec le BIOS Dreamcast connu et le média Dreamcast local, afin que le cœur accepte un support réellement compatible et libère toujours sa session après refus.
      - [x] Modifier `src/GWGUI.Emulation/Constants/ExternalCoreApiConstants.cs` et `src/GWGUI.Emulation/Interop/ExternalCoreApi.cs` pour déclarer `GET_PREFERRED_HW_RENDER`, `SET_HW_RENDER`, le contexte OpenGL libretro et ses callbacks natifs sans texte brut hors constantes.
      - [x] Créer `src/GWGUI.Emulation.Sega/Emulators/Flycast/Services/OpenGlHardwareRenderContext.cs` pour créer un contexte OpenGL caché compatible avec le cœur, exposer les adresses WGL/GL et détruire fenêtre, DC et contexte dans `Dispose`.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Flycast/Services/ExternalHostCallbacks.Environment.cs` pour négocier le contexte OpenGL demandé par Flycast et appeler les callbacks de cycle de vie du cœur.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Flycast/Services/ExternalHostCallbacks.AudioVideo.cs` pour lire le framebuffer matériel Flycast dans une image `VideoFrame` sans fabriquer de données et conserver le chemin logiciel existant.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Flycast/Services/ExternalHostCallbacks.cs` pour libérer le contexte OpenGL et ses délégués après l’arrêt du cœur.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour conserver un scénario Flycast uniquement lorsqu'un média Dreamcast réellement accepté est disponible dans le corpus externe, avec nettoyage dans `finally`.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger `ClassiCube-dc.cdi` avec `dc_boot.bin`, exécuter une frame Flycast et supprimer fenêtre, contexte et session temporaires dans `finally`.
  - [ ] Ajouter les cœurs Sega manquants explicitement demandés.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/ModelConstants.cs` pour ajouter l'identifiant technique vérifié de Sega Pico.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` pour publier Sega Pico avec ses caractéristiques matérielles vérifiées et sans port manette manuel.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/Machine.resx` pour ajouter le nom invariant Sega Pico dans la base commune.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Factories/GenesisPlusGXMachineFactory.cs` pour rattacher Sega Pico au cœur Genesis Plus GX qui l'annonce comme système pris en charge.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger une ROM Pico du corpus externe avec Genesis Plus GX et vérifier une frame vidéo avec nettoyage dans `finally`.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/ModelConstants.cs` pour ajouter les identifiants techniques vérifiés NAOMI, NAOMI 2 et Atomiswave; les autres familles arcade restent en attente d'un cœur confirmé.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` pour publier NAOMI, NAOMI 2 et Atomiswave avec leur SH-4, PowerVR2, AICA, RAM et supports cartouche/GD-ROM vérifiés.
    - [ ] Créer ou modifier `src/GWGUI.Emulation.Sega/Emulators/<cœur-arcade>/` pour l'adaptateur du cœur Sega arcade réellement retenu, avec les fichiers directement dans le dossier du cœur et sans dossier `Core`.
    - [x] Créer ou modifier `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/` pour l'adaptateur Sega Pico réellement retenu, avec les formats et périphériques vérifiés.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Flycast/Factories/FlycastMachineFactory.cs` et `Services/ExternalCore.cs` pour rattacher NAOMI, NAOMI 2 et Atomiswave à Flycast et installer leurs BIOS sous les noms attendus; aucun adaptateur parallèle n'est créé.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour exiger le rattachement de NAOMI, NAOMI 2 et Atomiswave à Flycast, vérifier leurs chemins BIOS et libérer le dossier temporaire dans `finally`.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger un média du corpus local par nouveau cœur et libérer cœur, processus, ressources graphiques et dossier temporaire dans `finally`.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger une cartouche `.32x` avec PicoDrive, vérifier son identité et une image vidéo, puis libérer le cœur et le dossier dans `finally`.
    - [x] Ajouter Atomiswave au rattachement Flycast après vérification du BIOS et du support matériel du cœur.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/ModelConstants.cs` pour ajouter l'identifiant technique `Atomiswave` et son backend Flycast.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` pour publier Atomiswave avec son SH-4, PowerVR2, AICA, RAM et support cartouche vérifiés.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/FirmwareConstants.cs` et `Dictionaries/FirmwareCatalog.cs` pour reconnaître uniquement l'empreinte vérifiée `awbios.zip` et son chemin système `dc/awbios.zip`.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/StorageFunctions.cs` et `Functions/SettingsFunctions.cs` pour exposer le support cartouche Atomiswave en `.zip` et son BIOS externe.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/Flycast/Factories/FlycastMachineFactory.cs` pour rattacher Atomiswave à Flycast sans créer de sous-dossier `Core`.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/Machine.resx` pour ajouter le nom invariant Atomiswave dans la base commune.
      - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier le modèle, le rattachement Flycast, le support cartouche et le profil BIOS Atomiswave avec suppression du dossier temporaire dans `finally`.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger le ZIP interne `Dolphin Blue (Atomiswave)\\dolphin.zip` fourni par `GWGUI_SEGA_ATOMISWAVE_MEDIA` avec le cœur et le BIOS fournis par `GWGUI_SEGA_ATOMISWAVE_CORE` et `GWGUI_SEGA_ATOMISWAVE_BIOS`, vérifier une frame puis libérer cœur, processus, ressources graphiques et session dans `finally`.

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
  - [x] Valider séparément les quatre parcours de chaque ROM Sega réellement prise en charge avant de clore le périmètre Sega.
     - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour ouvrir chaque ROM Sega dans le visualiseur, lire ses banques dans l'explorateur, convertir vers chaque format de sortie compatible et exporter le résultat vers ce format, avec un artefact temporaire par cas supprimé dans `finally`; le parcours réel du corpus Sega a réussi.
      - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour sélectionner un exemplaire réel de chaque extension Sega publiée sous `F:\Retro\Sega\Roms` lorsque `GWGUI_SEGA_MEDIA_ROOT` est fourni, vérifier les quatre parcours et supprimer chaque sortie temporaire dans `finally`; le corpus réel passe pour six extensions.
      - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour appeler `MediaEngineComposition.CreateDefault().ConversionService` sur chaque extension Sega cartouche publiée, vérifier toutes les extensions de sortie compatibles du format (dont les variantes Mega Drive), puis vérifier chaque fichier exporté avant suppression du dossier temporaire; la classe média passe avec 16 tests réussis.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour couvrir les mêmes ROM Sega avec le cœur correspondant, vérifier une image décodée et libérer le cœur, les ressources graphiques et le dossier de session dans `finally`.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger une ROM Game Gear `.gg` et une ROM Mark III `.sms` avec Genesis Plus GX, vérifier une image décodée et supprimer chaque session dans `finally`.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger le disque de démarrage Saturn `.ccd` avec Yabause et `saturn_bios.bin`, vérifier une image décodée et supprimer la session dans `finally`.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour vérifier l'identité `Yabause` et l'extension `.ccd` publiée par le cœur lors du chargement Saturn réel.

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
    - [x] Refaire `src/GWGUI.Emulation.Sega/Resources/00-Base/*.resx` à partir de la base canonique en supprimant les clés traduisibles déjà présentes dans `en-US` et en conservant uniquement les valeurs invariantes.
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
    - [x] Exécuter les tests ciblés après les feuilles de la tranche; noter le résultat dans ce document : 17 tests média cartouche, 15 tests de cycle de vie Sega et 1 test du corpus réel réussis, 0 échec.
    - [x] Exécuter `scripts\local-building.cmd --building=debug --modules=0`, puis utiliser `pwsh -NoProfile -File scripts/local-building/build.ps1 -Configuration Debug -AllModules` lorsque le wrapper ne charge pas les modules, et vérifier `build/Debug/GW GUI/gwgui.exe` avec les huit modules présents.
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
    - [x] Modifier `docs/project/console-family-emulation.md` pour conserver Genesis Plus GX comme candidat principal des SG-1000, Mark III, Master System I/II, Game Gear, Mega Drive/Genesis et Mega-CD, sans lui attribuer le 32X qu'il ne supporte pas; le DLL et le média SMS réels confirment cette matrice.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Dictionaries/EmulatorCatalog.cs` pour conserver ce rattachement uniquement sur les modèles effectivement publiés par le cœur.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Functions/GenesisPlusGXOptionFunctions.cs` pour filtrer les options persistées sur le catalogue réellement renvoyé par le cœur, puis vérifier les options système, région, BIOS, CD et lock-on avec un média SMS réel.
    - [ ] Vérifier le catalogue du DLL local avec un média Sega réel.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour vérifier les clés d'options système/BIOS/région/add-on/lock-on et les extensions publiées après initialisation, avec libération du cœur et du dossier temporaire dans `finally`; le DLL local passe le test avec le média SMS réel.
  - [ ] Ajouter le candidat PicoDrive pour les extensions que Genesis Plus GX ne couvre pas.
    - [x] Paramétrer les services libretro communs déjà utilisés par Genesis Plus GX sans créer une couche Libretro parallèle.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/CoreHost.cs` pour recevoir le nom de bibliothèque attendu et le profil de préparation du média, puis transmettre ces valeurs à `ExternalCore`.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` pour vérifier le nom de bibliothèque fourni par l'adaptateur et ne préparer `.sc` vers `.sg` que pour le profil Genesis Plus GX.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ProcessCore.cs` pour recevoir la commande d'hôte de l'adaptateur au lieu d'imposer celle de Genesis Plus GX.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Factories/GenesisPlusGXMachineFactory.cs` pour transmettre explicitement son profil existant et conserver son comportement actuel.
    - [x] Créer l'adaptateur PicoDrive directement sous `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/`.
      - [x] Créer `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Constants/PicoDriveConstants.cs` avec l'identifiant `picodrive`, le nom de bibliothèque `PicoDrive`, la commande d'hôte dédiée et l'URL officielle Windows x64 du DLL, sans texte utilisateur en constante technique.
      - [x] Créer `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Factories/PicoDriveMachineFactory.cs` avec les mêmes contrats `IEmulatorAdapter`, installation, sélection de média, création de machine et gestion de commande que Genesis Plus GX, en ne publiant d'abord que le modèle Mega Drive vérifié.
      - [x] Créer `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Services/PicoDriveCoreProvider.cs` pour trouver uniquement `picodrive_libretro.dll` dans le dossier du cœur et ne pas copier de DLL dans les sources.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/CoreReleaseService.cs` pour accepter l'URL et le nom de DLL du profil demandé, tout en conservant les valeurs Genesis Plus GX par défaut.
      - [x] Modifier les catalogues `src/GWGUI.Emulation.Sega/Resources/00-Base/Core.resx`, `Resources/en-US/Core.resx` et les 28 cultures avec le script Argos pour ajouter la description PicoDrive sans recopier les noms invariants.
    - [x] Vérifier PicoDrive avec le DLL récupérable et un média 32X réel avant de publier le rattachement.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger un `.32x` du corpus `F:\Retro\Sega\Roms`, vérifier l'extension et les options réellement annoncées par PicoDrive, vérifier une frame et libérer le cœur ainsi que le dossier temporaire dans `finally`.
      - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour préparer un BIOS Mega-CD vérifié avec PicoDrive et contrôler son nom et son empreinte dans le répertoire système isolé, puis supprimer la session dans `finally`.
      - [x] Vérifier dans `src/GWGUI.Emulation.Sega/Common/Dictionaries/EmulatorCatalog.cs` que l'auto-découverte rattache PicoDrive au seul modèle publié et conserve Mega-CD/32X comme extensions de Mega Drive.
      - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour verrouiller la matrice modèle/extension/cœur et refuser une combinaison non annoncée par le DLL.
    - [x] Valider la tranche PicoDrive dans les quatre parcours média avant de la déclarer terminée.
      - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour ouvrir un exemplaire réel de chaque extension Sega publiée dans le visualiseur, lire leurs banques dans l'explorateur, convertir chaque sortie compatible et exporter le résultat, avec suppression de chaque artefact dans `finally`.
      - [x] Exécuter les tests ciblés Sega, commiter la tranche fonctionnelle avec son test et son document mis à jour, puis lancer `pwsh -NoProfile -File scripts/local-building/build.ps1 -Configuration Debug -AllModules` et vérifier `build\\Debug\\GW GUI\\gwgui.exe` avec les huit modules présents.
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
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/FirmwareConstants.cs` avec les identifiants BIOS Master System (`bios_E.sms`, `bios_U.sms`, `bios_J.sms`), Game Gear (`bios.gg`), Mega Drive (`bios_MD.bin`) et Mega-CD (`bios_CD_E.bin`, `bios_CD_U.bin`, `bios_CD_J.bin`) et leurs empreintes documentées, y compris les variantes vérifiées du corpus local, sans inventer de BIOS 32X.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` pour associer chaque empreinte connue au fichier système attendu, au modèle et au cœur compatible, en laissant le 32X indisponible pour Genesis Plus GX; modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour verrouiller les profils standards et alternatifs.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` pour conserver les révisions régionales et les BIOS custom inconnus comme entrées non sélectionnables tant que leur empreinte, leur modèle et leur nom système attendu ne sont pas vérifiés.
  - [ ] Déclarer les profils PicoDrive vérifiés séparément.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour confirmer que PicoDrive utilise les mêmes noms de BIOS Mega-CD vérifiés que le catalogue Sega commun; aucune empreinte PicoDrive distincte n'est déclarée sans preuve dans le cœur ou le corpus local.
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

## 14. Exigence ajoutée — parcours média complet et ordre des familles

- [ ] Clore Sega avec tous les parcours de ses médias avant toute autre famille.
  - [ ] Rendre chaque ROM et support Sega réellement pris en charge dans le visualiseur, l'explorateur, la conversion et l'export.
    - [x] Modifier `src/GWGUI.MediaEngine/Images/Visualization/Providers/BlockMediaVisualizationProvider.cs` pour utiliser la longueur réellement stockée publiée par `src/GWGUI.MediaEngine/Images/Formats/Cartridge/Console/ConsoleCartridgeReader.cs` et ne jamais visualiser le remplissage d'une banque finale comme un bloc de données; modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour verrouiller cette absence de bloc noir; le test ciblé réussit.
    - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/ConsoleCartridgeFileSystemReader.cs` pour exposer chaque banque réelle avec son nom et sa taille dans l'explorateur et refuser les métadonnées de banque invalides; le test média couvre ce refus.
    - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/ImageFormatCatalog.cs` pour enregistrer chaque extension Sega déjà décodable par les lecteurs et les writers existants avec les constantes communes; le test média verrouille SG-1000, Master System, Mega Drive, Game Gear et 32X.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/StorageConstants.cs` et `Functions/StorageFunctions.cs` pour exposer dans le lecteur Mega Drive toutes les extensions de cartouche publiées par MediaEngine, puis modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour verrouiller cette matrice.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/StorageConstants.cs` et `Functions/StorageFunctions.cs` pour exposer `.ccd` au lecteur Saturn sans ajouter de playlist ni de format optique que le modèle sélectionné ne peut pas charger, puis modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour verrouiller la matrice optique Sega.
    - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour ouvrir les ROM Sega du corpus externe, vérifier la visualisation et l'exploration, convertir chaque sortie compatible, exporter le résultat, puis supprimer chaque artefact temporaire dans `finally`.
      - [x] Ajouter `EveryRealSegaCartridgeUsesVisualizationAndExploration` pour parcourir les 3 071 ROM Sega disponibles, convertir/exporter un exemplaire par extension et supprimer les sorties dans `finally`; le test réel réussit en 1 min 19 s.
  - [x] Valider les cœurs Sega et leurs médias après les quatre parcours.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour exécuter les médias Sega disponibles avec leur cœur réellement compatible, vérifier une image décodée, puis libérer cœur, processus, ressources graphiques et dossier de session dans `finally`; 74 tests Sega ont réussi sans processus résiduel.
  - [x] Fermer le périmètre Sega implémenté quand les feuilles Sega, MediaEngine, MediaFileSystems, MediaAnalysis, firmware, options, tests et traductions vérifiées sont cochées; les familles arcade sans cœur confirmé restent dans la liste future de la section 21.
    - [x] Modifier `docs/project/console-family-emulation.md` pour consigner les résultats réels des tests, la limitation des cœurs arcade absents et le chemin du build Debug incluant tous les modules.

- [ ] Reproduire exactement le périmètre média validé pour chaque famille suivante, une seule famille à la fois.
  - [ ] Terminer Nintendo après Sega.
    - [ ] Modifier les lecteurs, writers, représentations et catalogues existants dans `src/GWGUI.MediaEngine/`, `src/GWGUI.MediaFileSystems/` et `src/GWGUI.MediaAnalysis/` pour les formats Nintendo réellement décodés, puis modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour vérifier visualisation, exploration, conversion et export avec nettoyage dans `finally`.
    - [ ] Modifier `src/GWGUI.Emulation.Nintendo/` et `tests/GWGUI.Tests/Emulation/Nintendo/` pour rattacher et tester un cœur Nintendo à la fois, puis libérer chaque cœur, processus et ressource dans `finally`.
  - [ ] Terminer NEC après Nintendo.
    - [ ] Modifier les lecteurs, writers, représentations et catalogues existants dans `src/GWGUI.MediaEngine/`, `src/GWGUI.MediaFileSystems/` et `src/GWGUI.MediaAnalysis/` pour les formats NEC réellement décodés, puis modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour vérifier les quatre parcours avec nettoyage dans `finally`.
    - [ ] Modifier `src/GWGUI.Emulation.Nec/` et `tests/GWGUI.Tests/Emulation/Nec/` pour rattacher et tester un cœur NEC à la fois, puis libérer chaque cœur, processus et ressource dans `finally`.
  - [ ] Terminer Sony après NEC.
    - [ ] Modifier les lecteurs, writers, représentations et catalogues existants dans `src/GWGUI.MediaEngine/`, `src/GWGUI.MediaFileSystems/` et `src/GWGUI.MediaAnalysis/` pour les formats Sony réellement décodés, puis modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour vérifier les quatre parcours avec nettoyage dans `finally`.
    - [ ] Modifier `src/GWGUI.Emulation.Sony/` et `tests/GWGUI.Tests/Emulation/Sony/` pour rattacher et tester un cœur Sony à la fois, puis libérer chaque cœur, processus et ressource dans `finally`.
  - [ ] Terminer Microsoft après Sony.
    - [ ] Modifier les lecteurs, writers, représentations et catalogues existants dans `src/GWGUI.MediaEngine/`, `src/GWGUI.MediaFileSystems/` et `src/GWGUI.MediaAnalysis/` pour les formats Microsoft réellement décodés, puis modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour vérifier les quatre parcours avec nettoyage dans `finally`.
    - [ ] Modifier `src/GWGUI.Emulation.Microsoft/` et `tests/GWGUI.Tests/Emulation/Microsoft/` pour rattacher et tester un cœur Microsoft à la fois, puis libérer chaque cœur, processus et ressource dans `finally`.

- [ ] Conserver la règle d'exécution par tranche complète.
  - [ ] Construire, tester et commiter chaque famille seulement après ses fonctionnalités et traductions terminées.
    - [ ] Modifier `docs/project/console-family-emulation.md` dans le même commit que la fonctionnalité ou le correctif correspondant, puis lancer le build Debug avec tous les modules et conserver `build/Debug/GW GUI/gwgui.exe` pour le test utilisateur.

## 15. Tranche Sega — BIOS inconnus non sélectionnables

- [x] Conserver les BIOS custom et révisions non identifiés sans les présenter comme utilisables.
  - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour créer un fichier BIOS Sega inconnu dans le répertoire firmware temporaire, vérifier son affichage par nom, son état incompatible et l'absence de destination sélectionnable, puis supprimer le répertoire dans `finally`.

## 16. Tranche Sega — systèmes 8 bits PicoDrive vérifiés

- [x] Publier PicoDrive seulement pour les modèles 8 bits que son DLL local charge réellement.
  - [x] Vérifier le chargement PicoDrive avec un média SMS réel et une image vidéo non vide.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger `SMS BIOS V3.4 + Hang On.sms` avec `picodrive_libretro.dll`, contrôler le modèle, les extensions et une frame, puis libérer le cœur et le dossier temporaire dans `finally`.
  - [x] Rattacher les modèles confirmés au catalogue PicoDrive.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Factories/PicoDriveMachineFactory.cs` pour publier uniquement les modèles confirmés par le test, sans créer de nouvelle machine ni de dossier `Core`.
  - [x] Verrouiller la matrice d'adaptateurs.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier que PicoDrive est proposé sur chaque modèle confirmé et reste absent des modèles non vérifiés.
  - [x] Compiler et commiter la tranche complète.
    - [x] Modifier ce document pour cocher uniquement les feuilles réellement réussies, créer un commit code/tests/plan, puis exécuter le build Debug avec tous les modules et conserver `build/Debug/GW GUI/gwgui.exe`.

## 17. Tranche Sega — variantes BIOS Saturn du corpus local

- [x] Rendre sélectionnables uniquement les variantes Saturn validées par Yabause.
  - [x] Ajouter les empreintes des variantes réellement présentes dans `F:\Retro\Sega\Roms`.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/FirmwareConstants.cs` et `Dictionaries/FirmwareCatalog.cs` pour déclarer les profils `3240872c70984b6cbfda1586cab68dbe` et `85ec9ca47d8f6807718151cbcca8b964`, avec le nom système `saturn_bios.bin` attendu par Yabause.
  - [x] Vérifier leur chargement par le cœur Saturn.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour contrôler le staging et une frame Yabause avec chaque profil, puis libérer le cœur et le dossier temporaire dans `finally`.
  - [x] Verrouiller l’identité des profils dans le test d’architecture.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour exiger les deux empreintes et refuser toute autre empreinte Saturn.
  - [x] Compiler et commiter la tranche complète.
    - [x] Modifier ce document pour cocher uniquement les feuilles réellement réussies, créer un commit code/tests/plan, puis exécuter le build Debug avec tous les modules et conserver `build/Debug/GW GUI/gwgui.exe`.

## 18. Tranche Sega — modèles 8 bits PicoDrive supplémentaires

- [x] Publier PicoDrive pour chaque modèle 8 bits dont le chargement réel est confirmé.
  - [x] Vérifier SG-1000, SC-3000 et Game Gear avec des médias du corpus local.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour charger une ROM `.sg`, une ROM `.sc` et une ROM Game Gear avec PicoDrive, contrôler le modèle, l'extension et une frame, puis libérer chaque cœur et dossier temporaire dans `finally`.
  - [x] Rattacher uniquement les modèles qui passent ces chargements.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Factories/PicoDriveMachineFactory.cs` pour compléter `Definition.MachineIds` avec les modèles effectivement validés.
  - [x] Verrouiller la matrice publiée.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour exiger les modèles PicoDrive validés et refuser les modèles Sega non couverts par l’adaptateur.
  - [x] Compiler et commiter la tranche complète.
    - [x] Modifier ce document pour cocher uniquement les feuilles réellement réussies, créer un commit code/tests/plan, puis exécuter le build Debug avec tous les modules et conserver `build/Debug/GW GUI/gwgui.exe`.

- [x] Corriger la configuration matérielle Master System sans exposer d'options impossibles.
  - [ ] Déplacer la configuration des lunettes 3D dans le lecteur Sega Card.
    - [x] Modifier `src/GWGUI.Emulation/Contracts/EmulationStorageDeviceSettings.cs` pour porter les réglages propres à un lecteur de cartouche, sans réutiliser le champ général de la machine.
    - [x] Créer `src/GWGUI.App/Views/Dialogs/Emulation/Storage/CartridgeSlotConfigurationDialog.cs` avec une case à cocher lunettes 3D uniquement pour le lecteur Sega Card, puis fermer et libérer la fenêtre dans `finally`.
    - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs` pour ouvrir cette boîte de dialogue lorsque `ConfigurationKind` vaut `CartridgeSlot`, enregistrer le résultat dans le réglage du seul slot Sega Card et le restaurer après reconstruction de la liste.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/StorageFunctions.cs` pour appliquer ce réglage au verrouillage du Sega Card et conserver l'ancien chemin uniquement pour migrer les configurations existantes.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` pour retirer la case lunettes 3D de l'onglet Général et conserver le champ uniquement dans la description du slot Sega Card.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier que le slot Sega Card est configurable, que les lunettes 3D y sont stockées et que le slot est verrouillé seulement après activation.
  - [x] Supprimer les choix de RAM non supportés par le modèle.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` pour rendre la RAM informative et non modifiable lorsque le modèle n'expose pas une extension validée par le cœur, notamment Master System I/II.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier que Master System n'affiche pas 64/128/192/576 KiB comme choix et conserve ses 8 KiB réels.
  - [x] Remplir les onglets avec les options et composants réellement annoncés par le modèle et le cœur sélectionné.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/SettingsDescriptionTextConstants.cs` pour publier les CPU, vidéo, audio, ROM système et RAM réels sans inventer de valeur configurable.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/Video.resx`, `src/GWGUI.Emulation.Sega/Resources/00-Base/Machine.resx` et les catalogues de culture correspondants pour fournir uniquement les libellés traduisibles des puces vidéo et audio Sega.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/SettingsDescriptionTextConstants.cs` pour afficher ces puces comme informations et retirer les contrôles vidéo et lecteur de disquette qui ne sont pas déclarés par les machines Sega.
      - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/SettingsDescriptionChoicesConstants.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.Choices.cs` pour retirer uniquement leurs choix vidéo factices et conserver les choix communs encore utilisés.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour verrouiller les onglets Master System et Mega Drive contre les champs impossibles ou non traduits.
    - [x] Modifier `src/GWGUI.App/Contracts/Machine/MachineControllerOptions.cs`, `src/GWGUI.App/Contracts/Machine/MachineCommandActions.cs`, `src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs` et `src/GWGUI.App/Views/Dialogs/Emulation/CoreOptionsDialog.cs` pour représenter et configurer les options déclarées par le cœur sélectionné sans créer une seconde architecture de réglages.
    - [x] Vérifier `src/GWGUI.Emulation.Sega/Modules/SegaEmulationModule.cs` et les adaptateurs Sega afin de publier uniquement les options réellement retournées par le cœur choisi et d'appliquer les valeurs persistées avant le démarrage.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` et `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour vérifier que les options Genesis Plus GX/PicoDrive sont publiées et que les chemins réels SMS démarrent.

  - [x] Exposer les BIOS Sega connus du catalogue dans le parcours de sélection de l'onglet ROM.
    - [x] Conserver dans `src/GWGUI.Emulation.Sega/Modules/SegaEmulationModule.cs` et les contrats firmware communs le chemin sélectionné, son modèle et son cœur sans recopier de ROM dans le dépôt; l'onglet ROM utilise `IEmulationFirmwareManager`.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour sélectionner un BIOS Master System réel de `F:\Retro\Sega`, vérifier sa destination et refuser un BIOS inconnu.
  - [x] Reproduire le démarrage complet avec les cœurs installés et les médias réels.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour créer la machine via `SegaEmulationModule.CreateRuntimeAsync`, démarrer un Master System avec Genesis Plus GX puis PicoDrive, vérifier une frame et libérer la session dans `finally`.
    - [x] Vérifier `src/GWGUI.Emulation.Sega/Common/Services/Machine.Commands.cs` et le relais d'erreurs du cœur; le démarrage réel ne reproduit aucune panne nécessitant une modification.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaMachineLifecycleTests.cs` pour couvrir le chemin sans BIOS externe, le BIOS sélectionné et le média SMS réel sans laisser de `gwgui`, `testhost` ou processus de cœur.

  - [x] Vérifier les traductions des nouveaux champs et les valider.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/*.resx`, `Resources/en-US/*.resx` et les cultures correspondantes pour répartir les libellés dans les catalogues existants (`Machine`, `Memory`, `Firmware`, `Help`, `Error`) sans dupliquer les invariants.
      - [x] Exécuter `scripts/tools/translate-resx-argos.py` sur les clés ajoutées ou corrigées, puis corriger les termes matériels et supprimer les aides devenues sans objet.
      - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour auditer les clés de l'onglet ROM, du lecteur Sega Card et des aides Master System dans toutes les cultures.

- [x] Valider la tranche Sega implémentée.
  - [x] Exécuter les tests Sega ciblés avec les cœurs et médias de `F:\Retro\Sega`, puis le test complet et vérifier la libération des ressources : 74 tests Sega ciblés et 1 110 tests complets réussis, aucun processus `gwgui`, `testhost`, `vstest` ou cœur résiduel.
  - [x] Compiler tous les modules : `pwsh -NoProfile -File scripts/local-building/build.ps1 -Configuration Debug -AllModules` a produit `build/Debug/GW GUI/gwgui.exe` avec les huit modules.

## 20. Tranche Sega — retrait des aides de réglages inexistants

- [x] Retirer les aides qui décrivent des contrôles absents du catalogue Sega.
  - [x] Supprimer dans chaque `src/GWGUI.Emulation.Sega/Resources/<culture>/Help.resx` les clés `Emulation.Sega.Help.Video.Resolution.*`, `Emulation.Sega.Help.Video.Monitor.*`, `Emulation.Sega.Help.Video.Intensity.*`, `Emulation.Sega.Help.Video.Crop.*` et `Emulation.Sega.Help.Audio.FloppySound.*`.
  - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier que ces clés supprimées ne sont plus présentes dans les catalogues Sega et que chaque aide encore référencée par `SettingsHelpDictionary` existe dans toutes les cultures.

## 21. Tranche Sega — cœurs non disponibles et alternatives vérifiées

- [x] Conserver les limites explicites au lieu de publier un adaptateur incomplet.
  - [x] Modifier `docs/project/console-family-emulation.md` pour consigner que le corpus local fournit Genesis Plus GX, PicoDrive, Yabause et Flycast, que Flycast couvre Dreamcast/NAOMI/NAOMI 2/Atomiswave, et qu'aucun binaire/API vérifié de Model 2, Model 3, Chihiro ou des autres arcades Sega n'est disponible dans `F:\Retro\Sega\Cores`.
  - [x] Modifier `docs/project/console-family-emulation.md` pour conserver BizHawk, VirtualGens et Exodus comme candidats à vérifier avant tout futur adaptateur; aucune DLL fictive ni machine arcade non fonctionnelle n'est publiée.

## 22. Tranche Nintendo — stockage et réglages matériels réels

- [x] Corriger les supports de stockage Nintendo exposés par la configuration.
  - [x] Déclarer les extensions par modèle sans réutiliser les extensions Amstrad ou Sega.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Constants/StorageConstants.cs` pour définir les groupes d'extensions NES/Famicom, Famicom Disk, SNES, Virtual Boy, N64, Game Boy, Game Boy Color, Game Boy Advance, DS, 3DS, GameCube/Wii et Wii U à partir des constantes MediaEngine.
  - [x] Publier les lecteurs correspondant au matériel réel.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Functions/StorageFunctions.cs` pour ne publier un lecteur disquette que pour Famicom Disk, un lecteur cartouche avec les extensions du modèle, et un lecteur optique avec les extensions GameCube/Wii ou Wii U réellement enregistrées.
  - [x] Verrouiller la matrice de stockage.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier les extensions de chaque modèle Nintendo et supprimer le dossier temporaire dans `finally`.

- [x] Afficher uniquement les réglages matériels et audio déclarés.
  - [x] Remplacer les choix vidéo, RAM et son de disquette fictifs par des informations du modèle.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Constants/SettingsConstants.cs`, `SettingsDescriptionTextConstants.cs`, `SettingsDescriptionChoicesConstants.cs` et `SettingsHelpDictionary.cs` pour retirer les identifiants de moniteur, intensité, recadrage et son de disquette absents des modèles Nintendo.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Functions/SettingsFunctions.cs` pour afficher CPU, vidéo, audio et RAM comme informations invariantes, conserver seulement les réglages audio communs et laisser les options du cœur dans le dialogue générique.
  - [x] Vérifier les champs exposés dans chaque onglet.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour refuser les champs vidéo/RAM/son fictifs et vérifier les puces réelles du catalogue Nintendo.

## 23. Tranche Nintendo — limites de cœurs vérifiées

- [x] Refuser explicitement les modèles Nintendo sans adaptateur installé.
  - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour exiger `NotSupportedException` lors de la création Wii U ou Switch, au lieu de publier une configuration inexécutable.
  - [x] Conserver ces modèles dans le catalogue matériel, sans inventer de cœur Cemu ou Ryujinx tant qu'un adaptateur vérifié n'est pas intégré.

## 24. Tranche Nintendo — parcours média cartouche

- [x] Valider les cartouches Nintendo dans les parcours média communs.
  - [x] Vérifier chaque extension publiée par le lecteur console.
    - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour ouvrir NES, SNES, N64, Game Boy, Game Boy Color, Game Boy Advance, DS, Game & Watch, 3DS et Virtual Boy, puis vérifier visualisation, exploration et conversion/export.
