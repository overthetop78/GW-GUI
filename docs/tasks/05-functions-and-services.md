# 5 — Fonctions et services

- [ ] Relever les fonctions trop longues ou qui réalisent plusieurs opérations indépendantes.
- [ ] Extraire les fonctions communes dans le service propriétaire de la responsabilité, pas dans un fourre-tout `Helpers`.
- [ ] Séparer parsing, validation, transformation, sélection et présentation.
- [ ] Regrouper dans un même fichier les petites fonctions qui constituent une seule primitive cohérente.
- [ ] Isoler les accès disque, processus, réglages, journaux et dialogues derrière les services existants ou des contrats justifiés.
- [ ] Réutiliser le même résultat d’analyse entre Explorateur et Visualisateur.
- [ ] Annuler immédiatement l’analyse précédente lorsqu’une nouvelle image est chargée.
- [ ] Vérifier que chaque service a un propriétaire, une responsabilité et des tests ciblés.
