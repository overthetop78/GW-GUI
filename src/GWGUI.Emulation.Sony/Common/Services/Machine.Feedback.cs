using CommonControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
namespace GWGUI.Emulation.Sony.Common.Services;

internal sealed partial class Machine
{
    private MachineConfiguration? _feedbackConfiguration;
    private IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> _feedbackBindings =
        new Dictionary<(int, string), EmulationControllerOutputBinding>();

    public IReadOnlyList<EmulationControllerOutputState> ControllerOutputs
    {
        get
        {
            if (_disposed || State != EmulationMachineState.Running) return [];
            var configuration = Configuration;
            IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> bindings;
            lock (_gate)
            {
                if (!ReferenceEquals(configuration, _feedbackConfiguration))
                {
                    var selected = new Dictionary<(int, string), EmulationControllerOutputBinding>();
                    foreach (var port in configuration.Input?.ControllerBindings ?? [])
                    {
                        if (port.Port < 0 || port.Port >= CommonControllerFunctions.PortCount(configuration)) continue;
                        var type = CommonControllerFunctions.Resolve(configuration, port.Port);
                        foreach (var binding in InputSettingsFunctions.ValidOutputBindings(configuration, type, port.VisualId, port.OutputBindings))
                            selected[(port.Port, binding.Key)] = binding.Value;
                    }
                    _feedbackBindings = selected;
                    _feedbackConfiguration = configuration;
                }
                bindings = _feedbackBindings;
            }
            if (bindings.Count == 0) return [];
            var result = new List<EmulationControllerOutputState>();
            foreach (var effect in _core.ControllerFeedback)
                if (float.IsFinite(effect.Strength) && effect.Strength is >= 0 and <= 1 && bindings.TryGetValue((effect.Port, effect.CommandId), out var target))
                    result.Add(new(target.DeviceId, target.Motor, effect.Strength));
            return State == EmulationMachineState.Running && !_disposed ? result : [];
        }
    }
}
