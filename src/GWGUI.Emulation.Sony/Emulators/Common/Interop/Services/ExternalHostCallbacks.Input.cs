using static GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.ControllerInputFunctions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    internal static IReadOnlySet<EmulationKey> SupportedKeyboardKeys { get; } =
        KeyboardMap.Values.ToHashSet();

    private void HandleInputPoll()
    {
        lock (_inputGate)
        {
            _polledInput = _input.Consume();
            _pointerX = AccumulatePointerCoordinate(_pointerX, _polledInput.Pointer.DeltaX);
            _pointerY = AccumulatePointerCoordinate(_pointerY, _polledInput.Pointer.DeltaY);

        }
        PublishKeyboardTransitions(_controllerTypes.Any(type => (ControllerType)type != ControllerType.None) ? _polledInput.Keys : new HashSet<EmulationKey>());
    }

    private void PublishKeyboardTransitions(IReadOnlySet<EmulationKey> keys)
    {
        if (_keyboardEvent is null) return;
        keys = FilterReservedKeyboardChord(keys);
        var reverseMap = KeyboardMap.ToDictionary(pair => pair.Value, pair => pair.Key);
        var modifiers = (ushort)((keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift) ? 1 : 0)
            | (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl) ? 2 : 0)
            | (keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt) ? 4 : 0));
        foreach (var key in _previousKeys.Except(keys).OrderBy(key => IsModifier(key) ? 1 : 0))
            if (reverseMap.TryGetValue(key, out var code)) _keyboardEvent(false, code, 0, modifiers);
        foreach (var key in keys.Except(_previousKeys).OrderBy(key => IsModifier(key) ? 0 : 1))
            if (reverseMap.TryGetValue(key, out var code)) _keyboardEvent(true, code, CharacterFor(code, keys), modifiers);
        _previousKeys = new HashSet<EmulationKey>(keys);
    }

    internal IReadOnlySet<EmulationKey> FilterReservedKeyboardChord(IReadOnlySet<EmulationKey> keys)
    {
        var alt = keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt);
        var enter = keys.Contains(EmulationKey.Return) || keys.Contains(EmulationKey.NumpadEnter);
        if (alt && enter) _suppressFullscreenChord = true;
        if (!_suppressFullscreenChord) return keys;
        if (!alt && !enter) _suppressFullscreenChord = false;
        return keys.Where(key => key is not (EmulationKey.LeftAlt or EmulationKey.RightAlt or EmulationKey.Return or EmulationKey.NumpadEnter)).ToHashSet();
    }

    private static uint CharacterFor(uint code, IReadOnlySet<EmulationKey> keys)
    => GWGUI.Emulation.Functions.ExternalCoreKeyboardFunctions.Character(code, keys);

    private static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt;

    private void HandleLog(int level, nint format, nint firstArgument, nint secondArgument)
    {
        var message = NativeLogFunctions.Read(format, firstArgument, secondArgument);
        if (!string.IsNullOrWhiteSpace(message)) AddDiagnostic($"[{level}] {message.TrimEnd()}");
    }

    private void HandleLed(int led, int state)
    {
        if (led is not (>= 0 and < 256)) return;
        _ledStates[led] = state != 0;
        if (state != 0)
            _ledActivityUntil[led] = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 140 / 1000;
    }

    private void AddDiagnostic(string message)
    {
        DiagnosticMonitor.Capture(message);
        _diagnostics.Enqueue(message);
        while (_diagnostics.Count > 500) _diagnostics.TryDequeue(out _);
    }

    private short HandleInputState(uint port, uint device, uint index, uint id)
    {
        if (_controllerPorts is not null)
        {
            var portDevice = device == LightGunDevice ? LightGunDevice : JoypadDevice;
            var route = _controllerPorts.FirstOrDefault(item => item.NativePort == port && item.NativeDevice == portDevice);
            if (route is null) return default;
            port = checked((uint)route.ConfigurationPort);
        }
        if (port >= _controllerTypes.Length
            || (ControllerType)Volatile.Read(ref _controllerTypes[(int)port]) == ControllerType.None)
            return default;
        var input = _polledInput;
        var controller = port < input.Controllers.Count ? input.Controllers[(int)port] : EmulationControllerState.Empty;
        if (device == KeyboardDevice)
            return KeyboardMap.TryGetValue(id, out var key) && input.Keys.Contains(key) ? (short)1 : (short)0;

        if (device == MouseDevice)
            return (MouseInput)id switch
            {
                MouseInput.X => ClampToShort(input.Pointer.DeltaX),
                MouseInput.Y => ClampToShort(input.Pointer.DeltaY),
                MouseInput.Left => Bool(PortButton(controller, nameof(MouseAction.LeftButton), input.Pointer.Left)),
                MouseInput.Right => Bool(PortButton(controller, nameof(MouseAction.RightButton), input.Pointer.Right)),
                MouseInput.Middle => Bool(PortButton(controller, nameof(MouseAction.MiddleButton), input.Pointer.Middle)),
                MouseInput.WheelUp => Bool(input.Pointer.Wheel > NeutralWheelDelta),
                MouseInput.WheelDown => Bool(input.Pointer.Wheel < NeutralWheelDelta),
                MouseInput.HorizontalWheelUp => Bool(input.Pointer.HorizontalWheel > NeutralWheelDelta),
                MouseInput.HorizontalWheelDown => Bool(input.Pointer.HorizontalWheel < NeutralWheelDelta),
                MouseInput.Button4 => Bool(input.Pointer.ExtendedButton1),
                MouseInput.Button5 => Bool(input.Pointer.ExtendedButton2),
                _ => default
            };

        if (device == PointerDevice && index == PrimaryPointerIndex)
            return id switch
            {
                PointerX => (short)_pointerX,
                PointerY => (short)_pointerY,
                PointerPressed => Bool(input.Pointer.Left),
                PointerCount => input.Pointer.Left ? (short)SinglePointerCount : default,
                PointerOffscreen => Bool(PointerIsOffscreen()),
                _ => default
            };

        if (device == LightGunDevice)
        {
            var axisX = controller.Controls.ContainsKey(EmulationControllerCommandIds.LeftStickLeft) || controller.Controls.ContainsKey(EmulationControllerCommandIds.LeftStickRight);
            var axisY = controller.Controls.ContainsKey(EmulationControllerCommandIds.LeftStickUp) || controller.Controls.ContainsKey(EmulationControllerCommandIds.LeftStickDown);
            var x = axisX ? controller.LeftX : _pointerX;
            var y = axisY ? controller.LeftY : _pointerY;
            return (LightGunInput)id switch
            {
                LightGunInput.ScreenX => (short)x,
                LightGunInput.ScreenY => (short)y,
                LightGunInput.RelativeX => ClampToShort(input.Pointer.DeltaX),
                LightGunInput.RelativeY => ClampToShort(input.Pointer.DeltaY),
                LightGunInput.Offscreen => Bool(x <= PointerCoordinateMinimum || x >= PointerCoordinateMaximum || y <= PointerCoordinateMinimum || y >= PointerCoordinateMaximum),
                _ => Bool(LightGunButton(controller, (LightGunInput)id))
            };
        }

        if (port >= input.Controllers.Count) return 0;
        if (device == JoypadDevice)
        {
            var buttons = AnalogToggleFunctions.ButtonMask(controller, (ControllerType)_controllerTypes[(int)port]);
            if (id == JoypadMask) return unchecked((short)(buttons & ushort.MaxValue));
            return id < ControllerButtonMaskWidth && (buttons & (1u << (int)id)) != EmptyButtonMask ? Bool(true) : default;
        }
        if (device == AnalogDevice)
            return ((AnalogInputIndex)index, (AnalogAxis)id) switch
            {
                (AnalogInputIndex.LeftStick, AnalogAxis.X) => controller.LeftX,
                (AnalogInputIndex.LeftStick, AnalogAxis.Y) => controller.LeftY,
                (AnalogInputIndex.RightStick, AnalogAxis.X) => controller.RightX,
                (AnalogInputIndex.RightStick, AnalogAxis.Y) => controller.RightY,
                (AnalogInputIndex.Button, _) => AnalogButton(controller, (JoypadInput)id),
                _ => default
            };
        return 0;
    }

    private bool PointerIsOffscreen() => _pointerX <= PointerCoordinateMinimum || _pointerX >= PointerCoordinateMaximum || _pointerY <= PointerCoordinateMinimum || _pointerY >= PointerCoordinateMaximum;

    private static short Bool(bool value) => value ? (short)1 : (short)0;
    private static short ClampToShort(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    private static int AccumulatePointerCoordinate(int coordinate, int delta) => (int)Math.Clamp((long)coordinate + (long)delta * PointerCoordinateScale, PointerCoordinateMinimum, PointerCoordinateMaximum);

    private static IReadOnlyDictionary<uint, EmulationKey> CreateKeyboardMap()
    => GWGUI.Emulation.Functions.ExternalCoreKeyboardFunctions.CreateKeyMap().Where(pair => pair.Key is not (EmulationKey.LeftAmiga or EmulationKey.RightAmiga or EmulationKey.Break or EmulationKey.Undo)).ToDictionary(pair => pair.Value, pair => pair.Key);

}
