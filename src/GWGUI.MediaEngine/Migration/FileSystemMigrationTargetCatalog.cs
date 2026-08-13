using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Definitions;

namespace GWGUI.MediaEngine.Migration;

/// <summary>Répertorie les systèmes de fichiers que le moteur sait reconstruire depuis le modèle commun.</summary>
public static class FileSystemMigrationTargetCatalog
{
    public static IReadOnlyList<FileSystemMigrationTarget> All { get; } =
    [
        new(DiskImageFormatIds.AmigaDos, FileSystemIds.AmigaDosFfs, DiskImageFileExtensions.Adf),
        new(DiskImageFormatIds.AtariSt720, FileSystemIds.Fat12, DiskImageFileExtensions.St),
        new(DiskImageFormatIds.Ibm720, FileSystemIds.Fat12, DiskImageFileExtensions.Img),
        new(DiskImageFormatIds.Msx2Dd, FileSystemIds.Fat12, DiskImageFileExtensions.Dsk),
        new(DiskImageFormatIds.AppleIIAppleDos140, FileSystemIds.AppleDos, DiskImageFileExtensions.Do),
        new(DiskImageFormatIds.AppleIIProDos140, FileSystemIds.ProDos, DiskImageFileExtensions.Po),
        new(DiskImageFormatIds.AppleIIProDos800, FileSystemIds.ProDos, DiskImageFileExtensions.TwoMg),
        new(DiskImageFormatIds.AppleIIISos, FileSystemIds.Sos, DiskImageFileExtensions.TwoMg),
        new(DiskImageFormatIds.Commodore1541, FileSystemIds.CommodoreDos, DiskImageFileExtensions.D64),
        new(DiskImageFormatIds.Commodore1571, FileSystemIds.CommodoreDos, DiskImageFileExtensions.D71),
        new(DiskImageFormatIds.Commodore1581, FileSystemIds.CommodoreDos, DiskImageFileExtensions.D81)
    ];

    public static FileSystemMigrationTarget Get(string formatId) =>
        All.FirstOrDefault(target => target.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidDataException($"The migration target '{formatId}' is unsupported.");
}
