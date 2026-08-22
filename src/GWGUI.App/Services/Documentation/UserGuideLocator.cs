using System.IO;

namespace GWGUI.App.Services.Documentation;

internal static class UserGuideLocator
{
    internal const string EnglishCultureName = "en-US";
    private const string FilePrefix = "gwgui-user-guide-";

    internal static string? Find(string applicationDirectory, string cultureName)
    {
        var directory = Path.Combine(applicationDirectory, "Documentation", "user-guide");
        var localized = Path.Combine(directory, $"{FilePrefix}{cultureName}.pdf");
        if (File.Exists(localized)) return localized;

        var english = Path.Combine(directory, $"{FilePrefix}{EnglishCultureName}.pdf");
        return File.Exists(english) ? english : null;
    }
}
