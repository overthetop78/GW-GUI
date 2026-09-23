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

- [x] Reconnaître les morceaux Advanced MusicSystem II sans extension.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter la séquence musicale fixe observée dans les sept morceaux AMS II du média arrêté.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer cette signature comme contenu audio Atari 8 bits.

- [x] Ajouter la recherche ordonnée de groupes d'octets au catalogue de reconnaissance.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Enums/MediaContentSearchDirection.cs` pour ajouter `SearchStart` et `SearchEnd`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Contracts/MediaContentSignature.cs` pour conserver un ou plusieurs groupes d'octets dans chaque signature existante.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour adapter les signatures fixes et déclarer les marqueurs ordonnés Atari Binary Load.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Functions/MediaContentSignatureMatcher.cs` pour rechercher les groupes successivement dans la direction demandée et arrêter immédiatement au premier groupe introuvable.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer les fichiers portant les marqueurs Atari Binary Load ordonnés comme exécutables.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour vérifier l'ordre des groupes avec des octets autonomes et retirer les contrôles ABC construits depuis les constantes testées.

- [x] Ignorer les entrées Atari DOS encore ouvertes lors de l'extraction des fichiers.
  - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Atari/Dos/AtariDosDirectoryReader.cs` pour centraliser la distinction entre une entrée enregistrée et une entrée finalisée, puis ne lire et ne retourner que les entrées finalisées.
  - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Atari/Dos/AtariDosWarnings.cs` pour signaler une entrée `OpenForOutput` ignorée sans parcourir sa chaîne de secteurs.
  - [x] Créer `tests/GWGUI.LocalDiskImageTests/AtariDosDirectoryReaderSelfTests.cs` avec un catalogue autonome contenant une entrée ouverte et une entrée finalisée qui pointent vers la même chaîne.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/TemporaryMediaAuditProgram.cs` pour exécuter le contrôle autonome du lecteur Atari DOS.

## Fin de la campagne

- [x] Classer les fichiers internes `FOX`, `S` et `VER` d'AtariWriter+ : l'image passe désormais l'audit sans type inconnu.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour déclarer les signatures du bloc AtariWriter+, du bloc nul et du source assembleur tokenisé.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `FOX` et `S` comme données et `VER` comme code source.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler les trois classifications avec des contenus autonomes.

- [x] Classer `ATARI130` et `GRSWITCH.UTL` de la disquette Atari 1020 Plotter Utils : l'image passe désormais l'audit sans type inconnu.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour déclarer l'extension Atari `.utl`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour déclarer les deux champs fixes Micro Illustrator et le préfixe BASIC enregistré utilisé avec `.utl`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer l'image Micro Illustrator sans extension et le programme BASIC `.utl` sans masquer un `.utl` binaire.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler ces trois cas avec des contenus autonomes.

- [x] Classer les routines machine `CIOUSR` d'Atari Microsoft BASIC : l'image passe désormais l'audit sans type inconnu.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour déclarer les signatures fixes de début et de fin de `CIOUSR`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `CIOUSR` comme bibliothèque Atari 8 bits.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler cette classification avec un contenu autonome.

- [x] Corriger la classification des fichiers Artist Unleashed : l'audit de la face B passe sans erreur, les 24 fichiers auparavant inconnus et `OLDCAR` sont désormais les 25 images reconnues.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter les signatures Artist Unleashed et fixer le marqueur Atari Binary Load au début du fichier.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer ces fichiers comme images et utiliser le marqueur Atari Binary Load fixe.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler les deux classifications sans fichier externe.

- [x] Classer les neuf ressources internes d'AwardWare Side A et reprendre la campagne au point d'arrêt.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour déclarer les signatures des caractères, modèles courts, mises en page et largeurs AwardWare.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer les caractères comme police et les modèles, mises en page et largeurs comme données structurées.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler les quatre formats AwardWare avec des contenus autonomes.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de l'image AwardWare Side A pour cocher cette correction.

- [x] Classer les quatre fichiers internes de B-Graph v1.0 et reprendre la campagne au point d'arrêt.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour déclarer les signatures des trois routines machine B-Graph et de son image binaire.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer les routines comme bibliothèques et l'image B-Graph comme image.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler les routines et l'image B-Graph avec des contenus autonomes.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de B-Graph v1.0 pour cocher cette correction.

- [x] Classer l'extension BASIC XE `.OSS` et reprendre la campagne au point d'arrêt.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour déclarer l'extension Atari 8 bits `.oss`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.oss` comme bibliothèque.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler la classification d'un fichier `.oss` sans fichier externe.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de l'image BASIC XE Extensions pour cocher cette correction.

- [x] Classer l'écran binaire `PICTURE` de The Bear Essentials et reprendre la campagne au point d'arrêt.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour déclarer l'en-tête du fichier graphique Atari extrait.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer ce contenu comme image.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler la classification avec un contenu autonome.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de The Bear Essentials pour cocher cette correction.

- [x] Mettre en attente Big Asembler Side B et poursuivre avec l'image suivante.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour conserver le chemin de l'image et les cinq fichiers sans signature autonome établie, sans ajouter de règle de reconnaissance.
    - Image différée : `F:\Retro\A Trier\Atari 400-800\Atari 8bit - Applications - [ATR] (TOSEC-v2023-08-29)\Big Asembler (1990)(Cygert, Henryk)(pl)(Side B)\Big Asembler (1990)(Cygert, Henryk)(pl)(Side B).atr`.
    - Fichiers à analyser ultérieurement avec leur application : `KOALA1`, `KOALA2`, `PIC12`, `PIC13` et `PIC2`. Leurs préfixes et leurs tailles observées ne constituent pas une signature de format.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 163 et supprimer `artifacts/media-audit/failure.json`, tout en conservant le rapport de l'index 162 pour l'analyse ultérieure.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne depuis l'image suivant Big Asembler Side B jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour cocher la mise en attente et la reprise après leur réalisation.
    - La campagne a validé les index 163 et 164, puis s'est arrêtée à l'index 165 sur `Black Magic Composer (1992)(L.K. Safari)(Side A).atr`.

- [x] Classer les fichiers musicaux de Black Magic Composer et reprendre la campagne.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour déclarer les extensions Atari 8 bits `.msc` et `.drp`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.msc` et `.drp` comme contenus audio Atari 8 bits.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler de façon autonome les deux classifications par extension.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de Black Magic Composer Side A pour cocher cette correction.
    - Black Magic Composer Side A est validée. La campagne a poursuivi jusqu'à l'index 172 et s'est arrêtée sur les fichiers `HELP1` à `HELP4` de `Blazing Paddles (1986)(Baudville)(US).atr`.

- [x] Mettre en attente Blazing Paddles et poursuivre avec l'image suivante.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour conserver le chemin de l'image et les quatre images Micro-Painter brutes sans signature autonome, sans ajouter de règle fondée sur leur taille.
    - Image différée : `F:\Retro\A Trier\Atari 400-800\Atari 8bit - Applications - [ATR] (TOSEC-v2023-08-29)\Blazing Paddles (1986)(Baudville)(US)\Blazing Paddles (1986)(Baudville)(US).atr`.
    - Fichiers à analyser ultérieurement : `HELP1`, `HELP2`, `HELP3` et `HELP4`. Ils correspondent à des images d'aide Micro-Painter brutes, mais ne possèdent ni extension ni signature autonome permettant une reconnaissance fiable sans contexte.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 173 et supprimer `artifacts/media-audit/failure.json`, tout en conservant le rapport de l'index 172 pour l'analyse ultérieure.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne depuis l'image suivant Blazing Paddles jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour cocher la mise en attente et la reprise après leur réalisation.
    - La campagne a validé les index 173 à 191, puis s'est arrêtée à l'index 192 sur `MESSAGE.ISM` et `SURVEY1.SRV` de `Bullentin Board Construction Set (1985)(Antic Publishing)(Side A).atr`.

- [x] Classer les données de Bulletin Board Construction Set et reprendre la campagne.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour déclarer les extensions Atari 8 bits `.ism` et `.srv`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.ism` et `.srv` comme données Atari 8 bits.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler de façon autonome les deux classifications par extension.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de Bulletin Board Construction Set Side A pour cocher cette correction.
    - Bulletin Board Construction Set Side A est validée. La campagne a poursuivi jusqu'à l'index 213 et s'est arrêtée sur quinze fichiers de `CardStax v2.1 (1993)(Paterson, David A.)(PD)(Side A)[BASIC].atr`.

- [x] Retirer les contournements ajoutés pendant l'audit et revenir au premier résultat qu'ils ont faussé.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs`, `src/GWGUI.MediaFileSystems/FileSystems/Atari/BootDisk/AtariBootDiskFormatCatalog.cs` et `src/GWGUI.MediaFileSystems/FileSystems/Atari/BootDisk/AtariBootDiskFileSystemReader.cs` pour retirer le préfixe ATR dynamique et rétablir le catalogue explicite.
  - [x] Supprimer `tests/GWGUI.LocalDiskImageTests/AtariBootDiskFileSystemReaderSelfTests.cs` et modifier `tests/GWGUI.LocalDiskImageTests/TemporaryMediaAuditProgram.cs` pour retirer le contrôle créé pour ce contournement.
  - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Atari/Dos/AtariDosFileReader.cs` et `tests/GWGUI.LocalDiskImageTests/AtariDosDirectoryReaderSelfTests.cs` pour retirer la borne locale fondée sur le nombre déclaré de secteurs.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour supprimer les deux blocs qui présentaient ces contournements comme des corrections terminées.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` pour supprimer les résultats à partir de l'index 150 et reprendre sur `BBS (19xx)(-)[req APE].atr`.

- [x] Reconnaître entièrement le grand volume SpartaDOS de BBS et représenter correctement son adressage logique.
  - [x] Distinguer la géométrie physique d'une disquette de l'adressage logique d'un grand conteneur ATR.
    - [x] Créer `src/GWGUI.MediaEngine/Enums/SectorImageAddressingKind.cs` avec les valeurs `Physical` et `Logical`.
    - [x] Modifier `src/GWGUI.MediaEngine/Images/Models/Sectors/SectorImage.cs` pour conserver ce type d'adressage et le préserver lors d'un changement d'identifiant de format.
    - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Floppy/Atr/AtrGeometry.cs` et modifier `src/GWGUI.MediaEngine/Images/Formats/Floppy/Atr/AtrLayout.cs` ainsi que `AtrReader.cs` pour produire une géométrie physique seulement pour les profils ATR connus et un adressage logique pour les autres tailles valides.
    - [x] Modifier `src/GWGUI.MediaEngine/Images/Writing/Encoding/SectorImageTrackEncoder.cs` et `src/GWGUI.MediaEngine/Images/Visualization/SectorImageVisualizationExceptions.cs` pour refuser l'encodage en pistes physiques d'une image dont seule la disposition logique est connue.
  - [ ] Afficher les secteurs d'un grand ATR logique sans inventer des pistes ou des faces physiques.
    - [x] Créer `src/GWGUI.App/Enums/Rendering/Sectors/SectorMediaLayoutKind.cs` et modifier `src/GWGUI.App/Contracts/Rendering/Sectors/SectorMediaRenderModel.cs` pour indiquer une disposition physique ou logique.
    - [x] Modifier `src/GWGUI.App/Presenters/Visualization/SectorMediaInspectorPresenter.cs` pour construire une seule série logique à partir des blocs d'un ATR sans géométrie physique et présenter ses indices de blocs.
    - [x] Modifier `src/GWGUI.App/Constants/Rendering/Sectors/SectorMediaRenderConstants.cs` pour déclarer les espacements et dimensions de la grille logique.
    - [x] Modifier `src/GWGUI.App/Rendering/Sectors/SkiaSectorMediaRenderer.cs` pour dessiner et sélectionner cette série sous forme de grille logique, tout en conservant le rendu circulaire des géométries physiques.
  - [x] Afficher les secteurs d'un grand ATR logique sans inventer des pistes ou des faces physiques.
  - [ ] Lire le système de fichiers SpartaDOS et toutes ses entrées.
    - [x] Modifier `src/GWGUI.MediaFileSystems/Definitions/FileSystemIds.cs` et `src/GWGUI.MediaFileSystems/Definitions/FileSystemDisplayNames.cs` pour déclarer SpartaDOS.
    - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/SpartaDos/SpartaDosFileSystemLayout.cs`, `SpartaDosDirectoryFlags.cs` et `SpartaDosFileSystemExceptions.cs` avec les constantes, attributs et erreurs du format.
    - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/SpartaDos/SpartaDosDiskHeader.cs`, `SpartaDosDiskReader.cs` et `SpartaDosNameCodec.cs` pour valider les métadonnées du secteur d'amorçage et décoder le nom du volume.
    - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/SpartaDos/SpartaDosSectorAllocation.cs` et `SpartaDosFileData.cs` pour transporter séparément la carte d'allocation et le contenu reconstruit.
    - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Atari/SpartaDos/SpartaDosDirectoryFlags.cs` et `SpartaDosFileSystemLayout.cs` pour exposer les attributs protégé, caché et archivé des entrées.
    - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/SpartaDos/SpartaDosSectorMapReader.cs`, `SpartaDosFileReader.cs` et `SpartaDosDirectoryReader.cs` pour parcourir les cartes de secteurs, les fichiers creux et les sous-répertoires avec leurs tailles, attributs et dates.
    - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/SpartaDos/SpartaDosFileSystemReader.cs` et modifier `src/GWGUI.MediaFileSystems/Exploration/FileSystemReaderCatalog.cs` pour détecter SpartaDOS avant les lecteurs Atari sans catalogue.
    - [x] Modifier `src/GWGUI.MediaFileSystems/Exploration/SectorFileSystemRegistry.cs` pour sonder les lecteurs par leur structure lorsque l'identifiant dynamique d'un conteneur valide n'existe pas dans le catalogue fixe.
  - [x] Lire le système de fichiers SpartaDOS et toutes ses entrées.
  - [x] Vérifier le lecteur avec des données autonomes et reprendre l'audit.
    - [x] Créer `tests/GWGUI.LocalDiskImageTests/SpartaDosFileSystemReaderSelfTests.cs` avec un volume SpartaDOS construit en mémoire contenant un dossier, un fichier normal et un fichier creux.
    - [x] Modifier `tests/GWGUI.LocalDiskImageTests/TemporaryMediaAuditProgram.cs` pour exécuter le contrôle autonome SpartaDOS.
    - [x] Modifier `tests/GWGUI.LocalDiskImageTests/TemporaryMediaAuditProgram.cs` pour écrire le type d'adressage physique ou logique dans chaque rapport sectoriel.
    - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne à l'index 150 et vérifier que BBS expose le volume `APE_BBS`, ses dossiers et ses fichiers avant de poursuivre.
      - BBS est validé comme ATR logique de 8 192 secteurs de 128 octets contenant le volume SpartaDOS `APE_BBS`, avec 4 répertoires et 168 fichiers.
      - La campagne a poursuivi jusqu'à l'index 156 et s'est arrêtée sur `Bibo Menu Makers (1985)(Bibosoft).atr`.
    - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de BBS pour cocher cette correction et consigner le prochain point d'arrêt.
  - [x] Conserver la nature de disquette Atari pour les ATR à adressage logique.
    - [x] Modifier `src/GWGUI.MediaEngine/Images/Reading/MediaImageDocumentFactory.cs` pour réserver sa création sectorielle aux disquettes.
    - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/Floppy/Atr/AtrReader.cs` pour annoncer tous les conteneurs ATR comme des disquettes, indépendamment de leur adressage physique ou logique.
    - [x] Modifier `artifacts/media-audit/items/00000150-f0c483158344f96e/report.json` en relisant BBS et vérifier que le rapport indique `Floppy` avec un adressage `Logical`.
    - [x] Modifier `docs/tasks/media-corpus-audit.md` pour cocher la correction de la nature du média BBS.

- [x] Refaire la reconstruction des fichiers Atari DOS à partir du répertoire complet et des chaînes sectorielles partagées.
  - [ ] Séparer la description des entrées, la lecture bornée des chaînes et l'analyse des allocations.
    - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/Dos/AtariDosDirectoryEntry.cs` pour transporter les métadonnées brutes d'une entrée active.
    - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/Dos/AtariDosSectorChain.cs` pour transporter les secteurs parcourus, leur contenu et l'état structurel d'une chaîne bornée.
    - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Atari/Dos/AtariDosFileReader.cs` et `AtariDosFileData.cs` pour lire exactement l'étendue déclarée et conserver séparément les diagnostics de chaîne.
  - [x] Séparer la description des entrées, la lecture bornée des chaînes et l'analyse des allocations.
  - [ ] Construire les fichiers après l'analyse de toutes les entrées du volume.
    - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Atari/Dos/AtariDosAllocationAnalyzer.cs` pour reconnaître les vues sectorielles partagées et distinguer leurs propriétaires des incohérences isolées.
    - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Atari/Dos/AtariDosDirectoryReader.cs` pour parser le répertoire avant de reconstruire les fichiers et appliquer l'analyse globale des allocations.
    - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Atari/Dos/AtariDosWarnings.cs` pour décrire les fins prématurées, continuations et secteurs partagés sans déclarer ces vues corrompues.
  - [x] Construire les fichiers après l'analyse de toutes les entrées du volume.
  - [x] Vérifier la refonte avec des données autonomes et avec Bibo Menu Makers.
    - [x] Modifier `tests/GWGUI.LocalDiskImageTests/AtariDosDirectoryReaderSelfTests.cs` pour construire en mémoire une chaîne principale et plusieurs vues sectorielles partagées.
    - [x] Modifier `tests/GWGUI.LocalDiskImageTests/AtariDosDirectoryReaderSelfTests.cs` pour vérifier qu'un propriétaire incohérent sans partage démontré reste invalide.
    - [x] Modifier `artifacts/media-audit/items/00000156-b27d1ec907d8977a/report.json` en relisant Bibo et vérifier les 17 fichiers, les 82 870 octets logiques et l'absence d'erreur de capacité.
    - [x] Modifier `docs/tasks/media-corpus-audit.md` pour cocher la refonte après sa validation.

- [ ] Classer les documents et images de CardStax Side A après le retour de la campagne à l'index 150.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour déclarer les extensions Atari 8 bits `.crd`, `.gr8` et `.v` ; réutiliser la constante `.art` existante.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour déclarer l'en-tête fixe des cartes CardStax.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer les cartes `.crd` validées par leur en-tête comme documents et les fichiers `.art`, `.gr8` et `.v` comme images Atari 8 bits.
  - [ ] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler ces classifications avec des contenus autonomes et refuser une fausse carte `.crd`.
  - [ ] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
  - [ ] Modifier `docs/tasks/media-corpus-audit.md` après validation de CardStax Side A pour cocher cette correction et consigner le prochain point d'arrêt.

La campagne est terminée lorsque le script atteint la fin du corpus sans erreur et que `artifacts/media-audit/checkpoint.json` contient l'état `complete`.
