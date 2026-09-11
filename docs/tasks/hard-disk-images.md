# Images HDD — compléments après l’affichage des médias

Cette feuille commence seulement après l’achèvement de toutes les cases de
[`media-exploration.md`](media-exploration.md). Le deuxième commit a déjà été créé après la feuille
d’affichage. Les fonctions communes aux HDD, CD/DVD/optiques et cassettes/bandes sont réalisées à
ce stade par `media-exploration.md`; elles ne sont pas dupliquées ici. Cette feuille conserve les
compléments propres à la création, aux variantes et au cycle de vie des images HDD déjà recensés
dans [`../project/hard-disk-format-catalog.md`](../project/hard-disk-format-catalog.md).

Les cases sont exécutées et cochées une par une, dans l’ordre. Avant de continuer, toute action
manquante est ajoutée après la dernière case cochée et avant l’action qui en dépend. Une action
imprécise est réécrite avec son fichier exact avant son exécution; une action devenue fausse ou sans
objet est supprimée.

- [ ] 1. Réconcilier le travail restant après l’orchestration et l’affichage
  - [ ] 1.1 Vérifier les limites entre les feuilles sans dupliquer les fonctions communes
    - [ ] Modifier `docs/project/media-support-planning.md` avec l’état effectivement obtenu après `media-exploration.md` pour distinguer les capacités communes terminées des compléments HDD encore ouverts.
    - [ ] Modifier `docs/tasks/hard-disk-images.md` avec une action concrète et un chemin exact pour tout complément HDD, optique ou séquentiel encore nécessaire après cette vérification, en plaçant chaque nouvelle action après la dernière case cochée et avant l’action qui en dépend.
  - [ ] 1.2 Mettre le catalogue durable au niveau du code obtenu
    - [ ] Modifier `docs/project/hard-disk-format-catalog.md` pour corriger l’état de chaque conteneur, ensemble d’images, table de partitions et système de fichiers à partir des implémentations réellement présentes après les deux premières feuilles.

- [ ] 2. Compléter la description et la composition des images HDD
  - [ ] 2.1 Exposer les capacités intrinsèques des formats
    - [ ] Modifier `src/GWGUI.Emulation/HardDisks/HardDiskImageFormat.cs` pour exposer les tailles de secteurs, géométries, préparations, limites et opérations réellement acceptées par chaque format sans les déduire de son libellé.
    - [ ] Modifier `src/GWGUI.Emulation/HardDisks/DiskFormatRegistry.cs` pour fournir ces capacités depuis les enregistrements de conteneurs, ensembles d’images, tables de partitions et systèmes de fichiers.
    - [ ] Modifier `src/GWGUI.Emulation/HardDisks/DiskImagePlan.cs` pour valider une composition à partir des capacités déclarées par le registre avant toute création de fichier.
    - [ ] Modifier `docs/project/hard-disk-format-catalog.md` avec les paramètres, limites, tailles de secteurs et géométries effectivement exposés par le registre.
  - [ ] 2.2 Adapter uniquement les catalogues propres aux émulateurs
    - [ ] Modifier `src/GWGUI.Emulation.Amiga/Functions/AmigaHardDiskFormats.cs` pour composer ses choix depuis les capacités communes tout en conservant uniquement les combinaisons acceptées par les émulateurs Amiga.
    - [ ] Modifier `src/GWGUI.Emulation.Atari/Functions/AtariHardDiskFormats.cs` pour composer ses choix depuis les capacités communes tout en conservant uniquement les combinaisons acceptées par les émulateurs Atari.
    - [ ] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/HardDiskDriveConfigurationDialog.cs` pour afficher et valider les paramètres fournis par `HardDiskImageFormat` sans recréer les règles des formats dans l’interface.
    - [ ] Modifier `docs/project/hard-disk-format-catalog.md` avec les combinaisons Amiga et Atari réellement vérifiées après ces adaptations.

- [ ] 3. Étendre les variantes historiques encore retenues
  - [ ] 3.1 Définir chaque variante avant de modifier son implémentation
    - [ ] Modifier `docs/project/hard-disk-format-catalog.md` avec les références, licences et capacités réelles de chaque variante encore absente avant de décider de l’intégrer.
    - [ ] Modifier `docs/tasks/hard-disk-images.md` après cette étude pour ajouter une action séparée avec chemin exact pour chaque constructeur, Reader, Writer ou formateur retenu et supprimer les variantes finalement sans objet.
  - [ ] 3.2 Compléter les variantes Pascal retenues
    - [ ] Modifier `src/GWGUI.Emulation/HardDisks/FileSystems/PascalVolumeFormatter.cs` avec uniquement les variantes de répertoire et d’ordre des octets confirmées dans le catalogue.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/HardDisks/PascalFormattingTests.cs` avec une vérification autonome de chaque variante Pascal ajoutée et de ses combinaisons invalides.
    - [ ] Modifier `docs/project/hard-disk-format-catalog.md` avec les variantes Pascal effectivement validées.
  - [ ] 3.3 Compléter les dispositions EBR retenues
    - [ ] Modifier `src/GWGUI.Emulation/HardDisks/Partitioning/ExtendedMbrWriter.cs` avec uniquement les dispositions EBR historiques confirmées dans le catalogue.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/HardDisks/ExtendedMbrTests.cs` avec une vérification autonome de chaque disposition EBR ajoutée, de ses limites et de ses combinaisons invalides.
    - [ ] Modifier `docs/project/hard-disk-format-catalog.md` avec les dispositions EBR effectivement validées.

- [ ] 4. Compléter le cycle de vie des ensembles d’images HDD
  - [ ] 4.1 Résoudre les parents et membres associés
    - [ ] Modifier `src/GWGUI.Emulation/HardDisks/DiskImageDependencyReader.cs` pour lire les références par chemin, hash ou UUID réellement documentées pour les conteneurs retenus.
    - [ ] Modifier `src/GWGUI.Emulation/HardDisks/DiskImageDependencyIndex.cs` pour résoudre ces identités, détecter les cycles et conserver les ensembles segmentés comme une seule dépendance logique.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/HardDisks/DependencyResolutionTests.cs` avec les parents par chemin, hash et UUID ajoutés, les segments, les cycles, les absences et les références invalides.
    - [ ] Modifier `docs/project/hard-disk-format-catalog.md` avec les dépendances effectivement prises en charge et leurs limites.
  - [ ] 4.2 Publier un ensemble sans laisser de résultat partiel
    - [ ] Modifier `src/GWGUI.Emulation/HardDisks/DiskImageSetPublication.cs` pour publier collectivement tous les membres attendus et refuser un ensemble incomplet.
    - [ ] Modifier `src/GWGUI.Emulation/HardDisks/FileImageSetStorage.cs` pour préserver la destination existante et nettoyer uniquement les fichiers temporaires appartenant à l’opération interrompue.
    - [ ] Modifier `tests/GWGUI.Tests/Emulation/HardDisks/ImageSetPublicationTests.cs` avec les réussites, interruptions, collisions, membres manquants et nettoyages contrôlés.
  - [ ] 4.3 Supprimer collectivement une image et ses membres
    - [ ] Modifier `src/GWGUI.App/Services/Emulation/HardDiskDeletionService.cs` pour établir la liste complète des membres et dépendants avant de proposer une suppression.
    - [ ] Modifier `src/GWGUI.App/Services/Emulation/HardDiskDeletionFile.cs` pour supprimer uniquement la liste validée et conserver les originaux lorsqu’une vérification préalable échoue.
    - [ ] Créer `tests/GWGUI.Tests/Emulation/HardDisks/HardDiskDeletionTests.cs` avec des fichiers temporaires autonomes couvrant ensemble complet, dépendant actif, membre absent, refus et échec avant suppression.
  - [ ] 4.4 Convertir une image HDD sans mélanger la conversion avec l’application
    - [ ] Créer `src/GWGUI.MediaEngine/Conversion/HardDisk/HardDiskImageConversionService.cs` pour orchestrer le Reader et le Writer enregistrés, copier les blocs par plages et publier la destination seulement après une conversion complète.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaConversionComposition.cs` pour enregistrer le service de conversion HDD avec les capacités réellement fournies par les formats.
    - [ ] Modifier `src/GWGUI.App/Controllers/MainWindow/ConversionTabController.cs` pour appeler ce service commun sans interpréter les conteneurs, partitions ou blocs dans l’application.
    - [ ] Créer `tests/GWGUI.Tests/MediaEngine/HardDisk/HardDiskImageConversionTests.cs` avec des sources simulées couvrant copie par plages, géométrie connue ou inconnue, annulation, destination existante et absence de résultat partiel.

- [ ] 5. Valider les compléments sans utiliser le corpus local
  - [ ] 5.1 Exécuter les tests généraux autonomes après stabilisation du code
    - [ ] Modifier `docs/project/testing.md` avec la commande et le résultat des tests ciblés de `tests/GWGUI.Tests/Emulation/HardDisks` et `tests/GWGUI.Tests/MediaEngine/HardDisk`, sans utiliser `image_test`.
  - [ ] 5.2 Produire le build qui précède les essais manuels
    - [ ] Modifier `docs/project/testing.md` avec le résultat de `scripts/build.ps1 -Configuration Debug` et la présence vérifiée de `build/Debug/GW GUI/gwgui.exe`.
  - [ ] 5.3 Mettre à jour la documentation durable finale
    - [ ] Modifier `docs/architecture/media-format-orchestration.md` avec les capacités HDD, optiques et séquentielles réellement disponibles après cette feuille.
    - [ ] Modifier `docs/project/media-support-planning.md` pour conserver uniquement les questions encore ouvertes après les validations autonomes.
    - [ ] Modifier `docs/project/hard-disk-format-catalog.md` avec l’état final vérifié de chaque complément HDD.

Après achèvement et validation de toutes les cases de cette feuille, créer le troisième commit demandé
par l’utilisateur. Continuer ensuite seulement avec la première case de
[`validation.md`](validation.md), qui contient les tests manuels des images du corpus local puis les
validations matérielles.
