using System.IO;

namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public sealed class GreaseweazlePositionException(short cylinder, bool trackZeroAsserted)
    : IOException($"Track-zero signal {(trackZeroAsserted ? "asserted" : "absent")} after seeking to cylinder {cylinder}.")
{
    public short Cylinder { get; } = cylinder;

    public bool TrackZeroAsserted { get; } = trackZeroAsserted;
}
