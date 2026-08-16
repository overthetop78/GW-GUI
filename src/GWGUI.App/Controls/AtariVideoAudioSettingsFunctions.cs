using System.Globalization;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariVideoAudioSettingsFunctions
{
    internal static AtariVideoAudioView Create(AtariMachineConfiguration configuration)
    {
        var compatibility = AtariCompatibilityCatalog.Get(configuration.Model);
        var standards = Standards(configuration.Model);
        return new AtariVideoAudioView(
            standards,
            Regions(configuration.Model),
            Choices((AtariVideoAudioSettingsConstants.AutomaticValue, L(AtariVideoAudioSettingsConstants.AutomaticResource)),
                (AtariVideoAudioSettingsConstants.NativeValue, AtariVideoAudioSettingsConstants.NativeValue)),
            Choices((AtariVideoAudioSettingsConstants.AutomaticValue, L(AtariVideoAudioSettingsConstants.AutomaticResource)),
                (AtariVideoAudioSettingsConstants.FourByThreeValue, AtariVideoAudioSettingsConstants.FourByThreeValue),
                (AtariVideoAudioSettingsConstants.PixelAspectValue, AtariVideoAudioSettingsConstants.PixelAspectValue)),
            ToggleChoices(), NumericChoices(AtariVideoAudioSettingsConstants.MinimumFrameSkip,
                AtariVideoAudioSettingsConstants.MaximumFrameSkip, AtariVideoAudioSettingsConstants.FrameSkipStep),
            EmulationOptionCatalog.VideoRenderers().Select(value =>
                new AtariVideoAudioChoice(value.Renderer.ToString(), value.Label)).ToArray(),
            [new(AtariVideoAudioSettingsConstants.DefaultOutputValue,
                L(AtariVideoAudioSettingsConstants.DefaultAudioOutputResource))],
            NumericChoices(AtariVideoAudioSettingsConstants.MinimumLatencyMilliseconds,
                AtariVideoAudioSettingsConstants.MaximumLatencyMilliseconds,
                AtariVideoAudioSettingsConstants.LatencyStepMilliseconds),
            NumericChoices(AtariVideoAudioSettingsConstants.MinimumVolumePercent,
                AtariVideoAudioSettingsConstants.MaximumVolumePercent,
                AtariVideoAudioSettingsConstants.VolumeStepPercent),
            Choices((AtariVideoAudioSettingsConstants.LowQualityValue, AtariVideoAudioSettingsConstants.LowQualityValue),
                (AtariVideoAudioSettingsConstants.NormalQualityValue, AtariVideoAudioSettingsConstants.NormalQualityValue),
                (AtariVideoAudioSettingsConstants.HighQualityValue, AtariVideoAudioSettingsConstants.HighQualityValue)),
            compatibility.Options.Single(value => value.Option == AtariSettingOption.AudioEnabled).Availability
                != AtariOptionAvailability.Unavailable && configuration.AudioEnabled,
            configuration.VideoRenderer);
    }

    internal static AtariMachineConfiguration Apply(AtariMachineConfiguration source,
        IEnumerable<KeyValuePair<string, string>> displayed, bool audioEnabled, EmulationVideoRenderer renderer)
    {
        var options = AtariGeneralSettingsFunctions.MergeOptions(source.Options, displayed);
        return new AtariMachineConfiguration(source.Model, source.Firmwares, source.Media, options, source.Input,
            source.Id, source.SchemaVersion, audioEnabled, renderer, source.Folders);
    }

    internal static string Select(IReadOnlyDictionary<string, string> options, string key,
        IReadOnlyList<AtariVideoAudioChoice> choices, string fallback)
    {
        if (options.TryGetValue(key, out var configured) && choices.Any(value => value.Value == configured))
            return configured;
        return choices.Any(value => value.Value == fallback) ? fallback : choices.First().Value;
    }

    internal static string PreferredRegion(AtariMachineModel model, IReadOnlyList<AtariVideoAudioChoice> choices)
    {
        if (AtariCompatibilityCatalog.Get(model).Core != AtariCoreKind.Hatari) return choices.First().Value;
        var region = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "en" => AtariStRegion.UnitedKingdom,
            "de" => AtariStRegion.Germany,
            "fr" => AtariStRegion.France,
            "es" => AtariStRegion.Spain,
            "it" => AtariStRegion.Italy,
            "sv" => AtariStRegion.Sweden,
            "fi" => AtariStRegion.Finland,
            "nb" or "no" => AtariStRegion.Norway,
            "cs" => AtariStRegion.CzechRepublic,
            "ru" => AtariStRegion.Russia,
            "el" => AtariStRegion.Greece,
            _ => AtariStRegion.Multilingual
        };
        var value = region.ToString();
        return choices.Any(choice => choice.Value == value)
            ? value : AtariStRegion.Multilingual.ToString();
    }

    private static IReadOnlyList<AtariVideoAudioChoice> Standards(AtariMachineModel model)
    {
        if (AtariCompatibilityCatalog.Get(model).Core == AtariCoreKind.Hatari)
        {
            var video = AtariStModelCatalog.Get(model).Video;
            var values = new List<AtariVideoAudioChoice>();
            if (video.Contains(AtariStVideoCapability.Pal)) values.Add(Official(AtariVideoAudioSettingsConstants.PalValue));
            if (video.Contains(AtariStVideoCapability.Ntsc)) values.Add(Official(AtariVideoAudioSettingsConstants.NtscValue));
            if (video.Contains(AtariStVideoCapability.Monochrome)) values.Add(Official(AtariVideoAudioSettingsConstants.MonochromeValue));
            return values;
        }
        return AtariClassicModelCatalog.Get(model).Regions.Select(value => Official(value.ToString())).ToArray();
    }

    private static IReadOnlyList<AtariVideoAudioChoice> Regions(AtariMachineModel model) =>
        AtariHardwareSettingsFunctions.Create(model, new Dictionary<string, string>()).Regions
            .Select(value => new AtariVideoAudioChoice(value.Value, value.DisplayName)).ToArray();

    private static IReadOnlyList<AtariVideoAudioChoice> ToggleChoices() =>
        Choices((AtariVideoAudioSettingsConstants.DisabledValue, L(AtariVideoAudioSettingsConstants.DisabledResource)),
            (AtariVideoAudioSettingsConstants.EnabledValue, L(AtariVideoAudioSettingsConstants.EnabledResource)));

    private static IReadOnlyList<AtariVideoAudioChoice> NumericChoices(int minimum, int maximum, int step) =>
        Enumerable.Range(minimum, (maximum - minimum) / step + AtariVideoAudioSettingsConstants.FrameSkipStep)
            .Select(index => minimum + index * step)
            .Select(value => Official(value.ToString(CultureInfo.InvariantCulture))).ToArray();

    private static IReadOnlyList<AtariVideoAudioChoice> Choices(params (string Value, string Label)[] values) =>
        values.Select(value => new AtariVideoAudioChoice(value.Value, value.Label)).ToArray();
    private static AtariVideoAudioChoice Official(string value) => new(value, value);
    private static string L(string resource) => LocExtension.Get(resource);
}
