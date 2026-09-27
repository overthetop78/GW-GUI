using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class InputFrameStore
{
    private EmulationInputSnapshot _pending = EmulationInputSnapshot.Empty;
    private EmulationInputSnapshot _polled = EmulationInputSnapshot.Empty;

    internal void Update(EmulationInputSnapshot? snapshot)
    {
        var frozen = InputFunctions.Freeze(snapshot);
        EmulationInputSnapshot current;
        do
        {
            current = Volatile.Read(ref _pending);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _pending,
                     InputFunctions.Accumulate(current, frozen), current), current));
    }

    internal void Poll()
    {
        EmulationInputSnapshot current;
        do
        {
            current = Volatile.Read(ref _pending);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _pending,
                     InputFunctions.ConsumeRelativePointer(current), current), current));
        Interlocked.Exchange(ref _polled, current);
    }

    internal short State(uint port, uint device, uint index, uint id) =>
        InputFunctions.State(Volatile.Read(ref _polled), port, device, index, id);

    internal EmulationInputSnapshot Polled => Volatile.Read(ref _polled);
}
