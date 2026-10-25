namespace GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;

/// <param name="OutputBindings">Associations des commandes de retour du cœur aux périphériques physiques et à leurs moteurs ; null en absence de réglage.</param>
public sealed record ControllerBinding(
    int Port,
    PeripheralCategory Peripheral,
    string? DeviceId = null,
    IReadOnlyDictionary<string, string>? Mappings = null,
    int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent,
    string? VisualId = null, IReadOnlyDictionary<string, EmulationControllerOutputBinding>? OutputBindings = null);

public sealed record HardwareChoice(string Value, string DisplayName, long? Bytes = null);

public sealed record HardwareField(SettingOption Option, string ResourceKey, IReadOnlyList<HardwareChoice> Choices, string SelectedValue, OptionAvailability Availability, string? ExplanationResourceKey);

public sealed record HardwareView(IReadOnlyList<HardwareField> Cpu, IReadOnlyList<HardwareField> Memory, IReadOnlyList<FirmwareDefinition> Firmware, IReadOnlyList<HardwareChoice> Regions);

public sealed record MemoryExpansionChoice(string Value, long AdditionalBytes);
