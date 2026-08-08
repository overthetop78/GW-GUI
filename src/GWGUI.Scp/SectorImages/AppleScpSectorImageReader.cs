using GWGUI.Scp.Decoding;

namespace GWGUI.Scp.SectorImages;

public sealed class AppleScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private static readonly int[] ProDosToPhysical = [0, 2, 4, 6, 8, 10, 12, 14, 1, 3, 5, 7, 9, 11, 13, 15];
    private readonly AppleMacGcrDecoder _macDecoder = new();

    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith("apple2.appledos", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("apple2.nofs", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("apple2.dos", StringComparison.OrdinalIgnoreCase) == true)
            return DecodeAppleII(scp, false, cancellationToken);
        if (formatId?.StartsWith("apple2.prodos.140", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("apple3.sos", StringComparison.OrdinalIgnoreCase) == true)
            return DecodeAppleII(scp, true, cancellationToken);
        if (formatId?.StartsWith("apple2.prodos.800", StringComparison.OrdinalIgnoreCase) == true)
            return DecodeMacintosh(scp, formatId, cancellationToken);
        if (formatId?.StartsWith("mac.", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("applemac", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("applelisa", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.Equals("apple2.prodos", StringComparison.OrdinalIgnoreCase) == true)
            return DecodeMacintosh(scp, formatId, cancellationToken);

        // Compare both GCR families instead of accepting the first accidental prologue.
        // Macintosh/Apple II 3.5-inch media yield 512-byte sectors; Apple II 5.25-inch
        // media yield 256-byte sectors. The valid family reconstructs far more blocks.
        SectorImage? macintosh = null; SectorImage? appleII = null;
        try { macintosh = DecodeMacintosh(scp, null, cancellationToken); } catch (InvalidDataException) { }
        try { appleII = DecodeAppleII(scp, false, cancellationToken); } catch (InvalidDataException) { }
        if (macintosh is null) return appleII ?? throw new InvalidDataException("No Apple GCR sectors could be decoded from the SCP image.");
        if (appleII is null) return macintosh;
        return macintosh.AvailableBlocks.Count >= appleII.AvailableBlocks.Count ? macintosh : appleII;
    }

    private SectorImage DecodeAppleII(ScpImage scp, bool prodosOrder, CancellationToken cancellationToken)
    {
        var candidates = DecodeCandidates(scp, "apple2.gcr", 256, cancellationToken);
        if (candidates.Count == 0) throw new InvalidDataException("No Apple II GCR sectors could be decoded from the SCP image.");
        if (prodosOrder) return CreateProDosImage(candidates);
        var blocks = candidates.Where(pair => pair.Key.Cylinder < 50 && pair.Key.Number is >= 0 and < 16)
            .Select(pair => Select(pair.Key.Cylinder * 16 + pair.Key.Number, pair.Key, pair.Value)).ToArray();
        return new("apple2.dos33", 256, Math.Max(35, blocks.Max(block => block.Address.Cylinder) + 1), 1, 16, blocks);
    }

    private static SectorImage CreateProDosImage(Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> candidates)
    {
        var tracks = Math.Max(35, candidates.Keys.Where(key => key.Cylinder < 50).Max(key => key.Cylinder) + 1);
        var blocks = new List<SectorBlock>();
        for (var track = 0; track < tracks; track++)
        {
            for (var blockOnTrack = 0; blockOnTrack < 8; blockOnTrack++)
            {
                var data = new byte[512]; var integrity = true; var revolution = 0; var complete = true;
                for (var half = 0; half < 2; half++)
                {
                    var logicalSector = blockOnTrack * 2 + half;
                    var address = new SectorAddress(track, 0, ProDosToPhysical[logicalSector]);
                    if (!candidates.TryGetValue(address, out var values)) { complete = false; break; }
                    var selected = Select(0, address, values);
                    selected.Data.ToArray().CopyTo(data, half * 256);
                    integrity &= selected.IntegrityValid == true;
                    revolution = Math.Max(revolution, selected.Revolution);
                }
                if (complete) blocks.Add(new(track * 8 + blockOnTrack, new(track, 0, blockOnTrack), data, integrity, revolution));
            }
        }
        if (blocks.Count == 0) throw new InvalidDataException("No usable Apple II ProDOS blocks could be reconstructed.");
        return new("apple2.prodos", 512, tracks, 1, 8, blocks);
    }

    private SectorImage DecodeMacintosh(ScpImage scp, string? requestedFormatId, CancellationToken cancellationToken)
    {
        var candidates = DecodeCandidates(scp, "applemac.gcr", 512, cancellationToken);
        if (candidates.Count == 0) throw new InvalidDataException("No Apple Macintosh GCR sectors could be decoded from the SCP image.");
        var heads = candidates.Keys.Any(address => address.Head == 1) ? 2 : 1;
        var blocks = new List<SectorBlock>();
        foreach (var pair in candidates)
        {
            var address = pair.Key; if (address.Cylinder >= 80 || address.Head >= heads) continue;
            var sectorsPerTrack = SectorsPerTrack(address.Cylinder);
            if (address.Number < 0 || address.Number >= sectorsPerTrack) continue;
            var priorCylinderBlocks = Enumerable.Range(0, address.Cylinder).Sum(cylinder => SectorsPerTrack(cylinder) * heads);
            var logical = priorCylinderBlocks + address.Head * sectorsPerTrack + address.Number;
            blocks.Add(Select(logical, address, pair.Value));
        }
        if (blocks.Count == 0) throw new InvalidDataException("No usable Apple Macintosh sectors could be reconstructed.");
        var count = 400 * heads * 2; // 400 KiB per side, in 512-byte blocks.
        var provisional = new SectorImage("applemac.gcr", 512, 80, heads, 12, blocks, capacity: count * 512L, logicalBlockCount: count);
        var formatId = requestedFormatId?.StartsWith("applelisa", StringComparison.OrdinalIgnoreCase) == true
            ? "applelisa.office" : "applemac.gcr";
        // Lisa page tags store the owning file identifier at bytes 4-5. File $0001
        // is the MDDF and therefore identifies a tagged Lisa Office disk without
        // requiring the user to select the format manually.
        if (requestedFormatId is null && blocks.Any(block => block.Tag is { Count: >= 6 } tag && tag[4] == 0 && tag[5] == 1))
            formatId = "applelisa.office";
        if (provisional.TryGetBlock(2, out var mdb) && mdb.Data.Count >= 2)
        {
            var signature = (mdb.Data[0] << 8) | mdb.Data[1];
            if (!formatId.StartsWith("applelisa", StringComparison.OrdinalIgnoreCase))
                formatId = signature == 0xd2d7 ? "applemac.mfs" : signature == 0x4244 ? "applemac.hfs" : "apple2.prodos";
        }
        return new(formatId, 512, 80, heads, 12, blocks, capacity: count * 512L, logicalBlockCount: count);
    }

    private Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> DecodeCandidates(ScpImage scp, string decoderId, int size, CancellationToken cancellationToken)
    {
        var result = new Dictionary<SectorAddress, List<(DecodedSector, int)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                var decoded = decoderId == "applemac.gcr"
                    ? DecodeMacTrack(track, track.Revolutions[revolution])
                    : decoders.Decode(decoderId, track.Revolutions[revolution]);
                foreach (var sector in decoded.Sectors ?? [])
                {
                    if (sector.Data is not { Count: var length } || length != size) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    if (!result.TryGetValue(address, out var list)) result[address] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        return result;
    }

    private FluxDecodeResult DecodeMacTrack(ScpTrack track, ScpRevolution revolution)
    {
        var expected = SectorsPerTrack(track.Cylinder);
        var initial = FluxBitstream.EstimateBitCell(revolution.FluxIntervals) * 2;
        var factors = new[] { 1.0, .95, 1.05, .9, 1.1, .85, 1.15 }.Distinct();
        FluxDecodeResult? best = null; var bestScore = int.MinValue;
        foreach (var factor in factors)
        {
            var candidate = _macDecoder.DecodeAtBitCell(revolution, initial * factor);
            var plausible = candidate.Sectors?.Where(sector => sector.Data?.Count == 512 && sector.Cylinder == track.Cylinder
                && sector.Head == track.Head && sector.Number >= 0 && sector.Number < expected).ToArray() ?? [];
            var score = plausible.Select(sector => sector.Number).Distinct().Count() * 100
                + plausible.Count(sector => sector.IntegrityValid == true) * 10 + plausible.Length;
            if (score > bestScore) { best = candidate; bestScore = score; }
            if (plausible.Where(sector => sector.IntegrityValid == true).Select(sector => sector.Number).Distinct().Count() == expected) break;
        }
        return best ?? _macDecoder.Decode(revolution);
    }

    private static SectorBlock Select(int logical, SectorAddress address, List<(DecodedSector Sector, int Revolution)> values)
    {
        var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
        return new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution,
            best.Sector.Tag?.ToArray());
    }

    private static int SectorsPerTrack(int cylinder) => cylinder switch { < 16 => 12, < 32 => 11, < 48 => 10, < 64 => 9, _ => 8 };
}
