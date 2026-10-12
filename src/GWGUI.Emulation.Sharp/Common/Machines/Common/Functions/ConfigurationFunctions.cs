namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;
internal static partial class ConfigurationFunctions
{
    internal static MachineFamily Family(MachineModel model) => model switch
    {
        MachineModel.X68000 or MachineModel.X68000Xvi or MachineModel.X68030 => MachineFamily.X68000,
        MachineModel.X1 or MachineModel.X1Turbo or MachineModel.X1TurboZ => MachineFamily.X1,
        _ => throw new ArgumentOutOfRangeException(nameof(model))};
    internal static Emulator DefaultEmulator(MachineModel model) => model switch
    {
        MachineModel.X68000 or MachineModel.X68000Xvi => Emulator.Px68k,
        MachineModel.X68030 => Emulator.None,
        MachineModel.X1 or MachineModel.X1Turbo or MachineModel.X1TurboZ => Emulator.X1,
        _ => throw new ArgumentOutOfRangeException(nameof(model))};
    internal static bool IsCompatible(MachineModel model, Emulator core) => Enum.IsDefined(model) && Enum.IsDefined(core) && (core == Emulator.None || core == DefaultEmulator(model));
    internal static IReadOnlyList<EmulationMediaSlot> DefaultSlots(MachineModel model) => Family(model) == MachineFamily.X68000 ? [EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1] : [EmulationMediaSlot.Floppy0];
    internal static IReadOnlyList<EmulationMediaSlot> AvailableSlots(MachineModel model) => Family(model) == MachineFamily.X68000 ? [EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1, EmulationMediaSlot.HardDisk0] : [EmulationMediaSlot.Floppy0];
    internal static int ControllerPortCount(MachineModel model) => Family(model) == MachineFamily.X68000 ? ControllerConstants.X68000PortCount : ControllerConstants.X1PortCount;
    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Id == Guid.Empty || configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || !IsCompatible(configuration.Model, configuration.Core))
            throw new ArgumentException(nameof(configuration));
        CoreOptionValidationFunctions.Validate(configuration);
        InputSettingsFunctions.Validate(configuration);
        var availableSlots = AvailableSlots(configuration.Model);
        if (configuration.ConfiguredSlots.Distinct().Count() != configuration.ConfiguredSlots.Count || configuration.ConfiguredSlots.Any(slot => !availableSlots.Contains(slot)))
            throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        if (configuration.Media.Select(media => media.Slot).Distinct().Count() != configuration.Media.Count)
            throw new ArgumentException(nameof(configuration.Media));
        foreach (var media in configuration.Media)
        {
            if (!configuration.ConfiguredSlots.Contains(media.Slot) || string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(nameof(configuration.Media));
            var category = media.Slot == EmulationMediaSlot.HardDisk0 ? MediaCategory.HardDisk : MediaCategory.Floppy;
            if (media.Category != category)
                throw new ArgumentException(nameof(configuration.Media));
        }

        var roles = new HashSet<FirmwareCategory>();
        foreach (var firmware in configuration.Firmwares)
        {
            if (!FirmwareFunctions.Roles(configuration.Model).Contains(firmware.Category) || !roles.Add(firmware.Category) || string.IsNullOrWhiteSpace(firmware.Path))
                throw new ArgumentException(nameof(configuration.Firmwares));
        }

        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
        {
            if (controller.Port < ControllerConstants.FirstPort || controller.Port >= ControllerPortCount(configuration.Model) || !ports.Add(controller.Port) || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentException(nameof(configuration.Input));
        }
    }
}