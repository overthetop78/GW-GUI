namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) => new(media.Path, media.Slot, ToType(media.Category), media.IsReadOnly, media.IsInserted);

    internal static MediaConfiguration ToModule(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var category = ToCategory(media.Type);
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null
            ? new MediaConfiguration(media.Path, category, media.Slot, IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted)
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
        MediaCategory.Cartridge => EmulationMediaType.Cartridge,
        MediaCategory.Floppy => EmulationMediaType.Floppy,
        MediaCategory.DataPack => EmulationMediaType.Cassette,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };

    private static MediaCategory ToCategory(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Cartridge => MediaCategory.Cartridge,
        EmulationMediaType.Floppy => MediaCategory.Floppy,
        EmulationMediaType.Cassette => MediaCategory.DataPack,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
