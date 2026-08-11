namespace GWGUI.MediaEngine.Definitions;

/// <summary>Définit et construit les identifiants des images sectorielles issues d'un conteneur 86F.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Préfixe des identifiants de repli 86F.</summary>
    public const string I86fPrefix = "86f.";

    /// <summary>Construit un identifiant 86F à partir de la géométrie sectorielle observée.</summary>
    /// <param name="sectorSize">Taille d'un secteur, en octets.</param>
    /// <param name="cylinders">Nombre de cylindres.</param>
    /// <param name="heads">Nombre de faces.</param>
    /// <param name="sectorsPerTrack">Nombre de secteurs par piste.</param>
    /// <returns>L'identifiant de repli décrivant la géométrie.</returns>
    public static string I86fFromGeometry(int sectorSize, int cylinders, int heads, int sectorsPerTrack) => $"{I86fPrefix}{sectorSize}.{cylinders}.{heads}.{sectorsPerTrack}";
}
