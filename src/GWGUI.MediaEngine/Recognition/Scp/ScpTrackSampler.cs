using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Sélectionne uniformément des pistes SCP exploitables entre la première et la dernière.</summary>
internal static class ScpTrackSampler
{
    /// <summary>Nombre maximal de pistes sondées.</summary>
    public const int MaximumTrackCount = 6;
    /// <summary>Indice de la première révolution sondée.</summary>
    public const int FirstRevolutionIndex = 0;

    /// <summary>Retourne au plus six pistes distinctes possédant au moins une révolution.</summary>
    public static IReadOnlyList<ScpTrack> Sample(IReadOnlyList<ScpTrack> tracks)
    {
        if (tracks.Count == 0) return [];
        var count = Math.Min(MaximumTrackCount, tracks.Count);
        return Enumerable.Range(0, count).Select(index => tracks[index * (tracks.Count - 1) / Math.Max(1, count - 1)]).DistinctBy(track => track.TrackNumber).Where(track => track.Revolutions.Count > 0).ToArray();
    }
}
