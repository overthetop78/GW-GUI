using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Emulation.Services;

/// <summary>Implémentation commune des descriptions et des contrôles de retours utilisés par les machines.</summary>
internal sealed class EmulationControllerFeedbackService : IEmulationControllerFeedbackProcessing
{
    private static readonly IReadOnlyList<EmulationControllerOutputDefinition> StandardOutputs = Array.AsReadOnly<EmulationControllerOutputDefinition>(
        [new(EmulationControllerFeedbackCommands.Strong, EmulationControllerFeedbackCommands.StrongResourceKey), new(EmulationControllerFeedbackCommands.Weak, EmulationControllerFeedbackCommands.WeakResourceKey)]);

    public IReadOnlyList<EmulationControllerOutputDefinition> DescribeOutputs<TPeripheral>(TPeripheral peripheral) where TPeripheral : struct, Enum
        => !Enum.IsDefined(peripheral) || peripheral.ToString() is nameof(EmulationPeripheralCategory.None) or nameof(EmulationPeripheralCategory.Keyboard) ? [] : StandardOutputs;

    public IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> CreateBindings(IReadOnlyList<EmulationControllerPort> ports, Func<EmulationControllerPort, IReadOnlyList<EmulationControllerOutputDefinition>> describeOutputs)
    {
        var result = new Dictionary<(int, string), EmulationControllerOutputBinding>();
        foreach (var port in ports)
        {
            if (port.Number <= 0 || port.Number > EmulationHostProtocolConstants.MaximumInputControllerCount) continue;
            var commands = describeOutputs(port).Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            foreach (var item in port.OutputBindings)
                if (commands.Contains(item.Key) && item.Value is { } binding && !string.IsNullOrWhiteSpace(binding.DeviceId) && Enum.IsDefined(binding.Motor))
                    result[(port.Number - 1, item.Key)] = binding;
        }
        return result;
    }

    public IReadOnlyList<EmulationControllerOutputState> ResolveStates(IReadOnlyList<ControllerFeedbackState> states, IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> bindings)
    {
        if (bindings.Count == 0) return [];
        var result = new List<EmulationControllerOutputState>();
        foreach (var state in states)
            if (float.IsFinite(state.Strength) && state.Strength is >= 0 and <= 1 && bindings.TryGetValue((state.Port, state.CommandId), out var target))
                result.Add(new(target.DeviceId, target.Motor, state.Strength));
        return result;
    }
}
