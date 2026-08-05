# Options, matériel et diagnostics

## Options générales

- Dossier d’images par défaut.
- Langue française ou anglaise.
- Configuration des tags de conversion.
- Gestion des profils utilisateur par onglet : renommer et supprimer.
- Valeur générale initiale de la case Tags et modèle de tag configurable. Le jeton `{tag}` est obligatoire, la valeur initiale est ` [{tag}]` et un aperçu montre le nom obtenu.
- Un changement temporaire dans Conversion n’écrase pas ce réglage général.

## Matériel

- Plusieurs contrôleurs Greaseweazle et lecteurs mémorisés.
- Identification USB et dernier port COM.
- Type physique du lecteur : 3 pouces, 3,5 pouces, 5,25 pouces ou 8 pouces.
- Sélection Greaseweazle A/B ou 0/1 lorsque nécessaire.
- Les lecteurs absents restent dans la configuration et sont signalés comme indisponibles.
- Le matériel est décrit par des listes : contrôleur associé, valeur de sélection A/B ou 0/1, taille 3, 3,5, 5,25 ou 8 pouces, densité/capacité connue et éventuellement vitesse habituelle.
- Ces propriétés servent d’abord à construire un libellé rapide; elles ne doivent pas modifier silencieusement les commandes.
- L’application peut mémoriser l’identifiant USB afin de retrouver le contrôleur même si Windows change son port COM.

## Menu Options

### Diagnostics — boîtes de dialogue

- Informations du contrôleur (`gw info`).
- Mesure de bande passante USB (`gw bandwidth`).
- Mesure de vitesse du lecteur (`gw rpm`).
- Déplacement vers un cylindre (`gw seek`).

### Matériel — boîtes de dialogue

- Lecture/modification des broches (`gw pin`).
- Réinitialisation du contrôleur (`gw reset`).
- Consultation/modification des délais (`gw delays`).

### Firmware

- Mise à jour du firmware du contrôleur (`gw update`).
- La mise à jour du firmware se trouve dans `Options > Matériel > Firmware`.
- Mise à jour du bootloader séparée et protégée par un avertissement renforcé.
- Distinguer clairement : mise à jour du GUI, mise à jour des outils hôte `gw`, mise à jour du firmware matériel.

## Maintenance dans l'onglet Outils

- Effacer une disquette (`gw erase`).
- Nettoyer les têtes (`gw clean`).
- Aucun profil pour ces actions.
- Valeurs conservées pendant la session puis retour aux réglages sûrs au redémarrage.
- Confirmations adaptées à la destruction des données et à l'insertion d'une disquette de nettoyage.

## Rôle des boîtes de dialogue

- `info` : modèle, version de firmware, connexion et informations de bootloader pour les matériels concernés.
- `bandwidth` : mesure ponctuelle du débit USB en lecture/écriture.
- `rpm` : mesure de la vitesse réelle du lecteur avec nombre d’itérations.
- `seek` : déplacement de la tête vers un cylindre pour test ou maintenance; options force et moteur actif.
- `pin` : lecture ou modification de broches d’interface pour les usages matériels experts.
- `reset` : retour du contrôleur à son état de démarrage en cas de problème.
- `delays` : lecture/modification des temporisations de sélection, pas, stabilisation, moteur, watchdog, pré/post-écriture et masque d’index.
- `update` : mise à jour normale ou avancée du firmware matériel.
