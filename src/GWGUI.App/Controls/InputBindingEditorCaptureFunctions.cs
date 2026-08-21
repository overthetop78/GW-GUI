using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using GWGUI.App.Input;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.App.ViewModels;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public partial class InputBindingEditor
{
    private void AssignClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: InputBindingRow row } button) return;
        _captureRow = row;
        _captureButton = button;
        _captureButtonContent = button.Content;
        _captureButtonHeight = button.Height;
        _capturePressed.Clear();
        _captureOrder.Clear();
        _captureModifiers = ModifierKeys.None;
        button.Content = new TextBlock
        {
            Text = LocExtension.Get("Emulation.Input.Press"),
            FontSize = 11,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        button.Height = 40;
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
                var index = Enumerable.Range(0, ControllerInputMap.ModernButtonSources.Length)
                    .FirstOrDefault(candidate => (pressed & (1u << candidate)) != 0, -1);
                if (index >= 0)
                {
                    _captureRow.Binding = ControllerBinding(port, ControllerInputMap.ModernButtonSources[index][InputBindingSyntax.ControllerPrefix.Length..]);
                    ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(port));
                    FinishCapture();
                    return;
                }
            }

            var direction = NewlyMovedDirection(states[port], port < _controllerBaseline.Count
                ? _controllerBaseline[port]
                : EmulationControllerState.Empty);
            if (direction is null) continue;
            _captureRow.Binding = ControllerBinding(port, direction[InputBindingSyntax.ControllerPrefix.Length..]);
            ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(port));
            FinishCapture();
            return;
        }
        _controllerBaseline = states;
    }

    private static string ControllerBinding(int port, string source) => InputBindingSyntax.Controller(port, source);

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

    private void FinishCapture()
    {
        if (_captureButton is not null)
        {
            _captureButton.Content = _captureButtonContent;
            _captureButton.Height = _captureButtonHeight;
        }
        _controllerCaptureTimer.Stop();
        _captureButtonContent = null;
        _captureButton = null;
        _captureRow = null;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
