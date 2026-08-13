using System.IO;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Migration;

namespace GWGUI.App.Services;

/// <summary>Prépare et exécute les migrations de fichiers sans les confondre avec les conversions d'images.</summary>
public sealed class FileMigrationCoordinator(
    DiskImageExplorer explorer,
    FileSystemMigrationService migrationService)
{
    public async Task<FileSystemVolume> ReadSourceAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) throw new FileNotFoundException("The migration source image does not exist.", path);
        var explored = await explorer.ExploreAsync(path, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!explored.FileSystemRecognized) throw new InvalidDataException("The source image does not contain a recognized file-system catalogue.");
        return explored.Volume;
    }

    public MigrationValidationReport Validate(FileSystemVolume source, string targetFormatId, bool acceptMetadataLoss = false) => migrationService.Validate(source, targetFormatId, acceptMetadataLoss);

    public Task<MigrationValidationReport> ExecuteAsync(FileSystemVolume source, string outputPath, string targetFormatId, bool acceptMetadataLoss, CancellationToken cancellationToken = default) => migrationService.WriteAsync(source, outputPath, targetFormatId, acceptMetadataLoss, cancellationToken);
}
