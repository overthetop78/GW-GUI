namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants;

internal static class RuntimeMediaConstants
{
    internal const string HardfileFormatId = "amiga-hdf";
    internal const string CompressedHardfileFormatId = "amiga-hdz";
    internal const string VirtualHardDiskController = "UAE";
    // Keep classic signed filesystem offsets below 2 GiB, by one sector.
    internal const long MaximumHardfileSizeBytes = 2L * 1024 * 1024 * 1024 - 512;
    internal const long DefaultHardfileSizeBytes = 40L * 1024 * 1024;
    internal const int ConversionCacheHashLength = 16;
    internal const string ConversionIdentitySeparator = "|";
    internal const string ConversionFileNameSeparator = "-";
}

internal static class StorageSettingsFunctionsConstants
{
    internal const string Adf = ".adf";
    internal const string Adz = ".adz";
    internal const string Dms = ".dms";
    internal const string Fdi = ".fdi";
    internal const string Ipf = ".ipf";
    internal const string Scp = ".scp";
    internal const string Hdf = ".hdf";
    internal const string Hdz = ".hdz";
    internal const string Cue = ".cue";
    internal const string Ccd = ".ccd";
    internal const string Chd = ".chd";
    internal const string Nrg = ".nrg";
    internal const string Mds = ".mds";
    internal const string Iso = ".iso";
    internal const string CD0 = "CD0:";
    internal const string GwguiFloppyDriveCount = "gwgui_floppy_drive_count";
    internal const string GwguiHardDriveCount = "gwgui_hard_drive_count";
    internal const string GwguiCdDriveEnabled = "gwgui_cd_drive_enabled";
    internal const string Enabled = "enabled";
    internal const string Disabled = "disabled";
    internal const string CompactDiscSummaryLabel = "CD";
    internal const string DoubleDensity35ModelId = "35dd";
    internal const string DoubleDensity35DisplayName = "3.5\" DD";
    internal const string AdfAdzDmsFdiIpfScp = "*.adf;*.adz;*.dms;*.fdi;*.ipf;*.scp";
    internal const string NominalFloppySpeed = "100";
    internal const string FloppyDriveLabelFormat = "DF{0}:";
    internal const string HardDriveLabelFormat = "DH{0}:";
    internal const string FloppyDriveModelOptionPrefix = "gwgui_floppy_drive_model_";
    internal const int DoubleDensityDiskSizeBytes = 901_120;
}
