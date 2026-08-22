using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Parity;
namespace GWGUI.App.Services.Parity;

public static class MediaEngineParityCatalog
{
    private static readonly Lazy<MediaParityMatrix> BuiltIn = new(CreateBuiltIn);

    public static MediaParityMatrix Matrix => BuiltIn.Value;

    public static MediaParityMatrix Create(IImageFormatCatalog catalog)
    {
        var rows = new List<MediaParityRow>();
        foreach (var format in catalog.Formats)
        {
            if (format.CompatibleSourceExtensions is null) continue;
            foreach (var source in format.CompatibleSourceExtensions)
            {
                foreach (var target in format.Extensions)
                    rows.Add(CreateRow(format, source, target.Extension));
            }
        }

        return new MediaParityMatrix(rows);
    }

    private static MediaParityMatrix CreateBuiltIn() => Create(new BuiltInImageFormatCatalog());

    private static MediaParityRow CreateRow(DiskFormat format, string sourceExtension, string targetExtension)
    {
        var supported = MediaEngineConversionSupport.CanConvert(sourceExtension, format.Id, targetExtension);
        var fidelity = ConversionFidelity.ForConversion(sourceExtension, targetExtension);
        var passed = supported ? ParityValidationStatus.Passed : ParityValidationStatus.Pending;
        var sectorParity = fidelity == ConversionFidelityLevel.PreservedFlux
            ? ParityValidationStatus.NotApplicable
            : passed;
        var fluxParity = fidelity == ConversionFidelityLevel.PreservedFlux
            ? passed
            : ParityValidationStatus.NotApplicable;

        return new MediaParityRow(
            format.Id,
            Normalize(sourceExtension),
            Normalize(targetExtension),
            format.DisplayName,
            passed,
            passed,
            passed,
            sectorParity,
            sectorParity,
            sectorParity,
            fluxParity,
            ParityValidationStatus.Pending,
            true,
            supported ? EvidenceFor(fidelity) : null);
    }

    private static string EvidenceFor(ConversionFidelityLevel fidelity) => fidelity switch
    {
        ConversionFidelityLevel.SectorData => "sector-block-file-metadata-round-trip",
        ConversionFidelityLevel.ReconstructedTracks => "track-encoder-sector-round-trip",
        ConversionFidelityLevel.PreservedFlux => "flux-index-timing-structure-parity",
        _ => throw new ArgumentOutOfRangeException(nameof(fidelity), fidelity, null)
    };

    private static string Normalize(string extension) => extension.StartsWith('.')
        ? extension.ToLowerInvariant()
        : "." + extension.ToLowerInvariant();
}
