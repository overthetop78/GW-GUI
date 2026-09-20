using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Acorn;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Atari;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Commodore;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Msx;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Spectrum;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Acorn;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Atari;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Commodore;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Msx;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Spectrum;

namespace GWGUI.MediaEngine.Images.Formats.Tape;

/// <summary>Assembles the sequential media codecs shared by conversion and exploration.</summary>
public sealed class SequentialMediaComposition
{
    private SequentialMediaComposition(
        SequentialDecoderRegistry decoders,
        SequentialEncoderRegistry encoders)
    {
        Decoders = decoders;
        Encoders = encoders;
    }

    public SequentialDecoderRegistry Decoders { get; }

    public SequentialEncoderRegistry Encoders { get; }

    public static SequentialMediaComposition CreateDefault() => new(
        new SequentialDecoderRegistry(
        [
            new AtariCassetteDecoder(),
            new SpectrumTapeDecoder(),
            new CommodoreTapeDecoder(),
            new MsxTapeDecoder(),
            new AcornTapeDecoder()
        ]),
        new SequentialEncoderRegistry(
        [
            new AtariCassetteEncoder(),
            new SpectrumTapeEncoder(),
            new CommodoreTapeEncoder(),
            new MsxTapeEncoder(),
            new AcornTapeEncoder()
        ]));
}
