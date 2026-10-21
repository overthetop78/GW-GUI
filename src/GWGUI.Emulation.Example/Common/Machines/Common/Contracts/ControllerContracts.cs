namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Association d’un port émulé avec un périphérique physique et ses commandes.</summary>
/// <param name="Port">Indice du port émulé, commençant à zéro et validé selon les capacités du modèle.</param>
/// <param name="Peripheral">Catégorie de périphérique émulé, incluant les choix automatique et désactivé.</param>
/// <param name="DeviceId">Identité du périphérique physique préféré ; null laisse sa résolution aux fonctions d’entrée.</param>
/// <param name="Mappings">Associations entre identifiants d’actions et expressions d’entrée ; null laisse les valeurs initiales au module.</param>
/// <param name="DeadZonePercent">Portion neutralisée de la course analogique, en pourcentage entier.</param>
/// <param name="VisualId">Identité facultative du visuel de contrôleur à présenter dans l’éditeur.</param>
/// <remarks>Ce contrat ne possède pas le périphérique physique. Le dictionnaire reçu n’est pas copié par le record
/// et doit rester stable ; la validation de configuration contrôle le port, la catégorie et la zone morte.</remarks>
public sealed record ControllerBinding(int Port, PeripheralCategory Peripheral, string? DeviceId = null, IReadOnlyDictionary<string, string>? Mappings = null,
    int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent, string? VisualId = null);


