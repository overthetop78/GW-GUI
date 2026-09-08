# Atari800 — corrections de la gestion des cassettes

## Constats confirmés

- L’Atari 400/800 prend bien en charge le lecteur de cassette dans le cœur Atari800.
- La cassette `Spindizzy.cas` est reconnue et lue par le cœur.
- Sans action utilisateur, la lecture ne commence pas et l’indicateur `TAPE0` reste inactif.
- En appuyant sur `Entrée`, la lecture commence et l’indicateur `TAPE0` signale correctement l’activité.
- Une vérification manuelle ponctuelle a relevé `0` trame active sans `Entrée`, puis `1376` trames actives avec `Entrée`.
- Le cœur considère le bouton Play du magnétophone comme maintenu en permanence ; la bande avance lorsque la machine Atari commande son moteur.
- Le test manuel confirme qu’une impulsion `RETURN`, envoyée après l’apparition de l’écran d’attente, déclenche la lecture : `TAPE0` passe à « Lecture » et le compteur progresse jusqu’à la fin de la bande.
- Le cœur fournit l’option `atari800_cassboot` pour maintenir `START` pendant l’amorçage. GW GUI la désactivait à tort sur les Atari 400/800 et tentait de la remplacer par une séquence de touches artificielle.
- Avec `Spindizzy.cas`, `atari800_sioaccel` désactivé provoque `BOOT ERROR` après la lecture complète ; GW GUI le maintient donc actif en interne.
- Une vérification temporaire avec le cœur Atari800 a ouvert directement `Bounty Bob Strikes Back!.car` depuis `F:\Rétro`, sans copie, puis a produit 600 trames en 384 × 240. Le test temporaire et ses sorties ont ensuite été supprimés.

## Modifications à réaliser

### ATA-CAS-001 — Appliquer immédiatement les options Atari800

- [x] Enregistrer une modification dès que l’utilisateur change une option, sans attendre la fermeture de la fenêtre Options.
- [x] Mettre à jour la configuration en mémoire même si la fenêtre Options reste ouverte.
- [x] Si l’émulateur est arrêté, employer les nouvelles valeurs au prochain démarrage sans imposer de fermer puis rouvrir la fenêtre Options.
- [x] Si l’émulateur fonctionne et que l’option est modifiable à chaud par le cœur, transmettre immédiatement la nouvelle valeur au cœur.
- [x] Si une option exige réellement un redémarrage du cœur, l’indiquer explicitement dans l’interface et garantir son application au redémarrage suivant.
- [x] Remplacer, dans chaque machine déjà ouverte, la configuration et la fabrique conservées en mémoire après un enregistrement.
- [x] Distinguer explicitement les options Atari800 applicables à chaud de celles appliquées au prochain démarrage.
- [x] Ne transmettre au cœur actif que les clés et valeurs réellement comprises par Atari800.
- [x] Retirer l’accélération SIO des options utilisateur et la maintenir active en interne.
- [x] Vérifier au minimum les options d’activité des lecteurs, compteur secteur/bloc, vitesse d’émulation, horloge temps réel, démarrage cassette, imprimante et port série.

### ATA-CAS-002 — Dissocier la lecture cassette du démarrage automatique

- [x] Ne plus obliger l’utilisateur à activer « Démarrer depuis la cassette » pour pouvoir lire une cassette montée.
- [x] Conserver cette option uniquement comme commande d’amorçage automatique au démarrage de la machine.
- [x] Retirer la commande Arrêt qui injectait `BREAK` sans arrêter réellement le lecteur de cassette.
- [x] Faire envoyer une impulsion `RETURN` par la commande Lecture, comme la validation manuelle observée sur Atari 400/800.
- [x] Ne proposer une commande de transport que lorsqu’elle correspond à une fonction réellement pilotable par l’API du cœur.
- [x] Afficher clairement l’état courant : cassette montée, arrêtée, en lecture, en pause, en enregistrement ou arrivée en fin de bande.
- [x] Faire suivre l’indicateur `TAPE0` à l’activité réelle du moteur de cassette, et non au simple fait qu’un fichier est monté.
- [x] Garder les commandes de transport génériques afin que l’interface ne dépende pas du cœur Atari800.
- [x] Ne pas afficher les commandes Pause, Rembobinage, Avance rapide ou Enregistrement tant qu’elles ne sont pas pilotables de manière fiable.

### ATA-CAS-003 — Corriger l’amorçage automatique des Atari 400/800

- [x] Supprimer la séquence artificielle `START`, `START`, `RETURN` injectée par GW GUI.
- [x] Activer l’option native `atari800_cassboot` pour les Atari 400/800 comme pour les XL, XE et XEGS.
- [x] Envoyer automatiquement `RETURN` après le délai de démarrage des Atari 400/800, lorsque l’amorçage cassette est demandé.
- [x] Maintenir l’accélération SIO toujours active afin que la lecture automatique ou manuelle aboutisse au lieu de terminer sur `BOOT ERROR`.
- [x] Tester que l’amorçage automatique est transmis au cœur pour chaque modèle Atari 8 bits pris en charge.

### ATA-CAS-004 — Corriger le montage des cartouches Atari

- [x] Limiter les extensions proposées aux formats compatibles avec le modèle et son cœur d’émulation.
- [x] Donner la priorité à la cartouche lorsqu’une cassette reste aussi enregistrée dans la configuration et que la cartouche vient d’être choisie.
- [x] Tester séparément les filtres Atari 8 bits, Atari 5200, Atari 2600, Atari 7800, Lynx et Jaguar.
- [x] Tester que la cartouche devient le contenu initial Atari800 même lorsqu’une cassette reste montée.
- [x] Appliquer l’ordre de démarrage cartouche, cassette, puis disquette, indépendamment de l’ordre de montage.
- [x] Transmettre directement au cœur Atari800 un chemin Windows compatible afin qu’il ouvre une cartouche placée dans un dossier comme `F:\Rétro` sans copier le fichier.

### ATA-CAS-005 — Présenter un véritable panneau de transport cassette

- [x] Placer les commandes de transport sous le lecteur cassette au lieu de les aligner avec son nom.
- [x] Afficher en permanence Lecture, Enregistrement, Avance rapide, Rembobinage, Pause et Arrêt.
- [x] Griser et désactiver les commandes que le cœur actif ne sait pas exécuter.
- [x] Colorer Lecture en vert et Enregistrement en rouge, avec une couleur vive pendant leur activité.
- [x] Désactiver Lecture lorsque la cassette est déjà en cours de lecture.
- [x] Faire clignoter la commande active Lecture ou Enregistrement lorsque la cassette est en pause.
- [x] Actualiser l’état visuel des commandes pendant l’émulation sans reconstruire toute la barre des supports.

### ATA-CAS-006 — Présenter les ROM Atari 8 bits selon le matériel

- [x] Afficher un emplacement unique « ROM système » pour l’Atari 400 et l’Atari 800.
- [x] Remplacer la ROM système précédente lors du choix d’une autre révision OS A, OS B, PAL ou NTSC.
- [x] Garder BASIC dans le port cartouche sur Atari 400/800 et ne pas afficher de faux emplacement de ROM BASIC interne.
- [x] Afficher l’emplacement de ROM système sur toutes les configurations utilisant le cœur Atari800 : 400, 800, 800XL, 130XE, XL/XE, XEGS et 5200.
- [x] Afficher ROM système et ROM BASIC sur 800XL, 130XE et XL/XE.
- [x] Afficher séparément ROM système, ROM BASIC et ROM XEGS sur XEGS.
- [x] Afficher le BIOS 5200 dans l’emplacement ROM système de l’Atari 5200.
- [x] Remplacer uniquement la ROM choisie sans effacer les autres ROM internes du modèle.

## Validation manuelle attendue

- [ ] Monter une cassette sans activer le démarrage cassette, lancer son chargement depuis le système Atari et vérifier l’activité de `TAPE0`.
- [ ] Modifier chaque option concernée en laissant la fenêtre Options ouverte, émulateur arrêté puis émulateur actif.
- [x] Démarrer automatiquement une cassette sur Atari 800 sans devoir appuyer manuellement sur `Entrée`.
- [ ] Démarrer automatiquement une cassette sur Atari 400 sans devoir appuyer manuellement sur `Entrée`.
- [x] Vérifier une cassette amorçable connue sur Atari 800.
- [ ] Vérifier une cassette amorçable connue sur Atari 800XL.
- [ ] Vérifier une cassette amorçable connue sur Atari 130XE.
- [x] Monter une cartouche `.car` Atari 8 bits connue et vérifier son démarrage avec une cassette encore enregistrée dans la configuration.

## Tests automatisés à prévoir

- [x] Tester la persistance immédiate des options sans fermeture de la fenêtre.
- [x] Tester l’application des options au prochain démarrage lorsque l’émulateur est arrêté.
- [x] Tester la propagation à chaud des options acceptées par le cœur.
- [x] Tester que Lecture envoie une seule impulsion `RETURN`, conserve les saisies utilisateur et qu’Arrêt n’est pas proposé.
- [x] Tester que l’amorçage automatique Atari 400/800 associe l’option native du cœur à une seule impulsion `RETURN` retardée.
- [x] Tester les états activé, désactivé, actif et clignotant des six commandes du panneau cassette.
- [x] Tester que le panneau affiche les six commandes sur une rangée distincte sous le lecteur cassette.
- [x] Ne conserver dans le dépôt aucun test qui charge un cœur, lance un émulateur ou dépend d’un média externe ; ces vérifications restent manuelles.
