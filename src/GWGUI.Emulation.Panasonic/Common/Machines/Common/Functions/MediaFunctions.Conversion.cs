namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media)
    {
        if (media.Category != EmulationMediaType.CompactDisc || media.Slot != EmulationMediaSlot.Cd0) throw new ArgumentOutOfRangeException(nameof(media));
        return new(media.Path, media.Slot, EmulationMediaType.CompactDisc, media.IsReadOnly, media.IsInserted);
    }
    internal static MediaConfiguration ToModule(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        if (media.Type != EmulationMediaType.CompactDisc || media.Slot != EmulationMediaSlot.Cd0) throw new ArgumentOutOfRangeException(nameof(media));
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null ? new(media.Path, media.Type, media.Slot, IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted) : existing with { Path = media.Path, IsReadOnly = media.IsReadOnly, IsInserted = media.IsInserted };
    }
}
