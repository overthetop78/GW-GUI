namespace GWGUI.Emulation;

public interface IEmulationLifecycle
{
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask PauseAsync(CancellationToken cancellationToken = default);
    ValueTask ResumeAsync(CancellationToken cancellationToken = default);
    ValueTask SoftResetAsync(CancellationToken cancellationToken = default);
    ValueTask HardResetAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
