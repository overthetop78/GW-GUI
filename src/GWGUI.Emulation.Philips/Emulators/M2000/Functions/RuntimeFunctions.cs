using GWGUI.Emulation.Philips.Emulators.M2000.Constants;
using GWGUI.Emulation.Philips.Emulators.M2000.Dictionaries;

namespace GWGUI.Emulation.Philips.Emulators.M2000.Functions;

internal static class M2000RuntimeFunctions
{
    internal static IReadOnlyDictionary<string, string> Options(MachineConfiguration configuration)
    {
        var options = M2000OptionCatalog.All.Where(option => configuration.Options.ContainsKey(option.Key))
            .ToDictionary(option => option.Key, option => configuration.Options[option.Key], StringComparer.Ordinal);
        options.TryAdd(OptionConstants.KeyboardMapping, OptionConstants.Symbolic);
        return options;
    }
}
