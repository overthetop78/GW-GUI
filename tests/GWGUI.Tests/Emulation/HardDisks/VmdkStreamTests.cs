using System.Buffers.Binary;
using System.IO.Compression;
using DiscUtils.Fat;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class VmdkStreamTests
{
    [Fact]
    public void IntegratedReaderReopensCompressedFilesAndExposesReadOnlyContent()
    {
        var payload = new byte[257 * 1024]; new Random(1234).NextBytes(payload);
        using var image = new MemoryStream();
        VmdkStreamImageWriter.Write(image, 8L << 20, content =>
        {
            FatVolumeFormatter.Format(content, "DATA");
            using var fs = new FatFileSystem(content);
            using var file = fs.OpenFile("DATA.BIN", FileMode.Create, FileAccess.Write); file.Write(payload);
        });
        using var disk = new DiscUtils.Vmdk.Disk(image, Ownership.None);
        Assert.False(disk.Content.CanWrite);
        using var reopened = new FatFileSystem(disk.Content);
        Assert.Equal("DATA", reopened.VolumeLabel.Trim());
        using var read = reopened.OpenFile("DATA.BIN", FileMode.Open, FileAccess.Read);
        Assert.Equal(payload, read.ReadExactly(payload.Length));
        Assert.Empty(DiskImageDependencyReader.Read(image, Path.GetFullPath("renamed.vmdk")));
    }

    [Fact]
    public void GrainTablesFooterAndZlibPayloadsCoverBothSidesOfATableBoundary()
    {
        const long boundary = 32L << 20;
        using var image = new MemoryStream();
        VmdkStreamImageWriter.Write(image, boundary + 512, content =>
        {
            content.Position = boundary - 1; content.WriteByte(11); content.WriteByte(12);
            content.Position = boundary + 511; content.WriteByte(13);
        });
        var bytes = image.ToArray();
        uint U32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset));
        ulong U64(int offset) => BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(offset));
        Assert.Equal(ulong.MaxValue, U64(56));
        var offset = checked((int)(U64(64) * 512));
        var grains = 0; var types = new List<uint>();
        while (offset < bytes.Length)
        {
            var size = U32(offset + 8);
            if (size != 0)
            {
                var lba = U64(offset);
                using var compressed = new MemoryStream(bytes, offset + 12, checked((int)size), writable: false);
                using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
                using var decoded = new MemoryStream(); zlib.CopyTo(decoded);
                var grain = decoded.ToArray(); Assert.Equal(65536, grain.Length);
                if (lba * 512 == boundary - 65536) Assert.Equal(11, grain[^1]);
                else
                {
                    Assert.Equal((ulong)boundary, lba * 512);
                    Assert.Equal(12, grain[0]); Assert.Equal(13, grain[511]);
                    Assert.All(grain.Skip(512), value => Assert.Equal(0, value));
                }
                offset = checked((int)((offset + 12L + size + 511) / 512 * 512)); grains++;
            }
            else
            {
                var type = U32(offset + 12); types.Add(type);
                var sectors = U64(offset);
                if (type == 3)
                {
                    Assert.Equal(1UL, sectors);
                    Assert.Equal("KDMV"u8.ToArray(), bytes[(offset + 512)..(offset + 516)]);
                    Assert.NotEqual(ulong.MaxValue, U64(offset + 512 + 56));
                }
                offset = checked(offset + 512 + (int)sectors * 512);
            }
        }
        Assert.Equal(2, grains); Assert.Equal(new uint[] { 1, 1, 2, 3, 0 }, types);
        using var disk = new DiscUtils.Vmdk.Disk(image, Ownership.None);
        disk.Content.Position = boundary - 1; Assert.Equal(11, disk.Content.ReadByte()); Assert.Equal(12, disk.Content.ReadByte());
        disk.Content.Position = boundary + 511; Assert.Equal(13, disk.Content.ReadByte());
    }

    [Fact]
    public void EmptyLargeImageHasSparseTablesAndReadsAsZeros()
    {
        using var image = new MemoryStream();
        DiskImageBuilder.Write(image, new(VmdkStreamImageWriter.MaximumCapacity, "vmdk-stream", "none", []));
        Assert.True(image.Length < 256 * 1024);
        using var disk = new DiscUtils.Vmdk.Disk(image, Ownership.None);
        Assert.Equal(VmdkStreamImageWriter.MaximumCapacity, disk.Capacity);
        disk.Content.Position = disk.Capacity - 512;
        Assert.All(disk.Content.ReadExactly(512), value => Assert.Equal(0, value));
    }

    [Fact]
    public void InvalidCapacityAllocationAndInitializerProduceNoImage()
    {
        using var image = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => VmdkStreamImageWriter.Write(image, VmdkStreamImageWriter.MaximumCapacity + 512));
        Assert.Throws<NotSupportedException>(() => DiskImageBuilder.Write(image, new(65536, "vmdk-stream", "none", [], true)));
        Assert.Throws<InvalidOperationException>(() => VmdkStreamImageWriter.Write(image, 65536, content => content.SetLength(0)));
        Assert.Equal(0, image.Length);
    }
}
