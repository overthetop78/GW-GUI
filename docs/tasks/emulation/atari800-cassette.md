# Atari800 — corrections de la gestion des cassettes

## Constats confirmés

- L’Atari 400/800 prend bien en charge le lecteur de cassette dans le cœur Atari800.
- La cassette `Spindizzy.cas` est reconnue et lue par le cœur.
- Sans action utilisateur, la lecture ne commence pas et l’indicateur `TAPE0` reste inactif.
- En appuyant sur `Entrée`, la lecture commence et l’indicateur `TAPE0` signale correctement l’activité.
- Le test diagnostic avec le vrai cœur a relevé `0` trame active sans `Entrée`, puis `1376` trames actives avec `Entrée`.
- Le cœur maintient automatiquement `START`, puis injecte `SPACE`. Pour l’Atari 400/800 avec son système d’origine, la séquence de démarrage cassette attend `RETURN` : l’automatisation actuelle emploie donc la mauvaise touche pour ce modèle.
- La cassette testée termine sur `BOOT ERROR`. Cela ne remet pas en cause la détection du média, la lecture de la bande ni l’affichage de son activité.

## Modifications à réaliser

### ATA-CAS-001 — Appliquer immédiatement les options Atari800

- [ ] Enregistrer une modification dès que l’utilisateur change une option, sans attendre la fermeture de la fenêtre Options.
- [ ] Mettre à jour la configuration en mémoire même si la fenêtre Options reste ouverte.
- [ ] Si l’émulateur est arrêté, employer les nouvelles valeurs au prochain démarrage sans imposer de fermer puis rouvrir la fenêtre Options.
- [ ] Si l’émulateur fonctionne et que l’option est modifiable à chaud par le cœur, transmettre immédiatement la nouvelle valeur au cœur.
- [ ] Si une option exige réellement un redémarrage du cœur, l’indiquer explicitement dans l’interface et garantir son application au redémarrage suivant.
- [ ] Vérifier au minimum les options d’activité des lecteurs, compteur secteur/bloc, vitesse d’émulation, accélération SIO, horloge temps réel, démarrage cassette, imprimante et port série.

### ATA-CAS-002 — Dissocier la lecture cassette du démarrage automatique

- [ ] Ne plus obliger l’utilisateur à activer « Démarrer depuis la cassette » pour pouvoir lire une cassette montée.
- [ ] Conserver cette option uniquement comme commande d’amorçage automatique au démarrage de la machine.
- [ ] Permettre de démarrer et d’arrêter la lecture d’une cassette montée pendant une session, indépendamment de l’amorçage automatique.
- [ ] Exposer les commandes réellement prises en charge par le cœur, au minimum Lecture et Arrêt ; ajouter Pause, Rembobinage, Avance rapide et Enregistrement seulement si l’API du cœur permet de les piloter correctement.
- [ ] Afficher clairement l’état courant : cassette montée, arrêtée, en lecture, en pause, en enregistrement ou arrivée en fin de bande.
- [ ] Faire suivre l’indicateur `TAPE0` à l’activité réelle du moteur de cassette, et non au simple fait qu’un fichier est monté.

### ATA-CAS-003 — Corriger l’amorçage automatique des Atari 400/800

- [ ] Lors d’un démarrage cassette automatique sur Atari 400/800, reproduire la séquence attendue par la machine : maintien de `START`, puis validation avec `RETURN` au moment approprié.
- [ ] Ne pas injecter continuellement `RETURN` et ne pas écraser les entrées clavier de l’utilisateur.
- [ ] Limiter cette adaptation aux modèles et versions de système qui en ont besoin.
- [ ] Vérifier que l’amorçage cassette des XL, XE et XEGS ne régresse pas.

## Validation manuelle attendue

- [ ] Monter une cassette sans activer le démarrage cassette, démarrer la machine, lancer la lecture manuellement et vérifier l’activité de `TAPE0`.
- [ ] Arrêter puis reprendre la lecture et vérifier que l’état affiché suit immédiatement le moteur de cassette.
- [ ] Modifier chaque option concernée en laissant la fenêtre Options ouverte, émulateur arrêté puis émulateur actif.
- [ ] Démarrer automatiquement une cassette sur Atari 400/800 sans devoir appuyer manuellement sur `Entrée`.
- [ ] Vérifier une cassette amorçable connue sur Atari 800, 800XL et 130XE.

## Tests automatisés à prévoir

- [ ] Tester la persistance immédiate des options sans fermeture de la fenêtre.
- [ ] Tester l’application des options au prochain démarrage lorsque l’émulateur est arrêté.
- [ ] Tester la propagation à chaud des options acceptées par le cœur.
- [ ] Tester les transitions Arrêt → Lecture → Pause → Lecture → Arrêt selon les fonctions réellement disponibles.
- [ ] Tester que l’amorçage automatique Atari 400/800 injecte une seule séquence adaptée sans modifier les saisies utilisateur.
- [ ] Les tests utilisant un cœur ou une cassette locale doivent rester des diagnostics locaux et ne pas être ajoutés à la suite ordinaire du dépôt.
