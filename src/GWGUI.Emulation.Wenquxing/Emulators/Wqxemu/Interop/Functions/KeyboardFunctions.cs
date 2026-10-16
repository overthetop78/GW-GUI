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
            [EmulationKey.WenquxingPower] = model == MachineModel.NC1020 ? KeyboardConstants.Delete : KeyboardConstants.NativeF12,
            [EmulationKey.WenquxingInputMethod] = KeyboardConstants.NativeInputMethod,
            [EmulationKey.WenquxingHelp] = KeyboardConstants.NativeHelp
        };
        if (model == MachineModel.NC3000)
        {
            map[EmulationKey.WenquxingGame] = KeyboardConstants.NativeF5;
            map[EmulationKey.WenquxingCalculator] = KeyboardConstants.NativeF6;
            map[EmulationKey.WenquxingClock] = KeyboardConstants.NativeF7;
            map[EmulationKey.WenquxingDictionary] = KeyboardConstants.NativeF9;
            map[EmulationKey.WenquxingLexicon] = KeyboardConstants.NativeF10;
            map[EmulationKey.WenquxingStudy] = KeyboardConstants.NativeF11;
        }
        else
        {
            map[EmulationKey.WenquxingDictionary] = KeyboardConstants.NativeF5;
            map[EmulationKey.WenquxingCard] = KeyboardConstants.NativeF6;
            map[EmulationKey.WenquxingCalculator] = KeyboardConstants.NativeF7;
            map[EmulationKey.WenquxingMemo] = KeyboardConstants.NativeF8;
            map[EmulationKey.WenquxingData] = KeyboardConstants.NativeF9;
            map[EmulationKey.WenquxingClock] = KeyboardConstants.NativeF10;
            map[EmulationKey.WenquxingNetwork] = KeyboardConstants.NativeF11;
        }
        AddRange(map, EmulationKey.A, KeyboardConstants.FirstLetter, KeyboardConstants.LetterCount);
        AddRange(map, EmulationKey.D0, KeyboardConstants.FirstDigit, KeyboardConstants.DigitCount);
        AddRange(map, EmulationKey.F1, KeyboardConstants.FirstFunctionKey, KeyboardConstants.CommonFunctionKeyCount);
        return map;
    }

    private static void AddRange(Dictionary<EmulationKey, uint> map, EmulationKey first, uint nativeFirst, int count)
    {
        for (var index = 0; index < count; index++)
            map.Add((EmulationKey)((int)first + index), nativeFirst + (uint)index);
    }
}
