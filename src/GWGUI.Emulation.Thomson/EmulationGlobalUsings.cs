global using DiskImageStatus = GWGUI.Emulation.Contracts.DiskImageStatus;
global using DiskStatus = GWGUI.Emulation.Contracts.DiskStatus;
global using EmulatorManagementContext = GWGUI.Emulation.Contracts.EmulatorManagementContext;
global using EnvironmentCallbackHost = GWGUI.Emulation.Services.ExternalCoreEnvironmentCallbackHost;
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationOptionValue;
global using VideoCallbackHost = GWGUI.Emulation.Services.ExternalCoreVideoCallbackHost;
global using AudioCallbackHost = GWGUI.Emulation.Services.ExternalCoreAudioCallbackHost;
global using MachineCommand = GWGUI.Emulation.Contracts.EmulationMachineCommand;
global using GWGUI.Emulation;
global using GWGUI.Emulation.Constants;
global using GWGUI.Emulation.Contracts;
global using GWGUI.Emulation.Dictionaries;
global using GWGUI.Emulation.Enums;
global using GWGUI.Emulation.Exceptions;
global using GWGUI.Emulation.Functions;
global using GWGUI.Emulation.Interfaces;
global using GWGUI.Emulation.Services;
global using GWGUI.Emulation.Thomson.Common.Constants;
global using GWGUI.Emulation.Thomson.Common.Contracts;
global using GWGUI.Emulation.Thomson.Common.Interfaces;
global using GWGUI.Emulation.Thomson.Common.Services;
global using GWGUI.Emulation.Thomson.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Thomson.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Thomson.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Thomson.Common.Exceptions;
global using GWGUI.Emulation.Thomson.Common.Enums;
global using GWGUI.Emulation.Thomson.Common.Functions;

global using FrameTimer = GWGUI.Emulation.Services.EmulationFrameTimer;

global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;
global using CoreActiveInstallation = GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationEnabledOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Thomson.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Thomson.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Thomson.Common.Machines.Common.Enums.MachineModel>;
global using HostError = GWGUI.Emulation.Contracts.HostError;

global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<GWGUI.Emulation.Thomson.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Thomson.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateControllerFingerprint = GWGUI.Emulation.Contracts.EmulationStateControllerFingerprint<GWGUI.Emulation.Thomson.Common.Machines.Common.Enums.PeripheralCategory>;
global using StateFile = GWGUI.Emulation.Contracts.EmulationSavedStateFile<GWGUI.Emulation.Thomson.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Thomson.Common.Machines.Common.Enums.Emulator>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts.MachineConfiguration>;
