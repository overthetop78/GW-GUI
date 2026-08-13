using GWGUI.Infrastructure.Hardware.Greaseweazle;

namespace GWGUI.App.Services.PhysicalDiskWriting;

public sealed record GreaseweazleDriveSelection(
    GreaseweazleBusType BusType,
    byte Unit);
