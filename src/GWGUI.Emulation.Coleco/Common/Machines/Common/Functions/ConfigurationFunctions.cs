namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static Emulator DefaultEmulator(MachineModel model) => new Engine().DefaultEmulator(model);

    internal static IReadOnlyList<EmulationMediaSlot> DefaultSlots(MachineModel model) => model switch
    {
        MachineModel.ColecoVision => [EmulationMediaSlot.Cartridge0],
        MachineModel.Adam => [EmulationMediaSlot.Cartridge0, EmulationMediaSlot.Cassette0],
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    internal static bool IsCompatible(MachineModel model, Emulator core) => Enum.IsDefined(model) && Enum.IsDefined(core) && new Engine().Adapter(core).Definition.MachineIds.Contains(model.ToString());

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion
            || configuration.Id == Guid.Empty || !IsCompatible(configuration.Model, configuration.Core))
            throw new ArgumentException(nameof(configuration));

        var configured = configuration.ConfiguredSlots.ToHashSet();
        if (configured.Count != configuration.ConfiguredSlots.Count
            || DefaultSlots(configuration.Model).Any(slot => !configured.Contains(slot))
            || configured.Any(slot => !SupportsSlot(configuration.Model, slot)))
            throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        var mounted = new HashSet<EmulationMediaSlot>();
        foreach (var media in configuration.Media)
        {
            var category = media.Slot.Category switch
            {
                EmulationMediaCategory.CartridgeSlot => MediaCategory.Cartridge,
                EmulationMediaCategory.FloppyDrive => MediaCategory.Floppy,
                EmulationMediaCategory.CassetteDrive => MediaCategory.DataPack,
                _ => throw new ArgumentException(nameof(configuration.Media))
            };
            if (!configured.Contains(media.Slot) || !mounted.Add(media.Slot)
                || media.Category != category || string.IsNullOrWhiteSpace(media.Path))
                throw new ArgumentException(nameof(configuration.Media));
        }
        var firmware = new HashSet<FirmwareCategory>();
        foreach (var entry in configuration.Firmwares)
            if (!Enum.IsDefined(entry.Category) || !firmware.Add(entry.Category)
                || configuration.Model != MachineModel.Adam && entry.Category != FirmwareCategory.Os7
                || string.IsNullOrWhiteSpace(entry.Path))
                throw new ArgumentException(nameof(configuration.Firmwares));

        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
        {
            if (controller.Port is < ControllerConstants.FirstPort or >= ControllerConstants.PortCount
                || !ports.Add(controller.Port) || !InputSettingsFunctions.Peripherals(configuration.Core).Contains(controller.Peripheral)
                || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent
                    or > ControllerConstants.MaximumDeadZonePercent)
                throw new ArgumentException(nameof(configuration.Input));
            if (controller.Mappings?.Any(mapping => !ControllerActionConstants.All.Contains(mapping.Key)) == true)
                throw new ArgumentException(nameof(configuration.Input));
        }
        if (configuration.Input.Keyboard is { Count: > 0 } keys
            && (configuration.Model != MachineModel.Adam
                || keys.Any(mapping => !KeyboardActionConstants.All.Contains(mapping.Key) || !string.IsNullOrEmpty(mapping.Value) && (!Enum.TryParse<EmulationKey>(mapping.Value, out var key) || !Enum.IsDefined(key)))))
            throw new ArgumentException(nameof(configuration.Input));
        if (configuration.Core == Emulator.GearColeco)
        {
            var rotaryTypes = (configuration.Input.Controllers ?? []).Select(binding => binding.Peripheral).Where(peripheral => peripheral is PeripheralCategory.SuperAction or PeripheralCategory.Roller or PeripheralCategory.Driving) .Distinct().Count();
            if (rotaryTypes > 1 || configuration.Media.Where(media => media.IsInserted && media.Category != MediaCategory.Cartridge).Select(media => media.IsReadOnly).Distinct().Count() > 1)
                throw new ArgumentException(nameof(configuration));
        }
        CoreOptionValidationFunctions.Validate(configuration);
    }

    internal static bool SupportsSlot(MachineModel model, EmulationMediaSlot slot) =>
        slot == EmulationMediaSlot.Cartridge0
        || model == MachineModel.Adam && slot.Index is >= StorageConstants.FirstDriveIndex and < StorageConstants.DriveCount
            && slot.Category is EmulationMediaCategory.FloppyDrive or EmulationMediaCategory.CassetteDrive;
}
