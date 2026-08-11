using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Coherent;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Coherent;

/// <summary>Lit les dumps sectoriels bruts des disquettes COHERENT, notamment ceux du Commodore 900.</summary>
public sealed class CoherentRawImageReader
{
    /// <summary>Lit puis valide un dump COHERENT depuis son chemin.</summary>
    /// <param name="path">Chemin du dump à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle reconstruite selon la géométrie du Commodore 900.</returns>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Read(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false), cancellationToken);

    /// <summary>Valide et reconstruit un dump COHERENT déjà chargé en mémoire.</summary>
    /// <param name="bytes">Contenu intégral du dump en lecture seule.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la reconstruction.</param>
    /// <returns>Image sectorielle reconstruite selon la géométrie du Commodore 900.</returns>
    public Task<SectorImage> ReadAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) => Task.FromResult(Read(bytes.Span, cancellationToken));

    /// <summary>Valide le superbloc et reconstruit les blocs sectoriels du dump.</summary>
    private static SectorImage Read(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken)
    {
        CoherentSuperblockProbe.ReadValidatedFileSystemBlockCount(bytes);
        var blockCount = bytes.Length / CoherentSuperblockProbe.BlockSize;
        var sectors = new List<SectorBlock>(blockCount);
        var block = 0;
        for (var cylinder = 0; cylinder < DiskGeometryConstants.EightyTrackCylinderCount && block < blockCount; cylinder++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sectorsPerTrack = SectorsPerTrack(cylinder);
            for (var head = 0; head < DiskGeometryConstants.DoubleSidedHeadCount && block < blockCount; head++)
                for (var sector = 0; sector < sectorsPerTrack && block < blockCount; sector++, block++)
                    sectors.Add(new(block, new(cylinder, head, sector), bytes.Slice(block * CoherentSuperblockProbe.BlockSize, CoherentSuperblockProbe.BlockSize).ToArray(), true));
        }
        return new(DiskImageFormatIds.Commodore900Coherent, CoherentSuperblockProbe.BlockSize, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 16, sectors, capacity: bytes.Length, logicalBlockCount: blockCount);
    }

    /// <summary>Retourne le nombre de secteurs physiques par piste pour un cylindre Commodore 900.</summary>
    private static int SectorsPerTrack(int cylinder) => cylinder switch { < 39 => 16, < 53 => 15, < 64 => 14, _ => 13 };
}
