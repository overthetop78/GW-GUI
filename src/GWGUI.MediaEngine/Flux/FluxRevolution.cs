using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Flux;

public sealed record FluxRevolution
{
    public FluxRevolution(uint IndexTimeTicks, IReadOnlyList<uint> FluxIntervals)
    {
        ArgumentNullException.ThrowIfNull(FluxIntervals);
        this.IndexTimeTicks = IndexTimeTicks;
        this.FluxIntervals = new ReadOnlyCollection<uint>(FluxIntervals.ToArray());
    }

    public uint IndexTimeTicks { get; }
    public IReadOnlyList<uint> FluxIntervals { get; }
}
