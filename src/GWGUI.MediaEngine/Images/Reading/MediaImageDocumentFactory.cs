using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Reading;

/// <summary>Creates media documents from format reader results without performing format recognition or decoding.</summary>
internal static class MediaImageDocumentFactory
{
    private static readonly IReadOnlyList<MediaVolumeDescriptor> EmptyVolumes = [];
    private static readonly IReadOnlyList<string> EmptyDiagnostics = [];
    private static readonly IReadOnlyDictionary<string, string> EmptyMetadata =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public static MediaImageDocument CreateFloppySector(
        MediaSourceDescriptor source,
        SectorImage image,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(image);

        return Create(
            source,
            image.FormatId,
            MediaKind.Floppy,
            new SectorMediaImageRepresentation(image),
            metadata);
    }

    public static MediaImageDocument CreateFloppyFlux(
        MediaSourceDescriptor source,
        string formatId,
        ProtectedTrackImage image,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(image);

        return Create(
            source,
            formatId,
            MediaKind.Floppy,
            new FluxMediaImageRepresentation(image),
            metadata);
    }

    private static MediaImageDocument Create(
        MediaSourceDescriptor source,
        string formatId,
        MediaKind mediaKind,
        IMediaImageRepresentation representation,
        IReadOnlyDictionary<string, string>? metadata = null)
        => new(
            source,
            formatId,
            mediaKind,
            representation,
            EmptyVolumes,
            EmptyDiagnostics,
            metadata ?? EmptyMetadata);
}
