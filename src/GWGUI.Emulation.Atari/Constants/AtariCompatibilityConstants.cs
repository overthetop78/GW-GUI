namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariCompatibilityConstants
{
    internal const int NoControllerPort = 0;
    internal const int OneControllerPort = 1;
    internal const int TwoControllerPorts = 2;
    internal const int FourControllerPorts = 4;
    internal const int EmptyCollectionCount = 0;
    internal const int SingleChoiceCount = 1;

    internal const string ForcedByModelResource = "Emulation.Atari.Unavailable.ForcedByModel";
    internal const string NoFpuResource = "Emulation.Atari.Unavailable.NoFpu";
    internal const string NoAlternateMemoryResource = "Emulation.Atari.Unavailable.NoAlternateMemory";
    internal const string NoFirmwareResource = "Emulation.Atari.Unavailable.NoFirmware";
    internal const string NoKeyboardResource = "Emulation.Atari.Unavailable.NoKeyboard";
    internal const string NoMouseResource = "Emulation.Atari.Unavailable.NoMouse";
    internal const string NoStorageResource = "Emulation.Atari.Unavailable.NoStorage";
    internal const string JaguarStandardNoCdResource = "Emulation.Atari.Unavailable.JaguarStandardNoCd";

    internal const string ForcedValueSeparator = ",";
    internal const string CoreManagedValue = "core-managed";
}
