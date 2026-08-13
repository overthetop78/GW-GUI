# Contrat de données de lecture et d'exploration des médias

## Objet du document

Ce document décrit :

1. l'architecture antérieure qui perdait l'association entre une machine, son format, ses secteurs et ses fichiers ;
2. le contrat commun désormais implémenté pour transporter toutes les interprétations sans les mélanger ;
3. les classes conservées et les contrats ajoutés ou enrichis.

Le contrat complet faisant foi est décrit dans `interface-image-disquette.fr.md`. Les sections intitulées « avant refactor » ci-dessous sont conservées uniquement pour expliquer l'origine des anomalies corrigées ; elles ne décrivent plus le comportement actuel.

Il ne s'agit pas de créer des interfaces graphiques dans la DLL. La DLL doit produire des données techniques structurées. L'application choisit ensuite une interprétation et traduit les identifiants pour l'affichage.

## Vocabulaire

- **Source** : fichier ouvert, par exemple SCP, ADF, ST, IMA ou IMG.
- **Conteneur** : organisation du fichier source. SCP est un conteneur de flux ; ADF et ST sont généralement des images sectorielles linéaires.
- **Données physiques** : pistes, faces, révolutions et intervalles de flux disponibles dans une capture comme SCP.
- **Interprétation** : résultat complet obtenu en essayant une machine et un format précis.
- **Image sectorielle** : secteurs reconstruits pour une interprétation précise.
- **Volume** : système de fichiers reconnu, son nom, ses dossiers et ses fichiers.
- **Interprétation affichée** : interprétation choisie par l'application. Le moteur ne déclare aucun format préférable aux autres.

## Architecture avant refactor (historique)

### Données SCP physiques existantes

Le modèle physique SCP est déjà correctement découpé et doit être conservé.

```text
ScpImage
├── Header : ScpHeader
│   ├── Version
│   ├── DiskType
│   ├── Revolutions
│   ├── StartTrack / EndTrack
│   ├── Heads
│   ├── Resolution
│   └── Checksum
├── Tracks[] : ScpTrack
│   ├── TrackNumber
│   ├── Cylinder
│   ├── Head
│   └── Revolutions[] : ScpRevolution
│       ├── IndexTimeTicks
│       ├── DeclaredFluxCount
│       ├── FluxIntervals[]
│       └── Origin
├── ChecksumValid
└── FileSize
```

Une piste SCP contient donc déjà un tableau de révolutions. Il n'est pas nécessaire de recréer cette partie.

### Image sectorielle existante

Chaque tentative de décodage produit déjà une `SectorImage` distincte.

```text
SectorImage
├── FormatId
├── BlockSize
├── Cylinders
├── Heads
├── SectorsPerTrack
├── BlockCount
├── Capacity
├── AvailableBlocks[] : SectorBlock
│   ├── LogicalBlock
│   ├── Address
│   ├── Data[]
│   ├── IntegrityValid
│   ├── Revolution
│   ├── Tag[]
│   ├── FormatCode
│   └── DiagnosticCode
└── MissingBlocks[]
```

Cette classe contient les secteurs d'une interprétation donnée. Deux formats reconnus sur le même SCP peuvent donc avoir deux `SectorImage` différentes.

### Volume et fichiers existants

Le résultat d'un lecteur de système de fichiers est déjà structuré.

```text
FileSystemVolume
├── Name
├── FileSystemId
├── Capacity
├── FreeBytes
├── FreeSpaceKnown
├── Created / Modified
├── Warnings[]
└── Entries[] : FileSystemEntry
    ├── Name
    ├── Kind
    ├── Size
    ├── Modified
    ├── Comment
    ├── RawAttributes
    ├── StorageReference
    ├── MetadataValid
    ├── Content[]
    └── Children[] : FileSystemEntry
```

Le tableau récursif `Entries` représente déjà les vrais dossiers et fichiers du volume.

### Résultat d'exploration actuellement public

```text
ExploredDiskImage
├── SourcePath
├── Image : SectorImage                 ← une seule image globale
├── Volume : FileSystemVolume           ← un seul volume global
├── FileSystemRecognized
├── DetectedFileSystems[]               ← plusieurs volumes détectés
│   └── ExploredFileSystem
│       ├── FormatId
│       ├── ReaderId
│       └── Volume : FileSystemVolume
├── DetectedImageFormatIds[]
├── PrimaryFormatId
└── Metadata : DiskImageMetadata
```

Le problème structurel est ici : chaque `ExploredFileSystem` contient son format et son volume, mais ne contient pas la `SectorImage` qui a produit ce volume.

### Résultat réel d'une lecture physique interne

La lecture physique interne ne sait actuellement produire qu'un fichier SCP. Elle appelle :

```text
InternalPhysicalDiskReader.ReadAsync(...)
└── PhysicalDiskReadService.ReadAsync(...)
    ├── PhysicalDiskFluxAcquisitionService.AcquireAsync(...)
    │   └── PhysicalDiskFluxAcquisition
    │       └── Image : ScpImage
    ├── ScpWriter.WriteAsync(...)
    ├── DecodeTracks(...)
    │   └── PhysicalDiskTrackDiagnostic[]
    └── DiskImageExplorer.ExploreAsync(fichierScp)
        └── ExploredDiskImage
```

L'objet réellement retourné est :

```text
PhysicalDiskReadResult
├── OutputPath
├── Acquisition : PhysicalDiskFluxAcquisition
│   └── Image : ScpImage
│       └── Tracks[]
│           └── Revolutions[]
├── TrackDiagnostics[]
└── Document : ExploredDiskImage
    ├── Image : SectorImage
    ├── Volume : FileSystemVolume
    │   └── Entries[]
    ├── DetectedFileSystems[]
    └── DetectedImageFormatIds[]
```

La liste des fichiers construite pendant cette lecture interne est donc actuellement dans :

```text
PhysicalDiskReadResult.Document.Volume.Entries[]
```

Les autres systèmes de fichiers détectés sont dans `Document.DetectedFileSystems[]`, mais ils ne conservent pas chacun leur propre `SectorImage`.

### Résultat réel d'une lecture physique avec `gw.exe`

Pour une sortie SCP, ADF, ST ou un autre format pris en charge, l'application lance une commande externe :

```text
GreaseweazleRunner.RunAsync(...)
└── GwExecutionResult
    ├── ExitCode
    ├── WasCancelled
    ├── Duration
    └── OutputLines[]
```

`gw.exe` écrit le fichier demandé sur le disque. `GwExecutionResult` ne contient ni `ScpImage`, ni `SectorImage`, ni volume, ni dossiers, ni fichiers.

Après une lecture externe vers ADF ou ST, aucune liste de fichiers n'est construite par l'opération de lecture elle-même. Elle sera construite seulement lorsque le fichier sera ouvert dans l'explorateur.

### Résultat réel de l'explorateur selon le moteur choisi

```text
Explorateur interne
└── DiskImageExplorer.ExploreAsync(source)
    └── ExploredDiskImage
        └── Volume.Entries[]

Explorateur avec gw.exe
├── gw.exe convertit la source vers un fichier sectoriel temporaire
└── DiskImageExplorer.ExploreAsync(fichierTemporaire)
    └── ExploredDiskImage
        └── Volume.Entries[]
```

Même lorsque l'option de l'explorateur est réglée sur `gw.exe`, ce n'est pas `gw.exe` qui fournit les fichiers. Il fournit l'image sectorielle temporaire ; `GWGUI.MediaEngine` analyse ensuite cette image et construit `ExploredDiskImage`.

### Différence actuelle entre moteur interne et `gw.exe`

| Opération | Interne | `gw.exe` |
|---|---|---|
| Lecture physique vers SCP | Oui ; retourne `PhysicalDiskReadResult`, incluant un `ExploredDiskImage` | Oui ; retourne seulement `GwExecutionResult` et écrit le SCP |
| Lecture physique vers ADF/ST | Non prise en charge actuellement | Oui ; écrit directement le fichier et retourne seulement `GwExecutionResult` |
| Liste de fichiers pendant la lecture | Oui pour la lecture SCP interne via `PhysicalDiskReadResult.Document` | Non |
| Ouverture dans l'explorateur | Analyse directe par `DiskImageExplorer` | Conversion temporaire par `gw.exe`, puis analyse par `DiskImageExplorer` |
| Type final reçu par l'explorateur | `ExploredDiskImage` | `ExploredDiskImage` également, après l'étape externe |

### Chemin SCP interne actuel

Pendant l'inspection, l'association est encore complète :

```text
ScpCandidateInspection
├── CandidateId
├── Image : SectorImage?
└── Matches[]
    ├── Match : ExploredFileSystem
    └── Image : SectorImage
```

Puis le classement transforme ce résultat en :

```text
ScpCandidateRanker.Result
├── BestRecognized : SectorImage?
├── BestFileSystem : ExploredFileSystem?
└── Detected[] : ExploredFileSystem
```

`Detected[]` ne conserve plus l'image associée. Le document final reçoit donc :

```text
une Image principale
+
plusieurs ExploredFileSystem sans leur Image
```

C'est la perte de données qui permet d'afficher, par exemple, la machine **Amiga** avec une capacité et un catalogue **FAT12 Atari ST**.

## Flux actuel simplifié

```text
Fichier source
   │
   ├── SCP ──> ScpImage ──> candidats Amiga / Atari ST / IBM PC / ...
   │                         │
   │                         ├── SectorImage Amiga ──> volume AmigaDOS
   │                         ├── SectorImage Atari ──> volume FAT12
   │                         └── SectorImage IBM ────> volume FAT12
   │
   └── ADF/ST/IMA/... ──> SectorImage ──> lecteur de système de fichiers

Résultat intermédiaire correct
   └── [(SectorImage, format, lecteur, volume), ...]

Classement actuel
   └── Image globale + [(format, lecteur, volume), ...]
                         ↑ l'image propre à chaque élément est perdue

Frontend
   └── essaie de reconstituer une association qui n'existe plus dans le contrat
```

## Proposition abandonnée (historique)

Les types `MediaExplorationResult`, `MediaSourceInfo`, `IMediaContainerData` et `DiskInterpretation` présentés dans cette section n'ont pas été retenus et ne font pas partie du contrat implémenté. Ils restent uniquement pour documenter une étape de conception. Le contrat réel est `IImageDisquette`, détaillé intégralement dans `interface-image-disquette.fr.md`.

### Vue d'ensemble

```text
MediaExplorationResult
├── Source : MediaSourceInfo
├── Container : IMediaContainerData
└── Interpretations[] : DiskInterpretation
```

Chaque élément de `Interpretations` est autonome et contient toutes les données nécessaires pour l'explorateur, la conversion et la visualisation sectorielle.

```text
DiskInterpretation
├── Id
├── SystemId
├── FormatId
├── Image : SectorImage
├── FileSystem : FileSystemInterpretation?
├── Metadata : DiskImageMetadata
└── Warnings[]

FileSystemInterpretation
├── ReaderId
├── Recognized
└── Volume : FileSystemVolume
```

La relation essentielle devient :

```text
une machine + un format
        │
        └── une SectorImage précise
                │
                └── un lecteur précis
                        │
                        └── un FileSystemVolume précis
                                └── ses dossiers et ses fichiers
```

### Contrat racine proposé

```csharp
public sealed record MediaExplorationResult
{
    public required MediaSourceInfo Source { get; init; }
    public required IMediaContainerData Container { get; init; }
    public required IReadOnlyList<DiskInterpretation> Interpretations { get; init; }
}
```

Rôle :

- transporter les informations communes du fichier une seule fois ;
- transporter les données physiques sans les confondre avec une interprétation logique ;
- transporter toutes les interprétations complètes dans leur ordre de détection.

Le tableau ne définit aucune préférence métier. Lors d'une nouvelle ouverture avec détection automatique, l'application peut initialiser ses sélecteurs avec le premier élément trouvé, puis conserver explicitement son propre choix d'affichage.

### Informations communes de la source

```csharp
public sealed record MediaSourceInfo
{
    public required string Path { get; init; }
    public required string FileName { get; init; }
    public required long FileSize { get; init; }
    public required string ContainerFormatId { get; init; }
}
```

Ces propriétés sont valables pour SCP, ADF, ST, IMA, IMG et les autres conteneurs.

### Données propres au conteneur

```csharp
public interface IMediaContainerData
{
    string FormatId { get; }
}
```

Pour un SCP :

```csharp
public sealed record ScpContainerData : IMediaContainerData
{
    public string FormatId => "scp";
    public required ScpImage Image { get; init; }
}
```

`ScpImage` conserve son `ScpHeader`, ses pistes et le tableau de révolutions de chaque piste. Le visualisateur travaille directement sur ces données physiques.

Pour une image qui ne possède pas de flux ni de révolutions :

```csharp
public sealed record SectorContainerData : IMediaContainerData
{
    public required string FormatId { get; init; }
    public required long FileSize { get; init; }
}
```

Cette classe décrit uniquement le conteneur. Les secteurs restent dans chaque `DiskInterpretation`.

### Interprétation complète d'une machine et d'un format

```csharp
public sealed record DiskInterpretation
{
    public required string Id { get; init; }
    public required string SystemId { get; init; }
    public required string FormatId { get; init; }
    public required SectorImage Image { get; init; }
    public required DiskImageMetadata Metadata { get; init; }
    public required FileSystemInterpretation FileSystem { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
}
```

Exemples d'éléments pour une disquette hybride :

```text
Interpretations[0]
├── SystemId = "atari-st"
├── FormatId = "atarist.720"
├── Image = secteurs Atari ST 720 Kio
└── FileSystem
    ├── ReaderId = "fat12"
    └── Volume = volume FAT12 avec ENTOMBED.DOC, ENTOMBED.PRG et PREHIS.EXE

Interpretations[1]
├── SystemId = "amiga"
├── FormatId = "amiga.amigados"
├── Image = secteurs Amiga 880 Kio
└── FileSystem
    ├── ReaderId = "amigados"
    └── Volume = volume AmigaDOS avec ses propres fichiers
```

Les deux volumes ne peuvent plus être échangés, car chacun reste attaché à son image et à son format.

### Système de fichiers d'une interprétation

```csharp
public sealed record FileSystemInterpretation
{
    public required string ReaderId { get; init; }
    public required FileSystemVolume Volume { get; init; }
}
```

Même en l'absence de catalogue, l'interprétation conserve une `SectorImage`. Un volume physique de repli peut être construit, mais il appartient uniquement à cette interprétation.

## Classes conservées sans changement de responsabilité

| Classe existante | Décision | Responsabilité conservée |
|---|---|---|
| `ScpHeader` | Conserver | Métadonnées de l'en-tête SCP |
| `ScpImage` | Conserver | Conteneur SCP complet |
| `ScpTrack` | Conserver | Piste et tableau de révolutions |
| `ScpRevolution` | Conserver | Tour capturé et intervalles de flux |
| `SectorImage` | Conserver | Reconstruction sectorielle d'un format précis |
| `SectorBlock` | Conserver | Secteur, intégrité et révolution source |
| `FileSystemVolume` | Conserver | Volume et entrées racines |
| `FileSystemEntry` | Conserver | Dossier ou fichier récursif |
| `DiskImageMetadata` | Conserver | Métadonnées techniques d'une interprétation |
| `FileSystemRegistry` | Conserver | Exécution des lecteurs compatibles |

## Contrats envisagés dans cette proposition historique

| Contrat cible | Origine | Changement |
|---|---|---|
| `MediaExplorationResult` | évolution de `ExploredDiskImage` | Devient le résultat racine contenant la source, le conteneur et toutes les interprétations |
| `MediaSourceInfo` | nouveau | Regroupe les informations communes du fichier source |
| `IMediaContainerData` | nouveau | Sépare les données physiques du conteneur des interprétations logiques |
| `ScpContainerData` | nouveau adaptateur | Transporte le `ScpImage` existant sans recopier ses pistes ou révolutions |
| `SectorContainerData` | nouveau adaptateur | Décrit un conteneur sans flux |
| `DiskInterpretation` | évolution de `ExploredFileSystem` | Ajoute `SystemId`, `SectorImage`, métadonnées et avertissements à l'association format/volume |
| `FileSystemInterpretation` | extraction de `ExploredFileSystem` | Porte le lecteur et le volume reconnu |

## Compatibilité envisagée dans cette proposition historique

Il n'est pas nécessaire de casser immédiatement tous les consommateurs de `ExploredDiskImage`. Les propriétés actuelles peuvent devenir des projections du premier élément :

```csharp
public DiskInterpretation DisplayedInterpretation => Interpretations[DisplayedInterpretationIndex];
public SectorImage Image => DisplayedInterpretation.Image;
public FileSystemVolume Volume => DisplayedInterpretation.FileSystem.Volume;
public string PrimaryFormatId => DisplayedInterpretation.FormatId;
```

`DetectedFileSystems` et `DetectedImageFormatIds` peuvent rester temporairement disponibles comme projections en lecture seule. Ils ne doivent plus constituer la source de vérité.

## Utilisation par chaque fonction

### Explorateur

```text
ouvrir une nouvelle image
   └── sélectionner FormatsDetectes[0]

changer Machine/Format
   └── trouver IFormatDetecte correspondant à MachineId + FormatId
       ├── afficher son Volume
       ├── afficher ses Entries
       ├── afficher sa capacité
       ├── afficher son espace libre
       └── afficher ses avertissements
```

La ligne « Détecté » et la coloration des listes utilisent directement :

```text
FormatsDetectes.Select(item => (item.MachineId, item.FormatId))
```

Le champ « Système » utilise uniquement l'interprétation actuellement sélectionnée.

### Visualisateur

- pour SCP, il utilise les pistes et révolutions brutes conservées par `ExploredDiskImage.ScpImage` et projetées dans `IImageDisquette.Pistes` ;
- pour une image sectorielle, il utilise les secteurs de `IFormatDetecte.Secteurs` ;
- changer l'interprétation logique ne modifie jamais les révolutions brutes du SCP.

### Conversion

- la conversion automatique utilise l'interprétation sélectionnée par l'application ;
- une conversion avec format source explicitement choisi utilise l'élément correspondant dans `Interpretations` ;
- elle ne doit jamais utiliser le volume d'une interprétation avec l'image d'une autre.

### Écriture physique

- l'écriture d'une source SCP peut utiliser les pistes et révolutions physiques du conteneur ;
- l'écriture sectorielle utilise la `SectorImage` de l'interprétation choisie ;
- le choix doit être explicite lorsqu'il existe plusieurs interprétations utilisables.

### Lecture physique

- les diagnostics de lecture et les révolutions capturées alimentent les données physiques du conteneur ;
- après reconstruction, chaque machine/format reconnu ajoute une `DiskInterpretation` complète au tableau ;
- l'ordre du tableau définit la sélection automatique initiale.

## Responsabilités entre moteur et application

### Moteur `GWGUI.MediaEngine`

Le moteur doit :

- essayer les candidats de format ;
- conserver chaque reconstruction sectorielle séparément ;
- utiliser uniquement les lecteurs compatibles avec le candidat courant ;
- construire une interprétation complète par résultat reconnu ;
- retourner les interprétations dans l'ordre où elles ont été validées ;
- retourner les identifiants techniques, jamais les textes traduits de l'interface.

### Application `GWGUI.App`

L'application doit uniquement :

- traduire les identifiants ;
- lister et colorer les couples `SystemId` / `FormatId` présents dans `Interpretations` ;
- sélectionner le premier résultat trouvé à l'ouverture automatique, sans le considérer comme préférable ;
- afficher l'interprétation choisie ;
- demander une autre interprétation lorsque l'utilisateur change Machine ou Format.

L'application ne doit pas :

- déduire une machine depuis les extensions des fichiers du volume ;
- fusionner deux systèmes détectés ;
- remplacer le `SystemId` d'un résultat par la machine actuellement sélectionnée ;
- associer un volume à une image sectorielle différente.

## Invariants à tester

1. Chaque `DiskInterpretation.FormatId` est identique au `FormatId` de sa `SectorImage`.
2. Chaque volume est produit en lisant la `SectorImage` contenue dans la même interprétation.
3. Une interprétation Amiga ne peut pas contenir un volume FAT12 obtenu depuis le candidat Atari.
4. Une interprétation Atari ST ne peut pas contenir un volume AmigaDOS obtenu depuis le candidat Amiga.
5. La ligne « Détecté » contient tous les couples reconnus, sans doublon.
6. Les éléments colorés dans les sélecteurs correspondent exactement aux couples présents dans le tableau.
7. Le champ « Système », le volume, la capacité et les fichiers proviennent tous du même élément sélectionné.
8. Les pistes et révolutions SCP restent inchangées quelle que soit l'interprétation sélectionnée.

## Changement minimal recommandé

Le premier changement de code doit rester limité :

1. enrichir le résultat d'interprétation pour conserver sa `SectorImage` ;
2. empêcher `ScpCandidateRanker` de supprimer cette association ;
3. faire porter au document final le tableau d'interprétations complètes ;
4. conserver temporairement les anciennes propriétés comme projections de l'interprétation principale ;
5. faire consommer ce tableau par l'explorateur ;
6. seulement après validation, faire utiliser la même source de vérité par conversion et écriture.

Il n'est pas nécessaire de réécrire `ScpImage`, `ScpTrack`, `ScpRevolution`, `SectorImage`, `FileSystemVolume` ou `FileSystemEntry`.
