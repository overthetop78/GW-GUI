namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Id == Guid.Empty || configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || !Enum.IsDefined(configuration.Model) || !Enum.IsDefined(configuration.Core)) throw new ArgumentException(nameof(configuration));
        if (configuration.ConfiguredSlots.Distinct().Count() != configuration.ConfiguredSlots.Count || configuration.ConfiguredSlots.Any(slot => slot != EmulationMediaSlot.Cartridge0)) throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        if (configuration.Media.Select(media => media.Slot).Distinct().Count() != configuration.Media.Count) throw new ArgumentException(nameof(configuration.Media));
        foreach (var media in configuration.Media) if (media.Slot != EmulationMediaSlot.Cartridge0 || media.Category != MediaCategory.Cartridge || !configuration.ConfiguredSlots.Contains(media.Slot) || string.IsNullOrWhiteSpace(media.Path)) throw new ArgumentException(nameof(configuration.Media));
        if (configuration.Options.Any(option => string.IsNullOrWhiteSpace(option.Key) || option.Value is null)) throw new ArgumentException(nameof(configuration.Options));
        InputSettingsFunctions.Validate(configuration);
        var commands = Enum.GetNames<ControllerCommand>().ToHashSet(StringComparer.Ordinal);
        var ports = new HashSet<int>();
        foreach (var controller in configuration.Input.Controllers ?? [])
        {
            if (controller.Port != ControllerConstants.FirstPort || !ports.Add(controller.Port) || controller.Peripheral != PeripheralCategory.Supervision || controller.DeadZonePercent is < ControllerConstants.MinimumDeadZonePercent or > ControllerConstants.MaximumDeadZonePercent) throw new ArgumentException(nameof(configuration.Input));
            if (controller.Mappings?.Any(pair => !commands.Contains(pair.Key) || pair.Value is null) == true) throw new ArgumentException(nameof(configuration.Input));
        }
    }
}
