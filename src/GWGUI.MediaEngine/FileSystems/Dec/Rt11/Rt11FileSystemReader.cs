using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Lit un volume RT-11 remis dans l'ordre de ses blocs logiques.</summary>
public sealed class Rt11FileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => FileSystemIds.Rt11;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.DecRx02 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanRead(SectorImage image) => CatalogFormatIds.Contains(image.FormatId) && image.BlockSize == Rt11FileSystemLayout.BlockSize && image.TryGetBlock(Rt11FileSystemLayout.HomeBlock, out var home) && home.Data.Count == Rt11FileSystemLayout.BlockSize && Rt11HomeBlockProbe.LooksLikeRt11(home.Data.ToArray());

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!image.TryGetBlock(Rt11FileSystemLayout.HomeBlock, out var homeBlock) || homeBlock.Data.Count != Rt11FileSystemLayout.BlockSize) throw Rt11FileSystemExceptions.InvalidHomeBlock(string.Empty, 0);
        var home = homeBlock.Data.ToArray();
        var signature = Rt11Primitives.DecodeAscii(home.AsSpan(Rt11FileSystemLayout.SystemIdOffset, Rt11FileSystemLayout.SystemIdLength));
        var directoryBlock = Rt11Primitives.ReadUInt16(home, Rt11FileSystemLayout.DirectoryBlockOffset);
        if (!CanRead(image)) throw Rt11FileSystemExceptions.InvalidHomeBlock(signature, directoryBlock);
        var volumeName = Rt11Primitives.DecodeAscii(home.AsSpan(Rt11FileSystemLayout.VolumeNameOffset, Rt11FileSystemLayout.VolumeNameLength));
        var directory = Rt11DirectoryReader.Read(image, directoryBlock);
        var modified = directory.Entries.Select(entry => entry.Modified).Where(date => date.HasValue).Max();
        return new(volumeName, FileSystemIds.Rt11, image.Capacity, directory.FreeBlocks * Rt11FileSystemLayout.BlockSize, null, modified, directory.Entries, directory.Warnings);
    }
}
