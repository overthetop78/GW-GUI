# État réel de l’implémentation

Ce document complète le plan fonctionnel. Il décrit ce qui existe dans le code à la date de sa dernière mise à jour et ne réduit pas le périmètre du produit final.

## Fonctionnel

- Solution .NET 10 structurée en Application WPF, Domaine, Infrastructure, moteur SCP et tests.
- Exécution de `gw` sans fenêtre console, arguments séparés, sortie intégrée, annulation et verrouillage à une opération.
- Configuration JSON versionnée et écrite atomiquement.
- Onglets Lecture, Écriture, Conversion, Visualisation et Outils; menus Diagnostics et Matériel.
- Lecture SCP ou format connu, nom sans extension, dossier persistant, numérotation numérique/alphabetique, conflits et profils.
- Écriture avec détection/modification du format, vérification par défaut et confirmation obligatoire.
- Multiconversion séquentielle, sorties compatibles, extensions implicites/explicites, tags, conflits et bilan.
- Effacement, nettoyage, diagnostics et commandes matérielles intégrés.
- Profils propres à Lecture, Écriture et Conversion; profil système Par défaut permanent.
- Renommage et suppression des profils utilisateur dans les Options.
- Registre matériel persistant : scan des ports Windows, interrogation `gw info --device`, identification stable, disponibilité, ajout et suppression de lecteurs décrits par sélection, taille, densité et RPM.
- Infrastructure bilingue `.resx` et culture chargée avant la première fenêtre. Toutes les vues utilisent désormais des clés pour leurs libellés naturels; seuls les nombres, valeurs techniques et noms natifs des langues restent littéraux. Options, diagnostics, profils, conflits, inspecteur SCP et messages dynamiques sont migrés. Un test contrôle toutes les vues et la parité exhaustive des catalogues FR/EN.
- Thèmes clair, sombre et système appliqués au démarrage et après les Options; le mode système suit Windows et reprend sa couleur d’accent.
- Taille, position multi-écran, maximisation, visibilité et hauteur de console restaurées; les positions hors écran sont rejetées.
- Tous les boutons Arrêter demandent confirmation; l’annulation tente une fermeture normale pendant deux secondes puis termine l’arbre du processus si nécessaire.
- Barre d’état dynamique : matériel et lecteur sélectionnés, profil actif, progression par piste/face extraite des sorties officielles `T<cylindre>.<face>`; les répétitions de tentative ne sont pas comptées deux fois.
- Gestionnaire Host Tools : détection chemin/configuration/PATH/dossier géré, consultation quotidienne discrète de la release officielle, téléchargement x64 volontaire avec progression, contrôle SHA-256 lorsqu’il est publié, extraction ZIP défensive, installations versionnées et retour au chemin précédent.
- Catalogue de formats recoupé au démarrage avec `gw read --help` : les profils et suffixes absents de la version active sont retirés, avec repli sur le catalogue intégré si l’interrogation échoue. Le catalogue couvre les géométries IBM PC officielles de 160 Kio à 2,88 Mio, DMF et scan, les six profils Atari ST officiels, et utilise l’identifiant Amiga HD corrigé `amiga.amigados_hd`. IMA reste l’extension IBM implicite sans être cochée automatiquement.
- Lecture défensive du conteneur SCP, pistes et révolutions, contrôle des limites et checksum.
- Visualisation circulaire par face, zoom, déplacement, sélection de piste et inspecteur.
- Décodeurs flux brut, ISO MFM, ISO FM et Amiga MFM; sélection automatique ou manuelle; extraction initiale des en-têtes de secteurs ISO.

## Encore à réaliser avant achèvement

- Valider le scan et le routage des commandes sur plusieurs contrôleurs physiques.
- Compléter toutes les options avancées de chaque commande `gw` et leurs profils.
- Vérifier visuellement les deux langues et traduire progressivement les descriptions provenant encore du catalogue métier lorsqu’elles sont exposées à l’utilisateur.
- Ajouter les états visuels succès/erreur et vérifier la progression avec plusieurs versions réelles des Host Tools.
- Valider le gestionnaire Host Tools avec une installation réelle et finaliser le comportement portable versus installé.
- Étendre les noms lisibles et règles de sortie du catalogue aux autres familles publiées dans les diskdefs de la version de `gw` active.
- Étendre le moteur SCP à tous les décodeurs définis dans le plan et améliorer PLL, anomalies et visualisation des structures.
- Ajouter journal rotatif, export, migrations et couverture de tests d’intégration/UI/matériel.
- Réaliser icône, aide utilisateur bilingue, ZIP portable, installateur Inno Setup, sommes SHA-256 et workflow GitHub Actions.

## Validation actuelle

- Compilation Release : zéro erreur et zéro avertissement.
- Tests automatisés : 51 réussis, dont capacités de formats, gestion release/ZIP/checksum Host Tools, parité FR/EN, placement multi-écran, annulation réelle et progression.
- Tests matériels Greaseweazle et validation visuelle interactive : non encore effectués sur cette machine.
