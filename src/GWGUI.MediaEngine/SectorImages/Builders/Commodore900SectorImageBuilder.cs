using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.MediaEngine.SectorImages.Builders;

/// <summary>Reconstruit les blocs physiques d'un dump Commodore 900 dans sa géométrie zonée.</summary>
internal static class Commodore900SectorImageBuilder
{
    /// <summary>Construit l'image sectorielle avec une numérotation des secteurs à partir de zéro.</summary>
    public static SectorImage Create(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken)
    {
        var blockCount = bytes.Length / Commodore900Geometry.SectorSize;
        var blocks = new SectorBlock[blockCount];
        for (var logicalBlock = 0; logicalBlock < blockCount; logicalBlock++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            blocks[logicalBlock] = new(logicalBlock, Commodore900Geometry.AddressOf(logicalBlock), bytes.Slice(logicalBlock * Commodore900Geometry.SectorSize, Commodore900Geometry.SectorSize).ToArray(), true);
        }
        return new(DiskImageFormatIds.Commodore900Coherent, Commodore900Geometry.SectorSize, Commodore900Geometry.CylinderCount, Commodore900Geometry.HeadCount, Commodore900Geometry.MaximumSectorsPerTrack, blocks, capacity: bytes.Length, logicalBlockCount: blockCount);
    }
}
