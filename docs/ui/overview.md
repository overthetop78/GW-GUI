# Interface — vue générale

## Spécification visuelle validée

### Fenêtre principale

- Taille minimale : 1280×720.
- Restaurer taille, position, écran, maximisation et thème; recentrer si l’écran précédent a disparu.
- Thèmes Système, Clair et Sombre; accent Windows.
- Menus Options et Aide.
- Onglets Lecture, Écriture, Conversion, Visualisation, Explorateur et Outils.
- Bouton Exécuter/Arrêter stable en bas à droite de l’onglet.

### Console et état

- Console inférieure réunissant commande et logs.
- Redimensionnable et réductible; restaurer hauteur et état de la dernière session.
- Premier lancement : ouverte à une hauteur raisonnable.
- Ne pas forcer sa réouverture pendant une commande.
- Commande en première ligne, lecture seule, copiable; arguments experts séparés.
- Barre d’état sous la console : diode, port COM, lecteur, profil et progression par face.
- Progressions masquées hors opération.

### Lecture

- Parcours vertical : Profil, Image à créer, Nom/extension, Dossier, Paramètres avancés, Exécuter.
- Deux choix visibles : Image brute SCP ou Disquette au format connu.
- Format connu : Famille, Format/géométrie et Type d’image compatible.
- Paramètres avancés en accordéons indépendants pouvant rester ouverts simultanément.
- Groupes : Pistes et faces; Lecture et récupération; Rotation et index; Traitement du signal; Matériel spécialisé.

### Écriture

- Structure cohérente avec Lecture.
- Source puis résumé du format détecté avec bouton Modifier.
- Les listes de format s’ouvrent sur demande ou automatiquement si ambigu.
- Cinq groupes avancés : Pistes/faces; Préparation/vérification; Rotation/index; Écriture du signal; Matériel spécialisé.
- Confirmation simple par résumé et boutons Annuler/Écrire.

### Conversion

- Source, type détecté, nom et case Tags restent fixes.
- Zone centrale défilante classée par machine/famille.
- Section Sélections épinglée en haut.
- Formats courants visibles; formats rares derrière une extension de liste.
- Exécuter et Paramètres avancés restent fixes.
- Profil mémorise sorties et options; pas la source, le nom, le dossier ou les tags temporaires.
- Aucun résumé avant lancement sauf conflit de fichier.

### Visualisateur et Explorateur

- Barre supérieure : image, ouverture, détection automatique/manuelle, machine, format, protection, lien des vues et réinitialisation.
- Le Visualisateur accepte les images prises en charge, pas uniquement SCP.
- Deux faces côte à côte lorsque le média possède deux faces ; une face unique est centrée et agrandie.
- Inspecteur flottant masquable pour face, piste, révolution, encodage, secteurs et anomalies.
- Zoom et déplacement, avec liaison facultative des faces.
- Légende et barres Face 0/Face 1 visibles.
- L’Explorateur partage l’image chargée et conserve ses trois colonnes : dossiers, contenu et informations.

### Options et dialogues

- Options persistantes dans une fenêtre à onglets horizontaux.
- Diagnostics et Matériel restent des dialogues ponctuels.
- Chaque dialogue montre résumé lisible, sortie brute repliable et ligne de commande en bas.
- Sélecteur de lecteur dans chaque opération seulement si plusieurs lecteurs sont configurés.

## Fenêtre principale et navigation

### Intention générale — Validé

L’ancienne fenêtre GreaseweazleGUI est jugée trop chargée, confuse et difficile à parcourir. La nouvelle application doit être moderne mais la beauté n’est pas l’objectif principal : elle doit surtout être claire, aérée, intuitive et rapide à utiliser.

La fenêtre principale n’utilise pas la page d’accueil de l’ancien GUI avec une liste de boutons radio pour sélectionner une action. Les opérations fréquentes sont accessibles par onglets.

### Onglets principaux connus — Validé

- **Lecture** : créer une image depuis une disquette.
- **Écriture** : écrire une image sur une disquette.
- **Conversion** : convertir une ou plusieurs représentations d’une image.
- **Visualisation** : analyser visuellement une image de disquette prise en charge.
- **Explorateur** : charger une image ou lire temporairement une disquette afin d’afficher son nom de volume, ses dossiers et ses fichiers lorsque son format et son système de fichiers peuvent être interprétés.
- **Outils** : uniquement les actions de maintenance retenues dans la fenêtre principale, actuellement Effacer et Nettoyer les têtes.

La navigation comprend désormais ce sixième onglet demandé. Les diagnostics et contrôles matériels ne deviennent pas des onglets.

### Menu Options — Validé

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

### Barre d’état globale — Validé

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

### Commande et journaux — Validé

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

### Exécution — Validé

- Gros bouton principal localisé `Exécuter` / `Execute`.
- Pendant l’opération, ce bouton devient `Arrêter` / `Stop`.
- Arrêter ouvre une confirmation avant d’interrompre la commande.
- À la fin, le bouton redevient Exécuter.
- Le bouton Back de l’ancien GUI disparaît : les onglets assurent la navigation.
- L’interface reste réactive pendant l’exécution.
