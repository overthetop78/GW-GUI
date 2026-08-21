namespace GWGUI.Emulation;

public sealed record EmulationStorageDeviceSettings(
    EmulationMediaSlot Slot,
    FloppyDriveSettings? Floppy = null,
    string? InterfaceId = null);
