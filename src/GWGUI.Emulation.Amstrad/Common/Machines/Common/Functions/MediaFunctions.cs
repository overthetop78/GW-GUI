using System.IO;

namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static IReadOnlyList<EmulationMedia> ToCommon(IEnumerable<MediaConfiguration> media)
    {
        var indexes = new Dictionary<EmulationMediaCategory, int>();
        return media.OrderBy(item => item.MountOrder).Select(item =>
        {
            var category = item.Category switch
            {
                MediaCategory.Floppy or MediaCategory.Snapshot => EmulationMediaCategory.FloppyDrive,
                MediaCategory.Cassette => EmulationMediaCategory.CassetteDrive,
                MediaCategory.Cartridge => EmulationMediaCategory.CartridgeSlot,
                _ => throw new ArgumentOutOfRangeException(nameof(media), item.Category, null)
            };
            var index = indexes.GetValueOrDefault(category);
            indexes[category] = index + 1;
            var type = item.Category switch
            {
                MediaCategory.Floppy or MediaCategory.Snapshot => EmulationMediaType.Floppy,
                MediaCategory.Cassette => EmulationMediaType.Cassette,
                MediaCategory.Cartridge => EmulationMediaType.Cartridge,
                _ => throw new ArgumentOutOfRangeException(nameof(media), item.Category, null)
            };
            return new EmulationMedia(Path.GetFullPath(item.Path),
                new EmulationMediaSlot(category, index), type, item.IsReadOnly, item.IsInserted);
        }).ToArray();
    }
}

internal static class EmulationMediaActivityFunctions
{
    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromLedStates(
        IReadOnlyDictionary<int, bool> leds) => new Dictionary<EmulationMediaSlot, bool>
    {
        [EmulationMediaSlot.Floppy0] = leds.GetValueOrDefault(0),
        [EmulationMediaSlot.Cassette0] = leds.GetValueOrDefault(1),
        [EmulationMediaSlot.Cartridge0] = leds.GetValueOrDefault(2)
    };
}
