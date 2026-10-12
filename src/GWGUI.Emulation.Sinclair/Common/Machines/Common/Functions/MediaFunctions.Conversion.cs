namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;
internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) => new(
        media.Path,
        media.Slot,
        media.Category switch
        {
            MediaCategory.Tape => EmulationMediaType.Cassette,
            MediaCategory.Floppy => EmulationMediaType.Floppy,
            MediaCategory.Cartridge => EmulationMediaType.Cartridge,
            _ => throw new ArgumentOutOfRangeException(nameof(media))
        },
        media.IsReadOnly,
        media.IsInserted
    );
    internal static MediaConfiguration ToSinclair(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var category = (media.Type, media.Slot.Category) switch
        {
            (EmulationMediaType.Cassette, EmulationMediaCategory.CassetteDrive) => MediaCategory.Tape,
            (EmulationMediaType.Floppy, EmulationMediaCategory.FloppyDrive) => MediaCategory.Floppy,
            (EmulationMediaType.Cartridge, EmulationMediaCategory.CartridgeSlot) => MediaCategory.Cartridge,
            _ => throw new ArgumentOutOfRangeException(nameof(media))};
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