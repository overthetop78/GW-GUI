using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class MediaRuntimeFunctions
{
    internal static void Register(List<MediaConfiguration> mountedMedia, MediaConfiguration media)
    {
        mountedMedia.RemoveAll(item => item.Slot == media.Slot);
        mountedMedia.Add(media);
    }

    internal static void MarkEjected(List<MediaConfiguration> mountedMedia, EmulationMediaSlot slot)
    {
        var index = mountedMedia.FindIndex(item => item.Slot == slot);
        if (index >= CommonConstants.FirstCollectionIndex)
            mountedMedia[index] = mountedMedia[index] with { IsInserted = false };
    }
}
