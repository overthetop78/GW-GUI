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

Il possède les conteneurs, flux, pistes, révolutions, secteurs, blocs, données séquentielles, codecs
et géométries. Il ne possède plus l’explorateur de fichiers et ne connaît ni les contrôles WPF, ni
les icônes de l’application, ni les textes traduits.

### `GWGUI.MediaFileSystems`

`GWGUI.MediaFileSystems` contient l’explorateur de fichiers retiré de `GWGUI.MediaEngine`. À partir
d’une représentation fournie par `GWGUI.MediaEngine`, il détecte les partitions, volumes et systèmes
de fichiers, puis lit leurs vrais dossiers, fichiers, métadonnées et contenus. Il conserve les noms
enregistrés dans le média et ne crée pas de faux fichiers pour remplacer un contenu qu’il ne sait
pas encore lire. Il ne lit et n’écrit aucun média physique, ne convertit pas les fichiers flux ou les
images sectorielles et ne produit pas leur visualisation graphique.

### `GWGUI.MediaAnalysis`

`GWGUI.MediaAnalysis` analyse les volumes, dossiers et fichiers fournis par
`GWGUI.MediaFileSystems`. Il détermine les types génériques utiles, leur catégorie sémantique, leur
identifiant stable d’icône et leur clé de traduction. Il produit une arborescence indépendante de
WPF, prête à être affichée par un consommateur.

### `GWGUI.App`

`GWGUI.App` orchestre les appels, traduit les clés reçues, associe les identifiants d’icônes aux
ressources graphiques et affiche les lignes préparées. Il ne relit pas les octets des fichiers et ne
possède pas de règle de reconnaissance de contenu.

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
├── Migration
├── Contracts
├── Registry
├── Definitions
└── Utilities
```

Une famille propre à une machine est placée sous cette famille. Un système partagé comme FAT12 reste
classé par système de fichiers et non sous une machine arbitraire. Les contrats, le registre, les
définitions et les utilitaires communs ne sont pas mélangés aux implémentations de formats.

`Migration` copie une arborescence déjà lue vers un autre système de fichiers. Par exemple, pour
recréer sur un volume FAT12 les dossiers et fichiers lus sur un volume AmigaDOS, cette couche adapte
les noms et métadonnées puis demande aux writers de systèmes de fichiers d'écrire le nouveau volume.
Elle ne décode pas une capture SCP et ne convertit pas elle-même une représentation physique : ces
opérations restent dans `GWGUI.MediaEngine`.
## Sens autorisé des dépendances

```text
GWGUI.App
    ↓
GWGUI.MediaAnalysis
    ↓
GWGUI.MediaFileSystems
    ↓
GWGUI.MediaEngine
    ↓
GWGUI.Domain
```

Une bibliothèque de cette chaîne peut également référencer `GWGUI.Domain` lorsque ses contrats le
nécessitent. Les dépendances inverses sont interdites. En particulier, aucune bibliothèque de
production ne référence `GWGUI.App`, et `GWGUI.MediaEngine` ne référence ni
`GWGUI.MediaFileSystems` ni `GWGUI.MediaAnalysis`.

## Réutilisation par un émulateur

Un émulateur qui doit seulement monter, lire ou convertir un média référence `GWGUI.MediaEngine`.
Il ajoute `GWGUI.MediaFileSystems` lorsqu’il doit exposer les fichiers du média, puis
`GWGUI.MediaAnalysis` lorsqu’il doit présenter une arborescence enrichie de types, catégories et
icônes. Les lecteurs et décodeurs restent communs ; ils ne sont pas recopiés dans l’émulateur.

## Données échangées

### Du moteur vers les systèmes de fichiers

`GWGUI.MediaEngine` fournit une représentation documentée du média : format du conteneur, capacité,
géométrie disponible et accès aux flux, pistes, secteurs, blocs ou données séquentielles. Cette
représentation ne contient aucune hypothèse sur l’interface qui la consommera.

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
