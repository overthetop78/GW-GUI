namespace GWGUI.Emulation.Watara.Common.Interfaces;

/// <summary>Résolution matérielle de sortie décrite par l’adaptateur.</summary>
/// <remarks>La résolution configurée est distincte des dimensions de la dernière frame calculée. Une réponse null laisse Common utiliser sa description matérielle sans inventer de dimensions pour le backend.</remarks>
internal interface IEmulatorVideoSettings
{
    /// <summary>Résout la largeur et la hauteur de sortie pour les options courantes.</summary>
    /// <param name="configuration">Configuration empruntée à consulter.</param>
    /// <returns>Dimensions en pixels, ou null lorsque le cœur n’annonce pas de taille fixe.</returns>
    (int Width, int Height)? GetOutputResolution(MachineConfiguration configuration);
}
