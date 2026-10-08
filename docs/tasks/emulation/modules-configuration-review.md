# Reprise de tous les modules

- [x] Reprendre Amstrad, Atari, Commodore, Microsoft, NEC, Nintendo, Sega et Sony : Common génériques, adaptateurs, réglages, ROM/BIOS et stockage
  - [x] Établir les responsabilités et les capacités existantes
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever pour Amstrad les fichiers de réglages, stockage, BIOS et options, leurs responsabilités et les corrections nécessaires après lecture de `src/GWGUI.Emulation.Amstrad`.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever pour Atari les fichiers de réglages, stockage, BIOS et options, leurs responsabilités et les corrections nécessaires après lecture de `src/GWGUI.Emulation.Atari`.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever pour Commodore les fichiers de réglages, stockage, BIOS et options, leurs responsabilités et les corrections nécessaires après lecture de `src/GWGUI.Emulation.Commodore`.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever pour Microsoft les fichiers de réglages, stockage, BIOS et options, leurs responsabilités et les corrections nécessaires après lecture de `src/GWGUI.Emulation.Microsoft`.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever pour Nec les fichiers de réglages, stockage, BIOS et options, leurs responsabilités et les corrections nécessaires après lecture de `src/GWGUI.Emulation.Nec`.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever pour Nintendo les fichiers de réglages, stockage, BIOS et options, leurs responsabilités et les corrections nécessaires après lecture de `src/GWGUI.Emulation.Nintendo`.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever pour Sega les fichiers de réglages, stockage, BIOS et options, leurs responsabilités et les corrections nécessaires après lecture de `src/GWGUI.Emulation.Sega`.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever pour Sony les fichiers de réglages, stockage, BIOS et options, leurs responsabilités et les corrections nécessaires après lecture de `src/GWGUI.Emulation.Sony`.
  - [x] Corriger les constats vérifiés
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : prendre Atari comme référence, sans le modifier, et comparer Commodore avant/après sa réorganisation.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : confirmer que les réglages Amiga CPU/RAM/ROM/vidéo/audio et les périphériques DF/DH/CD restent présents ; le backend retire déjà le bloc ROM spécialisé avant insertion des slots, sans doublon. Continuer la comparaison des interfaces HDD.
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/Machines/Common/Functions/SettingsFunctions.CrocoDS.cs` et `Emulators/CrocoDS/Constants/EmulatorConstants.cs` : placer le réglage de vitesse dans CPU, conserver les options vidéo dans Vidéo.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Contracts/CoreContracts.cs`, les `Emulators/*/Constants/OptionConstants.cs` et les constantes de portée associées : typer les onglets et les machines applicables aux options.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Emulators/Common/Interop/Functions/CoreSettingsFunctions.cs` et `Constants/CoreSettingsConstants.cs` : construire les options par onglet et machine, conserver les restrictions graphiques et les chemins BIOS existants.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Common/Machines/Common/Functions/SettingsFunctions.cs` et `Modules/NintendoEmulationModule.cs` : fusionner les champs dans les cadres existants et rendre visibles leurs onglets.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Nintendo/NintendoEmulatorCatalogTests.cs` : contrôler les cadres, onglets et portées des cœurs multisystèmes.
    - [x] Modifier `src/GWGUI.Emulation.Nintendo/Modules/NintendoEmulationModule.cs` : conserver la résolution de l’adaptateur dans UseFirmware après le raccordement des onglets.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever que Commodore expose les HDD Amiga sans dossier par défaut ; NEC expose les HDD PC-98 sans ce raccordement. Les autres systèmes sans HDD ne doivent pas recevoir ce champ.
    - [x] Modifier `src/GWGUI.Emulation.Commodore/Common/Machines/Common/Contracts/ConfigurationContracts.cs`, `Constants/SettingsPresentationConstants.cs`, `Functions/SettingsFunctions.cs`, `Common/Machines/AmigaComputers/Functions/StorageFunctions.cs` et `Modules/CommodoreEmulationModule.cs` : conserver un dossier HDD choisi, l’afficher lorsque le backend expose des HDD et le transmettre au dialogue HDD.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Emulators/Common/Interop/Contracts/CoreOptionDefinition.cs`, `Functions/CoreSettingsFunctions.cs`, `Emulators/BeetlePce/Constants/OptionConstants.cs` et créer `Emulators/BeetlePce/Constants/OptionMachineConstants.cs` : limiter les options CD aux machines CD et ranger les ROM System Card dans ROM.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Machines/Common/Contracts/ConfigurationContracts.cs`, `Constants/SettingsDescriptionTextConstants.cs`, `Functions/SettingsFunctions.cs`, `Functions/StorageFunctions.cs` et `Modules/NecEmulationModule.cs` : raccorder le dossier HDD aux PC-98 compatibles et conserver les options ROM en ajoutant les champs BIOS.
    - [x] Modifier les fonctions de stockage Commodore et NEC : utiliser le membre existant ImageDirectory du contrat SDK pour le dossier des images HDD.
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : relever Sony sans sélection BIOS malgré SwanStation/PCSX2, sans contrat de réglages ou formats par adaptateur ; Microsoft sans adaptateur installé, affichant une ROM intégrée non justifiée.
    - [x] Modifier `src/GWGUI.Emulation.Microsoft/Common/Machines/Common/Functions/SettingsFunctions.cs` et `Modules/MicrosoftEmulationModule.cs` : retirer la déclaration de ROM intégrée et masquer l’onglet ROM sans backend.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Interfaces/IEmulatorAdapter.cs`, créer `Common/Contracts/FirmwareSlot.cs`, modifier `Common/Machines/Common/Contracts/ConfigurationContracts.cs` et les trois fabriques Sony : exposer des slots BIOS, les formats et réglages par le contrat générique ; définir les besoins dans chaque backend.
    - [x] Créer `src/GWGUI.Emulation.Sony/Emulators/SwanStation/Constants/FirmwareConstants.cs` et `Emulators/Pcsx2/Constants/FirmwareConstants.cs` : définir les slots PS1 régionaux et les composants PS2, avec noms et chemins de destination propres au backend.
    - [x] Créer `src/GWGUI.Emulation.Sony/Common/Machines/Common/Functions/FirmwareConfigurationFunctions.cs`, modifier `Functions/SettingsFunctions.cs`, `Functions/StorageFunctions.cs`, `Functions/ConfigurationFunctions.cs` et `Modules/SonyEmulationModule.cs` : raccorder sélection, liste ROM, sauvegarde et validation, sans renommer les fichiers utilisateur ; déléguer réglages et formats au backend, définir dans l’interface et les fabriques le besoin de BIOS externe, et compléter les formats PPSSPP dans ses constantes.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Emulators/SwanStation/Services/ExternalCore.cs` et `Emulators/Pcsx2/Services/ExternalCore.cs` : copier les BIOS sélectionnés dans le dossier système de la session avant initialisation.
    - [x] Modifier `src/GWGUI.Emulation.Sony/Common/Dictionaries/EmulatorCatalog.cs` et `src/GWGUI.Emulation.Microsoft/Common/Dictionaries/EmulatorCatalog.cs` : utiliser les listes explicites des adaptateurs existants, sans réflexion.
    - [x] Modifier les fabriques Sony : préciser l’import des constantes BIOS du backend ; modifier `Common/Machines/Common/Functions/FirmwareConfigurationFunctions.cs` et `Resources/00-Base/Firmware.resx` pour utiliser les libellés BIOS invariants sans modifier le SDK ; définir la disposition dans `Common/Machines/Common/Constants/SettingsDescriptionTextConstants.cs`.
    - [x] Modifier les trois fabriques Sony : libérer le cœur et la sortie audio dans finally si la construction échoue avant transfert de propriété à Machine.
    - [x] Modifier les tests existants Nintendo et créer `tests/GWGUI.Tests/Emulation/ModuleConfigurationReviewTests.cs` : vérifier les onglets, les BIOS et les dossiers HDD pris en charge, sans DLL ni fichiers externes.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/ModuleConfigurationReviewTests.cs` : désambiguïser les alias des modules par rapport aux espaces de noms des tests.
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/Interfaces/IEmulatorAdapter.cs`, les fabriques Caprice32/CrocoDS et `Common/Machines/Common/Functions/SettingsFunctions.cs` ; déplacer les quatre fichiers `SettingsFunctions*.cs` de génération des réglages dans `Emulators/Common/Interop/Functions/CoreSettingsDescriptionFunctions*.cs` : le Common racine délègue les réglages au backend et conserve la signature utilisée par l’application.
    - [x] Modifier `src/GWGUI.Emulation.Amstrad/Common/Dictionaries/EmulatorCatalog.cs` : déclarer explicitement Caprice32 et CrocoDS.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Interfaces/IEmulatorAdapter.cs`, les fabriques NEC et `Common/Machines/Common/Functions/SettingsFunctions.cs` ; déplacer les trois générateurs `SettingsFunctions*.cs` vers `Emulators/Common/Interop/Functions/CoreSettingsDescriptionFunctions*.cs` : déléguer les blocs au backend, conserver le raccordement des machines et BIOS.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Common/Machines/Common/Functions/StorageFunctions.cs`, l’interface et les fabriques NEC ; déplacer sa génération vers `Emulators/Common/Interop/Functions/CoreStorageSettingsFunctions.cs` : déléguer description et application du stockage au backend au lieu de comparer les identifiants dans le Common racine.
    - [x] Supprimer `src/GWGUI.Emulation.Sony/Emulators/SwanStation/Constants/SwanStationOptionConstants.cs`, `Emulators/Pcsx2/Constants/Pcsx2OptionConstants.cs` et `Emulators/Ppsspp/Constants/PpssppOptionConstants.cs` : retirer les constantes CPC copiées et jamais utilisées ; retirer leurs commentaires trompeurs dans les fonctions ToNative correspondantes.
    - [x] Modifier les quatre fichiers NEC `Emulators/*/Functions/SettingsDescriptionFunctions.*.cs` : raccorder leurs imports statiques au générateur déplacé ; modifier `Common/Machines/PcEngineDuo/Functions/PcEngineDuoSettingsFunctions.cs` pour construire directement son information matérielle.
    - [x] Modifier `src/GWGUI.Emulation.Nec/Modules/NecEmulationModule.cs` : nommer les configurations locales NEC, sans référence copiée à Nintendo.
    - [x] Créer `src/GWGUI.Emulation.Sony/Common/Machines/Common/Enums/RamCapacity.cs` et modifier `Dictionaries/ModelCatalog.cs` : exprimer les capacités RAM en KiB avec des membres nommés par capacité, corriger la PSP de 32 KiB à 32 MiB.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/ModuleConfigurationReviewTests.cs` : vérifier l’unité de capacité RAM du catalogue Sony.
    - [x] Modifier `src/GWGUI.App/Resources/*/Emulation/EmulationHelp.resx` : ajouter avec Argos l’aide commune du dossier de recherche des images HDD dans toutes les langues.
    - [x] Modifier les constantes et fonctions de réglages Commodore et NEC : raccorder cette aide courte et détaillée au nouveau champ dossier HDD.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/EmulationArchitectureTests.cs` : remplacer les exigences de duplication Atari/Commodore par la vérification des contrats génériques ; autoriser uniquement le catalogue de composition à référencer les adaptateurs concrets, et les noms de machines dans leurs backends dédiés.
    - [x] Modifier `tests/GWGUI.Tests/Architecture/ConsoleFamilyModuleTests.cs` : vérifier les composants Common requis sans imposer des copies identiques, tenir compte de Cemu installé dans le catalogue, des clés Machine invariantes et des formats réellement pris en charge par les adaptateurs Sony.
    - [x] Modifier les deux tests d’architecture : conserver le catalogue Atari existant, vérifier le type de contexte déclaré dans EmulatorContracts plutôt qu’un nom de fichier supposé, et accepter les textes anglais de repli de la base Sony tout en vérifiant les traductions.
  - [x] Valider la revue
    - [x] Modifier `docs/tasks/emulation/modules-configuration-review.md` : inscrire les contrôles utiles et le résultat du build Debug avec les huit modules, après les corrections.

Atari sert de référence : aucun problème signalé, aucun changement à apporter. Comparer les autres modules à ses réglages et retrouver les fonctions Commodore perdues depuis la réorganisation.

## Constats de départ

Les sections suivantes consignent l’audit avant corrections.

### Amstrad

Réglages dédiés Caprice32/CrocoDS et stockage limité aux formats du cœur. CrocoDS : le réglage de vitesse crocods_hack est actuellement placé dans Vidéo avec les réglages du moniteur et du cadrage ; à placer dans CPU. Les BIOS intégrés sont présentés comme informations, sans liste de fichiers externe.

### Atari

Réglages distincts ST/8 bits/autres machines. SettingsFunctions.Machine.St.cs ajoute le dossier HDD par défaut dans Général. StorageFunctions.Settings.cs utilise séparément ce dossier comme DefaultDirectory des HDD ; les disques GEMDOS montés comme dossiers passent par StStorageFunctions. Le choix du dossier HDD fonctionne et doit rester intact. Les noms de glyphes existants ne font pas partie des corrections demandées ici.

### Commodore

Le Common délègue réglages, ROM et stockage aux adaptateurs. Amiga/CD32/CDTV et VICE/Frodo ont leurs modèles et formats distincts. Les ROM sont sélectionnées par slots et recopiées par le backend. VICE/Frodo utilisent les blocs de base ; les options supplémentaires non présentées ne doivent pas être inventées dans cette revue.

### Microsoft

Aucun adaptateur dans Emulators actuellement. Catalogue par réflexion inutile dans cet état. Les réglages de base affichent néanmoins System ROM comme firmware intégré, sans cœur installé : information non justifiée. Aucun nouvel émulateur demandé.

### Nec

Les options sont déjà typées par onglet. PCE, PCE Fast, SuperGrafx, PC-FX et Geargrafx ont leurs réglages propres. Vérifier la portée des options CD/Arcade Card et les sélecteurs de modèle des PC-88/PC-98, ainsi que leur raccordement au modèle configuré.

### Nintendo

55 catalogues de cœurs ; CoreSettingsFunctions place toutes les options dans un bloc Général, icône texte Cpu, deux colonnes et redémarrage systématique. Les cœurs multisystèmes Mesen2/Mesen-S/SkyEmu et les modèles GB/GBC/DS/DSi demandent une portée par machine et par valeur. Les blocs natifs sont ajoutés aux blocs communs au lieu de les intégrer.

### Sega

Reprise terminée : portée des options/valeurs par machine, onglets explicites, valeurs inapplicables neutralisées, blocs fusionnés et souris non proposée sur les machines sans souris. 101 contrôles réussis et Debug huit modules construit.

### Sony

Trois adaptateurs réels : SwanStation, PCSX2, PPSSPP. Le catalogue les recherche par réflexion. Le cadre Général affiche leur identifiant interne, et ROM indique firmware intégré sans décrire les besoins réels du cœur. Stockage à vérifier par cœur et type de support.


## Résultat de la reprise

- Les huit modules ont été examinés ; Atari reste la référence et aucun de ses fichiers source n’a changé.
- Les Common racine Amstrad et NEC délèguent les réglages et le stockage spécifique aux adaptateurs ; les générateurs natifs sont dans les backends. Les catalogues modifiés sont explicites.
- Nintendo et Sega : options réparties dans leurs onglets, filtrées selon les machines et fusionnées avec les cadres existants ; les valeurs inapplicables sont neutralisées.
- Commodore et NEC : dossier de recherche HDD sauvegardé et transmis aux dialogues compatibles via ImageDirectory, avec aide commune traduite dans les 29 cultures et texte anglais de repli.
- Sony : slots BIOS PS1/PS2 génériques, fichiers utilisateur conservés sous leur nom, copie vers les chemins attendus par le backend, formats distincts par adaptateur, libération du cœur et de l’audio après échec de construction. PPSSPP conserve l’absence de sélecteur BIOS externe. Capacités RAM exprimées en KiB et PSP corrigée à 32 MiB.
- Microsoft : aucune ROM intégrée annoncée sans backend ; aucun nouvel émulateur inventé.
- Tests de régression et d’architecture : 639 réussis. Le contrôle préexistant InputMappingsResolveSelectedDeviceKeysAndAxisThresholds attend un repli sur la première manette, contrairement au code committé d81ba32a5 qui renvoie une entrée vide si la manette choisie manque. Cette fonction et ce test sont inchangés et exclus du contrôle final ; l’exécution préalable l’a bien signalé en échec.
- Build `scripts/local-building.cmd --building=debug --modules=A` : code de sortie 0. Application et huit DLL présentes dans `build/Debug/GW GUI`.
- Les tests n’ont pas exécuté de jeux ni validé chaque cœur en situation réelle avec ses médias et BIOS.
