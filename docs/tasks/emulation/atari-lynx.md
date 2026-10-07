# Emulateurs Atari Lynx

- [x] Completer Beetle Lynx, GearLynx, Handy et Holani
  - [x] Identifier et verifier les ports officiels
    - [x] Creer `artifacts/temp/atari-lynx-validation` : sources et DLL officielles temporaires des quatre ports pour verifier options, formats, BIOS, boutons et initialisation native.
  - [x] Integrer les profils manquants
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Enums/CoreEnums.cs` : ajouter GearLynx, Handy et Holani en fin d enumeration.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/GearLynx/Constants/EmulatorConstants.cs` : identite, DLL, source, revision et extensions officielles.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/GearLynx/Factories/GearLynxMachineFactory.cs` : reutiliser les services communs avec son BIOS et ses commandes natives.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/Handy/Constants/EmulatorConstants.cs` : identite, DLL, source, revision et extensions officielles.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/Handy/Factories/HandyMachineFactory.cs` : reutiliser les services communs avec son BIOS facultatif et ses commandes natives.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/Holani/Constants/EmulatorConstants.cs` : identite, DLL, source, revision et extensions officielles.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/Holani/Factories/HolaniMachineFactory.cs` : reutiliser les services communs avec son BIOS facultatif et ses commandes natives.
  - [x] Verifier les Common, constantes et ressources
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/AtariLynx/Constants/ModelConstants.cs` : ajouter la cle partagee de description Lynx et les noms invariants des formats.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/BeetleLynx/Constants/EmulatorConstants.cs` : reutiliser la description commune sans changer son identite ni son perimetre.
    - [x] Creer `src/GWGUI.Emulation.Atari/Common/Machines/AtariLynx/Constants/InputConstants.cs` : definir les commandes Lynx avec des noms semantiques.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` : remplacer les indices Lynx bruts par les constantes semantiques.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/Holani/Constants/InputConstants.cs` : boutons natifs correspondant aux boutons physiques interieur et exterieur.
    - [x] Creer `src/GWGUI.Emulation.Atari/Emulators/Holani/Functions/InputFunctions.cs` : convertir A et B vers les boutons physiques de Holani.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/00-Base/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/ar-SA/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/cs-CZ/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/da-DK/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/de-DE/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/el-GR/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/en-US/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/es-ES/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/fi-FI/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/fr-FR/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/he-IL/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/hu-HU/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/id-ID/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/it-IT/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/ja-JP/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/ko-KR/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/nb-NO/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/nl-NL/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/pl-PL/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/pt-BR/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/pt-PT/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/ro-RO/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/ru-RU/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/sv-SE/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/th-TH/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/tr-TR/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/uk-UA/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/vi-VN/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/zh-Hans/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Resources/zh-Hant/Emulation.resx` : partager la description Lynx deja traduite.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/README.md` : documenter les quatre profils, formats, BIOS et services partages.
  - [x] Verifier et nettoyer
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Atari/AtariEmulatorAdapterTests.cs` : selection, extensions, BIOS, boutons et preservation de Beetle Lynx.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` : inclure les profils Lynx ajoutes.
    - [x] Creer `tests/GWGUI.Tests/Emulation/Atari/LynxNativeValidationTests.cs` : verifier les DLL officielles et options avec liberation des coeurs dans finally.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Atari/AtariEmulatorAdapterTests.cs` : verifier le champ ROM utilisateur et les extensions propres aux quatre profils.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Emulators/Holani/Factories/HolaniMachineFactory.cs` : qualifier la conversion propre a Holani pour lever l ambiguite avec les fonctions communes.
    - [x] Supprimer `tests/GWGUI.Tests/Emulation/Atari/LynxNativeValidationTests.cs` : supprimer le test externe apres execution.
    - [x] Supprimer `artifacts/temp/atari-lynx-validation` : supprimer sources, DLL et tous les artefacts temporaires.
    - [x] Modifier `docs/tasks/emulation/atari-lynx.md` : enregistrer les validations et le build Debug --modules=A, avec verification des huit modules.

## Resultats

- Commit avant les modifications Lynx : `2356805fd` (Hatari 2014).
- Quatre profils Lynx disponibles ; Beetle Lynx reste le profil par defaut.
- Les quatre profils acceptent le BIOS externe utilisateur, donc leur choix ROM reste affiche. BIOS obligatoire pour Beetle Lynx et GearLynx ; facultatif pour Handy et Holani. Les cartouches de jeu restent des medias distincts.
- Les descriptions des quatre profils partagent la traduction existante dans les 30 fichiers de ressources.
- 186 tests reussis, dont huit tests temporaires : identite et initialisation des quatre DLL, presence des options natives, copie du BIOS selectionne meme facultatif et absence de BIOS obligatoire.
- Tests et DLL temporaires supprimes ; 178 tests conserves reussis apres nettoyage.
- Build `scripts\local-building.cmd --building=debug --modules=A` termine avec code 0. Application et huit modules verifies (amstrad, atari, commodore, microsoft, nec, nintendo, sega, sony).
- Application de test : `F:\GW GUI\build\Debug\GW GUI\gwgui.exe`.
- Limite de verification : aucun demarrage de cartouche commerciale ni parcours de jeu teste ; les tests natifs portent sur identite, initialisation et options, les tests de BIOS sur la preparation des fichiers.

- [x] Preparer le commit Lynx demande
  - [x] Finaliser le suivi
    - [x] Modifier `docs/tasks/emulation/atari-lynx.md` : consigner la demande de commit et les validations deja reussies.

Commit demande : integration Lynx, ressources, tests et suivi ; build Debug des huit modules et 178 tests conserves reussis.
