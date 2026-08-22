using GWGUI.App.Services.Documentation;
using System.IO;

namespace GWGUI.Tests;

public sealed class UserGuideLocatorTests : IDisposable
{
    private readonly string _applicationDirectory = Path.Combine(Path.GetTempPath(), $"gwgui-guide-{Guid.NewGuid():N}");

    [Fact]
    public void FindsThePdfForTheRequestedCulture()
    {
        var french = CreateGuide("fr-FR");
        CreateGuide(UserGuideLocator.EnglishCultureName);

        Assert.Equal(french, UserGuideLocator.Find(_applicationDirectory, "fr-FR"));
    }

    [Fact]
    public void FallsBackToEnglishWhenTheRequestedCultureIsMissing()
    {
        var english = CreateGuide(UserGuideLocator.EnglishCultureName);

        Assert.Equal(english, UserGuideLocator.Find(_applicationDirectory, "de-DE"));
    }

    [Fact]
    public void ReturnsNullWhenNeitherRequestedNorEnglishGuideExists()
    {
        Assert.Null(UserGuideLocator.Find(_applicationDirectory, "de-DE"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_applicationDirectory)) Directory.Delete(_applicationDirectory, recursive: true);
    }

    private string CreateGuide(string cultureName)
    {
        var directory = Path.Combine(_applicationDirectory, "Documentation", "user-guide");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"gwgui-user-guide-{cultureName}.pdf");
        File.WriteAllBytes(path, "%PDF"u8.ToArray());
        return path;
    }
}
