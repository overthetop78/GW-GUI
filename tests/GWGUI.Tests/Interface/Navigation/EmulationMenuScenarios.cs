using GWGUI.App.Views.Controls.Shell;
using GWGUI.Emulation.Interfaces;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Views.Windows.EmulationModuleOptions;
using GWGUI.Tests.Interface.EmulationViews;

namespace GWGUI.Tests.Interface.Navigation;

internal static class EmulationMenuScenarios
{
    internal static void Empty()
    {
        var menu = new MainMenu();
        menu.SetEmulationModules([]);

        Assert.Empty(menu.EmulationMenuItem.Items.OfType<MenuItem>().Skip(1));
        Assert.Equal(Visibility.Collapsed,
            menu.EmulationMenuItem.Items.OfType<Separator>().Single().Visibility);
    }

    internal static void DynamicEntries()
    {
        var first = Module("module-one", "Options.General");
        var second = Module("module-two", "Options.Logs");
        var requested = new List<string>();
        var menu = new MainMenu();
        menu.EmulationModuleRequested += requested.Add;

        menu.SetEmulationModules([first, second]);

        var entries = menu.EmulationMenuItem.Items.OfType<MenuItem>().Skip(1).ToArray();
        Assert.Equal(2, entries.Length);
        Assert.Equal(new[] { "module-one", "module-two" },
            entries.Select(entry => Assert.IsType<string>(entry.Tag)).Order().ToArray());
        Assert.All(entries, entry =>
        {
            var label = Assert.IsType<string>(entry.Header);
            Assert.False(string.IsNullOrWhiteSpace(label));
            Assert.False(label.StartsWith('[') && label.EndsWith(']'));
        });
        var selected = entries.Single(entry => Equals(entry.Tag, "module-two"));
        selected.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, selected));
        Assert.Equal("module-two", Assert.Single(requested));
    }

    internal static void ArbitraryModulesUseTheSameWindowType()
    {
        var first = new MachineConfigurationScenarios.Module();
        var second = new MachineConfigurationScenarios.Module();
        var windows = new[]
        {
            new EmulationModuleOptionsWindow(first.Service),
            new EmulationModuleOptionsWindow(second.Service)
        };
        try
        {
            Assert.Equal(windows[0].GetType(), windows[1].GetType());
            Assert.Equal(first.Id, Assert.IsType<GWGUI.App.Views.Controls.Emulation.Options.EmulationModuleSettingsSection>(
                windows[0].ModuleContent.Content).CurrentConfiguration.ModuleId);
            Assert.Equal(second.Id, Assert.IsType<GWGUI.App.Views.Controls.Emulation.Options.EmulationModuleSettingsSection>(
                windows[1].ModuleContent.Content).CurrentConfiguration.ModuleId);
        }
        finally
        {
            foreach (var window in windows) window.Close();
            first.Cleanup();
            second.Cleanup();
        }
    }

    private static IEmulationModule Module(string id, string displayResourceKey) =>
        ControlledDependencies.Simulate<IEmulationModule>((method, _) => method.Name switch
        {
            "get_Id" => id,
            "get_DisplayResourceKey" => displayResourceKey,
            _ => throw new InvalidOperationException(method.Name)
        });
}
