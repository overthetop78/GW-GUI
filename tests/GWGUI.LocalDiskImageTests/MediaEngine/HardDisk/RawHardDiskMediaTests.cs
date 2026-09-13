using System.Buffers.Binary;
using System.Text;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Exploration.Partitioning;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.Tests.MediaEngine.HardDisk;

public sealed class RawHardDiskMediaTests
{
    private const int SectorSize = 512;

    [Fact]
    public void BlockRepresentationKeepsSixtyFourBitAddresses()
    {
        var length = (long)uint.MaxValue + SectorSize + 1;
        length -= length % SectorSize;

        var representation = new BlockMediaImageRepresentation(
            length,
            SectorSize,
            [new MediaDataRange(0, length, MediaDataRangeKind.Unallocated)]);

        Assert.Equal(length, representation.Capacity);
        Assert.Equal(length / SectorSize, representation.LogicalBlockCount);
        Assert.Null(representation.Geometry);
    }

    [Fact]
    public async Task MbrDetectorReturnsPrimaryPartition()
    {
        var bytes = new byte[16 * SectorSize];
        WriteMbrEntry(bytes, 0, 0x0B, 1, 4);
        WriteBootSignature(bytes, 0);

        var result = await new MbrVolumeDetector().DetectAsync(CreateDocument(bytes));

        var volume = Assert.Single(result!.Volumes);
        Assert.Equal(PartitionSchemeIds.Mbr, volume.PartitionScheme);
        Assert.Equal(SectorSize, volume.Start);
        Assert.Equal(4 * SectorSize, volume.Length);
        Assert.Equal("0x0B", volume.PartitionType);
    }

    [Fact]
    public async Task MbrDetectorFollowsExtendedPartitionChain()
    {
        var bytes = new byte[16 * SectorSize];
        WriteMbrEntry(bytes, 0, 0x0F, 1, 10);
        WriteBootSignature(bytes, 0);
        WriteMbrEntry(bytes, 1, 0x06, 1, 2);
        WriteBootSignature(bytes, 1);

        var result = await new MbrVolumeDetector().DetectAsync(CreateDocument(bytes));

        var volume = Assert.Single(result!.Volumes);
        Assert.Equal(2 * SectorSize, volume.Start);
        Assert.Equal(2 * SectorSize, volume.Length);
        Assert.Equal("0x06", volume.PartitionType);
    }

    [Fact]
    public async Task GptDetectorReturnsNamedPartition()
    {
        var bytes = CreateGptImage();

        var result = await new GptVolumeDetector().DetectAsync(CreateDocument(bytes));

        var volume = Assert.Single(result!.Volumes);
        Assert.Equal(PartitionSchemeIds.Gpt, volume.PartitionScheme);
        Assert.Equal(34 * SectorSize, volume.Start);
        Assert.Equal(17 * SectorSize, volume.Length);
        Assert.Equal("Data", volume.Name);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public async Task WholeMediaDetectorKeepsUnknownGeometryAndCreatesDirectVolume()
    {
        var document = CreateDocument(new byte[8 * SectorSize]);

        var result = await new WholeMediaVolumeDetector().DetectAsync(document);

        var representation = Assert.IsType<BlockMediaImageRepresentation>(document.Representation);
        var volume = Assert.Single(result!.Volumes);
        Assert.Null(representation.Geometry);
        Assert.Equal(MediaVolumeOrigins.DirectVolume, volume.Origin);
        Assert.Equal(PartitionSchemeIds.Direct, volume.PartitionScheme);
        Assert.Equal(representation.Capacity, volume.Length);
    }

    private static MediaImageDocument CreateDocument(byte[] bytes)
    {
        var source = new MemoryRandomAccessData(bytes);
        return new MediaImageDocument(
            new MediaSourceDescriptor("memory.img", [], bytes.LongLength, HardDiskImageFormatIds.Raw),
            HardDiskImageFormatIds.Raw,
            MediaKind.HardDisk,
            new BlockMediaImageRepresentation(
                bytes.LongLength,
                SectorSize,
                [new MediaDataRange(0, bytes.LongLength, MediaDataRangeKind.Stored, source)]),
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private static byte[] CreateGptImage()
    {
        const int sectorCount = 100;
        var bytes = new byte[sectorCount * SectorSize];
        var entries = new byte[SectorSize];
        new Guid("EBD0A0A2-B9E5-4433-87C0-68B6B72699C7").TryWriteBytes(entries.AsSpan(0, 16));
        new Guid("1B4519D1-2E41-4FC6-A622-40D205A32A3F").TryWriteBytes(entries.AsSpan(16, 16));
        BinaryPrimitives.WriteUInt64LittleEndian(entries.AsSpan(32, 8), 34);
        BinaryPrimitives.WriteUInt64LittleEndian(entries.AsSpan(40, 8), 50);
        Encoding.Unicode.GetBytes("Data").CopyTo(entries, 56);
        var entryCrc = ComputeCrc32(entries);
        entries.CopyTo(bytes, 2 * SectorSize);
        entries.CopyTo(bytes, 98 * SectorSize);

        var diskId = new Guid("42F52341-E05C-478B-B517-9BA6EB38BAA2");
        WriteGptHeader(bytes.AsSpan(SectorSize, SectorSize), 1, 99, 2, diskId, entryCrc);
        WriteGptHeader(bytes.AsSpan(99 * SectorSize, SectorSize), 99, 1, 98, diskId, entryCrc);
        return bytes;
    }

    private static void WriteGptHeader(
        Span<byte> header,
        ulong currentLba,
        ulong backupLba,
        ulong entryArrayLba,
        Guid diskId,
        uint entryCrc)
    {
        "EFI PART"u8.CopyTo(header);
        BinaryPrimitives.WriteUInt32LittleEndian(header[8..12], 0x00010000);
        BinaryPrimitives.WriteUInt32LittleEndian(header[12..16], 92);
        BinaryPrimitives.WriteUInt64LittleEndian(header[24..32], currentLba);
        BinaryPrimitives.WriteUInt64LittleEndian(header[32..40], backupLba);
        BinaryPrimitives.WriteUInt64LittleEndian(header[40..48], 34);
        BinaryPrimitives.WriteUInt64LittleEndian(header[48..56], 97);
        diskId.TryWriteBytes(header[56..72]);
        BinaryPrimitives.WriteUInt64LittleEndian(header[72..80], entryArrayLba);
        BinaryPrimitives.WriteUInt32LittleEndian(header[80..84], 4);
        BinaryPrimitives.WriteUInt32LittleEndian(header[84..88], 128);
        BinaryPrimitives.WriteUInt32LittleEndian(header[88..92], entryCrc);
        BinaryPrimitives.WriteUInt32LittleEndian(header[16..20], ComputeCrc32(header[..92]));
    }

    private static void WriteMbrEntry(byte[] bytes, int sector, byte type, uint firstLba, uint sectorCount)
    {
        var entry = bytes.AsSpan(sector * SectorSize + 446, 16);
        entry[4] = type;
        BinaryPrimitives.WriteUInt32LittleEndian(entry[8..12], firstLba);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[12..16], sectorCount);
    }

    private static void WriteBootSignature(byte[] bytes, int sector)
    {
        bytes[sector * SectorSize + 510] = 0x55;
        bytes[sector * SectorSize + 511] = 0xAA;
    }

    private static uint ComputeCrc32(ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc >> 1) ^ (0xEDB88320u & (uint)-(int)(crc & 1));
        }
        return ~crc;
    }
}
