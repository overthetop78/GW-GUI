using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Containers.Scp;

using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Recognition.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Apple;

/// <summary>Reconstruit les images Macintosh et Lisa Ã  zones depuis des secteurs SCP dÃ©codÃ©s.</summary>
/// <param name="decoder">DÃ©codeur commun chargÃ© de regrouper les candidats sectoriels Apple.</param>
internal sealed class AppleMacScpSectorReconstructor(AppleScpSectorDecoder decoder)
{
    /// <summary>Reconstruit et classe une image Macintosh ou Lisa.</summary>
    /// <param name="scp">Capture SCP dÃ©jÃ  analysÃ©e.</param>
    /// <param name="requestedFormatId">Identifiant demandÃ©, ou <see langword="null"/> pour dÃ©tecter automatiquement le format Apple.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le dÃ©codage des rÃ©volutions.</param>
    /// <returns>L'image Macintosh ou Lisa reconstruite avec sa capacitÃ© et son nombre logique de blocs.</returns>
    /// <exception cref="InvalidDataException">Aucun secteur GCR Apple IWM n'a Ã©tÃ© dÃ©codÃ© ou aucun candidat ne respecte la gÃ©omÃ©trie zonÃ©e.</exception>
    /// <remarks>La capacitÃ© est exprimÃ©e en octets et dÃ©pend du nombre de faces dÃ©tectÃ© dans les adresses candidates.</remarks>
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
        if (requestedFormatId is null && TryFlattenPayload(provisional, out var payload) &&
            AppleRawImageProbe.LooksLikeLisaOffice(payload))
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

    /// <summary>Reconstruit une charge utile linÃ©aire lorsque tous les blocs attendus sont prÃ©sents et valides.</summary>
    /// <param name="image">Image sectorielle Macintosh Ã  aplatir dans l'ordre des blocs logiques.</param>
    /// <param name="payload">Charge utile complÃ¨te crÃ©Ã©e lorsque la mÃ©thode retourne <see langword="true"/>.</param>
    /// <returns><see langword="true"/> lorsque chaque bloc logique possÃ¨de exactement la taille attendue ; sinon <see langword="false"/>.</returns>
    private static bool TryFlattenPayload(SectorImage image, out byte[] payload)
    {
        payload = new byte[image.BlockCount * image.BlockSize];
        if (image.AvailableBlocks.Count != image.BlockCount) return false;
        foreach (var block in image.AvailableBlocks)
        {
            if (block.LogicalBlock < 0 || block.LogicalBlock >= image.BlockCount || block.Data.Count != image.BlockSize) return false;
            block.Data.ToArray().CopyTo(payload, block.LogicalBlock * image.BlockSize);
        }
        return true;
    }
}
