# Onglet Lecture

## Objectif visuel — Validé

L’écran de l’ancien GUI affiche trop d’informations simultanément. Lecture doit rester lisible et ne montrer les réglages techniques qu’après activation du panneau avancé.

## Fonctionnement principal

- Choix entre `Image brute SCP` et `Disquette au format connu`.
- Pour un format connu, le format de disquette détermine les types d'image compatibles.
- SCP représente toujours une capture brute du flux.
- Le parcours principal sépare clairement :
  - `Image brute SCP` pour archiver la surface/les flux;
  - `Disquette au format connu` pour produire une image exploitable comme ADF, ST, IMG/IMA, etc.
- Pour un format connu, l’utilisateur choisit la famille et la géométrie/capacité; l’application choisit le type d’image normal et ne propose que les extensions compatibles.
- Exemple : AmigaDOS conduit naturellement à ADF; il n’est pas utile d’afficher une liste globale contenant des extensions sans rapport.
- Une capture SCP peut utiliser un format de vérification avancé sans cesser d’être une capture brute.
- Le nom du fichier est saisi sans extension et possède une action Copier.
- L’extension est affichée sur la même ligne que le nom, pas intégrée au champ du nom.
- Les extensions ont des libellés compréhensibles, par exemple `Image brute (SCP)`.
- SCP apparaît en tête des choix de capture brute.
- Le nom est vidé après redémarrage.
- Le dossier temporaire reste actif pendant la session puis revient au dossier par défaut au redémarrage.
- Au premier niveau restent visibles : profil, résultat, nom et dossier.
- Les paramètres techniques sont dans un panneau avancé intégré.
- Le bouton de choix du dossier se trouve près du nom. Il modifie le dossier courant pour la session sans changer automatiquement le dossier par défaut.
- Le dossier par défaut se règle dans Options.

## Numérotation automatique

- Compteurs numériques ou alphabétiques.
- Masques numériques `0`, `00`, `000` et alphabétiques `A`, `AA`, `AAA`.
- Après `Z`, continuation avec `AA`, `AB`, etc.
- Aperçu avec le nom courant ou un nom d’exemple si le champ est vide.
- Incrémentation seulement après une lecture réussie.
- En cas de conflit : Écraser, utiliser le numéro suivant ou modifier le nom.
- Le choix `Utiliser le numéro suivant` cherche un nom libre et actualise l’aperçu.
- `Modifier le nom` annule le lancement et rend le focus au champ du nom.
- Aucun écrasement et aucune incrémentation ne sont silencieux.

## Profils

- Un profil peut mémoriser les réglages de Lecture, le résultat/format et éventuellement un dossier particulier.
- Il ne mémorise jamais le nom du fichier ni le lecteur matériel.
- Réinitialiser recharge le profil actif. Avec Par défaut, il remet complètement les options à zéro.
- Le bouton d’enregistrement utilise une icône de sauvegarde et ouvre une boîte demandant le nom.
- La boîte ne présente pas un récapitulatif inutile de tous les paramètres.
- Les profils utilisateur peuvent porter des noms libres comme `Disquettes récalcitrantes`.
- Les Options permettent de renommer et supprimer les profils de Lecture.

## Paramètres avancés — Validé

- Le panneau se déplie dans l’onglet; ce n’est ni une fenêtre séparée ni un sous-onglet.
- Catégories fonctionnelles prévues : pistes/faces, lecture/récupération, synchronisation/vitesse, matériel spécialisé.
- Les catégories exactes seront validées lors de la maquette.
- Une case active chaque option facultative et son champ associé.
- Décocher retire l’argument mais conserve temporairement la valeur saisie.
- Le libellé reste humain; l’infobulle montre le drapeau `gw`, son rôle et un exemple.
- Le profil Par défaut et la réinitialisation sans profil utilisateur effectuent une remise à zéro complète.

Options `gw read` à couvrir :

- périphérique et lecteur lorsqu’ils ne sont pas implicites;
- fichier de définitions et format;
- nombre de révolutions;
- sélection des cylindres, faces, pas, échange de têtes et offsets;
- capture brute avec vérification de format;
- faux index et disques à secteurs matériels;
- ajustement de vitesse;
- tentatives et nouvelles tentatives après seek;
- protection contre l’écrasement, gérée aussi par l’interface;
- PLL : période, phase, filtre passe-bas;
- densité sur la broche 2, TG43 et inversion des données pour disquette flippy;
- arguments libres experts.
