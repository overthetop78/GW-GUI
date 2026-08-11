using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Représente le contenu interprété d'un conteneur SCP complet.</summary>
public sealed record ScpImage
{
    /// <summary>Initialise une image SCP en copiant les pistes fournies.</summary>
    /// <param name="header">En-tête SCP validé.</param>
    /// <param name="tracks">Pistes présentes et interprétées depuis la table du conteneur.</param>
    /// <param name="checksumValid"><see langword="true"/> lorsque la somme de contrôle respecte les règles du format SCP.</param>
    /// <param name="fileSize">Taille totale du conteneur source, en octets.</param>
    /// <exception cref="ArgumentNullException"><paramref name="header"/> ou <paramref name="tracks"/> est nul.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fileSize"/> est négatif.</exception>
    public ScpImage(ScpHeader header, IReadOnlyList<ScpTrack> tracks, bool checksumValid, long fileSize)
    {
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(tracks);
        ArgumentOutOfRangeException.ThrowIfNegative(fileSize);
        Header = header;
        Tracks = new ReadOnlyCollection<ScpTrack>(tracks.ToArray());
        ChecksumValid = checksumValid;
        FileSize = fileSize;
    }

    /// <summary>Obtient l'en-tête SCP validé.</summary>
    public ScpHeader Header { get; }

    /// <summary>Obtient les pistes présentes et interprétées depuis la table du conteneur.</summary>
    public IReadOnlyList<ScpTrack> Tracks { get; }

    /// <summary>Indique si la somme de contrôle respecte les règles du format SCP.</summary>
    public bool ChecksumValid { get; }

    /// <summary>Obtient la taille totale du conteneur source, en octets.</summary>
    public long FileSize { get; }
}
