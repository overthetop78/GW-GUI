# Architecture technique

## Portée de ce document

Ce document décrit l’organisation générale à conserver ou à atteindre. Il ne remplace pas l’audit fichier par fichier demandé dans [la première phase](tasks/01-full-code-audit.md). Aucun fichier n’est considéré comme correctement découpé uniquement parce qu’il apparaît dans cette vue d’ensemble.

## Projets de la solution

- `GWGUI.App` : interface WPF, navigation, composants visuels et présentation.
- `GWGUI.Domain` : modèles et règles métier indépendants de WPF et de Windows.
- `GWGUI.Infrastructure` : exécution de `gw`, matériel Windows, persistance et services externes.
- `GWGUI.Scp` : conteneurs, flux, décodage, encodage, reconstruction sectorielle et systèmes de fichiers liés aux images de disquette.
- `GWGUI.Tests` : vérifications automatiques. Son organisation sera revue après les fichiers de production, sans en faire une priorité prématurée.

Les frontières réelles entre ces projets et entre leurs fichiers doivent être contrôlées pendant l’audit complet. Les noms actuels ne constituent pas une preuve que les responsabilités sont déjà au bon endroit.

## Chaînes fonctionnelles

### Opérations utilisant Greaseweazle

Lecture, Écriture, Conversion et Outils construisent une commande typée, affichent exactement la commande exécutée, lancent `gw` sans console externe, diffusent sa sortie dans le terminal intégré, journalisent l’action selon les préférences et permettent son interruption contrôlée.

### Images de disquette

Le traitement d’une image doit distinguer clairement :

1. le conteneur ou fichier source ;
2. la géométrie et le format physique ;
3. le décodage ou l’encodage du flux ;
4. la reconstruction des pistes et secteurs ;
5. la protection éventuelle ;
6. le ou les systèmes présents sur une image multiformat ;
7. le système de fichiers et son arborescence ;
8. la présentation dans Conversion, Visualisateur et Explorateur.

Une image peut contenir plusieurs systèmes. La détection automatique doit donc pouvoir conserver plusieurs résultats compatibles. Un choix manuel sert à orienter l’opération demandée ; il ne permet pas de conclure arbitrairement que les autres systèmes présents dans l’image n’existent pas.

### Interface et état utilisateur

La fenêtre principale accueille les onglets sans concentrer toute leur logique. Les blocs réutilisables doivent être des composants indépendants lorsque cela évite une duplication réelle. Les paramètres, profils, langues, thème, matériel, journaux, dossiers récents et placement des fenêtres sont persistés par des services dédiés.

## Sources communes attendues

- Un catalogue central décrit machines, formats, géométries, protections, extensions et capacités disponibles.
- Lecture, Écriture, Conversion, Explorateur et Visualisateur consomment ce catalogue commun, avec le filtrage propre à leur opération.
- Les constantes techniques, enums, modèles, DTO, interfaces et tables de données sont séparés selon leur rôle.
- Les textes visibles proviennent des ressources de langue ; les identifiants techniques stables ne sont pas dupliqués dans chaque traduction lorsqu’ils ne se traduisent pas.
- Les algorithmes réellement communs sont mutualisés sans fusionner des formats qui ont des règles différentes.

## Problème structurel déjà confirmé

`AtariScpSectorImageReader.cs` contient des responsabilités qui ne sont pas limitées à Atari. C’est un exemple visible du problème, pas la liste du travail à réaliser. L’audit et le refactoring portent sur tout le dépôt : moteurs d’images, systèmes de fichiers, catalogues, services, fenêtres, composants, ressources et code de coordination.

Les contraintes permanentes du refactoring sont définies dans [les règles du projet](rules.md). Le détail ordonné du chantier se trouve dans [docs/tasks](tasks/README.md).
