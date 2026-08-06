using System.Globalization;
using System.ComponentModel;
using System.IO;
using GWGUI.App.Localization;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace GWGUI.Tests;

public sealed class LocalizationTests
{
    private static readonly string[] RequiredInstallerLanguages =
    [
        "english", "french", "german", "italian", "spanish", "polish", "russian", "japanese",
        "chinesesimplified", "chinesetraditional", "portuguese", "brazilianportuguese", "greek",
        "korean", "dutch", "czech", "hungarian", "turkish", "swedish", "danish", "norwegian",
        "finnish", "romanian", "ukrainian", "arabic", "hebrew", "thai", "indonesian", "vietnamese"
    ];

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
    public void LocalizationRefreshNotifiesEveryBoundResource()
    {
        string? property = null;
        PropertyChangedEventHandler handler = (_, args) => property = args.PropertyName;
        LocalizationSource.Instance.PropertyChanged += handler;
        try { LocalizationSource.Instance.Refresh(); }
        finally { LocalizationSource.Instance.PropertyChanged -= handler; }
        Assert.Equal("Item[]", property);
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

    [Fact]
    public void CodeGeneratedLabelsContainNoHardCodedNaturalLanguageText()
    {
        var root = FindRepositoryRoot();
        var directVisibleText = new Regex("(?:Text|Content|Title|ToolTip)\\s*=\\s*\\\"([^\\\"]+)\\\"", RegexOptions.CultureInvariant);
        var naturalLanguage = new Regex(@"[A-Za-zÀ-ÿ]{2,}\s+[A-Za-zÀ-ÿ]{2,}", RegexOptions.CultureInvariant);
        var offenders = Directory.EnumerateFiles(Path.Combine(root, "src", "GWGUI.App"), "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .SelectMany(file => directVisibleText.Matches(File.ReadAllText(file)).Select(match => $"{Path.GetFileName(file)}: {match.Groups[1].Value}"))
            .Where(value => naturalLanguage.IsMatch(value))
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void EveryLiteralLocalizationKeyUsedByTheApplicationExists()
    {
        var root = FindRepositoryRoot();
        var neutral = LocExtension.GetDefinedKeys(CultureInfo.InvariantCulture);
        var callPattern = new Regex("(?:LocExtension\\.Get|\\bL)\\(\\\"([^\\\"]+)\\\"", RegexOptions.CultureInvariant);
        var xamlPattern = new Regex(@"\{l:Loc\s+([^},\s]+)", RegexOptions.CultureInvariant);
        var missing = Directory.EnumerateFiles(Path.Combine(root, "src", "GWGUI.App"), "*.*", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(file => Path.GetExtension(file) is ".cs" or ".xaml")
            .SelectMany(file =>
            {
                var text = File.ReadAllText(file);
                return callPattern.Matches(text).Select(match => match.Groups[1].Value)
                    .Concat(xamlPattern.Matches(text).Select(match => match.Groups[1].Value));
            })
            .Where(key => !key.EndsWith(".", StringComparison.Ordinal) && !neutral.Contains(key))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(missing);
    }

    [Fact]
    public void InstallerContainsEveryRequestedLanguage()
    {
        var root = FindRepositoryRoot();
        var installerDirectory = Path.Combine(root, "installer");
        var script = File.ReadAllText(Path.Combine(installerDirectory, "GWGUI.iss"));
        var declarations = Regex.Matches(
                script,
                "^Name:\\s*\"([^\"]+)\";\\s*MessagesFile:\\s*\"([^\"]+)\"",
                RegexOptions.Multiline | RegexOptions.CultureInvariant)
            .Select(match => (Name: match.Groups[1].Value, MessagesFile: match.Groups[2].Value))
            .ToArray();

        Assert.Equal(RequiredInstallerLanguages, declarations.Select(declaration => declaration.Name));
        foreach (var declaration in declarations.Where(declaration => !declaration.MessagesFile.StartsWith("compiler:", StringComparison.OrdinalIgnoreCase)))
            Assert.True(File.Exists(Path.Combine(installerDirectory, declaration.MessagesFile)), $"Missing installer language file: {declaration.MessagesFile}");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return Assert.IsType<DirectoryInfo>(directory).FullName;
    }
}
