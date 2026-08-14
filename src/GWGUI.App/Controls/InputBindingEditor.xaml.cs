using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GWGUI.App.Input;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

public partial class InputBindingEditor : UserControl
{
    private readonly ObservableCollection<InputBindingRow> _rows = [];
    private readonly HashSet<Key> _capturePressed = [];
    private readonly List<Key> _captureOrder = [];
    private InputBindingRow? _captureRow;
    private Button? _captureButton;
    private ModifierKeys _captureModifiers;
    private IReadOnlySet<string> _reservedBindings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public InputBindingEditor()
    {
        InitializeComponent();
        BindingsGrid.ItemsSource = _rows;
        SearchBox.ToolTip = LocExtension.Get("Emulation.SearchBinding");
        Legend.Text = LocExtension.Get("Emulation.BindingLegend");
        AddHandler(PreviewKeyDownEvent, new KeyEventHandler(CaptureKeyDown), true);
        AddHandler(PreviewKeyUpEvent, new KeyEventHandler(CaptureKeyUp), true);
    }

    public bool HasErrors => _rows.Any(row => row.State is InputBindingState.Conflict or InputBindingState.Reserved);
    public IReadOnlyList<InputBindingRow> Rows => _rows;
    public event EventHandler? BindingsChanged;

    public void ConfigurePresentation(string firstColumnHeader, string searchPlaceholder)
    {
        TargetColumn.Header = firstColumnHeader;
        SearchPlaceholder.Text = searchPlaceholder;
    }

    public void SetRows(IEnumerable<InputBindingDefinition> definitions, IReadOnlyDictionary<string, string>? values)
    {
        _rows.Clear();
        foreach (var definition in definitions)
            _rows.Add(new InputBindingRow(definition.Id, definition.Label,
                values?.GetValueOrDefault(definition.Id) ?? definition.DefaultBinding, definition.DefaultBinding));
        ValidateBindings();
    }

    public void SetReservedBindings(IEnumerable<string> bindings)
    {
        _reservedBindings = bindings.Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        ValidateBindings();
    }

    private void AssignClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: InputBindingRow row } button) return;
        _captureRow = row;
        _captureButton = button;
        _capturePressed.Clear();
        _captureOrder.Clear();
        _captureModifiers = ModifierKeys.None;
        button.Content = LocExtension.Get("Emulation.PressInput");
        button.Focus();
    }

    private void CaptureKeyDown(object sender, KeyEventArgs e)
    {
        if (_captureRow is null) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        _captureModifiers |= Keyboard.Modifiers;
        if (!KeyboardChord.IsModifierKey(key) && _capturePressed.Add(key)) _captureOrder.Add(key);
        e.Handled = true;
    }

    private void CaptureKeyUp(object sender, KeyEventArgs e)
    {
        if (_captureRow is null) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (!KeyboardChord.IsModifierKey(key)) _capturePressed.Remove(key);
        e.Handled = true;
        if (_captureOrder.Count == 0 || _capturePressed.Count != 0) return;
        _captureRow.Binding = KeyboardChord.Format(_captureModifiers, _captureOrder);
        FinishCapture();
    }

    private void FinishCapture()
    {
        if (_captureButton is not null) _captureButton.Content = LocExtension.Get("Emulation.AssignInput");
        _captureButton = null;
        _captureRow = null;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ClearClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: InputBindingRow row }) return;
        row.Binding = string.Empty;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RestoreDefaultsClicked(object sender, RoutedEventArgs e)
    {
        foreach (var row in _rows) row.Binding = row.DefaultBinding;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ClearConflictsClicked(object sender, RoutedEventArgs e)
    {
        foreach (var row in _rows.Where(row => row.State is InputBindingState.Conflict or InputBindingState.Reserved))
            row.Binding = string.Empty;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SearchChanged(object sender, TextChangedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(_rows);
        var query = SearchBox.Text.Trim();
        view.Filter = item => item is InputBindingRow row && (query.Length == 0 ||
            row.Label.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
            row.Binding.Contains(query, StringComparison.CurrentCultureIgnoreCase));
    }

    private void ValidateBindings()
    {
        var duplicates = _rows.Where(row => !string.IsNullOrWhiteSpace(row.Binding))
            .GroupBy(row => row.Binding.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1).Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var row in _rows)
        {
            if (string.IsNullOrWhiteSpace(row.Binding)) row.SetState(InputBindingState.Unassigned);
            else if (!KeyboardChord.TryParse(row.Binding, out var chord) || KeyboardChord.IsWindowsReserved(chord) ||
                     _reservedBindings.Contains(row.Binding.Trim())) row.SetState(InputBindingState.Reserved);
            else if (duplicates.Contains(row.Binding.Trim())) row.SetState(InputBindingState.Conflict);
            else row.SetState(InputBindingState.Valid);
        }
        BindingsGrid.Items.Refresh();
    }
}

public sealed record InputBindingDefinition(string Id, string Label, string DefaultBinding);
public enum InputBindingState { Valid, Conflict, Reserved, Unassigned }

public sealed class InputBindingRow(string id, string label, string binding, string defaultBinding) : INotifyPropertyChanged
{
    private string _binding = binding;
    private InputBindingState _state;
    public string Id { get; } = id;
    public string Label { get; } = label;
    public string DefaultBinding { get; } = defaultBinding;
    public string Binding { get => _binding; set { _binding = value; OnChanged(); OnChanged(nameof(BindingParts)); } }
    public IReadOnlyList<InputBindingPart> BindingParts
    {
        get
        {
            var parts = _binding.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return parts.Select((part, index) => new InputBindingPart(part,
                index < parts.Length - 1 ? Visibility.Visible : Visibility.Collapsed)).ToArray();
        }
    }
    public InputBindingState State { get => _state; private set { _state = value; OnChanged(); } }
    public string StateText => LocExtension.Get(State switch
    {
        InputBindingState.Valid => "Emulation.BindingValid",
        InputBindingState.Conflict => "Emulation.BindingConflict",
        InputBindingState.Reserved => "Emulation.BindingReserved",
        _ => "Emulation.BindingUnassigned"
    });
    public Brush StateForeground => State switch
    {
        InputBindingState.Valid => Brushes.DarkGreen,
        InputBindingState.Conflict => Brushes.DarkRed,
        InputBindingState.Reserved => Brushes.RoyalBlue,
        _ => Brushes.DimGray
    };
    public Brush StateBackground => State switch
    {
        InputBindingState.Valid => Brushes.Honeydew,
        InputBindingState.Conflict => Brushes.MistyRose,
        InputBindingState.Reserved => Brushes.AliceBlue,
        _ => Brushes.Gainsboro
    };
    public string StateIcon => State switch
    {
        InputBindingState.Valid => "\uE73E",
        InputBindingState.Conflict => "\uEA39",
        InputBindingState.Reserved => "\uEA18",
        _ => "\uE711"
    };
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void SetState(InputBindingState state)
    {
        State = state;
        OnChanged(nameof(StateText)); OnChanged(nameof(StateForeground)); OnChanged(nameof(StateBackground));
        OnChanged(nameof(StateIcon));
    }
    private void OnChanged([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
}

public sealed record InputBindingPart(string Text, Visibility SeparatorVisibility);
