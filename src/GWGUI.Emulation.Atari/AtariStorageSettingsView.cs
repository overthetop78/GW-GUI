using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public sealed record AtariStorageTypeChoice(AtariMediaKind Kind, string DisplayName);

public sealed record AtariStorageSlotChoice(EmulationMediaSlot Slot, string DisplayName);

public sealed record AtariStorageBusChoice(AtariStorageBus Bus, string DisplayName);

public sealed record AtariStorageDeviceItem(
    AtariMediaConfiguration Configuration,
    string Identifier,
    string Model,
    bool CanRemove);

public sealed record AtariStorageView(
    IReadOnlyList<AtariStorageTypeChoice> Types,
    IReadOnlyDictionary<AtariMediaKind, IReadOnlyList<AtariStorageSlotChoice>> Slots,
    IReadOnlyDictionary<AtariMediaKind, IReadOnlyList<AtariStorageBusChoice>> Buses,
    IReadOnlyList<AtariStorageDeviceItem> Devices);
