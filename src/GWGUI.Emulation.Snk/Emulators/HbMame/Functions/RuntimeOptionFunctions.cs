using GWGUI.Emulation.Snk.Emulators.HbMame.Constants;
using GWGUI.Emulation.Snk.Emulators.HbMame.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.HbMame.Functions;
internal static class RuntimeOptionFunctions
{
    private static readonly IReadOnlySet<string> ManagedOptions = new HashSet<string>(StringComparer.Ordinal)
    {
        RuntimeOptionConstants.ReadConfig,
        RuntimeOptionConstants.WriteConfig,
        RuntimeOptionConstants.MamePathsEnable,
        RuntimeOptionConstants.SoftlistsEnable,
        RuntimeOptionConstants.SoftlistsAutoMedia,
        RuntimeOptionConstants.MediaType,
        RuntimeOptionConstants.ButtonsProfiles,
        RuntimeOptionConstants.MouseEnable,
        RuntimeOptionConstants.BootToOsd
    };
    private static readonly IReadOnlySet<string> UnsupportedInputs = new HashSet<string>(StringComparer.Ordinal)
    {
        RuntimeOptionConstants.LightgunMode,
        RuntimeOptionConstants.LightgunOffscreenMode
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
        return OptionCatalog.Options.Select(option => option with { CurrentValue = selected[option.Key], IsVisible = option.IsVisible && !ManagedOptions.Contains(option.Key) && !UnsupportedInputs.Contains(option.Key), IsEnabled = option.Key != RuntimeOptionConstants.Altres || selected[RuntimeOptionConstants.AlternateRenderer] != RuntimeOptionConstants.Disabled }).ToArray();
    }
}