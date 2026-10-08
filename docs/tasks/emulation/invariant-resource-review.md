# Vérification des ressources invariantes

- [x] Conserver les valeurs invariantes dans la base commune
  - [x] Corriger leur classement et leurs copies
    - [x] Modifier `scripts/tools/translate-resx-argos.py` : classifier explicitement les noms de machines et d’émulateurs, les valeurs numériques avec unités techniques, les formats, filtres et palettes nommées ; exclure les textes descriptifs et les états traduisibles. Ajouter le nettoyage des copies culturelles et le filtrage du générateur.
    - [x] Modifier `src/GWGUI.Emulation.*/Resources/*/*.resx` et les catalogues d’émulation `src/GWGUI.App/Resources/*/Emulation/*.resx` : conserver les clés invariantes dans 00-Base et retirer leurs copies, y compris dans en-US, sans changer leurs références.
  - [x] Vérifier le résultat
    - [x] Modifier `docs/tasks/emulation/invariant-resource-review.md` : inscrire les contrôles du classement, des clés et du repli sur la base commune, puis le résultat du build Debug avec les huit modules.

Résultat : 523 clés invariantes Nintendo conservées dans 00-Base ; 15 167 copies retirées des 29 cultures, en-US compris. Aucune copie des catégories contrôlées dans les sept autres modules ni les catalogues Emulation de l’application.

Contrôles : 1 906 catalogues XML valides, aucune clé en double, aucune copie des invariants classés restante. Les états et descriptions restent traduisibles. Le chargement existant EmulationModuleLocalization se replie sur 00-Base ; aucune référence de ressource modifiée. Le générateur filtre ces invariants et nettoie également leurs copies anglaises.

Build : scripts/local-building.cmd --building=debug --modules=A terminé avec le code 0. Exécutable build/Debug/GW GUI/gwgui.exe et huit DLL de modules présents.
