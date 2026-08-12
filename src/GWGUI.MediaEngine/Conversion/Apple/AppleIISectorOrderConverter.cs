using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Convertit les secteurs Apple II de l'ordre DOS vers l'ordre logique ProDOS.</summary>
public static class AppleIISectorOrderConverter
{
    /// <summary>Retourne le secteur physique correspondant à un secteur logique ProDOS à base zéro.</summary>
    public static int ProDosToPhysicalSector(int logicalSector)
    {
        if (logicalSector is < 0 or >= AppleIIGeometry.SectorsPerTrack) throw AppleIISectorOrderExceptions.InvalidSector(logicalSector, AppleIIGeometry.SectorsPerTrack);
        return AppleIIGeometry.ProDosToPhysical[logicalSector];
    }

    /// <summary>Retourne la position dans un fichier DOS du secteur physique à base zéro.</summary>
    public static int PhysicalToDosFileSector(int physicalSector)
    {
        if (physicalSector is < 0 or >= AppleIIGeometry.SectorsPerTrack) throw AppleIISectorOrderExceptions.InvalidSector(physicalSector, AppleIIGeometry.SectorsPerTrack);
        return AppleIIGeometry.PhysicalToDos[physicalSector];
    }

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
                var physicalSector = ProDosToPhysicalSector(logicalSector);
                var dosFileSector = PhysicalToDosFileSector(physicalSector);
                var sourceOffset = (track * AppleIIGeometry.SectorsPerTrack + dosFileSector) * AppleIIGeometry.SectorSize;
                var destinationOffset = (track * AppleIIGeometry.SectorsPerTrack + logicalSector) * AppleIIGeometry.SectorSize;
                dosOrder.Slice(sourceOffset, AppleIIGeometry.SectorSize).CopyTo(output.AsSpan(destinationOffset, AppleIIGeometry.SectorSize));
            }
        }
        return output;
    }
}
