global using DiskImageStatus = GWGUI.Emulation.Contracts.DiskImageStatus;
global using DiskStatus = GWGUI.Emulation.Contracts.DiskStatus;
global using EmulatorManagementContext = GWGUI.Emulation.Contracts.EmulatorManagementContext;
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationOptionValue;
global using VideoCallbackHost = GWGUI.Emulation.Interfaces.IEmulationVideoCallbackHost;
global using AudioCallbackHost = GWGUI.Emulation.Interfaces.IEmulationAudioCallbackHost;
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


global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;
global using CoreActiveInstallation = GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.MachineModel>;
global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Enums.EmulationMessageCategory, GWGUI.Emulation.Enums.EmulationMessageCode>;

global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateControllerFingerprint = GWGUI.Emulation.Contracts.EmulationStateControllerFingerprint<GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.PeripheralCategory>;
global using StateFile = GWGUI.Emulation.Contracts.EmulationSavedStateFile<GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.Emulator>;

global using ConfigurationDocument = GWGUI.Emulation.Contracts.EmulationConfigurationFirmwareDocument<GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts.FirmwareConfiguration, GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts.MediaConfiguration, GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts.InputConfiguration>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts.MachineConfiguration>;
