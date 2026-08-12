using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Images;

using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Recognition.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Apple;

/// <summary>Reconstruit les images Macintosh et Lisa à zones depuis des secteurs SCP décodés.</summary>
/// <param name="decoder">Décodeur commun chargé de regrouper les candidats sectoriels Apple.</param>
internal sealed class AppleMacScpSectorReconstructor(AppleScpSectorDecoder decoder)
{
    /// <summary>Reconstruit et classe une image Macintosh ou Lisa.</summary>
    /// <param name="scp">Capture SCP déjà analysée.</param>
    /// <param name="requestedFormatId">Identifiant demandé, ou <see langword="null"/> pour détecter automatiquement le format Apple.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le décodage des révolutions.</param>
    /// <returns>L'image Macintosh ou Lisa reconstruite avec sa capacité et son nombre logique de blocs.</returns>
    /// <exception cref="InvalidDataException">Aucun secteur GCR Apple IWM n'a été décodé ou aucun candidat ne respecte la géométrie zonée.</exception>
    /// <remarks>La capacité est exprimée en octets et dépend du nombre de faces détecté dans les adresses candidates.</remarks>
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

    /// <summary>Reconstruit une charge utile linéaire lorsque tous les blocs attendus sont présents et valides.</summary>
    /// <param name="image">Image sectorielle Macintosh à aplatir dans l'ordre des blocs logiques.</param>
    /// <param name="payload">Charge utile complète créée lorsque la méthode retourne <see langword="true"/>.</param>
    /// <returns><see langword="true"/> lorsque chaque bloc logique possède exactement la taille attendue ; sinon <see langword="false"/>.</returns>
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
