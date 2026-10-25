namespace GWGUI.Emulation.Interfaces;

/// <summary>Nettoyage des dossiers de session possédés par une machine.</summary>
/// <remarks>À appeler après arrêt et destruction effective du cœur, dans le finally du propriétaire.
/// Un dossier emprunté ou conservant des données utilisateur ne doit pas être confié à ces opérations.</remarks>
public interface IEmulationSessionDirectories
{
    /// <summary>Supprime la session avec ses règles existantes de chemin et de tolérance aux erreurs d’accès.</summary>
    void DeleteSessionDirectory(string sessionDirectory);

    /// <summary>Supprime une session générée dont le nom est un GUID, après retrait des attributs lecture seule.</summary>
    void DeleteGeneratedSessionDirectory(string sessionDirectory);
}
