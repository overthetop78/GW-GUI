# Extension aux disques durs, supports optiques, cassettes et bandes

Cette feuille commence seulement lorsque toutes les cases de
[`media-visualization.md`](media-visualization.md) sont cochées et que le deuxième commit demandé a
été créé. Elle réutilise le `MediaImageDocument`, les registres, les représentations communes et les
vues obtenus pendant les deux premiers chantiers. Les tâches sont exécutées et cochées une par une,
dans l’ordre.

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

Après achèvement et validation de toutes les cases de cette feuille, continuer avec la première case
non cochée de [`hard-disk-images.md`](hard-disk-images.md). Le troisième commit demandé est créé
après cette feuille complémentaire et avant les essais manuels du corpus `image_test`.
