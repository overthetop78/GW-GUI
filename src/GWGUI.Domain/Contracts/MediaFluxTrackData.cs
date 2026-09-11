using System.Collections.ObjectModel;

namespace GWGUI.Domain.Contracts;

/// <summary>Contains the ordered neutral flux revolutions of one physical track.</summary>
public sealed record MediaFluxTrackData
{
    public MediaFluxTrackData(IReadOnlyList<MediaFluxRevolutionData> revolutions)
    {
        ArgumentNullException.ThrowIfNull(revolutions);
        if (revolutions.Count == 0 || revolutions.Any(revolution => revolution is null))
            throw new ArgumentException("A flux track requires at least one revolution.", nameof(revolutions));
        Revolutions = new ReadOnlyCollection<MediaFluxRevolutionData>(revolutions.ToArray());
    }

    public IReadOnlyList<MediaFluxRevolutionData> Revolutions { get; }
}
