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

global using FrameTimer = GWGUI.Emulation.Services.EmulationFrameTimer;

global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;
global using CoreActiveInstallation = global::GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = global::GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = global::GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = global::GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationEnabledOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums.MachineModel>;
global using HostError = global::GWGUI.Emulation.Contracts.HostError;
