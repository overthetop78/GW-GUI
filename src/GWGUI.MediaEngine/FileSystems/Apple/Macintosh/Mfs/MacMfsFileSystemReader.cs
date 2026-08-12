using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

/// <summary>Lit un volume Macintosh MFS, sa carte d'allocation et son répertoire plat.</summary>
public sealed class MacMfsFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => FileSystemIds.MacMfs;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.AppleMacMfs, DiskImageFormatIds.Mac400, DiskImageFormatIds.Mac800, DiskImageFormatIds.Mac1440 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanRead(SectorImage image) => image.BlockSize == MacMfsFileSystemLayout.SectorSize && image.TryGetBlock(MacMfsFileSystemLayout.MasterDirectoryBlock, out var block) && block.Data.Count >= MacMfsFileSystemLayout.MinimumMdbLength && MacFileSystemPrimitives.ReadUInt16(block.Data.ToArray(), 0) == MacMfsFileSystemLayout.Signature;

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        var warnings = new List<string>();
        var volumeInformation = MacMfsBlockReader.Read(image, MacMfsFileSystemLayout.MasterDirectoryBlock, MacMfsFileSystemLayout.VolumeInformationBlockCount, MacMfsFileSystemLayout.VolumeDescription, warnings);
        if (!volumeInformation.IsValid || volumeInformation.Bytes.Count < MacMfsFileSystemLayout.MinimumMdbLength || MacFileSystemPrimitives.ReadUInt16(volumeInformation.Bytes.ToArray(), 0) != MacMfsFileSystemLayout.Signature)
        {
            var signature = volumeInformation.Bytes.Count >= sizeof(ushort) ? MacFileSystemPrimitives.ReadUInt16(volumeInformation.Bytes.ToArray(), 0) : (ushort)0;
            throw MacFileSystemExceptions.InvalidVolume(MacMfsFileSystemLayout.SystemName, signature);
        }
        var mdb = volumeInformation.Bytes.ToArray();
        var directoryStart = MacFileSystemPrimitives.ReadUInt16(mdb, MacMfsFileSystemLayout.DirectoryStartOffset);
        var directoryLength = MacFileSystemPrimitives.ReadUInt16(mdb, MacMfsFileSystemLayout.DirectoryLengthOffset);
        var allocationCount = MacFileSystemPrimitives.ReadUInt16(mdb, MacMfsFileSystemLayout.AllocationCountOffset);
        var allocationSize = MacFileSystemPrimitives.ReadUInt32(mdb, MacMfsFileSystemLayout.AllocationSizeOffset);
        if (allocationSize == 0 || allocationSize % MacMfsFileSystemLayout.SectorSize != 0) throw MacFileSystemExceptions.InvalidAllocationSize(MacMfsFileSystemLayout.SystemName, allocationSize, MacMfsFileSystemLayout.SectorSize);
        var allocationStart = MacFileSystemPrimitives.ReadUInt16(mdb, MacMfsFileSystemLayout.AllocationStartOffset);
        var freeAllocationCount = MacFileSystemPrimitives.ReadUInt16(mdb, MacMfsFileSystemLayout.FreeAllocationCountOffset);
        var volumeName = MacFileSystemPrimitives.ReadPascalString(mdb, MacMfsFileSystemLayout.VolumeNameOffset, MacMfsFileSystemLayout.MaximumVolumeNameLength);
        var map = MacMfsAllocationMap.Decode(mdb.AsSpan(MacMfsFileSystemLayout.AllocationMapOffset, MacMfsFileSystemLayout.AllocationMapLength), allocationCount);
        var entries = MacMfsDirectoryReader.Read(image, directoryStart, directoryLength, map, allocationStart, allocationSize, warnings);
        var freeBytes = freeAllocationCount <= allocationCount ? (long)freeAllocationCount * allocationSize : 0;
        if (freeAllocationCount > allocationCount) warnings.Add(MacFileSystemExceptions.InvalidFreeAllocationCount(MacMfsFileSystemLayout.SystemName, freeAllocationCount, allocationCount));
        return new(volumeName, FileSystemIds.MacMfs, image.Capacity, freeBytes, MacFileSystemTime.FromSeconds(MacFileSystemPrimitives.ReadUInt32(mdb, MacMfsFileSystemLayout.CreatedOffset)), MacFileSystemTime.FromSeconds(MacFileSystemPrimitives.ReadUInt32(mdb, MacMfsFileSystemLayout.ModifiedOffset)), entries, warnings);
    }
}
