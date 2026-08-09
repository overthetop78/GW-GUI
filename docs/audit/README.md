# Audit complet du code — index

Cet ensemble constitue le livrable de la phase 01. Il décrit l’état observé du dépôt avant toute refactorisation. Aucun document de ce dossier n’autorise à modifier un comportement produit : les propositions de déplacement concernent uniquement l’organisation du code et devront être validées avant la phase 2.

## Documents

- [Inventaire du dépôt](inventory.md) — projets, fichiers, ressources, scripts et workflows.
- [Chaîne de traitement des disquettes](disk-pipeline.md) — conteneurs, flux, secteurs, systèmes de fichiers, détection et parcours UI.
- [Dépendances et état partagé](dependencies-and-state.md) — dépendances entre projets, composition et responsabilités globales.
- [Constats structurels](structural-findings.md) — monolithes, noms trompeurs, chaînes de conditions, duplications et textes.
- [Données, constantes et textes](data-and-text-audit.md) — sources de vérité dispersées, nombres techniques et localisation.
- [Matrice de refactorisation](refactoring-matrix.md) — destination proposée, risque et validations nécessaires.
- [État du versioning](versioning-status.md) — comparaison entre `versioning.md` et l’implémentation réelle.

## Portée et méthode

L’audit couvre :

- les quatre projets de production et le projet de tests ;
- les fichiers C#, XAML, projets MSBuild, manifeste et ressources ;
- les 20 catalogues de traduction et leurs 30 variantes chacun ;
- les scripts de build, packaging, traduction et corpus ;
- l’installateur et le workflow GitHub ;
- les parcours Lecture, Écriture, Conversion, Visualisateur et Explorateur.

Les fichiers homogènes, comme les décodeurs et encodeurs ayant chacun leur propre fichier, sont inventoriés individuellement mais partagent une même conclusion lorsque leur structure est réellement identique.

## Limite de l’audit

Les décisions de comportement non confirmées ne sont pas transformées en tâches d’implémentation. En particulier, une sélection manuelle de format ne signifie pas qu’une image multiformat doit perdre ses autres interprétations valides. Le futur routage devra distinguer la préférence demandée de l’exclusion des autres systèmes.
