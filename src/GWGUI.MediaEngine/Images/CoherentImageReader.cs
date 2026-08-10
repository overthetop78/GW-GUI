using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

/// <summary>Reads raw 512-byte COHERENT file-system images, including Commodore 900 media.</summary>
public sealed class CoherentImageReader
{
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (bytes.Length < 1024 || bytes.Length % 512 != 0 || !LooksLikeCoherent(bytes))
            throw new InvalidDataException("The image does not contain a COHERENT file system.");

        var fileSystemBlocks = checked((int)ReadCanonicalUInt32(bytes.AsSpan(514, 4)));
        if (fileSystemBlocks < 3 || fileSystemBlocks > bytes.Length / 512)
            throw new InvalidDataException("The COHERENT file-system size is invalid.");

        // The C900 uses two heads and four zoned GCR rates.  The filesystem can
        // be smaller than the physical medium, so retain every complete block
        // present in the dump and map it to the documented physical geometry.
        var blockCount = bytes.Length / 512;
        var sectors = new List<SectorBlock>(blockCount);
        var block = 0;
        for (var cylinder = 0; cylinder < DiskGeometryConstants.EightyTrackCylinderCount && block < blockCount; cylinder++)
        {
            var sectorsPerTrack = SectorsPerTrack(cylinder);
            for (var head = 0; head < 2 && block < blockCount; head++)
                for (var sector = 0; sector < sectorsPerTrack && block < blockCount; sector++, block++)
                    sectors.Add(new(block, new(cylinder, head, sector),
                        bytes.AsSpan(block * 512, 512).ToArray(), true));
        }
        return new SectorImage(DiskImageFormatIds.Commodore900Coherent, 512, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 16, sectors,
            capacity: bytes.Length, logicalBlockCount: blockCount);
    }

    public static bool LooksLikeCoherent(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 1024) return false;
        var name = System.Text.Encoding.ASCII.GetString(bytes.Slice(996, 6));
        var pack = System.Text.Encoding.ASCII.GetString(bytes.Slice(1002, 6));
        return (name is "noname" or "xxxxx " || name.StartsWith("xxxxx", StringComparison.Ordinal)) &&
               (pack is "nopack" or "xxxxx\n" || pack.StartsWith("xxxxx", StringComparison.Ordinal));
    }

    internal static uint ReadCanonicalUInt32(ReadOnlySpan<byte> value)
    {
        if (value.Length < 4) throw new ArgumentException("Four bytes are required.", nameof(value));
        return (uint)(value[2] | value[3] << 8 | value[0] << 16 | value[1] << 24);
    }

    internal static int SectorsPerTrack(int cylinder) => cylinder switch
    {
        < 39 => 16,
        < 53 => 15,
        < 64 => 14,
        _ => 13
    };
}
