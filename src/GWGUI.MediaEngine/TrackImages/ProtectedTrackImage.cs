using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.TrackImages;

/// <summary>Regroupe des pistes protégées avec leur ordre physique et leur état d'écriture.</summary>
public sealed record ProtectedTrackImage
{
    public ProtectedTrackImage(IReadOnlyList<ProtectedTrack> tracks, bool writeProtected)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        if (tracks.Count == 0) throw new ArgumentException("A protected track image requires at least one track.", nameof(tracks));
        var duplicate = tracks.GroupBy(track => (track.Cylinder, track.Head)).FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null) throw new InvalidDataException($"Protected track {duplicate.Key.Cylinder}/{duplicate.Key.Head} is duplicated.");
        Tracks = new ReadOnlyCollection<ProtectedTrack>(tracks.OrderBy(track => track.Cylinder).ThenBy(track => track.Head).ToArray());
        WriteProtected = writeProtected;
    }

    public IReadOnlyList<ProtectedTrack> Tracks { get; }
    public bool WriteProtected { get; }
}
