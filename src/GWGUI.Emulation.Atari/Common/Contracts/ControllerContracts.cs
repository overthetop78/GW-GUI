namespace GWGUI.Emulation.Atari.Common.Contracts;

public sealed record ControllerBinding(
    int Port,
    PeripheralCategory Peripheral,
    string? DeviceId = null,
    IReadOnlyDictionary<string, string>? Mappings = null,
    int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent,
    string? VisualId = null);

internal sealed record ControllerDevice(string Description, uint Id);

internal sealed record ControllerPort(IReadOnlyList<ControllerDevice> Devices);

public sealed record HardwareChoice(string Value, string DisplayName, long? Bytes = null);

public sealed record HardwareField(
    SettingOption Option,
    string ResourceKey,
    IReadOnlyList<HardwareChoice> Choices,
    string SelectedValue,
    OptionAvailability Availability,
    string? ExplanationResourceKey);

public sealed record HardwareView(
    IReadOnlyList<HardwareField> Cpu,
    IReadOnlyList<HardwareField> Memory,
    IReadOnlyList<FirmwareDefinition> Firmware,
    IReadOnlyList<HardwareChoice> Regions);

internal sealed record InputDescriptor(uint Port, uint Device, uint Index, uint Id, string Description);

public sealed record MemoryExpansionChoice(
    string Value,
    long AdditionalBytes);
