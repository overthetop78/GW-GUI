namespace GWGUI.MediaEngine.Containers.Storage;

/// <summary>Publie une image seulement lorsque son écriture complète a réussi.</summary>
public interface IAtomicImageFileWriter
{
    /// <summary>Fournit un flux de sortie isolé et conserve la destination précédente en cas d'échec.</summary>
    Task WriteAsync(string path, Func<Stream, CancellationToken, Task> write, CancellationToken cancellationToken = default);
}

internal sealed class AtomicImageFileWriter : IAtomicImageFileWriter
{
    public async Task WriteAsync(string path, Func<Stream, CancellationToken, Task> write, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
                await write(output, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
