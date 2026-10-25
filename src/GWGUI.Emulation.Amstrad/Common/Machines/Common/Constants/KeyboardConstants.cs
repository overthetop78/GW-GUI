namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Constants;

internal static class KeyboardConstants
{
    internal const string Copy = "COPY";
    internal const string Clear = "CLR";
    internal const string F0 = "F0";
    internal const string FPeriod = "F.";
    internal static readonly IReadOnlyList<EmulationKey> SpecialKeys = [EmulationKey.AmstradCopy, EmulationKey.AmstradClear, EmulationKey.AmstradF0, EmulationKey.AmstradFPeriod];
}
