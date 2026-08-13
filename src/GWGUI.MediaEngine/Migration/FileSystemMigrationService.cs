using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Migration;

/// <summary>Planifie, valide et exécute une migration vers n'importe quel système de fichiers reconstructible.</summary>
public sealed class FileSystemMigrationService(
    Fat12AmigaDosMigrationService fatAndAmiga,
    AppleFileSystemMigrationService apple,
    CommodoreDosMigrationService commodore)
{
    public MigrationPlan CreatePlan(FileSystemVolume source, string targetFormatId)
    {
        var target = FileSystemMigrationTargetCatalog.Get(targetFormatId);
        return MigrationPlanner.Create(source, target.FileSystemId);
    }

    public MigrationValidationReport Validate(FileSystemVolume source, string targetFormatId, bool acceptMetadataLoss = false)
    {
        var plan = CreatePlan(source, targetFormatId);
        return MigrationValidator.Validate(plan, ResolveCapabilities(targetFormatId), acceptMetadataLoss);
    }

    public Task<MigrationValidationReport> WriteAsync(
        FileSystemVolume source,
        string outputPath,
        string targetFormatId,
        bool acceptMetadataLoss = false,
        CancellationToken cancellationToken = default)
    {
        FileSystemMigrationTargetCatalog.Get(targetFormatId);
        if (targetFormatId.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase)
            || Conversion.Fat12.Fat12TargetGeometryCatalog.TryResolve(targetFormatId, out _))
        {
            return fatAndAmiga.WriteAsync(source, outputPath, targetFormatId, acceptMetadataLoss, cancellationToken: cancellationToken);
        }
        if (targetFormatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase)
            || targetFormatId.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return apple.WriteAsync(source, outputPath, targetFormatId, acceptMetadataLoss, cancellationToken);
        }
        return commodore.WriteAsync(source, outputPath, targetFormatId, acceptMetadataLoss, cancellationToken: cancellationToken);
    }

    private static MigrationTargetCapabilities ResolveCapabilities(string targetFormatId)
    {
        var target = FileSystemMigrationTargetCatalog.Get(targetFormatId);
        if (target.FileSystemId.StartsWith(FileSystems.Definitions.FileSystemIds.AmigaDos, StringComparison.OrdinalIgnoreCase)) return FileSystemMigrationCapabilityCatalog.ForAmigaDos(FileSystems.Amiga.AmigaDosVariant.Ffs, targetFormatId);
        if (target.FileSystemId.Equals(FileSystems.Definitions.FileSystemIds.Fat12, StringComparison.OrdinalIgnoreCase)) return FileSystemMigrationCapabilityCatalog.ForFat12(targetFormatId);
        if (target.FileSystemId.Equals(FileSystems.Definitions.FileSystemIds.AppleDos, StringComparison.OrdinalIgnoreCase)) return FileSystemMigrationCapabilityCatalog.ForAppleDos(targetFormatId);
        if (target.FileSystemId.Equals(FileSystems.Definitions.FileSystemIds.ProDos, StringComparison.OrdinalIgnoreCase) || target.FileSystemId.Equals(FileSystems.Definitions.FileSystemIds.Sos, StringComparison.OrdinalIgnoreCase)) return FileSystemMigrationCapabilityCatalog.ForProDos(targetFormatId);
        return FileSystemMigrationCapabilityCatalog.ForCommodoreDos(targetFormatId);
    }
}
