namespace GWGUI.Emulation.Sharp.Common.Interfaces;

/// <summary>Description des blocs de réglages fournis par un adaptateur pour une configuration.</summary>
/// <remarks>Common assemble ces blocs avec les autres réglages de la machine. Le contrat retourne des données
/// de présentation et ne possède aucune session, sortie audio ou surface vidéo.</remarks>
internal interface IEmulatorSettings
{
    /// <summary>Décrit les réglages du backend avec les valeurs courantes de la configuration.</summary>
    /// <param name="configuration">Modèle, options et périphériques empruntés à présenter sans les modifier.</param>
    /// <param name="context">Services de gestion empruntés pour résoudre les options ou métadonnées qui en dépendent ; null si ces services ne sont pas nécessaires.</param>
    /// <returns>Blocs et éditeurs du backend, avec leurs clés de ressources et leurs valeurs courantes.</returns>
    /// <remarks>Les options natives sont résolues par les contrats complémentaires de l’adaptateur lorsqu’il en expose.</remarks>
    IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null);
}