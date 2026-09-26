# Cartographie des fichiers des modules d’émulation

Ce document décrit l’état existant avant toute création du module Amstrad. Les chemins des colonnes
Amiga et Atari sont relatifs respectivement à `src/GWGUI.Emulation.Amiga` et
`src/GWGUI.Emulation.Atari`. Une cellule vide signifie qu’aucun fichier de l’autre module
n’assume directement la même responsabilité. La colonne Amstrad reste volontairement vide jusqu’à
la fin de l’étude des deux modules fonctionnels. Le rapprochement porte sur la responsabilité réelle,
pas seulement sur la ressemblance du nom.

## Racine du module

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `EmulationGlobalUsings.cs` | `EmulationGlobalUsings.cs` |  | Dans les deux modules, importe globalement les contrats, enums, interfaces et espaces de noms communs utilisés par le module. |
| `GWGUI.Emulation.Amiga.csproj` | `GWGUI.Emulation.Atari.csproj` |  | Dans les deux modules, configure la cible .NET/x64, les références vers le SDK et MediaEngine, la copie du manifeste et l’intégration des ressources. |
| `module.json` | `module.json` |  | Dans les deux modules, déclare l’identifiant, l’assembly d’entrée, la version du module, la plage d’API hôte et l’URL du catalogue. |
## Constants

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `Constants/AmigaAudioConfigurationConstants.cs` |  |  | Amiga : centralise pour Audio Configuration Constants les valeurs `Anti`, `Emulated`. |
| `Constants/AmigaConfigurationStoreConstants.cs` | `Constants/AtariConfigurationStoreConstants.cs` |  | Amiga : centralise pour Configuration Store Constants les valeurs `MachineJson`, `Json`, `N`, `Tmp`, `ReplacementRetryCount`, `ReplacementRetryDelayMilliseconds`, …. Atari : centralise pour Configuration Store Constants les valeurs `MachineFileName`, `JsonSearchPattern`, `TemporaryFileSuffix`, `LegacyFileExtension`, `MachineIdentifierFormat`, `ParentDirectoryName`, …. |
| `Constants/AmigaConfigurationSummaryFunctionsConstants.cs` | `Constants/AtariConfigurationSummaryFunctionsConstants.cs` |  | Amiga : centralise pour Configuration Summary Functions Constants les valeurs `OptionCpuModel`, `OptionVideoStandard`, `NTSC`, `NTSC2`, `PAL`, `OptionChipmemSize`, …. Atari : centralise pour Configuration Summary Functions Constants les valeurs `Value`, `D3D11`, `AudioOn`, `AudioOff`, `TOS`, `Value2`. |
| `Constants/AmigaConfigurationValidationFunctionsConstants.cs` |  |  | Amiga : centralise pour Configuration Validation Functions Constants les valeurs `AMIROMTYPE1`. |
| `Constants/AmigaCoreHostConstants.cs` | `Constants/AtariCoreHostConstants.cs` |  | Amiga : centralise pour Core Host Constants les valeurs `HostName`. Atari : centralise pour Core Host Constants les valeurs `CommandLineArgument`, `HostName`, `PipePrefix`, `VideoMapPrefix`, `LocalPipeServerName`, `UniqueNameFormat`, …. |
| `Constants/AmigaCoreHostValues.cs` | `Constants/AtariCoreHostValues.cs` |  | Amiga : centralise pour Core Host Values les valeurs `Windows`, `Value`, `TheAmigaHostConfigurationIsInvalid`, `TheAmigaHostIsNotInitialized`. Atari : centralise pour Core Host Values les valeurs `Windows`. |
| `Constants/AmigaCoreReleaseServiceConstants.cs` |  |  | Amiga : centralise pour Core Release Service Constants les valeurs `Validated96ebfcfc`, `Value96ebfcfc31072026GWGUI`, `HttpsBuildbotLibretroComNightlyWindowsX8664LatestPuaeLibretroDllZip`, `OptionLibretroDll`, `CoreJson`, `Unknown`, …. |
| `Constants/AmigaEmulationModuleConstants.cs` | `Constants/AtariEmulationModuleConstants.cs` |  | Amiga : centralise pour Emulation Module Constants les valeurs `Amiga`, `ResourceFamilyAmiga`, `AmigaCoreHost`, `OptionModel`, `OptionVideoStandard`, `PAL`, …. Atari : centralise pour Emulation Module Constants les valeurs `Atari`, `ResourceFamilyAtari`, `Enabled`, `N`. |
| `Constants/AmigaExternalCoreConstants.cs` |  |  | Amiga : centralise pour External Core Constants les valeurs `Hash0B8442C311CA`, `Kick31034A1000`, `Hash1FA1F93D3D7B`, `Kick32034A1000`, `Hash85AD74194E87`, `Kick33180A500`, …. |
| `Constants/AmigaExternalCoreInstallerConstants.cs` |  |  | Amiga : centralise pour External Core Installer Constants les valeurs `Value96ebfcfc`, `HttpsBuildbotLibretroComNightlyWindowsX8664LatestPuaeLibretroDllZip`, `OptionLibretroDll`, `Download`, `Extract`, `TheOfficialAmigaCoreArchiveDoesNotContainPuaeLibretroDll`, …. |
| `Constants/AmigaExternalDiskControlConstants.cs` |  |  | Amiga : centralise pour External Disk Control Constants les valeurs `TheAmigaMediaDriveCouldNotBeEjected`, `TheAmigaCoreCouldNotSelectTheRequestedDisk`, `TheAmigaMediaDriveCouldNotInsertTheRequestedImage`, `TheAmigaMediaImageOrDirectoryWasNotFound`, `TheAmigaCoreCouldNotCreateAMediaSlot`, `TheAmigaCoreRefusedTheMediaImage`, …. |
| `Constants/AmigaExternalHostCallbacksConstants.cs` |  |  | Amiga : centralise pour External Host Callbacks Constants les valeurs `UnknownAmigaCoreOption`, `OptionKickstart`, `Extended`, `JoypadDevice`, `MouseDevice`, `KeyboardDevice`, …. |
| `Constants/AmigaFirmwareCatalogConstants.cs` | `Constants/AtariFirmwareCatalogConstants.cs` |  | Amiga : centralise pour Firmware Catalog Constants les valeurs `Rom`, `Bin`, `Key`, `Hash4BB3954CA7DC`, `HashEBE0A06715B8`, `Hash04FAEED31162`, …. Atari : centralise pour Firmware Catalog Constants les valeurs `RevAPAL`, `RevANTSC`, `RevBNTSC`, `Atari400OsbPcXformerPatched`, `RevBNTSCPCXformerPatched`, `BB01R2`, …. |
| `Constants/AmigaFirmwareConstants.cs` | `Constants/AtariFirmwareConstants.cs` |  | Amiga : centralise pour Firmware Constants les valeurs `DirectoryName`. Atari : centralise pour Firmware Constants les valeurs `Md5HexLength`, `Atari800ExternalFirmwareCount`, `FileBufferSize`, `DuplicateMinimumCount`, `FirmwareDirectoryName`, `DirectoryName`, …. |
| `Constants/AmigaInputSettingsFunctionsConstants.cs` | `Constants/AtariInputSettingsFunctionsConstants.cs` |  | Amiga : centralise pour Input Settings Functions Constants les valeurs `OptionTurboFire`, `L2`, `Enabled`, `Disabled`, `OptionTurboFireButton`, `ResourceKeyHelp`, …. Atari : centralise pour Input Settings Functions Constants les valeurs `Left`, `ResourceMouseButtonLeft`, `MouseLeft`, `Right`, `ResourceMouseButtonRight`, `MouseRight`, …. |
| `Constants/AmigaInputSnapshotFunctionsConstants.cs` | `Constants/AtariInputSnapshotFunctionsConstants.cs` |  | Amiga : centralise pour Input Snapshot Functions Constants les valeurs `B`, `Y`, `Select`, `Start`, `Up`, `Down`, …. Atari : centralise pour Input Snapshot Functions Constants les valeurs `Fire1`, `Fire2`, `Turbo`, `Up`, `Down`, `Left`, …. |
| `Constants/AmigaMachineConfigurationConstants.cs` | `Constants/AtariMachineConfigurationConstants.cs` |  | Amiga : centralise pour Machine Configuration Constants les valeurs `Amiga`, `A500`, `OptionModel`, `OptionVideoStandard`, `PAL`, `OptionFloppyMultidrive`, …. Atari : centralise pour Machine Configuration Constants les valeurs `Atari`. |
| `Constants/AmigaMachineConstants.cs` | `Constants/AtariMachineConstants.cs` |  | Amiga : centralise pour Machine Constants les valeurs `TheAmigaStateDoesNotMatchTheRunningMachine`, `TheAmigaStateFirmwareMediaOrOptionsDoNotMatchTheRunningMachine`, `TheAmigaStateMediaListDoesNotMatchTheRunningMachine`, `TheAmigaMachineMustBeRunningBeforeChangingAFloppy`, `AmigaDiagnostics`, `TheAmigaMachineStopped`. Atari : centralise pour Machine Constants les valeurs `MinimumFramesPerSecond`, `MaximumFramesPerSecond`, `PauseWaitMilliseconds`, `DiagnosticTailCount`, `EmptyCount`, `NoRemainingTicks`, …. |
| `Constants/AmigaModelCatalogConstants.cs` | `Constants/AtariModelCatalogConstants.cs` |  | Amiga : centralise pour Model Catalog Constants les valeurs `A500OG`, `A1200OG`, `A2000OG`, `A4030`, `A4040`, `CD32FR`, …. Atari : centralise pour Model Catalog Constants les valeurs `ResourceAtariModelSt`, `ResourceAtariModelStf`, `ResourceAtariModelStfm`, `ResourceAtariModelMegaSt`, `ResourceAtariModelSte`, `ResourceAtariModelMegaSte`, …. |
| `Constants/AmigaProcessCoreConstants.cs` | `Constants/AtariProcessCoreConstants.cs` |  | Amiga : centralise pour Process Core Constants les valeurs `TheAmigaCoreProcessIsAlreadyInitialized`, `TheGWGUIExecutableUsedToHostTheAmigaCoreWasNotFound`, `AmigaCoreHost`, `TheAmigaCoreHostProcessCouldNotBeStarted`, `TheSharedAmigaVideoBufferIsUnavailable`, `TheAmigaCoreProcessIsNoLongerAvailable`, …. Atari : centralise pour Process Core Constants les valeurs `Windows`. |
| `Constants/AmigaRuntimeMediaFunctionsConstants.cs` |  |  | Amiga : centralise pour Runtime Media Functions Constants les valeurs `Scp`. |
| `Constants/AmigaSettingsConstants.cs` | `Constants/AtariSettingsConstants.cs` |  | Amiga : centralise pour Settings Constants les valeurs `KickstartPath`, `ExtendedRomPath`, `RomKeyPath`, `AudioEnabled`, `CpuOriginalSpeed`, `CpuSpeed`, …. Atari : centralise pour Settings Constants les valeurs `Cpu`, `CpuFrequency`, `CpuOriginalFrequency`, `CpuPrecision`, `Fpu`, `AlternateMemory`, …. |
| `Constants/AmigaSettingsDescriptionFunctionsConstants.cs` | `Constants/AtariSettingsDescriptionFunctionsConstants.cs` |  | Amiga : centralise pour Settings Description Functions Constants les valeurs `OptionCpuModel`, `OptionCpuCompatibility`, `Exact`, `OptionVideoStandard`, `PAL`, `NTSC`, …. Atari : centralise pour Settings Description Functions Constants les valeurs `Mouse`, `ResourceTabMouse`, `Value`, `ResourceMouseSpeed`, `DefaultFolders`, `ResourceFolderDefault`, …. |
| `Constants/AmigaStateStoreConstants.cs` | `Constants/AtariStateStoreConstants.cs` |  | Amiga : centralise pour State Store Constants les valeurs `Tmp`, `TheFileIsNotAGWGUIAmigaState`, `TheAmigaStateHeaderLengthIsInvalid`, `TheAmigaStateHeaderIsInvalid`, `TheAmigaStatePayloadIsCorrupted`, `TheAmigaMediaPathWasNotFound`, …. Atari : centralise pour State Store Constants les valeurs `AtariDirectoryName`, `QuickStateName`, `StateFileExtension`, `MetadataFileExtension`, `MetadataSearchPattern`, `CaptureFileExtension`, …. |
| `Constants/AmigaStorageSettingsFunctionsConstants.cs` | `Constants/AtariStorageSettingsFunctionsConstants.cs` |  | Amiga : centralise pour Storage Settings Functions Constants les valeurs `Adf`, `Adz`, `Dms`, `Fdi`, `Ipf`, `Scp`, …. Atari : centralise pour Storage Settings Functions Constants les valeurs `StorageDevice`, `StorageModel`, `StorageSpeed`, `StorageWriteProtected`, `StorageRedirectWrites`, `StorageInterface`, …. |
|  | `Constants/Atari800MediaConstants.cs` |  | Atari : centralise pour 800 Media Constants les valeurs `SystemOptionKey`, `Atari5200SystemValue`, `CartridgeHeaderText`, `CartridgeHeaderLength`, `MinimumCartridgeType`. |
|  | `Constants/Atari800MediaErrors.cs` |  | Atari : centralise pour 800 Media Errors les valeurs `UnsupportedMediaCategory`, `InvalidExtension`, `ComputerMediaOn5200`, `ConsoleMediaOnComputer`, `CartridgeTypeInvalid`, `DynamicCartridgeUnsupported`, …. |
|  | `Constants/Atari800MediaFunctionsConstants.cs` |  | Atari : centralise pour 800 Media Functions Constants les valeurs `Value589824`, `Value1114112`, `A52`, `Car`. |
|  | `Constants/AtariAudioConstants.cs` |  | Atari : centralise pour Audio Constants les valeurs `StereoChannelCount`, `LeftChannelIndex`, `RightChannelIndex`, `SingleFrameCount`, `BufferDurationDivisor`, `MinimumBufferedFrameCount`, …. |
|  | `Constants/AtariAudioOutputConstants.cs` |  | Atari : centralise pour Audio Output Constants les valeurs `MinimumVolume`, `MaximumVolume`, `DefaultVolume`, `FirstSampleIndex`, `MinimumSampleValue`, `MaximumSampleValue`. |
|  | `Constants/AtariCartridgeConstants.cs` |  | Atari : centralise pour Cartridge Constants les valeurs `StellaRegionOptionKey`, `JaguarRegionOptionKey`, `AutomaticRegionValue`, `NtscRegionValue`, `PalRegionValue`, `SecamRegionValue`, …. |
|  | `Constants/AtariCartridgeErrors.cs` |  | Atari : centralise pour Cartridge Errors les valeurs `UnsupportedCore`, `CartridgeRequired`, `ExtensionUnsupported`, `FileUnreadable`, `ReplacementFailed`, `RollbackFailed`, …. |
|  | `Common/Machines/<famille>/Constants/ModelConstants.cs` |  | Atari : chaque famille `Atari8Bit`, `Atari2600`, `Atari5200`, `Atari7800`, `AtariLynx`, `AtariJaguar` ou `AtariST` possède ses identifiants, ressources et caractéristiques sans catalogue « Classic » transversal. |
|  | `Constants/AtariCompatibilityConstants.cs` |  | Atari : centralise pour Compatibility Constants les valeurs `NoControllerPort`, `OneControllerPort`, `TwoControllerPorts`, `FourControllerPorts`, `EmptyCollectionCount`, `SingleChoiceCount`, …. |
|  | `Constants/AtariConfigurationMigrationConstants.cs` |  | Atari : centralise pour Configuration Migration Constants les valeurs `SchemaVersionPropertyName`. |
|  | `Constants/AtariConfigurationOptionConstants.cs` |  | Atari : centralise pour Configuration Option Constants les valeurs `VideoStandard`, `VideoResolution`, `MainMemory`, `AudioOutput`, `AudioLatency`, `AudioVolume`, …. |
|  | `Constants/AtariConstants.cs` |  | Atari : centralise pour Constants les valeurs `CurrentConfigurationSchemaVersion`, `MaximumControllerPortCount`, `MinimumControllerPort`, `ExternalCoreApiVersion`, `MaximumStateSize`, `MessageInterfaceVersion`, …. |
|  | `Constants/AtariContentConstants.cs` |  | Atari : centralise pour Content Constants les valeurs `ExtensionSeparator`. |
|  | `Constants/AtariControllerConstants.cs` |  | Atari : centralise pour Controller Constants les valeurs `MinimumDeadZonePercent`, `MaximumDeadZonePercent`, `DefaultDeadZonePercent`, `MaximumAxisMagnitude`, `PercentageDivisor`, `NeutralAxis`, …. |
|  | `Constants/AtariControllerPortFunctionsConstants.cs` |  | Atari : centralise pour Controller Port Functions Constants les valeurs `Automatic`, `BoosterGrip`, `Genesis`, `Joy2B`. |
|  | `Constants/AtariCoreCatalogConstants.cs` |  | Atari : centralise pour Core Catalog Constants les valeurs `HatariId`, `Atari800Id`, `StellaId`, `ProSystemId`, `BeetleLynxId`, `VirtualJaguarId`, …. |
|  | `Constants/AtariCoreCatalogErrors.cs` |  | Atari : centralise pour Core Catalog Errors les valeurs `EmptyInstallationRoot`, `EmptyVersion`, `DuplicateCore`, `DuplicateModel`, `MissingModel`. |
|  | `Constants/AtariCoreHostErrors.cs` |  | Atari : centralise pour Core Host Errors les valeurs `InvalidConfiguration`, `NotInitialized`, `AlreadyInitialized`, `ExecutableMissing`, `ProcessStartFailed`, `ProcessUnavailable`, …. |
|  | `Constants/AtariCoreHostFunctionsConstants.cs` |  | Atari : centralise pour Core Host Functions Constants les valeurs `Windows`. |
|  | `Constants/AtariCoreIdentityConstants.cs` |  | Atari : centralise pour Core Identity Constants les valeurs `Hatari`, `Atari800`, `Stella`, `ProSystem`, `BeetleLynx`, `VirtualJaguar`. |
|  | `Constants/AtariCoreLifecycleConstants.cs` |  | Atari : centralise pour Core Lifecycle Constants les valeurs `NoDevice`, `DefaultJoypadDevice`, `JoypadDeviceName`, `KeyboardDeviceName`, `MouseDeviceName`, `JoystickDeviceName`, …. |
|  | `Constants/AtariCoreOptionConstants.cs` |  | Atari : centralise pour Core Option Constants les valeurs `SupportedInterfaceVersion`, `MaximumDefinitions`, `MaximumCategories`, `MaximumValues`, `LegacyDefinitionPointerCount`, `VersionTwoDefinitionPointerCountBeforeValues`, …. |
|  | `Constants/AtariCoreOptionProbeConstants.cs` |  | Atari : centralise pour Core Option Probe Constants les valeurs `CommandLineArgument`, `SuccessExitCode`, `FailureExitCode`, `ProcessTimeoutMilliseconds`. |
|  | `Constants/AtariCoreOptionProbeValues.cs` |  | Atari : centralise pour Core Option Probe Values les valeurs `GWGUIAtariOptionProbe`, `N`. |
|  | `Constants/AtariCoreReleaseConstants.cs` |  | Atari : centralise pour Core Release Constants les valeurs `ReleaseIdPrefix`, `ReleaseVersionFormat`, `TemporaryDownloadExtension`, `TemporaryExtractExtension`, `TemporaryManifestExtension`, `UnknownDiagnosticValue`, …. |
|  | `Constants/AtariCoreReleaseErrors.cs` |  | Atari : centralise pour Core Release Errors les valeurs `MissingPublishedDate`, `MissingExpectedLibraryFormat`, `InvalidExportDirectory`, `InstalledLibraryLockedFormat`. |
|  | `Constants/AtariDiskControlConstants.cs` |  | Atari : centralise pour Disk Control Constants les valeurs `InterfaceVersion`, `TextBufferSize`, `NoImageIndex`, `FirstImageIndex`, `FirstNativeImageIndex`, `NoNativeImageIndex`, …. |
|  | `Constants/AtariDiskControlErrors.cs` |  | Atari : centralise pour Disk Control Errors les valeurs `Unavailable`, `Incomplete`, `EjectFailed`, `SelectFailed`, `InsertFailed`, `CreateSlotFailed`, …. |
|  | `Constants/AtariEightBitSettingsCatalogConstants.cs` |  | Atari : centralise pour Eight Bit Settings Catalog Constants les valeurs `Default`, `Gray`, `Jakub`, `Real`, `Xformer`, `Value336x240`, …. |
|  | `Constants/AtariEightBitSettingsConstants.cs` |  | Atari : centralise pour Eight Bit Settings Constants les valeurs `VideoStandardOptionKey`, `ResolutionOptionKey`, `PokeyStereoOptionKey`, `ArtifactingModeOptionKey`, `ColorHueOptionKey`, `ColorSaturationOptionKey`, …. |
|  | `Constants/AtariEightBitSettingsFunctionsConstants.cs` |  | Atari : centralise pour Eight Bit Settings Functions Constants les valeurs `Auto`. |
|  | `Constants/AtariEngineConstants.cs` |  | Atari : centralise pour Engine Constants les valeurs `IdentifierFormat`. |
|  | `Constants/AtariEnvironmentConstants.cs` |  | Atari : centralise pour Environment Constants les valeurs `MaximumInputDescriptorCount`, `MaximumControllerPortCount`, `MaximumControllerTypeCount`, `MaximumMemoryDescriptorCount`, `NextCharacterOffset`, `FirstRotation`, …. |
|  | `Constants/AtariEnvironmentFunctionsConstants.cs` |  | Atari : centralise pour Environment Functions Constants les valeurs `Ja`, `Fr`, `Es`, `De`, `It`, `Nl`, …. |
|  | `Constants/AtariErrorMessages.cs` |  | Atari : centralise pour Error Messages les valeurs `UnknownFirmware`, `RequiredFirmwareMissing`, `FirmwareFileMissing`, `FirmwareFileUnreadable`, `FirmwareIdentityAmbiguous`, `FirmwareCannotBeSelected`, …. |
|  | `Constants/AtariFirmwareRuntimeConstants.cs` |  | Atari : centralise pour Firmware Runtime Constants les valeurs `SingleDefinitionCount`, `FirstDefinitionIndex`. |
|  | `Constants/AtariFirmwareScanFunctionsConstants.cs` |  | Atari : centralise pour Firmware Scan Functions Constants les valeurs `EmuTOS`, `KAOSTOS`. |
|  | `Constants/AtariHardwareSettingsConstants.cs` |  | Atari : centralise pour Hardware Settings Constants les valeurs `CompatibleResource`, `CycleExactResource`, `NoneResource`, `MultilingualResource`, `RegionFreeResource`, `FrequencyMhzSuffix`, …. |
|  | `Constants/AtariHardwareSettingsFunctionsConstants.cs` |  | Atari : centralise pour Hardware Settings Functions Constants les valeurs `EnUS`, `DeDE`, `FrFR`, `EnGB`, `EsES`, `ItIT`, …. |
|  | `Constants/AtariHatariContentConstants.cs` |  | Atari : centralise pour Hatari Content Constants les valeurs `FirstContentIndex`, `MaximumPrimaryContentCount`. |
|  | `Constants/AtariHatariContentErrors.cs` |  | Atari : centralise pour Hatari Content Errors les valeurs `MultiplePrimaryContentUnsupported`, `ContentTypeUnsupported`. |
|  | `Constants/AtariHatariStorageConstants.cs` |  | Atari : centralise pour Hatari Storage Constants les valeurs `AcsiExtension`, `IdeExtension`, `GemdosMarkerExtension`, `HardDriveWriteProtectionOption`, `WriteProtectionEnabled`, `WriteProtectionDisabled`, …. |
|  | `Constants/AtariHatariStorageErrors.cs` |  | Atari : centralise pour Hatari Storage Errors les valeurs `StorageMissing`, `StorageTypeInvalid`, `StorageExtensionInvalid`, `GemdosRequiresDirectory`, `StorageNotSupportedByModel`, `MultiplePrimaryStorageUnsupported`, …. |
|  | `Constants/AtariInputConstants.cs` |  | Atari : centralise pour Input Constants les valeurs `JoypadDevice`, `AnalogDevice`, `MouseDevice`, `KeyboardDevice`, `PrimaryPort`, `MouseXId`, …. |
|  | `Constants/AtariInputSettingsConstants.cs` |  | Atari : centralise pour Input Settings Constants les valeurs techniques déclarées dans ce fichier. |
|  | `Constants/AtariJaguarCdConstants.cs` |  | Atari : centralise pour Jaguar Cd Constants les valeurs `CueExtension`, `CueFileDirective`, `CueQuotedPathDelimiter`, `MissingCueDelimiterIndex`, `CueContentStartOffset`, `RequiresFullPath`. |
|  | `Constants/AtariJaguarCdErrors.cs` |  | Atari : centralise pour Jaguar Cd Errors les valeurs `ModelRequired`, `CompleteDiscRequired`, `MissingCueTrack`, `EmptyCue`, `FileUnreadable`, `EjectionUnsupported`. |
|  | `Constants/AtariKeyboardConstants.cs` |  | Atari : centralise pour Keyboard Constants les valeurs `Backspace`, `Tab`, `Return`, `Escape`, `Space`, `FirstPrintableCharacter`, …. |
|  | `Constants/AtariMachineOptionConstants.cs` |  | Atari : centralise pour Machine Option Constants les valeurs `MachineType`, `RamSize`, `CpuFrequency`, `HighResolution`, `RefreshRate`, `CropOverscan`, …. |
|  | `Constants/AtariMachineOptionFunctionsConstants.cs` |  | Atari : centralise pour Machine Option Functions Constants les valeurs `False`, `True`, `Value1`, `Value0`, `Monochrome`, `Enabled`, …. |
|  | `Constants/AtariMachineValues.cs` |  | Atari : centralise pour Machine Values les valeurs `Value0`, `Value1`, `HatariResetType`. |
|  | `Constants/AtariMediaConstants.cs` |  | Atari : centralise pour Media Constants les valeurs `DefaultMountOrder`. |
|  | `Constants/AtariMouseSettingsConstants.cs` |  | Atari : centralise pour Mouse Settings Constants les valeurs `SpeedOptionKey`, `MappingOptionPrefix`, `DefaultSpeedPercent`, `MinimumSpeedPercent`, `MaximumSpeedPercent`, `SpeedStepPercent`. |
|  | `Constants/AtariRuntimeConstants.cs` |  | Atari : centralise pour Runtime Constants les valeurs `NativeNtscRegion`, `NativePalRegion`, `MissingRegionValue`. |
|  | `Constants/AtariScpMediaFunctionsConstants.cs` |  | Atari : centralise pour Scp Media Functions Constants les valeurs `AnAtariSCPImageMustBeMountedAsFloppyMedia`. |
|  | `Constants/AtariSessionMediaConstants.cs` |  | Atari : centralise pour Session Media Constants les valeurs `SessionDirectoryName`, `PlaylistExtension`, `PlaylistCommentPrefix`, `RuntimeFileNameFormat`, `RuntimePlaylistFileName`, `SessionInstanceNameFormat`, …. |
|  | `Constants/AtariSessionMediaErrors.cs` |  | Atari : centralise pour Session Media Errors les valeurs `PlaylistEntryMissing`, `PlaylistEmpty`, `ExplicitSaveRequired`. |
|  | `Constants/AtariShortcutConstants.cs` |  | Atari : centralise pour Shortcut Constants les valeurs `MinimumMediaForSelection`. |
|  | `Constants/AtariStateConstants.cs` |  | Atari : centralise pour State Constants les valeurs `MagicText`, `TemporaryFileSuffix`, `CurrentFormatVersion`, `HeaderLengthSize`, `MaximumHeaderLength`, `HashBufferSize`, …. |
|  | `Constants/AtariStModelConstants.cs` |  | Atari : centralise pour St Model Constants les valeurs `StMachineId`, `SteMachineId`, `TtMachineId`, `FalconMachineId`, `StDisplayNameResource`, `StfDisplayNameResource`, …. |
|  | `Constants/AtariTosHeaderReaderConstants.cs` |  | Atari : centralise pour Tos Header Reader Constants les valeurs `EmuTOS`, `KAOS`, `KAOSTOS`, `Value09016Version090913`, `Version`, `Value09Version009090209`. |
|  | `Constants/AtariVideoAudioSettingsConstants.cs` |  | Atari : centralise pour Video Audio Settings Constants les valeurs `StandardOption`, `ResolutionOption`, `AspectRatioOption`, `CropOption`, `FrameSkipOption`, `AudioOutputOption`, …. |
|  | `Constants/AtariVideoConstants.cs` |  | Atari : centralise pour Video Constants les valeurs `BufferCount`, `FirstBuffer`, `FirstRow`, `NextBufferStep`. |
| `Constants/PuaeMachineFactoryConstants.cs` |  |  | Amiga : centralise pour Puae Machine Factory Constants les valeurs `N`. |
## Contracts

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `Contracts/AmigaAudioConfiguration.cs` |  |  | Amiga : définit `AmigaAudioConfiguration` avec `AmigaAudioConfiguration` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaControllerBinding.cs` | `Contracts/AtariControllerBinding.cs` |  | Amiga : définit `AmigaControllerBinding` avec `AmigaControllerBinding` pour transporter ces données sans comportement de service. Atari : définit `AtariControllerBinding` avec `AtariControllerBinding` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaControllerDevice.cs` | `Contracts/AtariControllerDevice.cs` |  | Amiga : définit `AmigaControllerDevice` avec `AmigaControllerDevice` pour transporter ces données sans comportement de service. Atari : définit `AtariControllerDevice` avec `AtariControllerDevice` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaCoreOption.cs` | `Contracts/AtariCoreOption.cs` |  | Amiga : définit `AmigaCoreOption` avec `AmigaCoreOption` pour transporter ces données sans comportement de service. Atari : définit `AtariCoreOption` avec `AtariCoreOption` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaCoreOptionValue.cs` | `Contracts/AtariCoreOptionValue.cs` |  | Amiga : définit `AmigaCoreOptionValue` avec `AmigaCoreOptionValue` pour transporter ces données sans comportement de service. Atari : définit `AtariCoreOptionValue` avec `AtariCoreOptionValue` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaCoreRelease.cs` | `Contracts/AtariCoreRelease.cs` |  | Amiga : définit `AmigaCoreRelease` avec `AmigaCoreRelease`, `ToString` pour transporter ces données sans comportement de service. Atari : définit `AtariCoreRelease` avec `AtariCoreRelease` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaFirmware.cs` |  |  | Amiga : définit `AmigaFirmware` avec `AmigaFirmware` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaFloppyConfiguration.cs` |  |  | Amiga : définit `AmigaFloppyConfiguration` avec `AmigaFloppyConfiguration` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaInputConfiguration.cs` | `Contracts/AtariInputConfiguration.cs` |  | Amiga : définit `AmigaInputConfiguration` avec `AmigaInputConfiguration` pour transporter ces données sans comportement de service. Atari : définit `AtariInputConfiguration` avec `AtariInputConfiguration` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaMachineConfiguration.cs` | `Contracts/AtariMachineConfiguration.cs` |  | Amiga : définit `AmigaMachineConfiguration` avec `AmigaMachineConfiguration`, `A500`, `EnsureId` pour transporter ces données sans comportement de service. Atari : définit `AtariMachineConfiguration` avec `AtariMachineConfiguration`, `SchemaVersion`, `Id`, `Model`, `Family`, `Core`, … pour transporter ces données sans comportement de service. |
| `Contracts/AmigaMachineCreationContext.cs` | `Contracts/AtariMachineCreationContext.cs` |  | Amiga : définit `AmigaMachineCreationContext` avec `AmigaMachineCreationContext` pour transporter ces données sans comportement de service. Atari : définit `AtariMachineCreationContext` avec `AtariMachineCreationContext` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaMediaConfiguration.cs` |  |  | Amiga : définit `AmigaMediaConfiguration` avec `AmigaMediaConfiguration` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaModel.cs` |  |  | Amiga : définit `AmigaModel` avec `AmigaModel` pour transporter ces données sans comportement de service. |
| `Contracts/AmigaSavedStateHeader.cs` | `Contracts/AtariSavedStateHeader.cs` |  | Amiga : définit `AmigaSavedStateHeader` avec `AmigaSavedStateHeader` pour transporter ces données sans comportement de service. Atari : définit `AtariSavedStateHeader` avec `AtariSavedStateHeader` pour transporter ces données sans comportement de service. |
|  | `Contracts/Atari800PreparedMedia.cs` |  | Atari : définit `Atari800PreparedMedia` avec `Atari800PreparedMedia` pour transporter ces données sans comportement de service. |
|  | `Common/Machines/Common/Contracts/HardwareModelContracts.cs` |  | Atari : définit les contrats partagés `HardwareModelDefinition` et `HardwarePortDefinition` produits directement par les catalogues des familles non-ST. |
|  | `Contracts/AtariCompatibilityDefinition.cs` |  | Atari : définit `AtariCompatibilityDefinition` avec `AtariCompatibilityDefinition` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariConfigurationDocument.cs` |  | Atari : définit `AtariConfigurationDocument` avec `AtariConfigurationDocument` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariControllerPort.cs` |  | Atari : définit `AtariControllerPort` avec `AtariControllerPort` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariCoreActiveInstallation.cs` |  | Atari : définit `AtariCoreActiveInstallation` avec `AtariCoreActiveInstallation` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariCoreCatalogEntry.cs` |  | Atari : définit `AtariCoreCatalogEntry` avec `AtariCoreCatalogEntry` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariCoreDiagnosticManifest.cs` |  | Atari : définit `AtariCoreDiagnosticManifest` avec `AtariCoreDiagnosticManifest` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariCoreInstallationPaths.cs` |  | Atari : définit `AtariCoreInstallationPaths` avec `AtariCoreInstallationPaths` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariCoreInstallProgress.cs` |  | Atari : définit `AtariCoreInstallProgress` avec `AtariCoreInstallProgress` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariCoreOptionCategory.cs` |  | Atari : définit `AtariCoreOptionCategory` avec `AtariCoreOptionCategory` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariDiskImageStatus.cs` |  | Atari : définit `AtariDiskImageStatus` avec `AtariDiskImageStatus` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariDiskStatus.cs` |  | Atari : définit `AtariDiskStatus` avec `AtariDiskStatus` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariEightBitNativeSetting.cs` |  | Atari : définit `AtariEightBitNativeSetting` avec `AtariEightBitNativeSetting` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariEnvironmentExtendedMessage.cs` |  | Atari : définit `AtariEnvironmentExtendedMessage` avec `AtariEnvironmentExtendedMessage` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariEnvironmentMessage.cs` |  | Atari : définit `AtariEnvironmentMessage` avec `AtariEnvironmentMessage` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariExternalCoreExports.cs` |  | Atari : définit `AtariExternalCoreExports` avec `AtariExternalCoreExports` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariExternalCoreInfo.cs` |  | Atari : définit `AtariExternalCoreInfo` avec `AtariExternalCoreInfo` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariFirmwareConfiguration.cs` |  | Atari : définit `AtariFirmwareConfiguration` avec `AtariFirmwareConfiguration` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariFirmwareDefinition.cs` |  | Atari : définit `AtariFirmwareDefinition` avec `AtariFirmwareDefinition` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariFirmwareFingerprint.cs` |  | Atari : définit `AtariFirmwareFingerprint` avec `AtariFirmwareFingerprint` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariFolderConfiguration.cs` |  | Atari : définit `AtariFolderConfiguration` avec `AtariFolderConfiguration` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariHardwareChoice.cs` |  | Atari : définit `AtariHardwareChoice` avec `AtariHardwareChoice` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariHardwareField.cs` |  | Atari : définit `AtariHardwareField` avec `AtariHardwareField` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariHardwareView.cs` |  | Atari : définit `AtariHardwareView` avec `AtariHardwareView` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariHatariContent.cs` |  | Atari : définit `AtariHatariContent` avec `AtariHatariContent` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariHatariStorageVolume.cs` |  | Atari : définit `AtariHatariStorageVolume` avec `AtariHatariStorageVolume` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariHostError.cs` |  | Atari : définit `AtariHostError` avec `AtariHostError` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariInputDescriptor.cs` |  | Atari : définit `AtariInputDescriptor` avec `AtariInputDescriptor` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariMachineCommand.cs` |  | Atari : définit `AtariMachineCommand` avec `AtariMachineCommand`, `Execute` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariMedia.cs` |  | Atari : définit `AtariMediaConfiguration` avec `AtariMediaConfiguration` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariMediaCompatibilityRule.cs` |  | Atari : définit `AtariMediaCompatibilityRule` avec `AtariMediaCompatibilityRule` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariMemoryDescriptor.cs` |  | Atari : définit `AtariMemoryDescriptor` avec `AtariMemoryDescriptor` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariMemoryExpansionChoice.cs` |  | Atari : définit `AtariMemoryExpansionChoice` avec `AtariMemoryExpansionChoice` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariOptionRule.cs` |  | Atari : définit `AtariOptionRule` avec `AtariOptionRule` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariPreparedCartridge.cs` |  | Atari : définit `AtariPreparedCartridge` avec `AtariPreparedCartridge` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariPreparedJaguarCd.cs` |  | Atari : définit `AtariPreparedJaguarCd` avec `AtariPreparedJaguarCd` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariRuntimeGeometry.cs` |  | Atari : définit `AtariRuntimeGeometry` avec `AtariRuntimeGeometry` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariRuntimeStatus.cs` |  | Atari : définit `AtariRuntimeStatus` avec `AtariRuntimeStatus` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariScannedFirmware.cs` |  | Atari : définit `AtariScannedFirmware` avec `AtariScannedFirmware` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariSessionMedia.cs` |  | Atari : définit `AtariSessionMedia` avec `AtariSessionMedia` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariShortcutExecutionContext.cs` |  | Atari : définit `AtariShortcutExecutionContext` avec `AtariShortcutExecutionContext` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariShortcutRule.cs` |  | Atari : définit `AtariShortcutRule` avec `AtariShortcutRule` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariStateConfigurationFingerprint.cs` |  | Atari : définit `AtariStateConfigurationFingerprint` avec `AtariStateConfigurationFingerprint` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariStateContentEntry.cs` |  | Atari : définit `AtariStateContentEntry` avec `AtariStateContentEntry` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariStateControllerFingerprint.cs` |  | Atari : définit `AtariStateControllerFingerprint` avec `AtariStateControllerFingerprint` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariStateFile.cs` |  | Atari : définit `AtariStateFile` avec `AtariStateFile` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariStateInputFingerprint.cs` |  | Atari : définit `AtariStateInputFingerprint` avec `AtariStateInputFingerprint` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariStModelDefinition.cs` |  | Atari : définit `AtariStModelDefinition` avec `AtariStModelDefinition` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariStoredStateMetadata.cs` |  | Atari : définit `AtariStoredStateMetadata` avec `AtariStoredStateMetadata` pour transporter ces données sans comportement de service. |
|  | `Contracts/AtariTosHeader.cs` |  | Atari : définit `AtariTosHeader` avec `AtariTosHeader` pour transporter ces données sans comportement de service. |
## Dictionaries

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `Dictionaries/AmigaControllerCatalog.cs` |  |  | Amiga : construit le catalogue `AmigaControllerCatalog` et expose `AmigaControllerCatalog`, `Types`, `Default`, `Normalize`, `ParallelPortTypes`. |
| `Dictionaries/AmigaFirmwareCatalog.cs` | `Dictionaries/AtariFirmwareCatalog.cs` |  | Amiga : construit le catalogue `AmigaFirmwareCatalog` et expose `AmigaFirmwareCatalog`, `Scan`, `Inspect`. Atari : construit le catalogue `AtariFirmwareCatalog` et expose `AtariFirmwareCatalog`, `Get`, `ForModel`. |
| `Dictionaries/AmigaMachineCatalog.cs` |  |  | Amiga : construit le catalogue `AmigaMachineCatalog` et expose `AmigaMachineCatalog`, `All`. |
| `Dictionaries/AmigaModelCatalog.cs` | `Dictionaries/AtariModelCatalog.cs` |  | Amiga : construit le catalogue `AmigaModelCatalog` et expose `AmigaModelCatalog`, `All`, `Get`, `FromLegacyId`, `BackendModelFor`. Atari : construit le catalogue `AtariModelCatalog` et expose `AtariModelCatalog`, `All`, `Parse`. |
|  | `Common/Machines/Common/Dictionaries/HardwareModelCatalog.cs` |  | Atari : agrège les catalogues des six familles non-ST et expose `All` et `Get` sur le contrat matériel commun. |
|  | `Dictionaries/AtariCompatibilityCatalog.cs` |  | Atari : construit le catalogue `AtariCompatibilityCatalog` et expose `AtariCompatibilityCatalog`, `Get`. |
|  | `Dictionaries/AtariCoreCatalog.cs` |  | Atari : construit le catalogue `AtariCoreCatalog` et expose `AtariCoreCatalog`, `Get`, `GetInstallationPaths`, `GetActiveManifestPath`. |
|  | `Dictionaries/AtariEightBitSettingsCatalog.cs` |  | Atari : construit le catalogue `AtariEightBitSettingsCatalog` et expose `AtariEightBitSettingsCatalog`, `SupportsOriginalComputerOptions`, `SupportsComputerOptions`, `SupportsMapRam`, `Mosaic`, `Axlon`, …. |
|  | `Dictionaries/AtariStModelCatalog.cs` |  | Atari : construit le catalogue `AtariStModelCatalog` et expose `AtariStModelCatalog`, `Get`. |
## Enums

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `Enums/AmigaControllerType.cs` |  |  | Amiga : définit l’ensemble fermé `AmigaControllerType` utilisé par les contrats et services du module. |
| `Enums/AmigaEmulator.cs` | `Enums/AtariEmulator.cs` |  | Amiga : définit l’ensemble fermé `AmigaEmulator` utilisé par les contrats et services du module. Atari : définit l’ensemble fermé `AtariEmulator` utilisé par les contrats et services du module. |
| `Enums/AmigaFirmwareType.cs` |  |  | Amiga : définit l’ensemble fermé `AmigaFirmwareType` utilisé par les contrats et services du module. |
| `Enums/AmigaHostCommand.cs` | `Enums/AtariHostCommand.cs` |  | Amiga : définit l’ensemble fermé `AmigaHostCommand` utilisé par les contrats et services du module. Atari : définit l’ensemble fermé `AtariHostCommand` utilisé par les contrats et services du module. |
| `Enums/AmigaMediaCategory.cs` | `Enums/AtariMediaCategory.cs` |  | Amiga : définit l’ensemble fermé `AmigaMediaCategory` utilisé par les contrats et services du module. Atari : définit l’ensemble fermé `AtariMediaCategory` utilisé par les contrats et services du module. |
| `Enums/AmigaMouseAction.cs` |  |  | Amiga : définit l’ensemble fermé `AmigaMouseAction` utilisé par les contrats et services du module. |
|  | `Enums/Atari800ContentType.cs` |  | Atari : définit l’ensemble fermé `Atari800ContentType` utilisé par les contrats et services du module. |
|  | `Enums/AtariCartridgePlatform.cs` |  | Atari : définit l’ensemble fermé `AtariCartridgePlatform` utilisé par les contrats et services du module. |
|  | `Enums/AtariCartridgeRegion.cs` |  | Atari : définit l’ensemble fermé `AtariCartridgeRegion` utilisé par les contrats et services du module. |
|  | `Common/Machines/Common/Enums/HardwareModelEnums.cs` |  | Atari : définit `HardwareAudioCapability`, `HardwareVideoCapability`, `HardwareCpu`, `HardwareRegion`, `HardwarePortCapability` et `HardwareStorageCapability` pour le contrat partagé. |
|  | `Enums/AtariEightBitSettingDisposition.cs` |  | Atari : définit l’ensemble fermé `AtariEightBitSettingDisposition` utilisé par les contrats et services du module. |
|  | `Enums/AtariEnvironmentLanguage.cs` |  | Atari : définit l’ensemble fermé `AtariEnvironmentLanguage` utilisé par les contrats et services du module. |
|  | `Enums/AtariErrorCategory.cs` |  | Atari : définit l’ensemble fermé `AtariErrorCategory` utilisé par les contrats et services du module. |
|  | `Enums/AtariErrorCode.cs` |  | Atari : définit l’ensemble fermé `AtariErrorCode` utilisé par les contrats et services du module. |
|  | `Enums/AtariFirmwareCategory.cs` |  | Atari : définit l’ensemble fermé `AtariFirmwareCategory` utilisé par les contrats et services du module. |
|  | `Enums/AtariFirmwareCompatibility.cs` |  | Atari : définit l’ensemble fermé `AtariFirmwareCompatibility` utilisé par les contrats et services du module. |
|  | `Enums/AtariFirmwareDetectionStatus.cs` |  | Atari : définit l’ensemble fermé `AtariFirmwareDetectionStatus` utilisé par les contrats et services du module. |
|  | `Enums/AtariFirmwareDistribution.cs` |  | Atari : définit l’ensemble fermé `AtariFirmwareDistribution` utilisé par les contrats et services du module. |
|  | `Enums/AtariFirmwareEvidence.cs` |  | Atari : définit l’ensemble fermé `AtariFirmwareEvidence` utilisé par les contrats et services du module. |
|  | `Enums/AtariFirmwareHashAlgorithm.cs` |  | Atari : définit l’ensemble fermé `AtariFirmwareHashAlgorithm` utilisé par les contrats et services du module. |
|  | `Enums/AtariFirmwareProvision.cs` |  | Atari : définit l’ensemble fermé `AtariFirmwareProvision` utilisé par les contrats et services du module. |
|  | `Enums/AtariHostProcessState.cs` |  | Atari : définit l’ensemble fermé `AtariHostProcessState` utilisé par les contrats et services du module. |
|  | `Enums/AtariHostResponseStatus.cs` |  | Atari : définit l’ensemble fermé `AtariHostResponseStatus` utilisé par les contrats et services du module. |
|  | `Enums/AtariMachineFamily.cs` |  | Atari : définit l’ensemble fermé `AtariMachineFamily` utilisé par les contrats et services du module. |
|  | `Enums/AtariMachineModel.cs` |  | Atari : définit l’ensemble fermé `AtariMachineModel` utilisé par les contrats et services du module. |
|  | `Enums/AtariMediaAvailability.cs` |  | Atari : définit l’ensemble fermé `AtariMediaAvailability` utilisé par les contrats et services du module. |
|  | `Enums/AtariOptionAvailability.cs` |  | Atari : définit l’ensemble fermé `AtariOptionAvailability` utilisé par les contrats et services du module. |
|  | `Enums/AtariPeripheralCategory.cs` |  | Atari : définit l’ensemble fermé `AtariPeripheralCategory` utilisé par les contrats et services du module. |
|  | `Enums/AtariRuntimeRegion.cs` |  | Atari : définit l’ensemble fermé `AtariRuntimeRegion` utilisé par les contrats et services du module. |
|  | `Enums/AtariSettingOption.cs` |  | Atari : définit l’ensemble fermé `AtariSettingOption` utilisé par les contrats et services du module. |
|  | `Enums/AtariSettingsGroup.cs` |  | Atari : définit l’ensemble fermé `AtariSettingsGroup` utilisé par les contrats et services du module. |
|  | `Enums/AtariSettingsTab.cs` |  | Atari : définit l’ensemble fermé `AtariSettingsTab` utilisé par les contrats et services du module. |
|  | `Enums/AtariShortcutAvailability.cs` |  | Atari : définit l’ensemble fermé `AtariShortcutAvailability` utilisé par les contrats et services du module. |
|  | `Enums/AtariStAudioCapability.cs` |  | Atari : définit l’ensemble fermé `AtariStAudioCapability` utilisé par les contrats et services du module. |
|  | `Enums/AtariStCpu.cs` |  | Atari : définit l’ensemble fermé `AtariStCpu` utilisé par les contrats et services du module. |
|  | `Enums/AtariStCpuPrecision.cs` |  | Atari : définit l’ensemble fermé `AtariStCpuPrecision` utilisé par les contrats et services du module. |
|  | `Enums/AtariStFpu.cs` |  | Atari : définit l’ensemble fermé `AtariStFpu` utilisé par les contrats et services du module. |
|  | `Enums/AtariStorageBus.cs` |  | Atari : définit l’ensemble fermé `AtariStorageBus` utilisé par les contrats et services du module. |
|  | `Enums/AtariStoredStateCategory.cs` |  | Atari : définit l’ensemble fermé `AtariStoredStateCategory` utilisé par les contrats et services du module. |
|  | `Enums/AtariStPortCapability.cs` |  | Atari : définit l’ensemble fermé `AtariStPortCapability` utilisé par les contrats et services du module. |
|  | `Enums/AtariStRegion.cs` |  | Atari : définit l’ensemble fermé `AtariStRegion` utilisé par les contrats et services du module. |
|  | `Enums/AtariStStorageCapability.cs` |  | Atari : définit l’ensemble fermé `AtariStStorageCapability` utilisé par les contrats et services du module. |
|  | `Enums/AtariStVideoCapability.cs` |  | Atari : définit l’ensemble fermé `AtariStVideoCapability` utilisé par les contrats et services du module. |
|  | `Enums/AtariTosVariant.cs` |  | Atari : définit l’ensemble fermé `AtariTosVariant` utilisé par les contrats et services du module. |
## Exceptions

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
|  | `Exceptions/AtariEmulationException.cs` |  | Atari : définit `AtariEmulationException` afin de transporter les catégories et codes d’erreur propres au module. |
## Factories

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
|  | `Factories/Atari800MachineFactory.cs` |  | Atari : construit la machine du moteur concerné via `Atari800MachineFactory` ; points d’entrée : `Atari800MachineFactory`. |
|  | `Factories/AtariMachineFactory.cs` |  | Atari : construit la machine du moteur concerné via `AtariMachineFactory` ; points d’entrée : `AtariMachineFactory`, `Emulator`, `Create`. |
|  | `Factories/BeetleLynxMachineFactory.cs` |  | Atari : construit la machine du moteur concerné via `BeetleLynxMachineFactory` ; points d’entrée : `BeetleLynxMachineFactory`. |
|  | `Factories/HatariMachineFactory.cs` |  | Atari : construit la machine du moteur concerné via `HatariMachineFactory` ; points d’entrée : `HatariMachineFactory`. |
|  | `Factories/ProSystemMachineFactory.cs` |  | Atari : construit la machine du moteur concerné via `ProSystemMachineFactory` ; points d’entrée : `ProSystemMachineFactory`. |
| `Factories/PuaeMachineFactory.cs` |  |  | Amiga : construit la machine du moteur concerné via `PuaeMachineFactory` ; points d’entrée : `Create`. |
|  | `Factories/StellaMachineFactory.cs` |  | Atari : construit la machine du moteur concerné via `StellaMachineFactory` ; points d’entrée : `StellaMachineFactory`. |
|  | `Factories/VirtualJaguarMachineFactory.cs` |  | Atari : construit la machine du moteur concerné via `VirtualJaguarMachineFactory` ; points d’entrée : `VirtualJaguarMachineFactory`. |
## Functions

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `Functions/AmigaConfigurationSummaryFunctions.cs` | `Functions/AtariConfigurationSummaryFunctions.cs` |  | Amiga : fournit les transformations/validations `AmigaConfigurationSummaryFunctions`, `Create` dans `AmigaConfigurationSummaryFunctions`. Atari : fournit les transformations/validations `AtariConfigurationSummaryFunctions`, `Create` dans `AtariConfigurationSummaryFunctions`. |
| `Functions/AmigaConfigurationValidationFunctions.cs` |  |  | Amiga : fournit les transformations/validations `AmigaConfigurationValidationFunctions`, `ValidateForSave`, `IsEncryptedKickstart` dans `AmigaConfigurationValidationFunctions`. |
| `Functions/AmigaCoreHostProtocol.cs` |  |  | Amiga : fournit les transformations/validations `AmigaCoreHostProtocol`, `WriteString`, `ReadString`, `WriteBytes`, `ReadBytes`, `WriteInput`, … dans `AmigaCoreHostProtocol`. |
| `Functions/AmigaHardDiskFormats.cs` | `Functions/AtariHardDiskFormats.cs` |  | Amiga : fournit les transformations/validations `AmigaHardDiskFormats` dans `AmigaHardDiskFormats`. Atari : fournit les transformations/validations `AtariHardDiskFormats` dans `AtariHardDiskFormats`. |
| `Functions/AmigaInputSettingsFunctions.cs` | `Functions/AtariInputSettingsFunctions.cs` |  | Amiga : fournit les transformations/validations `AmigaInputSettingsFunctions`, `Describe`, `Apply` dans `AmigaInputSettingsFunctions`. Atari : fournit les transformations/validations `AtariInputSettingsFunctions`, `Describe`, `Apply` dans `AtariInputSettingsFunctions`. |
| `Functions/AmigaInputSnapshotFunctions.cs` | `Functions/AtariInputSnapshotFunctions.cs` |  | Amiga : fournit les transformations/validations `AmigaInputSnapshotFunctions`, `Apply` dans `AmigaInputSnapshotFunctions`. Atari : fournit les transformations/validations `AtariInputSnapshotFunctions`, `Apply` dans `AtariInputSnapshotFunctions`. |
| `Functions/AmigaRuntimeMediaFunctions.cs` |  |  | Amiga : fournit les transformations/validations `AmigaRuntimeMediaFunctions`, `PrepareMediaAsync`, `PrepareConfigurationAsync`, `ConvertScpPathAsync` dans `AmigaRuntimeMediaFunctions`. |
| `Functions/AmigaSettingsDescriptionFunctions.cs` | `Functions/AtariSettingsDescriptionFunctions.cs` |  | Amiga : fournit les transformations/validations `AmigaSettingsDescriptionFunctions`, `Create` dans `AmigaSettingsDescriptionFunctions`. Atari : fournit les transformations/validations `AtariSettingsDescriptionFunctions`, `Create` dans `AtariSettingsDescriptionFunctions`. |
| `Functions/AmigaStorageSettingsFunctions.cs` | `Functions/AtariStorageSettingsFunctions.cs` |  | Amiga : fournit les transformations/validations `AmigaStorageSettingsFunctions`, `Describe`, `Apply` dans `AmigaStorageSettingsFunctions`. Atari : fournit les transformations/validations `AtariStorageSettingsFunctions`, `Describe`, `Apply` dans `AtariStorageSettingsFunctions`. |
|  | `Functions/Atari800MediaFunctions.cs` |  | Atari : fournit les transformations/validations `Atari800MediaFunctions`, `Primary`, `Prepare`, `Classify`, `ApplyOptions`, `HasCartridgeHeader` dans `Atari800MediaFunctions`. |
|  | `Functions/AtariAudioFunctions.cs` |  | Atari : fournit les transformations/validations `AtariAudioFunctions`, `MaximumBufferedFrames`, `SingleFrame`, `CopyBatch`, `RetainNewestFrames` dans `AtariAudioFunctions`. |
|  | `Functions/AtariAudioOutputFunctions.cs` |  | Atari : fournit les transformations/validations `AtariAudioOutputFunctions`, `NormalizeVolume`, `ApplyVolume` dans `AtariAudioOutputFunctions`. |
|  | `Functions/AtariCartridgeFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCartridgeFunctions`, `Supports`, `Prepare`, `ValidateNoUnsupportedMetadata`, `ApplyOptions`, `GetMediaOptions` dans `AtariCartridgeFunctions`. |
|  | `Functions/AtariCassetteBootFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCassetteBootFunctions`, `IsRequested`, `RequiresDelayedReturn` dans `AtariCassetteBootFunctions`. |
|  | `Functions/AtariCassetteStateFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCassetteStateFunctions`, `From` dans `AtariCassetteStateFunctions`. |
|  | `Common/Machines/Common/Functions/HardwareModelFunctions.cs` |  | Atari : construit, indexe et valide les `HardwareModelDefinition` sans constante propre à une famille. |
|  | `Functions/AtariCompatibilityFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCompatibilityFunctions`, `Index`, `Editable`, `Forced`, `Unavailable`, `Hidden`, … dans `AtariCompatibilityFunctions`. |
|  | `Functions/AtariConfigurationFunctions.cs` |  | Atari : fournit les transformations/validations `AtariConfigurationFunctions`, `GetCore`, `GetFamily`, `Validate` dans `AtariConfigurationFunctions`. |
|  | `Functions/AtariConfigurationMigrationFunctions.cs` |  | Atari : fournit les transformations/validations `AtariConfigurationMigrationFunctions`, `MigrateToCurrent` dans `AtariConfigurationMigrationFunctions`. |
|  | `Functions/AtariConfigurationStoreFunctions.cs` |  | Atari : fournit les transformations/validations `AtariConfigurationStoreFunctions`, `ToDocument`, `FromDocument`, `StorePath`, `ResolvePath`, `WriteDocumentAtomicallyAsync` dans `AtariConfigurationStoreFunctions`. |
|  | `Functions/AtariContentFunctions.cs` |  | Atari : fournit les transformations/validations `AtariContentFunctions`, `Validate`, `Create` dans `AtariContentFunctions`. |
|  | `Functions/AtariControllerFunctions.cs` |  | Atari : fournit les transformations/validations `AtariControllerFunctions`, `ApplyDeadZone`, `ApplyDeadZones`, `Peripherals` dans `AtariControllerFunctions`. |
|  | `Functions/AtariControllerPortFunctions.cs` |  | Atari : fournit les transformations/validations `AtariControllerPortFunctions`, `Configure`, `ResolveDevice`, `ConfigurePort` dans `AtariControllerPortFunctions`. |
|  | `Functions/AtariCoreCatalogFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCoreCatalogFunctions`, `Create`, `GetInstallationPaths`, `GetActiveManifestPath`, `CreateModelAssociations` dans `AtariCoreCatalogFunctions`. |
|  | `Functions/AtariCoreDiagnosticFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCoreDiagnosticFunctions`, `CalculateSha256`, `ReadArchitecture`, `ReadDeclaredVersion`, `ReadExports` dans `AtariCoreDiagnosticFunctions`. |
|  | `Functions/AtariCoreFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCoreFunctions`, `ReadInitializedInfo`, `CreateInvalidOptionValueMessage`, `ExpectedLibraryName`, `ParseExtensions`, `ResolveExports`, … dans `AtariCoreFunctions`. |
|  | `Functions/AtariCoreHostFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCoreHostFunctions`, `CreatePipeName`, `CreateVideoMapName`, `WriteRequestHeader`, `ReadRequestHeader`, `WriteResponseHeader`, … dans `AtariCoreHostFunctions`. |
|  | `Functions/AtariCoreLifecycleFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCoreLifecycleFunctions`, `Load`, `Cleanup` dans `AtariCoreLifecycleFunctions`. |
|  | `Functions/AtariCoreOptionFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCoreOptionFunctions`, `CopyLegacyDefinitions`, `CopyVersionTwoDefinitions`, `SelectInternationalDefinitions`, `MergeLocalizedDefinitions`, `MergeLocalizedCategories`, … dans `AtariCoreOptionFunctions`. |
|  | `Functions/AtariCoreOptionProbe.cs` |  | Atari : fournit les transformations/validations `AtariCoreOptionProbe`, `DescribeFailure`, `Inspect` dans `AtariCoreOptionProbe`. |
|  | `Functions/AtariCoreReleaseFunctions.cs` |  | Atari : fournit les transformations/validations `AtariCoreReleaseFunctions`, `ParseRelease`, `DownloadAsync`, `ExtractExpectedLibrary`, `WriteManifestAtomicallyAsync`, `WriteActiveInstallationAtomicallyAsync`, … dans `AtariCoreReleaseFunctions`. |
|  | `Functions/AtariDiskControlFunctions.cs` |  | Atari : fournit les transformations/validations `AtariDiskControlFunctions` dans `AtariDiskControlFunctions`. |
|  | `Functions/AtariEightBitSettingsFunctions.cs` |  | Atari : fournit les transformations/validations `AtariEightBitSettingsFunctions`, `Normalize` dans `AtariEightBitSettingsFunctions`. |
|  | `Functions/AtariEnvironmentFunctions.cs` |  | Atari : fournit les transformations/validations `AtariEnvironmentFunctions`, `CreateUnknownCommandDiagnostic`, `CopyInputDescriptors`, `CopyControllerPorts`, `CopyMemoryMap`, `CurrentLanguage`, … dans `AtariEnvironmentFunctions`. |
|  | `Functions/AtariExternalCoreProbe.cs` |  | Atari : fournit les transformations/validations `AtariExternalCoreProbe`, `Inspect` dans `AtariExternalCoreProbe`. |
|  | `Functions/AtariFirmwareFunctions.cs` |  | Atari : fournit les transformations/validations `AtariFirmwareFunctions`, `Index`, `IsValidFingerprint`, `TosId`, `Tos`, `Replaceable`, … dans `AtariFirmwareFunctions`. |
|  | `Functions/AtariFirmwareRuntimeFunctions.cs` |  | Atari : fournit les transformations/validations `AtariFirmwareRuntimeFunctions`, `PrepareSystemDirectory`, `ClearManagedFirmwareFiles`, `ValidateRequiredFirmware`, `ResolveDefinition`, `ValidateReadableFile` dans `AtariFirmwareRuntimeFunctions`. |
|  | `Functions/AtariFirmwareScanFunctions.cs` |  | Atari : fournit les transformations/validations `AtariFirmwareScanFunctions`, `FamilyDirectoryName`, `EnsureFamilyDirectories`, `IsRelevantFile`, `EnumerateCandidates`, `ComputeMd5Async`, … dans `AtariFirmwareScanFunctions`. |
|  | `Functions/AtariFirmwareSelectionFunctions.cs` |  | Atari : fournit les transformations/validations `AtariFirmwareSelectionFunctions`, `IsSystemRom`, `FieldId`, `ReplaceField` dans `AtariFirmwareSelectionFunctions`. |
|  | `Common/Machines/Common/Functions/MachineFunctions.Hardware.cs` |  | Atari : présente les choix matériels communs, dont `HardwareRegionChoice`, à partir des contrats de machines. |
|  | `Functions/AtariHatariContentFunctions.cs` |  | Atari : fournit les transformations/validations `AtariHatariContentFunctions`, `Prepare`, `Cleanup` dans `AtariHatariContentFunctions`. |
|  | `Functions/AtariHatariStorageFunctions.cs` |  | Atari : fournit les transformations/validations `AtariHatariStorageFunctions`, `Prepare`, `ApplyWriteProtection`, `Cleanup`, `ResolveBus` dans `AtariHatariStorageFunctions`. |
|  | `Functions/AtariInputFunctions.cs` |  | Atari : fournit les transformations/validations `AtariInputFunctions`, `Freeze`, `State`, `Accumulate`, `ConsumeRelativePointer` dans `AtariInputFunctions`. |
|  | `Functions/AtariJaguarCdFunctions.cs` |  | Atari : fournit les transformations/validations `AtariJaguarCdFunctions`, `IsSupported`, `Prepare`, `RejectForStandardJaguar`, `Unsupported` dans `AtariJaguarCdFunctions`. |
|  | `Functions/AtariKeyboardFunctions.cs` |  | Atari : fournit les transformations/validations `AtariKeyboardFunctions`, `CreateKeyMap`, `Modifiers`, `Character`, `IsModifier`, `IsConsoleKeyActive` dans `AtariKeyboardFunctions`. |
|  | `Functions/AtariMachineFunctions.cs` |  | Atari : fournit les transformations/validations `AtariMachineFunctions`, `ThreadName`, `NextFrameTimestamp`, `ReleaseInput`, `TryReleaseInput`, `DeleteSessionDirectory` dans `AtariMachineFunctions`. |
|  | `Functions/AtariMachineOptionFunctions.cs` |  | Atari : fournit les transformations/validations `AtariMachineOptionFunctions`, `Apply` dans `AtariMachineOptionFunctions`. |
|  | `Functions/AtariMediaRuntimeFunctions.cs` |  | Atari : fournit les transformations/validations `AtariMediaRuntimeFunctions`, `Register`, `MarkEjected` dans `AtariMediaRuntimeFunctions`. |
|  | `Functions/AtariMessageFunctions.cs` |  | Atari : fournit les transformations/validations `AtariMessageFunctions`, `Translate` dans `AtariMessageFunctions`. |
|  | `Functions/AtariRuntimeFunctions.cs` |  | Atari : fournit les transformations/validations `AtariRuntimeFunctions`, `Region`, `RegionValue`, `ReadRegion`, `ProcessState`, `Status` dans `AtariRuntimeFunctions`. |
|  | `Functions/AtariRuntimeOptionFunctions.cs` |  | Atari : fournit les transformations/validations `AtariRuntimeOptionFunctions`, `RequiresRestart` dans `AtariRuntimeOptionFunctions`. |
|  | `Functions/AtariSavedStateFunctions.cs` |  | Atari : fournit les transformations/validations `AtariSavedStateFunctions`, `CreateHeader`, `Validate`, `ValidatePayloadSize`, `Invalid`, `HashBytes`, … dans `AtariSavedStateFunctions`. |
|  | `Functions/AtariScpMediaFunctions.cs` |  | Atari : fournit les transformations/validations `AtariScpMediaFunctions`, `IsScp`, `Prepare` dans `AtariScpMediaFunctions`. |
|  | `Functions/AtariSessionMediaFunctions.cs` |  | Atari : fournit les transformations/validations `AtariSessionMediaFunctions`, `Prepare`, `Save`, `ReadSourcePaths` dans `AtariSessionMediaFunctions`. |
|  | `Functions/AtariShortcutFunctions.cs` |  | Atari : fournit les transformations/validations `AtariShortcutFunctions`, `Rules`, `IsAvailable` dans `AtariShortcutFunctions`. |
|  | `Functions/AtariStateFileFunctions.cs` |  | Atari : fournit les transformations/validations `AtariStateFileFunctions`, `Write`, `Read` dans `AtariStateFileFunctions`. |
|  | `Functions/AtariStateFunctions.cs` |  | Atari : fournit les transformations/validations `AtariStateFunctions`, `IsAvailable` dans `AtariStateFunctions`. |
|  | `Functions/AtariStateStoreFunctions.cs` |  | Atari : fournit les transformations/validations `AtariStateStoreFunctions`, `GetMachineDirectory`, `ValidateStateName`, `GetFileStem`, `WriteBytesAtomically`, `WriteMetadataAtomically`, … dans `AtariStateStoreFunctions`. |
|  | `Functions/AtariStModelFunctions.cs` |  | Atari : fournit les transformations/validations `AtariStModelFunctions`, `InclusiveRange`, `Index` dans `AtariStModelFunctions`. |
|  | `Functions/AtariStorageConfigurationFunctions.cs` |  | Atari : fournit les transformations/validations `AtariStorageConfigurationFunctions`, `Family`, `IsRemovable`, `IsPrimaryDevice` dans `AtariStorageConfigurationFunctions`. |
|  | `Functions/AtariTosHeaderReader.cs` |  | Atari : fournit les transformations/validations `AtariTosHeaderReader`, `ReadAsync` dans `AtariTosHeaderReader`. |
|  | `Functions/AtariVideoFunctions.cs` |  | Atari : fournit les transformations/validations `AtariVideoFunctions`, `FrameLength`, `CopyRows`, `Timestamp` dans `AtariVideoFunctions`. |
| `Functions/EmulationMediaActivityFunctions.cs` | `Functions/EmulationMediaActivityFunctions.cs` |  | Amiga : fournit les transformations/validations `EmulationMediaActivityFunctions`, `FromLedStates` dans `EmulationMediaActivityFunctions`. Atari : fournit les transformations/validations `EmulationMediaActivityFunctions`, `FromRuntimeStatus`, `FromLedStates`, `CaptureHatariOverlay`, `CaptureAtari800Overlay` dans `EmulationMediaActivityFunctions`. |
| `Functions/EmulationMediaConversionFunctions.cs` | `Functions/EmulationMediaConversionFunctions.cs` |  | Amiga : fournit les transformations/validations `EmulationMediaConversionFunctions`, `ToCommon` dans `EmulationMediaConversionFunctions`. Atari : fournit les transformations/validations `EmulationMediaConversionFunctions`, `ToCommon`, `ToAtari` dans `EmulationMediaConversionFunctions`. |
|  | `Functions/EmulationPeripheralConversionFunctions.cs` |  | Atari : fournit les transformations/validations `EmulationPeripheralConversionFunctions`, `ToAtari` dans `EmulationPeripheralConversionFunctions`. |
|  | `Functions/SettingsDescription/AtariSettingsDescriptionFunctions.AudioAndChoices.cs` |  | Atari : porte la partie Settings Description Functions.Audio And Choices de la description des réglages : `AtariSettingsDescriptionFunctions`. |
|  | `Functions/SettingsDescription/AtariSettingsDescriptionFunctions.Builders.cs` |  | Atari : porte la partie Settings Description Functions.Builders de la description des réglages : `AtariSettingsDescriptionFunctions`. |
|  | `Functions/SettingsDescription/AtariSettingsDescriptionFunctions.Classic.cs` |  | Atari : porte la partie Settings Description Functions.Classic de la description des réglages : `AtariSettingsDescriptionFunctions`. |
|  | `Functions/SettingsDescription/AtariSettingsDescriptionFunctions.St.cs` |  | Atari : porte la partie Settings Description Functions.St de la description des réglages : `AtariSettingsDescriptionFunctions`. |
## Interfaces

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `Interfaces/IAmigaCore.cs` | `Interfaces/IAtariCore.cs` |  | Amiga : définit `IAmigaCore` aux implémentations internes. Atari : définit `IAtariCore` aux implémentations internes. |
| `Interfaces/IAmigaMachineFactory.cs` | `Interfaces/IAtariMachineFactory.cs` |  | Amiga : définit `IAmigaMachineFactory` et impose `IAmigaMachineFactory` aux implémentations internes. Atari : définit `IAtariMachineFactory` et impose `IAtariMachineFactory` aux implémentations internes. |
|  | `Interfaces/IAtariCoreReleaseService.cs` |  | Atari : définit `IAtariCoreReleaseService` et impose `IAtariCoreReleaseService` aux implémentations internes. |
## Modules

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `Modules/AmigaEmulationModule.cs` | `Modules/AtariEmulationModule.cs` |  | Dans les deux modules, implémente la façade SDK du module : machines, réglages, configurations, runtime et services optionnels. |
| `Modules/AmigaEmulationModuleFactory.cs` | `Modules/AtariEmulationModuleFactory.cs` |  | Dans les deux modules, implémente le point d’entrée découvert par l’App et construit le module depuis son contexte. |
## Services

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `Services/AmigaConfigurationStore.cs` | `Services/AtariConfigurationStore.cs` |  | Amiga : implémente `AmigaConfigurationStore` ; opérations principales : `AmigaConfigurationStore`, `LoadAllAsync`, `SaveAsync`, `Delete`. Atari : implémente `AtariConfigurationStore` ; opérations principales : `AtariConfigurationStore`, `LoadAllAsync`, `SaveAsync`, `Delete`. |
| `Services/AmigaCoreHost.cs` | `Services/AtariCoreHost.cs` |  | Amiga : implémente `AmigaCoreHost` ; opérations principales : `AmigaCoreHost`, `Run`. Atari : implémente `AtariCoreHost` ; opérations principales : `AtariCoreHost`, `Run`. |
| `Services/AmigaCoreProvider.cs` | `Services/AtariCoreProvider.cs` |  | Amiga : implémente `AmigaCoreProvider` ; opérations principales : `AmigaCoreProvider`, `FindInstalledPathAsync`. Atari : implémente `AtariCoreProvider` ; opérations principales : `AtariCoreProvider`, `FindInstalledPathAsync`. |
| `Services/AmigaCoreReleaseService.cs` | `Services/AtariCoreReleaseService.cs` |  | Amiga : implémente `AmigaCoreReleaseService` ; opérations principales : `AmigaCoreReleaseService`, `GetInstalledVersion`, `GetAvailableAsync`, `IsInstalled`, `GetLibraryPath`, `InstallAsync`, …. Atari : implémente `AtariCoreReleaseService` ; opérations principales : `AtariCoreReleaseService`, `GetAvailableAsync`, `InstallAsync`, `GetActiveInstallationAsync`. |
| `Services/AmigaEngine.cs` | `Services/AtariEngine.cs` |  | Amiga : implémente `AmigaEngine` ; opérations principales : `AmigaEngine`, `CreateMachine`. Atari : implémente `AtariEngine` ; opérations principales : `AtariEngine`, `CreateMachine`. |
| `Services/AmigaExternalCore.cs` | `Services/AtariExternalCore.cs` |  | Amiga : implémente `AmigaExternalCore` ; opérations principales : `AmigaExternalCore`, `TryDequeueAudio`, `CoreName`, `CoreVersion`, `SupportedContentExtensions`, `CoreSha256`, …. Atari : implémente `AtariExternalCore` ; opérations principales : `AtariExternalCore`, `Emulator`, `CoreSha256`, `Region`, `TryDequeueAudio`, `Initialize`, …. |
| `Services/AmigaExternalCoreInstaller.cs` |  |  | Amiga : implémente `AmigaExternalCoreInstaller` ; opérations principales : `AmigaExternalCoreInstaller`, `IsInstalled`, `InstallAsync`, `WriteManifestAsync`, `Hash`. |
| `Services/AmigaExternalDiskControl.cs` |  |  | Amiga : implémente `AmigaExternalDiskControl` ; opérations principales : `AmigaExternalDiskControl`, `Capture`, `CaptureExtended`, `CurrentIndex`, `Select`, `GetPath`, …. |
| `Services/AmigaExternalHostCallbacks.cs` | `Services/AtariExternalHostCallbacks.cs` |  | Amiga : implémente `AmigaExternalHostCallbacks` ; opérations principales : `DiskControl`, `AmigaExternalHostCallbacks`, `SystemDirectory`, `ContentDirectory`, `SaveDirectory`, `LatestVideoFrame`, …. Atari : implémente `AtariExternalHostCallbacks` ; opérations principales : `AtariExternalHostCallbacks`, `Environment`, `Video`, `AudioSample`, `AudioBatch`, `InputPoll`, …. |
| `Services/AmigaInputAccumulator.cs` |  |  | Amiga : implémente `AmigaInputAccumulator` ; opérations principales : `AmigaInputAccumulator`, `Update`, `Consume`. |
| `Services/AmigaMachine.cs` | `Services/AtariMachine.cs` |  | Amiga : implémente `AmigaMachine`, `PendingCommand` ; opérations principales : `AmigaMachine`, `Id`, `Configuration`, `State`, `StartAsync`, `PauseAsync`, …. Atari : implémente `AtariMachine` ; opérations principales : `AtariMachine`, `Id`, `Configuration`, `State`, `StartAsync`, `PauseAsync`, …. |
| `Services/AmigaProcessCore.cs` | `Services/AtariProcessCore.cs` |  | Amiga : implémente `AmigaProcessCore` ; opérations principales : `AmigaProcessCore`, `LatestVideoFrame`, `LatestAudioChunk`, `Options`, `Diagnostics`, `LedStates`, …. Atari : implémente `AtariProcessCore` ; opérations principales : `AtariProcessCore`, `LatestVideoFrame`, `LatestAudioChunk`, `Options`, `Diagnostics`, `LedStates`, …. |
| `Services/AmigaStateStore.cs` |  |  | Amiga : implémente `AmigaStateStore` ; opérations principales : `AmigaStateStore`, `Write`, `HashFile`, `HashPath`, `HashBytes`. |
|  | `Services/AtariAudioBuffer.cs` |  | Atari : implémente `AtariAudioBuffer` ; opérations principales : `AtariAudioBuffer`, `BufferedFrames`, `OverrunCount`, `UnderrunCount`, `Enqueue`, `TryDequeue`. |
|  | `Services/AtariAudioOutputController.cs` |  | Atari : implémente `AtariAudioOutputController` ; opérations principales : `AtariAudioOutputController`, `IsMuted`, `Volume`, `Start`, `Write`, `SetMuted`, …. |
|  | `Services/AtariCassetteInputController.cs` |  | Atari : implémente `AtariCassetteInputController` ; opérations principales : `AtariCassetteInputController`, `AvailableCommands`, `SetPhysicalInput`, `Play`, `NextFrame`. |
|  | `Services/AtariContentPath.cs` |  | Atari : implémente `AtariContentPath` ; opérations principales : `AtariContentPath`, `Pointer`, `Dispose`. |
|  | `Services/AtariCoreOptionHost.cs` |  | Atari : implémente `AtariCoreOptionHost` ; opérations principales : `AtariCoreOptionHost`, `Catalog`, `Categories`, `RegisterLegacyVariables`, `RegisterVersionOne`, `RegisterVersionOneInternational`, …. |
|  | `Services/AtariDiskControl.cs` |  | Atari : implémente `AtariDiskControl` ; opérations principales : `AtariDiskControl`, `CurrentIndex`, `Capture`, `CaptureExtended`, `Select`, `Insert`, …. |
|  | `Services/AtariFirmwareScanner.cs` |  | Atari : implémente `AtariFirmwareScanner` ; opérations principales : `AtariFirmwareScanner`, `ScanAsync`. |
|  | `Services/AtariFrameTimer.cs` |  | Atari : implémente `AtariFrameTimer` ; opérations principales : `AtariFrameTimer`, `WaitUntil`, `Dispose`. |
|  | `Services/AtariHatariStorage.cs` |  | Atari : implémente `AtariHatariStorage` ; opérations principales : `AtariHatariStorage`, `Configuration`, `Bus`, `RuntimePath`, `Volumes`, `OwnsMarker`. |
|  | `Services/AtariInputFrameStore.cs` |  | Atari : implémente `AtariInputFrameStore` ; opérations principales : `AtariInputFrameStore`, `Update`, `Poll`, `State`. |
|  | `Services/AtariKeyboardState.cs` |  | Atari : implémente `AtariKeyboardState` ; opérations principales : `AtariKeyboardState`, `Publish`. |
|  | `Services/AtariLoadedContent.cs` |  | Atari : implémente `AtariLoadedContent` ; opérations principales : `AtariLoadedContent`, `GameInfo`, `Dispose`. |
|  | `Services/AtariSharedVideoWriter.cs` |  | Atari : implémente `AtariSharedVideoWriter` ; opérations principales : `AtariSharedVideoWriter`, `View`, `Name`, `SlotCapacity`, `EnsureCapacity`, `Dispose`. |
|  | `Services/AtariVideoBufferSet.cs` |  | Atari : implémente `AtariVideoBufferSet` ; opérations principales : `AtariVideoBufferSet`, `Rent`, `Dispose`. |
| `Services/ExternalHostCallbacks/AmigaExternalHostCallbacks.AudioVideo.cs` |  |  | Amiga : implémente la partie External Host Callbacks.Audio Video des callbacks Libretro conservés par l’hôte externe : `AmigaExternalHostCallbacks`, `ApplyInitialAvInfo`, `TryDequeueAudio`. |
| `Services/ExternalHostCallbacks/AmigaExternalHostCallbacks.Environment.cs` |  |  | Amiga : implémente la partie External Host Callbacks.Environment des callbacks Libretro conservés par l’hôte externe : `AmigaExternalHostCallbacks`, `ValidateConfiguredOptions`. |
| `Services/ExternalHostCallbacks/AmigaExternalHostCallbacks.Input.cs` |  |  | Amiga : implémente la partie External Host Callbacks.Input des callbacks Libretro conservés par l’hôte externe : `AmigaExternalHostCallbacks`. |
## Resources

| Amiga | Atari | Amstrad | Description |
|---|---|---|---|
| `Resources/00-Base/Emulation.resx` | `Resources/00-Base/Emulation.resx` |  | Dans les deux modules, contient les valeurs invariantes communes aux cultures. |
| `Resources/ar-SA/Emulation.resx` | `Resources/ar-SA/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture ar-SA. |
| `Resources/cs-CZ/Emulation.resx` | `Resources/cs-CZ/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture cs-CZ. |
| `Resources/da-DK/Emulation.resx` | `Resources/da-DK/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture da-DK. |
| `Resources/de-DE/Emulation.resx` | `Resources/de-DE/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture de-DE. |
| `Resources/el-GR/Emulation.resx` | `Resources/el-GR/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture el-GR. |
| `Resources/en-US/Emulation.resx` | `Resources/en-US/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture en-US. |
| `Resources/es-ES/Emulation.resx` | `Resources/es-ES/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture es-ES. |
| `Resources/fi-FI/Emulation.resx` | `Resources/fi-FI/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture fi-FI. |
| `Resources/fr-FR/Emulation.resx` | `Resources/fr-FR/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture fr-FR. |
| `Resources/he-IL/Emulation.resx` | `Resources/he-IL/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture he-IL. |
| `Resources/hu-HU/Emulation.resx` | `Resources/hu-HU/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture hu-HU. |
| `Resources/id-ID/Emulation.resx` | `Resources/id-ID/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture id-ID. |
| `Resources/it-IT/Emulation.resx` | `Resources/it-IT/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture it-IT. |
| `Resources/ja-JP/Emulation.resx` | `Resources/ja-JP/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture ja-JP. |
| `Resources/ko-KR/Emulation.resx` | `Resources/ko-KR/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture ko-KR. |
| `Resources/nb-NO/Emulation.resx` | `Resources/nb-NO/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture nb-NO. |
| `Resources/nl-NL/Emulation.resx` | `Resources/nl-NL/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture nl-NL. |
| `Resources/pl-PL/Emulation.resx` | `Resources/pl-PL/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture pl-PL. |
| `Resources/pt-BR/Emulation.resx` | `Resources/pt-BR/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture pt-BR. |
| `Resources/pt-PT/Emulation.resx` | `Resources/pt-PT/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture pt-PT. |
| `Resources/ro-RO/Emulation.resx` | `Resources/ro-RO/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture ro-RO. |
| `Resources/ru-RU/Emulation.resx` | `Resources/ru-RU/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture ru-RU. |
| `Resources/sv-SE/Emulation.resx` | `Resources/sv-SE/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture sv-SE. |
| `Resources/th-TH/Emulation.resx` | `Resources/th-TH/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture th-TH. |
| `Resources/tr-TR/Emulation.resx` | `Resources/tr-TR/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture tr-TR. |
| `Resources/uk-UA/Emulation.resx` | `Resources/uk-UA/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture uk-UA. |
| `Resources/vi-VN/Emulation.resx` | `Resources/vi-VN/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture vi-VN. |
| `Resources/zh-Hans/Emulation.resx` | `Resources/zh-Hans/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture zh-Hans. |
| `Resources/zh-Hant/Emulation.resx` | `Resources/zh-Hant/Emulation.resx` |  | Dans les deux modules, contient les traductions de la culture zh-Hant. |

## Raccordements extérieurs explicites

Ces fichiers extérieurs nomment réellement Amiga ou Atari et ont un effet sur le build, les tests,
la distribution ou la documentation d’architecture. Les simples mentions historiques dans des feuilles
de tâches terminées ne sont pas des raccordements exécutables et ne sont pas assimilées à des dépendances.

| Fichier depuis la racine du dépôt | Module nommé | Effet réel |
|---|---|---|
| `GWGUI.sln` | Amiga et Atari | Ajoute les deux projets à la solution et les place sous le dossier logique src avec leurs configurations de build. |
| `src/GWGUI.Emulation/GWGUI.Emulation.csproj` | Amiga et Atari | Déclare actuellement les deux assemblies comme InternalsVisibleTo : le SDK commun leur ouvre donc aussi ses membres internal. |
| `tests/GWGUI.Tests/GWGUI.Tests.csproj` | Amiga et Atari | Référence directement les deux projets afin que les scénarios de tests puissent instancier leurs types. |
| `module-registry/amiga.json` | Amiga | Publie l’identifiant et l’URL du catalogue du module Amiga dans l’annuaire public. |
| `module-registry/atari.json` | Atari | Publie l’identifiant et l’URL du catalogue du module Atari dans l’annuaire public. |
| `tests/GWGUI.Tests/Architecture/MediaEngineProjectBoundaryTests.cs` | Amiga et Atari | Instancie les factories des deux modules pour vérifier leurs frontières avec MediaEngine. |
| `tests/GWGUI.Tests/Emulation/Audio/AudioBufferScenarios.cs` | Atari | Teste directement le contrôleur de sortie audio Atari et son remplacement de périphérique. |
| `tests/GWGUI.Tests/Emulation/EmulationContracts/RuntimeOptionScenarios.cs` | Atari | Vérifie les options runtime et leurs règles de redémarrage avec les catalogues Atari. |
| `tests/GWGUI.Tests/Emulation/MachineAdapters/AtariCartridgeScenarios.cs` | Atari | Teste la préparation et les règles des cartouches Atari. |
| `tests/GWGUI.Tests/Emulation/MachineAdapters/AtariCassetteScenarios.cs` | Atari | Teste le démarrage et l’état du transport cassette Atari. |
| `tests/GWGUI.Tests/Emulation/MachineAdapters/AtariMediaActivityScenarios.cs` | Atari | Teste la conversion des états d’activité média Atari vers le contrat commun. |
| `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineAdapterFailureScenarios.cs` | Amiga et Atari | Provoque les échecs contrôlés des adaptateurs et cœurs des deux modules. |
| `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineAdaptersTests.cs` | Amiga et Atari | Expose à xUnit les scénarios communs et fournit les modèles Atari issus du catalogue. |
| `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineCapabilitiesScenarios.cs` | Amiga et Atari | Vérifie les capacités, réglages de stockage et médias annoncés par les deux modules. |
| `tests/GWGUI.Tests/Emulation/MachineAdapters/MachineConfigurationMappingScenarios.cs` | Amiga et Atari | Vérifie création, application, persistance et conversion des configurations/médias des deux modules. |
| `tests/GWGUI.Tests/Emulation/Modules/EmulationModuleManifestTests.cs` | Amiga comme exemple de manifeste | Utilise un manifeste d’exemple nommé amiga pour tester le parseur générique ; ce nom n’ajoute aucune dépendance runtime. |
| `tests/GWGUI.Tests/Updates/ModuleDirectoryServiceTests.cs` | Amiga comme donnée de test | Utilise un catalogue fictif nommé amiga pour vérifier la sélection de versions et l’état d’installation du service d’annuaire générique ; ce nom n’ajoute aucune dépendance runtime. |
| `tests/GWGUI.LocalDiskImageTests/TemporaryLibretroMediaReader/TemporaryLibretroMediaReader.csproj` | Atari | Référence temporairement le module Atari pour son lecteur Libretro d’audit. |
| `tests/GWGUI.LocalDiskImageTests/TemporaryLibretroMediaReader/TemporaryLibretroMediaReader.cs` | Atari | Utilise les contrats et services Atari afin de lire des médias pendant l’audit temporaire. |
| `docs/architecture/emulation.md` | Amiga et Atari | Décrit les frontières SDK/App/modules, le cycle d’une instance et les responsabilités observées des deux modules. |
| `docs/architecture/emulation-module-localization.md` | Amiga et Atari | Décrit le raccordement de leurs ressources localisées au SDK et à l’hôte, ainsi que la propriété observée des clés de traduction. |
| `docs/architecture/emulation-modules.md` | Amiga et Atari | Documente le paquet Modules/<id>, le manifeste et les assemblies actuelles. |
| `docs/architecture/emulation-process-display.md` | Amiga et Atari | Documente leurs processus hôtes sans fenêtre et la présentation vidéo partagée. |
| `docs/architecture/overview.md` | Amiga et Atari | Présente les deux projets comme modules spécialisés de l’architecture générale. |
| `docs/architecture/media-engine-file-layout.md` | Amiga et Atari | Mentionne leurs dépendances lors de la description du rangement MediaEngine. |
| `docs/reference/atari-libretro.md` | Atari | Conserve les preuves techniques, choix de cœurs et validations Libretro Atari. |
| `docs/project/testing.md` | Amiga et Atari | Donne les commandes de build ciblées des deux modules. |
| `docs/project/media-engine-rangement-inventory.md` | Amiga et Atari | Recense leurs consommations historiques de fichiers MediaEngine lors du rangement. |
| `docs/project/media-library-file-inventory.md` | Amiga et Atari | Recense les appels des deux modules vers les fonctions de la bibliothèque média. |
| `docs/tasks/emulation/amstrad.md` | Amiga et Atari | Feuille de travail actuelle ; ses anciennes propositions de modification ne constituent pas un raccordement du logiciel et devront être réévaluées après cette cartographie. |

## Raccordements génériques sans nom de famille

Ces fichiers prennent en charge tout module conforme au SDK. C’est cette chaîne qui permet d’ajouter
une future DLL sans ajouter son nom dans l’App.

| Fichier depuis la racine du dépôt | Responsabilité générique |
|---|---|
| `src/GWGUI.Emulation/Constants/EmulationHostApi.cs` | Fixe la version d’API hôte et le nom conventionnel module.json. |
| `src/GWGUI.Emulation/Contracts/EmulationModuleManifest.cs` | Définit le schéma de données du manifeste de tout module. |
| `src/GWGUI.Emulation/Contracts/EmulationModuleContext.cs` | Transmet au module ses dossiers, l’exécutable hôte et les services fournis par l’App. |
| `src/GWGUI.Emulation/Interfaces/IEmulationModuleFactory.cs` | Définit l’unique point d’entrée que la découverte dynamique recherche dans l’assembly. |
| `src/GWGUI.Emulation/Interfaces/IEmulationModule.cs` | Définit la façade commune utilisée par l’App pour machines, configurations, réglages et runtime. |
| `src/GWGUI.Emulation/Interfaces/IEmulationModuleLocalization.cs` | Permet à un module de résoudre ses propres ressources sans que l’App connaisse son nom. |
| `src/GWGUI.Emulation/Contracts/EmulationEmulatorInstallation.cs` | Transporte l’identifiant du moteur courant et sa version installée éventuelle. |
| `src/GWGUI.Emulation/Contracts/EmulationEmulatorRelease.cs` | Décrit une version de moteur téléchargeable, son libellé et son caractère obligatoire éventuel. |
| `src/GWGUI.Emulation/Contracts/EmulationEmulatorMessageContext.cs` | Associe l’identifiant du moteur au contexte des messages émis pendant sa gestion. |
| `src/GWGUI.Emulation/Interfaces/IEmulationEmulatorMessageContext.cs` | Étend le contexte de message commun avec l’identifiant du moteur concerné. |
| `src/GWGUI.Emulation/Interfaces/IEmulationEmulatorManager.cs` | Transporte la configuration opaque entre l’App et le module pour obtenir le moteur sélectionné, recevoir toutes les installations disponibles, appliquer un `EmulatorId` à la configuration et rechercher ou installer les releases correspondantes ; les opérations historiques par `MachineId` restent le comportement par défaut des modules à moteur unique. |
| `src/GWGUI.Emulation/README.md` | Documente le SDK, le manifeste et le contrat d’auteur de module. |
| `src/GWGUI.App/App.xaml.cs` | Construit le registre au démarrage, charge les modules découverts et leur transmet les arguments de commande hôte. |
| `src/GWGUI.App/Services/Emulation/EmulationModuleManifestReader.cs` | Lit et valide strictement module.json et sa compatibilité avec l’API hôte. |
| `src/GWGUI.App/Services/Emulation/EmulationModuleLoadContext.cs` | Charge l’assembly du module et ses dépendances depuis son dossier isolé. |
| `src/GWGUI.App/Services/Emulation/EmulationModuleRegistry.cs` | Parcourt les dossiers Modules, exige un manifeste, charge exactement une factory publique et refuse doublons/incompatibilités. |
| `src/GWGUI.App/Services/Emulation/LoadedEmulationModule.cs` | Conserve ensemble manifeste, façade chargée, contexte d’assembly et dossier d’un module. |
| `src/GWGUI.App/Localization/Extensions/LocExtension.cs` | Résout d’abord les ressources du module via IEmulationModuleLocalization puis les ressources communes de l’App. |
| `src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationListItem.cs` | Associe une configuration affichée à son IEmulationModule propriétaire. |
| `src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs` | Associe une ligne de tableau de configurations au module qui sait l’interpréter. |
| `src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationPresenter.cs` | Demande au module le résumé et localise le nom de machine. |
| `src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs` | Construit les lignes de configurations à partir des modules chargés. |
| `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationConfigurationPersistenceFunctions.cs` | Appelle les opérations génériques de sauvegarde/suppression du module sélectionné. |
| `src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs` | Présente les valeurs de réglage déclarées par n’importe quel module. |
| `src/GWGUI.App/Constants/Emulation/EmulationCoreManagementConstants.cs` | Centralise les clés de traduction, noms d’automatisation et valeurs visuelles du panneau générique de gestion des moteurs. |
| `src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs` | Active la gestion de firmware lorsque le module implémente l’interface optionnelle correspondante. |
| `src/GWGUI.App/Controllers/Emulation/Options/EmulationEmulatorManagementController.cs` | Interroge le gestionnaire du module avec l’`IEmulationConfiguration` courante, affiche les `EmulatorId` reçus, réinjecte le choix utilisateur dans cette configuration, verrouille le choix d’une machine déjà sauvegardée et conserve la recherche, la progression, l’annulation et l’installation existantes. |
| `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationCoreManagementPanel.cs` | Fournit le sélecteur de moteur déjà présent ainsi que le bouton d’installation, le statut, la progression et l’annulation. |
| `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` | Construit l’éditeur depuis `IEmulationModule`, fournit au contrôleur d’émulateurs la configuration opaque courante et laisse le parcours existant de brouillon ou de sauvegarde persister la configuration retournée. |
| `src/GWGUI.App/Views/Windows/EmulationModuleOptions/EmulationModuleOptionsWindow.xaml.cs` | Héberge la fenêtre d’options d’un module sans connaître sa famille. |
| `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSection.cs` | Héberge une instance de machine créée par le runtime du module. |
| `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionMachineFunctions.cs` | Pilote cycle de vie, vidéo, audio, entrées et médias par les contrats communs. |
| `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationPreferencesSection.cs` | Affiche les modules et machines découverts dans les préférences. |
| `src/GWGUI.App/Views/Controls/Shell/MainMenu.xaml.cs` | Construit les entrées de menu d’émulation depuis le registre chargé. |
| `src/GWGUI.App/Views/Windows/Shell/MainWindow.ComponentConnections.cs` | Raccorde les modules chargés aux composants de la fenêtre principale. |
| `src/GWGUI.App/Views/Windows/Shell/MainWindow.EventsAndCommands.cs` | Route les commandes d’ouverture/options vers le module choisi. |
| `src/GWGUI.App/Services/Windows/WpfWindowNavigationService.cs` | Ouvre les fenêtres génériques d’options/configuration avec le module concerné. |
| `src/GWGUI.App/Resources/00-Base/Emulation/EmulationCore.resx` | Définit les textes communs du sélecteur, de la recherche, du téléchargement, de l’état installé et du chemin ; le même fichier est traduit dans chacune des 29 cultures de l’App. |
| `src/GWGUI.App/Services/Emulation/HardDiskDeletionService.cs` | Interroge tous les modules pour empêcher la suppression d’un disque encore référencé. |
| `src/GWGUI.App/Services/Updates/ModuleDirectoryService.cs` | Gère les dossiers d’installation des modules par identifiant. |
| `src/GWGUI.App/Services/Updates/ModuleInstallationService.cs` | Installe/remplace un paquet de module validé sans liste de familles codée en dur. |
| `src/GWGUI.App/Services/Updates/ModuleUpdateService.cs` | Lit le catalogue déclaré par le manifeste et recherche la mise à jour du module. |
| `src/GWGUI.App/Services/Updates/ApplicationUpdateService.cs` | Coordonne les composants de mise à jour de l’application et les informations de modules sans connaître leurs familles. |
| `src/GWGUI.App/Services/Updates/UpdatePackagePreparationService.cs` | Valide la compatibilité hôte et prépare atomiquement l’installation du paquet. |
| `src/GWGUI.App/Options/Controllers/UpdateOptionsController.cs` | Présente dans l’interface l’état et les actions de mise à jour des composants, dont les modules découverts. |
| `src/GWGUI.Updates/Services/UpdateArchiveValidator.cs` | Valide l’arborescence Modules/<id>, le manifeste et l’assembly d’un paquet téléchargé. |
| `src/GWGUI.Launcher/Program.cs` | Résout les assemblies de l’application lancée ; il ne possède aucune liste Amiga/Atari. |
| `scripts/local-building.cmd` | Découvre chaque src/GWGUI.Emulation.* contenant module.json et l’ajoute au build local. |
| `scripts/local-building/build.ps1` | Copie dans la sortie les métadonnées et fichiers publiés de chaque module découvert. |
| `scripts/publish-modules.cmd` | Parcourt les projets de modules ou publie l’identifiant demandé à partir de son manifeste. |
| `scripts/module-release/package-module.ps1` | Construit et contrôle le paquet indépendant d’un module. |
| `scripts/module-directory/build-module-directory.ps1` | Transforme les fichiers module-registry/*.json en annuaire public. |
| `scripts/release/update-catalog/emulation-modules/emulation-modules.ps1` | Ajoute automatiquement les manifestes des projets GWGUI.Emulation.* au catalogue de publication. |
| `.github/workflows/module-release.yml` | Publie un module identifié par son manifeste, sans liste Amiga/Atari. |
| `.github/workflows/module-directory.yml` | Reconstruit et publie l’annuaire à chaque changement de module-registry. |
| `scripts/tests/test-installer.ps1` | Vérifie génériquement que l’installateur place ou préserve les manifests des modules. |
| `scripts/tests/test-installer-upgrade.ps1` | Crée un module fictif et vérifie sa conservation pendant une mise à niveau. |
| `sdk/module-template/GWGUI.Emulation.Module.csproj` | Fournit le modèle de projet externe avec copie de module.json et référence au SDK. |
| `sdk/module-template/ModuleFactory.cs` | Fournit l’exemple d’IEmulationModuleFactory découvert dynamiquement. |
| `sdk/module-template/module.json` | Fournit le manifeste générique à adapter par un auteur de module. |
| `sdk/module-template/.github/workflows/release-module.yml` | Fournit le workflow générique de paquet/catalogue d’un module externe. |
| `tests/GWGUI.Tests/Emulation/Modules/EmulationModuleManifestTests.cs` | Vérifie le parseur et les plages de compatibilité du manifeste générique. |
| `tests/GWGUI.Tests/Interface/SettingsViews/EmulationModuleSettingsNavigationScenarios.cs` | Vérifie l’affichage du moteur courant et le parcours générique de téléchargement depuis les options du module. |
| `tests/GWGUI.Tests/Interface/SettingsViews/SettingsViewsTests.cs` | Expose les scénarios de navigation et de gestion du moteur dans la suite de tests STA. |
| `tests/GWGUI.Tests/Updates/PendingModuleInstallationScenarios.cs` | Vérifie la préparation et l’activation différée d’un module fictif sans dépendre d’une famille réelle. |

## Contrôle d’exhaustivité

| Catégorie | Amiga attendu et recensé | Atari attendu et recensé |
|---|---:|---:|
| Racine | 3 | 3 |
| Constants | 26 | 80 |
| Contracts | 14 | 61 |
| Dictionaries | 4 | 7 |
| Enums | 6 | 46 |
| Exceptions | 0 | 1 |
| Factories | 1 | 7 |
| Functions | 11 | 66 |
| Interfaces | 2 | 3 |
| Modules | 2 | 2 |
| Services | 16 | 23 |
| Resources | 30 | 30 |
| **Total** | **115** | **329** |

- Chaque chemin retourné par `rg --files src/GWGUI.Emulation.Amiga src/GWGUI.Emulation.Atari` apparaît dans le tableau interne.
- Aucun chemin supplémentaire ou dupliqué n’est compté dans les totaux.
- Les 30 fichiers de ressources de chaque module sont recensés individuellement : `00-Base` et les 29 cultures.
- La colonne Amstrad ne contient encore aucun chemin ; elle ne décrit donc aucune architecture anticipée.
