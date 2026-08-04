# Onglet Écriture

## Relation avec Lecture — Validé

Écriture reprend la même organisation générale, le même panneau avancé, les mêmes règles de profils, la commande intégrée, les journaux et la progression. La différence principale est le choix d’un fichier source au lieu du nom d’un fichier à créer.

## Fonctionnement principal

- Sélection d’un fichier image source.
- Détection du type et du format lorsque c’est fiable.
- Le format détecté reste modifiable.
- L’interface distingue un format détecté avec certitude, un format déduit et un format imposé manuellement.
- `.scp` est une source brute auto-descriptive pour ses flux/pistes mais n’impose normalement pas un `--format` pour une écriture brute.
- ADF, ST, MSA et autres formats connus sont reconnus par leur conteneur, leur taille et leurs métadonnées lorsque disponibles.
- Si le format est ambigu, l’exécution est bloquée jusqu’au choix explicite d’un format compatible.
- Les paramètres avancés suivent les mêmes règles que Lecture.
- Les profils mémorisent le format imposé et les options, jamais le fichier, le dossier courant ou le lecteur.

## Sécurité

- Résumé obligatoire avant chaque écriture : fichier, format, lecteur et options sensibles.
- La vérification de `gw` reste active par défaut.
- `--no-verify` est disponible dans les options avancées, expliqué dans son infobulle et signalé dans le résumé.
- Le double avertissement ne signifie pas deux dialogues successifs : avertissement dans le réglage puis présence visible dans le résumé unique.
- La confirmation obligatoire ne peut pas être désactivée globalement.

## Profils — Validé

- Profils propres à Écriture.
- Mémorisent un format éventuellement imposé et toutes les options avancées.
- Ne mémorisent jamais le fichier source, le dossier courant ou le lecteur matériel.

## Paramètres `gw write` à couvrir

- périphérique et lecteur lorsque nécessaires;
- définitions et format;
- pistes/faces/pas/échange de têtes/inversion flippy;
- pré-effacement et effacement des pistes vides;
- faux index et secteurs matériels;
- vérification et tentatives après échec;
- précompensation;
- densité sur broche 2 et TG43;
- arguments libres experts.
