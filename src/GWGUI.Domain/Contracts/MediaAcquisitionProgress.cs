using System.Collections.ObjectModel;

namespace GWGUI.Domain.Contracts;

/// <summary>Reports neutral progress while physical media data units are acquired.</summary>
public sealed record MediaAcquisitionProgress
{
    public MediaAcquisitionProgress(
        int completedUnits,
        int totalUnits,
        int attempt,
        MediaPhysicalDataUnit? acquiredUnit = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(completedUnits);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalUnits);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(attempt);
        if (completedUnits > totalUnits) throw new ArgumentOutOfRangeException(nameof(completedUnits));
        CompletedUnits = completedUnits;
        TotalUnits = totalUnits;
        Attempt = attempt;
        AcquiredUnit = acquiredUnit;
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal));
    }

    public int CompletedUnits { get; }

    public int TotalUnits { get; }

    public int Attempt { get; }

    public MediaPhysicalDataUnit? AcquiredUnit { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
