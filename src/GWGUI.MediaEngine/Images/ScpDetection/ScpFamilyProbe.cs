using System.Collections.Concurrent;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;

namespace GWGUI.MediaEngine.Images.ScpDetection;

internal sealed class ScpFamilyProbe(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private static readonly IReadOnlyList<(ScpFormatFamily Family, string DecoderId)> Probes =
    [
        (ScpFormatFamily.Iso, FluxCodecIds.IsoMfm),
        (ScpFormatFamily.Iso, FluxCodecIds.IsoFm),
        (ScpFormatFamily.Amiga, FluxCodecIds.AmigaMfm),
        (ScpFormatFamily.Commodore, FluxCodecIds.CommodoreGcr),
        (ScpFormatFamily.Apple, FluxCodecIds.AppleIIGcr),
        (ScpFormatFamily.Apple, FluxCodecIds.AppleRwts18),
        (ScpFormatFamily.Apple, FluxCodecIds.AppleMacGcr),
        (ScpFormatFamily.Dec, FluxCodecIds.DecRx02)
    ];

    public async Task<IReadOnlySet<ScpFormatFamily>> DetectAsync(string path, CancellationToken cancellationToken)
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
                var result = decoders.Decode(decoderId, revolution.Flux);
                if (result.Sectors.Any(sector => sector.Data is not null && sector.IntegrityValid == true))
                    found.TryAdd(family, 0);
            }
        }, cancellationToken))).ConfigureAwait(false);
        return found.Keys.ToHashSet();
    }
}
