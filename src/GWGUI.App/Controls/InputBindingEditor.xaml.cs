using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using GWGUI.App.Input;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public partial class InputBindingEditor : UserControl
{
    private static readonly string[] LegacyControllerButtonNames =
        ["B", "Y", "Select", "Start", "Up", "Down", "Left", "Right", "A", "X", "L", "R", "L2", "R2", "L3", "R3"];
    private static readonly string[] ControllerButtonSources =
    [
        "Controller:ButtonB", "Controller:ButtonY", "Controller:View", "Controller:Menu",
        "Controller:DPadUp", "Controller:DPadDown", "Controller:DPadLeft", "Controller:DPadRight",
        "Controller:ButtonA", "Controller:ButtonX", "Controller:LeftShoulder", "Controller:RightShoulder",
        "Controller:LeftTrigger", "Controller:RightTrigger", "Controller:LeftStickClick", "Controller:RightStickClick",
        "Controller:XboxButton"
    ];
    private readonly ObservableCollection<InputBindingRow> _rows = [];
    private readonly HashSet<Key> _capturePressed = [];
    private readonly List<Key> _captureOrder = [];
    private InputBindingRow? _captureRow;
    private Button? _captureButton;
    private ModifierKeys _captureModifiers;
    private InputCaptureSources _captureSources = InputCaptureSources.Keyboard;
    private bool _prefixKeyboardSource;
    private readonly DispatcherTimer _controllerCaptureTimer;
    private IReadOnlyList<EmulationControllerState> _controllerBaseline = [];
    private IReadOnlySet<string> _reservedBindings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private HwndSource? _windowSource;

    public InputBindingEditor()
    {
        InitializeComponent();
        BindingsList.ItemsSource = _rows;
        SearchBox.ToolTip = LocExtension.Get("Emulation.SearchBinding");
        AddHandler(PreviewKeyDownEvent, new KeyEventHandler(CaptureKeyDown), true);
        AddHandler(PreviewKeyUpEvent, new KeyEventHandler(CaptureKeyUp), true);
        AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(CaptureMouseDown), true);
        AddHandler(PreviewMouseWheelEvent, new MouseWheelEventHandler(CaptureMouseWheel), true);
        _controllerCaptureTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(30), DispatcherPriority.Input,
            CaptureControllerInput, Dispatcher);
        _controllerCaptureTimer.Stop();
        Loaded += (_, _) => AttachWindowHook();
        Unloaded += (_, _) => DetachWindowHook();
    }

    public bool HasErrors => _rows.Any(row => row.State is InputBindingState.Conflict or InputBindingState.Reserved);
    public IReadOnlyList<InputBindingRow> Rows => _rows;
    public event EventHandler? BindingsChanged;
    public event EventHandler<ControllerCapturedEventArgs>? ControllerCaptured;

    public void ConfigurePresentation(string firstColumnHeader, string searchPlaceholder)
    {
        TargetHeader.Text = firstColumnHeader;
        SearchPlaceholder.Text = searchPlaceholder;
    }

    public void ConfigureCaptureSources(InputCaptureSources sources, bool prefixKeyboardSource = false)
    {
        _captureSources = sources;
        _prefixKeyboardSource = prefixKeyboardSource;
        ValidateBindings();
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
        if (_captureSources.HasFlag(InputCaptureSources.Controller))
        {
            _controllerBaseline = XInputControllerReader.ReadAll();
            _controllerCaptureTimer.Start();
        }
    }

    private void CaptureKeyDown(object sender, KeyEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Keyboard)) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        _captureModifiers |= Keyboard.Modifiers;
        if (!KeyboardChord.IsModifierKey(key) && _capturePressed.Add(key)) _captureOrder.Add(key);
        e.Handled = true;
    }

    private void CaptureKeyUp(object sender, KeyEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Keyboard)) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (!KeyboardChord.IsModifierKey(key)) _capturePressed.Remove(key);
        e.Handled = true;
        if (_captureOrder.Count == 0 || _capturePressed.Count != 0) return;
        var binding = KeyboardChord.Format(_captureModifiers, _captureOrder);
        _captureRow.Binding = _prefixKeyboardSource ? $"Keyboard:{binding}" : binding;
        FinishCapture();
    }

    private void CaptureMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Mouse)) return;
        var button = e.ChangedButton switch
        {
            MouseButton.Left => "Left",
            MouseButton.Right => "Right",
            MouseButton.Middle => "Middle",
            MouseButton.XButton1 => "XButton1",
            MouseButton.XButton2 => "XButton2",
            _ => null
        };
        if (button is null) return;
        _captureRow.Binding = $"Mouse:{button}";
        e.Handled = true;
        FinishCapture();
    }

    private void CaptureMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Mouse) || e.Delta == 0) return;
        _captureRow.Binding = e.Delta > 0 ? "Mouse:WheelUp" : "Mouse:WheelDown";
        e.Handled = true;
        FinishCapture();
    }

    private void AttachWindowHook()
    {
        if (_windowSource is not null || Window.GetWindow(this) is not Window window) return;
        _windowSource = PresentationSource.FromVisual(window) as HwndSource;
        _windowSource?.AddHook(WindowMessageHook);
    }

    private void DetachWindowHook()
    {
        _windowSource?.RemoveHook(WindowMessageHook);
        _windowSource = null;
    }

    private IntPtr WindowMessageHook(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int mouseHorizontalWheel = 0x020E;
        if (message != mouseHorizontalWheel || _captureRow is null ||
            !_captureSources.HasFlag(InputCaptureSources.Mouse)) return IntPtr.Zero;
        var delta = unchecked((short)((wParam.ToInt64() >> 16) & 0xffff));
        if (delta == 0) return IntPtr.Zero;
        _captureRow.Binding = delta > 0 ? "Mouse:WheelRight" : "Mouse:WheelLeft";
        handled = true;
        FinishCapture();
        return IntPtr.Zero;
    }

    private void CaptureControllerInput(object? sender, EventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Controller))
        {
            _controllerCaptureTimer.Stop();
            return;
        }
        var states = XInputControllerReader.ReadAll();
        for (var port = 0; port < states.Count; port++)
        {
            var baseline = port < _controllerBaseline.Count ? _controllerBaseline[port].Buttons : 0u;
            var pressed = states[port].Buttons & ~baseline;
            if (pressed != 0)
            {
                var index = Enumerable.Range(0, ControllerButtonSources.Length)
                    .FirstOrDefault(candidate => (pressed & (1u << candidate)) != 0, -1);
                if (index >= 0)
                {
                    _captureRow.Binding = ControllerBinding(port, ControllerButtonSources[index]["Controller:".Length..]);
                    ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(port));
                    FinishCapture();
                    return;
                }
            }

            var direction = NewlyMovedDirection(states[port], port < _controllerBaseline.Count
                ? _controllerBaseline[port]
                : EmulationControllerState.Empty);
            if (direction is null) continue;
            _captureRow.Binding = ControllerBinding(port, direction["Controller:".Length..]);
            ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(port));
            FinishCapture();
            return;
        }
        _controllerBaseline = states;
    }

    private static string ControllerBinding(int port, string source) => $"Controller:xinput:{port}:{source}";

    private static string? NewlyMovedDirection(EmulationControllerState current, EmulationControllerState baseline)
    {
        const short threshold = 14000;
        return Moved(current.LeftX, baseline.LeftX, -threshold) ? "Controller:LeftStickLeft"
            : Moved(current.LeftX, baseline.LeftX, threshold) ? "Controller:LeftStickRight"
            : Moved(current.LeftY, baseline.LeftY, -threshold) ? "Controller:LeftStickUp"
            : Moved(current.LeftY, baseline.LeftY, threshold) ? "Controller:LeftStickDown"
            : Moved(current.RightX, baseline.RightX, -threshold) ? "Controller:RightStickLeft"
            : Moved(current.RightX, baseline.RightX, threshold) ? "Controller:RightStickRight"
            : Moved(current.RightY, baseline.RightY, -threshold) ? "Controller:RightStickUp"
            : Moved(current.RightY, baseline.RightY, threshold) ? "Controller:RightStickDown"
            : null;

        static bool Moved(short current, short baseline, int limit) => limit < 0
            ? current < limit && baseline >= limit
            : current > limit && baseline <= limit;
    }

    private void FinishCapture()
    {
        if (_captureButton is not null) _captureButton.Content = LocExtension.Get("Emulation.AssignInput");
        _controllerCaptureTimer.Stop();
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
            else if (!TryValidateBinding(row.Binding, out var reserved) || reserved) row.SetState(InputBindingState.Reserved);
            else if (duplicates.Contains(row.Binding.Trim())) row.SetState(InputBindingState.Conflict);
            else row.SetState(InputBindingState.Valid);
        }
        BindingsList.Items.Refresh();
    }

    private bool TryValidateBinding(string value, out bool reserved)
    {
        reserved = false;
        var trimmed = value.Trim();
        if (trimmed.StartsWith("Mouse:", StringComparison.OrdinalIgnoreCase))
            return _captureSources.HasFlag(InputCaptureSources.Mouse) && trimmed.Length > "Mouse:".Length;
        if (trimmed.StartsWith("Controller:", StringComparison.OrdinalIgnoreCase))
            return _captureSources.HasFlag(InputCaptureSources.Controller) && trimmed.Length > "Controller:".Length;
        if (LegacyControllerButtonNames.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            return _captureSources.HasFlag(InputCaptureSources.Controller);
        var keyboard = trimmed.StartsWith("Keyboard:", StringComparison.OrdinalIgnoreCase)
            ? trimmed["Keyboard:".Length..]
            : trimmed;
        if (!_captureSources.HasFlag(InputCaptureSources.Keyboard) || !KeyboardChord.TryParse(keyboard, out var chord)) return false;
        reserved = KeyboardChord.IsWindowsReserved(chord) ||
                   _reservedBindings.Contains(trimmed) || _reservedBindings.Contains(keyboard);
        return true;
    }
}

public sealed record InputBindingDefinition(string Id, string Label, string DefaultBinding);
public enum InputBindingState { Valid, Conflict, Reserved, Unassigned }
[Flags]
public enum InputCaptureSources { Keyboard = 1, Mouse = 2, Controller = 4 }
public sealed record ControllerCapturedEventArgs(int Port);

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
            return parts.Select((part, index) => new InputBindingPart(DisplayPart(part),
                index < parts.Length - 1 ? Visibility.Visible : Visibility.Collapsed)).ToArray();
        }
    }
    private static string DisplayPart(string part)
    {
        if (part.StartsWith("Keyboard:", StringComparison.OrdinalIgnoreCase)) return part["Keyboard:".Length..];
        if (part.StartsWith("Controller:", StringComparison.OrdinalIgnoreCase))
            return DisplayControllerPart(part["Controller:".Length..]);
        if (!part.StartsWith("Mouse:", StringComparison.OrdinalIgnoreCase)) return part;
        return part["Mouse:".Length..].ToLowerInvariant() switch
        {
            "left" => LocExtension.Get("Emulation.MouseLeftButton"),
            "right" => LocExtension.Get("Emulation.MouseRightButton"),
            "middle" => LocExtension.Get("Emulation.MouseMiddleButton"),
            "xbutton1" => LocExtension.Get("Emulation.MouseButton4"),
            "xbutton2" => LocExtension.Get("Emulation.MouseButton5"),
            "wheelup" => LocExtension.Get("Emulation.MouseWheelUp"),
            "wheeldown" => LocExtension.Get("Emulation.MouseWheelDown"),
            "wheelleft" => LocExtension.Get("Emulation.MouseWheelLeft"),
            "wheelright" => LocExtension.Get("Emulation.MouseWheelRight"),
            _ => part
        };
    }

    private static string DisplayControllerPart(string source)
    {
        var separator = " · ";
        var segments = source.Split(':', StringSplitOptions.RemoveEmptyEntries);
        var input = segments[^1];
        var device = segments.Length >= 3 && segments[0].Equals("xinput", StringComparison.OrdinalIgnoreCase)
                     && int.TryParse(segments[1], out var port)
            ? $"X{port + 1}"
            : null;
        var inputName = input switch
        {
            "DPadUp" => $"D-pad{separator}{LocExtension.Get("Emulation.DirectionUp")}",
            "DPadDown" => $"D-pad{separator}{LocExtension.Get("Emulation.DirectionDown")}",
            "DPadLeft" => $"D-pad{separator}{LocExtension.Get("Emulation.DirectionLeft")}",
            "DPadRight" => $"D-pad{separator}{LocExtension.Get("Emulation.DirectionRight")}",
            "LeftStickUp" => $"{LocExtension.Get("Emulation.LeftStick")}{separator}{LocExtension.Get("Emulation.DirectionUp")}",
            "LeftStickDown" => $"{LocExtension.Get("Emulation.LeftStick")}{separator}{LocExtension.Get("Emulation.DirectionDown")}",
            "LeftStickLeft" => $"{LocExtension.Get("Emulation.LeftStick")}{separator}{LocExtension.Get("Emulation.DirectionLeft")}",
            "LeftStickRight" => $"{LocExtension.Get("Emulation.LeftStick")}{separator}{LocExtension.Get("Emulation.DirectionRight")}",
            "RightStickUp" => $"{LocExtension.Get("Emulation.RightStick")}{separator}{LocExtension.Get("Emulation.DirectionUp")}",
            "RightStickDown" => $"{LocExtension.Get("Emulation.RightStick")}{separator}{LocExtension.Get("Emulation.DirectionDown")}",
            "RightStickLeft" => $"{LocExtension.Get("Emulation.RightStick")}{separator}{LocExtension.Get("Emulation.DirectionLeft")}",
            "RightStickRight" => $"{LocExtension.Get("Emulation.RightStick")}{separator}{LocExtension.Get("Emulation.DirectionRight")}",
            "ButtonA" => "A", "ButtonB" => "B", "ButtonX" => "X", "ButtonY" => "Y",
            "View" => "View", "Menu" => "Menu", "LeftShoulder" => "LB", "RightShoulder" => "RB",
            "LeftTrigger" => "LT", "RightTrigger" => "RT", "LeftStickClick" => "L3", "RightStickClick" => "R3",
            "XboxButton" => "Xbox",
            _ => input
        };
        return device is null ? inputName : $"{device}{separator}{inputName}";
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
        InputBindingState.Valid => "✓",
        InputBindingState.Conflict => "!",
        InputBindingState.Reserved => "◆",
        _ => "−"
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
