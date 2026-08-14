using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaEngine : IEmulationEngine<AmigaMachineConfiguration>
{
    private readonly string _sessionsDirectory;
    private readonly string? _externalCorePath;

    public AmigaEngine(string sessionsDirectory, string? externalCorePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionsDirectory);
        _sessionsDirectory = Path.GetFullPath(sessionsDirectory);
        _externalCorePath = externalCorePath;
    }

    public IEmulatedMachine CreateMachine(AmigaMachineConfiguration configuration) =>
        CreateAmigaMachine(configuration);

    public IAmigaMachine CreateAmigaMachine(AmigaMachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var id = Guid.NewGuid();
        return new AmigaMachine(id, configuration,
            new AmigaExternalCore(_externalCorePath), Path.Combine(_sessionsDirectory, id.ToString("N")));
    }
}
