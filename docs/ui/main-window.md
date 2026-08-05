# Fenêtre principale et navigation

## Intention générale — Validé

L’ancienne fenêtre GreaseweazleGUI est jugée trop chargée, confuse et difficile à parcourir. La nouvelle application doit être moderne mais la beauté n’est pas l’objectif principal : elle doit surtout être claire, aérée, intuitive et rapide à utiliser.

La fenêtre principale n’utilise pas la page d’accueil de l’ancien GUI avec une liste de boutons radio pour sélectionner une action. Les opérations fréquentes sont accessibles par onglets.

## Onglets principaux connus — Validé

- **Lecture** : créer une image depuis une disquette.
- **Écriture** : écrire une image sur une disquette.
- **Conversion** : convertir une ou plusieurs représentations d’une image.
- **Visualisation** : analyser visuellement un fichier SCP.
- **Outils** : uniquement les actions de maintenance retenues dans la fenêtre principale, actuellement Effacer et Nettoyer les têtes.

La liste finale des onglets sera confirmée après l’étude de toutes les opérations. Les diagnostics et contrôles matériels ne deviennent pas des onglets.

## Menu Options — Validé

Le menu Options ouvre des boîtes de dialogue. Il ne remplace pas les onglets d’opérations.

Le menu est accessible au clavier par `Alt+O`. En français, le menu Aide utilise `Alt+A`; les marqueurs sont fournis par les ressources de langue et non écrits directement dans la vue.

- **Options → Diagnostics**
  - Informations du contrôleur (`gw info`).
  - Bande passante USB (`gw bandwidth`).
  - Mesure RPM (`gw rpm`).
  - Déplacement de la tête vers un cylindre (`gw seek`).
- **Options → Matériel**
  - État/commande des broches (`gw pin`).
  - Réinitialisation du contrôleur (`gw reset`).
  - Temporisations du contrôleur/lecteur (`gw delays`).
- **Options → Matériel → Firmware (`gw update`)**.

## Barre d’état globale — Validé

Une barre d’état peut afficher sans encombrer les onglets :

- le port COM utilisé;
- le lecteur actif lorsqu’un choix existe;
- le profil actif de l’onglet courant;
- une diode d’état;
- une infobulle sur la diode expliquant l’état;
- états envisagés : attente/prêt, lecture, écriture, erreur, terminé, interrompu;
- deux barres de progression correspondant aux pistes/cylindres de la face 0 et de la face 1;
- une seule barre pour une opération simple face;
- plages adaptées à la commande réelle plutôt qu’un `0–79` figé;
- barres masquées lorsqu’aucune opération n’est en cours.

## Commande et journaux — Validé

Validé :

- La ligne de commande `gw` générée doit être visible et copiable.
- Présentation envisagée sur fond noir avec texte clair ou vert.
- Le curseur doit rester visible lorsque le contrôle possède le focus.
- La commande générée est en lecture seule.
- Un champ expert permet d’ajouter des arguments libres non couverts par l’interface.
- Les journaux de `gw` doivent être visibles dans l’application et aucune fenêtre DOS ne doit apparaître.
- Le panneau peut être masqué.

- Panneau inférieur réunissant commande et logs.
- Panneau redimensionnable et réductible.
- Restaurer hauteur et état précédent; premier lancement ouvert.
- Ne pas le rouvrir automatiquement si l’utilisateur l’a réduit.

## Exécution — Validé

- Gros bouton principal localisé `Exécuter` / `Execute`.
- Pendant l’opération, ce bouton devient `Arrêter` / `Stop`.
- Arrêter ouvre une confirmation avant d’interrompre la commande.
- À la fin, le bouton redevient Exécuter.
- Le bouton Back de l’ancien GUI disparaît : les onglets assurent la navigation.
- L’interface reste réactive pendant l’exécution.
