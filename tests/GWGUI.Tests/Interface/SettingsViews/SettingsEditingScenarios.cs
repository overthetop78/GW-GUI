using GWGUI.App.Options.Controllers;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using System.Windows;
namespace GWGUI.Tests.Interface.SettingsViews;
internal static class SettingsEditingScenarios
{
    public static void Browse(string? response)
    {
        var settings = new AppSettings { DefaultImagesFolder = "virtual-old", Language = "en-US" };
        var section = new OptionsGeneralSection(); var saves = 0; var requests = 0;
        GeneralOptionsController? controller = null;
        controller = new GeneralOptionsController(new Window(), section, settings, () => false,
            () => { controller!.ApplyTo(settings); saves++; return Task.CompletedTask; }, () => { }, (key, _) => key,
            () => { requests++; return response; });
        section.Measure(new Size(1200, 800)); section.Arrange(new Rect(0, 0, 1200, 800));
        var browse = Assert.Single(GWGUI.Tests.Interface.Navigation.NavigationScenarios.Visuals<System.Windows.Controls.Button>(section),
            button => Equals(button.Content, GWGUI.App.Localization.Extensions.LocExtension.Get("Common.Browse")));
        browse.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        Assert.Equal(1, requests); Assert.Equal(response is null ? 0 : 1, saves);
        Assert.Equal(response?.Trim() ?? "virtual-old", settings.DefaultImagesFolder);
        Assert.Equal(response ?? "virtual-old", section.ImagesFolder.Text);
        Assert.Equal("en-US", settings.Language);
        section.Themes.SelectedIndex = -1; controller.ApplyTo(settings);
        Assert.Equal(AppTheme.System, settings.Theme);
    }
    public static void Engines()
    {
        var settings = new AppSettings();
        settings.Engines.PhysicalRead = settings.Engines.PhysicalWrite = settings.Engines.Conversion = settings.Engines.ExplorerRead = OperationEngine.Internal;
        var section = new OptionsEnginesSection();
        var saves = 0;
        var controller = new EngineOptionsController(section, settings.Engines, () => false, () => { saves++; return Task.CompletedTask; });
        Assert.Equal(0, saves);
        foreach (var combo in new[] { section.PhysicalRead, section.PhysicalWrite, section.Conversion, section.ExplorerRead }) combo.SelectedIndex = 1;
        controller.ApplyTo(settings);
        Assert.Equal(4, saves);
        Assert.Equal(OperationEngine.GreaseweazleHostTools, settings.Engines.PhysicalRead);
        Assert.Equal(OperationEngine.GreaseweazleHostTools, settings.Engines.PhysicalWrite);
        Assert.Equal(OperationEngine.GreaseweazleHostTools, settings.Engines.Conversion);
        Assert.Equal(OperationEngine.GreaseweazleHostTools, settings.Engines.ExplorerRead);
        section.PhysicalRead.SelectedIndex = 0;
        controller.ApplyTo(settings);
        Assert.Equal(OperationEngine.Internal, settings.Engines.PhysicalRead);
        Assert.Equal(OperationEngine.GreaseweazleHostTools, settings.Engines.PhysicalWrite);
        Assert.Equal(5, saves);
    }
    public static void General()
    {
        var settings = new AppSettings { Language = "en-US", DefaultImagesFolder = "virtual-old", Theme = AppTheme.Light };
        var section = new OptionsGeneralSection();
        var saves = 0;
        var refreshed = 0;
        var controller = new GeneralOptionsController(new Window(), section, settings, () => false,
            () => { saves++; return Task.CompletedTask; }, () => refreshed++, (key, _) => key);
        Assert.Equal("virtual-old", section.ImagesFolder.Text);
        Assert.Equal(0, saves);
        section.ImagesFolder.Text = "  virtual-new  ";
        section.Themes.SelectedIndex = (int)AppTheme.Dark;
        controller.ApplyTo(settings);
        Assert.Equal("virtual-new", settings.DefaultImagesFolder);
        Assert.Equal(AppTheme.Dark, settings.Theme);
        Assert.Equal("en-US", settings.Language);
        Assert.Equal(1, saves);
        Assert.Equal(0, refreshed);
    }
}
