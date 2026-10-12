using GWGUI.Emulation.Snk.Emulators.FbNeo.Constants;
using GWGUI.Emulation.Snk.Emulators.FbNeo.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.FbNeo.Functions;
internal static class RuntimeOptionFunctions
{
    internal static string DefaultMode(MachineModel model) => model == MachineModel.NeoGeoAes ? RuntimeOptionConstants.AesEurope : RuntimeOptionConstants.MvsEurope;
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = OptionCatalog.Options.Where(option => IsApplicable(configuration, option.Key)).ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        if (configuration.Family == MachineFamily.NeoGeoCartridge)
            result[RuntimeOptionConstants.NeogeoMode] = DefaultMode(configuration.Model);
        foreach (var option in configuration.Options)
            result[option.Key] = option.Value;
        if (configuration.Family == MachineFamily.NeoGeoCartridge && configuration.Media.Any(media => media.Slot == EmulationMediaSlot.MemoryCard0))
            result[RuntimeOptionConstants.MemcardMode] = configuration.Media.Any(media => media.Slot == EmulationMediaSlot.MemoryCard0 && media.IsInserted) ? RuntimeOptionConstants.PerGameCard : RuntimeOptionConstants.Disabled;
        return result;
    }

    internal static IReadOnlyList<CoreOption> Describe(MachineConfiguration configuration)
    {
        var selected = Prepare(configuration);
        return OptionCatalog.Options.Select(option =>
        {
            var values = option.Values;
            var defaultValue = option.DefaultValue;
            var visible = option.IsVisible && IsApplicable(configuration, option.Key);
            var enabled = option.IsEnabled;
            if (option.Key == RuntimeOptionConstants.NeogeoMode)
            {
                defaultValue = DefaultMode(configuration.Model);
                values = values.Where(value => value.Value is RuntimeOptionConstants.UniverseBios or RuntimeOptionConstants.DipSwitch || (configuration.Model == MachineModel.NeoGeoAes
                    ? value.Value is RuntimeOptionConstants.AesEurope or RuntimeOptionConstants.AesJapan
                    : value.Value is RuntimeOptionConstants.MvsEurope or RuntimeOptionConstants.MvsUsa or RuntimeOptionConstants.MvsJapan)).ToArray();
            }

            if (option.Key == RuntimeOptionConstants.MemcardMode && configuration.Media.Any(media => media.Slot == EmulationMediaSlot.MemoryCard0))
                visible = false;
            if (option.Key == RuntimeOptionConstants.FrameskipManualThreshold)
                enabled = selected.GetValueOrDefault(RuntimeOptionConstants.FrameskipType) == RuntimeOptionConstants.ManualFrameskip;
            if (option.Key == RuntimeOptionConstants.FixedFrameskip)
                enabled = selected.GetValueOrDefault(RuntimeOptionConstants.FrameskipType) == RuntimeOptionConstants.FixedFrameskipMode;
            return option with
            {
                DefaultValue = defaultValue,
                CurrentValue = selected.GetValueOrDefault(option.Key, defaultValue),
                Values = values,
                IsVisible = visible,
                IsEnabled = enabled
            };
        }).ToArray();
    }

    private static bool IsApplicable(
        MachineConfiguration configuration,
        string key
    )
    {
        if (key.StartsWith(RuntimeOptionConstants.DebugOptionPrefix, StringComparison.Ordinal))
            return false;
        if (key is RuntimeOptionConstants.LightgunCrosshairEmulation or RuntimeOptionConstants.Resolution or RuntimeOptionConstants.AnalogSpeed)
            return false;
        if (key is RuntimeOptionConstants.NeogeoMode or RuntimeOptionConstants.MemcardMode)
            return configuration.Family == MachineFamily.NeoGeoCartridge;
        if (key == RuntimeOptionConstants.Samplerate)
            return configuration.Family != MachineFamily.NeoGeoCd;
        if (key is RuntimeOptionConstants.Hiscores or RuntimeOptionConstants.DiagnosticInput)
            return configuration.Family == MachineFamily.NeoGeoCartridge;
        return true;
    }
}