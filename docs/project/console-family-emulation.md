# Familles de consoles et supports média

Ce chantier est réalisé sur la branche `feature/console-families-media`, créée
depuis `main` après la fusion de `amstrad` (`0ecd3512`). Il suit les frontières
déjà utilisées par les modules Amiga, Atari et Amstrad :

- [x] Fusionner `amstrad` dans `main` (`0ecd3512`).
- [x] Créer et sélectionner `feature/console-families-media`.
- [x] Écrire ce cadrage avant toute création de module ou de format.

```text
GWGUI.App -> GWGUI.Emulation -> GWGUI.Emulation.<Famille>/Common
                              -> Emulators/<Cœur>
GWGUI.App -> GWGUI.MediaEngine -> GWGUI.MediaFileSystems -> GWGUI.MediaAnalysis
```

## Périmètre des modules

Les identifiants de modules seront `sega`, `nintendo`, `sony` et `microsoft`.
Chaque module aura son propre `module.json`, son projet `net10.0` x64, une
factory publique `IEmulationModuleFactory`, un catalogue de machines et un
adaptateur par cœur réellement intégré. Aucun projet ne sera ajouté à
`GWGUI.App` : la découverte existante charge les modules depuis leur manifeste.

Le module supplémentaire `nec` couvrira PC Engine/TurboGrafx-16, CoreGrafx,
SuperGrafx, PC Engine Duo/TurboDuo et les variantes portables concernées.

## Consignation complète des exigences utilisateur

Cette section est la note de référence écrite avant toute nouvelle
modification de code. Les cases à cocher du plan qui suit ne signifient pas
que ces exigences sont déjà réalisées. Aucune décision technique ne doit
remplacer une exigence explicitement notée ici.

### Méthode de travail imposée

- Le travail doit commencer par la note complète des exigences, puis par une
  liste de tâches hiérarchique conforme à `.codex/config.toml`; il ne faut pas
  modifier un fichier « à l'arrache » au moment où une idée est évoquée.
- Le formatage doit conserver les lignes existantes lisibles : ne pas casser une
  déclaration ou une expression sur plusieurs lignes sans nécessité réelle;
  une coupure n'est permise que si la ligne est vraiment très longue et
  deviendrait illisible en restant entière.
- Lire l'architecture et les fichiers Amiga, Atari et Amstrad avant chaque
  changement; reprendre exactement leurs structures, noms de fichiers,
  contrats `Common` et liens `Machines/Common`.
- `Machine/Common` est le lien/adaptateur vers le `Common` racine partagé; le
  `Common` racine se relie ensuite à `GWGUI.Emulation`. Ne pas recopier une
  implémentation différente et ne pas renommer les fichiers communs.
- `GWGUI.App` ne doit pas être modifié pour implémenter une famille ou un
  support média particulier. L'App ne fait qu'afficher et utiliser les
  contrats existants. Si une capacité réellement générique, réutilisable par
  plusieurs machines, ne peut pas être intégrée autrement, il faut d'abord
  signaler le besoin, expliquer le contrat proposé et attendre sa validation;
  aucune modification de l'App ne doit être forcée pour contourner un module.
- Ne pas ajouter Libretro, une façade générique, une couche parallèle ou une
  extrapolation non demandée. Les adaptateurs doivent suivre le protocole et
  les fichiers déjà utilisés par les familles existantes; les cœurs sont
  ajoutés uniquement lorsqu'un adaptateur concret et fonctionnel est défini.
- Ne pas recréer `Emulators/<émulateur>/Core`: les fichiers de chaque cœur sont
  directement dans `Emulators/<émulateur>/`, comme dans l'arborescence déjà
  aplatie.
- Ne faire un commit qu'après une tranche réellement complète, fonctionnelle,
  testée et terminée; ne pas committer trois fichiers isolés. Tout changement
  important ou toute nouvelle fonctionnalité doit avoir son commit cohérent.
- Ne modifier aucun fichier média de l'utilisateur pour adapter le logiciel.
  Les archives ZIP peuvent être inspectées en les extrayant dans le dossier du
  ZIP, sans supprimer l'archive et sans ré-extraire lorsqu'un dossier du même
  nom existe déjà; l'audit de `F:\Retro` doit couvrir tous les fichiers et
  rester rapide, sans bloquer des heures sur un seul média.

### Familles et machines demandées

- Créer et rendre fonctionnels les modules `Emulation.Sega`,
  `Emulation.Nintendo`, `Emulation.Sony`, `Emulation.Microsoft` et
  `Emulation.Nec`, sans ajouter de famille non demandée.
- Sega doit couvrir SG-1000, SC-3000, Mark III, Master System, Master System
  II, Game Gear, Mega Drive, Saturn et Dreamcast.
- Nintendo doit couvrir Game & Watch, NES/Famicom, Famicom Disk System, SNES,
  Virtual Boy, Nintendo 64, Game Boy, Game Boy Color, Game Boy Advance,
  Nintendo DS/DSi, 3DS, GameCube, Wii, Wii U et Switch si un cœur vérifiable
  est réellement intégrable.
- Sony doit couvrir PlayStation/PS one, PlayStation 2/PStwo, PSP et ses
  variantes Slim/Lite/Go, PS Vita, PS3, PS4 et PS5 lorsqu'un cœur local
  vérifiable existe.
- Microsoft doit couvrir Xbox et Xbox 360.
- NEC doit couvrir PC Engine/TurboGrafx-16, CoreGrafx, SuperGrafx, PC Engine
  Duo/TurboDuo, PC Engine GT/TurboExpress et PC Engine LT lorsqu'ils sont
  réellement pris en charge.
- Pour chaque famille, le catalogue doit recenser tous les modèles, révisions,
  variantes commerciales, supports et périphériques officiels demandés; une
  capacité absente d'un cœur ne justifie jamais de supprimer l'entrée. Seule
  l'utilisation effective est refusée par la validation de ce cœur.
- Ajouter les ordinateurs uniquement s'ils ne sont pas IBM PC compatible
  x86/x64 et si leur adaptateur et leurs médias sont disponibles.

### Règle matérielle Sega : une seule Mega Drive et des extensions

- Le catalogue ne doit contenir qu'une machine `MegaDrive`. Mega Drive I,
  Mega Drive II, Genesis US et Mega Drive européen/japonais sont des options de
  configuration, jamais des machines séparées.
- Le nom affiché est traduit par culture (par exemple « Mega Drive » en
  français et « Genesis » en anglais); il ne faut pas dupliquer une machine
  pour le nom commercial.
- Le modèle I/II est choisi dans les options matérielles (CPU/ROM ou la
  section équivalente existante). RAM, puissance ou CPU ne sont différenciés
  que si les valeurs réelles et le cœur les exposent; aucune valeur inventée
  ne doit devenir modifiable.
- La région doit proposer Auto, NTSC-U, NTSC-J, PAL et SECAM. La fréquence
  50/60 Hz doit être cohérente avec la région mais rester sélectionnable si le
  cœur le permet; NTSC-U, NTSC-J, PAL et SECAM ne doivent pas être confondus
  avec le nom du modèle.
- Mega-CD I et Mega-CD II sont des extensions du lecteur CD ajouté à Mega
  Drive, pas des machines indépendantes. La configuration doit permettre
  d'activer/désactiver le lecteur, de choisir I ou II et d'appliquer uniquement
  la vitesse/latence réellement associée au lecteur choisi. Une image Mega-CD
  est refusée si le lecteur ajouté est désactivé.
- Le 32X est une extension du port cartouche de Mega Drive, pas une machine
  indépendante. Il doit être activable/désactivable; les cartouches `.32x`
  sont acceptées seulement lorsqu'il est activé.
- Les profils matériels doivent aussi couvrir le Master System Converter et
  le Game Gear Converter, ainsi que les ROM/BIOS Mega-CD, Saturn et Dreamcast.

### Règle matérielle Master System et supports Sega Card

- Le catalogue doit contenir une seule machine `MasterSystem`; SMS I et SMS II
  sont deux variantes choisies par une case de configuration, pas deux machines
  séparées. La variante SMS II doit reprendre les mêmes réglages de base que le
  SMS I puis appliquer uniquement les différences matérielles vérifiées.
- Le modèle SMS I/SMS II doit conserver les vraies valeurs de CPU, ROM, RAM,
  VDP et puce audio. Une différence n'est exposée que lorsqu'elle est établie
  par le matériel et par le cœur; aucune différence supposée ne devient une
  option.
- Le SMS I doit exposer simultanément deux lecteurs distincts dans le même
  modèle : un lecteur de cartouche et un lecteur de Sega Card/My Card. Les deux
  supports peuvent être insérés en même temps. La priorité de lecture réelle
  entre les deux doit être représentée et vérifiée sur le modèle, sans inventer
  un ordre arbitraire.
- La configuration Sega Card doit proposer le mode Sega 3-D Glasses. Lorsque ce
  mode est activé, le lecteur Sega Card est verrouillé et une Sega Card ne peut
  plus être insérée. Il faut vérifier si le cœur applique déjà l'affichage 3D
  des jeux SMS concernés ou si un filtre d'affichage séparé est nécessaire;
  aucune conversion 3D ne doit être ajoutée sans cette vérification.
- Le SMS II doit exposer l'absence du lecteur Sega Card si elle est confirmée
  par le matériel, tout en conservant le lecteur de cartouche.

### Périphériques officiels et entrées Sega

- Ne pas afficher « joystick » comme périphérique universel. Le catalogue doit
  consigner **tous** les périphériques officiels de chaque machine et de chaque
  port, même lorsqu'un cœur donné ne sait pas encore les piloter. La
  compatibilité du cœur est une validation séparée : elle autorise uniquement
  les modèles effectivement pris en charge, sans supprimer les autres du
  catalogue.
- Master System: manette standard, Control Stick et Light Phaser. Le
  pistolet est piloté par la souris et son pointeur doit être visible dans
  l'affichage sous forme de gros point ou de petit cercle; cette règle vaut
  pour tous les pistolets de toutes les machines.
- Mega Drive: manette 3 boutons, manette 6 boutons, Mega Mouse (notamment Art
  Alive), Menacer et tous les autres périphériques officiels documentés.
- Recenser de la même façon tous les périphériques officiels propres à
  SG-1000, SC-3000, Mark III, Game Gear, Saturn et Dreamcast; ne rien inventer
  et ne rien retirer parce qu'un cœur est incomplet.
- La liste de référence des modèles et des visuels officiels Sega est
  `docs/tasks/interface/controller-artwork-backlog.md`; elle doit être reprise
  dans le catalogue de la machine, puis filtrée séparément selon les capacités
  réellement déclarées par chaque cœur.
- Les options d'entrée doivent relier `GameInput` aux fonctions de la famille,
  puis au cœur, sans casser clavier, souris ou manette existants. Toute fenêtre,
  surface, processus ou cœur doit être détaché et libéré dans `finally`.

### ROM, CPU, RAM, vidéo et audio

- Exposer les vrais noms de CPU, GPU/VPU et puces audio dans l'onglet CPU et
  les vraies valeurs de RAM/ROM. Ces valeurs sont fixes sauf réglage réellement
  prévu par le matériel et le cœur.
- Master System doit proposer les profils de ROM système Hang On + Snail Maze,
  Hang On, Hang On + Safari Hunt, Alex Kidd, Sonic et Master System II.
- Prévoir les ROM/BIOS Mega Drive II, Mega-CD I/II, 32X, Master System
  Converter, Game Gear Converter, Saturn et Dreamcast, sans créer de fichier
  BIOS fictif.
- Toutes les clés et valeurs traduisibles doivent exister dans la base commune
  et dans chaque culture existante; « Cartridge » devient « Cartouche » en
  français, et les libellés Sega Card/My Card/Mark doivent être localisés.

### Supports média, conversion, visualisation et exploration

- Les lecteurs, writers, représentations et conversions vivent dans
  `GWGUI.MediaEngine`; les systèmes de fichiers dans
  `GWGUI.MediaFileSystems`; la détection et l'analyse dans
  `GWGUI.MediaAnalysis`. Les modules d'émulation ne recopient aucun lecteur.
- Couvrir tous les supports demandés et ceux présents dans `F:\Retro`, sans
  oublier Famicom Disk, Sega Card/My Card, HuCard/PCE, cartouches console,
  disquettes, cassettes, flux SCP/HxC/KryoFlux, optiques et multi-pistes.
  Pour Dreamcast, traiter le disque GD-ROM (environ 1 Go) avec ses pistes et
  son descripteur, sans le présenter comme un simple LBA de cartouche.
- Pour les cartouches SMS, déclarer des formats distincts pour la cartouche et
  la Sega Card, deux slots simultanés dans MediaEngine et MediaFileSystems, et
  refuser l'insertion d'une Sega Card lorsque le mode 3-D Glasses est actif.
- Les flux audio/cassette (CDT, TAP, TZX, TSX, VOC, WAV, FLAC, MP3, AAC, M4A,
  RAW audio et HXCSTREAM lorsqu'il est décodable) doivent pouvoir être lus,
  visualisés comme bande découpée en blocs, convertis et explorés lorsqu'un
  décodage réel existe. Les images sectorielles (DSK, EDSK, ADF, ATR, etc.)
  restent distinctes des flux.
- Une cartouche doit être représentée comme une cartouche adaptée à sa machine,
  avec tous ses blocs/banques visibles et lisibles, sans forme de disque, sans
  LBA générique et sans blocs noirs lorsque des données sont présentes. Le
  compteur et les segments doivent correspondre au nombre réel de banques.
- La bande et ses blocs doivent apparaître dans la barre de status et en bas du
  visualisateur, avec la progression de lecture bloc par bloc déjà utilisée par
  les autres vues. Ne pas introduire une seconde barre de progression générique.
- La conversion doit écrire dans le dossier source lorsque ce comportement est
  demandé, mémoriser correctement les dossiers via MediaEngine et continuer une
  conversion par lot après une erreur. L'erreur doit rester un message court,
  traduit et rouge dans la console; la boîte de dialogue ne doit pas afficher la
  stack trace technique.
- La console est indépendante par onglet: chaque onglet conserve ouvert/fermé
  et sa hauteur. Cette persistance ne doit pas modifier la logique de l'App ou
  les barres de secteurs/blocs communes.
- Aucun dossier `Images/Formats/Cartridge/Raw`, aucun identifiant
  `RawCartridge`, aucun « raw cartridge » générique et aucun code inventé ne
  doit être ajouté. Les formats doivent porter le nom réel de la machine ou du
  support.

### Cibles de traduction et de non-régression

- Ne jamais laisser apparaître une clé de ressource telle que
  `[Conversion.MediaEngineFamily]`; chaque nouveau libellé doit être résolu
  dans toutes les langues avec Argos et la base commune.
- Conserver les barres de secteurs/blocs partagées par Lecture, Écriture,
  Visualisation, Explorateur et Conversion; supprimer toute progression
  concurrente qui n'est pas cette barre commune.
- Les erreurs détaillées restent disponibles dans la console pour le diagnostic,
  mais l'interface utilisateur reçoit un message traduit et court. Les images
  corrompues doivent signaler leur erreur sans être modifiées ou complétées.

### Sega

- SG-1000, SC-3000, Mark III, Master System et Master System II ;
- une seule machine Mega Drive/Genesis avec extensions Mega-CD I/II et 32X ;
- Game Gear ;
- Saturn ;
- Dreamcast.

### Exigences Sega à reprendre avant toute nouvelle implémentation

- [ ] Corriger le catalogue matériel Sega en conservant exactement les frontières `Common` et `Machines/Common` déjà utilisées par Amiga, Atari et Amstrad.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ModelContracts.cs` pour porter les caractéristiques matérielles vérifiées (CPU, puce vidéo, puce audio, RAM fixe, ports et supports) sans ajouter de champs propres à une autre famille.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` pour décrire séparément SG-1000, SC-3000, Mark III, une seule machine Master System configurable SMS I/SMS II, Game Gear, une seule machine Mega Drive, Saturn et Dreamcast, avec les valeurs matérielles vérifiées ; Mega-CD I/II et 32X ne doivent pas être des machines du catalogue.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/ConfigurationFunctions.cs` pour afficher les caractéristiques fixes et refuser les combinaisons de supports ou de ports incompatibles avec le modèle sélectionné.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/SettingsConstants.cs` pour afficher les noms réels des CPU, GPU/VPU et puces audio, et ne proposer une modification de RAM/CPU que lorsque le matériel la permet réellement.
- [ ] Recenser tous les périphériques officiels Sega, puis valider séparément la compatibilité effective de chaque cœur au moment de la sélection.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Enums/ControllerEnums.cs` pour déclarer les catégories Sega officielles sans renommer ni dupliquer les contrats communs existants.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ControllerCatalog.cs` pour décrire la topologie des ports et **tous** les périphériques officiels connus de SG-1000, SC-3000, Mark III, Master System, Game Gear, Mega Drive, Mega-CD, 32X, Saturn et Dreamcast, indépendamment de la couverture actuelle des cœurs.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Settings.cs` pour exposer les choix officiels et leurs actions propres, au lieu de présenter `Joystick` pour tout.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Visuals.cs` pour utiliser les visuels existants et les contrôles propres aux manettes, souris et pistolets.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/EmulationPeripheralConversionFunctions.cs` pour convertir les catégories Sega vers `GWGUI.Emulation.Enums.EmulationPeripheralCategory` sans perte de périphérique.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Services/Machine.cs` et `src/GWGUI.Emulation.Sega/Common/Services/Machine.Lifecycle.cs` pour appliquer réellement le périphérique choisi au port du cœur et libérer toute ressource empruntée dans `finally`.
  - [ ] Ajouter dans `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ControllerCatalog.cs` le Master System Controller, le Control Stick et le Light Phaser, avec le pointeur souris demandé pour les pistolets.
  - [ ] Ajouter dans `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ControllerCatalog.cs` les manettes Mega Drive 3 boutons et 6 boutons, la Mega Mouse, le Menacer et tous les autres modèles officiels recensés, même lorsqu'un cœur ne les expose pas encore.
  - [ ] Ajouter dans les mêmes catalogues tous les périphériques officiels recensés de Saturn, Dreamcast, SG-1000, SC-3000, Mark III et Game Gear, sans inventer de périphérique; la sélection effective sera filtrée séparément par les capacités de chaque cœur.
- [ ] Ajouter les ROM système, BIOS et extensions officielles Sega dans la configuration et les cœurs.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/FirmwareConstants.cs` et les contrats de firmware pour définir les profils vérifiés de Master System (Hang On + Snail Maze, Hang On, Hang On + Safari Hunt, Alex Kidd, Sonic), Master System II, Mega Drive II, Mega-CD I/II, 32X, Master System Converter, Game Gear Converter, Saturn et Dreamcast.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/FirmwareCatalog.cs` pour relier chaque profil à son modèle et à son chemin de fichier sans créer de BIOS fictif.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` pour afficher et persister la sélection de ROM système dans la configuration du modèle.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs`, `src/GWGUI.Emulation.Sega/Emulators/Yabause/Services/ExternalCore.cs` et `src/GWGUI.Emulation.Sega/Emulators/Flycast/Services/ExternalCore.cs` pour transmettre au cœur les profils firmware réellement disponibles et refuser explicitement ceux qu’il ne sait pas charger.
- [ ] Corriger les cartouches, cartes et disques Sega sans libellé générique ni structure inventée.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/StorageConstants.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/StorageFunctions.cs` pour déclarer les extensions et supports Sega vérifiés (Sega Card/My Card, cartouches SG-1000/Mark III/Master System/Game Gear/Mega Drive/32X, Mega-CD, Saturn et Dreamcast).
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/MediaFunctions.cs` pour relier les deux slots SMS (cartouche et Sega Card), conserver leur insertion simultanée et appliquer la priorité matérielle vérifiée, tout en séparant les lecteurs cartouche, carte et optique.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ConfigurationContracts.cs` pour persister les extensions Mega Drive activées (modèle Mega-CD du lecteur ajouté, vitesse du lecteur correspondante et présence du 32X) sans créer une machine fictive séparée.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/ConfigurationFunctions.cs` pour n’accepter les images Mega-CD que lorsque le lecteur ajouté est activé et pour n’accepter les cartouches 32X que lorsque l’extension 32X est activée.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` pour afficher dans la configuration Mega Drive le choix Mega-CD I/Mega-CD II et l’activation du 32X, avec les valeurs de vitesse propres au lecteur sélectionné.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/SettingsConstants.cs`, `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/ConfigurationFunctions.cs` pour conserver une seule machine `MegaDrive` et proposer ses variantes matérielles (Mega Drive I/II, nom Genesis/Mega Drive selon la langue), sa région Auto/NTSC-U/NTSC-J/PAL/SECAM et sa fréquence 50/60 Hz sans mélanger les choix régionaux avec le nom du modèle, ainsi qu'une seule machine `MasterSystem` avec l'option SMS I/SMS II et l'option Sega 3-D Glasses.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Functions/GenesisPlusGXOptionFunctions.cs` et `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` pour transmettre au cœur les extensions Mega-CD/32X choisies sur Mega Drive et refuser les supports incompatibles.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/ar-SA/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/cs-CZ/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/da-DK/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/de-DE/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/el-GR/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/en-US/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/es-ES/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/fi-FI/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/fr-FR/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/he-IL/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/hu-HU/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/id-ID/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/it-IT/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/ja-JP/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/ko-KR/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/nb-NO/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/nl-NL/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/pl-PL/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/pt-BR/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/pt-PT/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/ro-RO/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/ru-RU/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/sv-SE/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/th-TH/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/tr-TR/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/uk-UA/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/vi-VN/Emulation.resx`, `src/GWGUI.Emulation.Sega/Resources/zh-Hans/Emulation.resx` et `src/GWGUI.Emulation.Sega/Resources/zh-Hant/Emulation.resx` pour remplacer « Cartridge » par « Cartouche », ajouter les libellés Sega Card/Mark et traduire chaque nouveau périphérique, profil ROM et support.
  - [ ] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs`, `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs`, `src/GWGUI.MediaEngine/Images/Formats/CapabilityAwareImageFormatCatalog.cs`, `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/CommonMediaContentRecognitionTable.cs` pour relier uniquement les formats Sega démontrés, sans ajouter de dossier `Raw` ou de type générique.
- [ ] Implémenter et vérifier l’exécution complète des périphériques et options Sega.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Services/ExternalCore.cs` et ses constantes pour sélectionner le périphérique officiel demandé par port, mapper la souris/pointeur du Light Phaser et ne plus imposer `JoypadDevice` à tous les ports.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Emulators/Yabause/Services/ExternalCore.cs` et `src/GWGUI.Emulation.Sega/Emulators/Flycast/Services/ExternalCore.cs` pour appliquer les périphériques et options propres à Saturn et Dreamcast.
  - [ ] Modifier `tests/GWGUI.Tests/Emulation/MachineAdapters/ConsoleFamilyMediaScenarios.cs` et `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineCapabilitiesScenarios.cs` pour ajouter les tests autonomes du catalogue matériel, des ports et périphériques officiels, des profils ROM, des extensions Sega, des traductions et du mapping des adaptateurs.
  - [ ] Compiler le module Sega et exécuter les tests concernés après toutes les sous-tâches, puis seulement créer un commit cohérent de cette tranche terminée.
  - [ ] Créer `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Constants/PicoDriveConstants.cs`, `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Factories/PicoDriveMachineFactory.cs` et `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Services/ExternalCore.cs` en reprenant les contrats existants, car Genesis Plus GX ne prend pas en charge le 32X ; réserver cet adaptateur aux extensions et options que PicoDrive expose réellement.

### Tranche Sega 1 — catalogue matériel et périphériques officiels

- [ ] Remplacer le catalogue Sega générique par le catalogue matériel demandé,
  sans créer de machine pour un add-on.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ModelContracts.cs` pour stocker le nom de ressource, les CPU, les puces vidéo/audio, la RAM fixe, les ports et les supports de chaque modèle.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` pour supprimer `MegaCd` et `ThirtyTwoX` comme machines, conserver une seule entrée `MasterSystem` configurable en SMS I/SMS II et conserver une seule entrée `MegaDrive`.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/MachineCatalog.cs` pour publier uniquement les modèles du catalogue matériel.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Dictionaries/EmulatorCatalog.cs`, `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Factories/GenesisPlusGXMachineFactory.cs`, `src/GWGUI.Emulation.Sega/Emulators/Yabause/Factories/YabauseMachineFactory.cs` et `src/GWGUI.Emulation.Sega/Emulators/Flycast/Factories/FlycastMachineFactory.cs` pour rattacher les cœurs aux machines réelles et laisser les extensions Mega-CD/32X dans la configuration Mega Drive.
- [ ] Recenser tous les modèles de périphériques officiels Sega avant tout
  filtrage par cœur.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Enums/ControllerEnums.cs` pour déclarer les catégories officielles (manettes, Control Stick, Light Phaser, pistolets, Mega Mouse, Menacer et autres modèles recensés) sans garder `Joystick` comme catégorie unique.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ControllerCatalog.cs` pour rattacher chaque modèle officiel à sa machine et à son port, même s'il n'est pas encore accepté par un cœur.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Settings.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Visuals.cs` pour présenter les noms de périphériques et les visuels adaptés, avec le pointeur Light Phaser souris.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/InputSnapshotDictionary.cs` pour préserver les mappings clavier/souris/manette et appliquer la catégorie sélectionnée sans supprimer les modèles catalogués.
- [ ] Exposer les premières options matérielles Sega sans modifier App.
  - [x] Modifier `src/GWGUI.Emulation/Constants/EmulationMediaSlotConstants.cs` et `src/GWGUI.Emulation/Contracts/EmulationMediaSlot.cs` pour ajouter un second slot cartouche générique et sa valeur de protocole, en réutilisant le préfixe déjà présent dans `src/GWGUI.Emulation/Dictionaries/EmulationMediaSlotDictionaries.cs`, sans modifier `GWGUI.App`.
  - [x] Modifier `src/GWGUI.Emulation/Constants/EmulationMediaSlotConstants.cs` et `src/GWGUI.Emulation/Contracts/EmulationMediaSlot.cs` pour supprimer le message anglais embarqué dans l'exception de protocole et laisser la couche de présentation fournir les messages traduits.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/SettingsConstants.cs`, `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/SettingsFunctions.cs` et `src/GWGUI.Emulation.Sega/Modules/SegaEmulationModule.cs` pour Mega Drive I/II, Genesis/Mega Drive selon la culture, région Auto/NTSC-U/NTSC-J/PAL/SECAM et fréquence 50/60 Hz.
  - [ ] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/ConfigurationFunctions.cs` et `src/GWGUI.Emulation.Sega/Common/Machines/Common/Contracts/ConfigurationContracts.cs` pour valider ces options et ne pas transformer Mega-CD/32X en machines.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/Emulation.resx` et chaque `src/GWGUI.Emulation.Sega/Resources/<culture>/Emulation.resx` pour localiser les modèles, périphériques, régions et extensions sans laisser de clé technique.
- [ ] Vérifier la tranche Sega 1 avant de la considérer comme terminée.
  - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` pour tester l'absence de machines `MegaCd`/`ThirtyTwoX`, la présence des deux slots Master System et le catalogue des périphériques.
  - [ ] Modifier `docs/project/console-family-emulation.md` pour cocher uniquement les sous-tâches effectivement réalisées après compilation et tests du module Sega.

### Nintendo

- Game & Watch ;
- NES/Famicom, SNES/Super Famicom ;
- Virtual Boy ;
- Nintendo 64 ;
- Game Boy, Game Boy Color, Game Boy Advance ;
- Nintendo DS/DSi, 3DS ;
- GameCube, Wii, Wii U ;
- Switch si un cœur compatible et redistribuable est effectivement intégré.

### Sony

- PlayStation/PS one ;
- PlayStation 2/PStwo ;
- PSP, PSP Slim/Lite et PSP Go ;
- PlayStation Vita ;
- PlayStation 3, 4 et 5 lorsqu'un cœur ou un adaptateur local vérifiable existe.

### Microsoft

- Xbox ;
- Xbox 360.

### NEC

- PC Engine/TurboGrafx-16 et CoreGrafx ;
- SuperGrafx ;
- PC Engine Duo/TurboDuo ;
- PC Engine GT/TurboExpress et PC Engine LT lorsqu'un cœur les prend en charge.

Les ordinateurs non compatibles IBM PC x86/x64 peuvent être ajoutés uniquement
comme machines d'une famille lorsque leur cœur et leurs médias sont réellement
disponibles. Aucun ordinateur IBM PC générique ne sera ajouté à ces modules.

## Cœurs ouverts retenus pour l'intégration

Les adaptateurs restent propres à chaque module et suivent les contrats déjà
utilisés par Amiga, Atari et Amstrad. Aucune couche Libretro commune n'est
ajoutée : un cœur n'est déclaré qu'après l'ajout de son adaptateur concret,
de son installation et de son protocole avec le module.

Les cœurs et leurs versions seront inscrits dans les catalogues propres aux
modules uniquement avec leur adaptateur concret, leur protocole, leur
installation et leurs médias vérifiés. Aucun nom de cœur n'est injecté dans
`GWGUI.Emulation` ou partagé entre les familles.

Les extensions sont dérivées des capacités déclarées par chaque adaptateur, et non
d'une liste générique : FDS, Sega Card/HuCard, CD/GD-ROM et les formats
multi-disques doivent donc être ajoutés avec leur lecteur MediaEngine et leur
préparation de contenu correspondants.

## Supports média

Les formats de disquette, cassette, flux, cartouche, disque optique et disque
dur restent dans `GWGUI.MediaEngine`. Les lecteurs et Writers spécialisés
retournent les représentations déjà consommées par la visualisation et la
conversion. `GWGUI.MediaFileSystems` reçoit la représentation décodée pour
l'exploration ; `GWGUI.MediaAnalysis` classe les entrées. Les modules
d'émulation ne recopient aucun lecteur de média.

Les formats à traiter sont inventoriés avant leur implémentation :

| Support | Exemples de formats | Propriétaire |
|---|---|---|
| Cartouche | ROM, BIN, NES, FDS (Famicom Disk System), SNES/SFC, N64, GBA, GB/GBC, NDS, 3DS/CIA, Sega Card et HuCard/PCE, CPR | MediaEngine ; MediaFileSystems seulement si un système de fichiers est démontré |
| Disquette | DSK/EDSK, ST, ADF, G64, D64/D71/D81, XDF et variantes propres aux machines | MediaEngine + MediaFileSystems |
| Cassette/bande | TAP, TZX, CDT, VOC, WAV, FLAC, MP3, AAC, M4A, HXCSTREAM | MediaEngine pour le flux et les blocs ; MediaFileSystems pour les contenus décodés |
| Optique | ISO, BIN/CUE, CHD, CCD/MDS, CDI, GDI (Dreamcast) et images multi-pistes | MediaEngine + systèmes ISO/UDF existants |
| Disque dur | IMG, VHD, VDI, VMDK, QCOW2, CHD | MediaEngine + systèmes de fichiers existants |

Une extension n'est déclarée comme lisible, visualisable, explorable ou
convertible qu'après l'ajout d'un Reader/Writer et d'une représentation réelle.
Une simple ligne d'extension dans un catalogue ne constitue pas un support.

### Support Microsoft Xbox XDVDFS/XISO

- [x] Ajouter le format nommé `microsoft.xbox.xdvdfs` sans masquer les ISO 9660 existantes.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` et `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs` pour déclarer l'identifiant XDVDFS.
  - [x] Conserver `.iso` comme extension optique existante et ajouter uniquement la constante `.xbe` nécessaire à l'analyse des exécutables Xbox.
  - [x] Créer le Reader/Writer XDVDFS dans `src/GWGUI.MediaEngine/Images/Formats/Optical/Xdvdfs/` avec validation de la signature du secteur 32, des bornes et des entrées.
  - [x] Enregistrer le Reader/Writer dans les compositions MediaEngine avant le lecteur ISO générique, sans modifier le comportement ISO 9660.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Xbox/Xdvdfs/` pour parcourir l'arbre binaire des répertoires et lire les fichiers.
  - [x] Ajouter la règle de contenu des exécutables Xbox `.xbe` dans `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` avec la signature `XBEH`.
  - [x] Ajouter les tests de lecture, exploration, conversion et nettoyage dans `tests/GWGUI.Tests/Media/XdvdfsMediaFormatTests.cs`.
  - [x] Compiler MediaEngine, MediaFileSystems et MediaAnalysis puis exécuter les tests ciblés.
- [x] Stage et commit de la tranche XDVDFS seulement après réussite de toutes les sous-tâches ci-dessus.

## Ordre d'implémentation

### Formats de cartouche spécifiques (à réaliser avant l'exposition utilisateur)

- [x] Déclarer les formats de cartouche par famille, sans identifiant générique visible.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` pour ajouter les identifiants Nintendo, Sega et NEC correspondant aux extensions réelles.
  - [x] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs` pour exposer les mêmes identifiants au moteur.
  - [x] Créer les lecteurs et writers de cartouches dans `src/GWGUI.MediaEngine/Images/Formats/Cartridge/Console/`, avec un identifiant et des extensions propres à chaque famille.
  - [x] Créer l’explorateur des banques dans `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/`, avec des formats console identifiés explicitement.
  - [x] Supprimer les anciens lecteurs de cartouche génériques et vérifier qu’aucune référence à cette ancienne structure ne reste dans `src`, `tests` ou la documentation.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/ImageFormatCatalog.cs` pour afficher le nom du format console réel.
  - [x] Modifier `src/GWGUI.App/Resources/00-Base/Formats.resx` pour localiser les formats spécifiques ajoutés dans la base commune.
  - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` avec un test autonome et nettoyage des fichiers temporaires dans `finally`.
  - [x] Compiler MediaEngine, MediaFileSystems et MediaAnalysis puis exécuter les tests ciblés.
  - [x] Retirer le libellé historique de cartouche générique de la documentation.

### Tranche en cours (à terminer avant tout commit)

- [x] Corriger les ressources de modèles copiées depuis Amstrad.
  - [x] Modifier les cinq `Resources/00-Base/Emulation.resx` pour supprimer les clés de modèles d’une autre famille et écrire celles correspondant aux `ModelCatalog` Sega, Nintendo, Sony, Microsoft et NEC.
  - [x] Reproduire ces clés dans les cultures existantes de chaque module sans conserver de valeur Amstrad résiduelle.
  - [x] Rechercher les identifiants `464`, `664`, `6128`, `GX4000` et les descriptions Amstrad résiduelles dans ces ressources, puis compiler les cinq modules.

### Identifiants interprocessus des hôtes de cœur

- [x] Supprimer les préfixes `gwgui-amstrad-*` restés dans les hôtes Sega et Nintendo.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/GenesisPlusGX/Constants/ProcessCoreConstants.cs` pour utiliser les préfixes `gwgui-sega-genesisplusgx-*`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Mesen/Constants/ProcessCoreConstants.cs` pour utiliser les préfixes `gwgui-nintendo-mesen-*`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Snes9x/Constants/ProcessCoreConstants.cs` pour utiliser les préfixes `gwgui-nintendo-snes9x-*`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/GameWatch/Constants/ProcessCoreConstants.cs` pour utiliser les préfixes `gwgui-nintendo-gw-*`.
  - [x] Rechercher les anciennes chaînes dans les cinq modules, compiler les modules concernés et exécuter les tests d’architecture.
- [x] Stage et commit de cette correction seulement après réussite des vérifications.

### Plan d’intégration Wii U sans couche générique

- [x] Remplacer la référence Cemu non vérifiée par un contrat d’intégration natif.
  - [x] Modifier `docs/project/console-family-emulation.md` pour retirer l’ancien paquet générique non vérifié de la checklist.
  - [x] Décrire dans cette section le binaire Cemu natif, son protocole de lancement et la capture vidéo/input nécessaires avant toute création de fichier sous `src/GWGUI.Emulation.Nintendo/Emulators/Cemu/`.
- [ ] Finaliser les cinq modules sans laisser de façade non exécutable.
  - [ ] Créer `src/GWGUI.Emulation.Nec/GWGUI.Emulation.Nec.csproj`, `module.json`, `Modules/NecEmulationModuleFactory.cs` et le catalogue PC Engine.
  - [ ] Ajouter dans chaque module un adaptateur concret sous `Emulators/<Cœur>` qui implémente `IEmulatorAdapter`, son installation, son protocole et la création de `Machine`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs`, `src/GWGUI.Emulation.Sony/Common/Dictionaries/EmulatorCatalog.cs`, `src/GWGUI.Emulation.Microsoft/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nec/Common/Dictionaries/EmulatorCatalog.cs` pour refuser explicitement une machine tant qu’aucun adaptateur concret ne la prend en charge.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de refus explicite des machines sans adaptateur.
  - [ ] Relier `CreateRuntimeAsync` de chaque module à l’adaptateur sélectionné et refuser explicitement uniquement les machines sans cœur installé.
  - [ ] Ajouter les lecteurs/writers MediaEngine et les représentations MediaFileSystems/MediaAnalysis réellement nécessaires aux supports déclarés.
  - [ ] Ajouter les ressources de traduction de chaque nouveau libellé dans la base commune existante.
  - [ ] Ajouter les tests autonomes de découverte, configuration, adaptateur, média et nettoyage des artefacts temporaires.
  - [x] Exécuter la build complète avec tous les modules et vérifier l’exécutable résultant (`F:\GW GUI\build\Debug\GW GUI\gwgui.exe`).
- [ ] Stage et commit uniques de cette tranche seulement après réussite de toutes les sous-tâches ci-dessus.

### Alignement des constantes Common

- [x] Supprimer l’écart introduit dans le fichier `MediaConstants` d’Amiga sans modifier les autres familles.
  - [x] Ajouter `DiskChangeDelayMilliseconds` au contrat commun `src/GWGUI.Emulation/Constants/EmulationMediaSlotConstants.cs`.
  - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Services/Machine.Lifecycle.cs` pour utiliser la constante commune.
  - [x] Modifier `src/GWGUI.Emulation.Amiga/Common/Constants/MediaConstants.cs` pour retrouver exactement la structure des autres familles.
  - [x] Exécuter le test `FamilyCommonConstantFilesAndMembersAreIdentical`.
- [x] Stage et commit de cette correction seulement après réussite du test d’architecture.

### Image optique Nintendo GameCube/Wii

- [x] Reconnaître les images `.gcm` qui utilisent le profil ISO sectoriel existant.
  - [x] Ajouter la constante d’extension dans `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs`.
  - [x] Ajouter `.gcm` aux extensions du profil `IsoFormat` pour la lecture et l’écriture lossless.
  - [x] Ajouter `.gcm` aux extensions d’analyse et à l’icône optique existante.
  - [x] Ajouter un test autonome de lecture, conversion et nettoyage dans `tests/GWGUI.Tests/Media/GameCubeIsoMediaFormatTests.cs`.
  - [x] Compiler les projets concernés et exécuter le test ciblé.
- [x] Stage et commit de cette tranche seulement après réussite du test média.

### Exploration FST Nintendo GameCube/Wii

- [x] Exposer l'arbre FST des images `.gcm` sans les traiter comme un ISO9660 générique.
  - [x] Ajouter `src/GWGUI.MediaFileSystems/Definitions/FileSystemIds.cs` avec l'identifiant technique FST Nintendo.
  - [x] Ajouter `src/GWGUI.MediaEngine/Images/Formats/Optical/Iso/IsoReader.cs` avec l'extension source reconnue dans les métadonnées du document.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/GameCube/GameCubeFstFileSystemConstants.cs` avec les offsets, limites et attributs FST.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/GameCube/GameCubeFstFileSystemReader.cs` pour lire la table FST big-endian, reconstruire les dossiers, lire les fichiers et refuser les bornes invalides.
  - [x] Enregistrer le lecteur dans `src/GWGUI.MediaFileSystems/Exploration/MediaExplorer.cs` avant les lecteurs ISO/UDF génériques.
  - [x] Ajouter `tests/GWGUI.Tests/Media/GameCubeFstFileSystemReaderTests.cs` avec une image synthétique, l'exploration et le nettoyage des artefacts temporaires dans `finally`.
  - [x] Exécuter le test ciblé.
  - [x] Exécuter la suite complète.
- [x] Stage et commit de cette tranche seulement après réussite de toutes les sous-tâches ci-dessus.

### Détection des ISO GameCube par en-tête

- [x] Reconnaître les dumps GameCube `.iso` par leur en-tête réel, sans détourner les ISO 9660 ordinaires.
  - [x] Ajouter dans `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/GameCube/GameCubeFstFileSystemConstants.cs` les constantes du mot magique et de son offset.
  - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/GameCube/GameCubeFstFileSystemReader.cs` pour autoriser `.iso` uniquement après validation du mot magique GameCube.
  - [x] Ajouter dans `tests/GWGUI.Tests/Media/GameCubeFstFileSystemReaderTests.cs` un cas `.iso` et conserver le cas `.gcm`.
  - [x] Exécuter le test ciblé puis la suite complète.
- [x] Stage et commit de cette tranche seulement après réussite de toutes les sous-tâches ci-dessus.

### Analyse des exécutables Xbox 360

- [x] Classer les fichiers Xbox 360 `.xex` à partir de leur signature `XEX2`.
  - [x] Ajouter la signature `XEX2` dans `src/GWGUI.MediaAnalysis/Constants/MediaContentSignatures.cs`.
  - [x] Ajouter la règle commune `.xex` signée dans `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` sans supprimer la règle Atari 8-bit.
  - [x] Ajouter un test de classification signé dans `tests/GWGUI.Tests/Media/MediaContentRecognitionCatalogScenarios.cs`.
  - [x] Compiler MediaAnalysis et exécuter le test média ciblé.
- [x] Stage et commit de cette tranche seulement après réussite du test.

### Constantes de l’explorateur de cartouches

- [x] Supprimer les littéraux techniques du lecteur de banques.
  - [x] Créer `src/GWGUI.MediaFileSystems/Constants/ConsoleCartridgeMetadataConstants.cs` avec les clés, attributs, type d’entrée et limites utilisés par l’explorateur.
  - [x] Modifier `src/GWGUI.MediaFileSystems/FileSystems/Console/Cartridge/ConsoleCartridgeFileSystemReader.cs` pour consommer uniquement ces constantes et le format de nom de banque défini.
  - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` pour vérifier les entrées et métadonnées produites après cette centralisation.
  - [x] Compiler MediaFileSystems et exécuter le test média ciblé.
- [x] Stage et commit de cette correction seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Sega Saturn

- [x] Ajouter un adaptateur Sega Saturn concret avec le protocole déjà utilisé par Sega.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Sega/Emulators/Yabause/` en reprenant les contrats, services et hôte du cœur Sega existant, puis remplacer les identifiants par ceux de Yabause.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Dictionaries/ModelCatalog.cs` et `src/GWGUI.Emulation.Sega/Resources/00-Base/Emulation.resx` pour rattacher Saturn au nouvel adaptateur et à sa description.
  - [x] Ajouter `src/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de découverte de l’adaptateur Saturn.
  - [x] Modifier `src/GWGUI.Emulation.Sega/EmulationGlobalUsings.cs` et les fichiers de `Emulators/GenesisPlusGX` et `Emulators/Yabause` pour importer explicitement leurs namespaces propres sans ambiguïté entre cœurs.
  - [x] Compiler le module Sega et exécuter les tests d’architecture.
- [x] Stage et commit de l’adaptateur Sega Saturn seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo NES/Famicom Disk System

- [x] Ajouter un adaptateur Nintendo NES/Famicom Disk System concret avec le protocole propre au cœur Mesen.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Mesen/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Mesen.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `Nes` et `FamicomDisk` à Mesen.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de sélection de Mesen pour NES et Famicom Disk System.
  - [x] Relier `NintendoEmulationModule.CreateRuntimeAsync` et `TryHandleHostCommand` au moteur et à l’adaptateur Mesen.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l'adaptateur Nintendo NES/Famicom Disk System seulement après réussite de toutes les sous-tâches ci-dessus.

### Support MediaEngine du Famicom Disk System

- [x] Ajouter le format nommé Famicom Disk System sans identifiant de cartouche générique.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` et `src/GWGUI.MediaEngine/Constants/DiskImageFormatIds.cs` pour ajouter `nintendo.fds`.
  - [x] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs` pour ajouter l’extension `.fds`.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Floppy/FamicomFds/FamicomFdsConstants.cs` avec la signature, la taille d’une face et les clés de métadonnées.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Floppy/FamicomFds/FamicomFdsReader.cs` pour lire les images FDS avec ou sans en-tête et exposer les faces comme blocs adressables.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Floppy/FamicomFds/FamicomFdsWriter.cs` pour réécrire les faces sans ajouter de remplissage inventé.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Reading/MediaRecognitionComposition.cs` et `src/GWGUI.MediaEngine/Images/Writing/MediaWritingComposition.cs` pour enregistrer ce lecteur et ce writer.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/ImageFormatCatalog.cs` pour publier le format `.fds` dans la famille Nintendo.
  - [x] Créer `src/GWGUI.MediaFileSystems/FileSystems/Nintendo/FamicomDisk/FamicomDiskFileSystemReader.cs` et l’enregistrer dans `src/GWGUI.MediaFileSystems/Exploration/MediaExplorer.cs` pour explorer les fichiers FDS réellement décodables.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` et `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` pour classer `.fds` comme image disque.
  - [x] Modifier `tests/GWGUI.Tests/Media/AmstradCpcMediaFormatTests.cs` avec un test média autonome et suppression des fichiers temporaires dans `finally`.
- [x] Compiler les projets concernés et exécuter le test ciblé avant le commit de cette tranche.

### Support optique Dreamcast GDI

- [x] Ajouter le descripteur GDI nommé et ses constantes sans réintroduire de format générique.
  - [x] Modifier `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs` et `src/GWGUI.MediaEngine/Constants/OpticalImageFormatIds.cs` pour déclarer `.gdi` et `optical-gdi`.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/Gdi/GdiConstants.cs` et `GdiDescriptorReader.cs` pour valider les pistes et leurs fichiers associés.
- [x] Ajouter la lecture et l’écriture des pistes GDI dans MediaEngine.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/Gdi/GdiReader.cs` avec une représentation `OpticalMediaImageRepresentation` fidèle aux secteurs déclarés.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/Gdi/GdiWriter.cs` pour écrire le descripteur et les données de piste sans compléter artificiellement les fichiers.
  - [x] Enregistrer le lecteur et le writer dans `MediaRecognitionComposition` et `MediaWritingComposition`.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/ImageFormatCatalog.cs` pour afficher « Dreamcast GDI ».
- [x] Exposer GDI dans les couches d’exploration et d’analyse.
  - [x] Modifier `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` et `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` pour l’identifiant et l’extension GDI.
  - [x] Ajouter le classement `.gdi` dans `CommonMediaContentRecognitionTable`; l’exploration optique existante consomme les pistes GDI décodées.
- [x] Ajouter le test autonome GDI, compiler les projets concernés et exécuter les tests avant le commit de cette tranche.

### Cartouches Sega My Card et NEC SuperGrafx

- [x] Reconnaître `.mv` comme variante SG-1000 / Sega My Card dans les constantes, l’analyse, l’icône, le catalogue et le lecteur de cartouches existants.
- [x] Ajouter le format nommé `nec.supergrafx` pour `.sgx`, avec lecture, écriture, visualisation par banques et exploration via les couches déjà utilisées par les autres cartouches.
- [x] Ajouter le test de round-trip et d’exploration des deux extensions.
- [ ] Vérifier les images My Card et SuperGrafx réelles du corpus utilisateur.

### Descripteurs optiques déjà décodés

- [x] Publier `.cue`, `.ccd`, `.mds` et `.chd` dans les constantes d’analyse et la classification disque.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` avec les extensions optiques manquantes.
  - [x] Modifier `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs` pour classer ces extensions comme images disque.
- [x] Ajouter l’icône optique pour `.chd` dans `src/GWGUI.MediaEngine/Constants/MediaIconExtensions.cs` et `src/GWGUI.MediaEngine/Images/Visualization/MediaIconSelector.cs`.
- [x] Ajouter un test autonome de publication des extensions optiques et nettoyer ses artefacts temporaires.

### Pistes audio WAVE dans BIN/CUE

- [x] Lire les feuilles CUE qui référencent des fichiers WAVE PCM Red Book.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Reading/Sources/WavePcmRandomAccessData.cs` pour exposer uniquement le bloc `data` PCM d’un fichier RIFF/WAVE validé.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/BinCue/WavePcmConstants.cs` avec les signatures, paramètres PCM et tailles de secteurs audio.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/Optical/BinCue/BinCueFormat.cs` pour accepter `.wav` comme fichier associé d’une feuille CUE.
  - [x] Modifier `src/GWGUI.MediaEngine/Images/Formats/Optical/BinCue/BinCueReader.cs` pour lire les pistes WAVE uniquement en mode `AUDIO` et les exposer dans la représentation optique commune.
  - [x] Ajouter `tests/GWGUI.Tests/Media/BinCueWaveMediaFormatTests.cs` avec une feuille CUE/WAVE synthétique, une conversion BIN/CUE et la suppression des artefacts dans `finally`.
- [x] Compiler MediaEngine et exécuter le test média ciblé avant le commit de cette tranche.

### Adaptateur Nintendo Virtual Boy

- [x] Ajouter un adaptateur Nintendo Virtual Boy concret avec le protocole propre au cœur Beetle VB.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/BeetleVb/` en reprenant les contrats et l’hôte déjà utilisés par les cœurs Nintendo, puis adapter l’installation, les identifiants et le média `.vb`.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `VirtualBoy` à Beetle VB.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Beetle VB pour Virtual Boy.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de configuration.
- [x] Stage et commit de l’adaptateur Nintendo Virtual Boy seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo Game & Watch

- [x] Ajouter l’adaptateur nommé `gw` et son cœur Windows x64 dans `src/GWGUI.Emulation.Nintendo/Emulators/GameWatch/`, en reprenant le protocole d’un adaptateur Nintendo existant sans créer de couche générique.
  - [x] Relier la factory exclusivement au modèle `GameWatch` et à la commande hôte Nintendo dédiée.
  - [x] Déclarer la description et les messages du cœur dans les ressources Nintendo.
  - [x] Compiler le module Nintendo et vérifier la sélection du cœur par un test d’architecture.
- [x] Ajouter le format nommé `.mgw` dans `MediaEngine`, `MediaFileSystems` et `MediaAnalysis`, avec lecture, écriture, exploration par banques et icône cartouche.
- [x] Ajouter les URL de catalogue de mise à jour aux cinq manifestes de modules afin que le build avec modules puisse les valider.
- [ ] Vérifier avec un fichier `.mgw` réel fourni par l’utilisateur.

### Adaptateur Nintendo 3DS (Citra)

- [x] Ajouter l’adaptateur nommé `citra` dans `src/GWGUI.Emulation.Nintendo/Emulators/Citra/`, avec le téléchargement x64 du cœur Citra et une factory limitée au modèle `Nintendo3Ds`.
- [x] Déclarer les messages d’erreur et la description Citra dans les ressources Nintendo.
- [x] Ajouter les extensions Citra `.3ds`, `.3dsx`, `.elf`, `.axf`, `.cci`, `.cxi` et `.app` au format nommé `nintendo.3ds` pour lecture, écriture, visualisation et exploration par banques.
- [x] Ajouter le test de sélection de Citra pour Nintendo 3DS et compiler le module Nintendo.
- [ ] Vérifier le rendu matériel Citra avec un fichier 3DS réel fourni par l’utilisateur.

### Adaptateur Nintendo GameCube/Wii (Dolphin)

- [x] Ajouter l’adaptateur nommé `dolphin` dans `src/GWGUI.Emulation.Nintendo/Emulators/Dolphin/`, avec le cœur Windows x64 et une factory limitée aux modèles `GameCube` et `Wii`.
- [x] Déclarer le rendu logiciel par défaut et les messages d’erreur/description Dolphin dans les ressources Nintendo.
- [x] Ajouter le test de sélection de Dolphin pour GameCube et Wii et compiler le module Nintendo.
- [ ] Vérifier un jeu GameCube et un jeu Wii réels avec les fichiers système Dolphin requis.

### Adaptateur Nintendo Wii U (Cemu)

L’adaptation devra utiliser la distribution native de Cemu, sans DLL générique.
Avant toute création de l’adaptateur, il faut établir le contrat réel de
lancement de Cemu, le chargement du jeu, le transport des images vidéo,
les entrées et la libération du processus ; tant que ces points ne sont pas
vérifiés, aucun fichier Cemu ne doit être ajouté au module.

- [ ] Étendre l’hôte de cœur Nintendo avec un contexte graphique matériel Vulkan/OpenGL partagé, requis par Cemu.
  - [ ] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Cemu/Services/ExternalHostCallbacks.Environment.cs` pour fournir `SetHwRender` et les callbacks de contexte.
  - [ ] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Cemu/Services/ProcessCore.cs` et le protocole d’hôte pour transférer les frames matérielles vers la surface vidéo existante.
  - [ ] Ajouter le test d’initialisation et de libération du contexte matériel dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs`.
- [ ] Ajouter l’adaptateur Cemu dans `src/GWGUI.Emulation.Nintendo/Emulators/Cemu/` avec le protocole de cœur déjà utilisé par les adaptateurs Nintendo.
  - [ ] Déclarer la distribution Windows x64 de Cemu natif, son exécutable et sa commande hôte après vérification du protocole.
  - [ ] Relier uniquement le modèle `WiiU` à Cemu et laisser le cœur fournir ses options vidéo et système.
  - [ ] Ajouter les ressources de description et d’erreur Cemu dans les ressources Nintendo.
  - [ ] Ajouter le test d’architecture de sélection Cemu pour `WiiU`.
- [ ] Compiler le module Nintendo et exécuter le test d’architecture avant le commit.

### Adaptateur Nintendo SNES

- [x] Ajouter un adaptateur Nintendo SNES concret avec le protocole propre au cœur Snes9x.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Snes9x/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Snes9x.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `Snes` à Snes9x.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Snes9x pour SNES.
  - [x] Relier le moteur Nintendo à l’adaptateur Snes9x par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo SNES seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur NEC PC Engine / SuperGrafx

- [x] Ajouter un adaptateur NEC PC Engine concret avec le protocole propre au cœur Beetle PCE FAST.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nec/Emulators/BeetlePce/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Beetle PCE FAST.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nec/Resources/00-Base/Emulation.resx` pour rattacher uniquement `PcEngine`, `CoreGrafx`, `SuperGrafx`, `PcEngineDuo` et `TurboExpress` à Beetle PCE FAST.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de sélection de Beetle PCE FAST pour les modèles PC Engine.
  - [x] Relier `NecEmulationModule.CreateRuntimeAsync` et `TryHandleHostCommand` au moteur et à l’adaptateur Beetle PCE FAST.
  - [x] Compiler le module NEC et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur NEC PC Engine / SuperGrafx seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Sony PlayStation

- [x] Ajouter un adaptateur Sony PlayStation concret avec le protocole propre au cœur SwanStation.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Sony/Emulators/SwanStation/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de SwanStation.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Sony/Resources/00-Base/Emulation.resx` pour rattacher uniquement `PlayStation` à SwanStation.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de SwanStation pour PlayStation.
  - [x] Relier `SonyEmulationModule.CreateRuntimeAsync` et `TryHandleHostCommand` au moteur et à l’adaptateur SwanStation.
  - [x] Compiler le module Sony et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Sony PlayStation seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo Game Boy

- [x] Ajouter un adaptateur Nintendo Game Boy / Game Boy Color concret avec le protocole propre au cœur Gambatte.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Gambatte/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Gambatte.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `GameBoy` et `GameBoyColor` à Gambatte.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de sélection de Gambatte pour Game Boy et Game Boy Color.
  - [x] Relier le moteur Nintendo à l’adaptateur Gambatte par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo Game Boy seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo Game Boy Advance

- [x] Ajouter un adaptateur Nintendo Game Boy Advance concret avec le protocole propre au cœur mGBA.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Mgba/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de mGBA.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `GameBoyAdvance` à mGBA.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de mGBA pour Game Boy Advance.
  - [x] Relier le moteur Nintendo à l’adaptateur mGBA par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo Game Boy Advance seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo 64

- [x] Ajouter un adaptateur Nintendo 64 concret avec le protocole propre au cœur Mupen64Plus-Next.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/Mupen64PlusNext/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Mupen64Plus-Next.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Dictionaries/ModelCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `Nintendo64` à Mupen64Plus-Next.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Mupen64Plus-Next pour Nintendo 64.
  - [x] Relier le moteur Nintendo à l’adaptateur Mupen64Plus-Next par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo 64 seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Nintendo DS

- [x] Ajouter un adaptateur Nintendo DS / DSi concret avec le protocole propre au cœur melonDS.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nintendo/Emulators/MelonDs/` en reprenant le protocole d’hôte d’un cœur déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de melonDS.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Dictionaries/ModelCatalog.cs` et `src/GWGUI.Emulation.Nintendo/Resources/00-Base/Emulation.resx` pour rattacher uniquement `NintendoDs` à melonDS.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de melonDS pour Nintendo DS.
  - [x] Relier le moteur Nintendo à l’adaptateur melonDS par sa découverte d’adaptateurs.
  - [x] Compiler le module Nintendo et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Nintendo DS seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateurs Sony PlayStation 2 et PSP

- [x] Ajouter des adaptateurs Sony PlayStation 2 et PSP concrets avec les protocoles propres aux cœurs PCSX2 et PPSSPP.
  - [x] Créer `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/` et `src/GWGUI.Emulation.Sony/Emulators/Ppsspp/` en reprenant les contrats, services et hôtes Sony déjà intégrés, puis remplacer les identifiants, téléchargements et médias.
  - [x] Modifier `src/GWGUI.Emulation.Sony/Resources/00-Base/Emulation.resx` pour ajouter les erreurs et descriptions localisées des deux cœurs.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` les assertions de sélection PCSX2 pour `PlayStation2` et PPSSPP pour `Psp`.
  - [x] Relier le moteur Sony aux deux adaptateurs par sa découverte d’adaptateurs.
  - [x] Compiler le module Sony et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit des adaptateurs Sony PlayStation 2 et PSP seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur NEC PC-FX

- [x] Ajouter un adaptateur NEC PC-FX concret avec le protocole propre au cœur Beetle PC-FX.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Nec/Emulators/BeetlePcfx/` en reprenant le protocole d’hôte NEC déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux du cœur PC-FX.
  - [x] Modifier `src/GWGUI.Emulation.Nec/Resources/00-Base/Emulation.resx` pour ajouter les erreurs et la description localisées de Beetle PC-FX.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Beetle PC-FX pour `PcFx`.
  - [x] Relier le moteur NEC à l’adaptateur Beetle PC-FX par sa découverte d’adaptateurs.
  - [x] Compiler le module NEC et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur NEC PC-FX seulement après réussite de toutes les sous-tâches ci-dessus.

### Adaptateur Sega Dreamcast

- [x] Ajouter un adaptateur Sega Dreamcast concret avec le protocole propre au cœur Flycast.
  - [x] Créer les fichiers `src/GWGUI.Emulation.Sega/Emulators/Flycast/` en reprenant le protocole d’hôte Sega déjà intégré, puis remplacer les identifiants, le téléchargement et les médias par ceux de Flycast.
  - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/00-Base/Emulation.resx` pour ajouter les erreurs et la description localisées de Flycast.
  - [x] Ajouter dans `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` l’assertion de sélection de Flycast pour `Dreamcast`.
  - [x] Relier le moteur Sega à l’adaptateur Flycast par sa découverte d’adaptateurs.
  - [x] Compiler le module Sega et exécuter les tests d’architecture et de création de runtime.
- [x] Stage et commit de l’adaptateur Sega Dreamcast seulement après réussite de toutes les sous-tâches ci-dessus.

- [x] Créer les cinq projets de modules, leurs manifestes et leurs factories.
  - [x] Ajouter la façade `IEmulationModule` et le catalogue de machines Sega.
- [x] Ajouter la façade `IEmulationModule` et le catalogue de machines Nintendo.
- [x] Ajouter la façade `IEmulationModule` et le catalogue de machines Sony.
- [x] Ajouter la façade `IEmulationModule` et le catalogue de machines Microsoft.
- [x] Ajouter la façade `IEmulationModule` et le catalogue de machines NEC.
- [ ] Brancher les cœurs, un adaptateur à la fois, dans `Emulators/<Cœur>`.
  - [ ] Documenter pour chaque adaptateur son identifiant, sa version et ses médias.
  - [ ] Refuser explicitement une machine dont aucun adaptateur n'est installé.
- [ ] Compléter les formats communs de `MediaEngine`.
  - [ ] Ajouter les lecteurs/writers nommés des cartouches console et leurs représentations par banques.
  - [ ] Ajouter les lecteurs de banques nommés par famille à `MediaFileSystems`.
  - [ ] Ajouter les règles de contenu ROM pour les extensions de cartouches et les banques extraites à `MediaAnalysis`.
  - [ ] Ajouter les Readers, Writers, représentations visuelles et conversions des autres supports listés.
- [ ] Localiser les nouveaux libellés avec la base commune et Argos.
- [ ] Ajouter les tests utiles, puis exécuter le build Debug avec tous les modules.

## Fichiers de réalisation

Les feuilles suivantes sont les actions concrètes restantes ; chaque case ne
sera cochée qu'après écriture du fichier et compilation du comportement associé.

- [x] Créer `src/GWGUI.Emulation.Sega/GWGUI.Emulation.Sega.csproj` avec la référence SDK.
- [x] Créer `src/GWGUI.Emulation.Sega/module.json` et `Modules/SegaEmulationModuleFactory.cs`.
- [x] Créer `src/GWGUI.Emulation.Sega/Common/Machines/MachineCatalog.cs` et les configurations Sega.
- [x] Créer les contrats, interfaces et services `Common`/`Common/Machines/Common` de Sega en reprenant les frontières des modules existants.
- [x] Créer les adaptateurs concrets `src/GWGUI.Emulation.Sega/Emulators/<Cœur>/*` pour les cœurs Sega retenus.
- [x] Créer `src/GWGUI.Emulation.Nintendo/GWGUI.Emulation.Nintendo.csproj`, `module.json` et la factory.
- [x] Créer `src/GWGUI.Emulation.Nintendo/Common/Machines/MachineCatalog.cs` et les configurations Nintendo.
- [ ] Créer les adaptateurs concrets `src/GWGUI.Emulation.Nintendo/Emulators/<Cœur>/*` pour NES/FDS, SNES, GB/GBA, N64, DS, 3DS, GameCube/Wii et Wii U.
- [x] Créer `src/GWGUI.Emulation.Sony/GWGUI.Emulation.Sony.csproj`, `module.json` et la factory.
- [x] Créer `src/GWGUI.Emulation.Sony/Common/Machines/MachineCatalog.cs` et les configurations Sony.
- [x] Créer les adaptateurs concrets `src/GWGUI.Emulation.Sony/Emulators/<Cœur>/*` pour PS1, PS2 et PSP.
- [x] Créer `src/GWGUI.Emulation.Nec/GWGUI.Emulation.Nec.csproj`, `module.json` et la factory.
- [x] Créer `src/GWGUI.Emulation.Nec/Common/Machines/MachineCatalog.cs` et les configurations PC Engine.
- [x] Créer les adaptateurs concrets `src/GWGUI.Emulation.Nec/Emulators/<Cœur>/*` pour PCE, SGX et PC-FX.
- [x] Créer `src/GWGUI.Emulation.Microsoft/GWGUI.Emulation.Microsoft.csproj`, `module.json` et la factory.
- [ ] Créer les lecteurs MediaEngine pour FDS, Sega Card/HuCard et CDI/GDI/CHD multi-pistes.

### Support média Nintendo Wii U (WUD/WUX)

- [x] Ajouter la lecture nommée des images Wii U dans `MediaEngine` sans introduire de format générique.
  - [x] Écrire `src/GWGUI.MediaFileSystems/Constants/MediaImageFormatIds.cs` avec l'identifiant `nintendo.wiiu`.
  - [x] Écrire `src/GWGUI.MediaEngine/Constants/DiskImageFileExtensions.cs`, `DiskImageFormatIds.cs`, `MediaImageWriterIds.cs` et `src/GWGUI.MediaAnalysis/Constants/FileTypeExtensions.cs` avec les constantes `.wud` et `.wux`.
  - [x] Ajouter la clé invariante `Format.nintendo.wiiu` dans `src/GWGUI.App/Resources/00-Base/Formats.resx` pour éviter l'affichage de la clé technique.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/WiiU/WiiUFormat.cs`, `WiiUMetadataConstants.cs`, `WiiUWuxRandomAccessData.cs` et `WiiUReader.cs` pour lire les données logiques WUD et l'index WUX.
  - [x] Enregistrer `WiiUReader` dans `src/GWGUI.MediaEngine/Images/Reading/MediaRecognitionComposition.cs` et le format dans `ImageFormatCatalog.cs`.
- [x] Ajouter la conversion Wii U vers WUD/WUX dans `MediaEngine`.
  - [x] Créer `src/GWGUI.MediaEngine/Images/Formats/Optical/WiiU/WiiUWriter.cs` pour écrire un flux WUD ou WUX à partir d'une représentation par blocs complète.
  - [x] Enregistrer le writer dans `src/GWGUI.MediaEngine/Images/Writing/MediaWritingComposition.cs`.
- [x] Rendre les extensions WUD/WUX visibles dans `MediaAnalysis`.
  - [x] Ajouter les deux extensions à `src/GWGUI.MediaAnalysis/Dictionaries/ContentRecognition/CommonMediaContentRecognitionTable.cs`.
- [x] Vérifier la tranche Wii U.
  - [x] Ajouter dans `tests/GWGUI.Tests/Media/WiiUMediaFormatTests.cs` la lecture WUD/WUX, la lecture aléatoire WUX, la conversion WUD et le nettoyage des fichiers temporaires.
  - [x] Exécuter les tests ciblés puis la suite `GWGUI.Tests`.
- [x] Cocher ce groupe et créer un commit unique seulement après réussite de toutes les actions ci-dessus.

### Raccordement des médias console et des options de cœur

- [x] Raccorder les supports optiques déjà représentés par le contrat commun aux familles qui en ont besoin.
  - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Enums/MediaEnums.cs`, `Contracts/ModelContracts.cs`, `Dictionaries/ModelCatalog.cs`, `Functions/MediaFunctions.cs`, `Functions/StorageFunctions.cs`, `Functions/ConfigurationFunctions.cs` et `Modules/NintendoEmulationModule.cs` pour déclarer et convertir le lecteur optique des modèles GameCube, Wii et Wii U.
  - [x] Modifier les mêmes fichiers correspondants dans `src/GWGUI.Emulation.Sega/` pour Mega-CD, Saturn et Dreamcast.
  - [x] Modifier les mêmes fichiers correspondants dans `src/GWGUI.Emulation.Sony/` pour les modèles PlayStation à support optique.
  - [x] Ajouter dans chaque `StorageConstants.cs` concerné les extensions optiques déjà connues du MediaEngine (`.cue`, `.chd`, `.iso`, `.gdi`, `.cdi`) et les libellés de lecteur.
- [x] Vérifier le traitement des options propres aux cœurs sans introduire de couche générique.
  - [x] Vérifier les fonctions `*OptionFunctions.cs` des adaptateurs existants : elles conservent les options persistées et le protocole du cœur valide les clés et valeurs exposées par le cœur.
  - [x] Ajouter `tests/GWGUI.Tests/Emulation/MachineAdapters/ConsoleFamilyMediaScenarios.cs` pour vérifier le mapping optique et la conservation des options.
- [x] Compiler les modules Nintendo, Sega et Sony et exécuter le test ciblé puis la suite complète.
- [x] Cocher ce groupe après réussite de toutes les actions ci-dessus.

### Support du format Famicom Disk `.fds`

- [x] Ajouter `Fds` dans `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Constants/StorageConstants.cs` et le modèle `FamicomDisk` dans `ModelConstants.cs`.
- [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Functions/StorageFunctions.cs` pour proposer `.fds` uniquement au lecteur du Famicom Disk System.
- [x] Ajouter et exécuter `tests/GWGUI.Tests/Emulation/MachineAdapters/ConsoleFamilyMediaScenarios.cs` pour vérifier que `.fds` est accepté.
- [x] Cocher ce groupe après compilation et réussite de la suite complète.

### Structure des adaptateurs console

- [x] Supprimer le niveau intermédiaire `Core` des adaptateurs que cette branche a créés.
  - [x] Placer directement les fichiers de `src/GWGUI.Emulation.Nec/Emulators/BeetlePce/` et `BeetlePcfx/` dans leurs dossiers d’émulateur respectifs.
  - [x] Placer directement les fichiers de `src/GWGUI.Emulation.Nintendo/Emulators/BeetleVb/`, `Citra/`, `Dolphin/`, `Gambatte/`, `GameWatch/`, `MelonDs/`, `Mesen/`, `Mgba/`, `Mupen64PlusNext/` et `Snes9x/` dans leurs dossiers d’émulateur respectifs.
  - [x] Placer directement les fichiers de `src/GWGUI.Emulation.Sega/Emulators/Flycast/`, `GenesisPlusGX/` et `Yabause/` dans leurs dossiers d’émulateur respectifs.
  - [x] Placer directement les fichiers de `src/GWGUI.Emulation.Sony/Emulators/Pcsx2/`, `Ppsspp/` et `SwanStation/` dans leurs dossiers d’émulateur respectifs.
- [x] Vérifier que les namespaces, les découvertes de factories, les projets et les tests ne dépendent pas du chemin `Core`.
- [x] Compiler les cinq modules concernés et exécuter la suite complète avant le commit.
- [x] Cocher ce groupe uniquement après réalisation effective de tous les déplacements et vérifications.

Ce document ne prétend pas qu'un cœur ou un format est déjà implémenté : chaque
case sera cochée seulement après le fichier et le comportement correspondants.
