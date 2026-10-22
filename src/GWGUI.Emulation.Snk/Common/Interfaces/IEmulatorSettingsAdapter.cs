namespace GWGUI.Emulation.Snk.Common.Interfaces;

internal interface IEmulatorSettingsAdapter
{
    /// <summary>Fournit les descriptions d’options par le même contrat que les autres adaptateurs.</summary>
    /// <param name="configuration">Configuration facultative permettant de filtrer les options propres au modèle.</param>
    /// <param name="context">Services de gestion empruntés lorsque le catalogue exige un cœur installé.</param>
    /// <returns>Catalogue existant, conservé dans son ordre et avec ses règles de visibilité.</returns>
    IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null);
    bool IsDeferredOption(MachineConfiguration configuration, string key);
}
