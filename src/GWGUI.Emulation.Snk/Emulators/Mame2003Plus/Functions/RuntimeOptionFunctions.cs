using GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Functions;
internal static class RuntimeOptionFunctions
{
    internal const string AesBios = "asia-aes";
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        foreach (var option in configuration.Options)
            result[option.Key] = option.Value;
        result[RuntimeOptionConstants.InputInterface] = RuntimeOptionConstants.Retropad;
        result[RuntimeOptionConstants.MameRemapping] = RuntimeOptionConstants.Disabled;
        result[RuntimeOptionConstants.CoreSysSubfolder] = RuntimeOptionConstants.Enabled;
        result[RuntimeOptionConstants.CoreSaveSubfolder] = RuntimeOptionConstants.Enabled;
        if (configuration.Model == MachineModel.NeoGeoAes)
            result[RuntimeOptionConstants.NeogeoBios] = AesBios;
        var bios = result[RuntimeOptionConstants.NeogeoBios];
        if (!OptionCatalog.Options.Single(option => option.Key == RuntimeOptionConstants.NeogeoBios).Values.Any(value => value.Value == bios) || configuration.Model == MachineModel.NeoGeoMvs && bios == AesBios)
            throw new ArgumentException(nameof(configuration));
        return result;
    }

    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration)
    {
        var selected = Prepare(configuration);
        return OptionCatalog.Options.Select(option => option with
        {
            DefaultValue = option.Key == RuntimeOptionConstants.NeogeoBios && configuration.Model == MachineModel.NeoGeoAes ? AesBios : option.DefaultValue,
            CurrentValue = selected[option.Key],
            Values = option.Key == RuntimeOptionConstants.NeogeoBios
                ? option.Values.Where(value => configuration.Model == MachineModel.NeoGeoAes ? value.Value == AesBios : value.Value != AesBios).ToArray()
                : option.Values
        }).ToArray();
    }
}