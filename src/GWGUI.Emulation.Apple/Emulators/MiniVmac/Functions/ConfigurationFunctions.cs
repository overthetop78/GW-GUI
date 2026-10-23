using GWGUI.Emulation.Apple.Emulators.MiniVmac.Dictionaries;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions;

internal static class ConfigurationFunctions
{

    internal static void ValidateConfiguration(MachineConfiguration configuration)
    {
        foreach (var option in MiniVmacOptionCatalog.All)
            if (configuration.Options.TryGetValue(option.Key, out var value) && !option.Values.Any(choice => string.Equals(choice.Value, value, StringComparison.Ordinal))) throw new ArgumentException(nameof(configuration.Options));
        // The frontend accepts one initial disk and exposes no disk-control callbacks.
        if (configuration.Media.Count > 1 || configuration.Media.Any(media => media.Slot == EmulationMediaSlot.Floppy1)) throw new ArgumentException(nameof(configuration.Media));
    }
}
