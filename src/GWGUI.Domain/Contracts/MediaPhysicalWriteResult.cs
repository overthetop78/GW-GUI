using System.Collections.ObjectModel;

namespace GWGUI.Domain.Contracts;

/// <summary>Contains the neutral outcome of writing data units to a physical medium.</summary>
public sealed record MediaPhysicalWriteResult
{
    public MediaPhysicalWriteResult(
        int completedUnits,
        int totalUnits,
        bool cancelled,
        IReadOnlyList<string>? diagnostics = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(completedUnits);
        ArgumentOutOfRangeException.ThrowIfNegative(totalUnits);
        if (completedUnits > totalUnits) throw new ArgumentOutOfRangeException(nameof(completedUnits));
        CompletedUnits = completedUnits;
        TotalUnits = totalUnits;
        Cancelled = cancelled;
        Diagnostics = new ReadOnlyCollection<string>((diagnostics ?? []).ToArray());
    }

    public int CompletedUnits { get; }

    public int TotalUnits { get; }

    public bool Cancelled { get; }

    public IReadOnlyList<string> Diagnostics { get; }

    public bool IsSuccess => !Cancelled && CompletedUnits == TotalUnits && Diagnostics.Count == 0;
}
