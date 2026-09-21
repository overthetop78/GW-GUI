# Audit autonome du corpus de médias

Source par défaut : `F:\Retro`

Script : `scripts/temp/analyze-media-data.ps1`

Validateur : `tests/GWGUI.LocalDiskImageTests`

Résultats : `artifacts/media-audit`

## Préparation

- [x] Réinitialiser la feuille avant la nouvelle campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour retirer l'ancien historique et définir le fonctionnement, les contrôles et le classement des prochains défauts.
- [x] Réinitialiser le dossier de résultats avant la nouvelle campagne.
  - [x] Supprimer le contenu existant de `artifacts/media-audit/items` et supprimer `artifacts/media-audit/single-tests`, `checkpoint.json`, `content-signature-state.json` et `failure.json`.
  - [x] Créer `artifacts/media-audit/items` vide pour recevoir les prochains sous-dossiers numérotés avec leur identifiant.
- [x] Démarrer la nouvelle campagne depuis le début de `F:\Retro`.
  - [x] Modifier `scripts/temp/analyze-media-data.ps1` pour utiliser `F:\Retro` comme racine par défaut et supprimer l'ancien point de départ imposé sous `F:\Rétro`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour déclarer `F:\Retro` comme source par défaut.
- [ ] Exécuter la campagne continue depuis le premier fichier du premier dossier feuille.
  - [ ] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en laissant `scripts/temp/analyze-media-data.ps1` parcourir tout `F:\Retro` jusqu'au premier défaut ou jusqu'à la fin du corpus.

## Fonctionnement de la campagne

Le script parcourt les images reconnues sous le dossier demandé. Il crée un rapport distinct pour chaque image et enregistre la progression dans `artifacts/media-audit/checkpoint.json`.

Lorsqu'une image échoue, la campagne s'arrête sur cette image. Son dossier de résultat conserve :

- `report.json` avec toutes les informations obtenues avant l'arrêt ;
- `failure.json` avec l'exception et son contexte ;
- `execution.log` avec la sortie du validateur ;
- `failed-source.<extension>` avec une copie de l'image concernée.

Une exécution directe avec `-ImagePath` écrit son résultat sous `artifacts/media-audit/single-tests` et ne modifie pas le checkpoint de la campagne complète.

## Contrôles appliqués à chaque image

Le validateur contrôle :

- la reconnaissance du format et de la famille du média ;
- les informations et la structure de la représentation décodée : secteurs, flux, blocs, pistes optiques ou contenu séquentiel ;
- les blocs absents, invalides, vides ou de taille incohérente ;
- les bornes, les capacités et les dépassements numériques des volumes ;
- l'espace libre, qui ne peut être négatif ni dépasser la capacité annoncée ;
- les nombres de dossiers et de fichiers, en parcourant toute l'arborescence ;
- la somme des tailles logiques et occupées des fichiers, qui ne peut dépasser la capacité du volume ;
- les métadonnées, les attributs, les dates et les diagnostics obtenus pour chaque entrée ;
- l'extraction complète du contenu de chaque fichier et sa conformité avec la taille déclarée ;
- les informations ajoutées par MediaAnalysis, dont le type et l'icône ;
- les contenus qui restent inconnus après analyse.

L'absence de volume est enregistrée comme avertissement, car elle peut être normale. Un système de fichiers reconnu sans fichier, ou une disquette contenant un volume mais aucun fichier extrait, est une erreur à examiner.

## Classement des erreurs

Cette feuille ne contient aucune erreur en attente au début de la nouvelle campagne.

Lorsqu'une erreur est rencontrée, créer un titre de niveau 2 portant le type de l'image concernée, par exemple `## ATR`, `## ATX`, `## SCP` ou `## HFE`. Toutes les tâches concernant ce type restent sous ce titre. Ne créer le titre qu'au premier arrêt rencontré pour ce type.

Sous ce titre, créer une tâche pour l'image fautive, puis des sous-tâches concrètes indiquant chaque fichier à créer, modifier, déplacer, renommer ou supprimer. Consigner le chemin du rapport et le défaut exact observé avant de corriger le code.

## Fin de la campagne

La campagne est terminée lorsque le script atteint la fin du corpus sans erreur et que `artifacts/media-audit/checkpoint.json` contient l'état `complete`.
