using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;

namespace GWGUI.MediaEngine.SectorImages;

internal sealed class AppleIIScpSectorReconstructor(AppleScpSectorDecoder decoder)
{
    public SectorImage Decode(ScpImage scp, bool prodosOrder, CancellationToken cancellationToken)
    {
        var candidates = decoder.DecodeCandidates(scp, DiskImageFormatIds.AppleIIGcr, 256, cancellationToken);
        if (candidates.Count == 0)
            throw new InvalidDataException("No Apple II GCR sectors could be decoded from the SCP image.");
        if (prodosOrder) return CreateProDosImage(candidates);
        var sectorsPerTrack = candidates.Keys.Any(address => address.Number >= 13) ? 16 : 13;
        var blocks = candidates.Where(pair => pair.Key.Cylinder < 50 && pair.Key.Number >= 0 &&
                                               pair.Key.Number < sectorsPerTrack)
            .Select(pair => AppleScpSectorDecoder.Select(
                pair.Key.Cylinder * sectorsPerTrack + (sectorsPerTrack == 16
                    ? AppleDiskGeometry.PhysicalToDos[pair.Key.Number]
                    : pair.Key.Number), pair.Key, pair.Value)).ToArray();
        var formatId = sectorsPerTrack == 13 ? DiskImageFormatIds.AppleIIDos32 : DiskImageFormatIds.AppleIIDos33;
        return new(formatId, 256, Math.Max(35, blocks.Max(block => block.Address.Cylinder) + 1),
            1, sectorsPerTrack, blocks);
    }

    private static SectorImage CreateProDosImage(Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> candidates)
    {
        var tracks = Math.Max(35, candidates.Keys.Where(key => key.Cylinder < 50)
            .Max(key => key.Cylinder) + 1);
        var blocks = new List<SectorBlock>();
        for (var track = 0; track < tracks; track++)
        for (var blockOnTrack = 0; blockOnTrack < 8; blockOnTrack++)
        {
            var data = new byte[512];
            var integrity = true;
            var revolution = 0;
            var complete = true;
            for (var half = 0; half < 2; half++)
            {
                var logicalSector = blockOnTrack * 2 + half;
                var address = new SectorAddress(track, 0, AppleDiskGeometry.ProDosToPhysical[logicalSector]);
                if (!candidates.TryGetValue(address, out var values))
                {
                    complete = false;
                    break;
                }
                var selected = AppleScpSectorDecoder.Select(0, address, values);
                selected.Data.ToArray().CopyTo(data, half * 256);
                integrity &= selected.IntegrityValid == true;
                revolution = Math.Max(revolution, selected.Revolution);
            }
            if (complete)
                blocks.Add(new(track * 8 + blockOnTrack, new(track, 0, blockOnTrack), data,
                    integrity, revolution));
        }
        if (blocks.Count == 0)
            throw new InvalidDataException("No usable Apple II ProDOS blocks could be reconstructed.");
        return new(DiskImageFormatIds.AppleIIProDos, 512, tracks, 1, 8, blocks);
    }
}
