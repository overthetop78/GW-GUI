namespace GWGUI.Emulation;

public sealed record EmulationStorageSettings(
    IReadOnlyList<EmulationMediaDevice> AvailableDevices,
    IReadOnlyList<EmulationMediaSlot> ConfiguredSlots,
    IReadOnlyList<EmulationMedia> MountedMedia,
    IReadOnlyList<EmulationStorageDeviceSettings>? DeviceSettings = null);
