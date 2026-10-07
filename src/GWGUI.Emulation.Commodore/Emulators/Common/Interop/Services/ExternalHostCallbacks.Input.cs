
using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Commodore.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    private void HandleInputPoll()
    {
        lock (_inputGate)
        {
            _polledInput = _pendingInput;
            _pendingInput = _pendingInput with
            {
                Pointer = _pendingInput.Pointer with { DeltaX = InputSnapshotFunctionsConstants.NoPointerMovement, DeltaY = InputSnapshotFunctionsConstants.NoPointerMovement, Wheel = InputSnapshotFunctionsConstants.NoWheelMovement, HorizontalWheel = InputSnapshotFunctionsConstants.NoWheelMovement }
            };
        }
        PublishKeyboardTransitions(_polledInput.Keys);
    }

    private void PublishKeyboardTransitions(IReadOnlySet<EmulationKey> keys)
    {
        if (_keyboardEvent is null) return;
        var reverseMap = KeyboardMap.ToDictionary(pair => pair.Value, pair => pair.Key);
        var modifiers = (ushort)((keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift) ? NativeKeyboardModifier.Shift : NativeKeyboardModifier.None)
            | (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl) ? NativeKeyboardModifier.Control : NativeKeyboardModifier.None)
            | (keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt) ? NativeKeyboardModifier.Alt : NativeKeyboardModifier.None));
        foreach (var key in _previousKeys.Except(keys).OrderBy(key => IsModifier(key)))
            if (reverseMap.TryGetValue(key, out var code)) _keyboardEvent(false, code, NoCharacter, modifiers);
        foreach (var key in keys.Except(_previousKeys).OrderBy(key => !IsModifier(key)))
            if (reverseMap.TryGetValue(key, out var code)) _keyboardEvent(true, code, CharacterFor(code, keys), modifiers);
        _previousKeys = new HashSet<EmulationKey>(keys);
    }

    private static uint CharacterFor(uint code, IReadOnlySet<EmulationKey> keys)
    {
        var shifted = keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);
        var caps = keys.Contains(EmulationKey.CapsLock);
        if (code is >= (uint)NativeKeyboardKey.FirstLetter and <= (uint)NativeKeyboardKey.LastLetter) return shifted ^ caps ? code - AsciiCaseOffset : code;
        if (!shifted) return code is >= FirstPrintableCharacter and <= LastPrintableCharacter ? code : NoCharacter;
        return code switch
        {
            (uint)CharacterDigitZero => CharacterRightParenthesis, (uint)CharacterDigitOne => CharacterExclamationMark, (uint)CharacterDigitTwo => CharacterAtSign, (uint)CharacterDigitThree => CharacterNumberSign, (uint)CharacterDigitFour => CharacterDollarSign,
            (uint)CharacterDigitFive => CharacterPercent, (uint)CharacterDigitSix => CharacterCaret, (uint)CharacterDigitSeven => CharacterAmpersand, (uint)CharacterDigitEight => CharacterAsterisk, (uint)CharacterDigitNine => CharacterLeftParenthesis,
            (uint)CharacterMinus => CharacterUnderscore, (uint)CharacterEquals => CharacterPlus, (uint)CharacterLeftBracket => CharacterLeftBrace, (uint)CharacterRightBracket => CharacterRightBrace, (uint)CharacterBackslash => CharacterVerticalBar,
            (uint)CharacterSemicolon => CharacterColon, (uint)CharacterQuote => CharacterDoubleQuote, (uint)CharacterComma => CharacterLessThan, (uint)CharacterPeriod => CharacterGreaterThan, (uint)CharacterSlash => CharacterQuestionMark,
            (uint)CharacterBackquote => CharacterTilde, _ => code is >= FirstPrintableCharacter and <= LastPrintableCharacter ? code : NoCharacter
        };
    }

    private static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift
        or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt
        or EmulationKey.LeftAmiga or EmulationKey.RightAmiga;

    private void HandleLog(int level, nint format)
    {
        var message = format == nint.Zero ? null : Marshal.PtrToStringUTF8(format);
        if (!string.IsNullOrWhiteSpace(message)) AddDiagnostic(string.Format(LogMessageFormat, level, message.TrimEnd()));
    }

    private void HandleLed(int led, int state)
    {
        if (led is not (>= FirstLedIndex and < MaximumLedCount)) return;
        _ledStates[led] = state != ExternalCoreInteropConstants.InactiveState;
        if (state != ExternalCoreInteropConstants.InactiveState)
            _ledActivityUntil[led] = Stopwatch.GetTimestamp() + Stopwatch.Frequency * LedActivityMilliseconds / MillisecondsPerSecond;
    }

    private void AddDiagnostic(string message)
    {
        _diagnostics.Enqueue(message);
        while (_diagnostics.Count > MaximumDiagnosticCount) _diagnostics.TryDequeue(out _);
    }

    private short HandleInputState(uint port, uint device, uint index, uint id)
    {
        var input = _polledInput;
        if (device == KeyboardDevice)
            return KeyboardMap.TryGetValue(id, out var key) && input.Keys.Contains(key) ? PressedInput : ReleasedInput;

        if (device == MouseDevice && port == ControllerPortConstants.MinimumControllerPort)
            return id switch
            {
                (uint)NativeMouseInput.DeltaX => ClampToShort(input.Pointer.DeltaX),
                (uint)NativeMouseInput.DeltaY => ClampToShort(input.Pointer.DeltaY),
                (uint)NativeMouseInput.LeftButton => Bool(input.Pointer.Left),
                (uint)NativeMouseInput.RightButton => Bool(input.Pointer.Right),
                (uint)NativeMouseInput.WheelUp => Bool(input.Pointer.Wheel > InputSnapshotFunctionsConstants.NoWheelMovement),
                (uint)NativeMouseInput.WheelDown => Bool(input.Pointer.Wheel < InputSnapshotFunctionsConstants.NoWheelMovement),
                (uint)NativeMouseInput.MiddleButton => Bool(input.Pointer.Middle),
                _ => ReleasedInput
            };

        if (port >= input.Controllers.Count) return ReleasedInput;
        var controller = input.Controllers[(int)port];
        if (device == JoypadDevice)
        {
            if (id == JoypadMask) return unchecked((short)(controller.Buttons & ushort.MaxValue));
            return id < ControllerButtonBitCount && (controller.Buttons & (InputSnapshotFunctionsConstants.SingleButtonMask << (int)id)) != InputSnapshotFunctionsConstants.NoPressedButtons ? PressedInput : ReleasedInput;
        }
        if (device == AnalogDevice)
            return (index, id) switch
            {
                ((uint)NativeAnalogStick.Left, (uint)NativeAnalogAxis.X) => controller.LeftX,
                ((uint)NativeAnalogStick.Left, (uint)NativeAnalogAxis.Y) => controller.LeftY,
                ((uint)NativeAnalogStick.Right, (uint)NativeAnalogAxis.X) => controller.RightX,
                ((uint)NativeAnalogStick.Right, (uint)NativeAnalogAxis.Y) => controller.RightY,
                _ => ReleasedInput
            };
        return ReleasedInput;
    }

    private static short Bool(bool value) => value ? PressedInput : ReleasedInput;
    private static short ClampToShort(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    private static int SaturatingAdd(int left, int right) => (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);

    private static IReadOnlyDictionary<uint, EmulationKey> CreateKeyboardMap()
    {
        var map = new Dictionary<uint, EmulationKey>
        {
            [(uint)NativeKeyboardKey.Backspace] = EmulationKey.Backspace, [(uint)NativeKeyboardKey.Tab] = EmulationKey.Tab, [(uint)NativeKeyboardKey.Return] = EmulationKey.Return,
            [(uint)NativeKeyboardKey.Escape] = EmulationKey.Escape, [(uint)NativeKeyboardKey.Space] = EmulationKey.Space, [(uint)NativeKeyboardKey.Comma] = EmulationKey.Comma,
            [(uint)NativeKeyboardKey.Minus] = EmulationKey.Minus, [(uint)NativeKeyboardKey.Period] = EmulationKey.Period, [(uint)NativeKeyboardKey.Slash] = EmulationKey.Slash,
            [(uint)NativeKeyboardKey.Semicolon] = EmulationKey.Semicolon, [(uint)NativeKeyboardKey.Equals] = EmulationKey.Equals, [(uint)NativeKeyboardKey.LeftBracket] = EmulationKey.LeftBracket,
            [(uint)NativeKeyboardKey.Backslash] = EmulationKey.Backslash, [(uint)NativeKeyboardKey.RightBracket] = EmulationKey.RightBracket, [(uint)NativeKeyboardKey.Quote] = EmulationKey.Quote,
            [(uint)NativeKeyboardKey.Backquote] = EmulationKey.Backquote, [(uint)NativeKeyboardKey.Delete] = EmulationKey.Delete,
            [(uint)NativeKeyboardKey.Up] = EmulationKey.Up, [(uint)NativeKeyboardKey.Down] = EmulationKey.Down, [(uint)NativeKeyboardKey.Right] = EmulationKey.Right,
            [(uint)NativeKeyboardKey.Left] = EmulationKey.Left, [(uint)NativeKeyboardKey.Insert] = EmulationKey.Insert, [(uint)NativeKeyboardKey.Home] = EmulationKey.Home,
            [(uint)NativeKeyboardKey.End] = EmulationKey.End, [(uint)NativeKeyboardKey.PageUp] = EmulationKey.PageUp, [(uint)NativeKeyboardKey.PageDown] = EmulationKey.PageDown,
            [(uint)NativeKeyboardKey.CapsLock] = EmulationKey.CapsLock, [(uint)NativeKeyboardKey.RightShift] = EmulationKey.RightShift, [(uint)NativeKeyboardKey.LeftShift] = EmulationKey.LeftShift,
            [(uint)NativeKeyboardKey.RightControl] = EmulationKey.RightControl, [(uint)NativeKeyboardKey.LeftControl] = EmulationKey.LeftControl,
            [(uint)NativeKeyboardKey.RightAlt] = EmulationKey.RightAlt, [(uint)NativeKeyboardKey.LeftAlt] = EmulationKey.LeftAlt,
            [(uint)NativeKeyboardKey.LeftAmiga] = EmulationKey.LeftAmiga, [(uint)NativeKeyboardKey.RightAmiga] = EmulationKey.RightAmiga, [(uint)NativeKeyboardKey.Help] = EmulationKey.Help
        };
        for (var index = BufferConstants.FirstCollectionIndex; index < AlphabetLetterCount; index++) map[(uint)((uint)NativeKeyboardKey.FirstLetter + index)] = (EmulationKey)((int)EmulationKey.A + index);
        for (var index = BufferConstants.FirstCollectionIndex; index < DecimalDigitCount; index++) map[(uint)(CharacterDigitZero + index)] = (EmulationKey)((int)EmulationKey.D0 + index);
        for (var index = BufferConstants.FirstCollectionIndex; index < FunctionKeyCount; index++) map[(uint)((uint)NativeKeyboardKey.FirstFunctionKey + index)] = (EmulationKey)((int)EmulationKey.F1 + index);
        for (var index = BufferConstants.FirstCollectionIndex; index < DecimalDigitCount; index++) map[(uint)((uint)NativeKeyboardKey.FirstNumpadDigit + index)] = (EmulationKey)((int)EmulationKey.Numpad0 + index);
        map[(uint)NativeKeyboardKey.NumpadPeriod] = EmulationKey.NumpadPeriod; map[(uint)NativeKeyboardKey.NumpadDivide] = EmulationKey.NumpadDivide;
        map[(uint)NativeKeyboardKey.NumpadMultiply] = EmulationKey.NumpadMultiply; map[(uint)NativeKeyboardKey.NumpadMinus] = EmulationKey.NumpadMinus;
        map[(uint)NativeKeyboardKey.NumpadPlus] = EmulationKey.NumpadPlus; map[(uint)NativeKeyboardKey.NumpadEnter] = EmulationKey.NumpadEnter;
        return map;
    }

}
