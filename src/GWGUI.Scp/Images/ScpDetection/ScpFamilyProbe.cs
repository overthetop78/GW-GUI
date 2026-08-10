using System.Collections.Concurrent;
using GWGUI.Scp.Containers.Scp;
using GWGUI.Scp.Decoding;

namespace GWGUI.Scp.Images.ScpDetection;

internal sealed class ScpFamilyProbe(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private static readonly IReadOnlyList<(ScpFormatFamily Family, string DecoderId)> Probes =
    [
        (ScpFormatFamily.Iso, "iso.mfm"),
        (ScpFormatFamily.Iso, "iso.fm"),
        (ScpFormatFamily.Amiga, "amiga.mfm"),
        (ScpFormatFamily.Commodore, "commodore.gcr"),
        (ScpFormatFamily.Apple, "apple2.gcr"),
        (ScpFormatFamily.Apple, "apple2.rwts18"),
        (ScpFormatFamily.Apple, "applemac.gcr"),
        (ScpFormatFamily.Dec, "dec.rx02")
    ];

    public async Task<IReadOnlySet<ScpFormatFamily>> DetectAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (scp.Tracks.Count == 0) return new HashSet<ScpFormatFamily>();
        var sampleCount = Math.Min(6, scp.Tracks.Count);
        var samples = Enumerable.Range(0, sampleCount)
            .Select(index => scp.Tracks[index * (scp.Tracks.Count - 1) / Math.Max(1, sampleCount - 1)])
            .DistinctBy(track => track.TrackNumber)
            .Where(track => track.Revolutions.Count > 0)
            .ToArray();
        var found = new ConcurrentDictionary<ScpFormatFamily, byte>();
        await Task.WhenAll(samples.Select(track => Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var revolution = track.Revolutions[0];
            foreach (var (family, decoderId) in Probes)
            {
                if (found.ContainsKey(family)) continue;
                var result = decoders.Decode(decoderId, revolution);
                if ((result.Sectors ?? []).Any(sector => sector.Data is not null && sector.IntegrityValid == true))
                    found.TryAdd(family, 0);
            }
        }, cancellationToken))).ConfigureAwait(false);
        return found.Keys.ToHashSet();
    }
}
