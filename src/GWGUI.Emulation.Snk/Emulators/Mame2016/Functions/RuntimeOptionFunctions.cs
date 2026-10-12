using GWGUI.Emulation.Snk.Emulators.Mame2016.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2016.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.Mame2016.Functions;
internal static class RuntimeOptionFunctions
{
    private static readonly IReadOnlySet<string> ManagedOptions = new HashSet<string>(StringComparer.Ordinal)
    {
        RuntimeOptionConstants.ReadConfig,
        RuntimeOptionConstants.WriteConfig,
        RuntimeOptionConstants.MouseEnable,
        RuntimeOptionConstants.Throttle,
        RuntimeOptionConstants.SoftlistsEnable,
        RuntimeOptionConstants.SoftlistsAutoMedia,
        RuntimeOptionConstants.MediaType,
        RuntimeOptionConstants.BootToBios,
        RuntimeOptionConstants.BootToOsd,
        RuntimeOptionConstants.BootFromCli
    };
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        foreach (var option in configuration.Options)
            result[option.Key] = option.Value;
        foreach (var key in ManagedOptions)
            result[key] = key == RuntimeOptionConstants.MediaType ? RuntimeOptionConstants.RomMedia : RuntimeOptionConstants.Disabled;
        return result;
    }

    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration)
    {
        var selected = Prepare(configuration);
        return OptionCatalog.Options.Select(option => option with { CurrentValue = selected[option.Key] }).ToArray();
    }
}