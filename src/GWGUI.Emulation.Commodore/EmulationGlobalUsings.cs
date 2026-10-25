global using GWGUI.Emulation.Commodore.Common.Exceptions;
global using GWGUI.Emulation.Commodore.Common.Enums;
global using GWGUI.Emulation.Commodore.Common.Functions;
global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<string, string>;
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
global using GWGUI.Emulation.Commodore.Common.Constants;
global using GWGUI.Emulation.Commodore.Common.Dictionaries;
global using GWGUI.Emulation.Commodore.Common.Interfaces;
global using GWGUI.Emulation.Commodore.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Commodore.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Commodore.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Commodore.Common.Machines.Common.Exceptions;
global using GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Commodore.Modules;
global using GWGUI.Emulation.Commodore.Common.Services;
global using GWGUI.Emulation.Commodore.Emulators.Common.Interop.Contracts;
global using GWGUI.Emulation.Commodore.Emulators.Common.Interop.Enums;

global using GWGUI.Emulation.Commodore.Emulators.Common.Interop.Constants;
global using GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;
global using GWGUI.Emulation.Commodore.Emulators.Common.Interop.Factories;
global using GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Contracts;
global using GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants;
global using GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;
global using AmigaFirmwareCatalog = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Dictionaries.AmigaFirmwareCatalog;
global using AmigaSettingsHelpDictionary = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Dictionaries.AmigaSettingsHelpDictionary;
global using GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Enums;
global using GWGUI.Emulation.Commodore.Emulators.UAE.Common.Interop.Factories;
global using GWGUI.Emulation.Commodore.Emulators.UAE.Common.Interop.Functions;
global using GWGUI.Emulation.Commodore.Emulators.UAE.Common.Interop.Constants;

global using InputAccumulator = GWGUI.Emulation.Interfaces.IEmulationInputAccumulator;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationCatalogCoreRelease<GWGUI.Emulation.Commodore.Common.Machines.Common.Enums.Emulator>;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationDefinedCatalogEntry<GWGUI.Emulation.Commodore.Common.Machines.Common.Enums.Emulator>;

global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts.MachineConfiguration>;

global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Enums.EmulationMessageCategory, GWGUI.Emulation.Enums.EmulationMessageCode>;

global using CoreInstallProgress = GWGUI.Emulation.Contracts.EmulationCoreInstallProgress;
global using CoreInstallationPaths = GWGUI.Emulation.Contracts.EmulationCoreInstallationPaths;
