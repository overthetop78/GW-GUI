using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.SectorImages;

public sealed class AmigaScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var candidates = new Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                var result = decoders.Decode("amiga.mfm", track.Revolutions[revolution]);
                foreach (var sector in result.Sectors ?? [])
                {
                    if (sector.Data is not { Count: 512 } || sector.Cylinder != track.Cylinder || sector.Head != track.Head) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        if (candidates.Count == 0) throw new InvalidDataException("No Amiga sectors could be decoded from the SCP image.");
        var sectorsPerTrack = InferSectorsPerTrack(candidates.Keys);
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            if (address.Cylinder >= DiskGeometryConstants.EightyTrackCylinderCount || address.Head >= DiskGeometryConstants.DoubleSidedHeadCount || address.Number < 0 || address.Number >= sectorsPerTrack) continue;
            var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true)
                .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
            var logical = checked((address.Cylinder * 2 + address.Head) * sectorsPerTrack + address.Number);
            blocks.Add(new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
        }
        var formatId = sectorsPerTrack == 22 ? DiskImageFormatIds.AmigaDosHighDensity : DiskImageFormatIds.AmigaDos;
        return new(formatId, 512, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, sectorsPerTrack, blocks);
    }

    public static int InferSectorsPerTrack(IEnumerable<SectorAddress> addresses)
    {
        // A damaged or copy-protected DD track can yield an isolated bogus sector ID above 10.
        // Treat the image as HD only when multiple physical tracks contain a convincing 22-sector set.
        var convincingHighDensityTracks = addresses
            .Where(address => address.Cylinder < DiskGeometryConstants.EightyTrackCylinderCount && address.Head < DiskGeometryConstants.DoubleSidedHeadCount && address.Number is >= 0 and < 22)
            .GroupBy(address => (address.Cylinder, address.Head))
            .Count(track => track.Select(address => address.Number).Distinct().Count() >= 17 && track.Any(address => address.Number >= 11));
        return convincingHighDensityTracks >= 2 ? 22 : 11;
    }
}
