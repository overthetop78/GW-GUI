# Organisation et entretien de la documentation

La documentation de GW GUI décrit le produit actuel, ses références, ses procédures et les travaux
qui restent réellement à effectuer. Un même sujet ne doit pas être maintenu simultanément comme
architecture actuelle et comme ancienne feuille d’implémentation.

## Responsabilité des dossiers

- `architecture` décrit la structure actuelle du code, les contrats et les échanges entre composants.
- `project` contient les décisions confirmées, les procédures de développement, de test, de
  publication et d’entretien du dépôt.
- `reference` rassemble les catalogues techniques vérifiés : machines, moteurs, formats, filtres et
  périphériques.
- `ui` explique le comportement visible de l’application et l’organisation de ses fenêtres.
- `future` conserve les idées et orientations qui ne sont pas encore engagées.
- `tasks` contient uniquement des actions encore ouvertes. Une feuille entièrement terminée est
  supprimée après transfert de son résultat utile dans les autres dossiers.

## Où consigner une information

- Une décision durable va dans `project/decisions.md` ou dans le document d’architecture concerné.
- Le fonctionnement visible va dans `ui`.
- Une procédure reproductible va dans `project`.
- Une donnée technique vérifiée va dans `reference`.
- Une idée non autorisée ou non planifiée va dans `future`.
- Une action concrète restante va dans `tasks`, sous forme de checklist conforme à
  `.codex/config.toml`.

Les résultats détaillés d’un test temporaire ne sont conservés que s’ils expliquent une limite, un
choix ou une procédure encore utile. Les historiques de modifications et les listes d’actions déjà
réalisées appartiennent à Git et ne restent pas sous forme de feuilles de tâches terminées.

## Contrôle

`scripts/audit-docs.ps1` contrôle les liens Markdown locaux, l’indexation des documents et l’absence
de feuille terminée dans `docs/tasks`. Le résultat du dernier nettoyage complet est inscrit ici à la
fin de l’opération.

Le nettoyage complet du 10 septembre 2026 aboutit à l’inventaire suivant :

| Emplacement | Documents Markdown |
|---|---:|
| Racine de `docs` | 1 |
| `architecture` | 11 |
| `future` | 4 |
| `project` | 10 |
| `reference` | 5 |
| `tasks` | 10, dont l’index et neuf feuilles encore ouvertes |
| `ui` | 5 |
| **Total** | **46** |

L’audit final ne trouve aucun lien local cassé, aucun document non indexé et aucune feuille terminée
conservée dans `docs/tasks`.
