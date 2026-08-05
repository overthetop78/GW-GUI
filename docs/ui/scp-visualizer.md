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
- Les marqueurs décodés sont dessinés directement sur chaque anneau : en-têtes/adresses, données et anomalies utilisent des couleurs distinctes de la densité du flux brut.
- Si une piste contient plusieurs révolutions, le visualiseur compare leur décodage et affiche celle qui fournit les structures les plus fiables; l’inspecteur précise le numéro retenu.
- La cellule de temps est suivie progressivement pendant la révolution : une légère dérive de vitesse ne décale donc pas toute la suite du décodage.

## Décodage

- Lecteur SCP universel.
- Décodeurs modulaires.
- Le moteur doit permettre la couverture complète des analyseurs proposés par HxC.
- L’architecture reste modulaire afin que chaque famille de codage soit isolée et testable.
- Les décodeurs actuellement réalisés couvrent Atari/IBM PC (ISO MFM/FM), Amiga MFM, Apple II GCR, Commodore GCR, Membrain MFM, AED 6200P MFM, QD MO5 MFM, Centurion MFM, NorthStar MFM, Heathkit FM, E-mu Emulator FM, TYCOM FM, DEC RX02 M²FM, Arburg et Victor 9000 GCR. Centurion extrait notamment le cylindre et le secteur et vérifie le CRC16 XMODEM de l’en-tête. Les signatures rares ont été confrontées aux sources officielles HxC. Les autres familles HxC définies dans le plan restent à couvrir intégralement; cela n’est pas présenté comme une version volontairement incomplète.
- Le niveau exact de chaque analyseur est suivi dans `../scp-decoder-coverage.md`. NorthStar extrait piste/secteur et valide le checksum du bloc de 512 octets; Heathkit extrait volume/cylindre/secteur et valide son checksum; Membrain extrait cylindre/face/secteur et son CRC16; AED 6200P extrait cylindre/secteur/taille variable et son CRC-CCITT; QD MO5 extrait le numéro de secteur sur 16 bits et valide la somme du bloc de 128 octets; E-mu extrait cylindre/face et valide les CRC de l’en-tête et du bloc de 3584 octets avec sa reconstruction FM spécifique; TYCOM extrait cylindre/secteur, distingue F8–FB et valide les CRC de l’en-tête et du bloc de 128 octets.
