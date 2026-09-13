using System.Buffers.Binary;
using DiscUtils.Fat;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class SparseImageTests
{
    [Fact]
    public void PhysicalBandsMapToTheirLogicalIndexAndReopenTheFilesystem()
    {
        var payload = new byte[2 * 1024 * 1024 + 9]; new Random(11).NextBytes(payload);
        using var image = new SparseMemoryStream();
        SparseImageWriter.Write(image, 16L << 20, content =>
        {
            FatVolumeFormatter.Format(content, "DATA");
            using var fs = new FatFileSystem(content);
            using var file = fs.OpenFile("FILE.BIN", FileMode.Create, FileAccess.Write); file.Write(payload);
        }, 1 << 20);
        using var raw = Reassemble(image);
        using var reopened = new FatFileSystem(raw);
        using var read = reopened.OpenFile("FILE.BIN", FileMode.Open, FileAccess.Read);
        var actual = new byte[payload.Length]; read.ReadExactly(actual); Assert.Equal(payload, actual);
    }

    [Fact]
    public void FinalLogicalBandCanOccupyTheFirstPhysicalSlot()
    {
        const long capacity = (long)SparseImageWriter.MaximumBands * (1 << 20);
        using var image = new MemoryStream();
        SparseImageWriter.Write(image, capacity, content =>
        { content.Position = capacity - 1; content.WriteByte(73); }, 1 << 20);
        Assert.Equal(4096 + (1 << 20), image.Length);
        Assert.Equal((uint)SparseImageWriter.MaximumBands, BinaryPrimitives.ReadUInt32BigEndian(image.ToArray().AsSpan(64)));
        using var raw = Reassemble(image);
        Assert.Equal(capacity, raw.Length);
        Assert.Equal(0, raw.ReadByte()); raw.Position = capacity - 1; Assert.Equal(73, raw.ReadByte());
    }

    [Fact]
    public void EmptyComposedImageIsRecognizedAndCannotMasqueradeAsRaw()
    {
        using var image = new MemoryStream();
        DiskImageBuilder.Write(image, new DiskImagePlan(512, "sparseimage", "none", []));
        Assert.Equal(4096, image.Length);
        image.Position = 19; Assert.Equal("sparseimage", DiskContainerSignatures.Identify(image)); Assert.Equal(19, image.Position);
        var raw = new HardDiskImageFormat("raw", ".img", "block", 65536, 4096);
        Assert.Throws<InvalidDataException>(() => HardDiskImageValidation.ValidateExisting(image, "renamed.img", raw));
        Assert.Empty(DiskImageDependencyReader.Read(image, Path.GetFullPath("image.sparseimage")));
    }

    [Fact]
    public void PartialBandIsPaddedAndInvalidInputsLeaveDestinationEmpty()
    {
        using var image = new MemoryStream();
        SparseImageWriter.Write(image, 512, content => { content.Position = 511; content.WriteByte(42); }, 1 << 20);
        Assert.All(image.ToArray().Skip(4096 + 512), value => Assert.Equal(0, value));
        using var empty = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => SparseImageWriter.Write(empty,
            (long)SparseImageWriter.MaximumBands * SparseImageWriter.DefaultBandBytes + 512));
        Assert.Throws<NotSupportedException>(() => DiskImageBuilder.Write(empty, new DiskImagePlan(512, "sparseimage", "none", [], true)));
        Assert.Throws<IOException>(() => SparseImageWriter.Write(empty, 512, _ => throw new IOException()));
        Assert.Equal(0, empty.Length);
    }

    private static SparseMemoryStream Reassemble(Stream image)
    {
        var header = new byte[4096]; image.Position = 0; image.ReadExactly(header);
        uint U32(int offset) => BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(offset));
        Assert.True(header.AsSpan(0, 4).SequenceEqual("sprs"u8)); Assert.Equal(3U, U32(4)); Assert.Equal(1U, U32(12));
        var bandBytes = (long)U32(8) * 512;
        var raw = new SparseMemoryStream(); raw.SetLength((long)U32(16) * 512);
        var seen = new HashSet<uint>();
        var bytes = new byte[bandBytes];
        for (var i = 0; i < SparseImageWriter.MaximumBands; i++)
        {
            var reference = U32(64 + i * 4); if (reference == 0) continue;
            Assert.True(seen.Add(reference));
            image.Position = 4096 + i * bandBytes; image.ReadExactly(bytes);
            raw.Position = (reference - 1) * bandBytes;
            Assert.True(raw.Position < raw.Length);
            raw.Write(bytes.AsSpan(0, (int)Math.Min(bytes.Length, raw.Length - raw.Position)));
        }
        raw.Position = 0; return raw;
    }
}
