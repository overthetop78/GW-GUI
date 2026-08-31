using GWGUI.App.Constants.Emulation;
using GWGUI.App.Controllers.Emulation.Input;
using GWGUI.App.Contracts.Emulation.Controllers;
using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Emulation.Enums;
using System.Windows;
using System.Windows.Controls;


namespace GWGUI.App.Views.Controls.Emulation.Input;

internal sealed class EmulationControllerPortEditor
{
    private readonly EmulationBindingVisualizationController _visualizationController;
    private IReadOnlyDictionary<EmulationControllerVisualControl, string>? _visualCommandIds;
    internal EmulationControllerPortEditor(int number, InputCaptureSources captureSources,
        bool prefixKeyboardSource, string actionLabel, string searchLabel,
        string moduleId, string machineId)
    {
        Number = number;
        ModuleId = moduleId;
        MachineId = machineId;
        Type = new ComboBox
        {
            MaxDropDownHeight = EmulationControllerSettingsConstants.SelectorDropDownMaximumHeight
        };
        Visual = new ComboBox
        {
            MaxDropDownHeight = EmulationControllerSettingsConstants.SelectorDropDownMaximumHeight,
            DisplayMemberPath = nameof(KeyValuePair<string, string>.Value),
            SelectedValuePath = nameof(KeyValuePair<string, string>.Key)
        };
        Visualizer = new ControllerVisualizer { Visibility = Visibility.Collapsed };
        Bindings = new InputBindingEditor();
        Visualizer.VisualZoneClicked += control =>
        {
            if (_visualCommandIds?.TryGetValue(control, out var commandId) == true)
                Bindings.SelectAndStartCapture(commandId);
        };
        _visualizationController = new EmulationBindingVisualizationController(Bindings, Visualizer);
        Visualizer.Loaded += VisualizerLoaded;
        Visualizer.Unloaded += VisualizerUnloaded;
        Bindings.ConfigurePresentation(actionLabel, searchLabel);
        Bindings.ConfigureCaptureSources(captureSources, prefixKeyboardSource);
    }

    private void VisualizerLoaded(object sender, RoutedEventArgs args) =>
        _visualizationController.Start();

    private void VisualizerUnloaded(object sender, RoutedEventArgs args) =>
        _visualizationController.Stop();

    internal int Number { get; }
    internal string ModuleId { get; }
    internal string MachineId { get; }
    internal ComboBox Type { get; }
    internal ComboBox Visual { get; }
    internal string? SelectedVisualId => Visual.SelectedValue as string;
    internal string? PhysicalDeviceId { get; set; }
    internal ControllerVisualizer Visualizer { get; }
    internal InputBindingEditor Bindings { get; }
    internal int DeadZonePercent { get; set; }

    internal void SetVisualProfile(
        ControllerArtworkProfile? profile,
        IReadOnlyDictionary<EmulationControllerVisualControl, string>? commandIds)
    {
        _visualCommandIds = commandIds;
        Visualizer.ArtworkProfile = profile;
        Visualizer.VisualCommandIds = commandIds;
        Visualizer.Visibility = profile is null ? Visibility.Collapsed : Visibility.Visible;
    }

    internal EmulationControllerPortSettings Settings =>
        new(Number, Type, Visual, Visualizer, Bindings);
}
