using GWGUI.MediaEngine.TrackImages;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Expose une capture SCP dans le contrat de piste protégée sans toucher aux révolutions brutes.</summary>
public static class ScpProtectedTrackImageAdapter
{
    public static ProtectedTrackImage Create(ScpImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        var tracks = image.Tracks.Select(track => new ProtectedTrack(track.Cylinder, track.Head, null, [], [], [], track.Revolutions.Select(revolution => new TrackFluxRevolution(image.Header.ResolutionNanoseconds, revolution.Flux)).ToArray())).ToArray();
        return new(tracks, !image.Header.Flags.HasFlag(ScpFlags.Writable));
    }
}
