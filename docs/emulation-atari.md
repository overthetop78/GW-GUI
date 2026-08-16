# Émulation Atari — feuille d’exécution complète

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
- Après chaque tâche : lancer les tests ciblés, exécuter `git diff --check`, puis seulement remplacer `[ ]` par `[x]`.
- Ne jamais ajouter de ROM, BIOS, TOS, cartouche, disque ou CD protégé au dépôt ou aux packages.
- `GWGUI.App` ne doit jamais appeler directement une fonction `retro_*`.
- Chaque instance de cœur s’exécute dans un processus hôte isolé, comme pour Amiga.
- Le modèle choisi détermine automatiquement le cœur ; l’utilisateur ne choisit pas un cœur technique.
- Une option sans effet pour la machine choisie reste visible mais grisée, avec une explication traduite.
- Tous les textes visibles doivent provenir des ressources de traduction, dans toutes les langues prises en charge.
- Toute version proposée par l’interface de gestion des cœurs doit pouvoir être téléchargée et remplacer la version installée ; taille, empreinte et en-tête restent des diagnostics, pas des motifs de rejet.

## Architecture cible

- `GWGUI.Emulation.Atari` contient le domaine Atari, les catalogues, la persistance, l’installation des cœurs et les adaptateurs de moteurs externes.
- `GWGUI.App` contient uniquement la présentation, les fournisseurs applicatifs, la préparation des médias et le démarrage des hôtes.
- Le protocole hôte reprend le modèle Amiga et transporte commandes, vidéo, audio, entrées, médias, état et erreurs structurées.
- Les abstractions réellement communes à Amiga et Atari sont extraites sans créer de dépendance entre les deux domaines.
- Les bibliothèques propres au projet restent dans `lib`; les dépendances tierces suivent le rangement déjà défini par bibliothèque.

## Matrice obligatoire des fichiers Amiga à couvrir

Cette matrice empêche d’oublier un morceau déjà nécessaire à Amiga. Un fichier Atari peut être découpé différemment si la responsabilité reste couverte et testée ; il ne doit pas être créé comme simple copie si un composant commun convient réellement.

| Référence Amiga actuelle | Responsabilité Atari attendue |
|---|---|
| `IAmigaMachine.cs` | contrat public `IAtariMachine` |
| `AmigaEngine.cs` | factory du cœur déterminé par le modèle |
| `AmigaMachine.cs` | cycle de vie, boucle et commandes de la machine |
| `AmigaMachineConfiguration.cs` | document Atari complet et versionné |
| `AmigaConfigurationStore.cs` | chargement, sauvegarde atomique, migration et suppression |
| `AmigaModelCatalog.cs` | catalogues ST, Atari 8 bits et consoles |
| `AmigaFirmwareCatalog.cs` | TOS, BIOS, ROM système et compatibilités |
| `AmigaCoreOption.cs` | catégories, valeurs, défauts et visibilité des options des six cœurs |
| `AmigaStateStore.cs` | états rapides et nommés, captures et métadonnées |
| `AmigaExternalCoreInstaller.cs` | installation et remplacement atomique par cœur |
| `AmigaCoreReleaseService.cs` | recherche des versions officielles des six cœurs |
| `Cores/IAmigaCore.cs` | interface interne indépendante d’une DLL précise |
| `Common/ExternalCoreApi.cs` | ABI commune des moteurs externes |
| `Cores/AmigaExternalCore.cs` | adaptateur natif sélectionnant l’un des six cœurs |
| `Cores/AmigaExternalHostCallbacks.cs` | environnement, options, chemins, vidéo, audio, entrée, logs et LED |
| `Cores/AmigaExternalDiskControl.cs` | contrôleurs de disquette/CD réellement exposés |
| `Cores/AmigaInputAccumulator.cs` | accumulation et consommation des entrées par frame |
| `Cores/AmigaProcessCore.cs` | proxy vers un hôte isolé par machine |
| `Cores/AmigaCoreHostProtocol.cs` | commandes, réponses et sérialisation IPC Atari |
| `App.xaml.cs` / `AmigaCoreHost.Run` | mode `--atari-core-host` et arrêt sans fenêtre |
| `AmigaConfigurationDocuments.cs` | validation et documents Atari côté application |
| `AmigaCoreManagementSection.cs` | recherche, sélection, téléchargement et remplacement du cœur requis |
| `AmigaEmulationSection.cs` | choix de configuration et onglets des machines Atari |
| `AmigaMachineView.cs` | surface, barre d’outils, médias, états, entrées, plein écran et captures |
| `OptionsEmulationSection.cs` | paramètres Atari et sous-onglets par modèle |
| `AmigaCoreProvider.cs` | résolution et installation du cœur avant démarrage |
| `AmigaRuntimeMedia.cs` | préparation non destructive des médias et fichiers auxiliaires |
| `AmigaKeyMapper.cs` | adaptation clavier WPF vers touches d’émulation Atari |
| `EmulationShortcutMap.cs` | actions globales et spécifiques Atari |
| `EmulationResourceKeys.cs` et `Emulation.resx` | toutes les clés et les 28 traductions |
| `EmulationVideoSurface.cs` | rendu partagé, sans appel `retro_*` dans l’application |
| tests `Amiga*` et `EmulationControlRefactoringTests` | équivalents Atari plus non-régression Amiga |

## Sources officielles verrouillées

Les références ci-dessous ont été vérifiées le 16 août 2026 directement dans les dépôts officiels Libretro, leurs fichiers `Makefile`, le dépôt officiel `libretro-core-info` et le buildbot Libretro Windows x64. Les archives du répertoire `latest` sont roulantes : les révisions indiquées sont celles des sources inspectées et ne sont pas présentées comme les révisions exactes des binaires servis par le buildbot.

Révision de métadonnées commune inspectée : [`libretro/libretro-core-info`](https://github.com/libretro/libretro-core-info) à `f105af2925f70f2d72a8676d04a5f2282c1d01ba` (commit du 1er août 2026).

| Cœur | Dépôt officiel et révision inspectée | Documentation / métadonnées officielles | Licence déclarée par Libretro | DLL Windows x64 / archive buildbot |
|---|---|---|---|---|
| Hatari | [`libretro/hatari`](https://github.com/libretro/hatari), `24e7bd744f24f20b464385f365a3850c269bd140` (11 août 2026) | [documentation](https://docs.libretro.com/library/hatari/) / [`hatari_libretro.info`](https://github.com/libretro/libretro-core-info/blob/master/hatari_libretro.info) | GPLv2 | `hatari_libretro.dll` / [`hatari_libretro.dll.zip`](https://buildbot.libretro.com/nightly/windows/x86_64/latest/hatari_libretro.dll.zip) |
| Atari800 | [`libretro/libretro-atari800`](https://github.com/libretro/libretro-atari800), `cd721790a0aa0e0772810949abcf5bd699c15371` (15 août 2026) | [documentation](https://docs.libretro.com/library/atari800/) / [`atari800_libretro.info`](https://github.com/libretro/libretro-core-info/blob/master/atari800_libretro.info) | GPLv2 | `atari800_libretro.dll` / [`atari800_libretro.dll.zip`](https://buildbot.libretro.com/nightly/windows/x86_64/latest/atari800_libretro.dll.zip) |
| Stella 2023 | [`libretro/stella`](https://github.com/libretro/stella), `878a9c8d5f03ef0b7cd190b5713d6bf31c48df38` (16 juillet 2026) | [documentation](https://docs.libretro.com/library/stella/) / [`stella2023_libretro.info`](https://github.com/libretro/libretro-core-info/blob/master/stella2023_libretro.info) | GPLv2 | `stella2023_libretro.dll` / [`stella2023_libretro.dll.zip`](https://buildbot.libretro.com/nightly/windows/x86_64/latest/stella2023_libretro.dll.zip) |
| ProSystem | [`libretro/prosystem-libretro`](https://github.com/libretro/prosystem-libretro), `363b6dfbd3e240762e022c2b4897b4fe55722be3` (4 juin 2026) | [documentation](https://docs.libretro.com/library/prosystem/) / [`prosystem_libretro.info`](https://github.com/libretro/libretro-core-info/blob/master/prosystem_libretro.info) | GPLv2 | `prosystem_libretro.dll` / [`prosystem_libretro.dll.zip`](https://buildbot.libretro.com/nightly/windows/x86_64/latest/prosystem_libretro.dll.zip) |
| Beetle Lynx | [`libretro/beetle-lynx-libretro`](https://github.com/libretro/beetle-lynx-libretro), `fcdefcfb3c11d6d2e71be076a5d3df2e88ab73ed` (20 avril 2026) | [documentation](https://docs.libretro.com/library/beetle_lynx/) / [`mednafen_lynx_libretro.info`](https://github.com/libretro/libretro-core-info/blob/master/mednafen_lynx_libretro.info) | zlib et GPLv2 | `mednafen_lynx_libretro.dll` / [`mednafen_lynx_libretro.dll.zip`](https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_lynx_libretro.dll.zip) |
| Virtual Jaguar | [`libretro/virtualjaguar-libretro`](https://github.com/libretro/virtualjaguar-libretro), `385c4d458538fd473c4bc8dc8dab4778897e8ac6` (13 août 2026) | [documentation](https://docs.libretro.com/library/virtual_jaguar/) / [`virtualjaguar_libretro.info`](https://github.com/libretro/libretro-core-info/blob/master/virtualjaguar_libretro.info) | GPLv3 | `virtualjaguar_libretro.dll` / [`virtualjaguar_libretro.dll.zip`](https://buildbot.libretro.com/nightly/windows/x86_64/latest/virtualjaguar_libretro.dll.zip) |

### Obtention et compilation

- Méthode binaire commune : télécharger l’archive correspondante depuis le répertoire officiel [`nightly/windows/x86_64/latest`](https://buildbot.libretro.com/nightly/windows/x86_64/latest/), puis extraire la DLL. Les six URL directes du tableau ont répondu avec le statut HTTP 200 lors de la vérification.
- Hatari : depuis la racine du dépôt, `make -f Makefile.libretro EXTERNAL_ZLIB=1`, conformément au `README.md` officiel.
- Atari800 : depuis la racine du dépôt, `make -f Makefile`; le dépôt documente aussi les surcharges Libretro, notamment `platform=win`.
- Stella 2023 : utiliser `src/os/libretro/Makefile` ; pour Windows, ce Makefile produit `stella2023_libretro.dll` et accepte les variantes de plateforme Windows qu’il déclare.
- ProSystem : depuis la racine du dépôt, utiliser le `Makefile` Libretro avec `platform=win` pour produire `prosystem_libretro.dll`.
- Beetle Lynx : depuis la racine du dépôt, utiliser le `Makefile` Libretro avec `platform=win` pour produire `mednafen_lynx_libretro.dll`.
- Virtual Jaguar : depuis la racine du dépôt, `make`; la plateforme est détectée automatiquement et le `README.md` officiel annonce `virtualjaguar_libretro.dll` comme sortie Windows.

Les licences du tableau sont les licences globales publiées dans les fichiers `.info`. Beetle Lynx est le seul des six à y déclarer explicitement deux licences (`Zlib|GPLv2`). Les dépôts peuvent aussi contenir des composants tiers sous leurs propres avis ; leur inventaire distributif complet sera contrôlé lors du packaging, sans remplacer ici la licence officielle déclarée pour chaque cœur.

## Tâches

### A — Sources et capacités réelles

#### ATA-001 — Figer les sources des six cœurs

- [x] Enregistrer séparément les dépôts officiels de Hatari, Atari800, Stella, ProSystem, Beetle Lynx et Virtual Jaguar.
- [x] Enregistrer pour chaque cœur la page de documentation Libretro et le fichier `.info` officiel.
- [x] Relever la licence exacte de chaque cœur et les éventuelles licences multiples de ses composants.
- [x] Relever le nom exact de chaque DLL Windows x64 et de son archive buildbot.
- [x] Documenter la méthode officielle de téléchargement du binaire et la méthode de compilation depuis les sources.
- [x] Enregistrer la révision inspectée et la date de consultation sans prétendre qu’elle est celle d’un binaire non vérifié.
- [x] Vérifier tous les liens et toutes les valeurs contre les sources primaires avant de cocher ce ticket.

#### ATA-002 — Établir la matrice vérifiée des capacités

Documents de preuves vérifiés : [`atari-core-capabilities.md`](atari-core-capabilities.md) et inventaire exhaustif [`atari-core-options.md`](atari-core-options.md).

- [x] Relever les modèles réellement sélectionnables par chacun des six cœurs.
- [x] Relever les extensions de contenu, `need_fullpath` et la possibilité de démarrer sans contenu.
- [x] Relever les firmwares obligatoires, facultatifs et intégrés ainsi que leurs noms attendus.
- [x] Relever toutes les Core Options, leurs valeurs, leurs défauts et leurs conditions de visibilité.
- [x] Relever les périphériques déclarés par port et les identifiants d’entrée réellement interrogés.
- [x] Vérifier séparément Disk Control standard, Disk Control étendu et sous-systèmes Libretro.
- [x] Vérifier `retro_serialize`, `retro_unserialize`, mémoire sauvegardée et limites des états.
- [x] Relever formats de pixels, géométries variables, régions, fréquences vidéo et taux audio.
- [x] Consigner les limites connues sans transformer une hypothèse ou une fonction de l’émulateur autonome en capacité du cœur Libretro.
- [x] Corriger la matrice cible de ce document avec les résultats prouvés.

### B — Contrats communs d’émulation

#### ATA-003 — Généraliser les types de médias

- [x] Ajouter `Cartridge` et `Cassette` à `EmulationMediaType` sans modifier les valeurs persistées existantes.
- [x] Ajouter des slots explicites de cartouche et cassette ; étendre les slots CD et disque uniquement si la matrice l’exige.
- [x] Définir les règles d’unicité, d’éjection, de lecture seule et de remplacement pour chaque type.
- [x] Mettre à jour la sérialisation du protocole et les configurations sans casser les documents Amiga existants.
- [x] Ajouter les tests de compatibilité ascendante, de round-trip et de slot invalide.

#### ATA-004 — Extraire uniquement les éléments des moteurs externes réellement communs

- [x] Comparer toutes les structures et constantes du contrat externe Amiga existant à l’API nécessaire aux six cœurs Atari.
- [x] Déplacer dans `GWGUI.Emulation` uniquement l’ABI des moteurs externes indépendante d’une machine.
- [x] Mutualiser le chargement et la résolution des exports sans exposer `retro_*` publiquement.
- [x] Mutualiser les allocations UTF-8, la copie vidéo et les primitives de sérialisation IPC réellement identiques.
- [x] Conserver options, firmwares, médias, contrôleurs et règles de modèle dans les projets spécialisés.
- [x] Prouver par tests que l’intégration Amiga continue de charger, fonctionner et se libérer après l’extraction.

### C — Domaine Atari et processus hôte

#### ATA-005 — Créer le projet `GWGUI.Emulation.Atari`

- [x] Créer le projet en `net10.0` avec nullable et implicit usings, sans WPF ni package d’interface.
- [x] Référencer uniquement `GWGUI.Emulation` et les dépendances strictement nécessaires.
- [x] Ajouter le projet à `GWGUI.sln`, à `GWGUI.App` et aux tests sans dépendance inverse vers l’application.
- [x] Fixer assembly, namespace et fichiers produits selon la convention `gwgui` du package.
- [x] Faire passer une compilation x64 Debug et Release avant de cocher.

#### ATA-006 — Définir les contrats Atari

- [x] Créer `IAtariMachine` avec le cycle de vie commun, événements vidéo et commandes Atari.
- [x] Créer `IAtariCore` pour isoler la machine du cœur natif concret.
- [x] Créer `AtariCoreKind` et les six identifiants de cœur sans les déduire du nom de DLL.
- [x] Créer `AtariMachineConfiguration` avec version de schéma, modèle, cœur déterminé, firmwares, médias, options et entrées.
- [x] Créer les valeurs de modèle, famille, firmware, média et périphérique sans dépendance WPF.
- [x] Créer les erreurs structurées pour cœur, firmware, contenu, option, hôte et état.
- [x] Tester les invariants des contrats et le refus des combinaisons manifestement incohérentes.

#### ATA-007 — Implémenter l’adaptateur des moteurs externes Atari

- [x] Déclarer les delegates Cdecl, structures séquentielles et marshaling booléen requis en x64.
- [x] Tester tailles et offsets natifs de toutes les structures utilisées.
- [x] Refuser les chemins relatifs et produire une erreur structurée si la DLL ou un export manque.
- [x] Résoudre l’ensemble des exports requis et vérifier la version d’ABI attendue.
- [x] Installer les callbacks environnement, vidéo, audio et entrée dans l’ordre imposé par l’API.
- [x] Vérifier `retro_system_info` contre le cœur attendu au lieu de se fier au nom du fichier.
- [x] Charger le contenu avec `retro_game_info` selon `need_fullpath` et les extensions annoncées.
- [x] Nettoyer uniquement les étapes initialisées, rendre le second arrêt inoffensif et libérer le module une seule fois.
- [x] Exécuter ces tests séparément pour les six DLL.

#### ATA-008 — Implémenter l’hôte Atari isolé

- [x] Ajouter `--atari-core-host` au démarrage de `gwgui.exe` sans ouvrir l’interface principale.
- [x] Définir commandes, réponses, versions et erreurs du protocole Atari.
- [x] Sérialiser configuration, entrées, médias, options, états et statuts sans envoyer de type WPF.
- [x] Transporter une requête à la fois sur un named pipe privé et répondre avant la suivante.
- [x] Transporter la dernière frame par mémoire partagée redimensionnable et les blocs audio sans corruption.
- [x] Créer un processus distinct par machine, même lorsque deux machines utilisent le même cœur.
- [x] Gérer connexion, timeout, annulation, fermeture normale, crash et processus bloqué.
- [x] Ne tuer que l’hôte fautif et laisser fonctionner les autres machines.
- [x] Vérifier qu’aucun processus, pipe, mapping ou fichier temporaire ne reste après arrêt.

### D — Installation et versions des cœurs

#### ATA-009 — Créer le catalogue des six cœurs

- [x] Associer à chaque identifiant le nom de bibliothèque, le nom de DLL, l’archive et la source officielle.
- [x] Associer chaque modèle à exactement un cœur et refuser une association ambiguë.
- [x] Définir les dossiers installés et manifestes séparément pour chaque cœur et chaque version.
- [x] Enregistrer URL, date, taille ZIP, taille DLL, SHA-256 calculé, architecture et version déclarée comme diagnostic.
- [x] Tester les six associations, chemins et noms sans accès réseau.

#### ATA-010 — Implémenter recherche, téléchargement et remplacement

- [x] Interroger la source officielle de chaque cœur et parser toutes les versions qu’elle propose.
- [x] Afficher les erreurs réseau et de format sans masquer leur cause technique utile.
- [x] Télécharger dans un fichier temporaire avec progression et annulation.
- [x] Extraire uniquement la DLL attendue et refuser une archive qui ne la contient pas.
- [x] Autoriser l’installation de toute version proposée par l’interface.
- [x] Remplacer atomiquement la version installée lorsque l’utilisateur en choisit une autre.
- [x] Calculer taille, SHA-256, architecture et exports pour le manifeste et le diagnostic, sans bloquer une version proposée sur ces diagnostics.
- [x] Nettoyer les temporaires après succès, annulation ou erreur.
- [x] Tester hors ligne, archive tronquée, DLL absente, remplacement, fichier verrouillé et annulation.

#### ATA-011 — Ajouter l’interface de gestion des cœurs Atari

- [x] Afficher le cœur requis automatiquement pour le modèle sélectionné.
- [x] Afficher version installée, absence d’installation et chemin local sans ambiguïté.
- [x] N’afficher la liste et le bouton de téléchargement qu’après une recherche réussie.
- [x] Permettre de choisir chaque version retournée puis de la télécharger et remplacer.
- [x] Afficher progression, annulation, succès et erreur détaillée avec ressources traduites.
- [x] Actualiser la configuration et l’état installé après remplacement sans redémarrage des machines déjà actives.
- [x] Tester le contrôle avec un faux service pour chacun des six cœurs.

### E — Modèles et compatibilité matérielle

#### ATA-012 — Cataloguer la famille ST

- [x] Définir séparément ST, STF, STFM, Mega ST, STE, Mega STE, TT et Falcon.
- [x] Associer CPU, FPU, fréquences et niveaux de précision réellement disponibles.
- [x] Associer tailles et types de RAM compatibles par modèle.
- [x] Associer TOS, région, vidéo, audio, stockage et ports compatibles.
- [x] Traduire les noms descriptifs tout en conservant les identifiants techniques stables.
- [x] Ajouter une ligne de test exhaustive par modèle.

#### ATA-013 — Cataloguer les Atari 8 bits et consoles

- [x] Définir 400, 800, XL, XE et XEGS avec leurs variantes réellement supportées.
- [x] Définir séparément 5200, 2600, 7800, Lynx, Jaguar et Jaguar CD.
- [x] Associer automatiquement Atari800, Stella, ProSystem, Beetle Lynx ou Virtual Jaguar.
- [x] Associer CPU, mémoire, région, vidéo, audio, stockage et ports vérifiés.
- [x] Empêcher qu’un modèle reçoive un firmware ou média d’une autre famille.
- [x] Ajouter une ligne de test exhaustive par modèle et variante.

#### ATA-014 — Centraliser les règles d’activation

- [x] Créer un catalogue unique de compatibilité consulté par moteur et interface.
- [x] Déterminer pour chaque modèle les onglets et groupes visibles.
- [x] Déterminer pour chaque option si elle est modifiable, imposée ou indisponible.
- [x] Déterminer slots, types de médias et nombre de ports utilisables.
- [x] Fournir pour chaque indisponibilité une clé de ressource explicative.
- [x] Tester toutes les règles par données, sans reproduire les conditions dans les contrôles WPF.

### F — Firmwares et ROM système

#### ATA-015 — Créer le catalogue des firmwares Atari

- [x] Cataloguer les TOS par version, région et modèle compatible.
- [x] Cataloguer les ROM Atari 8 bits et 5200 réellement utilisées par Atari800.
- [x] Cataloguer les BIOS facultatifs ou obligatoires des 2600, 7800 et Lynx.
- [x] Cataloguer les BIOS Jaguar et Jaguar CD ainsi que tout firmware de lecteur requis.
- [x] Enregistrer nom attendu, taille et empreintes publiques uniquement lorsqu’une source fiable les fournit.
- [x] Marquer clairement firmware requis, facultatif, intégré ou remplaçable.
- [x] Vérifier qu’aucun firmware protégé n’entre dans Git ou un package.

#### ATA-016 — Implémenter détection et sélection des firmwares

- [x] Créer les dossiers de firmware par famille sous le stockage d’émulation.
- [x] Scanner sans bloquer l’interface et ignorer proprement les fichiers non pertinents.
- [x] Identifier version et région par empreinte lorsqu’elles sont connues, sinon conserver un état inconnu explicite.
- [x] Classer chaque fichier compatible, partiellement compatible ou incompatible avec le modèle sélectionné.
- [x] Permettre sélection et rafraîchissement sans copier ni modifier le fichier source.
- [x] Refuser le démarrage avec un message précis lorsque le firmware réellement obligatoire manque.
- [x] Transmettre au cœur les chemins et noms attendus sans choisir silencieusement une autre ROM.
- [x] Tester dossier absent, doublons, fichier inconnu, mauvaise région, fichier verrouillé et firmware valide.

### G — Médias et stockage

#### ATA-017 — Implémenter les disquettes Hatari

- [x] Construire `retro_game_info` conformément à `need_fullpath` et conserver ses allocations jusqu’au déchargement.
- [x] Accepter uniquement les extensions réellement annoncées par Hatari et afficher les formats refusés.
- [x] Enregistrer le média dans le slot seulement après chargement réussi.
- [x] Capturer Disk Control standard ou étendu lorsqu’il est réellement fourni.
- [x] Implémenter insertion, éjection, remplacement, index et libellé de chaque image.
- [x] Gérer les listes multidisques et la rotation sans perdre l’ordre.
- [x] Rouvrir les fichiers en accès exclusif après arrêt pour prouver l’absence de handle restant.
- [x] Tester média absent, invalide, protégé en écriture et changement à chaud.
- [x] Si l’écriture est activée, travailler sur une copie de session et proposer explicitement l’enregistrement ; ne jamais modifier silencieusement l’image source.

#### ATA-018 — Implémenter les stockages Hatari

- [x] Gérer séparément images ACSI, images IDE et tout autre disque dur vérifié.
- [x] Gérer les dossiers GEMDOS sans confondre dossier hôte et faux fichier `.GEM`.
- [x] Préserver lecture seule, ordre de montage, lettres/partitions et chemins externes.
- [x] Valider les combinaisons de stockage en fonction du modèle ST/STE/TT/Falcon.
- [x] Restaurer les montages d’une configuration au démarrage et après remise sous tension.
- [x] Tester chemins avec espaces, dossier absent, image verrouillée, plusieurs volumes et arrêt propre.

#### ATA-019 — Implémenter les médias Atari800

- [x] Gérer les images de disquette avec leurs lecteurs et opérations réellement exposées.
- [x] Gérer les images cassette avec insertion, éjection, lecture et options moteur disponibles.
- [x] Gérer les cartouches d’ordinateur et distinguer leur type lorsque le cœur le demande.
- [x] Gérer les cartouches 5200 sans appliquer une configuration BIOS d’ordinateur.
- [x] Déterminer le type de contenu par le modèle et les métadonnées, pas seulement par extension ambiguë.
- [x] Tester chaque type, changement à chaud autorisé, contenu incompatible et arrêt sans verrou.
- [x] Appliquer aux disquettes et cassettes inscriptibles la même politique de copie de session et d’enregistrement explicite.

#### ATA-020 — Implémenter les cartouches 2600, 7800, Lynx et Jaguar

- [x] Créer un contrôleur de cartouche commun sans mélanger les règles des quatre cœurs.
- [x] Valider les extensions et besoins de chemin ou de données selon chaque cœur.
- [x] Transmettre type de mapper, région ou métadonnée uniquement lorsque le cœur le permet.
- [x] Implémenter insertion initiale, remplacement par remise sous tension et éjection si elle est supportée.
- [x] Conserver le média de chaque machine et restaurer uniquement sa propre cartouche.
- [x] Tester les quatre cœurs avec contenu factice ou légal, mauvais format et fichier verrouillé.

#### ATA-021 — Implémenter Jaguar CD

- [x] Vérifier d’abord que le cœur Virtual Jaguar retenu expose réellement Jaguar CD et ses formats.
- [x] Définir le slot CD et les fichiers descriptifs/feuilles nécessaires sans confondre une piste avec un disque complet.
- [x] Charger, éjecter et remplacer le CD selon les interfaces réellement fournies.
- [x] Associer BIOS et périphérique CD uniquement au modèle Jaguar CD.
- [x] Griser le lecteur CD sur Jaguar standard avec une explication traduite.
- [x] Tester image incomplète, piste manquante, changement autorisé et absence de support signalée proprement.

### H — Environnement des moteurs externes, exécution, vidéo et audio

#### ATA-022 — Répondre aux commandes d’environnement communes

- [x] Créer avant `retro_set_environment` les dossiers absolus System, Saves, Content et Assets nécessaires.
- [x] Retourner des pointeurs UTF-8 stables pendant toute la session et les libérer après `retro_deinit`.
- [x] Implémenter répertoires, duplication de frame, format de pixels, géométrie, informations AV et messages.
- [x] Copier immédiatement toute structure native reçue sans conserver son pointeur.
- [x] Accepter descripteurs d’entrée, informations contrôleurs, cartes mémoire et achievements lorsque demandés.
- [x] Journaliser une seule fois chaque commande inconnue et répondre honnêtement aux interfaces non implémentées.
- [x] Fournir l’interface de log sans interpréter la chaîne de format native comme une chaîne .NET ordinaire.
- [x] Capturer l’interface LED lorsqu’elle est fournie et transmettre ses changements au statut de machine.
- [x] Fournir une interface VFS seulement si un cœur l’exige réellement ; sinon retourner `false` sans simuler sa présence.
- [x] Traiter correctement langues, fast-forward, rotation, rumble et capteurs si un des six cœurs les demande.
- [x] Tester chaque numéro de commande utilisé par chacun des six cœurs avec buffers natifs.

#### ATA-023 — Héberger toutes les Core Options

- [x] Implémenter Core Options V2, V2 international, V1 et `SET_VARIABLES`.
- [x] Copier catégories, clés, libellés, aides, valeurs, labels, défauts et visibilité sans perdre une entrée.
- [x] Retourner la valeur configurée ou le défaut avec un pointeur stable dans `GET_VARIABLE`.
- [x] Implémenter `GET_VARIABLE_UPDATE`, `SET_VARIABLE` et le callback de mise à jour d’affichage.
- [x] Valider les valeurs contre celles annoncées tout en conservant les clés inconnues dans les documents.
- [x] Isoler les catalogues d’options par cœur et par instance.
- [x] Tester qu’aucune option annoncée par chacune des six DLL n’est perdue.

#### ATA-024 — Respecter l’ordre d’initialisation des six cœurs

- [x] Installer environnement puis callbacks vidéo, audio et entrée avant l’initialisation.
- [x] Appeler `retro_init`, lire `retro_system_info`, préparer firmware et contenu, puis appeler `retro_load_game`.
- [x] Ne démarrer sans contenu que si le cœur l’a explicitement autorisé.
- [x] Lire `retro_system_av_info` seulement après chargement réussi.
- [x] Configurer les périphériques de chaque port au moment attendu.
- [x] Nettoyer dans l’ordre inverse uniquement les étapes réussies.
- [x] Tester l’ordre exact avec un faux module pour chaque profil de cœur.

#### ATA-025 — Implémenter la boucle d’exécution Atari

- [x] Créer un thread LongRunning nommé avec l’identifiant de machine et le cœur.
- [x] Faire passer tous les appels natifs d’une instance par ce thread unique.
- [x] Utiliser une file mono-consommateur pour reset, option, média et état.
- [x] Exécuter une frame en état Running, aucune en pause, tout en continuant à traiter les commandes.
- [x] Rendre arrêt et double arrêt sûrs et annulables.
- [x] Transformer toute exception en état Faulted avec erreur structurée et nettoyage complet.
- [x] Tester 300 frames, pause, reprise, reset, arrêt, double arrêt et exception injectée.

#### ATA-026 — Copier et publier la vidéo

- [x] Traiter un pointeur nul comme duplication de la dernière frame.
- [x] Copier `pitch * height`, ligne par ligne, sans supposer `width * bytesPerPixel`.
- [x] Gérer tous les formats de pixels réellement négociés par les six cœurs.
- [x] Alterner des buffers loués pour ne jamais publier un buffer en cours d’écriture.
- [x] Publier dimensions, pitch, format, ratio, séquence et horodatage.
- [x] Redimensionner et restituer les buffers sans fuite lors d’un changement de géométrie.
- [x] Tester padding, formats, pointeur nul, résolutions dynamiques et ratios de chaque famille.

#### ATA-027 — Mettre le PCM dans une file bornée

- [x] Convertir correctement frames stéréo et échantillons des callbacks batch et unitaire.
- [x] Copier dans une file bornée selon le taux annoncé par le cœur.
- [x] Supprimer les blocs les plus anciens en dépassement et compter les overruns.
- [x] Compter les underruns sans fabriquer de faux `AudioChunk` dans le moteur.
- [x] Isoler complètement les tampons de deux machines simultanées.
- [x] Tester canaux gauche/droit, limites, ordre, compteurs et changement de taux.

#### ATA-028 — Sortir et synchroniser l’audio

- [x] Réutiliser `IAudioOutput`/WASAPI sans ajouter NAudio au moteur Atari.
- [x] Démarrer avec le taux réel, gérer mute, volume, pause, reprise, reset et arrêt.
- [x] Recréer la sortie après changement de périphérique ou erreur sans arrêter la machine.
- [x] Asservir la cadence à une cible et des bornes de tampon explicitement définies.
- [x] Respecter PAL, NTSC et les cadences propres aux consoles portables ou Jaguar.
- [x] Ne jamais dormir dans un callback natif et utiliser des attentes annulables.
- [x] Tester la dérive, les bornes mémoire et les compteurs sur une durée définie par famille.

#### ATA-029 — Publier les informations de fonctionnement

- [x] Exposer région, FPS, fréquence audio, géométrie et nom du cœur actif.
- [x] Exposer activité des médias et LED uniquement lorsque le cœur fournit une information fiable.
- [x] Exposer compteurs audio, dernière erreur et état du processus hôte.
- [x] Ne pas inventer une LED ou un état matériel absent.
- [x] Tester les instantanés de statut et leur isolement entre machines.

### I — Clavier, souris et contrôleurs

#### ATA-030 — Figer les entrées par frame

- [x] Stocker un snapshot immuable complet par échange atomique.
- [x] Copier le snapshot actif dans `input_poll` et répondre depuis cette copie jusqu’au poll suivant.
- [x] Retourner zéro pour port, device, index ou id inconnu.
- [x] Transporter exactement le même snapshot à travers le processus hôte.
- [x] Tester un changement concurrent en milieu de frame et deux instances indépendantes.

#### ATA-031 — Mapper les claviers Atari

- [x] Créer une table exhaustive `EmulationKey` vers les codes clavier des moteurs externes sans dépendance WPF dans le moteur.
- [x] Mapper le clavier ST/STE/TT/Falcon et ses touches propres.
- [x] Mapper le clavier Atari 8 bits et ses touches console Option, Select, Start et Help.
- [x] Transmettre down/up, caractère Unicode et modificateurs au mécanisme utilisé par le cœur.
- [x] Conserver l’adaptation `System.Windows.Input.Key` dans l’application.
- [x] Tester toutes les touches mappées, touches inconnues, dispositions et relâchement après perte de focus.

#### ATA-032 — Implémenter souris et capture relative

- [x] Mapper mouvement relatif, boutons, molette et éventuelle souris par port selon le cœur.
- [x] Réutiliser la capture Raw Input et la libération de souris de la vue Amiga.
- [x] Restaurer curseur, clipping et état des boutons après perte de focus, plein écran, pause ou arrêt.
- [x] Griser vitesse et mappages souris sur les modèles qui ne les utilisent pas.
- [x] Tester accumulation, consommation par frame, capture, libération et fermeture forcée.

#### ATA-033 — Implémenter les contrôleurs

- [x] Importer les informations de contrôleurs et descripteurs annoncés par chaque cœur.
- [x] Définir le nombre de ports et périphériques compatibles par modèle.
- [x] Mapper RetroPad, joystick numérique, sticks analogiques, gâchettes et boutons supplémentaires nécessaires.
- [x] Gérer les quatre ports Atari 8 bits lorsque disponibles et les contrôleurs spécifiques 5200/Jaguar.
- [x] Appeler `retro_set_controller_port_device` à la configuration et lors d’un changement permis.
- [x] Tester bitmask, axes extrêmes, zones mortes, ports absents et deux manettes distinctes.

#### ATA-034 — Intégrer les raccourcis d’émulation

- [x] Ajouter les actions communes : alimentation, pause, reset, plein écran et libération souris.
- [x] Ajouter sauvegarde/chargement rapide uniquement lorsque les états sont disponibles.
- [x] Ajouter insertion, éjection et média suivant pour les périphériques applicables.
- [x] Afficher dans la barre les raccourcis réellement configurés, sans texte en dur.
- [x] Réutiliser attribution, restauration, suppression et détection de conflits.
- [x] Tester priorité globale/machine, répétition clavier et conflit entre actions.

### J — États, configurations et cycle de vie

#### ATA-035 — Implémenter les états natifs

- [x] Vérifier taille et disponibilité de sérialisation après chargement du contenu.
- [x] Allouer exactement la taille annoncée et traiter proprement une taille nulle ou variable.
- [x] Implémenter sauvegarde et chargement sur le thread du cœur.
- [x] Ajouter un en-tête GW GUI avec cœur, version, modèle, configuration et empreinte du contenu.
- [x] Refuser un état d’un autre cœur, modèle ou contenu avec une erreur précise.
- [x] Rendre l’indisponibilité visible lorsque le cœur ne fournit pas les états.
- [x] Tester round-trip, données tronquées, incompatibilité et échec de `retro_unserialize` pour les six cœurs.

#### ATA-036 — Implémenter le magasin d’états

- [ ] Créer un dossier par machine sous le stockage partagé ou portable approprié.
- [ ] Écrire par fichier temporaire puis remplacement atomique.
- [ ] Ajouter état rapide, états nommés, date, capture et métadonnées.
- [ ] Ne jamais enregistrer firmware ou contenu protégé dans le fichier d’état.
- [ ] Nettoyer uniquement les états de la configuration supprimée après confirmation.
- [ ] Tester noms invalides, écriture interrompue, lecture concurrente et restauration.

#### ATA-037 — Implémenter le stockage des configurations

- [ ] Définir un schéma JSON versionné distinct des configurations Amiga.
- [ ] Persister modèle, cœur déterminé, options, firmwares, médias, entrées, dossiers et renderer.
- [ ] Conserver relatifs les chemins sous `Data` et absolus les chemins externes.
- [ ] Écrire atomiquement et ne jamais démarrer une machine pendant le chargement des documents.
- [ ] Isoler un document corrompu tout en chargeant les autres.
- [ ] Migrer les futures versions avec des fonctions explicites et testées.
- [ ] Tester deux configurations par famille, round-trip, corruption et suppression sans toucher aux fichiers partagés.
- [ ] Centraliser dans `StoragePaths` tous les chemins définitifs Atari pour mode installé et portable.
- [ ] Vérifier que sessions et temporaires restent hors des documents utilisateur et sont nettoyés après exécution.

#### ATA-038 — Implémenter le cycle de vie de la machine Atari

- [ ] Faire dépendre `AtariMachine` uniquement de `IAtariCore`.
- [ ] Implémenter transitions Created, Starting, Running, Paused, Stopping, Stopped et Faulted.
- [ ] Refuser les commandes interdites et rendre deux arrêts successifs inoffensifs.
- [ ] Acheminer reset, options, entrées, médias et états sur la boucle dédiée.
- [ ] Publier vidéo, audio, statut et erreurs sans bloquer le thread d’interface.
- [ ] Restaurer souris et audio lors de pause, faute et arrêt.
- [ ] Tester tout le cycle avec un faux cœur puis avec chaque processus réel disponible.

#### ATA-039 — Isoler plusieurs machines Atari

- [ ] Démarrer deux instances du même cœur avec dossiers, options, médias et callbacks distincts.
- [ ] Démarrer simultanément deux familles utilisant deux cœurs différents.
- [ ] Envoyer entrée, option et changement de média à une seule instance et vérifier l’autre inchangée.
- [ ] Arrêter ou faire planter une instance et vérifier que l’autre continue vidéo et audio.
- [ ] Vérifier noms uniques de pipes, mappings, dossiers de session et fichiers de cœur.
- [ ] Tester fermeture de toutes les machines lors de l’arrêt de l’application.

### K — Interface Atari

#### ATA-040 — Ajouter Atari à la page Émulation

- [ ] Ajouter la famille Atari dans la navigation existante sans créer une seconde page incohérente.
- [ ] Charger les documents Atari à l’ouverture des paramètres et après enregistrement.
- [ ] Ajouter création depuis un modèle, sélection, modification, sauvegarde et suppression confirmée.
- [ ] Ne pas ajouter de duplication de configuration tant qu’elle n’est pas demandée.
- [ ] Ne jamais modifier silencieusement une machine déjà active.
- [ ] Tester que toutes les fonctions Amiga existantes restent inchangées.

#### ATA-041 — Construire les paramètres généraux Atari

- [ ] Ajouter le choix du modèle et déterminer automatiquement le cœur associé.
- [ ] Intégrer le panneau d’installation du cœur correspondant.
- [ ] Ajouter les dossiers partagés et ceux propres aux disquettes, cassettes, cartouches, CD, états et captures.
- [ ] Ajouter firmware principal et complémentaires avec détection et compatibilité.
- [ ] Ajouter alimentation/reset/démarrage lorsque ces valeurs sont réellement configurables.
- [ ] Afficher toutes les erreurs et indisponibilités avec ressources localisées.
- [ ] Afficher toutes les Core Options annoncées avec catégorie, aide, valeurs et visibilité, sans perdre les clés inconnues.

#### ATA-042 — Construire les onglets CPU, RAM et ROM

- [ ] Afficher CPU, fréquence, précision et FPU selon la matrice du modèle.
- [ ] Afficher mémoire principale et extensions avec totaux et compatibilité.
- [ ] Afficher TOS/BIOS/ROM système, région, version et fichiers détectés.
- [ ] Alimenter toutes les listes depuis les catalogues, jamais depuis des tableaux du contrôle.
- [ ] Griser toute valeur imposée ou non applicable avec motif traduit.
- [ ] Tester chaque modèle ST, 8 bits et console contre la matrice.

#### ATA-043 — Construire les onglets Vidéo et Audio

- [ ] Ajouter standard, région, résolution, ratio, recadrage, frameskip et options de rendu réellement exposées.
- [ ] Ajouter sortie, activation, latence, volume et options de qualité audio réellement exposées.
- [ ] Distinguer options frontend et Core Options sans écrire deux fois la même valeur.
- [ ] Actualiser les disponibilités lors du changement de modèle sans perdre une valeur persistée valide.
- [ ] Afficher valeurs techniques non traduites seulement lorsqu’elles sont des noms officiels.
- [ ] Tester mise en page, sélection et persistance pour chaque famille.

#### ATA-044 — Construire l’onglet Stockage

- [ ] Présenter une liste de périphériques issue du modèle sélectionné.
- [ ] Ajouter/configurer/supprimer lecteurs de disquette et disques Hatari autorisés.
- [ ] Ajouter/configurer/supprimer lecteurs de disquette, cassette et cartouche Atari800 autorisés.
- [ ] Afficher un lecteur cartouche pour 2600, 7800, Lynx et Jaguar.
- [ ] Afficher cartouche et lecteur CD pour Jaguar CD.
- [ ] Empêcher les doublons d’identifiant et les types incompatibles.
- [ ] Expliquer que les médias amovibles peuvent être insérés ou remplacés depuis la machine active.
- [ ] Tester toutes les transitions de modèle et la persistance des périphériques.

#### ATA-045 — Construire les onglets Clavier, Souris et Contrôleurs

- [ ] Réutiliser les tableaux communs d’association, recherche, statut, attribution et suppression.
- [ ] Afficher les actions clavier propres au modèle et conserver les raccourcis globaux séparés.
- [ ] Afficher options souris et actions uniquement sur les modèles compatibles.
- [ ] Afficher détection, ports, types et périphériques de contrôleur selon la matrice.
- [ ] Ajouter mappings spécifiques 5200 et Jaguar lorsque nécessaires.
- [ ] Réutiliser zones mortes, vitesses analogiques, turbo et détection des conflits.
- [ ] Tester restauration des défauts, suppression, conflit et changement de modèle.

#### ATA-046 — Construire la section principale Atari

- [ ] Ajouter sélection de configuration et bouton d’ouverture dans l’onglet principal Émulation.
- [ ] Ouvrir chaque machine dans son propre sous-onglet avec titre et fermeture asynchrone.
- [ ] Recharger la liste après sauvegarde ou fermeture des paramètres.
- [ ] Valider cœur, firmware, configuration et média avant de démarrer.
- [ ] Afficher l’erreur précise au lieu d’un état `Unknown` générique.
- [ ] Arrêter toutes les machines Atari lors de la fermeture de l’application.

#### ATA-047 — Construire la vue de machine Atari

- [ ] Réutiliser la surface vidéo Direct3D 11 et son fallback sans appel natif depuis le contrôle.
- [ ] Ajouter alimentation, pause, resets, états, capture et plein écran selon les capacités.
- [ ] Conserver des marges équilibrées autour du bloc renderer.
- [ ] Afficher les raccourcis utiles et localisés dans les groupes existants.
- [ ] Construire dynamiquement disquettes, disques, cassette, cartouche et CD depuis les périphériques configurés.
- [ ] Afficher activité, son, contrôleurs, souris, résolution, fréquence et FPS sans inventer d’état.
- [ ] Restaurer les médias montés après remise sous tension.
- [ ] Tester rendu 4:3 et autres ratios, plein écran, capture souris, médias et arrêt.
- [ ] Enregistrer les captures dans le dossier configuré avec nom sûr, format PNG et erreur détaillée en cas d’échec.

### L — Traductions, aide et accessibilité

#### ATA-048 — Ajouter les ressources Atari de référence

- [ ] Inventorier chaque texte visible ajouté dans moteur, erreurs, contrôles et aide.
- [ ] Créer des clés nommées dans les fichiers de ressources adéquats, sans texte brut dans les contrôles.
- [ ] Fournir les valeurs anglaises de référence et françaises vérifiées.
- [ ] Ajouter des tests qui détectent clés absentes, doublons et textes visibles écrits en dur.
- [ ] Vérifier pluriels, paramètres, ponctuation, noms de machines et termes techniques.

#### ATA-049 — Traduire toutes les langues prises en charge

- [ ] Traduire toutes les nouvelles clés dans les 28 langues actuellement distribuées.
- [ ] Conserver placeholders, raccourcis, noms officiels, unités et direction du texte.
- [ ] Vérifier automatiquement que chaque langue possède exactement les clés de référence.
- [ ] Relire les écrans longs en RTL, CJK et langues à expansion importante.
- [ ] Ne considérer ce ticket terminé qu’après absence de fallback anglais involontaire dans chaque langue.

#### ATA-050 — Intégrer Atari au guide utilisateur

- [ ] Ajouter architecture utilisateur, création de configuration et installation des cœurs.
- [ ] Documenter chaque famille, ses firmwares, ses médias et ses limites réelles.
- [ ] Documenter réglages CPU, RAM, ROM, vidéo, audio, stockage et entrées.
- [ ] Documenter exécution, raccourcis, états, changement de média et erreurs courantes.
- [ ] Créer des captures actuelles cadrées correctement sans données personnelles.
- [ ] Générer les PDF compressés disponibles et conserver les sources Markdown.
- [ ] Ouvrir le PDF de la langue active avec repli vers l’anglais s’il manque.

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

- [ ] Inclure code Atari, ressources, PDF et installateur .NET prévu dans le ZIP portable.
- [ ] Exclure sources Markdown, images de documentation et fichiers temporaires.
- [ ] Exclure les six cœurs téléchargés, firmwares et médias privés sauf décision de licence explicitement validée.
- [ ] Vérifier au lancement le runtime .NET comme le fait la distribution actuelle.
- [ ] Tester sur un dossier neuf, avec et sans runtime déjà installé.
- [ ] Vérifier installation d’un cœur puis démarrage après extraction dans un chemin avec espaces.

#### ATA-054 — Mettre à jour l’installateur

- [ ] Inclure les mêmes fichiers applicatifs et PDF que le portable.
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
