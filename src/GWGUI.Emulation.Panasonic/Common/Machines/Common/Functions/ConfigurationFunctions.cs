namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Id == Guid.Empty || configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || !Enum.IsDefined(configuration.Model) || !Enum.IsDefined(configuration.Core))
            throw new ArgumentException(nameof(configuration));
        if (!configuration.ConfiguredSlots.SequenceEqual(new[] { EmulationMediaSlot.Cd0 }) || configuration.ConfiguredSlots.Distinct().Count() != configuration.ConfiguredSlots.Count || configuration.ConfiguredSlots.Any(slot => slot != EmulationMediaSlot.Cd0))
            throw new ArgumentException(nameof(configuration.ConfiguredSlots));
        if (configuration.Media.GroupBy(media => media.Slot).Any(group => group.Count() > 1) || configuration.Media.Any(media => media.Category != EmulationMediaType.CompactDisc || media.Slot != EmulationMediaSlot.Cd0 || !configuration.ConfiguredSlots.Contains(media.Slot) || string.IsNullOrWhiteSpace(media.Path)))
            throw new ArgumentException(nameof(configuration.Media));
        if (configuration.Firmware.Images.Any(image => !Enum.IsDefined(image.Key) || string.IsNullOrWhiteSpace(image.Value)))
            throw new ArgumentException(nameof(configuration.Firmware));
        if (configuration.Options.Any(option => string.IsNullOrWhiteSpace(option.Key) || option.Value is null))
            throw new ArgumentException(nameof(configuration.Options));
        InputSettingsFunctions.Validate(configuration);
    }
}
