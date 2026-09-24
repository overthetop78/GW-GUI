using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains neutral data acquired from a physical medium and the diagnostics reported by its reader.</summary>
public sealed record MediaAcquisitionResult
{
    public MediaAcquisitionResult(
        MediaKind mediaKind,
        MediaRepresentationKind representationKind,
        IReadOnlyList<MediaPhysicalDataUnit> dataUnits,
        IReadOnlyDictionary<string, string>? metadata = null,
        IReadOnlyList<string>? hardwareDiagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(dataUnits);
        if (dataUnits.Any(unit => unit is null))
            throw new ArgumentException("An acquired data unit cannot be null.", nameof(dataUnits));
        MediaKind = mediaKind;
        RepresentationKind = representationKind;
        DataUnits = new ReadOnlyCollection<MediaPhysicalDataUnit>(dataUnits.OrderBy(unit => unit.Position).ToArray());
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal));
        HardwareDiagnostics = new ReadOnlyCollection<string>((hardwareDiagnostics ?? []).ToArray());
    }

    public MediaKind MediaKind { get; }

    public MediaRepresentationKind RepresentationKind { get; }

    public IReadOnlyList<MediaPhysicalDataUnit> DataUnits { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    public IReadOnlyList<string> HardwareDiagnostics { get; }
}
