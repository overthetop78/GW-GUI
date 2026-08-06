# Options, matériel et diagnostics

## Options générales

- La fenêtre conserve sa taille, reste modale et utilise des onglets horizontaux. Un bouton Fermer global hors des onglets a le même effet que la croix.
- Il n’existe aucun bouton général Enregistrer/Annuler : les listes sont appliquées immédiatement, les champs texte à la perte du focus et la fermeture effectue une dernière sauvegarde de sécurité.
- Dossier d’images par défaut.
- Langue française ou anglaise.
- La langue et le thème utilisent des listes déroulantes compactes côte à côte et s’appliquent immédiatement sans fermer ni recréer de fenêtre.
- Les hauteurs natives des champs et listes déroulantes sont conservées; leur texte est centré verticalement. Le dossier occupe le cadre gauche de la première ligne et le cadre plus court Langue/Thème se trouve à sa droite.
- Configuration des tags de conversion : activation générale, modèles prédéfinis, champ modifiable, légende des variables, aperçu et cinq modèles personnalisés récents.
- Gestion des profils utilisateur par onglet : renommer et supprimer.
- La valeur initiale du modèle est `[{FAMILY}-{FORMAT}] ` et le texte produit est placé avant le nom. Les variables disponibles couvrent nom, famille, format, extension, date et heure dans plusieurs écritures compatibles avec les noms de fichiers Windows; chacune est expliquée dans la légende à droite.
- L’aperçu est présenté dans un cadre sombre à texte vert; le bouton immédiatement voisin parcourt les exemples dans un ordre déterministe.
- Les cinq derniers modèles personnalisés sont conservés en ordre MRU : un doublon remonte en tête et le sixième chasse le plus ancien. Les modèles prédéfinis ne modifient pas cet historique.
- L’historique affiche toujours cinq emplacements numérotés de 1 à 5; les emplacements encore vides restent visibles.
- La légende présente chaque variable dans un cartouche monospace et son explication traduite à côté.
- Un changement temporaire dans Conversion n’écrase pas ce réglage général.

## Matériel

- Plusieurs contrôleurs Greaseweazle et lecteurs mémorisés.
- Host Tools, contrôleurs et lecteurs sont regroupés dans le même onglet Matériel; Scanner et Ajouter un lecteur sont placés au-dessus de la liste compacte.
- Le scan détecte les contrôleurs USB/série, mais Windows et `gw info` ne peuvent pas énumérer les lecteurs physiques placés derrière la nappe. Le premier lecteur est donc configuré sur le contrôleur détecté et le bouton Ajouter un lecteur permet d’en déclarer un autre manuellement.
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
