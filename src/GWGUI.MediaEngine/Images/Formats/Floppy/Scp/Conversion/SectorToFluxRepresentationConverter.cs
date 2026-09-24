using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Interfaces.Conversion;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Conversion;

/// <summary>Reconstructs an SCP flux document from an encodable sector representation.</summary>
internal sealed class SectorToFluxRepresentationConverter : IMediaRepresentationConverter
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SourceKinds =
        new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sectors };
    private static readonly IReadOnlySet<MediaRepresentationKind> TargetKinds =
        new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Flux };
    private readonly SectorImageScpConversionService conversion;

    public SectorToFluxRepresentationConverter(SectorImageScpConversionService conversion)
    {
        ArgumentNullException.ThrowIfNull(conversion);
        this.conversion = conversion;
    }

    public string Id => "sectors-to-scp-flux";

    public IReadOnlySet<MediaRepresentationKind> SourceRepresentationKinds => SourceKinds;

    public IReadOnlySet<MediaRepresentationKind> TargetRepresentationKinds => TargetKinds;

    public bool CanConvert(
        MediaImageDocument source,
        string targetFormatId,
        MediaRepresentationKind targetRepresentationKind)
        => source.Representation is SectorMediaImageRepresentation sectors &&
           (targetFormatId.Equals(DiskImageFormatIds.RawScp, StringComparison.OrdinalIgnoreCase) ||
            targetFormatId.Equals(DiskImageFormatIds.RawHfe, StringComparison.OrdinalIgnoreCase)) &&
           targetRepresentationKind == MediaRepresentationKind.Flux &&
           conversion.CanCreate(sectors.Image);

    public Task<MediaRepresentationConversionResult> ConvertAsync(
        MediaImageDocument source,
        string targetFormatId,
        MediaRepresentationKind targetRepresentationKind,
        IReadOnlyDictionary<string, string> options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options);
        if (!CanConvert(source, targetFormatId, targetRepresentationKind) ||
            source.Representation is not SectorMediaImageRepresentation sectors)
            throw new NotSupportedException(
                $"Sector format '{source.FormatId}' cannot be reconstructed as flux for '{targetFormatId}'.");

        cancellationToken.ThrowIfCancellationRequested();
        var image = conversion.Create(sectors.Image, cancellationToken);
        var document = MediaImageDocumentFactory.CreateFloppyFlux(
            source.Source,
            DiskImageFormatIds.RawScp,
            ScpProtectedTrackImageAdapter.Create(image),
            ScpMetadataFunctions.Create(image));
        return Task.FromResult(new MediaRepresentationConversionResult(
            document,
            [],
            ["The generated flux is reconstructed from sectors and does not contain the original physical capture."]));
    }
}
