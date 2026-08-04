# Visualiseur SCP

## Objectif

- Ouvrir tout fichier SCP dans un onglet dédié.
- Proposer l’ouverture automatique après une lecture SCP réussie.
- Afficher les deux faces sous forme de disques circulaires avec pistes concentriques.
- Moderniser le principe du Visual Floppy Disk de HxC sans dépendre de HxC.
- La référence fonctionnelle montre deux grands disques côte à côte, un par face, avec les pistes concentriques et des segments colorés représentant structures, données et anomalies.
- Le but n’est pas de reproduire l’ancienne disposition HxC, mais de fournir les mêmes informations de façon moderne et lisible.

## Interactions

- Zoom et déplacement fluides.
- Sélection d’une piste et d’une face.
- Survol avec informations techniques.
- Légende claire des couleurs, secteurs et anomalies.
- Affichage du flux brut et des résultats de décodage.

## Décodage

- Lecteur SCP universel.
- Décodeurs modulaires.
- Le moteur doit permettre la couverture complète des analyseurs proposés par HxC.
- L’architecture reste modulaire afin que chaque famille de codage soit isolée et testable.
- Atari/IBM PC (ISO MFM/FM) et Amiga MFM font partie du socle, puis les autres familles HxC doivent également être couvertes; cela n’est pas présenté comme une version volontairement incomplète.
