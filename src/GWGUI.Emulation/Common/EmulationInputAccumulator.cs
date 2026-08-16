namespace GWGUI.Emulation.Common;

internal sealed class EmulationInputAccumulator
{
    private readonly object _gate = new();
    private EmulationInputSnapshot _current = EmulationInputSnapshot.Empty;

    internal void Update(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        lock (_gate)
        {
            _current = snapshot with
            {
                Pointer = snapshot.Pointer with
                {
                    DeltaX = SaturatingAdd(_current.Pointer.DeltaX, snapshot.Pointer.DeltaX),
                    DeltaY = SaturatingAdd(_current.Pointer.DeltaY, snapshot.Pointer.DeltaY),
                    Wheel = SaturatingAdd(_current.Pointer.Wheel, snapshot.Pointer.Wheel)
                }
            };
        }
    }

    internal EmulationInputSnapshot Consume()
    {
        lock (_gate)
        {
            var result = _current;
            _current = _current with
            {
                Pointer = _current.Pointer with
                {
                    DeltaX = EmulationHostProtocolConstants.EmptyPointerDelta,
                    DeltaY = EmulationHostProtocolConstants.EmptyPointerDelta,
                    Wheel = EmulationHostProtocolConstants.EmptyPointerDelta
                }
            };
            return result;
        }
    }

    private static int SaturatingAdd(int left, int right) =>
        (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);
}
