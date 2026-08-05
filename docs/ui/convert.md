# Onglet Conversion

## Sources et compatibilité

- Tous les formats source acceptés par `gw convert` sont pris en charge.
- Le panneau de sorties est dynamique selon le type et la géométrie de la source.
- Un SCP permet de sélectionner les différents décodages pris en charge.
- ADF, ST/MSA, IMG/IMA et les autres sources ne proposent que les conversions compatibles.
- Les sorties incompatibles restent visibles mais désactivées, avec une infobulle explicative.
- `gw convert` change le conteneur ou décode une représentation; il ne transforme pas le système de fichiers d’une machine en celui d’une autre.
- Cette distinction ne doit pas ajouter une étape inutile : pour une source SCP, la case de destination choisie fournit déjà le `--format` nécessaire.
- Exemple : `Atari ST — 720 Kio` génère `--format=atarist.720` et une sortie compatible.

## Multiconversion

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

## Nommage

- Le nom de sortie reprend automatiquement le nom de la source sans extension à chaque nouveau chargement.
- Le nom reste modifiable.
- Les fichiers produits utilisent le dossier courant partagé avec Lecture. Ce dossier revient à la valeur générale des Options au prochain démarrage.
- Une option mémorisée permet d’ajouter des tags précis comme `[ST-720]`, `[PC-1440]` ou `[AMIGA-DD]`.
- Les Options définissent l’état initial des tags; le changement dans Conversion reste valable pour la session.
- Les Options proposent des modèles prédéfinis et un modèle personnalisé.
- Le modèle intégré par défaut est `[FAMILLE-CAPACITÉ]`.
- La case Tags dans Conversion reprend la valeur générale au lancement, puis peut être changée pour le reste de la session sans modifier les Options.
- Si les tags sont activés sans modèle personnalisé, les tags intégrés sont utilisés.

## Exécution

- Une commande `gw convert` distincte est créée pour chaque couple format/extension.
- Un échec n’interrompt pas les autres conversions.
- Le bilan final distingue réussites, échecs et fichiers créés.
- Avant lancement, les conflits sont regroupés dans un résumé permettant Écraser, Ignorer ou Numéroter individuellement ou globalement.

## Paramètres avancés

Les options suivent les mêmes règles que Lecture et Écriture et couvrent : format/diskdefs, pistes d’entrée, pistes de sortie, pas, échange des têtes, vitesse, PLL, secteurs matériels, inversion flippy, protection contre écrasement et arguments experts.
