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

- [x] Consigner Bulletin Board Construction Set Side C comme disquette Atari DOS vierge valide et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour consigner le résultat de l'index 194.
    - Image valide et vierge : `F:\Retro\A Trier\Atari 400-800\Atari 8bit - Applications - [ATR] (TOSEC-v2023-08-29)\Bullentin Board Construction Set (1985)(Antic Publishing)(Side C)\Bullentin Board Construction Set (1985)(Antic Publishing)(Side C).atr`.
    - Le catalogue Atari DOS est vide, le VTOC déclare tous les secteurs de données libres et les trois secteurs d'amorçage sont présents. L'absence de fichier est normale pour cette disquette vierge.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 195 et supprimer `artifacts/media-audit/failure.json`, tout en conservant le rapport de l'index 194.

- [x] Classer les configurations RAMDisk de BW Tape System et poursuivre la campagne.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour déclarer l'extension Atari 8 bits `.rd`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.rd` comme configuration binaire Atari 8 bits.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour vérifier avec des contenus autonomes que toutes les configurations `.rd` restent des configurations, même lorsque leurs octets ressemblent à de l'ATASCII.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant BW Tape System à l'index 197 jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de BW Tape System pour cocher cette classification et consigner le prochain point d'arrêt.
    - BW Tape System est validé : ses quatre fichiers `.rd` sont classés comme configurations binaires Atari 8 bits. La campagne a validé les index 197 à 212, puis s'est arrêtée à l'index 213 sur le fichier sans extension `MENU` de CardStax Side A.

- [x] Classer les documents et images de CardStax Side A après le retour de la campagne à l'index 150.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour déclarer les extensions Atari 8 bits `.crd`, `.gr8` et `.v` ; réutiliser la constante `.art` existante.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour déclarer l'en-tête fixe des cartes CardStax.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer les cartes `.crd` validées par leur en-tête comme documents et les fichiers `.art`, `.gr8` et `.v` comme images Atari 8 bits.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour reconnaître les variantes existantes des programmes Atari BASIC tokenisés avec les recherches de signatures déjà disponibles, sans retirer la variante actuellement reconnue.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour ajouter la variante Atari BASIC fondée sur l'en-tête sauvegardé et la séquence tokenisée commune, tout en conservant la règle historique.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler ces classifications avec des contenus autonomes et refuser une fausse carte `.crd`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de CardStax Side A pour cocher cette correction et consigner le prochain point d'arrêt.
    - CardStax Side A est validé : `MENU` est reconnu comme programme Atari BASIC tokenisé, tandis que les classifications `.crd`, `.art`, `.gr8` et `.v` restent valides. La campagne s'est arrêtée au média suivant, Cartridge Dumper, sur le fichier sans extension `ROM16`.

- [x] Reconnaître les fichiers ROM et la ROM de cartouche Atari `ROM16`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Enums/MediaContentCategory.cs` et `src/GWGUI.MediaAnalysis/Enums/MediaContentFormat.cs` pour représenter un fichier ROM indépendamment d'un média physique.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentIconIds.cs`, `src/GWGUI.MediaAnalysis/Functions/MediaContentRecognitionFunctions.cs` et la présentation de l'explorateur pour afficher le type et l'icône ROM.
    - [x] Copier depuis `artifacts/icon-proposals` les icônes adaptées à ROM, System, Library et Shortcut dans `src/GWGUI.App/Assets/Icons/FileTypes`, puis modifier `src/GWGUI.App/Views/Controls/Common/FileEntryIcon.xaml.cs` pour utiliser ces quatre fichiers.
    - [x] Remplacer les choix System, Library et Shortcut par les icônes réellement fournies par le Shell de l'Explorateur Windows pour des fichiers `.sys`, `.dll` et `.lnk`, dans `src/GWGUI.App/Assets/Icons/FileTypes`, puis mettre à jour `src/GWGUI.App/Views/Controls/Common/FileEntryIcon.xaml.cs`.
  - [x] Modifier la ressource commune `src/GWGUI.App/Resources/00-Base/Explorer.resx` pour fournir le libellé invariant `ROM` à toutes les langues prises en charge.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Contracts/MediaContentRecognitionRule.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/MediaContentRecognitionCatalog.cs` pour accepter des longueurs de contenu explicites dans une règle générale.
  - [x] Renommer `src/GWGUI.MediaAnalysis/Functions/MediaContentSignatureMatcher.cs` en `MediaContentRecognitionRuleMatcher.cs` et l'étendre pour valider ensemble longueur et signatures.
  - [x] Créer `src/GWGUI.MediaAnalysis/Constants/MediaContentLengths.cs` et modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour nommer la longueur et les champs structurels de la ROM de cartouche Atari 16 Kio.
  - [x] Modifier les tables commune et Atari 8 bits de `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition` pour classer `.rom` et reconnaître une ROM brute de cartouche Atari par sa longueur et ses champs structurels.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler les ROM avec extension, la ROM Atari sans extension et les contenus invalides de même taille.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en relançant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` après validation de Cartridge Dumper pour cocher cette reconnaissance et consigner le prochain point d'arrêt.
    - Cartridge Dumper est validé : le fichier sans extension `ROM16` est reconnu comme fichier ROM de cartouche Atari 16 Kio. La campagne s'est poursuivie jusqu'à l'index 231 et s'est arrêtée sur `CONTROL` dans `Centro de Costos (1987)(Telematica SA)(CL).atr`, dont le type reste inconnu.

- [x] Examiner le fichier Atari 8 bits `CONTROL` de `Centro de Costos`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les propriétés et les octets structurels de `CONTROL` fournis par `artifacts/media-audit/items/00000231-e614936017a4d9e1/report.json`, puis définir la classification justifiée avant toute modification du code.
    - `CONTROL` est un fichier Atari DOS sans extension de 16 octets : `02` répété sept fois, `01` répété sept fois, puis `1D 9B`. `EXIS140` contient la référence `D2:CONTROL`. Il s'agit donc de données binaires propres à l'application, mais ces valeurs ne constituent pas une signature de format réutilisable. La catégorie justifiée est `Data`; le catalogue ne doit pas transformer cette valeur particulière ou le nom `CONTROL` en règle générale.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en conservant ce média comme point à reprendre ultérieurement et en poursuivant la campagne au média suivant.
    - Le média reste documenté pour une reprise ultérieure. Aucune règle particulière n'a été ajoutée. La campagne a repris au média suivant et s'est arrêtée sur `MAP` dans `Character Set Display Utility (1989)(ANALOG Computing)[a].atr`.

- [x] Identifier le fichier Atari 8 bits `MAP` de `Character Set Display Utility`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les propriétés et les octets structurels de `MAP` fournis par son rapport, puis définir sa classification justifiée avant toute modification du code.
    - `MAP` contient un écran Micro-Painter brut : 7 680 octets de pixels suivis des quatre registres couleur `1E AF 19 E1`, soit 7 684 octets. Comme les fichiers différés de Blazing Paddles, il ne possède aucun en-tête autonome. Il reste associé au travail ultérieur sur les images Micro-Painter sans extension ; aucune règle fondée uniquement sur sa taille n'est ajoutée.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en conservant ce média avec les autres images Micro-Painter différées et en poursuivant au média suivant.
    - La campagne a repris à l'index 244 et s'est arrêtée sur l'entrée `--------.---` de `Code3 Cruncher v2.2d (1993)(Bienias, Adam)(PL)(en)(SW).atr`.

- [x] Identifier la nature de l'entrée Atari DOS `--------.---` de `Code3 Cruncher v2.2d`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les métadonnées et le contenu de cette entrée fournis par son rapport, puis déterminer si elle représente un fichier ou une décoration de catalogue avant toute modification du code.
    - La première entrée `--------.---` contient 4 250 octets de code, commence au secteur 4 immédiatement après les trois secteurs d'amorçage et contient le système BDOS : c'est le fichier système chargé au démarrage dont le nom de catalogue a été remplacé. Les deux autres entrées homonymes sont vides et participent, avec `DISK`, `DON'T PA.CK`, `DON'T SA.VE` et `ON THI.S`, à un message affiché dans le catalogue.
    - Une reconnaissance propre doit provenir du contexte d'amorçage fourni par le lecteur Atari DOS. Aucune règle fondée sur le nom falsifié ou sur une séquence particulière de ce programme n'est ajoutée.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en conservant ce média pour la future prise en charge des fichiers système d'amorçage renommés, puis poursuivre au média suivant.
    - La campagne a repris à l'index 256 et s'est arrêtée sur `XSYSTEXT` dans `CodeBuster (19xx)(Wells, Tom).atr`.

- [ ] Identifier le fichier Atari 8 bits `XSYSTEXT` de `CodeBuster`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec ses métadonnées et ses octets structurels fournis par le rapport, puis définir sa classification justifiée avant toute modification du code.
    - `XSYSTEXT` est un texte ATASCII sans extension de 3 203 octets contenant des équates du système d'exploitation : caractères textuels et séparateurs de lignes `9B`. Sans extension, son rôle précis de source ne peut pas être établi de manière générale ; la classification fiable est `Text` avec l'encodage `Atascii`.
  - [x] Mettre cette reconnaissance en attente avec les autres contenus sans extension qui nécessitent une reconnaissance directe générale.
    - [x] Modifier `docs/tasks/media-corpus-audit.md` pour conserver la décision d'intégrer ultérieurement `ContentLengths` et `PreviewKind` aux critères du tableau existant, sans ajouter une règle particulière à `XSYSTEXT`.
      - La reconnaissance directe devra réutiliser le tableau existant : chaque règle sera évaluée à partir des seuls critères qu'elle renseigne, notamment l'extension, les signatures, les longueurs et le type d'aperçu. Les critères renseignés seront cumulés. Une règle limitée à `PreviewKind`, à `ContentLengths`, ou à leur combinaison devra appeler l'analyse générale correspondante du contenu. Cette refonte est différée afin d'être conçue pour tous les contenus concernés et de ne pas introduire une exception ATASCII propre à `XSYSTEXT`.
    - [x] Modifier `artifacts/media-audit/checkpoint.json` et supprimer `artifacts/media-audit/failure.json` afin de reprendre au média suivant l'index 258.
    - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne jusqu'au prochain défaut ou à la fin du corpus.
      - La campagne a validé les index 259 et 260, puis s'est arrêtée à l'index 261 sur `BETH.PZM` dans `Colorizer v1.1 (1992)(AtariServ).atr`.

- [x] Identifier les fichiers Atari 8 bits `.PZM` de `Colorizer v1.1`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les entrées `.PZM`, leurs propriétés structurelles et la signification documentée du format avant toute modification du catalogue.
    - `BETH.PZM` est l'unique entrée `.PZM` du média. Elle contient 15 360 octets, sans en-tête ni fin de fichier distinctifs.
    - La documentation des suffixes Atari 8 bits identifie `.PZM` comme une image Pryzm non compressée de 80 × 192 pixels et 256 couleurs. Sa longueur correspond exactement aux 15 360 pixels de cette définition. La documentation de Colorizer confirme que l'application manipule des images Atari 8 bits à 256 couleurs composées à partir des modes GTIA.
    - Comme `.PZM` possède ici une signification documentée et propre à la famille Atari 8 bits, la classification proposée est `Image` par extension dans le catalogue Atari 8 bits. La longueur observée reste une confirmation du fichier examiné et ne devient pas un critère général de cette règle.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs`, `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` et `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour classer `.pzm` comme image Atari 8 bits par sa seule extension, après accord de l'utilisateur.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt après la classification approuvée.
    - `Colorizer v1.1` est validé. La campagne s'est arrêtée au média suivant, à l'index 262, sur les fichiers `.B`, `.G` et `.R` de `Colorview v2.5`.

- [x] Classer correctement les trois plans RGB de `Colorview v2.5`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les propriétés des fichiers et la définition documentée des extensions avant toute modification du catalogue.
    - `COLORS.B`, `COLORS.G`, `COLORS.R`, `SGIRL15.B`, `SGIRL15.G` et `SGIRL15.R` contiennent chacun 7 680 octets. ColorView représente une image 80 × 192 par trois écrans de 7 680 octets, un pour chaque composante rouge, verte et bleue. Les références de formats Atari 8 bits recensent `.B`, `.G` et `.R` comme extensions des fichiers RGB.
    - `UNICRN15.B` commence par `FF FF` et a été classé à tort comme exécutable par la signature Atari Binary Load avant que son extension puisse être consultée. L'ajout des trois extensions ne suffit donc pas à garantir la classification correcte de tous les fichiers du média.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.b`, `.g` et `.r` comme images Atari 8 bits.
  - [x] Refaire la priorité des règles de reconnaissance sans donner systématiquement la priorité aux extensions.
    - [x] Créer `src/GWGUI.MediaAnalysis/Enums/MediaContentRecognitionPriority.cs` avec les niveaux `Primary`, `Standard` et `Fallback`.
    - [x] Modifier `src/GWGUI.MediaAnalysis/Contracts/MediaContentRecognitionRule.cs` pour placer la priorité après la famille, parmi les critères de reconnaissance.
    - [x] Modifier toutes les tables de `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition` pour renseigner explicitement la priorité de chaque règle, avec les signatures structurelles fiables en `Primary`, les extensions en `Standard` et les indices faibles en `Fallback`.
    - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/MediaContentRecognitionCatalog.cs` pour rechercher une règle dans une priorité demandée tout en conservant l'ordre famille exacte, famille parente, puis règles communes.
    - [x] Modifier `src/GWGUI.MediaAnalysis/Functions/MediaContentClassifier.cs` pour appliquer successivement `Primary`, `Standard`, les métadonnées et la reconnaissance directe existante, puis `Fallback`.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler les trois extensions, le cas `.B` commençant par `FF FF`, un Atari Binary Load sans autre reconnaissance et la priorité des signatures structurelles fiables.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Colorview v2.5` est validé : les fichiers `.B`, `.G` et `.R` sont classés comme images même lorsque leur contenu commence par l'indice faible `FF FF`. La campagne s'est arrêtée à l'index 264 sur `ARTLOAD` dans `Colour Enhancer for Micropainter & Atari Artist`.

- [x] Identifier le fichier Atari 8 bits sans extension `ARTLOAD` de `Colour Enhancer for Micropainter & Atari Artist`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec sa structure et sa classification justifiée avant toute modification du moteur de reconnaissance.
    - `ARTLOAD` contient 1 739 octets et commence par `FE FE C7 06`. Le champ little endian `C7 06` vaut 1 735, soit exactement la longueur totale moins les quatre octets de l'en-tête.
    - Les fichiers `ARTIST.M65` et `MYCIO.M65` du même média commencent également par `FE FE`, suivis d'un champ égal à leur longueur totale moins quatre. La documentation MAC/65 définit cette structure comme celle d'un source sauvegardé sous forme tokenisée : marque `FE FE`, longueur du programme sur deux octets, puis lignes tokenisées.
    - `ARTLOAD` est donc un source MAC/65 tokenisé sans extension. Une règle limitée à `FE FE` ne suffit pas : la reconnaissance fiable doit aussi valider le champ de longueur et la structure successive des lignes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour décrire séparément les trois positions fixes communes aux sources MAC/65 observés : `FE FE` à l'octet 0, `0A 00` à l'octet 4 et `58 3B` à l'octet 7.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer comme source MAC/65 la combinaison complète de ces trois signatures en priorité `Fallback`.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler la combinaison complète et refuser les contenus qui ne possèdent que `FE FE`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `ARTLOAD` est reconnu comme source MAC/65 tokenisé par la combinaison des trois positions fixes. La campagne a validé les médias suivants jusqu'à l'index 273, où elle s'est arrêtée sur `NAN8` dans `Computereyes v1.3`.

- [x] Identifier le fichier Atari 8 bits sans extension `NAN8` de `Computereyes v1.3`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec ses propriétés, ses signatures éventuelles et sa classification justifiée avant toute modification du catalogue.
    - `NAN8` contient 7 680 octets de données graphiques brutes. Le média et la documentation ComputerEyes l'identifient comme une capture à huit niveaux utilisant la zone bitmap Graphics 8.
    - Le fichier n'a ni extension ni en-tête ou fin de fichier propre au format. Sa taille correspond à la zone graphique, mais elle ne suffit pas à distinguer ce contenu d'une autre image brute de même taille. Aucune règle fondée uniquement sur le nom `NAN8` ou sur sa longueur n'est ajoutée.
  - [x] Modifier `artifacts/media-audit/checkpoint.json`, supprimer `artifacts/media-audit/failure.json` et poursuivre la campagne au média suivant en conservant ce cas avec les images brutes sans signature à reprendre ultérieurement.
    - La campagne a repris à l'index 274 et s'est arrêtée sur `HERMAN9` et `NAN9` dans `Computereyes v2.0`.

- [x] Identifier les fichiers Atari 8 bits sans extension `HERMAN9` et `NAN9` de `Computereyes v2.0`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leurs propriétés et leur classification justifiée avant toute modification du catalogue.
    - `HERMAN9` et `NAN9` contiennent chacun 7 680 octets. Ce sont les images brutes Graphics 9 fournies avec ComputerEyes v2.0, sans extension, en-tête ou fin de fichier distinctifs.
    - Comme pour `NAN8`, aucune règle fondée seulement sur leur nom ou leur longueur n'est ajoutée. Ils restent avec les images brutes sans signature à reprendre ultérieurement.
  - [x] Modifier `artifacts/media-audit/checkpoint.json`, supprimer `artifacts/media-audit/failure.json` et poursuivre la campagne au média suivant.
    - La campagne s'est poursuivie jusqu'à `Disk Doctor II` et s'est arrêtée sur `LABELS.LDT`.

- [x] Identifier le fichier Atari 8 bits `LABELS.LDT` de `Disk Doctor II`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec son rôle documenté et sa classification avant toute modification du catalogue.
    - Le manuel de Disk Doctor II indique que ses fichiers de labels de désassemblage reçoivent automatiquement l'extension `.LDT`. `LABELS.LDT` contient la table des noms associés aux adresses système ; il doit être classé comme fichier de données Atari 8 bits.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter l'extension `.ldt`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.ldt` dans `Data`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Disk Doctor II` est validé. La campagne s'est arrêtée plus loin sur le fichier sans extension `MENU` de `Disk Tool 4`.

- [x] Identifier le fichier Atari 8 bits sans extension `MENU` de `Disk Tool 4`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec sa structure et la variante de format observée avant toute modification du catalogue.
    - `MENU` est un programme Atari BASIC tokenisé commençant par `00 00 10 01`. Le fichier `POLYCPY4.BAS` du même média possède le même préfixe et est reconnu grâce à son extension `.BAS`. Cette variante complète l'en-tête `00 00 00 01` déjà pris en charge.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter la variante d'en-tête Atari BASIC `00 00 10 01` sans retirer la variante existante.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour reconnaître cette variante en priorité `Fallback`.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour contrôler la nouvelle variante et conserver la reconnaissance historique.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Disk Tool 4` est validé. La campagne s'est arrêtée plus loin sur `SETUP.DC` dans `DOS Control v1.0`.

- [x] Identifier le fichier Atari 8 bits `SETUP.DC` de `DOS Control v1.0`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec son contenu, son rôle dans l'application et sa classification justifiée avant toute modification du catalogue.
    - `SETUP.DC` est un fichier binaire de 4 980 octets dont l'en-tête commence par l'identifiant `DC-MOD1`. Il s'agit d'un module chargé par l'application `DC.COM`, à classer comme `Library` dans la famille Atari 8 bits.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter l'extension `.dc`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.dc` dans `Library`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `DOS Control v1.0` est validé. La campagne s'est arrêtée sur sept fichiers `.DCT` de `DOS Control v2.5`.

- [x] Identifier les fichiers Atari 8 bits `.DCT` de `DOS Control v2.5`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leurs en-têtes, leur rôle et leur classification justifiée avant toute modification du catalogue.
    - Les sept fichiers `.DCT` commencent par l'identifiant `DC-MOD20`. Ils représentent les modules de DOS Control 2.0/2.5 et doivent être classés comme `Library` dans la famille Atari 8 bits, comme les modules `.DC` de la version 1.0.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter l'extension `.dct`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.dct` dans `Library`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `DOS Control v2.5` est validé. La campagne s'est arrêtée sur neuf fichiers `.NLQ` dans `Dot-Magic`.

- [x] Identifier les fichiers Atari 8 bits `.NLQ` de `Dot-Magic`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leur rôle documenté et leur classification avant toute modification du catalogue.
    - Les fichiers `BLOCK.NLQ`, `BROADWAY.NLQ`, `CURSIVE.NLQ`, `OHIO.NLQ`, `OLDE.NLQ`, `OLDWEST.NLQ`, `ROMAN.NLQ`, `SANSERIF.NLQ` et `SCRIPT.NLQ` sont des polices Near Letter Quality. Les références de formats Atari 8 bits recensent également `.NLQ` comme police Daisy Dot. Ils doivent être classés dans `Font`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter l'extension `.nlq`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.nlq` dans `Font`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Dot-Magic` est validé. La campagne s'est arrêtée sur les fichiers `.PB`, `.3` et `.7` de `Draw7 XE`.

- [x] Identifier les fichiers Atari 8 bits `.PB`, `.3` et `.7` de `Draw7 XE`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leurs propriétés, leurs signatures éventuelles et leur classification justifiée avant toute modification du catalogue.
    - Les cinq fichiers `.PB` partagent l'en-tête `77 02 00 D8 E0 16 03 50 81 00 00 0F`. Ils stockent les commandes de dessin rejouables de Draw7 ; `EAGLE1.PB` référence le fichier suivant `EAGLE2.PB`. Ils doivent être classés dans `Data`.
    - `PIC.3` contient 244 octets, soit 240 octets d'écran Graphics 3 et quatre octets de couleurs. `PIC.7` contient 7 684 octets, soit 7 680 octets d'écran et quatre octets de couleurs. Ces deux fichiers doivent être classés dans `Image` par leurs extensions Atari 8 bits propres à Draw7.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter les extensions `.pb`, `.3` et `.7`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.pb` dans `Data` et `.3`/`.7` dans `Image`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Draw7 XE` est validé. La campagne s'est arrêtée sur `PLANTARY` et `WATCH` sans extension dans `Drawing Board`.

- [x] Identifier les fichiers Atari 8 bits sans extension `PLANTARY` et `WATCH` de `Drawing Board`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leurs propriétés, leurs signatures éventuelles et leur classification justifiée avant toute modification du catalogue.
    - `PLANTARY` et `WATCH` font partie des huit images Drawing Board du média. Ces huit fichiers possèdent exactement la même séquence finale de 32 octets, `AE E7 02 AC E8 02 86 80 84 81 A9 00 85 92 85 CA C8 8A A2 82 95 00 E8 94 00 E8 E0 92 90 F6 A2 86`.
    - Certains de ces fichiers étaient classés `Data` à cause de leur préfixe nul. La fin commune constitue une signature de format beaucoup plus précise et doit les classer dans `Image` sans utiliser leur nom ni leur longueur.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter la signature finale commune des images Drawing Board.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer cette signature dans `Image` en priorité `Primary`.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour vérifier que cette signature l'emporte sur le préfixe nul classé en `Fallback`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Drawing Board` est validé. La campagne s'est arrêtée sur les fichiers `.G15`, `.G9` et `.DEM` de `Easy Scan v2.0`.

- [x] Identifier les fichiers Atari 8 bits `.G15`, `.G9` et `.DEM` de `Easy Scan v2.0`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leurs propriétés, leurs signatures éventuelles et leur classification justifiée avant toute modification du catalogue.
    - Les fichiers `.G15` sont des images Graphics 15 de 7 684 octets et `SALLY.G9` est une image Graphics 9. Leur taille confirme les fichiers observés mais ne devient pas un critère du catalogue.
    - `ESCAN20.DEM` commence par `00 00 30 01` et contient un programme Atari BASIC tokenisé. Comme `.DEM` peut désigner plus généralement des données de démonstration, cette classification doit exiger ensemble l'extension et cet en-tête.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter les extensions `.g15`, `.g9` et `.dem`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter l'en-tête Atari BASIC `00 00 30 01`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.g15`/`.g9` dans `Image` et la combinaison `.dem` avec l'en-tête dans `BasicProgram`.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaContentClassifierSelfTests.cs` pour vérifier que `.dem` exige aussi l'en-tête attendu.
  - [x] Restaurer le point d'entrée permanent et le nettoyage complet du validateur avant de reprendre la campagne.
    - [x] Déplacer `tests/GWGUI.LocalDiskImageTests/TemporaryMediaAuditProgram.cs` vers `tests/GWGUI.LocalDiskImageTests/Program.cs` en conservant le validateur courant, son bloc `finally` et l'appel centralisé à `WpfResourceCleanup.Release()`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Easy Scan v2.0` est validé. La campagne s'est arrêtée plus loin sur le fichier sans extension `PRT` de `Financial Wizard`.

- [x] Garantir la libération complète des ressources WPF créées par `GWGUI.LocalDiskImageTests`.
  - [x] Centraliser la fermeture et le détachement de toutes les fenêtres, l'arrêt de l'`Application` et du `Dispatcher`, et la collecte finale dans `tests/GWGUI.LocalDiskImageTests/TestInfrastructure/WpfResourceCleanup.cs`, en poursuivant le nettoyage de toutes les ressources même si l'une d'elles échoue.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/LocalExplorerOpeningCheck.cs` pour confier au nettoyage centralisé l'`Application` et le `Dispatcher` qu'il possède, puis attendre et contrôler leur destruction effective avec celle du thread STA.
  - [x] Créer `tests/GWGUI.LocalDiskImageTests/WpfResourceCleanupSelfTests.cs` pour ouvrir une vraie fenêtre sur un thread STA et vérifier après nettoyage que son handle est détruit, que le `Dispatcher` est arrêté et que le thread est terminé.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/Program.cs` pour exécuter ce contrôle autonome et garantir la collecte finale même lorsqu'une étape du nettoyage échoue.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec le résultat de la compilation et du contrôle autonome du cycle de vie WPF.
    - La compilation Debug ciblée de `GWGUI.LocalDiskImageTests` réussit sans erreur.
    - Le contrôle autonome crée une vraie fenêtre et un handle natif, puis confirme la destruction du handle, l'arrêt complet du `Dispatcher` et la fin du thread STA.
    - La recherche ciblée dans le projet confirme que le test Explorateur et ce contrôle autonome sont les seuls propriétaires WPF ; tous deux passent désormais par `WpfResourceCleanup`.
    - Le processus exécutant `--self-test` se termine normalement avec le code de sortie `0`.

- [x] Identifier le fichier Atari 8 bits sans extension `PRT` de `Financial Wizard`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec son contenu, ses signatures éventuelles et sa classification justifiée avant toute modification du catalogue.
    - `PRT` contient trois lignes ATASCII de commandes d'imprimante : `1B 21 9B`, `1B 22 9B` et `1B 4E 9B`.
    - Les programmes BASIC du même média ouvrent `D:PRT` et demandent à l'utilisateur de saisir les commandes de contrôle de son imprimante. `PRT` est donc un fichier de configuration d'impression.
    - Les commandes enregistrées peuvent être remplacées par l'utilisateur. La séquence observée et la longueur de neuf octets ne définissent donc pas un format stable et ne doivent pas devenir une signature du catalogue.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour décrire la structure variable du fichier : trois enregistrements de commande commençant par `1B` et terminés par `9B`, sans imposer les commandes ni la longueur observées.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer cette structure dans `Configuration` avec un aperçu hexadécimal.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Financial Wizard` est validé. La campagne s'est arrêtée sur `KHARM`, `P50` et `P50B` sans extension dans `K3 Wave Table Editor`.

- [x] Identifier les fichiers Atari 8 bits sans extension `KHARM`, `P50` et `P50B` de `K3 Wave Table Editor`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leur contenu, leurs signatures éventuelles et leur classification justifiée avant toute modification du catalogue.
    - `P50B` contient l'interface « PATCH DUMP, SAVE FOR THE K3 », référence `D:KHARM` et effectue ses accès disque par CIO. `P50` est un petit module d'entrée-sortie qui prépare également les blocs IOCB et appelle CIO. `KHARM` contient du code 6502 et ses tables internes.
    - Aucun des trois fichiers ne possède l'enveloppe Atari Binary Load `FF FF`. Ils sont chargés à des adresses imposées par l'application et ne constituent pas des exécutables autonomes ; ils doivent être classés dans `Library`.
    - Leurs tailles et leurs noms ne participent pas à la reconnaissance. Chacun possède en revanche un début de code distinctif suffisamment long pour identifier le module sans confondre les données musicales `.P50`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter les trois débuts de code des modules internes de K3 Wave Table Editor.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer ces trois signatures dans `Library`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `K3 Wave Table Editor` est validé. La campagne s'est arrêtée sur `CORE.BIN` dans `Laserteller`.

- [x] Identifier les modules Atari 8 bits `.BIN` de `Laserteller`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leur contenu, leurs signatures éventuelles et leur classification justifiée avant toute modification du catalogue.
    - `CORE.BIN`, `LASER.BIN` et `SETUP.BIN` commencent chacun par un point d'entrée 6502, contiennent du code machine et sont chargés par l'application à des adresses imposées. Ils doivent être classés dans `Library` plutôt que comme exécutables Atari Binary Load autonomes.
    - `LASER.BIN` et `SETUP.BIN` étaient classés à tort dans `Text` par la détection générique, car leurs nombreuses chaînes d'interface rendaient plus de 90 % de l'échantillon affichable. Leurs signatures de code doivent être prioritaires sur cette détection.
    - L'extension `.BIN`, le nom et la taille ne suffisent pas à distinguer ces modules des autres fichiers binaires Atari.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter les débuts de code distinctifs des trois modules de Laserteller.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer ces signatures dans `Library` avant la détection générique de texte.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Laserteller` est validé. La campagne s'est arrêtée sur `WIZTALK.SPK` dans `Math Wizard II`.

- [x] Identifier le fichier Atari 8 bits `.SPK` de `Math Wizard II`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec son contenu, ses signatures éventuelles et sa classification justifiée avant toute modification du catalogue.
    - `WIZTALK.SPK` est la banque de parole de « Talking Math Wizard ». Le programme BASIC associé appelle `SPEAK`, tandis que le fichier contient une table d'offsets suivie des données vocales encodées.
    - L'extension Atari 8 bits `.SPK` suffit à désigner ces données de parole. Le nom, la taille et les octets particuliers de cette banque ne doivent pas participer à la reconnaissance.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter `.spk`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.spk` dans `Audio`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Math Wizard II` est validé. La campagne s'est arrêtée sur huit images Micro-Painter sans extension.

- [x] Mettre de côté les images sans extension de `Micro-Painter` jusqu'à la reconnaissance générale de ce format.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les données observées et la raison du report.
    - `CAR`, `COFEMILL`, `EINSTEIN`, `GUITAR`, `SAILBOAT`, `STILLIFE`, `SUNMOON` et `TIGER` sont huit images Micro-Painter de 7 684 octets.
    - La longueur correspond au format observé, mais elle reste volontairement exclue comme critère unique. Aucun en-tête distinctif commun n'est présent ; les octets graphiques commencent immédiatement.
    - Ce cas rejoint les images Micro-Painter sans extension déjà reportées. Il sera repris avec la reconnaissance générale des contenus graphiques dépourvus de signature.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour conserver ce média comme cas différé et reprendre au média suivant.

- [x] Mettre de côté le bloc graphique sans extension `SHUT` de `Micro-Painter`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les données observées et la raison du report.
    - `SHUT` contient 3 072 octets de données graphiques brutes, sans extension ni en-tête, parmi les images `.MIC` du média.
    - Il ne possède pas la structure complète de 7 684 octets des images Micro-Painter. Une signature tirée de ses pixels identifierait seulement ce dessin particulier.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour conserver ce média comme cas différé et reprendre au média suivant.

- [x] Identifier le fichier Atari 8 bits `.MO2` de `Mini Office II`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec son contenu et sa classification justifiée avant toute modification du catalogue.
    - L'extension `.MO2` est partagée par plusieurs contenus de Mini Office II : `COM.MO2`, `GRAPHICS.MO2`, `MAINMENU.MO2` et `WORDPROC.MO2` sont des exécutables Atari Binary Load, tandis que `ADDRESS.MO2` est une base d'adresses structurée contenant ses champs et ses textes en ATASCII.
    - `ADDRESS.MO2` possède l'en-tête interne `CD C4 01 0D` suivi de sa table de champs. Il doit être classé dans `Data` par cette structure, sans donner un sens unique à l'extension `.MO2` et sans employer son nom ou sa taille.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter l'en-tête de la base d'adresses Mini Office II.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer cette signature dans `Data`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Mini Office II` est validé. La campagne s'est arrêtée sur `BOY1.GR9` et `NEWS.PLM` dans `Multi Graph View`.

- [x] Identifier les fichiers Atari 8 bits `.GR9` et `.PLM` de `Multi Graph View`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leur contenu et leur classification justifiée avant toute modification du catalogue.
    - `BOY1.GR9` est une image Atari Graphics 9 et `NEWS.PLM` une image Atari 8 bits PLM de 80 × 96 pixels en 256 couleurs.
    - Les deux fichiers contiennent 7 684 octets, soit les données attendues de ces formats graphiques, mais leur taille ne sera pas utilisée comme critère de reconnaissance.
    - Dans le contexte Atari 8 bits, les extensions `.GR9` et `.PLM` suffisent à identifier ces formats d'image.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter `.gr9` et `.plm`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.gr9` et `.plm` dans `Image`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Multi Graph View` est validé. La campagne s'est arrêtée sur le fichier graphique sans extension `MAP` de `MultiDOS`.

- [x] Mettre de côté le fichier graphique sans extension `MAP` de `MultiDOS`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les données observées et la raison du report.
    - `MAP` contient 7 684 octets de données graphiques brutes sans extension ni en-tête de format distinctif.
    - Son contenu présente des données d'écran et de palette, mais aucun critère structurel ne permet actuellement de distinguer ce fichier des autres images brutes Atari 8 bits sans employer son nom ou sa taille seule.
    - Ce cas rejoint les images Micro-Painter sans extension déjà reportées et sera repris avec leur reconnaissance générale.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour conserver ce média comme cas différé et reprendre au média suivant.

- [x] Nettoyer les artefacts détaillés des anciens arrêts désormais dépassés.
  - [x] Modifier `artifacts/media-audit/items/00000647-e94c1b39f8e1306a`, `artifacts/media-audit/items/00000648-565acdc6615efe4a` et `artifacts/media-audit/items/00000670-3e2bb2a5af78e353` pour ne conserver que `report.json`.

- [x] Identifier les fichiers musicaux sans extension de `Music Construction Set`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leur rôle et leurs marqueurs structurels avant toute modification du catalogue.
    - Chaque morceau possède deux fichiers homonymes : un fichier `.MUS` déjà reconnu et un fichier sans extension. La documentation du logiciel confirme que les morceaux Atari sont stockés par paires.
    - Les dix fichiers sans extension commencent par le marqueur commun `1F 0A 10 00` et contiennent, 32 octets avant leur fin, le bloc terminal commun `1F 0A 00 36 1F 0A 60 36 1F 0A C0 36 1F 0A 20 37 1F 0A 80 37 1F 0A E0 37 1F 0A 40 38`.
    - La reconnaissance combinera ces deux emplacements fixes. Elle n'utilisera ni le nom, ni la longueur variable, ni les notes propres à un morceau.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter les marqueurs de début et de fin des données musicales de `Music Construction Set`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer leur combinaison dans `Audio`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Music Construction Set` est validé. La campagne s'est arrêtée sur les morceaux et instruments de `The Music Studio`.

- [x] Identifier les fichiers musicaux sans extension stable de `The Music Studio`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leur structure et leur classification avant toute modification du catalogue.
    - Les fichiers dont le nom commence par `M` contiennent les morceaux ; ceux dont le nom commence par `S` contiennent les instruments. Les fragments placés après le point proviennent des titres longs stockés dans les champs 8.3 et ne constituent pas des extensions de format fiables.
    - Tous possèdent la même table d'instruments de 256 octets. Trois blocs binaires non textuels sont identiques aux positions 68, 187 et 238 dans les morceaux comme dans les fichiers d'instruments.
    - La reconnaissance utilisera la combinaison de ces trois blocs fixes et ne dépendra ni du nom, ni de la pseudo-extension, ni de la longueur du morceau.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter les trois marqueurs fixes de la table d'instruments de `The Music Studio`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer leur combinaison dans `Audio`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `The Music Studio` est validé. La campagne s'est arrêtée sur `PANTHER.FN0` dans `MyDOS 4.50T & Utils`.

- [x] Identifier l'extension de fonte Atari 8 bits `.FN0`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec son contenu et sa classification avant toute modification du catalogue.
    - `PANTHER.FN0` contient une fonte bitmap Atari complète de 1 024 octets, aux côtés de `PANTHER.FNT` et de plusieurs autres fontes `.FNT` sur le même média.
    - Dans ce contexte Atari 8 bits, `.FN0` est une extension de fonte et suffit à la classification ; la longueur ne sera pas utilisée comme critère.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter `.fn0`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.fn0` dans `Font`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `MyDOS 4.50T & Utils` est validé. La campagne s'est arrêtée sur `MEM.SA` dans `The NewsRoom`.

- [x] Identifier le module interne `MEM.SA` de `The NewsRoom`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec son contenu et sa classification avant toute modification du catalogue.
    - `MEM.SA` contient du code machine 6502 brut appelé par les autres modules de `The NewsRoom`. Il ne possède pas l'enveloppe Atari Binary Load et n'est donc pas un exécutable autonome.
    - L'extension `.SA` n'est pas suffisamment générale pour être classée seule. Le module sera reconnu par son en-tête machine de 16 octets et classé comme bibliothèque interne.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter l'en-tête du module mémoire de `The NewsRoom`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer cette signature dans `Library`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `The NewsRoom` est validé. La campagne s'est arrêtée sur `TITLE.SCN` dans `Picility v9G`.

- [x] Identifier l'image Atari 8 bits `.SCN` de `Picility v9G`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec son contenu et sa classification avant toute modification du catalogue.
    - `TITLE.SCN` contient un écran bitmap Atari suivi de ses octets de couleur. Il est chargé comme écran de titre par l'application graphique `Picility v9G`.
    - Dans la famille Atari 8 bits, l'extension `.SCN` désigne ici un écran et suffit à le classer dans `Image`; la longueur ne sera pas utilisée comme critère.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter `.scn`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.scn` dans `Image`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Picility v9G` est validé. La campagne s'est arrêtée sur les dessins sans extension de `Player-Missile Graphics Tablet`.

- [x] Identifier les dessins de `Player-Missile Graphics Tablet` indépendamment de leur nom et de leur extension.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leur structure et leur classification avant toute modification du catalogue.
    - `LOG` et `TEST1` à `TEST5` sont des dessins de l'éditeur Player-Missile, au même format que `EXAMPLE.DAT` et `EXPRINT.DAT` présents sur le média.
    - Tous commencent par l'en-tête binaire `17 28 CA 94 46 00 70 70 70 4D 60 90 0D 0D 0D 0D`. La longueur identique de 4 206 octets ne sera pas utilisée comme critère.
    - Une règle de contenu prioritaire permettra également de corriger la classification générique `Data` des deux fichiers `.DAT` de ce même format.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter l'en-tête des dessins de `Player-Missile Graphics Tablet`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer cette signature dans `Image`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne au point d'arrêt jusqu'au prochain défaut ou à la fin du corpus.
    - `Player-Missile Graphics Tablet` est validé. La campagne s'est arrêtée sur les entrées du volume spécialisé `Atari CLK graphics library`.

- [x] Mettre de côté la bibliothèque graphique CLK de `The Print Shop Companion` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les données observées et la raison du report.
    - Le volume est correctement reconnu comme `atari-clk-graphics-library` et ses 50 entrées sont correctement extraites avec `NativeTypeId = atari-clk-graphic`, le commentaire `Atari CLK graphic` et l'attribut `graphics`.
    - Le défaut restant concerne uniquement leur classification dans MediaAnalysis : elles restent `Unknown` bien que le système de fichiers ait déjà identifié leur type natif.
    - Ce cas est reporté afin de définir plus tard une utilisation générale des types natifs déjà reconnus, sans ajouter une colonne à toutes les tables pour cette seule disquette.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 783 et supprimer `artifacts/media-audit/failure.json`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne jusqu'au prochain défaut ou à la fin du corpus.
    - Le média 783 est validé. La campagne s'est arrêtée à l'index 784 sur `The Print Shop - Graphics Library (Disk 1 of 3)`, qui utilise le même système `atari-clk-graphics-library` et présente le même défaut de classification différé.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 785 et supprimer `artifacts/media-audit/failure.json`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne jusqu'au prochain défaut.
    - La campagne s'est arrêtée à l'index 785 sur le disque 2/3 de la même bibliothèque CLK ; ce média est rattaché au même cas différé.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 786 et supprimer `artifacts/media-audit/failure.json`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne jusqu'au prochain défaut.
    - Le média 786 est validé. La campagne s'est arrêtée à l'index 787 sur `The Print Shop - Icons 01`, encore reconnu comme `atari-clk-graphics-library` avec uniquement ses graphismes classés `Unknown`.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` et supprimer `artifacts/media-audit/failure.json` après chaque arrêt strictement identique sur un volume `atari-clk-graphics-library`, afin de reprendre au média suivant.
    - Les index 787 à 796, 798, 801 et 802 présentant strictement ce même cas ont été différés ; les index 797, 799, 800, 803 et 804 ont été validés normalement.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne jusqu'au prochain défaut d'une autre nature ou à la fin du corpus.
    - La campagne s'est arrêtée à l'index 805 sur `PrintPower (1987)(Hi Tech Expressions)(US)(Side B).atr` : le volume Atari DOS contient 17 fichiers extraits, dont 14 fichiers `.001` classés `Unknown`.
  - [x] Modifier les sous-dossiers antérieurs à `artifacts/media-audit/items/00000805-99d4401efaa283e8` afin de ne conserver que leur `report.json`.

- [x] Identifier les fichiers `.001` de `PrintPower`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leur rôle, les critères fiables de reconnaissance et la classification retenue avant toute modification du catalogue.
    - `.001` est un suffixe commun aux ressources du disque et ne désigne pas un type unique : il ne doit donc pas être ajouté seul au catalogue.
    - `FONTS.001` associe les noms affichés aux fichiers `AVANT.001`, `HEADLINE.001`, `OLDENG.001`, `TIMES.001` et `ZAPF.001`. Ce sont les fontes de PrintPower et elles doivent être classées dans `Font`.
    - `GRAPHICS.001` associe chaque dessin à un fichier `GV1.001` à `GV7.001`, avec un numéro de variante et une catégorie. Ces sept fichiers sont des bibliothèques graphiques de PrintPower et doivent être classés dans `Image`.
    - `BORDERS.001` associe les bordures aux fichiers `B.001` et `B2.001`, avec leur numéro de variante. Ces deux fichiers sont des bibliothèques de bordures et doivent être classés dans `Image`.
    - Les cinq fontes partagent `6B 29` à l'offset 2 puis la même organisation bitmap. Les deux bibliothèques de bordures commencent par `14 00 00 00` et une table de vingt offsets. Les bibliothèques graphiques commencent par `05 00 00 00` et une table de cinq offsets, mais cette seule valeur n'est pas assez distinctive pour être utilisée isolément.
    - La documentation de PrintPower confirme que le produit fournit plusieurs fontes, des graphismes et des bordures ; les fichiers d'index du média établissent précisément la fonction de chaque fichier binaire observé.

- [x] Reconnaître les ressources `.001` de `PrintPower` avec le catalogue existant.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les combinaisons de critères retenues.
    - Les fontes seront reconnues par l'extension `.001` et leur bloc fixe de 16 octets à l'offset 2.
    - Les bibliothèques de bordures seront reconnues par l'extension `.001`, leur en-tête `14 00 00 00` à l'offset 0 et la fin fixe de leur table à l'offset 42.
    - Les bibliothèques graphiques seront reconnues par l'extension `.001` et leur en-tête `05 00 00 00` à l'offset 0.
    - Les trois règles seront prioritaires afin que leur contenu précis soit examiné avant toute classification générique de l'extension.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter la constante `.001`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter les signatures PrintPower des fontes, bordures et bibliothèques graphiques.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer les trois structures dans `Font` ou `Image`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour corriger l'offset de la seconde signature des bordures : le dernier des vingt offsets de leur table commence à l'offset 42 et se poursuit avec les données à l'offset 44.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour placer la signature `F0 00 1A 00 9E 00 1A 00 1A 00 9E 00 1A 00 1A 00` à l'offset 42.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour limiter la signature de table des bordures aux six octets structurels communs, les valeurs suivantes décrivant des données de longueur variable.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour limiter `AtariPrintPowerBorderTable` à `F0 00 1A 00 9E 00`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne à l'index 805 jusqu'au prochain défaut ou à la fin du corpus.
    - `PrintPower` est validé avec ses cinq fontes, ses deux bibliothèques de bordures et ses sept bibliothèques graphiques reconnues. La campagne a validé les médias suivants jusqu'à l'index 832 et s'est arrêtée à l'index 833 sur `PATTERN1.USR` de `RAMbrandt Utilities`.
  - [x] Modifier les sous-dossiers antérieurs à `artifacts/media-audit/items/00000833-4dc17f7c21fc8914` afin de ne conserver que leur `report.json`.

- [x] Identifier `PATTERN1.USR` de `RAMbrandt Utilities`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec son rôle, ses critères fiables de reconnaissance et sa classification avant toute modification du catalogue.
    - Le manuel de RAMbrandt décrit cinq motifs utilisateur prédéfinis pouvant être redéfinis puis enregistrés sur une disquette DOS au moyen du module fourni.
    - `PATTERN1.USR` contient ces données de motifs : il commence par la table fixe `0B 00 00 03 00 06 00 09 00 0C`, suivie des données graphiques des motifs.
    - `.USR` ne sera pas classée seule, car cette extension peut également désigner des routines machine appelées par la fonction BASIC `USR`.
    - La combinaison de l'extension `.USR` et de l'en-tête complet classera ce format RAMbrandt dans `Image`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour ajouter la constante `.usr`.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` pour ajouter l'en-tête du jeu de motifs utilisateur RAMbrandt.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer cette combinaison dans `Image`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant la campagne à l'index 833 jusqu'au prochain défaut ou à la fin du corpus.
    - `PATTERN1.USR` est validé comme ressource graphique RAMbrandt. La campagne a validé les médias suivants jusqu'à l'index 847 et s'est arrêtée à l'index 848 sur `Rubber Stamp v1.0`, dont la somme des tailles logiques dépasse la capacité du média.
  - [x] Modifier les sous-dossiers antérieurs à `artifacts/media-audit/items/00000848-41bb787a8586180b` afin de ne conserver que leur `report.json`.

- [x] Mettre de côté les médias qui exigent une analyse ou une refonte ultérieure et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour reporter `Rubber Stamp v1.0` avec les anomalies observées.
    - Index 848 : `Rubber Stamp v1.0 (1985)(XLEnt Software)[cr Spiders].atr`.
    - Le lecteur Atari DOS extrait 27 fichiers mais totalise 98 764 octets logiques sur une capacité de 92 160 octets.
    - Les chaînes des fontes `CURSIVE1.FNT`, `ADVEN.FNT`, `ARCHAIC2.FNT`, `FANCY2.FNT`, `FANCY3.FNT`, `STANDARD.FNT`, `STYLISH.FNT` et de `DDOP.PIC` rejoignent des secteurs également attribués à `EDIT16.ASM`.
    - Ce média est reporté afin de reprendre ultérieurement l'analyse complète de ses chaînes Atari DOS sans modifier le lecteur pour ce seul cas.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 849 et supprimer `artifacts/media-audit/failure.json`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne jusqu'au prochain défaut ou à la fin du corpus.
    - La campagne a repris à l'index 849 et s'est arrêtée à l'index 859 sur `Screen Dump II`.

- [x] Identifier le fichier de paramètres d'imprimante `.PAR` de `Screen Dump II` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec le contenu observé et la classification retenue.
    - Index 859 : `DRUCKER.PAR` contient 64 octets de paramètres binaires pour l'imprimante `DELTA 15X`, avec plusieurs séquences de contrôle `ESC`.
    - Dans ce média Atari 8 bits, l'extension `.PAR` désigne un fichier de paramètres d'imprimante ; il est classé dans `Configuration` avec un aperçu hexadécimal.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer `.PAR` dans `Configuration` sur Atari 8 bits.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant à l'index 859 jusqu'au prochain défaut ou à la fin du corpus.
    - `Screen Dump II` est validé. La campagne s'est arrêtée à l'index 871 sur `Sesame Street Print Kit`.

- [x] Identifier les ressources `.004` et les configurations d'imprimante de `Sesame Street Print Kit`.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les groupes observés et leurs critères de reconnaissance.
    - Index 871 : les vingt fichiers `.004` sont les graphismes Sesame Street du kit d'impression.
    - `CITOH`, `IMAGEWRI`, `OKIMATE1`, `P321TOSH`, `QUIETJET`, `WIDENB24` et `WIDEQUIE` sont des configurations binaires d'imprimante de 291 octets sans extension.
    - Ces configurations partagent le bloc de contrôle fixe à l'offset 20 et le marqueur terminal à l'offset 274 ; leurs octets de paramètres variables ne sont pas utilisés comme signature.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs`, `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer les graphismes `.004` et les configurations d'imprimante sans extension.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant à l'index 871 jusqu'au prochain défaut ou à la fin du corpus.
    - `Sesame Street Print Kit` est validé. La campagne s'est arrêtée à l'index 890 sur `Softsynth`.

- [x] Identifier les morceaux reconnaissables de `Softsynth`, reporter ses données brutes et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les contenus observés et le cas différé.
    - Index 890 : vingt fichiers musicaux sans extension commencent par `53 59 4E 9B`, soit `SYN` suivi du séparateur ATASCII.
    - `BOOSTER` est une table brute de 256 octets sans extension ni en-tête distinctif. Sa suite de valeurs forme une courbe, mais elle ne fournit pas de signature générale suffisante pour déterminer seule son type ; ce fichier est reporté.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour classer les fichiers portant l'en-tête Softsynth dans `Audio`.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reporter `BOOSTER`, reprendre à l'index 891 et supprimer `artifacts/media-audit/failure.json`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne jusqu'au prochain défaut ou à la fin du corpus.
    - La campagne a repris à l'index 891 et s'est arrêtée à l'index 911 sur `SpartaDOS v1.1 HS [b]`.

- [x] Reporter l'incohérence de capacité de `SpartaDOS v1.1 HS [b]` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec l'erreur exacte observée à l'index 911 et la raison du report.
    - Le volume `UTILS1` annonce une capacité de 184 320 octets alors que sa plage dans l'ATR ne contient que 183 936 octets, soit un dépassement de 384 octets.
    - Le média est marqué `[b]` dans le corpus. Il est reporté comme image incohérente sans assouplir la validation générale ni modifier le lecteur pour l'accepter.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 912 et supprimer `artifacts/media-audit/failure.json`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne jusqu'au prochain défaut ou à la fin du corpus.
    - La campagne a repris à l'index 912 et s'est arrêtée à l'index 922 sur `SpartaDOS v3.2g [m APE]`.

- [x] Reporter l'incohérence de capacité de `SpartaDOS v3.2g [m APE]` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec l'erreur exacte observée à l'index 922 et son rattachement au contrôle de capacité ATR à reprendre.
    - La capacité annoncée est de 16 776 960 octets, tandis que la plage disponible dans l'ATR contient 16 776 576 octets : le dépassement est encore de 384 octets.
    - Cette image modifiée par APE est reportée avec `SpartaDOS v1.1 HS [b]` pour reprendre globalement le calcul de capacité des ATR concernés.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 923 et supprimer `artifacts/media-audit/failure.json`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant la campagne jusqu'au prochain défaut ou à la fin du corpus.
    - La campagne a repris à l'index 923 et s'est arrêtée à l'index 925 sur `SpartaDOS v3.3b [m OS Ram]` avec le même dépassement de 384 octets.

- [x] Recenser et passer les autres ATR présentant le même dépassement de capacité de 384 octets.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter chaque index, média, volume et capacité concernés.
    - Index 925 : `SpartaDOS v3.3b (199x)(-)[m OS Ram].atr`, volume `SPARTA`, capacité 184 320 octets dans une plage de 183 936 octets.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` et supprimer `artifacts/media-audit/failure.json` après chaque occurrence strictement identique afin de poursuivre au média suivant.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant jusqu'au prochain défaut d'une autre nature ou à la fin du corpus.
    - Aucun autre cas identique n'a été rencontré avant l'arrêt d'une autre nature à l'index 935 sur `Super 3D Plotter II`.

- [x] Reporter l'image brute sans extension `PICTURE` de `Super 3D Plotter II` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les données observées et la raison du report.
    - Index 935 : `PICTURE` contient 7 680 octets de données graphiques brutes, sans extension ni en-tête de format distinctif.
    - Index 936 : la version non crackée de `Super 3D Plotter II` contient le même fichier `PICTURE` et rejoint le même cas différé.
    - Index 967 : `Technicolor Dream (Side A)` contient un autre `PICTURE` graphique brut de 7 680 octets sans extension ni en-tête distinctif ; il rejoint ce cas différé.
    - Ce cas rejoint les autres images Atari 8 bits brutes sans signature déjà différées ; le nom et la longueur seuls ne sont pas employés pour inventer une reconnaissance.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 936 et supprimer `artifacts/media-audit/failure.json`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant jusqu'au prochain défaut ou à la fin du corpus.
    - La campagne a repris aux index 936 puis 968 après les occurrences identiques et s'est arrêtée à l'index 985 sur `The Trick - Cheat Maker`.

- [x] Reporter les fichiers propriétaires non identifiés de `The Trick - Cheat Maker` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les fichiers observés et la raison du report.
    - Index 985 : `DRED` (15 431 octets), `TRIC` (10 391 octets) et `HORROR.SP2` (15 488 octets) sont des données propriétaires du logiciel.
    - Aucun ne porte un en-tête Atari Binary Load ou une signature déjà connue. Leur reconnaissance demande l'analyse du format de données de `The Trick` ; le média est reporté sans règle fondée sur leurs noms ou leurs longueurs.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 986 et supprimer `artifacts/media-audit/failure.json`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant jusqu'au prochain défaut ou à la fin du corpus.
    - La campagne a repris à l'index 986 et s'est arrêtée à l'index 989 sur les modules `DOS.2` et `DOS.4` de Turbo DOS XE.

- [x] Identifier les modules système `DOS.2` et `DOS.4` de Turbo DOS XE et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec leur structure commune et la classification retenue.
    - Index 989 et 998 : les variantes de `DOS.2` et `DOS.4` commencent par l'en-tête stable `01 03 00 07 40 15 4C 16 07 02`, tandis que leur fin varie entre les versions.
    - Ces modules binaires de Turbo DOS XE sont classés dans `System` par cet en-tête ; les extensions générales `.2` et `.4` ne deviennent pas des règles globales.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/Atari8BitMediaContentRecognitionTable.cs` pour les reconnaître dans `System` sans employer leurs extensions numériques seules.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant à l'index 989 jusqu'au prochain défaut ou à la fin du corpus.
    - Les deux modules sont validés. La campagne s'est arrêtée à l'index 990 sur `Turbo Paint`.

- [x] Reporter l'image brute sans extension `S` de `Turbo Paint` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les données observées et la raison du report.
    - Index 990 : `S` contient 7 680 octets de données graphiques brutes, sans extension ni en-tête distinctif ; il rejoint les autres images Atari 8 bits brutes différées.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 991 et supprimer `artifacts/media-audit/failure.json`.

- [x] Reporter `Utilities for KMK IDE Interface [SDX]` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec toutes les anomalies observées à l'index 1018.
    - Le volume `IDE_KMK` annonce 368 640 octets dans une plage de 368 256 octets, soit le même dépassement ATR de 384 octets déjà recensé.
    - `LDRACOS.MAE`, `MASTER.LOG`, `MS_SL.LOG` et `SLAVE.LOG` restent des données propriétaires non identifiées. Leur extension seule ne permet pas de leur attribuer une catégorie sûre dans ce contexte.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 1019 et supprimer `artifacts/media-audit/failure.json`.

- [x] Reporter les écrans d'aide encodés de `Video 130XE` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les fichiers observés et la raison du report.
    - Index 1022 : `HELP.000`, puis `HELP.100` à `HELP.190`, contiennent des textes d'aide encodés en codes écran Atari après un en-tête variable.
    - Les extensions numériques ne définissent pas un type général et le catalogue ne possède pas encore de reconnaissance du texte en codes écran Atari ; ces onze fichiers sont reportés ensemble.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 1023 et supprimer `artifacts/media-audit/failure.json`.

- [x] Reporter les ressources propriétaires de `Visualiser` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec les fichiers observés et leur rattachement aux formats différés.
    - Index 1033 : `HELP.C10`, `HELP.C41` et `HELP.C91` sont des écrans d'aide en codes écran Atari ; ils rejoignent les écrans d'aide de `Video 130XE`.
    - `DISK` et `MASTER` sont des blocs graphiques propriétaires sans extension ni en-tête général identifiable ; ils rejoignent les images brutes différées.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 1034 et supprimer `artifacts/media-audit/failure.json`.

- [x] Identifier les fichiers de dictionnaire `.DIC` et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec le fichier observé et la classification retenue.
    - Index 1044 : `MASTER.DIC` contient les données du dictionnaire de `The Writer's Tool` ; `.DIC` est classé comme fichier de données commun.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` pour classer `.DIC` dans `Data`.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en reprenant à l'index 1044 jusqu'au prochain défaut ou à la fin du corpus.
    - `MASTER.DIC` est validé. La campagne s'est arrêtée à l'index 1066 sur un ATX incomplet.

- [x] Recenser et passer les ATX auxquels il manque des blocs logiques.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter chaque index, média et nombre de blocs manquants.
    - Index 1066 : `Atari Macro Assembler v1.0A (1981)(Atari)(US).atx`, 1 bloc manquant sur 720.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` et supprimer `artifacts/media-audit/failure.json` après chaque erreur strictement limitée à des blocs logiques manquants.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant jusqu'au prochain défaut d'une autre nature ou à la fin du corpus.
    - La campagne s'est arrêtée à l'index 1068 sur un ATX contenant des blocs à intégrité invalide.

- [x] Recenser et passer les ATX contenant des blocs à intégrité invalide.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter chaque index, média et nombre de blocs invalides.
    - Index 1068 : `The Music Studio`, 2 blocs invalides.
    - Index 1069 : `SynCalc (1983)`, 1 bloc invalide.
    - Index 1070 : `SynCalc (1985)`, 1 bloc invalide.
    - Index 1071 : `SynChron`, 1 bloc invalide.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` et supprimer `artifacts/media-audit/failure.json` après chaque erreur strictement limitée à des blocs à intégrité invalide.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant jusqu'au prochain défaut d'une autre nature ou à la fin du corpus.
    - La campagne s'est arrêtée à l'index 1072 sur un ATX auquel il manque 558 blocs logiques.

- [x] Poursuivre le recensement des deux anomalies physiques ATX déjà identifiées.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter les occurrences suivantes de blocs absents ou invalides.
    - Index 1072 : `Text Wizard v1.3 (Side A)[OS-B]`, 558 blocs manquants sur 720.
    - Index 1073 : `Text Wizard v1.3 (Side B)[OS-B]`, 612 blocs manquants sur 720.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` et supprimer `artifacts/media-audit/failure.json` après chaque occurrence limitée à l'une de ces deux erreurs.
  - [x] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant jusqu'au prochain défaut d'une autre nature ou à la fin du corpus.
    - La campagne s'est arrêtée à l'index 1074 sur une cassette reconnue sans fichier nommé extractible.

- [x] Reporter la cassette `Adaxbaud [b]` sans fichier nommé et poursuivre la campagne.
  - [x] Modifier `docs/tasks/media-corpus-audit.md` avec le résultat exact du lecteur séquentiel.
    - Index 1074 : le volume `Adax` est reconnu comme `sequential-content`, avec une capacité de 62 040 octets et un bloc décodé dépourvu de nom stocké.
    - Le lecteur signale `No named file is available for exploration`; aucun fichier nommé ne peut donc être présenté par l'explorateur. Ce cas est reporté sans créer de nom synthétique.
  - [x] Modifier `artifacts/media-audit/checkpoint.json` pour reprendre à l'index 1075 et supprimer `artifacts/media-audit/failure.json`.

- [ ] Recenser et passer les autres cassettes reconnues sans fichier nommé extractible.
  - [ ] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter chaque index, média, volume, capacité et avertissement du lecteur.
  - [ ] Modifier `artifacts/media-audit/checkpoint.json` et supprimer `artifacts/media-audit/failure.json` après chaque erreur strictement limitée à l'absence de fichier extrait.
  - [ ] Modifier `artifacts/media-audit/items` et `artifacts/media-audit/checkpoint.json` en poursuivant jusqu'au prochain défaut d'une autre nature ou à la fin du corpus.

La campagne est terminée lorsque le script atteint la fin du corpus sans erreur et que `artifacts/media-audit/checkpoint.json` contient l'état `complete`.
