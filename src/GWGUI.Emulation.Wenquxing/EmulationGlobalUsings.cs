global using DiskImageStatus = GWGUI.Emulation.Contracts.DiskImageStatus;
global using DiskStatus = GWGUI.Emulation.Contracts.DiskStatus;
global using EmulatorManagementContext = GWGUI.Emulation.Contracts.EmulatorManagementContext;
global using EnvironmentCallbackHost = GWGUI.Emulation.Interfaces.IEmulationEnvironmentCallbackHost;
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
global using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Wenquxing.Common.Constants;
global using GWGUI.Emulation.Wenquxing.Common.Contracts;
global using GWGUI.Emulation.Wenquxing.Common.Interfaces;
global using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Wenquxing.Common.Services;
global using GWGUI.Emulation.Wenquxing.Common.Enums;
global using GWGUI.Emulation.Wenquxing.Common.Exceptions;
global using GWGUI.Emulation.Wenquxing.Common.Functions;
global using KeyboardDictionary = GWGUI.Emulation.Wenquxing.Common.Machines.ElectronicDictionaries.Dictionaries.KeyboardDictionary;
global using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Dictionaries;


global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;
global using CoreActiveInstallation = GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationEnabledOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.MachineModel>;
global using HostError = GWGUI.Emulation.Contracts.HostError;

global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.Emulator>;
global using StateContentEntry = GWGUI.Emulation.Contracts.EmulationStateContentEntry;
global using StateFile = GWGUI.Emulation.Contracts.EmulationSavedStateFile<GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.Emulator>;

global using ConfigurationDocument = GWGUI.Emulation.Contracts.EmulationConfigurationFirmwareDocument<GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts.FirmwareConfiguration, GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts.MediaConfiguration, GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts.InputConfiguration>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts.MachineConfiguration>;
