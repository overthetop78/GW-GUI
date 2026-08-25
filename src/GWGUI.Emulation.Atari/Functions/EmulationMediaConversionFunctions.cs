using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia? ToCommon(AtariMediaConfiguration media) => media.Category == AtariMediaCategory.Directory
        ? null
        : new EmulationMedia(media.Path, media.Slot, ToType(media.Category), media.IsReadOnly, media.IsInserted);

    internal static AtariMediaConfiguration ToAtari(EmulationMedia media,
        IReadOnlyList<AtariMediaConfiguration> mountedMedia)
    {
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null
            ? new AtariMediaConfiguration(media.Path, ToAtariCategory(media.Type), media.Slot,
                IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted)
            : existing with
            {
                Path = media.Path,
                Category = ToAtariCategory(media.Type),
                IsReadOnly = media.IsReadOnly,
                IsInserted = media.IsInserted
            };
    }

    private static EmulationMediaType ToType(AtariMediaCategory media) => media switch
    {
        AtariMediaCategory.Floppy => EmulationMediaType.Floppy,
        AtariMediaCategory.HardDisk => EmulationMediaType.HardDisk,
        AtariMediaCategory.Cassette => EmulationMediaType.Cassette,
        AtariMediaCategory.Cartridge => EmulationMediaType.Cartridge,
        AtariMediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
    };

    private static AtariMediaCategory ToAtariCategory(EmulationMediaType media) => media switch
    {
        EmulationMediaType.Floppy => AtariMediaCategory.Floppy,
        EmulationMediaType.HardDisk => AtariMediaCategory.HardDisk,
        EmulationMediaType.Cassette => AtariMediaCategory.Cassette,
        EmulationMediaType.Cartridge => AtariMediaCategory.Cartridge,
        EmulationMediaType.CompactDisc => AtariMediaCategory.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
    };
}
