# Présentation des paramètres des machines

- [x] Ajuster les paramètres partagés de l’application
  - [x] Placer Créer dans le cadre de gestion de l’émulateur
    - [x] Modifier `src/GWGUI.App/Constants/Emulation/EmulationCoreManagementConstants.cs` : définir la marge droite du bouton.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationCoreManagementPanel.cs` : accueillir le bouton à l’extrémité droite du cadre sur la ligne de Rechercher, avec une marge au bord droit.
    - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Options/EmulationEmulatorManagementController.cs` : transmettre le bouton fourni par la section.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.Layout.cs` : conserver la création et sa visibilité, déplacer son bouton dans le cadre ; conserver l’en-tête pour les modules sans gestionnaire.
  - [x] Afficher les images existantes avant les noms sans augmenter les lignes
    - [x] Modifier `src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineChoice.cs` : ajouter la source d’image facultative.
    - [x] Modifier `src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs` : définir la largeur des miniatures et l’espace avant le nom.
    - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs` : ajouter une miniature dont la hauteur suit celle du texte, conserver les styles de configuration.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` : charger les ressources existantes lors de la création et du rechargement des choix.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.Persistence.cs` : réutiliser le chargement des choix au changement de langue.
  - [x] Valider
    - [x] Modifier ce document : consigner le résultat du build Debug de l’application et vérifier `build/Debug/GW GUI/gwgui.exe`.

Validation : `scripts/local-building.cmd --building=debug --modules=0` terminé avec le code 0 ; `build/Debug/GW GUI/gwgui.exe` présent. Aucun changement de l’action de création ni de ses conditions de visibilité. La hauteur de la miniature est liée à celle du texte ; les espacements verticaux des lignes sont conservés.

- [x] Corriger la livraison Debug sans modules
  - [x] Construire et vérifier les huit modules
    - [x] Recréer `build/Debug/GW GUI/gwgui.exe` et `build/Debug/GW GUI/Modules/{amstrad,atari,commodore,microsoft,nec,nintendo,sega,sony}/gwgui.emulation.<id>.dll` avec `scripts/local-building.cmd --building=debug --modules=A`.
    - [x] Modifier `docs/tasks/emulation/settings-machine-list-layout.md` : consigner le code de sortie et la présence de l’exécutable et des huit DLL.

Livraison corrigée : `scripts/local-building.cmd --building=debug --modules=A` terminé avec le code 0 ; exécutable et huit modules présents dans le dossier Debug final.


- [x] Corriger les miniatures et les noms demandés
  - [x] Gérer les miniatures par leur hauteur
    - [x] Modifier `src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs` : remplacer la largeur imposée par une hauteur maximale de miniature.
    - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs` : conserver les proportions, empêcher la hauteur du texte et de l’image de s’amplifier mutuellement, disposer l’image avant le nom.
  - [x] Corriger les noms invariants dans la base commune
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Resources/00-Base/Model.resx` : supprimer Commodore devant Amiga, y compris CD32.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Model.resx` : afficher NES et SNES.
  - [x] Ajouter les images manquantes
    - [x] Créer `src/GWGUI.Emulation.Atari/Assets/Machines/AtariXEGS.png` : photo XEGS transparente de Wikimedia.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Constants/MachineConstants.cs` : définir le fichier image XEGS.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Common/Machines/Common/Dictionaries/MachineCatalog.cs` : relier le modèle XEGS à son image.
    - [x] Créer `src/GWGUI.Emulation.Sony/Assets/Machines/PlayStation5.png` : photo PS5 transparente de Wikimedia, chargée par le catalogue existant.
    - [x] Modifier ce document : consigner le choix PET 4032B en attente ; ne pas substituer une photo de clavier graphique sans réponse.
  - [x] Livrer le build complet
    - [x] Recréer `build/Debug/GW GUI/gwgui.exe` et les huit DLL `build/Debug/GW GUI/Modules/<id>/gwgui.emulation.<id>.dll` avec `scripts/local-building.cmd --building=debug --modules=A`.
    - [x] Modifier ce document : consigner le résultat et les crédits des images utilisées.

PET 4032B : question en attente sur la réutilisation de `Pet4032.png` ou une photo distincte de la variante Business. Le build des autres corrections reste indépendant de ce choix.

Premier build de ces corrections : code 0, application et huit modules présents.
Crédits : AtariXEGS.png, https://commons.wikimedia.org/wiki/File:Xegs-transparent.png, MikeAtari / Lincolnh, domaine public ; PlayStation5.png, https://commons.wikimedia.org/wiki/File:PlayStation_5_and_DualSense_with_transparent_background.png, Osh33m / Soberian, CC BY-SA 4.0, https://creativecommons.org/licenses/by-sa/4.0/, PNG original inchangé.

- [x] Terminer après le choix PET 4032B
  - [x] Relier l’image et préserver les tests existants
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Dictionaries/MachineCatalog.cs` : donner au PET 4032B la ressource Pet4032ImageResource existante.
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/EmulationModuleSettingsNavigationScenarios.cs` : transmettre le bouton aux cinq appels existants de CreateView.
  - [x] Valider et livrer
    - [x] Modifier ce document : consigner les tests existants ciblés du contrôleur d’émulateurs.
    - [x] Recréer `build/Debug/GW GUI/gwgui.exe` et les huit modules avec `scripts/local-building.cmd --building=debug --modules=A`, après le lien PET 4032B.
    - [x] Modifier `docs/tasks/emulation/commodore-eight-bit.md` : retirer PET 4032B des photos distinctes restant à trouver et consigner le partage autorisé de Pet4032.png.
    - [x] Modifier ce document : consigner la livraison finale et le partage du PNG PET 4032.

Tests existants ciblés : 5 réussis, 0 échec (choix, catalogue vide, libération du contrôleur, localisation et installation des versions).

Livraison finale : build Debug avec les huit modules, code 0 ; exécutable et DLL présents. PET 4032B utilise directement Pet4032ImageResource, sans copie du PNG.
