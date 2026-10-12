using GWGUI.Emulation.Snk.Emulators.Mame2000.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2000.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.Mame2000.Functions;

internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        foreach (var option in configuration.Options) result[option.Key] = option.Value;
        result[RuntimeOptionConstants.QsoundOutputFilter] = RuntimeOptionConstants.Disabled;
        return result;
    }

    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration)
    {
        var selected = Prepare(configuration);
        return OptionCatalog.Options.Select(option => option with { CurrentValue = selected[option.Key], IsVisible = option.IsVisible && option.Key != RuntimeOptionConstants.QsoundOutputFilter, IsEnabled = option.Key == RuntimeOptionConstants.FrameskipThreshold
            ? selected[RuntimeOptionConstants.Frameskip] == RuntimeOptionConstants.Threshold
            : option.Key != RuntimeOptionConstants.FrameskipInterval || selected[RuntimeOptionConstants.Frameskip] != RuntimeOptionConstants.Disabled }).ToArray();
    }
}
