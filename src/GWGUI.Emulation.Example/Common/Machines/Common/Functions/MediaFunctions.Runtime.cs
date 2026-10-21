using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Mise à jour de la liste des montages détenue par une machine en cours de session.</summary>
internal static class MediaRuntimeFunctions
{
    /// <summary>Remplace les montages du même slot puis ajoute le média reçu en fin de liste.</summary>
    /// <param name="mountedMedia">Liste mutable possédée par la machine ; l’appelant sérialise l’accès pendant cette mutation.</param>
    /// <param name="media">Description du montage à enregistrer dans la liste.</param>
    /// <remarks>Cette méthode ne monte pas le contenu dans le cœur et ne modifie pas le fichier média.</remarks>
    internal static void Register(List<MediaConfiguration> mountedMedia, MediaConfiguration media)
    {
        mountedMedia.RemoveAll(item => item.Slot == media.Slot);
        mountedMedia.Add(media);
    }
}
