using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.AcornScsiConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

/// <summary>Décode l'interface hôte Acorn ; le cœur conserve la possession des images attachées.</summary>
internal sealed class Winchester(bool enabled)
{
    private readonly IEmulationDiskImage?[] _images = new IEmulationDiskImage?[C.UnitCount];
    private AcornScsi? _controller;
    private AcornScsi Controller => _controller ??= new AcornScsi(unit => _images[unit]);
    internal bool Irq => enabled && Controller.Irq;
    internal byte Read(ushort address) => enabled ? Controller.Read(address & 3) : (byte)255;
    internal void Write(ushort address, byte value) { if (enabled) Controller.Write(address & 3, value); }
    internal void Tick() { if (enabled) Controller.Tick(); }
    internal void Reset() { if (enabled) Controller.Reset(); }
    internal bool Active(int unit) => enabled && unit is >= 0 and < C.UnitCount && _images[unit] is not null && Controller.Active(unit);
    internal bool Writing(int unit) => Active(unit) && Controller.Writing;
    internal void Configure(int unit, int latencyMilliseconds, int speedPercent, int? writeLatencyMilliseconds = null) { if (enabled) Controller.Configure(unit, latencyMilliseconds, speedPercent, writeLatencyMilliseconds); }
    internal void Attach(int unit, IEmulationDiskImage? image)
    {
        if (unit is < 0 or >= C.UnitCount) throw new ArgumentOutOfRangeException(nameof(unit));
        if (!enabled && image is not null) throw new InvalidOperationException("The Winchester interface is not installed.");
        if (enabled && Controller.Busy && Controller.Unit == unit) Controller.Reset();
        _images[unit] = image;
    }
}
