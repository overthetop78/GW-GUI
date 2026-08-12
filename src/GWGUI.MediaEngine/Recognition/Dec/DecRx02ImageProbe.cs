using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.FileSystems.Dec.Rt11;
using GWGUI.MediaEngine.Geometries.Dec;

namespace GWGUI.MediaEngine.Recognition.Dec;

/// <summary>Recherche un home block RT-11 crédible dans un dump physique DEC RX02.</summary>
internal static class DecRx02ImageProbe
{
    /// <summary>Vérifie la capacité RX02 puis reconstruit les secteurs logiques du home block.</summary>
    public static bool LooksLikeRt11(ReadOnlyMemory<byte> bytes)
    {
        if (bytes.Length != DecRx02Geometry.Capacity) return false;
        Span<byte> homeBlock = stackalloc byte[DecRx02Geometry.LogicalBlockSize];
        DecRx02SectorOrder.CopyLogicalSector(bytes.Span, 2, homeBlock[..DecRx02Geometry.PhysicalSectorSize]);
        DecRx02SectorOrder.CopyLogicalSector(bytes.Span, 3, homeBlock[DecRx02Geometry.PhysicalSectorSize..]);
        return Rt11HomeBlockProbe.LooksLikeRt11(homeBlock);
    }
}
