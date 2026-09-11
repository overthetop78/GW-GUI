using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Conversion;
using GWGUI.MediaEngine.Conversion.Atari;
using GWGUI.MediaEngine.Conversion.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Formats.Floppy.Adf;
using GWGUI.MediaEngine.Formats.Floppy.Atr;
using GWGUI.MediaEngine.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Formats.Floppy.St;
using GWGUI.MediaEngine.Interfaces.Conversion;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Provides the registered in-memory media representation converters.</summary>
public sealed class MediaConversionComposition
{
    internal MediaConversionComposition(
        MediaRecognitionComposition recognition,
        ScpSectorDecodingComposition scpDecoding)
    {
        ArgumentNullException.ThrowIfNull(recognition);
        ArgumentNullException.ThrowIfNull(scpDecoding);
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

    public AmigaAdfConversionService AmigaAdfRuntimeConverter { get; }

    public AtariScpRuntimeConversionService AtariScpRuntimeConverter { get; }
}
