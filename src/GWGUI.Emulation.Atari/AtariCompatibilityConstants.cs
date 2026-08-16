namespace GWGUI.Emulation.Atari;

internal static class AtariCompatibilityConstants
{
    internal const int NoControllerPort = 0;
    internal const int OneControllerPort = 1;
    internal const int TwoControllerPorts = 2;
    internal const int FourControllerPorts = 4;
    internal const int EmptyCollectionCount = 0;
    internal const int SingleChoiceCount = 1;

    internal const string ForcedByModelResource = "Emulation.AtariUnavailable.ForcedByModel";
    internal const string NoFpuResource = "Emulation.AtariUnavailable.NoFpu";
    internal const string NoAlternateMemoryResource = "Emulation.AtariUnavailable.NoAlternateMemory";
    internal const string NoFirmwareResource = "Emulation.AtariUnavailable.NoFirmware";
    internal const string NoKeyboardResource = "Emulation.AtariUnavailable.NoKeyboard";
    internal const string NoMouseResource = "Emulation.AtariUnavailable.NoMouse";
    internal const string NoStorageResource = "Emulation.AtariUnavailable.NoStorage";

    internal const string ForcedValueSeparator = ",";
    internal const string CoreManagedValue = "core-managed";
}
