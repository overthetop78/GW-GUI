# Plan ordonné — conversions internes et autonomie vis-à-vis de `gw.exe`

## Portée et règle d’exécution

Ce document transforme les capacités envisagées en tâches d’action ordonnées. Les groupes suivent l’ordre de dépendance technique : les Writers sectoriels précèdent les conversions de conteneurs, les encodeurs alimentent ensuite le Writer SCP, et le protocole matériel n’est abordé qu’après stabilisation des fichiers et flux.

Une tâche du dernier niveau doit produire un résultat utilisable et testé. Les paragraphes `Informations nécessaires` placés sous les tâches ne sont pas des actions supplémentaires : ils consignent les structures, fichiers locaux et références nécessaires à l’exécution.

Les références externes servent à comprendre les formats. Le code de HxC, distribué sous GPL, ne doit pas être copié dans GW GUI ; les algorithmes sont réimplémentés en C# à partir des structures documentées et validés par des tests indépendants. CiderPress2 est sous Apache-2.0. Les licences des autres références doivent être contrôlées avant toute reprise de code.

Révisions déjà étudiées localement :

```text
Greaseweazle : 26690f89967d519e0106ab9566019a026b920bb4
HxCFloppyEmulator : b1eee4cd73391ceaf2ad4ac57e28bf11c91333ba
```

Références communes :

- [Catalogue des conteneurs Greaseweazle](https://github.com/keirf/greaseweazle/tree/master/src/greaseweazle/image)
- [Commande de conversion Greaseweazle](https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/tools/convert.py)
- [Lecteurs et Writers HxC](https://github.com/jfdelnero/HxCFloppyEmulator/tree/master/libhxcfe/sources/loaders)
- [Formats de pistes HxC](https://github.com/jfdelnero/HxCFloppyEmulator/tree/master/libhxcfe/sources/tracks)
- [CiderPress2](https://github.com/fadden/CiderPress2)
- [Références déjà qualifiées dans GW GUI](../scp-decoder-references.md)
- [Couverture actuelle des codecs](../scp-decoder-coverage.md)

## 1. Conversions sectorielles internes simples

### 1.1 Amiga — ADF 880 Kio et 1,76 Mio

- [x] Implémenter et raccorder un `AmigaAdfWriter` et un `AmigaAdfConversionService` produisant les ADF DD et HD depuis une image SCP ou sectorielle, puis vérifier l’égalité de tous les secteurs après réouverture.

  Informations nécessaires :

  ```text
  Sortie DD : 80 cylindres × 2 faces × 11 secteurs × 512 = 901 120 octets.
  Sortie HD : 80 cylindres × 2 faces × 22 secteurs × 512 = 1 802 240 octets.
  Un ADF standard est une concaténation de secteurs logiques ; il ne conserve pas les protections physiques.
  Réutiliser AmigaScpSectorImageReader, AmigaDiskImageReader et SectorImage.
  Ajouter le routage dans ConversionBatchExecutor seulement après validation du Writer.
  Référence : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/adf.py
  Référence HxC : libhxcfe/sources/loaders/adf_loader/adf_writer.c
  ```

### 1.2 IBM PC — IMA et IMG, 160 Kio à 2,88 Mio

- [x] Implémenter et raccorder un Writer brut ISO CHS commun pour `.ima` et `.img`, couvrant 160, 180, 320, 360, 720, 800, 1 200, 1 440, 1 680, DMF 1 680 et 2 880 Kio, puis comparer chaque sortie au Reader et à `gw convert` sur un corpus validé.

  Informations nécessaires :

  ```text
  Les fichiers IMA/IMG bruts concatènent les secteurs selon la géométrie logique du format.
  Le Writer doit refuser une géométrie indéterminée et tout bloc absent, sauf politique explicite décidée par l’utilisateur.
  Réutiliser IsoSectorImageBuilder, IbmRawImageReader et les géométries IBM existantes.
  Ne pas déduire DMF depuis la seule longueur : conserver son profil explicite.
  Référence : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/img.py
  Référence codecs : https://github.com/keirf/greaseweazle/tree/master/src/greaseweazle/codec/ibm
  ```

### 1.3 MSX — DSK 1D, 1DD, 2D et 2DD

- [x] Raccorder le Writer brut ISO commun aux sorties MSX `.dsk`, valider les quatre géométries cataloguées et prouver que le BPB, les FAT, les répertoires et les contenus restent identiques après réouverture.

  Informations nécessaires :

  ```text
  Formats GW GUI : msx.1d, msx.1dd, msx.2d, msx.2dd.
  Le suffixe DSK ne suffit pas à identifier la géométrie : utiliser le format sélectionné et le BPB validé.
  Réutiliser MsxRawImageReader, Fat12FileSystemReader et le futur Writer brut ISO.
  ```

### 1.4 Acorn — ADFS 800 Kio

- [x] Raccorder le Writer brut sectoriel à `acorn.adfs.800`, produire un `.adf` Acorn strictement dimensionné et valider carte libre, catalogue, fichiers et checksums après réouverture.

  Informations nécessaires :

  ```text
  L’extension .adf est partagée avec Amiga mais le contenu et la géométrie sont distincts.
  Le routage doit reposer sur l’identifiant acorn.adfs.800, jamais sur l’extension seule.
  Référence GW : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/acorn.py
  Référence HxC : libhxcfe/sources/loaders/acornadf_loader/acornadf_loader.c
  ```

### 1.5 BBC Micro — SSD et DSD

- [x] Implémenter et raccorder un `BbcDfsImageWriter` produisant SSD 40/80 pistes et DSD 40/80 pistes, avec ordre des faces explicite, puis comparer catalogue et contenus après réouverture.

  Informations nécessaires :

  ```text
  Formats : acorn.dfs.ss, acorn.dfs.ss80, acorn.dfs.ds, acorn.dfs.ds80.
  SSD stocke une face ; DSD stocke deux faces selon l’ordre défini par le conteneur.
  Réutiliser BbcDfsReader et BbcDfsGeometry.
  Référence GW : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/acorn.py
  ```

### 1.6 Atari ST — ST et MSA

- [x] Durcir `AtariStConversionService` pour valider ou transformer réellement la géométrie demandée au lieu d’ignorer le format cible pour une source non-SCP, puis couvrir 180, 360, 400, 440, 720, 800, 810, 880 et 1 440 Kio.

  Informations nécessaires :

  ```text
  AtariStWriter écrit déjà .st ; la branche non-SCP de ConvertAsync relit actuellement la source mais ne reconstruit pas la géométrie cible.
  Une transformation de géométrie ne doit être permise que si tous les blocs nécessaires peuvent être mappés sans perte.
  Fichiers : Conversion/Atari/AtariStConversionService.cs, Containers/Atari/St/AtariStWriter.cs.
  ```

- [x] Implémenter et raccorder un `MsaWriter` pour les images Atari compatibles, avec compression par piste et relecture comparative par `MsaReader`.

  Informations nécessaires :

  ```text
  La sortie MSA possède un en-tête big-endian, une plage de pistes, un nombre de faces et des blocs de piste compressés ou bruts.
  Référence GW : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/msa.py
  Rechercher aussi le Writer MSA dans libhxcfe/sources/loaders avant implémentation.
  ```

### 1.7 Commodore 1581 — D81

- [x] Implémenter et raccorder un `D81Writer` à partir de l’image ISO MFM reconstruite, puis vérifier les 80×2×10 secteurs, le BAM, le répertoire et les fichiers après réouverture.

  Informations nécessaires :

  ```text
  D81 est une image sectorielle de 819 200 octets ; ne pas appliquer l’ordre variable des pistes D64.
  Réutiliser D81Reader et Commodore1581FileSystemReader.
  Référence GW : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/d81.py
  Référence HxC : libhxcfe/sources/loaders/d81_loader/d81_loader.c
  ```

> Blocage d'environnement : le push du groupe 1 vers `origin/main` a été refusé par la politique d'autorisation. Les commits 1.1 à 1.7 sont conservés localement et prêts à être poussés.

## 2. Conversions sectorielles internes nécessitant un Writer spécialisé

### 2.1 Atari 8-bit — ATR 90, 130 et 180 Kio

- [x] Implémenter et raccorder un `AtrWriter` complet produisant l’en-tête ATR et les secteurs de tailles attendues pour les trois formats, puis remplacer l’usage limité d’`AtrPayloadWriter` par la façade adaptée.

  Informations nécessaires :

  ```text
  ATR contient un en-tête de 16 octets et une taille exprimée en paragraphes de 16 octets.
  Les trois premiers secteurs peuvent faire 128 octets alors que les suivants en font 256 en double densité.
  Fichiers existants : Containers/Atari/Atr/AtrReader.cs, Conversion/Atari/AtrPayloadWriter.cs.
  Référence HxC : libhxcfe/sources/loaders/atr_loader/atr_format.h et atr_loader.c
  ```

### 2.2 Commodore 1541 et 1571 — D64 et D71

- [x] Implémenter et raccorder un Writer commun Commodore DOS produisant D64 et D71 selon les zones de pistes, avec prise en charge séparée de la table facultative d’erreurs, puis valider BAM, chaînes de répertoire et fichiers.

  Informations nécessaires :

  ```text
  D64 utilise un nombre de secteurs variable par piste ; D71 ajoute la seconde face et un BAM étendu.
  Ne pas confondre l’ordre logique du conteneur avec les identifiants physiques GCR.
  Références GW :
  https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/d64.py
  https://github.com/keirf/greaseweazle/tree/master/src/greaseweazle/codec/commodore
  Référence HxC : libhxcfe/sources/loaders/d64_loader/d64_loader.c
  ```

### 2.3 Apple II et Apple III — D13, DO, DSK, PO et 2MG

- [x] Implémenter et raccorder les Writers sectoriels Apple D13, DOS-order et ProDOS-order en centralisant les tables d’interleave, puis valider DOS 3.2, DOS 3.3, ProDOS 140/800 Kio et SOS.

  Informations nécessaires :

  ```text
  D13 : 13 secteurs de 256 octets par piste.
  DO/DSK : ordre DOS ; PO : ordre ProDOS ; l’extension .dsk reste ambiguë.
  Réutiliser AppleIISectorImageBuilder, AppleDiskImageReader et les catalogues d’ordre existants.
  Référence GW : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/apple2.py
  Référence HxC : libhxcfe/sources/loaders/apple2_do_loader/apple2_do_writer.c
  Référence de validation : https://github.com/fadden/CiderPress2
  ```

- [x] Implémenter et raccorder un `TwoImgWriter` enveloppant une image Apple validée dans un conteneur 2MG, avec offsets, longueur, type d’image et drapeaux contrôlés à la réouverture.

  Informations nécessaires :

  ```text
  Le Writer doit produire l’en-tête 2IMG avant les données et ne pas déduire l’ordre DOS/ProDOS depuis l’extension seule.
  Référence HxC : libhxcfe/sources/loaders/apple2_2mg_loader/apple2_2mg_format.h
  Référence CiderPress2 : https://github.com/fadden/CiderPress2
  ```

- [x] Corriger le raccordement d’Apple II Brøderbund RWTS18 afin que le format technique détecté appelle réellement `AppleRwts18ConversionService` pour les sorties NIB et WOZ, puis tester le chemin complet depuis l’interface.

  Informations nécessaires :

  ```text
  Le service, AppleDiskImageWriter, NibWriter, WozWriter et AppleRwts18TrackEncodingService existent déjà.
  DiskClassificationCatalog ramène actuellement apple2.rwts18 vers apple2.appledos.140, alors que ConversionBatchExecutor.IsInternal exige apple2.rwts18.
  Corriger le modèle de sélection sans inventer un faux format de volume.
  ```

### 2.4 Amstrad CPC et PCW — DSK et EDSK

- [x] Implémenter et raccorder un Writer DSK/EDSK commun conservant les descripteurs de pistes, tailles sectorielles, statuts et données disponibles, puis valider les variantes CPC et PCW.

  Informations nécessaires :

  ```text
  DSK standard impose une taille uniforme ; EDSK permet des tailles de pistes et secteurs plus variées.
  Réutiliser CpcDskReader et ses modèles plutôt que reconstruire les structures dans l’interface.
  Références GW :
  https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/dsk.py
  https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/edsk.py
  Référence HxC : libhxcfe/sources/loaders/cpcdsk_loader/cpcdsk_writer.c
  ```

### 2.5 Epson QX-10 — IMG et IMD

- [x] Raccorder les formats Epson 320, 396, 399, 400 Kio et Logo au Writer brut lorsque la géométrie est uniforme, puis valider l’ordre et la capacité de chaque variante.

  Informations nécessaires :

  ```text
  Réutiliser EpsonQx10GeometryCatalog et EpsonQx10SectorImageBuilder.
  Ne pas aplatir une variante contenant des tailles sectorielles incompatibles sans diagnostic explicite.
  ```

- [x] Implémenter et raccorder un `ImdWriter` conservant mode FM/MFM, cylindre, tête, cartes de secteurs, tailles, données absentes et états, puis vérifier l’aller-retour avec `ImdReader`.

  Informations nécessaires :

  ```text
  IMD est auto-descriptif et stocke les pistes sous forme de records, avec plusieurs types de données/erreurs/compression.
  Référence GW : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/imd.py
  ```

### 2.6 DEC RX02 — IMG

- [x] Implémenter et raccorder un Writer IMG RX02 appliquant la géométrie et l’ordre logique DEC, puis comparer chaque secteur avec `Rx02Reader` et une reconstruction SCP.

  Informations nécessaires :

  ```text
  Réutiliser DecRx02ScpSectorImageReader, DecRx02TrackEncoder et les définitions RX02.
  Le format physique combine FM et M²FM ; le fichier IMG ne conserve que la charge utile ordonnée.
  Référence GW codec : https://github.com/keirf/greaseweazle/tree/master/src/greaseweazle/codec/dec
  ```

### 2.7 UCSD p-System — IMG

- [x] Raccorder l’image UCSD IBM MFM au Writer brut avec géométrie explicite et valider catalogue, segments et fichiers après réouverture.

  Informations nécessaires :

  ```text
  Réutiliser UcsdFileSystemReader et UcsdIbmMfmGeometry.
  La sortie TD0 appartient au groupe complexe et ne doit pas être simulée par un IMG renommé.
  ```

### 2.8 Commodore 900 COHERENT — BIN et IMG

- [x] Implémenter et raccorder un Writer sectoriel Commodore 900 produisant BIN/IMG selon l’ordre documenté, puis valider superbloc, inodes, répertoires et contenu des fichiers.

  Informations nécessaires :

  ```text
  Réutiliser Commodore900GcrDecoder/TrackEncoder et le Reader COHERENT.
  Définir un seul ordre logique central avant d’écrire les deux extensions.
  Référence HxC : libhxcfe/sources/tracks/track_formats/commodore900_gcr_track.c
  ```

> Blocage d'environnement : le push du groupe 2 vers `origin/main` a été refusé par la politique d'autorisation, qui ne considère pas encore cette destination comme approuvée. Les commits 2.1 à 2.8 sont conservés localement et prêts à être poussés.

## 3. Conversions internes complexes

### 3.1 Macintosh — IMG, IMAGE et DC42

- [x] Implémenter un Writer brut Macintosh pour MFM 1,44 Mio et GCR 400/800 Kio en respectant la géométrie zonée, puis valider MFS/HFS et chaque secteur après réouverture.

  Informations nécessaires :

  ```text
  Le 1,44 Mio MFM est uniforme ; les formats GCR 400/800 Kio utilisent des zones avec nombres de secteurs variables.
  Réutiliser AppleMacScpSectorReconstructor et AppleMacGcrTrackEncoder.
  Référence HxC : libhxcfe/sources/tracks/track_formats/apple_mac_gcr_track.c
  Référence de validation : https://github.com/fadden/CiderPress2
  ```

- [x] Implémenter un Writer DiskCopy 4.2 commun à `.image` et `.dc42`, incluant nom, tailles des données et tags, checksums et encodage, puis vérifier les forks et métadonnées après réouverture.

  Informations nécessaires :

  ```text
  Ne pas supprimer les tags de 12 octets quand le format source en fournit.
  Réutiliser les modèles DiskCopy existants dans Containers/Apple.
  Référence de validation : https://github.com/fadden/CiderPress2
  ```

### 3.2 Apple Lisa — IMAGE et DC42

- [x] Étendre le Writer DiskCopy aux images Lisa Office et MacWorks en conservant pages, tags et checksums, puis valider MDDF, catalogue et fichiers.

  Informations nécessaires :

  ```text
  Les tags Lisa font partie de l’adressage logique ; une sortie contenant uniquement 512 octets de données par bloc serait incomplète.
  Réutiliser LisaPageTagReader, LisaMddfReader et AppleLisaFileWareGcrTrackEncoder.
  Référence LisaFS : https://lisa.sunder.net/lisafsh/index.html
  Référence CiderPress2 : https://github.com/fadden/CiderPress2
  ```

### 3.3 HFE

- [x] Implémenter et raccorder un Writer HFE capable de sérialiser les pistes encodées avec leur bitrate, leurs faces et leur table d’offsets, puis valider le flux produit par relecture et comparaison des secteurs.

  Informations nécessaires :

  ```text
  HFE représente des pistes encodées et non une simple suite de secteurs.
  Commencer par les pistes uniformes FM/MFM ; ajouter GCR seulement après validation du premier chemin.
  Référence GW : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/hfe.py
  Références HxC : rechercher hfe_format.h et hfe_writer.c sous libhxcfe/sources/loaders.
  ```

### 3.4 TeleDisk TD0

- [x] Implémenter et raccorder un `Td0Writer` non compressé conservant commentaires, pistes, cartes de secteurs, tailles et états, puis ajouter la compression seulement après validation de l’aller-retour non compressé.

  Informations nécessaires :

  ```text
  Td0Reader existe déjà. Séparer clairement sérialisation des records et compression avancée.
  Référence GW : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/td0.py
  Réalisé : le modèle détaillé et le Writer `TD` conservent les métadonnées réinscriptibles ; l'encodage sectoriel compact par mot répété n'a été activé qu'après validation de l'aller-retour brut. La compression globale avancée `td` reste une couche distincte et n'est pas émise.
  ```

### 3.5 Formats protégés Apple, Commodore et autres

- [x] Définir et implémenter un contrat d’image de piste préservant marques, gaps, erreurs intentionnelles, timing et zones faibles avant de raccorder une sortie protégée à NIB, WOZ, HFE ou SCP.

  Informations nécessaires :

  ```text
  SectorImage ne suffit pas à représenter une protection qui dépend du timing ou d’une erreur volontaire.
  Réutiliser FluxStructure, TrackEncoder, les pistes brutes du SCP et les modèles de visualisation sans modifier les révolutions originales.
  Une sortie sectorielle standard ne doit jamais être annoncée comme préservant ces informations.
  Réalisé : `ProtectedTrackImage` conserve les cellules, segments de timing, marques, gaps, erreurs intentionnelles, zones faibles et révolutions de flux. Les adaptateurs de pistes encodées et SCP partagent les structures existantes sans modifier les révolutions ; le raccordement flux→flux aux Writers reste dans la tâche 10.2.
  ```

> Blocage d'environnement : le push du groupe 3 vers `origin/main` a été interrompu pendant la demande d'autorisation. Les commits 3.1 à 3.5 sont conservés localement et prêts à être poussés.

## 4. Conversions entre conteneurs compatibles

### 4.1 IMA ↔ IMG

- [x] Raccorder une conversion interne IBM IMA/IMG validant format, longueur et géométrie avant copie sectorielle, puis vérifier que les deux extensions sont relues avec la même identité de blocs.

  Informations nécessaires :

  ```text
  Ces conteneurs sont souvent identiques octet pour octet, mais l’extension seule ne prouve pas la géométrie.
  Utiliser le Reader et le Writer communs plutôt qu’un simple changement de nom.
  ```

### 4.2 ST ↔ MSA

- [x] Raccorder la conversion bidirectionnelle Atari ST/​MSA au service interne après disponibilité du `MsaWriter`, puis valider les géométries et l’égalité de tous les blocs.

  Informations nécessaires :

  ```text
  AtariStReader et MsaReader existent ; AtariStWriter existe ; MsaWriter est la dépendance manquante.
  ```

### 4.3 NIB ↔ WOZ

- [x] Généraliser `AppleDiskImageWriter` pour convertir NIB/WOZ des formats Apple effectivement représentables, sans le limiter implicitement à RWTS18, puis valider le flux de chaque piste.

  Informations nécessaires :

  ```text
  NibWriter et WozWriter existent. Le service actuel encode seulement Apple II RWTS18.
  Les formats Apple II standard possèdent déjà AppleIIGcrTrackEncoder.
  ```

### 4.4 DO ↔ PO ↔ 2MG

- [x] Raccorder les Writers Apple sectoriels et 2MG à un service de conversion commun appliquant explicitement l’ordre DOS ou ProDOS, puis tester les chaînes aller-retour sans se fier au suffixe `.dsk`.

  Informations nécessaires :

  ```text
  L’ordre des secteurs change ; une copie brute DO→PO est incorrecte.
  La comparaison finale doit se faire par adresse logique et contenu, pas seulement par octets du fichier.
  ```

### 4.5 DSK ↔ EDSK

- [x] Raccorder la conversion Amstrad DSK/EDSK en refusant toute réduction EDSK→DSK qui perdrait tailles, statuts ou géométries non représentables.

  Informations nécessaires :

  ```text
  Une conversion vers EDSK peut conserver davantage d’informations ; l’inverse exige une validation de représentabilité.
  ```

### 4.6 IMAGE ↔ DC42

- [x] Raccorder la conversion DiskCopy après disponibilité du Writer commun et comparer données, tags, nom, encodage et checksums.

  Informations nécessaires :

  ```text
  `.image` et `.dc42` peuvent désigner la même famille DiskCopy, mais `.image` est aussi utilisé de façon ambiguë ailleurs.
  La reconnaissance de signature reste obligatoire.
  ```

Blocage d'environnement : le push du groupe 4 vers `origin/main` a été refusé par la politique de sécurité, la destination distante n'étant pas reconnue comme approuvée. Les commits 4.1 à 4.6 restent disponibles localement.

## 5. Création interne de fichiers SCP

### 5.1 Socle commun SCP

- [x] Implémenter et raccorder `ScpWriter` avec en-tête, table de 168 pistes, blocs `TRK`, descripteurs de révolutions, intervalles 16 bits avec débordements, résolution, drapeaux et checksum, puis valider l’aller-retour par `ScpReader`.

  Informations nécessaires :

  ```text
  Réutiliser ScpFormatConstants, ScpFormatAlgorithms, ScpHeader, ScpTrack et ScpRevolution.
  Le Writer doit écrire d’abord dans un fichier temporaire, finaliser table/checksum, puis remplacer la destination.
  Référence principale : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/image/scp.py
  Implémentation indépendante : https://gitlab.com/FozzTexx/pySuperCardPro
  Format public version 1.9 disponible dans le dossier doc de pySuperCardPro.
  ```

- [x] Implémenter un service commun transformant `EncodedTrack` en intervalles SCP indexés avec une révolution déterministe, puis vérifier durée, RPM, résolution et absence de dérive cumulative.

  Informations nécessaires :

  ```text
  Les 24 codecs sectoriels possèdent déjà un TrackEncoder correspondant.
  Une image sectorielle ne permet de générer qu’une piste synthétique ; elle ne recrée pas les révolutions originales perdues.
  Ne pas inventer trois révolutions identiques : une révolution synthétique explicitement signalée suffit.
  ```

### 5.2 Amiga, Atari ST/8-bit, IBM, MSX, Acorn/BBC, Amstrad et Epson

- [x] Raccorder les familles Amiga MFM et ISO FM/MFM au service de création SCP en appliquant géométrie, bitrate, RPM et ordre sectoriel de chaque format, puis valider SCP→secteurs contre l’image source.

  Informations nécessaires :

  ```text
  Encodeurs disponibles : AmigaMfmTrackEncoder, IsoMfmTrackEncoder, IsoFmTrackEncoder.
  Sous-formats concernés : Amiga DD/HD ; Atari ST/8-bit ; IBM ; MSX ; Acorn/BBC ; Amstrad ; Epson.
  Les profils de format doivent fournir gaps et marques ; ne pas les disperser dans ScpWriter.
  ```

### 5.3 Apple II/III, Macintosh et Lisa

- [x] Raccorder Apple II GCR, RWTS18, Macintosh GCR et Lisa FileWare au service SCP après validation des Writers sectoriels correspondants, puis comparer chaque piste redécodée.

  Informations nécessaires :

  ```text
  Encodeurs disponibles : AppleIIGcrTrackEncoder, AppleRwts18TrackEncoder, AppleMacGcrTrackEncoder, AppleLisaFileWareGcrTrackEncoder.
  Les zones de vitesse Macintosh et les tags Lisa doivent provenir du modèle source, pas de valeurs globales.
  ```

### 5.4 Commodore 64/128 et Commodore 900

- [x] Raccorder Commodore GCR et Commodore 900 GCR au service SCP avec zones de débit et géométries propres, puis vérifier l’identité après redécodage.

  Informations nécessaires :

  ```text
  Encodeurs disponibles : CommodoreGcrTrackEncoder et Commodore900GcrTrackEncoder.
  Les pistes D64 ont des nombres de secteurs variables selon la zone.
  ```

### 5.5 DEC RX02 et formats rares déjà encodés

- [x] Raccorder DEC RX02, HP MMFM, Data General, Micropolis, Membrain, AED, QD MO5, Centurion, NorthStar, Heathkit, Micral N, E-mu, TYCOM, Arburg et Victor 9000 au service SCP, format par format, avec un test aller-retour et un corpus physique lorsqu’il existe.

  Informations nécessaires :

  ```text
  Les TrackEncoder correspondants existent déjà sous Encoding/Encoders.
  Utiliser docs/scp-decoder-references.md pour les sources précises HxC/Greaseweazle.
  Un test synthétique prouve l’algorithme, pas la compatibilité physique : conserver cette distinction dans les résultats.
  ```

  Validation : les quinze formats passent par le service SCP commun et sont redécodés avec leurs données intactes. Aucun corpus physique de ces formats rares n'est distribué dans le dépôt ; ces tests valident donc la chaîne synthétique sans prétendre valider chaque support réel.

### 5.6 Interface de conversion SCP

- [x] Ajouter la sortie SCP permanente au catalogue et à l’onglet Conversion, afficher qu’il s’agit d’un flux reconstruit quand la source est sectorielle, et router vers MediaEngine sans exiger `gw.exe`.

  Informations nécessaires :

  ```text
  ConversionFormatPresenter exclut actuellement raw.scp.
  MainWindow crée seulement des SCP temporaires via gw.exe pour la visualisation et l’écriture.
  Le texte visible doit être localisé dans toutes les langues distribuées.
  ```

## 6. Écriture physique sans `gw.exe`

### 6.1 Transport et protocole Greaseweazle

- [x] Implémenter dans Infrastructure un client de protocole Greaseweazle couvrant ouverture série, négociation de version, sélection du lecteur, moteur, seek, écriture de flux, index, terminaison, annulation et fermeture sûre, puis le valider sur un faux transport déterministe.

  Informations nécessaires :

  ```text
  Le protocole matériel n’appartient pas à MediaEngine : placer transport et périphérique dans GWGUI.Infrastructure.
  Référence de protocole : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/usb.py
  Référence du parcours : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/tools/write.py
  Ne copier aucun code sans contrôle de licence ; reproduire les commandes et états documentés.
  ```

### 6.2 Pipeline d’écriture

- [x] Implémenter un `PhysicalDiskWriteService` consommant directement les pistes de flux SCP ou celles produites par les TrackEncoder, avec progression par piste, arrêt, précompensation, vérification optionnelle et remontée structurée des erreurs.

  Informations nécessaires :

  ```text
  Formats visés : tous les formats possédant un TrackEncoder et les flux SCP/HFE lisibles.
  Conserver `WriteCommandBuilder` comme solution de repli tant que la parité fonctionnelle n’est pas atteinte.
  L’écriture physique ne doit jamais être appelée Writer de fichier dans l’architecture.
  ```

### 6.3 Raccordement et validation matérielle

- [ ] Raccorder l’onglet Écriture au service interne derrière une option explicite, puis valider sur disquettes de test Amiga, Atari ST, IBM, MSX, Apple, Commodore, Acorn/BBC, Amstrad, Epson et DEC avant de retirer le repli `gw.exe` pour une famille.

  État : le raccordement logiciel est disponible derrière l’option explicite « Écriture physique interne (expérimental) ». Le chemin `gw.exe` reste le comportement par défaut et aucune famille n’en a été retirée. La validation matérielle destructive est bloquée tant qu’un corpus de disquettes sacrifiables couvrant les dix familles et une autorisation explicite d’écriture physique ne sont pas disponibles ; aucun résultat matériel n’est donc affirmé.

  Informations nécessaires :

  ```text
  Chaque famille n’abandonne gw.exe qu’après écriture, relecture indépendante et comparaison complète.
  Les formats GCR dépendent aussi des capacités du lecteur physique PC utilisé.
  TG43, densité, faux index, flippy, précompensation et sélection de lecteur doivent rester disponibles.
  ```

## 7. Lecture physique sans `gw.exe`

### 7.1 Acquisition de flux

- [x] Étendre le client Greaseweazle avec lecture de flux, index, révolutions, sélection de pistes, faux index, secteurs matériels, temporisations, annulation et reprise d’erreur, puis produire directement les modèles `ScpImage` en mémoire.

  Informations nécessaires :

  ```text
  Référence de protocole : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/usb.py
  Référence du parcours : https://github.com/keirf/greaseweazle/blob/master/src/greaseweazle/tools/read.py
  Les révolutions acquises restent brutes et séparées ; l’enrichissement sectoriel intervient après acquisition.
  Réalisé : le client décode les flux et index bruts du protocole, reprend les débordements transitoires et conserve le flux source. Le service d’acquisition sélectionne les pistes physiques, gère les index réels, simulés ou à secteurs matériels, puis construit en mémoire des révolutions SCP capturées et séparées. Les tests utilisent un périphérique et un transport déterministes ; aucune lecture physique n’est revendiquée avant la validation du titre 7.3.
  ```

### 7.2 Sauvegarde et décodage en direct

- [x] Implémenter un `PhysicalDiskReadService` capable d’écrire un SCP par `ScpWriter`, de décoder progressivement les pistes avec MediaEngine et de présenter les mêmes diagnostics que l’ouverture d’un fichier SCP.

  Informations nécessaires :

  ```text
  Formats visés : tous les décodeurs déjà enregistrés dans FluxDecoderRegistry.
  Ne pas coupler l’acquisition au choix d’un système de fichiers : une capture brute doit rester possible.
  ```

  Réalisé : `PhysicalDiskReadService` conserve la capture brute, écrit le conteneur par `IScpWriter`, produit un diagnostic progressif pour chaque piste avec tous les codecs de `FluxDecoderRegistry`, puis confie le SCP sauvegardé au même `DiskImageExplorer` que l’ouverture d’un fichier existant. Le choix d’un système de fichiers n’intervient jamais dans l’acquisition.

### 7.3 Raccordement et validation matérielle

- [ ] Raccorder l’onglet Lecture au service interne derrière une option explicite et valider checksum SCP, nombre de révolutions, pistes, décodage, annulation et reprise avant de retirer `gw.exe`.

  Informations nécessaires :

  ```text
  Garder le chemin gw.exe comme référence comparative pendant toute la qualification.
  Une capture interne et une capture gw de la même opération doivent être comparées au niveau flux, index et secteurs décodés.
  ```

  Réalisé partiellement : l’onglet Lecture expose une option interne explicitement expérimentale, limitée aux SCP bruts. Sans cette option, `gw.exe` reste le chemin par défaut. Les révolutions, tentatives, reprises de seek, pistes, faux index, secteurs durs, annulation, sauvegarde SCP, décodage et exploration sont raccordés et couverts par des tests déterministes. Blocage : la comparaison physique capture interne/capture `gw.exe` sur le même média et la validation de reprise sur matériel réel nécessitent un lecteur configuré et des disquettes de qualification ; la tâche reste donc non cochée et `gw.exe` n’est pas retiré.

## 8. Réinterprétation entre formats FAT12 compatibles

### 8.1 Atari ST, IBM PC et MSX

- [x] Implémenter une politique de compatibilité FAT12 prouvant égalité de taille de secteur, géométrie, BPB, capacité et disposition avant d’autoriser Atari ST↔IBM PC↔MSX, puis écrire la sortie avec le Writer de la famille cible.

  Implémenté avec validation stricte des blocs, de la capacité, de la géométrie et du BPB FAT12. Les images hybrides, incomplètes ou seulement compatibles par hypothèse sont refusées ; les sorties utilisent les Writers Atari ST, IBM PC et MSX existants.

  Informations nécessaires :

  ```text
  Une réinterprétation ne convertit pas les programmes ni les conventions propres à la machine.
  Elle est sûre seulement lorsque les mêmes blocs logiques représentent le même volume FAT12.
  Réutiliser FatBpbGeometryDetector, Fat12LayoutReader et les géométries cataloguées.
  Refuser les formats hybrides, les BPB contradictoires et les géométries seulement supposées.
  ```

## 9. Migration de fichiers entre systèmes différents

### 9.1 Contrat commun de migration

- [x] Implémenter un modèle de migration représentant dossiers, fichiers, contenu, date, commentaire, attributs et pertes de métadonnées, puis un validateur empêchant toute migration silencieusement destructive.

  Le modèle commun conserve contenu, hiérarchie et métadonnées source. Le validateur bloque les données absentes, types non représentables, conflits de noms, caractères interdits et dépassements de taille ; toute perte de date, commentaire ou attribut requiert une acceptation explicite.

  Informations nécessaires :

  ```text
  Une migration crée un nouveau système de fichiers ; elle ne copie pas les secteurs de la source.
  Réutiliser FileSystemVolume et FileSystemEntry pour la lecture, mais définir séparément les capacités d’écriture.
  Les conflits de noms, caractères interdits, tailles maximales et attributs non représentables doivent être présentés avant exécution.
  ```

### 9.2 FAT12 ↔ AmigaDOS

- [x] Implémenter les primitives d’écriture AmigaDOS OFS/FFS nécessaires à la création de volume, répertoires, fichiers, bitmap et checksums, puis raccorder une migration FAT12↔AmigaDOS avec rapport de pertes.

  Les constructeurs de volumes AmigaDOS OFS/FFS et FAT12 créent leurs structures propres avant passage aux Writers de conteneurs. Les migrations dans les deux sens sont relues intégralement ; les noms incompatibles, contenus absents et pertes de métadonnées sont refusés ou exigent une acceptation explicite.

  Informations nécessaires :

  ```text
  Le Reader AmigaDOS existe mais aucun Writer de système de fichiers AmigaDOS n’existe.
  Références HxC/ADFlib : adf_blk.h, adf_raw.c, adf_dir.c, adf_file.c, adf_bitm.c dans la révision déjà qualifiée.
  Le Writer ADF sectoriel du groupe 1 est une dépendance mais ne crée pas à lui seul un volume.
  ```

### 9.3 Apple DOS/ProDOS/SOS

- [x] Implémenter les Writers de systèmes de fichiers Apple nécessaires à la création de volumes et raccorder les migrations depuis/vers le modèle commun, puis valider avec CiderPress2.

  Les créateurs Apple DOS 3.2/3.3 reconstruisent VTOC, bitmap, catalogue, listes T/S et fichiers binaires avec leur longueur logique. Le créateur ProDOS gère répertoires, fichiers seedling/sapling/tree et bitmap pour 140 et 800 Kio ; SOS conserve un profil et un amorçage distincts sur la structure de fichiers ProDOS/SOS commune. Les migrations valident noms, hiérarchie, tailles et pertes de métadonnées avant d'utiliser les Writers de conteneurs Apple existants.

  Validation : relecture interne des volumes et conteneurs D13, DO, PO et 2MG, puis `catalog` et `test` avec CiderPress2 v2.0.0-dev3. DOS 3.2, DOS 3.3, ProDOS et SOS ont tous été reconnus ; les quatre tests CiderPress2 ont parcouru leurs fichiers sans avertissement ni erreur.

  Informations nécessaires :

  ```text
  Séparer DOS 3.2, DOS 3.3, ProDOS et SOS : catalogues, allocation et noms diffèrent.
  Référence : https://github.com/fadden/CiderPress2
  Les Writers de conteneurs Apple du groupe 2 sont des dépendances distinctes.
  ```

### 9.4 Commodore DOS

- [x] Implémenter les primitives de création BAM, répertoires et chaînes de fichiers D64/D71/D81, puis raccorder les migrations depuis/vers le modèle commun avec validation sur émulateur et relecture interne.

  Le créateur de volume commun reconstruit les BAM 1541/1571/1581, les chaînes de répertoire et de données ainsi que les secteurs latéraux REL classiques. Les types PRG, SEQ, USR et REL, les drapeaux verrouillé/fermé et les splats restent dans les attributs bruts lors d'une migration Commodore→Commodore ; une politique explicite choisit le type lors d'une migration venant d'un autre système de fichiers.

  Validation : relecture interne D64, D71 et D81 sans avertissement, 72 tests Commodore existants sans régression, puis lecture du BAM, catalogue et extraction binaire des fichiers avec `c1541` de VICE 3.10 pour les trois conteneurs.

  Informations nécessaires :

  ```text
  Gérer les noms PETSCII, types PRG/SEQ/USR/REL, fichiers verrouillés et splats sans les réduire silencieusement à un fichier générique.
  Les Writers de conteneurs Commodore des groupes 1 et 2 sont des dépendances.
  ```

### 9.5 Interface de migration

- [x] Ajouter une opération distincte « Migration de fichiers » présentant source, destination, compatibilités et pertes, sans la mélanger avec la conversion d’image secteur-à-secteur.

  Réalisation : une fenêtre dédiée est accessible depuis l’onglet Conversion sans réutiliser son exécuteur secteur-à-secteur. Elle analyse le catalogue source, propose uniquement les systèmes de fichiers reconstructibles, affiche les incompatibilités bloquantes et les pertes de métadonnées, exige leur acceptation explicite, puis écrit une nouvelle image sans pouvoir remplacer silencieusement la source. Le service unifié accepte toute source reconnue et route les destinations FAT12, AmigaDOS, Apple DOS/ProDOS/SOS et Commodore DOS vers leurs Writers respectifs.

  Validation : migration générique Commodore DOS→IBM FAT12 relue avec le Reader FAT12, tests des services FAT12/AmigaDOS et contrôle des catalogues de localisation pour les 29 langues prises en charge.

  Informations nécessaires :

  ```text
  ST→ADF, Amiga→MSX ou Commodore→IBM appartiennent ici.
  Le terme « conversion » reste réservé aux représentations du même média logique.
  Tous les textes visibles doivent être localisés.
  ```

## 10. Préservation exacte des protections

### 10.1 Garanties de fidélité

- [x] Ajouter au modèle de conversion un niveau de fidélité déclaré — sectoriel, piste reconstruite ou flux préservé — et empêcher l’interface d’annoncer une conservation de protection lors d’une sortie sectorielle.

  Réalisation : `ConversionOutput` déclare désormais `SectorData`, `ReconstructedTracks` ou `PreservedFlux`. Les sorties ADF/ST/IMG/DSK et autres conteneurs sectoriels sont explicitement sectorielles ; les SCP/HFE issus des encodeurs actuels sont explicitement des pistes reconstruites. Seul un chemin marqué `PreservedFlux` peut annoncer la conservation de la protection. La liste des sorties sélectionnées expose cette garantie à l’utilisateur.

  Validation : tests dédiés des trois niveaux, avec refus explicite de toute promesse de conservation pour les sorties sectorielles et les pistes reconstruites.

  Informations nécessaires :

  ```text
  Sectoriel : ADF, ST, IMG, DSK simples ; données de fichiers mais pas le flux original.
  Piste reconstruite : SCP/HFE produit depuis des secteurs et TrackEncoder.
  Flux préservé : SCP/HFE produit directement depuis les révolutions d’une capture de flux.
  Référence conceptuelle : https://github.com/keirf/greaseweazle/wiki/Supported-Image-Types
  ```

### 10.2 Flux original

- [ ] Raccorder les conversions flux→flux SCP/HFE de façon à conserver les pistes et révolutions disponibles sans passage par `SectorImage`, puis vérifier index, timings et structures avant/après.

  Informations nécessaires :

  ```text
  Ne jamais enrichir ou remplacer les révolutions brutes avec le résultat sectoriel fusionné.
  Le résultat sectoriel enrichi sert à l’exploration ; le flux original sert à la préservation.
  ```

### 10.3 Validation finale et retrait progressif de `gw.exe`

- [ ] Construire une matrice automatisée de parité MediaEngine/`gw.exe` par format et opération, puis retirer la dépendance à `gw.exe` uniquement pour les lignes dont lecture, conversion, réouverture et éventuellement écriture physique sont toutes validées.

  Informations nécessaires :

  ```text
  Colonnes minimales : format, conteneur source, conteneur cible, géométrie, blocs identiques, fichiers identiques, métadonnées, flux, test physique, repli gw disponible.
  Le retrait est progressif par capacité ; aucune suppression globale de gw.exe avant parité complète des commandes matérielles et de maintenance.
  Les commandes erase, clean, seek, delays, update, pin, reset, bandwidth, rpm et align restent hors de ce plan tant qu’un remplacement explicite n’est pas décidé.
  ```
