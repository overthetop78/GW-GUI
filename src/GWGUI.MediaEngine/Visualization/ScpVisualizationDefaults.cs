namespace GWGUI.MediaEngine.Visualization;

/// <summary>Définit les valeurs de l'en-tête SCP synthétique produit pour la visualisation.</summary>
internal static class ScpVisualizationDefaults
{
    /// <summary>Version de l'en-tête synthétique.</summary>
    public const byte Version = 0;
    /// <summary>Type de disque non spécifié.</summary>
    public const byte DiskType = 0;
    /// <summary>Nombre de révolutions produites par piste.</summary>
    public const byte RevolutionCount = 1;
    /// <summary>Résolution SCP par défaut.</summary>
    public const byte Resolution = 0;
    /// <summary>Checksum initial de l'image synthétique.</summary>
    public const uint Checksum = 0;
}
