namespace GWGUI.Emulation.Sharp.Common.Interfaces;

/// <summary>Choix et description des contrôleurs, distincts de la projection des instantanés physiques.</summary>
/// <remarks>Les adaptateurs conservent leurs règles de modèle, de périphérique et de port ; le contrat hérité assure leur raccordement au runtime.</remarks>
internal interface IEmulatorControllerAdapter : IEmulatorInputMapper
{
    /// <summary>Décrit les catégories de périphériques autorisées par cet adaptateur.</summary>
    /// <param name="model">Modèle matériel dont les contrôleurs sont consultés.</param>
    /// <returns>Descriptions existantes du backend, conservées dans leur ordre de présentation.</returns>
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model);
}
