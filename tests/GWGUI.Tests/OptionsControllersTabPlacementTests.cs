using System.IO;
using System.Xml.Linq;

namespace GWGUI.Tests;

public sealed class OptionsControllersTabPlacementTests
{
    [Fact]
    public void ControllersTabIsImmediatelyAfterEmulation()
    {
        var repository = FindRepositoryRoot();
        var document = XDocument.Load(Path.Combine(repository,
            "src", "GWGUI.App", "Views", "Windows", "Options", "OptionsWindow.xaml"));
        var tabControl = Assert.Single(document.Descendants(), element =>
            element.Name.LocalName == "TabControl" &&
            (string?)element.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml")) == "Navigation");
        var tabs = tabControl.Elements()
            .Where(element => element.Name.LocalName == "TabItem")
            .ToArray();

        Assert.True(tabs.Length >= 2);
        Assert.Contains(tabs[^2].Descendants(), element => element.Name.LocalName == "OptionsEmulationSection");
        Assert.Contains(tabs[^1].Descendants(), element => element.Name.LocalName == "OptionsControllersSection");
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GWGUI.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("GW GUI repository root was not found.");
    }
}
