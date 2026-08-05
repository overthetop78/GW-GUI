# Guide utilisateur — GW GUI

![Fenêtre principale de GW GUI — onglet Lecture](images/main-read-fr.png)

## Première configuration

1. Ouvrez **Options → Préférences**.
2. Détectez ou sélectionnez `gw.exe`. Le téléchargement intégré des Host Tools reste volontaire.
3. Dans **Matériel**, recherchez le contrôleur Greaseweazle puis décrivez chaque lecteur raccordé.
4. Définissez le dossier d’images par défaut, la langue et le thème.

Les contrôleurs et lecteurs débranchés restent mémorisés. Une nouvelle recherche n’est nécessaire que si leur port ou leur configuration change.

## Lecture

- **Image brute SCP** archive directement les flux de la disquette.
- **Disquette au format connu** décode vers ADF, ST, IMA ou un autre conteneur compatible.
- Le champ du nom ne contient pas l’extension.
- La numérotation automatique accepte chiffres ou lettres et ne progresse qu’après une lecture réussie.
- Les options techniques sont repliées dans **Paramètres avancés**.
- Le profil **Par défaut** désactive toutes les options facultatives. Sauvegarder crée un profil propre à Lecture.

En cas de fichier existant, choisissez explicitement l’écrasement, le prochain numéro libre ou la modification du nom.

## Écriture

Choisissez l’image source. GW GUI détecte son format par conteneur et par taille lorsqu’il le peut; une ambiguïté doit être résolue manuellement. Un résumé obligatoire affiche le fichier, le format, le lecteur et l’état de la vérification avant l’écriture. Désactiver la vérification est une option avancée signalée comme risquée.

## Conversion

Les cases de formats servent à la conversion simple comme multiple. Les sorties incompatibles avec la source sont désactivées. Sans extension explicitement cochée, l’extension normale du format est utilisée; cocher une ou plusieurs extensions remplace ce choix implicite. Les formats sélectionnés restent épinglés en haut.

Les tags tels que `[PC-720]` ou `[AMIGA-DD]` évitent les collisions lors d’une multiconversion. Chaque sortie est exécutée séparément et le bilan final conserve les réussites même si une conversion échoue.

## Visualisation SCP

Ouvrez une capture SCP pour afficher les deux faces, zoomer, déplacer la vue et sélectionner une piste. L’inspecteur indique les révolutions, la vitesse estimée, le checksum et les structures reconnues par le décodeur automatique ou choisi.

## Outils, diagnostics et matériel

- **Outils** contient l’effacement de disquette et le nettoyage des têtes, avec confirmation.
- **Options → Diagnostics** contient information, bande passante USB, RPM et déplacement de tête.
- **Options → Matériel** contient broches, réinitialisation, délais et firmware.

Les actions potentiellement dangereuses restent dans des dialogues séparés de l’usage courant.

## Console, arrêt et historique

La commande exacte et les sorties de `gw` sont intégrées en bas de la fenêtre et peuvent être masquées ou exportées. Le bouton **Exécuter** devient **Arrêter** pendant une opération et demande confirmation. L’historique dans le menu Options conserve dix journaux de 5 Mio maximum.

## Données et mode portable

- Installation classique : données dans les dossiers utilisateur Windows.
- ZIP portable : la présence de `portable.flag` place réglages, journaux et Host Tools gérés dans le dossier `Data` voisin de l’application.

GW GUI n’envoie aucune télémétrie.
