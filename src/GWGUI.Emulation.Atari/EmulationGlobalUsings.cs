global using DiskImageStatus = GWGUI.Emulation.Contracts.DiskImageStatus;
global using DiskStatus = GWGUI.Emulation.Contracts.DiskStatus;
global using EmulatorManagementContext = GWGUI.Emulation.Contracts.EmulatorManagementContext;
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationOptionValue;
global using AudioBuffer = GWGUI.Emulation.Services.EmulationAudioBuffer;
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
global using GWGUI.Emulation.Atari.Common.Contracts;
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
global using GWGUI.Emulation.Atari.Emulators.Common.Enums;
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

global using FrameTimer = GWGUI.Emulation.Services.EmulationFrameTimer;

global using LoadedContent = GWGUI.Emulation.Services.ExternalCoreLoadedContent;

global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;

global using VideoFunctions = GWGUI.Emulation.Functions.EmulationVideoFunctions;
global using NativeAudioFunctions = GWGUI.Emulation.Functions.EmulationAudioFunctions;
global using CoreActiveInstallation = global::GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = global::GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = global::GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = global::GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOptionCategory = GWGUI.Emulation.Contracts.EmulationOptionCategory;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationCategorizedOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Atari.Common.Machines.Common.Enums.MachineModel>;
global using HostError = global::GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Atari.Emulators.Common.Enums.ErrorCategory, GWGUI.Emulation.Atari.Emulators.Common.Enums.ErrorCode>;

global using SavedStateHeader = global::GWGUI.Emulation.Contracts.EmulationSavedStateHeader<global::GWGUI.Emulation.Atari.Common.Machines.Common.Enums.MachineModel, global::GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = global::GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateControllerFingerprint = global::GWGUI.Emulation.Contracts.EmulationStateControllerFingerprint<global::GWGUI.Emulation.Atari.Common.Machines.Common.Enums.PeripheralCategory>;
global using StateFile = global::GWGUI.Emulation.Contracts.EmulationSavedStateFile<global::GWGUI.Emulation.Atari.Common.Machines.Common.Enums.MachineModel, global::GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator>;

global using ConfigurationDocument = global::GWGUI.Emulation.Contracts.EmulationConfigurationDocument<global::GWGUI.Emulation.Atari.Common.Machines.Common.Enums.MachineModel, global::GWGUI.Emulation.Atari.Common.Machines.Common.Enums.Emulator, global::GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.FirmwareConfiguration, global::GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.MediaConfiguration, global::GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.InputConfiguration, global::GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.FolderConfiguration>;
