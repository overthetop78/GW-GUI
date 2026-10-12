namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;

internal static partial class ConfigurationFunctions
{
    internal static MachineFamily Family(MachineModel model) => model switch
    {
        MachineModel.NeoGeoAes or MachineModel.NeoGeoMvs => MachineFamily.NeoGeoCartridge,
        MachineModel.NeoGeoPocket or MachineModel.NeoGeoPocketColor => MachineFamily.NeoGeoPocket,
        MachineModel.NeoGeoCd or MachineModel.NeoGeoCdz => MachineFamily.NeoGeoCd,
        _ => throw new ArgumentOutOfRangeException(nameof(model))};
    internal static Emulator DefaultEmulator(MachineModel model) => Family(model) switch { MachineFamily.NeoGeoCartridge => Emulator.Geolith, MachineFamily.NeoGeoPocket => Emulator.MednafenNgp, MachineFamily.NeoGeoCd => Emulator.NeoCd, _ => throw new ArgumentOutOfRangeException(nameof(model)) };
    internal static bool IsCompatible(MachineModel model, Emulator core) => Enum.IsDefined(model) && Enum.IsDefined(core) && (core == Emulator.None || EmulatorCatalog.Get(core).Models.Contains(model));
    internal static int ControllerPortCount(MachineModel model) => Family(model) == MachineFamily.NeoGeoPocket ? ControllerConstants.PocketPortCount : ControllerConstants.NeoGeoPortCount;
    internal static int ControllerPortCount(MachineConfiguration configuration) => configuration.Core != Emulator.None && EmulatorCatalog.CreateAdapter(configuration.Core) is IEmulatorControllerAdapter adapter ? adapter.ControllerPortCount(configuration) : ControllerPortCount(configuration.Model);
    internal static IReadOnlyList<EmulationMediaSlot> DefaultSlots(MachineModel model) => Family(model) switch
    {
        MachineFamily.NeoGeoCartridge => [EmulationMediaSlot.Cartridge0, EmulationMediaSlot.MemoryCard0],
        MachineFamily.NeoGeoPocket => [EmulationMediaSlot.Cartridge0],
        MachineFamily.NeoGeoCd => [EmulationMediaSlot.Cd0],
        _ => throw new ArgumentOutOfRangeException(nameof(model))};
    internal static IReadOnlyList<EmulationMediaSlot> AvailableSlots(MachineModel model) => DefaultSlots(model);

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
                EmulationMediaCategory.CartridgeSlot => MediaCategory.Cartridge,
                EmulationMediaCategory.CompactDiscDrive => MediaCategory.CdRom,
                EmulationMediaCategory.MemoryCard => MediaCategory.MemoryCard,
                _ => throw new ArgumentException(nameof(configuration.Media))};
            if (media.Category != expected)
                throw new ArgumentException(nameof(configuration.Media));
        }
    
        var roles = new HashSet<FirmwareCategory>();
        foreach (var firmware in configuration.Firmwares)
            if (!FirmwareFunctions.Roles(configuration.Model, configuration.Core).Contains(firmware.Category) || !roles.Add(firmware.Category) || string.IsNullOrWhiteSpace(firmware.Path))
                throw new ArgumentException(nameof(configuration.Firmwares));
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
            if (controller.Port < ControllerConstants.FirstPort || controller.Port >= ControllerPortCount(configuration) || !ports.Add(controller.Port) || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentException(nameof(configuration.Input));
    }
}
