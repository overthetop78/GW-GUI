using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Dec;

/// <summary>Décode les secteurs RX02 M²FM d'une capture SCP et expose les blocs logiques RT-11.</summary>
/// <param name="scpReader">Lecteur utilisé pour analyser le conteneur SCP.</param>
/// <param name="decoders">Registre fournissant le décodeur M²FM RX02.</param>
public sealed class DecRx02ScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    /// <summary>Correspondance non modifiable entre chaque adresse physique RX02 et son numéro de secteur logique intermédiaire.</summary>
    private static readonly IReadOnlyDictionary<(int Track, int Sector), int> PhysicalToLogical =
        Enumerable.Range(0, DecRx02Geometry.PhysicalSectorCount).ToDictionary(DecRx02SectorOrder.LogicalToPhysical, logical => logical);

    /// <summary>Lit la capture et réunit chaque paire de secteurs physiques en bloc logique.</summary>
    /// <param name="path">Chemin de la capture SCP à reconstruire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la reconstruction.</param>
    /// <returns>L'image RX02 reconstruite en blocs RT-11 de 512 octets.</returns>
    /// <exception cref="InvalidDataException">Aucun secteur RX02 n'a été décodé ou aucune paire physique complète ne peut former un bloc logique.</exception>
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
            blocks.Add(new(block, new(block / DecRx02Geometry.LogicalBlocksPerTrack, DecRx02Geometry.FirstHead, block % DecRx02Geometry.LogicalBlocksPerTrack + DecRx02Geometry.FirstLogicalSectorNumber), data, valid, Math.Max(first.Value.Revolution, second.Value.Revolution)));
        }
        if (blocks.Count == 0) throw ScpReconstructionExceptions.NoUsableSectors(DecRx02Format.StructureDescriptionName);
        return new(DiskImageFormatIds.DecRx02, DecRx02Geometry.LogicalBlockSize, DecRx02Geometry.TrackCount, DecRx02Geometry.HeadCount, DecRx02Geometry.LogicalBlocksPerTrack, blocks, capacity: DecRx02Geometry.Capacity, logicalBlockCount: DecRx02Geometry.LogicalBlockCount);
    }

    /// <summary>Sélectionne le meilleur candidat d'un secteur physique logique.</summary>
    /// <param name="sectors">Candidats regroupés par numéro logique intermédiaire.</param>
    /// <param name="logical">Numéro logique intermédiaire recherché.</param>
    /// <returns>Le candidat dont l'intégrité est la meilleure, ou <see langword="null"/> lorsque le secteur manque.</returns>
    private static (DecodedSector Sector, int Revolution)? Best(IReadOnlyDictionary<int, List<(DecodedSector Sector, int Revolution)>> sectors, int logical)
    {
        if (!sectors.TryGetValue(logical, out var values)) return null;
        return values.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
    }
}
