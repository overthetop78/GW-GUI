using GWGUI.MediaFileSystems.Formats.Commodore;
using AmigaDosLayout = global::GWGUI.MediaFileSystems.FileSystems.Amiga.AmigaDosLayout;
using AppleDosFileSystemLayout = global::GWGUI.MediaFileSystems.FileSystems.Apple.Dos.AppleDosFileSystemLayout;
using ProDosFileSystemLayout = global::GWGUI.MediaFileSystems.FileSystems.Apple.ProDos.ProDosFileSystemLayout;
using GWGUI.MediaEngine.Conversion.Fat12;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Conversion.Migration.Apple;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.Formats.Floppy.Adf;

using GWGUI.MediaEngine.Formats.Floppy.D64;

using GWGUI.MediaEngine.Formats.Floppy.D71;

using GWGUI.MediaEngine.Formats.Floppy.D81;

using GWGUI.MediaEngine.Formats.Floppy.Raw;

namespace GWGUI.MediaEngine.Conversion.Migration;

public static class FileSystemMigrationCapabilityCatalog
{
    public static MigrationTargetCapabilities ForAmigaDos(AmigaDosVariant variant, string formatId)
    {
        var capacity = formatId.Equals(Constants.DiskImageFormatIds.AmigaDosHighDensity, StringComparison.OrdinalIgnoreCase) ? AmigaAdfGeometry.HighDensityCapacity : AmigaAdfGeometry.DoubleDensityCapacity;
        return new(variant.FileSystemId(), AmigaDosLayout.OrdinaryNameMaximumLength, capacity, true, false, true, true, false, false, "/:", NamePolicy: new AmigaDosNamePolicy(), MaximumVolumeNameLength: AmigaDosLayout.OrdinaryNameMaximumLength, VolumeNamePolicy: new AmigaDosNamePolicy());
    }

    public static MigrationTargetCapabilities ForFat12(string formatId)
    {
        if (!Fat12TargetGeometryCatalog.TryResolve(formatId, out var geometry)) throw new InvalidDataException($"The target format '{formatId}' is not a supported FAT12 geometry.");
        return new(FileSystemIds.Fat12, FatDirectoryLayout.NameLength + 1 + FatDirectoryLayout.ExtensionLength, geometry.Capacity, true, false, true, false, false, false, "\"*+,/:;<=>?[\\]|", NamePolicy: new Fat12ShortNamePolicy(), MaximumVolumeNameLength: FatBootSectorLayout.VolumeLabelLength, VolumeNamePolicy: new Fat12VolumeNamePolicy());
    }

    /// <summary>Retourne les contraintes propres à Apple DOS 3.2 ou 3.3.</summary>
    public static MigrationTargetCapabilities ForAppleDos(string formatId)
    {
        var capacity = formatId.Equals(Constants.DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase) ? AppleIIGeometry.Dos32Capacity : formatId.Equals(Constants.DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase) ? AppleIIGeometry.Capacity : throw new InvalidDataException($"The Apple DOS format '{formatId}' is unsupported.");
        return new(FileSystemIds.AppleDos, AppleDosFileSystemLayout.EntryNameLength, Math.Min(capacity, ushort.MaxValue), false, false, false, false, false, false, ",", NamePolicy: new AppleDosNamePolicy(), MaximumVolumeNameLength: AppleDosFileSystemLayout.VolumeNamePrefix.Length + 3, VolumeNamePolicy: new AppleDosVolumeNamePolicy());
    }

    /// <summary>Retourne les contraintes ProDOS ou SOS en conservant leur identité distincte.</summary>
    public static MigrationTargetCapabilities ForProDos(string formatId)
    {
        var fileSystemId = formatId.Equals(Constants.DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase) ? FileSystemIds.Sos : formatId.Equals(Constants.DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase) || formatId.Equals(Constants.DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase) || formatId.Equals(Constants.DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) ? FileSystemIds.ProDos : throw new InvalidDataException($"The ProDOS/SOS format '{formatId}' is unsupported.");
        return new(fileSystemId, ProDosFileSystemLayout.MaximumNameLength, ProDosFileSystemLayout.MaximumFileLength, true, false, true, false, false, false, string.Empty, NamePolicy: new ProDosNamePolicy(), MaximumVolumeNameLength: ProDosFileSystemLayout.MaximumNameLength, VolumeNamePolicy: new ProDosNamePolicy());
    }

    /// <summary>Retourne les contraintes du catalogue plat Commodore DOS.</summary>
    public static MigrationTargetCapabilities ForCommodoreDos(string formatId)
    {
        var capacity = formatId switch
        {
            Constants.DiskImageFormatIds.Commodore1541 => Commodore1541Geometry.BlocksPerSide(Commodore1541Geometry.StandardTrackCount) * CommodoreDosLayout.DataBytesPerSector,
            Constants.DiskImageFormatIds.Commodore1571 => Commodore1541Geometry.BlocksPerSide(Commodore1541Geometry.StandardTrackCount) * Commodore1571Geometry.SideCount * CommodoreDosLayout.DataBytesPerSector,
            Constants.DiskImageFormatIds.Commodore1581 => Commodore1581Geometry.LogicalCylinderCount * Commodore1581Geometry.LogicalBlocksPerTrack * CommodoreDosLayout.DataBytesPerSector,
            _ => throw new InvalidDataException($"The Commodore DOS format '{formatId}' is unsupported.")
        };
        return new(FileSystemIds.CommodoreDos, CommodoreDosLayout.NameLength, capacity, false, false, false, false, false, false, string.Empty, NamePolicy: new CommodoreDosNamePolicy(), MaximumVolumeNameLength: CommodoreDosLayout.NameLength, VolumeNamePolicy: new CommodoreDosNamePolicy());
    }
}
