# 9 — Validation finale des images et du matériel

Cette phase reste la dernière. Elle commence seulement lorsque les refactors, catalogues, traductions, interfaces et contrôles précédents sont terminés.

## 9.1 Ordre du corpus

- [ ] Parcourir `image_test` dans l’ordre des dossiers.
- [ ] Construire la liste à traiter en excluant `image_test/validated_images`.
- [ ] Inclure les images et flux présents dans les dossiers générés.
- [ ] Tester une image à la fois et communiquer son résultat avant la suivante.
- [ ] Corriger le code par format ou famille si un parcours échoue.
- [ ] Rejouer les contrôles concernés afin de vérifier que la correction ne casse pas les autres familles.

## 9.2 Contrôles par image

- [ ] Vérifier lecture du conteneur.
- [ ] Vérifier détection simple ou multiformat de la machine, du format, du système de fichiers et de la protection.
- [ ] Vérifier géométrie, faces, pistes, secteurs et intégrité.
- [ ] Vérifier décodeur et encodeur correspondant, avec aller-retour lorsqu’il est possible.
- [ ] Vérifier les conversions internes ou via Greaseweazle réellement compatibles.
- [ ] Vérifier la Lecture et l’Écriture proposées pour ce format.
- [ ] Vérifier le Visualisateur : média, faces, pistes, couleurs, légende, progression et inspecteur.
- [ ] Vérifier l’Explorateur : volume, systèmes, protections, dossiers, fichiers, types, tailles, dates, contenu, espace libre et avertissements.
- [ ] Vérifier les disquettes protégées sans inventer de faux fichiers ; exposer leur structure physique réelle lorsque le catalogue logique n’existe pas.
- [ ] Vérifier les listes de formats de Lecture, Écriture, Conversion, Explorateur et Visualisateur.
- [ ] Vérifier toutes les traductions nécessaires à ce format.
- [ ] Vérifier performance, annulation, changement rapide d’image, erreurs et journaux.

## 9.3 Classement après validation

- [ ] Déplacer l’image validée vers `validated_images/<marque>/<modèle>/<type de disquette>/`.
- [ ] Vérifier qu’elle n’existe plus dans son dossier d’origine après le déplacement.
- [ ] Classer les images générées dans la même arborescence finale.
- [ ] Supprimer les fichiers parasites du dossier terminé.
- [ ] Supprimer le dossier source lorsqu’il ne contient plus d’image utile.

## 9.4 Essais matériels finaux

- [ ] Tester la Lecture réelle avec les disquettes et le Greaseweazle disponibles.
- [ ] Tester l’Écriture sur une disquette sacrifiable, puis la relire et comparer.
- [ ] Tester les conversions des captures physiques.
- [ ] Tester le Visualisateur et l’Explorateur sur les captures obtenues.
- [ ] Tester l’Effacement uniquement sur le support prévu.
- [ ] Reporter les essais multi-contrôleurs physiques jusqu’à disponibilité du matériel nécessaire.
## 9.5 Validation des entrées/sorties physiques internes

- [ ] Raccorder l’onglet Écriture au service interne derrière une option explicite, puis valider sur disquettes de test Amiga, Atari ST, IBM, MSX, Apple, Commodore, Acorn/BBC, Amstrad, Epson et DEC avant de retirer le repli `gw.exe` pour une famille.
- [ ] Raccorder l’onglet Lecture au service interne derrière une option explicite et valider checksum SCP, nombre de révolutions, pistes, décodage, annulation et reprise avant de retirer `gw.exe`.
