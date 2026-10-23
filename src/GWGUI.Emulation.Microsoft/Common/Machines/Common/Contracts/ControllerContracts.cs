namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Contracts;

/// <param name="OutputBindings">Associations des commandes de retour du cœur aux périphériques physiques et à leurs moteurs ; null en absence de réglage.</param>
public sealed record ControllerBinding(int Port, PeripheralCategory Type, string? DeviceId = null, IReadOnlyDictionary<string, string>? ButtonMappings = null, string? VisualId = null, IReadOnlyDictionary<string, EmulationControllerOutputBinding>? OutputBindings = null);

