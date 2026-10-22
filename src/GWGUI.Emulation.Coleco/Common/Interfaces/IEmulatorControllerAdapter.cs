namespace GWGUI.Emulation.Coleco.Common.Interfaces;

/// <summary>Choix et description des contrôleurs, distincts de la projection des instantanés physiques.</summary>
/// <remarks>Les adaptateurs conservent leurs règles de modèle, de périphérique et de port ; le contrat hérité assure leur raccordement au runtime.</remarks>
internal interface IEmulatorControllerAdapter : IEmulatorInputMapper
{
    /// <summary>Décrit les catégories de périphériques autorisées par cet adaptateur.</summary>
    /// <param name="model">Modèle matériel dont les périphériques sont consultés.</param>
    /// <returns>Choix existants du backend dans leur ordre de présentation.</returns>
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model);
    /// <summary>Décrit les commandes et associations proposées pour le périphérique émulé.</summary>
    /// <param name="model">Modèle matériel dont les commandes sont décrites.</param>
    /// <param name="peripheral">Catégorie du périphérique émulé à décrire.</param>
    /// <param name="port">Indice du port émulé, commençant à zéro.</param>
    /// <returns>Descriptions existantes du backend, conservées dans leur ordre de présentation.</returns>
    IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, int port, PeripheralCategory peripheral);
}
