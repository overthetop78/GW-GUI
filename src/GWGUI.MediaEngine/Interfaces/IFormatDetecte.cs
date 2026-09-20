namespace GWGUI.MediaEngine.Exploration.Contracts;

/// <summary>Interprétation complète d'un couple machine et format reconnu.</summary>
public interface IFormatDetecte
{
    string MachineId { get; }
    string FormatId { get; }
    string Encodage { get; }
    int Cylindres { get; }
    int Faces { get; }
    int? SecteursParPiste { get; }
    int? TailleSecteur { get; }
    long CapaciteOctets { get; }
    int NombreSecteursValides { get; }
    int NombreSecteursInvalides { get; }
    int NombreSecteursAbsents { get; }
    IReadOnlyList<ISecteur> Secteurs { get; }
    string? SystemeFichiers { get; }
    string? NomVolume { get; }
    long? CapaciteVolume { get; }
    long? EspaceUtilise { get; }
    long? EspaceLibre { get; }
    DateTimeOffset? CreationVolume { get; }
    DateTimeOffset? ModificationVolume { get; }
    IReadOnlyList<string> AttributsVolume { get; }
    bool? Amorcable { get; }
    int? NumeroDisque { get; }
    int? NombreDisques { get; }
    string? OrigineNumeroDisque { get; }
    int NombreEntrees { get; }
    string? Organisation { get; }
    string? Chargeur { get; }
    IReadOnlyList<string> Compactages { get; }
    string? Crack { get; }
    string? Protection { get; }
    IReadOnlyList<IEntree> Entrees { get; }
    IReadOnlyList<IDiagnostic> Diagnostics { get; }
}

/// <summary>Secteur final d'une interprétation reconnue.</summary>
public interface ISecteur
{
    int BlocLogique { get; }
    int Cylindre { get; }
    int Face { get; }
    int Numero { get; }
    int Taille { get; }
    string Etat { get; }
    ReadOnlyMemory<byte> Donnees { get; }
    bool? EnteteValide { get; }
    bool? DonneesValides { get; }
    ReadOnlyMemory<byte>? Tag { get; }
    byte? CodeFormat { get; }
    byte? CodeDiagnostic { get; }
    IReadOnlyList<int> Revolutions { get; }
}
