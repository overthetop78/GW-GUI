namespace GWGUI.Emulation;

public interface IEmulatedMachine : IAsyncDisposable
{
    Guid Id { get; }
    EmulationMachineState State { get; }
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask PauseAsync(CancellationToken cancellationToken = default);
    ValueTask ResumeAsync(CancellationToken cancellationToken = default);
    ValueTask SoftResetAsync(CancellationToken cancellationToken = default);
    ValueTask HardResetAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
