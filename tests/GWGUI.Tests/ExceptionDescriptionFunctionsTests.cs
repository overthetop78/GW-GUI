using System.Collections;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Resources;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Localization.Sources;

namespace GWGUI.Tests;

[Collection(LocalizationTestCollection.Name)]
public sealed class ExceptionDescriptionFunctionsTests
{
    private static readonly string[] Cultures =
    [
        "ar-SA", "cs-CZ", "da-DK", "de-DE", "el-GR", "en-US", "es-ES", "fi-FI", "fr-FR",
        "he-IL", "hu-HU", "id-ID", "it-IT", "ja-JP", "ko-KR", "nb-NO", "nl-NL", "pl-PL",
        "pt-BR", "pt-PT", "ro-RO", "ru-RU", "sv-SE", "th-TH", "tr-TR", "uk-UA", "vi-VN",
        "zh-Hans", "zh-Hant"
    ];

    [Fact]
    public void EveryDescriptionExistsInEveryCultureAndPreservesPlaceholders()
    {
        var keys = typeof(ErrorDescriptionResourceKeys).GetFields()
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToArray();
        var resources = new ResourceManager("GWGUI.App.Resources.Errors",
            typeof(ErrorDescriptionResourceKeys).Assembly);
        var neutral = Values(resources, CultureInfo.InvariantCulture, keys);

        foreach (var cultureName in Cultures)
        {
            var localized = Values(resources, CultureInfo.GetCultureInfo(cultureName), keys);
            foreach (var key in keys)
                Assert.Equal(neutral[key].Contains("{0}", StringComparison.Ordinal),
                    localized[key].Contains("{0}", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void NetworkTimeoutIsExplainedInFrenchWithoutReferringToLogs()
    {
        var description = InCulture("fr-FR", () => ExceptionDescriptionFunctions.Describe(
            new HttpRequestException("Technical English message",
                new SocketException((int)SocketError.TimedOut))));

        Assert.Equal("Le service distant n'a pas répondu avant l'expiration du délai de connexion. " +
            "Vérifiez la connexion réseau, puis réessayez.", description);
        Assert.DoesNotContain("log", description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("journal", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MissingFileIsExplainedInSelectedLanguageAndNamesTheFile()
    {
        var description = InCulture("fr-FR", () => ExceptionDescriptionFunctions.Describe(
            new FileNotFoundException("Technical English message", @"C:\images\disk.scp")));

        Assert.Equal("Le fichier requis est introuvable : disk.scp", description);
    }

    private static Dictionary<string, string> Values(ResourceManager resources, CultureInfo culture,
        IReadOnlyCollection<string> keys)
    {
        var set = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        Assert.NotNull(set);
        var values = set.Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key,
            entry => (string)entry.Value!, StringComparer.Ordinal);
        var missing = keys.Where(key => !values.ContainsKey(key)).ToArray();
        Assert.True(missing.Length == 0, $"{culture.Name}: missing {string.Join(", ", missing)}");
        return keys.ToDictionary(key => key, key => values[key], StringComparer.Ordinal);
    }

    private static T InCulture<T>(string cultureName, Func<T> action)
    {
        var source = LocalizationSource.Instance;
        var originalCulture = source.Culture;
        var originalUiCulture = source.UiCulture;
        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            source.SetCultures(culture, culture, refresh: false);
            return action();
        }
        finally
        {
            source.SetCultures(originalCulture, originalUiCulture, refresh: false);
        }
    }
}
