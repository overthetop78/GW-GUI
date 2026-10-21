namespace GWGUI.Emulation.Contracts;

/// <summary>Erreur transportée entre hôte et module, avec catégorie et code communs facultatifs.</summary>
/// <remarks>Les noms de propriétés appartiennent au protocole JSON existant ; le message ne remplace pas les identifiants structurés.</remarks>
public sealed record HostError(string Type, string Message, EmulationMessageCategory? Category, EmulationMessageCode? Code);
