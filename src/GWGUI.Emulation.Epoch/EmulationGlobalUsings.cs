global using DiskImageStatus = GWGUI.Emulation.Contracts.DiskImageStatus;
global using DiskStatus = GWGUI.Emulation.Contracts.DiskStatus;
global using EmulatorManagementContext = GWGUI.Emulation.Contracts.EmulatorManagementContext;
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
global using GWGUI.Emulation;
global using GWGUI.Emulation.Epoch.Common.Constants;
global using GWGUI.Emulation.Epoch.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Epoch.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Epoch.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Epoch.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Epoch.Common.Contracts;
global using GWGUI.Emulation.Epoch.Common.Dictionaries;
global using GWGUI.Emulation.Epoch.Common.Interfaces;
global using GWGUI.Emulation.Epoch.Common.Services;
global using GWGUI.Emulation.Epoch.Common.Enums;
global using GWGUI.Emulation.Epoch.Common.Exceptions;
global using GWGUI.Emulation.Epoch.Common.Functions;

global using FrameTimer = GWGUI.Emulation.Services.EmulationFrameTimer;

global using LoadedContent = GWGUI.Emulation.Interfaces.IEmulationLoadedContent;

global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;

global using VideoFunctions = GWGUI.Emulation.Functions.EmulationVideoFunctions;
global using CoreActiveInstallation = GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOptionCategory = GWGUI.Emulation.Contracts.EmulationOptionCategory;
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationTimingOptionValue;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationTimingOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.MachineModel>;
global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Epoch.Common.Enums.ErrorCategory, GWGUI.Emulation.Epoch.Common.Enums.ErrorCode>;

global using InputFrameStore = GWGUI.Emulation.Services.EmulationInputFrameStore;

global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateControllerFingerprint = GWGUI.Emulation.Contracts.EmulationStateControllerFingerprint<GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.PeripheralCategory>;
global using StateFile = GWGUI.Emulation.Contracts.EmulationSavedStateFile<GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.Emulator>;

global using ConfigurationDocument = GWGUI.Emulation.Contracts.EmulationConfigurationSlotsDocument<GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Epoch.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Epoch.Common.Machines.Common.Contracts.FirmwareConfiguration, GWGUI.Emulation.Epoch.Common.Machines.Common.Contracts.MediaConfiguration, GWGUI.Emulation.Epoch.Common.Machines.Common.Contracts.InputConfiguration, GWGUI.Emulation.Epoch.Common.Machines.Common.Contracts.FolderConfiguration>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Epoch.Common.Machines.Common.Contracts.MachineConfiguration>;
