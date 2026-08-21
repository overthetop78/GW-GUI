using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal static class EmulationMediaConversionFunctions
{
    internal static IReadOnlyList<EmulationMedia> ToCommon(
        IReadOnlyList<AmigaMediaConfiguration> media)
    {
        var indexes = new Dictionary<EmulationMediaCategory, int>();
        var result = new List<EmulationMedia>();
        foreach (var item in media)
        {
            if (!TryCategory(item.Category, out var category)) continue;
            var index = indexes.GetValueOrDefault(category);
            indexes[category] = index + 1;
            result.Add(new EmulationMedia(Path.GetFullPath(item.Path),
                new EmulationMediaSlot(category, index), ToType(item.Category), item.IsReadOnly, true));
        }
        return result;
    }

    private static bool TryCategory(AmigaMediaCategory media, out EmulationMediaCategory category)
    {
        category = media switch
        {
            AmigaMediaCategory.Floppy => EmulationMediaCategory.FloppyDrive,
            AmigaMediaCategory.HardDrive => EmulationMediaCategory.HardDisk,
            AmigaMediaCategory.CompactDisc => EmulationMediaCategory.CompactDiscDrive,
            _ => default
        };
        return media is AmigaMediaCategory.Floppy or AmigaMediaCategory.HardDrive or AmigaMediaCategory.CompactDisc;
    }

    private static EmulationMediaType ToType(AmigaMediaCategory media) => media switch
    {
        AmigaMediaCategory.Floppy => EmulationMediaType.Floppy,
        AmigaMediaCategory.HardDrive => EmulationMediaType.HardDisk,
        AmigaMediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
    };
}
