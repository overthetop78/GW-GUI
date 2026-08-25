using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

public sealed class SectorImageTests
{
    [Fact]
    public void ConstructorRejectsInvalidDimensionsAndLogicalBlockCount()
    {
        Assert.Throws<ArgumentException>(() => new SectorImage(" ", 1, 1, 1, 1, []));
        AssertInvalid(nameof(SectorImage.BlockSize), 0, () => new("test", 0, 1, 1, 1, []));
        AssertInvalid(nameof(SectorImage.Cylinders), 0, () => new("test", 1, 0, 1, 1, []));
        AssertInvalid(nameof(SectorImage.Heads), 0, () => new("test", 1, 1, 0, 1, []));
        AssertInvalid(nameof(SectorImage.SectorsPerTrack), 0, () => new("test", 1, 1, 1, 0, []));
        AssertInvalid(nameof(SectorImage.Capacity), 0, () => new("test", 1, 1, 1, 1, [], capacity: 0));
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
        Assert.Equal("La propriété Data du bloc logique 0 vaut '1' ; valeur attendue : '2'.", sizeException.Message);
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

    [Fact]
    public void ConstructorRejectsDuplicateAndOutOfRangeLogicalBlocksAndInsufficientCapacity()
    {
        var duplicate = Assert.Throws<InvalidDataException>(() => new SectorImage("test", 1, 1, 1, 2,
        [
            new(0, new(0, 0, 0), [0x10]),
            new(0, new(0, 0, 1), [0x20])
        ]));
        Assert.Contains(nameof(SectorBlock.LogicalBlock), duplicate.Message);

        Assert.Throws<InvalidDataException>(() => new SectorImage("test", 1, 1, 1, 1, [new(-1, new(0, 0, 0), [0x10])]));
        Assert.Throws<InvalidDataException>(() => new SectorImage("test", 1, 1, 1, 1, [new(1, new(0, 0, 0), [0x10])]));

        var capacity = Assert.Throws<InvalidDataException>(() => new SectorImage("test", 2, 1, 1, 1, [new(0, new(0, 0, 0), [0x10, 0x20])], capacity: 1));
        Assert.Contains(nameof(SectorImage.Capacity), capacity.Message);
        Assert.Contains("1", capacity.Message);
        Assert.Contains("2", capacity.Message);
    }

    [Fact]
    public void ConstructorCopiesBlockCollectionDataAndTags()
    {
        byte[] data = [0x10, 0x11];
        byte[] tag = [0x20];
        var source = new List<SectorBlock> { new(0, new(0, 0, 0), data, Tag: tag) };
        var image = new SectorImage("test", 2, 1, 1, 1, source);

        data[0] = 0xFF;
        tag[0] = 0xFF;
        source.Clear();

        var block = Assert.Single(image.AvailableBlocks);
        Assert.Equal([0x10, 0x11], block.Data);
        Assert.Equal([0x20], block.Tag);
        Assert.Throws<NotSupportedException>(() => ((IList<byte>)block.Data)[0] = 0xFF);
        Assert.Throws<NotSupportedException>(() => ((IList<byte>)block.Tag!)[0] = 0xFF);
    }

    private static void AssertInvalid(string parameterName, int value, Func<SectorImage> create)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(create);
        Assert.Equal(char.ToLowerInvariant(parameterName[0]) + parameterName[1..], exception.ParamName);
        Assert.Equal((long)value, Convert.ToInt64(exception.ActualValue));
    }
}
