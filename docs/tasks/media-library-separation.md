# Séparation de la lecture des médias, des systèmes de fichiers et de l’analyse

Cette feuille conserve les actions encore ouvertes pour la séparation des bibliothèques médias.
La chaîne retenue est la suivante :

1. `GWGUI.MediaEngine` reconnaît, lit, convertit et reconstruit les représentations physiques ou
   logiques d’un média : flux, pistes, secteurs, blocs et données séquentielles ;
2. `GWGUI.MediaFileSystems` détecte les volumes et systèmes de fichiers, puis produit leurs vrais
   dossiers, fichiers, métadonnées et contenus sans inventer de noms ;
3. `GWGUI.MediaAnalysis` reçoit de `GWGUI.MediaFileSystems` les propriétés des dossiers et fichiers
   déjà extraits, puis renvoie pour chacun son type, sa catégorie, son identifiant d’icône et sa clé
   de traduction ; il ne lit ni ne décode l’image ;
4. `GWGUI.MediaFileSystems` complète son arborescence avec ces identifiants et la renvoie à
   `GWGUI.MediaEngine`, qui la transmet avec les informations de l’image à `GWGUI.App` ; celle-ci
   traduit les clés, résout les ressources graphiques et affiche le résultat sans reclassifier les fichiers.

Le seul point d'entrée média de `GWGUI.App` est `GWGUI.MediaEngine`. Celui-ci appelle
`GWGUI.MediaFileSystems` pour l'explorateur et la migration de fichiers ; `GWGUI.MediaFileSystems`
appelle `GWGUI.MediaAnalysis` pour la reconnaissance des types de fichiers. La lecture et l'écriture
de disquettes physiques, la conversion d'images compatibles et la visualisation restent dans
`GWGUI.MediaEngine`. Pour une migration demandée par `App`, `MediaEngine` crée l’image vierge,
`MediaFileSystems` y injecte les fichiers et la renvoie à `MediaEngine`, qui remet le résultat à `App`.

Un futur émulateur réutilise `GWGUI.MediaEngine` pour monter, convertir ou explorer un média ;
aucun lecteur de média ou de système de fichiers ne doit être recopié dans l’émulateur.

- [ ] 1. Traiter les textes destinés à l’interface
  - [ ] 1.1 Recenser les chaînes brutes encore exposées
    - [ ] Créer `docs/project/media-library-raw-texts.md` avec les chaînes brutes encore présentes dans `GWGUI.MediaEngine`, `GWGUI.MediaFileSystems` et `GWGUI.MediaAnalysis`, leurs fichiers sources et les clés de traduction à créer pour les textes destinés à l’interface.

- [ ] 2. Actualiser la documentation après vérification du trajet complet
  - [ ] 2.1 Décrire le trajet de l’explorateur
    - [ ] Modifier `docs/architecture/media.md` pour décrire le trajet de l’image, de l’arborescence et des données d’affichage vérifié dans le code.
