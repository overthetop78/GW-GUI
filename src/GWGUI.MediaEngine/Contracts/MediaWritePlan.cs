using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes neutral data units and constraints to be consumed by a physical media writer.</summary>
public sealed record MediaWritePlan
{
    public MediaWritePlan(
        MediaKind mediaKind,
        MediaRepresentationKind representationKind,
        IReadOnlyList<MediaPhysicalDataUnit> dataUnits,
        IReadOnlyList<int> writeOrder,
        IReadOnlyDictionary<string, string> constraints)
    {
        ArgumentNullException.ThrowIfNull(dataUnits);
        ArgumentNullException.ThrowIfNull(writeOrder);
        ArgumentNullException.ThrowIfNull(constraints);
        if (dataUnits.Any(unit => unit is null))
            throw new ArgumentException("A physical write data unit cannot be null.", nameof(dataUnits));
        if (writeOrder.Any(index => index < 0 || index >= dataUnits.Count) || writeOrder.Distinct().Count() != writeOrder.Count)
            throw new ArgumentException("The physical write order must contain unique valid data-unit indexes.", nameof(writeOrder));
        MediaKind = mediaKind;
        RepresentationKind = representationKind;
        DataUnits = new ReadOnlyCollection<MediaPhysicalDataUnit>(dataUnits.ToArray());
        WriteOrder = new ReadOnlyCollection<int>(writeOrder.ToArray());
        Constraints = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(constraints, StringComparer.Ordinal));
    }

    public MediaKind MediaKind { get; }

    public MediaRepresentationKind RepresentationKind { get; }

    public IReadOnlyList<MediaPhysicalDataUnit> DataUnits { get; }

    public IReadOnlyList<int> WriteOrder { get; }

    public IReadOnlyDictionary<string, string> Constraints { get; }
}
