using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Watara.Common.Interfaces;

/// <summary>Description des sorties de contrôleur que le cœur peut produire.</summary>
/// <remarks>Les descriptions de vibrations et autres retours restent distinctes des commandes d’entrée.
/// L’hôte possède les périphériques physiques auxquels ces sorties seront appliquées.</remarks>
internal interface IEmulatorControllerFeedbackAdapter
{
    /// <summary>Décrit les sorties disponibles pour le contrôleur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée du modèle et de ses ports.</param>
    /// <param name="peripheral">Catégorie du contrôleur émulé, identique à celle des contrats d’entrée et de ports.</param>
    /// <returns>Sorties prises en charge, ou liste vide si le cœur n’en fournit aucune.</returns>
    IReadOnlyList<EmulationControllerOutputDefinition> GetControllerOutputs(MachineConfiguration configuration, PeripheralCategory peripheral);
}
