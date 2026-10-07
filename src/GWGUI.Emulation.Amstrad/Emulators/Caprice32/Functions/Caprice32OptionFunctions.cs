using OptionConstants = GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants.OptionConstants;
namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;

internal static class Caprice32OptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var options = new Dictionary<string, string>(
            configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [OptionConstants.Model] = model.BackendModel,
            [OptionConstants.Ram] = configuration.Options?.GetValueOrDefault(
                SettingsConstants.Ram) ?? model.RamKib.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
            [OptionConstants.Language] = Language(),
            [OptionConstants.Resolution] = Value(configuration,
                SettingsConstants.VideoResolution, SettingsDescriptionFunctionsConstants.Resolution384),
            [OptionConstants.Monitor] = Value(configuration,
                SettingsConstants.VideoMonitor, SettingsDescriptionFunctionsConstants.Color),
            [OptionConstants.Intensity] = Value(configuration,
                SettingsConstants.VideoIntensity, SettingsDescriptionFunctionsConstants.DefaultVideoIntensity),
            [OptionConstants.Crop] = Value(configuration,
                SettingsConstants.VideoCrop, SettingsDescriptionFunctionsConstants.Disabled),
            [OptionConstants.FloppySound] = Value(configuration,
                SettingsConstants.FloppySound, SettingsDescriptionFunctionsConstants.Enabled),
            [OptionConstants.Autorun] = OptionConstants.Enabled
        };
        return configuration with { Options = options };
    }

    private static string Value(MachineConfiguration configuration, string key, string defaultValue) =>
        configuration.Options?.GetValueOrDefault(key) ?? defaultValue;

    private static string Language() =>
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            OptionConstants.FrenchLanguageCode => OptionConstants.French,
            OptionConstants.SpanishLanguageCode => OptionConstants.Spanish,
            _ => OptionConstants.English
        };
}
