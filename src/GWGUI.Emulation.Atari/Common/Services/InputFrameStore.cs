using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class InputFrameStore : EmulationInputFrameStore
{
    internal short State(uint port, uint device, uint index, uint id) => InputFunctions.State(Polled, port, device, index, id);
}
