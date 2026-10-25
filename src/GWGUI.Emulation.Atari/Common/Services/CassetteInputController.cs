using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class CassetteInputController
{
    private readonly object _gate = new();
    private readonly Queue<EmulationKey> _pulses = new();
    private readonly IReadOnlyDictionary<EmulationCassetteCommand, IReadOnlyList<EmulationKey>> _keyCommands;
    private EmulationInputSnapshot _physicalInput = EmulationInputSnapshot.Empty;
    private int _automaticReturnFrames;

    internal IReadOnlySet<EmulationCassetteCommand> AvailableCommands => _keyCommands.Keys.ToHashSet();

    internal CassetteInputController(MachineConfiguration configuration, IEmulatorRuntimePolicy? runtimePolicy = null)
    {
        var policy = runtimePolicy ?? new Engine().SelectedAdapter(configuration);
        _keyCommands = policy?.GetCassetteKeyCommands(configuration) ?? new Dictionary<EmulationCassetteCommand, IReadOnlyList<EmulationKey>>();
        _automaticReturnFrames = _keyCommands.ContainsKey(EmulationCassetteCommand.Play) ? policy?.GetCassetteBootReturnDelayFrames(configuration) ?? -1 : -1;
    }

    internal void SetPhysicalInput(EmulationInputSnapshot snapshot)
    {
        lock (_gate) _physicalInput = snapshot;
    }

    internal void Execute(EmulationCassetteCommand command)
    {
        lock (_gate)
        {
            if (!_keyCommands.TryGetValue(command, out var keys)) throw new NotSupportedException(command.ToString());
            foreach (var key in keys) _pulses.Enqueue(key);
        }
    }

    internal EmulationInputSnapshot NextFrame()
    {
        lock (_gate)
        {
            if (_automaticReturnFrames > 0) _automaticReturnFrames--;
            else if (_automaticReturnFrames == 0)
            {
                Execute(EmulationCassetteCommand.Play);
                _automaticReturnFrames = -1;
            }

            if (_pulses.Count == 0) return _physicalInput;
            var keys = _physicalInput.Keys.ToHashSet();
            keys.Add(_pulses.Dequeue());
            return _physicalInput with { Keys = keys };
        }
    }
}