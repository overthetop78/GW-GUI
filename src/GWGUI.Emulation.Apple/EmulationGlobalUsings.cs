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
global using GWGUI.Emulation.Services;


global using GWGUI.Emulation;
global using GWGUI.Emulation.Apple.Common.Constants;
global using GWGUI.Emulation.Apple.Common.Contracts;
global using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Apple.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Apple.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Apple.Common.Services;
global using GWGUI.Emulation.Apple.Common.Interfaces;
global using GWGUI.Emulation.Apple.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Apple.Common.Enums;
global using GWGUI.Emulation.Apple.Common.Exceptions;
global using GWGUI.Emulation.Apple.Common.Functions;

global using FrameTimer = GWGUI.Emulation.Services.EmulationFrameTimer;

global using LoadedContent = GWGUI.Emulation.Interfaces.IEmulationLoadedContent;

global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;

global using CoreActiveInstallation = GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOptionCategory = GWGUI.Emulation.Contracts.EmulationOptionCategory;
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationResourceOptionValue;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Apple.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Apple.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Apple.Common.Machines.Common.Enums.MachineModel>;
global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Apple.Common.Enums.ErrorCategory, GWGUI.Emulation.Apple.Common.Enums.ErrorCode>;

global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<GWGUI.Emulation.Apple.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Apple.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateControllerFingerprint = GWGUI.Emulation.Contracts.EmulationStateControllerFingerprint<GWGUI.Emulation.Apple.Common.Machines.Common.Enums.PeripheralCategory>;
global using StateFile = GWGUI.Emulation.Contracts.EmulationSavedStateFile<GWGUI.Emulation.Apple.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Apple.Common.Machines.Common.Enums.Emulator>;

global using ConfigurationDocument = GWGUI.Emulation.Contracts.EmulationConfigurationDocument<GWGUI.Emulation.Apple.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Apple.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Apple.Common.Machines.Common.Contracts.FirmwareConfiguration, GWGUI.Emulation.Apple.Common.Machines.Common.Contracts.MediaConfiguration, GWGUI.Emulation.Apple.Common.Machines.Common.Contracts.InputConfiguration, GWGUI.Emulation.Apple.Common.Machines.Common.Contracts.FolderConfiguration>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Apple.Common.Machines.Common.Contracts.MachineConfiguration>;
