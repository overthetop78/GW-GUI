using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Reconstruction.Scp;

/// <summary>Construit les vues de décodage d'une piste sans modifier ses révolutions SCP originales.</summary>
internal static class ScpTrackDecodeWindowFactory
{
    /// <summary>Raccorde chaque révolution à la suivante et conserve la dernière telle quelle.</summary>
    public static IReadOnlyList<ScpTrackDecodeWindow> Create(ScpTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        if (track.Revolutions.Count == 0) return [];
        if (track.Revolutions.Count == 1) return [new(track.Revolutions[0].Flux, 1, false)];

        var windows = new ScpTrackDecodeWindow[track.Revolutions.Count];
        for (var index = 0; index < track.Revolutions.Count - 1; index++)
        {
            var current = track.Revolutions[index];
            var next = track.Revolutions[index + 1];
            var intervals = new List<uint>(checked(current.FluxIntervals.Count + next.FluxIntervals.Count));
            intervals.AddRange(current.FluxIntervals);
            intervals.AddRange(next.FluxIntervals);
            var indexTime = checked(current.IndexTimeTicks + next.IndexTimeTicks);
            windows[index] = new(new FluxRevolution(indexTime, intervals), index + 1, true);
        }

        var last = track.Revolutions[^1];
        windows[^1] = new(last.Flux, track.Revolutions.Count, false);
        return windows;
    }

    /// <summary>Retourne la première vue chronologique disponible.</summary>
    public static ScpTrackDecodeWindow Primary(ScpTrack track)
    {
        var windows = Create(track);
        if (windows.Count == 0) throw new InvalidDataException("The SCP track contains no revolution to decode.");
        return windows[0];
    }
}
