using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Example.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Contrat Common de description et de projection des entrées propres à un adaptateur.</summary>
/// <remarks>Les signatures réunissent les descriptions par port et par périphérique. Chaque adaptateur
/// reçoit le même contexte ; ses règles conservent les dispositions propres au modèle et au protocole du cœur.
/// Les instantanés reçus sont empruntés : une projection retourne ses données sans modifier celles de l’appelant.</remarks>
internal interface IEmulatorInputMapper
{
    /// <summary>Décrit les périphériques émulés pris en charge pour le modèle.</summary>
    /// <param name="model">Modèle matériel dont les périphériques doivent être décrits.</param>
    /// <returns>Catégories proposées par cet adaptateur pour ce modèle, dans leur ordre de présentation.</returns>
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model);

    /// <summary>Décrit les commandes configurables d’un périphérique raccordé à un port.</summary>
    /// <param name="model">Modèle matériel concerné par la description.</param>
    /// <param name="port">Indice du port émulé, commençant à zéro ; les limites sont celles du modèle.</param>
    /// <param name="peripheral">Catégorie de périphérique sélectionnée pour ce port.</param>
    /// <returns>Actions accompagnées des libellés, associations initiales et touches émulées éventuelles.</returns>
    IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, int port, PeripheralCategory peripheral);

    /// <summary>Projette les sources physiques vers les entrées attendues par le cœur pour ce modèle.</summary>
    /// <param name="snapshot">Instantané physique stable pendant l’opération, conservé par son appelant.</param>
    /// <param name="configuration">Configuration complète donnant le modèle, les options du cœur, les associations et l’organisation des ports.</param>
    /// <returns>Instantané adapté au protocole du cœur, avec des collections stables pour leur consommation.</returns>
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);
}
