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
        if (sender is Button { Tag: InputBindingRow row } button)
            SelectAndStartCapture(row.Id, button);
    }

    private void BeginCapture(InputBindingRow row, Button? button)
    {
        CancelCapture();
        _captureRow = row;
        _captureButton = button;
        _captureButtonContent = button?.Content;
        _capturePressed.Clear();
        _captureOrder.Clear();
        _captureModifiers = ModifierKeys.None;
        _captureModifierState.Reset();
        _captureSourcesPressed.Clear();
        _capturedSources.Clear();
        _captureDeadlineUtc = DateTime.UtcNow.AddSeconds(15);
        if (button is not null)
        {
            button.Content = new TextBlock
            {
                Text = LocExtension.Get("Emulation.Input.Press"),
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.NoWrap
            };
            button.Focus();
        }
        else
        {
            BindingsList.Focus();
        }
        _controllerBaseline = _captureSources.HasFlag(InputCaptureSources.Controller) ? GameInputControllerReader.ReadAll() : [];
        _controllerDetailedBaseline = _captureSources.HasFlag(InputCaptureSources.Controller) ? GameInputControllerReader.ReadAllDetailedStates() : [];
        _controllerAxisBaseline = _controllerBaseline;
        _controllerDetailedAxisBaseline = _controllerDetailedBaseline;
        _controllerCaptureTimer.Start();
    }

    private void CaptureKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (CaptureKeyboardKeyDown(key, Keyboard.Modifiers, e.Timestamp)) e.Handled = true;
    }

    internal bool CaptureKeyboardKeyDown(Key key, ModifierKeys modifiers, int? timestamp = null)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Keyboard)) return false;
        if (KeyboardChordFunctions.IsWindowsReserved(new KeyboardChord(modifiers | KeyboardChordFunctions.ModifierForKey(key), [key])))
        {
            CancelCapture();
            return false;
        }
        if (!_capturePressed.Contains(key)) _captureModifierState.Press(key, timestamp, _capturePressed);
        if (_captureModifierState.HasSyntheticLeftControl)
        {
            _captureOrder.Remove(Key.LeftCtrl);
            _captureModifiers &= ~ModifierKeys.Control;
        }
        modifiers = _captureModifierState.Normalize(modifiers, _capturePressed);
        _captureModifiers |= modifiers | KeyboardChordFunctions.ModifierForKey(key);
        if (_capturePressed.Add(key) && !_captureOrder.Contains(key)) _captureOrder.Add(key);
        return true;
    }

    private void CaptureKeyUp(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (CaptureKeyboardKeyUp(key)) e.Handled = true;
    }

    internal bool CaptureKeyboardKeyUp(Key key)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Keyboard)) return false;
        _capturePressed.Remove(key);
        _captureModifierState.Release(key);
        CompleteReleasedCapture();
        return true;
    }

    internal bool CapturePhysicalSourceDown(string source)
    {
        if (_captureRow is null) return false;
        _capturedSources.Add(source);
        if (!_captureSourcesPressed.Add(source)) return false;
        return true;
    }

    internal bool CapturePhysicalSourceUp(string source)
    {
        if (_captureRow is null) return false;
        _captureSourcesPressed.Remove(source);
        CompleteReleasedCapture();
        return true;
    }

    private void CompleteReleasedCapture()
    {
        if (_captureRow is null || _capturePressed.Count != 0 || _captureSourcesPressed.Count != 0 || _captureOrder.Count + _capturedSources.Count == 0) return;
        var keyboard = KeyboardChordFunctions.Format(_captureModifiers, _captureOrder);
        var parts = new List<string>();
        if (keyboard.Length > 0) parts.Add(_prefixKeyboardSource || _capturedSources.Count > 0 ? InputBindingSyntax.Keyboard(keyboard) : keyboard);
        parts.AddRange(_capturedSources.OrderBy(source => source.StartsWith(InputBindingSyntaxConstants.MousePrefix, StringComparison.OrdinalIgnoreCase) ? 0 : 1).ThenBy(source => source, StringComparer.Ordinal));
        _captureRow.Binding = string.Join("+", parts);
        FinishCapture();
    }

    private void CaptureMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Mouse)) return;
        var button = MouseSource(e.ChangedButton);
        if (button is null) return;
        CapturePhysicalSourceDown(button);
        e.Handled = true;
    }

    private void CaptureMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Mouse)) return;
        var source = MouseSource(e.ChangedButton);
        if (source is not null && CapturePhysicalSourceUp(source)) e.Handled = true;
    }

    private static string? MouseSource(MouseButton button) => button switch
        {
            MouseButton.Left => "Mouse:Left",
            MouseButton.Right => "Mouse:Right",
            MouseButton.Middle => "Mouse:Middle",
            MouseButton.XButton1 => "Mouse:XButton1",
            MouseButton.XButton2 => "Mouse:XButton2",
            _ => null
        };

    private void CaptureMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_captureRow is null || !_captureSources.HasFlag(InputCaptureSources.Mouse) || e.Delta == 0) return;
        var source = InputBindingSyntax.Mouse(e.Delta > 0 ? "WheelUp" : "WheelDown");
        CapturePhysicalSourceDown(source);
        CapturePhysicalSourceUp(source);
        e.Handled = true;
    }

    private void AttachWindowHook()
    {
        if (_windowSource is not null || Window.GetWindow(this) is not Window window) return;
        _windowSource = PresentationSource.FromVisual(window) as HwndSource;
        _windowSource?.AddHook(WindowMessageHook);
        _captureWindow = window;
        _captureWindow.Deactivated += CaptureWindowDeactivated;
    }

    private void DetachWindowHook()
    {
        if (_captureWindow is { } window) window.Deactivated -= CaptureWindowDeactivated;
        _captureWindow = null;
        _windowSource?.RemoveHook(WindowMessageHook);
        _windowSource = null;
    }

    private void CaptureWindowDeactivated(object? sender, EventArgs e) => CancelCapture();

    private IntPtr WindowMessageHook(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != WindowsInputMessages.MouseHorizontalWheel || _captureRow is null ||
            !_captureSources.HasFlag(InputCaptureSources.Mouse)) return IntPtr.Zero;
        var delta = unchecked((short)((wParam.ToInt64() >> 16) & 0xffff));
        if (delta == 0) return IntPtr.Zero;
        var source = InputBindingSyntax.Mouse(delta > 0 ? "WheelRight" : "WheelLeft");
        CapturePhysicalSourceDown(source);
        CapturePhysicalSourceUp(source);
        handled = true;
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
        var detailedStates = GameInputControllerReader.ReadAllDetailedStates();
        foreach (var source in _captureSourcesPressed.Where(source => source.StartsWith(InputBindingSyntaxConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            var device = EmulationSdk.InputProcessing.ParseControllerDeviceId(source);
            var state = EmulationSdk.InputProcessing.ResolveController(device, states, -1);
            var value = EmulationSdk.InputProcessing.ControllerSourceValue(source, state);
            var detailed = detailedStates.FirstOrDefault(item => item.DeviceId == device);
            var name = source[(source.LastIndexOf(':') + 1)..];
            if (detailed is not null && name.StartsWith("Button", StringComparison.OrdinalIgnoreCase) && int.TryParse(name[6..], out var index))
                value = detailed.Controls.FirstOrDefault(control => control.Type == GameInputControlType.Button && control.Index == index - (device?.StartsWith("rawgamecontroller:", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0))?.Value ?? value;
            if (value < .2f) _captureSourcesPressed.Remove(source);
        }
        foreach (var state in states)
        {
            var baselineState = _controllerBaseline.FirstOrDefault(item => string.Equals(item.DeviceId, state.DeviceId, StringComparison.OrdinalIgnoreCase)) ?? EmulationControllerState.Empty;
            var pressed = state.Buttons & ~baselineState.Buttons;
            if (pressed != 0)
            {
                foreach (var index in Enumerable.Range(0, ControllerInputConstants.ModernButtonSources.Length).Where(candidate => (pressed & (1u << candidate)) != 0))
                {
                    var source = ControllerInputConstants.ModernButtonSources[index] [InputBindingSyntaxConstants.ControllerPrefix.Length..];
                    if (!ShouldCaptureStandardControl(state, source)) continue;
                    CapturePhysicalSourceDown(InputBindingSyntax.Controller(state.DeviceId, source));
                    ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
                }
            }
            foreach (var generic in state.Controls.Where(control => !control.Key.StartsWith("Axis", StringComparison.OrdinalIgnoreCase) && control.Value > .5f && baselineState.Controls.GetValueOrDefault(control.Key) <= .5f))
            {
                CapturePhysicalSourceDown(InputBindingSyntax.Controller(state.DeviceId, generic.Key));
                ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
            }
            var axisBaseline = _controllerAxisBaseline.FirstOrDefault(item => string.Equals(item.DeviceId, state.DeviceId, StringComparison.OrdinalIgnoreCase)) ?? EmulationControllerState.Empty;
            var genericAxis = NewlyMovedGenericAxis(state, axisBaseline);
            if (genericAxis is not null)
            {
                CapturePhysicalSourceDown(InputBindingSyntax.Controller(state.DeviceId, genericAxis));
                ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
            }
            var direction = NewlyMovedDirection(state, baselineState);
            if (direction is null || !ShouldCaptureStandardControl(state, direction[InputBindingSyntaxConstants.ControllerPrefix.Length..])) continue;
            CapturePhysicalSourceDown(InputBindingSyntax.Controller(state.DeviceId, direction[InputBindingSyntaxConstants.ControllerPrefix.Length..]));
            ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
        }
        foreach (var state in detailedStates)
        {
            if (_capturedSources.Any(source => string.Equals(EmulationSdk.InputProcessing.ParseControllerDeviceId(source), state.DeviceId, StringComparison.OrdinalIgnoreCase))
                && states.Any(controller => controller.DeviceId == state.DeviceId && (controller.Buttons != 0 || controller.Controls.Count > 0))) continue;
            var baseline = _controllerDetailedBaseline.FirstOrDefault(item => string.Equals(item.DeviceId, state.DeviceId, StringComparison.OrdinalIgnoreCase)) ?? GameInputLiveState.Empty(state.DeviceId);
            var axisBaseline = _controllerDetailedAxisBaseline.FirstOrDefault(item => string.Equals(item.DeviceId, state.DeviceId, StringComparison.OrdinalIgnoreCase)) ?? GameInputLiveState.Empty(state.DeviceId);
            foreach (var source in NewlyActivatedDetailedControls(state, baseline, axisBaseline))
            {
                CapturePhysicalSourceDown(InputBindingSyntax.Controller(state.DeviceId, source));
                ControllerCaptured?.Invoke(this, new ControllerCapturedEventArgs(state.DeviceId));
            }
        }
        _controllerBaseline = states;
        _controllerDetailedBaseline = detailedStates;
        CompleteReleasedCapture();
    }

    internal static string? NewlyActivatedDetailedControl(GameInputLiveState current, GameInputLiveState baseline, GameInputLiveState? axisBaseline = null)
        => NewlyActivatedDetailedControls(current, baseline, axisBaseline).FirstOrDefault();

    internal static bool ShouldCaptureStandardControl(EmulationControllerState state, string source)
    {
        // Raw controls are the canonical identities when the same device also
        // exposes a mapped gamepad view. System buttons have no raw equivalent.
        if (source is "XboxButton" or "Share") return true;
        if (source.StartsWith("DPad", StringComparison.Ordinal) && state.Controls.Keys.Any(key => key.StartsWith("Switch", StringComparison.OrdinalIgnoreCase))) return false;
        var prefix = source.EndsWith("Trigger", StringComparison.Ordinal) || source.Contains("Stick", StringComparison.Ordinal) && !source.EndsWith("Click", StringComparison.Ordinal) ? "Axis" : "Button";
        return !state.Controls.Keys.Any(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    internal static IEnumerable<string> NewlyActivatedDetailedControls(GameInputLiveState current, GameInputLiveState baseline, GameInputLiveState? axisBaseline = null)
    {
        var rawController = current.DeviceId.StartsWith("rawgamecontroller:", StringComparison.OrdinalIgnoreCase);
        foreach (var control in current.Controls.Where(item => item.Type == GameInputControlType.Button))
        {
            var previous = baseline.Controls.FirstOrDefault(item => item.Type == control.Type && item.Index == control.Index)?.Value ?? 0f;
            if (control.Value >= .5f && previous < .5f)
                yield return $"Button{control.Index + (rawController ? 1 : 0)}";
        }
        foreach (var control in current.Controls.Where(item => item.Type == GameInputControlType.Axis))
        {
            var previous = (axisBaseline ?? baseline).Controls.FirstOrDefault(item => item.Type == control.Type && item.Index == control.Index)?.Value ?? .5f;
            var delta = control.Value - previous;
            if (delta >= .35f) yield return $"Axis{control.Index}Positive";
            if (delta <= -.35f) yield return $"Axis{control.Index}Negative";
        }
    }

    internal static string? NewlyMovedGenericAxis(EmulationControllerState current, EmulationControllerState baseline)
    {
        const float threshold = .35f;
        foreach (var control in current.Controls.Where(item => item.Key.StartsWith("Axis", StringComparison.OrdinalIgnoreCase)))
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
            : Moved(current.RightX, baseline.RightX, threshold) ? "Controller:RightStickRight" : Moved(current.RightY, baseline.RightY, -threshold) ? "Controller:RightStickUp" : Moved(current.RightY, baseline.RightY, threshold) ? "Controller:RightStickDown" : null;
    }

    private static bool Moved(short current, short baseline, int limit) => limit < 0 ? current < limit && baseline >= limit : current > limit && baseline <= limit;

    private void CancelCapture()
    {
        if (_captureButton is not null)
            _captureButton.Content = _captureButtonContent;
        _controllerCaptureTimer.Stop();
        _controllerBaseline = [];
        _controllerDetailedBaseline = [];
        _controllerAxisBaseline = [];
        _controllerDetailedAxisBaseline = [];
        _capturePressed.Clear();
        _captureOrder.Clear();
        _captureSourcesPressed.Clear();
        _capturedSources.Clear();
        _captureModifierState.Reset();
        _captureButtonContent = null;
        _captureButton = null;
        _captureRow = null;
    }

    internal void ApplyOppositeAxisBinding(InputBindingRow row)
    {
        if (row.OppositeDirectionId is { } oppositeId
            && EmulationSdk.InputProcessing.OppositeControllerAxisSource(row.Binding) is { } oppositeSource
            && _rows.FirstOrDefault(candidate => candidate.Id == oppositeId) is { } oppositeRow)
            oppositeRow.Binding = oppositeSource;
    }

    private void FinishCapture()
    {
        if (_captureRow is { } capturedRow) ApplyOppositeAxisBinding(capturedRow);
        if (_captureButton is not null)
        {
            _captureButton.Content = _captureButtonContent;
        }
        _controllerCaptureTimer.Stop();
        _controllerBaseline = [];
        _controllerDetailedBaseline = [];
        _controllerAxisBaseline = [];
        _controllerDetailedAxisBaseline = [];
        _capturePressed.Clear();
        _captureOrder.Clear();
        _captureModifierState.Reset();
        _captureSourcesPressed.Clear();
        _capturedSources.Clear();
        _captureButtonContent = null;
        _captureButton = null;
        _captureRow = null;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
