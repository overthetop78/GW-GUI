# Architecture des médias et images de disquette

## Chaîne de traitement actuelle

Ce document décrit l'organisation fonctionnelle actuellement composée par `GWGUI.MediaEngine`. Les catalogues et registres du code restent les sources de vérité lorsqu'un format ou un lecteur est ajouté.

### Couches distinctes

```text
Fichier ou matériel
        ↓
Lecteur de conteneur ou Greaseweazle
        ↓
Flux, pistes et révolutions
        ↓
Décodeur de piste (FM, MFM, GCR, MMFM…)
        ↓
Candidats secteurs, structures et anomalies
        ↓
Reconstruction d'image sectorielle et géométrie
        ↓
Lecteur de système de fichiers
        ↓
Volume, dossiers, fichiers, attributs et avertissements
        ↓
Projection Explorateur, Visualisateur ou Conversion
```

Ces couches ne sont pas interchangeables. Un décodeur de flux ne lit pas un répertoire, un lecteur de système de fichiers ne reconstruit pas les timings SCP et un encodeur de piste ne crée pas à lui seul un conteneur de sortie.

### Composition actuelle

`MediaEngineFactory` compose les services utilisés par le moteur :

- `DiskImageRecognitionRegistry` ordonne les politiques de reconnaissance des images directes ;
- `ScpImageExplorationService` sépare l'exploration automatique d'une capture SCP de sa reconstruction sectorielle explicite ;
- `FluxDecoderRegistry` et `FluxEncoderRegistry` fournissent les codecs de pistes ;
- `FileSystemRegistry` exécute les lecteurs de systèmes de fichiers compatibles ;
- `DiskImageInterpretationService` applique les normalisations et interprétations supplémentaires ;
- `DiskImageDocumentFactory` construit le document présenté par l'Explorateur.

`DiskImageExplorer` orchestre ces services. Il ne contient plus directement le catalogue des lecteurs ni les algorithmes de reconstruction.

### Reconnaissance des conteneurs et images

La composition actuelle enregistre des politiques pour les familles suivantes :

- ADF ;
- SSD et DSD ;
- Coherent ;
- DEC RX02 ;
- Atari ST, MSA et ATR ;
- Commodore D64, D71 et D81 ;
- images Apple ;
- images MSX ;
- CPC DSK et EDSK ;
- images brutes IMG et IMA ;
- TD0, 86F, CP2 et IMD ;
- SCP.

Une extension sert d'indice pour certaines politiques, mais les formats ambigus utilisent des sondes ou des politiques spécialisées. Les politiques sont essayées dans l'ordre explicite défini par `MediaEngineFactory`. La première lecture entièrement validée est retenue. Si plusieurs candidats rejettent le contenu, leurs échecs sont conservés dans `DiskImageCandidatesRejectedException`.

### Décodeurs et encodeurs de pistes

`FluxDecoderCatalog` fournit actuellement 25 décodeurs. `FluxEncoderCatalog` fournit 24 encodeurs correspondants. Le flux brut possède un décodeur, mais aucun encodeur sectoriel correspondant.

Les familles couvertes comprennent ISO FM/MFM, Amiga MFM, Apple II GCR, Apple RWTS18, Apple Macintosh GCR, Lisa FileWare, Commodore GCR et Commodore 900, DEC RX02 ainsi que les autres codecs spécialisés enregistrés dans ces deux catalogues.

Chaque codec reste séparé dans son propre fichier. Les catalogues définissent leur ordre public ; les registres valident les identifiants et fournissent la sélection.

`FluxDecoderRegistry` met en cache le résultat d'un décodeur par révolution et par identifiant de codec au moyen d'un cache faible attaché à la révolution.

### Reconstruction des captures SCP

La composition actuelle utilise six reconstructeurs principaux :

- `IsoScpSectorImageReader` pour la famille ISO FM/MFM partagée ;
- `AmigaScpSectorImageReader` ;
- `AtariScpSectorImageReader`, qui adapte les formats Atari au reconstructeur ISO commun ;
- `CommodoreScpSectorImageReader` ;
- `AppleScpSectorImageReader` ;
- `DecRx02ScpSectorImageReader`.

`ScpCandidateRegistry` associe les identifiants de formats aux candidats adaptés. `ScpFamilyProbe` détermine les familles plausibles pour l'exploration automatique. Une sélection explicite passe par le candidat enregistré pour l'identifiant demandé.

### Systèmes de fichiers

`FileSystemReaderCatalog` fournit actuellement 18 lecteurs :

- AmigaDOS et archives de ressources Amiga plates ;
- Acorn ADFS/FileCore et BBC DFS ;
- Coherent ;
- RT-11 et UCSD ;
- Apple Inform/Xzip, DOS, ProDOS, Macintosh MFS, Macintosh HFS et Lisa ;
- CP/M Amstrad et CP/M générique ;
- Commodore DOS ;
- FAT12 ;
- Atari DOS.

`FileSystemRegistry.ReadCandidates` exécute tous les lecteurs correspondant à l'identifiant demandé. `TryRead` retourne la première correspondance du rapport obtenu. L'ordre du catalogue reste donc significatif lorsqu'un appel utilise `TryRead`.

### Détection automatique actuelle

Pour une capture portant réellement la signature SCP et sans format imposé, `DiskImageExplorer` délègue à l'exploration automatique SCP.

Pour les autres images :

1. `DiskImageRecognitionRegistry` choisit la première politique qui lit entièrement le contenu ;
2. `FileSystemRegistry` recherche les systèmes de fichiers associés au format reconnu ;
3. `DiskImageInterpretationService` produit les interprétations supplémentaires compatibles ;
4. les résultats identiques sont dédupliqués ;
5. `DiskImageDocumentFactory` construit le document final.

Si aucune politique ne reconnaît le fichier, l'Explorateur construit un document inconnu. Une annulation est propagée pendant la reconnaissance et l'exploration.

### Choix explicite actuel

Lorsqu'un identifiant de format est fourni, le registre de reconnaissance le transmet aux politiques. L'image reconnue est ensuite associée à cet identifiant et `FileSystemRegistry.TryRead` retient la première lecture compatible.

Ce chemin explicite ne conserve donc pas actuellement toutes les autres interprétations possibles. Toute évolution de ce comportement doit être décidée séparément avant modification.

### Parcours fonctionnels

#### Lecture

La lecture pilote Greaseweazle, suit la progression et produit un fichier. Une nouvelle commande doit réinitialiser l'état de progression ; une annulation doit nettoyer toute sortie partielle.

#### Écriture

L'écriture reconnaît le fichier source, applique la classification choisie, construit la commande Greaseweazle et suit sa progression.

#### Conversion

La conversion reconnaît la source puis dirige chaque sortie vers Greaseweazle ou vers un service interne explicitement pris en charge. Les compatibilités de sortie proviennent des catalogues du produit.

#### Visualisateur

Le Visualisateur charge une capture de flux directement ou construit une représentation de flux depuis une image sectorielle. Le rendu reste une projection distincte des données décodées.

#### Explorateur

L'Explorateur reconnaît le conteneur, reconstruit éventuellement une image sectorielle, lit les systèmes de fichiers puis construit les documents, dossiers, fichiers, détails et avertissements. Lorsqu'aucun système de fichiers n'est reconnu, il ne doit pas inventer de noms de fichiers.

### Données partagées et recalculs

Les données techniques réutilisables sont le conteneur analysé, les pistes et révolutions, les résultats de codecs, les secteurs reconstruits et les interprétations reconnues. Le rendu bitmap, l'arbre de fichiers, les panneaux de détails et le texte localisé restent des projections séparées.

Le cache de décodage par révolution existe dans `FluxDecoderRegistry`. Aucun cache global de document par chemin n'est décrit ici tant qu'un tel mécanisme n'a pas été vérifié dans le code.

## Contrat complet d’une image de disquette

Ce document décrit la structure complète retournée après l'ouverture d'une image disquette, quel que soit son format et quel que soit l'écran qui l'utilise.

### Schéma complet relié

```text
IImageDisquette
├── TypeImage                         : string                  // Conteneur ouvert : SCP, ADF, ST, IMA…
├── VersionImage                      : int?                    // Version de l'en-tête source ; null si le format n'en possède pas.
├── TailleImage                       : long                    // Taille totale de l'image en octets.
├── MetadonneesImage                  : IMetadonneesImage       // Informations générales du conteneur ouvert.
│   ├── Signature                     : string?                 // Signature lue dans l'en-tête, par exemple SCP.
│   ├── TypeDisquette                 : string?                 // Type déclaré par le conteneur, s'il existe.
│   ├── ResolutionNanosecondes        : int?                    // Résolution temporelle du flux, si applicable.
│   ├── NombreRevolutions             : int?                    // Nombre de tours enregistrés par piste, si applicable.
│   ├── PremierePiste                 : int?                    // Première piste déclarée dans l'image.
│   ├── DernierePiste                 : int?                    // Dernière piste déclarée dans l'image.
│   ├── NombrePistes                  : int                     // Nombre total de pistes présentes dans l'image.
│   ├── NombreFaces                   : int?                    // Nombre de faces déclaré ou déterminé.
│   ├── ChecksumPresent               : bool                    // Indique si le conteneur possède une somme de contrôle.
│   ├── ChecksumDeclare               : string?                 // Valeur inscrite dans le conteneur ; null si absente.
│   ├── ChecksumCalcule               : string?                 // Valeur recalculée par le moteur ; null si non calculable.
│   ├── ChecksumValide                : bool?                   // Résultat du contrôle ; null en l'absence de checksum.
│   └── ProprietesFormat              : Dictionary<string,string> // Valeurs d'en-tête propres au format non représentées ailleurs.
├── Pistes                            : IPiste[]                // Toutes les pistes physiques ou logiques de l'image.
│   └── IPiste
│       ├── NumeroSource              : int?                    // Numéro brut de la piste dans le conteneur ; null s'il n'existe pas.
│       ├── Cylindre                  : int                     // Numéro du cylindre.
│       ├── Face                      : int                     // Numéro de la face.
│       ├── Revolutions               : IRevolution[]           // Tours capturés ; vide pour une image purement sectorielle.
│       │   └── IRevolution
│       │       ├── Numero            : int                     // Position du tour dans la capture.
│       │       ├── DebutIndex        : long                    // Position temporelle cumulée du début du tour dans la piste, en nanosecondes.
│       │       ├── DureeNanosecondes : long                    // Durée complète du tour.
│       │       ├── Resolution        : int                     // Résolution temporelle des échantillons.
│       │       ├── NombreFluxDeclare : uint?                   // Nombre de mots de flux annoncé par le conteneur ; null si absent.
│       │       ├── Origine           : string                  // Origine de ce tour : capturé ou synthétique.
│       │       └── TransitionsFlux   : uint[]                  // Intervalles entre les transitions magnétiques.
│       └── SecteursSource            : ISecteurSource[]        // Secteurs directement présents dans ADF, ST, IMA… ; vide pour du flux non décodé.
│           └── ISecteurSource
│               ├── Numero            : int                     // Numéro logique du secteur.
│               ├── Taille            : int                     // Taille du secteur en octets.
│               └── Donnees           : byte[]                  // Contenu exact du secteur.
├── FormatsDetectes                   : IFormatDetecte[]        // Tous les formats réellement reconnus, sans format prioritaire.
│   └── IFormatDetecte
│       ├── MachineId                 : string                  // Identifiant technique stable de la machine reconnue : amiga, atari-st, ibm-pc…
│       ├── FormatId                  : string                  // Identifiant technique stable du format reconnu : amiga.amigados.880, atarist.720…
│       ├── Encodage                  : string                  // Encodage reconnu : Amiga MFM, IBM MFM, FM…
│       ├── Cylindres                 : int                     // Nombre de cylindres de cette interprétation.
│       ├── Faces                     : int                     // Nombre de faces de cette interprétation.
│       ├── SecteursParPiste          : int?                    // Valeur nominale ; null si elle varie selon les pistes.
│       ├── TailleSecteur             : int?                    // Valeur nominale ; null si elle varie selon les secteurs.
│       ├── CapaciteOctets            : long                    // Capacité logique de cette interprétation.
│       ├── NombreSecteursValides     : int                     // Nombre total de secteurs correctement décodés pour ce format.
│       ├── NombreSecteursInvalides   : int                     // Nombre total de secteurs présents mais non validés pour ce format.
│       ├── NombreSecteursAbsents     : int                     // Nombre total de secteurs attendus mais introuvables pour ce format.
│       ├── Secteurs                  : ISecteur[]              // Résultat final du décodage pour ce format.
│       │   └── ISecteur
│       │       ├── BlocLogique       : int                     // Position logique du secteur dans l'image reconstruite.
│       │       ├── Cylindre          : int                     // Cylindre logique.
│       │       ├── Face              : int                     // Face logique.
│       │       ├── Numero            : int                     // Numéro logique.
│       │       ├── Taille            : int                     // Taille attendue en octets.
│       │       ├── Etat              : string                  // Disponible, invalide ou absent.
│       │       ├── Donnees           : byte[]                  // Données finales ; vide si le secteur est absent.
│       │       ├── EnteteValide      : bool?                   // Validité de l'en-tête ; null si non applicable.
│       │       ├── DonneesValides    : bool?                   // Validité des données ; null si non applicable.
│       │       ├── Tag               : byte[]?                 // Métadonnées sectorielles natives ; null si absentes.
│       │       ├── CodeFormat        : byte?                   // Code de format sectoriel source ; null si absent.
│       │       ├── CodeDiagnostic    : byte?                   // Code de diagnostic source ; null si absent.
│       │       └── Revolutions       : int[]                   // Tours ayant fourni ou confirmé ce secteur.
│       ├── SystemeFichiers           : string?                 // amigados.ofs, amigados.ffs, fat12… ; null si aucun catalogue.
│       ├── NomVolume                 : string?                 // Nom réellement inscrit ; null si le volume n'en possède pas.
│       ├── CapaciteVolume            : long?                   // Capacité du volume ; null si aucun système de fichiers.
│       ├── EspaceUtilise             : long?                   // Espace utilisé ; null s'il est impossible à calculer.
│       ├── EspaceLibre               : long?                   // Espace libre ; null s'il est impossible à calculer.
│       ├── CreationVolume            : DateTimeOffset?         // Date de création du volume si le système de fichiers la stocke.
│       ├── ModificationVolume        : DateTimeOffset?         // Date de modification du volume si le système de fichiers la stocke.
│       ├── AttributsVolume           : string[]                // Attributs du volume réellement lus ; vide si aucun n'est disponible.
│       ├── Amorcable                 : bool?                   // true si amorçable, false si vérifié non amorçable, null si indéterminable.
│       ├── NumeroDisque              : int?                    // Numéro dans un jeu de disques, uniquement lorsqu'une source fiable le fournit.
│       ├── NombreDisques             : int?                    // Nombre total du jeu de disques, uniquement lorsqu'une source fiable le fournit.
│       ├── OrigineNumeroDisque       : string?                 // Origine de l'information : catalogue, chargeur, structure connue ou nom fourni.
│       ├── NombreEntrees             : int                     // Nombre total de fichiers et dossiers contenus dans l'arborescence.
│       ├── Organisation              : string?                 // Organisation particulière réellement identifiée.
│       ├── Chargeur                  : string?                 // Chargeur personnalisé identifié, s'il existe.
│       ├── Compactages               : string[]                // Compactages réellement détectés.
│       ├── Crack                     : string?                 // Groupe ou information de crack réellement identifié.
│       ├── Protection                : string?                 // Protection encore présente, si elle est identifiée.
│       ├── Entrees                   : IEntree[]               // Arborescence complète du format reconnu.
│           └── IEntree
│               ├── Nom              : string                  // Nom réellement lu dans le catalogue.
│               ├── Type             : string                  // Fichier, dossier ou lien.
│               ├── TypeNatifId      : string?                 // Type propre au système de fichiers ; null s'il n'est pas disponible.
│               ├── Taille           : long                    // Taille logique en octets.
│               ├── TailleOccupee     : long?                   // Espace occupé ; null si inconnu.
│               ├── Creation         : DateTimeOffset?         // Date de création avec son décalage horaire, si elle est stockée.
│               ├── Modification     : DateTimeOffset?         // Date de modification avec son décalage horaire, si elle est stockée.
│               ├── Acces            : DateTimeOffset?         // Date d'accès avec son décalage horaire, si elle est stockée.
│               ├── Commentaire      : string?                 // Commentaire réellement stocké par le système de fichiers.
│               ├── Attributs        : string[]                // Attributs réellement stockés.
│               ├── AttributsBruts   : uint?                   // Valeur originale des attributs avant leur interprétation.
│               ├── ReferenceStockage : long?                  // Bloc, cluster ou secteur où commence l'entrée.
│               ├── MetadonneesValides : bool                  // Indique si les métadonnées décodées sont cohérentes.
│               ├── DonneesValides   : bool?                   // Indique si le contenu complet a été lu et validé ; null si indéterminable.
│               ├── NomSynthetique   : bool                    // Indique que le nom a été construit par le moteur et ne vient pas du catalogue.
│               ├── CibleLien        : string?                 // Chemin ou identifiant ciblé par un lien ; null pour les autres entrées.
│               ├── Donnees          : byte[]?                 // null si non récupérées, vide si le fichier est réellement vide.
│               ├── Enfants          : IEntree[]               // Même structure IEntree, récursivement.
│               └── Diagnostics      : IDiagnostic[]           // Informations utiles, avertissements et erreurs propres à cette entrée.
│       └── Diagnostics              : IDiagnostic[]           // Informations utiles, avertissements et erreurs propres à ce format.
└── Diagnostics                       : IDiagnostic[]           // Résultats techniques de l'analyse.
    └── IDiagnostic
        ├── Niveau                    : string                  // Information, avertissement ou erreur.
        ├── Code                      : string                  // Identifiant stable utilisé par les traductions.
        ├── Parametres                : Dictionary<string,string> // Valeurs nécessaires pour construire le texte traduit.
        ├── Cylindre                  : int?                    // Cylindre concerné, si applicable.
        ├── Face                      : int?                    // Face concernée, si applicable.
        ├── Revolution                : int?                    // Révolution concernée, si applicable.
        └── Secteur                   : int?                    // Secteur concerné, si applicable.
```

Pour 80 cylindres et 2 faces, `Pistes` contient 160 éléments. Chaque piste est identifiée par le couple `Cylindre` de 0 à 79 et `Face` égal à 0 ou 1.

Pour une source SCP, `Revolutions` est rempli et `SecteursSource` est vide. Pour une source sectorielle comme ADF, ST ou IMA, `Revolutions` est vide et `SecteursSource` est rempli. Les secteurs reconstruits depuis un SCP sont placés dans `FormatsDetectes[].Secteurs`.

### Découpage C# correspondant

```csharp
public interface IImageDisquette
{
    string TypeImage { get; } // Conteneur ouvert : SCP, ADF, ST, IMA…
    int? VersionImage { get; } // Version de l'en-tête source ; null si le format n'en possède pas.
    long TailleImage { get; } // Taille totale de l'image en octets.
    IMetadonneesImage MetadonneesImage { get; } // Informations générales du conteneur ouvert.
    IReadOnlyList<IPiste> Pistes { get; } // Toutes les pistes physiques ou logiques de l'image.
    IReadOnlyList<IFormatDetecte> FormatsDetectes { get; } // Tous les formats réellement reconnus, sans format prioritaire.
    IReadOnlyList<IDiagnostic> Diagnostics { get; } // Résultats techniques utiles, avertissements et erreurs de l'analyse.
}

public interface IMetadonneesImage
{
    string? Signature { get; } // Signature lue dans l'en-tête, par exemple SCP.
    string? TypeDisquette { get; } // Type déclaré par le conteneur, s'il existe.
    int? ResolutionNanosecondes { get; } // Résolution temporelle du flux, si applicable.
    int? NombreRevolutions { get; } // Nombre de tours enregistrés par piste, si applicable.
    int? PremierePiste { get; } // Première piste déclarée dans l'image.
    int? DernierePiste { get; } // Dernière piste déclarée dans l'image.
    int NombrePistes { get; } // Nombre total de pistes présentes dans l'image.
    int? NombreFaces { get; } // Nombre de faces déclaré ou déterminé.
    bool ChecksumPresent { get; } // Indique si le conteneur possède une somme de contrôle.
    string? ChecksumDeclare { get; } // Valeur inscrite dans le conteneur ; null si absente.
    string? ChecksumCalcule { get; } // Valeur recalculée par le moteur ; null si non calculable.
    bool? ChecksumValide { get; } // Résultat du contrôle ; null en l'absence de checksum.
    IReadOnlyDictionary<string, string> ProprietesFormat { get; } // Valeurs d'en-tête propres au format non représentées ailleurs.
}

public interface IPiste
{
    int? NumeroSource { get; } // Numéro brut de la piste dans le conteneur ; null s'il n'existe pas.
    int Cylindre { get; } // Numéro du cylindre.
    int Face { get; } // Numéro de la face.
    IReadOnlyList<IRevolution> Revolutions { get; } // Tours capturés ; vide pour une image purement sectorielle.
    IReadOnlyList<ISecteurSource> SecteursSource { get; } // Secteurs directement présents dans ADF, ST, IMA… ; vide pour du flux non décodé.
}

public interface IRevolution
{
    int Numero { get; } // Position du tour dans la capture.
    long DebutIndex { get; } // Position temporelle cumulée du début du tour dans la piste, en nanosecondes.
    long DureeNanosecondes { get; } // Durée complète du tour.
    int Resolution { get; } // Résolution temporelle des échantillons.
    uint? NombreFluxDeclare { get; } // Nombre de mots de flux annoncé par le conteneur ; null si absent.
    string Origine { get; } // Origine de ce tour : capturé ou synthétique.
    IReadOnlyList<uint> TransitionsFlux { get; } // Intervalles entre les transitions magnétiques.
}

public interface ISecteurSource
{
    int Numero { get; } // Numéro logique du secteur.
    int Taille { get; } // Taille du secteur en octets.
    ReadOnlyMemory<byte> Donnees { get; } // Contenu exact du secteur.
}

public interface IFormatDetecte
{
    string MachineId { get; } // Identifiant technique stable de la machine reconnue : amiga, atari-st, ibm-pc…
    string FormatId { get; } // Identifiant technique stable du format reconnu : amiga.amigados.880, atarist.720…
    string Encodage { get; } // Encodage reconnu : Amiga MFM, IBM MFM, FM…
    int Cylindres { get; } // Nombre de cylindres de cette interprétation.
    int Faces { get; } // Nombre de faces de cette interprétation.
    int? SecteursParPiste { get; } // Valeur nominale ; null si elle varie selon les pistes.
    int? TailleSecteur { get; } // Valeur nominale ; null si elle varie selon les secteurs.
    long CapaciteOctets { get; } // Capacité logique de cette interprétation.
    int NombreSecteursValides { get; } // Nombre total de secteurs correctement décodés pour ce format.
    int NombreSecteursInvalides { get; } // Nombre total de secteurs présents mais non validés pour ce format.
    int NombreSecteursAbsents { get; } // Nombre total de secteurs attendus mais introuvables pour ce format.
    IReadOnlyList<ISecteur> Secteurs { get; } // Résultat final du décodage pour ce format.
    string? SystemeFichiers { get; } // amigados.ofs, amigados.ffs, fat12… ; null si aucun catalogue.
    string? NomVolume { get; } // Nom réellement inscrit ; null si le volume n'en possède pas.
    long? CapaciteVolume { get; } // Capacité du volume ; null si aucun système de fichiers.
    long? EspaceUtilise { get; } // Espace utilisé ; null s'il est impossible à calculer.
    long? EspaceLibre { get; } // Espace libre ; null s'il est impossible à calculer.
    DateTimeOffset? CreationVolume { get; } // Date de création du volume si le système de fichiers la stocke.
    DateTimeOffset? ModificationVolume { get; } // Date de modification du volume si le système de fichiers la stocke.
    IReadOnlyList<string> AttributsVolume { get; } // Attributs du volume réellement lus ; vide si aucun n'est disponible.
    bool? Amorcable { get; } // true si amorçable, false si vérifié non amorçable, null si indéterminable.
    int? NumeroDisque { get; } // Numéro dans un jeu de disques, uniquement lorsqu'une source fiable le fournit.
    int? NombreDisques { get; } // Nombre total du jeu de disques, uniquement lorsqu'une source fiable le fournit.
    string? OrigineNumeroDisque { get; } // Origine de l'information : catalogue, chargeur, structure connue ou nom fourni.
    int NombreEntrees { get; } // Nombre total de fichiers et dossiers contenus dans l'arborescence.
    string? Organisation { get; } // Organisation particulière réellement identifiée.
    string? Chargeur { get; } // Chargeur personnalisé identifié, s'il existe.
    IReadOnlyList<string> Compactages { get; } // Compactages réellement détectés.
    string? Crack { get; } // Groupe ou information de crack réellement identifié.
    string? Protection { get; } // Protection encore présente, si elle est identifiée.
    IReadOnlyList<IEntree> Entrees { get; } // Arborescence complète du format reconnu.
    IReadOnlyList<IDiagnostic> Diagnostics { get; } // Informations utiles, avertissements et erreurs propres à ce format.
}

public interface ISecteur
{
    int BlocLogique { get; } // Position logique du secteur dans l'image reconstruite.
    int Cylindre { get; } // Cylindre logique.
    int Face { get; } // Face logique.
    int Numero { get; } // Numéro logique.
    int Taille { get; } // Taille attendue en octets.
    string Etat { get; } // Disponible, invalide ou absent.
    ReadOnlyMemory<byte> Donnees { get; } // Données finales ; vide si le secteur est absent.
    bool? EnteteValide { get; } // Validité de l'en-tête ; null si non applicable.
    bool? DonneesValides { get; } // Validité des données ; null si non applicable.
    ReadOnlyMemory<byte>? Tag { get; } // Métadonnées sectorielles natives ; null si absentes.
    byte? CodeFormat { get; } // Code de format sectoriel source ; null si absent.
    byte? CodeDiagnostic { get; } // Code de diagnostic source ; null si absent.
    IReadOnlyList<int> Revolutions { get; } // Tours ayant fourni ou confirmé ce secteur.
}

public interface IEntree
{
    string Nom { get; } // Nom réellement lu dans le catalogue.
    string Type { get; } // Fichier, dossier ou lien.
    string? TypeNatifId { get; } // Type propre au système de fichiers ; null s'il n'est pas disponible.
    long Taille { get; } // Taille logique en octets.
    long? TailleOccupee { get; } // Espace occupé ; null si inconnu.
    DateTimeOffset? Creation { get; } // Date de création avec son décalage horaire, si elle est stockée.
    DateTimeOffset? Modification { get; } // Date de modification avec son décalage horaire, si elle est stockée.
    DateTimeOffset? Acces { get; } // Date d'accès avec son décalage horaire, si elle est stockée.
    string? Commentaire { get; } // Commentaire réellement stocké par le système de fichiers.
    IReadOnlyList<string> Attributs { get; } // Attributs réellement stockés.
    uint? AttributsBruts { get; } // Valeur originale des attributs avant leur interprétation.
    long? ReferenceStockage { get; } // Bloc, cluster ou secteur où commence l'entrée.
    bool MetadonneesValides { get; } // Indique si les métadonnées décodées sont cohérentes.
    bool? DonneesValides { get; } // Indique si le contenu complet a été lu et validé ; null si indéterminable.
    bool NomSynthetique { get; } // Indique que le nom a été construit par le moteur et ne vient pas du catalogue.
    string? CibleLien { get; } // Chemin ou identifiant ciblé par un lien ; null pour les autres entrées.
    ReadOnlyMemory<byte>? Donnees { get; } // null si non récupérées, vide si le fichier est réellement vide.
    IReadOnlyList<IEntree> Enfants { get; } // Même structure IEntree, récursivement.
    IReadOnlyList<IDiagnostic> Diagnostics { get; } // Informations utiles, avertissements et erreurs propres à cette entrée.
}

public interface IDiagnostic
{
    string Niveau { get; } // Information, avertissement ou erreur.
    string Code { get; } // Identifiant stable utilisé par les traductions.
    IReadOnlyDictionary<string, string> Parametres { get; } // Valeurs nécessaires pour construire le texte traduit.
    int? Cylindre { get; } // Cylindre concerné, si applicable.
    int? Face { get; } // Face concernée, si applicable.
    int? Revolution { get; } // Révolution concernée, si applicable.
    int? Secteur { get; } // Secteur concerné, si applicable.
}
```

### Données disponibles pendant la lecture physique

`IImageDisquette` est le résultat final. Pendant l'acquisition, l'application reçoit successivement des objets `IEtatLectureDisquette` :

```text
IEtatLectureDisquette
├── Etape                         : string                  // Acquisition, sauvegarde, décodage ou exploration.
├── NombrePistesTerminees         : int                     // Nombre de pistes dont la lecture est terminée.
├── NombrePistesTotal             : int                     // Nombre total de pistes demandées.
├── Cylindre                      : int?                    // Cylindre actuellement traité ; null hors traitement d'une piste.
├── Face                          : int?                    // Face actuellement traitée ; null hors traitement d'une piste.
├── Tentative                     : int                     // Numéro de la tentative de lecture de la piste courante.
├── PisteAcquise                  : IPiste?                 // Piste et révolutions venant d'être capturées ; null si aucune nouvelle piste.
├── EtatsPistes                   : IEtatPisteLecture[]      // État courant de toutes les cases affichées dans les barres des faces.
│   └── IEtatPisteLecture
│       ├── Cylindre              : int                     // Cylindre représenté par la case.
│       ├── Face                  : int                     // Face représentée par la case.
│       ├── Etat                  : string                  // En attente, active, réussie, nouvelle tentative ou échouée.
│       └── Tentatives            : int                     // Nombre de tentatives déjà effectuées.
├── CodeMessage                   : string?                 // Identifiant traduisible du message courant ; null si aucun message.
├── ParametresMessage             : Dictionary<string,string> // Valeurs du message traduisible.
└── MessageExterne                : string?                 // Ligne brute de gw.exe lorsqu'elle ne possède pas d'équivalent structuré.
```

```csharp
public interface IEtatLectureDisquette
{
    string Etape { get; } // Acquisition, sauvegarde, décodage ou exploration.
    int NombrePistesTerminees { get; } // Nombre de pistes dont la lecture est terminée.
    int NombrePistesTotal { get; } // Nombre total de pistes demandées.
    int? Cylindre { get; } // Cylindre actuellement traité ; null hors traitement d'une piste.
    int? Face { get; } // Face actuellement traitée ; null hors traitement d'une piste.
    int Tentative { get; } // Numéro de la tentative de lecture de la piste courante.
    IPiste? PisteAcquise { get; } // Piste et révolutions venant d'être capturées ; null si aucune nouvelle piste.
    IReadOnlyList<IEtatPisteLecture> EtatsPistes { get; } // État courant de toutes les cases affichées dans les barres des faces.
    string? CodeMessage { get; } // Identifiant traduisible du message courant ; null si aucun message.
    IReadOnlyDictionary<string, string> ParametresMessage { get; } // Valeurs du message traduisible.
    string? MessageExterne { get; } // Ligne brute de gw.exe lorsqu'elle ne possède pas d'équivalent structuré.
}

public interface IEtatPisteLecture
{
    int Cylindre { get; } // Cylindre représenté par la case.
    int Face { get; } // Face représentée par la case.
    string Etat { get; } // En attente, active, réussie, nouvelle tentative ou échouée.
    int Tentatives { get; } // Nombre de tentatives déjà effectuées.
}
```

Le moteur interne peut fournir `PisteAcquise` immédiatement après chaque piste, car les flux sont déjà dans sa mémoire. Avec `gw.exe`, la sortie standard permet d'alimenter l'étape, les compteurs, les barres et `MessageExterne`, mais les flux de `PisteAcquise` ne deviennent disponibles qu'après la fin de la commande et la relecture du fichier produit.

## Compléments au contrat

### Vocabulaire

- **Source** : fichier ouvert, par exemple SCP, ADF, ST, IMA ou IMG.
- **Conteneur** : organisation du fichier source. SCP est un conteneur de flux ; ADF et ST sont généralement des images sectorielles linéaires.
- **Données physiques** : pistes, faces, révolutions et intervalles de flux disponibles dans une capture comme SCP.
- **Interprétation** : résultat complet obtenu en essayant une machine et un format précis.
- **Image sectorielle** : secteurs reconstruits pour une interprétation précise.
- **Volume** : système de fichiers reconnu, son nom, ses dossiers et ses fichiers.
- **Interprétation affichée** : interprétation choisie par l'application. Le moteur ne déclare aucun format préférable aux autres.

### Responsabilités entre moteur et application

#### Moteur `GWGUI.MediaEngine`

Le moteur doit :

- essayer les candidats de format ;
- conserver chaque reconstruction sectorielle séparément ;
- utiliser uniquement les lecteurs compatibles avec le candidat courant ;
- construire une interprétation complète par résultat reconnu ;
- retourner les interprétations dans l'ordre où elles ont été validées ;
- retourner les identifiants techniques, jamais les textes traduits de l'interface.

#### Application `GWGUI.App`

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

### Invariants à tester

1. Chaque `DiskInterpretation.FormatId` est identique au `FormatId` de sa `SectorImage`.
2. Chaque volume est produit en lisant la `SectorImage` contenue dans la même interprétation.
3. Une interprétation Amiga ne peut pas contenir un volume FAT12 obtenu depuis le candidat Atari.
4. Une interprétation Atari ST ne peut pas contenir un volume AmigaDOS obtenu depuis le candidat Amiga.
5. La ligne « Détecté » contient tous les couples reconnus, sans doublon.
6. Les éléments colorés dans les sélecteurs correspondent exactement aux couples présents dans le tableau.
7. Le champ « Système », le volume, la capacité et les fichiers proviennent tous du même élément sélectionné.
8. Les pistes et révolutions SCP restent inchangées quelle que soit l'interprétation sélectionnée.
