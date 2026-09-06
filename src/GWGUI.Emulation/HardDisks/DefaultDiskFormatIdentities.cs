namespace GWGUI.Emulation.HardDisks;

internal static class DefaultDiskFormatIdentities
{
    internal static DiskFormatIdentity Container(string id) => id switch
    {
        "raw" => new("RAW", "Linear sectors", [".img", ".raw", ".hdf", ".vhd", ".ide", ".hdn", ".dhd", ".d90"]),
        "gzip" => new("gzip", "Compressed linear sectors", [".gz", ".hdz"], "gzip"),
        "vhd" => new("VHD", "Autonomous fixed or dynamic", [".vhd"], "vhd"),
        "vhdx" => new("VHDX", "Autonomous fixed or dynamic, 512-byte logical sectors", [".vhdx"], "vhdx"),
        "vdi" => new("VDI", "Autonomous fixed or dynamic", [".vdi"], "vdi"),
        "vmdk" => new("VMDK", "Monolithic sparse", [".vmdk"], "vmdk"),
        "qcow" => new("QCOW", "Version 1, autonomous", [".qcow"], "qcow"),
        "qcow2" => new("QCOW2", "Version 3, autonomous, 16-bit refcounts", [".qcow2"], "qcow"),
        "qcow2-v2" => new("QCOW2", "Version 2, autonomous, 16-bit refcounts", [".qcow2"], "qcow"),
        "qed" => new("QED", "Autonomous, 64 KiB clusters", [".qed"], "qed"),
        "chd" => new("CHD", "Version 5, uncompressed HDD", [".chd"], "chd"),
        "twoimg" => new("2IMG", "Version 1, linear 512-byte blocks", [".2mg", ".2img"], "twoimg"),
        "parallels" => new("Parallels", "WithouFreSpacExt, version 2", [".hdd"], "parallels"),
        "hdi" => new("HDI", "4096-byte header", [".hdi"]),
        "nhd" => new("NHD", "R0", [".nhd"], "nhd"),
        "thd" => new("THD", "256-byte header", [".thd"]),
        "udif" => new("UDIF", "Version 4, RAW runs", [".dmg"], "udif"),
        "udif-zlib" => new("UDIF", "Version 4, zlib and RAW runs", [".dmg"], "udif"),
        "bochs-v1" => new("Bochs redolog", "Growing, version 1", signatureFamily: "bochs"),
        "bochs-v2" => new("Bochs redolog", "Growing, version 2", signatureFamily: "bochs"),
        "cloop-v2" => new("cloop", "Version 2, zlib", [".cloop"], "cloop"),
        _ => throw new NotSupportedException($"Missing built-in container identity: {id}")
    };

    internal static DiskFormatIdentity FileSystem(string id) => id switch
    {
        "none" => new("None", "Unformatted volume"),
        "bfs" => new("BFS", "Little-endian boot filesystem, 512-byte blocks"),
        "fat" => new("FAT", "Automatic FAT variant, 512-byte sectors"),
        "fat12" => new("FAT", "FAT12"), "fat16" => new("FAT", "FAT16"), "fat32" => new("FAT", "FAT32"),
        "fat16-adapted" => new("FAT", "FAT16 with adapted logical sectors"),
        "fatx" => new("FATX", "Little-endian"), "fatx-be" => new("FATX", "Big-endian"),
        "exfat" => new("exFAT", "512-byte sectors"), "ntfs" => new("NTFS", "Integrated formatter profile"),
        "ofs" => new("OFS", "DOS0"), "ffs" => new("FFS", "DOS1"),
        "ofs-intl" => new("OFS", "DOS2, international"), "ffs-intl" => new("FFS", "DOS3, international"),
        "ofs-dircache" => new("OFS", "DOS4, directory cache"), "ffs-dircache" => new("FFS", "DOS5, directory cache"),
        "mfs" => new("MFS", "Empty volume, Mac Roman label"),
        "hfs" => new("HFS", "Classic, Mac Roman label"),
        "hfsplus" => new("HFS+", "Version 4, not journaled"), "hfsx" => new("HFSX", "Version 5, not journaled"),
        "ext2" => new("ext2", "4 KiB blocks, 128-byte inodes"),
        "ext3" => new("ext3", "4 KiB blocks, internal JBD journal"),
        "ext4" => new("ext4", "4 KiB blocks, extents, internal journal"),
        "minix1" => new("Minix", "Version 1, little-endian, 1 KiB blocks"),
        "minix2" => new("Minix", "Version 2, little-endian, 1 KiB blocks"),
        "minix3" => new("Minix", "Version 3, little-endian, 1 KiB blocks"),
        "v7-le" => new("V7", "Little-endian"), "v7-be" => new("V7", "Big-endian"), "v7-pdp" => new("V7", "PDP-endian"),
        "prodos" => new("ProDOS", "Empty volume, 512-byte blocks"),
        "pascal" => new("Pascal", "Little-endian, four directory blocks"),
        "cpm22" => new("CP/M", "2.2 with explicit volume parameters"),
        "d90-dos" => new("D90 DOS", "153 cylinders, four or six heads"),
        _ => throw new NotSupportedException($"Missing built-in filesystem identity: {id}")
    };

    internal static DiskFormatIdentity PartitionTable(string id) => id switch
    {
        "none" => new("None", "Direct volume"),
        "mbr" => new("MBR", "Primary partitions and LBA EBR chain"),
        "gpt" => new("GPT", "128 entries, 512-byte sectors"),
        "apm" => new("APM", "63 map entries, 512-byte sectors"),
        "ahdi" => new("AHDI", "Primary partitions and XGM chain"),
        "icd" => new("ICD/Supra", "Twelve primary partitions"),
        "rdb" => new("RDB", "512-byte sectors, configurable geometry"),
        "bsd-disklabel" => new("BSD disklabel", "32-bit, little-endian"),
        "bsd-disklabel-be" => new("BSD disklabel", "32-bit, big-endian"),
        "sun-vtoc" => new("Sun disklabel", "VTOC version 1, primary label"),
        "sgi" => new("SGI volume header", "Big-endian, empty header directory"),
        _ => throw new NotSupportedException($"Missing built-in partition-table identity: {id}")
    };
}
