using System.Collections.Concurrent;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Reconstruction.Scp;

namespace GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Sonde un échantillon de pistes afin d'identifier les familles de reconstruction SCP.</summary>
internal sealed class ScpFamilyProbe(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    /// <summary>Retourne chaque famille ayant produit au moins un secteur doté de données et d'une intégrité valide.</summary>
    public async Task<IReadOnlySet<ScpFormatFamily>> DetectAsync(string path, CancellationToken cancellationToken)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var samples = ScpTrackSampler.Sample(scp.Tracks);
        if (samples.Count == 0) return new HashSet<ScpFormatFamily>();
        var found = new ConcurrentDictionary<ScpFormatFamily, byte>();
        await Task.WhenAll(samples.Select(track => Task.Run(() => ProbeTrack(track, found, cancellationToken), cancellationToken))).ConfigureAwait(false);
        return found.Keys.ToHashSet();
    }

    /// <summary>Sonde la première fenêtre chronologique et évite les décodeurs d'une famille déjà trouvée.</summary>
    private void ProbeTrack(ScpTrack track, ConcurrentDictionary<ScpFormatFamily, byte> found, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var flux = ScpTrackDecodeWindowFactory.Primary(track).Flux;
        foreach (var definition in ScpFamilyProbeCatalog.Definitions)
        {
            if (found.ContainsKey(definition.Family)) continue;
            var result = decoders.Decode(definition.DecoderId, flux);
            if (HasValidSector(result)) found.TryAdd(definition.Family, 0);
        }
    }

    private static bool HasValidSector(FluxDecodeResult result) =>
        result.Sectors.Any(sector => sector.Data is not null && sector.IntegrityValid == true);
}
