using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Reconstruction;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Décode les secteurs RX02 M²FM d'une capture SCP et expose les blocs logiques RT-11.</summary>
public sealed class DecRx02ScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private static readonly IReadOnlyDictionary<(int Track, int Sector), int> PhysicalToLogical =
        Enumerable.Range(0, DecRx02Geometry.PhysicalSectorCount).ToDictionary(LogicalToPhysical, logical => logical);

    /// <summary>Lit la capture et réunit chaque paire de secteurs physiques en bloc logique.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var sectors = new Dictionary<int, List<(DecodedSector Sector, int Revolution)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            foreach (var sector in decoders.Decode(FluxCodecIds.DecRx02, track.Revolutions[revolution].Flux).Sectors)
            {
                if (sector.Data is not { Count: DecRx02Geometry.PhysicalSectorSize } || sector.Head != 0 ||
                    !PhysicalToLogical.TryGetValue((sector.Cylinder, sector.Number), out var logical)) continue;
                if (!sectors.TryGetValue(logical, out var values)) sectors[logical] = values = [];
                values.Add((sector, revolution + 1));
            }
        }
        if (sectors.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors(DecRx02Format.StructureDescriptionName);

        var blocks = new List<SectorBlock>();
        for (var block = 0; block < DecRx02Geometry.LogicalBlockCount; block++)
        {
            var first = Best(sectors, block * DecRx02Geometry.PhysicalSectorsPerLogicalBlock);
            var second = Best(sectors, block * DecRx02Geometry.PhysicalSectorsPerLogicalBlock + 1);
            if (first is null || second is null) continue;
            var data = new byte[DecRx02Geometry.LogicalBlockSize];
            first.Value.Sector.Data!.ToArray().CopyTo(data, 0);
            second.Value.Sector.Data!.ToArray().CopyTo(data, DecRx02Geometry.PhysicalSectorSize);
            var valid = first.Value.Sector.IntegrityValid == true && second.Value.Sector.IntegrityValid == true;
            blocks.Add(new(block, new(block / DecRx02Geometry.LogicalBlocksPerTrack, 0, block % DecRx02Geometry.LogicalBlocksPerTrack + 1), data, valid,
                Math.Max(first.Value.Revolution, second.Value.Revolution)));
        }
        return new(DiskImageFormatIds.DecRx02, DecRx02Geometry.LogicalBlockSize, DecRx02Geometry.TrackCount, DecRx02Geometry.HeadCount, DecRx02Geometry.LogicalBlocksPerTrack, blocks, capacity: DecRx02Geometry.Capacity, logicalBlockCount: DecRx02Geometry.LogicalBlockCount);
    }

    /// <summary>Sélectionne le meilleur candidat d'un secteur physique logique.</summary>
    private static (DecodedSector Sector, int Revolution)? Best(IReadOnlyDictionary<int, List<(DecodedSector Sector, int Revolution)>> sectors, int logical)
    {
        if (!sectors.TryGetValue(logical, out var values)) return null;
        return values.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
    }

    /// <summary>Convertit un index logique RX02 en piste et secteur physiques entrelacés.</summary>
    private static (int Track, int Sector) LogicalToPhysical(int logicalSector)
    {
        var logicalTrack = logicalSector / DecRx02Geometry.PhysicalSectorsPerTrack;
        var position = logicalSector % DecRx02Geometry.PhysicalSectorsPerTrack;
        position = (DecRx02Geometry.PhysicalSectorsPerLogicalBlock * position + (position >= DecRx02Geometry.LogicalBlocksPerTrack ? 1 : 0)) % DecRx02Geometry.PhysicalSectorsPerTrack;
        var sector = 1 + (position + 6 * logicalTrack) % DecRx02Geometry.PhysicalSectorsPerTrack;
        var track = logicalTrack + 1;
        if (track >= DecRx02Geometry.TrackCount) track = 0;
        return (track, sector);
    }
}
