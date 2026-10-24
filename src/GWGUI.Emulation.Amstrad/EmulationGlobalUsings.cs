global using GWGUI.Emulation.Amstrad.Common.Exceptions;
global using GWGUI.Emulation.Amstrad.Common.Enums;
global using GWGUI.Emulation.Amstrad.Common.Functions;
global using EmulatorManagementContext = GWGUI.Emulation.Contracts.EmulatorManagementContext;
global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationOptionValue;
global using PendingCommand = GWGUI.Emulation.Contracts.EmulationMachineCommand;
global using GWGUI.Emulation.Constants;
global using GWGUI.Emulation.Contracts;
global using GWGUI.Emulation.Dictionaries;
global using GWGUI.Emulation.Enums;
global using GWGUI.Emulation.Exceptions;
global using GWGUI.Emulation.Functions;
global using GWGUI.Emulation.Interfaces;
global using GWGUI.Emulation.Interop;
global using GWGUI.Emulation.Services;
global using GWGUI.Emulation.Amstrad.Common.Dictionaries;
global using GWGUI.Emulation.Amstrad.Common.Interfaces;
global using GWGUI.Emulation.Amstrad.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Amstrad.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Amstrad.Common.Machines.Common.Exceptions;
global using GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Amstrad.Modules;
global using GWGUI.Emulation.Amstrad.Common.Services;
global using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
global using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Factories;
global using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
global using GWGUI.Emulation.Amstrad.Common.Constants;
global using GWGUI.Emulation.Amstrad.Common.Machines.Common.Constants;

global using InputAccumulator = GWGUI.Emulation.Interfaces.IEmulationInputAccumulator;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationDownloadCoreRelease;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationDefinedCatalogEntry<GWGUI.Emulation.Amstrad.Common.Machines.Common.Enums.Emulator>;

global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<string, string>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Amstrad.Common.Machines.Common.Contracts.MachineConfiguration>;

global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Enums.EmulationMessageCategory, GWGUI.Emulation.Enums.EmulationMessageCode>;

global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
