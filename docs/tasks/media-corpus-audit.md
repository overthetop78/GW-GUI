# Audit autonome du corpus de médias

Source par défaut : `F:\Retro`

Script : `scripts/temp/analyze-media-data.ps1`

Validateur : `tests/GWGUI.LocalDiskImageTests`

Résultats : `artifacts/media-audit`

## MediaAnalysis

- [x] Centraliser les formats d’images de médias dans la table commune.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/CommonFileTypeTable.cs` pour y déclarer chaque extension d’image de média avec la catégorie `DiskImage` et l’icône générique actuelle.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/AmigaFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes et sa règle `.gz` déjà fournie par la table commune.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/AtariFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/AppleFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/BbcMicroFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/CommodoreFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/CpmFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/DecFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/IbmPcFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/MsxFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/UcsdFileTypeTable.cs` pour retirer ses règles `DiskImage` désormais communes.

- [x] Ajouter l’héritage des règles Atari.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Enums/MediaFileSystemFamily.cs` pour ajouter le parent `Atari` et renommer `AtariSt` en `AtariTos`.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Exploration/MediaFileSystemFamilyResolver.cs` pour renvoyer `AtariTos` pour les formats ST, STE, TT et Falcon.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/AtariFileTypeTable.cs` pour utiliser `Atari`, `Atari8Bit` et `AtariTos`, avec `.bas` au niveau Atari commun et les autres règles dans leur branche actuelle.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/FileTypes/MediaContentTypeCatalog.cs` pour rechercher dans l’ordre famille Atari précise, Atari commun, Common général.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Functions/MediaContentClassifier.cs` pour appliquer la reconnaissance exécutable TOS à `AtariTos`.
  - [x] Modifier `tests/GWGUI.Tests/Media/MediaContentTypeCatalogScenarios.cs` pour vérifier le renommage `AtariTos` et l’héritage Atari avec surcharge spécialisée.

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
