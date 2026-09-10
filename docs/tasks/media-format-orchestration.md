# Orchestration commune des formats et des représentations de médias

La finalité et la structure retenue sont décrites dans
[`../project/media-support-planning.md`](../project/media-support-planning.md). Cette feuille doit être
exécutée avant [`media-exploration.md`](media-exploration.md).

Seul `GWGUI.MediaEngine` doit être rangé. Les autres projets ne doivent pas être réorganisés. Leurs
fichiers ne sont modifiés que lorsqu’une action ci-dessous nomme précisément un point d’intégration.
Les tâches sont exécutées et cochées une par une, dans l’ordre. Toute action découverte doit être
ajoutée après la dernière action cochée et avant son exécution, conformément à `.codex/config.toml`.

- [ ] 1. Préparer le rangement exact de `GWGUI.MediaEngine`
  - [ ] 1.1 Établir la correspondance complète entre la structure actuelle et la structure hybride
    - [ ] Créer `docs/architecture/media-engine-file-layout.md` avec l’inventaire de chaque fichier C# de `src/GWGUI.MediaEngine`, sa responsabilité, son chemin actuel, son chemin cible exact, les définitions qu’il contient et les doublons seulement suspectés ou confirmés.
    - [ ] Modifier `docs/architecture/media-engine-file-layout.md` pour décrire les dossiers racine `Constants`, `Enums`, `Exceptions`, `Functions`, `Interfaces`, `Primitives` et `Contracts`, puis les domaines `Formats`, `Recognition`, `Reading`, `Writing`, `Acquisition`, `Decoding`, `Encoding`, `Reconstruction`, `Conversion`, `Representations`, `FileSystems`, `Exploration`, `Visualization` et `Composition`.
    - [ ] Modifier `docs/architecture/media-engine-file-layout.md` pour appliquer la règle support puis format sous `Formats`, sans niveau constructeur ou machine lorsque le format suffit à l’identifier.
    - [ ] Modifier `docs/architecture/media-engine-file-layout.md` pour interdire les dossiers vides, les dossiers artificiels à un seul fichier et les sous-dossiers de catégorie qui ne regroupent pas plusieurs fichiers ou une extension déjà définie.
  - [ ] 1.2 Transformer l’inventaire en actions exécutables avant le premier déplacement
    - [ ] Modifier `docs/tasks/media-format-orchestration.md` en insérant immédiatement après le point 1 une sous-tâche distincte pour chaque fichier à déplacer, renommer ou supprimer, avec son chemin source, son chemin cible et les modifications de namespace attendues.
    - [ ] Modifier `docs/tasks/media-format-orchestration.md` en insérant après chaque groupe de déplacements les modifications exactes des fichiers consommateurs dont les `using`, noms de types ou appels doivent suivre ces déplacements.
    - [ ] Modifier `docs/tasks/media-format-orchestration.md` en ajoutant après la dernière action de rangement la suppression explicite de chaque dossier devenu vide, avec son chemin exact.

- [ ] 2. Installer les contrats communs sans déplacer les algorithmes spécialisés
  - [ ] 2.1 Référencer les contrats neutres
    - [ ] Modifier `src/GWGUI.MediaEngine/GWGUI.MediaEngine.csproj` pour référencer `src/GWGUI.Domain/GWGUI.Domain.csproj` sans ajouter de dépendance vers `GWGUI.App`, `GWGUI.Infrastructure` ou les projets d’émulation.
    - [ ] Créer `src/GWGUI.Domain/Enums/MediaKind.cs` avec les valeurs anglaises `Unknown`, `Floppy`, `HardDisk`, `Optical` et `Tape`.
    - [ ] Créer `src/GWGUI.Domain/Enums/MediaRepresentationKind.cs` avec les valeurs anglaises `Flux`, `Sectors`, `Blocks`, `OpticalTracks` et `Sequential`.
    - [ ] Créer `src/GWGUI.Domain/Contracts/MediaSourceDescriptor.cs` avec le chemin principal, les fichiers associés, la taille connue et le format explicitement demandé.
    - [ ] Créer `src/GWGUI.Domain/Contracts/MediaAcquisitionResult.cs` avec la famille du support, la représentation acquise, les données neutres et les diagnostics matériels sans type provenant de `GWGUI.Infrastructure`.
    - [ ] Créer `src/GWGUI.Domain/Contracts/MediaWritePlan.cs` avec la famille du support, les unités à écrire, leur ordre et les contraintes nécessaires au Writer physique sans type provenant de `GWGUI.MediaEngine`.
  - [ ] 2.2 Définir le document média interne
    - [ ] Créer `src/GWGUI.MediaEngine/Interfaces/IMediaImageRepresentation.cs` avec le type de représentation et les capacités communes en lecture seule.
    - [ ] Créer `src/GWGUI.MediaEngine/Representations/Flux/FluxMediaImageRepresentation.cs` pour conserver les surfaces, pistes, révolutions et transitions réellement présentes.
    - [ ] Créer `src/GWGUI.MediaEngine/Representations/Sectors/SectorMediaImageRepresentation.cs` pour envelopper une image sectorielle sans fabriquer de flux.
    - [ ] Créer `src/GWGUI.MediaEngine/Representations/Blocks/BlockMediaImageRepresentation.cs` avec des adresses et longueurs 64 bits sans imposer CHS ni plateaux.
    - [ ] Créer `src/GWGUI.MediaEngine/Representations/Optical/OpticalMediaImageRepresentation.cs` avec sessions, pistes, secteurs, couches, faces et fichiers associés uniquement lorsqu’ils sont décrits par la source.
    - [ ] Créer `src/GWGUI.MediaEngine/Representations/Sequential/SequentialMediaImageRepresentation.cs` avec faces, pistes, canaux, segments et chronologie uniquement lorsqu’ils sont décrits par la source.
    - [ ] Créer `src/GWGUI.MediaEngine/Contracts/MediaVolumeDescriptor.cs` avec la plage du volume, son origine, ses informations de partition ou session et son système de fichiers éventuel.
    - [ ] Créer `src/GWGUI.MediaEngine/Contracts/MediaImageDocument.cs` avec la source, le format reconnu, le type de média, la représentation, les volumes, les diagnostics et les métadonnées réellement disponibles.

- [ ] 3. Unifier la reconnaissance et la lecture des fichiers images
  - [ ] 3.1 Définir les Readers spécialisés et leur sélection
    - [ ] Créer `src/GWGUI.MediaEngine/Interfaces/Reading/IMediaImageReader.cs` avec l’identifiant du format, les extensions, les signatures, les fichiers associés, les types de médias et les représentations pris en charge.
    - [ ] Créer `src/GWGUI.MediaEngine/Recognition/MediaRecognitionContext.cs` avec une source partagée qui ne lit les mêmes octets qu’une seule fois et permet les lectures bornées nécessaires aux grandes images.
    - [ ] Créer `src/GWGUI.MediaEngine/Recognition/MediaRecognitionCandidate.cs` avec le Reader candidat, sa confiance et la raison de sa présélection.
    - [ ] Créer `src/GWGUI.MediaEngine/Recognition/MediaRecognitionResult.cs` avec le Reader retenu, le document produit et les échecs des autres candidats.
    - [ ] Créer `src/GWGUI.MediaEngine/Recognition/MediaRecognitionRegistry.cs` pour classer les Readers candidats puis appeler leur lecture complète sans contenir l’algorithme d’un format.
    - [ ] Créer `src/GWGUI.MediaEngine/Reading/MediaImageReadingService.cs` pour recevoir un `MediaSourceDescriptor`, appeler le registre et retourner un `MediaImageDocument`.
  - [ ] 3.2 Raccorder les formats de disquettes existants
    - [ ] Modifier `docs/tasks/media-format-orchestration.md` pour ajouter une action individuelle par Reader existant qui doit implémenter `IMediaImageReader`, avec son chemin exact déterminé par `docs/architecture/media-engine-file-layout.md`.
    - [ ] Modifier `src/GWGUI.MediaEngine/Recognition/DiskImageRecognitionRegistry.cs` pour devenir temporairement un adaptateur de la chaîne commune pendant la migration des consommateurs existants.
    - [ ] Modifier `src/GWGUI.MediaEngine/Exploration/DiskImageExplorer.cs` pour utiliser `MediaImageReadingService` sans branche de reconnaissance SCP fondée sur l’extension.
    - [ ] Supprimer `src/GWGUI.Domain/Formats/Detection/ImageFormatDetector.cs` après migration de tous ses consommateurs vers `MediaRecognitionRegistry`.
    - [ ] Supprimer `src/GWGUI.MediaEngine/Recognition/DiskImageRecognitionRegistry.cs` après migration de tous ses consommateurs vers `MediaRecognitionRegistry`.

- [ ] 4. Séparer clairement flux physique et données sectorielles
  - [ ] 4.1 Conserver les données réelles du format
    - [ ] Modifier `src/GWGUI.MediaEngine/Formats/Floppy/Scp/ScpReader.cs` après son déplacement inscrit au point 1 afin qu’il produise une représentation Flux conservant la capture SCP complète.
    - [ ] Modifier `src/GWGUI.MediaEngine/SectorImages/SectorImage.cs` pour exposer les informations sectorielles nécessaires à `SectorMediaImageRepresentation` sans dépendre du rendu SCP.
    - [ ] Créer `src/GWGUI.MediaEngine/Visualization/MediaVisualizationDescriptor.cs` avec le type de représentation, les surfaces, l’unité de progression, l’ordre et la direction des éléments à préparer.
    - [ ] Créer `src/GWGUI.MediaEngine/Interfaces/Visualization/IMediaVisualizationProvider.cs` pour produire un descripteur depuis une représentation compatible.
    - [ ] Créer `src/GWGUI.MediaEngine/Visualization/MediaVisualizationProviderRegistry.cs` pour sélectionner un fournisseur par représentation et capacités, sans condition sur l’extension.
  - [ ] 4.2 Retirer la fabrication de faux flux
    - [ ] Modifier `docs/architecture/media-engine-file-layout.md` pour réserver `SectorImageFluxVisualizer` et son dernier appel à la phase finale d’affichage, après le premier commit demandé, afin que la suppression du faux flux et l’ajout de la vue Sectors restent une seule transition compilable.

- [ ] 5. Généraliser l’exploration des volumes et systèmes de fichiers
  - [ ] 5.1 Remplacer les contrats limités aux disquettes
    - [ ] Créer `src/GWGUI.MediaEngine/Interfaces/Exploration/IMediaFileSystemReader.cs` pour sonder et lire un volume adressable indépendamment de son support.
    - [ ] Créer `src/GWGUI.MediaEngine/Exploration/MediaExplorer.cs` pour recevoir un `MediaImageDocument`, détecter ses volumes et construire leurs arborescences.
    - [ ] Créer `src/GWGUI.MediaEngine/Exploration/Results/ExploredMediaImage.cs` avec le document source, les volumes reconnus, les arborescences et les diagnostics.
    - [ ] Modifier `src/GWGUI.MediaEngine/FileSystems/IFileSystemReader.cs` pour servir temporairement d’adaptateur sectoriel vers `IMediaFileSystemReader`.
    - [ ] Modifier `src/GWGUI.MediaEngine/FileSystems/FileSystemRegistry.cs` pour enregistrer les Readers par capacités de volume plutôt que seulement par identifiant de format de disquette.
    - [ ] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour conserver un `ExploredMediaImage` commun au Visualiseur et à l’Explorateur.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Explorer/ExplorerSection.xaml.cs` pour recevoir les volumes et entrées du document commun.
    - [ ] Supprimer `src/GWGUI.MediaEngine/Exploration/Contracts/IImageDisquette.cs` après migration de tous ses consommateurs vers les contrats médias anglais.
    - [ ] Supprimer `src/GWGUI.MediaEngine/Exploration/Results/ExploredDiskImage.cs` après migration de tous ses consommateurs vers `ExploredMediaImage`.

- [ ] 6. Généraliser l’écriture et la conversion internes
  - [ ] 6.1 Sélectionner les Writers par capacité
    - [ ] Créer `src/GWGUI.MediaEngine/Interfaces/Writing/IMediaImageWriter.cs` avec le format cible, les représentations acceptées, les fichiers produits et les capacités d’écriture.
    - [ ] Créer `src/GWGUI.MediaEngine/Writing/MediaImageWriterRegistry.cs` pour sélectionner un Writer compatible sans contenir le code du format.
    - [ ] Créer `src/GWGUI.MediaEngine/Writing/MediaImageWritingService.cs` pour valider la représentation puis appeler le Writer retenu.
  - [ ] 6.2 Composer les transformations nécessaires
    - [ ] Créer `src/GWGUI.MediaEngine/Interfaces/Conversion/IMediaRepresentationConverter.cs` avec les représentations source et cible prises en charge.
    - [ ] Créer `src/GWGUI.MediaEngine/Conversion/MediaConversionRequest.cs` avec le document source, le format cible et les options explicitement demandées.
    - [ ] Créer `src/GWGUI.MediaEngine/Conversion/MediaConversionResult.cs` avec les fichiers produits, les diagnostics et les pertes déclarées.
    - [ ] Créer `src/GWGUI.MediaEngine/Conversion/MediaConversionService.cs` pour sélectionner les transformations puis le Writer sans chaîne conditionnelle par extension.
    - [ ] Modifier `src/GWGUI.App/Services/Conversion/ConversionBatchExecutor.cs` pour appeler uniquement `MediaConversionService` au lieu d’instancier chaque service de conversion spécialisé.
    - [ ] Modifier `src/GWGUI.App/Services/Parity/MediaEngineConversionSupport.cs` pour interroger les capacités des registres de Readers, convertisseurs et Writers.

- [ ] 7. Remplacer la factory générale par des compositions limitées
  - [ ] Créer `src/GWGUI.MediaEngine/Composition/MediaRecognitionComposition.cs` pour enregistrer uniquement les Readers et règles de reconnaissance.
  - [ ] Créer `src/GWGUI.MediaEngine/Composition/MediaExplorationComposition.cs` pour enregistrer uniquement les détecteurs de volumes et Readers de systèmes de fichiers.
  - [ ] Créer `src/GWGUI.MediaEngine/Composition/MediaConversionComposition.cs` pour enregistrer uniquement les convertisseurs de représentations.
  - [ ] Créer `src/GWGUI.MediaEngine/Composition/MediaWritingComposition.cs` pour enregistrer uniquement les Writers de fichiers images.
  - [ ] Créer `src/GWGUI.MediaEngine/Composition/MediaVisualizationComposition.cs` pour enregistrer uniquement les fournisseurs de données de visualisation.
  - [ ] Créer `src/GWGUI.MediaEngine/Composition/MediaEngineComposition.cs` pour assembler les compositions précédentes sans contenir d’algorithme de format.
  - [ ] Modifier `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml.cs` pour créer une seule composition MediaEngine et injecter ses services aux contrôleurs.
  - [ ] Supprimer `src/GWGUI.MediaEngine/Composition/MediaEngineFactory.cs` après migration de son dernier consommateur.

- [ ] 8. Adapter uniquement les points d’intégration des autres projets
  - [ ] 8.1 Conserver dans chaque module la décision propre à son émulateur
    - [ ] Modifier `src/GWGUI.Emulation.Amiga/Functions/AmigaRuntimeMediaFunctions.cs` pour remplacer uniquement l’appel à `MediaEngineFactory.CreateAmigaAdfConversionService()` par le point d’entrée de conversion commun obtenu après le point 7, tout en conservant dans le module la détection de SCP, le choix d’ADF, l’identité du cache et le chemin du fichier temporaire exigé par l’émulateur Amiga.
    - [ ] Modifier `src/GWGUI.Emulation.Atari/Functions/AtariScpMediaFunctions.cs` pour remplacer uniquement l’appel à `MediaEngineFactory.CreateAtariScpRuntimeConversionService()` par le point d’entrée de conversion commun obtenu après le point 7, tout en conservant dans le module le choix d’ATR ou de ST selon la famille de machine, la création du média de session et sa validation par l’émulateur Atari.
  - [ ] 8.2 Préserver les limites entre projets
    - [ ] Modifier `docs/architecture/media-engine-file-layout.md` pour expliquer que les algorithmes de lecture, décodage, transformation et écriture restent dans `src/GWGUI.MediaEngine`, tandis que les modules Amiga et Atari conservent seulement la décision du format accepté par leur émulateur et la gestion de leurs médias temporaires.
    - [ ] Modifier `docs/architecture/media-engine-file-layout.md` pour indiquer explicitement que `src/GWGUI.Emulation`, `src/GWGUI.Emulation.Amiga`, `src/GWGUI.Emulation.Atari`, `src/GWGUI.Infrastructure` et `src/GWGUI.App` ne sont pas restructurés avec MediaEngine et ne reçoivent que les adaptations de références ou d’appels nommées dans cette feuille.
    - [ ] Modifier `docs/architecture/media-engine-file-layout.md` pour consigner qu’aucun changement de contrat dans `src/GWGUI.Emulation` n’est prévu tant que l’orchestration commune reste une API de `GWGUI.MediaEngine`; si l’implémentation du point 7 rend réellement nécessaire un nouveau contrat public du SDK, ajouter d’abord les actions exactes correspondantes à cette feuille avant de modifier le projet.

- [ ] 9. Valider le socle avant l’ajout des autres médias

  Ce point commence seulement lorsque les points 1 à 8 sont entièrement terminés. Il contient les
  tests généraux autonomes de `GWGUI.Tests`, fondés sur des données simulées en mémoire et adaptés
  aux contrôles normaux du projet. Il n’utilise pas le corpus local `image_test`.

  - [ ] 9.1 Conserver les comportements actuels utiles
    - [ ] Créer `tests/GWGUI.Tests/MediaEngine/Recognition/MediaRecognitionRegistryTests.cs` avec des Readers simulés couvrant signature, extension, format demandé, ordre, fichiers associés, annulation et lecture partagée.
    - [ ] Créer `tests/GWGUI.Tests/MediaEngine/Reading/FloppyMediaReadingTests.cs` vérifiant qu’un SCP produit Flux et qu’ADF, ATR, ST, MSA et IMA produisent Sectors sans SCP synthétique.
    - [ ] Créer `tests/GWGUI.Tests/MediaEngine/Conversion/MediaConversionServiceTests.cs` vérifiant la sélection du Writer, la transformation nécessaire et la réutilisation du document déjà lu.
    - [ ] Créer `tests/GWGUI.Tests/MediaEngine/Exploration/MediaExplorerTests.cs` vérifiant qu’un même document alimente l’exploration et la visualisation sans seconde reconnaissance.
    - [ ] Créer `tests/GWGUI.Tests/Architecture/MediaEngineProjectBoundaryTests.cs` vérifiant les dépendances interdites entre Domain, MediaEngine, Infrastructure, App et les modules.
  - [ ] 9.2 Consigner les résultats réels
    - [ ] Modifier `docs/project/testing.md` avec les commandes et résultats obtenus après compilation de `GWGUI.MediaEngine`, `GWGUI.App`, `GWGUI.Emulation.Amiga`, `GWGUI.Emulation.Atari` et exécution des tests ciblés.
    - [ ] Modifier `docs/project/testing.md` avec le résultat de `scripts/build.ps1 -Configuration Debug` et la présence vérifiée de `build/Debug/GW GUI/gwgui.exe`.
    - [ ] Créer `docs/architecture/media-format-orchestration.md` avec la structure réellement obtenue, les registres, la chaîne Reader-document-Writer, les points d’intégration et les limites encore ouvertes.
