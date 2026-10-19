namespace GWGUI.Emulation.Services;

internal class EmulationInputFrameStore
{
    private EmulationInputSnapshot _pending = EmulationInputSnapshot.Empty;
    private EmulationInputSnapshot _polled = EmulationInputSnapshot.Empty;

    internal void Update(EmulationInputSnapshot? snapshot)
    {
        var frozen = EmulationInputMappingFunctions.Freeze(snapshot);
        EmulationInputSnapshot current;
        do
        {
            current = Volatile.Read(ref _pending);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _pending, Accumulate(current, frozen), current), current));
    }

    internal void Poll()
    {
        EmulationInputSnapshot current;
        do
        {
            current = Volatile.Read(ref _pending);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _pending, ConsumeRelativePointer(current), current), current));
        Interlocked.Exchange(ref _polled, current);
    }



    internal EmulationInputSnapshot Polled => Volatile.Read(ref _polled);
    protected virtual EmulationInputSnapshot Accumulate(EmulationInputSnapshot current, EmulationInputSnapshot update) => new(new HashSet<EmulationKey>(update.Keys), update.Pointer with
    {
        DeltaX = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.DeltaX, update.Pointer.DeltaX),
        DeltaY = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.DeltaY, update.Pointer.DeltaY),
        Wheel = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.Wheel, update.Pointer.Wheel),
        HorizontalWheel = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.HorizontalWheel, update.Pointer.HorizontalWheel)
    }, update.Controllers.ToArray());

    protected virtual EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) =>
        snapshot with
        {
            Pointer = snapshot.Pointer with
            {
                DeltaX = EmulationHostProtocolConstants.EmptyPointerDelta,
                DeltaY = EmulationHostProtocolConstants.EmptyPointerDelta,
                Wheel = EmulationHostProtocolConstants.EmptyPointerDelta,
                HorizontalWheel = EmulationHostProtocolConstants.EmptyPointerDelta
            }
        };
}
