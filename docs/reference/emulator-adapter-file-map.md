# Carte des fichiers communs des modules d'émulation

`GWGUI.Emulation` communique uniquement avec les prises génériques exposées par le module familial.
Les dossiers `Emulators/<nom>/` traduisent les données propres à chaque cœur vers ces prises.

## Structure canonique générale

Les rôles équivalents emploient les mêmes noms dans chaque projet `GWGUI.Emulation.Xxxx`.
Un fichier supplémentaire est autorisé uniquement lorsqu'il décrit une machine, un format ou une
capacité sans équivalent dans l'autre famille.

| Catégorie | Fichiers généraux |
|---|---|
| `Constants` | `AudioConstants.cs`, `ConfigurationConstants.cs`, `ControllerConstants.cs`, `CoreConstants.cs`, `CoreManagementConstants.cs`, `EmulationModuleConstants.cs`, `FirmwareConstants.cs`, `InputConstants.cs`, `MachineConstants.cs`, `MediaConstants.cs`, `ModelConstants.cs`, `RuntimeConstants.cs`, `SettingsConstants.cs`, `SettingsTextConstants.cs`, `SettingsChoiceConstants.cs`, `StateConstants.cs`, `StorageConstants.cs`, `VideoConstants.cs` |
| `Contracts` | `AudioContracts.cs`, `ConfigurationContracts.cs`, `ControllerContracts.cs`, `CoreContracts.cs`, `EmulatorContracts.cs`, `FirmwareContracts.cs`, `InputContracts.cs`, `MachineContracts.cs`, `MediaContracts.cs`, `ModelContracts.cs`, `RuntimeContracts.cs`, `SettingsContracts.cs`, `StateContracts.cs`, `StorageContracts.cs`, `VideoContracts.cs` |
| `Dictionaries` | `ControllerCatalog.cs`, `EmulatorCatalog.cs`, `FirmwareCatalog.cs`, `MachineCatalog.cs`, `ModelCatalog.cs` |
| `Enums` | `ControllerEnums.cs`, `CoreEnums.cs`, `FirmwareEnums.cs`, `InputEnums.cs`, `MachineEnums.cs`, `MediaEnums.cs`, `RuntimeEnums.cs`, `SettingsEnums.cs`, `StateEnums.cs`, `StorageEnums.cs`, `VideoEnums.cs` |
| `Functions` | fichiers `AudioFunctions`, `ConfigurationFunctions`, `ControllerFunctions`, `CoreFunctions`, `FirmwareFunctions`, `InputFunctions`, `MachineFunctions`, `MediaFunctions`, `ModelFunctions`, `RuntimeFunctions`, `SettingsFunctions`, `StateFunctions`, `StorageFunctions` et `VideoFunctions`, avec des fichiers partiels lorsqu'un domaine dépasserait 200 lignes |
| `Interfaces` | `IEmulatorAdapter.cs`, `IEmulatorCore.cs`, puis une interface séparée uniquement pour une capacité réellement optionnelle |
| `Services` | `ConfigurationStore.cs`, `Engine.cs`, `Machine.cs`, puis un service séparé lorsqu'il possède son propre état, sa propre ressource ou son propre cycle de vie |

## Règles de rangement

- Un fichier général ne porte pas le nom Atari, Amiga, Amstrad ou celui d'un émulateur.
- Les constantes et petits types d'un même domaine sont regroupés sans dépasser inutilement 200 lignes.
- Les classes à état ou possédant des ressources restent séparées et sont découpées en fichiers partiels si nécessaire.
- Les éléments propres à une famille matérielle sont placés sous `Common/Machines/<famille>/`.

## Familles matérielles

- Atari utilise `Common/Machines/AtariClassic/` pour le catalogue matériel classique partagé,
  `Common/Machines/Atari8Bit/` pour les réglages propres aux ordinateurs 8 bits et
  `Common/Machines/AtariST/` pour les modèles ST, STE, TT et Falcon ainsi que TOS.
- Amiga utilise `Common/Machines/AmigaComputers/` pour les ordinateurs A500 à A4000,
  `Common/Machines/AmigaCDTV/` pour le CDTV et `Common/Machines/AmigaCD32/` pour le CD32.
- Les contrats réellement identiques entre plusieurs familles restent dans le `Common` général ; les
  constantes et catalogues propres à une famille restent dans son dossier.
