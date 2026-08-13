using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Convertit une piste encodée en révolution synthétique exprimée dans la résolution d'un conteneur SCP.</summary>
public sealed class ScpEncodedTrackFluxService
{
    /// <summary>Construit une unique révolution SCP sans reproduire artificiellement plusieurs tours identiques.</summary>
    /// <param name="track">Piste encodée par le moteur.</param>
    /// <param name="resolution">Indice de résolution stocké dans l'en-tête SCP.</param>
    /// <returns>Révolution synthétique dont les intervalles conservent les instants absolus des transitions.</returns>
    public ScpRevolution Create(EncodedTrack track, byte resolution)
    {
        ArgumentNullException.ThrowIfNull(track);
        var targetTickNanoseconds = ScpFormatConstants.ResolutionStepNanoseconds * (resolution + ScpFormatConstants.ResolutionIndexOffset);
        var intervals = ConvertIntervals(track.Revolution.FluxIntervals, targetTickNanoseconds);
        var indexTime = ConvertTicks(track.Revolution.IndexTimeTicks, targetTickNanoseconds);
        return new ScpRevolution(new FluxRevolution(indexTime, intervals), 0, ScpRevolutionOrigin.Synthetic);
    }

    /// <summary>Convertit les instants cumulés afin que les arrondis ne s'ajoutent pas d'un intervalle au suivant.</summary>
    private static IReadOnlyList<uint> ConvertIntervals(IReadOnlyList<uint> source, int targetTickNanoseconds)
    {
        var result = new List<uint>(source.Count);
        ulong sourceTime = 0;
        ulong previousTargetTime = 0;
        foreach (var interval in source)
        {
            sourceTime = checked(sourceTime + interval);
            var targetTime = ConvertTicks(sourceTime, targetTickNanoseconds);
            if (targetTime > previousTargetTime)
            {
                result.Add(checked((uint)(targetTime - previousTargetTime)));
                previousTargetTime = targetTime;
            }
        }

        return result.AsReadOnly();
    }

    /// <summary>Convertit une durée en arrondissant une seule fois sa valeur absolue.</summary>
    private static uint ConvertTicks(ulong ticks, int targetTickNanoseconds)
    {
        var nanoseconds = checked(ticks * ScpSyntheticFluxConstants.EncoderTickNanoseconds);
        var converted = checked((nanoseconds + (ulong)targetTickNanoseconds / 2) / (ulong)targetTickNanoseconds);
        return checked((uint)converted);
    }
}
