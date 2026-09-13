using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Formats.Floppy.Xfd;

namespace GWGUI.Tests.MediaEngine.Reading;

public sealed class XfdReaderTests
{
    [Theory]
    [InlineData(90 * 1024, DiskImageFormatIds.AtariXfd90, 128, 720, false)]
    [InlineData(130 * 1024, DiskImageFormatIds.AtariXfd130, 128, 1040, false)]
    [InlineData(140 * 1024, DiskImageFormatIds.AtariXfd140, 128, 1120, false)]
    [InlineData(183936, DiskImageFormatIds.AtariXfd180, 256, 720, true)]
    public void ReadsKnownHeaderlessAtariLayouts(
        int length,
        string expectedFormat,
        int expectedBlockSize,
        int expectedBlockCount,
        bool variableBlockSize)
    {
        var image = XfdReader.Read(new byte[length]);

        Assert.Equal(expectedFormat, image.FormatId);
        Assert.Equal(expectedBlockSize, image.BlockSize);
        Assert.Equal(expectedBlockCount, image.BlockCount);
        Assert.Equal(length, image.Capacity);
        Assert.Equal(variableBlockSize, image.AllowsVariableBlockSize);
        Assert.Equal(expectedBlockCount, image.AvailableBlocks.Count);
        Assert.Empty(image.MissingBlocks);
    }

    [Fact]
    public void RejectsUnknownImageLength()
    {
        Assert.Throws<InvalidDataException>(() => XfdReader.Read(new byte[12345]));
    }
}
