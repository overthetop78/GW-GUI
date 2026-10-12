namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) => new(media.Path, media.Slot, EmulationMediaType.Cartridge, media.IsReadOnly, media.IsInserted);

    internal static MediaConfiguration ToFairchild(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        if (media.Type != EmulationMediaType.Cartridge || media.Slot != EmulationMediaSlot.Cartridge0)
            throw new ArgumentOutOfRangeException(nameof(media));
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null ? new MediaConfiguration(media.Path, MediaCategory.Cartridge, media.Slot, IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted) : existing with { Path = media.Path, IsReadOnly = media.IsReadOnly, IsInserted = media.IsInserted };
    }
}
