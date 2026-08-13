using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.TrackImages;

/// <summary>Transforme des pistes reconstruites en contrat de piste sans leur attribuer de protection inexistante.</summary>
public static class EncodedTrackImageFactory
{
    public static ProtectedTrackImage Create(IReadOnlyList<EncodedDiskTrack> tracks, int resolutionNanoseconds)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        if (resolutionNanoseconds <= 0) throw new ArgumentOutOfRangeException(nameof(resolutionNanoseconds));
        var protectedTracks = tracks.Select(track => new ProtectedTrack(track.Cylinder, track.Head, track.Track.Bits, [new(0, track.Track.Bits.Count, track.BitCellTicks * (double)resolutionNanoseconds)], [], [], [new(resolutionNanoseconds, track.Track.Revolution)])).ToArray();
        return new(protectedTracks, false);
    }
}
