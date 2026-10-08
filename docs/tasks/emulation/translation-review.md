# Revue des traductions d’émulation

- [x] Corriger les contresens techniques dans les textes des modules
  - [x] Prévenir leur génération et corriger les catalogues existants
    - [x] Modifier `scripts/tools/translate-resx-argos.py` : réutiliser les traductions communes des états et ajouter une réparation ciblée des contresens à partir des textes anglais, sans modifier les noms ni les valeurs internes.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/*/*.resx` : harmoniser les états dans toutes les langues et corriger les traductions techniques françaises identifiées.
    - [x] Modifier `scripts/tools/translate-resx-argos.py` : appliquer les corrections d’un catalogue en une seule écriture, en conservant sa structure XML.
    - [x] Modifier les ressources des sept autres modules et `src/GWGUI.App/Resources/*/Emulation/*.resx` : appliquer la même revue technique dans toutes les langues.
    - [x] Modifier `scripts/tools/translate-resx-argos.py` et les ressources Nintendo concernées : corriger les cinq phrases restantes en français, danois, tchèque et roumain, puis contrôler à nouveau les catalogues.
    - [x] Modifier `scripts/tools/translate-resx-argos.py` et `src/GWGUI.Emulation.Sega/Resources/fr-FR/Emulators.resx` : rédiger complètement les explications des cartouches de sauvegarde pour éviter les accords incorrects après correction du terme.
  - [x] Vérifier et fournir le résultat
    - [x] Modifier `docs/tasks/emulation/translation-review.md` : inscrire les contrôles XML, la conservation des clés et le résultat du build Debug avec les huit modules.

## Résultat

- Les ressources des huit modules et les textes communs d’émulation ont été examinés pour les contresens identifiés ; corrections dans les 29 cultures à partir des termes communs existants.
- Disabled / Off affichent les termes techniques de désactivation ; Enabled / On ceux d’activation. 42 924 libellés d’état contrôlés et cohérents.
- Français : plans graphiques, cartouches, bordures et défilement corrigés. Libellés et explications des cartouches de sauvegarde Sega relus ; choix par cartouche / par jeu corrigés.
- Les cinq descriptions restantes en français, danois, tchèque et roumain ont été corrigées. Aucun terme de handicap résiduel dans les catalogues d’émulation inspectés.
- 1 906 catalogues XML valides, sans clés dupliquées ; aucune clé ni valeur interne de configuration remplacée par une traduction.
- L’outil Argos utilise désormais les états communs et applique les corrections techniques aux futures traductions. Les mises à jour existantes sont groupées en une écriture par catalogue.
- Build Debug avec `scripts/local-building.cmd --building=debug --modules=A` réussi, code de sortie 0. Application et huit modules présents dans `build/Debug/GW GUI`.
- Cette revue cible les contresens techniques repérés ; les contrôles automatiques ne constituent pas une validation linguistique exhaustive de chaque phrase dans les 29 langues.
