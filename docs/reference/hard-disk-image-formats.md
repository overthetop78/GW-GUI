# Formats d’images de disques durs

Ce document fixe le périmètre des Readers et Writers HDD de MediaEngine. Un conteneur décrit le
stockage de blocs logiques ; les tables de partitions et systèmes de fichiers sont détectés ensuite
par l’Explorateur. Une extension de fichier aide à choisir des candidats, mais ne prouve jamais à
elle seule le format.

## Règles communes

- Le document produit utilise `MediaKind.HardDisk` et une représentation `Blocks` adressée avec
  des positions 64 bits.
- La taille logique, la taille de secteur et les plages présentes, nulles ou non allouées viennent
  du conteneur. Une géométrie CHS n’est exposée que si le format ou ses métadonnées la fournit.
- Une image dépendante conserve l’identité et le chemin de ses parents. Un Reader refuse les
  cycles, les parents absents et les caractéristiques incompatibles.
- Un Writer publie le résultat seulement après l’écriture complète de ses métadonnées et données.
- Les variantes chiffrées sont refusées tant qu’aucun contrat explicite de clé n’existe.
- RAW/IMG ne peut pas être reconnu sûrement sans contexte, signature interne de partition ou
  géométrie confirmée. Le choix explicite reste possible.

## Tableau de décision

| Format | Identification et fichiers | Adressage et allocation | Compression et parents | Lecture retenue | Écriture retenue |
|---|---|---|---|---|---|
| RAW / IMG | Aucun en-tête commun ; un seul fichier, extension non probante | Suite contiguë de blocs ; trous éventuels propres au système hôte | Aucun parent ni codec défini par le format | Oui, avec taille de secteur fournie ou 512 octets par défaut après sélection explicite ou preuve structurelle | Oui, flux brut complet et publication atomique |
| VHD | Pied de page de 512 octets portant `conectix`; en-tête dynamique pour les variantes concernées | Fixe, dynamique par BAT, ou différentiel ; secteurs logiques de 512 octets | Bitmap de blocs ; parent pour le différentiel | Fixe et dynamique d’abord ; différentiel après ajout et validation d’un résolveur de parents | Nouveau Writer fixe et dynamique ; différentiel reporté jusqu’au résolveur de parents |
| VHDX | Signature `vhdxfile` au début ; deux en-têtes et deux tables de régions dans le premier Mio | BAT, blocs de données et log ; secteurs logiques 512 ou 4096 octets | Blocs absents ou présents ; parent locator pour le différentiel | Fixe et dynamique d’abord ; différentiel après ajout et validation du journal et d’un résolveur de parents | Nouveau Writer fixe et dynamique en 512 ou 4096 ; différentiel reporté |
| VDI | Pré-en-tête texte VirtualBox puis signature binaire `0xBEDA107F`; un fichier | Table de blocs ; fixe ou dynamique ; géométrie facultative | Blocs libres, zéro ou alloués ; UUID de parent pour les variantes différentielles | Images fixes et dynamiques autonomes | Nouveau Writer fixe et dynamique autonome |
| VMDK | Descripteur texte commençant par `# Disk DescriptorFile` ou en-tête sparse `KDMV`; plusieurs extents possibles | Descripteur + extents FLAT, SPARSE ou streamOptimized ; grains et tables pour sparse | Grains non alloués ou compressés ; `parentFileNameHint` et identifiants de contenu | MonolithicFlat et monolithicSparse d’abord ; autres extents ajoutés seulement après leurs Readers | Nouveau Writer monolithicFlat et monolithicSparse d’abord ; split, streamOptimized, VMFS et parents reportés |
| QCOW2 | Signature big-endian `QFI\xFB`; versions 2 et 3 | Tables L1/L2, clusters et refcounts ; taille virtuelle 64 bits | Clusters absents, zéro ou compressés ; backing file, snapshots et fichier de données externe possibles | V2/V3 autonomes non chiffrées et non compressées d’abord | Nouveau Writer V2/V3 autonome, refcounts 16 bits, sans compression ni parent |
| CHD | Signature `MComprHD`; version et taille d’en-tête big-endian | Hunks mappés vers des unités logiques ; métadonnées HDD `GDDD` pour la géométrie | Codecs par hunk et SHA-1 ; hash de parent possible | V5 HDD autonome non compressé en premier ; codecs et parents comme variantes séparées | Nouveau Writer V5 HDD autonome non compressé, avec unités et géométrie explicites |

## Détails par format

### RAW / IMG

RAW représente directement les octets du disque logique. La capacité est la longueur logique du
fichier ; une image sparse du système hôte reste logiquement remplie de zéros dans ses trous. Les
extensions `.img`, `.raw`, `.hdd`, `.hdf` ou `.bin` sont partagées avec d’autres médias et formats.
Le registre essaie donc d’abord les Readers signés et les images de disquette dont la géométrie est
établie. Le Reader RAW HDD intervient seulement après une sélection explicite ou lorsqu’une table de
partitions ou un système de fichiers fournit une preuve cohérente.

### VHD

Le pied `conectix` fournit le type de disque, la capacité logique, la géométrie historique et un
checksum. Une image fixe place les données avant ce pied. Les images dynamiques et différentielles
emploient un en-tête dynamique, une BAT et des bitmaps de secteurs. La lecture vérifie les checksums,
les alignements, les limites et les UUID. Une image différentielle n’est ouverte qu’avec une chaîne
de parents compatible. Le premier Writer à créer couvre les images fixes et dynamiques autonomes ;
la variante différentielle attend le résolveur de parents.

### VHDX

Le premier Mio contient l’identifiant, deux en-têtes et deux copies de la table des régions. Le
Reader choisit l’en-tête valide au numéro de séquence le plus récent, vérifie ses CRC32C, traite les
régions obligatoires, la BAT, les métadonnées et le journal avant d’exposer les blocs. Les tailles de
secteurs logiques de 512 et 4096 octets sont retenues. Une région obligatoire inconnue provoque un
refus, conformément à la spécification.

### VDI

Le Reader vérifie la signature, la version, les offsets, la taille de bloc, la capacité, la table de
blocs et les UUID. Les entrées libres et zéro sont distinguées des blocs réellement stockés. Le
périmètre initial couvre les images fixes et dynamiques autonomes. Le Writer correspondant doit être
créé ; les différentiels nécessiteront d’abord un inventaire d’images par UUID.

### VMDK

Le descripteur définit la capacité en secteurs, le type de création, les extents et la géométrie
facultative. Un ensemble split est un seul document logique avec plusieurs fichiers associés. Les
Readers d’extents restent séparés du Reader de descripteur : le premier périmètre couvre FLAT et
SPARSE. streamOptimized, split, VMFS et les chaînes de parents sont reportés jusqu’à disposer de
leurs Readers et validations propres. Le premier Writer produit seulement des ensembles autonomes
monolithicFlat ou monolithicSparse.

### QCOW2

Le Reader vérifie `QFI\xFB`, la version 2 ou 3, les bits de fonctions incompatibles, la taille de
cluster, les tables L1/L2 et les refcounts avant d’exposer une plage. Le premier périmètre est
autonome, non chiffré et sans compression. Le Writer de ce profil doit être créé avec ses tables et
refcounts. Les clusters compressés, sous-clusters, snapshots, backing files et fichiers de données
externes seront ajoutés séparément afin qu’une variante inconnue ne soit jamais interprétée comme un
bloc normal.

### CHD

CHD peut contenir plusieurs familles de médias. Le Reader ne classe donc le fichier comme HDD que
si les métadonnées décrivent un disque dur. Le premier périmètre couvre le profil V5 autonome non
compressé à lire et à écrire : hunks et unités explicites, avec métadonnée `GDDD`. Les codecs,
anciennes versions, parents par SHA-1 et autres familles CHD auront leurs propres actions.

## Ordre d’implémentation

1. RAW fournit le chemin bloc le plus simple et valide la détection MBR, EBR et GPT.
2. VHD, VHDX et VDI valident les conteneurs à allocations éparses et les tailles de secteurs.
3. VMDK valide les ensembles de fichiers et les extents.
4. QCOW2 valide les tables à deux niveaux et les refcounts.
5. CHD valide les hunks, métadonnées de média et géométrie facultative.

## Références

- [QEMU — Disk Images](https://www.qemu.org/docs/master/system/images)
- [QEMU — QCOW2 Image File Format](https://www.qemu.org/docs/master/interop/qcow2.html)
- [Microsoft — Virtual Hard Disk v2 (VHDX) File Format](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-vhdx/83e061f8-f6e2-4de1-91bd-5d518a43d477)
- [Microsoft — VHD format](https://learn.microsoft.com/en-us/windows/win32/vstor/about-vhd)
- [Oracle VM VirtualBox — Disk image files](https://docs.oracle.com/en/virtualization/virtualbox/6.0/user/vdidetails.html)
- [VMware — Virtual Disk Format 5.0](https://developer.broadcom.com/sdks/vmware-virtual-disk-development-kit-vddk/latest)
- [MAME source — CHD header format](https://github.com/mamedev/mame/blob/master/src/lib/util/chd.h)
- [MAME — chdman](https://docs.mamedev.org/tools/chdman.html)
