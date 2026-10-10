# Correction Coleco : configuration de la cartouche

- [x] Déplacer le type natif dans la configuration de la cartouche
  - [x] Adapter les contrats et la fenêtre existants
    - [x] Modifier `src/GWGUI.Emulation/Contracts/CartridgeSlotSettings.cs`, `src/GWGUI.App/Views/Dialogs/Emulation/Storage/CartridgeSlotConfigurationDialog.cs` et `src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs` : transmettre les champs et choix du module à la fenêtre cartouche existante, retourner leurs valeurs sans changer les options des autres modules.
  - [x] Déplacer le champ Coleco et sa persistance
    - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Functions/{SettingsFunctions,StorageFunctions.Settings}.cs` : retirer le cadre cartouche de la page Stockage, activer Configurer pour l'emplacement Gearcoleco, fournir le choix natif et enregistrer le résultat dans l'option déjà appliquée au cœur ; conserver les ressources traduites existantes.
  - [x] Vérifier et livrer la correction
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Coleco/ColecoModuleTests.cs` : vérifier absence du champ global, présence dans la configuration cartouche, modification, validation et retour de la valeur enregistrée.
    - [x] Modifier `tests/GWGUI.Tests/Interface/SettingsViews/SettingsViewsTests.cs` : vérifier les choix et la valeur retournée par la vraie fenêtre cartouche, fermée dans finally.
    - [x] Renommer `src/GWGUI.Emulation.Epoch/module.json` en `module.json.pending` : conserver le manifeste du module inachevé hors discovery pendant la livraison de Coleco ; restituer son nom lors du raccordement Epoch prévu dans son plan.
    - [x] Produire `build/Debug/GW GUI/gwgui.exe` et les DLL des modules avec `scripts/local-building.cmd --building=debug --modules=A` : attendre réussite terminale, contrôler les artefacts et exécuter les essais Coleco et vues concernés.
    - [x] Modifier `docs/tasks/emulation/{chronometre-modules,methode-modules}.md` : exprimer le dépassement en durée négative et préserver les timestamps ; consigner la configuration du support dans sa fenêtre, sans cadre global.
    - [x] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/CartridgeSlotConfigurationDialog.cs` : utiliser TryGetValue pour le choix enregistré afin de supprimer l'avertissement nullable introduit, sans changer le comportement.
    - [x] Produire de nouveau `build/Debug/GW GUI/gwgui.exe` et les modules par `scripts/local-building.cmd --building=debug --modules=A` : livrer le correctif nullable compilé et vérifier les choix cartouche.
    - [x] Modifier `.git/index` et la référence de branche par un commit limité à la correction cartouche : préserver Epoch inachevé sans le commiter.
