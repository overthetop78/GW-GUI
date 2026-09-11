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

Les Readers actuellement enregistrés couvrent les formats de disquettes déjà gérés. SCP produit
une représentation `Flux`; ADF, ATR, ST, MSA, IMA et les autres images sectorielles prises en
charge produisent directement une représentation `Sectors`.

## Document commun

Un Reader retourne un `MediaImageDocument`. Ce document rassemble :

- la source principale et ses fichiers associés ;
- l’identifiant exact du format ;
- la famille de média ;
- la représentation réellement conservée par l’image ;
- les volumes connus ;
- les diagnostics de lecture ;
- les métadonnées particulières qu’un autre traitement doit préserver.

Les représentations sont indépendantes du format de conteneur. Le moteur possède actuellement les
formes `Flux`, `Sectors`, `Blocks`, `OpticalTracks` et `Sequential`; seuls les Readers de disquettes
et les traitements `Flux` et `Sectors` sont raccordés au parcours complet à ce stade.

Le même `MediaImageDocument` peut alimenter l’Explorateur, la préparation du Visualiseur et la
conversion. Il n’est pas nécessaire de recommencer la reconnaissance entre ces opérations.

## Exploration

`MediaExplorer` reçoit un document déjà reconnu. Il utilise ses volumes déclarés ou, lorsque le
format n’en fournit aucun, crée un volume couvrant la longueur logique connue du média.
`FileSystemRegistry` sélectionne ensuite les Readers de systèmes de fichiers compatibles avec la
représentation réelle du document. Chaque résultat conserve le volume, le système de fichiers
reconnu, son arborescence et ses diagnostics.

Dans l’application, `DiskImageWorkspaceController` conserve ce résultat commun pour le
Visualiseur et l’Explorateur. Le panneau Explorer sait présenter les volumes et entrées communs
sans exiger l’ancien modèle `ExploredDiskImage`.

## Préparation de la visualisation

`MediaVisualizationProviderRegistry` sélectionne un `IMediaVisualizationProvider` à partir de la
représentation du document et de ses capacités, et non à partir de son extension.

Deux fournisseurs sont actuellement enregistrés :

- `FluxMediaVisualizationProvider` décrit les surfaces et les pistes réellement présentes dans la
  capture, dans l’ordre défini par la source ;
- `SectorMediaVisualizationProvider` décrit les surfaces, cylindres et pistes d’une image
  sectorielle dans l’ordre croissant, sans créer de faux flux SCP.

Ils produisent un `MediaVisualizationDescriptor` qui indique la représentation, les surfaces,
l’unité de progression, la direction et les éléments à préparer. Ce descripteur sépare les données
du média du dessin WPF ou Skia, qui reste dans `GWGUI.App`.

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
délèguent l’écriture aux Writers spécialisés existants. Les métadonnées indispensables à une
réécriture fidèle, notamment celles de SCP, HFE, DiskCopy et ImageDisk, restent attachées au
document commun.

`ConversionBatchExecutor` orchestre ce parcours depuis l’application sans instancier ni choisir
les convertisseurs spécialisés. La conversion automatique utilisée par les modules Amiga et Atari
réutilise les reconstructeurs partagés de la composition, tandis que chaque module conserve le
choix du format accepté par son émulateur et la gestion de son média temporaire.

## Points d’intégration

`MainWindow` crée une composition MediaEngine et injecte ses services aux contrôleurs de lecture,
conversion, exploration et visualisation concernés. `GWGUI.App` garde les vues, les présentateurs
et la coordination de l’interface. Les algorithmes de formats, de décodage, de reconstruction, de
conversion et d’écriture restent dans `GWGUI.MediaEngine`.

`GWGUI.Infrastructure` reste le point prévu pour les appareils physiques et les outils externes.
Les projets d’émulation n’ont pas été restructurés : seuls leurs appels vers le service commun ont
été adaptés. Aucun nouveau contrat public du SDK d’émulation n’a été nécessaire.

## Limites encore ouvertes

- L’ancien `MediaEngineFactory` et certains parcours de compatibilité sont encore utilisés par la
  migration de systèmes de fichiers et des tests existants ; ils seront retirés uniquement après
  le remplacement ordonné de ces consommateurs.
- `ImageFormatDetector`, `DiskImageRecognitionRegistry` et des chemins spécialisés SCP subsistent
  dans certaines fonctions de l’application. Le parcours commun est raccordé, mais leur retrait
  complet doit suivre les actions prévues avant l’extension générale des supports.
- Le rendu visible utilise encore le visualiseur SCP historique pour une partie du parcours. Les
  descripteurs Flux et Sectors sont prêts, mais les moteurs de rendu séparés et leur progression
  commune appartiennent à la phase d’affichage suivante.
- Les représentations `Blocks`, `OpticalTracks` et `Sequential` existent comme base de contrat ;
  leurs Readers, fournisseurs de visualisation, détecteurs de volumes et systèmes de fichiers
  seront ajoutés par support.
- La lecture et l’écriture de supports physiques autres que les parcours existants ne sont pas
  implémentées. Leur ajout devra passer par `GWGUI.Infrastructure` et fournir ou consommer les
  contrats neutres de MediaEngine.
- Les validations finales avec les véritables fichiers du corpus `image_test` restent séparées et
  seront effectuées manuellement après les phases d’implémentation et leurs commits prévus.
