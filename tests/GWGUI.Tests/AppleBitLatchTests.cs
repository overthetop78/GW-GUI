using GWGUI.MediaEngine.Decoding.Apple;
using GWGUI.MediaEngine.Exploration.Scp;

namespace GWGUI.Tests;

public sealed class AppleBitLatchTests
{
    [Theory]
    [InlineData("10100101", 8)]
    [InlineData("010100101", 9)]
    public void ReadsSynchronizedByteWithOrWithoutInitialShift(string source, int expectedOffset)
    {
        var bits = source.Select(bit => bit == '1').ToArray();
        var offset = 0;

        var result = AppleBitLatch.TryReadBytes(bits, ref offset, 1);

        Assert.Equal([0xA5], result);
        Assert.Equal(expectedOffset, offset);
    }

    [Fact]
    public void ReadsSuccessiveBytesAndAdvancesOffsetToConsumedBits()
    {
        var bits = "1010010111000011".Select(bit => bit == '1').ToArray();
        var offset = 0;

        var result = AppleBitLatch.TryReadBytes(bits, ref offset, 2);

        Assert.Equal([0xA5, 0xC3], result);
        Assert.Equal(16, offset);
    }

    [Fact]
    public void ReturnsNullForTruncatedStream()
    {
        var offset = 0;

        var result = AppleBitLatch.TryReadBytes(new bool[7], ref offset, 1);

        Assert.Null(result);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void RejectsNegativeByteCount()
    {
        var offset = 0;

        Assert.Throws<ArgumentOutOfRangeException>(() => AppleBitLatch.TryReadBytes([], ref offset, -1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void RejectsOffsetOutsideBitStream(int invalidOffset)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AppleBitLatch.TryReadBytes([true], ref invalidOffset, 0));
    }
}
