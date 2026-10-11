using GWGUI.Emulation.Snk.Emulators.Mame2010.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2010.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.Mame2010.Functions;

internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        foreach (var option in configuration.Options) result[option.Key] = option.Value;
        result[RuntimeOptionConstants.CurrentXyType] = RuntimeOptionConstants.NoPointer;
        result[RuntimeOptionConstants.CurrentTurboButton] = RuntimeOptionConstants.Disabled;
        result[RuntimeOptionConstants.QsoundHle] = RuntimeOptionConstants.Disabled;
        return result;
    }

    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration)
    {
        var selected = Prepare(configuration);
        return OptionCatalog.Options.Select(option => option with { CurrentValue = selected[option.Key] }).ToArray();
    }
}
