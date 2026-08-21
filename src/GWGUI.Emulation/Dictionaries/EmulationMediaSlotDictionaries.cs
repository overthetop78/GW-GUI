namespace GWGUI.Emulation;

internal static class EmulationMediaSlotDictionaries
{
    internal static IReadOnlyDictionary<string, EmulationMediaCategory> Prefixes { get; } =
        new Dictionary<string, EmulationMediaCategory>(StringComparer.OrdinalIgnoreCase)
        {
            ["Floppy"] = EmulationMediaCategory.FloppyDrive,
            ["HardDisk"] = EmulationMediaCategory.HardDisk,
            ["Cd"] = EmulationMediaCategory.CompactDiscDrive,
            ["Cartridge"] = EmulationMediaCategory.CartridgeSlot,
            ["Cassette"] = EmulationMediaCategory.CassetteDrive
        };
}
