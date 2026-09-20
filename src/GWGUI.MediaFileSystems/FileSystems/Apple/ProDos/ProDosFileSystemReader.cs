using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;
using System.Collections.Frozen;
using GWGUI.MediaFileSystems.Definitions;

using IMediaSectorImage = global::GWGUI.MediaFileSystems.Interfaces.IMediaSectorImage;

namespace GWGUI.MediaFileSystems.FileSystems.Apple.ProDos;

/// <summary>Lit les volumes, répertoires et fichiers ProDOS ou Apple III SOS.</summary>
public sealed class ProDosFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => FileSystemIds.ProDos;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { MediaImageFormatIds.AppleIIProDos, MediaImageFormatIds.AppleIIProDos140, MediaImageFormatIds.AppleIIProDos800, MediaImageFormatIds.AppleIIISos }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanRead(IMediaSectorImage image) => ProDosVolumeHeaderReader.TryRead(image, out _);

    /// <inheritdoc />
    public FileSystemVolume Read(IMediaSectorImage image)
    {
        if (!ProDosVolumeHeaderReader.TryRead(image, out var header) || header is null)
        {
            var observed = image.TryGetBlock(ProDosFileSystemLayout.RootBlock, out var candidate) && candidate.Data.Count > ProDosFileSystemLayout.HeaderOffset ? candidate.Data[ProDosFileSystemLayout.HeaderOffset] : (byte)0;
            throw ProDosFileSystemExceptions.UnsupportedVolume(ProDosFileSystemLayout.RootBlock, observed);
        }
        var warnings = new List<string>();
        var directory = ProDosDirectoryReader.Read(image, ProDosFileSystemLayout.RootBlock, warnings, new HashSet<int>(), 0);
        var effectiveTotal = header.TotalBlocks;
        if (header.TotalBlocks > image.BlockCount)
        {
            warnings.Add(ProDosFileSystemExceptions.InvalidTotalBlockCount(header.TotalBlocks, image.BlockCount));
            effectiveTotal = image.BlockCount;
        }
        var bitmap = ProDosBitmapReader.Read(image, header.BitmapBlock, effectiveTotal, warnings);
        var freeBytes = bitmap.IsValid ? (long)bitmap.FreeBlocks * ProDosFileSystemLayout.BlockSize : 0;
        var fileSystemId = image.FormatId.Equals(MediaImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase) ? FileSystemIds.Sos : FileSystemIds.ProDos;
        return new(header.Name, fileSystemId, (long)effectiveTotal * ProDosFileSystemLayout.BlockSize, freeBytes, header.Created, null, directory.Entries, warnings);
    }
}
