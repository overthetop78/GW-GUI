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
    - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/Emulation.resx` et chaque culture pour les libellés traduits avec Argos.
  - [ ] Valider les combinaisons matérielles.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/ConfigurationFunctions.cs` pour refuser un CD Mega-CD désactivé, une cartouche `.32x` sans 32X et une Sega Card verrouillée par SMS II/3-D Glasses.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/StorageFunctions.cs` pour n'ajouter le lecteur CD Mega-CD qu'après sa coche d'activation et exposer les extensions Mega Drive/32X.
  - [ ] Transmettre seulement ce que connaît le cœur.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Functions/GenesisPlusGXOptionFunctions.cs` pour filtrer les options persistées sur le catalogue renvoyé par le cœur, puis appliquer ce filtre après l'initialisation du cœur.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` pour refuser proprement une extension non supportée au lieu de la charger comme une autre machine.
  - [ ] Tester les ports et les addons.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour les deux slots SMS, Mega-CD activé/désactivé, 32X activé/désactivé et le rejet des incompatibilités.

## 5. Sega — firmwares et ROM système

- [ ] Déclarer les profils vérifiés.
  - [ ] Ajouter les identifiants sans texte brut.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/FirmwareConstants.cs` avec les empreintes vérifiées Genesis Plus GX pour Master System, Game Gear, Mega Drive et Mega-CD; laisser les profils non vérifiés (BIOS 32X, BIOS custom et autres révisions) non sélectionnables.
  - [ ] Relier un fichier réellement fourni.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` pour reconnaître les empreintes connues, conserver les noms système attendus par le cœur et laisser les fichiers inconnus sans profil utilisable.
  - [ ] Exposer le profil sélectionné.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Modules/SegaEmulationModule.cs` pour scanner le dossier Firmware, exposer les candidats compatibles et persister le chemin sélectionné dans les options de la configuration.
  - [ ] Appliquer le profil au cœur.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` pour identifier l'empreinte sélectionnée et copier le fichier sous les noms système attendus, y compris plusieurs alias régionaux.
    - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/Yabause/Services/ExternalCore.cs` et `Flycast/Services/ExternalCore.cs` pour transmettre uniquement un firmware accepté par chacun de ces cœurs.
  - [ ] Vérifier les profils.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour vérifier les empreintes connues, leurs modèles et leurs noms système attendus, ainsi que le refus d'une empreinte fictive.

## 6. MediaEngine — formats et représentations

- [ ] Ajouter les constantes de formats réels.
  - [ ] Déclarer les extensions.
    - [ ] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs` pour `.fds`, Sega Card/My Card, HuCard/PCE, cartouches et optiques démontrés.
    - [ ] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs` pour un identifiant nommé par machine/support, jamais `RawCartridge`.
    - [ ] Modifier `src/GWGUI.MediaEngine/Constants/MediaImageWriterIds.cs` pour les writers correspondants.
  - [ ] Lire et écrire les cartouches par banques.
    - [ ] Créer/modifier `src/GWGUI.MediaEngine/Images/Formats/Cartridge/<Machine>/` avec le reader nommé et la représentation par blocs réels.
    - [ ] Créer/modifier le writer du même dossier pour refuser une banque absente au lieu d'écrire des blocs noirs.
    - [ ] Supprimer tout dossier `src/GWGUI.MediaEngine/Images/Formats/Cartridge/Raw/` et toute constante `RawCartridge` si une occurrence subsiste.
  - [ ] Préserver les optiques multipistes.
    - [ ] Créer/modifier `src/GWGUI.MediaEngine/Images/Formats/Optical/<Machine>/` pour pistes Mega-CD, Saturn et GD-ROM Dreamcast avec leur descripteur.
  - [ ] Enregistrer chaque composant.
    - [ ] Modifier `src/GWGUI.MediaEngine/Images/Reading/MediaRecognitionComposition.cs` pour les readers créés.
    - [ ] Modifier `src/GWGUI.MediaEngine/Images/Writing/MediaWritingComposition.cs` pour les writers créés.
    - [ ] Modifier `src/GWGUI.MediaEngine/Images/Formats/CapabilityAwareImageFormatCatalog.cs` pour les capacités exactes.

## 7. MediaFileSystems et MediaAnalysis

- [ ] Exposer les volumes démontrés.
  - [ ] Ajouter les identifiants nommés.
    - [ ] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` pour Sega Card, HuCard, FDS et banques console réelles.
  - [ ] Lire les entrées sans inventer de système de fichiers.
    - [ ] Créer/modifier `src/GWGUI.MediaFileSystems/FileSystems/Console/<Machine>/` seulement lorsque la structure des fichiers est démontrée.
    - [ ] Modifier les lecteurs existants pour retourner les noms et tailles réels des banques/blocs.
- [ ] Reconnaître les contenus.
  - [ ] Déclarer les extensions.
    - [ ] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour les extensions ajoutées dans MediaEngine.
  - [ ] Déclarer les signatures et contenus.
    - [ ] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/CommonMediaContentRecognitionTable.cs` pour distinguer flux, image sectorielle, cartouche et optique.
  - [ ] Vérifier les fichiers extraits.
    - [ ] Modifier `tests/GWGUI.Tests/Media/` avec un test autonome par famille de formats et suppression explicite de ses artefacts temporaires.

## 8. Nintendo, Sony, Microsoft et NEC — même ordre par famille

- [ ] Terminer Nintendo avant Sony.
  - [ ] Terminer le catalogue avant les cœurs.
    - [ ] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Contracts/ModelContracts.cs` puis `Dictionaries/ModelCatalog.cs` pour Game & Watch, NES/Famicom, 3DS, GameCube, Wii, Wii U et Switch vérifiable.
  - [ ] Terminer un adaptateur à la fois.
    - [ ] Créer/modifier `src/GWGUI.Emulation.Nintendo/Emulators/<cœur>/` avec les fichiers directement dans le dossier du cœur.
  - [ ] Tester le rattachement.
    - [ ] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour modèle, cœur et média de chaque adaptateur.
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

## 9. Cassette, flux et progressions communes

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

## 10. Traductions, tests et commits

- [ ] Traduire toute nouvelle clé.
  - [ ] Ajouter la clé anglaise dans la base commune du module.
    - [ ] Modifier `src/GWGUI.Emulation.<famille>/Resources/00-Base/Emulation.resx` avec les noms réels, `Cartouche`, Sega Card/My Card, périphériques et erreurs.
  - [ ] Synchroniser les cultures avec Argos.
    - [ ] Modifier chaque `Resources/<culture>/Emulation.resx` correspondant et vérifier qu'aucune clé `[... ]` n'est affichée.
- [ ] Vérifier chaque tranche.
  - [ ] Tester sans média utilisateur permanent.
    - [ ] Modifier le fichier de test ciblé pour créer puis supprimer ses fichiers temporaires dans `finally`.
  - [x] Auditer toutes les suites de tests, pas uniquement le test de la tranche.
    - [x] Modifier `docs/project/console-family-emulation.md` pour consigner les 1067 tests réussis et l'absence de processus `vstest`, `testhost` ou GW GUI résiduel après l'exécution; les fenêtres WPF, dispatchers, cœurs, threads et dossiers temporaires existants sont libérés par leurs `finally` ou leurs fixtures partagées.
  - [ ] Compiler la tranche.
    - [ ] Exécuter les tests ciblés après les feuilles de la tranche; noter le résultat dans ce document.
    - [ ] Exécuter `scripts\local-building.cmd --building=debug --modules=0` seulement après une tranche complète et vérifier `build/Debug/GW GUI/gwgui.exe`.
- [ ] Commiter une tranche complète.
  - [ ] Contrôler le contenu du commit.
    - [ ] Modifier ce document pour cocher les feuilles réellement terminées.
    - [ ] Créer un seul commit contenant code, tests, traductions et plan; ne jamais créer un commit documentaire seul.

## 11. Cœurs Sega à comparer avant toute nouvelle intégration

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

## 12. BIOS et firmware Sega par machine et par cœur

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
