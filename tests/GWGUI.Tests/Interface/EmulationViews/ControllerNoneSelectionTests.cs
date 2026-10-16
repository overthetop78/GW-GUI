using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Contracts.Emulation.Controllers;
using GWGUI.App.Controllers.Emulation.Input;
using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Tests.Application.TestInfrastructure;
using Xunit.Abstractions;

namespace GWGUI.Tests.Interface.EmulationViews;

[Collection("WPF")]
public sealed class ControllerNoneSelectionTests(StaExecutionScenarios sta, ITestOutputHelper output)
{
    [Theory]
    [InlineData("Acorn")] [InlineData("Amstrad")] [InlineData("Apple")]
    [InlineData("Atari")] [InlineData("Bandai")] [InlineData("Coleco")]
    [InlineData("Commodore")] [InlineData("Epoch")] [InlineData("Fairchild")]
    [InlineData("Mattel")] [InlineData("Microsoft")] [InlineData("Nec")]
    [InlineData("Nintendo")] [InlineData("Panasonic")] [InlineData("Philips")]
    [InlineData("Sega")] [InlineData("Sharp")] [InlineData("Sinclair")]
    [InlineData("Snk")] [InlineData("Sony")] [InlineData("Thomson")]
    [InlineData("Watara")] [InlineData("Wenquxing")]
    public Task EveryPortPublishesCompleteSelectionsWhenDisconnected(string family) => sta.RunAsync(async () =>
    {
        using var http = new HttpClient();
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var factoryType = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
        var factory = (IEmulationModuleFactory)Activator.CreateInstance(factoryType)!;
        var module = factory.Create(new EmulationModuleContext(Path.GetTempPath(), Path.GetTempPath(), http));
        var manager = Assert.IsAssignableFrom<IEmulationInputSettingsManager>(module);
        var emulatorManager = Assert.IsAssignableFrom<IEmulationEmulatorManager>(module);
        var portCount = 0;
        var transitions = 0;
        var fixedPorts = 0;
        foreach (var machine in module.Machines)
        {
            var initial = module.CreateConfiguration(machine.Id);
            foreach (var emulator in await emulatorManager.GetEmulatorInstallationsAsync(initial))
            {
                var current = await emulatorManager.UseEmulatorAsync(initial, emulator.EmulatorId);
                var controller = new EmulationInputSettingsController(manager);
                var root = controller.CreateContent(EmulationMachineTab.Controllers, current, []);
                var notifications = 0;
                controller.SettingsChanged += ApplyImmediately;
                void ApplyImmediately(object? sender, EventArgs args)
                {
                    current = controller.Apply(current);
                    foreach (var port in manager.DescribeInputSettings(current).ControllerPorts)
                        Assert.Contains(port.ControllerChoices, choice => choice.Id == port.SelectedControllerId);
                    notifications++;
                }
                try
                {
                    var selectors = Descendants(root).OfType<ComboBox>()
                        .Where(combo => combo.Items.OfType<EmulationControllerCategoryChoiceView>().Any()).ToArray();
                    Assert.Equal(manager.DescribeInputSettings(current).ControllerPorts.Count, selectors.Length);
                    foreach (var selector in selectors)
                    {
                        var categories = selector.Items.OfType<EmulationControllerCategoryChoiceView>().ToArray();
                        var none = categories.SingleOrDefault(category => category.Category == EmulationControllerCategory.None);
                        if (none is null)
                        {
                            var index = Array.IndexOf(selectors, selector);
                            Assert.DoesNotContain(manager.DescribeInputSettings(current).ControllerPorts[index].ControllerChoices, choice => choice.Id == "None");
                            foreach (var category in categories) selector.SelectedItem = category;
                            fixedPorts++;
                            continue;
                        }
                        foreach (var category in categories.Where(category => category != none))
                        {
                            selector.SelectedItem = category;
                            var before = notifications;
                            selector.SelectedItem = none;
                            Assert.Equal(before + 1, notifications);
                            var settings = manager.DescribeInputSettings(current);
                            var index = Array.IndexOf(selectors, selector);
                            Assert.Equal("None", settings.ControllerPorts[index].SelectedControllerId);
                            Assert.Empty(settings.ControllerPorts[index].Bindings.Definitions);
                            transitions++;
                        }
                        portCount++;
                    }
                }
                finally
                {
                    controller.SettingsChanged -= ApplyImmediately;
                    foreach (var editor in Descendants(root).OfType<InputBindingEditor>())
                        editor.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                    foreach (var visualizer in Descendants(root).OfType<ControllerVisualizer>())
                        visualizer.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                }
            }
        }
        output.WriteLine($"{family}: {portCount} disconnectable ports; {transitions} transitions to None; {fixedPorts} ports without a None choice.");
    });

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            foreach (var descendant in Descendants(child)) yield return descendant;
    }
}
