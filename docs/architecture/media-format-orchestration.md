# Orchestration des formats de médias

Cette architecture fournit un point d’entrée commun pour reconnaître, lire, explorer, visualiser,
convertir et écrire une image de média. Elle sépare l’orchestration des algorithmes propres aux
formats et permet de réutiliser un même document lu en mémoire dans plusieurs fonctions.

## Composition du moteur

`MediaEngineComposition.CreateDefault()` assemble des compositions limitées par responsabilité :

- `MediaRecognitionComposition` enregistre les Readers et expose le registre ainsi que le service
  de lecture ;
- `MediaExplorationComposition` enregistre les Readers de systèmes de fichiers et expose
  `MediaExplorer` ;
- `MediaConversionComposition` enregistre les transformations entre représentations ;
- `MediaWritingComposition` enregistre les Writers et expose le service d’écriture ;
- `MediaVisualizationComposition` enregistre les fournisseurs de données de visualisation ;
- `ScpSectorDecodingComposition` partage les décodeurs et reconstructeurs SCP nécessaires aux
  conversions sectorielles, sans recopier leurs algorithmes.

La composition racine construit `MediaConversionService` à partir des registres de conversion et
d’écriture. Elle ne reconnaît aucun format et n’effectue elle-même aucune transformation.

## Reconnaissance et lecture

`MediaImageReadingService` construit un `MediaRecognitionContext` pour la source demandée, puis
délègue la sélection à `MediaRecognitionRegistry`.

Chaque `IMediaImageReader` déclare :

- les identifiants de formats acceptés ;
- les extensions principales et celles des fichiers associés ;
- ses signatures éventuelles ;
- les familles de médias et les représentations qu’il peut produire ;
- sa sonde et sa lecture complète spécialisées.

Le registre classe les candidats selon le format explicitement demandé, la signature, la sonde,
les fichiers associés et l’extension. Un classement identique conserve l’ordre d’enregistrement.
Le contexte partage des pages de lecture mises en cache entre les candidats afin que leurs sondes
ne rouvrent pas et ne relisent pas inutilement les mêmes zones. Le Reader sélectionné reste seul
responsable de valider et décoder son format.

Les Readers enregistrés couvrent désormais quatre familles :

- disquettes : SCP produit `Flux`; ADF, ATR, ST, MSA, IMA et les autres formats de données
  produisent `Sectors` ;
- disques durs : RAW, QCOW2, VHD, VHDX, VDI, VMDK et CHD produisent `Blocks` avec un adressage
  64 bits et une géométrie CHS uniquement lorsqu'elle est réellement connue ;
- médias optiques : ISO, BIN/CUE, CloneCD, Alcohol MDS et CHD optique produisent
  `OpticalTracks`, avec leurs fichiers associés, sessions, pistes, modes et fenêtres de données
  disponibles ;
- cassettes et bandes : WAV, UEF, CAS Atari, TZX, TAP Commodore, CAS MSX, TAP Spectrum et TAP
  SIMH produisent `Sequential`, avec durée, faces, pistes, canaux et segments disponibles.

Une extension ambiguë ne décide donc pas seule du format. La signature, la sonde et, si elle est
fournie, la famille demandée départagent les Readers concurrents.

## Document commun

Un Reader retourne un `MediaImageDocument`. Ce document rassemble :

- la source principale et ses fichiers associés ;
- l’identifiant exact du format ;
- la famille de média ;
- la représentation réellement conservée par l’image ;
- les volumes connus ;
- les diagnostics de lecture ;
- les métadonnées particulières qu’un autre traitement doit préserver.

Les représentations sont indépendantes du format de conteneur. Le moteur possède les formes
`Flux`, `Sectors`, `Blocks`, `OpticalTracks` et `Sequential`. Chacune est produite par les Readers
concernés, reconnue par le registre de visualisation et exploitable par l'Explorateur lorsque les
données nécessaires sont disponibles.

Le même `MediaImageDocument` peut alimenter l’Explorateur, la préparation du Visualiseur et la
conversion. Il n’est pas nécessaire de recommencer la reconnaissance entre ces opérations.

## Exploration

`MediaExplorer` reçoit un document déjà reconnu. `MediaVolumeDetectorRegistry` détermine les
volumes sans réinterpréter le format : contenu séquentiel, pistes optiques, partitions GPT, chaîne
MBR/EBR ou volume couvrant tout le média. `FileSystemRegistry` sélectionne ensuite les Readers de
systèmes de fichiers compatibles avec la représentation réelle du document. Outre les systèmes de
fichiers de disquettes existants, il sait lire ISO 9660 et ses extensions Joliet et Rock Ridge,
UDF, ainsi que les contenus séquentiels décodés. Chaque résultat conserve le volume, le système de
fichiers reconnu, son arborescence et ses diagnostics.

Dans l’application, `DiskImageWorkspaceController` conserve ce résultat commun pour le
Visualiseur et l’Explorateur. Le panneau Explorer sait présenter les volumes et entrées communs
sans exiger l’ancien modèle `ExploredDiskImage`.

## Préparation de la visualisation

`MediaVisualizationProviderRegistry` sélectionne un `IMediaVisualizationProvider` à partir de la
représentation du document et de ses capacités, et non à partir de son extension.

Les cinq fournisseurs enregistrés correspondent aux cinq représentations :

- `FluxMediaVisualizationProvider` décrit les surfaces et les pistes réellement présentes dans la
  capture, dans l’ordre défini par la source ;
- `SectorMediaVisualizationProvider` décrit les surfaces, cylindres et pistes d’une image
  sectorielle dans l’ordre croissant, sans créer de faux flux SCP ;
- `BlockMediaVisualizationProvider` découpe l'espace 64 bits aux limites des plages et volumes
  connus ;
- `OpticalMediaVisualizationProvider` décrit les faces, couches, sessions et pistes uniquement
  lorsqu'elles sont déclarées par l'image ;
- `SequentialMediaVisualizationProvider` répartit les segments sur des lignes distinctes selon
  leurs faces, pistes et canaux, en conservant le sens de déplacement fourni par la source.

Ils produisent un `MediaVisualizationDescriptor` qui indique la représentation, les surfaces,
l’unité de progression, la direction et les éléments à préparer. Ce descripteur sépare les données
du média du dessin WPF ou Skia, qui reste dans `GWGUI.App`. L'application possède des vues et des
présentateurs séparés pour le flux, les secteurs, les blocs, les pistes optiques et les contenus
séquentiels ; le contrôleur d'espace de travail choisit la vue à partir de la représentation du
document, jamais de son extension.

## Conversion et écriture

La chaîne de conversion réutilise un document déjà lu :

```text
MediaImageDocument
    -> MediaConversionService
    -> Writer compatible, si la représentation convient déjà
    -> ou IMediaRepresentationConverter puis Writer compatible
    -> fichier ou fichiers produits
```

`MediaImageWriterRegistry` sélectionne les Writers selon le format cible, l’extension produite et
la représentation du document. `MediaRepresentationConverterRegistry` intervient seulement si le
Writer cible exige une autre représentation. Le résultat conserve les diagnostics et les pertes
d’information déclarées par la transformation.

Les adaptateurs de Writers sectoriels et de flux extraient la représentation attendue puis
délèguent l’écriture aux Writers spécialisés existants. Les Writers enregistrés couvrent aussi RAW,
QCOW2, VHD, VHDX, VDI, VMDK et CHD pour les disques durs ; ISO et BIN/CUE pour l'optique ; WAV,
CAS Atari, TZX, TAP Spectrum, TAP Commodore, CAS MSX, UEF et TAP SIMH pour les médias séquentiels.
Les métadonnées indispensables à une réécriture fidèle, notamment celles de SCP, HFE, DiskCopy et
ImageDisk, restent attachées au document commun.

`ConversionBatchExecutor` orchestre ce parcours depuis l’application sans instancier ni choisir
les convertisseurs spécialisés. `OpticalImageConversionService` gère les destinations optiques et
`SequentialMediaConversionService` les destinations cassette/bande, avec déclaration préalable
des pertes éventuelles. La conversion automatique utilisée par les modules Amiga et Atari
réutilise les reconstructeurs partagés de la composition, tandis que chaque module conserve le
choix du format accepté par son émulateur et la gestion de son média temporaire.

## Cycle de vie propre aux images HDD

Les opérations qui concernent un ensemble de fichiers HDD restent séparées de la reconnaissance
du contenu. `DiskImageDependencyReader` expose distinctement les parents et les membres d'une
image. Les parents peuvent être désignés par un chemin, un UUID VDI ou un SHA-1 CHD ;
`DiskImageDependencyIndex` les résout dans un inventaire borné d'images voisines, détecte les
cycles et traite les segments d'un même ensemble comme une dépendance logique unique.

`DiskImageSetPublication` écrit d'abord tous les membres dans une zone temporaire appartenant à
l'opération. Il ne publie l'ensemble qu'après avoir vérifié la présence de son point d'entrée et
préserve une destination existante si la préparation échoue. La suppression suit la règle
symétrique : `HardDiskImageSetResolver` établit d'abord l'ensemble complet VMDK ou sparsebundle,
le service vérifie tous ses utilisateurs et dépendants, puis `HardDiskDeletionFile` verrouille et
valide tous les membres avant d'en supprimer un seul.

La conversion HDD ne possède pas d'orchestrateur parallèle. Elle passe par le même
`MediaConversionService` que les autres familles. Les Writers RAW, QCOW2, VHD, VHDX, VDI, VMDK
et CHD consomment la représentation `Blocks` par plages et publient leur résultat de façon
atomique. Les médias optiques et séquentiels conservent leurs Readers, Writers et services de
conversion spécialisés déjà décrits ci-dessus ; aucune fonction de cycle de vie HDD ne leur est
appliquée.

## Points d’intégration

`MainWindow` crée une composition MediaEngine et injecte ses services aux contrôleurs de lecture,
conversion, exploration et visualisation concernés. `GWGUI.App` garde les vues, les présentateurs
et la coordination de l’interface. Les algorithmes de formats, de décodage, de reconstruction, de
conversion et d’écriture restent dans `GWGUI.MediaEngine`.

`GWGUI.Infrastructure` reste le point prévu pour les appareils physiques et les outils externes.
Les projets d’émulation n’ont pas été restructurés : seuls leurs appels vers le service commun ont
été adaptés. Aucun nouveau contrat public du SDK d’émulation n’a été nécessaire.

## Limites encore ouvertes

- La reconnaissance, l'ouverture, l'exploration et la visualisation des nouvelles familles sont
  couvertes avec des images synthétiques minimales. Leur compatibilité avec les variantes réelles
  doit encore être validée avec le corpus local.
- Les métadonnées qu'un conteneur ne fournit pas restent inconnues : MediaEngine n'invente ni
  plateaux CHS, ni faces ou couches optiques, ni faces et pistes de bande.
- Les codecs séquentiels décodent les familles enregistrées ; un signal WAV inconnu reste visible
  comme signal et ne devient pas artificiellement un fichier exploré.
- L'écriture physique neutre est préparée par contrats et registres. Les appareils et outils réels
  autres que Greaseweazle devront être ajoutés dans `GWGUI.Infrastructure`, sans déplacer leurs
  protocoles dans MediaEngine ou dans l'interface.
- Les validations finales avec les véritables fichiers du corpus `image_test` restent séparées et
  seront effectuées manuellement après les phases d'implémentation et les commits prévus.
- Les validations autonomes des compléments HDD couvrent les services HDD et MediaEngine avec des
  fichiers ou documents synthétiques. Elles ne remplacent pas la vérification finale des variantes
  réelles du corpus local.
