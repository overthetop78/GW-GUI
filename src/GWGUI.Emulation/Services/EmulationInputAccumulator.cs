namespace GWGUI.Emulation.Services;

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
                    DeltaX = EmulationInputMappingFunctions.SaturatingAdd(_current.Pointer.DeltaX, snapshot.Pointer.DeltaX),
                    DeltaY = EmulationInputMappingFunctions.SaturatingAdd(_current.Pointer.DeltaY, snapshot.Pointer.DeltaY),
                    Wheel = EmulationInputMappingFunctions.SaturatingAdd(_current.Pointer.Wheel, snapshot.Pointer.Wheel),
                    HorizontalWheel = EmulationInputMappingFunctions.SaturatingAdd(_current.Pointer.HorizontalWheel, snapshot.Pointer.HorizontalWheel)
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
                    Wheel = EmulationHostProtocolConstants.EmptyPointerDelta,
                    HorizontalWheel = EmulationHostProtocolConstants.EmptyPointerDelta
                }
            };
            return result;
        }
    }

    internal void Reset()
    {
        lock (_gate) _current = EmulationInputSnapshot.Empty;
    }
}
