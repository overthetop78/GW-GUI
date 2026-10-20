using GWGUI.Emulation;

namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class MediaRuntimeFunctions
{
    internal static void Register(List<MediaConfiguration> mountedMedia, MediaConfiguration media)
    {
        mountedMedia.RemoveAll(item => item.Slot == media.Slot);
        mountedMedia.Add(media);
    }
}

