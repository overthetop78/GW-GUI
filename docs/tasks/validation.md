# Validation finale du corpus et du matériel

Cette phase est reportée. Les parcours courants étaient utilisables lors des validations historiques,
mais l’ensemble du corpus et du matériel devra être revérifié avant de déclarer leur couverture
complète. Elle commence seulement lorsque toutes les cases de `media-format-orchestration.md`,
`media-exploration.md` et `hard-disk-images.md` sont cochées et que le troisième commit demandé à la
fin de ce chantier est créé.

- [ ] 1. Valider le corpus `image_test`
  - [ ] 1.1 Définir les cas locaux à partir des images réellement disponibles
    - [ ] Modifier `docs/tasks/validation.md` après inventaire de `image_test` pour ajouter, à la suite de cette case et avant les essais, une action distincte avec le chemin exact de chaque fichier de test local à créer ou modifier et le résultat attendu pour sa reconnaissance, sa visualisation et son exploration.
    - [ ] Modifier `tests/GWGUI.LocalDiskImageTests/GWGUI.LocalDiskImageTests.csproj` pour inclure uniquement les tests locaux ajoutés par l’action précédente et conserver ce projet hors de la solution principale ainsi que des workflows de publication.
  - [ ] 1.2 Parcourir les images une par une
    - [ ] Modifier `docs/reference/media-formats.md` après chaque famille testée pour consigner le conteneur, la détection, la géométrie, le système de fichiers, le décodage, l’encodage, les conversions, la Lecture, l’Écriture, le Visualisateur, l’Explorateur, les traductions, les performances et les erreurs réellement vérifiés.
    - [ ] Modifier `docs/project/testing.md` avec les commandes et résultats des essais locaux sur `image_test`, séparés des tests généraux de `GWGUI.Tests` et des contrôles exécutés pendant une publication.
  - [ ] 1.3 Classer les images validées
    - [ ] Modifier `docs/tasks/validation.md` après les essais pour remplacer cette action par une action distincte indiquant le chemin source et le chemin exact sous `image_test/validated_images/` de chaque image confirmée, puis exécuter ces actions une par une dans leur nouvel ordre.
    - [ ] Modifier `docs/reference/media-formats.md` avec le classement final de chaque image déplacée et le statut des images conservées hors de `validated_images`.

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
