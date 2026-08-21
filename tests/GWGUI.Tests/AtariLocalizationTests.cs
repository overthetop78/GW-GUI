using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GWGUI.Tests;

public sealed class AtariLocalizationTests
{
    private const string ResourceDirectory = "Resources";
    private const string EnglishCulture = "en-US";
    private const string FrenchCulture = "fr-FR";
    private const string AtariSourcePattern = "Atari*Constants.cs";
    private const string ResourceConstantPattern = "const string (?<name>[A-Za-z0-9_]*Resource[A-Za-z0-9_]*)\\s*=\\s*\"(?<key>[^\"]+)\"";
    private const string StringConstantPattern = "const string (?<name>[A-Za-z0-9_]+)\\s*=\\s*\"(?<value>[^\"]*)\"";
    private const string PlaceholderPattern = "\\{\\d+(?::[^{}]+)?\\}";
    private const string NaturalLanguagePattern = "[A-Za-zÀ-ÿ]{2,}\\s+[A-Za-zÀ-ÿ]{2,}";
    private const string ContextSuffix = "Context";
    private const string ActiveConfigurationKey = "Emulation.Atari.Error.ActiveConfiguration";
    private const string HostExecutableMissingKey = "Emulation.Atari.Error.HostExecutableMissing";
    private const string CoreNotInstalledKey = "Emulation.Atari.Error.CoreNotInstalled";
    private const string ConfigurationKey = "Emulation.Configuration";
    private const string AudioTabKey = "Emulation.Tab.Audio";
    private const string EjectKey = "Common.Eject";
    private const string PauseKey = "Common.Pause";
    private const string ErrorDetailsKey = "Emulation.Atari.Error.Details";
    private const string AtariModelKeyPrefix = "Emulation.Atari.Model.";
    private const string MissingResourceMessage = "Missing Atari resource key: {0}";
    private const string DuplicateResourceMessage = "Duplicate Atari resource key: {0}";
    private const string RawTextMessage = "Raw visible Atari text in {0}: {1}";
    private const int UniqueEntryCount = 1;
    private const int ReferenceCatalogIndex = 0;
    private const int FirstTranslatedCatalogIndex = 1;

    private static readonly string[] ReferenceKeys =
    [
        ActiveConfigurationKey,
        "Emulation.Atari.Error.RequiredFirmwareMissing",
        "Emulation.Atari.Error.FirmwareFileMissing",
        "Emulation.Atari.Error.MediaFileMissing",
        HostExecutableMissingKey,
        CoreNotInstalledKey,
        "Emulation.Atari.Storage.MediaFilter",
        ConfigurationKey,
        AudioTabKey,
        EjectKey,
        PauseKey,
        ErrorDetailsKey
    ];

    [Fact]
    public void AtariReferenceResourcesExistWithoutDuplicates()
    {
        foreach (var directory in ReferenceResourceDirectories())
        {
            var names = ReadCultureResourceNames(directory).ToArray();
            foreach (var key in ReferenceKeys)
                Assert.True(names.Contains(key), string.Format(MissingResourceMessage, key));
            Assert.Empty(names.GroupBy(key => key, StringComparer.Ordinal)
                .Where(group => group.Count() > UniqueEntryCount)
                .Select(group => string.Format(DuplicateResourceMessage, group.Key)));
        }
    }

    [Fact]
    public void EveryNamedAtariResourceConstantExistsInReferenceResources()
    {
        var root = FindRepositoryRoot();
        var keys = AtariConstantFiles(root)
            .SelectMany(path => Regex.Matches(File.ReadAllText(path), ResourceConstantPattern,
                RegexOptions.CultureInvariant).Select(match => match.Groups["key"].Value))
            .Where(key => key.Contains('.', StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        foreach (var directory in ReferenceResourceDirectories())
        {
            var names = ReadCultureResourceNames(directory).ToHashSet(StringComparer.Ordinal);
            Assert.DoesNotContain(keys, key => !names.Contains(key));
        }
    }

    [Fact]
    public void AtariConstantsContainNoRawVisibleNaturalLanguage()
    {
        var root = FindRepositoryRoot();
        var naturalLanguage = new Regex(NaturalLanguagePattern, RegexOptions.CultureInvariant);
        var offenders = AtariConstantFiles(root).SelectMany(path =>
                Regex.Matches(File.ReadAllText(path), StringConstantPattern, RegexOptions.CultureInvariant)
                    .Where(match => !match.Groups["name"].Value.EndsWith(ContextSuffix, StringComparison.Ordinal))
                    .Where(match => !match.Groups["name"].Value.Contains("Resource", StringComparison.Ordinal))
                    .Where(match => naturalLanguage.IsMatch(match.Groups["value"].Value))
                    .Select(match => string.Format(RawTextMessage,
                        Path.GetRelativePath(root, path), match.Groups["name"].Value)))
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void AtariReferenceTranslationsPreserveParametersAndTechnicalTerms()
    {
        var resources = ReferenceResourceDirectories().Select(ReadCultureResources).ToArray();
        var placeholder = new Regex(PlaceholderPattern, RegexOptions.CultureInvariant);
        foreach (var key in ReferenceKeys)
        {
            var expected = placeholder.Matches(resources[ReferenceCatalogIndex][key])
                .Select(match => match.Value).ToArray();
            foreach (var catalog in resources.Skip(FirstTranslatedCatalogIndex))
                Assert.Equal(expected, placeholder.Matches(catalog[key]).Select(match => match.Value).ToArray());
        }

        foreach (var catalog in resources)
        {
            Assert.Contains("Atari", catalog[CoreNotInstalledKey], StringComparison.Ordinal);
            Assert.Contains("Atari", catalog[HostExecutableMissingKey], StringComparison.Ordinal);
            Assert.EndsWith(".", catalog[ActiveConfigurationKey], StringComparison.Ordinal);
        }

        var officialModelKeys = resources[ReferenceCatalogIndex].Keys
            .Where(key => key.StartsWith(AtariModelKeyPrefix, StringComparison.Ordinal)).ToArray();
        foreach (var key in officialModelKeys)
            foreach (var catalog in resources.Skip(FirstTranslatedCatalogIndex))
                if (catalog.TryGetValue(key, out var translated))
                    Assert.Equal(resources[ReferenceCatalogIndex][key], translated);
    }

    private static IEnumerable<string> AtariConstantFiles(string root) =>
        Directory.EnumerateFiles(Path.Combine(root, "src", "GWGUI.App"), AtariSourcePattern,
            SearchOption.AllDirectories);

    private static IEnumerable<string> ReferenceResourceDirectories()
    {
        var resources = Path.Combine(FindRepositoryRoot(), "src", "GWGUI.App", ResourceDirectory);
        yield return resources;
        yield return Path.Combine(resources, EnglishCulture);
        yield return Path.Combine(resources, FrenchCulture);
    }

    private static string[] ReadResourceNames(string path) => XDocument.Load(path).Root!
        .Elements("data").Select(element => element.Attribute("name")!.Value).ToArray();

    private static IEnumerable<string> ReadCultureResourceNames(string directory) =>
        Directory.EnumerateFiles(directory, "*.resx", SearchOption.TopDirectoryOnly).SelectMany(ReadResourceNames);

    private static Dictionary<string, string> ReadResources(string path) => XDocument.Load(path).Root!
        .Elements("data").ToDictionary(element => element.Attribute("name")!.Value,
            element => element.Element("value")?.Value ?? string.Empty, StringComparer.Ordinal);

    private static Dictionary<string, string> ReadCultureResources(string directory) =>
        Directory.EnumerateFiles(directory, "*.resx", SearchOption.TopDirectoryOnly)
            .SelectMany(path => ReadResources(path))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln")))
            directory = directory.Parent;
        return Assert.IsType<DirectoryInfo>(directory).FullName;
    }
}
