using System.Globalization;
using System.IO;
using GWGUI.App.Localization;
using System.Xml.Linq;

namespace GWGUI.Tests;

public sealed class LocalizationTests
{
    private static readonly string[] RequiredKeys =
    [
        "Common.Cancel", "Common.Save", "Common.Delete", "Options.Title", "Options.General",
        "Options.Tools", "Options.Hardware", "Options.Profiles", "Hardware.EditorTitle",
        "Hardware.Controller", "Profiles.ByOperation"
    ];

    [Theory]
    [InlineData("fr")]
    [InlineData("en")]
    public void RequiredUserInterfaceStringsExist(string cultureName)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            foreach (var key in RequiredKeys)
                Assert.False(LocExtension.Get(key).StartsWith("[", StringComparison.Ordinal));
        }
        finally { CultureInfo.CurrentUICulture = previous; }
    }

    [Theory]
    [InlineData("", "fr-FR", "fr")]
    [InlineData("", "fr-CA", "fr")]
    [InlineData("", "en-US", "en")]
    [InlineData("", "ru-RU", "en")]
    [InlineData("", "zh-CN", "en")]
    [InlineData("", "de-DE", "en")]
    [InlineData("", "it-IT", "en")]
    [InlineData("fr", "en-US", "fr")]
    [InlineData("en", "fr-FR", "en")]
    public void InitialLanguageUsesSavedChoiceOrSupportedWindowsLanguage(
        string savedLanguage, string windowsCulture, string expected)
    {
        Assert.Equal(expected, UiLanguageResolver.Resolve(savedLanguage, CultureInfo.GetCultureInfo(windowsCulture)));
    }

    [Fact]
    public void InitialLanguageFallsBackToEnglishWhenDetectionIsUnavailable()
    {
        Assert.Equal("en", UiLanguageResolver.Resolve("", null));
        Assert.Equal("en", UiLanguageResolver.Resolve("unsupported", CultureInfo.GetCultureInfo("fr-FR")));
    }

    [Fact]
    public void AvailableLanguagesUseStableNativeNames()
    {
        Assert.Collection(UiLanguageCatalog.Available,
            french => { Assert.Equal("fr", french.Code); Assert.Equal("Français", french.NativeName); },
            english => { Assert.Equal("en", english.Code); Assert.Equal("English", english.NativeName); });
    }

    [Fact]
    public void FrenchAndEnglishCatalogsCoverEveryNeutralKey()
    {
        var neutral = LocExtension.GetDefinedKeys(CultureInfo.InvariantCulture);
        Assert.NotEmpty(neutral);
        Assert.Empty(neutral.Except(LocExtension.GetDefinedKeys(CultureInfo.GetCultureInfo("fr"))));
        Assert.Empty(neutral.Except(LocExtension.GetDefinedKeys(CultureInfo.GetCultureInfo("en"))));
    }

    [Fact]
    public void ViewsContainNoHardCodedNaturalLanguageLabels()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        Assert.NotNull(directory);
        var visibleAttributes = new HashSet<string>(StringComparer.Ordinal) { "Title", "Header", "Content", "Text", "ToolTip" };
        var technical = new System.Text.RegularExpressions.Regex(@"^(?:\d+(?:\.\d+)?(?: ?(?:RPM|rpm))?|[A-Z]|DD|HD|ED|\d+ / [A-Z]+|\d{2,3} / [A-Z]{2,3}|c=.+|period=.+|type=.+|Français|English)$", System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        var hardCoded = Directory.EnumerateFiles(Path.Combine(directory!.FullName, "src", "GWGUI.App"), "*.xaml")
            .SelectMany(file => XDocument.Load(file).Descendants().Attributes())
            .Where(x => visibleAttributes.Contains(x.Name.LocalName) && !x.Value.StartsWith('{') && !technical.IsMatch(x.Value))
            .Select(x => x.Value).Distinct().ToArray();
        Assert.Empty(hardCoded);
    }
}
