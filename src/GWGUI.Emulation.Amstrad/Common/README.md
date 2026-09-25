# Gestion commune Amstrad

Le module Amstrad doit utiliser directement les contrats génériques de `GWGUI.Emulation` :

- `IEmulationModule` pour le catalogue des machines et les configurations ;
- `IEmulationEmulatorManager` pour le choix, la recherche et l’installation des cœurs ;
- `IEmulatedMachine` et ses interfaces de capacités pour les entrées, médias, vidéo, audio, états et cycle de vie ;
- `EmulationEmulatorDefinition` et `EmulationEmulatorInstallation` pour les données communes.

La gestion commune du module doit exposer aux cœurs ses propres fichiers
`Common/Interfaces/IEmulatorAdapter.cs` et `Common/Contracts/EmulatorCreationContext.cs`. Ces fichiers
gardent exactement les mêmes noms, rôles et emplacements dans chaque projet `GWGUI.Emulation.Xxxx`.

Une interface propre à Amstrad ne doit être créée que pour une donnée matérielle qui ne peut pas être
représentée par ces contrats. Elle reste alors interne au module et ne remplace jamais un contrat générique.

## Fichiers communs à reproduire

Atari et Amiga possèdent actuellement les mêmes chemins relatifs suivants. Le
module Amstrad doit reprendre ces noms et ces rôles, puis adapter leur contenu à
ses machines :

- `Constants/AudioConstants.cs`, `ConfigurationConstants.cs`, `ControllerConstants.cs`, `CoreConstants.cs` et `CoreManagementConstants.cs` ;
- `Constants/EmulationModuleConstants.cs`, `FirmwareConstants.cs`, `InputConstants.cs`, `MachineConstants.cs`, `MediaConstants.cs` et `ModelConstants.cs` ;
- `Constants/RuntimeConstants.cs`, `SettingsConstants.cs`, `SettingsTextConstants.cs`, `SettingsChoiceConstants.cs`, `StateConstants.cs`, `StorageConstants.cs` et `VideoConstants.cs` ;
- `Contracts/ControllerBinding.cs`, `ControllerDevice.cs`, `CoreOption.cs` et `CoreOptionValue.cs` ;
- `Contracts/EmulatorCreationContext.cs` et `EmulatorManagementContext.cs` ;
- `Contracts/InputConfiguration.cs`, `MachineConfiguration.cs` et `SavedStateHeader.cs` ;
- `Dictionaries/EmulatorCatalog.cs`, `FirmwareCatalog.cs` et `ModelCatalog.cs` ;
- `Enums/Emulator.cs`, `HostCommand.cs` et `MediaCategory.cs` ;
- `Functions/ConfigurationSummaryFunctions.cs`, `EmulationMediaActivityFunctions.cs`, `EmulationMediaConversionFunctions.cs`, `HardDiskFormats.cs`, `InputSettingsFunctions.cs`, `InputSnapshotFunctions.cs`, `SettingsDescriptionFunctions.cs` et `StorageSettingsFunctions.cs` ;
- `Interfaces/IEmulatorAdapter.cs` et `IEmulatorCore.cs` ;
- `Services/ConfigurationStore.cs`, `Engine.cs` et `Machine.cs`.

Une catégorie ou une capacité inutile aux machines Amstrad peut rester absente,
mais un rôle équivalent ne doit pas recevoir un autre nom.

Les données propres à une famille matérielle Amstrad seront rangées sous
`Common/Machines/<famille>/`, avec leurs propres sous-dossiers `Constants`, `Contracts`,
`Dictionaries`, `Enums` et `Functions`. Le `Common` général conservera uniquement les prises et
données réellement partagées entre plusieurs familles.
