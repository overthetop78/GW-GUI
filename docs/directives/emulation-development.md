- Respecter l'architecture, les interfaces et les services déjà en place pour créer ou compléter un émulateur.
  - Appliquer le cycle Power commun : éteindre arrête et libère le cœur ; rallumer crée une nouvelle instance, en conservant l'application et le module ouverts.
  - Réserver les modifications d'App et d'Emulation aux cas d'extrême nécessité.
    - Concevoir ces modifications pour toute catégorie d'émulateur : ordinateur, console, arcade, flipper ou autre électronique.
    - Adapter tous les consommateurs concernés pour disposer d'un parcours cohérent et unique.
  - Centraliser dans MediaEngine les formats, extensions, géométries et caractéristiques des médias et lecteurs ; les modules obtiennent ces données par Emulation.
    - Vérifier la compatibilité réelle du support avec le lecteur avant montage ; présenter faces, densité et capacité et conserver les limites physiques de géométrie.
    - Fournir les sons mécaniques communs depuis MediaEngine, avec des profils adaptés aux lecteurs de bandes, disquettes, disques durs et CD, y compris insertion, éjection, moteur, accès et déplacements applicables.
  - Déclarer dans les contrats du module les champs utilisables, groupes, choix, présélections et conditions ; réserver à App leur présentation compacte et les dimensions de fenêtre.
    - Ranger le contrôleur de disquettes dans la configuration du lecteur ; proposer des commandes adaptées et montrer les réglages mécaniques en mode réel.
  - Regrouper les identifiants et données fixes dans des constantes ou catalogues dédiés et placer les énumérations dans leurs fichiers dédiés.
  - Donner la priorité aux touches système et aux affectations existantes lors du traitement des entrées.
  - Réutiliser les modèles et traitements Tape existants pour les bandes ; faire passer leurs signaux et leur écriture par Common, les interfaces Emulation et MediaEngine, et conserver le matériel émulé dans la machine.
    - Proposer les modes réel, accéléré et direct, les débits propres à la machine et les déplacements entre blocs ; associer les bruits mécaniques au mode réel.
    - Préserver les données hors de la zone enregistrée, respecter la protection en écriture et sauvegarder avant éjection et extinction.
    - Utiliser 48 kHz mono PCM signé 16 bits little-endian par défaut pour le RAW ; proposer son profil dans la configuration du lecteur en lecture, dans REC en écriture et dans les options d'entrée/sortie de conversion.
- Séparer les modèles dans l'émulateur et réunir les traitements réellement communs.
  - Répertorier les identités des BIOS/ROM dans un catalogue partagé de Common/Machines et définir leurs compatibilités dans les profils des machines, indépendamment des émulateurs qui les utilisent.
    - Afficher les ROM connues avec leur compatibilité définie pour la machine, et conserver le statut inconnu pour les fichiers non reconnus ; distinguer cette identification des contraintes de chargement.
  - Réserver aux adaptateurs d'émulateurs les champs de configuration, les modalités de chargement et les limites de leur implémentation.
  - Découper les composants et leurs responsabilités dans des fichiers ciblés, même si cela nécessite beaucoup de fichiers.
  - Organiser cette découpe pour pouvoir déplacer et réutiliser les fichiers dans les autres modèles, gammes ou marques concernés.
- Rechercher sur Internet les informations techniques du modèle et étudier les sources des émulateurs existants pour comprendre et améliorer son émulation.
  - Consigner les informations dans `docs/project/emulations/<marque>/<gamme>/<modèle>/`.
  - Placer les informations communes à une gamme dans son dossier, et celles communes à une marque ou à ses émulateurs dans le dossier de la marque.
  - Ajouter les sources consultées et les liens de téléchargement des BIOS et ROM trouvés, en précisant leurs modèles, versions et rôles.
  - Conserver dans les documents les détails techniques utilisés, les fichiers sources et leurs révisions, pour retrouver les informations localement pendant le développement.
- Utiliser les BIOS et ROM disponibles séparément comme fichiers de firmware externes, sélectionnés par l'utilisateur et chargés par les interfaces de firmware existantes.
- Faire suivre à la saisie la disposition active du clavier hôte, AZERTY ou autre, pavé numérique compris, en adaptant les caractères aux touches de la machine émulée.
- Documenter complètement le modèle et son émulation dans son document de référence.
  - Expliquer chaque information technique utile et chaque fichier : chemin, contenu, rôle, fonctionnement, liaisons et échanges avec les autres éléments.
  - Compléter cette documentation au fur et à mesure des recherches et de la création des fichiers.
  - Expliquer également le code dans les fichiers sources avec les commentaires et la documentation C# nécessaires.
- Fournir régulièrement un build Debug avec le module concerné pour permettre à l'utilisateur de tester les avancées et de diriger la suite.
  - Tenir compte de ses retours dans le développement et compléter les directives et documents concernés.

- Les écritures des lecteurs utilisent le fichier image déjà monté. Ne pas créer de dossiers ou copies de travail imposés par le module ; réutiliser les emplacements HDD existants pour la création.
- La création d’une disquette vierge et la création d’une bande depuis REC proposent explicitement « Créer et monter », « Créer », « Fermer » et « Annuler ». Créer seul ne monte pas le média ; le montage suit le parcours commun existant.

- Les bruitages mécaniques doivent reproduire les lecteurs réels identifiés, pour toutes leurs opérations pertinentes. Des tonalités synthétiques génériques ne constituent pas des profils fidèles. Documenter les références et la provenance des enregistrements.
- Fournir des sons à un niveau suffisant, sans saturation, avec un réglage permettant de réduire leur volume. Après remplacement complet des sons, vider les anciens exports de `artifacts/audio-preview` et régénérer tous les WAV correspondants.

- La couverture des sons mécaniques concerne les machines existantes de tous les modules, y compris les variantes de lecteurs d’une même machine. Partir des catalogues matériels du dépôt.
- Pour le volume mécanique, 50 % représente le niveau normal de référence du profil ; au-delà, amplifier jusqu’à un niveau nettement supérieur à 100 %, tout en évitant la saturation. Cette référence numérique ne garantit pas une pression acoustique physique sans calibration des haut-parleurs.

- Les seuils −50 dB minimum et 0 à −10 dB maximum demandés concernent le niveau des sons sources audibles, pas une plage imposée au curseur de volume. Le volume du lecteur doit ensuite permettre une atténuation classique, par exemple à 20 %.

- Les limites de niveau demandées portent uniquement sur la crête des bruitages non silencieux. Préserver les silences, les passages faibles et la dynamique ; ne pas relever le signal entier au seuil minimal.

- Ne pas normaliser les bruitages par lecteur ou par profil : conserver les écarts de niveau entre lecteurs silencieux et bruyants, ainsi que la dynamique des enregistrements. Appliquer uniquement le réglage de volume d’écoute.

- La réserve concernant la normalisation vise les sons à créer : préserver la différence entre mécanismes silencieux et bruyants. Elle ne prescrit pas un retraitement particulier des enregistrements récupérés.

- Ranger les bruitages externes dans `Assets/MechanicalAudio/<type de lecteur>/<marque>/<modèle>/<bruitage>.flac`. Garder insertion, éjection et moteur ensemble ; séparer les séries Seek et Buzz. Aucun nom de source logicielle ne remplace une référence matérielle inconnue. Ne pas répéter marque et modèle dans les noms de fichiers.
- Définir identité et nom/chemin des sons ensemble dans les catalogues dédiés de MediaEngine, regroupés par type, marque et modèle ; un catalogue principal agrège les catalogues par type pour le chargement commun. Ne pas disperser les références dans des fichiers séparés par son ou trier l’index source par empreinte. Conserver `catalog.json` ; ne pas générer ni distribuer `preparation-report.json` ou `provenance.json` dans la banque. Les sources originales restent identifiées dans le manifeste de recherche.

- Tous les fichiers `Assets` des modules Emulation.xxx restent externes aux DLL. Les distribuer sous le dossier racine `Assets/Emulation/<module>/`, conserver leur arborescence interne et utiliser des chemins explicites. Les images de manettes suivent le même principe sous `Assets/Controllers/`. Préserver les fichiers des autres modules pendant les mises à jour et inclure les Assets du module dans sa sauvegarde/restauration transactionnelle.

- Les noms et identifiants audio doivent décrire le matériel et l’événement ; ne pas utiliser les empreintes comme identifiants fonctionnels. Pour les déplacements de têtes, conserver distance et direction avec des noms explicites (vers le centre / vers le bord), sans confondre direction et face du disque. Garder le nom et le chemin source dans le catalogue pour identifier chaque enregistrement. Refuser les collisions plutôt qu’inventer des numéros de variante.

- Bases mécaniques en l’absence de son spécifique : Alps Electric DF354H148F pour les disquettes, Seagate ST3120A pour les disques durs, le seul ensemble optique à tiroir disponible pour les lecteurs optiques. Conserver les sons spécifiques disponibles ; faire suivre à la durée d’événement le même repli par opération que le rendu audio.
