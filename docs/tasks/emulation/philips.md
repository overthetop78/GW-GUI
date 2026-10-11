# Travail restant : philips

- [ ] Correction des ressources communes des reglages.
  - [ ] Relier les adaptateurs aux descriptions communes.
    - [ ] Creer `src/GWGUI.Emulation.Philips/Common/Machines/Common/Constants/SettingsResourceConstants.cs` : constantes partagees des libelles, aides et choix ; reutiliser les ressources SDK existantes.
    - [ ] Modifier `src/GWGUI.Emulation.Philips/Common/Contracts/CoreContracts.cs` : references de ressources communes sans changement des cles ou valeurs natives.
    - [ ] Modifier `src/GWGUI.Emulation.Philips/Emulators/M2000/Dictionaries/OptionCatalog.cs` : relier chaque option et choix natifs aux constantes Common, selon leur sens et leur valeur, sans traduction par coeur.
    - [ ] Modifier `src/GWGUI.Emulation.Philips/Emulators/O2em/Dictionaries/OptionCatalog.cs` : relier chaque option et choix natifs aux constantes Common, selon leur sens et leur valeur, sans traduction par coeur.
    - [ ] Modifier `src/GWGUI.Emulation.Philips/Emulators/SameCdi/Dictionaries/OptionCatalog.cs` : relier chaque option et choix natifs aux constantes Common, selon leur sens et leur valeur, sans traduction par coeur.
    - [ ] Modifier `src/GWGUI.Emulation.Philips/Common/Machines/Common/Functions/SettingsFunctions.cs` : afficher les ressources Common et supprimer la fabrication de cles depuis les prefixes natifs ou indices.
  - [ ] Regenerer et verifier les ressources.
    - [ ] Modifier `src/GWGUI.Emulation.Philips/Resources/{00-Base,en-US,<culture>}/Emulation.resx` : ressources communes uniques, invariants uniquement dans 00-Base, anciennes traductions natives supprimees, copie base vers en-US nettoyee puis Argos global.
    - [ ] Creer `docs/tasks/emulation/philips-ressources-verification.json` : references et valeurs natives verifiees, cultures et invariants controles, compilation et affichage reels constates ; ne pas declarer des essais non executes.
