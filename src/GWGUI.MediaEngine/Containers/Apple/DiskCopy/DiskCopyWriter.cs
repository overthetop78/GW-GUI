using System.Buffers.Binary;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.DiskCopy;

/// <summary>Écrit les conteneurs Apple DiskCopy 4.2 avec données, tags et checksums.</summary>
public sealed class DiskCopyWriter
{
    /// <summary>Écrit une image avec les métadonnées d'en-tête indiquées.</summary>
    public async Task WriteAsync(SectorImage image, string path, DiskCopyImage? source = null, CancellationToken cancellationToken = default)
    {
        var metadata = source ?? CreateMetadata(image, Path.GetFileNameWithoutExtension(path));
        var bytes = Build(image, metadata);
        await AppleRawImageWriter.WriteAtomicallyAsync(path, bytes, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Sérialise l'en-tête, les blocs et leurs tags facultatifs.</summary>
    internal static byte[] Build(SectorImage image, DiskCopyImage metadata)
    {
        var blocks = image.AvailableBlocks.OrderBy(block => block.LogicalBlock).ToArray();
        if (blocks.Length != image.BlockCount || blocks.Select((block, index) => block.LogicalBlock == index).Any(valid => !valid)) throw new InvalidDataException("DiskCopy exige une image sectorielle complète et ordonnée.");
        var data = blocks.SelectMany(block => block.Data).ToArray();
        if (data.Length != image.BlockCount * DiskCopyLayout.DataBlockSize) throw new InvalidDataException("DiskCopy exige des blocs de 512 octets.");
        var tagged = blocks.Any(block => block.Tag is not null);
        if (tagged && blocks.Any(block => block.Tag?.Count != DiskCopyLayout.TagSizePerBlock)) throw new InvalidDataException("Tous les tags DiskCopy doivent contenir 12 octets.");
        var tags = tagged ? blocks.SelectMany(block => block.Tag!).ToArray() : [];
        var container = new byte[checked(DiskCopyLayout.HeaderSize + data.Length + tags.Length)];
        WriteName(container, metadata.NameBytes);
        BinaryPrimitives.WriteUInt32BigEndian(container.AsSpan(DiskCopyLayout.DataLengthOffset), checked((uint)data.Length));
        BinaryPrimitives.WriteUInt32BigEndian(container.AsSpan(DiskCopyLayout.TagLengthOffset), checked((uint)tags.Length));
        BinaryPrimitives.WriteUInt32BigEndian(container.AsSpan(DiskCopyLayout.DataChecksumOffset), DiskCopyReader.CalculateChecksum(data));
        var tagChecksum = tags.Length == 0 ? DiskCopyFormat.MissingChecksum : DiskCopyReader.CalculateChecksum(tags.AsSpan(DiskCopyLayout.TagChecksumExcludedPrefixSize));
        BinaryPrimitives.WriteUInt32BigEndian(container.AsSpan(DiskCopyLayout.TagChecksumOffset), tagChecksum);
        container[DiskCopyLayout.DiskFormatOffset] = metadata.DiskFormat;
        container[DiskCopyLayout.FormatByteOffset] = metadata.FormatByte;
        BinaryPrimitives.WriteUInt16BigEndian(container.AsSpan(DiskCopyLayout.PrivateWordOffset), DiskCopyFormat.PrivateWord);
        data.CopyTo(container, DiskCopyLayout.HeaderSize);
        tags.CopyTo(container, DiskCopyLayout.HeaderSize + data.Length);
        return container;
    }

    /// <summary>Crée des métadonnées DiskCopy cohérentes pour une image sans en-tête source.</summary>
    private static DiskCopyImage CreateMetadata(SectorImage image, string name)
    {
        var diskFormat = image.Capacity switch
        {
            MacintoshGcrGeometry.Capacity400K => DiskCopyFormat.DiskFormat400K,
            MacintoshGcrGeometry.Capacity800K => DiskCopyFormat.DiskFormat800K,
            MacintoshMfmGeometry.Capacity => DiskCopyFormat.DiskFormat1440K,
            _ => throw new InvalidDataException($"La capacité DiskCopy {image.Capacity} n'est pas prise en charge.")
        };
        var formatByte = image.FormatId.Contains("mfs", StringComparison.OrdinalIgnoreCase) ? DiskCopyFormat.FormatByteMacintoshMfs : DiskCopyFormat.FormatByteMacintoshHfs;
        return new(image, System.Text.Encoding.ASCII.GetBytes(name), diskFormat, formatByte);
    }

    /// <summary>Écrit le nom Pascal DiskCopy sans dépasser son champ de 64 octets.</summary>
    private static void WriteName(Span<byte> container, IReadOnlyList<byte> nameBytes)
    {
        var length = Math.Min(nameBytes.Count, DiskCopyLayout.MaximumNameLength);
        container[DiskCopyLayout.NameLengthOffset] = checked((byte)length);
        nameBytes.Take(length).ToArray().CopyTo(container[DiskCopyLayout.NameOffset..]);
    }
}
