# AcornEmu — BBC Micro Model A

- [x] Première version exécutable et testable du Model A.
  - [x] Consigner les retours de l'utilisateur.
    - [x] Modifier `docs/directives/emulation-development.md` : builds Debug réguliers et chargement des ROM depuis des fichiers de firmware externes sélectionnés par l'utilisateur.
  - [x] Construire les composants matériels séparés.
    - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Constants/EmulatorConstants.cs` et `Constants/HardwareConstants.cs` : identité, fréquences et limites matérielles.
    - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Interfaces/ICpuBus.cs` et `Services/Cpu6502.cs`, `Services/Cpu6502.Instructions.cs` : bus, registres, instructions et interruptions du CPU.
    - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Services/Via6522.cs`, `Dictionaries/KeyboardDictionary.cs` et `Services/Keyboard.cs` : ports, timers, interruptions et matrice BBC.
    - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Services/Video.cs` : CRTC, ULA, mémoire écran et premier rendu ; les glyphes proviennent de la ROM MOS externe, sans données ROM intégrées.
    - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Services/ModelAMachine.cs` : RAM, ROM, décodage, horloges et assemblage des composants.
  - [x] Raccorder le cœur au module existant.
    - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Services/EmulatorCore.cs` : implémenter le contrat local `IEmulatorCore` et les sorties de la machine.
    - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Factories/AcornEmuMachineFactory.cs` et `Functions/FirmwareFunctions.cs` : adaptateur, réglages et chargement des ROM utilisateur.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Machines/Common/Enums/CoreEnums.cs` : ajouter AcornEmu sans changer la valeur B2 ; compléter les ressources existantes utiles à sa sélection. Le nom technique invariant reste dans la base commune.
  - [x] Vérifier et livrer le premier jalon.
    - [x] Modifier `Emulators/AcornEmu/Factories/AcornEmuMachineFactory.cs` et `Services/EmulatorCore.cs` dans le module Acorn : compléter les facettes exigées par les interfaces Common et le retour de tampon audio. Compilation du module réussie.
    - [x] Modifier `Emulators/AcornEmu/Services/Video.cs` dans le module Acorn : tenir compte de la fréquence de caractères ULA pour distinguer les pixels des modes bitmap.
    - [x] Créer `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` : vérifications autonomes du CPU, du VIA, du décodage Model A, de la vidéo et du raccordement Common. Sept cas passent.
    - [x] Créer temporairement `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuBootTemporaryTests.cs` : lire les ROM et le programme de validation CPU en mémoire, vérifier le démarrage et une saisie BASIC sans lancer de fenêtre. Deux cas passent.
    - [x] Supprimer `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuBootTemporaryTests.cs` après exécution ; aucun fichier ROM ni artefact de session n'est écrit par ce test.
    - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` : documenter les fichiers réellement créés, leurs liaisons et les capacités effectives du jalon testé.
    - [x] Modifier `docs/directives/emulation-development.md` : conserver les détails techniques utilisés et les révisions des sources pour les retrouver localement.
    - [x] Produire `build/Debug/GW GUI/gwgui.exe` avec uniquement le module demandé Acorn par `scripts/local-building.cmd --building=debug --modules=acorn`, après vérification que l'application de l'utilisateur n'est pas en cours d'exécution. Example reste fourni automatiquement par le script existant. Script réussi ; exécutable, manifestes et identité de la DLL Acorn distribuée vérifiés.
    - [x] Modifier `docs/project/emulations/Acorn/acornemu-references.md` et `BBC Micro/Model A/model-a-reference.md` : remplacer les mentions antérieures à l'implémentation par l'état du premier jalon validé.
    - [x] Modifier `docs/directives/emulation-development.md` : conserver la formulation « module concerné », suffisante pour cibler le build demandé, sans puce redondante.

Le premier jalon vise le démarrage réel sur MOS/BASIC et l'interaction clavier/écran. Le son, la cassette et la sauvegarde matérielle complète seront ajoutés dans les jalons suivants, avant d'annoncer une émulation Model A complète. Leurs actions détaillées seront inscrites ici avant leur réalisation.

- [x] Suivre la disposition du clavier hôte, pavé numérique compris.
  - [x] Modifier `docs/directives/emulation-development.md` : inscrire la saisie selon la disposition active du clavier hôte.
  - [x] Modifier `src/GWGUI.Emulation/Contracts/EmulationInputSnapshot.cs`, `Functions/EmulationInputMappingFunctions.cs` et `Services/EmulationInputFrameStore.cs` : transporter et copier les caractères hôtes facultatifs sans changer les touches existantes.
  - [x] Modifier `src/GWGUI.App/Functions/Input/Keyboard/EmulationKeyMapper.cs`, créer `EmulationKeyMapper.HostCharacters.cs` dans le même dossier et modifier `src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs` : produire les caractères selon la disposition Windows active et les transmettre par le contrat existant.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Dictionaries/KeyboardDictionary.cs`, `Services/Keyboard.cs` et `Services/ModelAMachine.cs` : traduire les caractères hôtes en touches et SHIFT BBC, avec les chiffres et opérateurs du pavé numérique.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` : vérifier les symboles, SHIFT, le pavé numérique, le relâchement et le transport des caractères ; les 20 cas AcornEmu passent.
  - [x] Modifier `docs/directives/emulation-development.md` : réserver les modifications App/Emulation aux nécessités extrêmes et conserver la priorité des raccourcis système et des affectations existantes.
  - [x] Créer `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuHostKeyboardTemporaryTests.cs` : traduction Windows vérifiée avec les dispositions française, américaine et allemande (quatre cas réussis), dispositions libérées dans finally et fichier supprimé après exécution. Trois contrôles existants de raccourcis passent également.
  - [x] Modifier `docs/directives/emulation-development.md` : préciser que les changements nécessaires d'App/Emulation doivent servir à toute catégorie d'émulateur.
  - [x] Modifier `docs/directives/emulation-development.md` : traiter les changements nécessaires dans tous leurs consommateurs concernés avec un parcours unique.
  - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs`, `Functions/Input/Keyboard/EmulationKeyMapper.HostCharacters.cs` dans App et `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Services/Keyboard.cs` : transmettre les caractères sans règle propre à une machine dans App, en donnant priorité aux raccourcis et affectations.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/ControllerInput/ControllerInputTests.cs` : vérifier que la lecture physique complémentaire ne réintroduit pas une touche de raccourci et que les caractères n'écrasent pas une affectation ; 47 contrôles ciblés passent, transitions et associations comprises.
  - [x] Modifier `src/GWGUI.Emulation/Functions/EmulationHostProtocolFunctions.cs` : transporter aussi les caractères dans le protocole commun des cœurs exécutés dans un processus distinct.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` : transport en mémoire, lecture successive et rejet des nombres invalides vérifiés ; 61 contrôles ciblés passent, consommateurs du protocole compris.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Services/Keyboard.cs` et `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` : conserver SHIFT pendant une saisie d'espace ; contrôle ciblé réussi.
  - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` : décrire le parcours hôte, les fichiers et les limites du jeu de caractères BBC.
  - [x] Produire `build/Debug/GW GUI/gwgui.exe` avec `scripts/local-building.cmd --building=debug --modules=acorn` : script réussi, application préalablement fermée, exécutable et DLL Acorn vérifiés ; seuls Acorn et Example automatiquement fourni sont présents.
