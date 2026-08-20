using System.Globalization;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariVideoAudioSettingsFunctions
{
    internal static AtariVideoAudioView Create(AtariMachineConfiguration configuration,
        IReadOnlyList<AtariVideoAudioChoice>? audioOutputs = null)
    {
        var compatibility = AtariCompatibilityCatalog.Get(configuration.Model);
        var standards = Standards(configuration.Model);
        return new AtariVideoAudioView(
            standards,
            Regions(configuration.Model),
            Resolutions(configuration.Model),
            ArtifactingModes(configuration.Model),
            DecimalChoices(-1.0m, 1.0m, 0.05m),
            DecimalChoices(-1.0m, 1.0m, 0.05m),
            DecimalChoices(-2.0m, 2.0m, 0.05m),
            DecimalChoices(-2.0m, 2.0m, 0.05m),
            DecimalChoices(1.0m, 3.5m, 0.05m),
            [new(AtariEightBitSettingsConstants.DefaultColorDelay,
                    L(AtariVideoAudioSettingsConstants.AutomaticResource)),
                ..DecimalChoices(10.0m, 50.0m, 0.5m, "°")],
            Choices(("none", L(AtariHardwareSettingsConstants.NoneResource)),
                ("default", L(AtariVideoAudioSettingsConstants.AutomaticResource)),
                ("gray", "Gray"), ("jakub", "Jakub"), ("real", "Real"), ("xformer", "Xformer")),
            Choices((AtariVideoAudioSettingsConstants.AutomaticValue, L(AtariVideoAudioSettingsConstants.AutomaticResource)),
                (AtariVideoAudioSettingsConstants.FourByThreeValue, AtariVideoAudioSettingsConstants.FourByThreeValue),
                (AtariVideoAudioSettingsConstants.PixelAspectValue, AtariVideoAudioSettingsConstants.PixelAspectValue)),
            ToggleChoices(), NumericChoices(AtariVideoAudioSettingsConstants.MinimumFrameSkip,
                AtariVideoAudioSettingsConstants.MaximumFrameSkip, AtariVideoAudioSettingsConstants.FrameSkipStep)
                .Append(Official("10")).ToArray(),
            EmulationOptionCatalog.VideoRenderers().Select(value =>
                new AtariVideoAudioChoice(value.Renderer.ToString(), value.Label)).ToArray(),
            audioOutputs ?? [new(AtariVideoAudioSettingsConstants.DefaultOutputValue,
                L(AtariVideoAudioSettingsConstants.DefaultAudioOutputResource))],
            new[] { 20, 35, 50, 75, 100, 150, 250 }.Select(value =>
                new AtariVideoAudioChoice(value.ToString(CultureInfo.InvariantCulture), $"{value} ms")).ToArray(),
            NumericChoices(AtariVideoAudioSettingsConstants.MinimumVolumePercent,
                AtariVideoAudioSettingsConstants.MaximumVolumePercent,
                AtariVideoAudioSettingsConstants.VolumeStepPercent, " %"),
            Choices((AtariVideoAudioSettingsConstants.LowQualityValue,
                    L(AtariVideoAudioSettingsConstants.AudioQualityLowResource)),
                (AtariVideoAudioSettingsConstants.NormalQualityValue,
                    L(AtariVideoAudioSettingsConstants.AudioQualityNormalResource)),
                (AtariVideoAudioSettingsConstants.HighQualityValue,
                    L(AtariVideoAudioSettingsConstants.AudioQualityHighResource))),
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
        if (UsesApplicationCultureForVideoStandard(model))
            return PreferredVideoStandard(model, choices);
        if (AtariCompatibilityCatalog.Get(model).Core != AtariCoreKind.Hatari) return choices.First().Value;
        var culture = CultureInfo.CurrentUICulture;
        var region = culture.Name switch
        {
            AtariHardwareSettingsConstants.UnitedStatesCulture => AtariStRegion.UnitedStates,
            AtariHardwareSettingsConstants.UnitedKingdomCulture => AtariStRegion.UnitedKingdom,
            AtariHardwareSettingsConstants.GermanyCulture => AtariStRegion.Germany,
            AtariHardwareSettingsConstants.FranceCulture => AtariStRegion.France,
            AtariHardwareSettingsConstants.SpainCulture => AtariStRegion.Spain,
            AtariHardwareSettingsConstants.ItalyCulture => AtariStRegion.Italy,
            AtariHardwareSettingsConstants.SwedenCulture => AtariStRegion.Sweden,
            AtariHardwareSettingsConstants.SwitzerlandCulture => AtariStRegion.Switzerland,
            AtariHardwareSettingsConstants.FinlandCulture => AtariStRegion.Finland,
            AtariHardwareSettingsConstants.NorwayCulture => AtariStRegion.Norway,
            AtariHardwareSettingsConstants.CzechRepublicCulture => AtariStRegion.CzechRepublic,
            AtariHardwareSettingsConstants.RussiaCulture => AtariStRegion.Russia,
            AtariHardwareSettingsConstants.GreeceCulture => AtariStRegion.Greece,
            _ => RegionFromLanguage(culture.TwoLetterISOLanguageName)
        };
        var value = region.ToString();
        return choices.Any(choice => choice.Value == value)
            ? value : AtariStRegion.Multilingual.ToString();
    }

    internal static string PreferredVideoStandard(AtariMachineModel model,
        IReadOnlyList<AtariVideoAudioChoice> choices)
    {
        if (!UsesApplicationCultureForVideoStandard(model))
            return choices.First().Value;

        var preferred = AtariVideoAudioSettingsConstants.PalApplicationCultures.Contains(
            CultureInfo.CurrentUICulture.Name)
            ? AtariClassicRegion.Pal.ToString()
            : AtariClassicRegion.Ntsc.ToString();
        return choices.Any(choice => string.Equals(choice.Value, preferred, StringComparison.Ordinal))
            ? preferred
            : choices.First().Value;
    }

    private static bool UsesApplicationCultureForVideoStandard(AtariMachineModel model)
    {
        if (AtariCompatibilityCatalog.Get(model).Core == AtariCoreKind.Hatari) return false;
        var regions = AtariClassicModelCatalog.Get(model).Regions;
        return regions.Contains(AtariClassicRegion.Pal) && regions.Contains(AtariClassicRegion.Ntsc);
    }

    private static AtariStRegion RegionFromLanguage(string language) => language switch
    {
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
        return AtariClassicModelCatalog.Get(model).Regions
            .Select(value => new AtariVideoAudioChoice(value.ToString(),
                AtariRegionDisplayFunctions.DisplayName(value)))
            .ToArray();
    }

    private static IReadOnlyList<AtariVideoAudioChoice> Regions(AtariMachineModel model) =>
        AtariHardwareSettingsFunctions.Create(model, new Dictionary<string, string>()).Regions
            .Select(value => new AtariVideoAudioChoice(value.Value, value.DisplayName)).ToArray();

    private static IReadOnlyList<AtariVideoAudioChoice> Resolutions(AtariMachineModel model) =>
        AtariEightBitSettingsCatalog.SupportsOriginalComputerOptions(model)
            ? AtariEightBitSettingsCatalog.OriginalComputerResolutions.Select(value =>
                new AtariVideoAudioChoice(value, value.Replace("x", " × ", StringComparison.Ordinal))).ToArray()
            : Choices((AtariVideoAudioSettingsConstants.AutomaticValue,
                    L(AtariVideoAudioSettingsConstants.AutomaticResource)),
                (AtariVideoAudioSettingsConstants.NativeValue, AtariVideoAudioSettingsConstants.NativeValue));

    private static IReadOnlyList<AtariVideoAudioChoice> ArtifactingModes(AtariMachineModel model) =>
        AtariEightBitSettingsCatalog.SupportsComputerOptions(model)
            ? AtariEightBitSettingsCatalog.ArtifactingModes.Select(value => new AtariVideoAudioChoice(value,
                value == AtariEightBitSettingsConstants.None
                    ? L(AtariHardwareSettingsConstants.NoneResource) : value)).ToArray()
            : [];

    private static IReadOnlyList<AtariVideoAudioChoice> ToggleChoices() =>
        Choices((AtariVideoAudioSettingsConstants.DisabledValue, L(AtariVideoAudioSettingsConstants.DisabledResource)),
            (AtariVideoAudioSettingsConstants.EnabledValue, L(AtariVideoAudioSettingsConstants.EnabledResource)));

    private static IReadOnlyList<AtariVideoAudioChoice> NumericChoices(int minimum, int maximum, int step,
        string suffix = "") =>
        Enumerable.Range(minimum, (maximum - minimum) / step + AtariVideoAudioSettingsConstants.InclusiveEndpointCount)
            .Select(index => minimum + index * step)
            .Select(value => new AtariVideoAudioChoice(value.ToString(CultureInfo.InvariantCulture),
                value.ToString(CultureInfo.CurrentCulture) + suffix)).ToArray();

    private static IReadOnlyList<AtariVideoAudioChoice> DecimalChoices(decimal minimum, decimal maximum,
        decimal step, string suffix = "")
    {
        var count = decimal.ToInt32((maximum - minimum) / step) + 1;
        return Enumerable.Range(0, count).Select(index => minimum + index * step).Select(value =>
            new AtariVideoAudioChoice(value.ToString("0.00", CultureInfo.InvariantCulture),
                value.ToString("0.00", CultureInfo.CurrentCulture) + suffix)).ToArray();
    }

    private static IReadOnlyList<AtariVideoAudioChoice> Choices(params (string Value, string Label)[] values) =>
        values.Select(value => new AtariVideoAudioChoice(value.Value, value.Label)).ToArray();
    private static AtariVideoAudioChoice Official(string value) => new(value, value);
    private static string L(string resource) => LocExtension.Get(resource);
}
