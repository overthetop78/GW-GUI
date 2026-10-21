namespace GWGUI.Emulation.Contracts;

/// <summary>Enveloppe des formats d’état historiques utilisant un modèle texte, les options et les empreintes des médias.</summary>
/// <remarks>Ce contrat préserve les fichiers existants ; il ne convertit pas leur identité vers celle d’un autre format.</remarks>
public sealed record EmulationLegacySavedStateHeader(
    int FormatVersion,
    string Model,
    string CoreSha256,
    IReadOnlyDictionary<string, string>? Options,
    IReadOnlyList<string> MediaSha256s,
    string StateSha256);
