using System.Buffers.Binary;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Formats.Floppy.Atx;

namespace GWGUI.Tests;

public sealed class AtxReaderTests
{
    [Fact]
    public void ReadsTracksAndPrefersAValidDuplicateSector()
    {
        var data = CreateAtx();

        var image = AtxReader.Read(data);

        Assert.Equal(DiskImageFormatIds.AtariAtx, image.FormatId);
        Assert.Equal((128, 40, 1, 18, 720), (image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, image.BlockCount));
        var sector = Assert.Single(image.AvailableBlocks);
        Assert.Equal(0, sector.LogicalBlock);
        Assert.True(sector.IntegrityValid);
        Assert.Equal((byte)0, sector.DiagnosticCode);
        Assert.All(sector.Data, value => Assert.Equal(0xbb, value));
        Assert.Equal(719, image.MissingBlocks.Count);
    }

    [Fact]
    public void RejectsSectorDataOutsideItsTrack()
    {
        var data = CreateAtx();
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(92), uint.MaxValue);

        Assert.Throws<InvalidDataException>(() => AtxReader.Read(data));
    }

    private static byte[] CreateAtx()
    {
        const int trackOffset = 48;
        const int listOffset = trackOffset + 32;
        const int firstDataOffset = listOffset + 8 + 16;
        var data = new byte[firstDataOffset + 256];
        "AT8X"u8.CopyTo(data);
        data[4] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(28), trackOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(trackOffset + 10), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(trackOffset + 20), 32);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(listOffset), 24);

        data[listOffset + 8] = 1;
        data[listOffset + 9] = 0x08;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(listOffset + 10), 10);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(listOffset + 12), firstDataOffset - trackOffset);
        data[listOffset + 16] = 1;
        data[listOffset + 17] = 0;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(listOffset + 18), 20);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(listOffset + 20), firstDataOffset + 128 - trackOffset);
        data.AsSpan(firstDataOffset, 128).Fill(0xaa);
        data.AsSpan(firstDataOffset + 128, 128).Fill(0xbb);
        return data;
    }
}
