# Extensions disquette du BBC Micro Model A

- [x] Préparer le travail selon la demande confirmée.
  - [x] Modifier `docs/tasks/acorn-floppy-extensions.md` : remplacer le cadrage initial par le périmètre confirmé et les actions ordonnées ci-dessous.
- [x] Ajouter la présentation commune Extensions.
  - [x] Modifier `src/GWGUI.Emulation/Enums/EmulationMachineTab.cs` et `src/GWGUI.App/Constants/Emulation/EmulationMachineTabConstants.cs` : ajouter Extensions en préservant les valeurs existantes.
  - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs` et `src/GWGUI.App/Views/Controls/Emulation/Options/ModuleSettings/EmulationModuleSettingsSection.{Layout,Persistence}.cs` : réutiliser le gestionnaire ROM dans les onglets qui déclarent des champs firmware, respecter leur activation et conserver la consultation des ROM inconnues.
- [x] Définir et raccorder les extensions Acorn.
  - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Constants/FloppyExtensionConstants.cs` et `Functions/FloppyExtensionFunctions.cs` : définir les clés, les deux champs ROM, les boutons radio, Solidisk, la persistance et les autorisations communes aux deux lecteurs.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Factories/AcornEmuMachineFactory.cs`, `Functions/FirmwareFunctions.cs` et `Functions/StorageFunctions.cs` : déplacer le réglage vers Extensions, séparer les destinations ROM et les banques chargées, préserver les anciens chemins enregistrés et supprimer le contrôleur des réglages de chaque lecteur.
  - [x] Modifier les mêmes fonctions de stockage : accepter les conteneurs gérés par MediaEngine indépendamment du contrôleur choisi, en conservant les contraintes du lecteur physique et la réaction matérielle du contrôleur au contenu.
- [x] Traduire et documenter.
  - [x] Modifier les ressources App et Acorn dans toutes les langues avec Argos : traduire Extensions et Extensions Disquette ; garder les noms invariants dans la base commune.
  - [x] Modifier la documentation du Model A sous `docs/project/emulations/Acorn/BBC Micro/Model A/` : décrire les champs, leur persistance, les autorisations, les ROM et le contrôleur commun.
- [x] Vérifier sans perturber le build utilisateur.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Functions/FloppyExtensionFunctions.cs` et le gestionnaire firmware App : conserver les identifiants de machine des emplacements ROM et détacher les anciennes vues lors de la reconstruction de l'éditeur.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuWd1770Tests.cs`, `AcornEmuScsiTests.cs` et créer `AcornEmuExtensionTests.cs` au même emplacement : adapter les assertions au contrôleur commun dans Extensions et vérifier les autorisations, les chemins persistants et la migration de l'ancien champ sans fichiers externes.
  - [x] Modifier les fichiers C#, RESX et documents concernés : vérifier leur encodage UTF-8, normaliser leurs fins de ligne et corriger les erreurs découvertes par les vérifications ciblées.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuDiskControllerTests.cs` : remplacer l'ancien emplacement DFS par le champ 8271 et vérifier son caractère obligatoire uniquement avec une interface installée.
  - [x] Modifier `EmulationModuleSettingsSection.Layout.cs` et `EmulationFirmwareManagementController.cs` : supprimer les lignes vides doublées par l'édition, en conservant uniquement les changements fonctionnels demandés.
  - [x] Créer si nécessaire un vérificateur temporaire sous `artifacts/acorn-extensions-check/` : vérifier la description des champs, la persistance, les banques ROM et les autorisations ; supprimer ensuite ce vérificateur et ses artefacts temporaires.
  - [x] Créer le build `build/Debug/GW GUI/gwgui.exe` et `Modules/acorn/gwgui.emulation.acorn.dll` avec `scripts/local-building.cmd --building=debug --modules=acorn`, puis vérifier leur présence et la réussite du script.
  - [x] Modifier ce document : consigner les vérifications réellement effectuées et les limites restantes.

## Demande confirmée

Extensions Disquette est un cadre de l'onglet Extensions. Il déclare un choix commun 8271/WD1770, un champ ROM par contrôleur et une case Solidisk. Sans Solidisk, le champ du contrôleur non sélectionné est grisé ; son chemin reste enregistré. Solidisk rend les deux champs accessibles. Une ROM renseignée rend son contrôleur utilisable, sous réserve de son autorisation par le choix commun. Le contrôleur actif exige sa ROM au démarrage, pas lors de la simple édition d'une configuration incomplète.

La liste et le bouton d'ouverture du dossier ROM réutilisent le gestionnaire commun. Les deux lecteurs suivent la configuration commune ; aucun choix indépendant par lecteur n'est ajouté dans cette réalisation. La carte double est représentée par ses deux ROM disponibles et son choix de contrôleur actif commun, comme le commutateur physique. Il ne s'agit pas d'une bascule automatique suivant l'extension du fichier.

Le contenu d'une disquette peut nécessiter un contrôleur particulier, notamment lorsqu'un programme accède directement à ses registres. On laisse la machine et le contrôleur émuler cette lecture. Le conteneur image est ouvert par MediaEngine : son extension seule ne doit pas faire office de liste blanche de compatibilité du contrôleur. Les contraintes physiques du lecteur restent applicables.

Une seule ROM est prévue par contrôleur dans cette demande. Cela n'ajoute pas plusieurs ROM WD1770 pour DFS et ADFS simultanément.

Référence : [Solidisk WD1770 + 8271 Disc Interface](https://www.4corn.co.uk/archive/showpdfs/11%20Micro%20User-Electron%20User%20Nov%201987/Solidisk%20-%20WD-1770%2B8271%20Disc%20Interface.pdf).


## Résultats

- 207 vérifications réussies : 206 cas Acorn (existants et nouveaux) et une vérification WPF temporaire. Les nouvelles vérifications autonomes sont conservées dans `AcornEmuExtensionTests.cs` ; les assertions existantes ont été adaptées au déplacement du contrôleur dans Extensions.
- La vérification WPF utilise la façade publique du module avec des candidats en mémoire : listes ROM et Extensions indépendantes, fichier inconnu visible, Utiliser inactif pour WD1770 grisé, refus effectif même si un clic est déclenché, réactivation avec Solidisk. Aucune fenêtre ouverte ; le thread STA et son Dispatcher ont été arrêtés et rejoints dans finally.
- Le contrat Common des identités de ROM déclare désormais `FirmwareDiskController` ; les identités DFS 0.9H, DFS/NET 1.0 et DNFS 1.20 sont attribuées au 8271. Les identités non évaluées gardent Unknown, sans déduction depuis leur nom de fichier.
- 30 catalogues App et 30 catalogues Acorn contiennent les nouveaux titres. Les libellés invariants des deux champs ROM et de Solidisk restent dans la base commune Acorn. UTF-8, XML et `git diff --check` vérifiés.
- Le projet de vérification temporaire et tous ses artefacts sous `artifacts/acorn-extensions-check/` ont été supprimés après la fin du processus de test.
- `scripts/local-building.cmd --building=debug --modules=acorn` terminé avec code 0. Le script ajoute lui-même Example à la sélection ; aucun autre module de marque construit. Exécutable et DLL Acorn vérifiés dans `build/Debug/GW GUI/`.
- Aucun GW GUI n'était lancé avant le build. Aucune configuration utilisateur ni ROM externe modifiée. Le test du build livré par l'utilisateur reste à faire.


## Correction du gel au clic Utiliser

- [x] Corriger le parcours de reconnaissance utilisé synchroniquement par Utiliser.
  - [x] Créer un vérificateur temporaire dans `artifacts/acorn-firmware-use-check/` : reproduire avec le véritable scanner Acorn et un contexte de synchronisation sans boucle de messages ; consigner le résultat borné dans ce document.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Factories/AcornEmuMachineFactory.cs` : empêcher la capture du contexte UI dans la nouvelle projection asynchrone des candidats.
  - [x] Modifier le vérificateur temporaire pour vérifier le véritable clic Utiliser et la reconnaissance avec fichier externe ; supprimer ensuite le vérificateur et tous ses fichiers temporaires après arrêt effectif du processus.
  - [x] Créer le build Debug Acorn corrigé avec `scripts/local-building.cmd --building=debug --modules=acorn` ; vérifier l'exécutable et la DLL.
  - [x] Modifier ce document : consigner la reproduction avant/après, la correction et le résultat du build.

Reproduction avant correction : première tentative bloquée après 2 secondes, tâche inachevée et une continuation en attente sur le contexte UI. La continuation est ensuite exécutée explicitement par le vérificateur afin de libérer le scanner avant sa fermeture.

Après correction : 20 reconnaissances successives terminées sans boucle de messages et sans continuation UI en attente. Le véritable bouton WPF Utiliser appelle ensuite le véritable gestionnaire Acorn (relecture et revalidation du fichier DNFS externe) : retour du clic et chemin enregistré dans la configuration en mémoire, sans modifier le fichier de configuration utilisateur. Le Dispatcher du vérificateur est arrêté dans finally, le processus a terminé avec code 0 et tous les fichiers temporaires ont été supprimés.

Build corrigé terminé avec code 0 ; `build/Debug/GW GUI/gwgui.exe` et `Modules/acorn/gwgui.emulation.acorn.dll` présents et actualisés. Le processus utilisateur figé (PID 56384) a été arrêté et sa disparition vérifiée. Aucun autre réglage fonctionnel modifié pendant cette correction.
