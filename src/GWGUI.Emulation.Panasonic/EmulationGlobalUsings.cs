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
global using GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Panasonic.Common.Constants;
global using GWGUI.Emulation.Panasonic.Common.Contracts;
global using GWGUI.Emulation.Panasonic.Common.Interfaces;
global using GWGUI.Emulation.Panasonic.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Panasonic.Common.Services;
global using GWGUI.Emulation.Panasonic.Common.Enums;
global using GWGUI.Emulation.Panasonic.Common.Exceptions;
global using GWGUI.Emulation.Panasonic.Common.Functions;
global using GWGUI.Emulation.Panasonic.Common.Machines.Common.Dictionaries;

global using FrameTimer = GWGUI.Emulation.Services.EmulationFrameTimer;

global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;
global using CoreActiveInstallation = global::GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = global::GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = global::GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = global::GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationEnabledOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.MachineModel>;
global using HostError = global::GWGUI.Emulation.Contracts.HostError;

global using SavedStateHeader = global::GWGUI.Emulation.Contracts.EmulationSavedStateHeader<global::GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.MachineModel, global::GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = global::GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateControllerFingerprint = global::GWGUI.Emulation.Contracts.EmulationStateControllerFingerprint<global::GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.PeripheralCategory>;
global using StateFile = global::GWGUI.Emulation.Contracts.EmulationSavedStateFile<global::GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.MachineModel, global::GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.Emulator>;
