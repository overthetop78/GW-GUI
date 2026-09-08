# Noms des machines dans le Gestionnaire des tâches

- [x] 1. Établir la différence observée entre Amiga et Atari
  - [x] 1.1. Produire un diagnostic fondé sur les processus et les fenêtres
    - [x] Créer `scripts/temporary-inspect-emulation-processes.ps1` pour relever en lecture seule les commandes, PID, descriptions et fenêtres des processus GW GUI ; exécuter le relevé avec Amiga et Atari démarrés.
    - [x] Compléter `scripts/temporary-inspect-emulation-processes.ps1` pour lire les lignes GW GUI du Gestionnaire des tâches par UI Automation, sans modifier ses réglages ; comparer ces lignes aux processus. Limite observée : Windows expose seulement huit éléments sans le contenu de la liste.
    - [x] Ajouter au script temporaire une capture limitée à la fenêtre du Gestionnaire des tâches dans `build/temporary-taskmanager.png`, examiner la liste affichée, puis supprimer cette image avec le script.
    - [x] Créer `docs/architecture/emulation-process-display.md` avec les observations, les chemins du code de lancement et la faisabilité du nom réel des machines dans la liste Windows ; distinguer les faits vérifiés et les limites.
    - [x] Supprimer `scripts/temporary-inspect-emulation-processes.ps1` après le diagnostic.
- [x] 2. Appliquer une adaptation seulement si elle reste simple
  - [x] 2.1. Définir les actions selon le diagnostic
    - [x] Compléter cette liste avec les fichiers exacts à modifier et les vérifications utiles si une solution simple est établie ; sinon consigner pourquoi cette adaptation reste différée dans `docs/architecture/emulation-process-display.md`.

Aucun commit ni push. Ne pas modifier les moteurs pour contourner le classement Windows.
Les noms propres des machines ne demandent pas de nouvelles traductions.

Résultat : diagnostic consigné ; personnalisation des noms différée, aucune modification de production. Script et capture temporaires supprimés.
