# Frontières des bibliothèques de médias

## Responsabilités

### `GWGUI.MediaEngine`

`GWGUI.MediaEngine` possède quatre responsabilités liées au média lui-même :

- lire un média physique et produire un fichier flux ou une image sectorielle ;
- écrire un fichier flux ou une image sectorielle sur un média physique ;
- convertir un fichier flux ou une image sectorielle vers un autre fichier flux ou une autre image
  sectorielle, y compris dans le sens image sectorielle vers SCP ;
- produire les données nécessaires à la visualisation graphique d’un fichier SCP ou d’une image
  sectorielle.

La conversion d'images compatibles agit sur les représentations physiques (SCP, images sectorielles
et autres formats compatibles). Elle reste dans `GWGUI.MediaEngine`.

Il possède les conteneurs, flux, pistes, révolutions, secteurs, blocs, données séquentielles, codecs
et géométries. Il ne possède plus l’explorateur de fichiers et ne connaît ni les contrôles WPF, ni
les icônes de l’application, ni les textes traduits.

### `GWGUI.MediaFileSystems`

`GWGUI.MediaFileSystems` contient la lecture des fichiers de l’explorateur retirée de `GWGUI.MediaEngine`. À partir
d’une représentation fournie par `GWGUI.MediaEngine`, il détecte les partitions, volumes et systèmes
de fichiers, puis lit leurs vrais dossiers, fichiers, métadonnées et contenus. Il conserve les noms
enregistrés dans le média et ne crée pas de faux fichiers pour remplacer un contenu qu’il ne sait
pas encore lire. Il ne lit et n’écrit aucun média physique, ne
convertit pas les fichiers flux ou les images sectorielles et ne produit pas leur visualisation
graphique.

### `GWGUI.MediaAnalysis`

`GWGUI.MediaAnalysis` analyse les volumes, dossiers et fichiers fournis par
`GWGUI.MediaFileSystems`. Il détermine les types génériques utiles, leur catégorie sémantique, leur
identifiant stable d’icône et leur clé de traduction. Il produit une arborescence indépendante de
WPF, prête à être affichée par un consommateur.

### `GWGUI.App`

`GWGUI.App` appelle uniquement l'API de `GWGUI.MediaEngine` pour ouvrir et explorer le média. Il
traduit les clés reçues, associe les identifiants d’icônes aux ressources graphiques et affiche les
lignes préparées. Il ne relit pas les octets des fichiers et ne possède pas de règle de reconnaissance
de contenu.

## Arborescence de `GWGUI.MediaFileSystems`

Les lecteurs sont rangés sous `FileSystems` selon la nature du système lu :

```text
GWGUI.MediaFileSystems
├── FileSystems
│   ├── Amiga
│   ├── Apple
│   │   ├── Dos
│   │   ├── ProDos
│   │   └── Sos
│   ├── Commodore
│   │   └── Dos
│   └── Fat12
├── Contracts
├── Registry
├── Definitions
└── Utilities
```

Une famille propre à une machine est placée sous cette famille. Un système partagé comme FAT12 reste
classé par système de fichiers et non sous une machine arbitraire. Les contrats, le registre, les
définitions et les utilitaires communs ne sont pas mélangés aux implémentations de formats.

« Exporter vers… » par migration de fichiers est une opération distincte de la conversion d'images :
elle lit les vrais dossiers et fichiers de l'image source, puis les injecte dans un système de fichiers
cible d'une autre image, y compris quand les formats d'image ne sont pas compatibles pour la conversion
physique. `MediaEngine` lit l'image source, résout son format physique et écrit le conteneur cible.
`MediaFileSystems` planifie et valide la migration, adapte les noms et métadonnées, puis construit le
volume cible avec les fichiers et dossiers. Le volume construit revient à `MediaEngine` pour l'écriture
du conteneur. `MediaAnalysis` n'intervient que pour la reconnaissance des types utiles à l'explorateur.
## Sens autorisé des dépendances

```text
GWGUI.App
    ↓
GWGUI.MediaEngine
    ↓
GWGUI.MediaFileSystems
    ↓
GWGUI.MediaAnalysis
```

`GWGUI.App` n'appelle ni `GWGUI.MediaFileSystems` ni `GWGUI.MediaAnalysis` ;
`GWGUI.MediaEngine` n'appelle pas directement `GWGUI.MediaAnalysis`. Les résultats reviennent par
les retours de méthodes dans l'ordre inverse, sans relancer l'exploration ou l'analyse.
Les contrats de média décodé utilisés par `MediaFileSystems` sont déclarés dans cette bibliothèque
et implémentés par `MediaEngine`. Les informations que `App` reçoit sont exposées par l'API de
`MediaEngine`. Aucune référence inverse entre ces projets ni DLL `GWGUI.Domain` n'est nécessaire.

Les références de projets suivent ce graphe. `GWGUI.App` ne référence pas directement
`GWGUI.MediaFileSystems` ni `GWGUI.MediaAnalysis` ; `GWGUI.MediaFileSystems` ne référence pas
`GWGUI.MediaEngine`.
Le raccordement des appels de production reste en cours : `MediaEngine` utilise encore ses anciens
lecteurs de systèmes de fichiers, et `MediaFileSystems` ne déclenche pas encore la classification
de `MediaAnalysis`. Le chargement et la double exploration actuellement utilisés par l'application
restent orchestrés une seule fois dans `MediaEngine` pendant cette extraction.

## Réutilisation par un émulateur

Un émulateur appelle `GWGUI.MediaEngine` pour monter, lire ou convertir un média. S'il expose
l'explorateur de fichiers, il demande aussi son résultat à cette même API ; `MediaEngine` déclenche
alors `MediaFileSystems`, qui déclenche `MediaAnalysis`. Les lecteurs et décodeurs restent communs ;
ils ne sont pas recopiés dans l'émulateur.

## Données échangées

### Du moteur vers les systèmes de fichiers

`GWGUI.MediaEngine` fournit une représentation documentée du média : format du conteneur, capacité,
géométrie disponible et accès aux flux, pistes, secteurs, blocs ou données séquentielles. Cette
représentation ne contient aucune hypothèse sur l’interface qui la consommera.

Pour une bande, l'adaptateur de `MediaEngine` choisit le décodeur et transmet les blocs déjà décodés,
leurs noms enregistrés, leurs métadonnées et diagnostics. Les contrats
`MediaSequentialDecodedBlock` et `IMediaSequentialContent` sont déclarés dans
`MediaFileSystems/Exploration/Sequential` ; son lecteur y construit les fichiers sans relancer le
décodage et sans attribuer de nom aux blocs anonymes.

### Des systèmes de fichiers vers l’analyse

`GWGUI.MediaFileSystems` fournit les volumes reconnus et leur arborescence réelle. Chaque entrée
porte son nom enregistré, sa nature, sa taille, ses dates disponibles, ses attributs natifs, sa
référence de stockage, ses enfants, son contenu lorsqu’il a pu être lu, son état de validité et ses
diagnostics. Une absence de système de fichiers reconnu reste explicitement une absence de résultat ;
elle n’est pas remplacée par des noms de fichiers inventés.

### De l’analyse vers l’application

`GWGUI.MediaAnalysis` fournit des lignes hiérarchiques indépendantes de l’interface. Chaque ligne
conserve les données de l’entrée d’origine et ajoute une définition de type comprenant :

- un identifiant technique stable ;
- une catégorie sémantique ;
- un identifiant stable d’icône ;
- une clé de traduction.

La clé n’est pas traduite dans la bibliothèque d’analyse. L’identifiant d’icône ne désigne pas un
objet graphique ou un fichier de ressources précis. `GWGUI.App` effectue ces deux adaptations au
dernier moment, selon sa langue et son thème actifs.

## Règles d’identification

Une extension peut fournir un type générique lorsqu’elle possède un sens habituel, par exemple
`.BAS` pour un fichier BASIC ou `.SYS` pour un fichier système. Cette classification reste une
indication fondée sur le nom et n’affirme pas que la structure interne a été validée.

Une identification fondée sur le contenu doit reposer sur une propriété réutilisable : signature de
début ou de fin, structure interne, taille imposée par le format, champs cohérents ou séquence
caractéristique démontrée sur plusieurs fichiers. La règle ne dépend ni du nom complet du fichier, ni
du titre du logiciel, ni de sa position dans une disquette particulière.

Le nom exact d’un fichier ne constitue jamais à lui seul un format. Une règle limitée à `LEVEL.A`,
`AUTORUN.SYS` ou à tout autre exemplaire particulier doit être supprimée ou remplacée par une règle
générale démontrée. Lorsqu’aucune structure générale n’est connue, l’entrée conserve son vrai nom et
son type générique au lieu de recevoir une identification inventée.
