using GWGUI.Emulation.Snk.Emulators.FbAlpha2012NeoGeo.Constants;
using GWGUI.Emulation.Snk.Emulators.FbAlpha2012NeoGeo.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012NeoGeo.Functions;
internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        result[RuntimeOptionConstants.NeoGeoMode] = configuration.Model == MachineModel.NeoGeoAes ? RuntimeOptionConstants.Aes : RuntimeOptionConstants.Mvs;
        foreach (var option in configuration.Options)
            result[option.Key] = option.Value;
        return result;
    }

    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration)
    {
        var options = Prepare(configuration);
        var mode = configuration.Model == MachineModel.NeoGeoAes ? RuntimeOptionConstants.Aes : RuntimeOptionConstants.Mvs;
        return OptionCatalog.Options.Select(option => option with
        {
            DefaultValue = option.Key == RuntimeOptionConstants.NeoGeoMode ? mode : option.DefaultValue,
            CurrentValue = options.GetValueOrDefault(option.Key, option.DefaultValue),
            IsEnabled = option.Key switch
            {
                RuntimeOptionConstants.FrameskipThreshold => options[RuntimeOptionConstants.Frameskip] == RuntimeOptionConstants.ManualFrameskip,
                RuntimeOptionConstants.LowpassRange => options[RuntimeOptionConstants.LowpassFilter] == RuntimeOptionConstants.FilterEnabled,
                _ => true
            },
            Values = option.Key == RuntimeOptionConstants.NeoGeoMode
                ? option.Values.Where(value => value.Value == mode || value.Value is RuntimeOptionConstants.UniverseBios or RuntimeOptionConstants.DipSwitch).ToArray()
                : option.Values
        }).ToArray();
    }
}