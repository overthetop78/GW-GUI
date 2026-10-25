using GWGUI.Emulation.Apple.Common.Constants;
using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Dictionaries;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static bool SupportsRuntime(MachineModel model) => new Engine().Adapters(model.ToString()).Count > 0;

    internal static Emulator DefaultEmulator(MachineModel model)
    {
        var preferred = HardwareModelCatalog.Get(model).Emulator;
        var adapters = new Engine().Adapters(model.ToString());
        var adapter = adapters.FirstOrDefault(item => string.Equals(item.EmulatorKey, preferred.ToString(), StringComparison.Ordinal)) ?? adapters.FirstOrDefault();
        return adapter is null ? preferred : Enum.Parse<Emulator>(adapter.EmulatorKey);
    }

    internal static int ControllerPortCount(MachineModel model) => model is MachineModel.AppleII or MachineModel.AppleIIPlus or MachineModel.AppleIIJPlus or MachineModel.AppleIIe or MachineModel.AppleIIeEnhanced ? 1 : 0;

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(configuration.SchemaVersion));
        if (configuration.Id == Guid.Empty || !Enum.IsDefined(configuration.Model))
            throw new ArgumentException(nameof(configuration));
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var engine = new Engine();
        var selectedAdapter = engine.SelectedAdapter(configuration);
        if (selectedAdapter is null
            ? hardware.Emulator != Emulator.None || engine.Adapters(configuration.MachineId).Count > 0
            : !selectedAdapter.Definition.MachineIds.Contains(configuration.MachineId) && (engine.Adapters(configuration.MachineId).Count > 0 || configuration.Core != hardware.Emulator))
            throw new ArgumentException(nameof(configuration.Core));
        var firmwareRoles = new HashSet<FirmwareCategory>();
        foreach (var firmware in configuration.Firmwares)
            if (!firmwareRoles.Add(firmware.Category)
                || !HardwareModelFunctions.IsFirmwareCompatible(hardware, firmware.Category)
                || string.IsNullOrWhiteSpace(firmware.Path))
                throw new ArgumentException(nameof(configuration.Firmwares));
        var slots = new HashSet<EmulationMediaSlot>();
        foreach (var media in configuration.Media)
        {
            if (!SupportsRuntime(configuration.Model) || string.IsNullOrWhiteSpace(media.Path)
                || !slots.Add(media.Slot) || !HardwareModelFunctions.IsMediaCompatible(hardware, media.Category, media.Slot)
                || media.Category == MediaCategory.Cassette)
                throw new ArgumentException(nameof(configuration.Media));
        }
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
            if (controller.Port < 0 || controller.Port >= ControllerPortCount(configuration.Model)
                || !ports.Add(controller.Port) || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent
                    or > ControllerConstants.MaximumDeadZonePercent
                || controller.Peripheral is not (PeripheralCategory.None or PeripheralCategory.Automatic
                    or PeripheralCategory.AnalogJoystick))
                throw new ArgumentException(nameof(configuration.Input));
        selectedAdapter?.ValidateConfiguration(configuration);
    }
}
