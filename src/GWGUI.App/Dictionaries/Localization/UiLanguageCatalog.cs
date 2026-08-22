using GWGUI.App.Contracts.Localization;
namespace GWGUI.App.Dictionaries.Localization;

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
