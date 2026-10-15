using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class Pcsx2OptionFunctions
{
    internal static MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        if (options.TryGetValue(OptionConstants.RendererKey, out var previous)
            && Dictionaries.OptionCatalog.LegacyValues[OptionConstants.RendererKey].TryGetValue(previous, out var current))
            options[OptionConstants.RendererKey] = current;
        return GraphicsSettingsFunctions.Configure(configuration with { Options = options, GraphicsApi = graphicsApi }, Dictionaries.OptionCatalog.All);
    }

    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var (key, aliases) in Dictionaries.OptionCatalog.LegacyValues)
            if (options.TryGetValue(key, out var value) && aliases.TryGetValue(value, out var current)) options[key] = current;
        foreach (var option in Dictionaries.OptionCatalog.All.Where(option => option.NumericRange is not null))
            if (options.TryGetValue(option.Key, out var value)) options[option.Key] = CoreSettingsFunctions.NativeNumber(option, value);
        return HardDiskRuntimeFunctions.Configure(GraphicsSettingsFunctions.Configure(configuration with { Options = options }, Dictionaries.OptionCatalog.All));
    }
}


