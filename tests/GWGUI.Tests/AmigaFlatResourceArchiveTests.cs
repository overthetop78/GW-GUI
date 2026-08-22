using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Tests;

public sealed class AmigaFlatResourceArchiveTests
{
    [Fact]
    public void ReadsNamedResourcesFromTheFlatPayload()
    {
        var image = BuildImage();

        var reader = new AmigaFlatResourceArchiveReader();
        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Equal(FileSystemIds.AmigaFlatResourceArchive, volume.FileSystemId);
        Assert.Empty(volume.Name);
        var resource = Assert.Single(volume.Entries);
        Assert.Equal("rampic", resource.Name);
        Assert.Equal("FORM", Encoding.ASCII.GetString(resource.Content!.ToArray()));
    }

    [Fact]
    public void RejectsAnArchiveWhoseDirectoryBlockIsInvalid()
    {
        var image = BuildImage(directoryIntegrityValid: false);

        Assert.False(new AmigaFlatResourceArchiveReader().CanRead(image));
    }

    [Fact]
    public void KeepsAResourceVisibleAndWarnsWhenItsPayloadBlockIsMissing()
    {
        var image = BuildImage(includePayload: false);
        var volume = new AmigaFlatResourceArchiveReader().Read(image);

        var resource = Assert.Single(volume.Entries);
        Assert.True(resource.MetadataValid);
        Assert.Equal(new byte[4], resource.Content!.ToArray());
        Assert.Single(volume.Warnings, warning => warning.Contains("missing source block(s) 3", StringComparison.Ordinal));
    }

    private static SectorImage BuildImage(bool includePayload = true, bool directoryIntegrityValid = true)
    {
        const int blockSize = 512;
        var directory = Enumerable.Repeat(byte.MaxValue, blockSize).ToArray();
        WriteDescriptor(directory, 0, "Reserved", 3 * blockSize, zeroPadding: true);
        WriteDescriptor(directory, 16, "rampic", 4, zeroPadding: false);
        var payload = new byte[blockSize];
        "FORM"u8.CopyTo(payload);
        var blocks = new List<SectorBlock>
        {
            Block(0, new byte[blockSize]),
            Block(1, new byte[blockSize]),
            Block(2, directory, directoryIntegrityValid)
        };
        if (includePayload) blocks.Add(Block(3, payload));
        return new(DiskImageFormatIds.AmigaDos, blockSize, 2, 1, 2, blocks);
    }

    private static SectorBlock Block(int logical, byte[] data, bool? integrityValid = true) =>
        new(logical, new(logical / 2, 0, logical % 2), data, integrityValid);

    private static void WriteDescriptor(Span<byte> destination, int offset, string name, uint length, bool zeroPadding)
    {
        destination.Slice(offset, 12).Fill(zeroPadding ? (byte)0 : byte.MaxValue);
        Encoding.ASCII.GetBytes(name).CopyTo(destination[offset..]);
        destination[offset + name.Length] = 0;
        BinaryPrimitives.WriteUInt32BigEndian(destination.Slice(offset + 12, sizeof(uint)), length);
    }
}
