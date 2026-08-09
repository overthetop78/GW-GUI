# Cartographie de la chaîne disque

## Couches distinctes

```text
Fichier / matériel
        ↓
Lecteur de conteneur ou Greaseweazle
        ↓
Flux / pistes / révolutions
        ↓
Décodeur de piste (FM, MFM, GCR, MMFM…)
        ↓
Candidats secteurs + structures + anomalies
        ↓
Reconstruction d’image sectorielle et géométrie
        ↓
Lecteur de système de fichiers
        ↓
Volume, dossiers, fichiers, attributs et avertissements
        ↓
Projection Explorateur / Visualisateur / Conversion
```

Ces couches ne sont pas interchangeables. Un décodeur de flux ne lit pas un répertoire ; un lecteur de système de fichiers ne sait pas reconstruire des timings SCP ; un encodeur de piste ne crée pas à lui seul un conteneur de sortie.

## Entrées et lecteurs de conteneurs

| Famille de conteneur | Lecteur actuel | Sortie principale |
|---|---|---|
| SCP | `ScpImage` | pistes, révolutions et timings de flux |
| ADF | `AdfImageReader` | image sectorielle Amiga |
| ST/IMG Atari | `AtariStImageReader` | image sectorielle FAT12 Atari |
| MSA | `MsaImageReader` | image sectorielle Atari décompressée |
| ATR | `AtrImageReader` | secteurs Atari 8 bits, y compris tailles mixtes |
| DSK/EDSK | `AmstradDskImageReader` | secteurs Amstrad/CPM |
| SSD/DSD | `BbcDfsImageReader` | secteurs BBC DFS |
| D64/D71/D81 | lecteurs Commodore dédiés | secteurs Commodore |
| Apple DO/DSK/PO/2MG/NIB/WOZ/DiskCopy | `AppleDiskImageReader` et `AppleNibbleImageDecoder` | secteurs ou pistes nibble Apple |
| DEC RX02 | `DecRx02ImageReader` | secteurs DEC |
| CP2 | `Cp2ImageReader` | secteurs |
| 86F | `I86fImageReader` | pistes/secteurs selon le conteneur |
| IMD | `ImdImageReader` | pistes et secteurs |
| TD0 | `Td0ImageReader` | pistes et secteurs |
| IBM/IMA/IMG | `IbmPcImageReader` | image sectorielle selon géométrie |
| MSX | `MsxImageReader` | image sectorielle FAT/MSX |
| Coherent | `CoherentImageReader` | image sectorielle Coherent |

Le routage par extension est actuellement centralisé dans `DiskImageExplorer`. Une extension ne suffit pas toujours à identifier la machine : `.img` et `.dsk` sont ambigus. La cible est un registre de lecteurs déclarant extensions, signatures, géométries et niveau de confiance.

## Décodeurs et encodeurs de pistes

`FluxDecoderRegistry` enregistre 25 décodeurs et `FluxEncoderRegistry` les encodeurs correspondants, à l’exception volontaire du flux brut qui n’est pas un codec de secteurs.

Familles couvertes : ISO FM/MFM, Amiga MFM, Apple II GCR, Apple RWTS18, Apple Macintosh GCR, Lisa FileWare, Commodore GCR/900, DEC RX02, AED 6200P, Arburg, Centurion, Data General, EMU, Heathkit, HP MMFM, Membrain, Micral N, Micropolis, Northstar, QD MO5, TYCOM et Victor 9K.

Chaque codec est déjà dans un fichier séparé. Les registres restent toutefois des listes concrètes et les noms/identifiants doivent être fournis par un catalogue technique commun.

RWTS18 est un codec/protection Apple II, jamais une extension de fichier. Sa conversion interne passe par `AppleRwts18ConversionService` et doit produire un véritable conteneur Apple compatible choisi, pas un fichier `.rwts18`.

## Reconstruction sectorielle depuis SCP

| Reconstruction | Code actuel | Portée |
|---|---|---|
| Amiga | `AmigaScpSectorImageReader` | Amiga MFM, choix entre révolutions |
| Apple | `AppleScpSectorImageReader` | Apple II/Mac/RWTS18 selon candidat |
| ISO FM/MFM multi-machine | `AtariScpSectorImageReader` | Atari, IBM, Amstrad, BBC, Epson, UCSD ; nom incorrect |
| Commodore | `CommodoreScpSectorImageReader` | GCR Commodore |
| DEC RX02 | `DecRx02ScpSectorImageReader` | DEC RX02 |

Le point critique est la reconstruction ISO partagée. Le décodage des marques ISO peut être commun, mais les géométries et interprétations machine doivent être des stratégies distinctes.

## Systèmes de fichiers

`FileSystemRegistry` enregistre : Acorn ADFS/FileCore, AmigaDOS, CP/M Amstrad et générique, Apple DOS, Inform/Xzip, Atari DOS, BBC DFS, Coherent, Commodore DOS, FAT12, Lisa, Macintosh MFS/HFS, ProDOS, RT-11 et UCSD.

La détection doit pouvoir retourner plusieurs documents valides pour une image multiformat. `ReadAll` est le chemin adapté à cette conservation. `TryRead`, qui s’arrête au premier lecteur compatible, ne doit être utilisé que lorsque le contexte garantit une interprétation unique ou qu’une préférence explicite est demandée sans supprimer les autres résultats disponibles.

## Détection

La classification complète contient des dimensions différentes :

- conteneur : SCP, ADF, ST, MSA, ATR, etc. ;
- machine/famille : Amiga, Atari ST, Atari 8 bits, Apple II, IBM PC… ;
- format/géométrie : capacité, pistes, faces, secteurs ;
- codec : ISO MFM, Amiga MFM, GCR… ;
- système de fichiers : FAT12, AmigaDOS, ProDOS… ;
- protection : RWTS18 et futures protections documentées ;
- interprétations multiples : plusieurs systèmes dans une même capture.

`DiskClassificationCatalog` doit devenir la source commune de ces relations. Les lecteurs techniques déclarent leurs capacités par identifiants, sans recopier les noms visibles.

### Détection automatique

Pour une image sectorielle ciblée, signatures, en-tête et taille peuvent permettre une route courte. Pour une capture SCP/flux :

1. lire le conteneur une seule fois ;
2. produire les candidats codecs et géométries plausibles ;
3. décoder les pistes utiles avec annulation ;
4. évaluer toutes les interprétations valides ;
5. conserver les résultats multiformats ;
6. sélectionner la meilleure interprétation visible sans jeter les autres.

Si aucun résultat n’est reconnu et que l’auto-détection est cochée, les sélecteurs Machine/Format doivent revenir à vide ou `Aucun`, conformément à la décision existante.

### Choix manuel

Le choix manuel fournit une classification préférée et permet de retenter une image non reconnue. Pour un conteneur ambigu ou un SCP, il ne signifie pas automatiquement « ne jamais examiner les autres systèmes ». La stratégie exacte de priorité et l’affichage des résultats multiformats devront être validés avant implémentation.

## Parcours de l’interface

### Lecture

```text
ReadOperationViewModel / MainWindow
→ ReadCommandBuilder
→ HardwareRoutingPolicy
→ GreaseweazleRunner
→ GwProgressTracker + Terminal + journal read.log
→ fichier produit
→ résumé / ouverture Visualisateur ou Explorateur
```

Une nouvelle commande doit remettre les blocs de pistes à l’état non lu. Une annulation doit supprimer le fichier partiel.

### Écriture

```text
fichier source
→ ImageFormatDetector + classification choisie
→ WriteCommandBuilder
→ HardwareRoutingPolicy
→ GreaseweazleRunner
→ progression / terminal / write.log
```

Le choix du lecteur n’est visible dans l’onglet que lorsqu’il existe réellement plusieurs lecteurs configurés.

### Conversion

```text
source
→ détection/classification
→ ConversionFormatPresenter + sélections multiples
→ ConversionPlanner
→ ConversionBatchExecutor
→ gw convert ou service interne explicitement pris en charge
→ sorties, conflits, tags, journaux et progression
```

Chaque sortie successive réinitialise son affichage de pistes. Les sorties compatibles dépendent du format source et du catalogue commun. RWTS18/déprotection doit être une capacité de conversion Apple II, pas une extension inventée.

### Visualisateur

```text
fichier courant partagé
→ politique de visualisation
→ SCP direct OU représentation de flux construite depuis l’image sectorielle
→ ScpDocumentLoader / FluxDecoderRegistry
→ SkiaScpRenderer
→ vues des faces, légende, aperçu de pistes et inspecteur
```

Le chargement doit être annulable, progressif et remplacé immédiatement lorsqu’un autre fichier est choisi. Le document précédent doit être effacé au début du nouveau chargement.

### Explorateur

```text
fichier courant partagé
→ DiskImageExplorer
→ lecteur de conteneur
→ reconstruction sectorielle éventuelle
→ FileSystemRegistry
→ document(s) disque
→ arbre dossiers + liste + détails + avertissements
```

Si aucun système de fichiers n’est reconnu mais que des pistes/secteurs sont décodés, l’Explorateur peut présenter la structure physique réelle. Il ne doit jamais inventer des noms de fichiers.

## Recalculs et cache cible

Les données à mutualiser sont : octets du fichier, conteneur analysé, pistes/révolutions, résultats de codecs, candidats secteurs et classification. Les projections à recalculer séparément sont : rendu bitmap, arbre de fichiers, panneaux de détails et texte localisé.

Clé de cache proposée pour la phase 2, sans décision de comportement : chemin normalisé + taille + date de modification + choix de classification. Toute ouverture d’un nouveau fichier annule le travail précédent avant de publier son résultat.

