using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Décrit les retours standard et projette les effets du cœur vers les associations physiques de l’hôte.</summary>
/// <remarks>Ce service sans état ne possède aucun cœur ni périphérique. Les collections et le callback reçus
/// restent empruntés et doivent être stables pendant chaque appel. Les résultats sont des instantanés indépendants.</remarks>
public interface IEmulationControllerFeedbackProcessing
{
    /// <summary>Décrit les moteurs fort et faible pour une catégorie de contrôleur du module.</summary>
    /// <typeparam name="TPeripheral">Énumération du module, sans enregistrement préalable auprès du SDK.</typeparam>
    /// <param name="peripheral">Catégorie déclarée ; None et Keyboard ne décrivent aucun moteur.</param>
    /// <returns>Définitions standard immuables, ou liste vide pour une catégorie invalide, désactivée ou clavier.</returns>
    IReadOnlyList<EmulationControllerOutputDefinition> DescribeOutputs<TPeripheral>(TPeripheral peripheral) where TPeripheral : struct, Enum;

    /// <summary>Conserve les associations valides des ports décrits et des commandes annoncées par leur adaptateur.</summary>
    /// <param name="ports">Ports publics numérotés à partir de un ; leurs effets utilisent des indices à partir de zéro.</param>
    /// <param name="describeOutputs">Décrit les commandes réellement acceptées par le backend pour ce port.</param>
    /// <returns>Associations par indice de port et commande, avec appareil non vide et moteur défini.</returns>
    IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> CreateBindings(IReadOnlyList<EmulationControllerPort> ports, Func<EmulationControllerPort, IReadOnlyList<EmulationControllerOutputDefinition>> describeOutputs);

    /// <summary>Projette les effets autorisés dont les intensités sont finies et comprises entre zéro et un.</summary>
    /// <param name="states">Instantané des effets produits par le cœur, sans consommation de ses valeurs.</param>
    /// <param name="bindings">Associations déjà contrôlées pour les entrées et le backend courants.</param>
    /// <returns>Sorties physiques à publier ; une liste vide arrête les sorties précédentes.</returns>
    IReadOnlyList<EmulationControllerOutputState> ResolveStates(IReadOnlyList<ControllerFeedbackState> states, IReadOnlyDictionary<(int Port, string CommandId), EmulationControllerOutputBinding> bindings);
}
