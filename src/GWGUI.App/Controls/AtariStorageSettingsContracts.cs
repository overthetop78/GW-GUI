using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed record AtariStorageTypeChoice(AtariMediaKind Kind, string DisplayName);
internal sealed record AtariStorageSlotChoice(EmulationMediaSlot Slot, string DisplayName);
internal sealed record AtariStorageBusChoice(AtariStorageBus Bus, string DisplayName);
internal sealed record AtariStorageDeviceItem(
    AtariMediaConfiguration Configuration,
    string Identifier,
    string Model,
    bool CanRemove);
internal sealed record AtariStorageView(
    IReadOnlyList<AtariStorageTypeChoice> Types,
    IReadOnlyDictionary<AtariMediaKind, IReadOnlyList<AtariStorageSlotChoice>> Slots,
    IReadOnlyDictionary<AtariMediaKind, IReadOnlyList<AtariStorageBusChoice>> Buses,
    IReadOnlyList<AtariStorageDeviceItem> Devices);
