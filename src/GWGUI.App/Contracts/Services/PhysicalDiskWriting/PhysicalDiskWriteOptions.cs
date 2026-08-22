using GWGUI.App.Enums.Services.PhysicalDiskWriting;
using GWGUI.Infrastructure.Hardware.Greaseweazle;

namespace GWGUI.App.Contracts.Services.PhysicalDiskWriting;

public sealed record PhysicalDiskWriteOptions(
    string PortName,
    GreaseweazleBusType BusType,
    byte DriveUnit,
    bool CueAtIndex = true,
    bool TerminateAtIndex = true,
    bool Verify = false,
    PhysicalTrackEncoding PrecompensationEncoding = PhysicalTrackEncoding.Mfm,
    IReadOnlyList<PhysicalWritePrecompensationStep>? Precompensation = null,
    uint HardSectorTicks = 0,
    int ScpRevolution = 0);
