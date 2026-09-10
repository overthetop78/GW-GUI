# Images de disques durs

## Périmètre

Le service de disques est généraliste. Son catalogue couvre les formats de stockage indépendamment des modules présents dans l’application. L’ajout d’un module ne doit pas nécessiter de dupliquer un constructeur d’image ou un formateur.

L’inventaire des formats et variantes à couvrir est conservé dans [le catalogue](hard-disk-format-catalog.md). Une entrée recensée n’est pas une capacité implémentée. Le catalogue reste ouvert : aucun ensemble fini de formats ne garantit à lui seul la compatibilité avec tous les systèmes existants ou futurs.

## Couches indépendantes

| Couche | Responsabilité |
|---|---|
| Conteneur | Stocker les secteurs logiques : en-tête, allocation fixe ou dynamique, compression, images différentielles, fichiers multiples. |
| Organisation du disque | Décrire les partitions, volumes, zones réservées, alignements et géométrie nécessaires. Un volume peut aussi occuper directement le disque. |
| Système de fichiers | Initialiser le volume : structures d’allocation, répertoire racine, métadonnées, journal éventuel. |
| Amorçage et installation | Installer séparément le chargeur, les pilotes et le système lorsque cela est demandé et possible. |
| Adaptateur de stockage | Déclarer les combinaisons acceptées par le consommateur du disque et effectuer son branchement. |

Le code de construction se trouve dans `src/GWGUI.Emulation/HardDisks`, avec les dossiers `Containers`, `Partitioning` et `FileSystems`. Les opérations travaillent sur des flux ; les tests construisent leurs données en mémoire.

Un nom de format historique reste un identifiant technique valide. Il ne signifie pas que son implémentation dépend d’une machine ou d’un émulateur. L’organisation se fait par format, jamais par la liste des modules installés.

## État du socle

| Couche | Construction actuellement implémentée |
|---|---|
| Conteneurs autonomes | RAW ; gzip ; VHD, VHDX et VDI fixes ou dynamiques ; VMDK monolithique sparse ou streamOptimized ; QCOW v1 et QCOW2 v2/v3 ; QED ; 2IMG v1 à blocs ; CHD v5 non compressé ; Parallels extensible v2 ; HDI, NHD R0 et THD ; UDIF v4 RAW ou zlib ; Bochs Growing v1/v2 ; cloop v2. |
| Conteneurs avec dépendances | Chaînes différentielles VHD/VHDX fournies sur flux ; ensembles VMDK monolithicFlat, split flat/sparse et VMFS flat/sparse à descripteur séparé. Publication par dossier et contrôle des références disponibles ; exposition des nouveaux profils à poursuivre. |
| Partitionnement | MBR primaire ou étendu avec EBR ; GPT jusqu’à 128 partitions ; AHDI primaire ou étendu avec XGM ; ICD jusqu’à douze partitions ; RDB avec géométrie et réserve de métadonnées paramétrables ; APM jusqu’à 62 partitions de données ; disklabel BSD dans les deux ordres d’octets ; Sun VTOC v1 ; SGI volume header. |
| Formatage | FAT automatique ou FAT12/16/32 explicites ; FAT16 à secteurs logiques adaptés ; exFAT ; FATX little-endian et big-endian ; NTFS ; OFS et FFS DOS0 à DOS5 ; ProDOS ; Pascal little-endian ; CP/M 2.2 linéaire ; ext2/3/4 ; Minix v1/v2/v3 ; V7 little/big/PDP-endian ; DOS D90 ; MFS ; HFS classique ; HFS+/HFSX non journalisés. |
| Image vierge | Contenu logique sans partition ni système de fichiers. |

Les variantes exactes, limitations et extensions futures figurent dans le catalogue. Les constructeurs VHD, VHDX, VDI, VMDK, FAT automatique, exFAT et NTFS utilisent LTRData.DiscUtils 1.0.88. Les lecteurs ext et HFS+ de cette bibliothèque permettent de vérifier indépendamment les volumes correspondants. Les formatages n’installent pas de système d’exploitation ni de pilote propriétaire ; une image formatée n’est pas nécessairement amorçable.

`DiskImagePlan` décrit le conteneur, la table et les volumes ; chaque `DiskVolumePlan` précise sa position, sa longueur, son système de fichiers et ses paramètres. `DiskImageBuilder.Write` valide puis construit sur un flux ; `HardDiskImageCreation.Create` possède une surcharge utilisant le même plan avec publication sans écrasement.

`DiskImageBuilder.Validate` permet de vérifier un plan sans construire son image et sans ouvrir de fichier. Il applique les mêmes contrôles que la création.

`DiskFormatRegistry` enregistre séparément les constructeurs, tables et formateurs par identifiant. Un module peut fournir une nouvelle implémentation sans ajouter une valeur à une énumération de combinaisons. Le registre annonce uniquement les implémentations enregistrées, sans prétendre prendre en charge les formats encore recensés comme manquants. Les anciens profils prédéfinis restent disponibles pour les appelants existants.

Les collections `Containers`, `PartitionTables` et `FileSystems` exposent leurs descripteurs. `Identity` distingue famille, variante, extensions suggérées et famille de signature lorsqu’elle existe. Les opérations annoncées par ce registre de constructeurs sont limitées à `Create` ; cela n’annonce ni lecteur, ni éditeur, ni convertisseur. `FindContainersByExtension` renvoie tous les candidats : `.vhd` peut désigner RAW ou VHD, `.qcow2` plusieurs versions et `.dmg` plusieurs profils UDIF. Le contenu doit être contrôlé séparément. Les extensions de conteneurs ne sont pas attribuées aux systèmes de fichiers.

Exemple de composition sur un flux en mémoire :

```csharp
using var image = new DiscUtils.Streams.SparseMemoryStream();
DiskImageBuilder.Write(image, new DiskImagePlan(
    128L << 20, "raw", "gpt",
    [new DiskVolumePlan(1L << 20, 32L << 20, "fat16", "DATA"),
     new DiskVolumePlan(40L << 20, 64L << 20, "exfat", "ARCHIVE")]));
```

Les types de partitions peuvent être fournis explicitement (`MbrType`, `GptType`, `PartitionType`). CP/M exige des paramètres d’allocation, de répertoire et de zone réservée explicites. La composition rejette les chevauchements, débordements et zones de métadonnées occupées avant toute écriture. Les constructeurs de fichiers conservent leur nettoyage du temporaire si une étape échoue.

Les types MBR/GPT connus suivent le système de fichiers choisi ; une association sans type par défaut exige une valeur explicite. `PartitionName` permet de nommer la partition indépendamment de `Label`, qui reste le nom interne du volume. Sans `PartitionName`, le comportement existant utilisant `Label` est conservé.

Pour MBR, `MbrLogical: true` déclare un volume logique. Le constructeur réserve le secteur de 512 octets immédiatement avant ses données pour son EBR. L’étendue de type LBA 0x0F englobe la chaîne, triée par position ; aucune partition primaire ne peut occuper cet intervalle, même entre deux volumes logiques. Une partition active doit rester primaire. Cette option est refusée pour les autres tables. Le profil n’installe aucun chargeur et ne fournit pas de variante d’adressage CHS historique.

Le profil distinct `mbr-chs` utilise une étendue 0x05 et une piste réservée avant chaque volume logique. Ses départs de partitions sont alignés sur les pistes ; les fins ne sont pas contraintes aux cylindres. La géométrie enregistrée est de 16 têtes et 63 secteurs de 512 octets, soit 504 Mio adressables sur 1 024 cylindres. `ChsMbrPartitionWriter` accepte aussi une `DiskChsGeometry` explicite. Les adresses CHS et LBA sont calculées ensemble, les dépassements refusés. Les types de données explicitement LBA sont refusés ; FAT32 reçoit 0x0B par défaut dans ce profil.

Les descripteurs de tables indiquent `SupportsMbrLogical` et peuvent fournir `BiosGeometry`. La composition reporte cette géométrie dans les volumes et refuse une valeur contradictoire. Les formateurs FAT et NTFS l’inscrivent dans leurs secteurs de démarrage ; FAT32 l’utilise également pour sa copie de secours. Sans géométrie explicite, les profils existants conservent leur comportement.

## Capacités et validation

Un descripteur de format doit distinguer l’identifiant, la version, les signatures, les extensions usuelles, la taille de secteur, les opérations disponibles et leurs limites. Une extension n’identifie pas à elle seule le contenu. Un bus de stockage n’est ni un conteneur ni un système de fichiers.

Les limites se calculent pour la combinaison choisie : conteneur, organisation du disque, système de fichiers, adressage du contrôleur, pilote et système consommateur. Distinguer capacité totale du disque, taille d’une partition, taille d’un fichier et taille physique occupée. Aucun plafond global fondé sur les modules actuellement présents ne doit limiter les constructeurs génériques.

Les adaptateurs déclarent leurs capacités ; le service commun les croise avec celles des constructeurs. La création, la lecture, l’écriture, la conversion et l’amorçage sont des capacités distinctes. Une bibliothèque capable de lire un format ne justifie pas d’annoncer sa création.

L’API composée accepte un `DiskConsumerProfile` facultatif. Il décrit des combinaisons explicites de conteneur, partitionnement et systèmes de fichiers, avec les secteurs autorisés, la capacité minimale/maximale du disque, son alignement et les limites distinctes des volumes. Deux combinaisons déclarées ne rendent pas leur produit croisé valide. Un callback permet les restrictions supplémentaires du contrôleur ou du pilote. Ces contrôles s’ajoutent à ceux des formats et s’exécutent avant l’ouverture du fichier de création. Sans consommateur fourni, la construction générique garde les seules limites de ses formats. Les anciens adaptateurs utilisant `HardDiskImageFormat` conservent leur chemin de validation existant ; ils ne sont pas implicitement migrés vers toutes les nouvelles combinaisons.

`DiskVolumePlan.SectorBytes` permet de choisir les secteurs de 512, 1024, 2048 ou 4096 octets pour les FAT explicites. Le registre refuse les tailles non prises en charge par le formateur et les volumes mal alignés. GPT possède les profils `gpt` (512 octets), `gpt-1024`, `gpt-2048` et `gpt-4096` ; ils sont croisés avec les secteurs déclarés par le conteneur. RAW et gzip peuvent porter ces profils. `vhdx-4kn` crée un VHDX fixe ou dynamique avec secteurs logiques et physiques de 4096 octets ; `vhdx` conserve les secteurs logiques de 512 octets. Les autres conteneurs enregistrés gardent leur profil logique de 512 octets. Les volumes directs sont également contrôlés : un système de fichiers à secteurs de 512 octets ne peut pas être installé dans le profil VHDX 4Kn. Les tables historiques gardent leurs unités de 512 octets. Les constructeurs HDI/NHD possèdent également leurs paramètres de secteur et de géométrie propres.

La sérialisation GPT initialise les octets réservés, les deux en-têtes et les deux tableaux d’entrées, avec leurs CRC. Le lecteur intégré vérifie la réouverture et le recours à la copie de secours. Les profils automatiques historiques conservent leur méthode de disposition et effacent également la fin réservée des en-têtes après modification par la bibliothèque.

Le profil `sun-vtoc-backup` ajoute cinq copies du label sur les secteurs impairs 1 à 9 de la dernière piste du dernier cylindre alternatif. Les deux cylindres alternatifs réservés par défaut sont exclus des volumes et de la slice globale ; les champs de géométrie distinguent cylindres physiques, utilisables et alternatifs. `SunDisklabelWriter.Describe` accepte `SunLabelBackupOptions` pour régler la réserve et la tête portant les secours. `sun-vtoc` conserve son profil sans secours.

Les profils VHD et VHDX refusent respectivement les capacités dépassant 2 040 Gio et 64 Tio, avant toute construction ; ce sont les plafonds de compatibilité retenus à partir des [limites publiées par Microsoft](https://learn.microsoft.com/en-us/windows-server/virtualization/hyper-v/maximum-scale-limits). Ils s’appliquent aussi aux parents des constructeurs différentiels. Les limites supplémentaires d’un système de fichiers ou d’un consommateur restent distinctes.

L’initialisation des secteurs est préparée dans un flux mémoire sparse avant d’émettre le conteneur. Une exception du formateur ou une modification de la capacité logique est refusée sans publier de début d’image. Une panne du flux de destination pendant l’écriture peut néanmoins produire une sortie partielle ; la publication de fichiers utilise donc toujours un temporaire, supprimé en cas d’échec.

`VmdkImageSetWriter` construit les ensembles VMFS flat/sparse, monolithicFlat et split flat/sparse. `VmdkSplitImageWriter` permet aussi de régler la taille des extents. Le callback reçoit chaque nom et un flux à consommer immédiatement ; les extents sont émis avant le descripteur. Cette API est vérifiée avec des ensembles simulés en mémoire. Un échec du callback interrompt l’émission ; le service de publication conserve donc les membres dans une zone temporaire jusqu’à réussite complète.

`DiskImageSetPublication.Create` fournit cette publication pour un ensemble placé dans son propre dossier. Tous les membres sont écrits dans un dossier temporaire voisin, puis un renommage du dossier rend l’ensemble disponible sans remplacer une destination existante. Les noms simples, doublons et présence du point d’entrée sont vérifiés. Une erreur, même interceptée par le producteur, interdit la publication. Le nettoyage vise les fichiers créés pendant cette transaction ; il ne supprime pas récursivement des fichiers inattendus. Les tests remplacent le stockage par un simulateur en mémoire. L’exposition de cette API dans le dialogue de création reste à effectuer pour les consommateurs qui accepteront ces variantes.

`CreateTree` accepte aussi des sous-dossiers relatifs, avec `/` comme séparateur portable. Chaque composant est contrôlé, les remontées et noms de périphériques refusés, les collisions entre fichier et dossier détectées. Le nettoyage inclut les seuls sous-dossiers créés, sans suppression récursive. Les dossiers redirigés sont refusés avant publication et nettoyage.

Le registre `ImageSets` expose six profils distincts : `sparsebundle` et les cinq ensembles VMDK. `DiskImageBuilder.ValidateSet`, `WriteSet` et `CreateSet` utilisent le même `DiskImagePlan`, les mêmes formateurs et les mêmes contrôles consommateur que les conteneurs monofichier. `WriteSet` émet des flux ; `CreateSet` publie un dossier neuf et renvoie le chemin du point d’entrée (`Info.plist` ou `disk.vmdk`). L’identifiant d’un ensemble ne peut pas masquer celui d’un conteneur monofichier. Les noms internes VMDK sont dérivés de `disk` dans ces profils ; les constructeurs directs permettent toujours un autre nom.

`SparseBundleImageWriter` produit un ensemble non chiffré, avec deux copies de la plist, un token vide et des bandes indexées en hexadécimal. Le registre utilise des bandes de 8 Mio et un plafond de 1 Tio. `SparseImageWriter` produit le conteneur monofichier distinct `sparseimage`, limité à un en-tête et 1 008 bandes : 7,875 Gio pour ses bandes de 8 Mio par défaut. Ces deux constructeurs permettent des bandes de 1 à 128 Mio par puissances de deux. Ils reçoivent le contenu logique préparé par les formateurs communs.

`VhdDifferencingImageWriter` et `VhdxDifferencingImageWriter` construisent un enfant à partir d’un parent autonome fourni sur flux. `WriteChain` accepte aussi jusqu’à 64 ancêtres, du parent immédiat à la base autonome. Les identifiants de parenté, capacités et tailles de secteurs doivent concorder. Tous les ancêtres sont ouverts en lecture seule ; les écritures de l’enfant sont préparées en mémoire avant publication sur le flux de destination. Ces API demandent explicitement les références du parent immédiat, suivies par le contrôle de suppression. Leur sélection reste à exposer dans les futurs profils concernés ; elles ne sont pas présentées comme des images autonomes.

## Retrait et suppression

« Retirer le support » efface l’association de la configuration. La suppression du fichier est une action distincte.

Avant toute confirmation de suppression, rechercher les références dans les configurations enregistrées, les brouillons, la configuration éditée et les sessions ouvertes. Toute référence bloque la suppression et identifie ses utilisateurs. Une configuration illisible bloque également l’opération, puisque l’absence de référence ne peut pas être établie.

L’implémentation actuelle refuse les liens symboliques, les jonctions et les fichiers ayant plusieurs liens physiques. Elle conserve un accès exclusif pendant la confirmation et supprime le fichier ouvert. La création utilise un fichier temporaire puis une publication sans écrasement.

Le contrôle de suppression utilise aussi `DiskImageDependencyIndex`. Il suit les parents VHD/VHDX, les extents et parents VMDK, ainsi que les références backing explicites QCOW/QED. Les chemins sont résolus par rapport au descripteur qui les contient ; les alias Windows sont comparés via leur chemin final, tout en conservant le répertoire d’ouverture pour les références relatives. Ce contrôle est reconstruit avant puis après la confirmation. La lecture des dépendances s’effectue hors du thread de l’interface.

Un cycle, une métadonnée illisible ou une dépendance non résolue bloque le contrôle. Les parents CHD identifiés par hash, les VDI différentiels identifiés par UUID et les UDIF segmentés exigent encore un inventaire adapté ; ils ne sont pas assimilés à des images autonomes. L’opération de suppression reste celle d’un fichier sélectionné : la suppression collective d’un ensemble devra présenter tous ses membres et contrôler leurs utilisateurs avant d’être ajoutée au dialogue.

La résolution des alias utilise les API Windows documentées [GetFinalPathNameByHandle](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getfinalpathnamebyhandlew) et [CreateFile avec accès aux métadonnées](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew). Les tests simulent les identités de chemins ; ils ne créent pas de liens ni de fichiers images réels.

La validation d’un support RAW refuse les signatures de conteneurs reconnues, même si l’extension a été renommée. Pour gzip, le contrôle porte aussi sur les premiers et derniers octets du contenu décompressé, avec vérification de la capacité décompressée. Une absence de signature reconnue ne prouve pas la validité de tous les secteurs ; les formats sans marqueur univoque restent soumis aux règles de leur consommateur.

## Validation d’un nouveau format

Documenter la spécification et sa version, implémenter le constructeur dans sa couche, puis tester sur des flux simulés : réouverture indépendante lorsque possible, structures et sommes de contrôle, allocation, limites, erreurs et conservation des données. Les adaptateurs déclarent ensuite explicitement leur compatibilité.

Les références techniques et l’état confirmé figurent dans [le catalogue de formats](hard-disk-format-catalog.md).
Les capacités encore ouvertes sont suivies dans [la feuille HDD](../tasks/hard-disk-images.md).

Après ce chantier, les [tâches d’exploration et de visualisation des HDD et supports optiques](../tasks/media-exploration.md) prévoient la réutilisation des interfaces existantes, y compris la représentation des couches optiques lorsque les images en conservent les informations.
