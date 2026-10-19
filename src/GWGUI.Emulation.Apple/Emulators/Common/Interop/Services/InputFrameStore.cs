using GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Services;

internal sealed class InputFrameStore : EmulationInputFrameStore
{
    internal short State(uint port, uint device, uint index, uint id) => InputFunctions.State(Polled, port, device, index, id);
}
