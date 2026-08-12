using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Images;

using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reconstruit les images Macintosh et Lisa à zones depuis des secteurs SCP décodés.</summary>
internal sealed class AppleMacScpSectorReconstructor(AppleScpSectorDecoder decoder)
{
    /// <summary>Reconstruit et classe une image Macintosh ou Lisa.</summary>
    public SectorImage Decode(ScpImage scp, string? requestedFormatId, CancellationToken cancellationToken)
    {
        var candidates = decoder.DecodeCandidates(scp, FluxCodecIds.AppleMacGcr, AppleIwmGcrFormat.SectorByteCount, cancellationToken);
        if (candidates.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors(AppleIwmGcrFormat.StructureDescriptionName);
        var heads = candidates.Keys.Any(address => address.Head == 1) ? DiskGeometryConstants.DoubleSidedHeadCount : DiskGeometryConstants.SingleSidedHeadCount;
        var blocks = new List<SectorBlock>();
        foreach (var pair in candidates)
        {
            var address = pair.Key;
            if (address.Cylinder >= MacintoshGcrGeometry.CylinderCount || address.Head >= heads) continue;
            var sectorsPerTrack = MacintoshGcrGeometry.Sectors(address.Cylinder);
            if (address.Number < 0 || address.Number >= sectorsPerTrack) continue;
            var priorCylinderBlocks = Enumerable.Range(0, address.Cylinder)
                .Sum(cylinder => MacintoshGcrGeometry.Sectors(cylinder) * heads);
            var logical = priorCylinderBlocks + address.Head * sectorsPerTrack + address.Number;
            blocks.Add(AppleScpSectorDecoder.Select(logical, address, pair.Value));
        }
        if (blocks.Count == 0) throw ScpReconstructionExceptions.NoUsableSectors(AppleIwmGcrFormat.StructureDescriptionName);
        var count = MacintoshGcrGeometry.SingleSidedBlockCount * heads;
        var provisional = new SectorImage(DiskImageFormatIds.AppleMacGcr, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, heads, MacintoshGcrGeometry.MaximumSectorsPerTrack, blocks, capacity: count * (long)MacintoshGcrGeometry.BlockSize, logicalBlockCount: count);
        var formatId = requestedFormatId?.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) == true
            ? requestedFormatId
            : DiskImageFormatIds.AppleMacGcr;
        if (requestedFormatId is null && blocks.Any(block => block.Tag is { Count: >= 6 } tag && tag[4] == 0 && tag[5] == 1))
            formatId = DiskImageFormatIds.AppleLisaOffice;
        if (requestedFormatId is null && AppleScpSectorDecoder.TryFlattenPayload(provisional, out var payload) &&
            AppleDiskImageSignatures.LooksLikeLisaOfficePayload(payload))
            formatId = DiskImageFormatIds.AppleLisaRaw;
        if (provisional.TryGetBlock(2, out var mdb) && mdb.Data.Count >= 2)
        {
            if (mdb.Data.Take(Math.Min(16, mdb.Data.Count)).ToArray().AsSpan().IndexOf("PREBOOT"u8) >= 0)
                formatId = DiskImageFormatIds.AppleLisaMacWorks;
            var signature = (mdb.Data[0] << BitPrimitives.BitsPerByte) | mdb.Data[1];
            if (!formatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase))
                formatId = signature == 0xd2d7
                    ? DiskImageFormatIds.AppleMacMfs
                    : signature == 0x4244
                        ? DiskImageFormatIds.AppleMacHfs
                        : DiskImageFormatIds.AppleIIProDos;
        }
        return new(formatId, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, heads, MacintoshGcrGeometry.MaximumSectorsPerTrack, blocks, capacity: count * (long)MacintoshGcrGeometry.BlockSize, logicalBlockCount: count);
    }
}
