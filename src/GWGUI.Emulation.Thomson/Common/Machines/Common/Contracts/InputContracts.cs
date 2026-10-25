using System.Text.Json.Serialization;

using GWGUI.Emulation.Thomson.Common.Machines.Common.Constants;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;

/// <summary>Associations persistantes des entrées du module, décrites sous les mêmes noms que dans Example.</summary>
/// <remarks>Les données restent empruntées par les projections. Les noms JSON existants sont conservés par les attributs du contrat.</remarks>
public sealed record InputConfiguration([property: JsonPropertyName("keyboard")] IReadOnlyDictionary<string, string>? KeyboardBindings = null, IReadOnlyList<ControllerBinding>? Controllers = null, [property: JsonPropertyName("mouse")] IReadOnlyDictionary<string, string>? MouseMappings = null, int MouseSpeedPercent = InputMappingConstants.DefaultMouseSpeedPercent);
