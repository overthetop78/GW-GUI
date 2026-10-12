namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;

internal static class ConfigurationFunctions
{
    internal static void Validate(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Id == Guid.Empty || configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || !Enum.IsDefined(configuration.Model) || !Enum.IsDefined(configuration.Core)) throw new ArgumentException(nameof(configuration));
        if (configuration.Media.Count != 0 || configuration.ConfiguredSlots.Count != 0) throw new ArgumentException(nameof(configuration.Media));
        if (configuration.Firmware.Images.Any(image => !Enum.IsDefined(image.Key) || string.IsNullOrWhiteSpace(image.Value))) throw new ArgumentException(nameof(configuration.Firmware));
        if (configuration.Options.Any(option => string.IsNullOrWhiteSpace(option.Key) || option.Value is null)) throw new ArgumentException(nameof(configuration.Options));
        _ = KeyboardDictionary.Bindings(configuration.Input, configuration.Model);
    }
}