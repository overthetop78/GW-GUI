using System.IO;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class SectorImageTests
{
    [Fact]
    public void ConstructorRejectsInvalidDimensionsAndLogicalBlockCount()
    {
        AssertInvalid(nameof(SectorImage.BlockSize), 0, () => new("test", 0, 1, 1, 1, []));
        AssertInvalid(nameof(SectorImage.Cylinders), 0, () => new("test", 1, 0, 1, 1, []));
        AssertInvalid(nameof(SectorImage.Heads), 0, () => new("test", 1, 1, 0, 1, []));
        AssertInvalid(nameof(SectorImage.SectorsPerTrack), 0, () => new("test", 1, 1, 1, 0, []));
        AssertInvalid("logicalBlockCount", 0, () => new("test", 1, 1, 1, 1, [], logicalBlockCount: 0));
    }

    [Fact]
    public void GetBlockReportsMissingBlockAndInvalidFixedSizeValues()
    {
        var missing = new SectorImage("test", 2, 1, 1, 1, []);
        var missingException = Assert.Throws<InvalidDataException>(() => missing.GetBlock(0));
        Assert.Equal("Le bloc logique 0 est absent.", missingException.Message);

        var invalidSize = new SectorImage("test", 2, 1, 1, 1, [new SectorBlock(0, new(0, 0, 0), [0x42])]);
        var sizeException = Assert.Throws<InvalidDataException>(() => invalidSize.GetBlock(0));
        Assert.Equal("Le bloc logique 0 contient 1 octets au lieu des 2 octets attendus.", sizeException.Message);
    }

    [Fact]
    public void VariableBlocksExposeCapacityAvailableAndMissingBlocks()
    {
        SectorBlock[] blocks =
        [
            new(0, new(0, 0, 0), [0x10]),
            new(2, new(0, 0, 2), [0x20, 0x21, 0x22])
        ];
        var image = new SectorImage("test", 2, 1, 1, 3, blocks, allowVariableBlockSize: true, capacity: 99, logicalBlockCount: 4);

        Assert.Equal(99, image.Capacity);
        Assert.Equal([0, 2], image.AvailableBlocks.Select(block => block.LogicalBlock).Order().ToArray());
        Assert.Equal([1, 3], image.MissingBlocks);
        Assert.Equal([0x10], image.GetBlock(0).ToArray());
        Assert.Equal([0x20, 0x21, 0x22], image.GetBlock(2).ToArray());
    }

    private static void AssertInvalid(string parameterName, int value, Func<SectorImage> create)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(create);
        Assert.Equal(char.ToLowerInvariant(parameterName[0]) + parameterName[1..], exception.ParamName);
        Assert.Equal(value, exception.ActualValue);
    }
}
