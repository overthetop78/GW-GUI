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
global using GWGUI.Emulation.Snk.Common.Constants;
global using GWGUI.Emulation.Snk.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Snk.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Snk.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Snk.Common.Interfaces;
global using GWGUI.Emulation.Snk.Common.Services;
global using GWGUI.Emulation.Snk.Common.Enums;
global using GWGUI.Emulation.Snk.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Snk.Common.Dictionaries;
global using GWGUI.Emulation.Snk.Common.Exceptions;
global using GWGUI.Emulation.Snk.Common.Functions;



global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;

global using CoreActiveInstallation = GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOptionCategory = GWGUI.Emulation.Contracts.EmulationOptionCategory;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Snk.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Snk.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Snk.Common.Machines.Common.Enums.MachineModel>;
global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Snk.Common.Enums.ErrorCategory, GWGUI.Emulation.Snk.Common.Enums.ErrorCode>;


global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<GWGUI.Emulation.Snk.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Snk.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateControllerFingerprint = GWGUI.Emulation.Contracts.EmulationStateControllerFingerprint<GWGUI.Emulation.Snk.Common.Machines.Common.Enums.PeripheralCategory>;
global using StateFile = GWGUI.Emulation.Contracts.EmulationSavedStateFile<GWGUI.Emulation.Snk.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Snk.Common.Machines.Common.Enums.Emulator>;

global using ConfigurationDocument = GWGUI.Emulation.Contracts.EmulationConfigurationContentDocument<GWGUI.Emulation.Snk.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Snk.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Snk.Common.Machines.Common.Contracts.FirmwareConfiguration, GWGUI.Emulation.Snk.Common.Machines.Common.Contracts.MediaConfiguration, GWGUI.Emulation.Snk.Common.Machines.Common.Contracts.InputConfiguration, GWGUI.Emulation.Snk.Common.Machines.Common.Contracts.FolderConfiguration>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Snk.Common.Machines.Common.Contracts.MachineConfiguration>;
global using CoreOption = GWGUI.Emulation.Contracts.EmulationOption;
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationOptionValue;
global using FrameSkipRatio = GWGUI.Emulation.Enums.EmulationFrameSkipRatio;
