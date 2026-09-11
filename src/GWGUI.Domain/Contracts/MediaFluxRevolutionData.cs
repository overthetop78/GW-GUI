using System.Collections.ObjectModel;

namespace GWGUI.Domain.Contracts;

/// <summary>Contains one neutral flux revolution expressed in nanoseconds.</summary>
public sealed record MediaFluxRevolutionData
{
    public MediaFluxRevolutionData(uint indexTimeNanoseconds, IReadOnlyList<uint> fluxIntervalsNanoseconds)
    {
        ArgumentNullException.ThrowIfNull(fluxIntervalsNanoseconds);
        if (indexTimeNanoseconds == 0) throw new ArgumentOutOfRangeException(nameof(indexTimeNanoseconds));
        if (fluxIntervalsNanoseconds.Any(interval => interval == 0))
            throw new ArgumentException("Flux intervals must be positive.", nameof(fluxIntervalsNanoseconds));
        IndexTimeNanoseconds = indexTimeNanoseconds;
        FluxIntervalsNanoseconds = new ReadOnlyCollection<uint>(fluxIntervalsNanoseconds.ToArray());
    }

    public uint IndexTimeNanoseconds { get; }

    public IReadOnlyList<uint> FluxIntervalsNanoseconds { get; }
}
