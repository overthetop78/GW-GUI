namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Dictionaries;

internal static class BbcMicroKeyboardDictionary
{
    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultHostKeys =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.AcornF0] = EmulationKey.F10,
            [EmulationKey.AcornCopy] = EmulationKey.Insert,
            [EmulationKey.AcornShiftLock] = EmulationKey.PageDown
        };
}
