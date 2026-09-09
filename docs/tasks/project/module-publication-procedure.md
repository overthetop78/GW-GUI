# Procédure de publication des modules — tâches

- [x] 1. Définir la méthode de publication d’un module
  - [x] 1.1 Ajouter la règle permanente
    - [x] 1.1.1 Inscrire la procédure dans la configuration Codex
      - [x] Modifier `.codex/config.toml` pour imposer la version de `module.json`, les notes propres au module, le commit et le push, puis la création et le push du tag de module.
  - [x] 1.2 Aligner la documentation du projet
    - [x] 1.2.1 Retirer le contrôle manuel de la procédure obligatoire
      - [x] Modifier `docs/project/release.md` pour publier directement par le tag après le commit et le push, sans exiger une exécution manuelle préalable du workflow.
  - [x] 1.3 Vérifier la cohérence des deux procédures
    - [x] 1.3.1 Contrôler les étapes documentées
      - [x] Vérifier que `.codex/config.toml` et `docs/project/release.md` décrivent le même ordre et exécuter `git diff --check`.
