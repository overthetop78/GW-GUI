using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class Pcsx2OptionFunctions
{
    internal static MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) => GraphicsSettingsFunctions.Configure(configuration with { GraphicsApi = graphicsApi }, Dictionaries.OptionCatalog.All);

    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var option in Dictionaries.OptionCatalog.All.Where(option => option.NumericRange is not null))
            if (options.TryGetValue(option.Key, out var value)) options[option.Key] = CoreSettingsFunctions.NativeNumber(option, value);
        return HardDiskRuntimeFunctions.Configure(GraphicsSettingsFunctions.Configure(configuration with { Options = options }, Dictionaries.OptionCatalog.All));
    }
}


