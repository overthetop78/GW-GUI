using GWGUI.App.Constants.Input.Bindings;
using GWGUI.App.Constants.Input.Controllers;
using GWGUI.App.Constants.Input.Windows;
using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.ViewModels.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using GWGUI.Emulation;


namespace GWGUI.App.Views.Controls.Emulation.Input;

public partial class InputBindingEditor
{
    private void AssignClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: InputBindingRow row } button) return;
        CancelCapture();
        _captureRow = row;
        _captureButton = button;
        _captureButtonContent = button.Content;
        _capturePressed.Clear();
        _captureOrder.Clear();
        _captureModifiers = ModifierKeys.None;
        _captureDeadlineUtc = DateTime.UtcNow.AddSeconds(15);
        button.Content = new TextBlock
        {
            Text = LocExtension.Get("Emulation.Input.Press"),
            FontSize = 11,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.NoWrap
        };
        button.Focus();
        _controllerBaseline = _captureSources.HasFlag(InputCaptureSources.Controller)
            ? GameInputControllerReader.ReadAll() : [];
        _controllerDetailedBaseline = _captureSources.HasFlag(InputCaptureSources.Controller)
            ? GameInputControllerReader.ReadAllDetailedStates() : [];
        _controllerCaptureTimer.Start();
    }

    private void CaptureKeyDown(object sender, KeyEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Keyboard)) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        _captureModifiers |= Keyboard.Modifiers;
        if (!KeyboardChordFunctions.IsModifierKey(key) && _capturePressed.Add(key)) _captureOrder.Add(key);
        e.Handled = true;
    }

    private void CaptureKeyUp(object sender, KeyEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Keyboard)) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (!KeyboardChordFunctions.IsModifierKey(key)) _capturePressed.Remove(key);
        e.Handled = true;
        if (_captureOrder.Count == 0 || _capturePressed.Count != 0) return;
        var binding = KeyboardChordFunctions.Format(_captureModifiers, _captureOrder);
        _captureRow.Binding = _prefixKeyboardSource ? InputBindingSyntax.Keyboard(binding) : binding;
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
        _captureRow.Binding = InputBindingSyntax.Mouse(button);
        e.Handled = true;
        FinishCapture();
    }

    private void CaptureMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Mouse) || e.Delta == 0) return;
        _captureRow.Binding = InputBindingSyntax.Mouse(e.Delta > 0 ? "WheelUp" : "WheelDown");
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
        if (message != WindowsInputMessages.MouseHorizontalWheel || _captureRow is null ||
            !_captureSources.HasFlag(InputCaptureSources.Mouse)) return IntPtr.Zero;
        var delta = unchecked((short)((wParam.ToInt64() >> 16) & 0xffff));
        if (delta == 0) return IntPtr.Zero;
        _captureRow.Binding = InputBindingSyntax.Mouse(delta > 0 ? "WheelRight" : "WheelLeft");
        handled = true;
        FinishCapture();
        return IntPtr.Zero;
    }

    private void CaptureControllerInput(object? sender, EventArgs e)
    {
        if (_captureRow is null)
        {
            _controllerCaptureTimer.Stop();
            return;
        }
        var remaining = _captureDeadlineUtc - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            CancelCapture();
            return;
        }
        if (_captureButton?.Content is TextBlock prompt)
            prompt.Text = $"{LocExtension.Get("Emulation.Input.Press")} ({Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds))} s)";
        if (!_captureSources.HasFlag(InputCaptureSources.Controller)) return;
        var states = GameInputControllerReader.ReadAll();
        foreach (var state in states)
        {
            var baselineState = _controllerBaseline.FirstOrDefault(item =>
                string.Equals(item.DeviceId, state.DeviceId, StringComparison.OrdinalIgnoreCase))
                ?? EmulationControllerState.Empty;
            var pressed = state.Buttons & ~baselineState.Buttons;
            if (pressed != 0)
            {
                var index = Enumerable.Range(0, ControllerInputConstants.ModernButtonSources.Length)
                    .FirstOrDefault(candidate => (pressed & (1u << candidate)) != 0, -1);
                if (index >= 0)
                {
                    var source = ControllerInputConstants.ModernButtonSources[index]
                        [InputBindingSyntaxConstants.ControllerPrefix.Length..];
                    _captureRow.Binding = InputBindingSyntax.Controller(state.DeviceId, source);
                    ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
                    FinishCapture();
                    return;
                }
            }
            var generic = state.Controls.FirstOrDefault(control =>
                !control.Key.StartsWith("Axis", StringComparison.OrdinalIgnoreCase) &&
                control.Value > .5f && baselineState.Controls.GetValueOrDefault(control.Key) <= .5f);
            if (!string.IsNullOrWhiteSpace(generic.Key))
            {
                _captureRow.Binding = InputBindingSyntax.Controller(state.DeviceId, generic.Key);
                ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
                FinishCapture();
                return;
            }
            var genericAxis = NewlyMovedGenericAxis(state, baselineState);
            if (genericAxis is not null)
            {
                _captureRow.Binding = InputBindingSyntax.Controller(state.DeviceId, genericAxis);
                ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
                FinishCapture();
                return;
            }
            var direction = NewlyMovedDirection(state, baselineState);
            if (direction is null) continue;
            _captureRow.Binding = InputBindingSyntax.Controller(state.DeviceId,
                direction[InputBindingSyntaxConstants.ControllerPrefix.Length..]);
            ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
            FinishCapture();
            return;
        }
        var detailedStates = GameInputControllerReader.ReadAllDetailedStates();
        foreach (var state in detailedStates)
        {
            var baseline = _controllerDetailedBaseline.FirstOrDefault(item =>
                string.Equals(item.DeviceId, state.DeviceId, StringComparison.OrdinalIgnoreCase))
                ?? GameInputLiveState.Empty(state.DeviceId);
            var source = NewlyActivatedDetailedControl(state, baseline);
            if (source is null) continue;
            _captureRow.Binding = InputBindingSyntax.Controller(state.DeviceId, source);
            ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
            FinishCapture();
            return;
        }
        _controllerBaseline = states;
        _controllerDetailedBaseline = detailedStates;
    }

    internal static string? NewlyActivatedDetailedControl(GameInputLiveState current,
        GameInputLiveState baseline)
    {
        var rawController = current.DeviceId.StartsWith("rawgamecontroller:",
            StringComparison.OrdinalIgnoreCase);
        foreach (var control in current.Controls.Where(item => item.Type == GameInputControlType.Button))
        {
            var previous = baseline.Controls.FirstOrDefault(item =>
                item.Type == control.Type && item.Index == control.Index)?.Value ?? 0f;
            if (control.Value >= .5f && previous < .5f)
                return $"Button{control.Index + (rawController ? 1 : 0)}";
        }
        foreach (var control in current.Controls.Where(item => item.Type == GameInputControlType.Axis))
        {
            var previous = baseline.Controls.FirstOrDefault(item =>
                item.Type == control.Type && item.Index == control.Index)?.Value ?? .5f;
            var delta = control.Value - previous;
            if (delta >= .35f) return $"Axis{control.Index}Positive";
            if (delta <= -.35f) return $"Axis{control.Index}Negative";
        }
        return null;
    }

    internal static string? NewlyMovedGenericAxis(EmulationControllerState current,
        EmulationControllerState baseline)
    {
        const float threshold = .35f;
        foreach (var control in current.Controls.Where(item =>
                     item.Key.StartsWith("Axis", StringComparison.OrdinalIgnoreCase)))
        {
            var previous = baseline.Controls.GetValueOrDefault(control.Key, .5f);
            var delta = control.Value - previous;
            if (delta >= threshold) return control.Key + "Positive";
            if (delta <= -threshold) return control.Key + "Negative";
        }
        return null;
    }

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
    }

    private static bool Moved(short current, short baseline, int limit) => limit < 0
        ? current < limit && baseline >= limit
        : current > limit && baseline <= limit;

    private void CancelCapture()
    {
        if (_captureButton is not null)
            _captureButton.Content = _captureButtonContent;
        _controllerCaptureTimer.Stop();
        _capturePressed.Clear();
        _captureOrder.Clear();
        _captureButtonContent = null;
        _captureButton = null;
        _captureRow = null;
    }

    private void FinishCapture()
    {
        if (_captureButton is not null)
        {
            _captureButton.Content = _captureButtonContent;
        }
        _controllerCaptureTimer.Stop();
        _captureButtonContent = null;
        _captureButton = null;
        _captureRow = null;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
