namespace GWGUI.Emulation.Contracts;

public sealed record EmulationControllerPort(
    int Number,
    IReadOnlyList<EmulationControllerChoice> ControllerChoices,
    string SelectedControllerId,
    string? PhysicalDeviceId,
    EmulationInputBindingSet Bindings,
    int DeadZonePercent = 0,
    string? VisualId = null)
{
    public IReadOnlyDictionary<string, EmulationControllerOutputBinding> OutputBindings { get; init; } = new Dictionary<string, EmulationControllerOutputBinding>();
}
