using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Cores;
using System.Runtime.Versioning;

namespace GWGUI.Emulation.Atari;

[SupportedOSPlatform("windows")]
public sealed class AtariEngine : IEmulationEngine<AtariMachineConfiguration>
{
    private readonly string _sessionsDirectory;
    private readonly string _corePath;
    private readonly string _hostExecutablePath;
    private readonly Func<IAudioOutput?>? _audioOutputFactory;
    private readonly Func<AtariMachineConfiguration, string>? _saveDirectoryResolver;

    public AtariEngine(string sessionsDirectory, string corePath, string hostExecutablePath,
        Func<IAudioOutput?>? audioOutputFactory = null,
        Func<AtariMachineConfiguration, string>? saveDirectoryResolver = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(corePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(hostExecutablePath);
        _sessionsDirectory = Path.GetFullPath(sessionsDirectory);
        _corePath = Path.GetFullPath(corePath);
        _hostExecutablePath = Path.GetFullPath(hostExecutablePath);
        _audioOutputFactory = audioOutputFactory;
        _saveDirectoryResolver = saveDirectoryResolver;
    }

    public IEmulatedMachine CreateMachine(AtariMachineConfiguration configuration) =>
        CreateAtariMachine(configuration);

    public IAtariMachine CreateAtariMachine(AtariMachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var machineId = Guid.NewGuid();
        var core = new AtariProcessCore(_hostExecutablePath, _corePath, configuration.Core);
        return new AtariMachine(machineId, configuration, core,
            Path.Combine(_sessionsDirectory, machineId.ToString(AtariEngineConstants.IdentifierFormat)),
            audioOutputFactory: configuration.AudioEnabled ? _audioOutputFactory : null,
            saveDirectory: _saveDirectoryResolver?.Invoke(configuration));
    }
}
