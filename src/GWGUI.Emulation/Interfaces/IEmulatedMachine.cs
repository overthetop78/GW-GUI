namespace GWGUI.Emulation.Interfaces;

public interface IEmulatedMachine : IAsyncDisposable
{
    Guid Id { get; }
    EmulationMachineState State { get; }
    IEmulationLifecycle Lifecycle { get; }
    IEmulationInput Input { get; }
    IEmulationMedia Media { get; }
    IEmulationVideo Video { get; }
    IEmulationAudio Audio { get; }
    IEmulationSavedStates SavedStates { get; }
    IEmulationRuntime Runtime { get; }
}
