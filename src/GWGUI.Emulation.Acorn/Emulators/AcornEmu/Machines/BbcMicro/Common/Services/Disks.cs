using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.DiskConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

/// <summary>Câblage des cartes BBC 8271 ou Acorn WD1770 ; les images sont empruntées au propriétaire du cœur.</summary>
internal sealed class Disks(bool enabled, bool wd1770 = false)
{
    private readonly IEmulationDiskImage?[] _images = new IEmulationDiskImage?[C.DriveCount];
    private readonly int[] _speeds = [100, 100];
    private Intel8271? _controller;
    private Intel8271 Controller => _controller ??= new Intel8271(index => _images[index]);
    private Wd1770? _wd;
    private byte _control;
    private Wd1770 Wd => _wd ??= new Wd1770(index => _images[index]) { Selected = false };
    internal bool Nmi => enabled && (wd1770 ? Wd.Nmi : Controller.Nmi);
    internal void Tick() { if (enabled) { if (wd1770) { if ((_control & 0x20) != 0) Wd.Tick(); } else Controller.Tick(); } }
    internal void Reset() { if (enabled) { if (wd1770) { _control = 0; Wd.Reset(); Wd.Selected = false; } else Controller.Reset(); } }
    internal byte Read(ushort address) => !enabled ? (byte)0xfe : wd1770 ? (address & 4) != 0 ? Wd.Read(address & 3) : (byte)0xfe : Controller.Read(address & 7);
    internal void Write(ushort address, byte value)
    {
        if (!enabled) return;
        if (wd1770)
        {
            if ((address & 4) != 0) { if ((_control & 0x20) != 0) Wd.Write(address & 3, value); }
            else
            {
                if ((value & 0x20) == 0) Wd.Reset();
                _control = value; Wd.Drive = (value & 2) != 0 ? 1 : 0; Wd.Side = (value & 4) != 0 ? 1 : 0;
                Wd.Selected = (value & 3) != 0; Wd.DoubleDensity = (value & 8) == 0;
                Wd.MechanicalSpeedPercent = _speeds[Wd.Drive];
            }
            return;
        }
        if ((address & 7) == 0) Controller.MechanicalSpeedPercent = _speeds[(value & 0x80) == 0 ? 0 : 1];
        Controller.Write(address & 7, value);
    }
    internal bool Active(int drive) => enabled && _images[drive] is not null && (wd1770 ? Wd.Selected && Wd.Motor && Wd.Drive == drive : Controller.Motor && Controller.ActiveDrive == drive);
    internal void SetSpeed(int drive, int percent)
    {
        if (drive is < 0 or >= C.DriveCount || percent < 0) throw new ArgumentOutOfRangeException(nameof(drive));
        _speeds[drive] = percent;
    }
    internal bool Writing(int drive) => enabled && (wd1770 ? Wd.Selected && Wd.Drive == drive && Wd.Writing : Controller.ActiveDrive == drive && Controller.Writing);
    internal void Attach(int drive, IEmulationDiskImage? image)
    {
        if (drive < 0 || drive >= C.DriveCount) throw new ArgumentOutOfRangeException(nameof(drive));
        if (!enabled && image is not null) throw new InvalidOperationException("The disk interface is not installed.");
        if (enabled)
        {
            if (wd1770) { if (Wd.Active && Wd.Drive == drive) Wd.Reset(); }
            else if (Controller.Active && Controller.ActiveDrive == drive) Controller.Reset();
        }
        _images[drive] = image;
    }
}
