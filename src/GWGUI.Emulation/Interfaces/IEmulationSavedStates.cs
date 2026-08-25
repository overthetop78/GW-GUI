namespace GWGUI.Emulation.Interfaces;

public interface IEmulationSavedStates
{
    bool IsSupported { get; }
    ValueTask SaveAsync(string path, CancellationToken cancellationToken = default);
    ValueTask LoadAsync(string path, CancellationToken cancellationToken = default);
}
