using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Cores;

internal sealed class AmigaInputAccumulator
{
    private readonly object _gate = new();
    private EmulationInputSnapshot _current = EmulationInputSnapshot.Empty;

    internal void Update(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        lock (_gate)
        {
            var pointer = snapshot.Pointer with
            {
                DeltaX = SaturatingAdd(_current.Pointer.DeltaX, snapshot.Pointer.DeltaX),
                DeltaY = SaturatingAdd(_current.Pointer.DeltaY, snapshot.Pointer.DeltaY),
                Wheel = SaturatingAdd(_current.Pointer.Wheel, snapshot.Pointer.Wheel),
                HorizontalWheel = SaturatingAdd(_current.Pointer.HorizontalWheel,
                    snapshot.Pointer.HorizontalWheel)
            };
            _current = snapshot with { Pointer = pointer };
        }
    }

    internal EmulationInputSnapshot Consume()
    {
        lock (_gate)
        {
            var result = _current;
            _current = _current with
            {
                Pointer = _current.Pointer with { DeltaX = 0, DeltaY = 0, Wheel = 0, HorizontalWheel = 0 }
            };
            return result;
        }
    }

    private static int SaturatingAdd(int left, int right) =>
        (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);
}
