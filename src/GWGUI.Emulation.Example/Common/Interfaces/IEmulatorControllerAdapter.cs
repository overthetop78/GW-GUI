using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Choix et description des contrôleurs suivant le modèle, les options et le port du cœur.</summary>
/// <remarks>Les noms GetControllerTypes et ControllerPortCount des contrats existants sont réunis sous
/// GetPeripherals et GetControllerPortCount. Les surcharges ajoutent les contraintes de configuration
/// et de port à la description par modèle ; elles conservent les mêmes catégories de périphériques.</remarks>
internal interface IEmulatorControllerAdapter : IEmulatorInputMapper
{
    /// <summary>Décrit les périphériques possibles pour les options courantes de la machine.</summary>
    /// <param name="configuration">Configuration empruntée fournissant modèle et options.</param>
    /// <returns>Choix du modèle par défaut ; un backend peut les restreindre ou les compléter selon ses options.</returns>
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration) => GetPeripherals(configuration.Model);

    /// <summary>Décrit les choix autorisés sur un port particulier.</summary>
    /// <param name="configuration">Configuration de machine dont les choix sont demandés.</param>
    /// <param name="port">Indice du port émulé, commençant à zéro.</param>
    /// <returns>Choix de la configuration par défaut, ou liste spécifique fournie par l’adaptateur.</returns>
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration, int port) => GetPeripherals(configuration);

    /// <summary>Décrit les périphériques raccordables à un port USB émulé.</summary>
    /// <param name="configuration">Configuration du modèle et de son bus USB.</param>
    /// <param name="port">Indice propre au bus USB, distinct des indices de ports de manettes.</param>
    /// <returns>Choix désactivé par défaut ; les adaptateurs disposant de ce bus fournissent leurs autres choix.</returns>
    IReadOnlyList<PeripheralCategory> GetUsbPeripherals(MachineConfiguration configuration, int port) => [PeripheralCategory.None];

    /// <summary>Calcule les ports disponibles après application des options et extensions de contrôleurs.</summary>
    /// <param name="configuration">Configuration pouvant notamment activer un multitap.</param>
    /// <returns>Quantité de ports, distincte du dernier indice et de la capacité maximale du transport.</returns>
    int GetControllerPortCount(MachineConfiguration configuration);

    /// <summary>Fournit un nom de port propre au matériel lorsque la présentation générique ne suffit pas.</summary>
    /// <param name="configuration">Configuration déterminant l’organisation des ports.</param>
    /// <param name="port">Indice technique du port.</param>
    /// <returns>Nom explicite, ou null pour laisser la présentation commune.</returns>
    string? GetControllerPortDisplayName(MachineConfiguration configuration, int port) => null;

    /// <summary>Fournit le numéro de présentation lorsque celui-ci diffère de l’ordre technique.</summary>
    /// <param name="configuration">Configuration déterminant l’organisation des ports.</param>
    /// <param name="port">Indice technique du port.</param>
    /// <returns>Numéro à présenter, ou null pour employer la numérotation commune.</returns>
    int? GetControllerPortDisplayNumber(MachineConfiguration configuration, int port) => null;

    /// <summary>Fournit des commandes particulières pour un périphérique et un port.</summary>
    /// <param name="configuration">Configuration dont le modèle et les options sont pris en compte.</param>
    /// <param name="peripheral">Périphérique émulé choisi.</param>
    /// <param name="port">Indice technique du port.</param>
    /// <returns>Définitions spécifiques, ou null pour utiliser les descriptions ordinaires des actions.</returns>
    IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, PeripheralCategory peripheral, int port) => null;
}
