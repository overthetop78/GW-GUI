using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Reconstruction.Scp;

namespace GWGUI.MediaEngine.Reconstruction.Amiga;

/// <summary>Reconstruit une image sectorielle Amiga depuis les révolutions d'une capture SCP.</summary>
/// <param name="scpReader">Lecteur utilisé pour analyser le conteneur SCP.</param>
/// <param name="decoders">Registre fournissant le décodeur MFM Amiga.</param>
public sealed class AmigaScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    /// <summary>Lit la capture, sélectionne les meilleurs secteurs Amiga et construit l'image sectorielle.</summary>
    /// <param name="path">Chemin de la capture SCP à reconstruire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la reconstruction.</param>
    /// <returns>L'image Amiga DD ou HD reconstruite à partir des secteurs utilisables.</returns>
    /// <exception cref="InvalidDataException">Aucun secteur Amiga n'a été décodé ou aucun secteur décodé ne respecte la géométrie Amiga.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var candidates = new Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var window in ScpTrackDecodeWindowFactory.Create(track))
            {
                var result = decoders.Decode(FluxCodecIds.AmigaMfm, window.Flux);
                foreach (var sector in result.Sectors)
                {
                    if (sector.Data is not { Count: AmigaMfmFormat.SectorByteCount } || sector.Cylinder != track.Cylinder || sector.Head != track.Head) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
                    list.Add((sector, window.Revolution));
                }
            }
        }
        if (candidates.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors(AmigaMfmFormat.StructureDescriptionName);
        var sectorsPerTrack = InferSectorsPerTrack(candidates.Keys);
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            if (address.Cylinder >= DiskGeometryConstants.EightyTrackCylinderCount || address.Head >= DiskGeometryConstants.DoubleSidedHeadCount || address.Number < 0 || address.Number >= sectorsPerTrack) continue;
            var best = SectorCandidateSelector.Best(values, value => value.Sector.IntegrityValid);
            var logical = checked((address.Cylinder * DiskGeometryConstants.DoubleSidedHeadCount + address.Head) * sectorsPerTrack + address.Number);
            blocks.Add(new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
        }
        if (blocks.Count == 0) throw ScpReconstructionExceptions.NoUsableSectors(AmigaMfmFormat.StructureDescriptionName, candidates.Count, blocks.Count);
        var formatId = sectorsPerTrack == AmigaMfmFormat.HighDensitySectorsPerTrack ? DiskImageFormatIds.AmigaDosHighDensity : DiskImageFormatIds.AmigaDos;
        return new(formatId, AmigaMfmFormat.SectorByteCount, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, sectorsPerTrack, blocks);
    }

    /// <summary>Détermine si les adresses observées décrivent une piste DD ou HD.</summary>
    /// <param name="addresses">Adresses des candidats Amiga décodés.</param>
    /// <returns>Le nombre de secteurs par piste de la géométrie DD ou HD retenue.</returns>
    /// <remarks>Un identifiant isolé supérieur à 10 peut provenir d'une piste DD endommagée ou protégée. La géométrie HD exige donc plusieurs pistes contenant un ensemble crédible de 22 secteurs.</remarks>
    public static int InferSectorsPerTrack(IEnumerable<SectorAddress> addresses)
    {
        var convincingHighDensityTracks = addresses
            .Where(address => address.Cylinder < DiskGeometryConstants.EightyTrackCylinderCount && address.Head < DiskGeometryConstants.DoubleSidedHeadCount && address.Number is >= 0 and < AmigaMfmFormat.HighDensitySectorsPerTrack)
            .GroupBy(address => (address.Cylinder, address.Head))
            .Count(track => track.Select(address => address.Number).Distinct().Count() >= AmigaMfmFormat.HighDensityCredibleSectorCount && track.Any(address => address.Number >= AmigaMfmFormat.DoubleDensitySectorsPerTrack));
        return convincingHighDensityTracks >= AmigaMfmFormat.HighDensityCredibleTrackCount ? AmigaMfmFormat.HighDensitySectorsPerTrack : AmigaMfmFormat.DoubleDensitySectorsPerTrack;
    }
}
