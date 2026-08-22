using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Acorn.FileCore;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
using GWGUI.MediaEngine.FileSystems.Acorn;

namespace GWGUI.Tests;

public sealed class AcornFileCoreNewMapTests
{
    [Fact]
    public void DiscRecordReadsEveryStoredField()
    {
        var data = CreateDiscRecord();
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(AcornFileCoreDiscRecordLayout.DiscSizeLowOffset), 0x89abcdef);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(AcornFileCoreDiscRecordLayout.DiscSizeHighOffset), 1);
        const long capacity = 0x200000000;
        Assert.True(AcornFileCoreDiscRecord.TryParse(data, capacity, out var record));
        Assert.NotNull(record);
        Assert.Equal(8, record.Log2SectorSize);
        Assert.Equal(11, record.IdLength);
        Assert.Equal(0, record.Log2BytesPerMapBit);
        Assert.Equal(1, record.ZoneCount);
        Assert.Equal(1, record.ZoneSpareBits);
        Assert.Equal(0x200, record.RootAddress);
        Assert.Equal(0x189abcdef, record.DiscSize);
        Assert.Equal("TEST", record.DiscName);
        Assert.Equal(3, record.Log2ShareSize);
    }

    [Theory]
    [InlineData(AcornFileCoreDiscRecordLayout.Log2SectorSizeOffset, 7)]
    [InlineData(AcornFileCoreDiscRecordLayout.Log2SectorSizeOffset, 11)]
    [InlineData(AcornFileCoreDiscRecordLayout.IdLengthOffset, 10)]
    [InlineData(AcornFileCoreDiscRecordLayout.IdLengthOffset, 20)]
    [InlineData(AcornFileCoreDiscRecordLayout.Log2BytesPerMapBitOffset, 9)]
    [InlineData(AcornFileCoreDiscRecordLayout.ZoneCountLowOffset, 0)]
    public void DiscRecordRejectsInvalidLimits(int offset, byte value)
    {
        var data = CreateDiscRecord();
        data[offset] = value;
        Assert.False(AcornFileCoreDiscRecord.TryParse(data, 4096, out _));
    }

    [Fact]
    public void DiscRecordRejectsInvalidRootSizeAndZoneReserve()
    {
        var data = CreateDiscRecord();
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(AcornFileCoreDiscRecordLayout.RootAddressOffset), 0);
        Assert.False(AcornFileCoreDiscRecord.TryParse(data, 4096, out _));
        data = CreateDiscRecord();
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(AcornFileCoreDiscRecordLayout.DiscSizeLowOffset), 0);
        Assert.False(AcornFileCoreDiscRecord.TryParse(data, 4096, out _));
        data = CreateDiscRecord();
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(AcornFileCoreDiscRecordLayout.DiscSizeLowOffset), 4097);
        Assert.False(AcornFileCoreDiscRecord.TryParse(data, 4096, out _));
        data = CreateDiscRecord();
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AcornFileCoreDiscRecordLayout.ZoneSpareBitsOffset), 2048);
        Assert.False(AcornFileCoreDiscRecord.TryParse(data, 4096, out _));
    }

    [Fact]
    public void BitReaderHandlesBoundariesAndInvalidRanges()
    {
        byte[] data = [0b1000_0001, 0b0000_0011];
        Assert.Equal(3u, AcornFileCoreBitReader.GetBits(data, 7, 0b11));
        Assert.Equal(7, AcornFileCoreBitReader.FindNextSetBit(data, 1, 9));
        Assert.Equal(16, AcornFileCoreBitReader.FindNextSetBit(data, 10, 16));
        Assert.Throws<ArgumentOutOfRangeException>(() => AcornFileCoreBitReader.GetBits(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => AcornFileCoreBitReader.GetBits(data, 15, 0b11));
        Assert.Throws<ArgumentOutOfRangeException>(() => AcornFileCoreBitReader.FindNextSetBit(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => AcornFileCoreBitReader.FindNextSetBit(data, 2, 17));
    }

    [Fact]
    public void ZoneCopiesItsDataAndValidatesItsBounds()
    {
        byte[] source = [1, 2];
        var zone = new AcornFileCoreZone(source, 0, 1, 16);
        source[0] = 9;
        Assert.Equal(1, zone.Data[0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcornFileCoreZone(source, 0, 8, 8));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcornFileCoreZone(source, 0, 1, 17));
    }

    [Fact]
    public void AddressAndShiftApplyFileCoreRules()
    {
        var address = AcornFileCoreAddress.Decode(0x123456);
        Assert.Equal(0x1234u, address.FragmentId);
        Assert.Equal(0x56, address.ShareOffset);
        Assert.Equal(16, AcornFileCoreShift.Apply(8, 1));
        Assert.Equal(4, AcornFileCoreShift.Apply(8, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => AcornFileCoreShift.Apply(1, 32));
        Assert.Throws<OverflowException>(() => AcornFileCoreShift.Apply(int.MaxValue, 1));
    }

    [Fact]
    public void OneZoneImageCreatesANewMap()
    {
        var bytes = new byte[256];
        var record = CreateDiscRecord();
        BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(AcornFileCoreDiscRecordLayout.DiscSizeLowOffset), 256);
        record.CopyTo(bytes, AcornFileCoreLayout.DiscRecordOffset);
        var image = new SectorImage(DiskImageFormatIds.AcornAdfs800, 256, 1, 1, 1, [new SectorBlock(0, new(0, 0, 0), bytes)]);
        Assert.True(AcornFileCoreNewMap.TryCreate(image, out var map));
        Assert.NotNull(map);
        Assert.True(map.RootAddress > 0);
        Assert.False(string.IsNullOrWhiteSpace(map.VolumeName));
        Assert.Equal(0, map.FreeBytes);
        Assert.False(map.TryResolveByteOffset(0, 0, out _));
        Assert.False(map.TryResolveByteOffset(map.RootAddress, -1, out _));
    }

    [Fact]
    public void MultipleZonesResolveRootAndDistributedFragments()
    {
        var rootImage = CreateTwoZoneImage((first, second) => WriteFragment(second, 32, AcornFileCoreLayout.RootFragmentId, 256));
        Assert.True(AcornFileCoreNewMap.TryCreate(rootImage, out var rootMap));
        Assert.True(rootMap!.TryResolveByteOffset(rootMap.RootAddress, 0, out var rootOffset));
        Assert.Equal(4 * 256, rootOffset);

        var ordinaryImage = CreateTwoZoneImage((first, second) =>
        {
            WriteFragment(first, 512, 1, 256);
            WriteFragment(second, 32, 1, 256);
        }, log2ShareSize: 0);
        Assert.True(AcornFileCoreNewMap.TryCreate(ordinaryImage, out var ordinaryMap));
        Assert.True(ordinaryMap!.TryResolveByteOffset((1 << 8) | 2, 0, out var sharedOffset));
        Assert.Equal(4 * 256, sharedOffset);
        Assert.False(ordinaryMap.TryResolveByteOffset(1 << 8, 0, out _));
        Assert.False(ordinaryMap.TryResolveByteOffset(0x7fffff00, 0, out _));
    }

    [Fact]
    public void FreeListHandlesTerminationOutsideZoneAndCapacityCap()
    {
        var validImage = CreateOneZoneImage(bytes =>
        {
            WriteBits(bytes, AcornFileCoreLayout.FreeLinkBitOffset, 504, 11);
            WriteFragment(bytes, 512, 12, 12);
            WriteFragment(bytes, 524, 0, 12);
        });
        Assert.True(AcornFileCoreNewMap.TryCreate(validImage, out var validMap));
        Assert.Equal(24, validMap!.FreeBytes);

        var outsideImage = CreateOneZoneImage(bytes => WriteBits(bytes, AcornFileCoreLayout.FreeLinkBitOffset, 1000, 11));
        Assert.True(AcornFileCoreNewMap.TryCreate(outsideImage, out var outsideMap));
        Assert.Equal(0, outsideMap!.FreeBytes);

        var cappedImage = CreateOneZoneImage(bytes =>
        {
            WriteBits(bytes, AcornFileCoreLayout.FreeLinkBitOffset, 504, 11);
            WriteFragment(bytes, 512, 0, 64);
        }, log2BytesPerMapBit: 2);
        Assert.True(AcornFileCoreNewMap.TryCreate(cappedImage, out var cappedMap));
        Assert.Equal(cappedImage.Capacity, cappedMap!.FreeBytes);
        Assert.Throws<OverflowException>(() => AcornFileCoreNewMap.AdvanceFreeLink(int.MaxValue, 1));
    }

    private static byte[] CreateDiscRecord()
    {
        var data = new byte[AcornFileCoreLayout.DiscRecordLength];
        data[AcornFileCoreDiscRecordLayout.Log2SectorSizeOffset] = 8;
        data[AcornFileCoreDiscRecordLayout.IdLengthOffset] = 11;
        data[AcornFileCoreDiscRecordLayout.Log2BytesPerMapBitOffset] = 0;
        data[AcornFileCoreDiscRecordLayout.ZoneCountLowOffset] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AcornFileCoreDiscRecordLayout.ZoneSpareBitsOffset), 1);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(AcornFileCoreDiscRecordLayout.RootAddressOffset), 0x200);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(AcornFileCoreDiscRecordLayout.DiscSizeLowOffset), 4096);
        "TEST"u8.CopyTo(data.AsSpan(AcornFileCoreDiscRecordLayout.DiscNameOffset));
        data[AcornFileCoreDiscRecordLayout.Log2ShareSizeOffset] = 3;
        return data;
    }

    private static SectorImage CreateOneZoneImage(Action<byte[]> configureZone, int log2BytesPerMapBit = 0)
    {
        var bytes = new byte[256];
        var record = CreateDiscRecord();
        record[AcornFileCoreDiscRecordLayout.Log2BytesPerMapBitOffset] = checked((byte)log2BytesPerMapBit);
        BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(AcornFileCoreDiscRecordLayout.DiscSizeLowOffset), 256);
        record.CopyTo(bytes, AcornFileCoreLayout.DiscRecordOffset);
        configureZone(bytes);
        return new SectorImage(DiskImageFormatIds.AcornAdfs800, 256, 1, 1, 1, [new SectorBlock(0, new(0, 0, 0), bytes)]);
    }

    private static SectorImage CreateTwoZoneImage(Action<byte[], byte[]> configureZones, int log2ShareSize = 3)
    {
        const int blockSize = 256;
        const int blockCount = 6;
        var firstBlock = new byte[blockSize];
        var firstZone = new byte[blockSize];
        var secondZone = new byte[blockSize];
        var record = CreateDiscRecord();
        record[AcornFileCoreDiscRecordLayout.ZoneCountLowOffset] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(AcornFileCoreDiscRecordLayout.ZoneSpareBitsOffset), 512);
        BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(AcornFileCoreDiscRecordLayout.DiscSizeLowOffset), blockSize * blockCount);
        record[AcornFileCoreDiscRecordLayout.Log2ShareSizeOffset] = checked((byte)log2ShareSize);
        record.CopyTo(firstBlock, AcornFileCoreLayout.DiscRecordOffset);
        configureZones(firstZone, secondZone);
        var blocks = new[]
        {
            new SectorBlock(0, new(0, 0, 0), firstBlock),
            new SectorBlock(4, new(0, 0, 4), firstZone),
            new SectorBlock(5, new(0, 0, 5), secondZone)
        };
        return new SectorImage(DiskImageFormatIds.AcornAdfs800, blockSize, 1, 1, blockCount, blocks);
    }

    private static void WriteFragment(byte[] data, int startBit, uint fragmentId, int length)
    {
        WriteBits(data, startBit, fragmentId, 11);
        SetBit(data, checked(startBit + length - 1));
    }

    private static void WriteBits(byte[] data, int startBit, uint value, int bitCount)
    {
        for (var index = 0; index < bitCount; index++)
        {
            if ((value & (1u << index)) != 0) SetBit(data, startBit + index);
        }
    }

    private static void SetBit(byte[] data, int bit) => data[bit / 8] |= checked((byte)(1 << (bit & 7)));

}
