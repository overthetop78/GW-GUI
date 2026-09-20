namespace GWGUI.MediaEngine.Exploration.Contracts;

/// <summary>Contrat commun complet d'une image de disquette ouverte et analysée.</summary>
public interface IImageDisquette
{
    string TypeImage { get; }
    int? VersionImage { get; }
    long TailleImage { get; }
    IMetadonneesImage MetadonneesImage { get; }
    IReadOnlyList<IPiste> Pistes { get; }
    IReadOnlyList<IFormatDetecte> FormatsDetectes { get; }
    IReadOnlyList<IDiagnostic> Diagnostics { get; }
}

/// <summary>Métadonnées propres au conteneur source.</summary>
public interface IMetadonneesImage
{
    string? Signature { get; }
    string? TypeDisquette { get; }
    int? ResolutionNanosecondes { get; }
    int? NombreRevolutions { get; }
    int? PremierePiste { get; }
    int? DernierePiste { get; }
    int NombrePistes { get; }
    int? NombreFaces { get; }
    bool ChecksumPresent { get; }
    string? ChecksumDeclare { get; }
    string? ChecksumCalcule { get; }
    bool? ChecksumValide { get; }
    IReadOnlyDictionary<string, string> ProprietesFormat { get; }
}
