
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

using GWGUI.MediaEngine.Images.Models.Flux;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Reconstruction;

/// <summary>Construit les vues de décodage d'une piste sans modifier ses révolutions SCP originales.</summary>
internal static class ScpTrackDecodeWindowFactory
{
    /// <summary>Raccorde chaque révolution à la suivante et conserve la dernière telle quelle.</summary>
    public static IReadOnlyList<ScpTrackDecodeWindow> Create(ScpTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return Create(track.Revolutions.Select(revolution => revolution.Flux).ToArray());
    }

    /// <summary>Construit les mêmes vues depuis une piste de flux commune, indépendamment de son conteneur.</summary>
    public static IReadOnlyList<ScpTrackDecodeWindow> Create(ProtectedTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return Create(track.Revolutions.Select(revolution => revolution.Flux).ToArray());
    }

    private static IReadOnlyList<ScpTrackDecodeWindow> Create(IReadOnlyList<FluxRevolution> revolutions)
    {
        if (revolutions.Count == 0) return [];
        if (revolutions.Count == 1) return [new(revolutions[0], 1, false)];

        var windows = new ScpTrackDecodeWindow[revolutions.Count];
        for (var index = 0; index < revolutions.Count - 1; index++)
        {
            var current = revolutions[index];
            var next = revolutions[index + 1];
            var intervals = new List<uint>(checked(current.FluxIntervals.Count + next.FluxIntervals.Count));
            intervals.AddRange(current.FluxIntervals);
            intervals.AddRange(next.FluxIntervals);
            var indexTime = checked(current.IndexTimeTicks + next.IndexTimeTicks);
            windows[index] = new(new FluxRevolution(indexTime, intervals), index + 1, true);
        }

        var last = revolutions[^1];
        windows[^1] = new(last, revolutions.Count, false);
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
