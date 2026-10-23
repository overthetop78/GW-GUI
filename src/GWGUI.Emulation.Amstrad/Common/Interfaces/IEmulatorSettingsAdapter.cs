namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

/// <summary>Options natives utilisées par le contrat commun de description des réglages.</summary>
/// <remarks>Les services fournis restent sous la responsabilité de leur appelant.</remarks>
internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    /// <summary>Lit les descriptions d’options avec les services de gestion fournis.</summary>
    /// <param name="configuration">Configuration facultative, suivant le contrat commun des adaptateurs.</param>
    /// <param name="context">Contexte emprunté de l’installation du cœur ; null conserve le catalogue vide du parcours sans contexte.</param>
    /// <returns>Descriptions natives utilisables pour présenter ses réglages.</returns>
    IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null);
}
