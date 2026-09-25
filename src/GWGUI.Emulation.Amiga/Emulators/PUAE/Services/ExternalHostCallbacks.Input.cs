using GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Contracts;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Factories;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Functions;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Amiga.Emulators.PUAE.Constants.AmigaExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

internal sealed partial class ExternalHostCallbacks
{
    private void HandleInputPoll()
    {
        lock (_inputGate)
        {
            _polledInput = _pendingInput;
            _pendingInput = _pendingInput with
            {
                Pointer = _pendingInput.Pointer with { DeltaX = 0, DeltaY = 0, Wheel = 0, HorizontalWheel = 0 }
            };
        }
        PublishKeyboardTransitions(_polledInput.Keys);
    }

    private void PublishKeyboardTransitions(IReadOnlySet<EmulationKey> keys)
    {
        if (_keyboardEvent is null) return;
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

    private static uint CharacterFor(uint code, IReadOnlySet<EmulationKey> keys)
    {
        var shifted = keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);
        var caps = keys.Contains(EmulationKey.CapsLock);
        if (code is >= (uint)'a' and <= (uint)'z') return shifted ^ caps ? code - 32 : code;
        if (!shifted) return code is >= 32 and <= 126 ? code : 0;
        return code switch
        {
            (uint)'0' => ')', (uint)'1' => '!', (uint)'2' => '@', (uint)'3' => '#', (uint)'4' => '$',
            (uint)'5' => '%', (uint)'6' => '^', (uint)'7' => '&', (uint)'8' => '*', (uint)'9' => '(',
            (uint)'-' => '_', (uint)'=' => '+', (uint)'[' => '{', (uint)']' => '}', (uint)'\\' => '|',
            (uint)';' => ':', (uint)'\'' => '"', (uint)',' => '<', (uint)'.' => '>', (uint)'/' => '?',
            (uint)'`' => '~', _ => code is >= 32 and <= 126 ? code : 0
        };
    }

    private static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift
        or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt
        or EmulationKey.LeftAmiga or EmulationKey.RightAmiga;

    private void HandleLog(int level, nint format)
    {
        var message = format == 0 ? null : Marshal.PtrToStringUTF8(format);
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

        if (port >= input.Controllers.Count) return 0;
        var controller = input.Controllers[(int)port];
        if (device == JoypadDevice)
        {
            if (id == JoypadMask) return unchecked((short)(controller.Buttons & ushort.MaxValue));
            return id < 32 && (controller.Buttons & (1u << (int)id)) != 0 ? (short)1 : (short)0;
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
    private static int SaturatingAdd(int left, int right) => (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);

    private static IReadOnlyDictionary<uint, EmulationKey> CreateKeyboardMap()
    {
        var map = new Dictionary<uint, EmulationKey>
        {
            [8] = EmulationKey.Backspace, [9] = EmulationKey.Tab, [13] = EmulationKey.Return,
            [27] = EmulationKey.Escape, [32] = EmulationKey.Space, [44] = EmulationKey.Comma,
            [45] = EmulationKey.Minus, [46] = EmulationKey.Period, [47] = EmulationKey.Slash,
            [59] = EmulationKey.Semicolon, [61] = EmulationKey.Equals, [91] = EmulationKey.LeftBracket,
            [92] = EmulationKey.Backslash, [93] = EmulationKey.RightBracket, [39] = EmulationKey.Quote,
            [96] = EmulationKey.Backquote, [127] = EmulationKey.Delete,
            [273] = EmulationKey.Up, [274] = EmulationKey.Down, [275] = EmulationKey.Right,
            [276] = EmulationKey.Left, [277] = EmulationKey.Insert, [278] = EmulationKey.Home,
            [279] = EmulationKey.End, [280] = EmulationKey.PageUp, [281] = EmulationKey.PageDown,
            [301] = EmulationKey.CapsLock, [303] = EmulationKey.RightShift, [304] = EmulationKey.LeftShift,
            [305] = EmulationKey.RightControl, [306] = EmulationKey.LeftControl,
            [307] = EmulationKey.RightAlt, [308] = EmulationKey.LeftAlt,
            [311] = EmulationKey.LeftAmiga, [312] = EmulationKey.RightAmiga, [315] = EmulationKey.Help
        };
        for (var index = 0; index < 26; index++) map[(uint)('a' + index)] = (EmulationKey)((int)EmulationKey.A + index);
        for (var index = 0; index < 10; index++) map[(uint)('0' + index)] = (EmulationKey)((int)EmulationKey.D0 + index);
        for (var index = 0; index < 10; index++) map[(uint)(282 + index)] = (EmulationKey)((int)EmulationKey.F1 + index);
        for (var index = 0; index < 10; index++) map[(uint)(256 + index)] = (EmulationKey)((int)EmulationKey.Numpad0 + index);
        map[266] = EmulationKey.NumpadPeriod; map[267] = EmulationKey.NumpadDivide;
        map[268] = EmulationKey.NumpadMultiply; map[269] = EmulationKey.NumpadMinus;
        map[270] = EmulationKey.NumpadPlus; map[271] = EmulationKey.NumpadEnter;
        return map;
    }

}
