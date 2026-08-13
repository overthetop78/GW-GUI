using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Détecte des caractéristiques documentées sans fabriquer d'entrées de fichiers.</summary>
internal sealed class DiskContentDetector
{
    private static ReadOnlySpan<byte> CrackedBySignature => "CRACKED BY"u8;
    private static ReadOnlySpan<byte> TheCompanySignature => "THE COMPANY"u8;
    private static ReadOnlySpan<byte> FireSignature => "FIRE"u8;

    /// <summary>Analyse les blocs disponibles dans leur ordre logique.</summary>
    public DiskContentMetadata Analyze(SectorImage image)
    {
        var bytes = Flatten(image);
        var modificationId = ContainsInOrder(bytes, CrackedBySignature, TheCompanySignature) ? DiskContentIds.CrackTheCompany : null;
        var compressionIds = new List<string>();
        if (Contains(bytes, FireSignature)) compressionIds.Add(DiskContentIds.CompressionFire);
        var organizationId = AtnImploderArchiveDetector.TryDetect(bytes, image.BlockSize, out var memberCount) ? DiskContentIds.OrganizationAtnArchive : null;
        if (organizationId is not null) compressionIds.Add(DiskContentIds.CompressionAtnImploder);
        return new(IsValidAmigaBootLoader(image, bytes), modificationId, compressionIds, organizationId, memberCount);
    }

    private static byte[] Flatten(SectorImage image)
    {
        if (image.Capacity > int.MaxValue) return [];
        var result = new byte[checked((int)image.Capacity)];
        foreach (var block in image.AvailableBlocks)
        {
            var offset = (long)block.LogicalBlock * image.BlockSize;
            if (offset < 0 || offset + block.Data.Count > result.Length) continue;
            block.Data.ToArray().CopyTo(result, checked((int)offset));
        }
        return result;
    }

    private static bool IsValidAmigaBootLoader(SectorImage image, ReadOnlySpan<byte> bytes)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase) || image.AvailableBlocks.Count != image.BlockCount || bytes.Length < 1024 || bytes[0] != (byte)'D' || bytes[1] != (byte)'O' || bytes[2] != (byte)'S') return false;

        uint sum = 0;
        for (var offset = 0; offset < 1024; offset += sizeof(uint))
        {
            var value = BinaryPrimitives.ReadUInt32BigEndian(bytes[offset..]);
            var previous = sum;
            sum += value;
            if (sum < previous) sum++;
        }
        return sum == uint.MaxValue && bytes[12..1024].IndexOfAnyExcept((byte)0) >= 0;
    }

    private static bool Contains(ReadOnlySpan<byte> source, ReadOnlySpan<byte> value) => source.IndexOf(value) >= 0;

    private static bool ContainsInOrder(ReadOnlySpan<byte> source, ReadOnlySpan<byte> first, ReadOnlySpan<byte> second)
    {
        var firstOffset = source.IndexOf(first);
        if (firstOffset < 0) return false;
        var secondOffset = source[(firstOffset + first.Length)..].IndexOf(second);
        return secondOffset >= 0 && secondOffset <= 128;
    }
}
