namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

/// <summary>Choix des contrôleurs et projection de leurs entrées par le même adaptateur Common.</summary>
/// <remarks>Le contrat hérité transmet l’instantané et la configuration complète au runtime.
/// La description conserve les catégories propres au modèle et leur ordre de présentation.</remarks>
internal interface IEmulatorControllerAdapter : IEmulatorInputMapper
{
    /// <summary>Décrit les catégories de périphériques autorisées par cet adaptateur pour le modèle.</summary>
    /// <param name="model">Modèle matériel dont les contrôleurs sont consultés.</param>
    /// <returns>Catégories existantes du backend, dans leur ordre de présentation.</returns>
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model);
}
