using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia? ToCommon(MediaConfiguration media) => media.Category == MediaCategory.Directory
        ? null
        : new EmulationMedia(media.Path, media.Slot, ToType(media.Category), media.IsReadOnly, media.IsInserted);

    internal static MediaConfiguration ToAtari(EmulationMedia media,
        IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null
            ? new MediaConfiguration(media.Path, ToAtariCategory(media.Type), media.Slot,
                IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted)
            : existing with
            {
                Path = media.Path,
                Category = ToAtariCategory(media.Type),
                IsReadOnly = media.IsReadOnly,
                IsInserted = media.IsInserted
            };
    }

    private static EmulationMediaType ToType(MediaCategory media) => media switch
    {
        MediaCategory.Floppy => EmulationMediaType.Floppy,
        MediaCategory.HardDisk => EmulationMediaType.HardDisk,
        MediaCategory.Cassette => EmulationMediaType.Cassette,
        MediaCategory.Cartridge => EmulationMediaType.Cartridge,
        MediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
    };

    private static MediaCategory ToAtariCategory(EmulationMediaType media) => media switch
    {
        EmulationMediaType.Floppy => MediaCategory.Floppy,
        EmulationMediaType.HardDisk => MediaCategory.HardDisk,
        EmulationMediaType.Cassette => MediaCategory.Cassette,
        EmulationMediaType.Cartridge => MediaCategory.Cartridge,
        EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(media), media, null)
    };
}
