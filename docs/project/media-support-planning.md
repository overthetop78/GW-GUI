# Visualisation et exploration des images de supports

Le but premier est de permettre à GW GUI de visualiser graphiquement la structure des images de disquettes, de disques durs, de CD et DVD, ainsi que de cassettes et de bandes, puis d’explorer les dossiers et les fichiers qu’elles contiennent.

## Visualiseur

Le Visualiseur doit ouvrir une image, reconnaître son format, en extraire sa structure, puis afficher cette structure avec un graphique adapté au support.

Après l’ouverture de l’image, il doit trouver le traitement correspondant afin de déterminer :

- le format de l’image ;
- les données à transmettre à l’affichage ;
- le type d’affichage à utiliser pour présenter ces données.

### Traitement nécessaire avant l’affichage

Le fichier ouvert doit être dirigé vers le lecteur correspondant à son format. Ce lecteur doit fournir toutes les informations nécessaires pour identifier le support et construire son affichage : type de média, organisation des données, nombre de faces, pistes, secteurs, plateaux, bandes, couches ou autres éléments réellement décrits par le format.

La reconnaissance du format et le choix du lecteur doivent être réunis dans un orchestrateur commun. Celui-ci doit remplacer la détection séparée actuellement réalisée par `ImageFormatDetector`, `DiskImageRecognitionRegistry` et les cas particuliers traités dans l’application. Chaque lecteur doit déclarer les extensions et signatures qu’il reconnaît, la famille du média concerné et la représentation qu’il produit.

Le résultat de lecture doit distinguer explicitement une capture physique de flux d’une image de données organisée en secteurs ou en blocs. Une capture SCP doit conserver ses transitions, ses révolutions et ses pistes physiques. Une image sectorielle comme ADF, ST, MSA ou IMA doit conserver ses secteurs et sa géométrie logique. Elle ne doit plus être transformée en SCP synthétique pour être affichée.

Le résultat doit également indiquer le type de graphique à utiliser et sa méthode de remplissage : par piste, par secteur ou selon une autre unité adaptée au support. Il doit préciser l’ordre des éléments, leur point de départ, leur direction, ainsi que leur répartition entre les faces, plateaux, pistes, bandes ou couches.

Ces informations dépendent du format. Une image de disquette doit notamment fournir ses faces et ses pistes. Une image de disque dur doit fournir en priorité son organisation CHS ou LBA et le nombre de plateaux seulement lorsque cette information existe réellement. Une image de CD ou DVD doit décrire ses pistes, sessions, couches et faces disponibles. Une image de cassette ou de bande doit décrire ses faces, pistes, canaux, segments et sens de lecture lorsque son format les conserve. Les formats constitués de plusieurs fichiers doivent être réunis en une seule description cohérente du média.

### Lecture interne et conversion

La Lecture interne doit pouvoir transmettre directement son résultat décodé ou reconstruit aux convertisseurs internes. Lorsqu’un Writer interne sait produire le format choisi, la conversion doit utiliser les données déjà obtenues pendant la lecture, sans relire le fichier final et sans lancer `gw.exe`. Le fichier final reste la source utilisée lorsque l’utilisateur ouvre ensuite séparément le Visualiseur ou l’Explorateur.

Les services de conversion interne déjà présents doivent être raccordés à ce résultat commun. Le recours à un outil externe reste réservé aux couples source-cible qui ne disposent pas encore d’un Reader et d’un Writer internes compatibles.

### Répartition entre les DLL

Les fonctions doivent rester dans la DLL propriétaire de leur action :

- `GWGUI.Domain` contient les contrats neutres, demandes, résultats et capacités, sans lecture de fichier, accès matériel, conversion ni interface ;
- `GWGUI.MediaEngine` contient la reconnaissance des formats, les Readers, décodeurs, représentations, systèmes de fichiers, explorateurs, Writers et conversions internes ;
- `GWGUI.Infrastructure` contient l’accès aux appareils, à Windows, aux processus et aux outils externes, sans interpréter les formats ni choisir un rendu ;
- `GWGUI.App` contient les vues, présentateurs et coordinateurs d’interface, sans algorithme de reconnaissance, décodage, exploration, écriture ou conversion.

Les fichiers doivent suivre la même séparation à l’intérieur de chaque projet. Un registre sélectionne ses composants, une politique décide si elle accepte une source, un Reader lit un format, un Writer produit un format et un convertisseur coordonne une transformation. Ces responsabilités ne doivent pas être regroupées dans un contrôleur d’interface ou une factory générale.

### Structure hybride cible de MediaEngine

`GWGUI.MediaEngine` doit employer une structure hybride. Les types véritablement partagés sont rangés par nature à la racine du projet. Les implémentations propres à un format sont rangées par support, puis directement par format. Une famille de machines ou une variante ne crée un niveau supplémentaire que lorsqu’elle possède réellement plusieurs fichiers spécialisés.

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

La composition doit être divisée par fonction ou famille de médias afin de remplacer la factory générale actuelle. Elle enregistre les composants disponibles, mais ne contient aucun algorithme de format. L’ajout d’un format consiste ainsi à ajouter ses composants et leur enregistrement, sans modifier les services généraux ni ajouter de nouvelle chaîne conditionnelle dans l’application.

Les modules d’émulation restent responsables des décisions propres à leurs émulateurs. Le module Amiga décide qu’une capture SCP doit devenir un ADF temporaire et gère le nom ainsi que le cache de ce fichier. Le module Atari choisit ATR ou ST selon la famille de machine et gère son média de session. Les Readers, décodeurs, transformations et Writers utilisés pour produire ces fichiers restent dans `GWGUI.MediaEngine`. Après le remplacement de la factory générale, les modules changent seulement leur appel vers le nouveau point d’entrée commun de MediaEngine; cette adaptation ne justifie pas de restructurer les projets d’émulation ni d’ajouter un contrat au SDK tant qu’aucun besoin public distinct n’est établi.

Pour les supports physiques futurs, les pilotes et appels aux appareils restent dans `GWGUI.Infrastructure`. `GWGUI.MediaEngine` reçoit une acquisition neutre, la décode en `MediaImageDocument`, ou transforme un document en `MediaWritePlan`. Cette séparation permet d’ajouter ultérieurement des lecteurs et Writers physiques de disquettes, disques durs, supports optiques ou bandes sans intégrer du code matériel dans le moteur de formats.

### Représentations visuelles distinctes

Le Visualiseur doit choisir le rendu à partir de la représentation réellement fournie par le lecteur :

- une vue de flux pour une capture comme SCP, fondée sur les transitions, les révolutions et les pistes physiques réellement enregistrées ;
- une vue sectorielle pour une image de données comme ADF, ST, MSA ou IMA, fondée sur les faces, les pistes, les secteurs et leur contenu logique ;
- plus tard, les vues propres aux blocs de disques durs, aux pistes et sessions optiques, ainsi qu’aux segments temporels des cassettes et bandes.

La vue sectorielle ne doit pas simuler un flux qui n’existe pas dans le fichier. Les deux vues peuvent partager le contrôleur de préparation progressive, la sélection, le zoom et les interactions communes, mais elles doivent conserver leurs données et leur moteur de rendu propres.

### Fonctionnement actuel pour les disquettes

Le fonctionnement actuel ne fournit pas encore cette description générale :

- la reconnaissance des formats est répartie entre `ImageFormatDetector`, utilisé par certains onglets, et les politiques de reconnaissance de `GWGUI.MediaEngine`, utilisées pour charger le Visualiseur ;
- les images sectorielles sont représentées par `SectorImage`, qui fournit des cylindres, des têtes, des secteurs par piste et des blocs, mais ne décrit pas le type de graphique à utiliser ;
- les captures SCP fournissent leurs pistes, leurs faces et leurs révolutions, mais elles sont traitées par un chemin particulier ;
- `DiskImageWorkspaceController` choisit lui-même le rendu : un SCP est chargé directement, tandis qu’une image sectorielle est transformée en SCP synthétique avec `SectorImageFluxVisualizer` ;
- le Visualiseur utilise donc toujours `ScpDiskView`, même lorsque le fichier d’origine ne contient aucun flux ;
- le type de média transmis au rendu ne couvre que les catégories de disquettes utilisées pour dessiner leur enveloppe ;
- les pistes sont ordonnées par cylindre croissant et dessinées de l’extérieur vers l’intérieur, la piste zéro étant placée à l’extérieur ;
- il n’existe aucun résultat commun indiquant au Visualiseur le support, le graphique, l’unité de remplissage, le nombre de surfaces et leur ordre.

### Préparation progressive de l’affichage

Actuellement, le fichier image est entièrement lu et transformé en données internes par `GWGUI.MediaEngine`. `GWGUI.App` prépare ensuite progressivement le rendu SCP, piste par piste, avec `SkiaScpRenderer`. Cette progression est donc liée au visualiseur SCP et à son moteur de rendu.

Avant d’étendre le Visualiseur aux autres représentations et aux autres supports, ce fonctionnement doit être réorganisé. Le contrôleur du Visualiseur doit piloter une préparation progressive commune, tandis que chaque visualiseur fournit les éléments graphiques propres au format et au support concernés. `SkiaScpRenderer` doit rester responsable du dessin du rendu SCP, sans porter à lui seul le principe général de progression.

Cette réorganisation concerne le contrôleur du Visualiseur, le contrat utilisé par les moteurs de rendu, la préparation et le cache de `SkiaScpRenderer`, ainsi que les tests du chargement, de l’annulation, du zoom et de la sélection. Elle ne demande pas de refaire la lecture des formats ni l’ensemble de l’interface.

### Question à définir

- Comment représenter graphiquement les images de disquettes, de cassettes et bandes, de disques durs, ainsi que de CD et DVD ?

## Explorateur

L’Explorateur doit ouvrir une image, reconnaître son format, extraire son système de fichiers, puis afficher son contenu sous forme de dossiers et de fichiers.
