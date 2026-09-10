# Validation finale du corpus et du matériel

Cette phase est reportée. Les parcours courants étaient utilisables lors des validations historiques,
mais l’ensemble du corpus et du matériel devra être revérifié avant de déclarer leur couverture
complète.

- [ ] 1. Valider le corpus `image_test`
  - [ ] 1.1 Parcourir les images une par une
    - [ ] Modifier `docs/reference/media-formats.md` après chaque famille testée pour consigner le conteneur, la détection, la géométrie, le système de fichiers, le décodage, l’encodage, les conversions, la Lecture, l’Écriture, le Visualisateur, l’Explorateur, les traductions, les performances et les erreurs réellement vérifiés.
  - [ ] 1.2 Classer les images validées
    - [ ] Déplacer chaque image confirmée vers `image_test/validated_images/<marque>/<modèle>/<type de disquette>/`, retirer les fichiers parasites du dossier source et mettre à jour `docs/reference/media-formats.md` avec son statut final.

- [ ] 2. Refaire les essais avec Greaseweazle
  - [ ] 2.1 Valider les opérations physiques disponibles
    - [ ] Modifier `docs/project/testing.md` après les essais réels de Lecture, Écriture sur support sacrifiable, relecture, conversion, Visualisateur, Explorateur et Effacement pour consigner le matériel, les médias et les résultats.
  - [ ] 2.2 Valider plusieurs contrôleurs lorsque le matériel existe
    - [ ] Modifier `docs/project/testing.md` après un essai avec plusieurs Greaseweazle et lecteurs pour consigner leur détection, leur sélection et les commandes produites.

- [ ] 3. Valider progressivement les entrées et sorties physiques internes
  - [ ] 3.1 Raccorder et vérifier l’Écriture interne
    - [ ] Modifier le service de l’onglet Écriture derrière une option explicite, puis modifier `docs/reference/media-formats.md` après essais Amiga, Atari ST, IBM, MSX, Apple, Commodore, Acorn/BBC, Amstrad, Epson et DEC avant de retirer le repli `gw.exe` d’une famille.
  - [ ] 3.2 Raccorder et vérifier la Lecture interne
    - [ ] Modifier le service de l’onglet Lecture derrière une option explicite, puis modifier `docs/reference/media-formats.md` après vérification du checksum SCP, des révolutions, pistes, décodages, annulations et reprises avant de retirer le repli `gw.exe`.
