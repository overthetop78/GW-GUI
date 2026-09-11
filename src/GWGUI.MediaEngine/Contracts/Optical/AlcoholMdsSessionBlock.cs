using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains one validated MDS session and the readable track blocks it declares.</summary>
public sealed class AlcoholMdsSessionBlock
{
    public AlcoholMdsSessionBlock(
        int number,
        long startSector,
        long endSector,
        int firstTrack,
        int lastTrack,
        IReadOnlyList<AlcoholMdsTrackBlock> tracks)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);
        if (endSector <= startSector) throw new ArgumentOutOfRangeException(nameof(endSector));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(firstTrack);
        if (lastTrack < firstTrack) throw new ArgumentOutOfRangeException(nameof(lastTrack));
        ArgumentNullException.ThrowIfNull(tracks);
        if (tracks.Count == 0) throw new ArgumentException("An MDS session requires at least one readable track.", nameof(tracks));

        Number = number;
        StartSector = startSector;
        EndSector = endSector;
        FirstTrack = firstTrack;
        LastTrack = lastTrack;
        Tracks = new ReadOnlyCollection<AlcoholMdsTrackBlock>(tracks.ToArray());
    }

    public int Number { get; }
    public long StartSector { get; }
    public long EndSector { get; }
    public int FirstTrack { get; }
    public int LastTrack { get; }
    public IReadOnlyList<AlcoholMdsTrackBlock> Tracks { get; }
}
