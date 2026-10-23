namespace GWGUI.Emulation.Commodore.Common.Interfaces;

/// <summary>Réglages graphiques et résolution de sortie décrits par l’adaptateur.</summary>
/// <remarks>La résolution configurée est distincte des dimensions de la dernière frame calculée. Une réponse null laisse Common utiliser sa description matérielle sans inventer de dimensions pour le backend.</remarks>
internal interface IEmulatorVideoSettings
{
    /// <summary>Adapte les options du cœur au choix graphique transmis par l'hôte.</summary>
    /// <param name="configuration">Configuration empruntée, conservée immuable pendant l'adaptation.</param>
    /// <param name="graphicsApi">API graphique choisie pour la présentation ou le rendu matériel.</param>
    /// <returns>Configuration adaptée lorsque le cœur possède une conversion native ; sinon la même configuration.</returns>
    /// <remarks>Le présentateur de l'application conserve son propre choix. Un cœur produisant des images CPU
    /// ne reçoit aucune option graphique inventée. Les adaptations matérielles conservent leurs conversions,
    /// les valeurs natives compatibles et les éventuelles règles de redémarrage de leur cœur.</remarks>
    MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi);

    /// <summary>Résout la largeur et la hauteur de sortie pour les options courantes.</summary>
    /// <param name="configuration">Configuration empruntée à consulter.</param>
    /// <returns>Dimensions en pixels, ou null lorsque le cœur n’annonce pas de taille fixe.</returns>
    (int Width, int Height)? GetOutputResolution(MachineConfiguration configuration);
}
