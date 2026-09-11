using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Decoding.Scp.Sectors;
using GWGUI.MediaEngine.Interfaces.Conversion;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.MediaEngine.Conversion.Scp;

/// <summary>Reconstructs a sector document from an already recognized SCP flux document.</summary>
internal sealed class ScpFluxToSectorRepresentationConverter : IMediaRepresentationConverter
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SourceKinds =
        new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Flux };
    private static readonly IReadOnlySet<MediaRepresentationKind> TargetKinds =
        new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sectors };
    private readonly ScpSectorImageReader reader;
    private readonly IReadOnlySet<string> targetFormatIds;

    public ScpFluxToSectorRepresentationConverter(
        ScpSectorImageReader reader,
        IReadOnlySet<string> targetFormatIds)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(targetFormatIds);
        this.reader = reader;
        this.targetFormatIds = targetFormatIds;
    }

    public string Id => "scp-flux-to-sectors";

    public IReadOnlySet<MediaRepresentationKind> SourceRepresentationKinds => SourceKinds;

    public IReadOnlySet<MediaRepresentationKind> TargetRepresentationKinds => TargetKinds;

    public bool CanConvert(
        MediaImageDocument source,
        string targetFormatId,
        MediaRepresentationKind targetRepresentationKind)
        => source.FormatId.Equals(DiskImageFormatIds.RawScp, StringComparison.OrdinalIgnoreCase) &&
           source.Representation is FluxMediaImageRepresentation &&
           targetRepresentationKind == MediaRepresentationKind.Sectors &&
           targetFormatIds.Contains(targetFormatId);

    public async Task<MediaRepresentationConversionResult> ConvertAsync(
        MediaImageDocument source,
        string targetFormatId,
        MediaRepresentationKind targetRepresentationKind,
        IReadOnlyDictionary<string, string> options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options);
        if (!CanConvert(source, targetFormatId, targetRepresentationKind))
            throw new NotSupportedException(
                $"SCP flux cannot be reconstructed as sector format '{targetFormatId}'.");

        var image = await reader.ReadAsync(
            source.Source.PrimaryPath,
            targetFormatId,
            cancellationToken).ConfigureAwait(false);
        var document = MediaImageDocumentFactory.CreateFloppySector(source.Source, image);
        return new MediaRepresentationConversionResult(
            document,
            [],
            ["Physical flux timing and protection information is not preserved in the sector representation."]);
    }
}
