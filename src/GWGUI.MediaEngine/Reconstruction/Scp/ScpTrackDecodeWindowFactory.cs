using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Reconstruction.Scp;

/// <summary>Construit les vues de décodage d'une piste sans modifier ses révolutions SCP originales.</summary>
internal static class ScpTrackDecodeWindowFactory
{
    /// <summary>Retourne l'unique révolution originale ou le flux continu chronologique de toutes les révolutions.</summary>
    public static IReadOnlyList<ScpTrackDecodeWindow> Create(ScpTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        if (track.Revolutions.Count == 0) return [];

        if (track.Revolutions.Count == 1) return [new(track.Revolutions[0].Flux, 1, false)];

        var indexTime = checked((uint)track.Revolutions.Sum(revolution => (long)revolution.IndexTimeTicks));
        var intervalCount = checked(track.Revolutions.Sum(revolution => revolution.FluxIntervals.Count));
        var intervals = new List<uint>(intervalCount);
        foreach (var revolution in track.Revolutions) intervals.AddRange(revolution.FluxIntervals);
        return [new(new FluxRevolution(indexTime, intervals), 0, true)];
    }

    /// <summary>Retourne la vue continue lorsqu'elle existe, sinon l'unique révolution disponible.</summary>
    public static ScpTrackDecodeWindow Primary(ScpTrack track)
    {
        var windows = Create(track);
        if (windows.Count == 0) throw new InvalidDataException("The SCP track contains no revolution to decode.");
        return windows[0];
    }
}
