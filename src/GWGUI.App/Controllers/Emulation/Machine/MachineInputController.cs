using GWGUI.App.Constants.Emulation;
using GWGUI.App.Constants.Input.Windows;
using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Emulation.Shortcuts;
using GWGUI.App.Functions.Emulation.Shortcuts;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.App.Functions.Input.Keyboard;
using GWGUI.App.Input.Mouse;
using GWGUI.App.Services.Input;
using GWGUI.App.Views.Controls.Emulation.Machine;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using GWGUI.Emulation;


namespace GWGUI.App.Controllers.Emulation.Machine;

internal sealed class MachineInputController : IDisposable
{
    private readonly MachineView _view;
    private readonly Func<IEmulatedMachine> _machine;
    private readonly IReadOnlyList<GlobalShortcutBinding> _globalShortcuts;
    private readonly Func<string, Task> _executeShortcut;
    private readonly IReadOnlyList<KeyboardShortcutBinding> _keyboardShortcuts;
    private readonly RelativeMouseCapture _pointerCapture = new();
    private readonly HashSet<EmulationKey> _keys = [];
    private readonly HashSet<Key> _physicalKeys = [];
    private readonly Dictionary<Key, EmulationKey> _pressedShortcutKeys = [];
    private readonly HashSet<string> _activeShortcuts = new(StringComparer.Ordinal);
    private readonly DispatcherTimer _timer = new()
    {
        Interval = EmulationRuntimeConstants.EmulationInputPollingInterval
    };
    private FrameworkElement _inputView;
    private IntPtr _inputHandle;
    private HwndSource? _windowSource;
    private bool _powered;
    private bool _hostTransition;
    private bool _restorePointerAfterHostTransition;
    private bool _disposed;

    internal MachineInputController(MachineView view, FrameworkElement inputView, IntPtr inputHandle,
        Func<IEmulatedMachine> machine, IReadOnlyList<GlobalShortcutBinding> globalShortcuts,
        Func<string, Task> executeShortcut)
    {
        _view = view;
        _inputView = inputView;
        _inputHandle = inputHandle;
        _machine = machine;
        _globalShortcuts = globalShortcuts;
        _executeShortcut = executeShortcut;
        _keyboardShortcuts = EmulationShortcutMap.KeyboardShortcuts(machine().Input.KeyboardBindings);
        Attach();
        _timer.Tick += TimerTick;
        _view.Loaded += ViewLoaded;
        _view.Unloaded += ViewUnloaded;
    }

    internal bool IsPointerCaptured => _pointerCapture.IsCaptured;

    internal void SetPowered(bool powered)
    {
        _powered = powered;
        if (powered) _timer.Start();
        else
        {
            _timer.Stop();
            ReleasePointer();
            _keys.Clear();
            _physicalKeys.Clear();
            _pressedShortcutKeys.Clear();
            _activeShortcuts.Clear();
        }
    }

    internal void SetInputView(FrameworkElement inputView, IntPtr inputHandle)
    {
        ReleasePointer();
        Detach();
        _inputView = inputView;
        _inputHandle = inputHandle;
        Attach();
    }

    internal void ReleasePointer() => _pointerCapture.Release(_inputView, _inputHandle);

    internal void BeginHostTransition()
    {
        _hostTransition = true;
        _restorePointerAfterHostTransition = _pointerCapture.IsCaptured;
    }

    internal void CompleteHostTransition()
    {
        _inputView.Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
        {
            var restorePointer = _restorePointerAfterHostTransition;
            _restorePointerAfterHostTransition = false;
            _hostTransition = false;
            if (!_powered || _disposed) return;
            RelativeMouseCapture.Focus(_inputView, _inputHandle);
            if (restorePointer)
                _pointerCapture.Capture(_inputView, _view.Screen, _inputHandle);
        });
    }

    public void Dispose()
    {
        if (_disposed) return;
        SetPowered(false);
        _timer.Tick -= TimerTick;
        _view.Loaded -= ViewLoaded;
        _view.Unloaded -= ViewUnloaded;
        DetachWindowHook();
        Detach();
        _disposed = true;
    }

    private void Attach()
    {
        _inputView.KeyDown += KeyDown;
        _inputView.KeyUp += KeyUp;
        _inputView.MouseDown += MouseDown;
        _inputView.MouseMove += MouseMove;
        _inputView.MouseWheel += MouseWheel;
        _inputView.LostKeyboardFocus += LostKeyboardFocus;
        if (_inputView is HwndHost host) host.MessageHook += NativeMessage;
    }

    private void Detach()
    {
        _inputView.KeyDown -= KeyDown;
        _inputView.KeyUp -= KeyUp;
        _inputView.MouseDown -= MouseDown;
        _inputView.MouseMove -= MouseMove;
        _inputView.MouseWheel -= MouseWheel;
        _inputView.LostKeyboardFocus -= LostKeyboardFocus;
        if (_inputView is HwndHost host) host.MessageHook -= NativeMessage;
    }

    private void KeyDown(object sender, KeyEventArgs args)
    {
        var source = args.Key == Key.System ? args.SystemKey : args.Key;
        args.Handled = HandleKeyDown(source);
    }

    private void KeyUp(object sender, KeyEventArgs args)
    {
        var source = args.Key == Key.System ? args.SystemKey : args.Key;
        args.Handled = HandleKeyUp(source);
    }

    private bool HandleKeyDown(Key source)
    {
        if (!KeyboardChordFunctions.IsModifierKey(source)) _physicalKeys.Add(source);
        var shortcut = EmulationShortcutFunctions.ResolveGlobal(_globalShortcuts, Keyboard.Modifiers,
            _physicalKeys, source, _activeShortcuts);
        if (shortcut.Category == EmulationShortcutMatchCategory.Global)
        {
            if (shortcut.ShouldExecute && shortcut.Action is not null
                && _activeShortcuts.Add(shortcut.Action)) _ = _executeShortcut(shortcut.Action);
            return true;
        }
        if (shortcut.Category == EmulationShortcutMatchCategory.ReservedForGlobal) return true;
        var machineShortcut = _keyboardShortcuts.FirstOrDefault(binding =>
            KeyboardChordFunctions.Matches(binding.Chord, Keyboard.Modifiers, _physicalKeys));
        if (machineShortcut is not null)
        {
            _pressedShortcutKeys[source] = machineShortcut.EmulationKey;
            _keys.Add(machineShortcut.EmulationKey);
            Publish();
            return true;
        }
        if (!EmulationKeyMapper.TryMap(source, out var key)) return false;
        _keys.Add(key);
        Publish();
        return true;
    }

    private bool HandleKeyUp(Key source)
    {
        _physicalKeys.Remove(source);
        EmulationShortcutFunctions.ReleaseInactive(_activeShortcuts, _globalShortcuts,
            Keyboard.Modifiers, _physicalKeys);
        if (_pressedShortcutKeys.Remove(source, out var shortcutKey))
        {
            _keys.Remove(shortcutKey);
            Publish();
            return true;
        }
        if (!EmulationKeyMapper.TryMap(source, out var key)) return false;
        _keys.Remove(key);
        Publish();
        return true;
    }

    private void MouseDown(object sender, MouseButtonEventArgs args)
    {
        _inputView.Focus();
        if (_machine().Input.SupportsPointerCapture && _machine().Input.CapturePointerOnClick
            && !_pointerCapture.IsCaptured)
            _pointerCapture.Capture(_inputView, _view.Screen, _inputHandle);
        Publish();
    }

    private void MouseMove(object sender, MouseEventArgs args) =>
        _pointerCapture.ProcessMovement(_view.Screen, (x, y) => Publish(x, y));

    private void MouseWheel(object sender, MouseWheelEventArgs args) => Publish(wheel: args.Delta);

    private void LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args)
    {
        if (_hostTransition) return;
        ReleasePointer();
        _keys.Clear();
        _physicalKeys.Clear();
        _pressedShortcutKeys.Clear();
        _activeShortcuts.Clear();
        Publish();
    }

    private void ViewLoaded(object sender, RoutedEventArgs args) => AttachWindowHook();

    private void ViewUnloaded(object sender, RoutedEventArgs args) => DetachWindowHook();

    private void AttachWindowHook()
    {
        if (_windowSource is not null || Window.GetWindow(_view) is not Window window) return;
        _windowSource = PresentationSource.FromVisual(window) as HwndSource;
        _windowSource?.AddHook(WindowMessage);
    }

    private void DetachWindowHook()
    {
        _windowSource?.RemoveHook(WindowMessage);
        _windowSource = null;
    }

    private IntPtr WindowMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != WindowsInputMessages.MouseHorizontalWheel || !_pointerCapture.IsCaptured
            || !_inputView.IsMouseOver) return IntPtr.Zero;
        var delta = unchecked((short)((wParam.ToInt64() >> WindowsInputMessages.WheelHighWordShift)
            & WindowsInputMessages.UnsignedWordMask));
        if (delta != WindowsInputMessages.NeutralWheelDelta) Publish(horizontalWheel: delta);
        return IntPtr.Zero;
    }

    private void TimerTick(object? sender, EventArgs args) => Publish();

    private void Publish(int deltaX = 0, int deltaY = 0, int wheel = 0, int horizontalWheel = 0)
    {
        if (!_powered || _disposed) return;
        var pointer = new EmulationPointerState(
            _pointerCapture.IsCaptured ? deltaX : 0,
            _pointerCapture.IsCaptured ? deltaY : 0,
            _pointerCapture.IsCaptured ? wheel : 0,
            _pointerCapture.IsCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.LeftMouseVirtualKey),
            _pointerCapture.IsCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.RightMouseVirtualKey),
            _pointerCapture.IsCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.MiddleMouseVirtualKey),
            _pointerCapture.IsCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.FirstExtendedMouseVirtualKey),
            _pointerCapture.IsCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.SecondExtendedMouseVirtualKey),
            _pointerCapture.IsCaptured ? horizontalWheel : 0);
        _machine().Input.SetInput(new EmulationInputSnapshot(_keys, pointer, XInputControllerReader.ReadAll()));
    }

    private IntPtr NativeMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (message)
        {
            case WindowsInputMessages.KeyDown:
            case WindowsInputMessages.SystemKeyDown:
                handled = HandleKeyDown(KeyInterop.KeyFromVirtualKey(unchecked((int)wParam.ToInt64())));
                break;
            case WindowsInputMessages.KeyUp:
            case WindowsInputMessages.SystemKeyUp:
                handled = HandleKeyUp(KeyInterop.KeyFromVirtualKey(unchecked((int)wParam.ToInt64())));
                break;
            case WindowsInputMessages.LeftButtonDown:
            case WindowsInputMessages.RightButtonDown:
            case WindowsInputMessages.MiddleButtonDown:
            case WindowsInputMessages.XButtonDown:
                RelativeMouseCapture.FocusNative(hwnd);
                if (_machine().Input.SupportsPointerCapture && _machine().Input.CapturePointerOnClick
                    && !_pointerCapture.IsCaptured)
                    _pointerCapture.Capture(_inputView, _view.Screen, _inputHandle);
                Publish();
                break;
            case WindowsInputMessages.LeftButtonUp:
            case WindowsInputMessages.RightButtonUp:
            case WindowsInputMessages.MiddleButtonUp:
            case WindowsInputMessages.XButtonUp:
                Publish();
                break;
            case WindowsInputMessages.MouseMove when _pointerCapture.IsCaptured:
                _pointerCapture.ProcessMovement(_view.Screen, (x, y) => Publish(x, y));
                break;
            case WindowsInputMessages.MouseWheel when _pointerCapture.IsCaptured:
                Publish(wheel: unchecked((short)((wParam.ToInt64() >> WindowsInputMessages.WheelHighWordShift)
                    & WindowsInputMessages.UnsignedWordMask)));
                break;
            case WindowsInputMessages.MouseHorizontalWheel when _pointerCapture.IsCaptured:
                Publish(horizontalWheel: unchecked((short)((wParam.ToInt64()
                    >> WindowsInputMessages.WheelHighWordShift) & WindowsInputMessages.UnsignedWordMask)));
                break;
            case WindowsInputMessages.SetCursor when _pointerCapture.IsCaptured:
                RelativeMouseCapture.HideNativeCursor();
                handled = true;
                break;
        }
        return IntPtr.Zero;
    }
}
