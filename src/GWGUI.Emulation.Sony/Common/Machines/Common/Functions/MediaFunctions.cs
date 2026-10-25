using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using System.IO;

namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    /// <summary>Adapte le média reçu de la machine sans perdre son indice de lecteur ni sa protection.</summary>
    /// <remarks>Cette description rejoint la file de commandes. Les conversions natives et le montage restent possédés par le cœur.</remarks>
    internal static MediaConfiguration ToModule(EmulationMedia media) => new(media.Path, media.Type switch
    {
        EmulationMediaType.Floppy => MediaCategory.Floppy,
        EmulationMediaType.Cassette => MediaCategory.Cassette,
        EmulationMediaType.Cartridge => MediaCategory.Cartridge,
        EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
        EmulationMediaType.HardDisk => MediaCategory.HardDisk,
        EmulationMediaType.MemoryCard => MediaCategory.MemoryCard,
        _ => throw new ArgumentOutOfRangeException(nameof(media))
    }, IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted, MountOrder: media.Slot.Index, SlotIndex: media.Slot.Index);

    internal static MachineConfiguration WithMedia(MachineConfiguration configuration, IEnumerable<EmulationMedia> media)
    {
        var mounted = media.ToArray();
        var cards = StorageSettingsFunctions.Describe(configuration).AvailableDevices
            .Where(device => device.ConfigurationKind == EmulationStorageConfigurationKind.MemoryCard).ToArray();
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var device in cards)
        {
            var card = mounted.FirstOrDefault(item => item.Slot == device.Slot && item.IsInserted);
            options[StorageSettingsFunctionsConstants.MemoryCardEnabledOptionPrefix + device.Slot.Index] = (card is not null).ToString();
            if (card is not null)
                options[StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + device.Slot.Index] = card.Path;
        }
        return configuration with
        {
        Options = options,
        Media = mounted.Where(item => !cards.Any(device => device.Slot == item.Slot)).Select((item, index) => ToModule(item) with { MountOrder = index }).ToArray()
        };
    }

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
    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromLedStates(IReadOnlyDictionary<int, bool> leds) => CommonCoreHostFunctions.MachineExecution.FromLedStatesWithCd(leds);
}
