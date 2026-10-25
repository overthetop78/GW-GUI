# AcornEmu — BBC Micro Model A

- [x] Retirer les ajouts de changement de version introduits par erreur.
  - [x] Modifier les contrôleurs et vues App de gestion d'émulateur, `MachineController.cs`, `MachineSession.cs` et leurs tests : retirer la notification de version installée et le redémarrage associé, en conservant le cycle Power existant et l'affichage à chaud.
  - [x] Modifier `docs/directives/emulation-development.md` : remplacer la directive inventée sur les versions par le cycle Power demandé, arrêt et libération du cœur puis nouvelle instance au rallumage.
  - [x] Modifier ce fichier pour consigner le retrait effectué : notifications et redémarrage sur installation supprimés ; cycle Power existant conservé ; 39 tests ciblés réussis (sessions, gestion des versions et vidéo AcornEmu).

- [x] Rétablir les abonnements de configuration existants.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSection.cs` : rétablir les abonnements dans Loaded et leur retrait dans Unloaded.
  - [x] Modifier `src/GWGUI.App/Views/Windows/Shell/MainWindow.ComponentConnections.cs` : retirer les deux lignes ajoutées pour déplacer ces abonnements dans la fenêtre.
  - [x] Modifier `docs/tasks/acornemu-model-a.md` : restauration vérifiée ; les deux fichiers sont identiques à leur version HEAD.
- [x] Compléter les composants vidéo BBC et charger les ROM SAA505x externes.
  - [x] Modifier `Common/Machines/Common/Enums/FirmwareEnums.cs`, `Dictionaries/FirmwareCatalog.cs`, `Common/Machines/BbcMicro/Constants/FirmwareConstants.cs`, `Dictionaries/FirmwareCompatibilityCatalog.cs`, `Dictionaries/ModelCatalog.cs` et `Functions/FirmwareFunctions.cs` dans Acorn : identifier les ROM de caractères et leur rôle par machine, y compris l'ancien relevé SAA5051, sans modifier les fichiers utilisateur.
  - [x] Modifier `Emulators/AcornEmu/Constants/EmulatorConstants.cs`, `Functions/FirmwareFunctions.cs`, `Factories/AcornEmuMachineFactory.cs`, `Dictionaries/ModelCatalog.cs` et la carte `Machines/BbcMicro/ModelA/Services/ModelAMachine.cs` : ajouter l'emplacement ROM de caractères externe et le transmettre au circuit vidéo.
  - [x] Créer `Emulators/AcornEmu/Services/Crtc6845.cs` et `Services/TeletextSaa5050.cs` ; modifier `Machines/BbcMicro/Common/Services/Video.cs` et la carte Model A : séparer le CRTC et le télétexte, rendre pendant les cycles CPU, respecter les registres, le curseur et les modifications de palette en cours d'image.
  - [x] Modifier les constantes et la fabrique AcornEmu, son catalogue de modèles et le rendu vidéo, ainsi que `src/GWGUI.Emulation.Acorn/Resources/*/Emulation.resx` : exposer ROM de caractères et sortie matérielle, puis traduire les nouveaux libellés avec le script Argos existant dans toutes les langues ; conserver les noms invariants dans 00-Base.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` et créer `AcornEmuVideoTests.cs` : vérifier ROM, registres, curseur, palette en cours de balayage, télétexte et sélection de sortie ; créer `AcornEmuVideoTemporaryTests.cs` pour les essais avec les ROM locales et son aperçu `tests/GWGUI.LocalEmulationTests/bin/Debug/net10.0-windows10.0.19041.0/acornemu-video.png` : 52 contrôles réussis, aperçu inspecté.
  - [x] Modifier `Services/Crtc6845.cs`, `Machines/BbcMicro/Common/Constants/VideoConstants.cs`, `Machines/BbcMicro/Common/Services/Video.cs` dans AcornEmu et `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuVideoTests.cs` : corriger le décalage et les largeurs du curseur observés, utiliser le bit de commande ULA pour les couleurs clignotantes ; 53 contrôles réussis, aperçu réinspecté puis test et aperçu temporaires supprimés.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/Acorn/AcornModuleTests.cs` : actualiser le contrôle Model A devenu exécutable par AcornEmu, en conservant l'absence de support Model A dans B2 ; les 77 contrôles Acorn passent.
  - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` : consigner les ROM, les composants, les réglages réels, les références, les 77 contrôles réussis et les limites de simulation.
  - [x] Produire `build/Debug/GW GUI/gwgui.exe` avec `scripts/local-building.cmd --building=debug --modules=acorn` après fermeture de l'application et vérifier les sorties : script réussi, exécutable et DLL Acorn présents ; Acorn et Example automatiquement fourni uniquement.

- [x] Consigner la vérification des composants et options vidéo réels.
  - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` : distinguer les fonctions CRTC/ULA déjà présentes, les compléments restants, la recherche locale de SAA5050, sa source externe et les sorties UHF/composite/RGB documentées.

- [x] Ajouter la RAM 16/32 Kio et compacter les statuts des ROM.
  - [x] Modifier `Common/Machines/BbcMicro/Constants/ModelAConstants.cs` et créer `Functions/MemoryFunctions.cs` dans Acorn : définir et résoudre le choix 16/32 Kio du Model A, avec 16 Kio par défaut et rejet des valeurs invalides.
  - [x] Modifier `Emulators/AcornEmu/Factories/AcornEmuMachineFactory.cs`, `Dictionaries/ModelCatalog.cs`, `Machines/BbcMicro/ModelA/Services/ModelAMachine.cs` et les fonctions Common `SettingsFunctions.cs` et `ConfigurationFunctions.Summary.cs` : exposer, enregistrer, résumer et appliquer la capacité choisie au bus et au rendu.
  - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs` : remplacer le texte du statut par une icône avec l'infobulle traduite et un nom accessible ; rendre sa colonne indépendante de celle du rôle ROM.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` : vérifier les deux capacités, les adresses indépendantes à 32 Kio, les alias à 16 Kio et le parcours de configuration.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` : utiliser la propriété `Id` des choix du contrat existant et l'assertion filtrée directe.
  - [x] Modifier le test temporaire existant `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuFirmwareTemporaryTests.cs` : essayer le démarrage BASIC avec les ROM locales en 16 et 32 Kio ; 41 contrôles AcornEmu passent, puis fichier temporaire supprimé.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Machines/BbcMicro/Dictionaries/FirmwareCompatibilityCatalog.cs` et `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` : attribuer Official aux six ROM d'origine identifiées pour le Model A et vérifier ces identités, en conservant les statuts des variantes d'autres modèles.
  - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` : consigner le choix RAM, le parcours des fichiers, les rôles MOS/langage et les six identités officielles ; 42 contrôles permanents passent après suppression du test temporaire.
  - [x] Produire `build/Debug/GW GUI/gwgui.exe` avec `scripts/local-building.cmd --building=debug --modules=acorn` et vérifier les sorties pour essai : script réussi, exécutable et DLL Acorn présents ; seuls Acorn et Example automatiquement fourni sont présents.

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

- [ ] Identifier et présenter les firmwares externes, y compris ceux non reconnus.
  - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` : identifier les 24 fichiers utilisateur par SHA-1, leurs rôles, les modèles associés et leur intérêt pour le Model A.
  - [x] Créer temporairement `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuFirmwareTemporaryTests.cs` : onze couples essayés en mémoire, sorties écran et appels OSWRCH relevés ; aucun fichier utilisateur modifié, test supprimé après utilisation.
  - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Dictionaries/FirmwareDictionary.cs` et modifier `Functions/FirmwareFunctions.cs` : reconnaître les variantes identifiées, présenter également les fichiers inconnus, charger les ROM sélectionnées sans liste blanche d'empreintes et conserver les contrôles de format matériel.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` : 31 cas passent, dont l'inspection des 24 fichiers locaux, les candidats inconnus, les en-têtes et les formats 8/16/32 Kio. Test temporaire supprimé après utilisation ; aucun firmware utilisateur modifié.
  - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` : consigner les onze couples essayés, les sorties effectives, les chemins de code et les limites.
  - [ ] Produire `build/Debug/GW GUI/gwgui.exe` avec `scripts/local-building.cmd --building=debug --modules=acorn` après fermeture de GW GUI par son utilisateur, puis vérifier l'exécutable et le module.


- [ ] Corriger Acorn : définitions communes, sélection par machine et composants d'AcornEmu correctement séparés.
  - [x] Modifier `docs/directives/emulation-development.md` : catalogue des identités partagé dans Common/Machines, compatibilités définies par modèle et chargement adapté par chaque émulateur.
  - [x] Modifier `docs/directives/emulation-development.md` : statuts par modèle pour les ROM connues ; identité inconnue conservée pour les fichiers non reconnus, distincte des contraintes de chargement.
  - [x] Modifier `Common/Machines/Common/Enums/FirmwareEnums.cs` et `Contracts/FirmwareContracts.cs` dans Acorn ; déplacer `Emulators/AcornEmu/Dictionaries/FirmwareDictionary.cs` vers `Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` : conserver les 24 identités et leurs rôles sans statut propre à un émulateur ou modèle.
  - [x] Créer `Common/Machines/BbcMicro/Dictionaries/FirmwareCompatibilityCatalog.cs`, `Constants/FirmwareConstants.cs`, `Constants/ModelAConstants.cs` et `Functions/FirmwareFunctions.cs` dans Acorn : sélection explicite pour le Model A, inspection par modèle et formats matériels partagés.
  - [x] Modifier `Emulators/AcornEmu/Functions/FirmwareFunctions.cs`, `Factories/AcornEmuMachineFactory.cs` et `Emulators/B2/Factories/B2MachineFactory.cs` : réutiliser la reconnaissance et la compatibilité communes ; garder dans les adaptateurs les champs et modes de chargement.
  - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs` : afficher les candidats reconnus sans destination chargeable, en réservant Utiliser aux candidats ayant une destination ; conserver la sélection existante des autres candidats.
  - [x] Déplacer `Emulators/AcornEmu/Services/ModelAMachine.cs` dans `Machines/BbcMicro/ModelA/Services/`, `Services/Video.cs` et `Services/Keyboard.cs` dans `Machines/BbcMicro/Common/Services/` ; déplacer les tables clavier vers le fichier Common/Machines/BbcMicro existant ; créer `Machines/BbcMicro/Common/Constants/VideoConstants.cs` et supprimer `Constants/HardwareConstants.cs` après répartition de ses valeurs.
  - [x] Créer `Emulators/AcornEmu/Interfaces/IAcornMachine.cs`, `Contracts/ModelContracts.cs` et `Dictionaries/ModelCatalog.cs` ; modifier `Services/EmulatorCore.cs` et la fabrique : sélectionner l'implémentation du modèle sans coder le Model A dans le cœur générique et annoncer seulement les modèles réellement implémentés.
  - [ ] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuTests.cs` : vérifier la portée des compatibilités, le catalogue partagé par B2/AcornEmu, les machines enregistrées et la préservation des contrôles matériels ; adapter les chemins des composants déplacés. Créer puis supprimer un test temporaire des 24 fichiers et du démarrage avec les ROM locales.
  - [ ] Modifier `docs/project/emulations/Acorn/acornemu-references.md`, `BBC Micro/bbc-micro-architecture.md` et `BBC Micro/Model A/model-a-reference.md` : documenter les responsabilités et les chemins finaux, les compatibilités par modèle et les résultats de validation.
  - [ ] Produire `build/Debug/GW GUI/gwgui.exe` avec `scripts/local-building.cmd --building=debug --modules=acorn` après fermeture de GW GUI par son utilisateur, puis vérifier les sorties.

- [x] Fournir le build Debug Acorn demandé pour essai.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Dictionaries/ModelCatalog.cs` et `Machines/BbcMicro/Common/Services/Video.cs` du même émulateur : identifier explicitement les constantes vidéo BBC pour résoudre le conflit de noms à la compilation.
  - [x] Produire `build/Debug/GW GUI/gwgui.exe` et son module Acorn avec `scripts/local-building.cmd --building=debug --modules=acorn`, puis vérifier leur présence : script réussi, exécutable et DLL Acorn présents ; Example fourni automatiquement par le script.

- [x] Émuler le son mono du Model A.
  - [x] Créer `Emulators/AcornEmu/Constants/SoundConstants.cs` et `Services/Sn76489.cs` dans Acorn : registres, trois tons, bruit, atténuation et compteurs du PSG, séparés du câblage BBC.
  - [x] Créer `Emulators/AcornEmu/Machines/BbcMicro/Common/Constants/AudioConstants.cs` et `Services/Audio.cs` : horloge BBC 4 MHz et production PCM mono 44 100 Hz avec phase persistante entre images.
  - [x] Modifier `Emulators/AcornEmu/Machines/BbcMicro/ModelA/Services/ModelAMachine.cs`, `Interfaces/IAcornMachine.cs` et `Services/EmulatorCore.cs` : raccorder VIA, horloges, blocs mono, réinitialisation et libération aux contrats existants.
  - [x] Créer `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuAudioTests.cs` : vérifier fréquence mesurée, débit sans dérive, atténuation, bruit, câblage VIA et format mono avec données synthétiques.
  - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` : composants, fréquences, raccordements et limites documentés ; 85 tests Acorn réussis ; build Debug Acorn réussi, exécutable et DLL vérifiés, Example automatiquement fourni.
- [x] Corriger la sortie physique du son AcornEmu.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Factories/AcornEmuMachineFactory.cs` : transmettre la fabrique audio fournie par le contexte au Common au lieu de null.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuAudioTests.cs` : vérifier que la sortie fournie au contexte reçoit le PCM mono et est libérée par la session.
  - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` et ce fichier : cause consignée, huit contrôles audio réussis dont le raccordement de sortie et sa libération.
  - [x] Produire "build/Debug/GW GUI/gwgui.exe" et "Modules/acorn/gwgui.emulation.acorn.dll" avec le build Debug Acorn : script réussi et sorties vérifiées ; Example fourni automatiquement.
