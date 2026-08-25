namespace GWGUI.Emulation.Constants;

internal static class EmulationMediaSlotConstants
{
    internal const int FirstIndex = 0;
    internal const int SecondIndex = 1;
    internal const int ThirdIndex = 2;
    internal const int FourthIndex = 3;
    internal const int HardDiskProtocolValue = 4;
    internal const int CompactDiscProtocolValue = 5;
    internal const int CartridgeProtocolValue = 6;
    internal const int CassetteProtocolValue = 7;
    internal const string FloppyPrefix = "Floppy";
    internal const string HardDiskPrefix = "HardDisk";
    internal const string CompactDiscPrefix = "Cd";
    internal const string CartridgePrefix = "Cartridge";
    internal const string CassettePrefix = "Cassette";
    internal const string MissingProtocolValueMessage =
        "This media slot has no numeric host-protocol representation.";
}
