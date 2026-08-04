# Spécification visuelle validée

## Fenêtre principale

- Taille minimale : 1280×720.
- Restaurer taille, position, écran, maximisation et thème; recentrer si l’écran précédent a disparu.
- Thèmes Système, Clair et Sombre; accent Windows.
- Menus Options et Aide.
- Onglets Lecture, Écriture, Conversion, Visualisation et Outils.
- Bouton Exécuter/Arrêter stable en bas à droite de l’onglet.

## Console et état

- Console inférieure réunissant commande et logs.
- Redimensionnable et réductible; restaurer hauteur et état de la dernière session.
- Premier lancement : ouverte à une hauteur raisonnable.
- Ne pas forcer sa réouverture pendant une commande.
- Commande en première ligne, lecture seule, copiable; arguments experts séparés.
- Barre d’état sous la console : diode, port COM, lecteur, profil et progression par face.
- Progressions masquées hors opération.

## Lecture

- Parcours vertical : Profil, Image à créer, Nom/extension, Dossier, Paramètres avancés, Exécuter.
- Deux choix visibles : Image brute SCP ou Disquette au format connu.
- Format connu : Famille, Format/géométrie et Type d’image compatible.
- Paramètres avancés en accordéons indépendants pouvant rester ouverts simultanément.
- Groupes : Pistes et faces; Lecture et récupération; Rotation et index; Traitement du signal; Matériel spécialisé.

## Écriture

- Structure cohérente avec Lecture.
- Source puis résumé du format détecté avec bouton Modifier.
- Les listes de format s’ouvrent sur demande ou automatiquement si ambigu.
- Cinq groupes avancés : Pistes/faces; Préparation/vérification; Rotation/index; Écriture du signal; Matériel spécialisé.
- Confirmation simple par résumé et boutons Annuler/Écrire.

## Conversion

- Source, type détecté, nom et case Tags restent fixes.
- Zone centrale défilante classée par machine/famille.
- Section Sélections épinglée en haut.
- Formats courants visibles; formats rares derrière une extension de liste.
- Exécuter et Paramètres avancés restent fixes.
- Profil mémorise sorties et options; pas la source, le nom, le dossier ou les tags temporaires.
- Aucun résumé avant lancement sauf conflit de fichier.

## Visualisation SCP

- Barre supérieure : fichier, ouverture, décodeur automatique/manuel, lien des vues et réinitialisation.
- Deux disques circulaires côte à côte.
- Inspecteur droit masquable pour face, piste, révolution, encodage, secteurs et anomalies.
- Une face unique est centrée et agrandie.
- Zoom/déplacement indépendants avec bouton Lier.
- Légende visible sous la vue.

## Options et dialogues

- Options persistantes dans une fenêtre à navigation latérale.
- Diagnostics et Matériel restent des dialogues ponctuels.
- Chaque dialogue montre résumé lisible, sortie brute repliable et ligne de commande en bas.
- Sélecteur de lecteur dans chaque opération seulement si plusieurs lecteurs sont configurés.
