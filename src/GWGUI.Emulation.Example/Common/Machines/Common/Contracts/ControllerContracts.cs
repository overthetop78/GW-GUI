namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Association d’un port émulé à ses sources physiques et à ses commandes.</summary>
/// <param name="Port">Indice technique du port, à partir de zéro.</param>
/// <param name="Peripheral">Catégorie générique sélectionnée pour ce port.</param>
/// <param name="DeviceId">Identité du périphérique physique, ou null pour la sélection commune.</param>
/// <param name="Mappings">Expressions physiques indexées par commande émulée.</param>
/// <param name="DeadZonePercent">Zone morte analogique exprimée en pourcentage.</param>
/// <param name="VisualId">Identifiant de présentation transmis à l’hôte, qui possède les images.</param>
/// <param name="OutputBindings">Associations des commandes d’effets aux périphériques physiques et à leurs moteurs.</param>
public sealed record ControllerBinding(int Port, PeripheralCategory Peripheral, string? DeviceId = null, IReadOnlyDictionary<string, string>? Mappings = null, int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent, string? VisualId = null, IReadOnlyDictionary<string, EmulationControllerOutputBinding>? OutputBindings = null);
