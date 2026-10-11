using GWGUI.Emulation.Snk.Emulators.Mame2015.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2015.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.Mame2015.Functions;

internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        foreach (var option in configuration.Options) result[option.Key] = option.Value;
        result[RuntimeOptionConstants.ReadConfig] = RuntimeOptionConstants.Disabled;
        result[RuntimeOptionConstants.MouseMode] = RuntimeOptionConstants.NoPointer;
        result[RuntimeOptionConstants.Throttle] = RuntimeOptionConstants.Disabled;
        result[RuntimeOptionConstants.BootToOsd] = RuntimeOptionConstants.Disabled;
        result[RuntimeOptionConstants.BootFromCli] = RuntimeOptionConstants.Disabled;
        return result;
    }

    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration)
    {
        var selected = Prepare(configuration);
        return OptionCatalog.Options.Select(option => option with { CurrentValue = selected[option.Key] }).ToArray();
    }
}
