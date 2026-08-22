using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Emulation.Storage;

public sealed record EmulationStorageDeviceItem(
    EmulationMediaSlot Slot,
    string Identifier,
    EmulationMediaType Type,
    string Model,
    string? SupportPath,
    bool CanRemove = true);
