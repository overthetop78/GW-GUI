using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

/// <summary>Reads physical-order DEC RX02 dumps and exposes RT-11 logical 512-byte blocks.</summary>
public sealed class DecRx02ImageReader
{
    public const int ImageSize = 77 * 26 * 256;

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (bytes.Length != ImageSize) throw new InvalidDataException("The image is not a complete DEC RX02 dump.");

        var logicalSectors = new byte[2002][];
        for (var logicalSector = 0; logicalSector < logicalSectors.Length; logicalSector++)
        {
            var (track, sector) = LogicalToPhysical(logicalSector);
            logicalSectors[logicalSector] = bytes.AsSpan((track * 26 + sector - 1) * 256, 256).ToArray();
        }

        var blocks = new SectorBlock[1001];
        for (var block = 0; block < blocks.Length; block++)
        {
            var data = new byte[512];
            logicalSectors[block * 2].CopyTo(data, 0);
            logicalSectors[block * 2 + 1].CopyTo(data, 256);
            blocks[block] = new(block, new(block / 13, 0, block % 13 + 1), data, true);
        }
        return new SectorImage(DiskImageFormatIds.DecRx02, 512, 77, 1, 13, blocks, capacity: ImageSize, logicalBlockCount: 1001);
    }

    public static bool LooksLikeRt11(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != ImageSize) return false;
        Span<byte> home = stackalloc byte[512];
        CopyLogicalSector(bytes, 2, home[..256]);
        CopyLogicalSector(bytes, 3, home[256..]);
        var directoryBlock = ReadUInt16(home, 468);
        var systemId = System.Text.Encoding.ASCII.GetString(home[496..508]).TrimEnd('\0', ' ');
        return directoryBlock is >= 2 and < 1001 && systemId.StartsWith("DECRT11", StringComparison.Ordinal);
    }

    private static void CopyLogicalSector(ReadOnlySpan<byte> source, int logicalSector, Span<byte> destination)
    {
        var (track, sector) = LogicalToPhysical(logicalSector);
        source.Slice((track * 26 + sector - 1) * 256, 256).CopyTo(destination);
    }

    private static (int Track, int Sector) LogicalToPhysical(int logicalSector)
    {
        var logicalTrack = logicalSector / 26;
        var position = logicalSector % 26;
        position = (2 * position + (position >= 13 ? 1 : 0)) % 26;
        var sector = 1 + (position + 6 * logicalTrack) % 26;
        var track = logicalTrack + 1;
        if (track >= 77) track = 0;
        return (track, sector);
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> source, int offset) =>
        (ushort)(source[offset] | source[offset + 1] << 8);
}
