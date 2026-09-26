namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;

internal static class Caprice32OptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var options = new Dictionary<string, string>(
            configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [Caprice32OptionConstants.Model] = model.BackendModel,
            [Caprice32OptionConstants.Ram] = configuration.Options?.GetValueOrDefault(
                SettingsConstants.Ram) ?? model.RamKib.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
            [Caprice32OptionConstants.Language] = Language(),
            [Caprice32OptionConstants.Resolution] = Value(configuration,
                SettingsConstants.VideoResolution, SettingsDescriptionFunctionsConstants.Resolution384),
            [Caprice32OptionConstants.Monitor] = Value(configuration,
                SettingsConstants.VideoMonitor, SettingsDescriptionFunctionsConstants.Color),
            [Caprice32OptionConstants.Intensity] = Value(configuration,
                SettingsConstants.VideoIntensity, "8"),
            [Caprice32OptionConstants.Crop] = Value(configuration,
                SettingsConstants.VideoCrop, SettingsDescriptionFunctionsConstants.Disabled),
            [Caprice32OptionConstants.FloppySound] = Value(configuration,
                SettingsConstants.FloppySound, SettingsDescriptionFunctionsConstants.Enabled),
            [Caprice32OptionConstants.Autorun] = Caprice32OptionConstants.Enabled
        };
        return configuration with { Options = options };
    }

    private static string Value(MachineConfiguration configuration, string key, string defaultValue) =>
        configuration.Options?.GetValueOrDefault(key) ?? defaultValue;

    private static string Language() =>
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "fr" => Caprice32OptionConstants.French,
            "es" => Caprice32OptionConstants.Spanish,
            _ => Caprice32OptionConstants.English
        };
}
