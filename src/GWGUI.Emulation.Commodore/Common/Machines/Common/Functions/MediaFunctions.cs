using System.IO;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static IReadOnlyList<EmulationMedia> ToCommon(
        IReadOnlyList<MediaConfiguration> media)
    {
        var indexes = new Dictionary<EmulationMediaCategory, int>();
        var result = new List<EmulationMedia>();
        foreach (var item in media)
        {
            if (!TryCategory(item.Category, out var category)) continue;
            var index = item.SlotIndex ?? indexes.GetValueOrDefault(category);
            if (index < BufferConstants.FirstCollectionIndex) throw new ArgumentOutOfRangeException(nameof(item.SlotIndex));
            indexes[category] = index;
            indexes[category]++;
            result.Add(new EmulationMedia(Path.GetFullPath(item.Path),
                new EmulationMediaSlot(category, index), ToType(item.Category), item.IsReadOnly, true));
        }
        return result;
    }

    internal static IReadOnlyList<MediaConfiguration> FromCommon(IEnumerable<EmulationMedia> media) =>
        media.Select(item => new MediaConfiguration(item.Path, item.Type switch
        {
            EmulationMediaType.Floppy => MediaCategory.Floppy,
            EmulationMediaType.HardDisk => MediaCategory.HardDrive,
            EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
            EmulationMediaType.Cassette => MediaCategory.Cassette,
            EmulationMediaType.Cartridge => MediaCategory.Cartridge,
            _ => throw new ArgumentOutOfRangeException(nameof(media), item.Type, null)
        }, IsReadOnly: item.IsReadOnly, SlotIndex: item.Slot.Index)).ToArray();

    private static bool TryCategory(MediaCategory media, out EmulationMediaCategory category)
    {
        category = media switch
        {
            MediaCategory.Floppy => EmulationMediaCategory.FloppyDrive,
            MediaCategory.HardDrive => EmulationMediaCategory.HardDisk,
            MediaCategory.CompactDisc => EmulationMediaCategory.CompactDiscDrive,
            MediaCategory.Cassette => EmulationMediaCategory.CassetteDrive,
            MediaCategory.Cartridge => EmulationMediaCategory.CartridgeSlot,
            _ => default
        };
        return media is MediaCategory.Floppy or MediaCategory.HardDrive or MediaCategory.CompactDisc
            or MediaCategory.Cassette or MediaCategory.Cartridge;
    }

    private static EmulationMediaType ToType(MediaCategory media) => media switch
    {
        MediaCategory.Floppy => EmulationMediaType.Floppy,
        MediaCategory.HardDrive => EmulationMediaType.HardDisk,
        MediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
        MediaCategory.Cassette => EmulationMediaType.Cassette,
        MediaCategory.Cartridge => EmulationMediaType.Cartridge,
        _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
    };
}

public static class RuntimeMediaFunctions
{
    public static ValueTask<EmulationMedia> PrepareMediaAsync(MachineConfiguration configuration,
        EmulationMedia media, string conversionDirectory) =>
        EmulatorCatalog.CreateAdapter(configuration.Core).PrepareMediaAsync(media, conversionDirectory);

    public static Task<MachineConfiguration> PrepareConfigurationAsync(
        MachineConfiguration configuration, string conversionDirectory) =>
        EmulatorCatalog.CreateAdapter(configuration.Core).PrepareRuntimeConfigurationAsync(configuration, conversionDirectory);
}
