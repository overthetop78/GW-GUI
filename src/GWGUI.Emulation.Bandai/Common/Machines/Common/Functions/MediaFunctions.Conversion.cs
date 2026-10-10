namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) =>
        new(media.Path, media.Slot, ToType(media.Category), media.IsReadOnly, media.IsInserted);

    internal static MediaConfiguration ToBandai(EmulationMedia media,
        IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var category = ToBandaiCategory(media.Type);
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
        MediaCategory.Cartridge => EmulationMediaType.Cartridge,
        MediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };

    private static MediaCategory ToBandaiCategory(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Cartridge => MediaCategory.Cartridge,
        EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
