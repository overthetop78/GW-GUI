using GWGUI.MediaEngine.Containers.Commodore;
using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.MediaEngine.SectorImages.Builders;

/// <summary>Construit les images sectorielles D64 et D71 à partir de leur ordre logique commun.</summary>
internal static class Commodore1541SectorImageBuilder
{
    /// <summary>Reconstruit les blocs et conserve le code de diagnostic de chaque secteur.</summary>
    public static SectorImage Create(ReadOnlySpan<byte> data, string formatId, int tracks, int sides, int blockCount, int? errorMapOffset, Func<int, int, Exception> invalidErrorMap, CancellationToken cancellationToken)
    {
        var errorEntries = errorMapOffset is { } offset ? data.Length - offset : 0;
        if (errorMapOffset is not null && errorEntries != blockCount) throw invalidErrorMap(blockCount, errorEntries);
        var blocks = new SectorBlock[blockCount];
        for (var logical = 0; logical < blockCount; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var address = Commodore1541Geometry.FromLogicalBlock(logical, tracks, sides);
            byte? errorCode = errorMapOffset is { } mapOffset ? data[mapOffset + logical] : null;
            var integrity = errorCode is null || errorCode == (byte)CommodoreDiskErrorCode.None;
            blocks[logical] = new(logical, new(Commodore1541Geometry.ToCylinder(address.Track), address.Side, address.Sector), data.Slice(logical * Commodore1541Geometry.SectorSize, Commodore1541Geometry.SectorSize).ToArray(), integrity, DiagnosticCode: errorCode);
        }
        return new(formatId, Commodore1541Geometry.SectorSize, tracks, sides, Commodore1541Geometry.MaximumSectorsPerTrack, blocks, capacity: blockCount * (long)Commodore1541Geometry.SectorSize, logicalBlockCount: blockCount);
    }
}
