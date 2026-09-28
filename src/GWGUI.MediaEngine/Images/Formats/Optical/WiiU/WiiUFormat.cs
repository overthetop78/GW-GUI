using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Images.Formats.Optical.WiiU;

internal static class WiiUFormat
{
    public const int WuxHeaderSize = 28;
    public const int OpticalSectorSize = 2048;
    public const int DefaultWuxSectorSize = 0x8000;
    public const int MinimumWuxSectorSize = 0x100;
    public const int MaximumWuxSectorSizeExclusive = 0x10000000;
    public const uint WuxMagic0 = 0x30585557;
    public const uint WuxMagic1 = 0x1099d02e;

    public static readonly byte[] WuxSignature =
    {
        (byte)'W', (byte)'U', (byte)'X', (byte)'0',
        0x2e, 0xd0, 0x99, 0x10
    };

    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Wud, DiskImageFileExtensions.Wux }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsWudLengthCompatible(long length) =>
        length >= OpticalSectorSize && length % OpticalSectorSize == 0;

    public static bool TryReadWuxHeader(
        ReadOnlySpan<byte> bytes,
        long sourceLength,
        out WiiUWuxHeader header)
    {
        header = default;
        if (bytes.Length < WuxHeaderSize || sourceLength < WuxHeaderSize)
            return false;
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes) != WuxMagic0
            || BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]) != WuxMagic1)
            return false;

        var sectorSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..]);
        var uncompressedSize = BinaryPrimitives.ReadUInt64LittleEndian(bytes[12..]);
        if (sectorSize < MinimumWuxSectorSize
            || sectorSize >= MaximumWuxSectorSizeExclusive
            || uncompressedSize == 0
            || uncompressedSize > long.MaxValue
            || uncompressedSize % OpticalSectorSize != 0)
            return false;

        var entryCount = checked((long)((uncompressedSize + (ulong)sectorSize - 1UL) / (ulong)sectorSize));
        if (entryCount == 0 || entryCount > int.MaxValue)
            return false;
        var indexOffset = WuxHeaderSize;
        var indexLength = checked(entryCount * sizeof(uint));
        var sectorArrayOffset = Align(checked(indexOffset + indexLength), (int)sectorSize);
        var sourceLengthLong = sourceLength;
        if (sectorArrayOffset > sourceLengthLong)
            return false;
        var storedLength = sourceLengthLong - sectorArrayOffset;
        if (storedLength <= 0 || storedLength % sectorSize != 0)
            return false;

        header = new WiiUWuxHeader(
            checked((int)sectorSize),
            checked((long)uncompressedSize),
            checked((int)entryCount),
            indexOffset,
            sectorArrayOffset,
            storedLength / sectorSize);
        return true;
    }

    private static long Align(long value, int alignment)
    {
        var remainder = value % alignment;
        return remainder == 0 ? value : checked(value + alignment - remainder);
    }
}

internal readonly record struct WiiUWuxHeader(
    int SectorSize,
    long UncompressedSize,
    int EntryCount,
    long IndexOffset,
    long SectorArrayOffset,
    long StoredSectorCount);
