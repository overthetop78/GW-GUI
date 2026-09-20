# Visualisation et exploration des images de supports

Le but premier est de permettre à GW GUI de visualiser graphiquement la structure des images de disquettes, de disques durs, de CD et DVD, ainsi que de cassettes et de bandes, puis d’explorer les dossiers et les fichiers qu’elles contiennent.

## Visualiseur

Le Visualiseur doit ouvrir une image, reconnaître son format, en extraire sa structure, puis afficher cette structure avec un graphique adapté au support.

Après l’ouverture de l’image, il doit trouver le traitement correspondant afin de déterminer :

- le format de l’image ;
- les données à transmettre à l’affichage ;
- le type d’affichage à utiliser pour présenter ces données.

### Traitement nécessaire avant l’affichage

Le fichier ouvert est dirigé vers le Reader correspondant à son format. Ce Reader fournit les informations disponibles pour identifier le support et construire son affichage : type de média, organisation des données, nombre de faces, pistes, secteurs, bandes, couches ou autres éléments réellement décrits par le format.

La reconnaissance du format et le choix du Reader sont réunis dans `MediaRecognitionRegistry`. Chaque Reader déclare les extensions et signatures qu’il reconnaît, les fichiers associés, la famille du média concerné et la représentation qu’il produit. `GWGUI.App` utilise `MediaImageReadingService` et ne choisit plus un Reader à partir d'une condition propre à un format.

`MediaImageDocument` distingue explicitement une capture physique de flux, une image sectorielle, une image par blocs, une image à pistes optiques et un contenu séquentiel. SCP conserve ses transitions, ses révolutions et ses pistes physiques. ADF, ST, MSA ou IMA conservent leurs secteurs et leur géométrie logique et ne sont plus transformés en SCP synthétique pour être affichés.

`MediaVisualizationDescriptor` indique la représentation à afficher et sa méthode de progression : piste de flux, piste sectorielle, plage de blocs, piste optique ou segment séquentiel. Il précise l’ordre, la direction et la répartition entre les surfaces ou lignes réellement connues.

Ces informations dépendent du format. Une image de disquette fournit ses faces et ses pistes. Une image de disque dur fournit son espace logique LBA 64 bits et sa géométrie CHS seulement lorsqu'elle existe. Une image de CD ou DVD décrit ses pistes, sessions, couches et faces disponibles. Une image de cassette ou de bande décrit ses faces, pistes, canaux, segments et sens de lecture lorsque son format les conserve. Les formats constitués de plusieurs fichiers sont réunis dans une seule source cohérente.

### Lecture interne et conversion

La Lecture interne peut transmettre directement son résultat décodé ou reconstruit aux convertisseurs internes. Lorsqu’un Writer interne sait produire le format choisi, la conversion utilise le `MediaImageDocument` déjà obtenu, sans relire le fichier final et sans lancer `gw.exe`. Le fichier final reste la source utilisée lorsque l’utilisateur ouvre ensuite séparément le Visualiseur ou l’Explorateur.

Les services de conversion internes sont raccordés à ce résultat commun. Le recours à un outil externe reste réservé aux couples source-cible qui ne disposent pas d’un Reader et d’un Writer internes compatibles.

### Répartition entre les DLL

Les fonctions doivent rester dans la DLL propriétaire de leur action :

- `GWGUI.MediaEngine` expose les contrats média nécessaires à `App` et contient la reconnaissance des formats, les Readers, décodeurs, représentations, Writers, conversions internes, lecture et écriture physiques, et l'orchestration de l'exploration ;
- `GWGUI.MediaFileSystems` déclare les contrats de média décodé qu'il reçoit de `MediaEngine`, lit les volumes, dossiers et fichiers, et construit les volumes cibles de la migration ;
- `GWGUI.MediaAnalysis` reconnaît les types et catégories des fichiers et fournit leurs identifiants d'icône et clés de traduction ;
- `GWGUI.Infrastructure` contient l’accès aux appareils, à Windows, aux processus et aux outils externes, sans interpréter les formats ni choisir un rendu ;
- `GWGUI.App` contient les vues, présentateurs et coordinateurs d’interface, sans algorithme de reconnaissance, décodage, exploration, écriture ou conversion.

Les fichiers doivent suivre la même séparation à l’intérieur de chaque projet. Un registre sélectionne ses composants, une politique décide si elle accepte une source, un Reader lit un format, un Writer produit un format et un convertisseur coordonne une transformation. Ces responsabilités ne doivent pas être regroupées dans un contrôleur d’interface ou une factory générale.

### Structure hybride cible de MediaEngine

`GWGUI.MediaEngine` emploie une structure hybride. Les types véritablement partagés sont rangés par nature à la racine du projet. Les implémentations propres à un format sont rangées par support, puis directement par format. Une famille de machines ou une variante ne crée un niveau supplémentaire que lorsqu’elle possède réellement plusieurs fichiers spécialisés.

```text
GWGUI.MediaEngine/
├── Constants/
├── Enums/
├── Exceptions/
├── Functions/
├── Interfaces/
├── Primitives/
├── Contracts/
├── Formats/
│   ├── Floppy/
│   │   ├── Adf/
│   │   ├── Atr/
│   │   ├── CpcDsk/
│   │   ├── D64/
│   │   ├── D71/
│   │   ├── D81/
│   │   ├── DiskCopy/
│   │   ├── Hfe/
│   │   ├── I86f/
│   │   ├── Imd/
│   │   ├── Msa/
│   │   ├── Nib/
│   │   ├── Raw/
│   │   ├── Scp/
│   │   ├── St/
│   │   ├── TeleDisk/
│   │   ├── TwoImg/
│   │   └── Woz/
│   ├── HardDisk/
│   ├── Optical/
│   └── Tape/
├── Recognition/
├── Reading/
├── Writing/
├── Acquisition/
├── Decoding/
├── Encoding/
├── Reconstruction/
├── Conversion/
├── Representations/
├── FileSystems/
├── Exploration/
├── Visualization/
└── Composition/
```

Les dossiers racine `Constants`, `Enums`, `Exceptions`, `Functions`, `Interfaces`, `Primitives` et `Contracts` ne reçoivent que les éléments utilisés par plusieurs formats ou plusieurs fonctions. Un élément propre à un format reste dans le dossier de ce format. Un sous-dossier local comme `Constants`, `Enums`, `Readers` ou `Writers` n’est créé que s’il rassemble plusieurs fichiers ou si une extension déjà définie le justifie. Aucun dossier ne doit être créé pour contenir artificiellement un seul fichier.

Un format de taille limitée conserve directement ses fichiers :

```text
Formats/Floppy/Atr/
├── AtrFormat.cs
├── AtrLayout.cs
├── AtrReader.cs
├── AtrWriter.cs
├── AtrConstants.cs
└── AtrExceptions.cs
```

Un format plus important peut être découpé localement lorsque le nombre de fichiers le nécessite :

```text
Formats/Floppy/Scp/
├── Constants/
├── Enums/
├── Interfaces/
├── Readers/
├── Writers/
├── ScpHeader.cs
├── ScpImage.cs
├── ScpTrack.cs
└── ScpRevolution.cs
```

Les formats bruts compatibles avec plusieurs machines utilisent des profils ou variantes sans dupliquer leur Reader et leur Writer communs :

```text
Formats/Floppy/Raw/
├── RawImageReader.cs
├── RawImageWriter.cs
├── RawImageFormat.cs
└── Profiles/
    ├── AtariStProfile.cs
    ├── IbmPcProfile.cs
    └── MsxProfile.cs
```

Les systèmes de fichiers restent indépendants des formats d’images. FAT, AmigaDOS, Atari DOS, ISO 9660 ou UDF sont rangés sous `FileSystems`, car un système de fichiers peut être contenu dans plusieurs formats et un format peut accepter plusieurs systèmes de fichiers.

### Orchestration des traitements

Il ne doit pas exister un Reader, Writer, convertisseur ou décodeur généraliste contenant les traitements de tous les formats. Chaque format conserve ses composants spécialisés et les services communs restent courts : ils sélectionnent le composant compétent, lui transmettent les données et retournent son résultat.

La chaîne de lecture doit fonctionner ainsi :

1. `MediaRecognitionRegistry` examine les signatures, extensions, fichiers associés et contraintes déclarées par les formats.
2. Il sélectionne un `IMediaImageReader` compatible sans contenir lui-même le code de lecture du format.
3. Le Reader spécialisé, par exemple `AtrReader`, `ScpReader` ou `IsoReader`, lit et valide son format.
4. Le Reader retourne un `MediaImageDocument` contenant la représentation réelle du média : flux, secteurs, blocs, pistes optiques ou séquence temporelle.
5. Le même document est transmis à la visualisation, à l’exploration, à la conversion ou à la préparation d’une écriture.

```text
Fichier image
    ↓
MediaRecognitionRegistry
    ↓
Reader spécialisé du format
    ↓
MediaImageDocument
    ├──→ fournisseur de visualisation
    ├──→ détection du volume et système de fichiers → Explorateur
    ├──→ transformation éventuelle → Writer du format cible
    └──→ préparation d’un MediaWritePlan
```

Le registre ne lit pas lui-même les formats. Le service général ne contient pas les algorithmes des composants qu’il coordonne. Chaque Reader, Writer, décodeur, encodeur, reconstructeur, explorateur de système de fichiers et fournisseur de visualisation reste une implémentation séparée, enregistrée avec ses capacités.

La chaîne d’écriture suit le même principe. `MediaImageWriterRegistry` sélectionne le Writer annoncé pour le format cible. Le Writer spécialisé produit le fichier et reste seul responsable de sa structure, de ses contrôles et de ses fichiers associés.

`MediaConversionService` ne doit pas contenir un grand ensemble de conditions par extension. Il obtient le document auprès du Reader, vérifie la représentation demandée par le Writer, applique uniquement les transformations nécessaires par des convertisseurs enregistrés, puis appelle le Writer. Une conversion directe réutilise le document déjà présent en mémoire.

Les décodeurs, encodeurs et reconstructeurs suivent également un système de registres. Le registre choisit une implémentation selon l’encodage et les capacités demandées ; l’algorithme reste dans la classe spécialisée. Un algorithme réellement commun, comme une lecture binaire, un checksum ou une opération MFM partagée, est extrait dans `Functions`, `Decoding` ou `Encoding` au lieu d’être recopié dans chaque format.

L’Explorateur reçoit le document média, détecte ses volumes, puis confie chaque volume à un Reader de système de fichiers. Le Visualiseur choisit son fournisseur à partir de la représentation du document et non de l’extension du fichier. Les rendus Skia et les vues WPF restent dans `GWGUI.App` ; `GWGUI.MediaEngine` fournit seulement les données structurées nécessaires au rendu.

La composition est divisée par fonction : reconnaissance, exploration, conversion, écriture, visualisation, décodage SCP et codecs séquentiels. Elle enregistre les composants disponibles, mais ne contient aucun algorithme de format. L’ajout d’un format consiste à ajouter ses composants et leur enregistrement, sans modifier les services généraux ni ajouter de nouvelle chaîne conditionnelle dans l’application. `MediaEngineFactory` subsiste seulement pour les consommateurs de compatibilité qui n'ont pas encore été retirés.

Les modules d’émulation restent responsables des décisions propres à leurs émulateurs. Le module Amiga décide qu’une capture SCP doit devenir un ADF temporaire et gère le nom ainsi que le cache de ce fichier. Le module Atari choisit ATR ou ST selon la famille de machine et gère son média de session. Les Readers, décodeurs, transformations et Writers utilisés pour produire ces fichiers restent dans `GWGUI.MediaEngine`. Après le remplacement de la factory générale, les modules changent seulement leur appel vers le nouveau point d’entrée commun de MediaEngine; cette adaptation ne justifie pas de restructurer les projets d’émulation ni d’ajouter un contrat au SDK tant qu’aucun besoin public distinct n’est établi.

Pour les supports physiques futurs, les pilotes et appels aux appareils restent dans `GWGUI.Infrastructure`. `GWGUI.MediaEngine` reçoit une acquisition neutre, la décode en `MediaImageDocument`, ou transforme un document en `MediaWritePlan`. Cette séparation permet d’ajouter ultérieurement des lecteurs et Writers physiques de disquettes, disques durs, supports optiques ou bandes sans intégrer du code matériel dans le moteur de formats.

### Représentations visuelles distinctes

Le Visualiseur choisit le rendu à partir de la représentation réellement fournie par le Reader :

- une vue de flux pour une capture comme SCP, fondée sur les transitions, les révolutions et les pistes physiques réellement enregistrées ;
- une vue sectorielle pour une image de données comme ADF, ST, MSA ou IMA, fondée sur les faces, les pistes, les secteurs et leur contenu logique ;
- une vue par plages de blocs pour les disques durs ;
- une vue par pistes et sessions pour les médias optiques ;
- une vue temporelle par segments et lignes pour les cassettes et bandes.

La vue sectorielle ne doit pas simuler un flux qui n’existe pas dans le fichier. Les deux vues peuvent partager le contrôleur de préparation progressive, la sélection, le zoom et les interactions communes, mais elles doivent conserver leurs données et leur moteur de rendu propres.

### Fonctionnement actuel pour les disquettes

Le fonctionnement actuel fournit deux parcours distincts :

- `FluxMediaImageRepresentation` conserve les captures, faces, pistes et révolutions d'un vrai flux ;
- `SectorMediaImageRepresentation` conserve les cylindres, faces, pistes, secteurs, tailles et états logiques d'une image de données ;
- `FluxMediaVisualizationProvider` et `SectorMediaVisualizationProvider` créent des descripteurs distincts ;
- `DiskImageWorkspaceController` sélectionne `ScpDiskView` ou `SectorMediaView` à partir de la représentation ;
- `SkiaScpRenderer` et `SkiaSectorMediaRenderer` dessinent leurs données propres ;
- aucune image sectorielle n'est convertie en faux SCP pour l'affichage.

### Préparation progressive de l’affichage

Le fichier image est entièrement lu et transformé en données internes par `GWGUI.MediaEngine`. Le contrôleur commun prépare ensuite les éléments du descripteur progressivement. Chaque vue conserve son moteur de rendu, son cache, sa sélection et ses interactions. La progression ne dépend donc plus exclusivement de `SkiaScpRenderer`.

### Choix graphiques de base

Les cinq représentations partagent le chargement progressif, le zoom, la sélection, la légende et
le panneau d’informations. Chaque rendu conserve toutefois une géométrie propre aux données qu’il
reçoit.

#### Flux

Une capture de flux de disquette utilise une surface circulaire par face. Les pistes concentriques
sont remplies progressivement dans l’ordre fourni par la source. Le rendu distingue les
révolutions, la densité des transitions, les structures décodées et les anomalies réellement
présentes. Une zone non décodée, une zone sans transition et une erreur déclarée utilisent des
états visuels différents. Les informations détaillées restent accessibles par face, piste et
révolution.

#### Sectors

Une image sectorielle de disquette utilise également une surface circulaire par face, mais chaque
piste est divisée selon ses secteurs réels. La couleur décrit uniquement un état logique connu :
secteur présent, absent, illisible dans l’image, réservé, libre, occupé ou associé à un fichier
lorsque le système de fichiers fournit cette relation. Le rendu n’invente ni transitions
magnétiques, ni révolutions, ni défaut physique.

#### Blocks

Une image de disque dur utilise par défaut une carte logique de plages LBA agrégées. Elle montre les
partitions, volumes et zones réservées, allouées, libres ou inconnues sans créer un élément par
secteur. Une seconde vue en plateaux et surfaces est disponible seulement lorsqu’une géométrie CHS
fiable est fournie. Le nombre de plateaux physiques n’est jamais déduit d’une simple capacité.

#### OpticalTracks

Une image optique utilise un disque par face réellement décrite. Les bandes concentriques suivent
l’ordre des sessions, pistes et index ; leur nature audio ou données et leurs couches sont montrées
uniquement lorsque le format les fournit. Les grandes plages de secteurs sont agrégées pour garder
un dessin lisible. Les sélecteurs de face, couche et session n’apparaissent que lorsqu’ils ont un
effet.

#### Sequential

Une cassette ou une bande utilise une chronologie horizontale répartie sur plusieurs lignes. Les
faces, pistes et canaux deviennent des voies séparées lorsqu’ils existent. Les segments conservent
leur ordre et leur sens de lecture ; les silences, impulsions, blocs décodés, fichiers reconnus et
erreurs de décodage restent distincts. Une forme d’onde est affichée seulement lorsque la source
contient réellement des échantillons.

#### Informations absentes

Une propriété absente du document n’est ni estimée ni affichée comme certaine. L’interface masque
les sélecteurs inutiles et emploie un état « information indisponible » uniquement lorsqu’il aide à
comprendre le rendu. Les couleurs ne servent jamais à transformer une information logique en état
physique supposé.

## Explorateur

L’Explorateur ouvre la même image par `MediaImageReadingService`, détecte ses volumes puis demande à `FileSystemRegistry` le Reader de système de fichiers compatible. Il affiche les dossiers, fichiers, contenus et diagnostics sans relire le format dans l'interface. Les partitions MBR, EBR et GPT, les pistes optiques, ISO 9660, Joliet, Rock Ridge, UDF et les contenus séquentiels décodés sont raccordés au parcours commun.

## Questions encore ouvertes

Les décisions suivantes nécessitent les images réelles du corpus ou du matériel et restent donc à vérifier plus tard :

- quelles variantes réelles de RAW, QCOW2, VHD, VHDX, VDI, VMDK et CHD exigent encore des métadonnées ou des chemins de données supplémentaires ;
- quels systèmes de fichiers de disques durs doivent être ajoutés après les premiers essais réels ;
- quelles variantes de sessions, pistes, index, sous-canaux, couches et faces sont réellement présentes dans les images CD/DVD disponibles ;
- quelles extensions optiques autres qu’ISO 9660, Joliet, Rock Ridge et UDF sont nécessaires ;
- quels formats de cassettes ou bandes doivent exposer plusieurs faces, pistes ou canaux et comment leurs fichiers doivent être nommés dans l’Explorateur ;
- quels seuils d’agrégation, couleurs et niveaux de détail restent lisibles avec de très grandes images ;
- quels appareils physiques pourront être utilisés pour les disques durs, les supports optiques et les bandes, et quelles capacités réelles ils déclareront ;
- quelles conversions entraînent une perte acceptable de métadonnées physiques, optiques ou temporelles ;
- quels écarts apparaissent lors des essais manuels des corpus locaux de médias.
