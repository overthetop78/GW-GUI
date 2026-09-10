# Images HDD — travail restant

Le catalogue durable des formats et l’état déjà réalisé se trouvent dans
[`../project/hard-disk-format-catalog.md`](../project/hard-disk-format-catalog.md). Cette feuille ne
conserve que les extensions encore ouvertes.

- [ ] 1. Compléter la composition des images
  - [ ] 1.1 Décrire les capacités intrinsèques
    - [ ] Modifier les descripteurs HDD dans le code de production puis `docs/project/hard-disk-format-catalog.md` pour exposer les paramètres, limites, tailles de secteurs et géométries réellement acceptés par chaque format.
  - [ ] 1.2 Migrer les anciens adaptateurs utiles
    - [ ] Modifier les adaptateurs qui doivent proposer de nouvelles combinaisons afin qu’ils utilisent la composition explicite, puis consigner les combinaisons vérifiées dans `docs/project/hard-disk-format-catalog.md`.

- [ ] 2. Étendre les formats et variantes
  - [ ] 2.1 Étudier chaque format absent avant intégration
    - [ ] Modifier `docs/project/hard-disk-format-catalog.md` avec les bibliothèques disponibles, leurs licences et leurs capacités réelles de création, lecture et écriture avant d’ajouter un constructeur ou un formateur.
  - [ ] 2.2 Compléter les variantes historiques connues
    - [ ] Modifier les constructeurs concernés puis `docs/project/hard-disk-format-catalog.md` après vérification des autres variantes Pascal de répertoire et d’ordre des octets et des autres dispositions EBR historiques.

- [ ] 3. Compléter le cycle de vie des ensembles d’images
  - [ ] 3.1 Résoudre les dépendances restantes
    - [ ] Modifier le graphe de dépendances HDD puis `docs/project/hard-disk-format-catalog.md` après prise en charge des parents identifiés par hash ou UUID, des ensembles segmentés et des organisations supplémentaires.
  - [ ] 3.2 Protéger le retrait et les conversions
    - [ ] Modifier les services de suppression et de conversion puis `docs/project/hard-disk-format-catalog.md` après vérification de la suppression collective contrôlée et de la préservation des originaux lors d’une interruption.

- [ ] 4. Valider les nouvelles variantes
  - [ ] 4.1 Vérifier chaque ajout sur des données simulées
    - [ ] Modifier `docs/project/testing.md` et `docs/project/hard-disk-format-catalog.md` après réouverture indépendante, contrôle des structures, allocations, checksums, limites et combinaisons invalides sans produire d’image réelle persistante.
