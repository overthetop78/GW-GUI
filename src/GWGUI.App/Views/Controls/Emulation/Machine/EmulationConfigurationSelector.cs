using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Functions.Views.Emulation.Machine;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Emulation.Machine;

internal sealed class EmulationConfigurationSelector : UserControl
{
    private readonly TextBlock _brandLabel = new();
    private readonly TextBlock _machineLabel = new();
    private readonly WrapPanel _brands = new() { Margin = new Thickness(0, 2, 0, 2) };
    private readonly WrapPanel _machines = new() { Margin = new Thickness(0, 2, 0, 2) };

    internal event Action<IEmulationModule>? BrandSelected;
    internal event Action<IEmulationModule, EmulationMachineDefinition>? MachineSelected;
    internal event Action<IEmulationModule, EmulationMachineDefinition>? MachineOpened;

    internal EmulationConfigurationSelector()
    {
        Content = new StackPanel
        {
            Children = { Row(_brandLabel, _brands), Row(_machineLabel, _machines) }
        };
    }

    internal void SetItems(IReadOnlyList<IEmulationModule> modules, IEmulationModule? selectedModule,
        IReadOnlyList<EmulationMachineDefinition> machines, string? selectedMachineId)
    {
        Label(_brandLabel, _brands, ControlVisualConstants.ConfigurationBrandResource);
        Label(_machineLabel, _machines, ControlVisualConstants.ConfigurationMachineResource);
        _brands.Children.Clear();
        _machines.Children.Clear();
        foreach (var module in modules)
        {
            var image = EmulationAssetFunctions.Load(module.BrandImagePath);
            if (image is null) continue;
            var button = EmulationSelectorButtonFunctions.Create(
                LocExtension.GetForModule(module, module.DisplayResourceKey), image, selectedModule?.Id == module.Id);
            button.Click += (_, _) => BrandSelected?.Invoke(module);
            _brands.Children.Add(button);
        }
        if (selectedModule is null) return;
        foreach (var machine in machines)
        {
            var image = EmulationAssetFunctions.Load(machine.ImagePath);
            if (image is null) continue;
            var button = EmulationSelectorButtonFunctions.Create(
                LocExtension.GetForModule(selectedModule, machine.DisplayResourceKey), image, machine.Id == selectedMachineId);
            button.Click += (_, _) => MachineSelected?.Invoke(selectedModule, machine);
            button.MouseDoubleClick += (_, args) =>
            {
                args.Handled = true;
                MachineOpened?.Invoke(selectedModule, machine);
            };
            _machines.Children.Add(button);
        }
    }

    private static DockPanel Row(TextBlock label, WrapPanel images)
    {
        var row = new DockPanel { MinHeight = 56 };
        DockPanel.SetDock(label, Dock.Left);
        row.Children.Add(label);
        row.Children.Add(images);
        return row;
    }

    private static void Label(TextBlock label, WrapPanel panel, string key)
    {
        label.Text = LocExtension.Get(key);
        label.VerticalAlignment = VerticalAlignment.Top;
        label.Margin = new Thickness(0, 11, 12, 5);
        label.FontWeight = FontWeights.SemiBold;
        AutomationProperties.SetName(panel, label.Text);
    }
}
