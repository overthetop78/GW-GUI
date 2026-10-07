# Reprise complete d Emulation.Commodore

- [x] Reorganiser tout le module Commodore et integrer les machines demandees
  - [x] Repartir les responsabilites des machines
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Contracts/ModelContracts.cs` et creer `Common/Machines/Amiga/Common/Contracts/ModelContracts.cs` : modele materiel commun et modele Amiga specialise, sans IsAmiga ni emulateur par defaut.
    - [x] Modifier les catalogues AmigaComputers, AmigaCD32 et CommodoreCDTV sous `src/GWGUI.Emulation.Commodore/Common/Machines` : modeles nommes, explicitement specialises pour les machines Amiga.
    - [x] Creer `Constants/ModelConstants.cs` et `Dictionaries/ModelCatalog.cs` pour les familles C64, C128, CbmII, Pet, Plus4 et Vic20 sous `src/GWGUI.Emulation.Commodore/Common/Machines` : les 29 modeles demandes avec constantes materielles semantiques.
    - [x] Deplacer `src/GWGUI.Emulation.Commodore/Common/Machines/Amiga/Common/Contracts/ModelContracts.cs` vers `Common/Machines/AmigaComputers/Contracts/ModelContracts.cs`, modifier son namespace et supprimer les dossiers Amiga/Common devenus vides : respecter les familles existantes et ne creer aucun Machine Amiga Common.
    - [x] Modifier les catalogues ModelCatalog et MachineCatalog et les constantes associees sous `src/GWGUI.Emulation.Commodore/Common/Machines/Common` : lister explicitement les 39 machines, leurs cles de ressources et leurs images.
    - [x] Deplacer les fonctions SettingsFunctions, StorageFunctions et ConfigurationFunctions, FirmwareCatalog et leurs constantes Amiga de `Common/Machines/Common` vers `Common/Machines/AmigaComputers` sous `src/GWGUI.Emulation.Commodore` : conserver les implementations Amiga sous des noms et namespaces propres aux machines Amiga.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/EmulationGlobalUsings.cs`, les fonctions InputFunctions et les dictionnaires ControllerCatalog : separer les commandes generales des capacites Amiga.
  - [x] Rendre les contrats et services independants des machines Amiga
    - [x] Modifier `Common/Machines/AmigaComputers/Constants/HardwareConstants.cs`, les contrats et catalogues de toutes les familles sous `src/GWGUI.Emulation.Commodore/Common/Machines` : remplacer les valeurs materielles et capacites brutes par des constantes semantiques; corriger Unknown dans les touches generiques.
    - [x] Creer `Common/Machines/Common/Enums/MemoryEnums.cs` et modifier les contrats Model et les catalogues et constantes de toutes les familles sous `src/GWGUI.Emulation.Commodore/Common/Machines` : remplacer les capacites RAM numeriques par RamCapacity et conserver les conversions KiB/MiB explicites.
    - [x] Creer `Common/Machines/Common/Enums/StorageEnums.cs` et modifier les contrats Model, les catalogues et leurs constantes sous `src/GWGUI.Emulation.Commodore/Common/Machines` : typer les limites de lecteurs avec DriveCapacity et supprimer les constantes de nombre de lecteurs redondantes.
    - [x] Refaire les constantes de description et leurs usages sur tout le module
      - [x] Creer `Common/Machines/Common/Constants/SettingsResourceKeys.cs` et `SettingsPresentationConstants.cs` sous `src/GWGUI.Emulation.Commodore` : ressources et presentation communes clairement nommees.
      - [x] Creer `Common/Machines/AmigaComputers/Constants/SettingsValueConstants.cs` et `ClockConstants.cs` sous `src/GWGUI.Emulation.Commodore` : valeurs de configuration avec noms semantiques et horloges materielles.
      - [x] Modifier `Common/Machines/Common/Enums/MemoryEnums.cs`, les trois fichiers SettingsFunctions et ConfigurationFunctions sous `Common/Machines/AmigaComputers/Functions`, HardwareConstants, les references du module et des emulateurs sous `src/GWGUI.Emulation.Commodore` : choix de RAM types, suppression des noms Value et des doublons de modeles, CPU et ressources.
      - [x] Creer `Common/Machines/Common/Enums/CpuEnums.cs` et modifier tous les contrats Model et catalogues sous `src/GWGUI.Emulation.Commodore/Common/Machines`, les choix de CPU/FPU et leurs constantes : typer les processeurs par CpuModel, separer les CPU simultanes des alternatives configurables et corriger les references de constantes remplacees.
      - [x] Creer `Common/Machines/Common/Enums/ChipsetEnums.cs` et `Constants/ChipsetDisplayConstants.cs`, modifier les contrats Model, tous les catalogues et constantes de familles sous `src/GWGUI.Emulation.Commodore/Common/Machines` : typer chaque composant de chipset sans chaines materielles brutes et conserver les assemblages propres aux machines.
      - [x] Supprimer `src/GWGUI.Emulation.Commodore/Common/Machines/AmigaComputers/Constants/SettingsDescriptionTextConstants.cs` et `SettingsDescriptionChoicesConstants.cs` : retirer les anciennes classes fourre-tout apres remplacement de leurs usages.
    - [x] Modifier ConfigurationContracts, FirmwareContracts, CoreEnums et MediaEnums sous `src/GWGUI.Emulation.Commodore/Common/Machines/Common` : selection de plusieurs firmwares, emulateurs demandes et medias distincts des ROM systeme.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Enums/MemoryEnums.cs` et toutes ses references : nommer les capacites _512KB, _1MB, _2MB, _4MB, etc., garder les valeurs en KiB et retirer les alias redondants.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Dictionaries/EmulatorCatalog.cs` : liste explicite des adaptateurs et selection d un coeur compatible.
    - [x] Modifier ConfigurationStore, StateStore, Machine, Machine.Lifecycle et Machine.Commands sous `src/GWGUI.Emulation.Commodore/Common/Services`, `Common/Machines/Common/Contracts/StateContracts.cs` et `Common/Machines/Common/Constants/StateStoreConstants.cs` : chemins et empreintes de plusieurs firmwares, entree selon les capacites materielles, fermeture des ressources dans finally.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Functions/MediaFunctions.cs`, creer les fonctions communes SettingsFunctions, StorageFunctions et ConfigurationFunctions, modifier `Common/Interfaces/IEmulatorAdapter.cs`, ConfigurationContracts et creer `Common/Machines/Common/Constants/SettingsConstants.cs` : deleguer firmware et medias au backend, utiliser les contrats SDK generiques et les chemins de firmware par role.
  - [x] Integrer les implementations d emulateurs
    - [x] Modifier MachineFactory, UaeMachineFactory, ExternalCore, CoreReleaseService, CoreDefinition et ExternalCoreInstaller sous `src/GWGUI.Emulation.Commodore/Emulators/Common` : deleguer firmware, contenu et controleurs au backend et retirer les suppositions Amiga des services partages.
    - [x] Modifier les fabriques PUAE, PUAE2021 et Amiberry et creer `Emulators/Common/Interop/Factories/AmigaMachineFactory.cs` sous `src/GWGUI.Emulation.Commodore` : adaptateurs reserves aux dix machines Amiga et preparation Kickstart specialisee.
    - [x] Modifier `artifacts/temp/commodore-eight-bit` avec les sources sysfile, ressources et initialisation VICE; creer `src/GWGUI.Emulation.Commodore/Emulators/VICE/Common/Constants/ContentConstants.cs`, modifier `Emulators/Common/Interop/Factories/MachineFactory.cs`, `Services/ExternalCore.cs` et `Factories/AmigaMachineFactory.cs`, creer `Factories/ViceMachineFactory.cs` : preparation des commandes VICE, modeles natifs, ROM externes par role et listes de disques.
    - [x] Creer les constantes et fabriques Frodo et des dix profils VICE sous `src/GWGUI.Emulation.Commodore/Emulators` : correspondances materielles explicites et comportements propres a chaque backend.
  - [x] Relier l application et toutes les langues
    - [x] Modifier `artifacts/temp/commodore-eight-bit` : conserver les sources C64.cpp, Prefs.h et main.cpp de Frodo pour verifier le support des ROM externes demande par l utilisateur.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/Frodo/Constants/EmulatorConstants.cs`, `Factories/MachineFactory.cs` et `Emulators/Common/Interop/Services/ProcessCore.cs` : selection des quatre ROM externes Frodo, copie dans la session isolee et repertoire de travail propre au processus.
    - [x] Creer `src/GWGUI.Emulation.Commodore/Common/Machines/AmigaComputers/Enums/ClockEnums.cs`, modifier `Constants/ClockConstants.cs` et `Functions/SettingsFunctions.Choices.cs` : typer les frequences par enum en hertz et convertir en MHz seulement pour la presentation.
    - [x] Creer `Common/Machines/AmigaComputers/Enums/FirmwareEnums.cs` et `SettingsEnums.cs`, creer `Constants/SettingsChoiceConstants.cs` et `Functions/SettingsValueFunctions.cs`, modifier HardwareConstants, FirmwareCatalogConstants, les contrats et catalogues Amiga, SettingsValueConstants et SettingsFunctions, FirmwareCatalog et EmulationGlobalUsings sous `src/GWGUI.Emulation.Commodore` : enums des versions Kickstart et des choix fermes, conversions natives separees des libelles et empreintes conserves en chaines.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Modules/CommodoreEmulationModule.cs`, ConfigurationStore, AudioContracts, les SettingsConstants communs et Amiga, SettingsFunctions et ConfigurationFunctions communs, SettingsFunctions Amiga et EmulationModuleConstants : configuration, options, ROM, stockage et demarrage de chaque famille sans chemins Amiga imposes.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Resources/00-Base/Emulation.resx` et `Model.resx` : noms et donnees invariantes des nouvelles machines et firmwares.
    - [x] Modifier les ressources Emulation.resx de toutes les langues sous `src/GWGUI.Emulation.Commodore/Resources` : traduire les nouveaux libelles avec Argos et la base existante.
  - [x] Revoir tout le module : constantes, enums et responsabilites
    - [x] Creer `artifacts/temp/commodore-eight-bit/audit-literals.ps1` : relever les litteraux C# avec Roslyn; modifier tous les fichiers C# hors dossiers Constants sous `src/GWGUI.Emulation.Commodore` et leurs classes Constants proprietaires, deplacer les fichiers UAE sous Emulators/UAE et les types et fonctions propres a Amiga sous sa famille : remplacer chaque chaine et nombre litteral par une constante semantique apres lecture de son usage, y compris le code existant Amiga et les definitions natives; modifier les contrats MediaConfiguration et les conversions pour conserver les slots; creer AmigaInputSettingsFunctions et SettingsApplicationFunctions sous AmigaComputers/Functions et deleguer les reglages, entrees, validation et stockage par IEmulatorAdapter sans imposer Amiga au Common.
    - [x] Modifier `docs/tasks/emulation/commodore-eight-bit.md` : consigner l audit final des litteraux, sans considerer commentaires et declarations de constantes comme des usages bruts. Audit Roslyn : aucun litteral numerique, chaine, caractere ou fragment de chaine interpolee hors Constants/Enums, imports System.IO explicites verifies.
  - [x] Verifier puis nettoyer
    - [x] Modifier les fichiers sources concernes sous `src/GWGUI.Emulation.Commodore` : corriger les ecarts releves par les tests et la compilation, notamment conserver la creation de configuration avant selection des ROM et reserver leur exigence au demarrage.
    - [x] Creer `tests/GWGUI.Tests/Emulation/Commodore/CommodoreMachineCatalogTests.cs` : correspondances exactes, selection des coeurs et absence de proprietes Amiga imposees aux autres familles; modifier les tests Amiga et MachineAdapters existants pour les contrats FirmwarePaths, Model et les namespaces UAE actuels.
    - [x] Modifier les sources sous `src/GWGUI.Emulation.Commodore`, les tests existants et les projets temporaires si la validation revele un ecart : conserver les aides des champs ROM, corriger les erreurs de compilation liees aux nouveaux contrats et creer Common/Machines/Common/Functions/CpuDisplayFunctions.cs avec les prefixes invariants existants pour presenter correctement les CPU.
    - [x] Creer les tests natifs temporaires sous `artifacts/temp/commodore-eight-bit` : verifier les DLL disponibles et leurs cycles de vie. Les 30 demarrages sur les dix DLL disponibles passent; nettoyage des outils et artefacts prevu en derniere action.
    - [x] Modifier `Common/Interfaces/IEmulatorAdapter.cs`, `Emulators/Common/Interop/Factories/MachineFactory.cs`, `Emulators/UAE/Common/Interop/Factories/AmigaMachineFactory.cs`, `Common/Machines/Common/Functions/MediaFunctions.cs`, `Common/Machines/AmigaComputers/Functions/MediaFunctions.cs`, `Modules/CommodoreEmulationModule.cs` et creer `Common/Machines/AmigaComputers/Functions/FirmwareFunctions.cs` sous `src/GWGUI.Emulation.Commodore` : deleguer les candidats ROM et la preparation des medias a l adaptateur, conserver la conversion SCP avant creation des listes multidisquettes, sans reconnaissance Amiga dans le module commun; modifier les tests concernes pour couvrir cette delegation.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Emulators/UAE/Common/Interop/Factories/AmigaMachineFactory.cs`, `Emulators/Amiberry/Factories/AmiberryMachineFactory.cs`, leurs constantes et `Emulators/VICE/Common/Factories/ViceMachineFactory.cs` : retirer les fichiers M3U internes, monter les medias avec les configurations UAE et les arguments VICE par lecteur; modifier les tests concernes et supprimer les constantes M3U inutilisees ainsi que leur ancien message d erreur dans tous les fichiers Resources/*/Errors.resx.
    - [x] Modifier `docs/tasks/emulation/commodore-eight-bit.md` : resultats des tests et du build Debug application avec les huit modules.
    - [x] Supprimer `src/GWGUI.Emulation.Commodore/Common/Constants/VideoConstants.cs` : retirer la classe vide sans references apres regroupement des constantes video.
    - [x] Supprimer `artifacts/temp/commodore-rejected-original` et `artifacts/temp/commodore-eight-bit` apres validation : retirer les copies de la premiere tentative, sources et DLL temporaires.

## Validation du 7 octobre 2026

- Audit Roslyn : aucun litteral chaine, nombre, caractere ou fragment interpole hors Constants/Enums. Imports System.IO explicites verifies.
- 153 tests cibles : 153 reussis, aucun echec (catalogues, configuration, ROM, stockage, adaptateurs et erreurs de cycle de vie).
- 30 demarrages natifs Frodo/VICE reussis sur les dix DLL disponibles.
- 29 demarrages VICE avec deux disquettes et arguments par lecteur reussis; aucune generation M3U.
- Configuration UAE temporaire verifiee : affectation explicite DF0/DF3, sans M3U. Test supprime au nettoyage.
- scripts/local-building.cmd --building=debug --modules=A : code de sortie 0. Executable build/Debug/GW GUI/gwgui.exe et modules amstrad, atari, commodore, microsoft, nec, nintendo, sega, sony presents.
- Limite de validation : VICE x64dtv n est pas publie dans les DLL Windows x64 du buildbot officiel. Le profil C64DTV est integre et requiert une DLL locale; il n a pas ete demarre nativement. Les demarrages Amiga avec Kickstart reel ne font pas partie des essais natifs effectues.

Nettoyage realise : outils de test, sources, DLL et copies temporaires supprimes.

## PNG des machines ajoutees

- [ ] Ajouter les images des 29 nouvelles machines Commodore
  - [x] Committer la reprise existante
    - [x] Modifier l index et creer le commit Git : inclure les sources, ressources, tests et ce suivi actuellement modifies.
  - [x] Creer et relier les images
    - [x] Creer `src/GWGUI.Emulation.Commodore/Assets/Machines/{C16,C64,C64Dtv,C64SuperCpu,C128,Pet2001}.png` : images fournies par l utilisateur, GIF SuperCPU converti en PNG.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Constants/MachineCatalogConstants.cs` et `Dictionaries/MachineCatalog.cs` : relier les six PNG fournis aux machines correspondantes.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Assets/Machines/Plus4.png` : utiliser la source WEBP de l utilisateur et retirer uniquement son fond pour obtenir un PNG transparent.
    - [x] Creer `artifacts/temp/commodore-machine-images/{CbmII610,CbmII710,Pet4032,Pet8032,Pet8296,C232,V364}.jpg` et `Pet3032.png` : photos sources verifiees pour detourage; suppression au nettoyage.
    - [x] Creer `artifacts/temp/commodore-machine-images/CbmII510.jpg` : photo du P500 correspondant au CBM-II 510 pour detourage.
    - [x] Creer `src/GWGUI.Emulation.Commodore/Assets/Machines/{CbmII510,CbmII610,CbmII710,Pet3032,Pet4032,Pet8032,Pet8296,Plus4,SuperPet,Vic20,C232,V364}.png` : photos et rendu de reference avec fond transparent; halo du PET 8296 retire.
    - [x] Creer `src/GWGUI.Emulation.Commodore/Assets/Machines/{CbmII620,CbmII620Plus,CbmII720,CbmII720Plus,Pet3008,Pet3016,Pet4016,Pet8096,Vic21}.png` : reutiliser le visuel du meme boitier pour les variantes de memoire et extensions internes.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Constants/MachineCatalogConstants.cs` et `Dictionaries/MachineCatalog.cs` : relier les 21 autres PNG disponibles.
    - [x] Supprimer `src/GWGUI.Emulation.Commodore/Assets/Machines/Vic21.png` : doublon du VIC-20; modifier `Constants/MachineCatalogConstants.cs` et `Dictionaries/MachineCatalog.cs` pour utiliser directement `Vic20ImageResource` pour VIC-21.
    - [x] Supprimer `src/GWGUI.Emulation.Commodore/Assets/Machines/{Pet8096,Pet4016,Pet3016,Pet3008,CbmII720,CbmII720Plus}.png` : doublons signales par l utilisateur; modifier `Constants/MachineCatalogConstants.cs` et `Dictionaries/MachineCatalog.cs` pour referencer respectivement 8032, 4032, 3032 et 710.
    - [x] Supprimer `src/GWGUI.Emulation.Commodore/Assets/Machines/{CbmII620,CbmII620Plus}.png` : doublons du 610; modifier `Constants/MachineCatalogConstants.cs` et `Dictionaries/MachineCatalog.cs` pour utiliser `CbmII610ImageResource`.
  - [x] Verifier et nettoyer
    - [x] Modifier ce suivi : verifier les dimensions, la transparence, les 28 ressources images embarquees et le build Debug avec huit modules; supprimer les fichiers de travail temporaires dans `artifacts/temp/commodore-machine-images` apres usage.
  - [ ] Completer les deux variantes a clavier business apres identification fiable
    - [ ] Creer `src/GWGUI.Emulation.Commodore/Assets/Machines/Pet3032B.png` : photographie fiable de cette variante avec fond transparent.
    - [ ] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Constants/MachineCatalogConstants.cs` et `Dictionaries/MachineCatalog.cs` : relier les images des deux variantes business.
    - [ ] Modifier ce suivi : verifier les deux PNG et les 30 ressources embarquees pour les 39 machines apres build Debug avec huit modules.

### Validation des images

- 18 nouveaux PNG uniques; 28 ressources PNG au total dans le module; 37 machines disposent d une image.
- PNG ouverts et contours verifies; transparence presente, quatre coins a alpha zero pour chaque image.
- Neuf doublons supprimes a la demande de l utilisateur. Ressources partagees : VIC-21 vers VIC-20; PET 3008/3016 vers 3032; PET 4016 vers 4032; PET 8096 vers 8032; CBM-II 620/620+ vers 610 et 720/720+ vers 710.
- Ressources embarquees de la DLL publiee comparees aux 28 PNG : aucune difference, aucune reference manquante.
- Build final scripts/local-building.cmd --building=debug --modules=A : sortie 0, executable build/Debug/GW GUI/gwgui.exe et huit modules presents.
- Dossier artifacts/temp/commodore-machine-images supprime apres verification.
- Reste a completer : photo fiable de PET 3032B, a clavier business. PET 4032B utilise la miniature Pet4032.png sur demande explicite de l utilisateur du 7 octobre 2026.

### Sources des nouveaux visuels

Photos choisies par l utilisateur :

- C64 : https://upload.wikimedia.org/wikipedia/commons/9/9d/Commodore-64-Computer-FL.png
- C16 : https://upload.wikimedia.org/wikipedia/commons/a/af/Commodore_16_002a.png
- C64 DTV : https://www.getdigital.de/cdn/shop/files/productImage-173-c64-dtv-joystick-1_14895fad-b760-4d92-ae0f-b620067866c3.png?v=1721573450&width=400
- SuperCPU : https://s3.amazonaws.com/com.c64os.resources/weblog/woc19presentationsreview2_2/supercpu.gif (conversion de format GIF vers PNG)
- C128 : https://upload.wikimedia.org/wikipedia/commons/8/8d/Commodore-128.png
- PET 2001 : https://upload.wikimedia.org/wikipedia/commons/8/88/Commodore_2001_Series-IMG_0448b.png
- Plus/4 : https://storage.googleapis.com/rtc-collector-images/images/master/1765739574852_t0fi6v.webp (fond retire)
- SuperPET : https://piermarcobarbe.github.io/informatics_history_HCI_atelier_2015/html/hardware/images/superpet.png

Sources recherchees; fond retire sauf pour le PNG VIC-20 deja transparent :

- VIC-20 : https://upload.wikimedia.org/wikipedia/commons/b/bb/Commodore-VIC-20-FL.png
- CBM-II 510 / P500 : https://vintagecomputer.net/commodore/p500/P500_front-view.JPG
- CBM-II 610 : https://commons.wikimedia.org/wiki/File:Cbm610_ta.jpg
- CBM-II 710 : https://commons.wikimedia.org/wiki/File:Cbm710_ta.jpg
- PET 3032 : https://www.homecomputermuseum.nl/wp-content/uploads/2019/09/Pet-1200x1086.png
- PET 4032 : https://jimvideo.wordpress.com/wp-content/uploads/2012/09/pet4032.jpg
- PET 8032 : https://commons.wikimedia.org/wiki/File:Commodore_PET_8032.jpg (phreakindee, CC0)
- PET 8296 : https://www.fib.upc.edu/retro-informatica/exposicio/micrordinadors/com_8296.html?lang=es
- C232 : https://www.pagetable.com/docs/232-264/case_232.jpg
- V364 : https://cbmmuseum.kuto.de/images/home_364.jpg (rendu de reference)
