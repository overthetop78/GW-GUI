# Formats d’images de supports optiques

Ce document fixe le périmètre des Readers et Writers optiques de MediaEngine. Une image optique
décrit d’abord la disposition enregistrée du support : secteurs, sessions, pistes, index, sous-canaux
et éventuellement couches. Les systèmes de fichiers comme ISO 9660 ou UDF sont détectés ensuite par
l’Explorateur. Une extension ne suffit jamais à identifier seule le contenu d’un fichier `.bin`,
`.img`, `.iso` ou `.chd`.

## Règles communes

- Le document produit utilise `MediaKind.Optical` et une représentation `OpticalTracks`.
- Une piste conserve son numéro, sa session, son mode, la taille de ses secteurs, ses index et ses
  plages réellement présentes. Les informations absentes restent inconnues.
- Les pistes audio et données ne sont jamais transformées l’une dans l’autre pendant la lecture.
- Les sous-canaux, pregaps, postgaps, CD-Text, catalogues et ISRC sont exposés seulement lorsque le
  conteneur les fournit.
- Une face ou une couche n’est déclarée que si le descripteur l’indique. Deux faces fournies comme
  deux images restent deux documents associés tant qu’aucun format commun ne les relie.
- Un Reader de format multifichier ouvre d’abord le descripteur, résout les chemins relativement à
  celui-ci et refuse les fichiers absents, les débordements et les références cycliques.
- Un Writer multifichier publie le descripteur en dernier, après l’écriture complète de tous les
  fichiers de données et de sous-canaux.
- Une conversion annonce les informations perdues avant de commencer. Une destination ISO ne peut
  pas conserver une piste audio, plusieurs pistes, les sous-canaux ou une disposition multisession.

## Tableau de décision

| Format | Identification et fichiers | Secteurs et structure | Audio, sous-canaux, sessions et couches | Lecture retenue | Écriture retenue |
|---|---|---|---|---|---|
| ISO | Aucun en-tête de conteneur ; un fichier. Un système ISO 9660 porte un descripteur `CD001` à partir du secteur logique 16, mais une image ISO peut contenir un autre système de fichiers | Suite contiguë de secteurs de données, généralement 2048 octets ; aucune table de pistes dans le conteneur | Une seule piste de données implicite ; pas de sous-canaux, de piste audio ni de description fiable des sessions, couches ou faces | Oui, après validation de la taille des secteurs et des structures réellement présentes | Oui pour une seule piste de données à secteurs de 2048 octets ; refus si la source exige d’autres informations |
| BIN/CUE | Descripteur texte `.cue` avec commandes `FILE`, `TRACK` et `INDEX`, associé à un ou plusieurs fichiers BIN ou audio | `MODE1/2048`, `MODE1/2352`, `MODE2/2336`, `MODE2/2352` et `AUDIO` selon le CUE ; pregaps et index exprimés en MM:SS:FF | Pistes audio ou données et plusieurs fichiers ; sous-canaux seulement si une extension explicitement prise en charge les décrit ; le CUE classique ne décrit pas complètement couches et faces | Oui pour les commandes et modes inventoriés ; refus des commandes qui changeraient silencieusement la disposition | Oui pour un ensemble autonome dont tous les secteurs et fichiers référencés sont disponibles ; conservation des commandes comprises |
| CCD/IMG/SUB | Descripteur INI `.ccd` contenant la section `[CloneCD]`, fichier `.img` obligatoire et fichier `.sub` facultatif de même base | IMG en secteurs bruts de 2352 octets ; SUB en enregistrements de sous-canaux de 96 octets lorsqu’il existe | Le CCD décrit TOC, sessions, pistes, index et entrées ; SUB conserve les sous-canaux | Oui pour CCD version 2 ou 3, IMG complet et SUB facultatif cohérent | Reportée jusqu’à validation d’un aller-retour exact CCD/IMG/SUB, notamment des pregaps et sous-canaux |
| MDF/MDS | Descripteur binaire `.mds` identifié par `MEDIA DESCRIPTOR`, associé à un ou plusieurs `.mdf` | Tables de sessions, blocs de pistes et extents pointant dans MDF ; tailles et modes de secteurs issus du descripteur | Peut décrire plusieurs sessions, pistes audio ou données, sous-canaux et informations DVD de couche | Oui pour les versions et structures documentées après validation stricte des offsets et fichiers associés | Reportée : la production doit préserver sessions, intersessions, sous-canaux et couches sans approximation |
| CHD optique | Signature `MComprHD` puis métadonnées `CHTR` ou `CHT2` pour CD/GD, ou `DVD ` pour DVD ; un parent peut être requis | Données réparties en hunks et unités ; les métadonnées de pistes distinguent type, sous-type, frames, pregap et postgap | CD/GD : pistes audio ou données et sous-canaux selon le sous-type ; DVD : média classé par métadonnée, sans inventer de pistes CD | V5 autonome, non chiffré, avec codecs optiques pris en charge et métadonnées optiques obligatoires | Reportée jusqu’à prise en charge validée des codecs, métadonnées et sommes de contrôle nécessaires |

## Détails par format

### ISO

Le suffixe `.iso` désigne ici une copie logique d’une piste de données et non la preuve d’un système
ISO 9660. Lorsque ce système est présent, sa séquence de reconnaissance commence au secteur 16 et
ses descripteurs portent l’identifiant `CD001`. Le Reader expose d’abord les secteurs comme une piste
optique ; ISO 9660, Joliet, Rock Ridge ou UDF sont ensuite détectés par les Readers de systèmes de
fichiers. La taille divisible par 2048 aide à valider un choix explicite, mais ne constitue pas une
signature suffisante face aux autres images brutes.

Le Writer initial accepte seulement une piste de données continue à secteurs utilisateur de
2048 octets. Il ne convertit pas implicitement les secteurs bruts de 2352 octets et refuse toute
source contenant de l’audio, plusieurs pistes, des sous-canaux ou une disposition multisession.

### UDF

Le premier Reader reconnaît les structures communes aux révisions UDF 1.02, 1.50, 2.00, 2.01,
2.50 et 2.60 décrites par ECMA-167 et ECMA TR/112. Il accepte un volume à partition physique,
un jeu de fichiers directement adressable et les descripteurs d’allocation courts, longs ou
intégrés. La révision annoncée par les identifiants de domaine doit rester comprise dans cette liste.

Les cartes de partitions virtuelles, éparses ou de métadonnées, les VAT, la récupération après
écriture incrémentale et les structures non comprises sont refusées explicitement. Le Reader ne
présente donc pas comme compatible un disque dont l’arborescence dépendrait d’une de ces cartes.
Ces profils pourront être ajoutés séparément lorsque leur traduction d’adresses sera implémentée.

### BIN/CUE

Le CUE est la source de la structure. Le Reader traite les contextes fichier et piste, résout chaque
`FILE`, puis conserve `TRACK`, `INDEX`, `PREGAP`, `POSTGAP`, `FLAGS`, `CATALOG` et `ISRC` lorsqu’ils
sont présents et valides. Le mode de piste fixe la taille de secteur attendue. Les fichiers WAVE
PCM référencés par un CUE audio peuvent être acceptés par un adaptateur audio séparé ; les formats
audio compressés restent hors du premier périmètre afin de ne pas modifier le nombre d’échantillons.

Le Writer produit un CUE UTF-8 et des fichiers BIN autonomes. Il refuse une disposition que sa
syntaxe prise en charge ne peut pas décrire sans perte. La conversion vers ISO n’est proposée que
pour une unique piste de données compatible 2048 octets.

### CCD/IMG/SUB

Le descripteur CCD est analysé comme un fichier INI et doit contenir `[CloneCD]`. Les sections du
disque, des sessions et les entrées de TOC déterminent les pistes et index. Le fichier IMG conserve
les secteurs bruts de 2352 octets. Le fichier SUB, s’il existe, doit fournir 96 octets par secteur ;
son absence est conservée comme une information absente et non remplacée par de faux sous-canaux.

La lecture est retenue avant l’écriture. Un Writer ne sera ajouté qu’après des essais d’aller-retour
prouvant que la TOC, les pregaps, les secteurs bruts et les sous-canaux sont reproduits exactement.

### MDF/MDS

MDS contient la description et MDF les données. Le Reader valide la signature, la version, les
offsets, le nombre de sessions, les blocs de pistes, les fichiers associés et les limites avant de
construire `OpticalTracks`. Les informations de couche ou de session ne sont exposées que lorsque les
structures correspondantes sont comprises. L’écriture est reportée car un ensemble apparemment
lisible peut perdre des intersessions ou des informations de sous-canaux lors d’une réécriture
partielle.

### CHD optique

CHD sert à plusieurs médias. La signature seule ne classe donc pas le document. Les métadonnées
`CHTR` ou `CHT2` identifient une image CD/GD et décrivent ses pistes ; `DVD ` identifie une image DVD.
Le Reader initial vise CHD V5 autonome et valide l’en-tête, la carte, les codecs, les sommes de
contrôle et les métadonnées avant d’exposer les unités. Un hash de parent non nul exige la résolution
explicite du parent. L’écriture reste reportée tant qu’un profil optique complet et reproductible
n’est pas validé.

## Sessions, couches et faces

- Une session regroupe des pistes et possède ses propres limites. ISO ne permet pas de reconstruire
  à lui seul la TOC ou les anciennes sessions d’un disque multisession.
- Une couche DVD est une propriété physique distincte d’une piste CD. Elle est conservée seulement
  si MDS ou une métadonnée CHD comprise la décrit.
- Une seconde face est un autre espace de lecture. Si le format fournit deux fichiers indépendants,
  MediaEngine les expose comme deux documents liés et ne fabrique pas une face commune.
- Le Visualiseur peut dessiner un ou deux disques seulement d’après ces informations. L’absence de
  métadonnée ne doit jamais devenir artificiellement « une face, une couche ».

## Ordre d’implémentation

1. ISO valide une piste de données simple et le passage à ISO 9660 ou UDF dans l’Explorateur.
2. BIN/CUE valide les ensembles de fichiers, les modes de secteurs et les pistes audio ou données.
3. CCD/IMG/SUB valide les secteurs bruts, la TOC et les sous-canaux séparés.
4. MDF/MDS valide les sessions et informations de couches décrites par un conteneur binaire.
5. CHD valide la classification par métadonnées, les hunks et les codecs optiques.

## Références

- [ECMA-119, 6e édition — Volume and File Structure of CD-ROM for Information Interchange](https://ecma-international.org/wp-content/uploads/ECMA-119_6th_edition_december_2025.pdf)
- [GNU ccd2cue — CUE sheet format](https://www.gnu.org/software/ccd2cue/manual/html_node/CUE-sheet-format.html)
- [cdrdao — formats de pistes, secteurs et sous-canaux](https://linux.die.net/man/1/cdrdao)
- [DuckStation — Reader CCD/IMG/SUB](https://github.com/stenzek/duckstation/blob/master/src/util/cd_image_ccd.cpp)
- [CDEmu — libMirage](https://github.com/cdemu/cdemu/tree/master/libmirage)
- [MAME source — en-tête et métadonnées CHD](https://github.com/mamedev/mame/blob/master/src/lib/util/chd.h)
- [MAME — chdman](https://docs.mamedev.org/tools/chdman.html)
