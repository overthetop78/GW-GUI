namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Constants;

internal static class FirmwareConstants
{
    internal const string SystemSubdirectory = "WQXEmu";
    internal const long Nc1020RomMinimumBytes = 0x8000L * 0x300;
    internal const long Pc1000Rom12MBytes = 0x8000L * 128 * 3;
    internal const long Pc1000Rom16MBytes = 0x1000000;
    internal const long Cc800RomMinimumBytes = 0x1000000;
    internal const long Nor512KBytes = 0x80000;
    internal const long Nor1MBytes = 0x100000;
}