namespace GWGUI.Emulation.Contracts;

/// <summary>Publication d'un cœur, commune aux catalogues distants et aux implémentations intégrées.</summary>
/// <param name="Emulator">Identité de cœur définie par le module propriétaire du catalogue.</param>
/// <param name="Id">Identifiant opaque de la publication, conservé lors de sa sélection.</param>
/// <param name="DeclaredVersion">Version proposée par la source ; elle peut être identique à Id.</param>
/// <param name="DownloadUri">Origine de la bibliothèque ou de son archive ; une implémentation intégrée peut désigner son assembly existant.</param>
/// <param name="PublishedUtc">Date annoncée par la source, ou null si aucune date n'est fournie.</param>
/// <param name="ExpectedArchiveSize">Nombre d'octets annoncé pour le téléchargement, ou null si inconnu ou sans téléchargement.</param>
public sealed record EmulationCatalogCoreRelease<TEmulator>(
    TEmulator Emulator,
    string Id,
    string DeclaredVersion,
    Uri DownloadUri,
    DateTimeOffset? PublishedUtc,
    long? ExpectedArchiveSize)
{
    /// <summary>Libellé proposé par la source ; null laisse l'appelant présenter la version et sa date.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Indique une version imposée ou validée par le catalogue, distincte des propositions facultatives.</summary>
    public bool IsRequired { get; init; }

    /// <summary>Indique si la source doit être extraite comme une archive ZIP ; false désigne une bibliothèque directe ou intégrée.</summary>
    public bool IsZip { get; init; } = true;

    public override string ToString()
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("CoreRelease { ");
        if (PrintMembers(builder)) builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
