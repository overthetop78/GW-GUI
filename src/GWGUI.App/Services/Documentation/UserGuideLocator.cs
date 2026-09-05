using GWGUI.App.Functions.Localization;

namespace GWGUI.App.Services.Documentation;

internal static class UserGuideLocator
{
    internal const string WikiUrl = "https://github.com/overthetop78/GW-GUI/wiki";

    internal static string GetUrl(string cultureName)
    {
        var language = UiLanguageResolver.Resolve(cultureName, null);
        return $"{WikiUrl}/{language}-Guide";
    }
}
