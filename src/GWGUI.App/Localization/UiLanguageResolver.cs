using System.Globalization;

namespace GWGUI.App.Localization;

public static class UiLanguageResolver
{
    private static readonly HashSet<string> SupportedLanguages =
        new(StringComparer.OrdinalIgnoreCase) { "en", "fr" };

    public static string Resolve(string? savedLanguage, CultureInfo? windowsUiCulture)
    {
        if (!string.IsNullOrWhiteSpace(savedLanguage))
        {
            var saved = Normalize(savedLanguage);
            if (SupportedLanguages.Contains(saved)) return saved;
            return "en";
        }

        if (windowsUiCulture is not null)
        {
            var detected = Normalize(windowsUiCulture.Name);
            if (SupportedLanguages.Contains(detected)) return detected;
        }

        return "en";
    }

    private static string Normalize(string cultureName)
    {
        try
        {
            return CultureInfo.GetCultureInfo(cultureName).TwoLetterISOLanguageName.ToLowerInvariant();
        }
        catch (CultureNotFoundException)
        {
            return cultureName.Trim().ToLowerInvariant();
        }
    }
}
