using GWGUI.MediaEngine.Containers.Commodore;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.FileSystems.Definitions;

namespace GWGUI.MediaEngine.Migration;

/// <summary>Crée et écrit des volumes Commodore DOS depuis le modèle commun.</summary>
public sealed class CommodoreDosMigrationService(CommodoreDosContainerWriter d64D71Writer, D81Writer d81Writer)
{
    /// <summary>Construit un plan dirigé vers Commodore DOS.</summary>
    public MigrationPlan CreatePlan(FileSystemVolume source) => MigrationPlanner.Create(source, FileSystemIds.CommodoreDos);

    /// <summary>Valide et écrit un volume en appliquant explicitement la politique des types Commodore.</summary>
    public Task<MigrationValidationReport> WriteAsync(FileSystemVolume source, string outputPath, string targetFormatId, bool acceptMetadataLoss = false, CommodoreDosWritePolicy? policy = null, CancellationToken cancellationToken = default) => WriteAsync(CreatePlan(source), outputPath, targetFormatId, acceptMetadataLoss, policy, cancellationToken);

    /// <summary>Valide et écrit un plan existant sans copier les secteurs du volume source.</summary>
    public async Task<MigrationValidationReport> WriteAsync(MigrationPlan plan, string outputPath, string targetFormatId, bool acceptMetadataLoss = false, CommodoreDosWritePolicy? policy = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var capabilities = FileSystemMigrationCapabilityCatalog.ForCommodoreDos(targetFormatId);
        if (plan.SourceFileSystemId.Equals(FileSystemIds.CommodoreDos, StringComparison.OrdinalIgnoreCase)) capabilities = capabilities with { SupportsRawAttributes = true };
        var report = MigrationValidator.Validate(plan, capabilities, acceptMetadataLoss);
        MigrationValidator.EnsureExecutable(report);
        var writable = MigrationMetadataReducer.Reduce(plan, capabilities);
        var image = new CommodoreDosVolumeWriter().Create(writable, targetFormatId, policy);
        await WriteImageAsync(image, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        return report;
    }

    private async Task WriteImageAsync(SectorImages.SectorImage image, string outputPath, string targetFormatId, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(outputPath);
        if (targetFormatId.Equals(DiskImageFormatIds.Commodore1541, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.D64, StringComparison.OrdinalIgnoreCase) || targetFormatId.Equals(DiskImageFormatIds.Commodore1571, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.D71, StringComparison.OrdinalIgnoreCase))
        {
            await d64D71Writer.WriteAsync(image, outputPath, cancellationToken: cancellationToken).ConfigureAwait(false);
            return;
        }
        if (targetFormatId.Equals(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.D81, StringComparison.OrdinalIgnoreCase))
        {
            await d81Writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
            return;
        }
        throw CommodoreDosMigrationExceptions.UnsupportedTarget(targetFormatId, extension);
    }
}
