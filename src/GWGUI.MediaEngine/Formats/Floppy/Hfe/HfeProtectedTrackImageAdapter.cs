using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.MediaEngine.Formats.Floppy.Hfe;

/// <summary>Exposes HFE bit cells and timing through the common flux representation without decoding sectors.</summary>
public static class HfeProtectedTrackImageAdapter
{
    public static ProtectedTrackImage Create(HfeImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        var tracks = image.Tracks.Select(track => new ProtectedTrack(
            track.Cylinder,
            track.Head,
            track.Bits,
            track.Timing,
            [],
            track.Features,
            [new TrackFluxRevolution(HfeFormat.TickNanoseconds, track.Revolution)])).ToArray();
        return new ProtectedTrackImage(tracks, false);
    }
}
