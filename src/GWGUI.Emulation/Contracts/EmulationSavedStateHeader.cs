namespace GWGUI.Emulation.Contracts;

/// <summary>En-tête complet commun à tous les modules : format, identité du cœur, modèle et empreintes SHA-256.</summary>
/// <remarks>Modèle et émulateur utilisent les identifiants typés du module, enum ou texte. Les empreintes de configuration, de contenu et d’état sont toutes vérifiées avant restauration. Aucun schéma historique distinct n’est utilisé.</remarks>
public sealed record EmulationSavedStateHeader<TModel, TCore>(
    int FormatVersion,
    TCore Core,
    string CoreName,
    string CoreVersion,
    string CoreSha256,
    TModel Model,
    string ConfigurationSha256,
    string ContentSha256,
    string StateSha256);
