namespace GWGUI.Emulation.Sony.Common.Interfaces;

/// <summary>Formats de contenu annoncés par l’adaptateur de cœur.</summary>
/// <remarks>Ces extensions décrivent les contenus acceptés ; les lecteurs, cartes mémoire et chemins
/// de stockage restent décrits par leurs contrats Common respectifs. Leur consultation ne démarre aucun cœur.</remarks>
internal interface IEmulatorContentFormats
{
    /// <summary>Extensions acceptées par le backend, dans leur ordre de catalogue.</summary>
    IReadOnlyList<string> SupportedContentExtensions { get; }
}
