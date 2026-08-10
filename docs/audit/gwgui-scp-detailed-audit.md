# Audit détaillé de `GWGUI.MediaEngine`

Date de l’audit : 10 août 2026

## Objet

Ce document décrit le code actuellement présent dans `src/GWGUI.MediaEngine`. Il complète l’audit général de la phase 01 et sert de base factuelle à la discussion sur la structure cible de ce projet.

L’audit porte sur les fichiers C# de production, leurs types, leurs dépendances internes, leurs responsabilités visibles, leurs points d’enregistrement et les duplications structurelles constatées. Il ne valide pas à lui seul l’exactitude de chaque algorithme sur tous les formats : cette validation nécessite les tests et le corpus prévus ultérieurement.

## État général observé

`GWGUI.MediaEngine` est autonome : son projet cible `net10.0`, active nullable et les imports implicites, et ne référence aucun autre projet ni paquet externe. Cette frontière est saine et doit être conservée.

Le projet ne traite pas seulement le format SCP. Il contient actuellement :

- le parser du conteneur SCP ;
- les modèles de capture et de révolutions ;
- les primitives de bits, CRC et flux ;
- 25 décodeurs de flux ;
- 24 encodeurs de pistes ;
- des lecteurs de nombreux conteneurs sectoriels ;
- la reconstruction de secteurs depuis des captures SCP ;
- la détection automatique et les interprétations supplémentaires ;
- 17 lecteurs de systèmes de fichiers ;
- l’orchestration de l’exploration ;
- une conversion interne RWTS18 ;
- la projection d’images sectorielles vers du flux destiné au Visualisateur.

Le nom du projet est donc historique et plus étroit que sa responsabilité réelle. Il constitue aujourd’hui le moteur interne complet d’analyse des images de disquettes.

## Constats anciens devenus inexacts

Deux constats importants de `structural-findings.md` ne décrivent plus le code actuel :

- `DiskImageExplorer.cs` ne fait plus environ 511 lignes ; il en fait environ 68. Une grande partie du routage a déjà été extraite vers les politiques de conteneurs, la détection SCP, les interprétations et les registres.
- `AtariScpSectorImageReader.cs` ne contient plus les branches Amstrad, IBM, BBC, Epson et UCSD. Il fait environ 22 lignes et délègue au pipeline ISO commun après avoir validé que l’identifiant demandé appartient à Atari.

L’audit général doit être mis à jour avant d’utiliser ces deux fichiers comme exemples de monolithes actuels. Le pipeline ISO a déjà été partiellement refactorisé ; il faut maintenant contrôler sa structure réelle au lieu de planifier à nouveau une extraction déjà faite.

## Chaîne technique réellement observée

### Images sectorielles directes

```text
chemin + format demandé
→ DiskImageRecognitionRegistry
→ politique de conteneur
→ lecteur sectoriel concret
→ SectorImage
→ FileSystemRegistry
→ DiskImageInterpretationService
→ ExploredDiskImage
```

### Captures SCP automatiques

```text
chemin SCP
→ ScpFamilyProbe
→ ScpCandidateRegistry
→ un ou plusieurs reconstructeurs
→ SectorImage candidate(s)
→ FileSystemRegistry.ReadAll
→ normalisations et interprétations supplémentaires
→ score et déduplication
→ ExploredDiskImage
```

### Captures SCP avec format explicite

```text
formatId
→ ScpCandidateRegistry.Selected
→ façade ou reconstructeur correspondant
→ SectorImage
→ FileSystemRegistry
→ ExploredDiskImage
```

### Visualisation d’une image sectorielle

```text
SectorImage
→ SectorImageVisualizationPolicyRegistry
→ politique de format
→ encodeur de piste
→ flux synthétique destiné au renderer de GWGUI.App
```

## Conteneur SCP et primitives

### Fichiers racine

| Fichier | Responsabilité observée | Constat |
|---|---|---|
| `Containers/Scp/IScpReader.cs` | contrat asynchrone de lecture SCP | Une seule implémentation de production observée. |
| `Containers/Scp/ScpReader.cs` | lecture de fichier, en-tête, table de pistes, révolutions et validation | Le parsing de l’en-tête et ses consommateurs utilisent directement `ScpReader.ReadHeader`; l’ancien relais `ScpHeaderReader` a été supprimé. |
| `Containers/Scp/ScpFlags.cs`, `ScpHeader.cs`, `ScpRevolution.cs`, `ScpTrack.cs`, `ScpImage.cs` | modèles du conteneur SCP | Les cinq types sont maintenant séparés. `ScpHeader` et `ScpRevolution` contiennent les calculs dérivés de version, résolution, durée et RPM. |
| `Exploration/ScpCaptureInfo.cs`, `ScpCaptureInfoReader.cs` | informations résumées de capture et lecture associée | Le record et son service de lecture sont maintenant séparés. |
| `Containers/Scp/ScpFormatConstants.cs` | constantes du format SCP | Détient les signatures, offsets, tailles, limites et unités fixes partagés par la lecture et ses tests. |
| `Containers/Scp/ScpExceptions.cs` | construction des erreurs de validation SCP | Centralise les erreurs paramétrées par les valeurs observées, les pistes attendues et les limites des sections incomplètes. |

`ScpFlags` utilise un enum à drapeaux. `DiskType`, `BitCellEncoding` et `Heads` restent des octets dans `ScpHeader`.

### Primitives

| Fichier | Responsabilité observée | Constat |
|---|---|---|
| `Flux/FluxBitstream.cs` | conversion des intervalles de flux en flux de bits navigable | Primitive interne partagée par les codecs ; elle n’appartient pas à une machine. |
| `Primitives/BitPrimitives.cs` | inversion de bits | Primitive neutre déjà réutilisée par l’encodage. |
| `Primitives/Crc16Calculator.cs` | calcul CRC16 paramétré | Primitive neutre ; les paramètres propres aux formats doivent rester dans leurs codecs ou définitions. |

La frontière `Flux`/`Primitives` est actuellement mince. Il faudra décider si les primitives de bits et CRC restent dans un sous-dossier autonome ou rejoignent un module de codecs. Aucun besoin ne justifie un grand dossier générique `Common`.

## Décodage de flux

### Contrat, modèles et registre

`IFluxDecoder` expose un identifiant, un nom visible et une opération synchrone de décodage d’une `ScpRevolution`. `FluxDecodeModels.cs` contient :

- `FluxStructureKind`, enum fermé de structures visuelles/techniques ;
- `SectorIntegrityKind`, enum fermé CRC/checksum ;
- `FluxStructure` ;
- `DecodedSector` ;
- `FluxDecodeResult`.

Les modèles mélangent actuellement résultat technique et présentation : `FluxDecodeResult.DisplayName` et `FluxStructure.Description` sont des chaînes visibles ou semi-visibles produites par le moteur. Cela crée une dépendance conceptuelle vers l’interface et la localisation, même sans référence WPF.

`FluxDecoderRegistry` possède trois responsabilités :

1. composition de la liste complète des décodeurs ;
2. cache des résultats par objet `ScpRevolution` et identifiant ;
3. sélection automatique ou meilleure révolution par score.

Le cache par `ConditionalWeakTable` est pertinent pour éviter de retenir indéfiniment les révolutions. En revanche, l’enregistrement, le cache et la politique de score sont trois responsabilités séparables. `Decode` recherche le décodeur par `First` à chaque première demande d’un identifiant et ne contrôle pas explicitement les doublons. Les identifiants sont des chaînes brutes.

### Bases et décodeurs concrets

`AppleBitLatch` est une primitive Apple interne. `SignatureMfmDecoder` est une base commune utilisée par plusieurs formats à signature MFM. Leur présence montre deux types de partage différents : partage au sein d’une famille et partage selon un codec.

Décodeurs inventoriés :

- ISO : `IsoFmDecoder`, `IsoMfmDecoder` ;
- Amiga : `AmigaMfmDecoder` ;
- Apple : `AppleIIGcrDecoder` dont le type est nommé `AppleGcrDecoder`, `AppleRwts18Decoder`, `AppleMacGcrDecoder`, `AppleLisaFileWareGcrDecoder` ;
- Commodore : `CommodoreGcrDecoder`, `Commodore900GcrDecoder` ;
- DEC : `DecRx02Decoder` ;
- autres : `Aed6200pMfmDecoder`, `ArburgDecoder`, `CenturionMfmDecoder`, `DataGeneralFmDecoder`, `EmuFmDecoder`, `HeathkitFmDecoder`, `HpMmfmDecoder`, `MembrainMfmDecoder`, `MicralNFmDecoder`, `MicropolisMfmDecoder`, `NorthstarMfmDecoder`, `QdMo5MfmDecoder`, `TycomFmDecoder`, `Victor9kGcrDecoder` ;
- repli : `RawFluxDecoder`.

Constats :

- Le fichier `AppleIIGcrDecoder.cs` déclare `AppleGcrDecoder`, ce qui rompt la correspondance habituelle fichier/type et rend la recherche moins évidente.
- Les décodeurs sont déjà séparés un par fichier et sont généralement courts. Les fusionner par famille recréerait des fichiers plus difficiles à maintenir.
- Le partage pertinent se situe dans les tables, primitives et définitions de codec, pas dans une classe géante par machine.
- `RawFluxDecoder` doit rester un repli et ne doit pas battre un codec reconnu ; cette règle vit actuellement dans `AutomaticScore` via l’identifiant brut `"raw"`.
- Les identifiants de codecs sont répétés entre registre, probe de familles, reconstructeurs et politiques de visualisation. Ils nécessitent une source neutre unique, mais pas un enum si des codecs doivent rester ajoutables.

## Encodage de pistes

`TrackEncodeModels.cs` mélange les modèles `TrackSector`, `TrackEncodeRequest`, `EncodedTrack` et le contrat `ITrackEncoder`. Les modèles utilisent des dictionnaires `string → int` pour des attributs spécifiques. Cette extensibilité évite un modèle géant, mais rend les attributs non découvrables et non validés à la compilation.

`FluxEncoderRegistry` compose les 25 encodeurs et effectue leur sélection par chaîne avec `First`. Il ne possède ni contrôle explicite des identifiants dupliqués, ni distinction déclarative entre codecs décodables et codecs encodables.

`FluxEncoding`, `TrackEncoding` et `TrackEncoderBase` contiennent les primitives partagées d’écriture de bits, MFM/FM/GCR et de construction de révolution. `TrackEncoding` est le plus chargé de ce groupe. Il faut vérifier méthode par méthode si les opérations sont réellement génériques ou appartiennent à un codec précis.

Encodeurs inventoriés :

- ISO : `IsoFmTrackEncoder`, `IsoMfmTrackEncoder` ;
- Amiga : `AmigaMfmTrackEncoder` ;
- Apple : `AppleIIGcrTrackEncoder`, `AppleRwts18TrackEncoder`, `AppleMacGcrTrackEncoder`, `AppleLisaFileWareGcrTrackEncoder` ;
- Commodore : `CommodoreGcrTrackEncoder`, `Commodore900GcrTrackEncoder` ;
- DEC : `DecRx02TrackEncoder` ;
- autres : `Aed6200pMfmTrackEncoder`, `ArburgTrackEncoder`, `CenturionMfmTrackEncoder`, `DataGeneralFmTrackEncoder`, `EmuFmTrackEncoder`, `HeathkitFmTrackEncoder`, `HpMmfmTrackEncoder`, `MembrainMfmTrackEncoder`, `MicralNFmTrackEncoder`, `MicropolisMfmTrackEncoder`, `NorthstarMfmTrackEncoder`, `QdMo5MfmTrackEncoder`, `TycomFmTrackEncoder`, `Victor9kGcrTrackEncoder`.

Les paires décodeur/encodeur partagent légitimement marques, tables et paramètres de format lorsqu’ils sont strictement identiques. Ces données doivent appartenir à une définition ciblée du codec. Les boucles de lecture et d’écriture, elles, restent des algorithmes distincts.

## Images sectorielles et conteneurs

### Modèle commun

`SectorImages/SectorImage.cs` contient `SectorAddress`, `SectorBlock` et `SectorImage`. Le modèle combine :

- identité de format sous forme de chaîne ;
- géométrie CHS ;
- adressage logique ;
- données et tag ;
- intégrité ;
- numéro de révolution ;
- capacité et tailles variables.

Il est central et utilisé par lecteurs, reconstructeurs, systèmes de fichiers, exploration et visualisation. Toute modification de ce fichier a donc un risque transversal élevé. Le constructeur déduplique silencieusement les blocs ayant le même numéro logique en conservant le premier. Cette politique est un comportement important à rendre explicite ou à tester avant refactorisation.

`Images/ISectorImageReader.cs` est correctement séparé de `AdfImageReader.cs`, contrairement à l’ancien constat d’audit. Le fichier d’audit général doit être corrigé sur ce point.

### Registre de conteneurs

`DiskImageRecognitionContext` conserve le chemin, la longueur, l’extension normalisée et le format demandé, puis met en cache les mêmes octets pendant toute la sélection des politiques. `IDiskImageRecognitionPolicy` définit la frontière entre présélection et lecture. `DiskImageRecognitionRegistry` essaie les politiques dans l’ordre, poursuit après le rejet d’un candidat et retourne le premier contenu entièrement validé. `DiskImageRecognitionExceptions` centralise les erreurs paramétrées par l’extension, le format demandé ou la politique qui a rejeté le contenu. La composition complète se trouve dans `DiskImageExplorerFactory`.

Politiques observées :

- directes par extensions : `DirectContainerPolicy` ;
- déléguées par fonction : `DelegatingContainerPolicy` ;
- spécialisées : `AmstradImageRecognitionPolicy`, `AppleImageRecognitionPolicy`, `CoherentImageRecognitionPolicy`, `DecRx02ContainerPolicy`, `MsxContainerPolicy`, `RawImgContainerPolicy`, `ScpContainerPolicy`.

Constats :

- L’ordre du registre est un comportement fonctionnel pour les extensions ambiguës `.img` et `.dsk`.
- Les politiques spécialisées utilisent signatures, tailles ou format demandé pour arbitrer ces ambiguïtés.
- `DiskImageRecognitionRegistry` retourne un seul lecteur gagnant après avoir essayé dans l’ordre tous les candidats compatibles nécessaires. Les interprétations internes multiples restent conservées ensuite.
- Les valeurs d’extension sont centralisées dans `DiskImageFileExtensions`; les ensembles propres à chaque politique restent distincts lorsqu’ils représentent des indices différents.
- `ScpContainerPolicy` vérifie désormais `ScpFormatConstants.FileSignature` et ne dépend plus de l’extension pour reconnaître le conteneur.

### Lecteurs sectoriels directs

Lecteurs inventoriés :

- Amiga : `AdfImageReader` ;
- Amstrad : `Containers/Amstrad/CpcDsk/CpcDskReader` ;
- Apple : `AppleDiskImageReader`, `AppleRawImageReader`, `AppleNibbleImageWriter`, `AppleSectorImageFactory`, `AppleDiskGeometry`, `AppleDiskImageSignatures`, `Containers/Apple/TwoImg/TwoImgReader`, `Containers/Apple/DiskCopy/DiskCopyReader`, `Containers/Apple/Woz/WozReader`, `WozFormat`, `WozLayout`, `WozExceptions`, `Recognition/Apple/NibTrackImageReader`, `NibTrackFormat` et `NibTrackExceptions` ;
- Atari : `AtariStImageReader`, `MsaImageReader`, `AtrReader` ;
- BBC : `BbcDfsImageReader` ;
- Commodore : `CommodoreD64ImageReader`, `CommodoreD71ImageReader`, `CommodoreD81ImageReader`, `CommodoreGeometry` ;
- IBM et conteneurs associés : `IbmPcImageReader`, `ImdImageReader`, `Td0ImageReader`, `I86fImageReader` ;
- autres : `CoherentImageReader`, `Cp2ImageReader`, `DecRx02ImageReader`, `MsxImageReader`.

Constats détaillés :

- `AppleImageRecognitionPolicy` distingue maintenant les marqueurs 2IMG, DiskCopy et WOZ des indices propres aux représentations brutes. `AppleDiskImageReader` route également ces trois conteneurs par leur contenu. Il conserve encore la liste générale d’extensions et `LooksLikeAppleImage`, dont le déplacement est prévu dans son groupe dédié ultérieur.
- `AppleDiskImageReader` expose aussi des façades statiques vers géométrie, signatures et factory Apple. Ces façades semblent préserver d’anciens consommateurs et masquent les propriétaires réels.
- `IbmPcImageReader` contient à la fois lecture brute, catalogue de géométries, analyse BPB, détection de géométrie de flux, reconnaissance OEM DOS et génération d’identifiant. Ce fichier reste réellement mélangé.
- Les géométries IBM sont utilisées au-delà de la seule lecture de `.img/.ima` et méritent un propriétaire distinct du lecteur de conteneur.
- `CommodoreGeometry` est déjà une source commune ciblée pour D64/D71/D81 ; ce modèle est préférable à la répétition des tailles dans chaque lecteur.
- `ImdImageReader`, `Td0ImageReader`, `I86fImageReader` et `Cp2ImageReader` ont chacun des structures privées de parsing ; celles-ci peuvent rester privées si elles ne sont pas partagées.

## Reconstruction sectorielle depuis SCP

### Pipeline ISO FM/MFM actuel

Le pipeline est déjà nettement séparé :

- `IsoScpSectorImageReader` lit toutes les pistes et révolutions, exécute les décodeurs autorisés par la politique et collecte les candidats ;
- `IsoSectorCandidate` et `IsoSectorCandidateSet` représentent les candidats ;
- `IIsoScpSectorImagePolicy` décrit décodeurs et construction ;
- `IsoScpSectorImagePolicyRegistry` choisit une politique par identifiant ;
- `IsoSectorImageBuilder` assemble l’image ;
- des politiques spécialisées portent les règles de famille.

Politiques : `AutomaticIsoScpSectorImagePolicy`, `GenericIsoScpSectorImagePolicy`, `AtariStIsoScpSectorImagePolicy`, `Atari8BitIsoScpSectorImagePolicy`, `AmstradIsoScpSectorImagePolicy`, `BbcIsoScpSectorImagePolicy`, `IbmPcIsoScpSectorImagePolicy`, `EpsonQx10IsoScpSectorImagePolicy`, `UcsdIsoScpSectorImagePolicy`.

Façades publiques : `AtariScpSectorImageReader`, `AmstradScpSectorImageReader`, `BbcScpSectorImageReader`, `IbmPcScpSectorImageReader`, `EpsonQx10ScpSectorImageReader`, `UcsdScpSectorImageReader`.

Constats :

- L’ancien monolithe Atari a déjà été défait.
- Les façades publiques sont très courtes et valident principalement la famille avant délégation. Leur utilité doit être évaluée face à un service unique prenant un identifiant typé/catalogué.
- `IsoScpSectorImagePolicyRegistry` répète les préfixes de format également présents dans `ScpCandidateRegistry`, les normaliseurs, les systèmes de fichiers et la visualisation.
- `IsoScpSectorImageReader` choisit pour chaque révolution le meilleur des décodeurs autorisés selon un score local distinct du score automatique de `FluxDecoderRegistry`. Deux politiques de score existent donc.
- Les candidats sont séparés entre adresse déclarée et adresse physique de piste. Cette distinction est importante et doit rester explicite.

### Epson QX-10

Le support Epson est déjà découpé en `EpsonQx10FormatDetector`, `EpsonQx10GeometryCatalog`, `EpsonQx10SectorImagePolicy`, `EpsonQx10IsoScpSectorImagePolicy`, `EpsonQx10SectorImageBuilder` et façade publique. Ce découpage reflète des responsabilités réelles, mais il faut vérifier que les cinq identifiants Epson ne sont pas encore recopiés dans le CP/M reader, le registre de candidats et les catalogues.

### Reconstructions non ISO

- Amiga : `AmigaScpSectorImageReader` ;
- Apple partagé : `AppleScpSectorDecoder` ;
- Apple II : `AppleIIScpSectorReconstructor` ;
- Apple RWTS18 : `AppleRwts18ScpSectorReconstructor` ;
- Macintosh : `AppleMacScpSectorReconstructor` ;
- orchestration Apple : `AppleScpSectorImageReader` ;
- Commodore : `CommodoreScpSectorImageReader` ;
- DEC : `DecRx02ScpSectorImageReader`.

Ces familles ont des structures réellement différentes. Elles ne doivent pas être forcées dans le pipeline ISO uniquement pour uniformiser l’arborescence.

## Détection et interprétations

### Probe et candidats SCP

`ScpFamilyProbe` échantillonne au plus six pistes et essaie huit identifiants de décodeurs pour produire un ensemble de familles. L’enum interne `ScpFormatFamily` est approprié à ce regroupement fermé utilisé uniquement pour optimiser la détection ; il ne remplace pas les identifiants extensibles de formats.

`ScpCandidateRegistry` contient :

- le routage d’un format explicitement sélectionné ;
- les candidats par défaut ;
- les candidats associés aux familles détectées ;
- la liste des cinq formats Epson.

Il constitue aujourd’hui une concentration majeure de chaînes de formats et de composition de reconstructeurs. La classe est un registre, une politique de sélection et une fabrique de délégués à la fois.

`ScpAutomaticImageExplorer` exécute les candidats en parallèle, lit tous les systèmes de fichiers reconnus, applique normalisations et interprétations supplémentaires, calcule un meilleur décodage, déduplique et filtre les alternatives jugées crédibles. La classe porte donc la politique de sélection automatique complète. Cette politique est distincte du simple registre de candidats et doit rester clairement identifiable. `ScpImageExplorationService` constitue la façade publique qui coordonne sélection explicite et exploration automatique.

### Interprétations et normalisations

Registres et contrats : `AdditionalImageInterpretationRegistry`, `IAdditionalImageInterpretationPolicy`, `RecognizedImageNormalizerRegistry`, `IRecognizedImageNormalizer`.

Politiques : `CompatibleFormatInterpretationPolicy`, `IbmAdditionalImageInterpretationPolicy`, `MsxAdditionalImageInterpretationPolicy`, `AtariRecognizedImageNormalizer`, `MacRecognizedImageNormalizer`, `MsxRecognizedImageNormalizer`, avec `SectorImageInterpretation` comme aide de transformation.

Le découpage registre/politique est déjà présent. Les risques restants sont la répétition d’identifiants, l’ordre implicite des politiques et la modification silencieuse du `FormatId` par retagging.

### Métadonnées et résultat

`DiskImageMetadata` produit directement des textes (`"—"`, noms de systèmes et concaténation `" + "`) via `DiskSystemCatalog` et `DiskProtectionCatalog`. Cette responsabilité est une présentation localisable et n’appartient pas entièrement au moteur technique.

`ExploredDiskImage` contient le chemin source, l’image sectorielle primaire, un volume primaire, un booléen de reconnaissance, tous les systèmes reconnus et tous les identifiants de formats décodés. Le modèle supporte déjà le multiformat, mais conserve aussi les champs singuliers `Image` et `Volume`. La règle de choix du primaire reste donc importante.

## Systèmes de fichiers

### Modèles et registre

`FileSystemModels.cs` contient les modèles, l’enum de type d’entrée et `IFileSystemReader`. `FileSystemEntry` contient données métier et contenu complet optionnel. `FileSystemVolume.Warnings` est une liste de chaînes techniques, ce qui limite la localisation et la structuration des avertissements.

`FileSystemRegistry` compose 17 lecteurs. `ReadAll` conserve tous les résultats reconnus, tandis que `TryRead` sans identifiant choisit le premier lecteur dont `CanRead` réussit. L’ordre reste donc un comportement fonctionnel dans certains parcours. Les `InvalidDataException` sont absorbées pour poursuivre la détection, sans conserver leur diagnostic.

### Lecteurs inventoriés

- Acorn/BBC : `AcornAdfsFileSystemReader`, `AcornFileCoreNewMap`, `BbcDfsFileSystemReader` ;
- Amiga : `AmigaDosFileSystemReader` ;
- CP/M : `CpmFileSystemReader`, `AmstradCpmFileSystemReader` ;
- Apple : `AppleDosFileSystemReader`, `AppleInformXzipFileSystemReader`, `ProDosFileSystemReader`, `MacMfsFileSystemReader`, `MacHfsFileSystemReader`, `LisaFileSystemReader` ;
- Atari : `AtariDosFileSystemReader` ;
- Commodore : `CommodoreDosFileSystemReader` ;
- IBM : `Fat12FileSystemReader` ;
- autres : `CoherentFileSystemReader`, `Rt11FileSystemReader`, `UcsdFileSystemReader`.

### Duplications observées

`CpmFileSystemReader` et `AmstradCpmFileSystemReader` dupliquent notamment :

- parsing des entrées de 32 octets ;
- décodage des noms 8.3 ;
- lecture des allocations 8 ou 16 bits ;
- regroupement des extents ;
- reconstruction du contenu ;
- comparateur `(User, Name)` ;
- aplatissement d’une image sectorielle.

Les dispositions de disque et certaines règles de reconnaissance diffèrent réellement. La bonne frontière est un lecteur CP/M commun paramétré par une définition de layout et une stratégie de découverte spécialisée, pas la fusion brute des deux classes.

Plusieurs lecteurs de systèmes de fichiers réimplémentent également l’aplatissement des blocs, les lectures endian et les conversions de dates. Chaque similarité doit être comparée : les blocs absents, tailles variables, époques et endianess ne sont pas nécessairement identiques.

## Exploration

`DiskImageExplorer` est aujourd’hui un orchestrateur raisonnablement court. Il conserve néanmoins quelques décisions métier :

- chemin SCP automatique court-circuité par extension ;
- retour `Unknown` sur certaines exceptions ;
- utilisation de `ReadAll` en automatique ;
- une seule interprétation supplémentaire retenue à cause d’un `break` ;
- retagging lors d’un format manuel ;
- déduplication par format, lecteur et nom de volume.

Le `break` dans les interprétations supplémentaires peut perdre plusieurs interprétations valides. Il contredit potentiellement la règle multiformat et doit être testé avant toute modification.

`DiskImageInterpretationService` possède plusieurs responsabilités : normalisation, interprétations supplémentaires, construction du document, fabrication du volume physique de repli, résultat inconnu, score, crédibilité et identité de déduplication. Ce fichier est le principal mélange structurel restant autour de l’exploration.

`DiskImageExplorerFactory` est le point de composition complet du moteur. Il construit lecteurs, registres, politiques et services. Sa liste rend la composition visible, mais elle répète extensions et identifiants déjà détenus ailleurs. Le point de composition doit rester unique même si les données d’enregistrement sont déplacées vers des définitions ciblées.

## Conversion et visualisation technique

`AppleRwts18ConversionService` est la seule conversion interne explicitement isolée dans ce projet. Elle appartient à une capacité de transformation, pas au dossier générique `Images`.

`SectorImageFluxVisualizer` choisit une politique, construit les requêtes d’encodage et produit du flux synthétique. Il dépend du registre d’encodeurs et du registre de politiques. Il ne fait pas de rendu WPF ou Skia, mais son nom le place encore dans `Images` au lieu d’une frontière de projection technique.

Politiques de visualisation : `ISectorImageVisualizationPolicy`, `SectorImageVisualizationPolicy`, `SectorImageVisualizationPolicyRegistry`, `AppleVisualizationPolicy`, `AtariVisualizationPolicy`, `CommodoreVisualizationPolicy`, `DecRx02VisualizationPolicy`, `ExactVisualizationPolicy`, `PrefixVisualizationPolicy`.

Les politiques exactes et par préfixe continuent d’utiliser les identifiants de formats comme mécanisme de routage. Leurs valeurs et préfixes sont désormais fournis par `DiskImageFormatIds`, sans faire dépendre `GWGUI.MediaEngine` de `GWGUI.Domain`.

## Sources de vérité dispersées confirmées

### Identifiants de formats

Les consommateurs sont notamment :

- `ScpCandidateRegistry` ;
- `IsoScpSectorImagePolicyRegistry` ;
- géométries IBM et Epson ;
- lecteurs sectoriels ;
- `CatalogFormatIds` des systèmes de fichiers ;
- normaliseurs et interprétations ;
- politiques de visualisation ;
- `DiskSystemCatalog` et `DiskProtectionCatalog`.

Ces identifiants sont extensibles et ne forment donc pas un enum fermé. Leurs valeurs fixes et leurs préfixes sont centralisés dans `Recognition/Definitions/DiskImageFormatIds.cs`. Ce fichier fournit également les constructions calculées utilisées pour les capacités Atari ST et IBM ainsi que les géométries ATR et SCP Atari. Les identifiants de codecs restent une responsabilité distincte.

### Géométries

Les géométries IBM sont concentrées dans `IbmPcImageReader` mais consommées conceptuellement par lecture directe, reconstruction SCP et détection. Epson et Commodore possèdent déjà des catalogues ciblés. Apple possède `AppleDiskGeometry`. Le modèle à suivre est une définition de géométrie par famille, pas un enum global de tailles de disquettes.

Une taille physique de média telle que 3,5 ou 5,25 pouces pourrait être un enum fermé dans une couche descriptive, mais elle ne remplace pas une géométrie : cylindres, faces, secteurs, taille de secteur, débit et ordre restent des données structurées.

### Codecs

Les identifiants sont répétés entre décodeurs, encodeurs, probe, reconstructeurs et visualisation. Une définition commune peut relier les capacités decode/encode, mais les algorithmes restent séparés.

### Textes

Le moteur produit des noms visibles, descriptions de structures, noms de systèmes et avertissements sous forme de chaînes anglaises. Les erreurs techniques peuvent rester dans les exceptions et journaux, mais les données destinées à l’interface doivent devenir des codes structurés ou être projetées/localisées dans `GWGUI.App`.

## Frontières structurelles déduites du code

Les frontières suivantes sont justifiées par des responsabilités et dépendances réelles :

1. conteneurs et parsing de fichiers ;
2. primitives de flux et de bits ;
3. décodage de flux ;
4. encodage de pistes ;
5. modèle d’image sectorielle ;
6. reconstruction sectorielle depuis flux ;
7. détection, classification, interprétation et normalisation ;
8. systèmes de fichiers ;
9. orchestration d’exploration ;
10. conversions internes ;
11. projections techniques de visualisation.

Les familles de machines sont des sous-ensembles à l’intérieur de ces frontières lorsque leurs règles sont spécialisées. Elles ne doivent pas devenir les racines principales du projet, car cela dupliquerait contrats, registres et primitives dans Apple, Atari, Commodore, etc.

## Parcours réel vérifié pendant l’audit

### Fichier utilisé

Le parcours a été vérifié avec la paire déjà classée sous `image_test/validated_images/Amstrad/CPC/3 pouces simple face - 180 Kio/` :

- `007 - A View to a Kill (1985)(Domark).dsk` ;
- `007 - A View to a Kill (1985)(Domark) [test].scp` ;
- les fichiers de secteurs source et décodés associés.

L’image n’a pas été déplacée ni modifiée. Une nouvelle conversion de travail a été créée sous `artifacts/understanding-test/amstrad-cpc-converted.scp`.

### Lecture et Explorateur

Le test ciblé `RealSingleAmstradCpcImageAndFluxRemainEquivalentWhenRequested` a été exécuté une première fois sur la paire validée, puis sur le SCP nouvellement converti. Les deux exécutions réussissent.

Le parcours observé est :

```text
DSK
→ DiskImageRecognitionRegistry
→ AmstradImageRecognitionPolicy
→ AmstradDskImageReader
→ SectorImage amstrad.cpc
→ AmstradCpmFileSystemReader
→ ExploredDiskImage
```

et, pour la capture :

```text
SCP
→ ScpReader
→ ScpFamilyProbe / ScpCandidateRegistry
→ IsoScpSectorImageReader
→ AmstradIsoScpSectorImagePolicy
→ IsoSectorImageBuilder
→ SectorImage amstrad.cpc
→ AmstradCpmFileSystemReader
→ ExploredDiskImage
```

Le volume, le système de fichiers, la capacité, l’espace libre, l’arborescence complète, le contenu des fichiers et les avertissements sont identiques entre le DSK et le SCP reconstruit. La détection automatique du SCP retrouve `amstrad.cpc`.

### Conversion réelle

Une première commande directe avec `--format amstrad.cpc` a échoué parce que ce format n’existe pas dans les définitions natives de Greaseweazle 1.23. La conversion correcte nécessite le fichier embarqué par GW GUI :

```text
gw.exe convert
  --diskdefs src/GWGUI.App/Assets/DiskDefinitions/built-in.cfg
  --format amstrad.cpc
  source.dsk sortie.scp
```

Avec ces définitions, Greaseweazle convertit 40 pistes simple face et annonce 360 secteurs trouvés sur 360. Cela confirme une frontière importante :

- `GWGUI.Domain` choisit l’identifiant et construit la commande ;
- `GWGUI.App`/le gestionnaire de définitions fournit le chemin des `diskdefs` embarquées ;
- `GWGUI.Infrastructure` exécute `gw.exe` ;
- `GWGUI.MediaEngine` relit, détecte, reconstruit et explore le résultat.

`GWGUI.MediaEngine` ne doit donc pas absorber la construction des commandes Greaseweazle ni le stockage des `diskdefs`, même si ses identifiants techniques doivent rester cohérents avec eux.

### Visualisateur et rendu

`SectorImageFluxVisualizer.Create` a produit les pistes de visualisation depuis l’image sectorielle DSK. Le SCP converti a ensuite été chargé par `ScpReader`, décodé par `FluxDecoderRegistry` avec `iso.mfm`, préparé par `SkiaScpRenderer` et rendu par le véritable contrôle WPF `ScpDiskView` dans un `RenderTargetBitmap`.

Les deux tests ciblés `PublicPhysicalCapturesLoadAndDecodeWhenRequested` et `PublicPhysicalCaptureRendersThroughTheRealWpfSkiaControlWhenRequested` réussissent. Le harnais exige au moins quatre entrées de corpus ; la même capture a été répétée quatre fois uniquement pour atteindre le parcours de rendu. Cette répétition ne constitue pas quatre validations de formats distincts.

Le parcours de visualisation traverse donc deux frontières :

```text
image sectorielle
→ SectorImageFluxVisualizer dans GWGUI.MediaEngine
→ ScpImage synthétique
→ SkiaScpRenderer et ScpDiskView dans GWGUI.App
```

ou directement :

```text
capture SCP
→ ScpReader / FluxDecoderRegistry dans GWGUI.MediaEngine
→ SkiaScpRenderer et ScpDiskView dans GWGUI.App
```

Cette vérification confirme que la projection technique appartient au moteur, tandis que le rendu graphique et le contrôle WPF appartiennent à l’application.

## Risques prioritaires pour la future structure

1. Casser l’ordre de sélection des politiques de conteneurs pour `.img` et `.dsk`.
2. Casser le classement des candidats SCP en fusionnant les différents scores.
3. Perdre des interprétations multiformats en conservant un premier résultat.
4. Centraliser trop largement des géométries ou CRC seulement ressemblants.
5. Remplacer les identifiants extensibles par un enum fermé.
6. Déplacer les textes visibles sans prévoir leur projection localisée.
7. Modifier `SectorImage` sans mesurer l’impact sur toutes les couches.
8. Créer plusieurs points de composition ou plusieurs instances de registre.
9. Supprimer des façades publiques sans adapter tous les consommateurs et tests.
10. Confondre la projection technique de visualisation avec le renderer WPF/Skia de l’application.

## Inventaire de couverture

Tous les fichiers C# de production observés sont couverts par les groupes ci-dessus :

- conteneur SCP : `IScpReader`, `ScpReader`, `ScpExceptions`, `ScpFlags`, `ScpHeader`, `ScpRevolution`, `ScpTrack`, `ScpImage` et `ScpFormatConstants` ;
- primitives : `FluxBitstream`, `BitPrimitives`, `Crc16Calculator` ;
- décodage : contrat, modèles, registre, deux bases et les 25 décodeurs présents ;
- encodage : modèles, registre, bases et les 24 encodeurs présents ;
- images directes : contrat, modèles communs, politiques de conteneurs et tous les lecteurs listés ;
- reconstruction : modèles ISO, registre, politiques, façades et reconstructeurs non ISO listés ;
- détection/interprétation : tous les fichiers de `Images/ScpDetection` et `Images/Interpretations` ;
- systèmes de fichiers : modèles, registre, 17 lecteurs et `AcornFileCoreNewMap` ;
- exploration : `DiskImageExplorer`, sa factory, le service d’interprétation et les modèles de résultat ;
- conversion : `AppleRwts18ConversionService` ;
- visualisation technique : `SectorImageFluxVisualizer` et toutes les politiques de `Images/Visualization`.

Les fichiers générés sous `bin` et `obj` ne font pas partie de l’audit source.

## Conclusion pour la discussion du 1.1

Le code justifie une organisation principale par étapes techniques, avec des sous-dossiers par famille uniquement à l’intérieur des codecs, lecteurs, reconstructeurs et systèmes de fichiers concernés.

La structure cible ne doit pas être décidée à partir des seuls noms actuels : `Images` mélange encore conteneurs, détection, exploration, conversion et visualisation, tandis que `SectorImages` mélange modèle sectoriel et reconstruction depuis SCP. Ces deux dossiers sont les principaux candidats à une redistribution.

Avant d’inscrire la structure comme règle dans le document de tâches, il reste à décider avec l’utilisateur les noms précis des dossiers et le niveau de regroupement des contrats/modèles/registres. Les responsabilités, elles, sont maintenant établies par le code et documentées ici.

## Matrice des axes réellement présents dans les lecteurs

La lecture croisée des politiques et des lecteurs montre que « format d’image », « encodage » et « machine » sont trois axes différents. Ils ne peuvent pas former une seule hiérarchie de dossiers sans créer des classements faux.

| Entrée | Nature réellement observée | Représentation ou traitement | Machine ou famille | Mélange actuel à corriger |
|---|---|---|---|---|
| SCP | conteneur de captures de flux | révolutions et intervalles de flux, puis décodage choisi séparément | plusieurs familles | `ScpContainerPolicy` déclenche directement l’exploration et le système de fichiers |
| 2IMG | conteneur avec métadonnées et charge utile | secteurs DOS/ProDOS ou pistes NIB selon l’en-tête | principalement Apple II/III | le lecteur du conteneur choisit immédiatement le décodeur ou le lecteur sectoriel final |
| DiskCopy | conteneur avec données et tags | secteurs ou secteurs tagués | Macintosh, Lisa ou ProDOS | reconnaissance par le mot privé `0x0100` à l’offset 82, puis validation des longueurs et checksums par `DiskCopyReader` |
| NIB | représentation brute de pistes nibblisées, pas une machine | découpage par `NibTrackImageReader`, puis décodage GCR Apple II ou RWTS18 | Apple II | longueur et erreurs NIB sont isolées dans `NibTrackFormat` et `NibTrackExceptions` |
| WOZ | conteneur de pistes sous forme de flux de bits | validation par `WozReader`, extraction du bitstream, puis codec Apple II | Apple II | signatures, disposition, CRC32 et erreurs sont isolés dans les définitions du module WOZ |
| CPCEMU DSK/EDSK | conteneur structuré de pistes et secteurs | secteurs déjà décrits par l’en-tête | généralement CPC ou PCW | le lecteur déduit immédiatement l’identifiant machine depuis la géométrie |
| `.dsk` brut | extension ambiguë, pas un format unique | secteurs bruts dont l’ordre et la géométrie doivent être interprétés | Apple, MSX et autres | plusieurs politiques se disputent la même extension selon un ordre fonctionnel |
| `.img`, `.ima` ou `.bin` bruts | extensions ambiguës, pas des formats uniques | secteurs bruts | IBM PC, Macintosh, Lisa, Amstrad, DEC, Coherent et autres | la sélection dépend de signatures de systèmes de fichiers placées dans les politiques de conteneurs |
| ADF | convention de fichier sectoriel brut | géométrie déduite principalement de la taille | Acorn ou Amiga | un même lecteur choisit la famille à partir de la taille ; ADF ne peut donc pas être rangé sous une marque unique |
| ST | image sectorielle brute à géométrie Atari ST | secteurs bruts et BPB éventuel | Atari ST | géométrie, détection et création de l’image sont réunies |
| MSA | conteneur compressé de pistes sectorielles | décompression puis secteurs Atari ST | Atari ST | le lien machine est intrinsèque au format, mais pas à l’encodage MFM générique |
| ATR | conteneur avec en-tête et secteurs | secteurs Atari 8-bit | Atari 8-bit | parsing et validation dans `Containers/Atari/Atr`; extraction de la charge utile dans `Conversion/Atari` |
| SSD/DSD | convention d’ordre de pistes et secteurs | secteurs DFS | BBC/Acorn | lecture physique et identification DFS sont étroitement liées |
| D64/D71/D81 | formats sectoriels à géométrie définie | secteurs et éventuelles cartes d’erreurs | Commodore | géométries communes utiles, sans justifier un classement global par marque |
| IMD et TD0 | conteneurs structurés de pistes et secteurs | secteurs, marques et métadonnées | plusieurs machines et géométries | parsing du conteneur et classement final sont actuellement couplés |
| 86F | conteneur de pistes encodées | bitstream puis FM ou MFM selon les drapeaux | plusieurs géométries de type PC | le lecteur parse, décode et reconstruit directement un `SectorImage` |
| CP2 | conteneur structuré de descripteurs et blocs de secteurs | secteurs décrits par piste | géométrie détectée ensuite | parsing et interprétation sont encore regroupés |
| RX02 brut | ordre physique particulier de secteurs | remise en ordre vers des blocs logiques | DEC, souvent RT-11 | la signature RT-11 participe à la détection du lecteur physique |
| Coherent brut | image sectorielle identifiée par son contenu | secteurs, avec géométrie zonée pour Commodore 900 | plateformes Coherent, dont C900 | `CoherentImageRecognitionPolicy` utilise le superbloc indépendamment de l’extension ; le Reader valide ensuite taille et géométrie |

### Conséquence architecturale établie par cette matrice

Le premier niveau ne doit être ni la marque, ni l’extension, ni FM/MFM. Le découpage cohérent suit le pipeline :

1. **Conteneurs** : lire uniquement l’enveloppe et ses métadonnées ;
2. **Représentations** : flux, bitstream, pistes sectorielles ou charge utile sectorielle brute ;
3. **Décodage/encodage** : FM, MFM, GCR et autres codecs indépendants de la marque lorsque l’algorithme est partagé ;
4. **Interprétation et reconstruction** : géométrie, ordre des secteurs et règles propres à un format ou à une famille ;
5. **Détection** : combiner extension, en-tête, taille, géométrie, signatures et résultats de décodage sans faire de l’extension le propriétaire du format ;
6. **Systèmes de fichiers** : intervenir seulement après obtention d’une image sectorielle interprétée ;
7. **Exploration, conversion et visualisation** : orchestrer ou projeter les résultats sans être appelées depuis un parser de conteneur.

Les sous-dossiers par machine ne sont donc justifiés qu’au niveau des définitions, détecteurs ou reconstructeurs réellement spécialisés. FM/MFM restent des codecs transversaux. Les noms `dsk`, `img` et `bin` ne peuvent pas devenir des modules de format puisqu’ils recouvrent plusieurs interprétations incompatibles.

Cette conclusion implique aussi de revoir le contrat actuel `ISectorImageReader` : imposer à tous les lecteurs de produire immédiatement un `SectorImage` classé force certains parsers à faire en même temps la détection, le décodage et la reconstruction. La cible devra permettre à un lecteur de conteneur de produire une représentation intermédiaire neutre, puis de confier les étapes suivantes aux composants appropriés.

## Relecture exhaustive utilisée pour le document de tâches

La relecture du 10 août 2026 a parcouru les 215 fichiers C# de production actuellement retournés sous `src/GWGUI.MediaEngine`, hors `bin` et `obj`. Pour chaque fichier, les types, membres, données statiques, textes d’exception et responsabilités visibles ont été contrôlés. Le renommage prioritaire décidé pour ce moteur est `GWGUI.MediaEngine`, afin de produire `GWGUI.MediaEngine.dll`. Cette passe confirme notamment les points suivants, désormais traduits en tâches précises dans `docs/tasks/02-gwgui-scp.md` :

- `DiskImageRecognitionRegistry.ReadAsync` poursuit maintenant la recherche lorsqu’un Reader présélectionné rejette le contenu comme invalide ou non pris en charge ; l’annulation et les erreurs d’accès restent propagées.
- `DirectContainerPolicy` et `DelegatingContainerPolicy` dupliquent la même sélection par extension et ne diffèrent que par la forme de l'appel au Reader.
- `DiskImageExplorerFactory` est la racine de composition actuelle : elle construit le Reader SCP, les registres, les reconstructeurs, les politiques de conteneurs et l'exploration.
- `RawImgContainerPolicy` choisit IBM par défaut et appelle directement deux détecteurs logiques de `AmstradCpmFileSystemReader`, ce qui couple reconnaissance physique et système de fichiers.
- `ScpContainerPolicy` appelle directement `ScpImageExplorationService`, ce qui couple parsing du conteneur et exploration.
- `SectorImage.cs`, `FluxDecodeModels.cs`, `TrackEncodeModels.cs`, `FileSystemModels.cs` et `ExploredDiskImage.cs` déclarent encore plusieurs types séparables. Les anciens `ScpModels.cs` et `ScpCaptureInfo.cs` ont déjà été découpés.
- `AppleIIGcrDecoder.cs` déclare le type `AppleGcrDecoder`; le fichier et le type doivent être réalignés.
- `FluxEncoding.cs` recopie des primitives FM/MFM déjà présentes sous une autre forme dans `TrackEncoding.cs`.
- `SectorImageInterpretation.cs` regroupe retagging générique, détection de programme Atari ST, lecture de BPB FAT et création d'une interprétation MSX.
- `DiskSystemCatalog.cs`, `DiskProtectionCatalog.cs` et `DiskImageMetadata.cs` mélangent identifiants techniques, correspondances de formats et textes directement affichables.
- `EpsonQx10ScpSectorImageReader` ainsi que les façades Amstrad, BBC, IBM et UCSD délèguent au pipeline ISO et peuvent être supprimés après raccordement direct.
- `CpmFileSystemReader.cs` et `AmstradCpmFileSystemReader.cs` dupliquent le décodage des noms, extents, allocations et assemblages de fichiers tout en nécessitant des layouts distincts.
- `MacMfsFileSystemReader.cs` et `MacHfsFileSystemReader.cs` partagent l'époque Macintosh, les lectures big-endian et les chaînes Pascal, mais pas leurs structures de catalogue et d'allocation.
- `SectorImageFluxVisualizer.cs` construit son registre de politiques, choisit les encodeurs par chaînes et fabrique directement un en-tête SCP de visualisation.

Les 215 noms de fichiers de production actuels apparaissent maintenant dans `docs/tasks/02-gwgui-scp.md`, y compris `TwoImgImageFormat.cs`, `WozReader.cs` et `NibTrackImageReader.cs` créés après la passe précédente.
