using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using static GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    internal static IReadOnlySet<EmulationKey> SupportedKeyboardKeys { get; } =
        KeyboardMap.Values.ToHashSet();

    private void HandleInputPoll()
    {
        lock (_inputGate)
        {
            _polledInput = _pendingInput;
            _pointerX = AccumulatePointerCoordinate(_pointerX, _polledInput.Pointer.DeltaX);
            _pointerY = AccumulatePointerCoordinate(_pointerY, _polledInput.Pointer.DeltaY);
            _pendingInput = _pendingInput with
            {
                Pointer = _pendingInput.Pointer with { DeltaX = EmulationHostProtocolConstants.EmptyPointerDelta, DeltaY = EmulationHostProtocolConstants.EmptyPointerDelta, Wheel = EmulationHostProtocolConstants.EmptyPointerDelta, HorizontalWheel = EmulationHostProtocolConstants.EmptyPointerDelta }
            };
        }
        PublishKeyboardTransitions(_polledInput.Keys);
    }

    private void PublishKeyboardTransitions(IReadOnlySet<EmulationKey> keys)
    {
        if (_keyboardEvent is null) return;
        keys = FilterReservedKeyboardChord(keys);
        var reverseMap = KeyboardMap.ToDictionary(pair => pair.Value, pair => pair.Key);
        var modifiers = (ushort)((keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift) ? ShiftModifier : NoKeyboardModifiers)
            | (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl) ? ControlModifier : NoKeyboardModifiers)
            | (keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt) ? AltModifier : NoKeyboardModifiers));
        foreach (var key in _previousKeys.Except(keys).OrderBy(key => IsModifier(key) ? KeyboardOrdinaryKeyPriority : KeyboardModifierPriority))
            if (reverseMap.TryGetValue(key, out var code)) _keyboardEvent(false, code, NoKeyboardCharacter, modifiers);
        foreach (var key in keys.Except(_previousKeys).OrderBy(key => IsModifier(key) ? KeyboardModifierPriority : KeyboardOrdinaryKeyPriority))
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
        return keys.Where(key => key is not (EmulationKey.LeftAlt or EmulationKey.RightAlt
            or EmulationKey.Return or EmulationKey.NumpadEnter)).ToHashSet();
    }

    private static uint CharacterFor(uint code, IReadOnlySet<EmulationKey> keys)
    {
        var shifted = keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);
        var caps = keys.Contains(EmulationKey.CapsLock);
        if (code is >= (uint)ExternalHostCallbacksConstants.LowercaseA and <= (uint)ExternalHostCallbacksConstants.LowercaseZ) return shifted ^ caps ? code - LowercaseToUppercaseOffset : code;
        if (!shifted) return code is >= FirstPrintableCharacter and <= LastPrintableCharacter ? code : NoKeyboardCharacter;
        return code switch
        {
            (uint)ExternalHostCallbacksConstants.DigitZero => ExternalHostCallbacksConstants.ClosingParenthesis, (uint)ExternalHostCallbacksConstants.DigitOne => ExternalHostCallbacksConstants.ExclamationMark, (uint)ExternalHostCallbacksConstants.DigitTwo => ExternalHostCallbacksConstants.AtSign, (uint)ExternalHostCallbacksConstants.DigitThree => ExternalHostCallbacksConstants.HashSign, (uint)ExternalHostCallbacksConstants.DigitFour => ExternalHostCallbacksConstants.DollarSign,
            (uint)ExternalHostCallbacksConstants.DigitFive => ExternalHostCallbacksConstants.PercentSign, (uint)ExternalHostCallbacksConstants.DigitSix => ExternalHostCallbacksConstants.Caret, (uint)ExternalHostCallbacksConstants.DigitSeven => ExternalHostCallbacksConstants.Ampersand, (uint)ExternalHostCallbacksConstants.DigitEight => ExternalHostCallbacksConstants.Asterisk, (uint)ExternalHostCallbacksConstants.DigitNine => ExternalHostCallbacksConstants.OpeningParenthesis,
            (uint)ExternalHostCallbacksConstants.Hyphen => ExternalHostCallbacksConstants.Underscore, (uint)ExternalHostCallbacksConstants.EqualsSign => ExternalHostCallbacksConstants.PlusSign, (uint)ExternalHostCallbacksConstants.OpeningBracket => ExternalHostCallbacksConstants.OpeningBrace, (uint)ExternalHostCallbacksConstants.ClosingBracket => ExternalHostCallbacksConstants.ClosingBrace, (uint)ExternalHostCallbacksConstants.Backslash => ExternalHostCallbacksConstants.VerticalBar,
            (uint)ExternalHostCallbacksConstants.Semicolon => ExternalHostCallbacksConstants.Colon, (uint)ExternalHostCallbacksConstants.Apostrophe => ExternalHostCallbacksConstants.QuotationMark, (uint)ExternalHostCallbacksConstants.Comma => ExternalHostCallbacksConstants.LessThanSign, (uint)ExternalHostCallbacksConstants.Period => ExternalHostCallbacksConstants.GreaterThanSign, (uint)ExternalHostCallbacksConstants.Slash => ExternalHostCallbacksConstants.QuestionMark,
            (uint)ExternalHostCallbacksConstants.Backquote => ExternalHostCallbacksConstants.Tilde, _ => code is >= FirstPrintableCharacter and <= LastPrintableCharacter ? code : NoKeyboardCharacter
        };
    }

    private static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift
        or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt;

    private void HandleLog(int level, nint format)
    {
        var message = format == nint.Zero ? null : Marshal.PtrToStringUTF8(format);
        if (!string.IsNullOrWhiteSpace(message)) AddDiagnostic($"{ExternalHostCallbacksConstants.DiagnosticPrefix}{level}{ExternalHostCallbacksConstants.DiagnosticLevelSuffix}{message.TrimEnd()}");
    }

    private void HandleLed(int led, int state)
    {
        if (led is not (>= BufferConstants.FirstCollectionIndex and < EmulationHostProtocolConstants.MaximumLedStateCount)) return;
        _ledStates[led] = state != ExternalCoreInteropConstants.InactiveState;
        if (state != ExternalCoreInteropConstants.InactiveState)
            _ledActivityUntil[led] = Stopwatch.GetTimestamp() + Stopwatch.Frequency * LedActivityDurationMilliseconds / MillisecondsPerSecond;
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
            return KeyboardMap.TryGetValue(id, out var key) && input.Keys.Contains(key) ? ExternalCoreInteropConstants.PressedInputState : ExternalCoreInteropConstants.ReleasedInputState;

        if (device == MouseDevice && port == ControllerPortConstants.MinimumControllerPort)
            return id switch
            {
                MouseX => ClampToShort(input.Pointer.DeltaX),
                MouseY => ClampToShort(input.Pointer.DeltaY),
                MouseLeftButton => Bool(input.Pointer.Left),
                MouseRightButton => Bool(input.Pointer.Right),
                MouseWheelUp => Bool(input.Pointer.Wheel > EmulationHostProtocolConstants.EmptyPointerDelta),
                MouseWheelDown => Bool(input.Pointer.Wheel < EmulationHostProtocolConstants.EmptyPointerDelta),
                MouseMiddleButton => Bool(input.Pointer.Middle),
                _ => ExternalCoreInteropConstants.ReleasedInputState
            };

        if (device == PointerDevice && port == ControllerPortConstants.MinimumControllerPort)
            return id switch
            {
                PointerX => (short)_pointerX,
                PointerY => (short)_pointerY,
                PointerPressed => Bool(input.Pointer.Left),
                _ => ExternalCoreInteropConstants.ReleasedInputState
            };

        if (port >= input.Controllers.Count) return ExternalCoreInteropConstants.ReleasedInputState;
        var controller = input.Controllers[(int)port];
        if (device == JoypadDevice)
        {
            var buttons = controller.Buttons;
            if (id == JoypadMask) return unchecked((short)(buttons & ushort.MaxValue));
            return id < ControllerButtonBitCount && (buttons & (ControllerButtonMask << (int)id)) != ExternalCoreInteropConstants.NoInputState ? ExternalCoreInteropConstants.PressedInputState : ExternalCoreInteropConstants.ReleasedInputState;
        }
        if (device == AnalogDevice)
            return (index, id) switch
            {
                (LeftAnalogStickIndex, AnalogXAxis) => controller.LeftX,
                (LeftAnalogStickIndex, AnalogYAxis) => controller.LeftY,
                (RightAnalogStickIndex, AnalogXAxis) => controller.RightX,
                (RightAnalogStickIndex, AnalogYAxis) => controller.RightY,
                _ => ExternalCoreInteropConstants.ReleasedInputState
            };
        return ExternalCoreInteropConstants.ReleasedInputState;
    }

    private static short Bool(bool value) => value ? ExternalCoreInteropConstants.PressedInputState : ExternalCoreInteropConstants.ReleasedInputState;
    private static short ClampToShort(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    private static int AccumulatePointerCoordinate(int coordinate, int delta) =>
        (int)Math.Clamp((long)coordinate + (long)delta * PointerCoordinateScale,
            PointerCoordinateMinimum, PointerCoordinateMaximum);
    private static int SaturatingAdd(int left, int right) => (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);

    private static IReadOnlyDictionary<uint, EmulationKey> CreateKeyboardMap()
    {
        var map = new Dictionary<uint, EmulationKey>
        {
            [BackspaceKeyCode] = EmulationKey.Backspace, [TabKeyCode] = EmulationKey.Tab, [ReturnKeyCode] = EmulationKey.Return,
            [EscapeKeyCode] = EmulationKey.Escape, [SpaceKeyCode] = EmulationKey.Space, [CommaKeyCode] = EmulationKey.Comma,
            [MinusKeyCode] = EmulationKey.Minus, [PeriodKeyCode] = EmulationKey.Period, [SlashKeyCode] = EmulationKey.Slash,
            [SemicolonKeyCode] = EmulationKey.Semicolon, [EqualsKeyCode] = EmulationKey.Equals, [LeftBracketKeyCode] = EmulationKey.LeftBracket,
            [BackslashKeyCode] = EmulationKey.Backslash, [RightBracketKeyCode] = EmulationKey.RightBracket, [QuoteKeyCode] = EmulationKey.Quote,
            [BackquoteKeyCode] = EmulationKey.Backquote, [DeleteKeyCode] = EmulationKey.Delete,
            [UpKeyCode] = EmulationKey.Up, [DownKeyCode] = EmulationKey.Down, [RightKeyCode] = EmulationKey.Right,
            [LeftKeyCode] = EmulationKey.Left, [InsertKeyCode] = EmulationKey.Insert, [HomeKeyCode] = EmulationKey.Home,
            [EndKeyCode] = EmulationKey.End, [PageUpKeyCode] = EmulationKey.PageUp, [PageDownKeyCode] = EmulationKey.PageDown,
            [CapsLockKeyCode] = EmulationKey.CapsLock, [RightShiftKeyCode] = EmulationKey.RightShift, [LeftShiftKeyCode] = EmulationKey.LeftShift,
            [RightControlKeyCode] = EmulationKey.RightControl, [LeftControlKeyCode] = EmulationKey.LeftControl,
            [RightAltKeyCode] = EmulationKey.RightAlt, [LeftAltKeyCode] = EmulationKey.LeftAlt, [HelpKeyCode] = EmulationKey.Help
        };
        for (var index = BufferConstants.FirstCollectionIndex; index < AlphabetKeyCount; index++) map[(uint)(ExternalHostCallbacksConstants.LowercaseA + index)] = (EmulationKey)((int)EmulationKey.A + index);
        for (var index = BufferConstants.FirstCollectionIndex; index < DigitKeyCount; index++) map[(uint)(ExternalHostCallbacksConstants.DigitZero + index)] = (EmulationKey)((int)EmulationKey.D0 + index);
        for (var index = BufferConstants.FirstCollectionIndex; index < FunctionKeyCount; index++) map[(uint)(FirstFunctionKeyCode + index)] = (EmulationKey)((int)EmulationKey.F1 + index);
        for (var index = BufferConstants.FirstCollectionIndex; index < DigitKeyCount; index++) map[(uint)(FirstNumpadDigitCode + index)] = (EmulationKey)((int)EmulationKey.Numpad0 + index);
        map[NumpadPeriodKeyCode] = EmulationKey.NumpadPeriod; map[NumpadDivideKeyCode] = EmulationKey.NumpadDivide;
        map[NumpadMultiplyKeyCode] = EmulationKey.NumpadMultiply; map[NumpadMinusKeyCode] = EmulationKey.NumpadMinus;
        map[NumpadPlusKeyCode] = EmulationKey.NumpadPlus; map[NumpadEnterKeyCode] = EmulationKey.NumpadEnter;
        return map;
    }

}
