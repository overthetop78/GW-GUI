using System.Buffers.Binary;
using GWGUI.MediaEngine.Geometries.Dec;

namespace GWGUI.MediaEngine.FileSystems.Rt11;

/// <summary>Valide les champs structurants d'un home block RT-11 remis en ordre logique.</summary>
internal static class Rt11HomeBlockProbe
{
    private const int DirectoryBlockOffset = 468;
    private const int SystemIdOffset = 496;
    private const int SystemIdLength = 12;
    private const string SystemIdPrefix = "DECRT11";

    /// <summary>Indique si le bloc contient un numéro de répertoire et un identifiant système RT-11 valides.</summary>
    public static bool LooksLikeRt11(ReadOnlySpan<byte> homeBlock)
    {
        if (homeBlock.Length != DecRx02Geometry.LogicalBlockSize) return false;
        var directoryBlock = BinaryPrimitives.ReadUInt16LittleEndian(homeBlock.Slice(DirectoryBlockOffset, sizeof(ushort)));
        var systemId = System.Text.Encoding.ASCII.GetString(homeBlock.Slice(SystemIdOffset, SystemIdLength)).TrimEnd('\0', ' ');
        return directoryBlock is >= 2 and < DecRx02Geometry.LogicalBlockCount && systemId.StartsWith(SystemIdPrefix, StringComparison.Ordinal);
    }
}
