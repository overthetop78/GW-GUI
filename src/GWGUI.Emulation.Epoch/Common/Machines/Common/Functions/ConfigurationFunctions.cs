namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;

internal static partial class ConfigurationFunctions
{
    internal static Emulator DefaultEmulator(MachineModel model) => new Engine().DefaultEmulator(model);

    internal static IReadOnlyList<EmulationMediaSlot> DefaultSlots(MachineModel model)
    {
        _ = DefaultEmulator(model);
        return [EmulationMediaSlot.Cartridge0];
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Id == Guid.Empty
            || configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion
            || !Enum.IsDefined(configuration.Model) || !Enum.IsDefined(configuration.Core)
            || !new Engine().Adapter(configuration.Core).Definition.MachineIds.Contains(configuration.MachineId))
            throw new ArgumentException(nameof(configuration));
        if (configuration.ConfiguredSlots.Count != 1
            || configuration.ConfiguredSlots[0] != EmulationMediaSlot.Cartridge0
            || configuration.Media.Count > 1
            || configuration.Media.Any(media => media.Slot != EmulationMediaSlot.Cartridge0 || media.Category != MediaCategory.Cartridge || string.IsNullOrWhiteSpace(media.Path)))
            throw new ArgumentException(nameof(configuration.Media));
        if (configuration.Firmwares.Count > 1
            || configuration.Firmwares.Any(firmware => configuration.Core != Emulator.EmuSCV || firmware.Category != FirmwareCategory.Console || string.IsNullOrWhiteSpace(firmware.Path)))
            throw new ArgumentException(nameof(configuration.Firmwares));
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
            if (controller.Port is < ControllerConstants.FirstPort or >= ControllerConstants.PortCount
                || !ports.Add(controller.Port) || controller.Peripheral != PeripheralCategory.Standard
                || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent
                    or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentException(nameof(configuration.Input));
        InputSettingsFunctions.Validate(configuration);
        CoreOptionValidationFunctions.Validate(configuration);
    }
}
