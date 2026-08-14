using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaEngine : IEmulationEngine<AmigaMachineConfiguration>
{
    private readonly string _sessionsDirectory;
    private readonly string? _externalCorePath;
    private readonly string? _hostExecutablePath;
    private readonly Func<IAudioOutput?>? _audioOutputFactory;
    private readonly Func<AmigaMachineConfiguration, string>? _saveDirectoryResolver;

    public AmigaEngine(string sessionsDirectory, string? externalCorePath = null,
        Func<IAudioOutput?>? audioOutputFactory = null,
        Func<AmigaMachineConfiguration, string>? saveDirectoryResolver = null,
        string? hostExecutablePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionsDirectory);
        _sessionsDirectory = Path.GetFullPath(sessionsDirectory);
        _externalCorePath = externalCorePath;
        _hostExecutablePath = hostExecutablePath;
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
        IAmigaCore core = _hostExecutablePath is null
            ? new AmigaExternalCore(_externalCorePath)
            : new AmigaProcessCore(_hostExecutablePath, _externalCorePath);
        return new AmigaMachine(machineId, configuration,
            core, Path.Combine(_sessionsDirectory, machineId.ToString("N")),
            configuration.AudioEnabled ? _audioOutputFactory?.Invoke() : null,
            _saveDirectoryResolver?.Invoke(configuration));
    }
}
