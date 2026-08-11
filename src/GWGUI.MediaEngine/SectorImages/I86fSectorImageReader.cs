using GWGUI.MediaEngine.Containers.I86f;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Flux.Conversion;
using GWGUI.MediaEngine.Images;

namespace GWGUI.MediaEngine.SectorImages;

public sealed class I86fSectorImageReader(I86fReader reader, FluxDecoderRegistry decoders)
{
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var container = await reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        foreach (var track in container.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var revolution = I86fBitCellFluxConverter.Convert(track.Bits);
            if (revolution is null) continue;
            var decoderId = (track.Flags & I86fTrackFlags.EncodingMask) == I86fTrackFlags.MfmEncoding ? FluxDecoderIds.IsoMfm : FluxDecoderIds.IsoFm;
            var decoded = decoders.Decode(decoderId, revolution);
            foreach (var sector in decoded.Sectors ?? [])
            {
                if (sector.Data is null || sector.Number < 0) continue;
                var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                if (!candidates.TryGetValue(address, out var values)) candidates[address] = values = [];
                values.Add(new(sector, Revolution: 0, SourceTrack: track.LogicalIndex));
            }
        }
        if (candidates.Count == 0) throw new InvalidDataException("No FM or MFM sector could be decoded from the 86F image.");
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var formatId = measured.SectorSize == 512 ? IbmPcImageReader.FormatIdForGeometry(measured.Cylinders, measured.Heads, measured.SectorsPerTrack, measured.SectorSize) : DiskImageFormatIds.I86fFromGeometry(measured.SectorSize, measured.Cylinders, measured.Heads, measured.SectorsPerTrack);
        return IsoSectorImageBuilder.CreateUniform(formatId, candidates, measured.SectorSize, measured.Cylinders, measured.Heads, measured.SectorsPerTrack, address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1, capacity: (long)measured.Cylinders * measured.Heads * measured.SectorsPerTrack * measured.SectorSize);
    }
}
