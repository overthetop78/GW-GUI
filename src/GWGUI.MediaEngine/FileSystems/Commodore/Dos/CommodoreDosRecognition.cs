namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Conserve la disposition, l'en-tête et la première coordonnée de répertoire reconnus.</summary>
internal sealed record CommodoreDosRecognition
{
    /// <summary>Crée le résultat immuable d'une reconnaissance réussie.</summary>
    public CommodoreDosRecognition(CommodoreDosLayout layout, IReadOnlyList<byte> header, int directoryTrack, int directorySector)
    {
        Layout = layout;
        Header = Array.AsReadOnly(header.ToArray());
        DirectoryTrack = directoryTrack;
        DirectorySector = directorySector;
    }

    /// <summary>Disposition reconnue.</summary>
    public CommodoreDosLayout Layout { get; }
    /// <summary>Copie non modifiable du secteur d'en-tête.</summary>
    public IReadOnlyList<byte> Header { get; }
    /// <summary>Piste du premier secteur de répertoire.</summary>
    public int DirectoryTrack { get; }
    /// <summary>Numéro du premier secteur de répertoire.</summary>
    public int DirectorySector { get; }
}
