using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Convertit les secteurs Apple II de l'ordre DOS vers l'ordre logique ProDOS.</summary>
public static class AppleIISectorOrderConverter
{
    /// <summary>Réordonne toutes les pistes DOS en blocs logiques ProDOS sans modifier les données sectorielles.</summary>
    public static byte[] DosToProDos(ReadOnlySpan<byte> dosOrder)
    {
        if (dosOrder.Length % AppleIIGeometry.TrackSize != 0) throw AppleIISectorOrderExceptions.InvalidLength(dosOrder.Length, AppleIIGeometry.TrackSize);
        var output = new byte[dosOrder.Length];
        var trackCount = dosOrder.Length / AppleIIGeometry.TrackSize;
        for (var track = 0; track < trackCount; track++)
        {
            for (var logicalSector = 0; logicalSector < AppleIIGeometry.SectorsPerTrack; logicalSector++)
            {
                var physicalSector = AppleIIGeometry.ProDosToPhysical[logicalSector];
                var dosFileSector = AppleIIGeometry.PhysicalToDos[physicalSector];
                var sourceOffset = (track * AppleIIGeometry.SectorsPerTrack + dosFileSector) * AppleIIGeometry.SectorSize;
                var destinationOffset = (track * AppleIIGeometry.SectorsPerTrack + logicalSector) * AppleIIGeometry.SectorSize;
                dosOrder.Slice(sourceOffset, AppleIIGeometry.SectorSize).CopyTo(output.AsSpan(destinationOffset, AppleIIGeometry.SectorSize));
            }
        }
        return output;
    }
}
