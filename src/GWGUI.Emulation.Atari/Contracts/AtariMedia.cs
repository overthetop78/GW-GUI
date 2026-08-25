using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariMediaConfiguration(
    string Path,
    AtariMediaCategory Category,
    EmulationMediaSlot Slot,
    string? Label = null,
    bool IsReadOnly = false,
    bool IsInserted = true,
    AtariStorageBus? StorageBus = null,
    string? MountPoint = null,
    int MountOrder = AtariMediaConstants.DefaultMountOrder,
    AtariCartridgePlatform? CartridgePlatform = null,
    int? CartridgeType = null,
    bool CassetteAutoBoot = false,
    AtariCartridgeRegion? CartridgeRegion = null);
