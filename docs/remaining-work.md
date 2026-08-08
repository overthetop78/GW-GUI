# Travail restant et besoins de validation

Audit croisé du plan, des spécifications, du code, des tests, des scripts de publication et de l’installateur, mis à jour le 5 août 2026.

Ce document ne remet pas en cause les validations successives déjà effectuées avant chaque commit. Il sépare ce qui reste réellement à développer de ce qui est déjà réalisé mais demande encore une preuve dans un environnement que le dépôt ne peut pas simuler.

## Résultat de l’audit

- Les fonctions principales prévues sont présentes : Lecture, Écriture, Conversion, Visualisation SCP, Outils, profils, matériel persistant, Host Tools, diagnostics, firmware, console intégrée, thèmes, publication portable et installateur.
- Les 14 commandes publiques de `gw` possèdent un parcours dans l’application.
- Aucun écran principal oublié dans les décisions de conception n’a été trouvé.
- Les anciens nombres de tests conservés dans l’historique décrivent des validations successives; ils ne sont pas des échecs ni des résultats contradictoires.
- Deux chantiers logiciels restent identifiés : la notification de mise à jour de GW GUI prévue au plan mais non implémentée, et l’extension demandée du bilingue vers plusieurs langues.
- Plusieurs validations finales nécessitent du matériel, des systèmes ou des données réelles supplémentaires.

## A. Reste à développer dans le logiciel

### A0.1 — Extensions des images

- remettre toutes les cases de progression à l’état gris au démarrage de chaque nouvelle commande, sans conserver le résultat de la commande précédente ;
- faire correspondre les barres permanentes du visualiseur aux résultats réels du décodage et aux anomalies, avec les couleurs de la légende ;
- définir un emplacement distinct permettant de renommer une image qui vient d’être créée ;
- étendre l’onglet `Explorateur`, déjà réalisé pour AmigaDOS OFS/FFS, Atari TOS FAT12/Atari DOS, Commodore CBM DOS/CP/M 3 et Amstrad CPC/PCW CP/M (DSK/EDSK et SCP), aux autres systèmes de fichiers et conteneurs pris en charge par GW GUI ;
- le sélecteur de l’Explorateur utilise maintenant exactement le catalogue central et le même ordre que les autres fonctions ; réaliser ensuite IBM PC/FAT, Apple, Acorn et les autres systèmes, puis compléter Atari et Amstrad avec les systèmes protégés ou différents des systèmes actuellement interprétés, afin que chaque choix affiché devienne effectivement ouvrable ;
- l’ouverture dans `Explorateur` après une Lecture SCP réussie et la lecture directe d’une disquette via une capture SCP temporaire sont réalisées ; la lecture directe confirme désormais le lecteur et la présence de la disquette avant de démarrer ; vérifier ces deux parcours avec le Greaseweazle réel et plusieurs disquettes Amiga ;
- découper `FluxDecoding.cs` par contrats, registre, traitement du flux et famille de décodeur ; conserver les encodeurs, décodeurs, conteneurs, reconstruction sectorielle et systèmes de fichiers dans des composants distincts ;
- brancher les 21 encodeurs de pistes au futur moteur de conversion interne ; les lecteurs ST, MSA et ATR ainsi que la reconstruction sectorielle Atari sont réalisés, mais les écrivains de ces conteneurs, IMA, D64 et des autres formats restent à ajouter ;
- ajouter un écrivain SCP complet autour des flux produits par les encodeurs, avec index, timings, révolutions, tables de pistes, checksum et tests de compatibilité avec Greaseweazle ;
- pour chacun de ces travaux, ajouter immédiatement les nouvelles clés dans le français, l’anglais et tous les autres fichiers de traduction, sans texte visible écrit en dur.

### A0 — Reprise progressive de l’interface

État : **l’interface actuelle est fonctionnelle mais ne doit pas être considérée comme visuellement définitive**.

- reprendre les écrans pas à pas avec l’utilisateur;
- comparer chaque écran réel à l’intention documentée, sans considérer qu’une structure déjà codée est automatiquement validée visuellement;
- proposer des dispositions concrètes et lisibles avant les modifications importantes;
- conserver les comportements déjà validés pendant les reprises visuelles;
- vérifier chaque étape à 1280×720, avec la console ouverte et fermée, avant de passer à l’écran suivant.
- séparer progressivement le contenu interne des cinq onglets dans des contrôles maintenables, sans supprimer ni remplacer la navigation par onglets.

Le découpage prévu de l’ensemble du code est décrit dans `refactoring-plan.md`. Il s’agit pour le moment d’une étude, pas d’une autorisation de modifier les fichiers.

Les sources HxC peuvent continuer à servir de référence technique pour les nouveaux décodeurs, la reconstruction sectorielle et les conteneurs. Les implémentations GW GUI restent indépendantes en C# et testées dans le projet.

La fenêtre Aide → À propos doit afficher les dépendances et références réellement utilisées, leur rôle, leur licence et un lien cliquable. Une référence étudiée comme HxC est distinguée d’une bibliothèque intégrée comme SkiaSharp.

### A1 — Notification de nouvelle version de GW GUI

État : **fonction prévue mais non trouvée dans le code**.

- Interroger périodiquement et discrètement la dernière release de `overthetop78/GW-GUI`.
- Ne jamais télécharger ni installer automatiquement.
- Afficher une notification non bloquante seulement lorsqu’une version plus récente existe.
- Proposer d’ouvrir la page officielle de la release.
- Mémoriser la date de dernière vérification et permettre de relancer la vérification depuis Aide.
- Distinguer clairement cette mise à jour de celle des Host Tools et du firmware matériel.
- Tester comparaison de versions, absence de réseau, réponse GitHub invalide et absence de notification lorsque la version est à jour.

### A2 — Passage du bilingue au multilingue

État : **architecture et traductions réalisées ; vérifications visuelles complémentaires à poursuivre**.

Les 545 clés sont réparties dans 19 fichiers fonctionnels par culture. Les ressources neutres et les 29 cultures distribuées sont contrôlées automatiquement catalogue par catalogue.

Architecture réalisée :

- créer un catalogue central des langues disponibles : code de culture, nom natif, ressource et langue de l’installateur;
- remplacer les choix binaires `fr/en` dans le démarrage, les Options, les dialogues et les tests;
- construire dynamiquement la liste Langue à partir de ce catalogue;
- utiliser la culture complète lorsque nécessaire, par exemple `pt-BR`, sans la réduire arbitrairement à deux lettres;
- définir un repli clair : langue choisie, puis anglais si une clé manque;
- conserver le choix après redémarrage et lors d’une mise à niveau;
- inclure toutes les ressources satellites dans le projet, le ZIP et l’installateur;
- conserver le contrôle automatique de parité des clés pour chaque langue;
- ajouter un contrôle des textes trop longs, raccourcis clavier, formats numériques et sens de lecture;
- vérifier la fenêtre principale, toutes les fenêtres secondaires, les messages dynamiques, infobulles, noms accessibles et textes du visualiseur dans chaque langue.

Travail de traduction de l’application :

- conserver le français et l’anglais comme langues de référence;
- ajouter la déclinaison de chacun des 19 catalogues `.resx` pour toute nouvelle culture;
- maintenir les 545 entrées sans traduire les identifiants techniques `gw`, noms de formats, arguments de commande ou chemins;
- faire une première passe terminologique, puis une relecture dans l’interface réelle;
- conserver un glossaire commun : piste, face, cylindre, flux, révolution, lecteur, contrôleur, image brute, secteur matériel et vérification;
- prendre des captures et effectuer un test UI dans chaque langue distribuée.

Travail de traduction de l’installateur :

- ajouter chaque langue dans la section `[Languages]` de `installer/GWGUI.iss`;
- utiliser les fichiers `.isl` officiels d’Inno Setup lorsqu’ils existent;
- fournir un fichier de messages complémentaire lorsque les textes propres à GW GUI en ont besoin;
- étendre les scripts de tests qui n’acceptent actuellement que `english` et `french`;
- tester pour chaque langue les pages du wizard, les tâches, le résumé, la fin, l’inscription de désinstallation et la conservation de la langue après mise à niveau;
- conserver l’indépendance validée entre les deux sélections : l’installateur choisit sa propre langue et l’application choisit au premier lancement la langue Windows prise en charge, avec repli anglais;
- ne pas considérer une traduction terminée avant une vérification visuelle du véritable installateur.

Langues retenues ou demandées pour organiser le travail :

1. français et anglais, déjà présents;
2. allemand, espagnol et italien;
3. russe, chinois et japonais;
4. portugais brésilien, néerlandais et polonais;
5. autres langues ajoutées ensuite si elles sont utiles.

Le nombre de langues n’est pas techniquement limité à cette liste. La traduction et la relecture seront réalisées avec ChatGPT/Codex, complétées par les contrôles automatiques et la vérification dans l’interface; aucun relecteur extérieur n’est exigé.

### A3 — Version, build et révision des binaires

État : **fonctionnement actuel inspecté; normalisation à réaliser**.

- la version principale reste actuellement `0.1.0` et ne s’incrémente pas à chaque commit;
- .NET ajoute déjà le hash Git aux informations produit;
- les DLL compilées seules prennent actuellement `1.0.0`, contrairement à l’application;
- certains paquets peuvent contenir deux fois le hash lorsque celui-ci est fourni manuellement puis ajouté par .NET;
- centraliser les versions de l’EXE et des DLL;
- distinguer version produit, numéro de build de compilation et révision Git;
- afficher et tester ces trois informations dans l’application et les paquets.

La convention complète et les tâches techniques sont définies dans `versioning.md`.

### A4 — Renforcement du test de fermeture du paquet

État : **amélioration de non-régression recommandée après la correction de `testhost.exe`**.

- faire vérifier par `test-app-accessibility.ps1` que le processus réel se ferme avec le code 0;
- pour le ZIP portable, vérifier que `Data/settings.json` est créé ou réécrit et contient un JSON valide;
- contrôler la présence des dépendances publiées nécessaires à la sauvegarde, notamment `System.IO.Pipelines.dll`;
- faire échouer le workflow si l’application doit être terminée de force.

### A5 — Entretien de la documentation

État : **quelques formulations de conception sont restées au futur alors que les décisions ont été prises**.

- retirer les mentions indiquant encore que la position de la console, la liste des onglets ou les groupes avancés restent à étudier;
- maintenir ce document comme liste unique du travail restant;
- conserver les nombres historiques de tests dans le journal d’implémentation, mais placer le résultat le plus récent en tête;
- actualiser les guides et captures lorsqu’une langue ou un écran change.

## B. Réalisé, mais encore à valider avec un environnement réel

### B1 — Plusieurs Greaseweazle et plusieurs lecteurs physiques

La détection native d’un Greaseweazle branché est validée en lecture seule. La vérification automatique au démarrage, la conservation des appareils absents, l’actualisation silencieuse du COM et la séparation configuré/non configuré sont implémentées et couvertes par les tests automatisés. Le comportement multi-contrôleur est testé par une matrice simulée. Il reste à reproduire avec au moins deux contrôleurs physiques :

- ports COM distincts;
- plusieurs lecteurs sur un même contrôleur et un lecteur sur chacun de deux contrôleurs;
- routage réel de `--device` et `--drive`;
- débranchement, entrée conservée comme indisponible, reconnexion;
- changement de port COM sans création de doublon grâce à l’identifiant USB.

### B2 — Commandes réelles et versions des Host Tools

Les commandes et les versions officielles 1.23/1.22 sont testées sans couvrir tous les effets physiques. La campagne matérielle retenue se concentre sur Lecture, Écriture, vérification, Conversion indirecte, Effacement et les diagnostics sans risque utiles. Nettoyer les têtes et Firmware restent disponibles dans le logiciel mais ne seront pas testés sur le matériel de l’utilisateur.

Les commandes destructives ou susceptibles de rendre le matériel inutilisable doivent utiliser un support de test et une procédure de récupération définie à l’avance.

### B3 — Windows 11 et SmartScreen

État : **différé pour le moment à la demande de l’utilisateur**.

- installer, mettre à niveau et désinstaller le paquet sur une véritable installation Windows 11;
- vérifier le ZIP portable et l’installateur non signé;
- consigner exactement les écrans SmartScreen rencontrés;
- vérifier les raccourcis, les droits utilisateur, `%AppData%`, `%LocalAppData%` et le nettoyage;
- refaire un lancement/fermeture après installation.

### B4 — Écrans, DPI et accessibilité humaine

Narrator et NVDA sont des lecteurs d’écran : ils lisent à voix haute les boutons, champs, états et dialogues pour les personnes aveugles ou malvoyantes. Le test multi-écran vérifie simplement le comportement lorsque la fenêtre passe entre plusieurs moniteurs, notamment avec des zooms Windows différents. Ces validations sont utiles mais peuvent être différées.

- tester physiquement plusieurs écrans avec des DPI différents et le déplacement entre écrans;
- faire un parcours complet au clavier;
- écouter l’application avec Narrator et idéalement NVDA;
- vérifier l’ordre de lecture, les annonces des changements d’état, la progression et les dialogues;
- relever les textes tronqués dans chaque nouvelle langue.

### B5 — Corpus SCP physique rare

Le moteur possède des tests synthétiques détaillés et quatre captures publiques réelles pour ISO/Amiga. Pour des tests locaux privés, les captures dont dispose l’utilisateur peuvent être utilisées sans devoir être redistribuées avec le projet. Une licence ou une autorisation claire reste nécessaire uniquement si un fichier doit être ajouté au dépôt, au paquet ou téléchargé automatiquement par le CI. Il reste à trouver ou produire des captures représentatives pour les familles rares, puis à vérifier :

- auto-détection;
- géométrie, secteurs et contrôles d’intégrité;
- choix de la meilleure révolution;
- qualification des anomalies;
- rendu et inspecteur;
- comparaison avec une référence indépendante lorsque disponible.

## C. Ce qui nécessite l’aide de l’utilisateur

### C1 — Traduction et relecture des langues

À fournir ou confirmer :

- appliquer l’ordre de langues retenu dans la section A2;
- effectuer la traduction et une seconde passe de relecture avec ChatGPT/Codex;
- demander uniquement à l’utilisateur la terminologie préférée lorsqu’un terme métier possède plusieurs traductions réellement différentes.

Le développement du système multilingue et les traductions peuvent avancer sans rechercher de relecteurs externes. Les tests de parité, de mise en page et de cohérence terminologique restent obligatoires.

### C2 — Accès au matériel

À fournir lorsque possible :

- modèle et firmware du ou des Greaseweazle;
- type des lecteurs disponibles;
- disquettes sacrifiables pour écriture/effacement;
- disquette de nettoyage;
- si possible, prêt ou accès à un second contrôleur/lecteur pour la matrice physique.

Avec un seul Greaseweazle, les tests mono-contrôleur peuvent être faits; la validation multi-contrôleurs restera simplement ouverte.

L’utilisateur peut fournir une disquette vierge réservée aux essais. Elle permettra de tester Lecture, Écriture, vérification, Conversion indirecte, Effacement et plusieurs scénarios d’erreur/récupération. Aucun essai de nettoyage des têtes ou de mise à jour du firmware n’est prévu.

### C3 — Environnement Windows et accessibilité

Validations différées pour le moment :

- accès à Windows 11 ou résultats d’un test guidé sur une autre machine;
- confirmation des écrans SmartScreen observés;
- retour d’un essai Narrator/NVDA et, si disponible, d’un véritable poste multi-écran.

### C4 — Captures SCP rares

À fournir si l’utilisateur en possède : un exemple SCP par famille ou machine, utilisable localement, avec le format attendu lorsqu’il est connu. Les fichiers seront conservés pour éviter de les télécharger à nouveau, renommés simplement selon la machine, placés dans un dossier local ignoré par Git et ne seront ni publiés ni transmis.

## D. Améliorations utiles, non bloquantes

- Ajouter un workflow de validation sur les branches et pull requests, distinct du workflow de publication sur tag.
- Exécuter périodiquement les tests réseau opt-in des releases Host Tools et du corpus SCP afin de détecter les liens devenus invalides.
- Ajouter les parcours interactifs de l’installateur aux validations manuelles de release; l’automatisation UI peut rester séparée si elle est trop fragile sur un runner hébergé.
- Générer un rapport de couverture fonctionnelle lisible à partir des tests, sans remplacer les essais matériels.
- Préparer un modèle de contribution pour les traductions : glossaire, règles `.resx`, contrôle des clés et captures attendues.

## Ordre conseillé

1. Reprendre progressivement l’interface avec l’utilisateur.
2. Normaliser version, build et révision de l’EXE et des DLL.
3. Généraliser le système de langues puis traduire application et installateur par lots relus avec ChatGPT/Codex.
4. Ajouter la notification de mise à jour de GW GUI.
5. Renforcer le test de fermeture du paquet et le workflow continu.
6. Effectuer les validations matérielles disponibles avec la disquette de test.
7. Étendre progressivement le corpus SCP physique rare.
8. Revenir plus tard aux validations Windows 11 et accessibilité humaine.

Les étapes 1 à 5 peuvent être réalisées dans le dépôt sans matériel supplémentaire. L’étape 6 utilise le matériel et la disquette de test de l’utilisateur; l’étape 7 dépend des captures disponibles. L’étape 8 est volontairement différée.

IBM PC FAT12 sur IMG/IMA/SCP est maintenant réalisé et validé. Restent pour cette famille les conteneurs protégés ou structurés distincts (`86F`, `TD0`, `CP2`, etc.) et les systèmes de fichiers autres que FAT12; ils ne doivent pas être présentés comme explorables tant que leur lecteur interne n’est pas ajouté.
