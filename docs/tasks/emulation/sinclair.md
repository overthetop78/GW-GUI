# Travail restant : sinclair

- [ ] Correction des ressources communes des reglages.
  - [ ] Relier les adaptateurs aux descriptions communes.
    - [ ] Creer `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Constants/SettingsResourceConstants.cs` : constantes partagees des libelles, aides et choix ; reutiliser les ressources SDK existantes.
    - [ ] Modifier `src/GWGUI.Emulation.Sinclair/Common/Contracts/CoreContracts.cs` : references de ressources communes sans changement des cles ou valeurs natives.
    - [ ] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/EightyOne/Dictionaries/OptionCatalog.cs` : relier chaque option et choix natifs aux constantes Common, selon leur sens et leur valeur, sans traduction par coeur.
    - [ ] Modifier `src/GWGUI.Emulation.Sinclair/Emulators/Fuse/Dictionaries/OptionCatalog.cs` : relier chaque option et choix natifs aux constantes Common, selon leur sens et leur valeur, sans traduction par coeur.
    - [ ] Modifier `src/GWGUI.Emulation.Sinclair/Common/Machines/Common/Functions/SettingsFunctions.cs` : afficher les ressources Common et supprimer la fabrication de cles depuis les prefixes natifs ou indices.
  - [ ] Regenerer et verifier les ressources.
    - [ ] Modifier `src/GWGUI.Emulation.Sinclair/Resources/{00-Base,en-US,<culture>}/Emulation.resx` : ressources communes uniques, invariants uniquement dans 00-Base, anciennes traductions natives supprimees, copie base vers en-US nettoyee puis Argos global.
    - [ ] Creer `docs/tasks/emulation/sinclair-ressources-verification.json` : references et valeurs natives verifiees, cultures et invariants controles, compilation et affichage reels constates ; ne pas declarer des essais non executes.
