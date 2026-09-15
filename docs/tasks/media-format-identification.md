# Identification technique des organisations de médias nommées d’après un logiciel

## Périmètre inventorié dans `src/GWGUI.MediaEngine`

Ce travail concerne les lecteurs qui attribuent à un média entier un système de fichiers ou une organisation portant le nom du logiciel qui a permis de la découvrir. Les détecteurs qui identifient seulement le type d’un fichier contenu dans un média ne sont pas à renommer pour cette seule raison.

Les noms contrôlés et conservés hors des tâches de requalification sont :

- `K-file`, qui désigne une structure technique Atari ;
- `Z-machine`, qui désigne le format technique du contenu extrait ;
- `ATN!/File Imploder`, dont le détecteur contrôle un en-tête `ATN!`, les tailles compressée et décompressée, les limites des membres et leur alignement sur les secteurs ;
- `The Company`, qui constitue une attribution de crack relevée après la mention `CRACKED BY`, et non le nom d’un format de média.

### Apple Inform/XZIP

- `src/GWGUI.MediaEngine/FileSystems/Apple/InformXzip/AppleInformXzipExceptions.cs`
- `src/GWGUI.MediaEngine/FileSystems/Apple/InformXzip/AppleInformXzipFileSystemReader.cs`
- `src/GWGUI.MediaEngine/FileSystems/Apple/InformXzip/AppleInformXzipLayout.cs`
- `src/GWGUI.MediaEngine/FileSystems/Apple/InformXzip/ZMachineV5Header.cs`
- `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemDisplayNames.cs`
- `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemIds.cs`
- `src/GWGUI.MediaEngine/FileSystems/FileSystemReaderCatalog.cs`

### Atari Print Shop

- `src/GWGUI.MediaEngine/FileSystems/Atari/PrintShop/AtariPrintShopFileSystemReader.cs`
- `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemDisplayNames.cs`
- `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemIds.cs`
- `src/GWGUI.MediaEngine/FileSystems/FileSystemReaderCatalog.cs`

### Atari Word Magic

- `src/GWGUI.MediaEngine/FileSystems/Atari/WordMagic/AtariWordMagicDictionaryFileSystemReader.cs`
- `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemDisplayNames.cs`
- `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemIds.cs`
- `src/GWGUI.MediaEngine/FileSystems/FileSystemReaderCatalog.cs`

## Travail à réaliser

- [x] Préparer l’identification technique future de l’organisation de média actuellement nommée Apple Inform/XZIP, faute de média disponible.
  - [x] Modifier `docs/tasks/media-format-identification.md` pour inscrire qu’aucun `report.json` contenant `apple-inform-xzip` ou `AppleInformXzip` n’a été trouvé parmi les 1 450 rapports disponibles.
  - [x] Créer `tests/GWGUI.LocalDiskImageTests/TemporaryMediaAuditProgram.cs` à partir du programme d’audit existant et y appeler le contrôle temporaire après l’exploration du média, sans modifier `Program.cs`.
  - [x] Créer `tests/GWGUI.LocalDiskImageTests/TemporaryNamedMediaFormatIdentification.cs` pour arrêter l’audit sur `apple-inform-xzip` en indiquant le média et les fichiers internes concernés.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/GWGUI.LocalDiskImageTests.csproj` pour employer temporairement `TemporaryMediaAuditProgram.cs` à la place de `Program.cs` pendant cette campagne.

- [x] Identifier puis requalifier techniquement l’organisation de média actuellement nommée Atari Print Shop.
  - [x] Modifier `docs/tasks/media-format-identification.md` pour inscrire les douze rapports contenant `atari-print-shop` :
    - `artifacts/media-audit/items/00000772-f8592960508afaa4/report.json`
    - `artifacts/media-audit/items/00000773-688870414f73ba8c/report.json`
    - `artifacts/media-audit/items/00000774-e7d2780344f1d870/report.json`
    - `artifacts/media-audit/items/00000775-34467d6898c6dc8c/report.json`
    - `artifacts/media-audit/items/00000776-f6d7f13bb24a4797/report.json`
    - `artifacts/media-audit/items/00000777-e53d146a6f248cfe/report.json`
    - `artifacts/media-audit/items/00000778-b31f3605abcf1cea/report.json`
    - `artifacts/media-audit/items/00000779-40182e6951130acc/report.json`
    - `artifacts/media-audit/items/00000780-b7597d78ab230bff/report.json`
    - `artifacts/media-audit/items/00000781-3db32aa9ec93131e/report.json`
    - `artifacts/media-audit/items/00000782-70253972fccbbd92/report.json`
    - `artifacts/media-audit/items/00000783-9675241d0c1076cb/report.json`
  - [x] Comparer les douze médias Atari Print Shop et consigner leur organisation commune indépendamment de leur nom externe.
    - [x] Créer `artifacts/media-audit/format-identification/atari-print-shop-layout.json` avec la géométrie, les secteurs réservés, la signature, les entrées de catalogue, les chaînes de secteurs et les propriétés communes relevées dans les douze images.
  - [x] Requalifier le lecteur avec le nom technique Atari CLK graphics-library disk démontré dans les douze images.
    - [x] Créer `src/GWGUI.MediaEngine/FileSystems/Atari/ClkGraphicsLibrary/AtariClkGraphicsLibraryFileSystemReader.cs` avec le décodage actuellement validé dans `AtariPrintShopFileSystemReader`, les identifiants techniques `atari-clk-graphics-library` et `atari-clk-graphic`, et les libellés de contenu Atari CLK.
    - [x] Modifier `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemIds.cs` pour remplacer l’identifiant Atari Print Shop par `atari-clk-graphics-library`.
    - [x] Modifier `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemDisplayNames.cs` pour remplacer le nom affiché Atari Print Shop par `Atari CLK graphics library`.
    - [x] Modifier `src/GWGUI.MediaEngine/FileSystems/FileSystemReaderCatalog.cs` pour enregistrer `AtariClkGraphicsLibraryFileSystemReader` à la place de `AtariPrintShopFileSystemReader`.
    - [x] Modifier `src/GWGUI.App/Functions/Explorer/ExplorerFileContentClassifier.cs` pour reconnaître le nouvel identifiant natif `atari-clk-graphic`.
    - [x] Supprimer `src/GWGUI.MediaEngine/FileSystems/Atari/PrintShop/AtariPrintShopFileSystemReader.cs` après compilation du nouveau lecteur et remplacement de tous ses appels.
    - [x] Supprimer `src/GWGUI.MediaEngine/FileSystems/Atari/PrintShop` lorsqu’il est vide.

- [x] Identifier puis requalifier techniquement l’organisation de média actuellement nommée Atari Word Magic.
  - [x] Modifier `docs/tasks/media-format-identification.md` pour inscrire le rapport contenant `atari-word-magic-dictionary` et les trois autres faces du même ensemble nécessaires à son identification :
    - `artifacts/media-audit/items/00001110-7873567e641b7494/report.json`
    - `artifacts/media-audit/items/00001111-34704cf9ea63f80b/report.json`
    - `artifacts/media-audit/items/00001112-76b12a957a0aa745/report.json`
    - `artifacts/media-audit/items/00001113-714738afce5c6af8/report.json`
  - [x] Déterminer la structure du dictionnaire compressé à partir du disque dictionnaire et du programme qui le lit.
    - [x] Créer `artifacts/media-audit/format-identification/atari-spell-magic-dictionary-layout.json` avec les rapports des quatre faces Word Magic, la géométrie, les zones d’index et de données, le codage des mots et les routines d’accès démontrées dans `SPELL.OBJ` ou `PROOF.OBJ`.
  - [x] Requalifier le lecteur comme dictionnaire Atari indexé par lettre et compressé par préfixe, puis exposer sa liste de mots décodée.
    - [x] Créer `src/GWGUI.MediaEngine/FileSystems/Atari/FrontCompressedDictionary/AtariFrontCompressedDictionaryFileSystemReader.cs` avec la validation de la table A–Z, du marqueur `SM`, des pointeurs secteur/position et du flux binaire, puis la reconstruction de `DICTIONARY.TXT` à partir des préfixes de 4 bits et symboles de 5 bits.
    - [x] Modifier `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemIds.cs` pour remplacer l’identifiant lié à Word Magic par `atari-front-compressed-dictionary`.
    - [x] Modifier `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemDisplayNames.cs` pour remplacer le nom affiché lié à Word Magic par `Atari front-compressed dictionary`.
    - [x] Modifier `src/GWGUI.MediaEngine/FileSystems/FileSystemReaderCatalog.cs` pour enregistrer `AtariFrontCompressedDictionaryFileSystemReader` à la place de `AtariWordMagicDictionaryFileSystemReader`.
    - [x] Modifier `src/GWGUI.App/Functions/Explorer/ExplorerFileContentClassifier.cs` pour reconnaître l’identifiant natif `atari-front-compressed-word-list` à la place de `word-magic-main-dictionary`.
    - [x] Supprimer `src/GWGUI.MediaEngine/FileSystems/Atari/WordMagic/AtariWordMagicDictionaryFileSystemReader.cs` après compilation du nouveau lecteur et remplacement de tous ses appels.
    - [x] Supprimer `src/GWGUI.MediaEngine/FileSystems/Atari/WordMagic` lorsqu’il est vide.

- [x] Préparer l’identification technique future de la compression actuellement nommée FIRE, faute de média disponible.
  - [x] Modifier `docs/tasks/media-format-identification.md` pour inscrire qu’aucun `report.json` contenant l’identifiant `compression-fire` n’a été trouvé parmi les 1 450 rapports disponibles.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/TemporaryNamedMediaFormatIdentification.cs` pour arrêter l’audit lorsqu’un média contient le marqueur actuellement associé à `compression-fire`, en indiquant le média et les fichiers internes concernés.

- [x] Identifier techniquement la règle actuellement nommée `compression-fire` à partir du média rencontré au checkpoint 1564.
  - [x] Établir si les octets `FIRE` désignent une structure de compression, un fichier contenu ou une simple chaîne sans rapport avec l'organisation du média.
    - [x] Créer `artifacts/media-audit/items/00001564-52ea73fc909f2b4d/content-analysis.json` avec toutes les positions de `FIRE`, leur rattachement au secteur de catalogue Atari DOS 364, l'analyse de `FIRE255.COM` et `FIRE255.DOC`, et la preuve que ce nom désigne une intro de 255 octets plutôt qu'une compression.
  - Résultat : les seules occurrences appartiennent aux entrées de catalogue `FIRE255.COM` et `FIRE255.DOC`. La première est un exécutable XEX de 255 octets et la seconde sa documentation ATASCII. Aucune structure de compression FIRE n'est présente dans ce média.

- [x] Supprimer la fausse détection de compression fondée sur la présence du texte `FIRE`.
  - [x] Retirer la règle de détection qui confond les noms de fichiers avec une signature de compression.
    - [x] Modifier `src/GWGUI.MediaEngine/Exploration/Metadata/DiskContentDetector.cs` pour supprimer `FireSignature` et ne plus ajouter `DiskContentIds.CompressionFire` à partir d'une recherche brute de texte.
    - [x] Modifier `src/GWGUI.MediaEngine/Exploration/Metadata/DiskContentIds.cs` pour supprimer l'identifiant `CompressionFire` qui ne correspond à aucun format démontré.
  - [x] Retirer la surveillance temporaire de la règle invalidée.
    - [x] Modifier `tests/GWGUI.LocalDiskImageTests/TemporaryNamedMediaFormatIdentification.cs` pour supprimer `FireMarker`, `ContainsFireMarker` et l'arrêt associé à `DiskContentIds.CompressionFire`.
  - [x] Consigner la requalification de la règle provisoire.
    - [x] Modifier `docs/tasks/media-format-identification.md` pour retirer `compression-fire` de l'inventaire des noms à requalifier et indiquer que le checkpoint 1564 a démontré un faux positif sur deux noms de fichiers Atari DOS.
  - [x] Vérifier la correction sur le média qui révélait le faux positif.
    - [x] Compiler le projet en Debug et écrire le résultat du contrôle dans `docs/tasks/media-format-identification.md`.
      - Résultat : le build Debug se termine sans erreur et produit `build/Debug/GW GUI/gwgui.exe`.
    - [x] Exécuter l'analyse unitaire de l'image du checkpoint 1564 et écrire dans `docs/tasks/media-format-identification.md` le système de fichiers, les fichiers reconstruits et l'absence de `compression-fire`.
      - Résultat : l'image `atari.180` est reconnue avec `atari-dos`, ses 34 entrées sont reconstruites sans erreur, `FIRE255.COM` est un exécutable natif de 255 octets, `FIRE255.DOC` un document texte de 256 octets et le rapport ne contient plus `compression-fire`.
  - [x] Supprimer les sorties temporaires du contrôle unitaire après validation.
    - [x] Supprimer `artifacts/media-audit/items/00001564-52ea73fc909f2b4d/content-analysis.json` après avoir consigné son résultat dans ce fichier de tâches.

## Nettoyage après la requalification de tous les formats

- [ ] Supprimer les outils et sorties temporaires créés pour cette identification.
  - [ ] Modifier `tests/GWGUI.LocalDiskImageTests/GWGUI.LocalDiskImageTests.csproj` pour rétablir la compilation directe de `Program.cs`.
  - [ ] Supprimer `tests/GWGUI.LocalDiskImageTests/TemporaryMediaAuditProgram.cs`.
  - [ ] Supprimer `tests/GWGUI.LocalDiskImageTests/TemporaryNamedMediaFormatIdentification.cs`.
