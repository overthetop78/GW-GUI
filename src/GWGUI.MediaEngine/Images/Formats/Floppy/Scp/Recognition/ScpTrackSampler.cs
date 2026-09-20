using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Recognition;

/// <summary>Sélectionne des pistes sur toute la capture afin de ne pas manquer une zone d'un média hybride.</summary>
internal static class ScpTrackSampler
{
    public const int MaximumTrackCount = 24;

    public static IReadOnlyList<ScpTrack> Sample(IReadOnlyList<ScpTrack> tracks)
    {
        var readable = tracks.Where(track => track.Revolutions.Count > 0).ToArray();
        if (readable.Length <= MaximumTrackCount) return readable;

        return Enumerable.Range(0, MaximumTrackCount)
            .Select(index => readable[index * (readable.Length - 1) / (MaximumTrackCount - 1)])
            .DistinctBy(track => track.TrackNumber)
            .ToArray();
    }
}
