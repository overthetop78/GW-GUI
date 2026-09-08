using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Services;

internal sealed class AtariCassetteInputController
{
    internal const int AutomaticReturnDelayFrames = 75;
    internal static IReadOnlySet<EmulationCassetteCommand> AvailableCommands { get; } =
        new HashSet<EmulationCassetteCommand> { EmulationCassetteCommand.Play };

    private readonly object _gate = new();
    private readonly Queue<EmulationKey> _pulses = new();
    private EmulationInputSnapshot _physicalInput = EmulationInputSnapshot.Empty;
    private int _automaticReturnFrames;

    internal AtariCassetteInputController(AtariMachineConfiguration configuration)
    {
        _automaticReturnFrames = AtariCassetteBootFunctions.RequiresDelayedReturn(configuration)
            ? AutomaticReturnDelayFrames
            : -1;
    }

    internal void SetPhysicalInput(EmulationInputSnapshot snapshot)
    {
        lock (_gate) _physicalInput = snapshot;
    }

    internal void Play()
    {
        lock (_gate) _pulses.Enqueue(EmulationKey.Return);
    }

    internal EmulationInputSnapshot NextFrame()
    {
        lock (_gate)
        {
            if (_automaticReturnFrames > 0) _automaticReturnFrames--;
            else if (_automaticReturnFrames == 0)
            {
                _pulses.Enqueue(EmulationKey.Return);
                _automaticReturnFrames = -1;
            }

            if (_pulses.Count == 0) return _physicalInput;
            var keys = _physicalInput.Keys.ToHashSet();
            keys.Add(_pulses.Dequeue());
            return _physicalInput with { Keys = keys };
        }
    }
}
