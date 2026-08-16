using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal sealed class AtariInputFrameStore
{
    private EmulationInputSnapshot _pending = EmulationInputSnapshot.Empty;
    private EmulationInputSnapshot _polled = EmulationInputSnapshot.Empty;

    internal void Update(EmulationInputSnapshot? snapshot) =>
        Interlocked.Exchange(ref _pending, AtariInputFunctions.Freeze(snapshot));

    internal void Poll() => Interlocked.Exchange(ref _polled, Volatile.Read(ref _pending));

    internal short State(uint port, uint device, uint index, uint id) =>
        AtariInputFunctions.State(Volatile.Read(ref _polled), port, device, index, id);
}
