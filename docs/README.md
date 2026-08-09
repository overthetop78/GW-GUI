# GW GUI — Documentation actuelle

Cette documentation décrit l’état actuel, les décisions confirmées et l’ordre des travaux. Les documents placés dans [`old`](old/README.md) sont historiques et ne servent plus de référence actuelle.

## Règles et ordre de travail

- [Règles permanentes](rules.md) — règles de décision, code, documentation, corpus et Git ; aucune case à cocher.
- [Ordre obligatoire des tâches](tasks/README.md) — index des phases à réaliser dans l’ordre demandé.

## État et décisions

- [État du projet pour reprendre une nouvelle discussion](project-handoff.md)
- [Décisions produit et interface](decisions.md)
- [Questions et réponses confirmées](questions-and-answers.md)
- [Architecture technique actuelle](architecture.md)
- [Version, build et révision](versioning.md)

## Tâches détaillées

1. [Audit complet de tout le code](tasks/01-full-code-audit.md)
2. [Refactorisation et découpage de tout le code](tasks/02-full-refactoring.md)
3. [Constantes et textes techniques](tasks/03-constants-and-text.md)
4. [Enums, modèles de données et contrats](tasks/04-models-and-contracts.md)
5. [Fonctions et services](tasks/05-functions-and-services.md)
6. [Réorganisation des traductions](tasks/06-localization.md)
7. [Interface, robustesse et maintenance](tasks/07-ui-robustness-maintenance.md)
8. [Workflow GitHub de build](tasks/08-github-build.md)
9. [Validation finale des images et du matériel](tasks/09-final-validation.md)

## Spécifications de l’interface

- [Spécification visuelle générale](ui/visual-specification.md)
- [Fenêtre principale et navigation](ui/main-window.md)
- [Onglet Lecture](ui/read.md)
- [Onglet Écriture](ui/write.md)
- [Onglet Conversion](ui/convert.md)
- [Visualisateur et Explorateur](ui/visualizer-explorer.md)
- [Options, matériel et diagnostics](ui/options.md)

## Références techniques et couverture

- [Familles et formats d’images](Liste-imagesdisk.md)
- [Couverture des commandes Greaseweazle](gw-command-coverage.md)
- [Couverture des décodeurs et encodeurs de flux](scp-decoder-coverage.md)
- [Références des décodeurs](scp-decoder-references.md)

## Guides

- [Guide utilisateur français](user-guide.fr.md)
- [English user guide](user-guide.en.md)

## Règle de lecture

En cas de contradiction, la décision la plus récente de l’utilisateur prévaut. Une ambiguïté n’autorise pas à inventer un comportement : elle doit être signalée avant réalisation.
