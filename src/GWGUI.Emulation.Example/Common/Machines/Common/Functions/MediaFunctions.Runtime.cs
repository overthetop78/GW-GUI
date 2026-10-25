using CommonMediaConstants = GWGUI.Emulation.Example.Common.Constants.MediaConstants;
namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Tenue des montages pendant les commandes sérialisées de la machine.</summary>
/// <remarks>La liste appartient à la machine et est modifiée uniquement dans son parcours de commandes.</remarks>
internal static class MediaRuntimeFunctions
{
    /// <summary>Remplace le montage d’un lecteur par la nouvelle description.</summary>
    /// <param name="mountedMedia">Liste possédée par la machine appelante.</param>
    /// <param name="media">Description à enregistrer après une insertion réussie.</param>
    internal static void Register(List<MediaConfiguration> mountedMedia, MediaConfiguration media)
    {
        mountedMedia.RemoveAll(item => item.Slot == media.Slot);
        mountedMedia.Add(media);
    }

    /// <summary>Marque un contenu éjecté tout en conservant son chemin et ses réglages.</summary>
    /// <param name="mountedMedia">Liste possédée par la machine appelante.</param>
    /// <param name="slot">Lecteur dont l’éjection a réussi.</param>
    internal static void MarkEjected(List<MediaConfiguration> mountedMedia, EmulationMediaSlot slot)
    {
        var index = mountedMedia.FindIndex(item => item.Slot == slot);
        if (index >= 0) mountedMedia[index] = mountedMedia[index] with { IsInserted = false };
    }

    /// <summary>Consulte les emplacements imposant une recréation dans les règles de base.</summary>
    /// <param name="slot">Emplacement logique à comparer aux données immuables du module.</param>
    /// <returns>True si cet emplacement figure dans RestartSlots ; false pour les autres emplacements.</returns>
    internal static bool RequiresMediaRestart(EmulationMediaSlot slot) => CommonMediaConstants.RestartSlots.Contains(slot);
}
