namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Effet demandé par un cœur pour une commande d’un port émulé.</summary>
/// <param name="Port">Indice du port émulé, à partir de zéro.</param>
/// <param name="CommandId">Identifiant commun d’effet, associé à un moteur physique par la configuration.</param>
/// <param name="Strength">Intensité normalisée entre zéro et un ; zéro arrête cet effet.</param>
/// <remarks>Ce contrat ne commande aucun périphérique physique. La machine vérifie l’intensité et
/// les associations avant de publier les sorties attendues par l’hôte.</remarks>
internal sealed record ControllerFeedbackState(int Port, string CommandId, float Strength);
