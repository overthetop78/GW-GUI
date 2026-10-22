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

global using GWGUI.Emulation.Acorn.Common.Constants;
global using GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Acorn.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Acorn.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
global using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Dictionaries;
global using GWGUI.Emulation;
global using GWGUI.Emulation.Acorn.Common.Services;
global using GWGUI.Emulation.Acorn.Common.Enums;
global using GWGUI.Emulation.Acorn.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Acorn.Common.Dictionaries;
global using GWGUI.Emulation.Acorn.Common.Interfaces;
global using GWGUI.Emulation.Acorn.Common.Exceptions;
global using GWGUI.Emulation.Acorn.Common.Functions;

global using FrameTimer = GWGUI.Emulation.Services.EmulationFrameTimer;

global using LoadedContent = GWGUI.Emulation.Services.ExternalCoreLoadedContent;

global using ExternalCoreExports = GWGUI.Emulation.Contracts.ExternalCoreExports;

global using VideoFunctions = GWGUI.Emulation.Functions.EmulationVideoFunctions;
global using CoreActiveInstallation = GWGUI.Emulation.Contracts.EmulationCoreActiveInstallation;
global using CoreDiagnosticManifest = GWGUI.Emulation.Contracts.EmulationCoreDiagnosticManifest;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;

global using CoreOptionCategory = GWGUI.Emulation.Contracts.EmulationOptionCategory;
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationResourceOptionValue;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationResourceRestartOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Acorn.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationCoreCatalogEntry<GWGUI.Emulation.Acorn.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Acorn.Common.Machines.Common.Enums.MachineModel>;
global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Acorn.Common.Enums.ErrorCategory, GWGUI.Emulation.Acorn.Common.Enums.ErrorCode>;

global using InputFrameStore = GWGUI.Emulation.Services.EmulationInputFrameStore;

global using ConfigurationDocument = GWGUI.Emulation.Contracts.EmulationConfigurationDocument<GWGUI.Emulation.Acorn.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Acorn.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts.FirmwareConfiguration, GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts.MediaConfiguration, GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts.InputConfiguration, GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts.FolderConfiguration>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts.MachineConfiguration>;
