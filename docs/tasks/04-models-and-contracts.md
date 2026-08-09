# 4 — Enums, modèles de données et contrats

## 4.1 Modèles de données

- [ ] Créer des DTO ou records distincts pour machine, format, géométrie, média, codec, protection, conteneur et système de fichiers.
- [ ] Créer des modèles distincts pour piste physique, secteur décodé, intégrité, révolution et avertissement.
- [ ] Séparer les modèles publics partagés des structures internes propres à un algorithme.
- [ ] Ajouter aux modèles les métadonnées nécessaires aux images multiformats et protégées.
- [ ] Définir les invariants et validations à la construction des données.

## 4.2 Enums et identifiants extensibles

- [ ] Utiliser des enums pour les ensembles fermés et stables : état d’intégrité, type de média, face, état d’opération et capacité.
- [ ] Utiliser des identifiants catalogués pour les formats, protections et définitions extensibles.
- [ ] Vérifier que les formats provenant des `diskdefs` et autres définitions extensibles restent catalogués dynamiquement.

## 4.3 Interfaces

- [ ] Définir des contrats distincts pour lecture/écriture de conteneur, décodage/encodage de piste, reconstruction sectorielle, système de fichiers, détection, visualisation et conversion.
- [ ] Justifier chaque nouvelle interface par une frontière réelle ou par des implémentations interchangeables.
- [ ] Vérifier les dépendances autorisées entre projets.
