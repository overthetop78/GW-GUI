using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Contracts;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Factories;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

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
        _diagnostics.Enqueue(message);
        while (_diagnostics.Count > 500) _diagnostics.TryDequeue(out _);
    }

    private short HandleInputState(uint port, uint device, uint index, uint id)
    {
        if (port >= _controllerTypes.Length
            || (ControllerType)Volatile.Read(ref _controllerTypes[(int)port]) == ControllerType.None)
            return default;
        var input = _polledInput;
        if (device == KeyboardDevice)
            return KeyboardMap.TryGetValue(id, out var key) && input.Keys.Contains(key) ? (short)1 : (short)0;

        if (device == MouseDevice && port == 0)
            return id switch
            {
                0 => ClampToShort(input.Pointer.DeltaX),
                1 => ClampToShort(input.Pointer.DeltaY),
                2 => Bool(input.Pointer.Left),
                3 => Bool(input.Pointer.Right),
                4 => Bool(input.Pointer.Wheel > 0),
                5 => Bool(input.Pointer.Wheel < 0),
                6 => Bool(input.Pointer.Middle),
                _ => 0
            };

        if (device == PointerDevice && port == 0)
            return id switch
            {
                PointerX => (short)_pointerX,
                PointerY => (short)_pointerY,
                PointerPressed => Bool(input.Pointer.Left),
                _ => 0
            };

        if (port >= input.Controllers.Count) return 0;
        var controller = input.Controllers[(int)port];
        if (device == JoypadDevice)
        {
            var buttons = controller.Buttons;
            if (id == JoypadMask) return unchecked((short)(buttons & ushort.MaxValue));
            return id < 32 && (buttons & (1u << (int)id)) != 0 ? (short)1 : (short)0;
        }
        if (device == AnalogDevice)
            return (index, id) switch
            {
                (0, 0) => controller.LeftX,
                (0, 1) => controller.LeftY,
                (1, 0) => controller.RightX,
                (1, 1) => controller.RightY,
                _ => 0
            };
        return 0;
    }

    private static short Bool(bool value) => value ? (short)1 : (short)0;
    private static short ClampToShort(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    private static int AccumulatePointerCoordinate(int coordinate, int delta) => (int)Math.Clamp((long)coordinate + (long)delta * PointerCoordinateScale, PointerCoordinateMinimum, PointerCoordinateMaximum);

    private static IReadOnlyDictionary<uint, EmulationKey> CreateKeyboardMap()
    => GWGUI.Emulation.Functions.ExternalCoreKeyboardFunctions.CreateKeyMap().Where(pair => pair.Key is not (EmulationKey.LeftAmiga or EmulationKey.RightAmiga or EmulationKey.Break or EmulationKey.Undo)).ToDictionary(pair => pair.Value, pair => pair.Key);

}
