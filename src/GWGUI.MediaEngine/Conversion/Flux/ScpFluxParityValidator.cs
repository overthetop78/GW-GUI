using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Conversion.Flux;

/// <summary>Vérifie la parité structurelle et temporelle de deux conteneurs SCP.</summary>
internal static class ScpFluxParityValidator
{
    public static void Validate(ScpImage expected, ScpImage actual)
    {
        if (!actual.ChecksumValid)
            throw new InvalidDataException("La copie SCP possède un checksum invalide.");
        if (expected.Header with { Checksum = 0 } != actual.Header with { Checksum = 0 })
            throw new InvalidDataException("L'en-tête SCP a changé pendant la conversion.");
        if (expected.Tracks.Count != actual.Tracks.Count)
            throw new InvalidDataException("Le nombre de pistes SCP a changé pendant la conversion.");
        for (var trackIndex = 0; trackIndex < expected.Tracks.Count; trackIndex++)
        {
            var sourceTrack = expected.Tracks[trackIndex];
            var targetTrack = actual.Tracks[trackIndex];
            if (sourceTrack.TrackNumber != targetTrack.TrackNumber ||
                sourceTrack.Cylinder != targetTrack.Cylinder ||
                sourceTrack.Head != targetTrack.Head ||
                sourceTrack.Revolutions.Count != targetTrack.Revolutions.Count)
                throw new InvalidDataException("La structure des pistes SCP a changé pendant la conversion.");
            for (var revolutionIndex = 0;
                 revolutionIndex < sourceTrack.Revolutions.Count;
                 revolutionIndex++)
            {
                var source = sourceTrack.Revolutions[revolutionIndex];
                var target = targetTrack.Revolutions[revolutionIndex];
                if (source.IndexTimeTicks != target.IndexTimeTicks ||
                    source.DeclaredFluxCount != target.DeclaredFluxCount ||
                    !source.FluxIntervals.SequenceEqual(target.FluxIntervals))
                    throw new InvalidDataException("Un index ou un timing SCP a changé pendant la conversion.");
            }
        }
    }
}
