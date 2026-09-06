namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>Known data-volume types only. Unrecognized combinations require an explicit type.</summary>
internal static class PartitionTypeDefaults
{
    internal static byte Mbr(DiskVolumePlan volume) => volume.MbrType ?? volume.FileSystemId.ToLowerInvariant() switch
    {
        "none" => 0xda,
        "fat12" => 0x01,
        "fat16" => 0x06,
        "fat32" => 0x0c,
        "ntfs" or "exfat" => 0x07,
        "ext2" or "ext3" or "ext4" => 0x83,
        "minix1" or "minix2" or "minix3" => 0x81,
        "hfs" or "hfsplus" or "hfsx" => 0xaf,
        var id when FileSystems.LinuxSwapVolumeFormatter.IsProfile(id) => 0x82,
        _ => throw new ArgumentException("This filesystem requires an explicit MBR partition type.")
    };

    internal static Guid Gpt(DiskVolumePlan volume) => volume.GptType ?? volume.FileSystemId.ToLowerInvariant() switch
    {
        "none" or "fat" or "fat12" or "fat16" or "fat32" or "ntfs" or "exfat" => new("EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"),
        "ext2" or "ext3" or "ext4" => new("0FC63DAF-8483-4772-8E79-3D69D8477DE4"),
        "hfs" or "hfsplus" or "hfsx" => new("48465300-0000-11AA-AA11-00306543ECAC"),
        var id when FileSystems.LinuxSwapVolumeFormatter.IsProfile(id) => new("0657FD6D-A4AB-43C4-84E5-0933C84B4F4F"),
        _ => throw new ArgumentException("This filesystem requires an explicit GPT partition type.")
    };
}
