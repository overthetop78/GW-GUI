namespace GWGUI.Emulation.Contracts;

/// <summary>Erreur de transport avec codes typés, contexte et indication de localisation déjà effectuée.</summary>
/// <remarks>Le module fournit ses catégories et codes sans déclaration dans une liste centrale. Contexte et marqueur IsLocalized conservent leur rôle et leur forme sérialisée.</remarks>
public sealed record HostError<TCategory, TCode>(
    string Type,
    string Message,
    TCategory? Category,
    TCode? Code,
    IReadOnlyDictionary<string, string> Context,
    bool IsLocalized)
    where TCategory : struct
    where TCode : struct;
