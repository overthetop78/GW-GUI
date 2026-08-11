using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Decodes RX02 M²FM sectors from SCP and exposes RT-11 logical 512-byte blocks.</summary>
public sealed class DecRx02ScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private static readonly IReadOnlyDictionary<(int Track, int Sector), int> PhysicalToLogical =
        Enumerable.Range(0, 2002).ToDictionary(LogicalToPhysical, logical => logical);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var sectors = new Dictionary<int, List<(DecodedSector Sector, int Revolution)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            foreach (var sector in decoders.Decode(FluxCodecIds.DecRx02, track.Revolutions[revolution]).Sectors ?? [])
            {
                if (sector.Data is not { Count: 256 } || sector.Head != 0 ||
                    !PhysicalToLogical.TryGetValue((sector.Cylinder, sector.Number), out var logical)) continue;
                if (!sectors.TryGetValue(logical, out var values)) sectors[logical] = values = [];
                values.Add((sector, revolution + 1));
            }
        }
        if (sectors.Count == 0) throw new InvalidDataException("No DEC RX02 sectors could be decoded from the SCP image.");

        var blocks = new List<SectorBlock>();
        for (var block = 0; block < 1001; block++)
        {
            var first = Best(sectors, block * 2); var second = Best(sectors, block * 2 + 1);
            if (first is null || second is null) continue;
            var data = new byte[512];
            first.Value.Sector.Data!.ToArray().CopyTo(data, 0);
            second.Value.Sector.Data!.ToArray().CopyTo(data, 256);
            var valid = first.Value.Sector.IntegrityValid == true && second.Value.Sector.IntegrityValid == true;
            blocks.Add(new(block, new(block / 13, 0, block % 13 + 1), data, valid,
                Math.Max(first.Value.Revolution, second.Value.Revolution)));
        }
        return new(DiskImageFormatIds.DecRx02, 512, 77, DiskGeometryConstants.SingleSidedHeadCount, 13, blocks, capacity: DecRx02Geometry.Capacity, logicalBlockCount: 1001);
    }

    private static (DecodedSector Sector, int Revolution)? Best(IReadOnlyDictionary<int, List<(DecodedSector Sector, int Revolution)>> sectors, int logical)
    {
        if (!sectors.TryGetValue(logical, out var values)) return null;
        return values.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
    }

    private static (int Track, int Sector) LogicalToPhysical(int logicalSector)
    {
        var logicalTrack = logicalSector / 26;
        var position = logicalSector % 26;
        position = (2 * position + (position >= 13 ? 1 : 0)) % 26;
        var sector = 1 + (position + 6 * logicalTrack) % 26;
        var track = logicalTrack + 1;
        if (track >= 77) track = 0;
        return (track, sector);
    }
}
