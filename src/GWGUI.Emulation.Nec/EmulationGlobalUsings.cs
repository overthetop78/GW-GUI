global using EmulatorManagementContext = GWGUI.Emulation.Contracts.EmulatorManagementContext;
global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Nec.Common.Machines.Common.Contracts.MachineConfiguration>;
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
global using GWGUI.Emulation.Nec.Common.Constants;
global using GWGUI.Emulation.Nec.Common.Dictionaries;
global using GWGUI.Emulation.Nec.Common.Interfaces;
global using GWGUI.Emulation.Nec.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Nec.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Nec.Common.Machines.Common.Exceptions;
global using GWGUI.Emulation.Nec.Common.Machines.Common.Functions;
global using GWGUI.Emulation.Nec.Common.Services;

global using InputAccumulator = GWGUI.Emulation.Interfaces.IEmulationInputAccumulator;

global using CoreOption = GWGUI.Emulation.Contracts.EmulationOption;
global using CoreRelease = GWGUI.Emulation.Contracts.EmulationDownloadCoreRelease;
global using CoreReleaseSettings = GWGUI.Emulation.Contracts.EmulationCoreReleaseSettings;
global using EmulatorCatalogEntry = GWGUI.Emulation.Contracts.EmulationDefinedCatalogEntry<GWGUI.Emulation.Nec.Common.Machines.Common.Enums.Emulator>;

global using SavedStateHeader = GWGUI.Emulation.Contracts.EmulationSavedStateHeader<string, string>;

// Le catalogue utilise les métadonnées observées sans définition supplémentaire.
global using ScannedFirmware = GWGUI.Emulation.Contracts.EmulationFirmwareObservation<object>;
