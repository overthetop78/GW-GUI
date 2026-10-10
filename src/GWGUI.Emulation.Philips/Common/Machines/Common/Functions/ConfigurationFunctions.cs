namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static partial class ConfigurationFunctions
{
    internal static MachineFamily Family(MachineModel model) => model switch
    {
        MachineModel.PhilipsCdi => MachineFamily.Cdi,
        MachineModel.P2000T => MachineFamily.P2000,
        MachineModel.Odyssey2 or MachineModel.VideopacG7000 => MachineFamily.Videopac,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static Emulator DefaultEmulator(MachineModel model) => Family(model) switch
    {
        MachineFamily.Cdi => Emulator.SameCdi,
        MachineFamily.P2000 => Emulator.M2000,
        MachineFamily.Videopac => Emulator.O2em,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static bool IsCompatible(MachineModel model, Emulator core) => Family(model) switch
    {
        MachineFamily.Cdi => core is Emulator.SameCdi,
        MachineFamily.P2000 => core == Emulator.M2000,
        MachineFamily.Videopac => core == Emulator.O2em,
        _ => false
    };

    internal static IReadOnlyList<EmulationMediaSlot> DefaultSlots(MachineModel model) => Family(model) switch
    {
        MachineFamily.Cdi => [EmulationMediaSlot.Cd0],
        MachineFamily.P2000 => [EmulationMediaSlot.Cassette0],
        MachineFamily.Videopac => [EmulationMediaSlot.Cartridge0],
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static int ControllerPortCount(MachineModel model) => Family(model) switch
    {
        MachineFamily.Cdi => ControllerConstants.CdiPortCount,
        MachineFamily.P2000 => ControllerConstants.P2000PortCount,
        MachineFamily.Videopac => ControllerConstants.VideopacPortCount,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Id == Guid.Empty
            || configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion
            || !IsCompatible(configuration.Model, configuration.Core))
            throw new ArgumentException(nameof(configuration));

        CoreOptionValidationFunctions.Validate(configuration);
        InputSettingsFunctions.Validate(configuration);

        var slot = DefaultSlots(configuration.Model)[0];
        if (configuration.ConfiguredSlots.Count != 1 || configuration.ConfiguredSlots[0] != slot
            || configuration.Media.Count > 1)
            throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        var category = configuration.Family switch
        {
            MachineFamily.Cdi => MediaCategory.CompactDisc,
            MachineFamily.P2000 => MediaCategory.Cassette,
            MachineFamily.Videopac => MediaCategory.Cartridge,
            _ => throw new ArgumentOutOfRangeException(nameof(configuration))
        };
        foreach (var media in configuration.Media)
            if (media.Slot != slot || media.Category != category || string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(nameof(configuration.Media));

        var roles = new HashSet<FirmwareCategory>();
        foreach (var firmware in configuration.Firmwares)
            if (!FirmwareFunctions.Roles(configuration.Model).Contains(firmware.Category)
                || !roles.Add(firmware.Category) || string.IsNullOrWhiteSpace(firmware.Path))
                throw new ArgumentException(nameof(configuration.Firmwares));

        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
            if (controller.Port < ControllerConstants.FirstPort
                || controller.Port >= ControllerPortCount(configuration.Model)
                || !ports.Add(controller.Port) || !Enum.IsDefined(controller.Peripheral)
                || (configuration.Family == MachineFamily.Videopac
                    && controller.Peripheral is not (PeripheralCategory.None or PeripheralCategory.Joystick))
                || (configuration.Family == MachineFamily.Cdi && controller.Peripheral == PeripheralCategory.Joystick)
                || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent
                    or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentException(nameof(configuration.Input));
    }
}
