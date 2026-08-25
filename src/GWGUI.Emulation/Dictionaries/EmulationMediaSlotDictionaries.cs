namespace GWGUI.Emulation.Dictionaries;

internal static class EmulationMediaSlotDictionaries
{
    internal static IReadOnlyDictionary<string, EmulationMediaCategory> Prefixes { get; } =
        new Dictionary<string, EmulationMediaCategory>(StringComparer.OrdinalIgnoreCase)
        {
        [EmulationMediaSlotConstants.FloppyPrefix] = EmulationMediaCategory.FloppyDrive,
        [EmulationMediaSlotConstants.HardDiskPrefix] = EmulationMediaCategory.HardDisk,
        [EmulationMediaSlotConstants.CompactDiscPrefix] = EmulationMediaCategory.CompactDiscDrive,
        [EmulationMediaSlotConstants.CartridgePrefix] = EmulationMediaCategory.CartridgeSlot,
        [EmulationMediaSlotConstants.CassettePrefix] = EmulationMediaCategory.CassetteDrive
        };
}
