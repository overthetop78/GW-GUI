using GWGUI.MediaEngine.Images;

namespace GWGUI.MediaEngine.SectorImages;

internal sealed class AppleMacScpSectorReconstructor(AppleScpSectorDecoder decoder)
{
    public SectorImage Decode(ScpImage scp, string? requestedFormatId, CancellationToken cancellationToken)
    {
        var candidates = decoder.DecodeCandidates(scp, "applemac.gcr", 512, cancellationToken);
        if (candidates.Count == 0)
            throw new InvalidDataException("No Apple Macintosh GCR sectors could be decoded from the SCP image.");
        var heads = candidates.Keys.Any(address => address.Head == 1) ? 2 : 1;
        var blocks = new List<SectorBlock>();
        foreach (var pair in candidates)
        {
            var address = pair.Key;
            if (address.Cylinder >= 80 || address.Head >= heads) continue;
            var sectorsPerTrack = AppleDiskGeometry.AppleMacSectors(address.Cylinder);
            if (address.Number < 0 || address.Number >= sectorsPerTrack) continue;
            var priorCylinderBlocks = Enumerable.Range(0, address.Cylinder)
                .Sum(cylinder => AppleDiskGeometry.AppleMacSectors(cylinder) * heads);
            var logical = priorCylinderBlocks + address.Head * sectorsPerTrack + address.Number;
            blocks.Add(AppleScpSectorDecoder.Select(logical, address, pair.Value));
        }
        if (blocks.Count == 0)
            throw new InvalidDataException("No usable Apple Macintosh sectors could be reconstructed.");
        var count = 400 * heads * 2;
        var provisional = new SectorImage("applemac.gcr", 512, 80, heads, 12, blocks,
            capacity: count * 512L, logicalBlockCount: count);
        var formatId = requestedFormatId?.StartsWith("applelisa", StringComparison.OrdinalIgnoreCase) == true
            ? requestedFormatId
            : "applemac.gcr";
        if (requestedFormatId is null && blocks.Any(block =>
                block.Tag is { Count: >= 6 } tag && tag[4] == 0 && tag[5] == 1))
            formatId = "applelisa.office";
        if (requestedFormatId is null && AppleScpSectorDecoder.TryFlattenPayload(provisional, out var payload) &&
            AppleDiskImageSignatures.LooksLikeLisaOfficePayload(payload))
            formatId = "applelisa.raw";
        if (provisional.TryGetBlock(2, out var mdb) && mdb.Data.Count >= 2)
        {
            if (mdb.Data.Take(Math.Min(16, mdb.Data.Count)).ToArray().AsSpan().IndexOf("PREBOOT"u8) >= 0)
                formatId = "applelisa.macworks";
            var signature = (mdb.Data[0] << 8) | mdb.Data[1];
            if (!formatId.StartsWith("applelisa", StringComparison.OrdinalIgnoreCase))
                formatId = signature == 0xd2d7
                    ? "applemac.mfs"
                    : signature == 0x4244
                        ? "applemac.hfs"
                        : "apple2.prodos";
        }
        return new(formatId, 512, 80, heads, 12, blocks,
            capacity: count * 512L, logicalBlockCount: count);
    }
}
