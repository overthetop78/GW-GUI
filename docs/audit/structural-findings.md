# Constats structurels

## Monolithes confirmés

### `MainWindow.xaml.cs`

Environ 1 975 lignes. Il compose les dépendances, pilote les cinq onglets, la console, les profils, le matériel, les commandes, la progression, l’ouverture des images, l’Explorateur, le Visualisateur et la fermeture. La séparation visuelle déjà commencée en composants ne suffit pas : la logique reste centralisée.

### `DiskImageExplorer.cs`

Environ 511 lignes. Il construit les lecteurs, route par extension, orchestre les images SCP, choisit des candidats de machine/format, appelle les systèmes de fichiers, calcule des scores, fabrique un arbre physique de remplacement et adapte certains résultats. Il mélange conteneur, détection, reconstruction sectorielle, système de fichiers et modèle d’Explorateur.

### `OptionsWindow.xaml.cs`

Environ 699 lignes. Il pilote Général, matériel, Host Tools, profils, tags et journaux, et contient plusieurs modèles de lignes/options dans le même fichier.

### `CoreTests.cs`

Environ 3 435 lignes. Il mélange les tests de nombreuses fonctions et des doubles de test. Sa taille ne casse pas le produit, mais ralentit la navigation et masque la couverture réelle par domaine.

## Nom trompeur confirmé

`AtariScpSectorImageReader.cs` ne traite pas seulement Atari. Il contient des branches pour Atari ST/8 bits mais aussi ISO FM/MFM utilisé par IBM PC, Amstrad, BBC, Epson QX-10 et UCSD. Le nom et l’emplacement cachent un reconstructeur sectoriel partagé et des géométries propres à plusieurs machines.

La correction ne consiste pas à renommer le fichier en `GenericReader` et conserver le bloc. Il faut séparer :

- la collecte ISO FM/MFM réellement commune ;
- les définitions de géométrie ;
- les adaptations propres à chaque famille ;
- la sélection des candidats.

## Fichiers courts mais mélangés

- `ImageFormatCatalog.cs` : modèles, interface, catalogue intégré et adaptation aux capacités de `gw`.
- `AppSettings.cs` : racine des réglages, UI, matériel, profils, journaux et contrats de persistance.
- `WriteRequest.cs` : détection du fichier source, modèles de détection, requête et construction de commande.
- `ReadRequest.cs` : requête, résultat, options, construction de commande et tokenisation.
- `ConversionPlanner.cs` : modèles de sélection/sortie, constructeur et planificateur.
- `IProfileStore.cs` : contrat et implémentation en mémoire dans le projet Domain.
- `AdfImageReader.cs` : le contrat générique `ISectorImageReader` est déclaré dans un fichier nommé pour ADF.
- `ExplorerSection.xaml.cs` : contrôle WPF, modèles de présentation et utilitaires de formatage.
- `ExplorerDetailsPanel.xaml.cs` : contrôle, records de vue et présentateur.

## Fichiers volumineux pouvant rester spécialisés

- `SkiaScpRenderer.cs` est volumineux, mais sa responsabilité reste le rendu Skia. Des helpers peuvent être extraits si l’audit de phase 2 prouve des sous-algorithmes autonomes ; un découpage uniquement par nombre de lignes serait artificiel.
- les lecteurs de systèmes de fichiers de 150 à 300 lignes peuvent rester un fichier par système lorsque toute la logique appartient au même format. Leurs modèles ou utilitaires communs ne seront extraits que s’ils sont réellement partagés.
- chaque décodeur et encodeur de piste possède déjà son propre fichier. Leur symétrie ne justifie pas de les fusionner.

## Chaînes de conditions et comparaisons de chaînes

Les concentrations principales sont :

| Fichier | Nature du problème |
|---|---|
| `MainWindow.xaml.cs` | nombreuses branches d’événements et d’états de cinq fonctions différentes ; symptôme du mélange de responsabilités. |
| `DiskImageExplorer.cs` | longue chaîne d’extensions, préfixes de formats et essais successifs ; registre insuffisamment déclaratif. |
| `ExplorerFileIconClassifier.cs` | listes d’extensions et règles par famille dans l’UI ; risque de classer `.bat`, `.prg`, etc. selon la mauvaise machine. |
| `AtariScpSectorImageReader.cs` | branches de machines et géométries autour d’un décodeur ISO partagé. |
| `AppleDiskImageReader.cs` | plusieurs conteneurs et ordres de secteurs Apple dans un même lecteur. |
| `SectorImageFluxVisualizer.cs` | décisions par identifiant de format mêlées à la génération visuelle. |
| `GwOptionValidator.cs` | grammaires d’options différentes dans un même validateur ; à séparer seulement par grammaire réelle. |

Un `switch` n’est pas automatiquement préférable à un `if`. La cible est un catalogue de définitions et des stratégies enregistrées. Un `switch` court reste acceptable pour convertir une valeur fermée en comportement local.

## Détection et images multiformats

- La détection SCP construit plusieurs candidats, mais certains chemins utilisent ensuite le premier lecteur ou le premier système de fichiers compatible.
- `FileSystemRegistry.TryRead` peut dépendre de l’ordre des lecteurs ; `ReadAll` conserve davantage d’interprétations.
- Le format choisi manuellement représente une préférence/interprétation demandée. Il ne doit pas supprimer automatiquement d’autres systèmes valides d’une disquette multiformat.
- Les images sectorielles ciblées peuvent emprunter un chemin direct lorsque leur conteneur et leur géométrie sont sans ambiguïté. Cela ne s’applique pas aux captures flux/SCP multiformats.

## Duplications et données dispersées

Les mêmes identifiants ou connaissances apparaissent dans plusieurs endroits :

- catalogue de formats Domain ;
- routage d’extensions de `DiskImageExplorer` ;
- classification d’icônes de l’Explorateur ;
- sélection des décodeurs/encodeurs ;
- politique de visualisation ;
- chaînes localisées de formats ;
- tests de corpus.

Les géométries, extensions et familles doivent devenir des données partagées par identifiant stable. Les algorithmes restent séparés lorsqu’ils n’ont pas exactement les mêmes CRC, marques, ordre de bits ou règles de secteurs.

## Textes et constantes

- Les textes visibles XAML utilisent majoritairement `Loc`, mais l’audit trouve encore des messages techniques anglais dans des exceptions et scripts.
- Une exception technique interne peut rester en anglais dans un journal, mais dès que son message est affiché directement il doit passer par une clé localisée et conserver le détail technique séparément.
- Les noms techniques officiels (`AmigaDOS`, `RWTS18`, identifiants `gw`, extensions) ne doivent pas être traduits arbitrairement.
- Les nombres de géométrie, marques, CRC et tailles situés dans les codecs ne sont pas tous des « constantes globales » : ils doivent vivre dans une définition du format ou dans l’algorithme nommé, jamais dans un fourre-tout unique.

## Recalculs Explorateur/Visualisateur

Le chargement est coordonné depuis `MainWindow`, mais `DiskImageExplorer`, `ScpDocumentLoader`, le registre des décodeurs, `SectorImageFluxVisualizer` et le renderer produisent des représentations différentes. Une même image peut donc subir :

1. lecture du conteneur ;
2. reconstruction sectorielle pour l’Explorateur ;
3. lecture/décodage du flux pour le Visualisateur ;
4. analyse de piste supplémentaire lors d’une sélection dans l’inspecteur.

La future couche commune doit mettre en cache les résultats immuables par fichier, date/taille et choix de classification, tout en permettant l’annulation immédiate lorsqu’une autre image est ouverte. Le rendu graphique et la construction de l’arbre de fichiers restent des projections séparées.

