using GWGUI.App.Functions.Emulation.Controllers;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options.ControllerPresentation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Emulation.Input;

internal sealed class EmulationControllerOutputEditor : StackPanel
{
    private readonly IGameInputControllerSource _source;
    private readonly Dictionary<string, EmulationControllerOutputBinding> _bindings = new(StringComparer.Ordinal);
    private IReadOnlyList<EmulationControllerOutputDefinition> _definitions = [];

    internal EmulationControllerOutputEditor(IGameInputControllerSource? source = null)
    {
        _source = source ?? GameInputControllerSource.Instance;
        Visibility = Visibility.Collapsed;
        Margin = new Thickness(0, 8, 0, 0);
    }

    internal event EventHandler? BindingsChanged;

    internal IReadOnlyDictionary<string, EmulationControllerOutputBinding> Values => _definitions
        .Where(definition => _bindings.ContainsKey(definition.Id)).ToDictionary(definition => definition.Id, definition => _bindings[definition.Id], StringComparer.Ordinal);

    internal void SetBindings(IReadOnlyDictionary<string, EmulationControllerOutputBinding> bindings)
    {
        _bindings.Clear();
        foreach (var binding in bindings) _bindings[binding.Key] = binding.Value;
    }

    internal void SetDefinitions(IReadOnlyList<EmulationControllerOutputDefinition>? definitions, string? moduleId)
    {
        _definitions = definitions ?? [];
        Children.Clear();
        Visibility = _definitions.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        if (_definitions.Count > 0 && _definitions.All(definition => definition.Id is
            EmulationControllerFeedbackCommands.Strong or EmulationControllerFeedbackCommands.Weak))
        {
            BuildRumbleDeviceSelector();
            return;
        }
        foreach (var definition in _definitions)
        {
            var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.Children.Add(new TextBlock { Text = LocExtension.GetForModule(moduleId, definition.DisplayResourceKey), VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
            var selector = new ComboBox { DisplayMemberPath = nameof(OutputChoice.Label), MinWidth = 0 };
            Populate(selector, definition.Id);
            selector.DropDownOpened += (_, _) => Populate(selector, definition.Id);
            selector.SelectionChanged += (_, _) =>
            {
                if (selector.SelectedItem is not OutputChoice choice) return;
                if (choice.Binding is null) _bindings.Remove(definition.Id);
                else _bindings[definition.Id] = choice.Binding;
                BindingsChanged?.Invoke(this, EventArgs.Empty);
            };
            Grid.SetColumn(selector, 1);
            row.Children.Add(selector);
            Children.Add(row);
        }
    }

    private void BuildRumbleDeviceSelector()
    {
        var selectedDevice = _definitions.Select(definition => _bindings.GetValueOrDefault(definition.Id)?.DeviceId)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        void Assign(string? deviceId)
        {
            foreach (var definition in _definitions)
                if (deviceId is null) _bindings.Remove(definition.Id);
                else _bindings[definition.Id] = new(deviceId, definition.Id == EmulationControllerFeedbackCommands.Strong
                    ? EmulationControllerMotor.LowFrequency : EmulationControllerMotor.HighFrequency);
        }
        Assign(selectedDevice);
        var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.Children.Add(new TextBlock { Text = LocExtension.Get("Emulation.Controller.Feedback.Device"),
            VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
        var selector = new ComboBox { DisplayMemberPath = nameof(RumbleDeviceChoice.Label), MinWidth = 0 };
        var populating = false;
        void PopulateDevices()
        {
            populating = true;
            try
            {
                _source.StartMonitoring();
                var choices = new List<RumbleDeviceChoice> { new(null, LocExtension.Get("Controllers.None")) };
                var devices = _source.GetConnectedDevices();
                for (var index = 0; index < devices.Count; index++)
                    if ((devices[index].RumbleMotors & (GameInputRumbleMotors.LowFrequency | GameInputRumbleMotors.HighFrequency)) != 0)
                        choices.Add(new(devices[index].Id, devices[index].GameInputDisplayName + " #" + (index + 1)));
                if (selectedDevice is not null && choices.All(choice => choice.DeviceId != selectedDevice))
                    choices.Add(new(selectedDevice, selectedDevice));
                selector.ItemsSource = choices;
                selector.SelectedItem = choices.First(choice => choice.DeviceId == selectedDevice);
            }
            finally { populating = false; }
        }
        PopulateDevices();
        selector.DropDownOpened += (_, _) => PopulateDevices();
        selector.SelectionChanged += (_, _) =>
        {
            if (populating || selector.SelectedItem is not RumbleDeviceChoice choice) return;
            selectedDevice = choice.DeviceId;
            Assign(selectedDevice);
            BindingsChanged?.Invoke(this, EventArgs.Empty);
        };
        Grid.SetColumn(selector, 1);
        row.Children.Add(selector);
        Children.Add(row);
    }

    private sealed record RumbleDeviceChoice(string? DeviceId, string Label);

    private void Populate(ComboBox selector, string commandId)
    {
        _source.StartMonitoring();
        _bindings.TryGetValue(commandId, out var selected);
        var choices = new List<OutputChoice> { new(null, LocExtension.Get("Controllers.None")) };
        var devices = _source.GetConnectedDevices();
        for (var index = 0; index < devices.Count; index++)
        {
            var device = devices[index];
            foreach (var motor in Enum.GetValues<EmulationControllerMotor>())
                if (device.RumbleMotors.HasFlag(ControllerFeedbackFunctions.PhysicalMotor(motor)))
                    choices.Add(new(new(device.Id, motor), $"{device.GameInputDisplayName} #{index + 1} · {GameInputDisplayFormatter.EnumValue(ControllerFeedbackFunctions.PhysicalMotor(motor))}"));
        }
        if (selected is not null && !choices.Any(choice => choice.Binding == selected))
            choices.Add(new(selected, $"{selected.DeviceId} · {GameInputDisplayFormatter.EnumValue(ControllerFeedbackFunctions.PhysicalMotor(selected.Motor))}"));
        selector.ItemsSource = choices;
        selector.SelectedItem = choices.FirstOrDefault(choice => choice.Binding == selected) ?? choices[0];
    }

    private sealed record OutputChoice(EmulationControllerOutputBinding? Binding, string Label);
}
