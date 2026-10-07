namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Constants;

internal static class StorageSettingsFunctionsConstants
{
    internal const string Dsk = ".dsk";
    internal const string M3u = ".m3u";
    internal const string Cdt = ".cdt";
    internal const string Tap = ".tap";
    internal const string Voc = ".voc";
    internal const string Cpr = ".cpr";
    internal const string FloppyDriveCountOption = "storage.floppyDriveCount";
    internal const string CassetteDriveEnabledOption = "storage.cassetteDriveEnabled";
    internal const string CartridgeSlotEnabledOption = "storage.cartridgeSlotEnabled";
    internal const string FloppyDriveLabel = "A:";
    internal const string SecondFloppyDriveLabel = "B:";
    internal const string CassetteDriveLabel = "Cassette";
    internal const string CartridgeSlotLabel = "Cartridge";

    internal const string Sna = ".sna";
    internal const string Kcr = ".kcr";

    internal const int NoFloppyDrives = 0;
    internal const int SingleFloppyDrive = 1;
    internal const int MaximumFloppyDrives = 2;
    internal const int FloppyActivityLed = 0;
    internal const int CassetteActivityLed = 1;
    internal const int CartridgeActivityLed = 2;
}
