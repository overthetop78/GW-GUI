using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Emulation.Controllers;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Enums.Input;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.App.Functions.Input.Keyboard;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.App.Views.Controls.Options;
using GWGUI.App.Views.Controls.Options.ControllerVisualization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GWGUI.Emulation;
using GWGUI.Emulation.Enums;


namespace GWGUI.App.Controllers.Emulation.Input;

internal sealed class EmulationInputSettingsController
{
    private const string ControllerVisualResourceKeyPrefix = "Emulation.Controller.Visual.Model.";
    private readonly IEmulationInputSettingsManager _manager;
    private readonly IReadOnlyList<string> _reservedBindings;
    private InputBindingEditor? _keyboard;
    private InputBindingEditor? _mouse;
    private IReadOnlyList<EmulationControllerPortEditor> _ports = [];
    private IReadOnlyDictionary<int, EmulationControllerPort> _portDefinitions = new Dictionary<int, EmulationControllerPort>();
    private EmulationInputSettings _settings = new(null, null, []);
    private string? _moduleId;
    private string? _machineId;
    private IEmulationConfiguration? _loadedConfiguration;

    internal EmulationInputSettingsController(IEmulationInputSettingsManager manager, IEnumerable<string>? reservedBindings = null)
    {
        _manager = manager;
        _reservedBindings = (reservedBindings ?? GWGUI.Infrastructure.Settings.Emulation.EmulationShortcutDefaults.Values.Values).ToArray();
    }

    internal event EventHandler? SettingsChanged;
    internal bool HasErrors => _keyboard?.HasErrors == true || _mouse?.HasErrors == true || _ports.Any(port => port.Bindings.HasErrors);

    internal UIElement CreateContent(EmulationMachineTab tab, IEmulationConfiguration configuration, IReadOnlyList<EmulationSettingsControlField> fields)
    {
        if (!ReferenceEquals(_loadedConfiguration, configuration))
        {
            _loadedConfiguration = configuration;
            _moduleId = configuration.ModuleId;
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
        if (HasErrors) return configuration;
        if (_loadedConfiguration is null || _loadedConfiguration.ModuleId != configuration.ModuleId
            || _loadedConfiguration.MachineId != configuration.MachineId)
            return configuration;
        var keyboard = ReadKeyboardBindings(_settings.Keyboard, _keyboard);
        var mouse = ReadBindings(_settings.Mouse, _mouse);
        var ports = _ports.Count == 0 ? _settings.ControllerPorts : _ports.Select(ReadPort).ToArray();
        _settings = new EmulationInputSettings(keyboard, mouse, ports);
        return _manager.ApplyInputSettings(configuration, _settings);
    }

    internal ValueTask SaveAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        GWGUI.App.Services.Emulation.EmulationVideoPresentationProfiles.Store.Get(configuration.ModuleId, configuration.Id);
        return _manager.SaveInputSettingsAsync(configuration, cancellationToken);
    }

    private UIElement CreateKeyboardView()
    {
        _keyboard = CreateEditor(_settings.Keyboard);
        return EmulationSettingsLayout.KeyboardSettingsPage(_keyboard, LocExtension.Get("Emulation.Keyboard.SpecialKeysOnlyHint"));
    }

    private UIElement CreateMouseView(IReadOnlyList<EmulationSettingsControlField> fields)
    {
        _mouse = CreateEditor(_settings.Mouse);
        ControllerVisualizer? visualizer = null;
        var mouseSettings = _settings.Mouse;
        var visualId = mouseSettings?.DefaultVisualId;
        if (mouseSettings is not null && visualId is not null
            && mouseSettings.CompatibleVisualIds?.Contains(visualId) == true
            && ControllerArtworkCatalog.TryGetProfile(visualId, out var profile))
        {
            var editor = _mouse;
            var commandIds = mouseSettings.VisualCommandIds;
            visualizer = new ControllerVisualizer
            {
                ArtworkProfile = profile,
                VisualCommandIds = commandIds
            };
            visualizer.VisualZoneClicked += control =>
            {
                if (commandIds?.TryGetValue(control, out var commandId) == true)
                    editor.SelectAndStartCapture(commandId);
            };
            var visualization = new EmulationBindingVisualizationController(editor, visualizer);
            visualizer.Loaded += (_, _) => visualization.Start();
            visualizer.Unloaded += (_, _) => visualization.Stop();
        }
        return EmulationSettingsLayout.MouseSettingsPage(fields, null, _mouse, visualizer: visualizer);
    }

    private UIElement CreateControllersView(IReadOnlyList<EmulationSettingsControlField> fields)
    {
        _ports = _settings.ControllerPorts.Select(CreatePort).ToArray();
        _portDefinitions = _settings.ControllerPorts.ToDictionary(port => port.Number);
        var section = new EmulationControllerSettingsSection();
        var view = fields.Count == 0 ? section.Build(_ports.Select(port => port.Settings).ToArray()) : section.Build(_ports.Select(port => port.Settings).ToArray(), fields, LocExtension.Get("Emulation.Input.Behavior"), ControlVisualConstants.GameControllerGlyph);
        return view;
    }

    private InputBindingEditor CreateEditor(EmulationInputBindingSet? set)
    {
        var editor = new InputBindingEditor();
        editor.SetReservedBindings(_reservedBindings);
        editor.BindingsChanged += (_, _) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        editor.ConfigurePresentation(LocExtension.Get("Emulation.Input.Actions"), LocExtension.Get("Emulation.Input.Binding.Search"));
        if (set is null) return editor;
        editor.ConfigureCaptureSources(InputCaptureSources.Keyboard | InputCaptureSources.Mouse | InputCaptureSources.Controller, set.PrefixKeyboardSource);
        editor.SetRows(set.Definitions, set.Values, _moduleId);
        return editor;
    }

    private EmulationControllerPortEditor CreatePort(EmulationControllerPort port)
    {
        var editor = EmulationControllerSettingsSection.CreatePort(port.Number,
            InputCaptureSources.Keyboard | InputCaptureSources.Mouse | InputCaptureSources.Controller, port.Bindings.PrefixKeyboardSource,
            LocExtension.Get("Emulation.Input.Actions"),
            LocExtension.Get("Emulation.Input.Binding.Search"),
            _moduleId ?? string.Empty, _machineId ?? string.Empty);
        var choices = port.ControllerChoices.Select(choice => new EmulationControllerChoiceView(choice, choice.InvariantDisplayValue ?? LocExtension.GetForModule(_moduleId, choice.DisplayResourceKey))).ToArray();
        if (choices.Length > 0 && choices.All(choice => choice.Choice.Category != EmulationControllerCategory.Unspecified))
        {
            var categories = choices.GroupBy(choice => choice.Choice.Category).Select(group => new EmulationControllerCategoryChoiceView(
                group.Key, LocExtension.Get(group.Key switch
                {
                    EmulationControllerCategory.None => "Emulation.Controller.None",
                    EmulationControllerCategory.Gamepad => "Controllers.Enum.Gamepad",
                    _ => "Emulation.Controller.Category." + group.Key
                }), group.Select(choice => choice.Choice).ToArray())).ToArray();
            editor.Type.ItemsSource = categories;
            editor.Type.DisplayMemberPath = nameof(EmulationControllerCategoryChoiceView.DisplayName);
            editor.Type.SelectedItem = categories.FirstOrDefault(category => category.Choices.Any(choice => choice.Id == port.SelectedControllerId)) ?? categories.FirstOrDefault();
        }
        else
        {
            editor.Type.ItemsSource = choices;
            editor.Type.DisplayMemberPath = nameof(EmulationControllerChoiceView.DisplayName);
            editor.Type.SelectedItem = choices.FirstOrDefault(choice => choice.Choice.Id == port.SelectedControllerId) ?? choices.FirstOrDefault();
        }
        editor.PhysicalDeviceId = port.PhysicalDeviceId;
        editor.Bindings.SetReservedBindings(_reservedBindings);
        editor.DeadZonePercent = port.DeadZonePercent;
        editor.Bindings.SetRows(port.Bindings.Definitions, port.Bindings.Values, _moduleId);
        UpdateControllerBindings(editor, port.VisualId, preserveCurrentBindings: true, preferredControllerId: port.SelectedControllerId);
        editor.Type.SelectionChanged += (_, _) =>
        {
            UpdateControllerBindings(editor, preserveCurrentBindings: false);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        };
        editor.Visual.SelectionChanged += (_, _) =>
        {
            UpdateControllerVisualProfile(editor);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        };
        editor.Bindings.BindingsChanged += (_, _) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        return editor;
    }

    private void UpdateControllerBindings(EmulationControllerPortEditor editor, string? preferredVisualId = null, bool preserveCurrentBindings = true, string? preferredControllerId = null)
    {
        if (editor.Type.SelectedItem is EmulationControllerCategoryChoiceView)
        {
            if (!preserveCurrentBindings) editor.Bindings.SetRows([], null, _moduleId);
            UpdateControllerVisuals(editor, preferredVisualId, preferredControllerId);
            UpdateControllerVisualProfile(editor);
            return;
        }
        if (editor.Type.SelectedItem is not EmulationControllerChoiceView selected
            || selected.Choice.BindingDefinitions is null) return;
        var current = preserveCurrentBindings ? editor.Bindings.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal) : new Dictionary<string, string>(StringComparer.Ordinal);
        editor.Bindings.SetRows(selected.Choice.BindingDefinitions, current, _moduleId);
        UpdateControllerVisuals(editor, preferredVisualId);
        UpdateControllerVisualProfile(editor);
    }

    private static void UpdateControllerVisuals(EmulationControllerPortEditor editor, string? preferredVisualId = null, string? preferredControllerId = null)
    {
        var selectedChoice = (editor.Type.SelectedItem as EmulationControllerChoiceView)?.Choice;
        var profiles = ControllerArtworkCatalog.AvailableProfiles(selectedChoice?.CompatibleVisualIds);
        var choices = profiles.Select(profile => new EmulationControllerVisualChoiceView(profile.VisualId,
            LocExtension.Get(ControllerVisualResourceKeyPrefix + profile.VisualId), profile.Artwork)).ToArray();
        if (editor.Type.SelectedItem is EmulationControllerCategoryChoiceView category)
        {
            choices = category.Choices.SelectMany(choice =>
            {
                var compatible = ControllerArtworkCatalog.AvailableProfiles(choice.CompatibleVisualIds);
                if (compatible.Count == 0)
                    return new[] { new EmulationControllerVisualChoiceView(choice.Id, choice.InvariantDisplayValue ?? LocExtension.GetForModule(editor.ModuleId, choice.DisplayResourceKey), null) { ControllerChoice = choice } };
                return compatible.Select(profile => new EmulationControllerVisualChoiceView(choice.Id + ":" + profile.VisualId,
                    LocExtension.Get(ControllerVisualResourceKeyPrefix + profile.VisualId), profile.Artwork) { ControllerChoice = choice, VisualId = profile.VisualId });
            }).ToArray();
        }
        var item = new FrameworkElementFactory(typeof(StackPanel));
        item.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        var image = new FrameworkElementFactory(typeof(Image));
        image.SetValue(FrameworkElement.WidthProperty, 56d);
        image.SetValue(FrameworkElement.HeightProperty, 40d);
        image.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 10, 0));
        image.SetBinding(Image.SourceProperty, new Binding(nameof(EmulationControllerVisualChoiceView.Artwork)));
        item.AppendChild(image);
        var name = new FrameworkElementFactory(typeof(TextBlock));
        name.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        name.SetBinding(TextBlock.TextProperty, new Binding(nameof(EmulationControllerVisualChoiceView.Value)));
        item.AppendChild(name);
        editor.Visual.DisplayMemberPath = string.Empty;
        editor.Visual.ItemTemplate = new DataTemplate { VisualTree = item };
        editor.Visual.ItemsSource = choices;

        var visualId = preferredVisualId;
        if (string.IsNullOrWhiteSpace(visualId)
            || choices.All(choice => !string.Equals(choice.Key, visualId, StringComparison.Ordinal)))
            visualId = choices.Any(choice => string.Equals(choice.Key, selectedChoice?.DefaultVisualId, StringComparison.Ordinal)) ? selectedChoice?.DefaultVisualId : choices.FirstOrDefault()?.Key;

        if (editor.Type.SelectedItem is EmulationControllerCategoryChoiceView)
        {
            editor.Visual.SelectedItem = choices.FirstOrDefault(choice => choice.ControllerChoice?.Id == preferredControllerId && choice.VisualId == preferredVisualId)
                ?? choices.FirstOrDefault(choice => choice.ControllerChoice is { } controller && controller.Id == preferredControllerId && choice.VisualId == controller.DefaultVisualId)
                ?? choices.FirstOrDefault(choice => choice.ControllerChoice?.Id == preferredControllerId) ?? choices.FirstOrDefault();
        }
        else editor.Visual.SelectedValue = visualId;
        editor.Visual.IsEnabled = choices.Length > 1;
    }

    private void UpdateControllerVisualProfile(EmulationControllerPortEditor editor)
    {
        var selectedChoice = editor.SelectedControllerChoice;
        if (editor.Type.SelectedItem is EmulationControllerCategoryChoiceView && selectedChoice?.BindingDefinitions is { } bindings)
        {
            if (editor.SelectedVisualId is { } selectedVisual && selectedChoice.VisualBindingDefinitions?.TryGetValue(selectedVisual, out var visualBindings) == true) bindings = visualBindings;
            var current = editor.Bindings.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal);
            editor.Bindings.SetRows(bindings, current, _moduleId);
        }
        if (editor.SelectedVisualId is { } visualId
            && ControllerArtworkCatalog.TryGetProfile(visualId, out var profile))
        {
            if (editor.Type.SelectedItem is not EmulationControllerCategoryChoiceView && selectedChoice?.VisualBindingDefinitions?.TryGetValue(visualId, out var definitions) == true)
            {
                var current = editor.Bindings.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal);
                editor.Bindings.SetRows(definitions, current, _moduleId);
            }
            editor.SetVisualProfile(profile, selectedChoice?.VisualCommandIds);
            return;
        }
        editor.SetVisualProfile(null, selectedChoice?.VisualCommandIds);
    }

    private static EmulationInputBindingSet? ReadBindings(EmulationInputBindingSet? original, InputBindingEditor? editor) => original is null || editor is null ? original : original with
    {
        Values = editor.Rows.ToDictionary(row => row.Id, row => NormalizeSourceBinding(row.Binding), StringComparer.Ordinal)
    };

    private static EmulationInputBindingSet? ReadKeyboardBindings(EmulationInputBindingSet? original, InputBindingEditor? editor) => original is null || editor is null ? original : original with
    {
        Values = editor.Rows.ToDictionary(row => row.Id, row => NormalizeKeyboardBinding(row.Binding), StringComparer.Ordinal)
    };

    private static string NormalizeKeyboardBinding(string binding)
    {
        if (string.IsNullOrWhiteSpace(binding)) return string.Empty;
        if (binding.Split('+').Any(part => part.StartsWith("Mouse:", StringComparison.OrdinalIgnoreCase) || part.StartsWith("Controller:", StringComparison.OrdinalIgnoreCase))) return NormalizeSourceBinding(binding);
        binding = string.Join("+", binding.Split('+', StringSplitOptions.TrimEntries).Select(part => InputBindingSyntax.TryRemovePrefix(part, "Keyboard:", out var source) ? source : part));
        if (!KeyboardChordFunctions.TryParse(binding, out var chord)) return string.Empty;
        var parts = new List<string>();
        var explicitModifiers = chord.Keys.Aggregate(System.Windows.Input.ModifierKeys.None, (value, key) => value | KeyboardChordFunctions.ModifierForKey(key));
        var prefix = KeyboardChordFunctions.Format(chord.Modifiers & ~explicitModifiers, []);
        if (prefix.Length > 0) parts.Add(prefix);
        foreach (var key in chord.Keys)
        {
            if (!EmulationKeyMapper.TryMap(key, out var mapped)) return string.Empty;
            parts.Add(mapped.ToString());
        }
        return string.Join("+", parts);
    }

    private static string NormalizeSourceBinding(string binding) => string.Join("+", binding.Split('+', StringSplitOptions.TrimEntries).Select(part =>
    {
        if (GWGUI.App.Constants.Input.Controllers.ControllerInputConstants.LegacyButtonNames.Contains(part, StringComparer.OrdinalIgnoreCase)) return part;
        if (part.StartsWith("Mouse:", StringComparison.OrdinalIgnoreCase) || part.StartsWith("Controller:", StringComparison.OrdinalIgnoreCase)) return part;
        var key = InputBindingSyntax.TryRemovePrefix(part, "Keyboard:", out var source) ? source : part;
        var normalized = NormalizeKeyboardBinding(key);
        return normalized.Length == 0 ? string.Empty : "Keyboard:" + normalized;
    }));

    private EmulationControllerPort ReadPort(EmulationControllerPortEditor editor)
    {
        var original = _portDefinitions[editor.Number];
        var selected = editor.SelectedControllerChoice;
        var choice = selected?.Id ?? string.Empty;
        var bindings = original.Bindings with
        {
            Definitions = editor.SelectedVisualId is { } visualId && selected?.VisualBindingDefinitions?.TryGetValue(visualId, out var definitions) == true
                ? definitions : selected?.BindingDefinitions ?? original.Bindings.Definitions,
            Values = editor.Bindings.Rows.ToDictionary(row => row.Id, row => NormalizeSourceBinding(row.Binding), StringComparer.Ordinal)
        };
        return original with
        {
            SelectedControllerId = choice,
            PhysicalDeviceId = editor.PhysicalDeviceId,
            Bindings = bindings,
            DeadZonePercent = editor.DeadZonePercent,
            VisualId = editor.SelectedVisualId
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
