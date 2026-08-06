using System.Globalization;

namespace GWGUI.App.Localization;

public sealed record UiLanguage(string Code, string CultureName, string NativeName);

public static class UiLanguageCatalog
{
    public const string FallbackCode = "en-US";

    public static IReadOnlyList<UiLanguage> Available { get; } =
    [
        new("fr-FR", "fr-FR", "Français"),
        new("en-US", "en-US", "English"),
        new("de-DE", "de-DE", "Deutsch"),
        new("it-IT", "it-IT", "Italiano"),
        new("es-ES", "es-ES", "Español"),
        new("pl-PL", "pl-PL", "Polski"),
        new("ru-RU", "ru-RU", "Русский"),
        new("ja-JP", "ja-JP", "日本語"),
        new("zh-Hans", "zh-CN", "简体中文"),
        new("zh-Hant", "zh-TW", "繁體中文"),
        new("pt-PT", "pt-PT", "Português"),
        new("pt-BR", "pt-BR", "Português (Brasil)"),
        new("el-GR", "el-GR", "Ελληνικά"),
        new("ko-KR", "ko-KR", "한국어"),
        new("nl-NL", "nl-NL", "Nederlands"),
        new("cs-CZ", "cs-CZ", "Čeština"),
        new("hu-HU", "hu-HU", "Magyar"),
        new("tr-TR", "tr-TR", "Türkçe"),
        new("sv-SE", "sv-SE", "Svenska"),
        new("da-DK", "da-DK", "Dansk"),
        new("nb-NO", "nb-NO", "Norsk bokmål"),
        new("fi-FI", "fi-FI", "Suomi"),
        new("ro-RO", "ro-RO", "Română"),
        new("uk-UA", "uk-UA", "Українська"),
        new("ar-SA", "ar-SA", "العربية"),
        new("he-IL", "he-IL", "עברית"),
        new("th-TH", "th-TH", "ไทย"),
        new("id-ID", "id-ID", "Bahasa Indonesia"),
        new("vi-VN", "vi-VN", "Tiếng Việt")
    ];

    private static readonly Dictionary<string, UiLanguage> ByCode =
        Available.ToDictionary(language => language.Code, StringComparer.OrdinalIgnoreCase);

    public static UiLanguage Fallback => ByCode[FallbackCode];

    public static bool TryGet(string? code, out UiLanguage language)
    {
        if (!string.IsNullOrWhiteSpace(code) && ByCode.TryGetValue(code.Trim(), out var found))
        {
            language = found;
            return true;
        }

        language = Fallback;
        return false;
    }
}

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
