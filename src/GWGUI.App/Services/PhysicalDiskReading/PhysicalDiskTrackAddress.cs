namespace GWGUI.App.Services.PhysicalDiskReading;

public sealed record PhysicalDiskTrackAddress(
    int Cylinder,
    int Head,
    int? PhysicalCylinder = null,
    int? PhysicalHead = null)
{
    public int DriveCylinder => PhysicalCylinder ?? Cylinder;

    public int DriveHead => PhysicalHead ?? Head;
}
