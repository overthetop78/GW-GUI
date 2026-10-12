namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static partial class ConfigurationFunctions
{
    internal static MachineFamily Family(MachineModel model) => model == MachineModel.Intellivision ? MachineFamily.Intellivision : throw new ArgumentOutOfRangeException(nameof(model));
    internal static Emulator DefaultEmulator(MachineModel model) { _ = Family(model); return Emulator.FreeIntv; }
    internal static bool IsCompatible(MachineModel model, Emulator core) => model == MachineModel.Intellivision && core == Emulator.FreeIntv;
    internal static IReadOnlyList<EmulationMediaSlot> DefaultSlots(MachineModel model) { _ = Family(model); return [EmulationMediaSlot.Cartridge0]; }
    internal static int ControllerPortCount(MachineModel model) { _ = Family(model); return ControllerConstants.PortCount; }

    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Id == Guid.Empty || configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion
            || !IsCompatible(configuration.Model, configuration.Core)) throw new ArgumentException(nameof(configuration));
        CoreOptionValidationFunctions.Validate(configuration);
        InputSettingsFunctions.Validate(configuration);
        var slot = DefaultSlots(configuration.Model).Single();
        if (configuration.ConfiguredSlots.Count != 1 || configuration.ConfiguredSlots[0] != slot || configuration.Media.Count > 1)
            throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        foreach (var media in configuration.Media)
            if (media.Slot != slot || media.Category != MediaCategory.Cartridge || string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(nameof(configuration.Media));
        var roles = new HashSet<FirmwareCategory>();
        foreach (var firmware in configuration.Firmwares)
            if (!FirmwareFunctions.Roles(configuration.Model).Contains(firmware.Category) || !roles.Add(firmware.Category)
                || string.IsNullOrWhiteSpace(firmware.Path)) throw new ArgumentException(nameof(configuration.Firmwares));
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
            if (controller.Port < ControllerConstants.FirstPort || controller.Port >= ControllerPortCount(configuration.Model)
                || !ports.Add(controller.Port) || controller.Peripheral != PeripheralCategory.Intellivision
                || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentException(nameof(configuration.Input));
    }
}
