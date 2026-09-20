using System.Collections.Frozen;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Constants;

using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Ucsd;

/// <summary>Lit les volumes, répertoires et fichiers UCSD p-System.</summary>
public sealed class UcsdFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => FileSystemIds.Ucsd;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { MediaImageFormatIds.UcsdIbmMfm }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanRead(IMediaSectorImage image) => CatalogFormatIds.Contains(image.FormatId) && image.BlockSize == UcsdFileSystemLayout.BlockSize && UcsdDirectoryHeaderReader.TryRead(image, out _);

    /// <inheritdoc />
    public FileSystemVolume Read(IMediaSectorImage image)
    {
        if (!UcsdDirectoryHeaderReader.TryRead(image, out var header) || header is null)
        {
            var probe = UcsdBlockReader.Read(image, UcsdFileSystemLayout.DirectoryBlock, 1);
            throw UcsdFileSystemExceptions.UnrecognizedSystem(UcsdFileSystemLayout.DirectoryBlock, probe.MissingBlocks);
        }
        var warnings = new List<string>();
        var directory = UcsdBlockReader.Read(image, UcsdFileSystemLayout.DirectoryBlock, header.DirectoryBlockCount);
        if (!directory.IsValid) warnings.Add(UcsdFileSystemExceptions.MissingDirectoryBlocks(directory.MissingBlocks));
        var decoded = UcsdDirectoryEntryReader.Read(image, header, directory, warnings);
        var freeBlocks = decoded.IsValid ? Math.Max(0, header.TotalBlocks - decoded.UsedBlocks.Count) : 0;
        return new(header.VolumeName, FileSystemIds.Ucsd, (long)header.TotalBlocks * UcsdFileSystemLayout.BlockSize, (long)freeBlocks * UcsdFileSystemLayout.BlockSize, null, header.VolumeDate, decoded.Entries, warnings);
    }
}
