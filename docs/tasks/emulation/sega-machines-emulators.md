# Intégration Sega

- [x] Intégrer les machines et émulateurs Sega du catalogue Libretro
  - [x] Préparer les données vérifiées
    - [x] Créer `artifacts/sega-integration/catalog.py` et les métadonnées temporaires : consulter les métadonnées et sources officielles des quinze cœurs Sega et de blueMSX, relever leurs extensions et options ; compléter par une inspection isolée des DLL après autorisation explicite, conserver les sources de compatibilité dans ce document.
  - [x] Relier les émulateurs au Common générique
    - [x] Créer `src/GWGUI.Emulation.Sega/Emulators/Common/Interop/**/*.cs` à partir du transport Nintendo validé : adapter les espaces de noms, catalogues, options et firmware à Sega ; conserver les fermetures et libérations dans les finally.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/*/**/*.cs` : définitions, constantes, options, BIOS et fabriques explicites pour chaque cœur ; supprimer les anciens transports dupliqués remplacés.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Interfaces/IEmulatorAdapter.cs`, `Common/Contracts/FirmwareSlot.cs`, `Common/Dictionaries/EmulatorCatalog.cs` et `Common/Machines/Common/Contracts/ConfigurationContracts.cs` : capacités génériques et chemins de firmware multiples ; catalogue explicite et défauts existants préservés.
  - [x] Compléter les machines et leur configuration
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Constants/ModelConstants.cs`, `Enums/*.cs`, `Dictionaries/ModelCatalog.cs` et `Dictionaries/MachineCatalog.cs` : SF-7000, Mega-CD, 32X, System SP, ST-V, Model 3 et VMU avec identifiants, caractéristiques et associations explicites.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Common/Machines/Common/Functions/{SettingsFunctions,StorageFunctions,ConfigurationFunctions,FirmwareConfigurationFunctions}.cs` et `Modules/SegaEmulationModule.cs` : champs des options et BIOS du cœur sélectionné, formats du cœur sélectionné et sélection de fichiers BIOS de nom libre.
  - [x] Finaliser les raccordements vérifiés
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/PicoDrive/Constants/CoreConstants.cs` et les fonctions de contenu GX : préserver SG-1000/SC-3000 et leurs formats existants.
    - [x] Modifier `docs/tasks/emulation/sega-machines-emulators.md` : relever la gestion des configurations intégrées par Supermodel et éviter leur duplication.
    - [x] Modifier les fichiers C# Sega concernés par les erreurs de compilation : corriger les références du transport mutualisé et compiler le module.
  - [x] Préparer la validation
    - [x] Modifier `tests/GWGUI.Tests/{Architecture/ConsoleFamilyModuleTests,Emulation/Sega/SegaMachineLifecycleTests}.cs` et créer `Emulation/Sega/SegaEmulatorCatalogTests.cs` : adapter les contrôles au transport mutualisé, couvrir les associations et les BIOS, corriger les raccordements révélés par ces contrôles.
  - [x] Compléter les ressources
    - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/*/*.resx` : options traduites avec Argos dans toutes les langues, noms et valeurs invariantes uniquement dans la base commune ; messages génériques du transport.
    - [x] Créer les PNG manquants dans `src/GWGUI.Emulation.Sega/Assets/Machines/` : images transparentes de 512 pixels maximum ; réutiliser les images existantes pour les variantes identiques.
  - [x] Valider et nettoyer
    - [x] Modifier `src/GWGUI.Emulation.Sega/Emulators/{PicoDrive,SmsPlusGX}/Constants/FirmwareConstants.cs`, les constantes du transport et `Common/Machines/Common/Functions/ConfigurationFunctions.cs` : sélectionner les BIOS par machine, supprimer les résidus Nintendo et afficher correctement la RAM du VMU ; adapter les anciens tests de firmware aux champs multiples.
    - [x] Modifier `src/GWGUI.Emulation.Sega/Resources/*/*.resx` et les tests de ressources concernés : supprimer les doublons de clés et garder les libellés traduits dans les langues, les valeurs invariantes dans la base.
    - [x] Modifier `Common/Machines/Common/Functions/{ConfigurationFunctions,StorageFunctions}.cs` et `tests/GWGUI.Tests/{Architecture/ConsoleFamilyModuleTests,Emulation/Sega/SegaMachineLifecycleTests}.cs` : supprimer les lignes vides finales ajoutées.
    - [x] Modifier `docs/tasks/emulation/sega-machines-emulators.md` : inscrire les résultats des 76 tests Sega (associations, BIOS multiples, options, formats et ressources dans les 29 langues) et du build Debug avec huit modules.
    - [x] Supprimer `artifacts/sega-integration` avec ses sources, scripts et résultats temporaires après validation et destruction des processus de contrôle.

Sources initiales :

- Liste du dépôt : `docs/tasks/emulation/liste-emulateur-libretro.md`, sections Sega et Ordinateurs et machines diverses.
- blueMSX : https://github.com/libretro/blueMSX-libretro/blob/master/README.md (SG-1000, SC-3000, SF-7000).
- Supermodel : https://docs.libretro.com/library/supermodel/ (Model 3).

Métadonnées : les seize DLL Windows x64 du buildbot ont été inspectées après autorisation explicite ; 741 options relevées sans initialiser de jeu. Les processus sont terminés et attendus. `ymir_libretro.dll` publie désormais le nom interne `Emir`.



Supermodel actuel : le dépôt officiel libretro/Libretro-Supermodel indique que Games.xml et Supermodel.ini sont intégrés au cœur et extraits au premier usage dans system/supermodel. Aucun téléchargement supplémentaire requis (README officiel, 8 octobre 2026).






Images :
- Mega-CD, 32X et Model 3 : fabricecaruso/es-theme-carbon, art/consoles (photos de matériel, PNG transparents).
- VMU : Evan-Amos, détourage Gunnar.offel, domaine public, https://commons.wikimedia.org/wiki/File:Sega-Dreamcast-VMU.png ; réduction à 512 pixels.
- SF-7000 : https://www.sc-3000.com/images/sega-sf-7000/galleries/sega-sf-7000-unit/sega-sf-7000.jpg ; détourage et réduction.
- System SP : https://i.ebayimg.com/images/g/y5kAAOSwMr1lp1oi/s-l1200.jpg ; détourage de la carte et suppression de la main.
- ST-V : https://www.tops-game.jp/upload/save_image/PCB/14_SEGA/SEGA-STV-0001.jpg ; détourage et réduction.





Validation du 8 octobre 2026 :
- 76 contrôles Sega/GenesisPlusGX réussis, dont les ressources des options dans les 29 langues. Les tests natifs conditionnels ne remplacent pas un essai de chaque jeu.
- Les seize DLL ont fourni leurs métadonnées et 741 options ; aucun jeu n'a été lancé lors de cette inspection.
- Build Debug avec --modules=A terminé, code de sortie 0. Exécutable et huit DLL présents dans build/Debug/GW GUI.
- Sept nouvelles images vérifiées avec canal alpha et dimensions maximales de 512 pixels.
- git diff --check réussi.


- [x] Corriger le rangement des options Sega dans l’interface
  - [x] Définir leur destination
    - [x] Modifier `Common/Contracts/CoreContracts.cs` et `Emulators/*/Constants/OptionConstants.cs` : déclarer explicitement l’onglet de chaque option et le redémarrage seulement lorsqu’il est requis par le cœur.
    - [x] Modifier `Emulators/Common/Interop/{Functions/CoreSettingsFunctions,Constants/CoreSettingsConstants}.cs` : créer les blocs dans leurs onglets, utiliser les glyphes existants et une colonne pour éviter les libellés tronqués.
  - [x] Corriger les libellés visibles
    - [x] Modifier `Resources/*/Emulators.resx` : reprendre les libellés Activé/Désactivé existants dans chaque langue, supprimer les mentions de redémarrage répétées et corriger les libellés français GX/GX Wide.
  - [x] Garder les options accessibles
    - [x] Modifier `Modules/SegaEmulationModule.cs` : conserver un onglet visible lorsqu’il contient des options du cœur, même sans sélecteur de BIOS externe.
  - [x] Valider et livrer
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaEmulatorCatalogTests.cs` : contrôler la destination des options ROM, vidéo, audio, CPU, stockage et manettes ; inscrire ici les résultats des tests et du build Debug avec les huit modules.


Correction de rangement validée : 86 contrôles Sega réussis ; build Debug --modules=A terminé avec code 0, exécutable et huit modules présents. Options réparties explicitement dans les onglets, glyphes corrigés, une colonne, mentions de redémarrage non répétées et libellés Activé/Désactivé repris dans les 29 langues.


- [x] Corriger les options selon la machine configurée
  - [x] Déclarer leur portée
    - [x] Modifier `Common/Contracts/CoreContracts.cs`, créer `Emulators/{GenesisPlusGX,GenesisPlusGXWide,Flycast,PicoDrive,Gearsystem,BlueMSX,BlastEm,Kronos}/Constants/OptionMachineConstants.cs` et modifier leurs `OptionConstants.cs` : associer les options propres à un système uniquement aux machines concernées.
    - [x] Modifier `Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : filtrer les champs par modèle configuré et rétablir les valeurs par défaut des options non applicables au changement de machine.
  - [x] Intégrer les champs dans les cadres existants
    - [x] Modifier `Common/Machines/Common/Functions/SettingsFunctions.cs` : rattacher les champs du cœur aux cadres de leur onglet sans créer un second cadre Général.
  - [x] Reprendre les choix et les modèles proposés
    - [x] Modifier `Common/Contracts/CoreContracts.cs`, `Emulators/BlueMSX/Constants/{OptionConstants,OptionMachineConstants}.cs` et `Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` : limiter les types de cartouches blueMSX aux types Sega de la machine configurée.
    - [x] Modifier `Emulators/BlastEm/Constants/OptionConstants.cs` et `Emulators/BlueMSX/Constants/OptionConstants.cs` : écarter les réglages de systèmes absents de leurs machines Sega.
  - [x] Vérifier les capacités des machines
    - [x] Modifier `Common/Machines/Common/Dictionaries/ModelCatalog.cs` : ne pas proposer de souris pour les machines dont le catalogue de contrôleurs ne prévoit aucune souris.
    - [x] Modifier `Emulators/*/Constants/OptionMachineConstants.cs` : retirer les groupes de machines sans usage après la reprise des options.
  - [x] Reprendre les descriptions des émulateurs
    - [x] Modifier `Resources/*/Emulators.resx` : utiliser les noms invariants lisibles des machines et retraduire la phrase courte avec Argos, sans traduire les noms des systèmes.
  - [x] Valider la correction
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Sega/SegaEmulatorCatalogTests.cs` : contrôler SG-1000, SC-3000, Master System, Game Gear, Mega Drive, Mega-CD et les systèmes Flycast ; inscrire les résultats des tests et du build Debug avec les huit modules dans ce document.

Reprise par machine : 101 contrôles Sega réussis ; tous les couples machine/cœur du catalogue sont parcourus pour vérifier la portée des champs. SG-1000 ne propose pas de Mega-CD, FM Mega Drive ni souris. Les cartouches blueMSX sont limitées aux types Sega. Les réglages Dreamcast/VMU ne sont pas proposés sur les machines arcade Flycast. Les cadres du cœur sont intégrés aux cadres existants. Descriptions retraduites dans les 29 langues avec noms des machines invariants. Build Debug --modules=A terminé avec code 0.
