using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Lit en lecture seule le système de fichiers COHERENT de type V7 utilisé par le Commodore 900.</summary>
public sealed class CoherentFileSystemReader : IFileSystemReader
{
    /// <summary>Obtient l'identifiant technique du lecteur.</summary>
    public string Id => Definitions.FileSystemIds.Coherent;
    /// <summary>Obtient les formats dont le catalogue peut être lu.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.Commodore900Coherent }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Indique si l'image utilise le format et la taille de bloc COHERENT attendus.</summary>
    public bool CanRead(SectorImage image) => CatalogFormatIds.Contains(image.FormatId) && image.BlockSize == CoherentFileSystemLayout.BlockSize;

    /// <summary>Lit le volume, ses métadonnées et son arborescence.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        var data = CoherentImageData.Create(image);
        if (!data.IsRangePresent(0, CoherentFileSystemLayout.MinimumImageSize)) throw CoherentExceptions.MissingSuperblockBlock();
        var fileSystemBlocks = CoherentFormat.ReadValidatedFileSystemBlockCount(data.Bytes);
        var inodeZoneEnd = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data.Bytes.AsSpan(CoherentFileSystemLayout.InodeZoneEndOffset, sizeof(ushort)));
        if (inodeZoneEnd < CoherentFileSystemLayout.MinimumInodeZoneEnd || inodeZoneEnd > fileSystemBlocks) throw CoherentExceptions.InvalidInodeZone(inodeZoneEnd, fileSystemBlocks);
        var warnings = new List<string>();
        var entries = CoherentDirectoryReader.ReadRoot(data, inodeZoneEnd, warnings);
        var volumeName = CoherentNameCodec.Decode(data.Bytes.AsSpan(CoherentFileSystemLayout.VolumeNameOffset, CoherentFileSystemLayout.NameLength));
        if (volumeName is CoherentFileSystemLayout.PlaceholderName or CoherentFileSystemLayout.DefaultVolumeName) volumeName = string.Empty;
        var freeBytes = (long)CoherentFormat.ReadCanonicalUInt32(data.Bytes.AsSpan(CoherentFileSystemLayout.FreeBlockCountOffset, CoherentFormat.UInt32Length)) * CoherentFileSystemLayout.BlockSize;
        var modified = CoherentFileSystemTime.Decode(CoherentFormat.ReadCanonicalUInt32(data.Bytes.AsSpan(CoherentFileSystemLayout.ModifiedTimeOffset, CoherentFormat.UInt32Length)));
        return new(volumeName, Definitions.FileSystemIds.Coherent, (long)fileSystemBlocks * CoherentFileSystemLayout.BlockSize, Math.Clamp(freeBytes, 0, (long)fileSystemBlocks * CoherentFileSystemLayout.BlockSize), null, modified, entries, warnings);
    }
}
