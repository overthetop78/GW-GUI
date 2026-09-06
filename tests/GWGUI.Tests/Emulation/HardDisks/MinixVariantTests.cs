using System.Buffers.Binary;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class MinixVariantTests
{
    public static IEnumerable<object[]> Variants()
    {
        foreach (var version in new[] { 1, 2, 3 })
        foreach (var order in Enum.GetValues<MinixStorageOrder>())
        foreach (var names in version == 3 ? new[] { 60 } : new[] { 14, 30 })
            yield return [version, order, names];
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void MetadataAndBitmapWordOrderAgreeForEveryRegisteredVariant(int version, MinixStorageOrder order, int names)
    {
        var suffix = order switch { MinixStorageOrder.BigEndian16 => "-be16", MinixStorageOrder.BigEndian32 => "-be32",
            MinixStorageOrder.BigEndian64 => "-be64", _ => "" };
        var id = $"minix{version}{suffix}{(names == 14 ? "-n14" : "")}";
        using var stream = new MemoryStream();
        DiskImageBuilder.Write(stream, new(1024L * 1031, "raw", "none", [new(0, 1024L * 1031, id, "")]));
        var bytes = stream.ToArray();
        var bigEndian = order != MinixStorageOrder.LittleEndian;
        ushort U16(int offset) => bigEndian ? BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset)) : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset));
        uint U32(int offset) => bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset)) : BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset));
        var imaps = U16(1024 + (version == 3 ? 6 : 4));
        var zmaps = U16(1024 + (version == 3 ? 8 : 6));
        var first = U16(1024 + (version == 3 ? 10 : 8));
        var magic = version == 1 ? (names == 14 ? 0x137f : 0x138f) : version == 2 ? (names == 14 ? 0x2468 : 0x2478) : 0x4d5a;
        Assert.Equal(magic, U16(1024 + (version == 3 ? 24 : 16)));
        var inode = (2 + imaps + zmaps) * 1024;
        var entry = names + (version == 3 ? 4 : 2);
        Assert.Equal((uint)(entry * 2), U32(inode + (version == 1 ? 4 : 8)));
        Assert.Equal((uint)first, version == 1 ? U16(inode + 14) : U32(inode + 24));
        Assert.Equal(1U, version == 3 ? U32(first * 1024) : U16(first * 1024));
        Assert.Equal(1U, version == 3 ? U32(first * 1024 + entry) : U16(first * 1024 + entry));
        var width = version == 3 ? 4 : 2;
        Assert.Equal(".."u8.ToArray(), bytes[(first * 1024 + entry + width)..(first * 1024 + entry + width + 2)]);
        var wordBytes = order switch { MinixStorageOrder.BigEndian16 => 2, MinixStorageOrder.BigEndian32 => 4,
            MinixStorageOrder.BigEndian64 => 8, _ => 1 };
        bool Allocated(int start, int bit)
        {
            var word = bit / (wordBytes * 8);
            var bitInWord = bit % (wordBytes * 8);
            var byteInWord = bigEndian ? wordBytes - 1 - bitInWord / 8 : bitInWord / 8;
            return (bytes[start + word * wordBytes + byteInWord] & (1 << (bitInWord % 8))) != 0;
        }
        for (var bit = 0; bit < imaps * 8192; bit++)
            Assert.Equal(bit < 2 || bit >= 257, Allocated(2048, bit));
        var validZones = 1031 - first + 1;
        for (var bit = 0; bit < zmaps * 8192; bit++)
            Assert.Equal(bit < 2 || bit >= validZones, Allocated((2 + imaps) * 1024, bit));
    }

    [Fact]
    public void InvalidNameAndByteOrderVariantsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MinixVolumeFormatter.Validate(1L << 20, 3, nameLength: 14));
        Assert.Throws<ArgumentOutOfRangeException>(() => MinixVolumeFormatter.Validate(1L << 20, 1, nameLength: 60));
        Assert.Throws<ArgumentOutOfRangeException>(() => MinixVolumeFormatter.Validate(1L << 20, 2, order: (MinixStorageOrder)99));
    }
}
