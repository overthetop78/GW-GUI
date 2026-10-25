using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Reconstruction;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Images.Reading.Decoding.Definitions;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Iso;

namespace GWGUI.MediaEngine.Images.Reading.Reconstruction;

internal sealed class DefinedScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpCandidateDecoder decoder = new(scpReader,
        new FluxDecoderRegistry(decoders.Decoders.Select(item => item is MicropolisMfmDecoder
            ? new MicropolisMfmDecoder(includeRecord: true) : item).ToArray()));

    public async Task<SectorImage> ReadAsync(string path, string formatId, IProgress<ScpExplorationProgress>? progress, CancellationToken token)
        => await ReadAsync(ScpProtectedTrackImageAdapter.Create(await scpReader.ReadAsync(path, token).ConfigureAwait(false)),
            formatId, progress, token).ConfigureAwait(false);

    public async Task<SectorImage> ReadAsync(ProtectedTrackImage image, string formatId, IProgress<ScpExplorationProgress>? progress, CancellationToken token)
    {
        var definition = DiskDefsFormatReader.RawDefinitions[formatId];
        var layout = definition.Layout();
        if (definition.Tracks.All(track => track.Codec == "northstar"))
            return ReadNorthstar(image, definition, layout, token);
        var candidates = new Dictionary<string, IsoSectorCandidateSet>(StringComparer.Ordinal);
        foreach (var codec in definition.Tracks.Select(track => track.Codec).Distinct())
        {
            var codecId = codec switch
            {
                "ibm.fm" => FluxCodecIds.IsoFm,
                "ibm.mfm" => FluxCodecIds.IsoMfm,
                "apple2.gcr" => FluxCodecIds.AppleIIGcr,
                "datageneral" => FluxCodecIds.DataGeneralFm,
                "hp.mmfm" => FluxCodecIds.HpMmfm,
                "micropolis" => FluxCodecIds.MicropolisMfm,
                _ => throw new NotSupportedException($"No native decoder for '{codec}'.")
            };
            candidates[codec] = await decoder.DecodeAsync(image, [codecId], progress, token).ConfigureAwait(false);
        }
        var blocks = new List<SectorBlock>();
        foreach (var sector in layout)
        {
            token.ThrowIfCancellationRequested();
            var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
            if (!candidates[sector.Track.Codec].Physical.TryGetValue(address, out var values)) continue;
            var valid = values.Where(value => value.Sector.Cylinder == sector.Cylinder
                && (sector.Track.Codec == "micropolis" || value.Sector.Head == sector.Track.Number("h", sector.Head))
                && value.Sector.Data?.Count == (sector.Track.Codec == "micropolis" ? MicropolisMfmFormat.RecordByteCount : sector.Size)).ToArray();
            if (valid.Length == 0) continue;
            var best = SectorCandidateSelector.Best(valid, candidate => candidate.Sector.IntegrityValid);
            var data = best.Sector.Data!.ToArray();
            if (sector.Track.Codec == "micropolis" && sector.Size == MicropolisMfmFormat.SectorSize)
                data = data.AsSpan(MicropolisMfmFormat.DataOffset, MicropolisMfmFormat.SectorSize).ToArray();
            blocks.Add(new(sector.LogicalBlock, address, data, best.Sector.IntegrityValid, best.Revolution));
        }
        return DefinedRawImageReader.CreateImage(definition, layout, blocks);
    }

    private static SectorImage ReadNorthstar(ProtectedTrackImage image, DiskDefinition definition,
        IReadOnlyList<DefinedDiskSector> layout, CancellationToken token)
    {
        var decoder = new NorthstarMfmDecoder();
        var blocks = new List<SectorBlock>();
        foreach (var track in image.Tracks)
        {
            token.ThrowIfCancellationRequested();
            var profile = definition.Track(track.Cylinder, track.Head);
            if (profile is null) continue;
            var values = track.Revolutions.SelectMany((revolution, index) =>
                decoder.DecodeHardSectors(revolution.Flux, profile.Parameters.GetValueOrDefault("mode") == "fm",
                    track.Cylinder, track.Head, profile.Number("secs", 10))
                    .Select(sector => (Sector: sector, Revolution: index + 1))).ToArray();
            foreach (var sector in layout.Where(sector => sector.Cylinder == track.Cylinder && sector.Head == track.Head))
            {
                var candidates = values.Where(value => value.Sector.Number == sector.Number && value.Sector.Data?.Count == sector.Size).ToArray();
                if (candidates.Length == 0) continue;
                var best = SectorCandidateSelector.Best(candidates, value => value.Sector.IntegrityValid);
                blocks.Add(new(sector.LogicalBlock, new(sector.Cylinder, sector.Head, sector.Number),
                    best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
            }
        }
        return DefinedRawImageReader.CreateImage(definition, layout, blocks);
    }
}
