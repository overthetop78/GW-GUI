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

    [Fact]
    public Task SonyUsbKeyboardMouseAndNoneUpdateTabsAndSurviveMachineSwitch() => sta.RunAsync(() =>
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(
            Path.GetTempPath(), Path.GetTempPath(), http, Path.GetTempPath());
        IEmulationConfiguration current = new GWGUI.Emulation.Sony.Common.Machines.Common.Contracts.MachineConfiguration(
            GWGUI.Emulation.Sony.Common.Machines.Common.Constants.ModelConstants.PlayStation2, "pcsx2");
        var controller = new EmulationInputSettingsController(module);
        var views = new List<UIElement>();
        UIElement BuildInputViews()
        {
            var visibility = module.Describe(current.MachineId, current).Visibility.Tabs;
            foreach (var tab in new[] { EmulationMachineTab.Keyboard, EmulationMachineTab.Mouse })
                if (visibility.GetValueOrDefault(tab)) views.Add(controller.CreateContent(tab, current, []));
            var root = controller.CreateContent(EmulationMachineTab.Controllers, current, []);
            views.Add(root);
            return root;
        }
        void ApplyImmediately(object? sender, EventArgs args) => current = controller.Apply(current);
        controller.SettingsChanged += ApplyImmediately;
        try
        {
            var root = BuildInputViews();
            foreach (var (id, category, keyboard, mouse) in new[]
            {
                ("Keyboard", EmulationControllerCategory.OtherSimulation, true, false),
                ("Mouse", EmulationControllerCategory.PointingDevice, false, true),
                ("None", EmulationControllerCategory.None, false, false)
            })
            {
                var selector = Descendants(root).OfType<ComboBox>().First(combo =>
                    combo.Items.OfType<EmulationControllerCategoryChoiceView>().Any());
                selector.SelectedItem = selector.Items.OfType<EmulationControllerCategoryChoiceView>()
                    .Single(choice => choice.Category == category);
                Assert.False(controller.HasErrors);
                var port = module.DescribeInputSettings(current).ControllerPorts.First(port => port.IsUsb);
                Assert.Equal(id, port.SelectedControllerId);
                var visibility = module.Describe(current.MachineId, current).Visibility.Tabs;
                Assert.Equal(keyboard, visibility[EmulationMachineTab.Keyboard]);
                Assert.Equal(mouse, visibility[EmulationMachineTab.Mouse]);
                root = BuildInputViews();
                Assert.False(controller.HasErrors);
            }
            views.Add(controller.CreateContent(EmulationMachineTab.Controllers,
                module.CreateConfiguration(GWGUI.Emulation.Sony.Common.Machines.Common.Constants.ModelConstants.PlayStation), []));
            root = BuildInputViews();
            var restored = controller.Apply(current);
            Assert.All(module.DescribeInputSettings(restored).ControllerPorts.Where(port => port.IsUsb),
                port => Assert.Equal("None", port.SelectedControllerId));
            Assert.False(module.Describe(restored.MachineId, restored).Visibility.Tabs[EmulationMachineTab.Keyboard]);
            Assert.False(module.Describe(restored.MachineId, restored).Visibility.Tabs[EmulationMachineTab.Mouse]);
        }
        finally
        {
            controller.SettingsChanged -= ApplyImmediately;
            foreach (var root in views)
                foreach (var control in Descendants(root).OfType<FrameworkElement>())
                    if (control is InputBindingEditor or ControllerVisualizer)
                        control.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
            views.Clear();
        }
        return Task.CompletedTask;
    });

    [Fact]
    public Task KeyboardIdentityKeysDoNotDisableSettingsButReservedRemappingsStillDo() => sta.RunAsync(() =>
    {
        var editor = new InputBindingEditor();
        try
        {
            editor.ConfigureCaptureSources(GWGUI.App.Enums.Input.InputCaptureSources.Keyboard,
                allowIdentityKeyboardBindings: true);
            editor.SetReservedBindings(["F12"]);
            InputBindingDefinition[] definitions = [new("A", "", "A", "A"), new("F12", "", "F12", "F12")];
            editor.SetRows(definitions, null);
            Assert.False(editor.HasErrors);
            editor.SetRows(definitions, new Dictionary<string, string> { ["A"] = "F12" });
            Assert.True(editor.HasErrors);
        }
        finally { editor.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent)); }
        return Task.CompletedTask;
    });

    [Fact]
    public Task RumbleUsesOneDeviceForBothMotors() => sta.RunAsync(() =>
    {
        var editor = new EmulationControllerOutputEditor(new DeviceSettingsViews.DeviceListScenarios.Source());
        var changes = 0;
        void Changed(object? sender, EventArgs args) => changes++;
        editor.BindingsChanged += Changed;
        try
        {
            editor.SetBindings(new Dictionary<string, EmulationControllerOutputBinding>());
            editor.SetDefinitions([
                new(GWGUI.Emulation.Constants.EmulationControllerFeedbackCommands.Strong, ""),
                new(GWGUI.Emulation.Constants.EmulationControllerFeedbackCommands.Weak, "")], null);
            var selector = Assert.Single(Descendants(editor).OfType<ComboBox>());
            selector.SelectedIndex = 2;
            Assert.Equal(1, changes);
            Assert.Equal("two", editor.Values[GWGUI.Emulation.Constants.EmulationControllerFeedbackCommands.Strong].DeviceId);
            Assert.Equal("two", editor.Values[GWGUI.Emulation.Constants.EmulationControllerFeedbackCommands.Weak].DeviceId);
            Assert.Equal(EmulationControllerMotor.LowFrequency, editor.Values[GWGUI.Emulation.Constants.EmulationControllerFeedbackCommands.Strong].Motor);
            Assert.Equal(EmulationControllerMotor.HighFrequency, editor.Values[GWGUI.Emulation.Constants.EmulationControllerFeedbackCommands.Weak].Motor);
            selector.SelectedIndex = 0;
            Assert.Empty(editor.Values);
        }
        finally
        {
            editor.BindingsChanged -= Changed;
            editor.Children.Clear();
        }
        return Task.CompletedTask;
    });

    [Fact]
    public Task GroupedStickSelectionRetainsKeyboardRowsAndRoundTripsPhysicalDevice() => sta.RunAsync(() =>
    {
        var source = new DeviceSettingsViews.DeviceListScenarios.Source();
        var editor = new InputBindingEditor { AnalogControllerSource = source };
        var changes = 0;
        void Changed(object? sender, EventArgs args) => changes++;
        editor.BindingsChanged += Changed;
        try
        {
            editor.ConfigureCaptureSources(GWGUI.App.Enums.Input.InputCaptureSources.Keyboard | GWGUI.App.Enums.Input.InputCaptureSources.Controller);
            var definitions = StickDefinitions("N64Stick");
            var keyboard = new Dictionary<string, string> { ["N64StickUp"] = "Keyboard:W", ["N64StickDown"] = "Keyboard:S",
                ["N64StickLeft"] = "Keyboard:A", ["N64StickRight"] = "Keyboard:D" };
            editor.SetRows(definitions, keyboard);
            Assert.Equal(keyboard.OrderBy(item => item.Key), editor.Rows.ToDictionary(row => row.Id, row => row.Binding).OrderBy(item => item.Key));
            var selector = Assert.Single(Descendants(editor).OfType<ComboBox>());
            Assert.Equal(0, selector.SelectedIndex);
            selector.SelectedIndex = 5; // Second controller, right stick.
            Assert.Equal(1, changes);
            var values = editor.Rows.ToDictionary(row => row.Id, row => row.Binding);
            Assert.Equal("Controller:two:RightStickUp", values["N64StickUp"]);
            Assert.Equal("Controller:two:RightStickRight", values["N64StickRight"]);
            Assert.Equal(4, editor.Rows.Count);
            var mapped = GWGUI.Emulation.Functions.EmulationInputMappingFunctions.MapControllerAxis(values, "N64StickRight", "N64StickLeft",
                EmulationControllerState.Empty, [EmulationControllerState.Empty with { DeviceId = "two", RightX = short.MaxValue / 2 }],
                new HashSet<EmulationKey>(), new Dictionary<string, bool>());
            Assert.InRange(mapped, (short)16000, (short)17000);
            editor.SetRows(definitions, values);
            Assert.Equal(5, Assert.Single(Descendants(editor).OfType<ComboBox>()).SelectedIndex);
            source.Devices = [];
            editor.SetRows(definitions, values);
            Assert.Equal(2, Assert.Single(Descendants(editor).OfType<ComboBox>()).SelectedIndex);
            Assert.Equal(values.OrderBy(item => item.Key), editor.Rows.ToDictionary(row => row.Id, row => row.Binding).OrderBy(item => item.Key));
            editor.SetRows(definitions, keyboard);
            Assert.False(editor.HasErrors);
            Assert.Equal(0, Assert.Single(Descendants(editor).OfType<ComboBox>()).SelectedIndex);
        }
        finally
        {
            editor.BindingsChanged -= Changed;
            editor.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
            editor.SetRows([], null);
        }
        return Task.CompletedTask;
    });

    [Fact]
    public Task GroupedStickUsesActualNonConsecutiveAxisIndexes() => sta.RunAsync(() =>
    {
        var source = new DeviceSettingsViews.DeviceListScenarios.Source();
        var device = DeviceSettingsViews.DeviceListScenarios.Device("flight");
        source.Devices = [device with
        {
            StandardCapabilities = device.StandardCapabilities with { HasGamepad = false },
            Controls = [new(GWGUI.App.Services.Input.GameInput.GameInputControlType.Axis, 2, default),
                new(GWGUI.App.Services.Input.GameInput.GameInputControlType.Axis, 7, default)]
        }];
        var editor = new InputBindingEditor { AnalogControllerSource = source };
        try
        {
            editor.ConfigureCaptureSources(GWGUI.App.Enums.Input.InputCaptureSources.Keyboard | GWGUI.App.Enums.Input.InputCaptureSources.Controller);
            editor.SetRows(StickDefinitions("PspStick"), null);
            var selector = Assert.Single(Descendants(editor).OfType<ComboBox>());
            Assert.Equal(4, selector.Items.Count); // Custom, None, axes 2/7, axes 7/2.
            selector.SelectedIndex = 2;
            var values = editor.Rows.ToDictionary(row => row.Id, row => row.Binding);
            Assert.Equal("Controller:flight:Axis2Negative", values["PspStickLeft"]);
            Assert.Equal("Controller:flight:Axis7Positive", values["PspStickDown"]);
            Assert.False(editor.HasErrors);
            selector.SelectedIndex = 1;
            Assert.All(editor.Rows, row => Assert.Empty(row.Binding));
            editor.SetRows([], null);
            Assert.Empty(Descendants(editor).OfType<ComboBox>());
        }
        finally { editor.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent)); }
        return Task.CompletedTask;
    });

    private static InputBindingDefinition[] StickDefinitions(string prefix) =>
    [
        new(prefix + "Up", "", "", "Up") { OppositeDirectionId = prefix + "Down" },
        new(prefix + "Down", "", "", "Down") { OppositeDirectionId = prefix + "Up" },
        new(prefix + "Left", "", "", "Left") { OppositeDirectionId = prefix + "Right" },
        new(prefix + "Right", "", "", "Right") { OppositeDirectionId = prefix + "Left" }
    ];

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            foreach (var descendant in Descendants(child)) yield return descendant;
    }
}
