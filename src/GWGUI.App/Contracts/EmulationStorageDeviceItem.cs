using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public sealed record EmulationStorageDeviceItem(
    EmulationMediaSlot Slot,
    string Identifier,
    EmulationMediaType Type,
    string Model,
    string? SupportPath,
    bool CanRemove = true);
