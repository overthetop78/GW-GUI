# Documentation et usage des scripts — tâches

- [x] 1. Documenter l’ensemble du dossier `scripts`
  - [x] 1.1 Créer une référence commune exhaustive
    - [x] 1.1.1 Décrire chaque script et son utilisation
      - [x] Créer `docs/project/scripts.md` avec les 15 scripts, leur rôle, leur commande, leurs paramètres, leurs prérequis, leurs sorties, leurs effets et leurs appelants.
    - [x] 1.1.2 Rendre la référence accessible
      - [x] Ajouter dans `README.md` un lien vers `docs/project/scripts.md`.
  - [x] 1.2 Examiner les scripts de test
    - [x] 1.2.1 Conserver uniquement les scripts de test utiles
      - [x] Documenter dans `docs/project/scripts.md` l’usage automatisé ou manuel de chaque script de test et confirmer qu’aucun ne doit être supprimé.
  - [x] 1.3 Vérifier la documentation
    - [x] 1.3.1 Contrôler la couverture de l’inventaire
      - [x] Comparer les fichiers de `scripts` à `docs/project/scripts.md` et corriger tout nom manquant.
    - [x] 1.3.2 Contrôler les modifications Markdown
      - [x] Exécuter `git diff --check` et corriger toute erreur de format détectée.
