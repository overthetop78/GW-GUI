using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Commodore;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Msx;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Spectrum;
using GWGUI.MediaEngine.Interfaces.Encoding;

namespace GWGUI.Tests.Media;

public sealed class SequentialEncodingCompatibilityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RejectsInvalidBlocksAndMismatchedFormatsOrMachines(bool commodore)
    {
        var (encoder, format, machine) = Profile(commodore);
        var valid = Block(1, true);

        Assert.True(encoder.CanEncode(new(format, machine, [valid])));
        Assert.True(encoder.CanEncode(new(format.ToUpperInvariant(), machine.ToUpperInvariant(), [valid])));
        Assert.False(encoder.CanEncode(new("other.format", machine, [valid])));
        Assert.False(encoder.CanEncode(new(format, "other-machine", [valid])));
        Assert.False(encoder.CanEncode(new(format, machine, [])));
        Assert.False(encoder.CanEncode(new(format, machine, [Block(0, true)])));
        Assert.False(encoder.CanEncode(new(format, machine, [Block(1, false)])));
        Assert.False(encoder.CanEncode(new(format, machine, [Block(1, null)])));
        Assert.False(encoder.CanEncode(new(format, machine, [valid, Block(1, false)])));
        Assert.False(encoder.CanEncode(new(format, machine, [valid], [new(0, SequentialSegmentKind.Unknown)])));
        Assert.True(encoder.CanEncode(new(format, machine, [valid], parameters: new Dictionary<string, string>
        {
            ["sampleRate"] = "invalid"
        })));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RequiresAnExplicitSupportedSampleRateOnlyForWaveOutput(bool commodore)
    {
        var (encoder, _, machine) = Profile(commodore);
        var valid = Block(1, true);
        Assert.False(encoder.CanEncode(new(TapeImageFormatIds.Wav, machine, [valid])));
        foreach (var sampleRate in new[] { "", "7999", "-8000", "+8000", "8.000", "8000 ", "2147483648" })
            Assert.False(encoder.CanEncode(new(TapeImageFormatIds.Wav, machine, [valid],
                parameters: new Dictionary<string, string> { ["sampleRate"] = sampleRate })));
        foreach (var sampleRate in new[] { "8000", "44100", "2147483647" })
            Assert.True(encoder.CanEncode(new(TapeImageFormatIds.Wav.ToUpperInvariant(), machine, [valid],
                parameters: new Dictionary<string, string> { ["sampleRate"] = sampleRate })));
        Assert.False(encoder.CanEncode(new(TapeImageFormatIds.Wav, machine, [valid],
            parameters: new Dictionary<string, string> { ["SampleRate"] = "8000" })));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(65535, true)]
    [InlineData(65536, false)]
    public void KeepsTheDistinctSpectrumBlockSizeConstraint(int length, bool spectrumAccepts)
    {
        var block = Block(length, true);
        Assert.True(new MsxTapeEncoder().CanEncode(new(TapeImageFormatIds.MsxCas, DiskSystemIds.Msx, [block])));
        Assert.True(new CommodoreTapeEncoder().CanEncode(new(TapeImageFormatIds.CommodoreTap, DiskSystemIds.Commodore, [block])));
        Assert.Equal(spectrumAccepts, new SpectrumTapeEncoder().CanEncode(
            new(TapeImageFormatIds.SpectrumTap, DiskSystemIds.Spectrum, [block])));
    }

    private static SequentialDecodedBlock Block(int length, bool? valid) => new(0, new byte[length], [], valid);

    private static (ISequentialMediaEncoder Encoder, string Format, string Machine) Profile(bool commodore) => commodore
        ? (new CommodoreTapeEncoder(), TapeImageFormatIds.CommodoreTap, DiskSystemIds.Commodore)
        : (new MsxTapeEncoder(), TapeImageFormatIds.MsxCas, DiskSystemIds.Msx);
}
