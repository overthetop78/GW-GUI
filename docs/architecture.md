# Architecture technique actuelle

## Projets de la solution

- `GWGUI.App` : coque WPF, composition des onglets, contrôles visuels et présentation.
- `GWGUI.Domain` : modèles et règles métier indépendants de WPF et de Windows.
- `GWGUI.Infrastructure` : exécution de `gw`, matériel Windows, persistance et services externes.
- `GWGUI.Scp` : conteneurs, flux, codecs, reconstruction sectorielle, systèmes de fichiers et exploration des images.
- `GWGUI.Tests` : tests unitaires, tests d’intégration ciblés et contrôles des corpus locaux.

La cartographie antérieure au refactoring reste conservée dans [`docs/audit`](audit/README.md). Le présent document décrit l’organisation obtenue après la phase 2.

## Traitement des images de disquette

Les niveaux techniques sont séparés :

1. `Images/Containers` reconnaît et lit le conteneur source.
2. `Decoding/Decoders` transforme le flux en structures et secteurs.
3. `SectorImages` reconstruit une image sectorielle et applique les règles propres au format.
4. `FileSystems/Readers` lit le catalogue, le volume, les dossiers et les fichiers.
5. `Encoding/Encoders` transforme des secteurs en piste encodée.
6. `Images/Visualization` choisit l’encodage permettant de représenter une image sectorielle.
7. `Images/ScpDetection` limite les lecteurs à essayer, conserve les interprétations crédibles et classe les résultats automatiques.

Les registres `DiskImageContainerRegistry`, `FluxDecoderRegistry`, `FluxEncoderRegistry`, `FileSystemRegistry`, `ScpCandidateRegistry`, `IsoScpSectorImagePolicyRegistry` et `SectorImageVisualizationPolicyRegistry` centralisent leurs extensions respectives. Ajouter une famille ne demande plus d’allonger un lecteur Atari ou un explorateur monolithique.

### ISO FM/MFM et règles machine

`IsoScpSectorImageReader`, `IsoSectorImageBuilder` et les primitives ISO communes collectent et reconstruisent les secteurs. Les règles propres aux familles restent dans des politiques distinctes :

- Atari ST ;
- Atari 8 bits ;
- Amstrad ;
- IBM PC ;
- BBC/Acorn ;
- Epson QX-10 ;
- UCSD p-System.

`AtariScpSectorImageReader` ne route plus que les identifiants `atari.*` et `atarist.*`. Il ne contient plus les comportements Amstrad, IBM, BBC, Epson ou UCSD.

### Primitives partagées

Les opérations communes de bits, CRC, MFM, FM, GCR, lecture circulaire, sélection de révolution et contrôle d’intégrité sont placées dans `GWGUI.Scp/Primitives`, `Flux` et les composants communs de décodage/encodage. Les différences simples de géométrie ou d’ordre des secteurs sont fournies par des politiques ou définitions de format, sans recopier l’algorithme complet.

La création cohérente des adresses, blocs, pistes, géométries et interprétations passe notamment par `IsoSectorImageBuilder`, `SectorImageInterpretation`, `AppleSectorImageFactory` et les modèles communs de `SectorImages`.

## Détection, choix manuel et images multiformat

### Détection automatique SCP

`ScpFamilyProbe` examine un échantillon de pistes pour déterminer les familles de codecs utiles. `ScpCandidateRegistry` fournit alors uniquement les lecteurs correspondant à ces familles. Cette présélection évite d’exécuter systématiquement tous les lecteurs connus.

`ScpAutomaticImageExplorer` inspecte les candidats compatibles en parallèle. Il :

- conserve tous les systèmes de fichiers reconnus dans `DetectedFileSystems` ;
- déduplique les résultats représentant le même contenu ;
- ne supprime pas une interprétation simplement parce qu’une autre famille a obtenu un meilleur résultat ;
- place en résultat principal l’image reconnue ayant la meilleure proportion de blocs disponibles ;
- conserve comme alternatives les résultats dont les avertissements restent crédibles.

Le score de reconstruction est : `blocs disponibles / nombre total de blocs`. Pour le décodage d’une révolution, `FluxDecoderRegistry` privilégie d’abord les secteurs valides, puis la confiance et les structures reconnues ; un faux résultat composé uniquement de secteurs invalides ne passe pas devant le flux brut.

### Rôle du choix manuel

Le choix manuel oriente l’opération demandée, sans servir de preuve que l’image ne contient aucun autre système :

- **Lecture** : une image brute SCP n’ajoute aucun `--format`; une lecture au format connu transmet le format choisi à `gw`.
- **Écriture** : la détection du fichier propose un format et ses candidats ; un choix manuel remplace cette proposition pour la commande courante.
- **Conversion** : chaque sortie cochée constitue une cible distincte ; la planification, la compatibilité et l’exécution restent séparées et la multiconversion conserve toutes les sorties choisies.
- **Explorateur** : en automatique, toutes les interprétations crédibles sont conservées ; en manuel, le lecteur correspondant au format sélectionné est utilisé directement pour cette ouverture.
- **Visualisateur** : en automatique, aucun décodeur n’est forcé ; un choix manuel fixe le codec et la représentation du média correspondant au format choisi.

## Catalogue commun des formats

`ImageFormatWorkspace` possède l’unique catalogue effectif de l’application. Il combine :

- le catalogue intégré ;
- les capacités signalées par la version de Greaseweazle installée ;
- les définitions de disquette additionnelles intégrées.

Lecture, Écriture, Conversion, Explorateur et Visualisateur reçoivent ce même catalogue, puis appliquent uniquement le filtrage propre à leur opération. Les identifiants, extensions par défaut et compatibilités ne sont donc plus maintenus dans cinq listes indépendantes.

La Conversion est séparée entre `ConversionPlanner`, `ConversionCompatibilityValidator`, `ConversionOutputFactory`, `ConversionCommandBuilder` et `ConversionBatchExecutor`.

## Application WPF

`MainWindow` reste la coque et le coordinateur des événements qui relient plusieurs parties de l’application. Le contenu visible est réparti dans des contrôles distincts :

- `ReadTabSection`, `WriteTabSection`, `ConversionTabSection`, `VisualizerTabSection`, `ExplorerSection` et `ToolsTabSection` ;
- `MainMenu`, `TerminalSection`, `ApplicationStatusBar` et `TrackProgressStrip` ;
- les blocs spécialisés Lecture, Écriture, Conversion, Visualisateur et Explorateur.

Les opérations en cours, la progression, le terminal, le placement de fenêtre, les profils, le matériel et l’espace de travail des images sont pilotés par des contrôleurs ou services dédiés. La coque n’implémente plus directement ces mécanismes.

Les trois blocs `ProfileSection` de Lecture, Écriture et Conversion réutilisent le même type de contrôle, mais chaque instance garde son opération, sa sélection et sa collection de profils propres.

`OptionsWindow` compose quatre pages distinctes : Général, Journaux, Contrôleurs et lecteurs, Profils. Leurs comportements sont répartis entre les contrôles `Options*Section`, les contrôleurs `Options/*` et les classes spécialisées de réglages.

## Persistance et état

Les réglages sont séparés par domaine : matériel, journaux, opérations, profils et placement des fenêtres. Les services de placement appliquent position et taille avant l’affichage. Les contrôleurs de matériel et d’images gèrent leur cycle de vie et leur annulation sans partager un état implicite avec les onglets.

## Garanties de la phase 2

Le refactoring a été effectué responsabilité par responsabilité. Chaque déplacement a supprimé l’ancien chemin après raccordement de ses consommateurs, puis a été compilé et vérifié par les tests concernés. La validation finale de cette phase est consignée dans [la tâche 02](tasks/02-full-refactoring.md).

Les phases suivantes restent distinctes : centralisation exhaustive des constantes et textes techniques, réorganisation complémentaire des contrats et fonctions, puis réorganisation des ressources de langue. Leur existence ne remet pas en cause les frontières structurelles établies ici.
