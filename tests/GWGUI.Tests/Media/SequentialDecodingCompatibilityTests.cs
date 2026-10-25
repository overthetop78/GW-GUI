using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Acorn;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Amstrad;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Atari;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Commodore;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Msx;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Spectrum;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Decoding;

namespace GWGUI.Tests.Media;

public sealed class SequentialDecodingCompatibilityTests
{
    private static ISequentialMediaDecoder[] Decoders() =>
        [new AtariCassetteDecoder(), new AcornTapeDecoder(), new SpectrumTapeDecoder(),
         new CommodoreTapeDecoder(), new AmstradCpcTapeDecoder(), new MsxTapeDecoder()];

    [Fact]
    public void AdvertisedMachinesAndFormatsSelectTheirDecoderIgnoringCase()
    {
        foreach (var decoder in Decoders())
        foreach (var format in decoder.FormatIds)
        foreach (var machine in decoder.MachineIds)
        {
            var document = Document(format.ToUpperInvariant());
            Assert.True(decoder.CanDecode(document, machine.ToUpperInvariant()));
            Assert.False(decoder.CanDecode(document, "unknown-machine"));
            Assert.False(decoder.CanDecode(Document(format, MediaKind.Floppy), machine));
            Assert.False(decoder.CanDecode(Document(format, representation: new BlockMediaImageRepresentation(0, [])), machine));
            Assert.False(decoder.CanDecode(Document("unknown-format"), machine));
            if (decoder is not MsxTapeDecoder || !format.Equals(TapeImageFormatIds.Tzx, StringComparison.OrdinalIgnoreCase))
                Assert.True(decoder.CanDecode(document));
        }
    }

    [Fact]
    public void WaveNeedsAMachineToResolveTheSharedFormat()
    {
        var registry = new SequentialDecoderRegistry(Decoders());
        var document = Document(TapeImageFormatIds.Wav);
        Assert.True(registry.FindCompatible(document).Count > 1);
        Assert.Throws<InvalidOperationException>(() => registry.GetRequired(document));
        foreach (var decoder in Decoders())
        foreach (var machine in decoder.MachineIds)
            Assert.Same(decoder.GetType(), registry.GetRequired(document, machine).GetType());
    }

    [Fact]
    public void MsxTzxRequiresAnExplicitMachineOrTsxContext()
    {
        var decoder = new MsxTapeDecoder();
        Assert.False(decoder.CanDecode(Document(TapeImageFormatIds.Tzx)));
        Assert.True(decoder.CanDecode(Document(TapeImageFormatIds.Tzx), decoder.MachineIds.Single()));
        Assert.True(decoder.CanDecode(Document(TapeImageFormatIds.Tzx,
            metadata: new Dictionary<string, string> { ["contextExtension"] = ".TSX" })));
        Assert.False(decoder.CanDecode(Document(TapeImageFormatIds.Tzx,
            metadata: new Dictionary<string, string> { ["contextExtension"] = ".tzx" })));
    }

    [Fact]
    public void ExistingNullGuardsRemainSpecificToEachDecoder()
    {
        foreach (var decoder in Decoders())
        {
            if (decoder is AtariCassetteDecoder or AcornTapeDecoder)
                Assert.Throws<ArgumentNullException>(() => decoder.CanDecode(null!));
            else
                Assert.Throws<NullReferenceException>(() => decoder.CanDecode(null!));
        }
    }

    private static MediaImageDocument Document(string format, MediaKind kind = MediaKind.Tape,
        IMediaImageRepresentation? representation = null, IReadOnlyDictionary<string, string>? metadata = null)
        => new(new MediaSourceDescriptor("memory", []), format, kind,
            representation ?? new SequentialMediaImageRepresentation(0), [], [], metadata ?? new Dictionary<string, string>());
}
