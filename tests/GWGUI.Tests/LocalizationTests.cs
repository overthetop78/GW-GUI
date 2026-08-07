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
    [InlineData("fr-FR")]
    [InlineData("en-US")]
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
    [InlineData("", "fr-FR", "fr-FR")]
    [InlineData("", "fr-CA", "fr-FR")]
    [InlineData("", "en-US", "en-US")]
    [InlineData("", "en-GB", "en-US")]
    [InlineData("", "ru-RU", "ru-RU")]
    [InlineData("", "de-AT", "de-DE")]
    [InlineData("", "pt-PT", "pt-PT")]
    [InlineData("", "pt-BR", "pt-BR")]
    [InlineData("", "pt-AO", "pt-PT")]
    [InlineData("", "zh-CN", "zh-Hans")]
    [InlineData("", "zh-SG", "zh-Hans")]
    [InlineData("", "zh-TW", "zh-Hant")]
    [InlineData("", "zh-HK", "zh-Hant")]
    [InlineData("fr", "en-US", "fr-FR")]
    [InlineData("en", "fr-FR", "en-US")]
    [InlineData("pt-BR", "fr-FR", "pt-BR")]
    [InlineData("zh-Hant", "fr-FR", "zh-Hant")]
    public void InitialLanguageUsesSavedChoiceOrSupportedWindowsLanguage(
        string savedLanguage, string windowsCulture, string expected)
    {
        Assert.Equal(expected, UiLanguageResolver.Resolve(savedLanguage, CultureInfo.GetCultureInfo(windowsCulture)));
    }

    [Fact]
    public void InitialLanguageFallsBackToEnglishWhenDetectionIsUnavailable()
    {
        Assert.Equal("en-US", UiLanguageResolver.Resolve("", null));
        Assert.Equal("en-US", UiLanguageResolver.Resolve("unsupported", CultureInfo.GetCultureInfo("fr-FR")));
    }

    [Fact]
    public void AvailableLanguagesUseStableNativeNames()
    {
        Assert.Equal(29, UiLanguageCatalog.Available.Count);
        Assert.Equal(new UiLanguage("fr-FR", "fr-FR", "Français"), UiLanguageCatalog.Available[0]);
        Assert.Equal(new UiLanguage("en-US", "en-US", "English"), UiLanguageCatalog.Available[1]);
        Assert.Contains(new UiLanguage("pt-PT", "pt-PT", "Português"), UiLanguageCatalog.Available);
        Assert.Contains(new UiLanguage("pt-BR", "pt-BR", "Português (Brasil)"), UiLanguageCatalog.Available);
        Assert.Contains(new UiLanguage("zh-Hans", "zh-CN", "简体中文"), UiLanguageCatalog.Available);
        Assert.Contains(new UiLanguage("zh-Hant", "zh-TW", "繁體中文"), UiLanguageCatalog.Available);
        Assert.Equal(UiLanguageCatalog.Available.Count, UiLanguageCatalog.Available.Select(language => language.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void EveryAvailableLanguageCanBeSavedAndResolved()
    {
        foreach (var language in UiLanguageCatalog.Available)
        {
            Assert.Equal(language.Code, UiLanguageResolver.Resolve(language.Code, CultureInfo.GetCultureInfo("en-US")));
            Assert.Equal(language.CultureName, UiLanguageResolver.GetCulture(language.Code).Name);
            Assert.Equal(language.Code, UiLanguageResolver.GetUiCulture(language.Code).Name);
        }
    }

    [Theory]
    [InlineData("zh-Hans", "zh-CN")]
    [InlineData("zh-Hant", "zh-TW")]
    [InlineData("pt-PT", "pt-PT")]
    [InlineData("pt-BR", "pt-BR")]
    public void ScriptAndRegionalVariantsUseTheCorrectFormattingCulture(string languageCode, string expectedCulture)
    {
        Assert.Equal(expectedCulture, UiLanguageResolver.GetCulture(languageCode).Name);
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
    public void EverySupportedLanguageCatalogCoversEveryNeutralKey()
    {
        var neutral = LocExtension.GetDefinedKeys(CultureInfo.InvariantCulture);
        Assert.NotEmpty(neutral);
        foreach (var language in UiLanguageCatalog.Available)
        {
            var culture = UiLanguageResolver.GetUiCulture(language.Code);
            Assert.Empty(neutral.Except(LocExtension.GetDefinedKeys(culture)));
            foreach (var catalog in LocExtension.CatalogNames)
                Assert.Equal(
                    LocExtension.GetDefinedKeys(catalog, CultureInfo.InvariantCulture).Order(),
                    LocExtension.GetDefinedKeys(catalog, culture).Order());
        }
    }

    [Fact]
    public void EverySupportedLanguagePreservesTechnicalTokensAndLayout()
    {
        var resources = Path.Combine(FindRepositoryRoot(), "src", "GWGUI.App", "Resources");
        var protectedSyntax = new Regex(@"\{[^{}]+\}|--[a-z0-9][a-z0-9-]*|\*\.[^;|\s]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        foreach (var catalog in LocExtension.CatalogNames)
        {
            var source = ReadResx(Path.Combine(resources, $"{catalog}.en-US.resx"));
            foreach (var language in UiLanguageCatalog.Available)
            {
                var target = ReadResx(Path.Combine(resources, $"{catalog}.{language.Code}.resx"));
                Assert.Equal(source.Keys.Order(), target.Keys.Order());
                foreach (var (key, sourceValue) in source)
                {
                    var targetValue = target[key];
                    Assert.Equal(
                        protectedSyntax.Matches(sourceValue).Select(match => match.Value).Order(),
                        protectedSyntax.Matches(targetValue).Select(match => match.Value).Order());
                    Assert.Equal(sourceValue.Count(character => character == '|'), targetValue.Count(character => character == '|'));
                    Assert.Equal(sourceValue.Count(character => character == '\n'), targetValue.Count(character => character == '\n'));
                    Assert.DoesNotMatch(@"__PH\d+__|ZXQ|IDX\d+", targetValue);
                }
            }
        }
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

    private static Dictionary<string, string> ReadResx(string path) =>
        XDocument.Load(path).Root!.Elements("data").ToDictionary(
            element => element.Attribute("name")!.Value,
            element => element.Element("value")?.Value ?? string.Empty,
            StringComparer.Ordinal);

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
