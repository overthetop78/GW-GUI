# Audit autonome du corpus de médias

Source par défaut : `F:\Retro`

Script : `scripts/temp/analyze-media-data.ps1`

Validateur : `tests/GWGUI.LocalDiskImageTests`

Résultats : `artifacts/media-audit`

## MediaAnalysis

- [x] Centraliser les formats d’images de médias dans la table commune.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` pour y déclarer chaque extension d’image de média avec la catégorie `DiskImage` et l’icône générique actuelle.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/AmigaMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes et sa règle `.gz` déjà fournie par la table commune.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/AtariMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/AppleMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/BbcMicroMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommodoreMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CpmMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/DecMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/IbmPcMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/MsxMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/UcsdMediaContentRecognitionTable.cs` pour retirer ses règles `DiskImage` désormais communes.

- [x] Ajouter l’héritage des règles Atari.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Enums/MediaFileSystemFamily.cs` pour ajouter le parent `Atari` et renommer `AtariSt` en `AtariTos`.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Exploration/MediaFileSystemFamilyResolver.cs` pour renvoyer `AtariTos` pour les formats ST, STE, TT et Falcon.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/AtariMediaContentRecognitionTable.cs` pour utiliser `Atari`, `Atari8Bit` et `AtariTos`, avec `.bas` au niveau Atari commun et les autres règles dans leur branche actuelle.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/MediaContentRecognitionCatalog.cs` pour rechercher dans l’ordre famille Atari précise, Atari commun, Common général.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Functions/MediaContentClassifier.cs` pour appliquer la reconnaissance exécutable TOS à `AtariTos`.
  - [x] Modifier `tests/GWGUI.Tests/Media/MediaContentRecognitionCatalogScenarios.cs` pour vérifier le renommage `AtariTos` et l’héritage Atari avec surcharge spécialisée.

- [x] Reconnaître les programmes Turbo-BASIC XL Atari 8-bit.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter la constante nommée `Tur` correspondant à l’extension `.tur`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/AtariMediaContentRecognitionTable.cs` pour classer `.tur` comme `BasicProgram` uniquement dans `Atari8Bit`.

## Contrôles appliqués à chaque image

Le validateur contrôle :

- la reconnaissance du format et de la famille du média ;
- les informations et la structure de la représentation décodée : secteurs, flux, blocs, pistes optiques ou contenu séquentiel ;
- les blocs absents, invalides, vides ou de taille incohérente ;
- les bornes, les capacités et les dépassements numériques des volumes ;
- l'espace libre, qui ne peut être négatif ni dépasser la capacité annoncée ;
- les nombres de dossiers et de fichiers, en parcourant toute l'arborescence ;
- la somme des tailles logiques et occupées des fichiers, qui ne peut dépasser la capacité du volume ;
- les métadonnées, les attributs, les dates et les diagnostics obtenus pour chaque entrée ;
- l'extraction complète du contenu de chaque fichier et sa conformité avec la taille déclarée ;
- les informations ajoutées par MediaAnalysis, dont le type et l'icône ;
- les contenus qui restent inconnus après analyse.

L'absence de volume est enregistrée comme avertissement, car elle peut être normale. Un système de fichiers reconnu sans fichier, ou une disquette contenant un volume mais aucun fichier extrait, est une erreur à examiner.

## Classement des erreurs

Cette feuille ne contient aucune erreur en attente au début de la nouvelle campagne.

Lorsqu'une erreur est rencontrée, créer un titre de niveau 2 portant le type de l'image concernée, par exemple `## ATR`, `## ATX`, `## SCP` ou `## HFE`. Toutes les tâches concernant ce type restent sous ce titre. Ne créer le titre qu'au premier arrêt rencontré pour ce type.

Sous ce titre, créer une tâche pour l'image fautive, puis des sous-tâches concrètes indiquant chaque fichier à créer, modifier, déplacer, renommer ou supprimer. Consigner le chemin du rapport et le défaut exact observé avant de corriger le code.

## ATR

- [x] Reconnaître les disquettes Atari K-file amorcées par KBoot sans inventer de fichier.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Definitions/FileSystemIds.cs` et `src/GWGUI.MediaFileSystems/Definitions/FileSystemDisplayNames.cs` pour déclarer l'identifiant `atari-k-file` et le libellé invariant `K-file (KBoot)`.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/KFile/AtariKFileFileSystemReader.cs` pour reconnaître structurellement KBoot et retourner un volume K-file vide, sans nom de fichier synthétique.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Exploration/FileSystemReaderCatalog.cs` pour enregistrer le lecteur K-file.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Contracts/FileSystemVolume.cs`, `src/GWGUI.MediaEngine/Contracts/Explorer/FileSystemVolume.cs` et `src/GWGUI.MediaEngine/Contracts/Explorer/FileSystemVolumeMapper.cs` pour transmettre le libellé du système de fichiers de MediaFileSystems à MediaEngine.
  - [x] Modifier `src/GWGUI.App/Views/Controls/Explorer/Sections/ExplorerSection.Media.cs` et `src/GWGUI.App/Presenters/Explorer/ExplorerDetailsPresenter.cs` pour afficher le libellé transmis dans le champ « Système de fichiers ».
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaAuditValidator.cs` et `tests/GWGUI.LocalDiskImageTests/MediaAuditValidatorSelfTests.cs` pour accepter un K-file reconnu sans catalogue tout en refusant les autres disquettes reconnues sans fichier.
  - [x] Ranger le code K-file selon sa nature.
    - [x] Créer `src/GWGUI.MediaFileSystems/Constants/AtariKFileConstants.cs` pour contenir les nombres et textes bruts propres à K-file et KBoot.
    - [x] Créer `src/GWGUI.MediaFileSystems/Functions/AtariKFileFunctions.cs` pour contenir la reconnaissance KBoot, la lecture de la longueur et la validation de la charge utile Atari.
    - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/KFile/AtariKFileFormatCatalog.cs` pour contenir l'ensemble des formats d'images acceptés.
    - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Atari/KFile/AtariKFileFileSystemReader.cs` pour retirer les constantes, le catalogue et les fonctions déplacés.
  - [x] Accepter les charges utiles KBoot contenant des données après leur en-tête exécutable Atari.
    - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/AtariKFileConstants.cs` pour retirer les constantes propres au parcours complet des segments XEX.
    - [x] Modifier `src/GWGUI.MediaFileSystems/Functions/AtariKFileFunctions.cs` pour vérifier le marqueur exécutable initial sans interpréter toute la charge utile comme un fichier XEX catalogué.
  - [x] Relancer `scripts/temp/analyze-media-data.ps1` sur `F:\Retro` depuis le point d'arrêt et vérifier que `artifacts/media-audit/items/00000003-a649db402509a1ed/report.json` identifie `atari-k-file` sans entrée synthétique avant de poursuivre.
  - [x] Reconnaître KBoot à partir de ses trois secteurs de démarrage sans fixer l'adresse d'initialisation du programme.
    - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/AtariKFileConstants.cs` pour retirer la constante d'adresse d'initialisation fixe.
    - [x] Modifier `src/GWGUI.MediaFileSystems/Functions/AtariKFileFunctions.cs` pour exiger la présence et la taille des trois secteurs KBoot et accepter leur adresse d'initialisation variable.
    - [x] Relancer `scripts/temp/analyze-media-data.ps1` depuis le point d'arrêt pour vérifier que l'image `850 Express! 1.0 (1986-03)(Ledbetter, Keith)(PD).atr` est reconnue comme `atari-k-file` sans entrée synthétique.

- [x] Classer les modules overlay `.OVL` dans les types communs.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter la constante nommée `Ovl` correspondant à `.ovl`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` pour classer `.OVL` comme `Library` dans la table commune.
  - [x] Relancer `scripts/temp/analyze-media-data.ps1` sur `F:\Retro` depuis le point d'arrêt et vérifier que les modules `.OVL` ne restent plus inconnus.

- [x] Classer `CONFIG.EXP` de 850 Express! comme configuration Atari 8-bit non textuelle.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter la constante nommée `Exp` correspondant à `.exp`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Functions/MediaContentRecognitionRuleFactory.cs` pour permettre à une règle de configuration non textuelle de demander l'aperçu hexadécimal sans lui attribuer un encodage de texte.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/AtariMediaContentRecognitionTable.cs` pour classer `.EXP` comme `Configuration` Atari 8-bit avec l'icône de configuration et l'aperçu hexadécimal.
  - [x] Relancer `scripts/temp/analyze-media-data.ps1` depuis le point d'arrêt pour vérifier `CONFIG.EXP`, puis poursuivre jusqu'au prochain défaut.

- [x] Identifier les disquettes Atari contenant un programme de démarrage direct sans catalogue.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Definitions/FileSystemIds.cs` et `src/GWGUI.MediaFileSystems/Definitions/FileSystemDisplayNames.cs` pour déclarer l'identifiant `atari-boot-disk` et le libellé `Atari boot disk`.
  - [x] Créer `src/GWGUI.MediaFileSystems/Constants/AtariBootDiskConstants.cs` pour contenir les offsets, limites et attributs du format de démarrage Atari.
  - [x] Créer `src/GWGUI.MediaFileSystems/Functions/AtariBootDiskFunctions.cs` pour valider l'en-tête et tous les secteurs consécutifs annoncés par le programme de démarrage.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/BootDisk/AtariBootDiskFormatCatalog.cs` et `src/GWGUI.MediaFileSystems/FileSystems/Atari/BootDisk/AtariBootDiskFileSystemReader.cs` pour exposer le média amorçable sans inventer de fichier.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Exploration/FileSystemReaderCatalog.cs` pour enregistrer le lecteur après les lecteurs Atari possédant un catalogue réel.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaAuditValidator.cs` et `tests/GWGUI.LocalDiskImageTests/MediaAuditValidatorSelfTests.cs` pour accepter `atari-boot-disk` sans fichier et conserver l'erreur pour une disquette non identifiée.

- [x] Centraliser les quatre modes de reconnaissance des fichiers dans le catalogue des types de contenu.
- [x] Refaire entièrement le catalogue de reconnaissance sous forme de tableaux explicites.
  - [x] Créer `src/GWGUI.MediaAnalysis/Contracts/MediaContentRecognitionRule.cs` avec les colonnes famille, extension, entête, fin, contenu, catégorie, encodage, exécution et aperçu, sans valeur implicite cachée dans les tables.
  - [x] Remplacer toutes les lignes des fichiers sous `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition` par des constructions explicites de `MediaContentRecognitionRule`, sans appel à `Rule`, `SignatureRule` ou `Pattern`.
  - [x] Séparer `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/AppleMediaContentRecognitionTable.cs` en tables Apple DOS, ProDOS, Macintosh et Lisa, chacune dans son fichier.
  - [x] Séparer `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/AtariMediaContentRecognitionTable.cs` en tables Atari commune, Atari TOS et Atari 8 bits, chacune dans son fichier.
  - [x] Supprimer `src/GWGUI.MediaAnalysis/Functions/MediaContentRecognitionRuleFactory.cs` et créer des fonctions distinctes pour normaliser les extensions et convertir une règle reconnue en résultat de présentation.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/MediaContentRecognitionCatalog.cs`, `src/GWGUI.MediaAnalysis/Functions/MediaContentPatternMatcher.cs` et `src/GWGUI.MediaAnalysis/Functions/MediaContentClassifier.cs` pour utiliser exclusivement les nouvelles lignes explicites.
  - [x] Modifier les contrôles dans `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` et `tests/GWGUI.Tests/Media/MediaContentRecognitionCatalogScenarios.cs` pour vérifier les tableaux explicites, leur séparation et les quatre modes de reconnaissance.

- [x] Uniformiser la représentation des lignes de reconnaissance par signature.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour exposer directement des collections d'octets en lecture seule sans conversion dans les tableaux.
  - [x] Modifier toutes les tables sous `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition` pour écrire chaque construction `new(...)` sur une seule ligne et supprimer tous les appels à `ToArray()`.

- [x] Regrouper la position, la direction et les octets dans chaque constante de signature.
  - [x] Créer `src/GWGUI.MediaAnalysis/Enums/MediaContentSearchDirection.cs` avec les directions `Start` et `End`.
  - [x] Remplacer `src/GWGUI.MediaAnalysis/Contracts/MediaContentBytePattern.cs` par `src/GWGUI.MediaAnalysis/Contracts/MediaContentSignature.cs`, contenant la position nullable, la direction et les octets.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour construire chaque signature complète avec `new(position, direction, [octets])`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Contracts/MediaContentRecognitionRule.cs` et toutes les tables sous `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition` pour ne conserver qu'une colonne de signatures référençant les constantes nommées.
  - [x] Renommer `src/GWGUI.MediaAnalysis/Functions/MediaContentPatternMatcher.cs` en `src/GWGUI.MediaAnalysis/Functions/MediaContentSignatureMatcher.cs` et interpréter position et direction dans une fonction unique de recherche.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Functions/MediaContentClassifier.cs`, `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` et `tests/GWGUI.Tests/Media/MediaContentRecognitionCatalogScenarios.cs` pour utiliser la nouvelle structure.

- [x] Reconnaître les composants binaires d'ABC A Basic Compiler.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter les signatures du runtime ABC et du chargeur de relocation MKRELO.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer les interpréteurs ABC comme bibliothèques et MKRELO comme exécutable.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour vérifier ces deux reconnaissances avec des contenus autonomes construits en mémoire.

- [x] Reconnaître les programmes Atari BASIC tokenisés et les textes ATASCII sans extension connue.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter l'en-tête enregistré d'un programme Atari BASIC tokenisé.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer cet en-tête comme `BasicProgram`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Functions/MediaContentClassifier.cs` pour reconnaître les caractères imprimables et la fin de ligne ATASCII dans les fichiers Atari 8-bit.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour vérifier un programme BASIC tokenisé et des textes ATASCII avec ou sans extension.

## Fin de la campagne

La campagne est terminée lorsque le script atteint la fin du corpus sans erreur et que `artifacts/media-audit/checkpoint.json` contient l'état `complete`.
