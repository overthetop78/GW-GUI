using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Contracts;

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

public sealed record OptionRule(
    SettingOption Option,
    OptionAvailability Availability,
    string? ForcedValue = null,
    string? ExplanationResourceKey = null);

public sealed record RuntimeGeometry(int Width, int Height, int Pitch, float AspectRatio);

public sealed record RuntimeStatus(
    MachineModel Model,
    RuntimeRegion? Region,
    double FramesPerSecond,
    int SampleRate,
    RuntimeGeometry? Geometry,
    string CoreName,
    IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity,
    IReadOnlyDictionary<int, bool> LedStates,
    int BufferedAudioFrames,
    long AudioOverrunCount,
    long AudioUnderrunCount,
    HostProcessState HostProcessState,
    int? HostProcessId);
