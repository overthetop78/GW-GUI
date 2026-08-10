using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Représente une entrée de piste SCP et les révolutions qu'elle contient.
/// </summary>
public sealed record ScpTrack
{
    /// <summary>
    /// Initialise une piste SCP en copiant les révolutions fournies.
    /// </summary>
    /// <param name="trackNumber">Numéro brut de l'entrée dans la table des pistes SCP.</param>
    /// <param name="cylinder">Numéro de cylindre calculé à partir de l'entrée SCP.</param>
    /// <param name="head">Numéro de face calculé à partir de l'entrée SCP.</param>
    /// <param name="revolutions">Révolutions capturées pour cette piste.</param>
    /// <exception cref="ArgumentNullException"><paramref name="revolutions"/> est nul.</exception>
    public ScpTrack(byte trackNumber, int cylinder, int head, IReadOnlyList<ScpRevolution> revolutions)
    {
        ArgumentNullException.ThrowIfNull(revolutions);
        TrackNumber = trackNumber;
        Cylinder = cylinder;
        Head = head;
        Revolutions = new ReadOnlyCollection<ScpRevolution>(revolutions.ToArray());
    }

    /// <summary>
    /// Obtient le numéro brut de l'entrée dans la table des pistes SCP.
    /// </summary>
    public byte TrackNumber { get; }

    /// <summary>
    /// Obtient le numéro de cylindre calculé à partir de l'entrée SCP.
    /// </summary>
    public int Cylinder { get; }

    /// <summary>
    /// Obtient le numéro de face calculé à partir de l'entrée SCP.
    /// </summary>
    public int Head { get; }

    /// <summary>
    /// Obtient les révolutions capturées pour cette piste.
    /// </summary>
    public IReadOnlyList<ScpRevolution> Revolutions { get; }
}
