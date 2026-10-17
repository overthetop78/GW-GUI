using GWGUI.App.Functions.Input.Bindings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.ViewModels.Input;
using GWGUI.Emulation.Functions;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Emulation.Input;

public partial class InputBindingEditor
{
    internal IGameInputControllerSource AnalogControllerSource { get; set; } = GameInputControllerSource.Instance;
    private readonly List<(ComboBox Selector, IReadOnlyDictionary<string, InputBindingRow> Rows)> _analogSelectors = [];
    private bool _updatingAnalog;

    private sealed record AnalogChoice(string Label, IReadOnlyDictionary<string, string>? Bindings);

    private void RebuildAnalogBindings()
    {
        _analogSelectors.Clear();
        AnalogBindings.Children.Clear();
        if (!_captureSources.HasFlag(GWGUI.App.Enums.Input.InputCaptureSources.Controller)) return;
        var rows = _rows.ToDictionary(row => row.Id, StringComparer.Ordinal);
        var used = new HashSet<string>(StringComparer.Ordinal);
        var groups = new List<IReadOnlyDictionary<string, InputBindingRow>>();
        bool Paired(string first, string second) => rows.TryGetValue(first, out var a)
            && rows.TryGetValue(second, out var b) && a.OppositeDirectionId == second && b.OppositeDirectionId == first;
        foreach (var up in _rows.Where(row => row.Id.EndsWith("Up", StringComparison.Ordinal)))
        {
            var prefix = up.Id[..^2];
            if (!Paired(prefix + "Up", prefix + "Down") || !Paired(prefix + "Left", prefix + "Right")) continue;
            var group = new Dictionary<string, InputBindingRow>(StringComparer.Ordinal);
            foreach (var direction in new[] { "Left", "Right", "Up", "Down" })
            {
                group[direction] = rows[prefix + direction];
                used.Add(prefix + direction);
            }
            groups.Add(group);
        }
        foreach (var row in _rows)
        {
            if (used.Contains(row.Id) || row.OppositeDirectionId is not { } opposite || !Paired(row.Id, opposite)) continue;
            var other = rows[opposite];
            var negative = row.Id.EndsWith("Right", StringComparison.Ordinal) || row.Id.EndsWith("Down", StringComparison.Ordinal)
                ? other : row;
            var positive = ReferenceEquals(negative, row) ? other : row;
            groups.Add(new Dictionary<string, InputBindingRow> { ["Left"] = negative, ["Right"] = positive });
            used.Add(row.Id);
            used.Add(opposite);
        }
        if (groups.Count == 0) return;
        AnalogBindings.Children.Add(new TextBlock { Text = LocExtension.Get("Emulation.Input.Analog.Title"),
            FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
        var number = 0;
        foreach (var group in groups)
        {
            var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.Children.Add(new TextBlock { Text = group.Count == 4
                ? string.Format(LocExtension.Get("Emulation.Input.Analog.Stick"), ++number)
                : group["Left"].Label + " / " + group["Right"].Label,
                TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            var selector = new ComboBox { DisplayMemberPath = nameof(AnalogChoice.Label), MinWidth = 0 };
            _analogSelectors.Add((selector, group));
            PopulateAnalogSelector(selector, group);
            selector.DropDownOpened += (_, _) => PopulateAnalogSelector(selector, group);
            selector.SelectionChanged += (_, _) =>
            {
                if (_updatingAnalog || selector.SelectedItem is not AnalogChoice { Bindings: { } bindings }) return;
                CancelCapture();
                _updatingAnalog = true;
                try
                {
                    foreach (var (direction, target) in group) target.Binding = bindings[direction];
                    ValidateBindings();
                }
                finally { _updatingAnalog = false; }
                RefreshAnalogSelections();
                BindingsChanged?.Invoke(this, EventArgs.Empty);
            };
            Grid.SetColumn(selector, 1);
            grid.Children.Add(selector);
            AnalogBindings.Children.Add(grid);
        }
    }

    private void PopulateAnalogSelector(ComboBox selector, IReadOnlyDictionary<string, InputBindingRow> group)
    {
        _updatingAnalog = true;
        try
        {
            AnalogControllerSource.StartMonitoring();
            var choices = new List<AnalogChoice>
            {
                new(LocExtension.Get("Emulation.Input.Analog.Custom"), null),
                new(LocExtension.Get("Controllers.None"), group.ToDictionary(item => item.Key, _ => string.Empty))
            };
            void Add(string label, string device, Func<string, string> source) => choices.Add(new(label,
                group.ToDictionary(item => item.Key, item => InputBindingSyntax.Controller(device, source(item.Key)))));
            var devices = AnalogControllerSource.GetConnectedDevices();
            for (var i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                var name = device.GameInputDisplayName + " #" + (i + 1) + " · ";
                if (device.StandardCapabilities.HasGamepad)
                    foreach (var stick in new[] { "Left", "Right" })
                    {
                        if (group.Count == 4)
                            Add(name + LocExtension.Get("Emulation.Input.Analog." + stick), device.Id, direction => stick + "Stick" + direction);
                        else
                        {
                            Add(name + LocExtension.Get("Controllers." + stick + "StickX"), device.Id, direction => stick + "Stick" + direction);
                            Add(name + LocExtension.Get("Controllers." + stick + "StickY"), device.Id, direction => stick + "Stick" + (direction == "Left" ? "Up" : "Down"));
                        }
                    }
                var axes = device.Controls.Where(control => control.Type == GameInputControlType.Axis).Select(control => control.Index).Distinct().Order().ToArray();
                foreach (var x in axes)
                {
                    if (group.Count == 2)
                        Add(name + string.Format(LocExtension.Get("Emulation.Input.Analog.Axis"), x), device.Id,
                            direction => "Axis" + x + (direction == "Left" ? "Negative" : "Positive"));
                    else foreach (var y in axes.Where(y => y != x))
                        Add(name + string.Format(LocExtension.Get("Emulation.Input.Analog.Axes"), x, y), device.Id,
                            direction => "Axis" + (direction is "Left" or "Right" ? x : y) + (direction is "Left" or "Up" ? "Negative" : "Positive"));
                }
            }
            if (!choices.Any(choice => Matches(choice, group)) && group.Values.All(row => row.Binding.StartsWith("Controller:", StringComparison.Ordinal)))
            {
                var device = EmulationInputMappingFunctions.ParseControllerDeviceId(group.Values.First().Binding);
                if (device is not null && group.Values.All(row => EmulationInputMappingFunctions.ParseControllerDeviceId(row.Binding) == device))
                    choices.Add(new(device + " · " + LocExtension.Get("Emulation.Input.Analog.Saved"), group.ToDictionary(item => item.Key, item => item.Value.Binding)));
            }
            selector.ItemsSource = choices;
            selector.SelectedItem = choices.FirstOrDefault(choice => Matches(choice, group)) ?? choices[0];
        }
        finally { _updatingAnalog = false; }
    }

    private static bool Matches(AnalogChoice choice, IReadOnlyDictionary<string, InputBindingRow> group) => choice.Bindings is not null
        && group.All(item => string.Equals(choice.Bindings.GetValueOrDefault(item.Key), item.Value.Binding, StringComparison.OrdinalIgnoreCase));

    private void RefreshAnalogSelections()
    {
        if (_updatingAnalog) return;
        _updatingAnalog = true;
        try
        {
            foreach (var (selector, rows) in _analogSelectors)
                selector.SelectedItem = selector.Items.OfType<AnalogChoice>().FirstOrDefault(choice => Matches(choice, rows))
                    ?? selector.Items.OfType<AnalogChoice>().FirstOrDefault();
        }
        finally { _updatingAnalog = false; }
    }
}
