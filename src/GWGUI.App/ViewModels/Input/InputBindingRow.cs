using GWGUI.App.Constants.Input.Bindings;
using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.Emulation;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;


namespace GWGUI.App.ViewModels.Input;

public sealed class InputBindingRow(string id, string label, string binding, string defaultBinding) : INotifyPropertyChanged
{
    private string _binding = binding;
    private InputBindingState _state;

    public string Id { get; } = id;
    public string Label { get; } = label;
    public string DefaultBinding { get; } = defaultBinding;
    public string Binding
    {
        get => _binding;
        set
        {
            _binding = value;
            OnChanged();
            OnChanged(nameof(BindingParts));
            OnChanged(nameof(ControllerDeviceName));
            OnChanged(nameof(ControllerDeviceNameVisibility));
        }
    }
    public string ControllerDeviceName
    {
        get
        {
            var deviceId = EmulationInputMappingFunctions.ParseControllerDeviceId(_binding);
            if (string.IsNullOrWhiteSpace(deviceId)) return string.Empty;
            return GameInputControllerReader.GetControllerName(deviceId) ?? deviceId;
        }
    }
    public Visibility ControllerDeviceNameVisibility => string.IsNullOrWhiteSpace(ControllerDeviceName)
        ? Visibility.Collapsed : Visibility.Visible;
    public IReadOnlyList<InputBindingPart> BindingParts
    {
        get
        {
            var parts = _binding.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return parts.Select((part, index) => new InputBindingPart(DisplayPart(part),
                index < parts.Length - 1 ? Visibility.Visible : Visibility.Collapsed)).ToArray();
        }
    }

    public InputBindingState State { get => _state; private set { _state = value; OnChanged(); } }
    public string StateText => LocExtension.Get(State switch
    {
        InputBindingState.Valid => "Emulation.Input.Binding.LabelValid",
        InputBindingState.Conflict => "Emulation.Input.Binding.LabelConflict",
        InputBindingState.Reserved => "Emulation.Input.Binding.LabelReserved",
        _ => "Emulation.Input.Binding.LabelUnassigned"
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
        OnChanged(nameof(StateText));
        OnChanged(nameof(StateForeground));
        OnChanged(nameof(StateBackground));
        OnChanged(nameof(StateIcon));
    }

    private static string DisplayPart(string part)
    {
        if (InputBindingSyntax.TryRemovePrefix(part, InputBindingSyntaxConstants.KeyboardPrefix, out var keyboardSource)) return keyboardSource;
        if (InputBindingSyntax.TryRemovePrefix(part, InputBindingSyntaxConstants.ControllerPrefix, out var controllerSource))
            return DisplayControllerPart(controllerSource);
        if (!InputBindingSyntax.TryRemovePrefix(part, InputBindingSyntaxConstants.MousePrefix, out var mouseSource)) return part;
        return mouseSource.ToLowerInvariant() switch
        {
            "left" => LocExtension.Get("Emulation.Mouse.Button.Left"),
            "right" => LocExtension.Get("Emulation.Mouse.Button.Right"),
            "middle" => LocExtension.Get("Emulation.Mouse.Button.Middle"),
            "xbutton1" => LocExtension.Get("Emulation.Mouse.Button.4"),
            "xbutton2" => LocExtension.Get("Emulation.Mouse.Button.5"),
            "wheelup" => LocExtension.Get("Emulation.Mouse.Wheel.Up"),
            "wheeldown" => LocExtension.Get("Emulation.Mouse.Wheel.Down"),
            "wheelleft" => LocExtension.Get("Emulation.Mouse.Wheel.Left"),
            "wheelright" => LocExtension.Get("Emulation.Mouse.Wheel.Right"),
            _ => part
        };
    }

    private static string DisplayControllerPart(string source)
    {
        const string separator = " · ";
        var segments = source.Split(':', StringSplitOptions.RemoveEmptyEntries);
        var input = segments[^1];
        var device = segments.Length >= 3 && segments[0].Equals("xinput", StringComparison.OrdinalIgnoreCase)
                     && int.TryParse(segments[1], out var port) ? $"X{port + 1}" : null;
        var inputName = input switch
        {
            "DPadUp" => $"D-pad{separator}{LocExtension.Get("Emulation.Controller.Action.Up")}",
            "DPadDown" => $"D-pad{separator}{LocExtension.Get("Emulation.Controller.Action.Down")}",
            "DPadLeft" => $"D-pad{separator}{LocExtension.Get("Emulation.Controller.Action.Left")}",
            "DPadRight" => $"D-pad{separator}{LocExtension.Get("Emulation.Controller.Action.Right")}",
            "LeftStickUp" => $"{LocExtension.Get("Emulation.Controller.Stick.Left")}{separator}{LocExtension.Get("Emulation.Controller.Action.Up")}",
            "LeftStickDown" => $"{LocExtension.Get("Emulation.Controller.Stick.Left")}{separator}{LocExtension.Get("Emulation.Controller.Action.Down")}",
            "LeftStickLeft" => $"{LocExtension.Get("Emulation.Controller.Stick.Left")}{separator}{LocExtension.Get("Emulation.Controller.Action.Left")}",
            "LeftStickRight" => $"{LocExtension.Get("Emulation.Controller.Stick.Left")}{separator}{LocExtension.Get("Emulation.Controller.Action.Right")}",
            "RightStickUp" => $"{LocExtension.Get("Emulation.Controller.Stick.Right")}{separator}{LocExtension.Get("Emulation.Controller.Action.Up")}",
            "RightStickDown" => $"{LocExtension.Get("Emulation.Controller.Stick.Right")}{separator}{LocExtension.Get("Emulation.Controller.Action.Down")}",
            "RightStickLeft" => $"{LocExtension.Get("Emulation.Controller.Stick.Right")}{separator}{LocExtension.Get("Emulation.Controller.Action.Left")}",
            "RightStickRight" => $"{LocExtension.Get("Emulation.Controller.Stick.Right")}{separator}{LocExtension.Get("Emulation.Controller.Action.Right")}",
            "ButtonA" => "A", "ButtonB" => "B", "ButtonX" => "X", "ButtonY" => "Y",
            "View" => "View", "Menu" => "Menu", "LeftShoulder" => "LB", "RightShoulder" => "RB",
            "LeftTrigger" => "LT", "RightTrigger" => "RT", "LeftStickClick" => "L3", "RightStickClick" => "R3",
            "XboxButton" => "Xbox",
            _ => input
        };
        return device is null ? inputName : $"{device}{separator}{inputName}";
    }

    private void OnChanged([CallerMemberName] string? property = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
}
