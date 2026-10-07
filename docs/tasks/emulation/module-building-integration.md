# Intégration des nouveaux modules d émulation

- [x] Intégrer les huit modules dans la chaîne existante
  - [x] Compléter l annuaire commun des modules
    - [x] Créer `module-registry/amstrad.json`, `module-registry/microsoft.json`, `module-registry/nec.json`, `module-registry/nintendo.json`, `module-registry/sega.json` et `module-registry/sony.json` avec id, displayName et catalogUrl issus des manifestes, sur le modèle Amiga et Atari.
  - [x] Construire et vérifier les sorties
    - [x] Modifier les sorties sous `build/Debug/GW GUI` par `scripts/local-building.cmd --building=debug --modules=A` pour construire l application et les huit modules.
    - [x] Créer `artifacts/emulation-module-directory-check.json` avec le script existant de génération de l annuaire ; vérifier les huit entrées et la présence exclusive des DLL de modules sous Modules/id.
    - [x] Supprimer `artifacts/emulation-module-directory-check.json` après conservation du bilan.
    - [x] Modifier ce suivi avec les résultats effectifs.

- [x] Nettoyer le rapport ponctuel inutile
  - [x] Retirer le fichier et ses références
    - [x] Supprimer docs/reference/emulation-module-integration.json.
    - [x] Modifier docs/tasks/emulation/module-building-integration.md pour retirer les références au rapport et conserver le bilan de construction.

Bilan conservé : construction Debug réussie des huit modules ; DLL présentes exclusivement dans Modules/id ; aucun fichier NAudio dans les dossiers des modules.
