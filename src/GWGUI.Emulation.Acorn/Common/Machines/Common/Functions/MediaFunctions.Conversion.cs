namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) => new(media.Path, media.Slot, ToType(media.Category), media.IsReadOnly, media.IsInserted) { Parameters = media.Parameters };

    internal static MediaConfiguration ToModule(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var category = ToCategory(media.Type);
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null
            ? new MediaConfiguration(media.Path, category, media.Slot, IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted, Parameters: media.Parameters)
            : existing with
            {
                Path = media.Path,
                Category = category,
                IsReadOnly = media.IsReadOnly,
                IsInserted = media.IsInserted,
                Parameters = media.Parameters
            };
    }

    private static EmulationMediaType ToType(MediaCategory category) => category switch
    {
        MediaCategory.Floppy => EmulationMediaType.Floppy,
        MediaCategory.Cassette => EmulationMediaType.Cassette,
        MediaCategory.Cartridge => EmulationMediaType.Cartridge,
        MediaCategory.HardDisk => EmulationMediaType.HardDisk,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };

    private static MediaCategory ToCategory(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Floppy => MediaCategory.Floppy,
        EmulationMediaType.Cassette => MediaCategory.Cassette,
        EmulationMediaType.Cartridge => MediaCategory.Cartridge,
        EmulationMediaType.HardDisk => MediaCategory.HardDisk,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
