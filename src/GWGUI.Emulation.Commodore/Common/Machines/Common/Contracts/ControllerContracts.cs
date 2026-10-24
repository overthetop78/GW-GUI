using System.Text.Json.Serialization;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;

/// <summary>Association d’un port émulé à un périphérique et à ses commandes physiques.</summary>
/// <param name="Port">Indice du port émulé, à partir de zéro.</param>
/// <param name="Peripheral">Catégorie du périphérique sélectionné dans le catalogue du module.</param>
/// <param name="DeviceId">Identité physique, ou null pour la sélection commune de la source.</param>
/// <param name="Mappings">Expressions de sources indexées par commande du périphérique émulé.</param>
/// <param name="VisualId">Identité de présentation transmise à l’hôte propriétaire des visuels.</param>
/// <param name="OutputBindings">Associations des effets du cœur aux sorties physiques de l’hôte.</param>
/// <remarks>Les membres portent les mêmes noms que ceux des autres Common. Les noms JSON restent ceux
/// du document de configuration du module ; la sérialisation ne crée ni contrat ni lecteur supplémentaire.</remarks>
public sealed record ControllerBinding(int Port, [property: JsonPropertyName("type")] PeripheralCategory Peripheral, string? DeviceId = null, [property: JsonPropertyName("buttonMappings")] IReadOnlyDictionary<string, string>? Mappings = null, string? VisualId = null, IReadOnlyDictionary<string, EmulationControllerOutputBinding>? OutputBindings = null);
