# Interface — opérations sur les disquettes

## Lecture

### Objectif visuel — Validé

L’écran de l’ancien GUI affiche trop d’informations simultanément. Lecture doit rester lisible et ne montrer les réglages techniques qu’après activation du panneau avancé.

### Fonctionnement principal

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
- Le bouton Exécuter/Arrêter reste fixe en bas à droite de l’onglet; seuls les réglages défilent lorsque la fenêtre utilise sa taille minimale.
- Le bouton de choix du dossier se trouve près du nom. Il modifie le dossier courant pour la session sans changer automatiquement le dossier par défaut.
- Le dossier par défaut se règle dans Options.

### Numérotation automatique

- Compteurs numériques ou alphabétiques.
- Masques numériques `0`, `00`, `000` et alphabétiques `A`, `AA`, `AAA`.
- Après `Z`, continuation avec `AA`, `AB`, etc.
- Aperçu avec le nom courant ou un nom d’exemple si le champ est vide.
- Incrémentation seulement après une lecture réussie.
- En cas de conflit : Écraser, utiliser le numéro suivant ou modifier le nom.
- Le choix `Utiliser le numéro suivant` cherche un nom libre et actualise l’aperçu.
- `Modifier le nom` annule le lancement et rend le focus au champ du nom.
- Aucun écrasement et aucune incrémentation ne sont silencieux.

### Profils

- Un profil peut mémoriser les réglages de Lecture, le résultat/format et éventuellement un dossier particulier.
- Il ne mémorise jamais le nom du fichier ni le lecteur matériel.
- Réinitialiser recharge le profil actif. Avec Par défaut, il remet complètement les options à zéro.
- Le bouton d’enregistrement utilise une icône de sauvegarde et ouvre une boîte demandant le nom.
- La boîte ne présente pas un récapitulatif inutile de tous les paramètres.
- Les profils utilisateur peuvent porter des noms libres comme `Disquettes récalcitrantes`.
- Les Options permettent de renommer et supprimer les profils de Lecture.

### Paramètres avancés — Validé

- Le panneau se déplie dans l’onglet; ce n’est ni une fenêtre séparée ni un sous-onglet.
- Catégories fonctionnelles prévues : pistes/faces, lecture/récupération, synchronisation/vitesse, matériel spécialisé.
- Les catégories retenues sont : pistes et faces, lecture et récupération, rotation et index, traitement du signal et matériel spécialisé.
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

## Écriture

### Relation avec Lecture — Validé

Écriture reprend la même organisation générale, le même panneau avancé, les mêmes règles de profils, la commande intégrée, les journaux et la progression. La différence principale est le choix d’un fichier source au lieu du nom d’un fichier à créer.

### Fonctionnement principal

- Sélection d’un fichier image source.
- Détection du type et du format lorsque c’est fiable.
- Le format détecté reste modifiable.
- L’interface distingue un format détecté avec certitude, un format déduit et un format imposé manuellement.
- `.scp` est une source brute auto-descriptive pour ses flux/pistes mais n’impose normalement pas un `--format` pour une écriture brute.
- ADF, ST, MSA et autres formats connus sont reconnus par leur conteneur, leur taille et leurs métadonnées lorsque disponibles.
- Si le format est ambigu, l’exécution est bloquée jusqu’au choix explicite d’un format compatible.
- Les paramètres avancés suivent les mêmes règles que Lecture.
- Les profils mémorisent le format imposé et les options, jamais le fichier, le dossier courant ou le lecteur.

### Sécurité

- Résumé obligatoire avant chaque écriture : fichier, format, lecteur et options sensibles.
- La vérification de `gw` reste active par défaut.
- `--no-verify` est disponible dans les options avancées, expliqué dans son infobulle et signalé dans le résumé.
- Le double avertissement ne signifie pas deux dialogues successifs : avertissement dans le réglage puis présence visible dans le résumé unique.
- La confirmation obligatoire ne peut pas être désactivée globalement.

### Profils — Validé

- Profils propres à Écriture.
- Mémorisent un format éventuellement imposé et toutes les options avancées.
- Ne mémorisent jamais le fichier source, le dossier courant ou le lecteur matériel.

### Paramètres `gw write` à couvrir

- périphérique et lecteur lorsque nécessaires;
- définitions et format;
- pistes/faces/pas/échange de têtes/inversion flippy;
- pré-effacement et effacement des pistes vides;
- faux index et secteurs matériels;
- vérification et tentatives après échec;
- précompensation;
- densité sur broche 2 et TG43;
- arguments libres experts.

## Conversion

### Sources et compatibilité

- Tous les formats source acceptés par `gw convert` sont pris en charge.
- Le panneau de sorties est dynamique selon le type et la géométrie de la source.
- Un SCP permet de sélectionner les différents décodages pris en charge.
- ADF, ST/MSA, IMG/IMA et les autres sources ne proposent que les conversions compatibles.
- Les sorties incompatibles restent visibles mais désactivées, avec une infobulle explicative.
- `gw convert` change le conteneur ou décode une représentation; il ne transforme pas le système de fichiers d’une machine en celui d’une autre.
- Cette distinction ne doit pas ajouter une étape inutile : pour une source SCP, la case de destination choisie fournit déjà le `--format` nécessaire.
- Exemple : `Atari ST — 720 Kio` génère `--format=atarist.720` et une sortie compatible.

### Multiconversion

- Il n’existe pas de mode séparé simple/multiple.
- Les formats de disquette sont sélectionnés par cases à cocher.
- Chaque format propose ses extensions compatibles.
- Il n’existe pas un premier sélecteur global `Convertir vers` en plus des cases : les cases sont l’unique mécanisme pour une conversion simple ou multiple.
- Si aucune extension n'est cochée, l'extension par défaut du format est utilisée; elle est indiquée dans l'infobulle.
- Cocher explicitement une extension remplace le choix implicite; en cocher plusieurs produit plusieurs fichiers.
- Exemple validé de comportement :
  - `IBM PC — 720 Kio` coché sans extension explicite produit l’extension par défaut définie par le catalogue validé;
  - `.img` seul coché produit IMG et supprime la sortie implicite IMA;
  - `.ima` et `.img` cochés produisent les deux.
- L’interface ne coche pas automatiquement une extension visible qu’il faudrait ensuite retirer.
- L’extension implicite est indiquée en infobulle, sans message textuel imposé dans le panneau.
- Les formats cochés sont épinglés en haut, même lorsqu’ils appartiennent à la partie rare normalement repliée.
- Décocher un format lui rend sa position normale.
- Les sélections sont mémorisées.
- Les formats courants sont visibles en premier; un bouton étend les formats rarement utilisés.
- Toutes les cases cochées restent visibles en haut même lorsque la partie rare est repliée.
- Pour IBM PC, les géométries sont des choix distincts : 160, 180, 320, 360, 720, 800 Kio, 1,2, 1,44, 1,68 et 2,88 Mio, DMF, ainsi que les fonctions particulières pertinentes comme `ibm.scan` sans les présenter comme une capacité normale.

### Nommage

- Le nom de sortie reprend automatiquement le nom de la source sans extension à chaque nouveau chargement.
- Le nom reste modifiable.
- Les fichiers produits utilisent le dossier courant partagé avec Lecture. Ce dossier revient à la valeur générale des Options au prochain démarrage.
- Une option mémorisée permet de placer avant le nom des tags précis comme `[ST-720] Disquette.st`, `[PC-1440] Disquette.ima` ou `[AMIGA-DD] Disquette.adf`.
- Les Options définissent l’état initial des tags; le changement dans Conversion reste valable pour la session.
- Les Options proposent des modèles prédéfinis et un modèle personnalisé libre. Le modèle intégré par défaut est `[{FAMILY}-{FORMAT}] `; son résultat est par exemple `[PC-720] Disquette.ima`.
- Les variables disponibles sont `{NAME}`, `{FAMILY}`, `{FORMAT}`, `{EXTENSION}`, trois écritures de `{DATE:...}` et trois écritures de `{TIME:...}`. L’aperçu peut parcourir plusieurs exemples sans modifier les réglages.
- La case Tags dans Conversion reprend la valeur générale au lancement, puis peut être changée pour le reste de la session sans modifier les Options.
- Si les tags sont activés sans modèle personnalisé, les tags intégrés sont utilisés.

### Exécution

- Une commande `gw convert` distincte est créée pour chaque couple format/extension.
- Un échec n’interrompt pas les autres conversions.
- Le bilan final distingue réussites, échecs et fichiers créés.
- Avant lancement, les conflits sont regroupés dans un résumé permettant Écraser, Ignorer ou Numéroter individuellement ou globalement.

### Paramètres avancés

Les options suivent les mêmes règles que Lecture et Écriture et couvrent : format/diskdefs, pistes d’entrée, pistes de sortie, pas, échange des têtes, vitesse, PLL, secteurs matériels, inversion flippy, protection contre écrasement et arguments experts.
