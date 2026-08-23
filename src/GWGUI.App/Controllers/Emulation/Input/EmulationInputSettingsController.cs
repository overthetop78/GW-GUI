using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Emulation.Controllers;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Contracts.Services.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.App.Functions.Input.Keyboard;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.App.Views.Controls.Emulation.Options;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;


namespace GWGUI.App.Controllers.Emulation.Input;

internal sealed class EmulationInputSettingsController
{
    private readonly IEmulationInputSettingsManager _manager;
    private InputBindingEditor? _keyboard;
    private InputBindingEditor? _mouse;
    private IReadOnlyList<EmulationControllerPortEditor> _ports = [];
    private IReadOnlyDictionary<int, EmulationControllerPort> _portDefinitions =
        new Dictionary<int, EmulationControllerPort>();
    private EmulationInputSettings _settings = new(null, null, []);
    private string? _machineId;

    internal EmulationInputSettingsController(IEmulationInputSettingsManager manager) => _manager = manager;

    internal event EventHandler? SettingsChanged;

    internal UIElement CreateContent(EmulationMachineTab tab, IEmulationConfiguration configuration,
        IReadOnlyList<EmulationSettingsControlField> fields)
    {
        if (!string.Equals(_machineId, configuration.MachineId, StringComparison.Ordinal))
        {
            _machineId = configuration.MachineId;
            _keyboard = null;
            _mouse = null;
            _ports = [];
            _portDefinitions = new Dictionary<int, EmulationControllerPort>();
            _settings = _manager.DescribeInputSettings(configuration);
        }
        return tab switch
        {
            EmulationMachineTab.Keyboard => CreateKeyboardView(),
            EmulationMachineTab.Mouse => CreateMouseView(fields),
            EmulationMachineTab.Controllers => CreateControllersView(fields),
            _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, null)
        };
    }

    internal IEmulationConfiguration Apply(IEmulationConfiguration configuration)
    {
        var keyboard = ReadKeyboardBindings(_settings.Keyboard, _keyboard);
        var mouse = ReadBindings(_settings.Mouse, _mouse);
        var ports = _ports.Count == 0 ? _settings.ControllerPorts : _ports.Select(ReadPort).ToArray();
        _settings = new EmulationInputSettings(keyboard, mouse, ports);
        return _manager.ApplyInputSettings(configuration, _settings);
    }

    internal ValueTask SaveAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) =>
        _manager.SaveInputSettingsAsync(configuration, cancellationToken);

    private UIElement CreateKeyboardView()
    {
        _keyboard = CreateEditor(_settings.Keyboard);
        return EmulationSettingsLayout.KeyboardSettingsPage(_keyboard,
            LocExtension.Get("Emulation.Keyboard.SpecialKeysOnlyHint"));
    }

    private UIElement CreateMouseView(IReadOnlyList<EmulationSettingsControlField> fields)
    {
        _mouse = CreateEditor(_settings.Mouse);
        return EmulationSettingsLayout.MouseSettingsPage(fields, null, _mouse);
    }

    private UIElement CreateControllersView(IReadOnlyList<EmulationSettingsControlField> fields)
    {
        _ports = _settings.ControllerPorts.Select(CreatePort).ToArray();
        _portDefinitions = _settings.ControllerPorts.ToDictionary(port => port.Number);
        var section = new EmulationControllerSettingsSection();
        var view = fields.Count == 0
            ? section.Build(_ports.Select(port => port.Settings).ToArray())
            : section.Build(_ports.Select(port => port.Settings).ToArray(), fields,
                LocExtension.Get("Emulation.Input.Behavior"), ControlVisualConstants.GameControllerGlyph);
        _ = section.DetectAsync();
        return view;
    }

    private InputBindingEditor CreateEditor(EmulationInputBindingSet? set)
    {
        var editor = new InputBindingEditor();
        editor.BindingsChanged += (_, _) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        editor.ConfigurePresentation(LocExtension.Get("Emulation.Input.Actions"),
            LocExtension.Get("Emulation.Input.Binding.Search"));
        if (set is null) return editor;
        editor.ConfigureCaptureSources(ToCaptureSources(set.Sources), set.PrefixKeyboardSource);
        editor.SetRows(set.Definitions, set.Values);
        return editor;
    }

    private EmulationControllerPortEditor CreatePort(EmulationControllerPort port)
    {
        var editor = EmulationControllerSettingsSection.CreatePort(port.Number,
            ToCaptureSources(port.Bindings.Sources), port.Bindings.PrefixKeyboardSource,
            LocExtension.Get("Emulation.Input.Actions"), LocExtension.Get("Emulation.Input.Binding.Search"));
        var choices = port.ControllerChoices.Select(choice => new EmulationControllerChoiceView(choice,
            choice.InvariantDisplayValue ?? LocExtension.Get(choice.DisplayResourceKey))).ToArray();
        editor.Type.ItemsSource = choices;
        editor.Type.DisplayMemberPath = nameof(EmulationControllerChoiceView.DisplayName);
        editor.Type.SelectedItem = choices.FirstOrDefault(choice => choice.Choice.Id == port.SelectedControllerId)
            ?? choices.FirstOrDefault();
        editor.Type.SelectionChanged += (_, _) =>
        {
            UpdateControllerBindings(editor);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        };
        editor.Device.Tag = port.PhysicalDeviceId;
        editor.DeadZonePercent = port.DeadZonePercent;
        editor.Bindings.SetRows(port.Bindings.Definitions, port.Bindings.Values);
        editor.Bindings.BindingsChanged += (_, _) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        editor.Device.DropDownClosed += (_, _) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        return editor;
    }

    private static void UpdateControllerBindings(EmulationControllerPortEditor editor)
    {
        if (editor.Type.SelectedItem is not EmulationControllerChoiceView selected
            || selected.Choice.BindingDefinitions is null) return;
        var current = editor.Bindings.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal);
        editor.Bindings.SetRows(selected.Choice.BindingDefinitions, current);
    }

    private static EmulationInputBindingSet? ReadBindings(EmulationInputBindingSet? original,
        InputBindingEditor? editor) => original is null || editor is null ? original : original with
    {
        Values = editor.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal)
    };

    private static EmulationInputBindingSet? ReadKeyboardBindings(EmulationInputBindingSet? original,
        InputBindingEditor? editor) => original is null || editor is null ? original : original with
    {
        Values = editor.Rows.Select(row => (row.Id, Key: ParseKeyboardBinding(row.Binding)))
            .Where(item => item.Key != EmulationKey.Unknown)
            .ToDictionary(item => item.Id, item => item.Key.ToString(), StringComparer.Ordinal)
    };

    private static EmulationKey ParseKeyboardBinding(string binding)
    {
        if (Enum.TryParse<EmulationKey>(binding, true, out var direct)) return direct;
        if (!KeyboardChordFunctions.TryParse(binding, out var chord) || chord.Keys.Count != 1)
            return EmulationKey.Unknown;
        return EmulationKeyMapper.TryMap(chord.Keys[0], out var mapped) ? mapped : EmulationKey.Unknown;
    }

    private EmulationControllerPort ReadPort(EmulationControllerPortEditor editor)
    {
        var original = _portDefinitions[editor.Number];
        var selected = (editor.Type.SelectedItem as EmulationControllerChoiceView)?.Choice;
        var choice = selected?.Id ?? string.Empty;
        var device = (editor.Device.SelectedItem as GameControllerDevice)?.Id ?? editor.Device.Tag as string;
        var bindings = original.Bindings with
        {
            Definitions = selected?.BindingDefinitions ?? original.Bindings.Definitions,
            Values = editor.Bindings.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal)
        };
        return original with
        {
            SelectedControllerId = choice,
            PhysicalDeviceId = device,
            Bindings = bindings,
            DeadZonePercent = editor.DeadZonePercent
        };
    }

    private static InputCaptureSources ToCaptureSources(EmulationInputSource sources)
    {
        var result = (InputCaptureSources)0;
        if (sources.HasFlag(EmulationInputSource.Keyboard)) result |= InputCaptureSources.Keyboard;
        if (sources.HasFlag(EmulationInputSource.Mouse)) result |= InputCaptureSources.Mouse;
        if (sources.HasFlag(EmulationInputSource.Controller)) result |= InputCaptureSources.Controller;
        return result;
    }
}
