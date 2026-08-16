using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariMediaRuntimeFunctions
{
    internal static void Register(List<AtariMediaConfiguration> mountedMedia, AtariMediaConfiguration media)
    {
        mountedMedia.RemoveAll(item => item.Slot == media.Slot);
        mountedMedia.Add(media);
    }

    internal static void MarkEjected(List<AtariMediaConfiguration> mountedMedia, EmulationMediaSlot slot)
    {
        var index = mountedMedia.FindIndex(item => item.Slot == slot);
        if (index >= AtariConstants.FirstCollectionIndex)
            mountedMedia[index] = mountedMedia[index] with { IsInserted = false };
    }
}
