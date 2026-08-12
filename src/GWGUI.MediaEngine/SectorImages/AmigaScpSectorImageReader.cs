using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Reconstruction;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reconstruit une image sectorielle Amiga depuis les révolutions d'une capture SCP.</summary>
public sealed class AmigaScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    /// <summary>Lit la capture, sélectionne les meilleurs secteurs Amiga et construit l'image.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var candidates = new Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                var result = decoders.Decode(FluxCodecIds.AmigaMfm, track.Revolutions[revolution].Flux);
                foreach (var sector in result.Sectors)
                {
                    if (sector.Data is not { Count: AmigaMfmFormat.SectorByteCount } || sector.Cylinder != track.Cylinder || sector.Head != track.Head) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        if (candidates.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors(AmigaMfmFormat.StructureDescriptionName);
        var sectorsPerTrack = InferSectorsPerTrack(candidates.Keys);
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            if (address.Cylinder >= DiskGeometryConstants.EightyTrackCylinderCount || address.Head >= DiskGeometryConstants.DoubleSidedHeadCount || address.Number < 0 || address.Number >= sectorsPerTrack) continue;
            var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true)
                .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
            var logical = checked((address.Cylinder * DiskGeometryConstants.DoubleSidedHeadCount + address.Head) * sectorsPerTrack + address.Number);
            blocks.Add(new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
        }
        var formatId = sectorsPerTrack == AmigaMfmFormat.HighDensitySectorsPerTrack ? DiskImageFormatIds.AmigaDosHighDensity : DiskImageFormatIds.AmigaDos;
        return new(formatId, AmigaMfmFormat.SectorByteCount, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, sectorsPerTrack, blocks);
    }

    /// <summary>Détermine si les adresses observées décrivent une piste DD ou HD.</summary>
    public static int InferSectorsPerTrack(IEnumerable<SectorAddress> addresses)
    {
        // A damaged or copy-protected DD track can yield an isolated bogus sector ID above 10.
        // Treat the image as HD only when multiple physical tracks contain a convincing 22-sector set.
        var convincingHighDensityTracks = addresses
            .Where(address => address.Cylinder < DiskGeometryConstants.EightyTrackCylinderCount && address.Head < DiskGeometryConstants.DoubleSidedHeadCount && address.Number is >= 0 and < AmigaMfmFormat.HighDensitySectorsPerTrack)
            .GroupBy(address => (address.Cylinder, address.Head))
            .Count(track => track.Select(address => address.Number).Distinct().Count() >= AmigaMfmFormat.HighDensityCredibleSectorCount && track.Any(address => address.Number >= AmigaMfmFormat.DoubleDensitySectorsPerTrack));
        return convincingHighDensityTracks >= AmigaMfmFormat.HighDensityCredibleTrackCount ? AmigaMfmFormat.HighDensitySectorsPerTrack : AmigaMfmFormat.DoubleDensitySectorsPerTrack;
    }
}
