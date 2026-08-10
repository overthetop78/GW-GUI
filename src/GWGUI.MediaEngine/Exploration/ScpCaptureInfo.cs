using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Exploration;

/// <summary>
/// Décrit les métadonnées finales d'une capture SCP sans contenir les données de flux des pistes.
/// </summary>
public sealed record ScpCaptureInfo
{
    /// <summary>
    /// Initialise les métadonnées d'une capture SCP.
    /// </summary>
    /// <param name="header">En-tête SCP validé à partir du début du fichier.</param>
    /// <param name="capturedTracks">Nombre d'entrées de piste présentes dans la table SCP.</param>
    /// <param name="missingTracks">Nombre d'entrées de piste attendues mais absentes.</param>
    /// <param name="cylinders">Nombre de cylindres distincts représentés par les entrées présentes.</param>
    /// <param name="sides">Nombre de faces distinctes représentées par les entrées présentes.</param>
    /// <param name="checksumValid"><see langword="true"/> lorsque la somme de contrôle de la capture est valide selon les règles du format SCP.</param>
    /// <param name="fileSize">Taille totale du fichier, en octets.</param>
    public ScpCaptureInfo(ScpHeader header, int capturedTracks, int missingTracks, int cylinders, int sides, bool checksumValid, long fileSize)
    {
        Header = header;
        CapturedTracks = capturedTracks;
        MissingTracks = missingTracks;
        Cylinders = cylinders;
        Sides = sides;
        ChecksumValid = checksumValid;
        FileSize = fileSize;
    }

    /// <summary>
    /// Obtient l'en-tête SCP validé à partir du début du fichier.
    /// </summary>
    public ScpHeader Header { get; }

    /// <summary>
    /// Obtient le nombre d'entrées de piste présentes dans la table SCP.
    /// </summary>
    public int CapturedTracks { get; }

    /// <summary>
    /// Obtient le nombre d'entrées de piste attendues mais absentes.
    /// </summary>
    public int MissingTracks { get; }

    /// <summary>
    /// Obtient le nombre de cylindres distincts représentés par les entrées présentes.
    /// </summary>
    public int Cylinders { get; }

    /// <summary>
    /// Obtient le nombre de faces distinctes représentées par les entrées présentes.
    /// </summary>
    public int Sides { get; }

    /// <summary>
    /// Indique si la somme de contrôle de la capture est valide selon les règles du format SCP.
    /// </summary>
    public bool ChecksumValid { get; }

    /// <summary>
    /// Obtient la taille totale du fichier, en octets.
    /// </summary>
    public long FileSize { get; }
}
