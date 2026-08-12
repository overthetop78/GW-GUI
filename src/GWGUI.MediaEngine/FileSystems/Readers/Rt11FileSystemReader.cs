using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Dec.Rt11;
using GWGUI.MediaEngine.FileSystems.Rt11;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit un volume RT-11 remis dans l'ordre de ses blocs logiques.</summary>
public sealed class Rt11FileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.Rt11;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { DiskImageFormatIds.DecRx02 };

    /// <inheritdoc />
    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != Rt11FileSystemLayout.BlockSize || !image.TryGetBlock(Rt11FileSystemLayout.HomeBlock, out var home)) return false;
        return Rt11HomeBlockProbe.LooksLikeRt11(home.Data is byte[] data ? data.AsSpan() : home.Data.ToArray().AsSpan());
    }

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!image.TryGetBlock(Rt11FileSystemLayout.HomeBlock, out var homeBlock)) throw Rt11FileSystemExceptions.InvalidHomeBlock(string.Empty, 0);
        var homeBytes = homeBlock.Data.ToArray();
        var home = homeBytes.AsSpan();
        var signature = System.Text.Encoding.ASCII.GetString(home.Slice(Rt11FileSystemLayout.SystemIdOffset, Rt11FileSystemLayout.SystemIdLength)).TrimEnd('\0', ' ');
        var directoryBlock = BinaryPrimitives.ReadUInt16LittleEndian(home[Rt11FileSystemLayout.DirectoryBlockOffset..]);
        if (!CanRead(image)) throw Rt11FileSystemExceptions.InvalidHomeBlock(signature, directoryBlock);
        var volumeName = System.Text.Encoding.ASCII.GetString(home.Slice(Rt11FileSystemLayout.VolumeNameOffset, Rt11FileSystemLayout.VolumeNameLength)).TrimEnd('\0', ' ');
        var directory = Rt11DirectoryReader.Read(image, directoryBlock);
        return new(volumeName, Definitions.FileSystemDisplayNames.Rt11, image.Capacity, directory.FreeBlocks * Rt11FileSystemLayout.BlockSize, null, directory.Entries.Select(entry => entry.Modified).Where(date => date.HasValue).Max(), directory.Entries, directory.Warnings);
    }
}
