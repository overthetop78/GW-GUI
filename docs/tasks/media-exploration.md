# Extension aux disques durs, supports optiques, cassettes et bandes

Cette feuille commence seulement lorsque toutes les cases de
[`media-format-orchestration.md`](media-format-orchestration.md) sont cochées. Elle réutilise le
`MediaImageDocument`, les registres et les représentations communes obtenus pendant ce premier
chantier. Les tâches sont exécutées et cochées une par une, dans l’ordre.

- [ ] 1. Définir les formats réellement pris en charge avant leur implémentation
  - [ ] 1.1 Inventorier les images de disques durs
    - [ ] Créer `docs/reference/hard-disk-image-formats.md` avec RAW/IMG, VHD, VHDX, VDI, VMDK, QCOW2 et CHD, leurs signatures, fichiers associés, adressages, partitions, allocations, compression, parents, capacités de lecture et capacités d’écriture réellement envisageables.
    - [ ] Modifier `docs/tasks/media-exploration.md` après cet inventaire pour ajouter une action distincte avec chemin exact pour chaque Reader, Writer et convertisseur HDD retenu au-delà du premier format RAW.
  - [ ] 1.2 Inventorier les images optiques
    - [ ] Créer `docs/reference/optical-image-formats.md` avec ISO, BIN/CUE, CCD/IMG/SUB, MDF/MDS et CHD, leurs signatures, fichiers associés, secteurs, sous-canaux, pistes audio ou données, sessions, couches, faces, capacités de lecture et capacités d’écriture réellement envisageables.
    - [ ] Modifier `docs/tasks/media-exploration.md` après cet inventaire pour ajouter une action distincte avec chemin exact pour chaque Reader, Writer et convertisseur optique retenu au-delà d’ISO et BIN/CUE.
  - [ ] 1.3 Inventorier les images de cassettes et bandes
    - [ ] Créer `docs/reference/tape-image-formats.md` avec WAV et les formats structurés retenus, leurs signatures, échantillons, impulsions, blocs, fichiers, silences, faces, pistes, canaux, sens de lecture, capacités de lecture et capacités d’écriture réellement envisageables.
    - [ ] Modifier `docs/tasks/media-exploration.md` après cet inventaire pour ajouter une action distincte avec chemin exact pour chaque Reader, Writer, décodeur et encodeur de cassette ou bande retenu au-delà de WAV.

- [ ] 2. Ajouter les images de disques durs
  - [ ] 2.1 Lire une première image HDD brute
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/HardDisk/Raw/RawHardDiskFormat.cs` avec les extensions, contraintes de taille et capacités déclarées sans identifier une image `.img` par sa seule extension.
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/HardDisk/Raw/RawHardDiskReader.cs` pour produire un `MediaImageDocument` contenant une représentation Blocks avec adresses 64 bits et géométrie CHS uniquement lorsqu’elle est fournie ou confirmée.
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/HardDisk/Raw/RawHardDiskWriter.cs` pour écrire une représentation Blocks brute lorsque toutes les plages nécessaires sont disponibles.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaRecognitionComposition.cs` pour enregistrer `RawHardDiskReader` après les Readers de disquettes pouvant employer l’extension `.img`.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaWritingComposition.cs` pour enregistrer `RawHardDiskWriter` avec ses capacités exactes.
  - [ ] 2.2 Détecter les partitions et volumes HDD
    - [ ] Créer `src/GWGUI.MediaEngine/Exploration/Partitioning/MbrVolumeDetector.cs` avec partitions primaires, chaîne EBR, limites 64 bits, boucles, chevauchements et entrées invalides retournées comme diagnostics.
    - [ ] Créer `src/GWGUI.MediaEngine/Exploration/Partitioning/GptVolumeDetector.cs` avec en-têtes principal et secondaire, CRC, GUID, plages utilisables et noms de partitions.
    - [ ] Créer `src/GWGUI.MediaEngine/Exploration/MediaVolumeDetectorRegistry.cs` pour sélectionner les détecteurs de volumes compatibles sans dépendre du Visualiseur.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaExplorationComposition.cs` pour enregistrer volume direct, MBR/EBR et GPT dans `MediaVolumeDetectorRegistry`.
  - [ ] 2.3 Visualiser un disque dur
    - [ ] Créer `src/GWGUI.MediaEngine/Visualization/Blocks/BlockVisualizationProvider.cs` pour produire les plages LBA, partitions, volumes et zones connues à partir de la représentation Blocks.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaVisualizationComposition.cs` pour enregistrer `BlockVisualizationProvider`.
  - [ ] 2.4 Explorer les fichiers d’un disque dur
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Explorer/ExplorerSection.xaml` pour présenter la liste des partitions et volumes avant leur arborescence.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Explorer/ExplorerSection.xaml.cs` pour charger à la demande le volume HDD sélectionné depuis `ExploredMediaImage`.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Explorer/ExplorerDetailsPanel.xaml.cs` pour afficher les informations de partition, de volume et d’espace disponibles.

- [ ] 3. Ajouter les images de CD, DVD et autres supports optiques
  - [ ] 3.1 Lire ISO et BIN/CUE
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/Optical/Iso/IsoFormat.cs` avec la structure monofichier et les capacités réellement prises en charge.
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/Optical/Iso/IsoReader.cs` pour produire une représentation OpticalTracks sans confondre ISO 9660 avec l’encodage de disquette ISO FM ou MFM.
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/Optical/Iso/IsoWriter.cs` seulement avec les variantes d’écriture validées dans `docs/reference/optical-image-formats.md`.
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/Optical/BinCue/CueSheetReader.cs` pour lire les fichiers référencés, pistes, index, modes et pregaps.
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/Optical/BinCue/BinCueReader.cs` pour réunir le descripteur CUE et ses fichiers BIN dans un seul `MediaImageDocument`.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaRecognitionComposition.cs` pour enregistrer `IsoReader` et `BinCueReader` avec leurs signatures et fichiers associés.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaWritingComposition.cs` pour enregistrer `IsoWriter` uniquement si son implémentation a été conservée après l’inventaire.
  - [ ] 3.2 Explorer ISO 9660 et UDF
    - [ ] Créer `src/GWGUI.MediaEngine/FileSystems/Iso9660/Iso9660FileSystemReader.cs` pour produire l’arborescence commune depuis un volume optique compatible.
    - [ ] Créer `src/GWGUI.MediaEngine/FileSystems/Iso9660/JolietExtensionReader.cs` pour appliquer les noms Joliet réellement présents.
    - [ ] Créer `src/GWGUI.MediaEngine/FileSystems/Iso9660/RockRidgeExtensionReader.cs` pour appliquer les informations Rock Ridge réellement présentes.
    - [ ] Créer `src/GWGUI.MediaEngine/FileSystems/Udf/UdfFileSystemReader.cs` avec les versions UDF explicitement retenues dans `docs/reference/optical-image-formats.md`.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaExplorationComposition.cs` pour enregistrer les Readers ISO 9660, Joliet, Rock Ridge et UDF.
  - [ ] 3.3 Visualiser la structure optique
    - [ ] Créer `src/GWGUI.MediaEngine/Visualization/Optical/OpticalVisualizationProvider.cs` pour produire sessions, pistes, secteurs, couches et faces uniquement lorsqu’ils sont décrits par le document.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaVisualizationComposition.cs` pour enregistrer `OpticalVisualizationProvider`.
  - [ ] 3.4 Explorer sessions, volumes et pistes
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Explorer/ExplorerSection.xaml` pour présenter les sessions et volumes optiques avant leurs arborescences.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Explorer/ExplorerSection.xaml.cs` pour parcourir le volume sélectionné et présenter séparément les pistes audio.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Explorer/ExplorerDetailsPanel.xaml.cs` pour afficher les informations de session, piste, couche et système de fichiers sans créer de fichiers fictifs.

- [ ] 4. Ajouter les images de cassettes et bandes
  - [ ] 4.1 Lire une première source audio WAV
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/Tape/Wav/WavTapeFormat.cs` avec les variantes PCM retenues et les capacités déclarées.
    - [ ] Créer `src/GWGUI.MediaEngine/Formats/Tape/Wav/WavTapeReader.cs` pour produire une représentation Sequential avec échantillons, canaux et chronologie sans inventer de blocs décodés.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaRecognitionComposition.cs` pour enregistrer `WavTapeReader` avec sa signature RIFF/WAVE.
  - [ ] 4.2 Décoder et explorer les contenus structurés
    - [ ] Créer `src/GWGUI.MediaEngine/Decoding/Sequential/SequentialDecoderRegistry.cs` pour sélectionner un décodeur selon le signal et la machine demandée sans intégrer leurs algorithmes au registre.
    - [ ] Créer `src/GWGUI.MediaEngine/Exploration/Sequential/SequentialContentVolumeDetector.cs` pour transformer les programmes, fichiers et blocs reconnus en volume explorable tout en conservant les segments inconnus comme diagnostics.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaExplorationComposition.cs` pour enregistrer `SequentialContentVolumeDetector`.
  - [ ] 4.3 Visualiser cassette et bande
    - [ ] Créer `src/GWGUI.MediaEngine/Visualization/Sequential/SequentialVisualizationProvider.cs` pour produire lignes, faces, pistes, canaux, segments et positions temporelles réellement disponibles.
    - [ ] Modifier `src/GWGUI.MediaEngine/Composition/MediaVisualizationComposition.cs` pour enregistrer `SequentialVisualizationProvider`.

- [ ] 5. Préparer les futurs appareils physiques sans déplacer leur code dans MediaEngine
  - [ ] 5.1 Généraliser l’acquisition physique
    - [ ] Créer `src/GWGUI.Domain/Interfaces/IMediaAcquisitionProvider.cs` avec les capacités du support et une sortie `MediaAcquisitionResult` sans type matériel concret.
    - [ ] Créer `src/GWGUI.Infrastructure/Hardware/Media/MediaAcquisitionProviderRegistry.cs` pour sélectionner un fournisseur par support et appareil.
    - [ ] Modifier `src/GWGUI.App/Controllers/MainWindow/ReadTabController.cs` pour appeler le registre d’acquisition puis transmettre son résultat au service de traitement de MediaEngine.
  - [ ] 5.2 Généraliser l’écriture physique
    - [ ] Créer `src/GWGUI.Domain/Interfaces/IMediaPhysicalWriter.cs` avec les capacités du support et une entrée `MediaWritePlan` sans type matériel concret.
    - [ ] Créer `src/GWGUI.Infrastructure/Hardware/Media/MediaPhysicalWriterRegistry.cs` pour sélectionner un Writer physique par support et appareil.
    - [ ] Modifier `src/GWGUI.App/Controllers/MainWindow/WriteTabController.cs` pour demander le plan à MediaEngine puis appeler le Writer physique sélectionné.
  - [ ] 5.3 Conserver Greaseweazle derrière les nouveaux contrats
    - [ ] Modifier `src/GWGUI.App/Services/PhysicalDiskReading/PhysicalDiskReadService.cs` après création de son adaptateur Infrastructure afin de ne plus construire directement une représentation propre à MediaEngine.
    - [ ] Modifier `src/GWGUI.App/Services/PhysicalDiskWriting/PhysicalDiskWriteService.cs` après création de son adaptateur Infrastructure afin de ne plus interpréter directement les pistes à écrire.
    - [ ] Modifier `docs/tasks/media-exploration.md` avant ces deux adaptations pour ajouter les chemins exacts des fichiers Infrastructure Greaseweazle à créer ou modifier après lecture de leur fonctionnement actuel.

- [ ] 6. Ajouter l’interface et toutes ses traductions
  - [ ] 6.1 Définir chaque texte avant traduction
    - [ ] Modifier `src/GWGUI.App/Resources/00-Base/Explorer.resx` avec les libellés invariants ou anglais nécessaires aux partitions, sessions, volumes, pistes et contenus séquentiels.
    - [ ] Modifier `docs/tasks/media-exploration.md` après définition des libellés de l’Explorateur pour ajouter une action séparée avec chemin exact pour chaque `Explorer.resx` de langue existant, toutes langues devant être traduites avec Argos avant validation de l’interface.

- [ ] 7. Valider chaque famille après son implémentation

  Ce point commence seulement lorsque les points 1 à 6 sont entièrement terminés. Il contient les
  tests généraux autonomes de `GWGUI.Tests`, créés ou adaptés après stabilisation du code et sans
  dépendance au corpus local `image_test`.

  - [ ] 7.1 Couvrir les Readers et structures sans matériel externe
    - [ ] Créer `tests/GWGUI.Tests/MediaEngine/HardDisk/RawHardDiskMediaTests.cs` avec des images minimales en mémoire couvrant adressage 64 bits, MBR, EBR, GPT, volumes directs et informations inconnues.
    - [ ] Créer `tests/GWGUI.Tests/MediaEngine/Optical/OpticalMediaTests.cs` avec ISO et BIN/CUE minimaux couvrant fichiers associés, sessions, pistes, ISO 9660 et informations absentes.
    - [ ] Créer `tests/GWGUI.Tests/MediaEngine/Tape/SequentialMediaTests.cs` avec WAV et segments minimaux couvrant chronologie, canaux, pistes, faces et données non décodées.
  - [ ] 7.2 Couvrir l’Explorateur
    - [ ] Créer `tests/GWGUI.Tests/Interface/ExplorerViews/OtherMediaExplorerScenarios.cs` avec sélection de partitions, sessions, volumes et contenus séquentiels.
  - [ ] 7.3 Consigner les résultats réels
    - [ ] Modifier `docs/project/testing.md` avec les commandes, fichiers d’essai et résultats obtenus pour HDD, optique et bande, en distinguant les validations automatisées des validations manuelles.
    - [ ] Modifier `docs/project/testing.md` avec le résultat de `scripts/build.ps1 -Configuration Debug` et la présence vérifiée de `build/Debug/GW GUI/gwgui.exe` après la dernière famille.
    - [ ] Modifier `docs/architecture/media-format-orchestration.md` avec les Readers, Writers, représentations, visualiseurs, systèmes de fichiers et limites réellement implémentés.
    - [ ] Modifier `docs/project/media-support-planning.md` pour remplacer les questions résolues par les décisions effectivement validées et conserver uniquement les inconnues restantes.

- [ ] 8. Finaliser l’affichage graphique de tous les supports

  Après achèvement et validation des points 1 à 7, créer le commit demandé par l’utilisateur avec
  tout le travail terminé jusque-là. Ce point de contrôle Git est explicitement autorisé par la
  demande du 10 septembre 2026. Commencer ensuite seulement les actions ci-dessous.

  - [ ] 8.1 Construire la présentation commune du Visualiseur
    - [ ] Modifier `docs/project/media-support-planning.md` pour consigner les choix graphiques réellement appliqués aux vues Flux, Sectors, Blocks, OpticalTracks et Sequential, sans présenter comme physique une information absente de l’image.
    - [ ] Créer `src/GWGUI.App/Contracts/ViewModels/Visualization/MediaInspectorModel.cs` avec le titre du support, ses sections d’informations, l’élément sélectionné et uniquement les propriétés fournies par le document média.
    - [ ] Créer `src/GWGUI.App/Contracts/ViewModels/Visualization/MediaInspectorSection.cs` avec un titre, une icône et une liste ordonnée de valeurs affichables.
    - [ ] Créer `src/GWGUI.App/Contracts/ViewModels/Visualization/MediaInspectorEntry.cs` avec le libellé, la valeur, l’unité éventuelle et le niveau d’information ou d’erreur.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/MediaInspectorPanel.xaml` avec des cartes de synthèse et de détails réutilisables par tous les supports.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/MediaInspectorPanel.xaml.cs` pour recevoir un `MediaInspectorModel` sans connaître le format du fichier ouvert.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml` pour organiser une zone de rendu principale, une barre d’outils courte, une légende, une progression et le panneau d’informations adaptable à la largeur disponible.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml.cs` pour sélectionner automatiquement la vue correspondant à Flux, Sectors, Blocks, OpticalTracks ou Sequential et conserver zoom, sélection et position entre les actualisations du même document.
    - [ ] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour transmettre le document et son descripteur à la vue sélectionnée sans décider du rendu d’après l’extension.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerHeaderSection.xaml` pour afficher le nom, le format, le support, la représentation et uniquement les sélecteurs utiles à la vue active.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerHeaderSection.xaml.cs` pour alimenter ces informations depuis le document et masquer les commandes incompatibles avec sa représentation.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerLegend.xaml` pour afficher une légende compacte composée d’éléments colorés et expliqués, propre à la représentation active.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerLegend.xaml.cs` pour construire la légende depuis le descripteur sans liste codée par extension.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/TrackProgressStrip.xaml` pour représenter une progression générique par pistes, secteurs, plages, pistes optiques ou segments.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/TrackProgressStrip.xaml.cs` pour recevoir l’unité, le total, l’avancement et l’état sans dépendre de `SkiaScpRenderer`.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTrackOverview.xaml` pour afficher dynamiquement les surfaces, plateaux, couches, faces, canaux ou lignes annoncés par le descripteur.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTrackOverview.xaml.cs` pour générer ces lignes de progression et synchroniser leur sélection avec la vue principale.

  - [ ] 8.2 Finaliser la vue d’une disquette en flux
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/ScpDiskView.xaml` pour présenter séparément les faces disponibles, la surface circulaire du flux, le zoom et la sélection d’une piste ou d’une révolution.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/ScpDiskView.xaml.cs` pour conserver la face sélectionnée, relier le pointeur aux pistes préparées et afficher les données de flux correspondantes dans `MediaInspectorPanel`.
    - [ ] Modifier `src/GWGUI.App/Rendering/Scp/SkiaScpRenderer.cs` pour préparer progressivement chaque piste et dessiner uniquement les transitions, révolutions, densités et anomalies réellement présentes dans la capture.
    - [ ] Modifier `src/GWGUI.App/Rendering/Scp/ScpTrackDrawingFunctions.cs` pour appliquer une palette lisible sur thèmes clair et sombre sans confondre absence de décodage, zone vide et erreur physique.
    - [ ] Modifier `src/GWGUI.App/Presenters/Visualization/ScpInspectorPresenter.cs` pour produire le modèle commun avec face, piste, révolution, durées, transitions, encodage détecté et structures décodées disponibles.
    - [ ] Modifier `src/GWGUI.App/Services/Visualization/ScpInspectorController.cs` pour alimenter `MediaInspectorPanel` dans la vue principale et dans la fenêtre détachée.
    - [ ] Modifier `src/GWGUI.App/Views/Windows/Visualization/ScpInspectorWindow.xaml` pour héberger `MediaInspectorPanel` avec le modèle de la sélection Flux.
    - [ ] Modifier `src/GWGUI.App/Views/Windows/Visualization/ScpInspectorWindow.xaml.cs` pour recevoir et actualiser le modèle commun de la sélection Flux.
    - [ ] Supprimer `src/GWGUI.App/Views/Controls/Visualization/ScpInspectorPanel.xaml` après migration de la vue principale et de la fenêtre détachée vers `MediaInspectorPanel`.
    - [ ] Supprimer `src/GWGUI.App/Views/Controls/Visualization/ScpInspectorPanel.xaml.cs` après suppression du contrôle XAML correspondant.

  - [ ] 8.3 Finaliser la vue d’une disquette sectorielle
    - [ ] Créer `src/GWGUI.App/Contracts/Rendering/Sectors/SectorMediaRenderModel.cs` avec les faces, pistes, secteurs, tailles, identifiants, positions, états connus et correspondance avec les fichiers lorsque l’Explorateur la fournit.
    - [ ] Créer `src/GWGUI.App/Rendering/Sectors/SkiaSectorMediaRenderer.cs` pour dessiner les faces comme des surfaces de pistes concentriques divisées en secteurs, avec l’ordre et la direction fournis par le descripteur.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/SectorMediaView.xaml` avec les faces disponibles, la carte sectorielle, le zoom et la sélection, sans commandes propres aux révolutions du flux.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/SectorMediaView.xaml.cs` pour relier la sélection d’un secteur à son adresse, sa taille, son état connu et son contenu logique dans `MediaInspectorPanel`.
    - [ ] Créer `src/GWGUI.App/Presenters/Visualization/SectorMediaInspectorPresenter.cs` pour construire les informations de face, piste et secteur sans déduire de défaut physique depuis une image de données.
    - [ ] Modifier `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` pour envoyer les images sectorielles à `SectorMediaView` et cesser de fabriquer un SCP synthétique.
    - [ ] Supprimer `src/GWGUI.MediaEngine/Visualization/SectorImageFluxVisualizer.cs` après suppression vérifiée de son dernier appel.

  - [ ] 8.4 Finaliser la vue d’une image de disque dur
    - [ ] Créer `src/GWGUI.App/Contracts/Rendering/Blocks/BlockMediaRenderModel.cs` avec les plages LBA, partitions, volumes, zones réservées, allouées, libres ou inconnues et la géométrie CHS uniquement lorsqu’elle est établie.
    - [ ] Créer `src/GWGUI.App/Rendering/Blocks/SkiaBlockMediaRenderer.cs` pour agréger les grandes plages sans créer un élément graphique par secteur et dessiner une carte logique sélectionnable.
    - [ ] Créer `src/GWGUI.App/Rendering/Blocks/SkiaHardDiskGeometryRenderer.cs` pour représenter plusieurs plateaux et surfaces seulement lorsque le document fournit une géométrie CHS exploitable.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/BlockMediaView.xaml` avec la carte logique et, lorsqu’elle existe, la vue CHS, ainsi que la sélection de partition, volume, plage ou surface.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/BlockMediaView.xaml.cs` pour choisir la vue disponible, synchroniser le zoom et alimenter `MediaInspectorPanel` avec les adresses et capacités connues.
    - [ ] Créer `src/GWGUI.App/Presenters/Visualization/BlockMediaInspectorPresenter.cs` pour présenter capacité, adressage, partitions, volumes, systèmes de fichiers et géométrie sans inventer le nombre de plateaux.

  - [ ] 8.5 Finaliser la vue d’une image de CD ou DVD
    - [ ] Créer `src/GWGUI.App/Contracts/Rendering/Optical/OpticalMediaRenderModel.cs` avec les faces, couches, sessions, pistes, index, plages de secteurs et la nature audio ou données réellement décrites.
    - [ ] Créer `src/GWGUI.App/Rendering/Optical/SkiaOpticalMediaRenderer.cs` pour dessiner un disque par face, répartir couches, sessions et pistes selon leur ordre réel et agréger les secteurs lorsque nécessaire.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/OpticalMediaView.xaml` avec le disque, les sélecteurs de face, couche et session disponibles, le zoom et la sélection de piste.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/OpticalMediaView.xaml.cs` pour synchroniser les sélecteurs avec le rendu et afficher les informations optiques connues dans `MediaInspectorPanel`.
    - [ ] Créer `src/GWGUI.App/Presenters/Visualization/OpticalMediaInspectorPresenter.cs` pour présenter face, couche, session, piste, index, mode, durée et volume associé sans déduire une structure absente.

  - [ ] 8.6 Finaliser la vue d’une image de cassette ou de bande
    - [ ] Créer `src/GWGUI.App/Contracts/Rendering/Sequential/SequentialMediaRenderModel.cs` avec les faces, pistes, canaux, segments, positions temporelles, silences, blocs décodés et la forme d’onde facultative.
    - [ ] Créer `src/GWGUI.App/Rendering/Sequential/SkiaSequentialMediaRenderer.cs` pour répartir la chronologie sur plusieurs lignes et voies, préparer progressivement les segments et conserver leur ordre ainsi que leur sens de lecture.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/SequentialMediaView.xaml` avec les lignes temporelles, les sélecteurs de face, piste ou canal disponibles, le zoom horizontal et la sélection d’un segment.
    - [ ] Créer `src/GWGUI.App/Views/Controls/Visualization/SequentialMediaView.xaml.cs` pour synchroniser défilement, zoom et sélection puis afficher les informations temporelles dans `MediaInspectorPanel`.
    - [ ] Créer `src/GWGUI.App/Presenters/Visualization/SequentialMediaInspectorPresenter.cs` pour présenter position, durée, canal, piste, type de segment, fichier ou bloc reconnu et erreurs de décodage disponibles.

  - [ ] 8.7 Harmoniser l’apparence et l’accessibilité
    - [ ] Modifier `src/GWGUI.App/Resources/ApplicationStyles.xaml` avec les styles communs des surfaces, sélecteurs, légendes, badges, cartes d’informations et états de sélection du Visualiseur sur thèmes clair et sombre.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/VisualizerTabSection.xaml` pour conserver une utilisation complète aux tailles minimale et maximale de la fenêtre, avec panneau d’informations repliable et défilement uniquement dans les zones nécessaires.
    - [ ] Modifier `src/GWGUI.App/Views/Controls/Visualization/MediaInspectorPanel.xaml` pour fournir ordre de tabulation, noms accessibles, contraste et lecture correcte des valeurs indisponibles.

  - [ ] 8.8 Ajouter tous les textes du Visualiseur
    - [ ] Modifier `src/GWGUI.App/Resources/00-Base/Visualizer.resx` avec les clés communes et les valeurs anglaises ou invariantes nécessaires aux cinq représentations, sans dupliquer CPU, CHS, LBA, CD, DVD ni les noms de formats.
    - [ ] Modifier `src/GWGUI.App/Resources/ar-SA/Visualizer.resx` avec les traductions arabes des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/cs-CZ/Visualizer.resx` avec les traductions tchèques des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/da-DK/Visualizer.resx` avec les traductions danoises des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/de-DE/Visualizer.resx` avec les traductions allemandes des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/el-GR/Visualizer.resx` avec les traductions grecques des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/en-US/Visualizer.resx` avec les textes anglais des nouvelles clés du Visualiseur qui ne sont pas déjà hérités de la base commune.
    - [ ] Modifier `src/GWGUI.App/Resources/es-ES/Visualizer.resx` avec les traductions espagnoles des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/fi-FI/Visualizer.resx` avec les traductions finnoises des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/fr-FR/Visualizer.resx` avec les traductions françaises des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/he-IL/Visualizer.resx` avec les traductions hébraïques des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/hu-HU/Visualizer.resx` avec les traductions hongroises des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/id-ID/Visualizer.resx` avec les traductions indonésiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/it-IT/Visualizer.resx` avec les traductions italiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/ja-JP/Visualizer.resx` avec les traductions japonaises des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/ko-KR/Visualizer.resx` avec les traductions coréennes des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/nb-NO/Visualizer.resx` avec les traductions norvégiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/nl-NL/Visualizer.resx` avec les traductions néerlandaises des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/pl-PL/Visualizer.resx` avec les traductions polonaises des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/pt-BR/Visualizer.resx` avec les traductions portugaises du Brésil des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/pt-PT/Visualizer.resx` avec les traductions portugaises du Portugal des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/ro-RO/Visualizer.resx` avec les traductions roumaines des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/ru-RU/Visualizer.resx` avec les traductions russes des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/sv-SE/Visualizer.resx` avec les traductions suédoises des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/th-TH/Visualizer.resx` avec les traductions thaïlandaises des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/tr-TR/Visualizer.resx` avec les traductions turques des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/uk-UA/Visualizer.resx` avec les traductions ukrainiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/vi-VN/Visualizer.resx` avec les traductions vietnamiennes des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/zh-Hans/Visualizer.resx` avec les traductions chinoises simplifiées des nouvelles clés du Visualiseur produites avec Argos.
    - [ ] Modifier `src/GWGUI.App/Resources/zh-Hant/Visualizer.resx` avec les traductions chinoises traditionnelles des nouvelles clés du Visualiseur produites avec Argos.

  - [ ] 8.9 Ajouter les tests généraux des vues
    - [ ] Créer `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationRoutingTests.cs` avec le routage de Flux, Sectors, Blocks, OpticalTracks et Sequential vers leur vue sans décision fondée sur l’extension.
    - [ ] Créer `tests/GWGUI.Tests/Interface/VisualizerViews/FloppyVisualizationTests.cs` avec la séparation des informations Flux et Sectors, la progression et la sélection simulées sans fichier du corpus local.
    - [ ] Créer `tests/GWGUI.Tests/Interface/VisualizerViews/OtherMediaVisualizationTests.cs` avec les sélections Blocks, OpticalTracks et Sequential, les informations absentes et les grandes plages simulées sans fichier du corpus local.
    - [ ] Créer `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationLayoutTests.cs` avec les tailles minimales, le panneau replié, les thèmes clair et sombre et les noms accessibles des commandes.

  - [ ] 8.10 Produire le build destiné à la vérification visuelle
    - [ ] Modifier `docs/project/testing.md` avec le résultat de `scripts/build.ps1 -Configuration Debug`, la présence vérifiée de `build/Debug/GW GUI/gwgui.exe` et les chemins manuels à ouvrir pour observer Flux, Sectors, Blocks, OpticalTracks et Sequential.

  Après achèvement et validation de toutes les cases du point 8, créer le second commit demandé par
  l’utilisateur avec cette base graphique. Continuer ensuite avec la première case non cochée de
  [`hard-disk-images.md`](hard-disk-images.md). Aucun essai manuel du corpus `image_test` ne commence
  avant l’achèvement de cette feuille complémentaire et le troisième commit qui la termine.
