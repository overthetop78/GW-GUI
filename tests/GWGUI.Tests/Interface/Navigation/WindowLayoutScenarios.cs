using GWGUI.Domain.Settings.Window;
using GWGUI.App.Views.Controls.Shell;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Controls.Write;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.App.Views.Controls.Tools;

namespace GWGUI.Tests.Interface.Navigation;

internal static class WindowLayoutScenarios
{
    internal static Grid ArrangeShell(double width, double height)
    {
        var root = (Grid)NavigationScenarios.Shell.Content;
        root.Measure(new Size(width, height));
        root.Arrange(new Rect(0, 0, width, height));
        root.UpdateLayout();
        return root;
    }

    public static void ShellLayout(int index, double width, double height)
    {
        var window = NavigationScenarios.Shell;
        var tabs = (TabControl)window.FindName("MainTabs");
        tabs.SelectedIndex = index;
        try
        {
            var root = ArrangeShell(width, height);
            var section = (FrameworkElement)window.FindName(NavigationScenarios.SectionNames[index]);
            Assert.Same(section, tabs.SelectedContent);
            AssertInside(tabs, root);
            AssertInside(section, root);
            AssertInside((FrameworkElement)window.FindName("ApplicationMenu"), root);
            AssertInside((FrameworkElement)window.FindName("StatusBarBlock"), root);
            var command = section switch
            {
                ReadTabSection read => read.ExecuteActionButton,
                WriteTabSection write => write.ExecuteActionButton,
                ConversionTabSection conversion => conversion.ExecuteActionButton,
                VisualizerTabSection visualizer => visualizer.Header.OpenButton,
                ExplorerSection explorer => explorer.OpenImageButton,
                ToolsTabSection tools => tools.ToolsList.SelectedIndex == 1 ? tools.CleanExecuteButton : tools.EraseExecuteButton,
                _ => NavigationScenarios.Visuals<Button>(section).Single(button => button.MinWidth == 130)
            };
            AssertInside(command, root);
            AssertInside(command, section);
            foreach (TabItem tab in tabs.Items) AssertInside(tab, root);
            Assert.False(window.IsLoaded);
        }
        finally { tabs.SelectedIndex = 0; }
    }

    internal static void AssertInside(FrameworkElement control, FrameworkElement container)
    {
        Assert.True(control.ActualWidth > 0 && control.ActualHeight > 0, $"{control.Name} has no arranged area.");
        var bounds = control.TransformToAncestor(container).TransformBounds(new Rect(control.RenderSize));
        const double tolerance = 1;
        Assert.InRange(bounds.Left, -tolerance, container.ActualWidth + tolerance);
        Assert.InRange(bounds.Top, -tolerance, container.ActualHeight + tolerance);
        Assert.InRange(bounds.Right, 0, container.ActualWidth + tolerance);
        Assert.InRange(bounds.Bottom, 0, container.ActualHeight + tolerance);
    }

    public static void MenuFits(double width, double height)
    {
        var menu = new MainMenu();
        menu.Measure(new Size(width, height));
        menu.Arrange(new Rect(0, 0, width, menu.DesiredSize.Height));
        menu.UpdateLayout();
        Assert.InRange(menu.ActualHeight, 1, height);
        foreach (var item in new[] { menu.OptionsMenuItem, menu.HelpMenuItem })
        {
            Assert.True(item.ActualWidth > 0);
            Assert.True(item.ActualHeight > 0);
            var bounds = item.TransformToAncestor(menu).TransformBounds(new Rect(item.RenderSize));
            Assert.InRange(bounds.Left, 0, width);
            Assert.InRange(bounds.Right, 0, width);
            Assert.InRange(bounds.Bottom, 0, menu.ActualHeight);
        }
    }

    public static void Restore(double width, double height, double? left, double? top,
        double expectedWidth, double expectedHeight, double? expectedLeft, double? expectedTop)
    {
        var saved = new WindowPlacementSettings { Width = width, Height = height, Left = left, Top = top };
        var actual = WindowPlacementPolicy.Normalize(saved, 1280, 720, -1920, 0, 3840, 1080);
        Assert.Equal(new NormalizedWindowPlacement(expectedWidth, expectedHeight, expectedLeft, expectedTop), actual);
        Assert.Equal(left, saved.Left);
        Assert.Equal(top, saved.Top);
    }

    public static void WorkArea(double? left, double? top, double expectedLeft, double expectedTop)
    {
        var placement = new NormalizedWindowPlacement(1360, 820, left, top);
        var actual = WindowPlacementPolicy.ConstrainToWorkArea(placement, -1920, 40, 1920, 1040);
        Assert.Equal(new NormalizedWindowPlacement(1360, 820, expectedLeft, expectedTop), actual);
    }

    public static void SmallScreen()
    {
        var actual = WindowPlacementPolicy.ConstrainToWorkArea(new(1360, 820, 2000, 1200), 0, 0, 1024, 600);
        Assert.Equal(new NormalizedWindowPlacement(1024, 600, 0, 0), actual);
    }

    public static void InvalidWorkArea(double left, double top, double width, double height)
    {
        var original = new NormalizedWindowPlacement(1360, 820, 50, 70);
        Assert.Equal(original, WindowPlacementPolicy.ConstrainToWorkArea(original, left, top, width, height));
    }
}
