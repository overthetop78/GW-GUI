namespace GWGUI.Emulation.Contracts;

public sealed record EmulationMachineRuntime(
    IEmulationConfiguration Configuration,
    Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> CreateMachine,
    IReadOnlyList<EmulationMediaDevice> MediaDevices,
    IReadOnlyList<EmulationMedia> MountedMedia,
    string DisplayResourceKey,
    bool SupportsPointerCapture,
    Func<EmulationMedia, CancellationToken, ValueTask<EmulationMedia>>? PrepareMediaAsync = null);
