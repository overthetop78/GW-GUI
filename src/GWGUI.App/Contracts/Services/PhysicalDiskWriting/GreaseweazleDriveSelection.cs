using GWGUI.Infrastructure.Hardware.Greaseweazle;

namespace GWGUI.App.Contracts.Services.PhysicalDiskWriting;

public sealed record GreaseweazleDriveSelection(
    GreaseweazleBusType BusType,
    byte Unit);
