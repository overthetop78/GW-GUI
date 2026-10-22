namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static IReadOnlyList<EmulationControllerOutputDefinition> RumbleOutputs { get; } =
    [
        new(EmulationControllerFeedbackCommands.Strong, EmulationControllerFeedbackCommands.StrongResourceKey),
        new(EmulationControllerFeedbackCommands.Weak, EmulationControllerFeedbackCommands.WeakResourceKey)
    ];

    private static IReadOnlyDictionary<string, IReadOnlyList<EmulationControllerOutputDefinition>>? VisualOutputs(MachineConfiguration configuration, PeripheralCategory type, Model model)
    {
        if (new Engine().Adapter(configuration) is not IEmulatorControllerFeedbackAdapter adapter) return null;
        var outputs = adapter.GetControllerOutputs(configuration, type);
        var visuals = CompatibleVisualIds(type, model);
        return outputs.Count == 0 || visuals is null ? null : visuals.ToDictionary(id => id, _ => outputs, StringComparer.Ordinal);
    }

    internal static IReadOnlyDictionary<string, EmulationControllerOutputBinding> ValidOutputBindings(MachineConfiguration configuration, PeripheralCategory type, string? visualId,
        IReadOnlyDictionary<string, EmulationControllerOutputBinding>? bindings)
    {
        if (bindings is null || bindings.Count == 0) return new Dictionary<string, EmulationControllerOutputBinding>();
        var model = ModelCatalog.Get(configuration.Model);
        var visual = visualId ?? DefaultVisualId(type, model);
        var outputs = VisualOutputs(configuration, type, model);
        if (visual is null || outputs is null || !outputs.TryGetValue(visual, out var definitions) || definitions is null) return new Dictionary<string, EmulationControllerOutputBinding>();
        var commands = definitions.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        return bindings.Where(item => commands.Contains(item.Key) && item.Value is { } binding && !string.IsNullOrWhiteSpace(binding.DeviceId) && Enum.IsDefined(binding.Motor))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    }
}
