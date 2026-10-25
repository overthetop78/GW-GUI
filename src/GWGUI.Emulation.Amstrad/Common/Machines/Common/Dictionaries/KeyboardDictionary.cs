namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

internal static class KeyboardDictionary
{
    internal static readonly IReadOnlyDictionary<EmulationKey, string> Labels = new Dictionary<EmulationKey, string>
    {
        [EmulationKey.AmstradCopy] = KeyboardConstants.Copy,
        [EmulationKey.AmstradClear] = KeyboardConstants.Clear,
        [EmulationKey.AmstradF0] = KeyboardConstants.F0,
        [EmulationKey.AmstradFPeriod] = KeyboardConstants.FPeriod
    };
    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultHostKeys = new Dictionary<EmulationKey, EmulationKey>
    {
        [EmulationKey.AmstradCopy] = EmulationKey.RightAlt,
        [EmulationKey.AmstradClear] = EmulationKey.Delete,
        [EmulationKey.AmstradF0] = EmulationKey.Numpad0,
        [EmulationKey.AmstradFPeriod] = EmulationKey.NumpadPeriod
    };
}
