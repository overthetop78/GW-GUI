using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Exploration.Contracts;

/// <summary>Convertit une piste de flux du moteur vers le contrat commun exposé aux opérations en cours.</summary>
public static class DiskTrackContractMapper
{
    public static IPiste FromScpTrack(ScpTrack track, int resolutionNanoseconds)
    {
        ArgumentNullException.ThrowIfNull(track);
        var revolutions = new List<IRevolution>(track.Revolutions.Count);
        long startNanoseconds = 0;
        for (var number = 0; number < track.Revolutions.Count; number++)
        {
            var revolution = track.Revolutions[number];
            var durationNanoseconds = checked((long)revolution.IndexTimeTicks * resolutionNanoseconds);
            revolutions.Add(new RevolutionData(
                number,
                startNanoseconds,
                durationNanoseconds,
                resolutionNanoseconds,
                revolution.DeclaredFluxCount,
                revolution.Origin.ToString(),
                revolution.FluxIntervals.ToArray()));
            startNanoseconds = checked(startNanoseconds + durationNanoseconds);
        }

        return new TrackData(track.TrackNumber, track.Cylinder, track.Head, revolutions, []);
    }

    private sealed record TrackData(
        int? NumeroSource,
        int Cylindre,
        int Face,
        IReadOnlyList<IRevolution> Revolutions,
        IReadOnlyList<ISecteurSource> SecteursSource) : IPiste;

    private sealed record RevolutionData(
        int Numero,
        long DebutIndex,
        long DureeNanosecondes,
        int Resolution,
        uint? NombreFluxDeclare,
        string Origine,
        IReadOnlyList<uint> TransitionsFlux) : IRevolution;
}
