using System.IO;

namespace GWGUI.MediaEngine.Images.Writing;

/// <summary>Remplace atomiquement un fichier par un contenu entièrement préparé en mémoire.</summary>
internal static class AtomicFileWriter
{
    public static async Task WriteAsync(string path, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        await WriteTemporaryAsync(fullPath,
            temporaryPath => File.WriteAllBytesAsync(temporaryPath, content.ToArray(), cancellationToken)).ConfigureAwait(false);
    }

    internal static async Task WriteTemporaryAsync(string fullPath, Func<string, Task> writeTemporary,
        CancellationToken publishCancellationToken = default)
    {
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await writeTemporary(temporaryPath).ConfigureAwait(false);
            publishCancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
