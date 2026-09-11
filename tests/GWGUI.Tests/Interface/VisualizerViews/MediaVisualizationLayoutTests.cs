using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.App.Services.Theming;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Domain.Settings;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.Tests.Interface.VisualizerViews;

[Collection("WPF")]
public sealed class MediaVisualizationLayoutTests(StaExecutionScenarios sta)
{
    [Theory]
    [InlineData(1280, 720)]
    [InlineData(2560, 1440)]
    public Task LayoutKeepsRenderingAndInspectorInsideAvailableSpace(double width, double height) => sta.Run(() =>
    {
        var section = new VisualizerTabSection();
        section.SetInspectorModel(Model());
        section.Measure(new Size(width, height));
        section.Arrange(new Rect(0, 0, width, height));

        var inspector = Assert.IsType<MediaInspectorPanel>(section.FindName("MediaInspector"));
        Assert.True(section.IsInspectorVisible);
        Assert.True(inspector.ActualWidth is >= 240 and <= 460);
        Assert.True(inspector.ActualHeight > 0);
        Assert.True(section.FirstSide.ActualWidth > 0);

        section.SetInspectorModel(null);
        Assert.False(section.IsInspectorVisible);
        var inspectorHost = Assert.IsType<Grid>(section.FindName("InspectorHost"));
        Assert.Equal(Visibility.Collapsed, inspectorHost.Visibility);
    });

    [Fact]
    public Task VisualizerStylesFollowLightAndDarkThemes() => sta.Run(() =>
    {
        var oldTheme = ThemeManager.IsDark ? AppTheme.Dark : AppTheme.Light;
        try
        {
            var surface = new Border();
            surface.Resources.MergedDictionaries.Add(System.Windows.Application.Current.Resources);
            surface.Style = (Style)surface.FindResource("VisualizerSurface");
            ThemeManager.Apply(AppTheme.Light);
            var light = Assert.IsType<SolidColorBrush>(surface.Background).Color;
            ThemeManager.Apply(AppTheme.Dark);
            var dark = Assert.IsType<SolidColorBrush>(surface.Background).Color;

            Assert.NotEqual(light, dark);
            Assert.Equal(Assert.IsType<SolidColorBrush>(System.Windows.Application.Current.Resources["CardBrush"]).Color, dark);
        }
        finally
        {
            ThemeManager.Apply(oldTheme);
        }
    });

    [Fact]
    public Task InspectorAndCommandsExposeAccessibleNames() => sta.Run(() =>
    {
        var section = new VisualizerTabSection();
        section.SetInspectorModel(Model());
        var inspector = Assert.IsType<MediaInspectorPanel>(section.FindName("MediaInspector"));

        Assert.Equal("Selected sector", AutomationProperties.GetName(inspector));
        foreach (var buttonName in new[] { "InspectorButton", "DetachInspectorButton" })
        {
            var button = Assert.IsType<Button>(section.FindName(buttonName));
            Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(button)));
        }
    });

    private static MediaInspectorModel Model() => new(
        "Selected sector",
        "Track 0 · Sector 1",
        [new MediaInspectorSection("Summary", "i", [new MediaInspectorEntry("State", "Available")])]);
}
