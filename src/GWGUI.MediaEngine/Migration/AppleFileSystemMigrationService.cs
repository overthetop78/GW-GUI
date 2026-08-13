using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Apple.Dos;
using GWGUI.MediaEngine.FileSystems.Apple.ProDos;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Sos;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Migration;

/// <summary>Crée puis écrit des volumes Apple DOS, ProDOS ou SOS depuis le modèle commun.</summary>
public sealed class AppleFileSystemMigrationService(AppleRawImageWriter rawWriter, TwoImgWriter twoImgWriter, AppleDiskImageWriter trackWriter)
{
    /// <summary>Construit le plan commun dirigé vers le système de fichiers du format Apple demandé.</summary>
    public MigrationPlan CreatePlan(FileSystemVolume source, string targetFormatId) => MigrationPlanner.Create(source, ResolveFileSystemId(targetFormatId));

    /// <summary>Valide, crée et écrit un nouveau système de fichiers Apple.</summary>
    public Task<MigrationValidationReport> WriteAsync(FileSystemVolume source, string outputPath, string targetFormatId, bool acceptMetadataLoss = false, CancellationToken cancellationToken = default) => WriteAsync(CreatePlan(source, targetFormatId), outputPath, targetFormatId, acceptMetadataLoss, cancellationToken);

    /// <summary>Valide, crée et écrit le plan fourni sans copier les secteurs du système source.</summary>
    public async Task<MigrationValidationReport> WriteAsync(MigrationPlan plan, string outputPath, string targetFormatId, bool acceptMetadataLoss = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var capabilities = ResolveCapabilities(targetFormatId);
        if (plan.SourceFileSystemId.Equals(capabilities.FileSystemId, StringComparison.OrdinalIgnoreCase)) capabilities = capabilities with { SupportsRawAttributes = true };
        var report = MigrationValidator.Validate(plan, capabilities, acceptMetadataLoss);
        MigrationValidator.EnsureExecutable(report);
        var writable = MigrationMetadataReducer.Reduce(plan, capabilities);
        var image = CreateImage(writable, targetFormatId);
        await WriteImageAsync(image, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        return report;
    }

    private static MigrationTargetCapabilities ResolveCapabilities(string formatId) => ResolveFileSystemId(formatId) == FileSystemIds.AppleDos ? FileSystemMigrationCapabilityCatalog.ForAppleDos(formatId) : FileSystemMigrationCapabilityCatalog.ForProDos(formatId);

    private static string ResolveFileSystemId(string formatId)
    {
        if (formatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase)) return FileSystemIds.AppleDos;
        if (formatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase)) return FileSystemIds.ProDos;
        if (formatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase)) return FileSystemIds.Sos;
        throw AppleFileSystemMigrationExceptions.UnsupportedTarget(formatId, string.Empty);
    }

    private static SectorImage CreateImage(MigrationPlan plan, string formatId)
    {
        if (plan.TargetFileSystemId.Equals(FileSystemIds.AppleDos, StringComparison.OrdinalIgnoreCase)) return new AppleDosVolumeWriter().Create(plan, formatId);
        if (plan.TargetFileSystemId.Equals(FileSystemIds.ProDos, StringComparison.OrdinalIgnoreCase)) return new ProDosVolumeWriter().Create(plan, formatId);
        if (plan.TargetFileSystemId.Equals(FileSystemIds.Sos, StringComparison.OrdinalIgnoreCase) && formatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase)) return new SosVolumeWriter().Create(plan);
        throw AppleFileSystemMigrationExceptions.UnsupportedTarget(formatId, string.Empty);
    }

    private async Task WriteImageAsync(SectorImage image, string outputPath, string targetFormatId, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(outputPath);
        if (extension.Equals(DiskImageFileExtensions.TwoMg, StringComparison.OrdinalIgnoreCase)) await twoImgWriter.WriteAsync(image, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        else if (AppleDiskImageWriter.SupportsExtension(extension) && !targetFormatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase) && !targetFormatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase)) await trackWriter.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
        else if (Conversion.Apple.AppleSectorConversionService.CanCreate(targetFormatId, extension)) await rawWriter.WriteAsync(image, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        else throw AppleFileSystemMigrationExceptions.UnsupportedTarget(targetFormatId, extension);
    }
}
