using static GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Dictionaries;

internal static class KeyboardDictionary
{
    internal static IReadOnlyDictionary<uint, EmulationKey?> NativeKeys { get; } = Create();

    private static IReadOnlyDictionary<uint, EmulationKey?> Create()
    {
        var keys = new Dictionary<uint, EmulationKey?>
        {
            [FirstNumpadDigitCode] = EmulationKey.AmstradF0, [NumpadPeriodKeyCode] = EmulationKey.AmstradFPeriod,
            [LeftAltKeyCode] = EmulationKey.AmstradCopy, [DeleteKeyCode] = null
        };
        for (var index = 1; index <= 9; index++) keys[(uint)(FirstNumpadDigitCode + index)] = (EmulationKey)((int)EmulationKey.F1 + index - 1);
        return keys;
    }
}