# Émulation Amiga — travaux restants

L’intégration principale est réalisée. Cette feuille conserve uniquement les validations et corrections encore ouvertes dans l’ancien plan.

## Synchronisation audio

- [ ] Définir une cible audio de 60 ms et une plage valide de 30 à 100 ms.
- [ ] Appeler `retro_run` sans attente lorsque le tampon descend sous 30 ms.
- [ ] Retarder la frame suivante avec une attente annulable lorsque le tampon dépasse 100 ms.
- [ ] Tester dix minutes PAL puis NTSC et vérifier que la dérive reste bornée et que la mémoire du tampon ne croît pas.

## Écriture virtuelle des ADF

Le build PUAE précédemment validé exécutait la commande AmigaDOS sur une copie ADF, mais ne persistait pas les octets après fermeture. Les validations restent donc ouvertes.

- [ ] Tester une écriture dans une copie de `Boot-DD-OFS.adf`, relancer et vérifier les octets modifiés.
- [ ] Tester que l’original en lecture seule conserve son SHA-256.

## Modèles nécessitant encore leurs firmwares

- [ ] Ajouter les validations CDTV puis CD32/CD32FR lorsque leurs ROM principale et étendue seront présentes localement ; indiquer précisément tout firmware manquant.

## Validation longue

- [ ] Exécuter 30 minutes PAL puis 30 minutes NTSC avec vidéo, audio et entrées, puis relever les underruns et overruns.

## Condition de fin

Ces huit cases doivent être vérifiées et cochées. Une absence de firmware ou de matériel reste consignée comme un prérequis manquant et ne devient pas un succès.
