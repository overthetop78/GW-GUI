global using GWGUI.Emulation.Atari.Common.Exceptions;
global using DiskImageStatus = GWGUI.Emulation.Contracts.DiskImageStatus;
global using DiskStatus = GWGUI.Emulation.Contracts.DiskStatus;
global using EmulatorManagementContext = GWGUI.Emulation.Contracts.EmulatorManagementContext;
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationOptionValue;
global using AudioBuffer = GWGUI.Emulation.Interfaces.IEmulationAudioBuffer;
global using MachineCommand = GWGUI.Emulation.Contracts.EmulationMachineCommand;
global using GWGUI.Emulation.Constants;
global using GWGUI.Emulation.Contracts;
global using GWGUI.Emulation.Dictionaries;
global using GWGUI.Emulation.Enums;
global using GWGUI.Emulation.Exceptions;
global using GWGUI.Emulation.Functions;
global using GWGUI.Emulation.Interfaces;
global using GWGUI.Emulation.Interop;
global using GWGUI.Emulation.Services;
global using GWGUI.Emulation.Atari.Common.Constants;
global using GWGUI.Emulation.Atari.Common.Dictionaries;
global using GWGUI.Emulation.Atari.Common.Interfaces;
global using GWGUI.Emulation.Atari.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Atari.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Atari.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Atari.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Constants;
global using GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Contracts;
global using GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Dictionaries;
global using GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Enums;
global using GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Functions;
global using GWGUI.Emulation.Atari.Common.Machines.Atari2600.Constants;
global using GWGUI.Emulation.Atari.Common.Machines.Atari2600.Dictionaries;
global using GWGUI.Emulation.Atari.Common.Machines.Atari5200.Constants;
global using GWGUI.Emulation.Atari.Common.Machines.Atari5200.Dictionaries;
global using GWGUI.Emulation.Atari.Common.Machines.Atari7800.Constants;
global using GWGUI.Emulation.Atari.Common.Machines.Atari7800.Dictionaries;
global using GWGUI.Emulation.Atari.Common.Machines.AtariJaguar.Constants;
global using GWGUI.Emulation.Atari.Common.Machines.AtariJaguar.Dictionaries;
global using GWGUI.Emulation.Atari.Common.Machines.AtariLynx.Constants;
global using GWGUI.Emulation.Atari.Common.Machines.AtariLynx.Dictionaries;
global using GWGUI.Emulation.Atari.Common.Machines.AtariST.Constants;
global using GWGUI.Emulation.Atari.Common.Machines.AtariST.Contracts;
global using GWGUI.Emulation.Atari.Common.Machines.AtariST.Dictionaries;
global using GWGUI.Emulation.Atari.Common.Machines.AtariST.Enums;
global using GWGUI.Emulation.Atari.Common.Machines.AtariST.Functions;
global using GWGUI.Emulation.Atari.Modules;
global using GWGUI.Emulation.Atari.Common.Services;
global using GWGUI.Emulation.Atari.Emulators.Common.Constants;
global using GWGUI.Emulation.Atari.Emulators.Common.Contracts;
global using GWGUI.Emulation.Atari.Common.Enums;
global using GWGUI.Emulation.Atari.Emulators.Common.Exceptions;
global using GWGUI.Emulation.Atari.Emulators.Common.Functions;
global using GWGUI.Emulation.Atari.Emulators.Common.Interfaces;

global using GWGUI.Emulation.Atari.Emulators.Common.Interop.Constants;
global using GWGUI.Emulation.Atari.Emulators.Common.Interop.Contracts;
global using GWGUI.Emulation.Atari.Emulators.Common.Interop.Dictionaries;
global using GWGUI.Emulation.Atari.Emulators.Common.Interop.Factories;
global using GWGUI.Emulation.Atari.Emulators.Common.Interop.Functions;
global using GWGUI.Emulation.Atari.Emulators.Common.Interop.Interfaces;
global using GWGUI.Emulation.Atari.Emulators.Common.Interop.Services;
global using GWGUI.Emulation.Atari.Emulators.Common.Interop.Exceptions;


global using LoadedContent = GWGUI.Emulation.Interfaces.IEmulationLoadedContent;

global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;

global using CoreActiveInstallation = GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOptionCategory = GWGUI.Emulation.Contracts.EmulationOptionCategory;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Atari.Common.Machines.Common.Enums.MachineModel>;
global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Atari.Common.Enums.ErrorCategory, GWGUI.Emulation.Atari.Common.Enums.ErrorCode>;

global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<GWGUI.Emulation.Atari.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateControllerFingerprint = GWGUI.Emulation.Contracts.EmulationStateControllerFingerprint<GWGUI.Emulation.Atari.Common.Machines.Common.Enums.PeripheralCategory>;
global using StateFile = GWGUI.Emulation.Contracts.EmulationSavedStateFile<GWGUI.Emulation.Atari.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator>;

global using ConfigurationDocument = GWGUI.Emulation.Contracts.EmulationConfigurationDocument<GWGUI.Emulation.Atari.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.FirmwareConfiguration, GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.MediaConfiguration, GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.InputConfiguration, GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.FolderConfiguration>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.MachineConfiguration>;

// Les observations partagées conservent la définition typée du catalogue du module.
global using ScannedFirmware = GWGUI.Emulation.Contracts.EmulationFirmwareObservation<GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.FirmwareDefinition>;
global using FirmwareCompatibility = GWGUI.Emulation.Enums.EmulationFirmwareCompatibility;
global using FirmwareDetectionStatus = GWGUI.Emulation.Enums.EmulationFirmwareDetectionStatus;
