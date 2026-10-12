namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) => new(
        media.Path,
        media.Slot,
        media.Category switch
        {
            MediaCategory.CdRom => EmulationMediaType.CompactDisc,
            MediaCategory.MemoryCard => EmulationMediaType.MemoryCard,
            MediaCategory.Cartridge => EmulationMediaType.Cartridge,
            _ => throw new ArgumentOutOfRangeException(nameof(media))
        },
        media.IsReadOnly,
        media.IsInserted
    );
    internal static MediaConfiguration ToSnk(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var category = (media.Type, media.Slot.Category) switch
        {
            (EmulationMediaType.CompactDisc, EmulationMediaCategory.CompactDiscDrive) => MediaCategory.CdRom,
            (EmulationMediaType.MemoryCard, EmulationMediaCategory.MemoryCard) => MediaCategory.MemoryCard,
            (EmulationMediaType.Cartridge, EmulationMediaCategory.CartridgeSlot) => MediaCategory.Cartridge,
            _ => throw new ArgumentOutOfRangeException(nameof(media))
        };
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
}