# Émulation Atari — feuille d’exécution complète

## Objectif

Intégrer à GW GUI les six cœurs Libretro retenus, avec le même niveau d’intégration que l’émulation Amiga : installation et remplacement des cœurs, configurations persistantes, firmwares, médias, vidéo, audio, entrées, états, interface traduite, documentation, packaging et tests.

| Cœur | Machines visées | Médias principaux |
|---|---|---|
| Hatari | ST, STF, STFM, Mega ST, STE, Mega STE, TT, Falcon | disquette, disque dur, dossier GEMDOS |
| Atari800 | 400, 800, XL, XE, XEGS, 5200 | disquette, cassette, cartouche |
| Stella | 2600 | cartouche |
| ProSystem | 7800 | cartouche |
| Beetle Lynx | Lynx | cartouche |
| Virtual Jaguar | Jaguar, Jaguar CD | cartouche, CD |

Cette matrice exprime la cible fonctionnelle. Les formats, options et limites exacts de chaque cœur doivent être confirmés à partir de son code et de ses métadonnées pendant les premières tâches, sans inventer de capacité absente.

## Règles d’exécution

- Exécuter les tâches dans l’ordre de leurs identifiants.
- Ne cocher une tâche qu’après son implémentation complète et sa validation.
- Après chaque tâche : lancer les tests ciblés, exécuter `git diff --check`, puis seulement remplacer `[ ]` par `[x]`.
- Ne jamais ajouter de ROM, BIOS, TOS, cartouche, disque ou CD protégé au dépôt ou aux packages.
- `GWGUI.App` ne doit jamais appeler directement une fonction `retro_*`.
- Chaque instance de cœur s’exécute dans un processus hôte isolé, comme pour Amiga.
- Le modèle choisi détermine automatiquement le cœur ; l’utilisateur ne choisit pas un cœur technique.
- Une option sans effet pour la machine choisie reste visible mais grisée, avec une explication traduite.
- Tous les textes visibles doivent provenir des ressources de traduction, dans toutes les langues prises en charge.
- Toute version proposée par l’interface de gestion des cœurs doit pouvoir être téléchargée et remplacer la version installée ; taille, empreinte et en-tête restent des diagnostics, pas des motifs de rejet.

## Architecture cible

- `GWGUI.Emulation.Atari` contient le domaine Atari, les catalogues, la persistance, l’installation des cœurs et les adaptateurs Libretro.
- `GWGUI.App` contient uniquement la présentation, les fournisseurs applicatifs, la préparation des médias et le démarrage des hôtes.
- Le protocole hôte reprend le modèle Amiga et transporte commandes, vidéo, audio, entrées, médias, état et erreurs structurées.
- Les abstractions réellement communes à Amiga et Atari sont extraites sans créer de dépendance entre les deux domaines.
- Les bibliothèques propres au projet restent dans `lib`; les dépendances tierces suivent le rangement déjà défini par bibliothèque.

## Tâches

### A — Sources et capacités réelles

- [ ] **ATA-001 — Figer les sources des six cœurs.** Enregistrer pour chaque cœur le dépôt officiel, la documentation, les métadonnées Libretro, la licence, le nom des DLL Windows x64 et la méthode officielle d’obtention des builds.
- [ ] **ATA-002 — Établir la matrice vérifiée des capacités.** Relever dans les sources les modèles, extensions, firmwares, options, périphériques, Disk Control, états sérialisés, résolutions, audio, entrées et limites ; corriger le tableau cible si une capacité réelle diffère.

### B — Contrats communs d’émulation

- [ ] **ATA-003 — Généraliser les types de médias.** Ajouter les types et emplacements nécessaires aux cartouches et cassettes sans casser les emplacements Amiga existants ; couvrir sérialisation et compatibilité par tests.
- [ ] **ATA-004 — Extraire uniquement les éléments Libretro réellement communs.** Mutualiser ABI, résolution des symboles, callbacks, mémoire partagée et structures de protocole réutilisables, tout en conservant les règles propres à chaque machine dans son projet.

### C — Domaine Atari et processus hôte

- [ ] **ATA-005 — Créer le projet `GWGUI.Emulation.Atari`.** Ajouter références, conventions de nommage `gwgui`, analyseurs, injection et raccordement à la solution sans fusionner le code dans l’exécutable.
- [ ] **ATA-006 — Définir les contrats Atari.** Créer machine, configuration, modèle, firmware, média, entrée, vidéo, audio, état, résultat et erreurs structurées.
- [ ] **ATA-007 — Implémenter l’adaptateur Libretro Atari.** Charger chacun des six cœurs, négocier environnement et options, transmettre les callbacks et libérer toutes les ressources de façon déterministe.
- [ ] **ATA-008 — Implémenter l’hôte Atari isolé.** Ajouter l’argument de démarrage dédié, le protocole IPC, les mémoires vidéo/audio, les délais, l’arrêt propre, la remontée des erreurs et les tests de panne du processus.

### D — Installation et versions des cœurs

- [ ] **ATA-009 — Créer le catalogue des six cœurs.** Associer identifiant, machines, source officielle, versions disponibles, DLL attendue et chemin d’installation à chaque cœur.
- [ ] **ATA-010 — Implémenter recherche, téléchargement et remplacement.** Réutiliser le parcours Amiga en autorisant toute version proposée, remplacer simplement l’ancienne DLL et conserver taille/empreinte/informations PE uniquement pour diagnostic.
- [ ] **ATA-011 — Ajouter l’interface de gestion des cœurs Atari.** Afficher version installée, versions disponibles, recherche, téléchargement/remplacement, progression et erreurs utiles pour le cœur déterminé par le modèle.

### E — Modèles et compatibilité matérielle

- [ ] **ATA-012 — Cataloguer la famille ST.** Définir ST, STF, STFM, Mega ST, STE, Mega STE, TT et Falcon avec CPU, fréquence, mémoire, vidéo, audio et périphériques compatibles.
- [ ] **ATA-013 — Cataloguer les Atari 8 bits et consoles.** Définir 400, 800, XL, XE, XEGS, 5200, 2600, 7800, Lynx, Jaguar et Jaguar CD avec le cœur automatiquement associé.
- [ ] **ATA-014 — Centraliser les règles d’activation.** Fournir une source unique indiquant quelles options, firmwares, ports et médias sont applicables à chaque modèle, avec motif de désactivation traduisible.

### F — Firmwares et ROM système

- [ ] **ATA-015 — Créer le catalogue des firmwares Atari.** Identifier TOS et autres BIOS requis ou facultatifs, noms reconnus, régions, versions et compatibilités, sans distribuer les fichiers protégés.
- [ ] **ATA-016 — Implémenter détection et sélection.** Scanner les dossiers configurés, identifier les fichiers quand c’est possible, signaler absent/inconnu/incompatible et transmettre le bon firmware au cœur.

### G — Médias et stockage

- [ ] **ATA-017 — Implémenter les médias Hatari.** Gérer insertion, éjection, remplacement et rotation des disquettes, listes multidisques, disques durs et dossiers GEMDOS selon les capacités vérifiées.
- [ ] **ATA-018 — Implémenter les médias Atari800.** Gérer disquettes, cassettes et cartouches, y compris les besoins propres à la 5200 et la sélection de type lorsque le cœur l’exige.
- [ ] **ATA-019 — Implémenter les cartouches console.** Gérer chargement et remplacement pour 2600, 7800, Lynx et Jaguar avec validation lisible des formats réellement pris en charge.
- [ ] **ATA-020 — Implémenter Jaguar CD.** Ajouter le lecteur CD, insertion, éjection et remplacement, et désactiver ce périphérique pour la Jaguar sans CD.

### H — Vidéo, audio et synchronisation

- [ ] **ATA-021 — Intégrer la vidéo.** Transmettre dimensions, pitch, format de pixels, fréquence et changements de géométrie ; rendre dans la surface commune sans fenêtre créée par le cœur.
- [ ] **ATA-022 — Intégrer l’audio.** Transmettre les lots stéréo, gérer tampon, volume, silence, reprise et périphérique Windows avec arrêt sans processus résiduel.
- [ ] **ATA-023 — Gérer cadence et région.** Respecter PAL/NTSC et les fréquences propres aux machines, synchroniser audio/vidéo, calculer les FPS et tester pause, reprise et accélération autorisée.

### I — Clavier, souris et contrôleurs

- [ ] **ATA-024 — Définir les actions Atari.** Créer les actions communes et spécifiques par famille sans chaînes ni codes magiques dispersés.
- [ ] **ATA-025 — Implémenter clavier et souris.** Mapper clavier ST/8 bits, clavier Libretro, souris et capture/libération ; désactiver proprement ce qui ne s’applique pas aux consoles.
- [ ] **ATA-026 — Implémenter les contrôleurs.** Gérer joysticks, joypads, contrôleurs analogiques et nombre de ports selon le modèle et les capacités du cœur.
- [ ] **ATA-027 — Intégrer les raccourcis.** Réutiliser le système commun pour alimentation, pause, reset, plein écran, libération souris, états rapides et changement de média, avec détection des conflits.

### J — États, configurations et cycle de vie

- [ ] **ATA-028 — Implémenter les états.** Ajouter sauvegarde, chargement, états rapides, captures et métadonnées lorsque le cœur le permet ; afficher une indisponibilité explicite sinon.
- [ ] **ATA-029 — Implémenter le stockage des configurations.** Sauvegarder modèle, options, firmwares, médias, entrées et dossiers dans un format versionné, migrable et testé.
- [ ] **ATA-030 — Implémenter le cycle de vie complet.** Ouvrir plusieurs machines en onglets, démarrer, arrêter, redémarrer, fermer, restaurer la souris et nettoyer processus, pipes, mappings et fichiers temporaires.

### K — Interface Atari

- [ ] **ATA-031 — Ajouter l’entrée Atari dans l’application.** Intégrer création, sélection, ouverture et suppression des configurations Atari sans modifier le fonctionnement Amiga.
- [ ] **ATA-032 — Construire les sous-onglets de paramètres.** Reprendre la structure Amiga : Général, CPU, RAM, ROM, Vidéo, Audio, Stockage, Clavier, Souris et Contrôleurs.
- [ ] **ATA-033 — Adapter dynamiquement l’interface au modèle.** Afficher les valeurs compatibles et griser les contrôles non applicables avec une explication, notamment disquette/cartouche/cassette/CD et périphériques d’entrée.
- [ ] **ATA-034 — Construire la vue de machine en cours d’exécution.** Ajouter barre d’outils, rendu, indicateurs, raccourcis, médias et statut, avec les mêmes règles de marge et de localisation que la vue Amiga.

### L — Traductions, aide et accessibilité

- [ ] **ATA-035 — Ajouter toutes les ressources Atari.** Créer les clés dans les fichiers appropriés et fournir les traductions pour toutes les langues actuellement prises en charge, sans texte visible écrit en dur.
- [ ] **ATA-036 — Intégrer Atari dans l’aide.** Documenter modèles, firmwares, médias, contrôles, états et limites, puis inclure les PDF disponibles avec repli vers l’anglais.
- [ ] **ATA-037 — Vérifier accessibilité et mise en page.** Contrôler navigation clavier, lecteurs d’écran, contrastes, textes longs, RTL, polices CJK et absence de débordement ou barre inutile.

### M — Packaging et distribution

- [ ] **ATA-038 — Intégrer le projet et les dépendances aux builds.** Mettre les DLL `gwgui` à la racine de `lib`, ranger chaque bibliothèque tierce dans son propre sous-dossier et mettre à jour le résolveur sans casser le démarrage.
- [ ] **ATA-039 — Mettre à jour portable, installateur et CI.** Inclure code, ressources, documentation et installateur .NET déjà prévu dans les deux distributions, sans inclure les cœurs, firmwares ou médias non autorisés.

### N — Tests et validation fonctionnelle

- [ ] **ATA-040 — Tester domaine et persistance.** Couvrir catalogues, compatibilités, migrations, firmwares, chemins, médias et erreurs pour toutes les familles.
- [ ] **ATA-041 — Tester les six adaptateurs.** Couvrir chargement/déchargement, options, vidéo, audio, entrées, médias, états et erreurs avec doubles contrôlés puis DLL réelles autorisées.
- [ ] **ATA-042 — Tester l’interface et les traductions.** Vérifier changement de modèle, grisage, ressources des 28 langues, RTL, commandes et absence de texte anglais résiduel hors valeurs techniques.
- [ ] **ATA-043 — Effectuer la validation manuelle par famille.** Démarrer au moins une machine Hatari, Atari800, 2600, 7800, Lynx, Jaguar et Jaguar CD lorsque les firmwares/médias de test légaux sont disponibles ; consigner précisément tout test bloqué.

### O — Audit final

- [ ] **ATA-044 — Exécuter la validation complète.** Lancer compilation Release, suite de tests, création portable, création installateur, contrôle de démarrage et vérification qu’aucun processus `dotnet` ou hôte Atari n’est laissé après fermeture.
- [ ] **ATA-045 — Auditer et finaliser la feuille.** Vérifier que chaque exigence de ce document correspond à du code, des tests ou une limite documentée ; contrôler le dépôt complet, cocher cette tâche en dernier et préparer un commit descriptif sans fichier orphelin.

## Critère de fin

L’intégration Atari est terminée uniquement lorsque les six cœurs sont gérés par le même parcours utilisateur, que chaque machine expose uniquement ses capacités réelles, que les distributions démarrent et que toutes les tâches ci-dessus sont cochées après validation.
