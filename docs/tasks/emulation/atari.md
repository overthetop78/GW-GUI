# Émulation Atari — travaux restants

## Objectif

Intégrer à GW GUI les six cœurs Atari retenus, avec le même niveau d’intégration que l’émulation Amiga : installation et remplacement des cœurs, configurations persistantes, firmwares, médias, vidéo, audio, entrées, états, interface traduite, documentation, packaging et tests.

| Cœur | Machines visées | Médias principaux |
|---|---|---|
| Hatari | ST, STE, TT, Falcon ; préréglages GW GUI explicites pour STF, STFM, Mega ST et Mega STE | disquette, disque dur, dossier GEMDOS |
| Atari800 | 400/800, 800XL, 130XE, XL/XE modernes 320/576/1088 Kio, XEGS, 5200 | disquette, cassette, cartouche |
| Stella 2023 | 2600 | cartouche |
| ProSystem | 7800 | cartouche |
| Beetle Lynx | Lynx | cartouche |
| Virtual Jaguar | Jaguar, Jaguar CD | cartouche ; image CD `.cue` ou `.cdi` chargée comme contenu principal |

Cette matrice exprime la cible fonctionnelle. Les formats, options et limites exacts de chaque cœur doivent être confirmés à partir de son code et de ses métadonnées pendant les premières tâches, sans inventer de capacité absente.

## Règles d’exécution

- Exécuter les tâches dans l’ordre de leurs identifiants.
- Ne cocher une tâche qu’après son implémentation complète et sa validation.
- Après chaque tâche : exécuter les contrôles ciblés disponibles et `git diff --check`, puis seulement remplacer `[ ]` par `[x]`.
- Ne jamais ajouter de ROM, BIOS, TOS, cartouche, disque ou CD protégé au dépôt ou aux packages.
- `GWGUI.App` ne doit jamais appeler directement une fonction `retro_*`.
- Chaque instance de cœur s’exécute dans un processus hôte isolé, comme pour Amiga.
- Le modèle choisi détermine automatiquement le cœur ; l’utilisateur ne choisit pas un cœur technique.
- Une option sans effet pour la machine choisie reste visible mais grisée, avec une explication traduite.
- Tous les textes visibles doivent provenir des ressources de traduction, dans toutes les langues prises en charge.
- Toute version proposée par l’interface de gestion des cœurs doit pouvoir être téléchargée et remplacer la version installée ; taille, empreinte et en-tête restent des diagnostics, pas des motifs de rejet.


## Travail restant

### Accessibilité et mise en page

#### ATA-051 — Vérifier accessibilité et mise en page

- [ ] Définir noms accessibles, ordre de tabulation, raccourcis et focus initial.
- [ ] Vérifier contraste, états grisés, indicateurs qui ne reposent pas seulement sur la couleur et zoom Windows.
- [ ] Vérifier absence de débordement, texte coupé et barre de défilement inutile.
- [ ] Vérifier langues RTL, CJK et chaînes longues sur tous les onglets Atari.
- [ ] Vérifier la vue de machine aux tailles minimales et en plein écran.
- [ ] Ajouter des tests automatisables et consigner les vérifications manuelles restantes.

### M — Packaging et distribution

#### ATA-052 — Intégrer le projet aux sorties de build

- [ ] Inclure l’assembly Atari et ses symboles selon les mêmes règles que les assemblies `gwgui` existantes.
- [ ] Placer les DLL créées par le projet à la racine de `lib` avec leur nom `gwgui` en minuscules.
- [ ] Ranger chaque dépendance tierce dans son propre sous-dossier de `lib`, jamais dans un dossier générique.
- [ ] Mettre à jour le résolveur d’assemblies et de DLL natives pour ces chemins.
- [ ] Ne pas incorporer les assemblies du projet dans l’exécutable.
- [ ] Tester le démarrage depuis une sortie propre et l’absence de doublons à la racine.

#### ATA-053 — Mettre à jour le portable

- [ ] Inclure code Atari, ressources et installateur .NET prévu dans le ZIP portable.
- [ ] Exclure les fichiers temporaires.
- [ ] Exclure les six cœurs téléchargés, firmwares et médias privés sauf décision de licence explicitement validée.
- [ ] Vérifier au lancement le runtime .NET comme le fait la distribution actuelle.
- [ ] Tester sur un dossier neuf, avec et sans runtime déjà installé.
- [ ] Vérifier installation d’un cœur puis démarrage après extraction dans un chemin avec espaces.

#### ATA-054 — Mettre à jour l’installateur

- [ ] Inclure les mêmes fichiers applicatifs que le portable.
- [ ] Vérifier le runtime .NET avant installation et lancer l’installateur embarqué seulement s’il manque réellement.
- [ ] Créer les dossiers de données sans écraser configurations, firmwares, cœurs ou médias existants.
- [ ] Mettre à niveau une installation précédente sans conserver de DLL obsolète en doublon.
- [ ] Désinstaller uniquement les fichiers appartenant au produit.
- [ ] Tester installation propre, mise à niveau, réparation, désinstallation et relance.

#### ATA-055 — Mettre à jour la CI et les releases

- [ ] Restaurer, compiler et tester le projet Atari dans les workflows existants sans build à chaque commit non demandé.
- [ ] Inclure portable et installateur Atari dans les builds snapshot et stables déclenchés manuellement.
- [ ] Ne télécharger aucun firmware ou média privé pendant la CI.
- [ ] Mettre en cache uniquement les dépendances sûres sans publier les cœurs comme artefacts involontaires.
- [ ] Vérifier les noms, versions, manifests et contenus des deux packages.
- [ ] Conserver le mécanisme snapshot/stable existant sans créer une release à chaque push.

### N — Tests et validation fonctionnelle

#### ATA-056 — Tester domaine, catalogues et persistance

- [ ] Couvrir tous les modèles, associations de cœur et règles de compatibilité par jeux de données.
- [ ] Couvrir tous les firmwares, statuts et choix compatibles/incompatibles.
- [ ] Couvrir tous les slots, types de médias, chemins relatifs/absolus et validations.
- [ ] Couvrir sauvegarde, chargement, migration, corruption et suppression des configurations.
- [ ] Couvrir codes d’erreur et messages structurés sans se contenter de `Unknown`.
- [ ] Vérifier que les tests ordinaires ne dépendent d’aucun binaire, firmware ou média local.

#### ATA-057 — Tester l’ABI et les hôtes des six cœurs

- [ ] Créer des doubles natifs minimaux pour les profils d’environnement différents.
- [ ] Couvrir chargement, exports, ordre d’appel, options, vidéo, audio, entrée, médias et états.
- [ ] Couvrir pointeurs, allocations, GC forcé, structures x64 et double libération.
- [ ] Couvrir protocole, mémoire partagée, pipe, timeout, annulation, crash et arrêt.
- [ ] Exécuter séparément les tests locaux contre chaque DLL officielle disponible.
- [ ] Vérifier qu’aucun test local n’est présenté comme réussi lorsqu’un prérequis manque.
- [ ] Tester les six services de versions et installateurs avec réponses réseau simulées, archives invalides et remplacements.

#### ATA-058 — Tester l’interface et les traductions

- [ ] Tester création, sélection, sauvegarde, suppression et ouverture de configurations Atari.
- [ ] Tester chaque changement de modèle et chaque contrôle activé, imposé ou grisé.
- [ ] Tester barre de machine, médias, raccourcis, plein écran, capture souris et erreurs.
- [ ] Comparer les clés des 28 langues et détecter les chaînes visibles codées en dur.
- [ ] Vérifier RTL, CJK, textes longs, accessibilité et tailles minimales.
- [ ] Rejouer les tests de non-régression de tous les contrôles Amiga et communs.

#### ATA-059 — Valider Hatari manuellement

- [ ] Démarrer légalement au moins un ST/STF/STFM, un STE, un TT et un Falcon quand les TOS requis sont disponibles.
- [ ] Vérifier boot, vidéo, audio, clavier, souris, joystick, pause, reset et arrêt.
- [ ] Vérifier disquette, changement de disque, disque dur et dossier GEMDOS selon support réel.
- [ ] Vérifier sauvegarde/chargement d’état ou indisponibilité explicite.
- [ ] Consigner firmware ou média légal manquant sans cocher le cas non exécuté.

#### ATA-060 — Valider Atari800 manuellement

- [ ] Démarrer légalement un ordinateur 400/800/XL/XE/XEGS représentatif et une 5200.
- [ ] Vérifier boot, vidéo, audio, clavier console, joysticks et quatre ports lorsqu’ils sont disponibles.
- [ ] Vérifier disquette, cassette et cartouche sur les modèles concernés.
- [ ] Vérifier isolation des BIOS ordinateur/5200, états et arrêt sans verrou.
- [ ] Consigner tout prérequis manquant sans transformer son absence en succès.

#### ATA-061 — Valider les consoles à cartouche manuellement

- [ ] Démarrer légalement une 2600 avec Stella et vérifier région, vidéo, audio et contrôleur.
- [ ] Démarrer légalement une 7800 avec ProSystem et vérifier BIOS éventuel et contrôleurs.
- [ ] Démarrer légalement une Lynx avec Beetle Lynx et vérifier orientation, rotation et entrées si exposées.
- [ ] Démarrer légalement une Jaguar avec Virtual Jaguar et vérifier BIOS, vidéo, audio et pavé de contrôleur.
- [ ] Vérifier états, remplacement de cartouche par redémarrage et arrêt propre pour chaque cœur.

#### ATA-062 — Valider Jaguar CD manuellement

- [ ] Confirmer avec la DLL choisie que Jaguar CD peut réellement démarrer.
- [ ] Démarrer avec BIOS et disque légaux lorsque disponibles.
- [ ] Vérifier lecture du disque, audio CD éventuel, commandes, états et arrêt.
- [ ] Vérifier qu’une Jaguar standard ne reçoit pas le lecteur CD.
- [ ] Si le cœur retenu ne supporte pas Jaguar CD, documenter la preuve et proposer un cœur distinct avant toute substitution.

### O — Audit final

#### ATA-063 — Exécuter la validation complète

- [ ] Compiler toute la solution en Debug x64 puis Release x64 depuis une sortie propre.
- [ ] Exécuter tous les tests ordinaires puis chaque suite locale dont les prérequis légaux existent.
- [ ] Boucler 100 démarrages/arrêts sur un représentant de chacun des six cœurs.
- [ ] Exécuter une session longue par famille et relever mémoire, handles, audio, vidéo et dérive.
- [ ] Créer portable et installateur puis tester leur démarrage et leur gestion du runtime .NET.
- [ ] Vérifier après fermeture l’absence de processus `dotnet`, `gwgui` hôte, pipe, mapping et fichier verrouillé.
- [ ] Vérifier qu’aucun firmware, média, chemin personnel ou secret n’est suivi ou emballé.
- [ ] Vérifier qu’aucun appel `retro_*` n’existe dans `GWGUI.App` ni dans les contrats communs publics.

#### ATA-064 — Auditer et finaliser la feuille

- [ ] Relire chaque ticket et chaque sous-tâche contre le code, les tests et les artefacts actuels.
- [ ] Considérer toute preuve absente, indirecte ou non exécutée comme non terminée.
- [ ] Vérifier que les six cœurs suivent le même parcours utilisateur sans masquer leurs différences réelles.
- [ ] Vérifier toutes les traductions, l’aide, le portable, l’installateur et la CI.
- [ ] Exécuter `git diff --check` et contrôler tous les fichiers du dépôt sans fichier orphelin.
- [ ] Cocher cette dernière sous-tâche uniquement après que toutes les autres cases sont réellement cochées.
- [ ] Créer un commit descriptif complet et laisser le dépôt propre.

## Critère de fin

L’intégration Atari est terminée uniquement lorsque les six cœurs sont gérés par le même parcours utilisateur, que chaque machine expose uniquement ses capacités réelles, que les distributions démarrent et que toutes les tâches ci-dessus sont cochées après validation.

## À faire plus tard quand moi je te le dirai

Les tâches de cette section sont volontairement différées. Elles ne doivent être reprises, cochées ou utilisées pour bloquer les tâches Atari non documentaires qu’après instruction explicite.

### ATA-065 — Ajouter Atari System 1 avec MAME et FBNeo

- [ ] Ajouter les cœurs libretro MAME et FBNeo à la gestion des moteurs, au téléchargement, à la détection de version et au lancement.
- [ ] Ajouter Atari System 1 comme famille arcade Atari distincte des consoles et ordinateurs Atari.
- [ ] Déterminer pour chaque jeu si MAME, FBNeo ou les deux le prennent réellement en charge, sans annoncer une compatibilité non vérifiée.
- [ ] Détecter `atarisy1.zip` comme ensemble de ROM/firmware Atari System 1 et le ranger dans l’emplacement arcade approprié, jamais dans un emplacement TOS ou BIOS de console.
- [ ] Recenser les ROM système, ROM de jeu, clés, PROM et échantillons éventuellement requis par chaque cœur et chaque version de catalogue.
- [ ] Ajouter les réglages, médias, contrôleurs, statuts de compatibilité et messages d’erreur propres à Atari System 1.
- [ ] Ajouter les traductions dans toutes les langues distribuées.
- [ ] Tester installation des deux cœurs, audit des ROM, démarrage de jeux compatibles, erreurs de jeux incomplets et absence de régression des machines Atari existantes.

### ATA-050 — Intégrer Atari au guide utilisateur

- [ ] Ajouter architecture utilisateur, création de configuration et installation des cœurs.
- [ ] Documenter chaque famille, ses firmwares, ses médias et ses limites réelles.
- [ ] Documenter réglages CPU, RAM, ROM, vidéo, audio, stockage et entrées.
- [ ] Documenter exécution, raccourcis, états, changement de média et erreurs courantes.
- [ ] Créer des captures actuelles cadrées correctement sans données personnelles.
- [ ] Traduire les sources Markdown du guide dans toutes les langues distribuées.
- [ ] Générer les PDF compressés disponibles et conserver les sources Markdown.
- [ ] Ouvrir le PDF de la langue active avec repli vers l’anglais s’il manque.
- [ ] Inclure tous les PDF du guide dans le ZIP portable, sans sources Markdown ni images séparées.
- [ ] Inclure tous les PDF du guide dans l’installateur, sans sources Markdown ni images séparées.
