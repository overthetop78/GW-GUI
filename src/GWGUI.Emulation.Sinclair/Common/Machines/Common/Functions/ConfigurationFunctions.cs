namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;

internal static partial class ConfigurationFunctions
{
    internal static MachineFamily Family(MachineModel model) => model switch { MachineModel.Zx80 => MachineFamily.Zx80, MachineModel.Zx81 => MachineFamily.Zx81, _ when Enum.IsDefined(model) => MachineFamily.Spectrum, _ => throw new ArgumentOutOfRangeException(nameof(model)) };
    internal static Emulator DefaultEmulator(MachineModel model) => Family(model) switch { MachineFamily.Zx80 => Emulator.None, MachineFamily.Zx81 => Emulator.EightyOne, MachineFamily.Spectrum => Emulator.Fuse, _ => throw new ArgumentOutOfRangeException(nameof(model)) };
    internal static bool IsCompatible(MachineModel model, Emulator core) => Enum.IsDefined(model) && Enum.IsDefined(core) && (core == Emulator.None || core == DefaultEmulator(model));
    internal static int ControllerPortCount(MachineModel model) => Family(model) == MachineFamily.Spectrum ? ControllerConstants.SpectrumPortCount : ControllerConstants.Zx81PortCount;
    internal static IReadOnlyList<EmulationMediaSlot> DefaultSlots(MachineModel model) => model switch
    {
        MachineModel.SpectrumPlus3 or MachineModel.SpectrumPlus3E or MachineModel.Pentagon128K or MachineModel.Pentagon512K or MachineModel.Pentagon1024K or MachineModel.Scorpion256K => [EmulationMediaSlot.Cassette0, EmulationMediaSlot.Floppy0],
        _ when Enum.IsDefined(model) => [EmulationMediaSlot.Cassette0],
        _ => throw new ArgumentOutOfRangeException(nameof(model))};
    internal static IReadOnlyList<EmulationMediaSlot> AvailableSlots(MachineModel model) => model switch
    {
        MachineModel.SpectrumPlus3 or MachineModel.SpectrumPlus3E => [EmulationMediaSlot.Cassette0, EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1],
        MachineModel.Pentagon128K or MachineModel.Pentagon512K or MachineModel.Pentagon1024K or MachineModel.Scorpion256K => [EmulationMediaSlot.Cassette0, EmulationMediaSlot.Floppy0, EmulationMediaSlot.Floppy1, EmulationMediaSlot.Floppy2, EmulationMediaSlot.Floppy3],
        MachineModel.TimexTc2068 or MachineModel.TimexTs2068 => [EmulationMediaSlot.Cassette0, EmulationMediaSlot.Cartridge0],
        _ when Enum.IsDefined(model) => [EmulationMediaSlot.Cassette0],
        _ => throw new ArgumentOutOfRangeException(nameof(model))};

    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Id == Guid.Empty || configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || !IsCompatible(configuration.Model, configuration.Core))
            throw new ArgumentException(nameof(configuration));
        CoreOptionValidationFunctions.Validate(configuration);
        InputSettingsFunctions.Validate(configuration);
        var available = AvailableSlots(configuration.Model);
        if (configuration.ConfiguredSlots.Distinct().Count() != configuration.ConfiguredSlots.Count || configuration.ConfiguredSlots.Any(slot => !available.Contains(slot)))
            throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        if (configuration.Media.Select(media => media.Slot).Distinct().Count() != configuration.Media.Count)
            throw new ArgumentException(nameof(configuration.Media));
        foreach (var media in configuration.Media)
        {
            if (!configuration.ConfiguredSlots.Contains(media.Slot) || string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(nameof(configuration.Media));
            var expected = media.Slot.Category switch
            {
                EmulationMediaCategory.CassetteDrive => MediaCategory.Tape,
                EmulationMediaCategory.FloppyDrive => MediaCategory.Floppy,
                EmulationMediaCategory.CartridgeSlot => MediaCategory.Cartridge,
                _ => throw new ArgumentException(nameof(configuration.Media))};
            if (media.Category != expected)
                throw new ArgumentException(nameof(configuration.Media));
        }
    
        var roles = new HashSet<FirmwareCategory>();
        foreach (var firmware in configuration.Firmwares)
            if (!FirmwareFunctions.Roles(configuration.Model).Contains(firmware.Category) || !roles.Add(firmware.Category) || string.IsNullOrWhiteSpace(firmware.Path))
                throw new ArgumentException(nameof(configuration.Firmwares));
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
            if (controller.Port < ControllerConstants.FirstPort || controller.Port >= ControllerPortCount(configuration.Model) || !ports.Add(controller.Port) || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentException(nameof(configuration.Input));
    }
}
