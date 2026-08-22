using GWGUI.App.Contracts.Services.PhysicalDiskWriting;
using GWGUI.Infrastructure.Hardware.Greaseweazle;

namespace GWGUI.App.Functions.Services.PhysicalDiskWriting;

public static class GreaseweazleDriveSelectionFunctions
{
    public static GreaseweazleDriveSelection Resolve(string selection)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selection);
        return selection.Trim().ToUpperInvariant() switch
        {
            "A" => new(GreaseweazleBusType.IbmPc, 0),
            "B" => new(GreaseweazleBusType.IbmPc, 1),
            "0" => new(GreaseweazleBusType.Shugart, 0),
            "1" => new(GreaseweazleBusType.Shugart, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "The drive selection is unsupported.")
        };
    }
}
