using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public enum AtariMediaKind
{
    Floppy,
    HardDisk,
    Directory,
    Cassette,
    Cartridge,
    CompactDisc
}

public enum AtariStorageBus
{
    Acsi,
    Ide,
    Gemdos
}

public sealed record AtariMediaConfiguration(
    string Path,
    AtariMediaKind Kind,
    EmulationMediaSlot Slot,
    string? Label = null,
    bool IsReadOnly = false,
    bool IsInserted = true,
    AtariStorageBus? StorageBus = null,
    string? MountPoint = null,
    int MountOrder = AtariMediaConstants.DefaultMountOrder);
