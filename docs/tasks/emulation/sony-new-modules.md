# Sony et nouveaux modules demandés

- [ ] Réaliser le plan Sony puis les quinze modules
  - [ ] Compléter Sony
    - [x] Préparer les actions Sony
      - [x] Créer `docs/tasks/emulation/sony-new-modules.md` : inventorier les correspondances de toutes les sections de `liste-emulateur-libretro.md`, les fichiers concernés et les validations, sans limiter le périmètre à un sous-ensemble.
    - [ ] Ajouter PocketStation
      - [x] Créer `src/GWGUI.Emulation.Sony/Common/Machines/PocketStation/Constants/ModelConstants.cs` et `Dictionaries/ModelCatalog.cs` : définir la machine, son ARM7TDMI, sa RAM 2 KiB, sa ROM 16 KiB et son contrôleur intégré ; modifier `Common/Machines/Common/Enums/RamCapacity.cs`, `Constants/ModelConstants.cs` et `Dictionaries/ModelCatalog.cs` pour l'exposer.
      - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Machines/Common/Contracts/ModelContracts.cs`, `Enums/MediaEnums.cs`, `Functions/MediaFunctions.cs`, `Functions/StorageFunctions.cs`, `Functions/ConfigurationFunctions.cs` et créer `Common/Machines/PocketStation/Constants/MediaConstants.cs` : utiliser le support MemoryCard existant du SDK pour les images flash PocketStation, sans les assimiler à des cartouches ou BIOS.
      - [x] Modifier `src/GWGUI.Emulation.Sony/Resources/00-Base/Machine.resx` : ajouter le nom invariant PocketStation.
      - [x] Créer `tmp/pocketstation-source.png` : télécharger la photo déjà transparente Evan-Amos depuis Wikimedia Commons.
      - [x] Créer `src/GWGUI.Emulation.Sony/Assets/Machines/PocketStation.png` : photo réelle, fond transparent, 512 pixels maximum ; inscrire sa source et les actions temporaires avant réalisation.
      - [x] Supprimer `tmp/pocketstation-source.png` après redimensionnement et contrôle du canal alpha.
    - [ ] Mutualiser le pilote Libretro Sony
      - [x] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler les déplacements et raccordements du pilote partagé avant réalisation.
      - [x] Déplacer depuis `src/GWGUI.Emulation.Sony/Emulators/SwanStation` vers `Emulators/Common/Interop` les fichiers `Services/ProcessCore.cs`, `ExternalCore.cs`, `CoreHost.cs`, `ExternalDiskControl.cs`, `ExternalHostCallbacks.cs`, `ExternalHostCallbacks.AudioVideo.cs`, `ExternalHostCallbacks.Environment.cs`, `ExternalHostCallbacks.Input.cs`, `Functions/CoreHostProtocol.cs`, `Enums/HostEnums.cs`, `Contracts/ControllerContracts.cs`, `Contracts/CoreRelease.cs`, `Constants/ProcessCoreConstants.cs`, `CoreHostConstants.cs`, `ExternalCoreConstants.cs`, `ExternalHostCallbacksConstants.cs` et `Exceptions/SwanStationExceptions.cs` renommé `CoreExceptions.cs` : adapter leurs espaces de noms et retirer les imports propres à SwanStation.
      - [x] Créer `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Contracts/CoreDefinition.cs`, `CoreReleaseSettings.cs`, `Dictionaries/CoreCatalog.cs`, `Factories/CoreMachineFactory.cs`, `Services/CoreReleaseService.cs`, `Functions/CoreManifestFunctions.cs`, `Constants/CoreReleaseConstants.cs` : paramétrer le téléchargement et la création de machines suivant les définitions des cœurs ; reprendre les services validés Nintendo sans référence entre modules.
      - [x] Renommer `src/GWGUI.Emulation.Sony/Emulators/SwanStation/Constants/SwanStationConstants.cs` en `CoreConstants.cs`, modifier `Factories/SwanStationMachineFactory.cs`, supprimer `Services/CoreProvider.cs`, `CoreReleaseService.cs`, `ExternalCoreInstaller.cs`, `Constants/CoreReleaseConstants.cs` et `Contracts/EmulatorContracts.cs` : conserver sa définition et ses ROM propres et déléguer au pilote commun.
      - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Services/ExternalCore.cs`, `CoreHost.cs`, `ProcessCore.cs`, `Functions/CoreHostProtocol.cs` et `Constants/ProcessCoreConstants.cs` : utiliser l'identité, le nom de DLL, les ROM et la commande du pilote commun ; garder la libération finale et l'isolation du processus.
      - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/Common/Interop/Constants/CoreReleaseConstants.cs` et `Services/CoreReleaseService.cs` : nommer les tailles de transfert et les constantes du format PE, employer les formats de publication explicites.
      - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : inscrire la compilation et les contrôles du déplacement avant intégration des nouveaux cœurs.
    - [ ] Ajouter et raccorder les sept émulateurs manquants
      - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler les fichiers pour Beetle PSX, Beetle PSX HW, PCSX ReARMed, PCEE2, Play!, pokketstation et RPCS3, options officielles, ROM utilisateur, supports, rendu et libération des ressources avant réalisation.
    - [ ] Valider Sony puis committer
      - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : inscrire les tests utiles, le contrôle de couverture, les validations de rendu et arrêt, le build Debug avec modules et le commit uniquement quand Sony est terminé.
  - [ ] Créer Acorn
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler les fichiers du module Acorn, b2, BBC Micro A/B et Master 128, ressources, images, ROM, supports, périphériques, options, lifecycle, reconnaissance par l'application, validations et commit.
  - [ ] Créer Apple
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler les fichiers du module Apple, AppleWin, Mini vMac, modèles documentés, intégration complète, validations et commit.
  - [ ] Créer Bandai
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler Bandai, Beetle WonderSwan, Playdia et Tamalibretro, toutes les machines associées, intégration complète, validations et commit.
  - [ ] Créer Coleco
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler ColecoVision, Gearcoleco, JollyCV et blueMSX présent dans la section ordinateurs, intégration complète, validations et commit.
  - [ ] Créer Epoch
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler Cassette Vision et Super Cassette Vision, PD777 et EmuSCV, intégration complète, validations et commit.
  - [ ] Créer Fairchild
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler Channel F et System II, FreeChaF, intégration complète, validations et commit.
  - [ ] Créer Philips
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler CD-i, P2000T, Odyssey²/Videopac, CDI2015, SAME CDi, M2000, O2EM, intégration complète, validations et commit.
  - [ ] Créer Mattel
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler Intellivision/FreeIntv, intégration complète, validations et commit.
  - [ ] Créer Sharp
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler X68000/PX68k et X1, intégration complète, validations et commit.
  - [ ] Créer Sinclair
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler les modèles ZX81 et ZX Spectrum, EightyOne et Fuse, intégration complète, validations et commit.
  - [ ] Créer SNK
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler Neo Geo, Neo Geo CD, Pocket/Pocket Color, les émulateurs des sections SNK et arcade dont FB Alpha 2012 Neo Geo, intégration complète, validations et commit.
  - [ ] Créer Thomson
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler les huit modèles Thomson de Theodore dans la section ordinateurs, intégration complète, validations et commit.
  - [ ] Créer Watara
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler Supervision/Potator dans les autres consoles, intégration complète, validations et commit.
  - [ ] Créer Wenquxing
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler NC1020/PC1000/CC800/NC2000/NC3000 et WqxEmu dans la section ordinateurs, intégration complète, validations et commit.
  - [ ] Créer Panasonic
    - [ ] Modifier `docs/tasks/emulation/sony-new-modules.md` : détailler 3DO/Opera dans les autres consoles, intégration complète, validations et commit.

## État initial vérifié

Sony : trois adaptateurs présents (SwanStation, PCSX2, PPSSPP). PS1, PS2, PSP, Vita, PS3, PS4 et PS5 existent ; PocketStation manque. Sept cœurs documentés manquent : mednafen_psx, mednafen_psx_hw, pcsx_rearmed, pcee2, play, pokketstation, rpcs3. Aucun autre système Sony supplémentaire dans les autres titres ; remotejoy est un utilitaire PSP et ne constitue pas une machine.

Les quinze familles n'ont actuellement aucun projet Emulation correspondant. Le Common racine contient les contrats génériques ; les détails du protocole Libretro restent sous Emulators/Common/Interop. Chaque module doit être entièrement raccordé, validé et committé avant le suivant. Aucun groupe n'est considéré terminé par la seule création d'un catalogue.

PocketStation : source technique primaire https://psx-spx.consoledev.net/ps1/sio/pocketstation/ ; ARM7TDMI, SRAM 2 KiB, ROM BIOS 16 KiB, flash 128 KiB, LCD monochrome 32×32, cinq boutons. Le BIOS du cœur doit être vérifié dans les sources de pokketstation : son fichier .info le déclare facultatif mais l'implémentation peut l'exiger.
PocketStation.png : photo Evan-Amos, https://commons.wikimedia.org/wiki/File:Sony-PocketStation.png , CC BY-SA 3.0 https://creativecommons.org/licenses/by-sa/3.0/ . Redimensionnement 346×512, alpha conservé, adaptation sous la même licence.
