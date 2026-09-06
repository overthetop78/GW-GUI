using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Views.Controls.Shell;
using GWGUI.App.Views.Windows.Shell;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Controls.Write;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.App.Views.Controls.Tools;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Navigation;
using GWGUI.App.Contracts.Services.Navigation;
using GWGUI.App.Enums.Services.Navigation;
using GWGUI.App.Services.Windows;
using GWGUI.Domain.Settings;
using GWGUI.Domain.HostTools;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows.Media;

namespace GWGUI.Tests.Interface.Navigation;

internal static class NavigationScenarios
{
    private static MainWindow? shell;
    private static readonly RecordingNavigation navigation = new();
    internal static readonly string[] SectionNames =
        ["ReadTabBlock", "WriteTabBlock", "ConvertTabBlock", "VisualizerTabBlock", "DiskExplorer", "ToolsTabBlock", "EmulationBlock"];

    internal static MainWindow Shell => shell ??= new MainWindow(
        ControlledDependencies.Reject<IMessageDialogService>(),
        ControlledDependencies.Reject<IFileDialogService>(),
        ControlledDependencies.Reject<IBusinessDialogService>(), navigation,
        hostTools: ControlledDependencies.Reject<IGwInstallationManager>(),
        runner: ControlledDependencies.Reject<IGreaseweazleRunner>(),
        settingsStore: ControlledDependencies.Reject<ISettingsStore>(),
        hardwareRegistry: ControlledDependencies.Reject<IHardwareRegistry>(), dataDirectory: "virtual-data");

    internal static IEnumerable<T> Visuals<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) yield return match;
            foreach (var descendant in Visuals<T>(child)) yield return descendant;
        }
    }

    public static void TabRoundTrip(int index, string expectedType)
    {
        var window = Shell;
        var tabs = (TabControl)window.FindName("MainTabs");
        Assert.Equal(7, tabs.Items.Count);
        var section = (FrameworkElement)window.FindName(SectionNames[index]);
        Assert.Equal(expectedType, section.GetType().Name);
        tabs.SelectedIndex = index;
        WindowLayoutScenarios.ArrangeShell(1360, 820);
        Assert.Same(section, tabs.SelectedContent);
        Assert.Contains(section, Visuals<FrameworkElement>(tabs));

        // User-editable state on operation screens; view state on the empty image/machine screens.
        Action verify;
        Action reset;
        if (section is ReadTabSection read)
        {
            var input = read.FileNameBlock.FileNameTextBox;
            var before = input.Text;
            input.Text = "navigation_read";
            verify = () => Assert.Equal("navigation_read", input.Text);
            reset = () => input.Text = before;
        }
        else if (section is WriteTabSection write)
        {
            var input = write.AdvancedBlock.NoVerifyCheckBox;
            var before = input.IsChecked;
            input.IsChecked = true;
            verify = () => Assert.True(input.IsChecked);
            reset = () => input.IsChecked = before;
        }
        else if (section is ConversionTabSection conversion)
        {
            var input = conversion.OutputBlock.OutputNameTextBox;
            var before = input.Text;
            input.Text = "navigation_conversion";
            verify = () => Assert.Equal("navigation_conversion", input.Text);
            reset = () => input.Text = before;
        }
        else if (section is ToolsTabSection tools)
        {
            var before = tools.ToolsList.SelectedIndex;
            tools.ToolsList.SelectedIndex = 1;
            verify = () => Assert.Equal(1, tools.ToolsList.SelectedIndex);
            reset = () => tools.ToolsList.SelectedIndex = before;
        }
        else
        {
            // Empty visualizer/explorer and the machine welcome view must keep their content instance.
            var content = ((UserControl)section).Content;
            var context = section.DataContext;
            verify = () => { Assert.Same(content, ((UserControl)section).Content); Assert.Same(context, section.DataContext); };
            reset = () => { };
        }
        try
        {
            for (var other = 0; other < 7; other++) tabs.SelectedIndex = other;
            tabs.SelectedIndex = index;
            WindowLayoutScenarios.ArrangeShell(1360, 820);
            Assert.Same(section, tabs.SelectedContent);
            verify();
            Assert.False(window.IsLoaded); // No startup, configuration files or device discovery.
        }
        finally { reset(); tabs.SelectedIndex = 0; }
    }

    public static void ShellDialogRequest(string request)
    {
        var menu = (MainMenu)Shell.FindName("ApplicationMenu");
        navigation.Calls.Clear();
        var item = request switch
        {
            "preferences" => (MenuItem)menu.OptionsMenuItem.Items[0],
            "history" => (MenuItem)menu.OptionsMenuItem.Items[1],
            _ => (MenuItem)menu.HelpMenuItem.Items[1]
        };
        item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, item));
        Assert.Equal(request, Assert.Single(navigation.Calls));
    }

    private sealed class RecordingNavigation : IWindowNavigationService
    {
        internal List<string> Calls { get; } = [];
        public bool ShowOptions(AppSettings settings, OptionsSection section = OptionsSection.General)
        { Calls.Add("preferences"); return false; }
        public void ShowLogHistory(string logsDirectory) => Calls.Add("history");
        public void ShowAbout() => Calls.Add("about");
        public void ShowGwTool(GwToolWindowRequest request) => Calls.Add(request.Verb);
    }

    public static void DialogOwnerAndArguments(string request, bool? result, bool fail = false)
    {
        var owner = new Window { Width = 320, Height = 200, ShowInTaskbar = false };
        var dialog = new Window();
        var settings = new AppSettings();
        var toolRequest = new GwToolWindowRequest("fake-gw", "seek", "fake-device", "A", "fake-logs", new());
        var factories = new List<string>();
        var shown = 0;
        var service = new WpfWindowNavigationService(owner,
            (actual, section) => { Assert.Same(settings, actual); Assert.Equal(OptionsSection.General, section); factories.Add("preferences"); return dialog; },
            path => { Assert.Equal("fake-logs", path); factories.Add("history"); return dialog; },
            () => { factories.Add("about"); return dialog; },
            actual => { Assert.Same(toolRequest, actual); factories.Add("tool"); return dialog; },
            (actual, actualOwner) =>
            {
                Assert.Same(dialog, actual);
                Assert.Same(owner, actualOwner);
                shown++;
                if (fail) throw new InvalidOperationException("Simulated presentation failure");
                return result;
            });
        try
        {
            void InvokeRequest()
            {
                switch (request)
                {
                    // Existing contract: preferences are saved as edited; ShowOptions returns true even on close.
                    case "preferences": Assert.True(service.ShowOptions(settings)); break;
                    case "history": service.ShowLogHistory("fake-logs"); break;
                    case "about": service.ShowAbout(); break;
                    case "tool": service.ShowGwTool(toolRequest); break;
                }
            }
            if (fail) Assert.Throws<InvalidOperationException>(InvokeRequest);
            else InvokeRequest();
            Assert.Equal(request, Assert.Single(factories));
            Assert.Equal(1, shown);
            Assert.False(owner.IsLoaded);
            Assert.False(dialog.IsLoaded);
        }
        finally { dialog.Close(); owner.Close(); }
    }

    public static IEnumerable<MenuItem> Descendants(ItemsControl parent)
    {
        foreach (var item in parent.Items.OfType<MenuItem>())
        {
            yield return item;
            foreach (var child in Descendants(item)) yield return child;
        }
    }

    public static void MenuRequest(string request)
    {
        var menu = new MainMenu();
        var calls = new List<(string Request, object Sender)>();
        menu.PreferencesRequested += (sender, _) => calls.Add(("preferences", sender));
        menu.LogHistoryRequested += (sender, _) => calls.Add(("history", sender));
        menu.DocumentationRequested += (sender, _) => calls.Add(("documentation", sender));
        menu.AboutRequested += (sender, _) => calls.Add(("about", sender));
        menu.ToolRequested += (sender, verb) => calls.Add((verb, sender));
        var item = request switch
        {
            "preferences" => (MenuItem)menu.OptionsMenuItem.Items[0],
            "history" => (MenuItem)menu.OptionsMenuItem.Items[1],
            "documentation" => (MenuItem)menu.HelpMenuItem.Items[0],
            "about" => (MenuItem)menu.HelpMenuItem.Items[1],
            _ => Descendants(menu.OptionsMenuItem).Single(item => Equals(item.Tag, request))
        };

        item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, item));

        var call = Assert.Single(calls);
        Assert.Equal(request, call.Request);
        Assert.Same(item, call.Sender);
    }

    public static void MissingToolVerbDoesNotDispatch()
    {
        var menu = new MainMenu();
        var calls = 0;
        menu.ToolRequested += (_, _) => calls++;
        menu.AlignMenuItem.Tag = null;
        menu.AlignMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, menu.AlignMenuItem));
        Assert.Equal(0, calls);
    }
}
