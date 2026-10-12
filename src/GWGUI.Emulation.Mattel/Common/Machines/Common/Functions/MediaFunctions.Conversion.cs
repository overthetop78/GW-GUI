namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) =>
        new(media.Path, media.Slot, media.Category switch
        {
            MediaCategory.Cartridge => EmulationMediaType.Cartridge,
            _ => throw new ArgumentOutOfRangeException(nameof(media))
        }, media.IsReadOnly, media.IsInserted);

    internal static MediaConfiguration ToMattel(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var category = (media.Type, media.Slot) switch
        {
            (EmulationMediaType.Cartridge, var slot) when slot == EmulationMediaSlot.Cartridge0 => MediaCategory.Cartridge,
            _ => throw new ArgumentOutOfRangeException(nameof(media))
        };
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null
            ? new MediaConfiguration(media.Path, category, media.Slot, IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted)
            : existing with { Path = media.Path, Category = category, IsReadOnly = media.IsReadOnly, IsInserted = media.IsInserted };
    }
}
