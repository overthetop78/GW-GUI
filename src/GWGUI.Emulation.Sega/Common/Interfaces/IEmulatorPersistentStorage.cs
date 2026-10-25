namespace GWGUI.Emulation.Sega.Common.Interfaces;

/// <summary>Préparation des emplacements persistants nécessaires au cœur.</summary>
/// <remarks>La préparation des sauvegardes et cartes mémoire est distincte des fichiers temporaires
/// de session. L’adaptateur respecte les chemins configurés et conserve les contenus utilisateur.</remarks>
internal interface IEmulatorPersistentStorage
{
    /// <summary>Prépare les dossiers et fichiers persistants requis par la configuration.</summary>
    /// <param name="configuration">Configuration empruntée décrivant les supports persistants.</param>
    /// <param name="directory">Répertoire persistant fourni par le propriétaire Common.</param>
    void EnsurePersistentStorage(MachineConfiguration configuration, string directory);
}
