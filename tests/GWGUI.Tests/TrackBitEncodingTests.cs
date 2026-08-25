using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.Tests;

/// <summary>Vérifie les primitives binaires, sectorielles et temporelles utilisées par les encodeurs.</summary>
public sealed class TrackBitEncodingTests
{
    [Fact]
    public void RawWritesMostSignificantBitFirst()
    {
        var bits = TrackBitEncoding.Bits();
        bits.Raw(0x81, 0x42);
        Assert.Equal("1000000101000010".Select(value => value == '1'), bits);
    }

    [Fact]
    public void RawHexAcceptsValidTextAndRejectsInvalidText()
    {
        var bits = TrackBitEncoding.Bits();
        bits.RawHex("81");
        Assert.Equal("10000001".Select(value => value == '1'), bits);
        Assert.Throws<FormatException>(() => bits.RawHex("not-hex"));
    }

    [Theory]
    [InlineData('2')]
    [InlineData('x')]
    public void RawBitsReportsInvalidCharacterAndPosition(char invalid)
    {
        var bits = TrackBitEncoding.Bits();
        bits.RawBits("01");
        var error = Assert.Throws<ArgumentException>(() => bits.RawBits($"01{invalid}"));
        Assert.Contains(invalid.ToString(), error.Message, StringComparison.Ordinal);
        Assert.Contains("2", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MfmPropagatesPreviousDataAcrossBytes()
    {
        var initialZero = TrackBitEncoding.Bits();
        initialZero.Mfm([0x00]);
        var initialOne = TrackBitEncoding.Bits();
        initialOne.Mfm([0x00], previousData: true);
        var consecutive = TrackBitEncoding.Bits();
        consecutive.Mfm([0x01, 0x00]);

        Assert.True(initialZero[0]);
        Assert.False(initialOne[0]);
        Assert.False(consecutive[BitPrimitives.BitsPerByte * 2]);
    }

    [Fact]
    public void FmAndDoubleFmProduceKnownCells()
    {
        var fm = TrackBitEncoding.Bits();
        fm.Fm([0x80]);
        var doubled = TrackBitEncoding.Bits();
        doubled.DoubleFm([0x80]);

        Assert.Equal("1110101010101010".Select(value => value == '1'), fm);
        Assert.Equal("01010100010001000100010001000100".Select(value => value == '1'), doubled);
    }

    [Fact]
    public void CompactFmPreservesClockDataOrderForEmptyAndMultipleBytes()
    {
        Assert.Empty(TrackBitEncoding.EncodeCompactFm());
        Assert.Equal(Convert.FromHexString("AAAA"), TrackBitEncoding.EncodeCompactFm(0x00));
        Assert.Equal(Convert.FromHexString("FFFF"), TrackBitEncoding.EncodeCompactFm(0xFF));
        Assert.Equal(Convert.FromHexString("EAAAAAAA"), TrackBitEncoding.EncodeCompactFm(0x80, 0x00));
    }

    [Fact]
    public void CompactMfmPropagatesDataStateAcrossBytes()
    {
        Assert.Empty(TrackBitEncoding.EncodeCompactMfm());
        Assert.Equal(Convert.FromHexString("AAAA"), TrackBitEncoding.EncodeCompactMfm(0x00));
        Assert.Equal(Convert.FromHexString("5555"), TrackBitEncoding.EncodeCompactMfm(0xFF));
        Assert.Equal(Convert.FromHexString("AAA92AAA"), TrackBitEncoding.EncodeCompactMfm(0x01, 0x00));
    }

    [Fact]
    public void StaticDecoderPatternsRemainIdenticalToCompactEncoding()
    {
        Assert.Equal(TrackBitEncoding.EncodeCompactFm(DataGeneralFmFormat.FirstSyncByte, DataGeneralFmFormat.SecondSyncByte), DataGeneralFmFormat.Sync);
        Assert.Equal(TrackBitEncoding.EncodeCompactFm(0, 0, 0, HeathkitFmFormat.AddressMark), HeathkitFmFormat.SectorMark);
        Assert.Equal(TrackBitEncoding.EncodeCompactFm(0, 0, 0, MicralNFmFormat.AddressMark), MicralNFmFormat.SectorMark);
        Assert.Equal(TrackBitEncoding.EncodeCompactMfm(0, 0, 0, MicropolisMfmFormat.AddressMark), MicropolisMfmFormat.Sync);
        Assert.Equal(TrackBitEncoding.EncodeCompactMfm(0, 0, 0, 0, 0, 0, 0, NorthstarMfmFormat.AddressMark), NorthstarMfmFormat.SectorMark);
    }

    [Fact]
    public void GapSupportsAlternatingOnesEmptyAndRejectsNegativeLength()
    {
        var alternating = TrackBitEncoding.Bits();
        alternating.Gap(4);
        var ones = TrackBitEncoding.Bits();
        ones.Gap(4, true);
        var empty = TrackBitEncoding.Bits();
        empty.Gap(0);

        Assert.Equal([true, false, true, false], alternating);
        Assert.All(ones, Assert.True);
        Assert.Empty(empty);
        Assert.Throws<ArgumentOutOfRangeException>(() => empty.Gap(-1));
    }

    [Fact]
    public void EverySectorSizeCodeRoundTripsAndInvalidValuesAreRejected()
    {
        for (byte code = SectorSizeCode.MinimumCode; code <= SectorSizeCode.MaximumCode; code++) Assert.Equal(code, SectorSizeCode.FromByteCount(SectorSizeCode.ToByteCount(code)));
        Assert.Throws<ArgumentException>(() => SectorSizeCode.FromByteCount(129));
        Assert.Throws<ArgumentOutOfRangeException>(() => SectorSizeCode.ToByteCount(8));
    }

    [Fact]
    public void RotatingChecksumUsesOneBitRotation()
    {
        Assert.Equal((byte)1, GWGUI.MediaEngine.Primitives.RotatingChecksumCalculator.Compute([0x80]));
        Assert.Equal((byte)0, GWGUI.MediaEngine.Primitives.RotatingChecksumCalculator.Compute([0x80, 0x01]));
    }

    [Fact]
    public void CrcAppendWritesHighByteThenLowByte()
    {
        byte[] values = [1, 2, 3];
        var crc = Crc16Calculator.Compute(values);
        Assert.Equal(values.Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]), Crc16Calculator.Append(values));
    }

    [Fact]
    public void FluxRevolutionFactoryKeepsTransitionsAndTerminalInterval()
    {
        Assert.Equal([(uint)6], GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create([false, false], 3, 20).FluxIntervals);
        Assert.Equal([(uint)3, (uint)6], GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create([true, false, true], 3, 20).FluxIntervals);
        Assert.Equal([(uint)3, (uint)3], GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create([true, false], 3, 20).FluxIntervals);
    }

    [Fact]
    public void FluxRevolutionFactoryRejectsInvalidDurationsAndOverflow()
    {
        Assert.Throws<ArgumentNullException>(() => GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create(null!, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create([true], 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create([true], 1, 0));
        var error = Assert.Throws<OverflowException>(() => GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create([false, false], uint.MaxValue, 1));
        Assert.Contains("2", error.Message, StringComparison.Ordinal);
        Assert.Contains(uint.MaxValue.ToString(), error.Message, StringComparison.Ordinal);
    }
}
