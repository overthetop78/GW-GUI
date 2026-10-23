using CommonMediaConstants = GWGUI.Emulation.Bandai.Common.Constants.MediaConstants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class MediaRuntimeFunctions
{
    internal static void Register(List<MediaConfiguration> mountedMedia, MediaConfiguration media)
    {
        mountedMedia.RemoveAll(item => item.Slot == media.Slot);
        mountedMedia.Add(media);
    }

    /// <summary>Consulte les emplacements imposant une recréation dans les règles de base.</summary>
    /// <param name="slot">Emplacement logique à comparer aux données immuables du module.</param>
    /// <returns>True si cet emplacement figure dans RestartSlots ; false pour les autres emplacements.</returns>
    internal static bool RequiresMediaRestart(EmulationMediaSlot slot) => CommonMediaConstants.RestartSlots.Contains(slot);
}
