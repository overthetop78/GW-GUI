using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

/// <summary>Lit un volume Macintosh HFS et reconstruit son catalogue ainsi que les forks de ses fichiers.</summary>
public sealed class MacHfsFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => FileSystemIds.MacHfs;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.AppleMacHfs, DiskImageFormatIds.Mac400, DiskImageFormatIds.Mac800, DiskImageFormatIds.Mac1440 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanRead(SectorImage image) => TryReadMdb(image, out var mdb) && MacFileSystemPrimitives.ReadUInt16(mdb, 0) == MacHfsFileSystemLayout.Signature;

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!TryReadMdb(image, out var mdb) || MacFileSystemPrimitives.ReadUInt16(mdb, 0) != MacHfsFileSystemLayout.Signature)
        {
            var signature = mdb.Length >= sizeof(ushort) ? MacFileSystemPrimitives.ReadUInt16(mdb, 0) : (ushort)0;
            throw MacFileSystemExceptions.InvalidVolume(MacHfsFileSystemLayout.SystemName, signature);
        }
        var allocationCount = MacFileSystemPrimitives.ReadUInt16(mdb, MacHfsFileSystemLayout.AllocationCountOffset);
        var allocationSize = MacFileSystemPrimitives.ReadUInt32(mdb, MacHfsFileSystemLayout.AllocationSizeOffset);
        if (allocationSize == 0 || allocationSize % MacHfsFileSystemLayout.SectorSize != 0) throw MacFileSystemExceptions.InvalidAllocationSize(MacHfsFileSystemLayout.SystemName, allocationSize, MacHfsFileSystemLayout.SectorSize);
        var allocationStart = MacFileSystemPrimitives.ReadUInt16(mdb, MacHfsFileSystemLayout.AllocationStartOffset);
        var freeAllocationCount = MacFileSystemPrimitives.ReadUInt16(mdb, MacHfsFileSystemLayout.FreeAllocationCountOffset);
        var volumeName = MacFileSystemPrimitives.ReadPascalString(mdb, MacHfsFileSystemLayout.VolumeNameOffset, MacHfsFileSystemLayout.MaximumVolumeNameLength);
        var catalogLength = MacFileSystemPrimitives.ReadUInt32(mdb, MacHfsFileSystemLayout.CatalogLengthOffset);
        var warnings = new List<string>();
        var catalogExtents = mdb.AsSpan(MacHfsFileSystemLayout.CatalogExtentsOffset, MacHfsFileSystemLayout.EmbeddedExtentsLength);
        var catalogContent = MacHfsExtentReader.Read(image, catalogExtents, allocationStart, allocationSize, catalogLength);
        foreach (var block in catalogContent.MissingBlocks) warnings.Add(MacFileSystemExceptions.MissingBlock(MacHfsFileSystemLayout.CatalogName, MacHfsFileSystemLayout.DataForkName, block));
        if (catalogContent.RemainingLength > 0) warnings.Add(MacFileSystemExceptions.IncompleteData(MacHfsFileSystemLayout.CatalogName, MacHfsFileSystemLayout.DataForkName, catalogLength - catalogContent.RemainingLength, catalogLength));
        var records = MacHfsCatalogReader.Read(catalogContent.Content, image, allocationStart, allocationSize, warnings);
        var entries = MacHfsDirectoryBuilder.Build(records, warnings);
        var capacity = (long)allocationCount * allocationSize;
        var freeBytes = freeAllocationCount <= allocationCount ? (long)freeAllocationCount * allocationSize : 0;
        if (freeAllocationCount > allocationCount) warnings.Add(MacFileSystemExceptions.InvalidFreeAllocationCount(MacHfsFileSystemLayout.SystemName, freeAllocationCount, allocationCount));
        return new(volumeName, FileSystemIds.MacHfs, capacity, freeBytes, MacFileSystemTime.FromSeconds(MacFileSystemPrimitives.ReadUInt32(mdb, MacHfsFileSystemLayout.CreatedOffset)), MacFileSystemTime.FromSeconds(MacFileSystemPrimitives.ReadUInt32(mdb, MacHfsFileSystemLayout.ModifiedOffset)), entries, warnings);
    }

    /// <summary>Tente d'obtenir un MDB HFS de taille suffisante.</summary>
    private static bool TryReadMdb(SectorImage image, out byte[] mdb)
    {
        if (image.BlockSize == MacHfsFileSystemLayout.SectorSize && image.TryGetBlock(MacHfsFileSystemLayout.MasterDirectoryBlock, out var block) && block.Data.Count >= MacHfsFileSystemLayout.MinimumMdbLength)
        {
            mdb = block.Data.ToArray();
            return true;
        }
        mdb = [];
        return false;
    }
}
