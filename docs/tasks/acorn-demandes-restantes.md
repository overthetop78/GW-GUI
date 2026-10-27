# Demandes Acorn — travail restant et ordre de réalisation

Ce document conserve les demandes et les défauts montrés dans les captures. Une nouvelle question ne remplace pas la tâche en cours. Répondre puis reprendre le travail ; inscrire les nouvelles actions à la suite. Ne pas réinscrire le gel Utiliser : l'utilisateur a confirmé qu'il est corrigé. Ne pas lancer une compilation, fermer l'application, supprimer/restaurer son exécutable ou publier sans respecter les instructions déjà données.

## 1. Terminer les corrections stockage déjà écrites

Exécution détaillée : `docs/tasks/acorn-storage-settings.md`. Les corrections décrites dans ce suivi sont vérifiées et livrées par le build Debug du 27 octobre 2026. La vérification doit couvrir chacun de ces points, sans les remplacer par un résumé générique.

- [x] Finaliser les corrections de configuration et de création.
  - [x] Modifier `src/GWGUI.App/Resources/*/Emulation/EmulationStorage.resx`, `EmulationCassette.resx`, `EmulationHdd.resx` : achever les traductions Argos, corriger le français et contrôler les libellés de toutes les langues ; conserver les valeurs invariantes dans la base commune.
  - [x] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/HardDiskDriveConfigurationDialog.cs` et les constantes associées : sélectionner une capacité par défaut autorisée, inclure celle du profil personnalisé dans les présélections et vérifier les capacités calculées par la géométrie.
  - [x] Modifier les tests autonomes stockage existants et créer puis supprimer `artifacts/acorn-storage-settings-check/` : vérifier les images autorisées/refusées, la création effective, la préallocation, les fenêtres et chaque défaut des captures ci-dessous ; libérer fenêtres, threads et objets possédés dans finally.
  - [x] Modifier la livraison `build/Debug/` par le script autorisé lorsque l'application est fermée ; vérifier l'exécutable et le module Acorn, puis consigner les résultats dans `docs/tasks/acorn-storage-settings.md`.

### Captures disquette : critères distincts

- Aucun lecteur Cassette0 ou autre ne doit apparaître sans branchement explicite ; réutiliser les contrôles App existants.
- Le contrôleur disque et ses ROM appartiennent aux extensions de la machine ; ils sont partagés par les deux lecteurs. Les choix et chemins sont persistants, les champs interdits grisés. Solidisk autorise les deux champs. Une ROM incompatible ne doit pas devenir utilisable silencieusement.
- Modèles : proposer les seuls modèles réellement pris en charge ; décrire faces, densité et capacité (SD, double face SD, DD, double face DD) plutôt que présenter seulement SS/DS et 40/80. La géométrie reste nécessaire au fonctionnement, même si elle n'est pas le libellé principal.
- Refuser une image de géométrie/capacité incompatible avec le modèle ; ne pas confondre extension du conteneur, formatage et dépendance d'un logiciel à un contrôleur. Géométries, formats et extensions appartiennent à MediaEngine ; le module utilise le relais Emulation.
- Supprimer le choix de profil sonore manuel ; activation et volume seuls. Le son vient du matériel et du repli convenu, sans attribution inventée.
- Remplacer Sons mécaniques, Profil sonore mécanique et Volume sonore mécanique par des libellés compréhensibles. Petit volume vertical ; vitesse en boutons radio ou crans prédéfinis.
- Afficher BBC Micro Model A, pas BbcMicroModelA. Réduire la hauteur, regrouper les réglages et contrôler visuellement le rendu.
- Créer une disquette vierge doit ouvrir réellement le choix de fichier : corriger l'erreur path vide. Ne pas imposer Floppies ni un nouveau dossier de copies ; l'écriture porte sur l'image déjà montée. Respecter le dossier HDD déjà existant.
- Dialogue de création : Créer et monter, Créer, Fermer, Annuler ; ne pas monter automatiquement après Créer seul.

### Captures HDD : critères distincts

- Préallouer le fichier doit être disponible quand le format le permet et réserver effectivement l'espace.
- Aucun texte de préparation spécifique Atari dans la configuration Acorn.
- Supprimer les deux cartes d'information Périphérique et Interface ; intégrer les informations utiles dans l'en-tête.
- Réduire/redimensionner la fenêtre ; groupes cohérents, volume et latences verticaux compacts, vitesse par choix discrets ; aucun sélecteur de son arbitraire.
- Profil de taille et préparation : ne proposer/accepter que les capacités compatibles avec le matériel émulé, son adressage et le formatage. Distinguer profils commerciaux et capacités compatibles de l'émulation. Ne pas réduire arbitrairement les quatre unités logiques existantes.
- Le repli sonore n'est pas un enregistrement attesté du Winchester réel ; ne pas le présenter comme tel. Les enregistrements exacts manquants restent du travail à réaliser.

## 2. Conserver les exigences cassette et audio à vérifier sans les déclarer faites

Le travail historique est détaillé dans `docs/tasks/acornemu-model-a.md`. Avant toute correction supplémentaire, comparer chaque exigence à ses résultats existants ; ne pas refaire un travail déjà validé. L'audit global de tous les fichiers Constants a été reporté par l'utilisateur.

- [x] Compléter le suivi des exigences cassette/audio encore non validées.
  - [x] Modifier `docs/tasks/acornemu-model-a.md` après vérification des résultats déjà consignés : conserver séparément les points ci-dessous, marquer les points effectivement réalisés et définir des actions de fichiers pour les manques établis avant de les exécuter.

### Cassette : exigences à conserver

- Réutiliser Tape et les contrats existants ; supprimer les doublons Cassette ajoutés à tort. App affiche les champs déclarés par le module et n'impose pas des réglages métier inutilisables par d'autres machines.
- Relais commun MediaEngine → Emulation → modules internes ; lecture ET écriture, moteur, transport, état REC/Recording, activité et voyants de lecture/écriture.
- UEF, CSW, WAV et RAW : lecture, écriture, création, conversion, blocs visibles et exploration des fichiers ; UEF doit aussi être proposé à l'enregistrement.
- Modes temps réel, accéléré et direct ; débit baud séparé, automatique/300/1200 pour le matériel BBC correspondant ; expliquer qui choisit le débit automatique.
- Temps de lecture/écriture, latences fournies par l'émulation ; avance/retour vers les blocs à la vitesse choisie, sons et durée réelle en mode réel, déplacements silencieux directs/accélérés.
- REC ouvre le dialogue de création et de choix de format avec Créer et monter, Créer, Fermer/Annuler. Protection en écriture, sauvegarde avant éjection/extinction, position de bande et conservation du reste de l'image lors de l'enregistrement.
- RAW par défaut 48000 Hz, mono, PCM signé 16 bits little-endian ; options de lecture dans le lecteur, options d'écriture dans REC, options de conversion accessibles dans la conversion. Ne pas présenter le PCM comme le son du moteur.
- Fenêtre compacte par groupes ; baud par boutons radio ; vitesse et délais seulement lorsque pertinents au temps réel, curseurs courts à crans et présélections ; traductions correctes, notamment Protection en écriture et Boutisme little-endian.

### Audio et assets : exigences à conserver

- Moteur, lecture/écriture, seek, avance/retour, insertion, arrêt, éjection selon le périphérique : cassette, disquette, HDD, optique ; générique MediaEngine et relais Emulation, y compris conversion, visualisateur et explorateur.
- Accès à une image : suivre moteur, position et déplacement des têtes, accès/lecture/écriture pour choisir réellement les sons ; la banque seule et un appel de rendu manuel ne suffisent pas.
- Sons correspondant aux machines et matériels des modules ; aucun bip synthétique substitué à un bruitage réel. Faire aussi les sons manquants, sans prétendre qu'une imitation est parfaitement identique à un enregistrement réel.
- Crêtes propres ne dépassant pas 0 dBFS ; crêtes des événements audibles non silencieux au-dessus de -50 dBFS. Respecter les différences de bruit entre matériels pour les sons créés ; réglage de volume utilisateur. Ne pas confondre crête et niveau constant.
- Replis : Alps Electric pour floppy sans son propre, HDD choisi explicitement, unique profil optique disponible ; affectation automatique dans les lecteurs, pas de sélection arbitraire.
- Prévisualisations WAV dans artifacts/audio-preview à régénérer après remplacement des sons et nettoyage ciblé de cette sortie.
- Originaux de référence dans artifacts/mechanical-audio-research ; conserver la vraie signification des seeks montants/descendants, pas faces A/B ni codes numériques opaques.
- Assets externes non incorporés aux assemblies, copiés vers Assets racine ; audio FLAC sans perte décodable avec les dépendances déjà présentes. Même principe pour images de manettes et de consoles de tous les modules.
- Classement Assets/MechanicalAudio/<type>/<marque>/<modèle>/nom.flac ; noms courts sans répétition marque/modèle, sans numéros de liste ; Insert/Eject/Motor ensemble, Seek/Buzz dans leurs sous-dossiers.
- Catalogues C# par dossiers/matériels et agrégateur ; dictionnaires contenant directement les noms/chemins utiles. Aucun catalogue JSON utilisé pour piloter le rendu ; aucune provenance/source/revision/hash recopiée dans le code, aucun fichier monolithique de 10000 lignes. Pas de preparation-report/provenance générés inutiles.
- Retirer les scripts temporaires de tools et leurs artefacts après utilisation ; ne pas retirer les outils préexistants nécessaires. Ne pas inventer une nouvelle sortie audio sans utiliser les dépendances/facettes existantes.

## 3. Réaliser ensuite les extensions mémoire autorisées

Exécution initiale : `docs/tasks/acorn-memory-extensions.md` ; suite ordonnée : `docs/tasks/acorn-memory-and-floppy-followup.md`. Aries B20 et Watford 32K sont réalisés et vérifiés. Les modules BooBip RAM/ROM (1 à 4, jusqu’à 128 Kio de RAM et 128 Kio de ROM) et EPROM (1 à 4, jusqu’à 256 Kio de ROM) sont vérifiés et livrés. Aries B12, ses douze supports et ses capacités 0/8/16 Kio sont vérifiés en source ; ce dernier ajout n’est pas encore livré. Les autres cartes et coprocesseurs restent à réaliser.

- [x] Réaliser les cartes et leurs interactions.
  - [x] Modifier les fichiers listés dans `docs/tasks/acorn-memory-extensions.md` dans leur ordre : bloc RAM Aucune/cartes, capacités réelles, ROM de gestion, routage mémoire, contraintes et conflits ; ajouter les tests utiles et toutes les traductions.

### Décisions utilisateur à respecter

- Conserver la base Model A réalisée. Model B utilisera ce code et restera une machine distincte ; déplacer les éléments partagés dans des Common spécifiques au moment de commencer le Model B.
- Le bloc RAM permet de choisir une extension ou Aucune ; ne pas remplacer la RAM principale par un simple chiffre supplémentaire.
- Coder effectivement les cartes proposées et gérer leurs incompatibilités ; ne pas afficher une option fonctionnelle sans implémentation.
- Une carte ROM peut ajouter des ROM et des supports ; certains supports existants peuvent être déplacés, conservés ou rendus inutilisables suivant le matériel. Gérer la place et les conflits comme des contraintes propres à la carte, pas des slots PC universels.
- Aucun chargement implicite de ROM lorsque son champ est vide. Les quatre ROM ajoutées ont ensuite été retirées à la demande de l’utilisateur, ainsi que leur référencement comme assets fournis ; la reconnaissance des fichiers utilisateur reste conservée.
- Les questions sur la taille/organisation ROM ne sont pas une autorisation d'inventer une nouvelle demande de chargement arbitraire d'une ROM de 256 Ko.

## Hors reprise automatique

L'audit global de tous les fichiers Constants reste reporté. Les anciennes demandes de commit ne déclenchent pas un nouveau commit des modifications actuelles. Les corrections déjà confirmées par l'utilisateur ne sont pas réinscrites comme travail restant.

## État après livraison

Le script Debug Acorn a terminé avec le code 0 après autorisation utilisateur. Les corrections des fenêtres stockage et les deux cartes d'extension RAM sont livrées ; 220 contrôles passent. Le rendu des fenêtres stockage a été inspecté. Les sons réels encore manquants, les éventuels manques Tape/audio hors émulation identifiés par la reprise et les autres extensions matérielles ne deviennent pas terminés par cette livraison : leurs exigences détaillées restent conservées ci-dessus.

## Vérifications complémentaires du 27 octobre

Les formats et capacités de création disquette, la protection attachée au média et les nouvelles présentations Tape/HDD/disquette ont été vérifiés dans le suivi complémentaire puis livrés. Les pages RAM/ROM ont été contrôlées avec les modules paginés et B12. La dernière validation en source compte 244 tests AcornEmu réussis ; B12 avec B20 explicitement sélectionnée totalise 68 Kio. Tous les projets de contrôle temporaires de cette reprise ont été supprimés après fermeture et libération de leurs fenêtres. Aucun de ces résultats ne clôt les exigences audio/Tape ou les autres cartes encore ouvertes ci-dessus.
