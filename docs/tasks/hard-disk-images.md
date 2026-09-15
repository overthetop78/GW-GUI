# Images HDD — compléments après l’affichage des médias

Cette feuille commence seulement après l’achèvement de toutes les cases de
[`media-exploration.md`](media-exploration.md). Le deuxième commit a déjà été créé après la feuille
d’affichage. Les fonctions communes aux HDD, CD/DVD/optiques et cassettes/bandes sont réalisées à
ce stade par `media-exploration.md`; elles ne sont pas dupliquées ici. Cette feuille conserve les
compléments propres à la création, aux variantes et au cycle de vie des images HDD déjà recensés
dans [`../project/hard-disk-format-catalog.md`](../project/hard-disk-format-catalog.md).

La réconciliation effectuée après cette première feuille n'a révélé aucun complément automatisé
optique ou séquentiel à intercaler ici. Leurs inconnues dépendent d'images réelles ou de matériel
et restent consignées dans la documentation de planification. Les actions ci-dessous concernent
donc seulement les capacités, variantes, dépendances, publications et conversions propres aux HDD.

Les cases sont exécutées et cochées une par une, dans l’ordre. Avant de continuer, toute action
manquante est ajoutée après la dernière case cochée et avant l’action qui en dépend. Une action
imprécise est réécrite avec son fichier exact avant son exécution; une action devenue fausse ou sans
objet est supprimée.

- [x] 1. Réconcilier le travail restant après l’orchestration et l’affichage
  - [x] 1.1 Vérifier les limites entre les feuilles sans dupliquer les fonctions communes
    - [x] Modifier `docs/project/media-support-planning.md` avec l’état effectivement obtenu après `media-exploration.md` pour distinguer les capacités communes terminées des compléments HDD encore ouverts.
    - [x] Modifier `docs/tasks/hard-disk-images.md` avec une action concrète et un chemin exact pour tout complément HDD, optique ou séquentiel encore nécessaire après cette vérification, en plaçant chaque nouvelle action après la dernière case cochée et avant l’action qui en dépend.
  - [x] 1.2 Mettre le catalogue durable au niveau du code obtenu
    - [x] Modifier `docs/project/hard-disk-format-catalog.md` pour corriger l’état de chaque conteneur, ensemble d’images, table de partitions et système de fichiers à partir des implémentations réellement présentes après les deux premières feuilles.

- [x] 2. Compléter la description et la composition des images HDD
  - [x] 2.1 Exposer les capacités intrinsèques des formats
    - [x] Modifier `src/GWGUI.Emulation/HardDisks/HardDiskImageFormat.cs` pour exposer les tailles de secteurs, géométries, préparations, limites et opérations réellement acceptées par chaque format sans les déduire de son libellé.
    - [x] Modifier `src/GWGUI.Emulation/HardDisks/DiskFormatRegistry.cs` pour fournir ces capacités depuis les enregistrements de conteneurs, ensembles d’images, tables de partitions et systèmes de fichiers.
    - [x] Modifier `src/GWGUI.Emulation/HardDisks/DiskImagePlan.cs` pour valider une composition à partir des capacités déclarées par le registre avant toute création de fichier.
    - [x] Modifier `docs/project/hard-disk-format-catalog.md` avec les paramètres, limites, tailles de secteurs et géométries effectivement exposés par le registre.
  - [x] 2.2 Adapter uniquement les catalogues propres aux émulateurs
    - [x] Modifier `src/GWGUI.Emulation.Amiga/Functions/AmigaHardDiskFormats.cs` pour composer ses choix depuis les capacités communes tout en conservant uniquement les combinaisons acceptées par les émulateurs Amiga.
    - [x] Modifier `src/GWGUI.Emulation.Atari/Functions/AtariHardDiskFormats.cs` pour composer ses choix depuis les capacités communes tout en conservant uniquement les combinaisons acceptées par les émulateurs Atari.
    - [x] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/HardDiskDriveConfigurationDialog.cs` pour afficher et valider les paramètres fournis par `HardDiskImageFormat` sans recréer les règles des formats dans l’interface.
    - [x] Modifier `docs/project/hard-disk-format-catalog.md` avec les combinaisons Amiga et Atari réellement vérifiées après ces adaptations.

- [x] 3. Vérifier les variantes historiques avant toute extension
  - [x] 3.1 Définir chaque variante avant de modifier son implémentation
    - [x] Modifier `docs/project/hard-disk-format-catalog.md` avec les références, licences et capacités réelles de chaque variante encore absente avant de décider de l’intégrer.
    - [x] Modifier `docs/tasks/hard-disk-images.md` après cette étude pour ajouter une action séparée avec chemin exact pour chaque constructeur, Reader, Writer ou formateur retenu et supprimer les variantes finalement sans objet.

- [x] 4. Compléter le cycle de vie des ensembles d’images HDD
  - [x] 4.1 Résoudre les parents et membres associés
    - [x] Créer `src/GWGUI.Emulation/HardDisks/DiskImageDependencyReference.cs` avec un contrat validé représentant une dépendance par chemin, UUID ou SHA-1 avant d'adapter le Reader et l'index.
    - [x] Modifier `src/GWGUI.Emulation/HardDisks/DiskImageDependencyReader.cs` pour lire les références par chemin, hash ou UUID réellement documentées pour les conteneurs retenus et exposer les UUID ou SHA-1 propres permettant à l'index de retrouver leurs parents.
    - [x] Modifier `src/GWGUI.Emulation/HardDisks/DiskImageDependencyIndex.cs` pour résoudre ces identités, détecter les cycles et conserver les ensembles segmentés comme une seule dépendance logique.
    - [x] Modifier `src/GWGUI.Emulation/HardDisks/DiskImageDependencyIndex.cs` pour transmettre le chemin de l'image enfant au résolveur d'inventaire afin de limiter la recherche aux images voisines pertinentes.
    - [x] Modifier `src/GWGUI.App/Services/Emulation/HardDiskDeletionService.cs` pour fournir à l'index l'inventaire des images voisines par UUID ou SHA-1 lors du contrôle avant suppression.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/HardDisks/DependencyResolutionTests.cs` avec les parents par chemin, hash et UUID ajoutés, les segments, les cycles, les absences et les références invalides.
    - [x] Modifier `docs/project/hard-disk-format-catalog.md` avec les dépendances effectivement prises en charge et leurs limites.
  - [x] 4.2 Publier un ensemble sans laisser de résultat partiel
    - [x] Modifier `src/GWGUI.Emulation/HardDisks/DiskImageSetPublication.cs` pour publier collectivement tous les membres attendus et refuser un ensemble incomplet.
    - [x] Modifier `src/GWGUI.Emulation/HardDisks/FileImageSetStorage.cs` pour préserver la destination existante et nettoyer uniquement les fichiers temporaires appartenant à l’opération interrompue.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/HardDisks/ImageSetPublicationTests.cs` avec les réussites, interruptions, collisions, membres manquants et nettoyages contrôlés.
  - [x] 4.3 Supprimer collectivement une image et ses membres
    - [x] Modifier `src/GWGUI.Emulation/HardDisks/DiskImageDependencyReader.cs` pour exposer séparément les fichiers membres d'un ensemble VMDK sans les confondre avec une image parente.
    - [x] Créer `src/GWGUI.App/Services/Emulation/HardDiskImageSetResolver.cs` pour établir la liste validée du point d'entrée et des membres VMDK ou sparsebundle qui doivent être supprimés ensemble.
    - [x] Modifier `src/GWGUI.App/Services/Emulation/HardDiskDeletionService.cs` pour établir la liste complète des membres et dépendants avant de proposer une suppression.
    - [x] Modifier `src/GWGUI.App/Services/Emulation/HardDiskDeletionFile.cs` pour supprimer uniquement la liste validée et conserver les originaux lorsqu’une vérification préalable échoue.
    - [x] Modifier `src/GWGUI.App/Services/Emulation/HardDiskDeletionService.cs` pour utiliser l'ouverture collective qui referme les handles déjà acquis si un membre suivant ne peut pas être verrouillé.
    - [x] Modifier `src/GWGUI.App/Services/Emulation/HardDiskDeletionFile.cs` pour valider chaque handle collectif contre son chemin absolu sans tenter de rouvrir un fichier déjà verrouillé sans partage.
    - [x] Créer `tests/GWGUI.Tests/Emulation/HardDisks/HardDiskDeletionTests.cs` avec des fichiers temporaires autonomes couvrant ensemble complet, dépendant actif, membre absent, refus et échec avant suppression.
  - [x] 4.4 Convertir une image HDD sans mélanger la conversion avec l’application
    - [x] Sans objet — ne pas créer `src/GWGUI.MediaEngine/Conversion/HardDisk/HardDiskImageConversionService.cs` : `MediaConversionService` orchestre déjà les Readers et Writers de toutes les familles, et les Writers HDD copient les blocs par plages vers une publication atomique.
    - [x] Sans objet — ne pas modifier `src/GWGUI.MediaEngine/Composition/MediaConversionComposition.cs` : la composition commune reçoit déjà le registre contenant les sept Writers HDD réellement disponibles.
    - [x] Sans objet — ne pas modifier `src/GWGUI.App/Controllers/MainWindow/ConversionTabController.cs` : l'application appelle déjà `MediaConversionService` et n'interprète ni conteneur, ni partition, ni bloc HDD.
    - [x] Créer `tests/GWGUI.Tests/MediaEngine/HardDisk/HardDiskImageConversionTests.cs` avec des sources simulées couvrant copie par plages, géométrie connue ou inconnue, annulation, destination existante et absence de résultat partiel à travers le service commun.

- [x] 5. Valider les compléments sans utiliser le corpus local
  - [x] 5.1 Exécuter les tests généraux autonomes après stabilisation du code
    - [x] Modifier `docs/project/testing.md` avec la commande et le résultat des tests ciblés de `tests/GWGUI.Tests/Emulation/HardDisks` et `tests/GWGUI.Tests/MediaEngine/HardDisk`, sans utiliser les corpus locaux de médias.
  - [x] 5.2 Produire le build qui précède les essais manuels
    - [x] Modifier `docs/project/testing.md` avec le résultat de `scripts/local-building/build.ps1 -Configuration Debug` et la présence vérifiée de `build/Debug/GW GUI/gwgui.exe`.
  - [x] 5.3 Mettre à jour la documentation durable finale
    - [x] Modifier `docs/architecture/media-format-orchestration.md` avec les capacités HDD, optiques et séquentielles réellement disponibles après cette feuille.
    - [x] Modifier `docs/project/media-support-planning.md` pour conserver uniquement les questions encore ouvertes après les validations autonomes.
    - [x] Modifier `docs/project/hard-disk-format-catalog.md` avec l’état final vérifié de chaque complément HDD.

Après achèvement et validation de toutes les cases de cette feuille, créer le troisième commit demandé
par l’utilisateur. Continuer ensuite seulement avec la première case de
[`validation.md`](validation.md), qui contient les tests manuels des images du corpus local puis les
validations matérielles.
