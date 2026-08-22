using GWGUI.App.Constants.Services.PhysicalDiskReading;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.App.Contracts.Services.PhysicalDiskReading;

public sealed record PhysicalDiskReadOptions(
    string PortName,
    GreaseweazleBusType BusType,
    byte DriveUnit,
    IReadOnlyList<PhysicalDiskTrackAddress> Tracks,
    ScpDiskType DiskType,
    int Revolutions = PhysicalDiskReadDefaults.Revolutions,
    int FluxOverflowRetries = PhysicalDiskReadDefaults.FluxOverflowRetries,
    int SeekRetries = PhysicalDiskReadDefaults.SeekRetries,
    TimeSpan? FakeIndexPeriod = null,
    bool HardSectors = false,
    TimeSpan? MotorSpinUpDelay = null,
    TimeSpan? TrackSettleDelay = null);
