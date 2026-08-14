using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaEngine : IEmulationEngine<AmigaMachineConfiguration>
{
    private readonly string _sessionsDirectory;
    private readonly string? _externalCorePath;
    private readonly Func<IAudioOutput?>? _audioOutputFactory;
    private readonly Func<AmigaMachineConfiguration, string>? _saveDirectoryResolver;

    public AmigaEngine(string sessionsDirectory, string? externalCorePath = null,
        Func<IAudioOutput?>? audioOutputFactory = null,
        Func<AmigaMachineConfiguration, string>? saveDirectoryResolver = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionsDirectory);
        _sessionsDirectory = Path.GetFullPath(sessionsDirectory);
        _externalCorePath = externalCorePath;
        _audioOutputFactory = audioOutputFactory;
        _saveDirectoryResolver = saveDirectoryResolver;
    }

    public IEmulatedMachine CreateMachine(AmigaMachineConfiguration configuration) =>
        CreateAmigaMachine(configuration);

    public IAmigaMachine CreateAmigaMachine(AmigaMachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        configuration = configuration.EnsureId();
        var machineId = Guid.NewGuid();
        return new AmigaMachine(machineId, configuration,
            new AmigaExternalCore(_externalCorePath), Path.Combine(_sessionsDirectory, machineId.ToString("N")),
            configuration.AudioEnabled ? _audioOutputFactory?.Invoke() : null,
            _saveDirectoryResolver?.Invoke(configuration));
    }
}
