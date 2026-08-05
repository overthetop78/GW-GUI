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

    [Fact]
    public void FrenchAndEnglishCatalogsCoverEveryNeutralKey()
    {
        var neutral = LocExtension.GetDefinedKeys(CultureInfo.InvariantCulture);
        Assert.NotEmpty(neutral);
        Assert.Empty(neutral.Except(LocExtension.GetDefinedKeys(CultureInfo.GetCultureInfo("fr"))));
        Assert.Empty(neutral.Except(LocExtension.GetDefinedKeys(CultureInfo.GetCultureInfo("en"))));
    }

    [Fact]
    public void MainWindowContainsNoHardCodedNaturalLanguageLabels()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        Assert.NotNull(directory);
        var document = XDocument.Load(Path.Combine(directory!.FullName, "src", "GWGUI.App", "MainWindow.xaml"));
        var visibleAttributes = new HashSet<string>(StringComparer.Ordinal) { "Title", "Header", "Content", "Text", "ToolTip" };
        var technical = new System.Text.RegularExpressions.Regex(@"^(?:\d+|[A-Z]|\d+ / [A-Z]+|\d{2,3} / [A-Z]{2,3}|c=.+)$", System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        var hardCoded = document.Descendants().Attributes()
            .Where(x => visibleAttributes.Contains(x.Name.LocalName) && !x.Value.StartsWith('{') && !technical.IsMatch(x.Value))
            .Select(x => x.Value).Distinct().ToArray();
        Assert.Empty(hardCoded);
    }
}
