namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;

internal static class BbcMicroKeyboardConstants
{
    internal static readonly IReadOnlyList<EmulationKey> SpecialKeys = [EmulationKey.AcornF0, EmulationKey.AcornCopy, EmulationKey.AcornShiftLock];

    internal static readonly IReadOnlyList<EmulationKey> MasterSpecialKeys = [.. SpecialKeys, EmulationKey.AcornKeypadHash, EmulationKey.AcornKeypadDelete, EmulationKey.AcornKeypadComma];

    internal const string F0Resource = "Emulation.Acorn.Key.F0";
    internal const string CopyResource = "Emulation.Acorn.Key.Copy";
    internal const string ShiftLockResource = "Emulation.Acorn.Key.ShiftLock";
    internal const string KeypadHashResource = "Emulation.Acorn.Key.KeypadHash";
    internal const string KeypadDeleteResource = "Emulation.Acorn.Key.KeypadDelete";
    internal const string KeypadCommaResource = "Emulation.Acorn.Key.KeypadComma";
}
