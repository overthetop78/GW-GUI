namespace GWGUI.MediaEngine.Contracts;

/// <summary>Reports neutral progress while physical media data units are written or verified.</summary>
public sealed record MediaPhysicalWriteProgress
{
    public MediaPhysicalWriteProgress(
        int completedUnits,
        int totalUnits,
        long position,
        bool verifying)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(completedUnits);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalUnits);
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        if (completedUnits > totalUnits) throw new ArgumentOutOfRangeException(nameof(completedUnits));
        CompletedUnits = completedUnits;
        TotalUnits = totalUnits;
        Position = position;
        Verifying = verifying;
    }

    public int CompletedUnits { get; }

    public int TotalUnits { get; }

    public long Position { get; }

    public bool Verifying { get; }
}
