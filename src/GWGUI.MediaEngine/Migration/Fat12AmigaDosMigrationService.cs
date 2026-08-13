using GWGUI.MediaEngine.Containers.Adf;
using GWGUI.MediaEngine.Conversion.Fat12;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Migration;

public sealed class Fat12AmigaDosMigrationService(
    AmigaAdfWriter amigaWriter,
    Fat12TargetImageWriter fatWriter)
{
    public MigrationPlan CreatePlan(FileSystemVolume source, string targetFormatId, AmigaDosVariant amigaVariant = AmigaDosVariant.Ffs)
    {
        ArgumentNullException.ThrowIfNull(source);
        var targetFileSystemId = targetFormatId.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase) ? amigaVariant.FileSystemId() : Fat12TargetGeometryCatalog.TryResolve(targetFormatId, out _) ? FileSystemIds.Fat12 : throw Fat12AmigaDosMigrationExceptions.UnsupportedDirection(source.FileSystemId, targetFormatId);
        return MigrationPlanner.Create(source, targetFileSystemId);
    }

    public Task<MigrationValidationReport> WriteAsync(FileSystemVolume source, string outputPath, string targetFormatId, bool acceptMetadataLoss = false, AmigaDosVariant amigaVariant = AmigaDosVariant.Ffs, CancellationToken cancellationToken = default) => WriteAsync(CreatePlan(source, targetFormatId, amigaVariant), outputPath, targetFormatId, acceptMetadataLoss, amigaVariant, cancellationToken);

    public async Task<MigrationValidationReport> WriteAsync(MigrationPlan plan, string outputPath, string targetFormatId, bool acceptMetadataLoss = false, AmigaDosVariant amigaVariant = AmigaDosVariant.Ffs, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var sourceIsFat = plan.SourceFileSystemId.Equals(FileSystemIds.Fat12, StringComparison.OrdinalIgnoreCase);
        var sourceIsAmiga = plan.SourceFileSystemId.StartsWith(FileSystemIds.AmigaDos, StringComparison.OrdinalIgnoreCase);
        var targetIsAmiga = targetFormatId.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase);
        var targetIsFat = Fat12TargetGeometryCatalog.TryResolve(targetFormatId, out _);
        if (!(sourceIsFat && targetIsAmiga || sourceIsAmiga && targetIsFat)) throw Fat12AmigaDosMigrationExceptions.UnsupportedDirection(plan.SourceFileSystemId, plan.TargetFileSystemId);
        var capabilities = targetIsAmiga ? FileSystemMigrationCapabilityCatalog.ForAmigaDos(amigaVariant, targetFormatId) : FileSystemMigrationCapabilityCatalog.ForFat12(targetFormatId);
        var report = MigrationValidator.Validate(plan, capabilities, acceptMetadataLoss);
        MigrationValidator.EnsureExecutable(report);
        var writable = MigrationMetadataReducer.Reduce(plan, capabilities);
        if (targetIsAmiga)
        {
            var image = new AmigaDosVolumeWriter().Create(writable, amigaVariant, targetFormatId);
            await amigaWriter.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var image = new Fat12VolumeWriter().Create(writable, targetFormatId);
            await fatWriter.WriteAsync(image, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        }
        return report;
    }

}
