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
global using GWGUI.Emulation;
global using GWGUI.Emulation.Mattel.Common.Constants;
global using GWGUI.Emulation.Mattel.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Mattel.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Mattel.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Mattel.Common.Contracts;
global using GWGUI.Emulation.Mattel.Common.Dictionaries;
global using GWGUI.Emulation.Mattel.Common.Interfaces;
global using GWGUI.Emulation.Mattel.Common.Services;
global using GWGUI.Emulation.Mattel.Common.Machines.Common.Constants;

global using GWGUI.Emulation.Mattel.Common.Enums;

global using GWGUI.Emulation.Mattel.Common.Exceptions;

global using GWGUI.Emulation.Mattel.Common.Functions;

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
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationTimingOptionValue;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationTimingOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.MachineModel>;
global using HostError = global::GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Mattel.Common.Enums.ErrorCategory, GWGUI.Emulation.Mattel.Common.Enums.ErrorCode>;

global using InputFrameStore = GWGUI.Emulation.Services.EmulationInputFrameStore;

global using SavedStateHeader = global::GWGUI.Emulation.Contracts.EmulationSavedStateHeader<global::GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.MachineModel, global::GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = global::GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateControllerFingerprint = global::GWGUI.Emulation.Contracts.EmulationStateControllerFingerprint<global::GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.PeripheralCategory>;
global using StateFile = global::GWGUI.Emulation.Contracts.EmulationSavedStateFile<global::GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.MachineModel, global::GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.Emulator>;

global using ConfigurationDocument = global::GWGUI.Emulation.Contracts.EmulationConfigurationSlotsDocument<global::GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.MachineModel, global::GWGUI.Emulation.Mattel.Common.Machines.Common.Enums.Emulator, global::GWGUI.Emulation.Mattel.Common.Machines.Common.Contracts.FirmwareConfiguration, global::GWGUI.Emulation.Mattel.Common.Machines.Common.Contracts.MediaConfiguration, global::GWGUI.Emulation.Mattel.Common.Machines.Common.Contracts.InputConfiguration, global::GWGUI.Emulation.Mattel.Common.Machines.Common.Contracts.FolderConfiguration>;
