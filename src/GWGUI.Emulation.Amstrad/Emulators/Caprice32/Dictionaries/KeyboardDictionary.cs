using static GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Dictionaries;

internal static class KeyboardDictionary
{
    internal static IReadOnlyDictionary<uint, EmulationKey?> NativeKeys { get; } = Create();

    private static IReadOnlyDictionary<uint, EmulationKey?> Create()
    {
        var keys = new Dictionary<uint, EmulationKey?>
        {
            [FirstNumpadDigitCode] = EmulationKey.AmstradF0, [NumpadPeriodKeyCode] = EmulationKey.AmstradFPeriod,
            [DeleteKeyCode] = EmulationKey.AmstradClear, [RightAltKeyCode] = EmulationKey.AmstradCopy
        };
        for (var index = 1; index <= 9; index++) keys[(uint)(FirstNumpadDigitCode + index)] = (EmulationKey)((int)EmulationKey.F1 + index - 1);
        return keys;
    }
}