namespace GWGUI.Emulation.Contracts;

/// <summary>Diagnostic complet échangé entre Common et le processus qui héberge un cœur.</summary>
/// <typeparam name="TCategory">Catégories décrites par le module ou par les contrats publics communs.</typeparam>
/// <typeparam name="TCode">Codes techniques correspondants, indépendants de la langue du message.</typeparam>
/// <param name="Type">Nom du type d’exception pour le diagnostic ; il ne provoque aucun chargement de type.</param>
/// <param name="Message">Texte de l’erreur transmis par Common, avec ses détails déjà collectés.</param>
/// <param name="Category">Catégorie structurée facultative ; null désigne une erreur générale.</param>
/// <param name="Code">Code structuré facultatif, interprété avec la catégorie.</param>
/// <param name="Context">Valeurs techniques nécessaires à la restitution du problème.</param>
/// <param name="IsLocalized">Indique que le texte est déjà traduit et doit être conservé.</param>
/// <remarks>Le module choisit ses types de codes sans inscription dans un registre du SDK.
/// Les propriétés ont les mêmes noms dans tous les transports. Le contexte reste emprunté et doit rester stable
/// pendant la sérialisation ; le codec ne conserve ni le diagnostic ni ses collections.</remarks>
public sealed record HostError<TCategory, TCode>(string Type, string Message, TCategory? Category, TCode? Code, IReadOnlyDictionary<string, string> Context, bool IsLocalized) where TCategory : struct where TCode : struct;
