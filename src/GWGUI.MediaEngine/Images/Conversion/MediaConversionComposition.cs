using GWGUI.MediaEngine.Images.Formats.Tape;
using GWGUI.MediaEngine.Images.Reading;

using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Images.Conversion.Atari;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Conversion;
using GWGUI.MediaEngine.Images.Conversion.Optical;
using GWGUI.MediaEngine.Images.Conversion.Sequential;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential;
using GWGUI.MediaEngine.Images.Writing.Encoding;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential;
using GWGUI.MediaEngine.Images.Formats.Floppy.Adf;
using GWGUI.MediaEngine.Images.Formats.Floppy.Atr;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Formats.Floppy.St;
using GWGUI.MediaEngine.Interfaces.Conversion;

namespace GWGUI.MediaEngine.Images.Conversion;

/// <summary>Provides the registered in-memory media representation converters.</summary>
public sealed class MediaConversionComposition
{
    private readonly MediaRecognitionComposition recognition;

    internal MediaConversionComposition(
        MediaRecognitionComposition recognition,
        ScpSectorDecodingComposition scpDecoding,
        SequentialMediaComposition sequentialMedia)
    {
        ArgumentNullException.ThrowIfNull(recognition);
        ArgumentNullException.ThrowIfNull(scpDecoding);
        ArgumentNullException.ThrowIfNull(sequentialMedia);
        this.recognition = recognition;
        var sectorFormatIds = new HashSet<string>(
            recognition.Readers
                .Where(reader => reader.RepresentationKinds.Contains(MediaRepresentationKind.Sectors))
                .SelectMany(reader => reader.FormatIds),
            StringComparer.OrdinalIgnoreCase);
        var sectorToFlux = new SectorToFluxRepresentationConverter(
            new SectorImageScpConversionService(
                new SectorImageTrackEncoder(),
                new ScpEncodedTrackFluxService(),
                new ScpWriter()));
        Converters =
        [
            new ScpFluxToSectorRepresentationConverter(scpDecoding.Reader, sectorFormatIds),
            sectorToFlux
        ];
        Registry = new MediaRepresentationConverterRegistry(Converters);
        SequentialDecoders = sequentialMedia.Decoders;
        SequentialEncoders = sequentialMedia.Encoders;
        AmigaAdfRuntimeConverter = new AmigaAdfConversionService(
            scpDecoding.AmigaReader,
            new AdfReader(),
            new AmigaAdfWriter());
        AtariScpRuntimeConverter = new AtariScpRuntimeConversionService(
            scpDecoding.AtariReader,
            new AtrWriter(),
            new AtariStWriter(new LinearSectorImageWriter()));
    }

    public IReadOnlyList<IMediaRepresentationConverter> Converters { get; }

    public MediaRepresentationConverterRegistry Registry { get; }

    public SequentialDecoderRegistry SequentialDecoders { get; }

    public SequentialEncoderRegistry SequentialEncoders { get; }

    public AmigaAdfConversionService AmigaAdfRuntimeConverter { get; }

    public AtariScpRuntimeConversionService AtariScpRuntimeConverter { get; }

    public OpticalImageConversionService CreateOpticalImageConverter(MediaWritingComposition writing)
    {
        ArgumentNullException.ThrowIfNull(writing);
        return new OpticalImageConversionService(
            recognition.ReadingService,
            writing.Registry,
            writing.WritingService);
    }

    public SequentialMediaConversionService CreateSequentialMediaConverter(MediaWritingComposition writing)
    {
        ArgumentNullException.ThrowIfNull(writing);
        return new SequentialMediaConversionService(
            recognition.ReadingService,
            SequentialDecoders,
            SequentialEncoders,
            writing.Registry,
            writing.WritingService);
    }
}
