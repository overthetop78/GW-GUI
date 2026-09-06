# Catalogue général des formats HDD

## Lecture du catalogue

Ce catalogue est organisé par structures de stockage. Il ne dépend pas des machines ou émulateurs intégrés. Les noms historiques désignent des formats, pas une restriction du périmètre.

**Implémenté** signifie qu’un constructeur existe pour les seules variantes indiquées. **À couvrir** signifie qu’aucune création de cette variante n’est annoncée. **À étudier** demande de préciser la spécification, les variantes HDD pertinentes et une méthode de validation avant l’implémentation. Les listes groupées sont des familles à décomposer, pas une promesse de compatibilité uniforme.

## 1. Conteneurs et représentation des secteurs

| Format | État | Variantes et points à traiter |
|---|---|---|
| RAW | Implémenté pour des secteurs de 512 octets | Les extensions usuelles sont des conventions ; pas un constructeur par extension. Tailles de secteur supplémentaires à couvrir. |
| gzip | Implémenté | Compression d’un flux RAW ; accès aléatoire et persistance des modifications à traiter séparément. |
| VHD | Fixe, dynamique et chaînes différentielles implémentés | Jusqu’à 64 ancêtres fournis sur flux, identités et capacités vérifiées, base autonome exigée, ancêtres en lecture seule. Références suivies par le contrôle de suppression. Sélection dans l’application et variantes supplémentaires à intégrer. |
| VHDX | Fixe, dynamique et chaînes différentielles, secteurs logiques 512 ou 4096 implémentés | Secteurs physiques de 4096 octets dans ces profils. Registre `vhdx` ou `vhdx-4kn`, GPT et volumes cohérents requis. Jusqu’à 64 ancêtres avec identités, capacités et secteurs concordants. Sélection et autres variantes à couvrir. |
| VDI | Fixe et dynamique implémentés | Variantes différentielles à couvrir. |
| VMDK | Monolithique sparse, streamOptimized, monolithicFlat, split flat/sparse et VMFS flat/sparse implémentés | streamOptimized autonome : grains zlib de 64 Kio, tables et footer finaux, lecture seule du résultat, jusqu’à 1 Tio. Split : extents de 64 Kio à 2 Gio, 2 047 Mio par défaut, au plus 1 024 extents ; plafond de profil 1 Tio. Publication par dossier et suivi des références présents ; sélection dans l’application à intégrer. Construction de chaînes de parents et autres variantes à couvrir. |
| QCOW2 | V2 et V3 autonomes implémentés | Clusters de 512 octets à 2 Mio paramétrables dans le constructeur, 64 Kio par défaut ; refcounts 16 bits et table sur plusieurs clusters. Plafond 1 Tio et L1 limitée à 32 Mio. Compression, snapshots, parents, autres refcounts et chiffrement à couvrir. |
| QCOW v1 | Autonome implémenté | Clusters de 64 Kio, tables L2 de 8 192 entrées ; jusqu’à 1 Tio. Compression, chiffrement et parents à couvrir. |
| QED | Autonome implémenté | Clusters de 64 Kio, tables d’un cluster, capacité jusqu’à 4 Tio ; parents et autres paramètres à couvrir. |
| Parallels | Image extensible v2 implémentée | Variante `WithouFreSpacExt`, clusters de 1 Mio, jusqu’à 1 Tio. Descripteur XML, fichiers associés, parents et autres variantes à couvrir. |
| CHD HDD | V5 autonome non compressé implémenté | Hunks de 64 Kio, unités de 512 octets, métadonnées GDDD et géométrie explicite dans le constructeur ; plafond `Int32.MaxValue × 512` octets. SHA-1 absent pour ce profil inscriptible. Compression, parents et anciennes versions à couvrir. |
| 2IMG / 2MG | V1 à blocs de 512 octets implémenté | En-tête de 64 octets, sans commentaire ; limite du constructeur inférieure à 2 Gio. Autres variantes à couvrir. |
| UDIF / DMG | V4 autonome RAW ou zlib implémenté | Table `blkx` XML, blocs de 8 Mio, zones nulles explicites, jusqu’à 1 Tio. Aucun checksum installé dans ce profil. Structures, décompression et contenu des fichiers vérifiés en mémoire ; pas de certification de montage par un système externe. Chiffrement, segmentation, autres codecs et checksums à couvrir. |
| sparseimage | V3 autonome non chiffré, un en-tête d’index implémenté | 1 008 bandes au maximum, de 1 à 128 Mio par puissances de deux ; profil enregistré à bandes de 8 Mio, soit 7,875 Gio maximum. Références big-endian, bandes nulles omises. En-têtes de continuation et variantes supplémentaires à couvrir. |
| sparsebundle | V1 autonome non chiffré implémenté | Bandes de 1 à 128 Mio par puissances de deux, 8 Mio par défaut ; jusqu’à 1 Tio. Métadonnées dupliquées, token et bandes hexadécimales. Composition et publication par dossier présentes ; suivi détaillé des membres, sélection et suppression collective restent à intégrer. |
| DHD | Représentation RAW existante | Organisation CMD interne et installation distinctes du conteneur ; leur constructeur reste à couvrir. |
| D90 | Représentation RAW et formatage DOS implémentés | Deux capacités : 5 013 504 et 7 520 256 octets ; géométrie 153 cylindres, quatre ou six têtes, 32 secteurs de 256 octets. Configuration, liste de défauts vide, BAM et répertoire construits séparément du conteneur. |
| HDI | En-tête de 4096 octets implémenté | Géométrie explicite ; capacité limitée au champ 32 bits en octets. Secteurs de 128 à 4096 octets dans le constructeur. |
| NHD | R0 implémenté | En-tête de 512 octets, commentaire ASCII, géométrie explicite ; profil jusqu’à 1 Tio et cylindres 32 bits. |
| THD | Implémenté | En-tête de 256 octets, huit têtes, 33 secteurs de 256 octets ; capacité exactement multiple d’un cylindre, au plus 65 535 cylindres. |
| HDN | Représentation RAW existante | La géométrie imposée par un consommateur appartient à son adaptateur ; aucune nouvelle signature de conteneur à construire. |
| HDF avec en-tête | À étudier | Désambiguïser les dialectes ; l’extension seule ne décrit pas un en-tête. |
| Bochs Growing redolog | V1 et V2 autonomes implémentés | Extents de 4 Kio à 8 Mio, bitmap par secteur de 512 octets et catalogue limité à 1 048 576 entrées ; 1 Mio par défaut, donc plafond de 1 Tio pour le profil enregistré. Autres sous-types et parents à couvrir. |
| cloop | V2 zlib implémenté | Blocs complets de 512 octets à 256 Kio, table d’offsets 64 bits, au plus 4 194 304 blocs ; 64 Kio par défaut, soit 256 Gio. Profil destiné à la lecture du contenu comprimé ; modification et conversion séparées. |
| VVFAT, EWF, AFF/AFF4 | À étudier | Distinguer image HDD inscriptible, projection d’un dossier et conteneur d’acquisition. |
| Autres conteneurs historiques ou propriétaires | À inventorier | Ajouter une entrée identifiée et sourcée par format et variante. |

Les variantes de conteneurs documentées par [QEMU](https://www.qemu.org/docs/master/system/images), la [spécification QCOW2](https://www.qemu.org/docs/master/interop/qcow2.html) et celle de [Parallels](https://www.qemu.org/docs/master/interop/prl-xml.html) servent de références pour leurs entrées respectives.

La documentation de [CHD](https://docs.mamedev.org/tools/chdman.html) distingue la création HDD des autres médias. La [spécification 2IMG](https://ciderpress2.com/formatdoc/TwoIMG-notes.html) distingue également plusieurs représentations. Les structures [DHD et D90](https://vice-emu.sourceforge.io/vice_17.html) ont leur propre documentation.

## 2. Organisation du disque et des volumes

| Organisation | État | Travail restant |
|---|---|---|
| Volume direct, sans table | Implémenté pour les formateurs disponibles | Généraliser les options de secteur et d’alignement. |
| MBR | Partitions primaires, chaînes EBR LBA et profil CHS implémentés | LBA : étendue 0x0F, EBR immédiatement avant les données. CHS : étendue 0x05, départs alignés sur les pistes, une piste réservée avant chaque volume logique, adresses CHS absolues et LBA relatives concordantes. Profil enregistré 16 têtes × 63 secteurs, plafond 504 Mio ; constructeur paramétrable de 1 à 255 têtes et de 1 à 63 secteurs. Autres dispositions historiques à couvrir. |
| GPT | Jusqu’à 128 partitions, secteurs de 512 à 4096 octets implémentés | Deux copies complètes, CRC et zones réservées initialisées ; types GUID, attributs et noms UTF-16 paramétrables. Tailles de secteurs croisées avec le conteneur et le volume. Autres nombres et tailles d’entrées à couvrir. |
| AHDI | Partitions primaires et chaîne XGM implémentées | Quatre entrées principales dont une pour l’étendue ; secteur racine auxiliaire avant chaque volume logique, collisions rejetées. |
| ICD / Supra | Jusqu’à douze partitions implémentées | Quatre entrées principales et huit supplémentaires ; types GEM, BGM, RAW, LNX, SWP ; sans chaîne XGM. |
| RDB | Géométrie et liste de partitions paramétrables | Secteurs de 512 octets ; têtes et secteurs par piste configurables via `RdbGeometry` et `Describe`. Cylindres réservés calculés selon les métadonnées, chaîne de quarante partitions testée. Blocs de pilotes à couvrir. |
| APM | Jusqu’à 62 partitions de données implémentées | Secteurs de 512 octets, carte réservée de 63 entrées, noms et types paramétrables ; pilotes à couvrir. |
| BSD disklabel | Profil 32 bits little-endian et big-endian implémenté | Huit entrées dont `c` pour le disque entier, sept volumes de données, label au secteur 1 et zone initiale de 8 Kio réservée. Types explicites. Imbrication et autres variantes à couvrir. |
| VTOC / Sun label | Label big-endian VTOC v1, principal seul ou avec cinq secours implémenté | Huit slices dont une pour la zone utilisable entière ; premier cylindre réservé, départs alignés sur les cylindres, tags et flags explicites. Secours sur les secteurs impairs 1 à 9 d’une piste du dernier cylindre alternatif ; deux cylindres alternatifs par défaut, tête réglable. Volumes partageant la zone initiale et autres variantes VTOC à couvrir. |
| SGI volume header | Label big-endian implémenté | Seize entrées dont deux pour l’en-tête et le disque entier ; quatorze volumes de données, réserve initiale de 2 Mio. Géométrie et types explicites. Répertoire du volume header vide ; installation des fichiers de démarrage à couvrir séparément. |
| FileCore et organisations dérivées | À étudier | Record de disque, géométrie, cartes d’allocation et éventuelles tables imbriquées. |
| APA | À couvrir | Organisation et chaînes de partitions ; indépendante du système de fichiers PFS. |
| Dispositions de volumes FATX | À étudier | Offsets, zones réservées et variantes ; FATX ne définit pas à lui seul tout le disque. |
| Partitionnement CMD | À couvrir | Types de partitions et métadonnées de gestion. |
| Organisations CP/M | À étudier | Paramètres du volume et zones réservées ; ne pas supposer une table standard. |
| LVM, volumes agrégés, RAID logiciel | À étudier | Couche de volumes supplémentaire, métadonnées et dépendances entre images. |
| Dispositions propriétaires, chiffrées ou signées | À étudier | Structure exacte et paramètres nécessaires ; un conteneur vide ne suffit pas. |

Les [implémentations de tables de partitions](https://android.googlesource.com/kernel/common/+/9f5cbdaae5f760c218c82e0a5e0f9c58bac56f0c/block/partitions/Kconfig) permettent d’identifier plusieurs familles historiques. Le code [APA](https://github.com/ps2dev/ps2sdk/tree/master/iop/hdd/apa) documente une organisation distincte du formatage des volumes.

## 3. Systèmes de fichiers

| Famille | État | Variantes et contraintes à traiter |
|---|---|---|
| FAT12 / FAT16 / FAT32 | Sélection automatique et variantes explicites implémentées | Variantes explicites : secteurs de 512, 1024, 2048 ou 4096 octets, deux FAT, clusters de 1 à 64 secteurs avec plafond de 32 Kio. Réouverture, allocation de fichiers et secteurs de secours testés pour chaque taille. Sélection automatique conservée en 512 octets. |
| FAT16 à secteurs logiques adaptés | Implémenté dans le profil existant | Généraliser le profil de paramètres sans dépendance au consommateur. |
| exFAT | Implémenté via bibliothèque intégrée | Profil de 32 Mio à 1 Tio, secteurs de 512 octets ; offset de partition et deux checksums de démarrage, réouverture et données testés. |
| NTFS | Construction présente via bibliothèque | Version et options exposées à préciser ; les extensions de fonctionnalités ne sont pas toutes couvertes. |
| Zone d’échange Linux | SWAPSPACE2 v1 implémenté | Volume sans fichiers ni dossiers. Pages de 4, 8, 16, 32 ou 64 Kio, entiers little-endian ou big-endian, UUID et liste de pages défectueuses explicites ; label ASCII de 15 caractères maximum. De 10 à 2³²−1 pages, avec au moins 10 pages non défectueuses, en-tête compris. Limites du noyau consommateur distinctes. Ancien SWAP-SPACE et hibernation à couvrir séparément. |
| ReFS, HPFS | À étudier | Versions et documentation exploitable. |
| OFS / FFS | DOS0 à DOS5 implémentés | Base, international et dircache ; cache vide alloué et référencé, type RDB cohérent. Long names et autres tailles de blocs à couvrir séparément. |
| PFS3 / PDS3 | PFS3 à secteurs de 512 octets et blocs réservés de 1 Kio implémenté | Moteur intégré au code de GWGUI.Emulation : formatage, lecture, écriture, noms longs, métadonnées, allocation, diagnostic et mode super-index ; profil de 8 Mio à 213 021 952 secteurs. PFS3 et PDS3 sont des choix de pilote RDB pour le même format de volume ; type PFS3 par défaut, PDS3 explicite. Modes expérimentaux à blocs réservés de 2/4 Kio et installation du pilote à couvrir. Distinct de PFS associé à APA. |
| SFS / SFS2 | À couvrir | Formateurs et variantes à identifier séparément. |
| MFS | Volume vide implémenté | Profil de 128 Kio à 64 Mio, MDB de deux secteurs et sa copie finale, répertoire de douze secteurs, au plus 640 blocs d’allocation. Labels Mac Roman de 1 à 27 octets ; caractères non représentables refusés. Autres paramètres à couvrir. |
| HFS classique | Volume vide implémenté | Profil 128 Kio à 4 Gio ; bitmap de seize secteurs, allocation adaptée, arbres catalogue/extents et copie MDB. Labels Mac Roman de 1 à 27 octets, y compris dans les clés du catalogue et le thread racine. Paramètres supplémentaires à couvrir. |
| HFS+ | V4 non journalisé implémenté | Blocs de 4 Kio, catalogue, extents et attributs, bitmap, en-tête secondaire ; 8 Mio à 1 Tio. Labels Latin-1, saisis composés ou décomposés, stockés en décomposition canonique ; limite de 255 unités UTF-16 après décomposition. Journalisation, wrapper HFS et normalisation Unicode complète à couvrir. |
| HFSX | V5 non journalisé implémenté | Même profil de stockage, comparaison binaire des clés du catalogue. Autres variantes à couvrir. |
| APFS | À étudier | Conteneur de volumes, métadonnées, checkpoints, partage d’espace et chiffrement. |
| ProDOS | Formateur de volume vide implémenté | Blocs de 512 octets, 16 à 65 535 blocs, répertoire de quatre blocs et bitmap ; aucun chargeur installé. |
| Pascal | Volume vide little-endian implémenté | Blocs de 512 octets, répertoire fixe de quatre blocs, 77 fichiers au maximum ; 6 à 65 535 blocs. Label ASCII majuscule de 1 à 7 caractères. Autres ordres des octets et variantes de répertoire à couvrir. |
| CP/M 2.2 | Répertoire linéaire vide implémenté | Allocation, entrées et zone réservée explicites ; traduction des secteurs à assurer par l’adaptateur. |
| CP/M 3 et autres variantes | À couvrir | Horodatages et extensions de répertoire propres à chaque variante. |
| CMD natif, structures DOS à BAM pour HDD | À couvrir | Types de partitions, répertoires et tables d’allocation. |
| DOS D90 | Volume vide implémenté | Chaîne BAM couvrant les 153 cylindres, métadonnées allouées, 19 441 ou 29 162 blocs disponibles hors piste zéro selon le profil. Labels de 1 à 16 caractères ASCII majuscules, identifiant de deux caractères paramétrable dans le constructeur. Secteurs de 256 octets. |
| FATX | Volumes little-endian et big-endian implémentés | Secteurs de 512 octets, clusters de 1 à 128 secteurs en little-endian, 8 à 128 en big-endian ; FAT à entrées de 16 ou 32 bits et répertoire vide. Profil 1 Mio à 1 Tio, sans label. Dispositions globales du disque à couvrir séparément. |
| PFS associé à APA | À couvrir | Zones, allocation et répertoires propres. |
| ext2 | Révision dynamique implémentée | Blocs de 4 Kio, inodes de 128 octets, feature filetype, copies du superbloc et des descripteurs dans tous les groupes ; 8 Mio à 128 Gio sous réserve de place pour les métadonnées du dernier groupe. |
| ext3 | Profil journalisé implémenté | Journal interne JBD v2 de 4 Mio, inode 8, adressage direct/indirect ; mêmes tailles de blocs et limites que le profil ext2. Autres tailles de journal et features à couvrir. |
| ext4 | Profil avec extents et journal implémenté | Journal interne de 4 Mio, extents dans les inodes, blocs de 4 Kio et inodes de 128 octets. Features 64 bits, checksums de métadonnées, flex_bg, bigalloc, chiffrement et autres variantes à couvrir. |
| UFS1 / UFS2, FFS BSD | À couvrir | Ordre des octets et variantes ; distinct de FFS DOS1. |
| Minix | V1, V2 et V3, quatre ordres de stockage implémentés | Little-endian et big-endian avec bitmaps indexés sur 16, 32 ou 64 bits. V1/V2 : noms de 14 ou 30 octets ; V3 : 60 octets. Blocs et zones de 1 Kio, 256 inodes par défaut, sans label. V1 jusqu’à 65 535 blocs, V2/V3 jusqu’à 256 Gio sous réserve de place pour les métadonnées. Autres tailles de blocs et de zones à couvrir. |
| System V, Xenix | À étudier | Versions et structures historiques. |
| XFS, Btrfs, ZFS, JFS, ReiserFS | À étudier | Chaque format et version constitue une implémentation séparée. |
| BFS (boot filesystem) | Volume vide little-endian implémenté | Blocs de 512 octets, inode racine et répertoire unique, 8 à 512 inodes par multiples de huit, labels ASCII de six octets, jusqu’à 4 Gio. Structure distincte de BeFS. Aucun chargeur installé. |
| BeFS, QNX4 / QNX6 | À étudier | Allocation, attributs et variantes. |
| ADFS / FileCore | À couvrir | Formats de cartes, répertoires et géométrie. |
| V7 | Volumes vides little-endian, big-endian et PDP-endian implémentés | Blocs de 512 octets, inodes de 64 octets, pointeurs de 24 bits, liste chaînée de blocs libres, racine et inode de défauts vide. 256 inodes par défaut, nombre paramétrable par multiples de huit ; plafond de 0xFFFFFF blocs. Sans label dans ce profil. |
| EFS, VxFS, autres formats UNIX historiques | À étudier | Spécification et tests propres à chaque variante. |
| UDF, ISO 9660 et extensions | À étudier pour les volumes HDD pertinents | Le système de fichiers est distinct du type de média ; pas de constructeur de disque optique implicite. |
| F2FS, systèmes compressés ou embarqués | À étudier | Pertinence pour un périphérique bloc et paramètres attendus. |
| Autres systèmes propriétaires ou historiques | À inventorier | Ajouter les variantes documentées ; ne pas les ramener arbitrairement à FAT. |

Références de structures : [HFS+ et HFSX](https://developer.apple.com/library/archive/technotes/tn/tn1150.html), [comparaison APFS/HFS+](https://developer.apple.com/library/archive/documentation/FileManagement/Conceptual/APFS_Guide/VolumeFormatComparison/VolumeFormatComparison.html), [formats documentés par CiderPress](https://ciderpress2.com/doc-index.html), [systèmes de fichiers du noyau Linux](https://docs.kernel.org/filesystems/), [FATX](https://free60.org/System-Software/Systems/FATX/), [PFS](https://github.com/ps2dev/ps2sdk/tree/master/iop/hdd/pfs) et [paramètres CP/M](https://www.cpm.z80.de/manuals/cpm22-m.pdf).

## 4. Travaux transversaux à effectuer

Les profils CHD, QCOW v1 et Parallels sont vérifiés par lecture des structures sérialisées dans les tests, sans lancer de programme externe. HFS+/HFSX et ext2/3/4 sont aussi rouverts par les lecteurs intégrés. Cela ne constitue pas un essai d’amorçage ni une certification de chaque consommateur possible.

Spécifications complémentaires : [CHD v5](https://github.com/mamedev/mame/blob/master/src/lib/util/chd.h), [lecture des hunks CHD](https://github.com/mamedev/mame/blob/master/src/lib/util/chd.cpp), [Parallels extensible](https://www.qemu.org/docs/master/interop/parallels.html), [QCOW v1](https://github.com/qemu/qemu/blob/master/block/qcow.c), [journal JBD](https://www.kernel.org/doc/html/latest/filesystems/ext4/journal.html), [extents](https://www.kernel.org/doc/html/latest/filesystems/ext4/ifork.html), [HFS+/HFSX](https://developer.apple.com/library/archive/technotes/tn/tn1150.html) et [structures HFS](https://github.com/apple-oss-distributions/hfs/blob/main/core/hfs_format.h).

Références supplémentaires utilisées pour les constructeurs : [ProDOS](https://ciderpress2.com/formatdoc/ProDOS-notes.html), [CP/M 2.2](https://www.seasip.info/Cpm/format22.html), [QED](https://www.qemu.org/docs/master/interop/qed_spec.html), [exFAT](https://learn.microsoft.com/en-us/windows/win32/fileio/exfat-specification), [superbloc ext](https://www.kernel.org/doc/html/latest/filesystems/ext4/super.html), [descripteurs de groupes ext](https://www.kernel.org/doc/html/latest/filesystems/ext4/group_descr.html), [structures APM](https://raw.githubusercontent.com/torvalds/linux/master/block/partitions/mac.h) et [lecteur FATX](https://github.com/mborgerson/fatx/tree/master/libfatx).

- [ ] Définir des descripteurs séparés pour conteneurs, partitionnements et systèmes de fichiers.
  - [x] Enregistrer des implémentations indépendantes par identifiant avec callbacks de validation et de construction.
  - [x] Identifier famille, variante construite, famille de signature et extensions suggérées sans les confondre ; renvoyer tous les candidats d’une extension ambiguë.
  - [x] Déclarer séparément création, lecture, écriture et conversion : le registre actuel annonce uniquement la création effectivement fournie par ses callbacks.
  - [ ] Décrire les paramètres et limites intrinsèques de chaque format.
- [ ] Remplacer les combinaisons prédéfinies par une composition explicite.
  - [x] Fournir une API de composition et une surcharge de création de fichier utilisant le même plan.
  - [x] Appliquer cette composition aux ensembles sparsebundle et VMDK : registre séparé, contrôle du consommateur, partitions et formatages communs, publication par dossier.
  - [x] Définir plusieurs partitions avec leurs systèmes de fichiers et options.
  - [x] Valider offsets, chevauchements et zones réservées pour les profils de secteurs de 512 octets implémentés.
  - [ ] Étendre les tailles de secteurs et géométries paramétrables.
  - [x] Croiser les capacités techniques avec un profil consommateur explicite dans l’API composée : combinaisons, secteurs, capacités du disque et des volumes, nombre de volumes, alignement et restrictions supplémentaires.
  - [ ] Migrer les adaptateurs utilisant les anciens profils vers la composition lorsqu’ils doivent exposer de nouvelles combinaisons.
- [ ] Décomposer chaque famille à couvrir en formats et variantes documentés.
  - [ ] Vérifier les bibliothèques intégrables, leurs licences et leurs capacités réelles de création.
  - [ ] Fournir un constructeur ou formateur indépendant pour chaque format absent.
  - [ ] Garder les fonctions d’installation et d’amorçage séparées du formatage.
- [ ] Étendre la gestion du cycle de vie des images.
  - [ ] Suivre les parents, fichiers associés, snapshots et volumes agrégés.
    - [x] Lire les parents VHD/VHDX, les extents et parents VMDK, les références backing QCOW/QED ; borner la lecture des métadonnées et le parcours.
    - [ ] Résoudre les parents identifiés par hash/UUID, les ensembles segmentés et les organisations supplémentaires.
  - [ ] Protéger toutes les dépendances lors du retrait et de la suppression.
    - [x] Raccorder le graphe connu au contrôle de suppression et l’actualiser après confirmation ; comparer les alias sans changer la base des références relatives.
    - [ ] Présenter et supprimer collectivement un ensemble après contrôle de chacun de ses membres.
  - [ ] Préserver les originaux lors des conversions et créations interrompues.
    - [x] Publier un ensemble complet par renommage de son dossier temporaire, sans remplacement ; tester écritures interrompues, collisions, erreurs de producteur et de nettoyage en mémoire.
    - [ ] Étendre cette garantie aux conversions lorsqu’elles seront implémentées.
- [ ] Valider chaque variante sur des données simulées en mémoire.
  - [ ] Réouvrir avec un lecteur indépendant lorsque disponible.
  - [ ] Vérifier structures, allocation, checksums, données et limites.
  - [ ] Tester les combinaisons invalides et les erreurs sans construire de vrais fichiers images.

La couverture s’étend format par format. Cette liste recense le travail ; elle n’ajoute aucune capacité au code par simple déclaration.

### Formatage Pascal

- [x] Construire un volume vide little-endian avec blocs de 512 octets, deux blocs système réservés et quatre blocs de répertoire.
- [x] Enregistrer le formateur `pascal` dans la composition commune et valider taille et nom avant toute écriture.
- [x] Vérifier la réouverture, les limites et la composition dans une partition sur des flux en mémoire.
- [ ] Couvrir les autres variantes de répertoire et d’ordre des octets.

La [description du format Pascal](https://ciderpress2.com/formatdoc/Pascal-notes.html) documente les champs et restrictions utilisés. Le lecteur UCSD existant sert à vérifier le résultat ; les blocs système réservés doivent être déduits de l’espace libre, même sans chargeur installé.

### Partitions étendues MBR

- [x] Déclarer les volumes logiques séparément des partitions primaires dans le plan commun.
- [x] Construire une étendue LBA 0x0F et sa chaîne EBR avec les deux bases d’adressage relatives.
- [x] Rejeter les collisions de données et de métadonnées, le dépassement des quatre entrées primaires et les volumes logiques actifs avant toute écriture.
- [x] Rouvrir une chaîne de six volumes logiques avec le lecteur intégré et lire/écrire leurs fichiers en mémoire.
- [x] Construire le profil CHS à étendue 0x05 avec géométrie explicite et une piste réservée par EBR ; vérifier les deux bases relatives et le cylindre 1023.
- [ ] Couvrir les autres dispositions EBR historiques.

La [lecture des partitions étendues du noyau Linux](https://github.com/torvalds/linux/blob/master/block/partitions/msdos.c) décrit les adresses relatives utilisées : données par rapport à leur EBR, liens par rapport au début de l’étendue.

### Extensions suivantes du socle

- [x] Construire et vérifier les chaînes XGM, leur réserve de métadonnées et les collisions.
- [x] Construire et vérifier les douze entrées ICD/Supra.
- [x] Construire QCOW2 v2 et v3 avec clusters paramétrables et table de refcounts sur plusieurs clusters.
- [x] Vérifier les en-têtes HDI/NHD/THD, leur géométrie et l’accès aux données sans lancer de programme externe.
- [x] Paramétrer la géométrie RDB et calculer la réserve de métadonnées selon le nombre de partitions.
- [x] Construire les volumes MFS et HFS classique vides et les rouvrir avec les lecteurs du dépôt.
- [x] Construire les variantes DOS2 à DOS5 et vérifier cache de répertoire, allocation et type de partition RDB.
- [x] Construire et vérifier les volumes Minix v1/v2/v3 sur des flux simulés.
- [x] Construire les disklabels BSD dans les deux ordres d’octets et vérifier les sept volumes de données.
- [x] Construire et rouvrir les ensembles VMDK à descripteur séparé, VMFS flat et sparse, entièrement en mémoire.
- [x] Construire FATX big-endian et vérifier le passage des entrées FAT de 16 à 32 bits.
- [x] Conserver les noms Mac Roman lors du formatage et de la relecture MFS/HFS classique.
- [x] Construire FAT12/16/32 avec secteurs de 512 à 4096 octets et lire/écrire des fichiers en mémoire.
- [x] Construire les labels Sun VTOC v1 et SGI, vérifier leurs checksums, limites, réserves et volumes.
- [x] Construire UDIF v4 RAW et zlib, reconstituer les secteurs depuis la table XML et relire les fichiers en mémoire.
- [x] Formater les deux profils D90 et vérifier la couverture BAM, les blocs libres et les références de métadonnées.
- [x] Construire les enfants VHD/VHDX de parents autonomes sur flux ; vérifier l’héritage, les écritures de l’enfant et l’intégrité du parent.
- [x] Construire Bochs Growing v1/v2 avec catalogue et bitmaps ; vérifier les frontières d’extents et le dernier secteur partiel.
- [x] Déduire les types MBR/GPT connus du système de fichiers et demander un type explicite pour les autres associations.
- [x] Séparer le nom de partition et le label du système de fichiers dans la composition GPT, APM et RDB.
- [x] Décomposer les labels Latin-1 HFS+/HFSX et valider leur longueur sérialisée.
- [x] Construire cloop v2 et vérifier offsets, décompression complète des blocs et relecture du volume.
- [x] Construire V7 dans les trois ordres d’octets et vérifier la chaîne complète d’allocation libre.
- [x] Construire BFS little-endian et vérifier inodes, extent du répertoire, état de compaction propre, labels et borne de 4 Gio en mémoire.
- [x] Construire les vingt profils Minix combinant version, ordre des métadonnées/bitmaps et longueur des noms ; vérifier chaque bit d’allocation et les entrées racines.
- [x] Construire GPT en secteurs de 512, 1024, 2048 et 4096 octets, vérifier les copies, les zones réservées, les combinaisons de secteurs et la récupération depuis le secours.
- [x] Construire les ensembles VMDK monolithicFlat et twoGbMaxExtentFlat/Sparse ; rouvrir leurs références et vérifier les frontières et le dernier extent partiel.
- [x] Construire les chaînes différentielles VHD/VHDX fournies explicitement ; vérifier identités, héritage, masquage par zéros et intégrité de tous les ancêtres.
- [x] Construire VHDX fixe/dynamique en 4Kn, rouvrir GPT/FAT, vérifier une frontière de chunk à 32 Gio et l’héritage différentiel 4Kn.
- [x] Construire VMDK streamOptimized avec grains zlib, marqueurs, tables et footer ; relire les fichiers, décompresser les grains et vérifier les images entièrement vides en mémoire.
- [x] Construire sparsebundle avec ses métadonnées, token et bandes ; reconstruire le disque et relire un fichier traversant plusieurs bandes en mémoire.
- [x] Publier les arborescences d’images avec validation de chaque composant de chemin, détection des collisions fichier/dossier et nettoyage non récursif des seuls éléments créés.
- [x] Construire sparseimage v3 à en-tête unique, vérifier les index physiques/logiques, la dernière bande, les zones nulles et le refus d’un renommage en RAW.
- [x] Propager la géométrie BIOS du profil CHS aux secteurs de démarrage FAT et NTFS, y compris la copie de secours FAT32 ; vérifier la réouverture des volumes primaires et logiques.
- [x] Construire les cinq copies de secours Sun VTOC v1, réserver les cylindres alternatifs et vérifier leurs checksums, emplacements et exclusions des volumes.
- [x] Construire les dix profils SWAPSPACE2 v1, vérifier UUID, label, pages défectueuses, signature à la fin de la page, limites et types MBR/GPT.
- [x] Intégrer le formateur PFS3 géré, fixer ses dépendances, distribuer ses notices et vérifier formatage, dossiers, fichiers, réouverture, mode super-index et capacité maximale en mémoire.
- [x] Vérifier les types de pilote RDB PFS3/PDS3 autour du même volume, sans charger de pilote ni de machine externe.
- [x] Exposer les identités de tous les profils enregistrés et conserver les extensions de registre indépendantes.
- [x] Vérifier les plafonds de compatibilité VHD/VHDX avant construction et empêcher qu’une initialisation échouée ou redimensionnée publie un début de conteneur.
- [x] Refuser les conteneurs identifiables renommés en RAW, y compris lorsqu’ils sont encapsulés dans gzip.

Références supplémentaires : [cloop v2](https://github.com/qemu/qemu/blob/master/block/cloop.c), [structures V7FS](https://github.com/NetBSD/src/blob/trunk/sys/fs/v7fs/v7fs.h) et [ordres d’octets V7FS](https://github.com/NetBSD/src/blob/trunk/sys/fs/v7fs/v7fs_endian.c).

Le profil BFS suit les [structures du système de fichiers](https://github.com/torvalds/linux/blob/master/include/uapi/linux/bfs_fs.h) et les contraintes du [formateur de référence](https://github.com/util-linux/util-linux/blob/master/disk-utils/mkfs.bfs.c). Ses tests relisent les métadonnées en mémoire ; aucun essai de montage externe n’est annoncé.

Les variantes Minix distinguent l’ordre des entiers et l’indexation des bitmaps, comme le [lecteur du noyau](https://github.com/torvalds/linux/blob/master/fs/minix/minix.h). Le suffixe `-be16`, `-be32` ou `-be64` indique la largeur de mot des bitmaps big-endian ; `-n14` sélectionne les noms courts V1/V2. Ces profils n’impliquent pas qu’un même consommateur sache tous les monter.

Les ensembles VMDK suivent les [variantes de conteneur documentées](https://www.qemu.org/docs/master/system/images) et les [champs du descripteur intégré](https://github.com/LTRData/DiscUtils/blob/f00254d88f8f119a99f6a36751baf0d5a7f26d65/Library/DiscUtils.Vmdk/DescriptorFile.cs). Les extents sparse utilisent la structure hosted sparse du constructeur intégré, avec la référence de descripteur embarqué désactivée au profit du descripteur externe. Leur réouverture est testée sur des flux mémoire.

Le profil streamOptimized utilise les marqueurs et le footer décrits par le [lecteur de référence VMDK](https://github.com/qemu/qemu/blob/master/block/vmdk.c). La [lecture des grains comprimés intégrée](https://github.com/LTRData/DiscUtils/blob/f00254d88f8f119a99f6a36751baf0d5a7f26d65/Library/DiscUtils.Vmdk/HostedSparseExtentStream.cs) vérifie le résultat ; les tests décompressent aussi les grains avec `ZLibStream`. Le profil est enregistré sous `vmdk-stream`, séparément du monolithique sparse inscriptible.

Le constructeur sparsebundle suit les métadonnées et l’adressage des bandes décrits par [l’implémentation Apple](https://github.com/apple-oss-distributions/hfs/blob/main/CopyHFSMeta/SparseBundle.c). Une bande absente ou tronquée représente des zéros pour sa partie absente. Le profil sparseimage suit [l’en-tête et son index](https://github.com/libyal/libmodi/blob/main/libmodi/libmodi_sparse_image_header.c) et reste limité à un en-tête de 4 Kio. Les tests reconstruisent les secteurs puis utilisent les lecteurs de volumes intégrés ; aucun montage externe de ces conteneurs n’est annoncé.

L’adressage CHS suit les [champs et contrôles de cohérence MBR](https://github.com/util-linux/util-linux/blob/master/libfdisk/src/dos.c). Le profil exige des adresses représentables, sans saturation des cylindres ; le consommateur reste responsable de déclarer la géométrie qu’il attend. Ce profil n’installe pas de chargeur et n’impose pas de fin de partition sur une frontière de cylindre.

Les secours Sun suivent les [emplacements vérifiés par le formateur illumos](https://github.com/illumos/illumos-gate/blob/master/usr/src/cmd/format/label.c) : cinq secteurs impairs du dernier cylindre alternatif, dernière tête par défaut ou tête fournie explicitement. `sun-vtoc-backup` réserve deux cylindres et limite la slice globale aux cylindres utilisables. `SunLabelBackupOptions` permet de régler cette réserve et la tête.

Les zones d’échange suivent [l’en-tête SWAPSPACE2](https://github.com/torvalds/linux/blob/master/include/linux/swap.h) et les bornes du [formateur mkswap](https://github.com/util-linux/util-linux/blob/master/disk-utils/mkswap.c). `linux-swap-4k`, `-8k`, `-16k`, `-32k`, `-64k` et leurs variantes suffixées `-be` sont enregistrés. Le constructeur génère des métadonnées sur flux ; il n’active aucune zone d’échange du système hôte.

Le moteur PFS3 intégré est adapté du [code Hst.Amiga figé au commit de la version 0.6.236](https://github.com/henrikstengaard/hst-amiga/blob/be46479b058264b88b73346a496a04525205f431/src/Hst.Amiga/FileSystems/Pfs3/Pfs3Formatter.cs). Les [constantes de ce profil](https://github.com/henrikstengaard/hst-amiga/blob/be46479b058264b88b73346a496a04525205f431/src/Hst.Amiga/FileSystems/Pfs3/Constants.cs) définissent son plafond sans mode expérimental. Il est compilé directement dans GWGUI.Emulation et n'ajoute aucune dépendance Hst au paquet. Les tests utilisent ce moteur intégré, des flux simulés et des fichiers synthétiques. Le secteur de volume porte `PFS\1`, distinct des types de pilote `PFS\3` et `PDS\3` dans RDB. This product includes software developed by Michiel Pelt. Les [notices distribuées](../../THIRD-PARTY-NOTICES.md) incluent les licences MIT et PFS3.

Références de représentation : [Growing redolog](https://github.com/qemu/qemu/blob/master/block/bochs.c), [types MBR](https://github.com/util-linux/util-linux/blob/master/include/pt-mbr-partnames.h), [types GPT](https://github.com/util-linux/util-linux/blob/master/include/pt-gpt-partnames.h) et [labels HFS+/HFSX](https://developer.apple.com/library/archive/technotes/tn/tn1150.html).

Le constructeur VHD différentiel sérialise ses métadonnées directement : la version intégrée de DiscUtils appelle `ArrayPool.Return(null)` lorsque la BAT est petite. Les lecteurs et flux de contenu intégrés restent utilisés pour la vérification et les écritures de l’enfant. Les constructeurs différentiels ne sont pas enregistrés comme conteneurs autonomes : ils exigent un parent et ses références explicites.

Références de ces extensions : [disklabel BSD](https://github.com/NetBSD/src/blob/trunk/sys/sys/disklabel.h), [label Sun](https://github.com/util-linux/util-linux/blob/master/include/pt-sun.h), [volume header SGI](https://github.com/util-linux/util-linux/blob/master/include/pt-sgi.h), [constructeur VMDK intégré](https://github.com/LTRData/DiscUtils/blob/f00254d88f8f119a99f6a36751baf0d5a7f26d65/Library/DiscUtils.Vmdk/DiskBuilder.cs), [spécification FAT](https://www.scs.stanford.edu/~zyedidia/docs/_other/fat.pdf), [structures UDIF](https://github.com/planetbeing/libdmg-hfsplus/blob/master/includes/dmg/dmg.h) et [lecture des blocs UDIF](https://github.com/qemu/qemu/blob/master/block/dmg.c).

Sources complémentaires : [XGM et ICD](https://github.com/torvalds/linux/blob/master/block/partitions/atari.c), [en-têtes HDI/NHD/THD](https://github.com/AZO234/NP2kai/blob/master/fdd/sxsihdd.h), [MFS](https://ciderpress2.com/formatdoc/MFS-notes.html), [HFS classique](https://ciderpress2.com/formatdoc/HFS-notes.html), [structures HFS](https://github.com/apple-oss-distributions/hfs/blob/main/core/hfs_format.h), [OFS/FFS et cache de répertoire](https://adflib.github.io/FAQ/adf_info.html), [structures Minix](https://github.com/torvalds/linux/blob/master/include/uapi/linux/minix_fs.h), [formateur Minix de référence](https://github.com/util-linux/util-linux/blob/master/disk-utils/mkfs.minix.c).

Ces tests ne certifient ni l’amorçage ni les variantes non implémentées. Les lecteurs structurels des tests HDI/NHD/THD, XGM/ICD et Minix ne constituent pas une nouvelle fonction d’exploration dans l’application.
