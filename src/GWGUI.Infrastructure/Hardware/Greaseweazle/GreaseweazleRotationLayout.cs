using System.Collections.ObjectModel;

namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public sealed record GreaseweazleRotationLayout
{
    public GreaseweazleRotationLayout(
        uint initialIndexTicks,
        IReadOnlyList<uint> revolutionTicks,
        IReadOnlyList<int>? hardSectorCounts = null)
    {
        ArgumentNullException.ThrowIfNull(revolutionTicks);
        if (revolutionTicks.Count == 0 || revolutionTicks.Any(value => value == 0))
            throw new ArgumentException("At least one non-empty revolution is required.", nameof(revolutionTicks));
        InitialIndexTicks = initialIndexTicks;
        RevolutionTicks = new ReadOnlyCollection<uint>(revolutionTicks.ToArray());
        HardSectorCounts = new ReadOnlyCollection<int>((hardSectorCounts ?? []).ToArray());
    }

    public uint InitialIndexTicks { get; }

    public IReadOnlyList<uint> RevolutionTicks { get; }

    public IReadOnlyList<int> HardSectorCounts { get; }
}
