// EmulationGlobalUsings fixe les contrats et interfaces disponibles au module.
// Aucun import de GWGUI.Emulation.Functions : les composants passent par le Common du mock.
global using GWGUI.Emulation.Contracts;
global using GWGUI.Emulation.Enums;
global using GWGUI.Emulation.Interfaces;

// Les types de données, descriptions et règles appartiennent au Common du module.
global using GWGUI.Emulation;
global using GWGUI.Emulation.Example.Common.Constants;
global using GWGUI.Emulation.Example.Common.Contracts;
global using GWGUI.Emulation.Example.Common.Dictionaries;
global using GWGUI.Emulation.Example.Common.Enums;
global using GWGUI.Emulation.Example.Common.Exceptions;
global using GWGUI.Emulation.Example.Common.Interfaces;
global using GWGUI.Emulation.Example.Common.Services;
global using GWGUI.Emulation.Example.Common.Machines.Common.Constants;
global using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
global using GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;
global using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
global using GWGUI.Emulation.Example.Common.Machines.Common.Functions;

// Ces alias nomment des contrats publics existants ; ils ne créent ni classe ni appel supplémentaire.
global using ConfigurationDocument = GWGUI.Emulation.Contracts.EmulationConfigurationSlotsDocument<GWGUI.Emulation.Example.Common.Machines.Common.Enums.MachineModel, GWGUI.Emulation.Example.Common.Machines.Common.Enums.Emulator, GWGUI.Emulation.Example.Common.Machines.Common.Contracts.FirmwareConfiguration, GWGUI.Emulation.Example.Common.Machines.Common.Contracts.MediaConfiguration, GWGUI.Emulation.Example.Common.Machines.Common.Contracts.InputConfiguration, GWGUI.Emulation.Example.Common.Machines.Common.Contracts.FolderConfiguration>;
global using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Example.Common.Machines.Common.Contracts.MachineConfiguration>;
global using HostError = GWGUI.Emulation.Contracts.HostError<GWGUI.Emulation.Example.Common.Enums.ErrorCategory, GWGUI.Emulation.Example.Common.Enums.ErrorCode>;

// Les observations partagées conservent la définition typée du catalogue du module.
global using ScannedFirmware = GWGUI.Emulation.Contracts.EmulationFirmwareObservation<GWGUI.Emulation.Example.Common.Machines.Common.Contracts.FirmwareDefinition>;
global using FirmwareCompatibility = GWGUI.Emulation.Enums.EmulationFirmwareCompatibility;
global using FirmwareDetectionStatus = GWGUI.Emulation.Enums.EmulationFirmwareDetectionStatus;
