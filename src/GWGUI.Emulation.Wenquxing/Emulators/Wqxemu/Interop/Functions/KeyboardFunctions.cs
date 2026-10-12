using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Constants;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Functions;
internal static class KeyboardFunctions
{
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap(MachineModel model)
    {
        if (!Enum.IsDefined(model))
            throw new ArgumentOutOfRangeException(nameof(model));
        var map = new Dictionary<EmulationKey, uint>
        {
            [EmulationKey.Return] = KeyboardConstants.Return,
            [EmulationKey.Escape] = KeyboardConstants.Escape,
            [EmulationKey.Space] = KeyboardConstants.Space,
            [EmulationKey.Backspace] = KeyboardConstants.Backspace,
            [EmulationKey.Up] = KeyboardConstants.Up,
            [EmulationKey.Down] = KeyboardConstants.Down,
            [EmulationKey.Left] = KeyboardConstants.Left,
            [EmulationKey.Right] = KeyboardConstants.Right,
            [EmulationKey.PageUp] = KeyboardConstants.PageUp,
            [EmulationKey.PageDown] = KeyboardConstants.PageDown,
            [EmulationKey.Period] = '.',
            [EmulationKey.LeftShift] = KeyboardConstants.NativeShift,
            [EmulationKey.RightShift] = KeyboardConstants.NativeShift,
            [EmulationKey.WenquxingDictionary] = KeyboardConstants.FirstFunctionKey + 4,
            [EmulationKey.WenquxingCard] = KeyboardConstants.FirstFunctionKey + 5,
            [EmulationKey.WenquxingCalculator] = KeyboardConstants.FirstFunctionKey + 6,
            [EmulationKey.WenquxingMemo] = KeyboardConstants.FirstFunctionKey + 7,
            [EmulationKey.WenquxingData] = KeyboardConstants.FirstFunctionKey + 8,
            [EmulationKey.WenquxingClock] = KeyboardConstants.FirstFunctionKey + 9,
            [EmulationKey.WenquxingNetwork] = KeyboardConstants.FirstFunctionKey + 10,
            [EmulationKey.WenquxingPower] = model == MachineModel.NC1020 ? KeyboardConstants.Delete : KeyboardConstants.FirstFunctionKey + 11,
            [EmulationKey.WenquxingInputMethod] = KeyboardConstants.NativeInputMethod,
            [EmulationKey.WenquxingHelp] = KeyboardConstants.NativeHelp
        };
        AddRange(map, EmulationKey.A, KeyboardConstants.FirstLetter, KeyboardConstants.LetterCount);
        AddRange(map, EmulationKey.D0, KeyboardConstants.FirstDigit, KeyboardConstants.DigitCount);
        AddRange(map, EmulationKey.F1, KeyboardConstants.FirstFunctionKey, KeyboardConstants.CommonFunctionKeyCount);
        return map;
    }

    private static void AddRange(
        Dictionary<EmulationKey, uint> map,
        EmulationKey first,
        uint nativeFirst,
        int count
    )
    {
        for (var index = 0; index < count; index++)
            map.Add((EmulationKey)((int)first + index), nativeFirst + (uint)index);
    }
}