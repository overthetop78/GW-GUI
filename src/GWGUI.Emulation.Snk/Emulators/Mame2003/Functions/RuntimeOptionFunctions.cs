using GWGUI.Emulation.Snk.Emulators.Mame2003.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2003.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.Mame2003.Functions;

internal static class RuntimeOptionFunctions
{
    private static readonly IReadOnlySet<string> SupportedBios = new HashSet<string>(StringComparer.Ordinal) { "default", "euro", "euro-s1", "us", "us-e", "asia", "japan", "japan-s2" };

    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        foreach (var option in configuration.Options) result[option.Key] = option.Value;
        result[RuntimeOptionConstants.InputInterface] = RuntimeOptionConstants.Retropad;
        result[RuntimeOptionConstants.MameRemapping] = RuntimeOptionConstants.Disabled;
        result[RuntimeOptionConstants.CoreSysSubfolder] = RuntimeOptionConstants.Enabled;
        result[RuntimeOptionConstants.CoreSaveSubfolder] = RuntimeOptionConstants.Enabled;
        result[RuntimeOptionConstants.RstickToBtns] = RuntimeOptionConstants.Disabled;
        if (!SupportedBios.Contains(result[RuntimeOptionConstants.NeogeoBios])) throw new ArgumentException(nameof(configuration));
        return result;
    }

    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration)
    {
        var selected = Prepare(configuration);
        return OptionCatalog.Options.Select(option => option with { CurrentValue = selected[option.Key], Values = option.Key == RuntimeOptionConstants.NeogeoBios ? option.Values.Where(value => SupportedBios.Contains(value.Value)).ToArray() : option.Values }).ToArray();
    }
}
