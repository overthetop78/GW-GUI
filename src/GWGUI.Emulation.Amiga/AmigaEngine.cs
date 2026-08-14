using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaEngine : IEmulationEngine<AmigaMachineConfiguration>
{
    private readonly string _sessionsDirectory;
    private readonly string? _externalCorePath;
    private readonly Func<IAudioOutput?>? _audioOutputFactory;

    public AmigaEngine(string sessionsDirectory, string? externalCorePath = null,
        Func<IAudioOutput?>? audioOutputFactory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionsDirectory);
        _sessionsDirectory = Path.GetFullPath(sessionsDirectory);
        _externalCorePath = externalCorePath;
        _audioOutputFactory = audioOutputFactory;
    }

    public IEmulatedMachine CreateMachine(AmigaMachineConfiguration configuration) =>
        CreateAmigaMachine(configuration);

    public IAmigaMachine CreateAmigaMachine(AmigaMachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        configuration = configuration.EnsureId();
        return new AmigaMachine(configuration.Id, configuration,
            new AmigaExternalCore(_externalCorePath), Path.Combine(_sessionsDirectory, configuration.Id.ToString("N")),
            configuration.AudioEnabled ? _audioOutputFactory?.Invoke() : null);
    }
}
