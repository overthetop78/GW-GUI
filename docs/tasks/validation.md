# Validation finale du corpus et du matériel

Cette phase commence après les trois commits du chantier média. Elle valide d'abord les images
réellement disponibles, puis le matériel. Les actions sont exécutées et cochées une par une, dans
l'ordre. Toute correction nécessaire découverte pendant un essai est ajoutée après la dernière
action cochée et avant l'essai qui en dépend.

Le corpus disponible le 11 septembre 2026 se trouve dans `F:\Rétro\image_test`. Il contient
1 496 fichiers, dont 153 images déjà rangées sous `validated_images`. Les formats d'images présents
sont 2MG, 86F, ADF, ADL, ATR, BIN, CP2, D64, D81, DC42, DO, DSD, DSK, EDSK, IMA, IMAGE, IMD, IMG,
JFD, MSA, NIB, PO, SCP, SSD, ST, TD0 et WOZ. Les fichiers CFG, ROM, SIT et ZIP sont des fichiers
associés ou des archives et ne sont pas audités seuls. Ce corpus ne contient actuellement aucune
image HDD, CD/DVD/optique ou cassette/bande.

`GWGUI.LocalDiskImageTests` est déjà absent de `GWGUI.sln` et des workflows GitHub. Son projet SDK
inclut déjà automatiquement ses fichiers `.cs`; aucune modification du `.csproj` n'est nécessaire
pour ajouter le test local prévu ci-dessous.

L'audit de `F:\Rétro\image_test` sera lancé manuellement par l'utilisateur, après la préparation et
la compilation du projet `GWGUI.LocalDiskImageTests`. Il ne fait pas partie de l'exécution de cette
feuille par Codex, des tests automatiques ni d'une release. Les résultats produits localement seront
ensuite utilisés pour reprendre les actions de documentation et de classement ci-dessous.

- [ ] 1. Valider le corpus `image_test`
  - [x] 1.1 Définir les cas locaux à partir des images réellement disponibles
    - [x] Modifier `docs/tasks/validation.md` avec l'inventaire de `F:\Rétro\image_test`, le fichier de test temporaire exact, les résultats attendus pour la reconnaissance, la visualisation et l'exploration, puis les actions de suppression de ce test et de son rapport.
    - [x] Créer `tests/GWGUI.LocalDiskImageTests/MediaCorpusAuditTests.cs` pour résoudre le corpus par `GWGUI_IMAGE_TEST_ROOT` puis `F:\Rétro\image_test`, auditer chaque image candidate sans modifier la source et produire `build/validation/media-corpus-audit.json`; chaque image déjà classée doit être reconnue, fournir la représentation attendue à un fournisseur de visualisation et terminer son exploration avec une arborescence ou un diagnostic explicite, tandis que chaque image non classée conserve dans le rapport son résultat reconnu, non pris en charge ou en erreur.
  - [x] 1.2 Réserver les validations dépendantes de ressources externes à une exécution manuelle locale
    - [x] Modifier `docs/project/testing.md` pour indiquer que tout test dépendant d'une image locale, d'un matériel, d'une application ou d'une DLL externe est exécuté manuellement par le développeur, reste hors de la solution et ne doit jamais être inclus dans les workflows, les contrôles automatiques ou la fabrication d'une release.
    - [x] Modifier `docs/tasks/validation.md` pour identifier explicitement l'audit de `F:\Rétro\image_test` comme une validation manuelle laissée à l'utilisateur et placer son exécution après la préparation et la compilation du projet de tests locaux.
  - [x] 1.3 Adapter les tests locaux aux signatures actuelles du chantier média
    - [x] Modifier `tests/GWGUI.LocalDiskImageTests/AppleDiskImageTests.cs` pour fournir à `ConversionBatchExecutor` les services de lecture et de conversion issus de la composition MediaEngine actuelle, tout en conservant la vérification de la conversion Apple II interne sans appel à Greaseweazle.
    - [x] Modifier `tests/GWGUI.LocalDiskImageTests/RealScpCorpusTests.cs` pour construire `ScpRenderRequest` avec les paramètres actuels, dont le libellé de face et la catégorie de disquette, tout en conservant la vérification du rendu SCP des deux faces.
  - [ ] 1.4 Faire exécuter et exploiter manuellement l'audit local avant tout déplacement
    - [ ] Modifier `docs/project/testing.md` avec la commande ciblant `MediaCorpusAuditTests`, la racine utilisée, le nombre d'images examinées et les totaux de reconnaissance, visualisation, exploration, formats non pris en charge et erreurs lus dans `build/validation/media-corpus-audit.json`.
    - [ ] Modifier `docs/reference/media-formats.md` avec les résultats réels regroupés par format et famille de machines, en distinguant reconnaissance, représentation Flux ou Sectors, Visualisateur, système de fichiers exploré, absence normale de système de fichiers et erreur à corriger.
    - [ ] Modifier `docs/tasks/validation.md` avant toute correction découverte pour ajouter, après la dernière action cochée et avant la reprise de l'audit, une action concrète par fichier de code, traduction ou test à modifier avec le défaut exact observé.
  - [ ] 1.5 Classer seulement les images réellement validées
    - [ ] Modifier `docs/tasks/validation.md` à partir du rapport final pour remplacer cette action par une action distincte indiquant le chemin source et le chemin exact sous `F:\Rétro\image_test\validated_images\` de chaque image reconnue, visualisée et explorée qui doit être déplacée.
    - [ ] Modifier `docs/reference/media-formats.md` avec le classement final de chaque image déplacée et le statut des images conservées hors de `validated_images`.
  - [ ] 1.6 Supprimer le test et le rapport temporaires après exploitation
    - [ ] Supprimer `tests/GWGUI.LocalDiskImageTests/MediaCorpusAuditTests.cs` après consignation de tous les résultats et déplacements décidés.
    - [ ] Supprimer `build/validation/media-corpus-audit.json` après consignation de tous les résultats et déplacements décidés.
    - [ ] Modifier `docs/project/testing.md` pour indiquer que l'audit temporaire et son rapport ont été retirés après validation et que `GWGUI.LocalDiskImageTests` reste hors de la solution et des publications.

- [ ] 2. Refaire les essais avec Greaseweazle
  - [ ] 2.1 Valider les opérations physiques disponibles
    - [ ] Modifier `docs/project/testing.md` après les essais réels de Lecture, Écriture sur support sacrifiable, relecture, conversion, Visualisateur, Explorateur et Effacement pour consigner le matériel, les médias et les résultats.
  - [ ] 2.2 Valider plusieurs contrôleurs lorsque le matériel existe
    - [ ] Modifier `docs/project/testing.md` après un essai avec plusieurs Greaseweazle et lecteurs pour consigner leur détection, leur sélection et les commandes produites.

- [ ] 3. Valider progressivement les entrées et sorties physiques internes
  - [ ] 3.1 Raccorder et vérifier l’Écriture interne
    - [ ] Modifier `src/GWGUI.App/Services/PhysicalDiskWriting/PhysicalDiskWriteService.cs` pour placer l’Écriture interne derrière l’option explicite existante et conserver le repli `gw.exe` pour toute famille qui n’a pas encore été validée manuellement.
    - [ ] Modifier `docs/reference/media-formats.md` après les essais Amiga, Atari ST, IBM, MSX, Apple, Commodore, Acorn/BBC, Amstrad, Epson et DEC pour indiquer famille par famille si le repli `gw.exe` peut être retiré.
  - [ ] 3.2 Raccorder et vérifier la Lecture interne
    - [ ] Modifier `src/GWGUI.App/Services/PhysicalDiskReading/PhysicalDiskReadService.cs` pour placer la Lecture interne derrière l’option explicite existante et conserver le repli `gw.exe` tant que les validations manuelles ne sont pas terminées.
    - [ ] Modifier `docs/reference/media-formats.md` après vérification du checksum SCP, des révolutions, pistes, décodages, annulations et reprises pour indiquer précisément les cas où le repli `gw.exe` peut être retiré.
