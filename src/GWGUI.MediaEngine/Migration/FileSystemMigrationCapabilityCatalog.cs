using GWGUI.MediaEngine.Conversion.Fat12;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Amiga;

namespace GWGUI.MediaEngine.Migration;

public static class FileSystemMigrationCapabilityCatalog
{
    public static MigrationTargetCapabilities ForAmigaDos(AmigaDosVariant variant, string formatId)
    {
        var capacity = formatId.Equals(Definitions.DiskImageFormatIds.AmigaDosHighDensity, StringComparison.OrdinalIgnoreCase) ? AmigaAdfGeometry.HighDensityCapacity : AmigaAdfGeometry.DoubleDensityCapacity;
        return new(variant.FileSystemId(), AmigaDosLayout.OrdinaryNameMaximumLength, capacity, true, false, true, true, false, false, "/:", NamePolicy: new AmigaDosNamePolicy(), MaximumVolumeNameLength: AmigaDosLayout.OrdinaryNameMaximumLength, VolumeNamePolicy: new AmigaDosNamePolicy());
    }

    public static MigrationTargetCapabilities ForFat12(string formatId)
    {
        if (!Fat12TargetGeometryCatalog.TryResolve(formatId, out var geometry)) throw new InvalidDataException($"The target format '{formatId}' is not a supported FAT12 geometry.");
        return new(FileSystemIds.Fat12, FatDirectoryLayout.NameLength + 1 + FatDirectoryLayout.ExtensionLength, geometry.Capacity, true, false, true, false, false, false, "\"*+,/:;<=>?[\\]|", NamePolicy: new Fat12ShortNamePolicy(), MaximumVolumeNameLength: FatBootSectorLayout.VolumeLabelLength, VolumeNamePolicy: new Fat12VolumeNamePolicy());
    }
}
