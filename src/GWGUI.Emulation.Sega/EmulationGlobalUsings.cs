global using GWGUI.Emulation.Sega.Common.Exceptions;
global using GWGUI.Emulation.Sega.Common.Enums;
global using GWGUI.Emulation.Sega.Common.Functions;
global using EmulatorManagementContext = GWGUI.Emulation.Contracts.EmulatorManagementContext;
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
global using GWGUI.Emulation.Sega.Common.Constants;
global using GWGUI.Emulation.Sega.Common.Dictionaries;
global using GWGUI.Emulation.Sega.Common.Interfaces;
global using GWGUI.Emulation.Sega.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Sega.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Sega.Common.Machines.Common.Exceptions;
global using GWGUI.Emulation.Sega.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Sega.Common.Services;

global using InputAccumulator = GWGUI.Emulation.Interfaces.IEmulationInputAccumulator;

global using CoreOptionValue = GWGUI.Emulation.Contracts.EmulationOptionValue;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationDownloadCoreRelease;
global using CoreReleaseSettings = GWGUI.Emulation.Contracts.EmulationCoreReleaseSettings;

global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<string, string>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Sega.Common.Machines.Common.Contracts.MachineConfiguration>;

global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Enums.EmulationMessageCategory, GWGUI.Emulation.Enums.EmulationMessageCode>;
