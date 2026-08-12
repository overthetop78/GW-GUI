namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

/// <summary>Définit la disposition binaire du MDB, de la carte et du répertoire MFS.</summary>
internal static class MacMfsFileSystemLayout
{
    /// <summary>Définit la valeur MFS nommée <c>SystemName</c>.</summary>
    public const string SystemName = FileSystems.Definitions.FileSystemDisplayNames.MacMfs;
    /// <summary>Définit la valeur MFS nommée <c>VolumeDescription</c>.</summary>
    public const string VolumeDescription = "volume";
    /// <summary>Définit la valeur MFS nommée <c>DirectoryDescription</c>.</summary>
    public const string DirectoryDescription = "répertoire";
    /// <summary>Définit la valeur MFS nommée <c>DataForkName</c>.</summary>
    public const string DataForkName = "données";
    /// <summary>Définit la valeur MFS nommée <c>ResourceForkName</c>.</summary>
    public const string ResourceForkName = "ressources";
    /// <summary>Définit la valeur MFS nommée <c>DefaultFileDescription</c>.</summary>
    public const string DefaultFileDescription = "Fichier Macintosh";
    /// <summary>Définit la valeur MFS nommée <c>SectorSize</c>.</summary>
    public const int SectorSize = 512;
    /// <summary>Définit la valeur MFS nommée <c>MasterDirectoryBlock</c>.</summary>
    public const int MasterDirectoryBlock = 2;
    /// <summary>Définit la valeur MFS nommée <c>VolumeInformationBlockCount</c>.</summary>
    public const int VolumeInformationBlockCount = 2;
    /// <summary>Définit la valeur MFS nommée <c>MinimumMdbLength</c>.</summary>
    public const int MinimumMdbLength = 64;
    /// <summary>Définit la valeur MFS nommée <c>Signature</c>.</summary>
    public const ushort Signature = 0xd2d7;
    /// <summary>Définit la valeur MFS nommée <c>CreatedOffset</c>.</summary>
    public const int CreatedOffset = 2;
    /// <summary>Définit la valeur MFS nommée <c>ModifiedOffset</c>.</summary>
    public const int ModifiedOffset = 6;
    /// <summary>Définit la valeur MFS nommée <c>DirectoryStartOffset</c>.</summary>
    public const int DirectoryStartOffset = 14;
    /// <summary>Définit la valeur MFS nommée <c>DirectoryLengthOffset</c>.</summary>
    public const int DirectoryLengthOffset = 16;
    /// <summary>Définit la valeur MFS nommée <c>AllocationCountOffset</c>.</summary>
    public const int AllocationCountOffset = 18;
    /// <summary>Définit la valeur MFS nommée <c>AllocationSizeOffset</c>.</summary>
    public const int AllocationSizeOffset = 20;
    /// <summary>Définit la valeur MFS nommée <c>AllocationStartOffset</c>.</summary>
    public const int AllocationStartOffset = 28;
    /// <summary>Définit la valeur MFS nommée <c>FreeAllocationCountOffset</c>.</summary>
    public const int FreeAllocationCountOffset = 34;
    /// <summary>Définit la valeur MFS nommée <c>VolumeNameOffset</c>.</summary>
    public const int VolumeNameOffset = 36;
    /// <summary>Définit la valeur MFS nommée <c>MaximumVolumeNameLength</c>.</summary>
    public const int MaximumVolumeNameLength = 27;
    /// <summary>Définit la valeur MFS nommée <c>AllocationMapOffset</c>.</summary>
    public const int AllocationMapOffset = 64;
    /// <summary>Définit la valeur MFS nommée <c>AllocationMapLength</c>.</summary>
    public const int AllocationMapLength = 960;
    /// <summary>Définit la valeur MFS nommée <c>MaximumAllocationCount</c>.</summary>
    public const int MaximumAllocationCount = 640;
    /// <summary>Définit la valeur MFS nommée <c>PackedPairLength</c>.</summary>
    public const int PackedPairLength = 3;
    /// <summary>Définit la valeur MFS nommée <c>BitsPerAllocationEntry</c>.</summary>
    public const int BitsPerAllocationEntry = 12;
    /// <summary>Définit la valeur MFS nommée <c>AllocationValueMask</c>.</summary>
    public const ushort AllocationValueMask = 0x0fff;
    /// <summary>Définit la valeur MFS nommée <c>LowNibbleMask</c>.</summary>
    public const byte LowNibbleMask = 0x0f;
    /// <summary>Définit la valeur MFS nommée <c>HalfByteShift</c>.</summary>
    public const int HalfByteShift = 4;
    /// <summary>Définit la valeur MFS nommée <c>ByteShift</c>.</summary>
    public const int ByteShift = 8;
    /// <summary>Définit la valeur MFS nommée <c>FreeCluster</c>.</summary>
    public const ushort FreeCluster = 0;
    /// <summary>Définit la valeur MFS nommée <c>FirstUsableCluster</c>.</summary>
    public const ushort FirstUsableCluster = 2;
    /// <summary>Définit la valeur MFS nommée <c>EndOfChain</c>.</summary>
    public const ushort EndOfChain = 0x0ff1;
    /// <summary>Définit la valeur MFS nommée <c>MinimumDirectoryEntryLength</c>.</summary>
    public const int MinimumDirectoryEntryLength = 51;
    /// <summary>Définit la valeur MFS nommée <c>ActiveEntryMask</c>.</summary>
    public const byte ActiveEntryMask = 0x80;
    /// <summary>Définit la valeur MFS nommée <c>FlagsOffset</c>.</summary>
    public const int FlagsOffset = 0;
    /// <summary>Définit la valeur MFS nommée <c>FinderInfoOffset</c>.</summary>
    public const int FinderInfoOffset = 2;
    /// <summary>Définit la valeur MFS nommée <c>FinderInfoLength</c>.</summary>
    public const int FinderInfoLength = 16;
    /// <summary>Définit la valeur MFS nommée <c>FinderTypeLength</c>.</summary>
    public const int FinderTypeLength = 4;
    /// <summary>Définit la valeur MFS nommée <c>FileNumberOffset</c>.</summary>
    public const int FileNumberOffset = 18;
    /// <summary>Définit la valeur MFS nommée <c>DataForkStartOffset</c>.</summary>
    public const int DataForkStartOffset = 22;
    /// <summary>Définit la valeur MFS nommée <c>DataForkLogicalLengthOffset</c>.</summary>
    public const int DataForkLogicalLengthOffset = 24;
    /// <summary>Définit la valeur MFS nommée <c>DataForkPhysicalLengthOffset</c>.</summary>
    public const int DataForkPhysicalLengthOffset = 28;
    /// <summary>Définit la valeur MFS nommée <c>ResourceForkStartOffset</c>.</summary>
    public const int ResourceForkStartOffset = 32;
    /// <summary>Définit la valeur MFS nommée <c>ResourceForkLogicalLengthOffset</c>.</summary>
    public const int ResourceForkLogicalLengthOffset = 34;
    /// <summary>Définit la valeur MFS nommée <c>ResourceForkPhysicalLengthOffset</c>.</summary>
    public const int ResourceForkPhysicalLengthOffset = 38;
    /// <summary>Définit la valeur MFS nommée <c>CreatedDateOffset</c>.</summary>
    public const int CreatedDateOffset = 42;
    /// <summary>Définit la valeur MFS nommée <c>ModifiedDateOffset</c>.</summary>
    public const int ModifiedDateOffset = 46;
    /// <summary>Définit la valeur MFS nommée <c>NameLengthOffset</c>.</summary>
    public const int NameLengthOffset = 50;
    /// <summary>Définit la valeur MFS nommée <c>NameOffset</c>.</summary>
    public const int NameOffset = 51;
    /// <summary>Définit la valeur MFS nommée <c>MaximumNameLength</c>.</summary>
    public const int MaximumNameLength = 63;
    /// <summary>Définit la valeur MFS nommée <c>EntryAlignment</c>.</summary>
    public const int EntryAlignment = 2;
}
