# Travail restant : coleco

- [ ] Correction des ressources communes des reglages.
  - [ ] Relier les adaptateurs aux descriptions communes.
  - [ ] Regenerer et verifier les ressources.
    - [ ] Modifier `src/GWGUI.Emulation.Coleco/Resources/{00-Base,en-US,<culture>}/Emulation.resx` : ressources communes uniques, invariants uniquement dans 00-Base, anciennes traductions natives supprimees, copie base vers en-US nettoyee puis Argos global.
    - [ ] Produire `build/Debug/GW GUI/gwgui.exe` et les modules par `scripts/local-building.cmd --building=debug --modules=A` : verifier la compilation du raccordement Common et les artefacts de livraison, sans modifier les sources pendant le build.
    - [ ] Creer `docs/tasks/emulation/coleco-ressources-verification.json` : references et valeurs natives verifiees, cultures et invariants controles, compilation et affichage reels constates ; ne pas declarer des essais non executes.
