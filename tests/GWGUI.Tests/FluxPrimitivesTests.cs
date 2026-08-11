using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

public sealed class FluxPrimitivesTests
{
    [Fact]
    public void FluxBitstreamCopiesSourceBits()
    {
        var source = new[] { true, false, true };
        var stream = new FluxBitstream(source, 1d);

        source[0] = false;
        source[1] = true;

        Assert.Equal([true, false, true], stream.Bits.ToArray());
    }

    [Fact]
    public void FluxBitstreamAppliesCircularTailLimits()
    {
        var stream = new FluxBitstream([true, false, true], 1d);

        Assert.Same(stream, stream.WithCircularTail(0));
        Assert.Same(stream, stream.WithCircularTail(-1));
        Assert.Equal([true, false, true, true, false], stream.WithCircularTail(2).Bits.ToArray());
        Assert.Equal([true, false, true, true, false, true], stream.WithCircularTail(3).Bits.ToArray());
        Assert.Equal([true, false, true, true, false, true], stream.WithCircularTail(4).Bits.ToArray());
    }

    [Fact]
    public void FluxBitstreamValidatesCellDurationAndLargeTailRequest()
    {
        var stream = new FluxBitstream([true, false], 1d);

        Assert.Equal(1d, stream.BitCellTicks);
        Assert.Throws<ArgumentOutOfRangeException>(() => new FluxBitstream([true], 0.999d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FluxBitstream([true], double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FluxBitstream([true], double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FluxBitstream([true], double.NegativeInfinity));
        Assert.Equal([true, false, true, false], stream.WithCircularTail(int.MaxValue).Bits.ToArray());
    }
}
