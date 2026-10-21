namespace GWGUI.Emulation.Contracts;

/// <summary>Enveloppe existante d’un état, identifiant son format, son cœur, sa machine et ses empreintes SHA-256.</summary>
/// <remarks>Les noms des propriétés font partie du format JSON conservé. Les empreintes configuration, contenu et état sont distinctes ; elles doivent toutes être vérifiées avant restauration.</remarks>
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
