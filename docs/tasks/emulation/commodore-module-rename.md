# Renommage du module Amiga en Commodore

- [x] Renommer la famille constructeur en respectant les machines Amiga
  - [x] Renommer les identités et références du module
    - [x] Modifier les sources, ressources, projets, solution, tests et documents référant à `GWGUI.Emulation.Amiga`, `gwgui.emulation.amiga`, `AmigaEmulationModule` ou `module-amiga` pour employer Commodore ; préserver les références aux formats et aux machines Amiga.
    - [x] Renommer `src/GWGUI.Emulation.Amiga` en `src/GWGUI.Emulation.Commodore`, son csproj et ses deux classes de module ; renommer le sous-dossier machine `Common/Machines/AmigaCDTV` en `CommodoreCDTV` et `module-registry/amiga.json` en `commodore.json`.
    - [x] Modifier le manifeste, les constantes du module et les ressources communes pour la famille Commodore et les modèles Commodore Amiga ; adapter la factory pour conserver l accès au stockage Amiga existant sans déplacer les données personnelles.
  - [x] Vérifier la construction et les tests existants concernés
    - [x] Modifier les sorties `build/Debug/GW GUI` par `scripts/local-building.cmd --building=debug --modules=A` et vérifier le dossier Modules/commodore, ses fichiers et l absence de module Amiga dans la sortie livrée.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` et `tests/GWGUI.Tests/Emulation/Modules/EmulationModuleManifestTests.cs` avec le dossier CommodoreCDTV et l identité commodore ; exécuter les tests existants d architecture et d émulation concernés.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Modules/EmulationModuleManifestTests.cs` pour renommer aussi les identifiants JSON échappés du test de doublon ; modifier les exemples actuels dans `docs/architecture/emulation-modules.md`, `emulation-module-localization.md` et `emulation.md` avec l identité Commodore et le stockage ancien conservé.
    - [x] Modifier le présent suivi avec le résultat du test WPF existant `SettingsViewsTests.AmigaModuleWindowBuildsItsVisualTree`, incluant fermeture des fenêtres et suppression du dossier temporaire dans son finally.
    - [x] Modifier le présent suivi avec les résultats des tests existants concernés et de la compilation ; aucune création de rapport JSON.

Résultats : build Debug complet réussi avec les huit modules, dont Modules/commodore/gwgui.emulation.commodore.dll ; aucun dossier Modules/amiga. Les 158 tests d architecture, de manifestes et d adaptateurs sélectionnés réussissent, ainsi que le test WPF de construction et fermeture de la fenêtre (1/1). Les machines et formats Amiga gardent leurs identités spécifiques ; le CDTV et son dossier sont Commodore CDTV. Les ressources de nom constructeur restent dans 00-Base. Aucun fichier JSON de rapport ajouté.
