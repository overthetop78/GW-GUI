using System.Collections.Frozen;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Constants;

internal static class HardDiskConstants
{
    internal const int LogicalSectorBytes = 256;
    internal const int SectorAddressBits = 21;
    internal const long MaximumBytes = (1L << SectorAddressBits) * LogicalSectorBytes;
    internal const long DefaultBytes = 40L * 1024 * 1024;
    internal const string InterfaceName = "SASI";
    internal static IReadOnlyList<HardDiskImageFormat> Formats { get; } =
    [
        new("sharp-sasi", ".hdf", InterfaceName, MaximumBytes, DefaultBytes)
        {
            MinimumBytes = LogicalSectorBytes,
            LogicalSectorSizes = new[] { LogicalSectorBytes }.ToFrozenSet()
        }
    ];
}
