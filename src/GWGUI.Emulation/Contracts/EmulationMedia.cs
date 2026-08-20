namespace GWGUI.Emulation;

public sealed record EmulationMedia(
    string Path,
    EmulationMediaSlot Slot,
    EmulationMediaType Type,
    bool IsReadOnly,
    bool IsInserted);
