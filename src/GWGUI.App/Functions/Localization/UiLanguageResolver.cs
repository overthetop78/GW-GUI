using GWGUI.App.Dictionaries.Localization;
using System.Globalization;

namespace GWGUI.App.Functions.Localization;

public static class UiLanguageResolver
{
    private static readonly Dictionary<string, string> LanguageDefaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fr"] = "fr-FR", ["en"] = "en-US", ["de"] = "de-DE", ["it"] = "it-IT",
        ["es"] = "es-ES", ["pl"] = "pl-PL", ["ru"] = "ru-RU", ["ja"] = "ja-JP",
        ["zh"] = "zh-Hans", ["pt"] = "pt-PT", ["el"] = "el-GR", ["ko"] = "ko-KR",
        ["nl"] = "nl-NL", ["cs"] = "cs-CZ", ["hu"] = "hu-HU", ["tr"] = "tr-TR",
        ["sv"] = "sv-SE", ["da"] = "da-DK", ["no"] = "nb-NO", ["nb"] = "nb-NO",
        ["fi"] = "fi-FI", ["ro"] = "ro-RO", ["uk"] = "uk-UA", ["ar"] = "ar-SA",
        ["he"] = "he-IL", ["iw"] = "he-IL", ["th"] = "th-TH", ["id"] = "id-ID",
        ["in"] = "id-ID", ["vi"] = "vi-VN"
    };

    private static readonly Dictionary<string, string> CultureAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["zh-CN"] = "zh-Hans", ["zh-SG"] = "zh-Hans", ["zh-Hans"] = "zh-Hans",
        ["zh-TW"] = "zh-Hant", ["zh-HK"] = "zh-Hant", ["zh-MO"] = "zh-Hant", ["zh-Hant"] = "zh-Hant"
    };

    public static string Resolve(string? savedLanguage, CultureInfo? windowsUiCulture)
    {
        if (!string.IsNullOrWhiteSpace(savedLanguage))
            return ResolveCultureName(savedLanguage) ?? UiLanguageCatalog.FallbackCode;

        return windowsUiCulture is null
            ? UiLanguageCatalog.FallbackCode
            : ResolveCultureName(windowsUiCulture.Name) ?? ResolveLanguageFamily(windowsUiCulture) ?? UiLanguageCatalog.FallbackCode;
    }

    public static CultureInfo GetCulture(string languageCode)
    {
        var resolved = Resolve(languageCode, null);
        UiLanguageCatalog.TryGet(resolved, out var language);
        return CultureInfo.GetCultureInfo(language.CultureName);
    }

    public static CultureInfo GetUiCulture(string languageCode) =>
        CultureInfo.GetCultureInfo(Resolve(languageCode, null));

    private static string? ResolveCultureName(string cultureName)
    {
        var value = cultureName.Trim();
        if (UiLanguageCatalog.TryGet(value, out var exact)) return exact.Code;
        if (CultureAliases.TryGetValue(value, out var alias)) return alias;
        if (LanguageDefaults.TryGetValue(value, out var legacy)) return legacy;

        try
        {
            var culture = CultureInfo.GetCultureInfo(value);
            if (CultureAliases.TryGetValue(culture.Name, out alias)) return alias;
            return ResolveLanguageFamily(culture);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }

    private static string? ResolveLanguageFamily(CultureInfo culture) =>
        LanguageDefaults.GetValueOrDefault(culture.TwoLetterISOLanguageName);
}
