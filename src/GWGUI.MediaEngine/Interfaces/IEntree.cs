namespace GWGUI.MediaEngine.Exploration.Contracts;

/// <summary>Entrée réelle d'un catalogue de fichiers.</summary>
public interface IEntree
{
    string Nom { get; }
    string Type { get; }
    string? TypeNatifId { get; }
    long Taille { get; }
    long? TailleOccupee { get; }
    DateTimeOffset? Creation { get; }
    DateTimeOffset? Modification { get; }
    DateTimeOffset? Acces { get; }
    string? Commentaire { get; }
    IReadOnlyList<string> Attributs { get; }
    uint? AttributsBruts { get; }
    long? ReferenceStockage { get; }
    bool MetadonneesValides { get; }
    bool? DonneesValides { get; }
    bool NomSynthetique { get; }
    string? CibleLien { get; }
    ReadOnlyMemory<byte>? Donnees { get; }
    IReadOnlyList<IEntree> Enfants { get; }
    IReadOnlyList<IDiagnostic> Diagnostics { get; }
}
