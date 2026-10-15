using System.IO;

namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static MachineConfiguration WithMedia(MachineConfiguration configuration, IEnumerable<EmulationMedia> media) => configuration with
    {
        Media = media.Select((item, index) => new MediaConfiguration(item.Path,
            item.Type switch
            {
                EmulationMediaType.Floppy => MediaCategory.Floppy,
                EmulationMediaType.Cassette => MediaCategory.Cassette,
                EmulationMediaType.Cartridge => MediaCategory.Cartridge,
                EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
                EmulationMediaType.MemoryCard => MediaCategory.MemoryCard,
                EmulationMediaType.HardDisk => MediaCategory.HardDisk,
                _ => throw new ArgumentOutOfRangeException(nameof(media), item.Type, null)
            }, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted, MountOrder: index, SlotIndex: item.Slot.Index)).ToArray()
    };

    internal static IReadOnlyList<EmulationMedia> ToCommon(IEnumerable<MediaConfiguration> media)
    {
        var indexes = new Dictionary<EmulationMediaCategory, int>();
        var slots = new HashSet<EmulationMediaSlot>();
        return media.OrderBy(item => item.MountOrder).Select(item =>
        {
            var category = item.Category switch
            {
                MediaCategory.Floppy or MediaCategory.Snapshot => EmulationMediaCategory.FloppyDrive,
                MediaCategory.Cassette => EmulationMediaCategory.CassetteDrive,
                MediaCategory.Cartridge => EmulationMediaCategory.CartridgeSlot,
                MediaCategory.CompactDisc => EmulationMediaCategory.CompactDiscDrive,
                MediaCategory.MemoryCard => EmulationMediaCategory.MemoryCard,
                MediaCategory.HardDisk => EmulationMediaCategory.HardDisk,
                _ => throw new ArgumentOutOfRangeException(nameof(media), item.Category, null)
            };
            var index = item.SlotIndex ?? indexes.GetValueOrDefault(category);
            indexes[category] = Math.Max(indexes.GetValueOrDefault(category), index + 1);
            if (index < 0 || !slots.Add(new EmulationMediaSlot(category, index)))
                throw new ArgumentOutOfRangeException(nameof(media), index, null);
            var type = item.Category switch
            {
                MediaCategory.Floppy or MediaCategory.Snapshot => EmulationMediaType.Floppy,
                MediaCategory.Cassette => EmulationMediaType.Cassette,
                MediaCategory.Cartridge => EmulationMediaType.Cartridge,
                MediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
                MediaCategory.MemoryCard => EmulationMediaType.MemoryCard,
                MediaCategory.HardDisk => EmulationMediaType.HardDisk,
                _ => throw new ArgumentOutOfRangeException(nameof(media), item.Category, null)
            };
            return new EmulationMedia(Path.GetFullPath(item.Path), new EmulationMediaSlot(category, index), type, item.IsReadOnly, item.IsInserted);
        }).ToArray();
    }
}

internal static class EmulationMediaActivityFunctions
{
    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromLedStates(IReadOnlyDictionary<int, bool> leds) => new Dictionary<EmulationMediaSlot, bool>
    {
        [EmulationMediaSlot.Floppy0] = leds.GetValueOrDefault(0),
        [EmulationMediaSlot.Cassette0] = leds.GetValueOrDefault(1),
        [EmulationMediaSlot.Cartridge0] = leds.GetValueOrDefault(2),
        [EmulationMediaSlot.Cd0] = leds.GetValueOrDefault(3)
    };
}
