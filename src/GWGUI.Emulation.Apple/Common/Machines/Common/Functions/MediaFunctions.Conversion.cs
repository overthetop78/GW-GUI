namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) =>
        new(media.Path, media.Slot, ToType(media.Category), media.IsReadOnly, media.IsInserted);

    internal static MediaConfiguration ToApple(EmulationMedia media,
        IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var category = ToAppleCategory(media.Type);
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null
            ? new MediaConfiguration(media.Path, category, media.Slot,
                IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted)
            : existing with
            {
                Path = media.Path,
                Category = category,
                IsReadOnly = media.IsReadOnly,
                IsInserted = media.IsInserted
            };
    }

    private static EmulationMediaType ToType(MediaCategory category) => category switch
    {
        MediaCategory.Floppy => EmulationMediaType.Floppy,
        MediaCategory.Cassette => EmulationMediaType.Cassette,
        MediaCategory.HardDisk => EmulationMediaType.HardDisk,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };

    private static MediaCategory ToAppleCategory(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Floppy => MediaCategory.Floppy,
        EmulationMediaType.Cassette => MediaCategory.Cassette,
        EmulationMediaType.HardDisk => MediaCategory.HardDisk,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
