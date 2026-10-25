using System.Globalization;
using Psp = GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants.OptionConstants;
using Ps3 = GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants.OptionConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class SystemLanguageFunctions
{
    internal static bool IsManagedOption(string key) => key is Psp.LanguageKey or Ps3.LanguageKey;

    internal static MachineConfiguration Configure(MachineConfiguration configuration, CultureInfo? culture = null)
    {
        var key = configuration.EmulatorId switch
        {
            Emulators.Ppsspp.Constants.PpssppConstants.Id => Psp.LanguageKey,
            Emulators.Rpcs3.Constants.CoreConstants.Id => Ps3.LanguageKey,
            _ => null
        };
        if (key is null) return configuration;
        // App.SetLanguage updates the default UI culture for every application thread.
        culture ??= CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture;
        var language = culture.TwoLetterISOLanguageName switch
        {
            "ja" => "Japanese", "fr" => "French", "es" => "Spanish", "de" => "German",
            "it" => "Italian", "nl" => "Dutch", "pt" => "Portuguese", "ru" => "Russian",
            "ko" => "Korean", "zh" => TraditionalChinese(culture) ? "Chinese Traditional" : "Chinese Simplified",
            _ => "English"
        };
        var native = key == Psp.LanguageKey ? language : language switch
        {
            "Chinese Traditional" => Ps3.LanguageChineseTrad,
            "Chinese Simplified" => Ps3.LanguageChineseSimp,
            _ => language.ToLowerInvariant()
        };
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        { [key] = native };
        return configuration with { Options = options };
    }

    private static bool TraditionalChinese(CultureInfo culture) =>
        !culture.Name.Contains("-Hans", StringComparison.OrdinalIgnoreCase)
        && (culture.Name.Contains("-Hant", StringComparison.OrdinalIgnoreCase)
            || culture.Name.Split('-').Any(part => part.Equals("TW", StringComparison.OrdinalIgnoreCase)
                || part.Equals("HK", StringComparison.OrdinalIgnoreCase) || part.Equals("MO", StringComparison.OrdinalIgnoreCase)));
}
