using GWGUI.MediaEngine.Representations.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.Tests;

public sealed class FluxPrimitivesTests
{
    [Theory]
    [InlineData(0x00, 0x00)]
    [InlineData(0xFF, 0xFF)]
    [InlineData(0x01, 0x80)]
    [InlineData(0x80, 0x01)]
    [InlineData(0x96, 0x69)]
    public void BitPrimitivesReversesKnownBytes(byte value, byte expected)
    {
        Assert.Equal(expected, BitPrimitives.ReverseBits(value));
    }

    [Fact]
    public void BitPrimitivesDoubleReversalRestoresEveryByte()
    {
        for (var value = (int)byte.MinValue; value <= byte.MaxValue; value++)
        {
            var source = (byte)value;
            Assert.Equal(source, BitPrimitives.ReverseBits(BitPrimitives.ReverseBits(source)));
        }
    }

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

    [Fact]
    public void FluxTimingEstimatorHandlesEmptyZeroIndexAndFallbackSamples()
    {
        Assert.Equal(FluxDecodingParameters.FallbackBitCellTicks, FluxTimingEstimator.EstimateNonFmBitCell([]));
        Assert.Equal(FluxDecodingParameters.FallbackBitCellTicks, FluxTimingEstimator.EstimateNonFmBitCell([0, 0, 0]));
        Assert.Equal(10d, FluxTimingEstimator.EstimateNonFmBitCell([1_000, 20, 20, 40]));
        Assert.Equal(10d, FluxTimingEstimator.EstimateNonFmBitCell([20, 0, 0]));
    }

    [Fact]
    public void FluxTimingEstimatorDistinguishesFmNonFmAndNrziModes()
    {
        Assert.Equal(10d, FluxTimingEstimator.EstimateFmBitCell([10, 20, 30]));
        Assert.Equal(10d, FluxTimingEstimator.EstimateNonFmBitCell([999, 20, 20, 40, 40]));
        Assert.Equal(10d, FluxTimingEstimator.EstimateNrziBitCell([999, 10, 20, 30]));
    }

    [Fact]
    public void FluxTimingEstimatorRejectsIsolatedNoiseAndSelectsLowerCluster()
    {
        uint[] fmIntervals = [1, 2, .. Enumerable.Repeat(10u, 98)];
        Assert.Equal(10d, FluxTimingEstimator.EstimateFmBitCell(fmIntervals));

        uint[] clusteredIntervals = [999, 10, 10, 20, 20, 30, 30, 40, 40, 50, 50];
        Assert.Equal(5d, FluxTimingEstimator.EstimateNonFmBitCell(clusteredIntervals));
    }

    [Fact]
    public void FluxTransitionDecoderReconstructsExpectedBitsAndCellDurations()
    {
        var adaptive = FluxTransitionDecoder.Reconstruct([10, 20], 10d, FluxDecodingParameters.MaximumFmMfmCellsPerInterval);
        var fixedClock = FluxTransitionDecoder.Reconstruct([10, 20], 10d, FluxDecodingParameters.MaximumFmMfmCellsPerInterval, adaptClock: false);
        var pll = FluxTransitionDecoder.ReconstructPll([10, 20], 10d, FluxDecodingParameters.MaximumFmMfmCellsPerInterval);
        var nrzi = FluxTransitionDecoder.DecodeNrzi([10, 20]);
        var doubledNrzi = FluxTransitionDecoder.DecodeAdaptiveDoubledNrzi([10, 20]);

        Assert.Equal([true, false, true], adaptive.Bits.ToArray());
        Assert.Equal(10d, adaptive.BitCellTicks);
        Assert.Equal([true, false, true], fixedClock.Bits.ToArray());
        Assert.Equal(10d, fixedClock.BitCellTicks);
        Assert.Equal([true, false, true], pll.Bits.ToArray());
        Assert.Equal(10d, pll.BitCellTicks);
        Assert.Equal([true, true], nrzi.Bits.ToArray());
        Assert.Equal(20d, nrzi.BitCellTicks);
        Assert.Equal([true, false, true], doubledNrzi.Bits.ToArray());
        Assert.Equal(10d, doubledNrzi.BitCellTicks);
    }

    [Fact]
    public void FluxTransitionDecoderClampsIntervalsAndExplicitCellDuration()
    {
        var zeroInterval = FluxTransitionDecoder.Reconstruct([0], 10d, FluxDecodingParameters.MaximumFmMfmCellsPerInterval);
        var fmMfmLimit = FluxTransitionDecoder.Reconstruct([320], 10d, FluxDecodingParameters.MaximumFmMfmCellsPerInterval);
        var nrziLimit = FluxTransitionDecoder.Reconstruct([640], 10d, FluxDecodingParameters.MaximumNrziCellsPerInterval);
        var minimumExplicitCell = FluxTransitionDecoder.DecodeNrzi([10], 0d);

        Assert.Equal([true], zeroInterval.Bits.ToArray());
        Assert.Equal(FluxDecodingParameters.MaximumFmMfmCellsPerInterval, fmMfmLimit.Bits.Length);
        Assert.True(fmMfmLimit.Bits[^1]);
        Assert.Equal(FluxDecodingParameters.MaximumNrziCellsPerInterval, nrziLimit.Bits.Length);
        Assert.True(nrziLimit.Bits[^1]);
        Assert.Equal(FluxDecodingParameters.MinimumBitCellTicks, minimumExplicitCell.BitCellTicks);
    }

    [Fact]
    public void FluxTransitionDecoderAppliesPllCorrectionBranchesAndFrequencyLimits()
    {
        var directCorrection = FluxTransitionDecoder.ReconstructPll([33], 10d, FluxDecodingParameters.MaximumFmMfmCellsPerInterval);
        var progressiveCorrection = FluxTransitionDecoder.ReconstructPll([53], 10d, FluxDecodingParameters.MaximumFmMfmCellsPerInterval);
        var upperCorrections = FluxTransitionDecoder.ReconstructPll(Enumerable.Repeat(21u, 200).ToArray(), 10d, FluxDecodingParameters.MaximumFmMfmCellsPerInterval);
        var lowerCorrections = FluxTransitionDecoder.ReconstructPll(Enumerable.Repeat(19u, 200).ToArray(), 10d, FluxDecodingParameters.MaximumFmMfmCellsPerInterval);

        Assert.Equal(10.15d, directCorrection.BitCellTicks, 10);
        Assert.Equal(10d, progressiveCorrection.BitCellTicks, 10);
        Assert.InRange(upperCorrections.BitCellTicks, 9d, 11d);
        Assert.InRange(lowerCorrections.BitCellTicks, 9d, 11d);
    }

    [Fact]
    public void FluxBitReaderValidatesPatternOffsetsAndLengths()
    {
        var stream = new FluxBitstream(Enumerable.Repeat(true, 32).ToArray(), 1d);

        Assert.False(FluxBitReader.Match(stream, -1, ushort.MaxValue));
        Assert.True(FluxBitReader.Match(stream, 16, ushort.MaxValue));
        Assert.False(FluxBitReader.Match(stream, 17, ushort.MaxValue));
        Assert.False(FluxBitReader.Match(stream, 0, uint.MaxValue, 0));
        Assert.True(FluxBitReader.Match(stream, 16, ushort.MaxValue, 16));
        Assert.True(FluxBitReader.Match(stream, 0, uint.MaxValue, 32));
        Assert.False(FluxBitReader.Match(stream, 0, uint.MaxValue, 33));
    }

    [Fact]
    public void FluxBitReaderValidatesBytePatternsAndLengthMultiplication()
    {
        var stream = new FluxBitstream([true, false, true, false, false, true, false, true], 1d);

        Assert.True(FluxBitReader.MatchBytes(stream, stream.Bits.Length, []));
        Assert.True(FluxBitReader.MatchBytes(stream, 0, [0xA5]));
        Assert.False(FluxBitReader.MatchBytes(stream, 0, new OversizedBytePattern()));
    }

    [Fact]
    public void FluxBitReaderDecodesCompleteLayoutsAndRejectsTruncatedBytes()
    {
        bool[] rawBits = [true, false, true, false, false, true, false, true];
        var mfmBits = new bool[16];
        var fmBits = new bool[32];
        for (var bit = 0; bit < rawBits.Length; bit++)
        {
            mfmBits[bit * 2 + 1] = rawBits[bit];
            fmBits[bit * 4 + 3] = rawBits[bit];
        }

        Assert.True(FluxBitReader.TryDecodeByte(new FluxBitstream(rawBits, 1d), 0, out var rawValue));
        Assert.Equal(0xA5, rawValue);
        Assert.True(FluxBitReader.TryDecodeMfmByte(new FluxBitstream(mfmBits, 1d), 0, out var mfmValue));
        Assert.Equal(0xA5, mfmValue);
        Assert.True(FluxBitReader.TryDecodeFmByte32(new FluxBitstream(fmBits, 1d), 0, out var fmValue));
        Assert.Equal(0xA5, fmValue);

        Assert.False(FluxBitReader.TryDecodeByte(new FluxBitstream(rawBits[..^1], 1d), 0, out var truncatedRaw));
        Assert.Equal(0, truncatedRaw);
        Assert.False(FluxBitReader.TryDecodeMfmByte(new FluxBitstream(mfmBits[..^1], 1d), 0, out var truncatedMfm));
        Assert.Equal(0, truncatedMfm);
        Assert.False(FluxBitReader.TryDecodeFmByte32(new FluxBitstream(fmBits[..^1], 1d), 0, out var truncatedFm));
        Assert.Equal(0, truncatedFm);
    }

    private sealed class OversizedBytePattern : IReadOnlyList<byte>
    {
        public int Count => int.MaxValue;
        public byte this[int index] => throw new InvalidOperationException();
        public IEnumerator<byte> GetEnumerator() => throw new InvalidOperationException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
