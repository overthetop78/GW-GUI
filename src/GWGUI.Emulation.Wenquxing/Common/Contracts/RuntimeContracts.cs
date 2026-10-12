using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Wenquxing.Common.Contracts;

internal sealed record MachineCommand(Action Action, TaskCompletionSource Completion)
{
    internal void Execute()
    {
        try
        {
            Action();
            Completion.TrySetResult();
        }
        catch (Exception error)
        {
            Completion.TrySetException(error);
            throw;
        }
    }
}

public sealed record RuntimeGeometry(int Width, int Height, int Pitch, float AspectRatio);

public sealed record RuntimeStatus( MachineModel Model, EmulationVideoStandard? Region, double FramesPerSecond, int SampleRate, RuntimeGeometry? Geometry, string CoreName, IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity, IReadOnlyDictionary<int, bool> LedStates, int BufferedAudioFrames, long AudioOverrunCount, long AudioUnderrunCount, HostProcessState HostProcessState, int? HostProcessId);
