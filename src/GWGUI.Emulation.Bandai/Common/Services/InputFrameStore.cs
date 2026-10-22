using GWGUI.Emulation;

namespace GWGUI.Emulation.Bandai.Common.Services;

internal sealed class InputFrameStore : EmulationInputFrameStore
{
    protected override EmulationInputSnapshot Accumulate(EmulationInputSnapshot current, EmulationInputSnapshot update) =>
        EmulationSdk.InputProcessing.Freeze(update);

    protected override EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) => snapshot;
}