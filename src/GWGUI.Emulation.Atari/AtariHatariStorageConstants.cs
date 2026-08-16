namespace GWGUI.Emulation.Atari;

internal static class AtariHatariStorageConstants
{
    internal const string AcsiExtension = ".vhd";
    internal const string IdeExtension = ".ide";
    internal const string GemdosMarkerExtension = ".GEM";
    internal const string HardDriveWriteProtectionOption = "hatari_writeprotect_hd";
    internal const string WriteProtectionEnabled = "on";
    internal const string WriteProtectionDisabled = "off";
    internal const string DefaultGemdosMountPoint = "C";
    internal const char FirstGemdosPartitionLetter = 'C';
    internal const char LastGemdosPartitionLetter = 'Z';
    internal const int PartitionDirectoryNameLength = 1;
    internal const int FirstStorageIndex = 0;
    internal const int MaximumPrimaryStorageCount = 1;
}
