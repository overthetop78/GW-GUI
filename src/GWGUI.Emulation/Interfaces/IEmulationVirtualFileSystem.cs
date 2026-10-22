namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire du système de fichiers virtuel proposé au cœur par ses callbacks natifs.</summary>
/// <remarks>Le propriétaire conserve les délégués, leur table native et les fichiers et répertoires ouverts
/// par le cœur. La session arrête les appels du cœur avant de libérer ce propriétaire dans son finally.
/// Dispose ferme les handles encore ouverts et libère la table, même si le cœur ne les a pas refermés.
/// Les callbacks fournis à la construction sont empruntés et ne sont pas détruits par ce service.</remarks>
public interface IEmulationVirtualFileSystem : IDisposable
{
    /// <summary>Négocie la version demandée et remplit la requête native avec la table de callbacks existante.</summary>
    /// <param name="data">Adresse empruntée d’une VfsInterfaceInfo valide et accessible en lecture et écriture pendant l’appel.</param>
    /// <returns>True si la table est fournie ; false pour une adresse nulle, une version non prise en charge ou un propriétaire libéré.</returns>
    /// <remarks>La requête appartient à l’appelant. La table retournée est un emprunt au propriétaire VFS :
    /// le cœur ne la libère pas et cesse de l’utiliser avant Dispose. Un refus laisse la requête inchangée.</remarks>
    bool Provide(nint data);
}
