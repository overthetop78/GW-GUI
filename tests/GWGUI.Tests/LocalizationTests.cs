using System.Globalization;
using GWGUI.App.Localization;

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
}
