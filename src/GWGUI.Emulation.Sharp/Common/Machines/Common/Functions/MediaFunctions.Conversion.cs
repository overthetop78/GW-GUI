namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) => new(media.Path, media.Slot, media.Category switch
    { MediaCategory.Floppy => EmulationMediaType.Floppy, MediaCategory.HardDisk => EmulationMediaType.HardDisk, _ => throw new ArgumentOutOfRangeException(nameof(media))
    }, media.IsReadOnly, media.IsInserted);

    internal static MediaConfiguration ToModule(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var category = (media.Type, media.Slot.Category) switch
        { (EmulationMediaType.Floppy, EmulationMediaCategory.FloppyDrive) => MediaCategory.Floppy, (EmulationMediaType.HardDisk, EmulationMediaCategory.HardDisk) when media.Slot == EmulationMediaSlot.HardDisk0 => MediaCategory.HardDisk, _ => throw new ArgumentOutOfRangeException(nameof(media))
        };
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null ? new MediaConfiguration(media.Path, category, media.Slot, IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted) : existing with { Path = media.Path, Category = category, IsReadOnly = media.IsReadOnly, IsInserted = media.IsInserted };
    }
}
